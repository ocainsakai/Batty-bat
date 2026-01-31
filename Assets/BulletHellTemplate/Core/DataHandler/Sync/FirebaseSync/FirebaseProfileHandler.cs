#if FIREBASE

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System.Linq;

namespace BulletHellTemplate
{
    /// <summary>
    /// Responsible for provisioning default Firestore documents for a user
    /// and mirroring essential fields to local PlayerSave/MonetizationManager.
    /// </summary>
    public static class FirebaseProfileHandler
    {
        /// <summary>
        /// Ensures Players/{uid} and Currencies docs exist with defaults,
        /// then mirrors to PlayerSave and MonetizationManager.
        /// </summary>
        public static async UniTask EnsureProvisionedAndMirroredAsync(FirebaseUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            await FirebaseAuthHandler.EnsureInitializedAsync();

            var db = FirebaseAuthHandler.Firestore;
            var doc = db.Collection("Players").Document(user.UserId);
            var snap = await doc.GetSnapshotAsync();

            // Defaults based on GameInstance data (first unlocked, then fallback to first item/id).
            string defaultName = $"Guest-{UnityEngine.Random.Range(10000, 99999)}";
            string defaultIcon = ResolveDefaultIconId();
            string defaultFrame = ResolveDefaultFrameId();

            if (!snap.Exists)
            {
                await doc.SetAsync(new Dictionary<string, object>
                {
                    { "PlayerName", defaultName },
                    { "PlayerIcon", defaultIcon },
                    { "PlayerFrame", defaultFrame },
                    { "selectedCharacter", 0 },
                    { "score", 0 },
                    { "AccountLevel", 1 },
                    { "AccountCurrentExp", 0 },
                });
            }
            else
            {
                var data = snap.ToDictionary();
                var patch = new Dictionary<string, object>();

                if (!data.ContainsKey("PlayerName")) patch["PlayerName"] = defaultName;
                if (!data.ContainsKey("PlayerIcon")) patch["PlayerIcon"] = defaultIcon;
                if (!data.ContainsKey("PlayerFrame")) patch["PlayerFrame"] = defaultFrame;
                if (!data.ContainsKey("selectedCharacter")) patch["selectedCharacter"] = 0;
                if (!data.ContainsKey("score")) patch["score"] = 0;
                if (!data.ContainsKey("AccountLevel")) patch["AccountLevel"] = 1;
                if (!data.ContainsKey("AccountCurrentExp")) patch["AccountCurrentExp"] = 0;

                if (patch.Count > 0)
                    await doc.SetAsync(patch, SetOptions.MergeAll);
            }

            // Re-read and mirror locally.
            snap = await doc.GetSnapshotAsync();
            var finalData = snap.ToDictionary();

            if (finalData.TryGetValue("PlayerName", out var pName) && pName != null)
                PlayerSave.SetPlayerName(pName.ToString());
            if (finalData.TryGetValue("PlayerIcon", out var pIcon) && pIcon != null)
                PlayerSave.SetPlayerIcon(pIcon.ToString());
            if (finalData.TryGetValue("PlayerFrame", out var pFrame) && pFrame != null)
                PlayerSave.SetPlayerFrame(pFrame.ToString());

            if (finalData.TryGetValue("AccountLevel", out var aLvl) && aLvl != null)
                PlayerSave.SetAccountLevel(Convert.ToInt32(aLvl));
            else
                PlayerSave.SetAccountLevel(1);

            if (finalData.TryGetValue("AccountCurrentExp", out var aExp) && aExp != null)
                PlayerSave.SetAccountCurrentExp(Convert.ToInt32(aExp));
            else
                PlayerSave.SetAccountCurrentExp(0);

            // Currencies subcollection
            await EnsureCurrenciesAndMirrorAsync(doc);
        }

        /// <summary>
        /// Ensures currency docs exist and updates MonetizationManager with current amounts.
        /// </summary>
        private static async UniTask EnsureCurrenciesAndMirrorAsync(DocumentReference playerDoc)
        {
            if (GameInstance.Singleton?.currencyData == null) return;

            var col = playerDoc.Collection("Currencies");

            foreach (var c in GameInstance.Singleton.currencyData)
            {
                var cDoc = col.Document(c.coinID);
                var cSnap = await cDoc.GetSnapshotAsync();

                if (!cSnap.Exists)
                {
                    await cDoc.SetAsync(new Dictionary<string, object>
                    {
                        { "initialAmount", c.initialAmount },
                        { "amount", c.initialAmount },
                    });
                    MonetizationManager.SetCurrency(c.coinID, c.initialAmount);
                }
                else
                {
                    int amount = c.initialAmount;
                    if (cSnap.TryGetValue("amount", out int a)) amount = a;
                    MonetizationManager.SetCurrency(c.coinID, amount);
                }
            }
        }

        /// <summary>
        /// Returns the first unlocked icon id or the first entry as fallback.
        /// </summary>
        private static string ResolveDefaultIconId()
        {
            var list = GameInstance.Singleton?.iconItems;
            if (list != null && list.Count() > 0)
            {
                foreach (var it in list)
                    if (it != null && it.isUnlocked) return it.iconId;
                return list[0].iconId;
            }
            return "icon1";
        }

        /// <summary>
        /// Returns the first unlocked frame id or the first entry as fallback.
        /// </summary>
        private static string ResolveDefaultFrameId()
        {
            var list = GameInstance.Singleton?.frameItems;
            if (list != null && list.Count() > 0)
            {
                foreach (var it in list)
                    if (it != null && it.isUnlocked) return it.frameId;
                return list[0].frameId;
            }
            return "frame1";
        }

        /// <summary>
        /// Changes the player's nickname enforcing GameInstance rules, optional ticket consumption,
        /// and global uniqueness via a reservation doc at /PlayerNames/{lowercase}.
        /// All reads happen before writes; old reservation is removed inside the same transaction
        /// only if it exists and belongs to the same user, avoiding post-commit cleanup.
        /// Error codes:
        /// "0" = invalid length, "1" = not enough tickets, "2" = same name, "3" = name already in use.
        /// </summary>
        public static async UniTask<RequestResult> ChangePlayerNameAsync(string newName)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var user = FirebaseAuthHandler.Auth.CurrentUser;
            if (user == null)
                return RequestResult.Fail("invalid_credentials");

            if (string.IsNullOrWhiteSpace(newName))
                return RequestResult.Fail("0");

            newName = newName.Trim();

            // Length policy from GameInstance
            var gi = GameInstance.Singleton;
            int minLen = gi?.minNameLength ?? 3;
            int maxLen = gi?.maxNameLength ?? 20;
            if (newName.Length < minLen || newName.Length > maxLen)
                return RequestResult.Fail("0");

            // Fast local equality check (authoritative check also happens in transaction)
            var localCurrent = PlayerSave.GetPlayerName();
            if (!string.IsNullOrEmpty(localCurrent) &&
                localCurrent.Equals(newName, StringComparison.OrdinalIgnoreCase))
                return RequestResult.Fail("2");

            bool needTicket = gi?.needTicket ?? false;
            string ticketCurrency = gi?.changeNameTick ?? "TKN";
            int ticketsRequired = gi?.ticketsToChange ?? 1;

            var db = FirebaseAuthHandler.Firestore;
            string uid = user.UserId;
            var playerDoc = db.Collection("Players").Document(uid);
            var namesRoot = db.Collection("PlayerNames");
            string newKey = newName.ToLowerInvariant();
            var newNameDoc = namesRoot.Document(newKey);
            var ticketDoc = playerDoc.Collection("Currencies").Document(ticketCurrency);

            try
            {
                await db.RunTransactionAsync<bool>(async tr =>
                {
                    /* ─────── READS (all before writes) ─────── */
                    // Player -> obtain old name
                    var playerSnap = await tr.GetSnapshotAsync(playerDoc);
                    string oldName = playerSnap.Exists && playerSnap.ContainsField("PlayerName")
                        ? playerSnap.GetValue<string>("PlayerName")
                        : null;
                    string oldKey = string.IsNullOrEmpty(oldName) ? null : oldName.Trim().ToLowerInvariant();
                    DocumentReference oldNameDoc = null;
                    DocumentSnapshot oldNameSnap = null;

                    // New name reservation
                    var newNameSnap = await tr.GetSnapshotAsync(newNameDoc);

                    // Tickets
                    int currentTickets = 0;
                    if (needTicket)
                    {
                        var tSnap = await tr.GetSnapshotAsync(ticketDoc);
                        currentTickets = (tSnap.Exists && tSnap.ContainsField("amount"))
                            ? tSnap.GetValue<int>("amount")
                            : 0;
                        if (currentTickets < ticketsRequired)
                            throw new InvalidOperationException("NO_TICKETS");
                    }

                    // If we intend to delete old reservation, read it now
                    if (!string.IsNullOrEmpty(oldKey))
                    {
                        oldNameDoc = namesRoot.Document(oldKey);
                        oldNameSnap = await tr.GetSnapshotAsync(oldNameDoc);
                    }

                    /* ─────── Validations based on read data ─────── */
                    // Uniqueness
                    if (newNameSnap.Exists)
                    {
                        string owner = newNameSnap.ContainsField("ownerUid")
                            ? newNameSnap.GetValue<string>("ownerUid") : string.Empty;

                        // If another user owns this name -> conflict
                        if (!string.Equals(owner, uid, StringComparison.Ordinal))
                            throw new InvalidOperationException("NAME_TAKEN");
                    }

                    // Same name (authoritative, using server value)
                    if (!string.IsNullOrEmpty(oldName) &&
                        oldName.Equals(newName, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("SAME_NAME");

                    /* ─────── WRITES (after all reads) ─────── */
                    // Deduct ticket
                    if (needTicket)
                    {
                        tr.Update(ticketDoc, new Dictionary<string, object>
                        {
                            { "amount", currentTickets - ticketsRequired }
                        });
                    }

                    // Reserve new name
                    tr.Set(newNameDoc, new Dictionary<string, object>
                    {
                        { "ownerUid", uid },
                        { "createdAt", FieldValue.ServerTimestamp }
                    }, SetOptions.MergeAll);

                    // Update player document
                    tr.Update(playerDoc, new Dictionary<string, object>
                    {
                        { "PlayerName", newName },
                        { "PlayerNameLower", newKey }
                    });

                    // Delete old reservation only if it exists and belongs to the same user
                    if (oldNameSnap != null && oldNameSnap.Exists)
                    {
                        string owner = oldNameSnap.ContainsField("ownerUid")
                            ? oldNameSnap.GetValue<string>("ownerUid") : string.Empty;
                        if (string.Equals(owner, uid, StringComparison.Ordinal) && oldKey != newKey)
                        {
                            tr.Delete(oldNameDoc);
                        }
                    }

                    return true;
                });

                // Mirror locally
                PlayerSave.SetPlayerName(newName);
                if (needTicket)
                {
                    int local = MonetizationManager.GetCurrency(ticketCurrency);
                    MonetizationManager.SetCurrency(ticketCurrency, Math.Max(0, local - ticketsRequired), pushToBackend: false);
                }

                return RequestResult.Ok();
            }
            catch (InvalidOperationException ioe)
            {
                if (ioe.Message == "NAME_TAKEN") return RequestResult.Fail("3");
                if (ioe.Message == "NO_TICKETS") return RequestResult.Fail("1");
                if (ioe.Message == "SAME_NAME") return RequestResult.Fail("2");
                Debug.LogException(ioe);
                return RequestResult.Fail("generic_error");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        /// <summary>
        /// Changes the player's icon, validating ownership and updating Firestore + local mirror.
        /// Error codes:
        /// "1" = icon not found, "0" = not owned.
        /// </summary>
        public static async UniTask<RequestResult> ChangePlayerIconAsync(string iconId)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var user = FirebaseAuthHandler.Auth.CurrentUser;
            if (user == null)
                return RequestResult.Fail("invalid_credentials");

            IconItem item = GameInstance.Singleton?.GetIconItemById(iconId);
            if (item == null) return RequestResult.Fail("1");

            bool owned = item.isUnlocked || PlayerSave.IsIconPurchased(iconId);
            if (!owned) return RequestResult.Fail("0");

            try
            {
                var db = FirebaseAuthHandler.Firestore;
                var playerDoc = db.Collection("Players").Document(user.UserId);

                await playerDoc.UpdateAsync(new Dictionary<string, object>
                {
                    { "PlayerIcon", iconId }
                });

                PlayerSave.SetPlayerIcon(iconId);
                return RequestResult.Ok();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        /// <summary>
        /// Changes the player's frame, validating ownership and updating Firestore + local mirror.
        /// Error codes:
        /// "1" = frame not found, "0" = not owned.
        /// </summary>
        public static async UniTask<RequestResult> ChangePlayerFrameAsync(string frameId)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var user = FirebaseAuthHandler.Auth.CurrentUser;
            if (user == null)
                return RequestResult.Fail("invalid_credentials");

            FrameItem item = GameInstance.Singleton?.GetFrameItemById(frameId);
            if (item == null) return RequestResult.Fail("1");

            bool owned = item.isUnlocked || PlayerSave.IsFramePurchased(frameId);
            if (!owned) return RequestResult.Fail("0");

            try
            {
                var db = FirebaseAuthHandler.Firestore;
                var playerDoc = db.Collection("Players").Document(user.UserId);

                await playerDoc.UpdateAsync(new Dictionary<string, object>
                {
                    { "PlayerFrame", frameId }
                });

                PlayerSave.SetPlayerFrame(frameId);
                return RequestResult.Ok();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }
    }
}
#endif