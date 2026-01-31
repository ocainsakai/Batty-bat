using System;

namespace BulletHellTemplate
{
    [Serializable]
    public class EndGameQuestProgressData
    {
        public bool won;
        public int mapId;
        public int characterId;
        public int monstersKilled;
    }
}
