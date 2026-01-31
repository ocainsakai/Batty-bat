using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles season timeline (start, end, remaining time) when
    /// the active backend is <b>Offline</b>.
    /// </summary>
    public static class OfflineProgressHandler
    {
        /*──────────── PUBLIC API ────────────*/

        /// <summary>Returns the current season (1-based).</summary>
        public static int GetCurrentSeason() =>
            PlayerSave.GetBattlePassCurrentSeason();

        /// <summary>Returns UTC end-time of the running season.</summary>
        public static DateTime GetSeasonEndUtc()
        {
            string iso = PlayerSave.GetBattlePassSeasonEndTime();
            return string.IsNullOrEmpty(iso)
                 ? DateTime.UtcNow
                 : DateTime.Parse(iso, null, System.Globalization.DateTimeStyles.RoundtripKind);
        }

        /// <summary>Flags current season as finished.</summary>
        public static void MarkSeasonEnded() => PlayerSave.SetSeasonEnded();

        public static RequestResult CompleteGameSession(EndGameSessionData data)
        {
            if (GameInstance.Singleton == null)
            {
                Debug.LogError("[CompleteGameSession] GameInstance missing.");
                return RequestResult.Fail("no_game");
            }

            // --- Score feedback (local) ---
            int newScore = PlayerSave.GetScore() + data.monstersKilled;
            PlayerSave.SetScore(newScore);

            // --- Gold feedback (local run loot) ---
            string goldCurrency = GameInstance.Singleton.goldCurrency;
            int currentGold = MonetizationManager.GetCurrency(goldCurrency);
            MonetizationManager.SetCurrency(goldCurrency, currentGold + data.gainedGold);

            // --- Win path: mark map completed (pending reward) + unlock next map ---
            if (data.won)
            {
                var mapData = GameInstance.Singleton.GetMapInfoDataById(data.mapId);

                if (mapData != null
                    && mapData.isRewardOnCompleteFirstTime
                    && (mapData.WinMapRewards.Count > 0 || mapData.rewardType != MapRewardType.None)
                    && !(RewardManagerPopup.Singleton?.HasLocalMapClaimed(data.mapId) ?? false))
                {
                    RewardManagerPopup.Singleton?.RegisterMapCompleted(data.mapId);
                }

                if (PlayerSave.IsLatestUnlockedMap(data.mapId))
                {
                    UnlockNextMap();
                }
            }

            // --- Quest progress (retain existing local call; migrate later if needed) ---
            ApplyEndGameProgress(new EndGameQuestProgressData
            {
                won = data.won,
                mapId = data.mapId,
                characterId = data.characterId,
                monstersKilled = data.monstersKilled
            });

            return RequestResult.Ok();
        }

        public static void ApplyEndGameProgress(EndGameQuestProgressData data)
        {
            QuestItem[] quests = GameInstance.Singleton.questData;
            foreach (QuestItem quest in quests)
            {
                if (PlayerSave.IsQuestCompleted(quest.questId))
                    continue;

                QuestRequirement requirement = quest.requirement;
                int currentProgress = PlayerSave.LoadQuestProgress(quest.questId);

                switch (requirement.requirementType)
                {
                    case QuestRequirementType.KillMonster:
                        currentProgress += data.monstersKilled;
                        PlayerSave.SaveQuestProgress(quest.questId, currentProgress);
                        break;

                    case QuestRequirementType.KillMonsterWithSpecificCharacter:
                        if (requirement.targetCharacter != null &&
                            requirement.targetCharacter.characterId == data.characterId)
                        {
                            currentProgress += data.monstersKilled;
                            PlayerSave.SaveQuestProgress(quest.questId, currentProgress);
                        }
                        break;

                    case QuestRequirementType.CompleteMap:
                        if (data.won && requirement.targetMap != null && requirement.targetMap.mapId == data.mapId)
                        {
                            currentProgress++;
                            PlayerSave.SaveQuestProgress(quest.questId, currentProgress);
                        }
                        break;

                    case QuestRequirementType.CompleteMapWithSpecificCharacter:
                        if (data.won
                            && requirement.targetMap != null && requirement.targetMap.mapId == data.mapId
                            && requirement.targetCharacter != null && requirement.targetCharacter.characterId == data.characterId)
                        {
                            currentProgress++;
                            PlayerSave.SaveQuestProgress(quest.questId, currentProgress);
                        }
                        break;

                    case QuestRequirementType.LevelUpAccount:
                    case QuestRequirementType.LevelUpCharacter:
                        // Ignored here – handled elsewhere
                        break;
                }
            }
        }

        public static RequestResult TryCompleteQuest(int questId)
        {
            QuestItem quest = GameInstance.Singleton.GetQuestItemById(questId);

            if (quest == null) return RequestResult.Fail("2");

            bool isCompleted = PlayerSave.IsQuestCompleted(quest.questId);
            if (isCompleted && quest.questType != QuestType.Repeat)
                return RequestResult.Fail("0");

            int progress = PlayerSave.LoadQuestProgress(quest.questId);
            int target = quest.requirement.targetAmount;
            if (progress < target)
                return RequestResult.Fail("1");

            ApplyQuestRewardInternal(quest);

            if (quest.questType == QuestType.Repeat)
                PlayerSave.SaveQuestProgress(quest.questId, 0);
            else
                PlayerSave.SaveQuestCompletion(quest.questId);

            return RequestResult.Ok();
        }

        public static void RefreshLevelBasedProgress()
        {
            int accountLevel = PlayerSave.GetAccountLevel();

            Dictionary<int, int> charLevels = new();
            foreach (var cd in GameInstance.Singleton.characterData)
                charLevels[cd.characterId] = PlayerSave.GetCharacterLevel(cd.characterId);

            foreach (var quest in GameInstance.Singleton.questData)
            {
                if (quest == null) continue;

                if (PlayerSave.IsQuestCompleted(quest.questId) &&
                    quest.questType != QuestType.Repeat)
                    continue;

                var req = quest.requirement;
                if (req.requirementType == QuestRequirementType.LevelUpAccount)
                {
                    if (accountLevel >= req.targetAmount)
                        PlayerSave.SaveQuestProgress(quest.questId, req.targetAmount);
                }
                else if (req.requirementType == QuestRequirementType.LevelUpCharacter &&
                         req.targetCharacter != null &&
                         charLevels.TryGetValue(req.targetCharacter.characterId,
                                                out int curLvl) &&
                         curLvl >= req.targetAmount)
                {
                    PlayerSave.SaveQuestProgress(quest.questId, req.targetAmount);
                }
            }
        }

        private static void ApplyQuestRewardInternal(QuestItem quest)
        {
            if (quest.accountExp > 0)
            {
                OfflineExpHandler.AddAccountExp(quest.accountExp);
            }

            if (quest.battlePassExp > 0)
                OfflineExpHandler.AddBattlePassXp(quest.battlePassExp);

            if (quest.selectedCharacterExp > 0)
            {
                int sel = PlayerSave.GetSelectedCharacter();
                UniTask.FromResult(OfflineExpHandler.AddCharacterExp(sel, quest.selectedCharacterExp));
            }

            if (quest.currencyAmount > 0 && !string.IsNullOrEmpty(quest.currencyReward))
            {
                int cur = MonetizationManager.GetCurrency(quest.currencyReward);
                MonetizationManager.SetCurrency(quest.currencyReward, cur + quest.currencyAmount);
            }

            if (quest.questReward != QuestReward.None)
            {
                var bpItem = ToBattlePassItem(quest);
                OfflinePurchasesHandler.ClaimBattlePassReward(bpItem);
            }
        }

        /// <summary>
        /// Unlocks the next map in the list of map data if the player has completed the latest unlocked map.
        /// The next map is unlocked and saved to the player's unlocked maps list.
        /// </summary>
        private static void UnlockNextMap()
        {
            List<int> unlockedMaps = PlayerSave.GetUnlockedMaps();
            MapInfoData[] allMaps = GameInstance.Singleton.mapInfoData;

            int lastUnlockedMapId = 0;

            if (unlockedMaps.Count > 0)
            {
                lastUnlockedMapId = unlockedMaps[unlockedMaps.Count - 1];
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
                    unlockedMaps.Add(nextMapId);
                    PlayerSave.SetUnlockedMaps(unlockedMaps);
                    break;
                }
            }
        }
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
            }
            return bpi;
        }
    }
}
