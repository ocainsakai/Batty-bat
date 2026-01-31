using Colyseus.Schema;
#if UNITY_5_3_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace BulletHellTemplate
{
    public partial class AuthState : Schema
    {
#if UNITY_5_3_OR_NEWER
        [Preserve]
#endif
        public AuthState() { }

    }
}

