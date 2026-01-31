namespace BulletHellTemplate
{
    [System.Serializable]
    public class CharacterStatsRuntime
    {
        public float baseHP;
        public float baseHPRegen;
        public float baseHPLeech;
        public float baseMP;
        public float baseMPRegen;
        public float baseDamage;
        public float baseAttackSpeed;
        public float baseCooldownReduction;
        public float baseCriticalRate;
        public float baseCriticalDamageMultiplier;
        public float baseDefense;
        public float baseShield;
        public float baseMoveSpeed;
        public float baseCollectRange;
        public float baseMaxStats;
        public float baseMaxSkills;

        // Constructor to copy from baseStats
        public CharacterStatsRuntime(CharacterStats baseStats)
        {
            baseHP = baseStats.baseHP;
            baseHPRegen = baseStats.baseHPRegen;
            baseHPLeech = baseStats.baseHPLeech;
            baseMP = baseStats.baseMP;
            baseMPRegen = baseStats.baseMPRegen;
            baseDamage = baseStats.baseDamage;
            baseAttackSpeed = baseStats.baseAttackSpeed;
            baseCooldownReduction = baseStats.baseCooldownReduction;
            baseCriticalRate = baseStats.baseCriticalRate;
            baseCriticalDamageMultiplier = baseStats.baseCriticalDamageMultiplier;
            baseDefense = baseStats.baseDefense;
            baseShield = baseStats.baseShield;
            baseMoveSpeed = baseStats.baseMoveSpeed;
            baseCollectRange = baseStats.baseCollectRange;
        }

        // Method to reset characterStatsComponent back to base characterStatsComponent
        public void ResetToBaseStats(CharacterStats baseStats)
        {
            baseHP = baseStats.baseHP;
            baseHPRegen = baseStats.baseHPRegen;
            baseHPLeech = baseStats.baseHPLeech;
            baseMP = baseStats.baseMP;
            baseMPRegen = baseStats.baseMPRegen;
            baseDamage = baseStats.baseDamage;
            baseAttackSpeed = baseStats.baseAttackSpeed;
            baseCooldownReduction = baseStats.baseCooldownReduction;
            baseCriticalRate = baseStats.baseCriticalRate;
            baseCriticalDamageMultiplier = baseStats.baseCriticalDamageMultiplier;
            baseDefense = baseStats.baseDefense;
            baseShield = baseStats.baseShield;
            baseMoveSpeed = baseStats.baseMoveSpeed;
            baseCollectRange = baseStats.baseCollectRange;
        }

        public void ApplyStatUpgrade(StatType statType, int upgradeLevel, StatUpgrade statUpgrade)
        {
            if (upgradeLevel < 1 || upgradeLevel > 5)
            {
                return;
            }

            float totalUpgradeAmount = 0f;
            for (int i = 0; i < upgradeLevel; i++)
            {
                totalUpgradeAmount += statUpgrade.upgradeAmounts[i];
            }

            switch (statType)
            {
                case StatType.HP:
                    baseHP += totalUpgradeAmount;
                    break;
                case StatType.HPRegen:
                    baseHPRegen += totalUpgradeAmount;
                    break;
                case StatType.HPLeech:
                    baseHPLeech += totalUpgradeAmount;
                    break;
                case StatType.MP:
                    baseMP += totalUpgradeAmount;
                    break;
                case StatType.MPRegen:
                    baseMPRegen += totalUpgradeAmount;
                    break;
                case StatType.Damage:
                    baseDamage += totalUpgradeAmount;
                    break;
                case StatType.AttackSpeed:
                    baseAttackSpeed += totalUpgradeAmount;
                    break;
                case StatType.CooldownReduction:
                    baseCooldownReduction += totalUpgradeAmount;
                    break;
                case StatType.CriticalRate:
                    baseCriticalRate += totalUpgradeAmount;
                    break;
                case StatType.CriticalDamageMultiplier:
                    baseCriticalDamageMultiplier += totalUpgradeAmount;
                    break;
                case StatType.Defense:
                    baseDefense += totalUpgradeAmount;
                    break;
                case StatType.Shield:
                    baseShield += totalUpgradeAmount;
                    break;
                case StatType.MoveSpeed:
                    baseMoveSpeed += totalUpgradeAmount;
                    break;
                case StatType.CollectRange:
                    baseCollectRange += totalUpgradeAmount;
                    break;
                default:
                    break;
            }
        }

    }
}
