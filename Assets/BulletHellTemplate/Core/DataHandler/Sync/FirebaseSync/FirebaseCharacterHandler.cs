#if FIREBASE
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Character operations backed by Firestore cloud-saves.
    /// Validation and effects are applied locally; Firestore is used only to persist state.
    /// Writes are compact (one batch) and avoid dotted keys.
    /// </summary>
    public static class FirebaseCharacterHandler
    {
        /* ────────────── Firebase shortcuts ────────────── */

        private static FirebaseAuth Auth => FirebaseAuthHandler.Auth;
        private static FirebaseFirestore Db => FirebaseAuthHandler.Firestore;

        private static string UidOrNull() => Auth?.CurrentUser?.UserId;

        private static DocumentReference PlayerDoc(string uid) =>
            Db.Collection("Players").Document(uid);

        private static DocumentReference CharacterDoc(string uid, int cid) =>
            PlayerDoc(uid).Collection("Characters").Document(cid.ToString());

        /// <summary>
        /// Adds currency writes to batch: subcollection docs (Currencies/{id}) + root Currencies map.
        /// Keeps 1-read loading layout and avoids dotted keys.
        /// </summary>
        private static void EnqueueCurrencies(WriteBatch batch, string uid, Dictionary<string, int> balances)
        {
            if (balances == null || balances.Count == 0) return;

            var rootMap = new Dictionary<string, object>();
            foreach (var kv in balances)
            {
                rootMap[kv.Key] = kv.Value;
                var cDoc = PlayerDoc(uid).Collection("Currencies").Document(kv.Key);
                batch.Set(cDoc, new Dictionary<string, object> { { "amount", kv.Value } }, SetOptions.MergeAll);
            }
            batch.Set(PlayerDoc(uid), new Dictionary<string, object> { { "Currencies", rootMap } }, SetOptions.MergeAll);
        }

        /* ────────────── Public API ────────────── */

        /// <summary>Sets the selected character (local + cloud field "selectedCharacter").</summary>
        public static async UniTask<RequestResult> TrySelectCharacterAsync(int cid)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = UidOrNull();
            if (uid == null) return RequestResult.Fail("invalid_credentials");

            var def = GameInstance.Singleton.GetCharacterDataById(cid);
            if (def == null) return RequestResult.Fail("0");

            bool owned = def.CheckUnlocked || PlayerSave.IsCharacterPurchased(cid.ToString());
            if (!owned) return RequestResult.Fail("1");

            PlayerSave.SetSelectedCharacter(cid);

            try
            {
                var batch = Db.StartBatch();
                batch.Set(PlayerDoc(uid), new Dictionary<string, object> { { "selectedCharacter", cid } }, SetOptions.MergeAll);
                await batch.CommitAsync();
                return RequestResult.Ok();
            }
            catch (Exception ex) { Debug.LogException(ex); return RequestResult.Fail("generic_error"); }
        }

        /// <summary>Marks a character as favourite (local + cloud field "PlayerCharacterFavourite").</summary>
        public static async UniTask<RequestResult> UpdatePlayerCharacterFavouriteAsync(int cid)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = UidOrNull();
            if (uid == null) return RequestResult.Fail("invalid_credentials");

            var def = GameInstance.Singleton.GetCharacterDataById(cid);
            if (def == null) return RequestResult.Fail("0");

            bool owned = def.CheckUnlocked || PlayerSave.IsCharacterPurchased(cid.ToString());
            if (!owned) return RequestResult.Fail("1");

            PlayerSave.SetFavouriteCharacter(cid);

            try
            {
                var batch = Db.StartBatch();
                batch.Set(PlayerDoc(uid), new Dictionary<string, object> { { "PlayerCharacterFavourite", cid } }, SetOptions.MergeAll);
                await batch.CommitAsync();
                return RequestResult.Ok();
            }
            catch (Exception ex) { Debug.LogException(ex); return RequestResult.Fail("generic_error"); }
        }

        /// <summary>
        /// Unlocks a character skin using local balance, then persists new balance and skin state.
        /// Writes: Currencies, Characters/{cid} (UnlockedSkins[], CharacterSelectedSkin).
        /// </summary>
        public static async UniTask<RequestResult> TryUnlockCharacterSkinAsync(int cid, int skinIndex)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = UidOrNull();
            if (uid == null) return RequestResult.Fail("invalid_credentials");

            var def = GameInstance.Singleton.GetCharacterDataById(cid);
            if (def == null || def.characterSkins == null || skinIndex < 0 || skinIndex >= def.characterSkins.Count())
                return RequestResult.Fail("0");

            var skin = def.characterSkins[skinIndex];

            var unlocked = PlayerSave.LoadCharacterUnlockedSkins(cid);
            if (skin.isUnlocked || unlocked.Contains(skinIndex)) return RequestResult.Fail("2");

            int bal = MonetizationManager.GetCurrency(skin.unlockCurrencyId);
            if (bal < skin.unlockPrice) return RequestResult.Fail("1");

            // Local apply
            MonetizationManager.SetCurrency(skin.unlockCurrencyId, bal - skin.unlockPrice);
            unlocked.Add(skinIndex);
            PlayerSave.SaveCharacterUnlockedSkins(cid, unlocked);
            PlayerSave.SetCharacterSkin(cid, skinIndex);

            // Cloud save
            try
            {
                var batch = Db.StartBatch();

                EnqueueCurrencies(batch, uid, new Dictionary<string, int> { { skin.unlockCurrencyId, bal - skin.unlockPrice } });

                var cDoc = CharacterDoc(uid, cid);
                batch.Set(cDoc, new Dictionary<string, object>
                {
                    { "CharacterSelectedSkin", skinIndex },
                    { "UnlockedSkins", new List<int>(unlocked) }
                }, SetOptions.MergeAll);

                await batch.CommitAsync();
                return RequestResult.Ok();
            }
            catch (Exception ex) { Debug.LogException(ex); return RequestResult.Fail("generic_error"); }
        }

        /// <summary>
        /// Sets an active skin already unlocked. Writes Characters/{cid}.CharacterSelectedSkin.
        /// </summary>
        public static async UniTask<RequestResult> UpdateCharacterSkinAsync(int cid, int skinIndex)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = UidOrNull();
            if (uid == null) return RequestResult.Fail("invalid_credentials");

            var def = GameInstance.Singleton.GetCharacterDataById(cid);
            if (def == null || def.characterSkins == null || skinIndex < 0 || skinIndex >= def.characterSkins.Count())
                return RequestResult.Fail("0");

            var unlocked = PlayerSave.LoadCharacterUnlockedSkins(cid);
            if (!def.characterSkins[skinIndex].isUnlocked && !unlocked.Contains(skinIndex))
                return RequestResult.Fail("1");

            PlayerSave.SetCharacterSkin(cid, skinIndex);

            try
            {
                var batch = Db.StartBatch();
                batch.Set(CharacterDoc(uid, cid), new Dictionary<string, object> { { "CharacterSelectedSkin", skinIndex } }, SetOptions.MergeAll);
                await batch.CommitAsync();
                return RequestResult.Ok();
            }
            catch (Exception ex) { Debug.LogException(ex); return RequestResult.Fail("generic_error"); }
        }

        /// <summary>
        /// Performs character level up using local EXP and currency. Writes: currency, level, and EXP remainder.
        /// </summary>
        public static async UniTask<RequestResult> TryCharacterLevelUpAsync(int cid)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = UidOrNull();
            if (uid == null) return RequestResult.Fail("invalid_credentials");

            var cd = GameInstance.Singleton.GetCharacterDataById(cid);
            if (cd == null) return RequestResult.Fail("0");

            int lvl = PlayerSave.GetCharacterLevel(cid);
            int curExp = PlayerSave.GetCharacterCurrentExp(cid);
            if (lvl >= cd.maxLevel) return RequestResult.Fail("0");

            int needExp = (lvl < cd.expPerLevel.Length) ? cd.expPerLevel[lvl] : int.MaxValue;
            int needCost = (lvl < cd.upgradeCostPerLevel.Length) ? cd.upgradeCostPerLevel[lvl] : int.MaxValue;

            if (curExp < needExp) return RequestResult.Fail("1");

            int bal = MonetizationManager.GetCurrency(cd.currencyId);
            if (bal < needCost) return RequestResult.Fail("2");

            // Local apply
            MonetizationManager.SetCurrency(cd.currencyId, bal - needCost);
            PlayerSave.SetCharacterCurrentExp(cid, curExp - needExp);
            PlayerSave.SetCharacterLevel(cid, lvl + 1);

            // Cloud save
            try
            {
                var batch = Db.StartBatch();

                EnqueueCurrencies(batch, uid, new Dictionary<string, int> { { cd.currencyId, bal - needCost } });

                batch.Set(CharacterDoc(uid, cid), new Dictionary<string, object>
                {
                    { "CharacterLevel",         lvl + 1 },
                    { "CharacterCurrentExp",    curExp - needExp }
                }, SetOptions.MergeAll);

                await batch.CommitAsync();
                return RequestResult.Ok();
            }
            catch (Exception ex) { Debug.LogException(ex); return RequestResult.Fail("generic_error"); }
        }

        /// <summary>
        /// Attempts to upgrade a character stat: local validation + deduction, then writes currency and stat level.
        /// Stat levels are stored at /Players/{uid}/Characters/{cid}/Upgrades/Stats as a map { StatType: level }.
        /// </summary>
        public static async UniTask<RequestResult> TryUpgradeStatAsync(
            StatUpgrade statUpgrade,
            CharacterStatsRuntime stats,
            int characterId)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = UidOrNull();
            if (uid == null) return RequestResult.Fail("invalid_credentials");

            int currentLevel = PlayerSave.GetCharacterUpgradeLevel(characterId, statUpgrade.statType);
            int nextLevel = currentLevel + 1;

            if (currentLevel >= statUpgrade.upgradeMaxLevel)
                return RequestResult.Fail("0");

            int cost = statUpgrade.GetUpgradeCost(nextLevel);
            int balance = MonetizationManager.GetCurrency(statUpgrade.currencyTag);
            if (balance < cost)
                return RequestResult.Fail("1");

            // Local apply
            MonetizationManager.SetCurrency(statUpgrade.currencyTag, balance - cost);
            statUpgrade.ApplyUpgrade(stats, nextLevel);
            PlayerSave.SetCharacterUpgradeLevel(characterId, statUpgrade.statType, nextLevel);

            // Cloud save
            try
            {
                var batch = Db.StartBatch();

                EnqueueCurrencies(batch, uid, new Dictionary<string, int> { { statUpgrade.currencyTag, balance - cost } });

                var statsDoc = CharacterDoc(uid, characterId).Collection("Upgrades").Document("Stats");
                batch.Set(statsDoc, new Dictionary<string, object>
                {
                    { statUpgrade.statType.ToString(), nextLevel }
                }, SetOptions.MergeAll);

                await batch.CommitAsync();
                return RequestResult.Ok();
            }
            catch (Exception ex) { Debug.LogException(ex); return RequestResult.Fail("generic_error"); }
        }

        /// <summary>
        /// Equips/unequips an inventory item. Validates locally and persists a compact "Slots" map:
        /// /Players/{uid}/Characters/{cid} -> { "Slots": { slotName: uniqueGuidOrEmpty } }.
        /// </summary>
        public static async UniTask<RequestResult> SetCharacterSlotItemAsync(int characterId, string slotName, string uniqueItemGuid)  
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = UidOrNull();
            if (uid == null)
                return RequestResult.Fail("invalid_credentials");

            if (!string.IsNullOrEmpty(uniqueItemGuid))
            {
                var purchased = PlayerSave.GetInventoryItems()
                                          .Find(i => i.uniqueItemGuid == uniqueItemGuid);
                if (purchased == null)
                    return RequestResult.Fail("0");  
            }
            RemoveItemFromAllCharactersLocal(uniqueItemGuid);
            PlayerSave.SetCharacterSlotItem(characterId, slotName, uniqueItemGuid ?? "");

            try
            {
                var batch = Db.StartBatch();

                var charDoc = CharacterDoc(uid, characterId);

                var data = new Dictionary<string, object>
                {
                    ["Slots"] = new Dictionary<string, object>
                    {
                        [slotName] = uniqueItemGuid ?? ""
                    }
                };

                batch.Set(charDoc, data, SetOptions.MergeAll);

                await batch.CommitAsync();
                return RequestResult.Ok();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        /// <summary>Removes the given item GUID from all local character slots.</summary>
        private static void RemoveItemFromAllCharactersLocal(string uniqueGuid)
        {
            if (string.IsNullOrEmpty(uniqueGuid)) return;

            var chars = GameInstance.Singleton.characterData;
            if (chars == null) return;

            foreach (var cd in chars)
            {
                var slots = cd.itemSlots;
                if (slots != null)
                {
                    foreach (string slot in slots)
                    {
                        if (PlayerSave.GetCharacterSlotItem(cd.characterId, slot) == uniqueGuid)
                            PlayerSave.SetCharacterSlotItem(cd.characterId, slot, "");
                    }
                }
                if (cd.runeSlots != null)
                {
                    foreach (string slot in cd.runeSlots)
                    {
                        if (PlayerSave.GetCharacterSlotItem(cd.characterId, slot) == uniqueGuid)
                            PlayerSave.SetCharacterSlotItem(cd.characterId, slot, "");
                    }
                }
            }
        }
    }
}
#endif
