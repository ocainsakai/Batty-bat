#if FUSION2
using Fusion;
using UnityEngine;

namespace BulletHellTemplate
{
    public static class FusionHelpers
    {
        /// <summary>Returns the current runner, if any, starting from a NetworkBehaviour or NetworkObject.</summary>
        public static NetworkRunner GetRunner(Component c)
            => c.TryGetComponent<NetworkObject>(out var nObj) ? nObj.Runner : null;
    }
}
#endif