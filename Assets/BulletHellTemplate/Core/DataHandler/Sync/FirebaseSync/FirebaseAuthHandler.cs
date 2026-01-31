#if FIREBASE

using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Collections;
using static BulletHellTemplate.PlayerSave;

namespace BulletHellTemplate
{
    /// <summary>
    /// Centralizes Firebase initialization, session persistence and authentication flows.
    /// Delegates profile provisioning/mirroring to FirebaseProfileHandler.
    /// Password policy is read from GameInstance at runtime.
    /// </summary>
    public static class FirebaseAuthHandler
    {
        /// <summary>Cached FirebaseAuth instance.</summary>
        public static FirebaseAuth Auth { get; private set; }

        /// <summary>Cached FirebaseFirestore instance.</summary>
        public static FirebaseFirestore Firestore { get; private set; }

        private static bool _initialized;

        /// <summary>
        /// Ensures Firebase dependencies are ready and caches Auth/Firestore.
        /// Also configures persistent sessions (WebGL explicitly).
        /// Safe to call multiple times.
        /// </summary>
        public static async UniTask EnsureInitializedAsync()
        {
            if (_initialized && Auth != null && Firestore != null)
                return;

            var status = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (status != DependencyStatus.Available)
                throw new Exception($"Firebase dependencies are not available: {status}");

            Auth = FirebaseAuth.DefaultInstance;
            Firestore = FirebaseFirestore.DefaultInstance;

#if UNITY_WEBGL
            await Auth.SetPersistenceAsync(Persistence.Local);
#endif
            _initialized = true;
        }

        /// <summary>
        /// Validates basic email format (UI-friendly, not exhaustive).
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            const string pattern = @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$";
            return Regex.IsMatch(email, pattern, RegexOptions.CultureInvariant);
        }

        /// <summary>
        /// Holds password policy values resolved from GameInstance.
        /// </summary>
        private readonly struct PasswordPolicy
        {
            public readonly int Min;
            public readonly int Max;
            public readonly bool RequireUpper;
            public readonly bool RequireLower;
            public readonly bool RequireDigit;
            public readonly bool RequireSpecial;

            public PasswordPolicy(int min, int max, bool upper, bool lower, bool digit, bool special)
            {
                Min = min; Max = max;
                RequireUpper = upper; RequireLower = lower;
                RequireDigit = digit; RequireSpecial = special;
            }
        }

        /// <summary>
        /// Reads password policy from GameInstance.Singleton, with sane fallbacks.
        /// </summary>
        private static PasswordPolicy GetPolicyFromGameInstance()
        {
            // Fallbacks mirror GameInstance defaults shown in the project
            int min = 8, max = 20;
            bool reqU = false, reqL = false, reqN = false, reqS = false;

            var gi = GameInstance.Singleton;
            if (gi != null)
            {
                min = Mathf.Max(1, gi.minPasswordLength);
                max = Mathf.Max(min, gi.maxPasswordLength);
                reqU = gi.requireUppercase;
                reqL = gi.requireLowercase;
                reqN = gi.requireNumbers;
                reqS = gi.requireSpecial;
            }

            return new PasswordPolicy(min, max, reqU, reqL, reqN, reqS);
        }

        /// <summary>
        /// Validates password against the policy from GameInstance.
        /// Returns a UI-friendly error code or null when valid.
        /// </summary>
        private static string ValidatePassword(string pass, string confirm = null)
        {
            var p = GetPolicyFromGameInstance();

            if (confirm != null && pass != confirm) return "PASSWORD_MISMATCH";
            if (string.IsNullOrEmpty(pass) || pass.Length < p.Min) return "PASSWORD_TOO_SHORT";
            if (pass.Length > p.Max) return "PASSWORD_TOO_LONG";

            if (p.RequireUpper && !pass.Any(char.IsUpper)) return "PASSWORD_MISSING_UPPERCASE";
            if (p.RequireLower && !pass.Any(char.IsLower)) return "PASSWORD_MISSING_LOWERCASE";
            if (p.RequireDigit && !pass.Any(char.IsDigit)) return "PASSWORD_MISSING_NUMBER";
            if (p.RequireSpecial && !pass.Any(ch => !char.IsLetterOrDigit(ch) && !char.IsWhiteSpace(ch)))
                return "PASSWORD_MISSING_SPECIAL_CHAR";

            return null;
        }

        /// <summary>
        /// Login with email/password. On success, provisions defaults and mirrors locally.
        /// </summary>
        public static async UniTask<RequestResult> LoginWithEmailAsync(string email, string pass)
        {
            try
            {
                if (!IsValidEmail(email)) return RequestResult.Fail("INVALID_EMAIL");
                if (string.IsNullOrWhiteSpace(pass)) return RequestResult.Fail("invalid_credentials");

                await EnsureInitializedAsync();
                await Auth.SignInWithEmailAndPasswordAsync(email, pass);
                if (Auth.CurrentUser == null) return RequestResult.Fail("invalid_credentials");

                await FirebaseProfileHandler.EnsureProvisionedAndMirroredAsync(Auth.CurrentUser);

                var load = await LoadAllAccountDataAsync(); 
                return load.Success ? RequestResult.Ok() : load;
            }
            catch (FirebaseException)
            {
                return RequestResult.Fail("invalid_credentials");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        /// <summary>
        /// Register with email/password using GameInstance password policy.
        /// On success, provisions defaults and mirrors locally.
        /// </summary>
        public static async UniTask<RequestResult> RegisterWithEmailAsync(string email, string pass, string confirm)
        {
            try
            {
                if (!IsValidEmail(email)) return RequestResult.Fail("INVALID_EMAIL");
                var reason = ValidatePassword(pass, confirm);
                if (reason != null) return RequestResult.Fail(reason);

                await EnsureInitializedAsync();
                await Auth.CreateUserWithEmailAndPasswordAsync(email, pass);
                if (Auth.CurrentUser == null) return RequestResult.Fail("generic_error");

                await FirebaseProfileHandler.EnsureProvisionedAndMirroredAsync(Auth.CurrentUser);

                var load = await LoadAllAccountDataAsync(); 
                return load.Success ? RequestResult.Ok() : load;
            }
            catch (FirebaseException fex)
            {
                var m = fex.Message?.ToLowerInvariant() ?? "";
                if (m.Contains("already") && m.Contains("exist")) return RequestResult.Fail("EMAIL_ALREADY_IN_USE");
                return RequestResult.Fail("generic_error");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        /// <summary>
        /// Anonymous sign-in. On success, provisions defaults and mirrors locally.
        /// </summary>
        public static async UniTask<RequestResult> SignInAnonymouslyAsync()
        {
            try
            {
                await EnsureInitializedAsync();
                await Auth.SignInAnonymouslyAsync();
                if (Auth.CurrentUser == null) return RequestResult.Fail("generic_error");

                await FirebaseProfileHandler.EnsureProvisionedAndMirroredAsync(Auth.CurrentUser);

                var load = await LoadAllAccountDataAsync();
                return load.Success ? RequestResult.Ok() : load;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        /// <summary>
        /// Auto-login whenever a persisted user exists (including anonymous).
        /// Provisions defaults and mirrors locally. Returns true on success.
        /// </summary>
        public static async UniTask<bool> TryAutoLoginAsync()
        {
            await EnsureInitializedAsync();
            var user = Auth.CurrentUser;
            if (user == null) return false;

            try
            {
                await FirebaseProfileHandler.EnsureProvisionedAndMirroredAsync(user);
                var load = await LoadAllAccountDataAsync();
                return load.Success;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// Signs out from Firebase Auth.
        /// </summary>
        public static UniTask LogoutAsync()
        {
            try { Auth?.SignOut(); } catch (Exception ex) { Debug.LogException(ex); }
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Loads all account data with a single snapshot of /Players/{uid}.
        /// No extra reads; if some mirrors are missing (e.g. Currencies map), local defaults are kept.
        /// </summary>
        public static async UniTask<RequestResult> LoadAllAccountDataAsync()
        {
            await EnsureInitializedAsync();
            var user = Auth.CurrentUser;
            if (user == null) return RequestResult.Fail("invalid_credentials");

            try
            {
                var playerDoc = Firestore.Collection("Players").Document(user.UserId);
                var snap = await playerDoc.GetSnapshotAsync();

                if (!snap.Exists)
                {
                    await FirebaseProfileHandler.EnsureProvisionedAndMirroredAsync(user);
                    snap = await playerDoc.GetSnapshotAsync();
                    if (!snap.Exists) return RequestResult.Fail("generic_error");
                }

                ApplySinglePlayerDocToLocal(snap, out bool hasDailyInRoot, out bool hasNewPlayerInRoot);

                if (!hasDailyInRoot || !hasNewPlayerInRoot)
                    await ApplyRewardsFromProgressDocsAsync(user.UserId, !hasDailyInRoot, !hasNewPlayerInRoot);

                await ApplyProgressFromDocsAsync(user.UserId);

                await ApplyCharactersFromDocsAsync(user.UserId);

                await ApplyPurchasesFromSubdocsAsync(user.UserId);

                await ApplyInventoryFromListAsync(user.UserId);

                await ApplyBattlePassSeasonInfoAsync();

                return RequestResult.Ok();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }


        /// <summary>
        /// Mirrors fields from a single /Players/{uid} snapshot to local saves (PlayerSave.*).
        /// Supports embedded mirrors: Currencies map, Purchased* arrays, BattlePass and Rewards blocks.
        /// </summary>
        private static void ApplySinglePlayerDocToLocal(DocumentSnapshot snap, out bool hasDaily, out bool hasNewPlayer)
        {
            hasDaily = false; hasNewPlayer = false;
            var data = snap.ToDictionary();

            if (TryGet<string>(data, "PlayerName", out var pName) && !string.IsNullOrEmpty(pName))
                PlayerSave.SetPlayerName(pName);
            if (TryGet<string>(data, "PlayerIcon", out var pIcon) && !string.IsNullOrEmpty(pIcon))
                PlayerSave.SetPlayerIcon(pIcon);
            if (TryGet<string>(data, "PlayerFrame", out var pFrame) && !string.IsNullOrEmpty(pFrame))
                PlayerSave.SetPlayerFrame(pFrame);

            if (TryGet<long>(data, "selectedCharacter", out var selected))
                PlayerSave.SetSelectedCharacter(Convert.ToInt32(selected));
            if (TryGet<long>(data, "PlayerCharacterFavourite", out var fav))
                PlayerSave.SetFavouriteCharacter(Convert.ToInt32(fav));

            if (TryGet<long>(data, "AccountLevel", out var aLvl))
                PlayerSave.SetAccountLevel(Convert.ToInt32(aLvl));
            else
                PlayerSave.SetAccountLevel(1);

            if (TryGet<long>(data, "AccountCurrentExp", out var aExp))
                PlayerSave.SetAccountCurrentExp(Convert.ToInt32(aExp));
            else
                PlayerSave.SetAccountCurrentExp(0);

            ApplyCurrenciesFromRootMap(data);

            if (TryGetDict(data, "BattlePass", out var bp))
            {
                int xp = GetInt(bp, "CurrentXP");
                int lvl = GetInt(bp, "CurrentLevel", 1);
                bool prem = GetBool(bp, "IsUnlocked");
                PlayerSave.SetBattlePassProgress(xp, lvl, prem);
            }

            if (TryGetDict(data, "DailyRewards", out var daily))
            {
                hasDaily = true;
                var claimed = GetIntList(daily, "claimedRewards") ?? new List<int>();
                var first = GetDate(daily, "firstClaimDate", DateTime.Now.Date);
                var last = GetDate(daily, "lastClaimDate", DateTime.MinValue);
                PlayerSave.SetDailyRewardsLocal(new DailyRewardsData { firstClaimDate = first.Date, lastClaimDate = last.Date, claimedRewards = claimed });
            }

            if (TryGetDict(data, "NewPlayerRewards", out var newbie))
            {
                hasNewPlayer = true;
                var claimed = GetIntList(newbie, "claimedRewards") ?? new List<int>();
                var accDate = GetDate(newbie, "accountCreationDate", DateTime.Now.Date);
                var last = GetDate(newbie, "lastClaimDate", DateTime.MinValue);
                PlayerSave.SetNewPlayerRewardsLocal(new NewPlayerRewardsData { accountCreationDate = accDate.Date, lastClaimDate = last.Date, claimedRewards = claimed });
            }

            if (TryGet<long>(data, "score", out var scoreNum))
                PlayerSave.SetScore(Convert.ToInt32(scoreNum));
            else if (TryGet<string>(data, "score", out var scoreStr))
            {
                if (int.TryParse(scoreStr, out int parsedScore))
                    PlayerSave.SetScore(parsedScore);
            }

            bool any = false;

            var ch = ParseJsonListField<PurchasedCharacter>(data, "PurchasedCharacters", out bool p1)
                     ?? ParseListOfMaps<PurchasedCharacter>(data, "PurchasedCharacters", out p1);
            if (p1) { PlayerSave.SetCharacters(ch ?? new()); any = true; }

            var ic = ParseJsonListField<PurchasedIcon>(data, "PurchasedIcons", out bool p2)
                     ?? ParseListOfMaps<PurchasedIcon>(data, "PurchasedIcons", out p2);
            if (p2) { PlayerSave.SetIcons(ic ?? new()); any = true; }

            var fr = ParseJsonListField<PurchasedFrame>(data, "PurchasedFrames", out bool p3)
                     ?? ParseListOfMaps<PurchasedFrame>(data, "PurchasedFrames", out p3);
            if (p3) { PlayerSave.SetFrames(fr ?? new()); any = true; }

            var si = ParseJsonListField<PurchasedShopItem>(data, "PurchasedShopItems", out bool p4)
                     ?? ParseListOfMaps<PurchasedShopItem>(data, "PurchasedShopItems", out p4);
            if (p4) { PlayerSave.SetShopItems(si ?? new()); any = true; }

            //var inv = ParseJsonListField<PurchasedInventoryItem>(data, "PurchasedInventoryItems", out bool p5)
            //          ?? ParseInventoryFromMapList(data, "PurchasedInventoryItems", out p5);
            //if (p5) { PlayerSave.SetInventoryItems(inv ?? new()); any = true; }

            if (any) PlayerSave.SaveAllPurchased();
        }

        /// <summary>
        /// Reads Currencies map (coinId -> amount) from the root document and applies locally.
        /// If missing, keeps current local/default values without extra reads.
        /// </summary>
        private static void ApplyCurrenciesFromRootMap(IDictionary<string, object> playerData)
        {
            if (!playerData.TryGetValue("Currencies", out var obj) || obj is not IDictionary<string, object> map)
                return;

            foreach (var kv in map)
            {
                try
                {
                    int amount = Convert.ToInt32(kv.Value);
                    MonetizationManager.SetCurrency(kv.Key, amount);
                }
                catch { }
            }
        }

        private static async UniTask ApplyRewardsFromProgressDocsAsync(string uid, bool needDaily, bool needNewPlayer)
        {
            var col = Firestore.Collection("Players").Document(uid).Collection("Progress");

            if (needDaily)
            {
                var dSnap = await col.Document("DailyRewards").GetSnapshotAsync();
                if (dSnap.Exists)
                {
                    var claimed = dSnap.ContainsField("claimedRewards")
                        ? dSnap.GetValue<IEnumerable<object>>("claimedRewards").Select(o => Convert.ToInt32(o)).ToList()
                        : new List<int>();

                    DateTime first = dSnap.ContainsField("firstClaimDate") ? dSnap.GetValue<Timestamp>("firstClaimDate").ToDateTime().Date : DateTime.Now.Date;
                    DateTime last = dSnap.ContainsField("lastClaimDate") ? dSnap.GetValue<Timestamp>("lastClaimDate").ToDateTime().Date : DateTime.MinValue;

                    PlayerSave.SetDailyRewardsLocal(new DailyRewardsData { firstClaimDate = first, lastClaimDate = last, claimedRewards = claimed });
                }
            }

            if (needNewPlayer)
            {
                var nSnap = await col.Document("NewPlayerRewards").GetSnapshotAsync();
                if (nSnap.Exists)
                {
                    var claimed = nSnap.ContainsField("claimedRewards")
                        ? nSnap.GetValue<IEnumerable<object>>("claimedRewards").Select(o => Convert.ToInt32(o)).ToList()
                        : new List<int>();

                    DateTime acc = nSnap.ContainsField("accountCreationDate") ? nSnap.GetValue<Timestamp>("accountCreationDate").ToDateTime().Date : DateTime.Now.Date;
                    DateTime last = nSnap.ContainsField("lastClaimDate") ? nSnap.GetValue<Timestamp>("lastClaimDate").ToDateTime().Date : DateTime.MinValue;

                    PlayerSave.SetNewPlayerRewardsLocal(new NewPlayerRewardsData { accountCreationDate = acc, lastClaimDate = last, claimedRewards = claimed });
                }
            }
        }

        /// <summary>
        /// Loads progress documents from /Players/{uid}/Progress and mirrors them locally:
        /// - UnlockedMaps: boolean map { "mapId": true } -> PlayerSave.SetUnlockedMaps(...)
        /// - Quests: numeric progress by questId and "Complete {id}" flags
        /// - BattlePass: CurrentXP, CurrentLevel, IsUnlocked
        /// - BattlePassRewards: claimed reward ids -> PlayerSave.MarkBattlePassReward(...)
        /// </summary>
        private static async UniTask ApplyProgressFromDocsAsync(string uid)
        {
            var col = Firestore.Collection("Players").Document(uid).Collection("Progress");
            // UnlockedMaps
            try
            {
                var mapsSnap = await col.Document("UnlockedMaps").GetSnapshotAsync();
                if (mapsSnap.Exists)
                {
                    var list = new List<int>();
                    var dict = mapsSnap.ToDictionary();
                    foreach (var kv in dict)
                    {
                        if (int.TryParse(kv.Key, out int mapId) && IsTruthy(kv.Value))
                            list.Add(mapId);
                    }
                    if (list.Count > 0)
                        PlayerSave.SetUnlockedMaps(list);
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
            // Quests (progress and "Complete {id}")
            try
            {
                var qSnap = await col.Document("Quests").GetSnapshotAsync();
                if (qSnap.Exists)
                {
                    var dict = qSnap.ToDictionary();
                    foreach (var kv in dict)
                    {
                        var key = kv.Key;
                        // Completion key: "Complete {id}"
                        const string prefix = "Complete ";
                        if (key.StartsWith(prefix, StringComparison.Ordinal))
                        {
                            var part = key.Substring(prefix.Length);
                            if (int.TryParse(part, out int qid) && IsTruthy(kv.Value))
                                PlayerSave.SaveQuestCompletion(qid);
                            continue;
                        }
                        // Numeric questId -> progress value
                        if (int.TryParse(key, out int questId))
                        {
                            int val = SafeToInt(kv.Value);
                            if (val > 0)
                                PlayerSave.SaveQuestProgress(questId, val);
                        }
                    }
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
            // BattlePass (XP/Level/IsUnlocked)
            try
            {
                var bpSnap = await col.Document("BattlePass").GetSnapshotAsync();
                if (bpSnap.Exists)
                {
                    var d = bpSnap.ToDictionary();
                    int xp = d.ContainsKey("CurrentXP") ? SafeToInt(d["CurrentXP"]) : 0;
                    int lvl = d.ContainsKey("CurrentLevel") ? SafeToInt(d["CurrentLevel"]) : 1;
                    bool prem = d.ContainsKey("IsUnlocked") && IsTruthy(d["IsUnlocked"]);
                    PlayerSave.SetBattlePassProgress(xp, lvl, prem);
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
            // BattlePassRewards (claimed flags)
            try
            {
                var brSnap = await col.Document("BattlePassRewards").GetSnapshotAsync();
                if (brSnap.Exists)
                {
                    var dict = brSnap.ToDictionary();
                    foreach (var kv in dict)
                    {
                        if (IsTruthy(kv.Value))
                            PlayerSave.MarkBattlePassReward(kv.Key);
                    }
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        /// <summary>
        /// Loads character data without using collection listing (queries are disallowed by rules).
        /// Performs direct GET on known character ids from GameInstance.
        /// Mirrors:
        /// - CharacterLevel, CharacterCurrentExp
        /// - CharacterMasteryLevel, CharacterCurrentMasteryExp
        /// - CharacterSelectedSkin, UnlockedSkins[]
        /// - Slots map (slotName -> guid)
        /// - Upgrades/Stats map (StatType -> level)
        /// </summary>
        private static async UniTask ApplyCharactersFromDocsAsync(string uid)
        {
            var gi = GameInstance.Singleton;
            if (gi == null || gi.characterData == null || gi.characterData.Length == 0)
                return;

            // Fetch each character doc individually (allowed by rules: allow get, deny list)
            var tasks = new List<UniTask>();
            var charsCol = Firestore.Collection("Players").Document(uid).Collection("Characters");

            foreach (var cd in gi.characterData)
            {
                if (cd == null) continue;
                var docRef = charsCol.Document(cd.characterId.ToString());
                tasks.Add(ApplySingleCharacterGetAsync(docRef));
            }
            await UniTask.WhenAll(tasks);
        }

        /// <summary>
        /// Safely gets a character document and applies it if it exists.
        /// </summary>
        private static async UniTask ApplySingleCharacterGetAsync(DocumentReference docRef)
        {
            try
            {
                var snap = await docRef.GetSnapshotAsync();
                if (snap.Exists)
                    await ApplySingleCharacterDocAsync(snap);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Mirrors a single character document to local save.
        /// Also attempts to load Upgrades/Stats subdocument.
        /// </summary>
        private static async UniTask ApplySingleCharacterDocAsync(DocumentSnapshot doc)
        {
            if (!int.TryParse(doc.Id, out int cid) || cid <= 0) return;

            var data = doc.ToDictionary();

            // Basic level/exp
            if (data.TryGetValue("CharacterLevel", out var lvlObj))
                PlayerSave.SetCharacterLevel(cid, SafeToInt(lvlObj));

            if (data.TryGetValue("CharacterCurrentExp", out var expObj))
                PlayerSave.SetCharacterCurrentExp(cid, SafeToInt(expObj));

            // Mastery (if present)
            if (data.TryGetValue("CharacterMasteryLevel", out var mlObj))
                PlayerSave.SetCharacterMasteryLevel(cid, SafeToInt(mlObj));

            if (data.TryGetValue("CharacterCurrentMasteryExp", out var mxpObj))
                PlayerSave.SetCharacterCurrentMasteryExp(cid, SafeToInt(mxpObj));

            // Selected skin
            if (data.TryGetValue("CharacterSelectedSkin", out var skinObj))
            {
                int skinIndex = SafeToInt(skinObj);
                if (skinIndex >= 0) PlayerSave.SetCharacterSkin(cid, skinIndex);
            }

            // Unlocked skins
            if (data.TryGetValue("UnlockedSkins", out var skinsObj) && skinsObj is IEnumerable list)
            {
                var unlocked = new List<int>();
                foreach (var e in list) { try { unlocked.Add(Convert.ToInt32(e)); } catch { } }
                PlayerSave.SaveCharacterUnlockedSkins(cid, unlocked);
            }

            foreach (var kv in data)
            {
                const string p = "Slots.";
                if (kv.Key.StartsWith(p, StringComparison.Ordinal))
                {
                    string slot = kv.Key.Substring(p.Length);
                    PlayerSave.SetCharacterSlotItem(cid, slot, kv.Value?.ToString() ?? "");
                }
            }

            // Slots map (slotName -> guid)
            if (data.TryGetValue("Slots", out var slotsObj) && slotsObj is IDictionary<string, object> slotsMap)
            {
                foreach (var kv in slotsMap)
                {
                    string slot = kv.Key;
                    string guid = kv.Value?.ToString() ?? "";
                    PlayerSave.SetCharacterSlotItem(cid, slot, guid);
                }
            }
            // Upgrades/Stats subdocument
            try
            {
                var upSnap = await doc.Reference.Collection("Upgrades").Document("Stats").GetSnapshotAsync();
                if (upSnap.Exists)
                {
                    var up = upSnap.ToDictionary();
                    foreach (var kv in up)
                    {
                        if (TryParseStatType(kv.Key, out StatType stat))
                            PlayerSave.SetCharacterUpgradeLevel(cid, stat, SafeToInt(kv.Value));
                    }
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        private static async UniTask ApplyPurchasesFromSubdocsAsync(string uid)
        {
            var baseCol = Firestore.Collection("Players").Document(uid).Collection("PurchasedItems");

            async UniTask apply(string docName, string fieldName, Action<List<string>> onJsonList)
            {
                try
                {
                    var snap = await baseCol.Document(docName).GetSnapshotAsync();
                    if (!snap.Exists) return;
                    if (!snap.ContainsField(fieldName)) return;

                    var raw = snap.GetValue<IEnumerable<object>>(fieldName);
                    var list = new List<string>();
                    foreach (var e in raw) if (e != null) list.Add(e.ToString());
                    onJsonList(list);
                }
                catch (Exception ex) { Debug.LogException(ex); }
            }

            await apply("Characters", "Characters", jsons =>
            {
                var outList = new List<PurchasedCharacter>();
                foreach (var j in jsons) { try { outList.Add(JsonUtility.FromJson<PurchasedCharacter>(j)); } catch { } }
                PlayerSave.SetCharacters(outList);
            });

            await apply("Icons", "Icons", jsons =>
            {
                var outList = new List<PurchasedIcon>();
                foreach (var j in jsons) { try { outList.Add(JsonUtility.FromJson<PurchasedIcon>(j)); } catch { } }
                PlayerSave.SetIcons(outList);
            });

            await apply("Frames", "Frames", jsons =>
            {
                var outList = new List<PurchasedFrame>();
                foreach (var j in jsons) { try { outList.Add(JsonUtility.FromJson<PurchasedFrame>(j)); } catch { } }
                PlayerSave.SetFrames(outList);
            });

            await apply("ShopItems", "ShopItems", jsons =>
            {
                var outList = new List<PurchasedShopItem>();
                foreach (var j in jsons) { try { outList.Add(JsonUtility.FromJson<PurchasedShopItem>(j)); } catch { } }
                PlayerSave.SetShopItems(outList);
            });

            PlayerSave.SaveAllPurchased();
        }

        private static async UniTask ApplyInventoryFromListAsync(string uid)
        {
            var listCol = Firestore.Collection("Players").Document(uid)
                           .Collection("PurchasedItems").Document("Items")
                           .Collection("List");

            try
            {
                var snaps = await listCol.GetSnapshotAsync(); 
                var invs = new List<PurchasedInventoryItem>();

                foreach (var d in snaps.Documents)
                {
                    var m = d.ToDictionary();
                    var it = new PurchasedInventoryItem
                    {
                        uniqueItemGuid = m.TryGetValue("uniqueItemGuid", out var g) ? g?.ToString() : d.Id,
                        itemId = m.TryGetValue("itemId", out var id) ? id?.ToString() : null,
                        itemLevel = m.TryGetValue("itemLevel", out var lvl) ? SafeToInt(lvl) : 0,
                        upgrades = new Dictionary<int, int>()
                    };

                    if (m.TryGetValue("itemUpgrades", out var up) && up is IDictionary<string, object> upMap)
                        foreach (var kv in upMap)
                            if (int.TryParse(kv.Key, out int idx))
                                it.upgrades[idx] = SafeToInt(kv.Value);

                    invs.Add(it);

                    PlayerSave.SetItemUpgradeLevel(it.uniqueItemGuid, it.itemLevel);
                }

                PlayerSave.SetInventoryItems(invs);
                PlayerSave.SaveAllPurchased();   
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        private static async UniTask ApplyBattlePassSeasonInfoAsync()
        {
            try
            {
                // collection root: BattlePass  ->  doc: SeasonInfo
                var docRef = Firestore.Collection("BattlePass").Document("SeasonInfo");
                var snap = await docRef.GetSnapshotAsync();
                if (!snap.Exists)
                {
                    FallbackSeasonMeta();
                    return;
                }

                int season = snap.ContainsField("Season") ? SafeToInt(snap.GetValue<object>("Season")) : 1;
                DateTime start = DateTime.UtcNow;
                if (snap.ContainsField("StartSeason"))
                {
                    var ts = snap.GetValue<Timestamp>("StartSeason");
                    start = ts.ToDateTime();       
                }

                int duration = 60;
                if (snap.ContainsField("DurationDays"))
                    duration = SafeToInt(snap.GetValue<object>("DurationDays"));
                else if (GameInstance.Singleton != null)
                    duration = GameInstance.Singleton.battlePassDurationDays;

                PlayerSave.SetBattlePassSeasonMeta(season, start, duration);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                FallbackSeasonMeta();
            }

            void FallbackSeasonMeta()
            {
                int defDuration = GameInstance.Singleton != null ?
                                  GameInstance.Singleton.battlePassDurationDays : 60;
                PlayerSave.SetBattlePassSeasonMeta(1, DateTime.UtcNow, defDuration);
            }
        }


        // ───────────────────── helpers ─────────────────────

        private static bool TryGet<T>(IDictionary<string, object> data, string key, out T value)
        {
            value = default;
            if (!data.TryGetValue(key, out var obj) || obj == null) return false;
            try
            {
                if (obj is T t) { value = t; return true; }
                if (typeof(T) == typeof(int) && obj is long l) { value = (T)(object)Convert.ToInt32(l); return true; }
                if (typeof(T) == typeof(long) && obj is int i) { value = (T)(object)Convert.ToInt64(i); return true; }
                if (typeof(T) == typeof(string)) { value = (T)(object)obj.ToString(); return true; }
            }
            catch { }
            return false;
        }

        private static bool TryGetDict(IDictionary<string, object> data, string key, out IDictionary<string, object> dict)
        {
            dict = null;
            if (!data.TryGetValue(key, out var obj) || obj == null) return false;
            dict = obj as IDictionary<string, object>;
            return dict != null;
        }

        private static int GetInt(IDictionary<string, object> d, string key, int def = 0)
        {
            if (!d.TryGetValue(key, out var v) || v == null) return def;
            try { return Convert.ToInt32(v); } catch { return def; }
        }

        private static bool GetBool(IDictionary<string, object> d, string key, bool def = false)
        {
            if (!d.TryGetValue(key, out var v) || v == null) return def;
            try { return Convert.ToBoolean(v); } catch { return def; }
        }

        private static DateTime GetDate(IDictionary<string, object> d, string key, DateTime def)
        {
            if (!d.TryGetValue(key, out var v) || v == null) return def;

            // Firebase Timestamp
            if (v is Timestamp ts) return ts.ToDateTime();

            // ISO/string
            if (DateTime.TryParse(v.ToString(), out var parsed))
                return parsed;

            // (epoch ms ou s)
            if (long.TryParse(v.ToString(), out var n))
            {
                var epoch = DateTime.UnixEpoch;
                return (n > 10_000_000_000L) ? epoch.AddMilliseconds(n) : epoch.AddSeconds(n);
            }
            return def;
        }

        private static List<int> GetIntList(IDictionary<string, object> d, string key)
        {
            if (!d.TryGetValue(key, out var v) || v is not IEnumerable list) return null;
            var result = new List<int>();
            foreach (var e in list)
            {
                try { result.Add(Convert.ToInt32(e)); } catch { }
            }
            return result;
        }

        /// <summary>
        /// Parses a JSON-string array field (compat) into a typed list.
        /// </summary>
        private static List<T> ParseJsonListField<T>(IDictionary<string, object> data, string key, out bool keyWasPresent)
        {
            keyWasPresent = false;
            if (!data.ContainsKey(key)) return null;
            keyWasPresent = true;

            var obj = data[key];
            if (obj is IEnumerable raw)
            {
                var result = new List<T>();
                foreach (var e in raw)
                {
                    if (e is string json && !string.IsNullOrEmpty(json))
                    {
                        try { result.Add(JsonUtility.FromJson<T>(json)); } catch { }
                    }
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// Parses a list of maps (object array) into a typed list using JsonUtility as bridge.
        /// </summary>
        private static List<T> ParseListOfMaps<T>(IDictionary<string, object> data, string key, out bool keyWasPresent)
        {
            keyWasPresent = false;
            if (!data.TryGetValue(key, out var obj) || obj is not IEnumerable raw) return null;
            keyWasPresent = true;

            var result = new List<T>();
            foreach (var e in raw)
            {
                if (e is IDictionary<string, object> m)
                {
                    try
                    {
                        string json = MiniJsonSerialize(m);
                        result.Add(JsonUtility.FromJson<T>(json));
                    }
                    catch { }
                }
            }
            return result;
        }

        /// <summary>
        /// Parses inventory list when stored as array of maps.
        /// </summary>
        private static List<PurchasedInventoryItem> ParseInventoryFromMapList(IDictionary<string, object> data, string key, out bool keyWasPresent)
        {
            keyWasPresent = false;
            if (!data.TryGetValue(key, out var obj) || obj is not IEnumerable raw) return null;
            keyWasPresent = true;

            var result = new List<PurchasedInventoryItem>();
            foreach (var e in raw)
            {
                if (e is IDictionary<string, object> m)
                {
                    var pi = new PurchasedInventoryItem
                    {
                        uniqueItemGuid = m.TryGetValue("uniqueItemGuid", out var g) ? g?.ToString() : null,
                        itemId = m.TryGetValue("itemId", out var id) ? id?.ToString() : null,
                        itemLevel = m.TryGetValue("itemLevel", out var lvl) ? SafeToInt(lvl) : 0,
                        upgrades = new Dictionary<int, int>()
                    };

                    if (m.TryGetValue("itemUpgrades", out var up) && up is IDictionary<string, object> upMap)
                    {
                        foreach (var kv in upMap)
                        {
                            if (int.TryParse(kv.Key, out int idx))
                                pi.upgrades[idx] = SafeToInt(kv.Value);
                        }
                    }
                    result.Add(pi);
                }
            }
            return result;
        }

        private static int SafeToInt(object v)
        {
            try { return Convert.ToInt32(v); } catch { return 0; }
        }

        /// <summary>
        /// Minimal map→JSON serializer for small dictionaries (evita libs extras).
        /// </summary>
        private static string MiniJsonSerialize(IDictionary<string, object> map)
        {
            var parts = new List<string>();
            foreach (var kv in map)
            {
                string k = $"\"{kv.Key}\"";
                string val = kv.Value switch
                {
                    null => "null",
                    string s => $"\"{EscapeJsonString(s)}\"",
                    bool b => b ? "true" : "false",
                    IDictionary<string, object> sub => MiniJsonSerialize(sub),
                    IEnumerable arr when !(kv.Value is string) => MiniJsonArray(arr),
                    Timestamp ts => $"\"{ts.ToDateTime():O}\"",
                    _ => Convert.ToString(kv.Value, System.Globalization.CultureInfo.InvariantCulture)
                };
                parts.Add($"{k}:{val}");
            }
            return "{" + string.Join(",", parts) + "}";
        }

        private static string MiniJsonArray(IEnumerable arr)
        {
            var parts = new List<string>();
            foreach (var v in arr)
            {
                parts.Add(v switch
                {
                    null => "null",
                    string s => $"\"{EscapeJsonString(s)}\"",
                    bool b => b ? "true" : "false",
                    IDictionary<string, object> sub => MiniJsonSerialize(sub),
                    Timestamp ts => $"\"{ts.ToDateTime():O}\"",
                    _ => Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture)
                });
            }
            return "[" + string.Join(",", parts) + "]";
        }

        private static string EscapeJsonString(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        /// <summary>Returns true for common truthy values (bool true, non-zero numbers, "true"/"1").</summary>
        private static bool IsTruthy(object v)
        {
            try
            {
                if (v == null) return false;
                if (v is bool b) return b;
                if (v is long l) return l != 0;
                if (v is int i) return i != 0;
                var s = v.ToString().Trim().ToLowerInvariant();
                return s == "true" || s == "1";
            }
            catch { return false; }
        }

        /// <summary>Parses StatType enum names safely (case-sensitive by default).</summary>
        private static bool TryParseStatType(string name, out StatType stat)
        {
            try { return Enum.TryParse(name, ignoreCase: false, out stat); }
            catch { stat = default; return false; }
        }
    }
}
#endif