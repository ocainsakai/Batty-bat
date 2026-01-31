#if FIREBASE
using BulletHellTemplate;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace BulletHellTemplate
{

    public static class FirebaseRankingHandler
    {
        private static FirebaseFirestore DB => FirebaseAuthHandler.Firestore;
        private static FirebaseAuth Auth => FirebaseAuthHandler.Auth;

        /* ------------------------------------------------------------------
         *  Top-N players (score DESC)
         * ----------------------------------------------------------------*/
        public static async UniTask<List<Dictionary<string, object>>> GetTopPlayersAsync(int limit = 20)
        {
            var result = new List<Dictionary<string, object>>();

            try
            {
                await FirebaseAuthHandler.EnsureInitializedAsync();

                QuerySnapshot snap = await DB.Collection("Players")
                                             .OrderByDescending("score")
                                             .Limit(limit)
                                             .GetSnapshotAsync();

                int rank = 1;
                foreach (DocumentSnapshot doc in snap.Documents)
                {
                    var d = doc.ToDictionary();
                    result.Add(new Dictionary<string, object>
                    {
                        ["rank"] = rank++,
                        ["score"] = d.TryGetValue("score", out var s) ? s : 0,
                        ["PlayerName"] = d.TryGetValue("PlayerName", out var n) ? n : "Guest",
                        ["PlayerIcon"] = d.TryGetValue("PlayerIcon", out var ic) ? ic : "",
                        ["PlayerFrame"] = d.TryGetValue("PlayerFrame", out var fr) ? fr : "",
                        ["PlayerCharacterFavourite"] = d.TryGetValue("PlayerCharacterFavourite", out var fav) ? fav : 0
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Ranking] {e.GetType().Name}: {e.Message}");
            }
            return result;
        }

        /* ------------------------------------------------------------------
         *  My rank (1-based). SDK v1 pagination – without AggregateQuery.
         * ----------------------------------------------------------------*/
        public static async UniTask<int> GetPlayerRankAsync()
        {
            try
            {
                await FirebaseAuthHandler.EnsureInitializedAsync();

                string uid = Auth.CurrentUser?.UserId;
                if (string.IsNullOrEmpty(uid))
                    return 0;

                var myDoc = await DB.Collection("Players").Document(uid).GetSnapshotAsync();
                if (!myDoc.Exists || !myDoc.ContainsField("score"))
                    return 0;

                long myScore = Convert.ToInt64(myDoc.GetValue<object>("score"));

                /* Count docs with score > myScore 5k */
                const int PAGE = 5000;
                int higher = 0;

                Query q = DB.Collection("Players")
                            .WhereGreaterThan("score", myScore)
                            .Limit(PAGE);

                while (true)
                {
                    var page = await q.GetSnapshotAsync();
                    higher += page.Count;

                    if (page.Count < PAGE) break;         
                    q = q.StartAfter(page.Documents.Last());
                }
                return higher + 1;                         
            }
            catch (Exception e)
            {
                Debug.LogError($"[Ranking] {e.GetType().Name}: {e.Message}");
                return 0;
            }
        }
    }
}
#endif