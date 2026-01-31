using UnityEngine;

namespace BulletHellTemplate.PVP
{
    public enum PvpModeType { TeamDeathmatch, Arena, BattleRoyale }

    public abstract class PvpModeData : ScriptableObject
    {
        [Header("Identity")]
        public string battleName;
        public Sprite iconPreview;

        [Header("Scene")]
        public string sceneName;

        [SerializeField, HideInInspector] protected PvpModeType modeType;
        public PvpModeType ModeType => modeType;

        public abstract int GetMaxPlayers();
        public abstract int GetTeamCount();
        public abstract int GetPlayersPerTeam();

        public virtual string GetModeKey() => $"{modeType}";
    }
}
