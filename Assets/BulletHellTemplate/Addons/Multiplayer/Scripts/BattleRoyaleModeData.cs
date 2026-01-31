using UnityEngine;
#if FUSION2

namespace BulletHellTemplate.PVP
{
    [CreateAssetMenu(menuName = "PVP/Battle Royale", fileName = "BattleRoyaleModeData")]
    public class BattleRoyaleModeData : PvpModeData
    {
        [Header("Rules")]
        [Min(2)] public int maxPlayers = 10;
        [Header("Safe Zone")]
        public SafeZoneController zonePrefab;
        public float initialRadius = 40f;
        public float finalRadius = 6f;

        public float[] stageDurations = new float[] { 30, 30, 30, 30, 30 };
        public float[] stageTargetRadius; 
        public float pauseBetweenStages = 10f;

        [Range(0.01f, 1f)] public float playerDamageTakenMultiplier = 0.1f;
        public float damagePerSecondOutside = 10f;
        public float tickInterval = 0.5f;

        private void OnEnable() => modeType = PvpModeType.BattleRoyale;

        public override int GetMaxPlayers() => maxPlayers;
        public override int GetTeamCount() => maxPlayers; // solo
        public override int GetPlayersPerTeam() => 1;
    }
}
#endif