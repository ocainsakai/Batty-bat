using UnityEngine;

namespace BulletHellTemplate.VFX
{
    /// <summary>
    ///     Scene‑level manager that pre‑heats the DamagePopup pool.
    /// </summary>
    [AddComponentMenu("Bullet Hell Template/VFX/Damage Popup Manager")]
    public sealed class DamagePopupManager : MonoBehaviour
    {
        [Header("Prefab & Pooling")]
        [Tooltip("Styled DamagePopup passEntryPrefab to duplicate. If left blank, a minimal default is used.")]
        [SerializeField] private GameObject damagePopupPrefab;

        [Tooltip("How many instances to pre‑allocate to avoid runtime allocation spikes.")]
        [Range(8, 256)]
        [SerializeField] private int initialPoolSize = 32;

        private void Awake()
        {
            DamagePopup.Configure(damagePopupPrefab, initialPoolSize);
        }
    }
}
