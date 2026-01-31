#if FIREBASE

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;

namespace BulletHellTemplate
{
    /// <summary>
    /// EXP / Level-up operations with one Firestore batch per call.
    /// </summary>
    public static class FirebaseExpHandler
    {
        private static string Uid => FirebaseAuthHandler.Auth?.CurrentUser?.UserId;
        private static FirebaseFirestore Db => FirebaseAuthHandler.Firestore;
        private static DocumentReference PlayerDoc => Db.Collection("Players").Document(Uid);

        /*──────── Account EXP ────────*/
        public static async UniTask<RequestResult> AddAccountExpAsync(int exp)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            if (Uid == null) return RequestResult.Fail("invalid_credentials");

            int max = GameInstance.Singleton.accountLevels.accountMaxLevel;
            int lvl = PlayerSave.GetAccountLevel();
            if (lvl >= max) return RequestResult.Fail("0");

            int pool = PlayerSave.GetAccountCurrentExp() + exp;
            int ups = 0;

            while (lvl < max)
            {
                int req = GameInstance.Singleton.GetAccountExpForLevel(lvl);
                if (pool < req) break;
                pool -= req; lvl++; ups++;
            }
            if (lvl >= max) pool = 0;

            PlayerSave.SetAccountLevel(lvl);
            PlayerSave.SetAccountCurrentExp(pool);

            var batch = Db.StartBatch();
            batch.Set(PlayerDoc, new Dictionary<string, object>
            {
                { "AccountLevel", lvl },
                { "AccountCurrentExp", pool }
            }, SetOptions.MergeAll);
            await batch.CommitAsync();

            return ups > 0 ? RequestResult.Ok($"up:{ups}") : RequestResult.Ok();
        }

        /*──────── Character EXP ─────*/
        public static async UniTask<RequestResult> AddCharacterExpAsync(int cid, int exp)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            if (Uid == null) return RequestResult.Fail("invalid_credentials");

            var cd = GameInstance.Singleton.GetCharacterDataById(cid);
            if (cd == null) return RequestResult.Fail("0");

            int lvl = PlayerSave.GetCharacterLevel(cid);
            if (lvl >= cd.maxLevel) return RequestResult.Fail("1");

            int pool = PlayerSave.GetCharacterCurrentExp(cid) + exp;
            PlayerSave.SetCharacterCurrentExp(cid, pool);

            var cDoc = PlayerDoc.Collection("Characters").Document(cid.ToString());
            var batch = Db.StartBatch();
            batch.Set(cDoc, new Dictionary<string, object> { { "CharacterCurrentExp", pool } }, SetOptions.MergeAll);
            await batch.CommitAsync();
            return RequestResult.Ok();
        }

        /*────── Character Mastery ───*/
        public static async UniTask<RequestResult> AddCharacterMasteryExpAsync(int cid, int exp)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            if (Uid == null) return RequestResult.Fail("invalid_credentials");

            int max = GameInstance.Singleton.characterMastery.maxMasteryLevel - 1;
            int lvl = PlayerSave.GetCharacterMasteryLevel(cid);
            if (lvl >= max) return RequestResult.Fail("max");

            int pool = PlayerSave.GetCharacterCurrentMasteryExp(cid) + exp;
            int ups = 0;

            while (lvl < max)
            {
                int req = GameInstance.Singleton.GetMasteryExpForLevel(lvl);
                if (pool < req) break;
                pool -= req; lvl++; ups++;
            }
            if (lvl >= max) pool = 0;

            PlayerSave.SetCharacterMasteryLevel(cid, lvl);
            PlayerSave.SetCharacterCurrentMasteryExp(cid, pool);

            var cDoc = PlayerDoc.Collection("Characters").Document(cid.ToString());
            var batch = Db.StartBatch();
            batch.Set(cDoc, new Dictionary<string, object>
            {
                { "CharacterMasteryLevel",       lvl  },
                { "CharacterCurrentMasteryExp",  pool }
            }, SetOptions.MergeAll);
            await batch.CommitAsync();

            return ups > 0 ? RequestResult.Ok($"up:{ups}") : RequestResult.Ok("ok");
        }

        /*──────── Battle-Pass XP ─────*/
        public static async UniTask<RequestResult> AddBattlePassXpAsync(int add)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            if (Uid == null) return RequestResult.Fail("invalid_credentials");

            var gi = GameInstance.Singleton;
            int maxLvl = gi.maxLevelPass;

            var (xp, lvl, prem) = PlayerSave.GetBattlePassProgress();
            xp += add;

            while (xp >= XPForLevel(lvl) && lvl < maxLvl)
            {
                xp -= XPForLevel(lvl);
                lvl++;
            }
            if (lvl >= maxLvl) xp = XPForLevel(maxLvl);

            PlayerSave.SetBattlePassProgress(xp, lvl, prem);

            var doc = PlayerDoc.Collection("Progress").Document("BattlePass");
            var batch = Db.StartBatch();
            batch.Set(doc, new Dictionary<string, object>
            {
                { "CurrentXP",    xp  },
                { "CurrentLevel", lvl }
            }, SetOptions.MergeAll);
            await batch.CommitAsync();
            return RequestResult.Ok();
        }

        public static int XPForLevel(int lvl) =>
            Mathf.FloorToInt(GameInstance.Singleton.baseExpPass *
                             Mathf.Pow(1f + GameInstance.Singleton.incPerLevelPass, lvl - 1));
    }
}

#endif