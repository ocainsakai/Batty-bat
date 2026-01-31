using UnityEngine;

namespace BulletHellTemplate
{
    public static class OfflineExpHandler
    {
        /// <summary>
        /// Adds EXP to the account, performs all level-ups needed,
        /// stops at max level and returns a RequestResult.
        /// </summary>
        public static RequestResult AddAccountExp(int exp)
        {
            int maxLevel = GameInstance.Singleton.accountLevels.accountMaxLevel;
            int level = PlayerSave.GetAccountLevel();

            if (level >= maxLevel)
                return RequestResult.Fail("0");

            int expPool = PlayerSave.GetAccountCurrentExp() + exp;
            int levelUps = 0;

            while (true)
            {
                int req = GameInstance.Singleton.GetAccountExpForLevel(level);
                if (expPool < req || level >= maxLevel) break;

                expPool -= req;
                level += 1;
                levelUps += 1;
            }

            if (level >= maxLevel)
                expPool = 0;

            // Persist
            PlayerSave.SetAccountLevel(level);
            PlayerSave.SetAccountCurrentExp(expPool);

            if (levelUps > 0) return RequestResult.Ok($"up:{levelUps}");
            return RequestResult.Ok();
        }

        public static RequestResult AddCharacterExp(int cid, int exp)
        {
            CharacterData cd = GameInstance.Singleton.GetCharacterDataById(cid);
            if (cd == null) return RequestResult.Fail("0");

            int level = PlayerSave.GetCharacterLevel(cid);
            if (level >= cd.maxLevel)
                return RequestResult.Fail("1");

            int pool = PlayerSave.GetCharacterCurrentExp(cid) + exp;
            PlayerSave.SetCharacterCurrentExp(cid, pool);
            return RequestResult.Ok();
        }

        public static RequestResult AddCharacterMasteryExp(int cid, int exp)
        {
            int maxMastery = GameInstance.Singleton.characterMastery.maxMasteryLevel - 1;
            int level = PlayerSave.GetCharacterMasteryLevel(cid);

            if (level >= maxMastery) return RequestResult.Fail("max");

            int pool = PlayerSave.GetCharacterCurrentMasteryExp(cid) + exp;
            int ups = 0;

            while (level < maxMastery)
            {
                int req = GameInstance.Singleton.GetMasteryExpForLevel(level);
                if (pool < req) break;

                pool -= req;
                level += 1;
                ups += 1;
            }

            if (level >= maxMastery) pool = 0;

            PlayerSave.SetCharacterMasteryLevel(cid, level);
            PlayerSave.SetCharacterCurrentMasteryExp(cid, pool);

            return ups > 0 ? RequestResult.Ok($"up:{ups}") : RequestResult.Ok("ok");
        }

        public static RequestResult AddBattlePassXp(int add)
        {
            (int xp, int lvl, bool _) = PlayerSave.GetBattlePassProgress();
            int maxLvl = GameInstance.Singleton.maxLevelPass;

            xp += add;
            while (xp >= XPForLevel(lvl) && lvl < maxLvl)
            {
                xp -= XPForLevel(lvl);
                lvl++;
            }
            if (lvl >= maxLvl) xp = XPForLevel(maxLvl);

            PlayerSave.SetBattlePassProgress(xp, lvl, PlayerSave.CheckBattlePassPremiumUnlocked());
            return RequestResult.Ok();
        }

        public static int XPForLevel(int lvl) =>
            Mathf.FloorToInt(GameInstance.Singleton.baseExpPass *
                             Mathf.Pow(1f + GameInstance.Singleton.incPerLevelPass, lvl - 1));

    }
}
