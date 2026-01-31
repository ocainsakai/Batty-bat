#if FUSION2

using Fusion;
using System.Threading.Tasks;
using UnityEngine;

namespace BulletHellTemplate
{
    public static class FusionUtils
    {
        /// <summary>
        /// Waits for a component to become available on the root or children of a NetworkObject.
        /// </summary>
        public static async Task<T> WaitForComponent<T>(NetworkObject obj, int maxFrames = 60) where T : Component
        {
            T component = null;
            while (component == null && maxFrames-- > 0)
            {
                component = obj.GetComponent<T>() ?? obj.GetComponentInChildren<T>(true);
                await Task.Yield();
            }
            return component;
        }
    }
}
#endif