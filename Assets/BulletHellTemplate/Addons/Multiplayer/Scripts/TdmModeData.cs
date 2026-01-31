using UnityEngine;

namespace BulletHellTemplate.PVP
{
    [CreateAssetMenu(menuName = "PVP/Team Deathmatch", fileName = "TdmModeData")]
    public class TdmModeData : PvpModeData
    {
        [Header("Rules TDM")]
        [Min(2)] public int teamCount = 2;
        [Min(1)] public int playersPerTeam = 2;
        [Min(30)] public int matchTimeSeconds = 300;
        [Min(1)] public int pointsPerKill = 1;

        [Range(0.01f, 1f)] public float playerDamageTakenMultiplier = 0.1f;
        private void OnEnable() => modeType = PvpModeType.TeamDeathmatch;

        public override int GetMaxPlayers() => teamCount * playersPerTeam;
        public override int GetTeamCount() => teamCount;
        public override int GetPlayersPerTeam() => playersPerTeam;
    }
}
