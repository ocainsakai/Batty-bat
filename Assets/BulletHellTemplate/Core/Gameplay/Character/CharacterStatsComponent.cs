using BulletHellTemplate.Core.Events;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static Unity.Collections.Unicode;


#if FUSION2
using Fusion;
#endif

namespace BulletHellTemplate
{
    [DisallowMultipleComponent]
    public partial class CharacterStatsComponent : MonoBehaviour
    {
        public float MaxHp { get; private set; }
        public float CurrentHP { get; private set; }
        public float CurrentHPRegen { get; private set; }
        public float CurrentHPLeech { get; private set; }
        public float MaxMp { get; private set; }
        public float CurrentMP { get; private set; }
        public float CurrentMPRegen { get; private set; }
        public float CurrentDamage { get; private set; }
        public float CurrentAttackSpeed { get; private set; }
        public float CurrentCooldownReduction { get; private set; }
        public float CurrentCriticalRate { get; private set; }
        public float CurrentCriticalDamageMultiplier { get; private set; }
        public float CurrentDefense { get; private set; }
        public float CurrentShield { get; private set; }
        public float CurrentMoveSpeed { get; private set; }
        public float CurrentCollectRange { get; private set; }
        public float CurrentMaxStats { get; private set; }
        public float CurrentMaxSkills { get; private set; }
        public int CurrentXP { get; private set; } = 0;
        public int CurrentLevel { get; private set; } = 1;

        public List<StatPerkData> StatsPerkData { get; private set; } = new List<StatPerkData>();
        private readonly Dictionary<MonsterSkill, CancellationTokenSource> dotSkillCts = new();
        private CharacterData characterData;
        private CharacterControllerComponent characterControllerComponent;
        private CharacterEntity characterOwner;

        private void Awake()
        {
            characterControllerComponent = GetComponent<CharacterControllerComponent>();
            characterOwner = GetComponent<CharacterEntity>();
        }

        public void Initialize(CharacterData _characterData)
        {
            if (characterControllerComponent == null) characterControllerComponent = GetComponent<CharacterControllerComponent>();
            characterData = _characterData;
            InitializeStats();
            RegenerateStatsAsync(this.GetCancellationTokenOnDestroy()).Forget();

            EventBus.Publish(new PlayerStatsChangedEvent(characterOwner, GetCurrentCharacterStats()));
            EventBus.Publish(new PlayerHealthChangedEvent(characterOwner, CurrentHP, MaxHp));
            EventBus.Publish(new PlayerEnergyChangedEvent(characterOwner, CurrentMP, MaxMp));
            EventBus.Publish(new PlayerShieldChangedEvent(characterOwner, CurrentShield, MaxHp));

#if FUSION2
            if (characterOwner != null && characterOwner.Object != null && characterOwner.Object.HasStateAuthority)
            {
                PublishAllStatEvents();
            }
#endif
        }


        public void SetUIGameplayStats() => PublishAllStatEvents();
        public void AlterAttackSpeed(float delta) { CurrentAttackSpeed = Mathf.Max(0, CurrentAttackSpeed - delta); PublishAllStatEvents(); }
        public void AlterDefense(float delta) { CurrentDefense = Mathf.Max(0, CurrentDefense + delta); PublishAllStatEvents(); }
        public void AlterDamage(float delta) { CurrentDamage = Mathf.Max(0, CurrentDamage + delta); PublishAllStatEvents(); }
        public void AlterCooldownReduction(float delta) { CurrentCooldownReduction = Mathf.Max(0, CurrentCooldownReduction + delta); PublishAllStatEvents(); }
        public void AlterCriticalRate(float delta) { CurrentCriticalRate = Mathf.Max(0f, CurrentCriticalRate + delta); PublishAllStatEvents(); }
        public void AlterCriticalDamageMultiplier(float delta) { CurrentCriticalDamageMultiplier = Mathf.Max(0f, CurrentCriticalDamageMultiplier + delta); PublishAllStatEvents(); }
        public void AlterHPRegen(float delta) { CurrentHPRegen = Mathf.Max(0f, CurrentHPRegen + delta); PublishAllStatEvents(); }
        public void AlterMPRegen(float delta) { CurrentMPRegen = Mathf.Max(0f, CurrentMPRegen + delta); PublishAllStatEvents(); }
        public void AlterHPLeech(float delta) { CurrentHPLeech = Mathf.Max(0f, CurrentHPLeech + delta); PublishAllStatEvents(); }
        public void AlterCollectRange(float delta) { CurrentCollectRange = Mathf.Max(0f, CurrentCollectRange + delta); PublishAllStatEvents(); }
        public void AlterMaxStats(float delta) { CurrentMaxStats = Mathf.Max(0f, CurrentMaxStats + delta); PublishAllStatEvents(); }
        public void AlterMaxSkills(float delta) { CurrentMaxSkills = Mathf.Max(0f, CurrentMaxSkills + delta); PublishAllStatEvents(); }
        public void AlterMoveSpeed(float deltaSpeed)
        {
            if (characterControllerComponent == null) return;
            float newSpeed = Mathf.Max(0f, CurrentMoveSpeed + deltaSpeed);
            CurrentMoveSpeed = newSpeed;
            characterControllerComponent.AlterSpeed(newSpeed);
            PublishAllStatEvents();
        }
        public void AlterMaxHp(int amount)
        {
            MaxHp = Mathf.Max(1, MaxHp + amount);
            CurrentHP = Mathf.Clamp(CurrentHP + amount, 0, MaxHp);
            EventBus.Publish(new PlayerHealthChangedEvent(characterOwner, CurrentHP, MaxHp));
            PublishAllStatEvents();
        }
        public void AlterMaxMP(float delta, bool restoreMP = true)
        {
            MaxMp = Mathf.Max(0f, MaxMp + delta);
            if (restoreMP) CurrentMP = Mathf.Clamp(CurrentMP + delta, 0, MaxMp);
            EventBus.Publish(new PlayerEnergyChangedEvent(characterOwner, CurrentMP, MaxMp));
            PublishAllStatEvents();
        }
        public bool ConsumeMP(float amount)
        {
            if (CurrentMP >= amount)
            {
                CurrentMP -= amount;
                EventBus.Publish(new PlayerEnergyChangedEvent(characterOwner, CurrentMP, MaxMp));
                PublishAllStatEvents();
                return true;
            }

            return false;
        }
        public void AddShield(int amount)
        {
            CurrentShield = Mathf.Min(CurrentShield + amount, MaxHp);
            EventBus.Publish(new PlayerShieldChangedEvent(characterOwner, CurrentShield, MaxHp));
            PublishAllStatEvents();
        }
        public void RemoveShield(int amount)
        {
            CurrentShield = Mathf.Max(CurrentShield - amount, 0);
            EventBus.Publish(new PlayerShieldChangedEvent(characterOwner, CurrentShield, MaxHp));
            PublishAllStatEvents();
        }

        public void HealHP(int amount)
        {
            CurrentHP = Mathf.Min(CurrentHP + amount, MaxHp);
            EventBus.Publish(new PlayerHealthChangedEvent(characterOwner, CurrentHP, MaxHp));
            PublishAllStatEvents();
        }

        public void HealMP(int amount)
        {
            CurrentMP = Mathf.Min(CurrentMP + amount, MaxMp);
            EventBus.Publish(new PlayerEnergyChangedEvent(characterOwner, CurrentMP, MaxMp));
            PublishAllStatEvents();
        }

        public void ApplyHpLeech(float damage)
        {
            if (damage <= 0f || CurrentHPLeech <= 0f)
                return;

            float rawLeech = damage * CurrentHPLeech;
            int leechAmount = Mathf.Max(1, Mathf.FloorToInt(rawLeech));

            CurrentHP = Mathf.Min(CurrentHP + leechAmount, MaxHp);
            EventBus.Publish(new PlayerHealthChangedEvent(characterOwner, CurrentHP, MaxHp));
            PublishAllStatEvents();
        }

        /// <summary>
        /// Applies incoming damage to the character, considering defense, shield, and invincibility.
        /// </summary>
        /// <param name="damage">Raw incoming damage value.</param>
        public void ReceiveDamage(float damage)
        {
            damage = Mathf.Max(damage - CurrentDefense, GameplayManager.Singleton.minDamage);
            if (damage <= 0f) return;

            if (CurrentShield > 0f)
            {
                float shieldAbsorbed = Mathf.Min(CurrentShield, damage);
                CurrentShield -= shieldAbsorbed;
                damage -= shieldAbsorbed;
                EventBus.Publish(new PlayerShieldChangedEvent(characterOwner, CurrentShield, MaxHp));
            }

            if (damage > 0f)
            {
                CurrentHP -= damage;
                EventBus.Publish(new PlayerHealthChangedEvent(characterOwner, CurrentHP, MaxHp));

                if (CurrentHP <= 0f)
                {
                    CurrentHP = 0f;
                    EventBus.Publish(new PlayerDiedEvent(characterOwner));
                    characterOwner?.OnDeath();
                }
            }

            PublishAllStatEvents();
        }

        /// <summary>
        /// Applies a damage-over-time effect coming from a <see cref="SkillData"/>.
        /// Identical sources restart the DoT instead of stacking.
        /// </summary>
        public void ApplyDot(MonsterSkill source, int totalDamage, float duration)
        {
            RestartDot(dotSkillCts, source, totalDamage, duration);
        }

        public void AddXP(int xpAmount)
        {
            if (xpAmount <= 0)
                return;

            CurrentXP += xpAmount;

            var maxLevel = GameplayManager.Singleton.maxLevel;
            var xpTable = GameplayManager.Singleton.xpToNextLevel;
            EventBus.Publish(new PlayerEXPChangeEvent(characterOwner, CurrentLevel, CurrentXP, xpTable[CurrentLevel]));
            while (CurrentLevel < maxLevel && CurrentXP >= xpTable[CurrentLevel])
            {
                LevelUp();
            }
        }
        private void LevelUp()
        {
            int previousLevel = CurrentLevel;
            var xpTable = GameplayManager.Singleton.xpToNextLevel;
            CurrentLevel++;
            CurrentXP -= xpTable[previousLevel];
            EventBus.Publish(new PlayerEXPChangeEvent(characterOwner, CurrentLevel, CurrentXP, xpTable[CurrentLevel]));
        }

        /// <summary>
        /// Revives the character to full HP and MP and grants temporary invincibility.
        /// </summary>
        public void Revive()
        {
            CurrentHP = MaxHp;
            CurrentMP = MaxMp;
            EventBus.Publish(new PlayerRevivedEvent(characterOwner));
            EventBus.Publish(new PlayerHealthChangedEvent(characterOwner, CurrentHP, MaxHp));
            EventBus.Publish(new PlayerEnergyChangedEvent(characterOwner, CurrentMP, MaxMp));
            EventBus.Publish(new PlayerStatsChangedEvent(characterOwner, GetCurrentCharacterStats()));

            PublishAllStatEvents();
        }

        public int GetXPToNextLevel()
        {
            if (CurrentLevel < GameplayManager.Singleton.maxLevel)
            {
                return GameplayManager.Singleton.xpToNextLevel[CurrentLevel];
            }
            return -1;
        }

        /// <summary>
        /// Builds and returns a new <see cref="CharacterStats"/> instance that mirrors
        /// the character's current run-time values (HP, MP, Damage, etc.).
        /// Use this snapshot when broadcasting <see cref="PlayerStatsChangedEvent"/>
        /// so listeners (e.g. UI) get an immutable copy of the latest stats.
        /// </summary>
        public CharacterStats GetCurrentCharacterStats()
        {
            return new CharacterStats
            {
                baseHP = MaxHp,
                baseHPRegen = CurrentHPRegen,
                baseHPLeech = CurrentHPLeech,
                baseMP = MaxMp,
                baseMPRegen = CurrentMPRegen,
                baseDamage = CurrentDamage,
                baseAttackSpeed = CurrentAttackSpeed,
                baseCooldownReduction = CurrentCooldownReduction,
                baseCriticalRate = CurrentCriticalRate,
                baseCriticalDamageMultiplier = CurrentCriticalDamageMultiplier,
                baseDefense = CurrentDefense,
                baseShield = CurrentShield,
                baseMoveSpeed = CurrentMoveSpeed,
                baseCollectRange = CurrentCollectRange,
                baseMaxStats = CurrentMaxStats,
                baseMaxSkills = CurrentMaxSkills
            };
        }

        /// <summary>
        /// Regenerates HP and MP over time, while respecting pause state.
        /// </summary>
        private async UniTaskVoid RegenerateStatsAsync(CancellationToken token)
        {
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

                if (GameplayManager.Singleton.IsPaused())
                    await UniTask.WaitWhile(() => GameplayManager.Singleton.IsPaused(), cancellationToken: token);

                if (characterOwner != null && characterOwner.IsDead)
                {
                    await UniTask.WaitUntil(() => characterOwner != null && !characterOwner.IsDead, cancellationToken: token);
                    continue;
                }

                if (CurrentHP < MaxHp && CurrentHPRegen > 0f)
                {
                    CurrentHP = Mathf.Min(CurrentHP + CurrentHPRegen, MaxHp);
                    EventBus.Publish(new PlayerHealthChangedEvent(characterOwner, CurrentHP, MaxHp));
                }

                if (CurrentMP < MaxMp && CurrentMPRegen > 0f)
                {
                    CurrentMP = Mathf.Min(CurrentMP + CurrentMPRegen, MaxMp);
                    EventBus.Publish(new PlayerEnergyChangedEvent(characterOwner, CurrentMP, MaxMp));
                }

                PublishAllStatEvents();
                token.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Initializes character characterStatsComponent based on the data provided in CharacterData.
        /// </summary>
        private void InitializeStats()
        {
           
            int _characterLevel = PlayerSave.GetCharacterLevel(characterData.characterId);
            float levelMultiplier = 1f + (characterData.statsPercentageIncreaseByLevel * (_characterLevel - 1));

            if (characterData != null && characterData.baseStats != null)
            {
                MaxMp = characterData.baseStats.baseMP * levelMultiplier;
                CurrentMP = MaxMp;

                MaxHp = characterData.baseStats.baseHP * levelMultiplier;
                CurrentHP = MaxHp;

                CurrentHPRegen = characterData.baseStats.baseHPRegen * levelMultiplier;
                CurrentHPLeech = characterData.baseStats.baseHPLeech * levelMultiplier;
                CurrentMPRegen = characterData.baseStats.baseMPRegen * levelMultiplier;
                CurrentDamage = characterData.baseStats.baseDamage * levelMultiplier;
                CurrentDefense = characterData.baseStats.baseDefense * levelMultiplier;
                CurrentShield = characterData.baseStats.baseShield * levelMultiplier;
                CurrentAttackSpeed = characterData.baseStats.baseAttackSpeed * levelMultiplier;
                CurrentMoveSpeed = characterData.baseStats.baseMoveSpeed * levelMultiplier;
                CurrentCollectRange = characterData.baseStats.baseCollectRange * levelMultiplier;
                CurrentMaxStats = characterData.baseStats.baseMaxStats;
                CurrentMaxSkills = characterData.baseStats.baseMaxSkills;

                if (characterControllerComponent != null)
                {
                    characterControllerComponent.AlterSpeed(CurrentMoveSpeed);
                }
            }
            else
            {
                Debug.LogError("CharacterData or CharacterStats is null.");
            }

            EventBus.Publish(new PlayerStatsChangedEvent(characterOwner, GetCurrentCharacterStats()));
            EventBus.Publish(new PlayerHealthChangedEvent(characterOwner, CurrentHP, MaxHp));
            EventBus.Publish(new PlayerEnergyChangedEvent(characterOwner, CurrentMP, MaxMp));
            EventBus.Publish(new PlayerShieldChangedEvent(characterOwner, CurrentShield, MaxHp));

            ApplyStatUpgrades();
            ApplyEquippedItemsStats();
        }

        /// <summary>
        /// Applies all the stat upgrades that have been accumulated for this character.
        /// </summary>
        private void ApplyStatUpgrades()
        {
            int characterId = characterData.characterId;
            Dictionary<StatType, int> upgradeLevels = PlayerSave.LoadAllCharacterUpgradeLevels(characterId);

            foreach (var upgrade in upgradeLevels)
            {
                if (upgrade.Value > 0)
                {
                    float totalUpgradeAmount = 0f;
                    for (int i = 1; i <= upgrade.Value; i++)
                    {
                        StatUpgrade statUpgrade = GetStatUpgrade(upgrade.Key);
                        if (statUpgrade != null)
                        {
                            totalUpgradeAmount += statUpgrade.upgradeAmounts[i - 1];
                        }
                        else
                        {
                            Debug.LogError($"StatUpgrade not found for {upgrade.Key}");
                        }
                    }
                    UpdateStatFromUpgrade(upgrade.Key, totalUpgradeAmount);
                }
            }
            EventBus.Publish(new PlayerStatsChangedEvent(characterOwner, GetCurrentCharacterStats()));
            EventBus.Publish(new PlayerHealthChangedEvent(characterOwner, CurrentHP, MaxHp));
            EventBus.Publish(new PlayerEnergyChangedEvent(characterOwner, CurrentMP, MaxMp));
            EventBus.Publish(new PlayerShieldChangedEvent(characterOwner, CurrentShield, MaxHp));
        }

        /// <summary>
        /// Applies characterStatsComponent from all equipped items (base item characterStatsComponent and upgrade bonuses) directly to the current characterStatsComponent.
        /// This method updates currentHP, currentMP, currentDamage, etc., based on the equipped items.
        /// </summary>
        private void ApplyEquippedItemsStats()
        {
            // For each slot in the character's item slots, find the equipped item GUID.
            foreach (string slotName in characterData.itemSlots)
            {
                string uniqueItemGuid = InventorySave.GetEquippedItemForSlot(characterData.characterId, slotName);
                if (string.IsNullOrEmpty(uniqueItemGuid))
                    continue;

                // Retrieve the purchased item data based on the unique GUID.
                var purchasedItem = PlayerSave.GetInventoryItems()
                    .Find(pi => pi.uniqueItemGuid == uniqueItemGuid);
                if (purchasedItem == null)
                    continue;

                // Find the corresponding scriptable inventory item.
                var soItem = FindItemById(purchasedItem.itemId);
                if (soItem == null)
                    continue;

                // Apply base item characterStatsComponent to the current characterStatsComponent.
                if (soItem.itemStats != null)
                {
                    MaxHp += soItem.itemStats.baseHP;
                    CurrentHP = MaxHp;
                    CurrentHPRegen += soItem.itemStats.baseHPRegen;
                    CurrentHPLeech += soItem.itemStats.baseHPLeech;
                    CurrentMP += soItem.itemStats.baseMP;
                    CurrentMPRegen += soItem.itemStats.baseMPRegen;
                    CurrentDamage += soItem.itemStats.baseDamage;
                    CurrentAttackSpeed += soItem.itemStats.baseAttackSpeed;
                    CurrentCooldownReduction += soItem.itemStats.baseCooldownReduction;
                    CurrentCriticalRate += soItem.itemStats.baseCriticalRate;
                    CurrentCriticalDamageMultiplier += soItem.itemStats.baseCriticalDamageMultiplier;
                    CurrentDefense += soItem.itemStats.baseDefense;
                    CurrentShield += soItem.itemStats.baseShield;
                    CurrentMoveSpeed += soItem.itemStats.baseMoveSpeed;
                    CurrentCollectRange += soItem.itemStats.baseCollectRange;
                }
                // Apply upgrade bonuses from the item.
                int itemLevel = InventorySave.GetItemUpgradeLevel(uniqueItemGuid);
                float totalUpgrade = 0f;
                for (int i = 0; i < itemLevel && i < soItem.itemUpgrades.Count; i++)
                {
                    totalUpgrade += soItem.itemUpgrades[i].statIncreasePercentagePerLevel;
                }
                if (totalUpgrade > 0f)
                {
                    float factor = totalUpgrade; // For example, 0.2 means +20%.
                    CurrentHP += soItem.itemStats.baseHP * factor;
                    CurrentHPRegen += soItem.itemStats.baseHPRegen * factor;
                    CurrentHPLeech += soItem.itemStats.baseHPLeech * factor;
                    CurrentMP += soItem.itemStats.baseMP * factor;
                    CurrentMPRegen += soItem.itemStats.baseMPRegen * factor;
                    CurrentDamage += soItem.itemStats.baseDamage * factor;
                    CurrentAttackSpeed += soItem.itemStats.baseAttackSpeed * factor;
                    CurrentCooldownReduction += soItem.itemStats.baseCooldownReduction * factor;
                    CurrentCriticalRate += soItem.itemStats.baseCriticalRate * factor;
                    CurrentCriticalDamageMultiplier += soItem.itemStats.baseCriticalDamageMultiplier * factor;
                    CurrentDefense += soItem.itemStats.baseDefense * factor;
                    CurrentShield += soItem.itemStats.baseShield * factor;
                    CurrentMoveSpeed += soItem.itemStats.baseMoveSpeed * factor;
                    CurrentCollectRange += soItem.itemStats.baseCollectRange * factor;
                }
            }
            EventBus.Publish(new PlayerStatsChangedEvent(characterOwner, GetCurrentCharacterStats()));
            EventBus.Publish(new PlayerHealthChangedEvent(characterOwner, CurrentHP, MaxHp));
            EventBus.Publish(new PlayerEnergyChangedEvent(characterOwner, CurrentMP, MaxMp));
            EventBus.Publish(new PlayerShieldChangedEvent(characterOwner, CurrentShield, MaxHp));
        }
        private InventoryItem FindItemById(string itemId)
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.inventoryItems == null)
                return null;
            foreach (InventoryItem item in GameInstance.Singleton.inventoryItems)
            {
                if (item.itemId == itemId)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// Applies a stat perk to the character, adjusting the relevant stat.
        /// </summary>
        /// <param name="statPerk">The stat perk to apply.</param>
        /// <param name="level">The level of the perk being applied.</param>
        public void ApplyStatPerk(StatPerkData statPerk, int level)
        {
            if (statPerk == null)
            {
                Debug.LogWarning("StatPerkData is null.");
                return;
            }

            if (StatsPerkData.Contains(statPerk))
            {
                ApplyPerkStats(statPerk, level);
            }
            else
            {
                StatsPerkData.Add(statPerk);
                ApplyPerkStats(statPerk, level);
            }
            EventBus.Publish(new PlayerStatsChangedEvent(characterOwner, GetCurrentCharacterStats()));
            EventBus.Publish(new PlayerHealthChangedEvent(characterOwner, CurrentHP, MaxHp));
            EventBus.Publish(new PlayerEnergyChangedEvent(characterOwner, CurrentMP, MaxMp));
            EventBus.Publish(new PlayerShieldChangedEvent(characterOwner, CurrentShield, MaxHp));
        }

        /// <summary>
        /// Applies the characterStatsComponent of a specific perk based on its level.
        /// </summary>
        /// <param name="statPerk">The stat perk to apply.</param>
        /// <param name="level">The level of the perk.</param>
        private void ApplyPerkStats(StatPerkData statPerk, int level)
        {
            float valueToAdd = 0f;
            StatType perkStatType = statPerk.statType;

            if (statPerk.fixedStat != null && level < statPerk.fixedStat.values.Count)
            {
                valueToAdd = statPerk.fixedStat.values[level];
                UpdateStat(statPerk.statType, valueToAdd);
                EventBus.Publish(new StatPerkUpdatedEvent(characterOwner, perkStatType, valueToAdd));
            }

            if (statPerk.rateStat != null && level < statPerk.rateStat.rates.Count)
            {
                valueToAdd = (statPerk.rateStat.rates[level] * GetStatBaseValue(statPerk.statType));
                UpdateStat(statPerk.statType, valueToAdd);
                EventBus.Publish(new StatPerkUpdatedEvent(characterOwner, perkStatType, valueToAdd));
            }
        }

        /// <summary>
        /// Updates a specific stat by adding a given value.
        /// </summary>
        private void UpdateStat(StatType statType, float valueToAdd)
        {
            ApplyStatChange(statType, valueToAdd);

            if (statType == StatType.MoveSpeed && characterControllerComponent != null)
            {
                characterControllerComponent.AlterSpeed(CurrentMoveSpeed);
            }
        }

        /// <summary>
        /// Gets the current value of a stat.
        /// </summary>
        private float GetStatBaseValue(StatType statType) => GetStatValue(statType);

        /// <summary>
        /// Handles stat value addition internally.
        /// </summary>
        private void ApplyStatChange(StatType statType, float value)
        {
            switch (statType)
            {
                case StatType.HP:
                    MaxHp += value;
                    CurrentHP += value;
                    break;
                case StatType.HPRegen: CurrentHPRegen += value; break;
                case StatType.HPLeech: CurrentHPLeech += value; break;
                case StatType.MP:
                    MaxMp += value;
                    CurrentMP += value;
                    break;
                case StatType.MPRegen: CurrentMPRegen += value; break;
                case StatType.Damage: CurrentDamage += value; break;
                case StatType.AttackSpeed: CurrentAttackSpeed -= value; break;
                case StatType.CooldownReduction: CurrentCooldownReduction += value; break;
                case StatType.CriticalRate: CurrentCriticalRate += value; break;
                case StatType.Defense: CurrentDefense += value; break;
                case StatType.MoveSpeed: CurrentMoveSpeed += value; break;
                case StatType.CollectRange: CurrentCollectRange += value; break;
            }
        }

        /// <summary>
        /// Returns the current value of the specified stat.
        /// </summary>
        private float GetStatValue(StatType statType)
        {
            return statType switch
            {
                StatType.HP => MaxHp,
                StatType.HPRegen => CurrentHPRegen,
                StatType.HPLeech => CurrentHPLeech,
                StatType.MP => MaxMp,
                StatType.MPRegen => CurrentMPRegen,
                StatType.Damage => CurrentDamage,
                StatType.AttackSpeed => CurrentAttackSpeed,
                StatType.CooldownReduction => CurrentCooldownReduction,
                StatType.CriticalRate => CurrentCriticalRate,
                StatType.Defense => CurrentDefense,
                StatType.MoveSpeed => CurrentMoveSpeed,
                StatType.CollectRange => CurrentCollectRange,
                _ => 0f
            };
        }

        /// <summary>
        /// Retrieves the corresponding StatUpgrade for the given StatType.
        /// </summary>
        private StatUpgrade GetStatUpgrade(StatType statType) => characterData.statUpgrades.Find(upgrade => upgrade.statType == statType);

        /// <summary>
        /// Updates a specific stat by a given value, based on the provided StatType.
        /// </summary>
        /// <param name="statType">The type of stat to update.</param>
        /// <param name="valueToAdd">The value to add to the stat.</param>
        private void UpdateStatFromUpgrade(StatType statType, float valueToAdd)
        {
            AddToStat(statType, valueToAdd);

            if (statType == StatType.MoveSpeed && characterControllerComponent != null)
            {
                characterControllerComponent.AlterSpeed(CurrentMoveSpeed);
            }
        }
        private void AddToStat(StatType statType, float value)
        {
            switch (statType)
            {
                case StatType.HP:
                    MaxHp += value;
                    CurrentHP += value;
                    break;
                case StatType.MP:
                    MaxMp += value;
                    CurrentMP += value;
                    break;
                case StatType.HPRegen: CurrentHPRegen += value; break;
                case StatType.HPLeech: CurrentHPLeech += value; break;
                case StatType.MPRegen: CurrentMPRegen += value; break;
                case StatType.Damage: CurrentDamage += value; break;
                case StatType.AttackSpeed: CurrentAttackSpeed -= value; break;
                case StatType.CooldownReduction: CurrentCooldownReduction += value; break;
                case StatType.CriticalRate: CurrentCriticalRate += value; break;
                case StatType.Defense: CurrentDefense += value; break;
                case StatType.MoveSpeed: CurrentMoveSpeed += value; break;
                case StatType.CollectRange: CurrentCollectRange += value; break;
                case StatType.MaxStats: CurrentMaxStats += value; break;
                case StatType.MaxSkills: CurrentMaxSkills += value; break;
            }
        }

        /// <summary>
        /// Retrieves the list of stat perks associated with the character.
        /// </summary>
        public List<StatPerkData> GetStatsPerkData() => StatsPerkData;

        public bool HasStatPerk(StatPerkData perk) => GetStatsPerkData().Contains(perk);

        /// <summary>
        /// Publishes every stat-related event so UI stays up-to-date.
        /// Call this once after any modification.
        /// </summary>
        private void PublishAllStatEvents()
        {
            var snapshot = GetCurrentCharacterStats();
            var xpTable = GameplayManager.Singleton.xpToNextLevel;

            bool canPublishLocal = true;

#if FUSION2
            if (characterOwner && characterOwner.Object && characterOwner.Runner && characterOwner.Runner.IsRunning)
                canPublishLocal = characterOwner.Object.HasStateAuthority || characterOwner.Object.HasInputAuthority;
#endif

            if (canPublishLocal)
            {
                EventBus.Publish(new PlayerStatsChangedEvent(characterOwner, snapshot));
                EventBus.Publish(new PlayerHealthChangedEvent(characterOwner, CurrentHP, MaxHp));
                EventBus.Publish(new PlayerEnergyChangedEvent(characterOwner, CurrentMP, MaxMp));
                EventBus.Publish(new PlayerShieldChangedEvent(characterOwner, CurrentShield, MaxHp));
                EventBus.Publish(new PlayerEXPChangeEvent(characterOwner, CurrentLevel, CurrentXP, xpTable[CurrentLevel]));
            }

#if FUSION2
            if (characterOwner && characterOwner.Object != null && characterOwner.Object.HasStateAuthority)
            {
                characterOwner.PushMiniHud(
                    (ushort)Mathf.RoundToInt(CurrentHP),
                    (ushort)Mathf.RoundToInt(MaxHp),
                    (ushort)Mathf.RoundToInt(CurrentMP),
                    (ushort)Mathf.RoundToInt(MaxMp),
                    (ushort)Mathf.RoundToInt(CurrentShield),
                    characterOwner.invincibleIcon.gameObject.activeSelf,
                    characterOwner.buffIcon.gameObject.activeSelf,
                    characterOwner.debuffIcon.gameObject.activeSelf
                );
            }
#endif
        }


        /// <summary>
        /// Stops a running DoT from the same source (if any) and starts a new async routine.
        /// </summary>
        private void RestartDot<TKey>(Dictionary<TKey, CancellationTokenSource> dict,
                                      TKey key,
                                      int totalDamage,
                                      float duration)
        {
            if (dict.TryGetValue(key, out var oldCts))
                oldCts.Cancel();                           

            var cts = new CancellationTokenSource();
            dict[key] = cts;

            DotRoutine(totalDamage, duration, cts.Token,
                       () => dict.Remove(key)).Forget();
        }

        /// <summary>
        /// Async routine that spreads <paramref name="total"/> damage over <paramref name="duration"/>
        /// in 0.2 s ticks. Never kills the player (leaves with 1 HP).
        /// </summary>
        private async UniTaskVoid DotRoutine(int total,
                                             float duration,
                                             CancellationToken token,
                                             Action onFinish)
        {
            const float tick = 0.2f;
            int ticks = Mathf.CeilToInt(duration / tick);
            int perTick = total / ticks;
            int remainder = total - perTick * (ticks - 1);

            try
            {
                for (int i = 0; i < ticks; i++)
                {
                    token.ThrowIfCancellationRequested();
                    if (CurrentHP <= 1f) break;

                    int dmg = i == ticks - 1 ? perTick + remainder : perTick;
                    dmg = Mathf.Min(dmg, Mathf.RoundToInt(CurrentHP) - 1);

                    if (dmg > 0) ReceiveDamage(dmg);

                    await UniTask.Delay(TimeSpan.FromSeconds(tick), cancellationToken: token);
                }
            }
            catch (OperationCanceledException) {  }
            finally
            {
                onFinish?.Invoke();
            }
        }
    }
}
