#if FUSION2
using Fusion;
#endif
using UnityEngine;
using Cysharp.Threading.Tasks;
using BulletHellTemplate.PVP;
using System.Collections.Generic;
using System.Linq;

namespace BulletHellTemplate
{
#if FUSION2
    [RequireComponent(typeof(NetworkObject))]
#endif
    public class PvpSync :
#if FUSION2
        NetworkBehaviour   
#else
        MonoBehaviour
#endif
    {
        public static PvpSync Instance { get; private set; }

#if FUSION2
        /* -------------------- Config -------------------- */
        [Networked] public NetworkBool RulesReady { get; private set; }
        [Networked] public NetworkBool TeamsAssigned { get; private set; }
        [Networked] public PvpModeType Mode { get; private set; }
        [Networked] public int TeamCount { get; private set; }
        [Networked] public int PlayersPerTeam { get; private set; }

        // Team Deathmatch
        [Networked] public int MatchTimeSeconds { get; private set; }
        [Networked] public int PointsPerKill { get; private set; }   

        // Arena
        public int KillLimit { get; private set; } = 5; 

        [Networked] public int TimeLeft { get; private set; }

        [Networked] public NetworkDictionary<byte, int> TeamScores => default;

        [Networked] public int Team0Kills { get; set; }
        [Networked] public int Team1Kills { get; set; }

        private TickTimer _tickTimer;

        [Networked] public float PlayerDamageTakenMultiplier { get; private set; }

        public static bool IsSpawnedReady =>
            Instance != null && Instance.Object != null && Instance.Object.IsValid;
#endif


        public float ReviveDelay { get; private set; } = 3f;

        private void Awake() => Instance = this;

        /* ----------------------------- API ----------------------------- */
#if FUSION2
        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                RulesReady = false; 
                TeamsAssigned = false;
            }
        }

        public int GetTeamScore(byte team)
            => TeamScores.TryGet(team, out var s) ? s : 0;

        public void InitializeFrom(PvpModeData data)
        {
            if (!HasStateAuthority) return;

            Mode = data.ModeType;
            TeamCount = data.GetTeamCount();
            PlayersPerTeam = data.GetPlayersPerTeam();

            for (byte i = 0; i < TeamCount; i++)
                TeamScores.Set(i, 0);

            ReviveDelay = 3f;
            PlayerDamageTakenMultiplier = 1f;

            switch (Mode)
            {
                case PvpModeType.TeamDeathmatch:
                    var tdm = (TdmModeData)data;
                    MatchTimeSeconds = Mathf.Max(30, tdm.matchTimeSeconds);
                    PointsPerKill = Mathf.Max(1, tdm.pointsPerKill);
                    PlayerDamageTakenMultiplier = Mathf.Clamp01(tdm.playerDamageTakenMultiplier);
                    TimeLeft = MatchTimeSeconds;
                    _tickTimer = TickTimer.CreateFromSeconds(Runner, 1f);
                    break;

                case PvpModeType.Arena:
                    var arena = (ArenaModeData)data;
                    KillLimit = Mathf.Max(1, arena.killLimit);
                    ReviveDelay = Mathf.Max(0f, arena.reviveDelay);
                    PlayerDamageTakenMultiplier = Mathf.Clamp01(arena.playerDamageTakenMultiplier); 
                    Team0Kills = 0;
                    Team1Kills = 0;
                    break;
                case PvpModeType.BattleRoyale:
                    {
                        var br = (BattleRoyaleModeData)data;
                        PlayerDamageTakenMultiplier = Mathf.Clamp01(br.playerDamageTakenMultiplier);

                        TeamCount = Runner.ActivePlayers.Count();
                        PlayersPerTeam = 1;

                        if (HasStateAuthority && br.zonePrefab)
                        {
                            var n = Runner.Spawn(br.zonePrefab.gameObject);
                            var zone = n.GetComponent<SafeZoneController>();
                            zone.InitializeFrom(br, Vector3.zero);
                        }
                        break;
                    }
            }

            TeamsAssigned = false;
            RulesReady = false;  
            RPC_SetPvpFlag(true);             
        }

        public void MarkTeamsAssignedAndReady()
        {
            if (!HasStateAuthority) return;
            Debug.Log($"[PVP] MarkTeamsAssignedAndReady() by {Runner.LocalPlayer}");
            TeamsAssigned = true;
            RulesReady = true;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SetPvpFlag(bool enabled)
        {
            GameplayManager.Singleton?.SetPvpSession(enabled);
        }

        public void SetTeamScore(byte team, int value)
        {
            if (Object && Object.HasStateAuthority)
                TeamScores.Set(team, value);
            else
                RPC_SetTeamScore(team, value);
        }

        public void AddTeamScore(byte team, int delta)
        {
            if (Object && Object.HasStateAuthority)
                AddTeamScoreAuthority(team, delta);
            else
                RPC_AddTeamScore(team, delta);
        }

        public void ReportKill(byte killerTeam)
        {
            RPC_ReportKill(killerTeam);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_NotifyKill(byte killerCid, NetworkString<_32> killerNick,
                           byte victimCid, NetworkString<_32> victimNick,
                           byte killerTeam)
        {
            switch (Mode)
            {
                case PvpModeType.TeamDeathmatch:
                    AddTeamScoreAuthority(killerTeam, Mathf.Max(1, PointsPerKill));
                    break;

                case PvpModeType.Arena:
                    AddTeamScoreAuthority(killerTeam, 1);
                    if (killerTeam == 0) Team0Kills++; else Team1Kills++;
                    if (GetTeamScore(killerTeam) >= KillLimit)
                        EndMatchWithWinner(killerTeam);
                    break;

                case PvpModeType.BattleRoyale:
                    AddTeamScoreAuthority(killerTeam, 1);
                    break;
            }
            RPC_AnnounceKill(killerCid, killerNick, victimCid, victimNick);

            if (Mode == PvpModeType.BattleRoyale)
                CheckAliveAndMaybeEnd();
        }



        public async UniTaskVoid ScheduleRevive(CharacterEntity who)
        {
            if (!HasStateAuthority || !who) return;
            if (Mode == PvpModeType.BattleRoyale) return;
            await UniTask.Delay(System.TimeSpan.FromSeconds(ReviveDelay));
            if (who && who.IsDead) who.CharacterRevive();
        }

        /* ----------------------------- RPCs ---------------------------- */

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_SetTeamScore(byte team, int value)
            => TeamScores.Set(team, value);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AddTeamScore(byte team, int delta)
            => AddTeamScoreAuthority(team, delta);

        private void AddTeamScoreAuthority(byte team, int delta)
        {
            var cur = TeamScores.TryGet(team, out var s) ? s : 0;
            TeamScores.Set(team, cur + delta);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ReportKill(byte killerTeam)
        {
            if (killerTeam >= TeamCount) return;

            switch (Mode)
            {
                case PvpModeType.TeamDeathmatch:
                    AddTeamScoreAuthority(killerTeam, Mathf.Max(1, PointsPerKill));
                    break;

                case PvpModeType.Arena:
                    AddTeamScoreAuthority(killerTeam, 1);

                    if (killerTeam == 0) Team0Kills++; else Team1Kills++;

                    if (GetTeamScore(killerTeam) >= KillLimit)
                        EndMatchWithWinner(killerTeam);
                    break;

                case PvpModeType.BattleRoyale:
                    break;
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_AnnounceKill(byte killerCid, NetworkString<_32> killerNick,
                                     byte victimCid, NetworkString<_32> victimNick)
        {
            var kData = GameInstance.Singleton.GetCharacterDataById(killerCid);
            var vData = GameInstance.Singleton.GetCharacterDataById(victimCid);

            var kIcon = kData ? kData.icon : null;
            var vIcon = vData ? vData.icon : null;

            UIGameplay.Singleton?.PushKillFeed(kIcon, killerNick.ToString(), vIcon, victimNick.ToString());
        }
        public void AnnounceEliminationNow(CharacterEntity victim)
        {
            if (!HasStateAuthority || Mode != PvpModeType.BattleRoyale || victim == null || !victim.Object)
                return;

#if UNITY_6000_0_OR_NEWER
            var all = UnityEngine.Object.FindObjectsByType<CharacterEntity>(FindObjectsSortMode.None);
#else
    var all = UnityEngine.Object.FindObjectsOfType<CharacterEntity>();
#endif
            int alive = all.Count(c => c && !c.IsDead);
            int placement = alive + 1; 

            RPC_AnnounceBattleRoyalePlacement(victim.Object.InputAuthority, placement);
        }


        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_AnnounceWinnerWithScores(PvpModeType mode, int winnerTeam, int[] scores)
        {
            UIGameplay.Singleton?.ShowPvpEndScreen(mode, winnerTeam, scores, -1);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_AnnounceBattleRoyalePlacement(PlayerRef target, int myPlacement)
        {
            if (target != Runner.LocalPlayer) return;
            UIGameplay.Singleton?.ShowPvpEndScreen(PvpModeType.BattleRoyale, -1, null, myPlacement);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_AnnounceWinner(int winnerTeam)
        {
            UIGameplay.Singleton?.ShowPvpEndScreen(Mode, winnerTeam, null, -1);
        }

        /* -------------------------- Lifecycle -------------------------- */

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;

            if (Mode == PvpModeType.TeamDeathmatch && _tickTimer.Expired(Runner))
            {
                _tickTimer = TickTimer.CreateFromSeconds(Runner, 1f);
                if (TimeLeft > 0) TimeLeft--;
                if (TimeLeft <= 0) TryEndByTime();
            }
        }

        public void OnTeamEliminatedCheck(int aliveTeams)
        {
            if (!HasStateAuthority || Mode != PvpModeType.BattleRoyale) return;
            if (aliveTeams <= 1)
            {
                EndMatchWithWinner(-1); 
            }
        }

        private void TryEndByTime()
        {
            if (!HasStateAuthority || Mode != PvpModeType.TeamDeathmatch) return;
            if (TeamCount <= 0)
            {
                EndMatchWithWinner(-1);
                return;
            }

            int bestTeam = 0;
            int bestScore = GetTeamScore(0);

            for (int i = 1; i < TeamCount; i++)
            {
                int s = GetTeamScore((byte)i);
                if (s > bestScore)
                {
                    bestScore = s;
                    bestTeam = i;
                }
            }

            EndMatchWithWinner(bestTeam);
        }
     
        private static int GetTeamOf(NetworkRunner runner, PlayerRef pref)
        {
            if (runner == null || !runner.IsRunning)
                return 0;

            var list = runner.ActivePlayers.OrderBy(p => p.RawEncoded).ToList();
            int idx = list.IndexOf(pref);
            if (idx < 0) return 0;

            int teams = Mathf.Max(1, Instance ? Instance.TeamCount : 1);
            return idx % teams;
        }

        private void CheckAliveAndMaybeEnd()
        {
            if (!HasStateAuthority || Mode != PvpModeType.BattleRoyale) return;
#if UNITY_6000_0_OR_NEWER
            var all = FindObjectsByType<CharacterEntity>(FindObjectsSortMode.None);
#else
            var all = FindObjectsOfType<CharacterEntity>();
#endif
            var alive = all.Where(c => c && !c.IsDead && (!c.IsNetworked || (c.Object && c.Object.IsValid))).ToList();

            if (alive.Count <= 1)
            {
                int winnerTeam = -1;
                if (alive.Count == 1)
                {
                    var ce = alive[0];
                    if (ce.IsNetworked && ce.Object)
                        winnerTeam = GetTeamOf(Runner, ce.Object.InputAuthority);

                }
                EndMatchWithWinner(winnerTeam);
            }
        }


        private void EndMatchWithWinner(int teamIndex)
        {
            if (!HasStateAuthority) return;

            switch (Mode)
            {
                case PvpModeType.TeamDeathmatch:
                    {
                        var scores = new int[TeamCount];
                        for (int i = 0; i < TeamCount; i++)
                            scores[i] = GetTeamScore((byte)i);

                        RPC_AnnounceWinnerWithScores(Mode, teamIndex, scores);
                        break;
                    }

                case PvpModeType.Arena:
                    {
                        var scores = new int[TeamCount];
                        for (int i = 0; i < TeamCount; i++)
                            scores[i] = GetTeamScore((byte)i);

                        RPC_AnnounceWinnerWithScores(Mode, teamIndex, scores);
                        break;
                    }

                case PvpModeType.BattleRoyale:
                    {
                        var rank = Enumerable.Range(0, TeamCount)
                                             .OrderByDescending(t => GetTeamScore((byte)t))
                                             .ToList();

                        foreach (var p in Runner.ActivePlayers)
                        {
                            int myTeam = GetTeamOf(Runner, p);     
                            int placement = rank.IndexOf(myTeam) + 1;
                            RPC_AnnounceBattleRoyalePlacement(p, placement);
                        }
                        break;
                    }
            }

            GameplayManager.Singleton?.EndGame();
        }
#endif
    }
}

