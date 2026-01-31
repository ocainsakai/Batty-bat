using System;

namespace BulletHellTemplate
{
    [Serializable]
    public class EndGameSessionData
    {
        public bool won;
        public int mapId;
        public int characterId;
        public int monstersKilled;
        public int gainedGold;
    }
}