using UnityEngine;

namespace BulletHellTemplate.PVP
{
    [CreateAssetMenu(menuName = "PVP/Arena", fileName = "ArenaModeData")]
    public class ArenaModeData : PvpModeData
    {
        public enum TeamSize { One = 1, Two = 2, Three = 3 }

        [Header("Rules Arena")]
        public TeamSize teamSize = TeamSize.One;   // 1x1, 2x2 ou 3x3
        [Min(1)] public int killLimit = 5;

        [Header("Arena Tuning")]
        [Min(0f)] public float reviveDelay = 3f;
        [Range(0.01f, 1f)] public float playerDamageTakenMultiplier = 0.1f;

        private void OnEnable() => modeType = PvpModeType.Arena;

        public override int GetMaxPlayers() => 2 * (int)teamSize;
        public override int GetTeamCount() => 2;
        public override int GetPlayersPerTeam() => (int)teamSize;
    }
}
