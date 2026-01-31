#if FUSION2
using Fusion;
#endif

using UnityEngine;

namespace BulletHellTemplate
{
#if FUSION2
    [RequireComponent(typeof(NetworkObject))]
#endif
    public class SharedModeMasterClientTracker :
#if FUSION2
        NetworkBehaviour   
#else
        MonoBehaviour
#endif
    {
        static SharedModeMasterClientTracker LocalInstance;

#if FUSION2
        public override void Spawned()
        {
            LocalInstance = this;
        }

        private void OnDestroy()
        {
            if (LocalInstance == this)
                LocalInstance = null;
        }

        public static bool IsPlayerSharedModeMasterClient(PlayerRef player)
        {
            if (LocalInstance == null)
                return false;

            return LocalInstance.Object.StateAuthority == player;
        }

        public static PlayerRef? GetSharedModeMasterClientPlayerRef()
        {
            if (LocalInstance == null)
                return null;

            return LocalInstance.Object.StateAuthority;
        }
#endif
    }
}
