#if FIREBASE
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Cloud-save for gameplay progress (score, gold, unlocked maps, quest progress).
    /// Mirrors local changes (PlayerSave/MonetizationManager) and writes a compact Firestore batch.
    /// No reads; all validation is local.
    /// </summary>
    public static class FirebaseProgressHandler
    {
        /* ─────────────── Firebase shortcuts ─────────────── */

        private static FirebaseAuth Auth => FirebaseAuthHandler.Auth;
        private static FirebaseFirestore Db => FirebaseAuthHandler.Firestore;
        private static string UidOrNull() => Auth?.CurrentUser?.UserId;

        private static DocumentReference PlayerDoc(string uid) =>
            Db.Collection("Players").Document(uid);

        private static DocumentReference ProgressDoc(string uid, string name) =>
            PlayerDoc(uid).Collection("Progress").Document(name);

        private static DocumentReference CurrencyDoc(string uid, string coinId) =>
            PlayerDoc(uid).Collection("Currencies").Document(coinId);

        /// <summary>
        /// Adds currency writes to batch (subcollection docs + root Currencies map) to keep 1-read loading layout.
        /// </summary>
        private static void EnqueueCurrencies(WriteBatch batch, string uid, Dictionary<string, int> balances)
        {
            var rootMap = new Dictionary<string, object>();
            foreach (var kv in balances)
            {
                rootMap[kv.Key] = kv.Value;
                batch.Set(
                    CurrencyDoc(uid, kv.Key),
                    new Dictionary<string, object> { { "amount", kv.Value } },
                    SetOptions.MergeAll
                );
            }
            batch.Set(PlayerDoc(uid), new Dictionary<string, object> { { "Currencies", rootMap } }, SetOptions.MergeAll);
        }

        /* ─────────────── Public API ─────────────── */

        /// <summary>
        /// Applies end-of-run effects locally (score, gold, unlock flow, quest deltas) and persists a cloud snapshot.
        /// </summary>
        public static async UniTask<RequestResult> CompleteGameSessionAsync(EndGameSessionData data)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = UidOrNull();
            if (uid == null) return RequestResult.Fail("invalid_credentials");
            if (GameInstance.Singleton == null) return RequestResult.Fail("no_game");

            // 1) Local mirrors (same as offline)
            int newScore = PlayerSave.GetScore() + data.monstersKilled;
            PlayerSave.SetScore(newScore);

            string goldId = GameInstance.Singleton.goldCurrency;
            int curGold = MonetizationManager.GetCurrency(goldId);
            int nextGold = curGold + Math.Max(0, data.gainedGold);
            MonetizationManager.SetCurrency(goldId, nextGold);

            if (data.won)
            {
                var map = GameInstance.Singleton.GetMapInfoDataById(data.mapId);
                if (map != null
                    && map.isRewardOnCompleteFirstTime
                    && (map.WinMapRewards.Count > 0 || map.rewardType != MapRewardType.None)
                    && !(RewardManagerPopup.Singleton?.HasLocalMapClaimed(data.mapId) ?? false))
                {
                    RewardManagerPopup.Singleton?.RegisterMapCompleted(data.mapId);
                }

                if (PlayerSave.IsLatestUnlockedMap(data.mapId))
                    UnlockNextMapLocal();
            }

            // Quest deltas calculated using the same rules as Offline
            var questDeltas = ApplyEndGameProgressAndCollectDeltas(new EndGameQuestProgressData
            {
                won = data.won,
                mapId = data.mapId,
                characterId = data.characterId,
                monstersKilled = data.monstersKilled
            });

            // 2) Cloud save (single batch)
            try
            {
                var batch = Db.StartBatch();

                // score
                batch.Set(PlayerDoc(uid), new Dictionary<string, object> { { "score", newScore } }, SetOptions.MergeAll);

                // gold
                EnqueueCurrencies(batch, uid, new Dictionary<string, int> { { goldId, nextGold } });

                // unlocked maps (write all current locals)
                var unlocked = PlayerSave.GetUnlockedMaps();
                if (unlocked != null && unlocked.Count > 0)
                {
                    var mapsData = new Dictionary<string, object>();
                    foreach (int m in unlocked)
                        mapsData[m.ToString()] = true;
                    batch.Set(ProgressDoc(uid, "UnlockedMaps"), mapsData, SetOptions.MergeAll);
                }

                // quest progress deltas
                if (questDeltas.Count > 0)
                {
                    var q = new Dictionary<string, object>();
                    foreach (var kv in questDeltas)
                        q[kv.Key.ToString()] = kv.Value;
                    batch.Set(ProgressDoc(uid, "Quests"), q, SetOptions.MergeAll);
                }

                await batch.CommitAsync();
                return RequestResult.Ok();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        /// <summary>
        /// Completes a quest using local validation and applies rewards with Firebase writers.
        /// - EXP/battle-pass: uses FirebaseExpHandler (writes cloud).
        /// - Currency & entitlements: uses local state and a currency batch write.
        /// - Marks quest completion/progress in /Progress/Quests.
        /// </summary>
        public static async UniTask<RequestResult> TryCompleteQuestAsync(int questId)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = UidOrNull();
            if (uid == null) return RequestResult.Fail("invalid_credentials");

            var gi = GameInstance.Singleton;
            var quest = gi?.GetQuestItemById(questId);
            if (quest == null) return RequestResult.Fail("2");

            bool alreadyCompleted = PlayerSave.IsQuestCompleted(quest.questId);
            if (alreadyCompleted && quest.questType != QuestType.Repeat) return RequestResult.Fail("0");

            int progress = PlayerSave.LoadQuestProgress(quest.questId);
            int target = quest.requirement.targetAmount;
            if (progress < target) return RequestResult.Fail("1");

            // Rewards (apply locally + cloud writers for EXP/currency/items)
            var currencyChanges = new Dictionary<string, int>();

            // EXP to account
            if (quest.accountExp > 0)
            {
                var r = await FirebaseExpHandler.AddAccountExpAsync(quest.accountExp);
                if (!r.Success) return r;
            }

            // Battle-pass XP
            if (quest.battlePassExp > 0)
            {
                var r = await FirebaseExpHandler.AddBattlePassXpAsync(quest.battlePassExp);
                if (!r.Success) return r;
            }

            // Selected character EXP
            if (quest.selectedCharacterExp > 0)
            {
                int sel = PlayerSave.GetSelectedCharacter();
                var r = await FirebaseExpHandler.AddCharacterExpAsync(sel, quest.selectedCharacterExp);
                if (!r.Success) return r;
            }

            // Currency
            if (quest.currencyAmount > 0 && !string.IsNullOrEmpty(quest.currencyReward))
            {
                int cur = MonetizationManager.GetCurrency(quest.currencyReward);
                int next = cur + quest.currencyAmount;
                MonetizationManager.SetCurrency(quest.currencyReward, next);
                currencyChanges[quest.currencyReward] = next;
            }

            // Item/Icon/Frame/Character rewards via the BattlePass-style writer
            if (quest.questReward != QuestReward.None)
            {
                var bpi = ToBattlePassItem(quest);
                var r = await FirebasePurchasesHandler.ClaimBattlePassRewardAsync(bpi);
                if (!r.Success) return r;
            }

            // Mark completion locally
            if (quest.questType == QuestType.Repeat)
                PlayerSave.SaveQuestProgress(quest.questId, 0);
            else
                PlayerSave.SaveQuestCompletion(quest.questId);

            // Cloud save (quest completion + currency changes)
            try
            {
                var batch = Db.StartBatch();

                // Completion flag (compatible with older layout if needed)
                var qDoc = ProgressDoc(uid, "Quests");
                if (quest.questType == QuestType.Repeat)
                {
                    batch.Set(qDoc, new Dictionary<string, object> { { quest.questId.ToString(), 0 } }, SetOptions.MergeAll);
                }
                else
                {
                    batch.Set(qDoc, new Dictionary<string, object>
                    {
                        { $"Complete {quest.questId}", 1 },
                        { $"Complete {quest.questId}_Timestamp", Timestamp.GetCurrentTimestamp() }
                    }, SetOptions.MergeAll);
                }

                if (currencyChanges.Count > 0)
                    EnqueueCurrencies(batch, uid, currencyChanges);

                await batch.CommitAsync();
                return RequestResult.Ok();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        /// <summary>
        /// Syncs "level-based" quest progress (Account/Character level) from local to cloud.
        /// Replicates OfflineProgressHandler.RefreshLevelBasedProgress but also writes the deltas.
        /// </summary>
        public static async UniTask<RequestResult> RefreshLevelBasedProgressAsync()
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = UidOrNull();
            if (uid == null) return RequestResult.Fail("invalid_credentials");

            OfflineProgressHandler.RefreshLevelBasedProgress();

            var gi = GameInstance.Singleton;
            var payload = new Dictionary<string, object>();
            foreach (var quest in gi.questData)
            {
                if (quest == null) continue;

                // Only write the two level-based requirement types
                var req = quest.requirement;
                if (req.requirementType == QuestRequirementType.LevelUpAccount ||
                    req.requirementType == QuestRequirementType.LevelUpCharacter)
                {
                    int val = PlayerSave.LoadQuestProgress(quest.questId);
                    if (val > 0)
                        payload[quest.questId.ToString()] = val;
                }
            }

            if (payload.Count == 0)
                return RequestResult.Ok();

            try
            {
                await ProgressDoc(uid, "Quests").SetAsync(payload, SetOptions.MergeAll);
                return RequestResult.Ok();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        /* ─────────────── Helpers (local logic) ─────────────── */

        /// <summary>Copies Offline unlock-next-map logic locally.</summary>
        private static void UnlockNextMapLocal()
        {
            List<int> unlockedMaps = PlayerSave.GetUnlockedMaps();
            MapInfoData[] allMaps = GameInstance.Singleton.mapInfoData;

            int lastUnlockedMapId = 0;
            if (unlockedMaps.Count > 0)
            {
                lastUnlockedMapId = unlockedMaps[^1];
            }
            else
            {
                for (int i = 0; i < allMaps.Length; i++)
                {
                    if (allMaps[i].isUnlocked)
                    {
                        lastUnlockedMapId = allMaps[i].mapId;
                        break;
                    }
                }
            }

            for (int i = 0; i < allMaps.Length; i++)
            {
                if (allMaps[i].mapId == lastUnlockedMapId && i + 1 < allMaps.Length)
                {
                    int nextMapId = allMaps[i + 1].mapId;
                    if (!unlockedMaps.Contains(nextMapId))
                    {
                        unlockedMaps.Add(nextMapId);
                        PlayerSave.SetUnlockedMaps(unlockedMaps);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Applies end-game quest rules locally and returns the subset of quest progress that changed.
        /// </summary>
        private static Dictionary<int, int> ApplyEndGameProgressAndCollectDeltas(EndGameQuestProgressData data)
        {
            var changed = new Dictionary<int, int>();
            var quests = GameInstance.Singleton.questData;

            foreach (var quest in quests)
            {
                if (quest == null) continue;
                if (PlayerSave.IsQuestCompleted(quest.questId) && quest.questType != QuestType.Repeat)
                    continue;

                int before = PlayerSave.LoadQuestProgress(quest.questId);
                int after = before;

                var req = quest.requirement;
                switch (req.requirementType)
                {
                    case QuestRequirementType.KillMonster:
                        after += data.monstersKilled;
                        break;

                    case QuestRequirementType.KillMonsterWithSpecificCharacter:
                        if (req.targetCharacter != null &&
                            req.targetCharacter.characterId == data.characterId)
                            after += data.monstersKilled;
                        break;

                    case QuestRequirementType.CompleteMap:
                        if (data.won && req.targetMap != null && req.targetMap.mapId == data.mapId)
                            after++;
                        break;

                    case QuestRequirementType.CompleteMapWithSpecificCharacter:
                        if (data.won
                            && req.targetMap != null && req.targetMap.mapId == data.mapId
                            && req.targetCharacter != null && req.targetCharacter.characterId == data.characterId)
                            after++;
                        break;

                    case QuestRequirementType.LevelUpAccount:
                    case QuestRequirementType.LevelUpCharacter:
                        // handled elsewhere
                        break;
                }

                if (after != before)
                {
                    PlayerSave.SaveQuestProgress(quest.questId, after);
                    changed[quest.questId] = after;
                }
            }

            return changed;
        }

        /// <summary>
        /// Converts a QuestItem reward to a BattlePassItem so we can reuse the same writer (icons/frames/characters/items/currency).
        /// </summary>
        private static BattlePassItem ToBattlePassItem(QuestItem quest)
        {
            var bpi = ScriptableObject.CreateInstance<BattlePassItem>();
            bpi.passId = $"Quest_{quest.questId}";
            bpi.itemTitle = quest.title;
            bpi.itemIcon = quest.icon;
            bpi.rewardTier = BattlePassItem.RewardTier.Free;

            switch (quest.questReward)
            {
                case QuestReward.CharacterReward:
                    bpi.rewardType = BattlePassItem.RewardType.CharacterReward;
                    bpi.characterData = new[] { quest.characterData };
                    break;
                case QuestReward.IconReward:
                    bpi.rewardType = BattlePassItem.RewardType.IconReward;
                    bpi.iconReward = quest.iconItem;
                    break;
                case QuestReward.FrameReward:
                    bpi.rewardType = BattlePassItem.RewardType.FrameReward;
                    bpi.frameReward = quest.frameItem;
                    break;
                case QuestReward.ItemReward:
                    bpi.rewardType = BattlePassItem.RewardType.InventoryItemReward;
                    bpi.inventoryItems = new[] { quest.inventoryItem };
                    break;
                case QuestReward.None:
                default:                  
                    break;
            }
            return bpi;
        }
    }
}
#endif
