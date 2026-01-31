using BulletHellTemplate.VFX;
using DentedPixel;            
using UnityEngine;

namespace BulletHellTemplate
{
    [DisallowMultipleComponent]
    public sealed class DropEntity : MonoBehaviour
    {
        /*──────── CONFIG────────*/
        public enum DropType
        {
            Gold, Experience, Health, Shield,
            CollectAll, CollectGold, CollectExperience, CollectHealth
        }

        [Header("Drop Settings")]
        public DropType type;
        [SerializeField] private int value;
        public bool canAutoCollect = true;
        /*──────── Runtime ───────*/
        private Transform _target;
        private float _collectRange;
        private bool _magnetActive;
        private LTDescr _spinTween;

        private void OnEnable()
        {
            _spinTween = LeanTween.rotateAround(
                             gameObject, Vector3.up, 360f, 3f)
                         .setRepeat(-1)
                         .setEaseLinear();

#if UNITY_6000_0_OR_NEWER
            var player = FindAnyObjectByType<CharacterEntity>();
#else
            var player = FindObjectOfType<CharacterEntity>();
#endif
            if (player)
            {
                _target = player.transform;
                _collectRange = player.GetCurrentCollectRange();
            }
        }

        private void OnDisable()
        {
            if (_spinTween != null) LeanTween.cancel(_spinTween.uniqueId);
            _magnetActive = false;
            canAutoCollect = true;     
        }

        public void SetValue(int amount)
        {
            value = amount;
        }

        /*──────────────────────── UPDATE ──────────────────────────────*/
        private void Update()
        {
            if (GameplayManager.Singleton.IsPaused()) return;
            if (!_target) return;

            if (!_magnetActive && canAutoCollect)
            {
                float dist = Vector3.Distance(transform.position, _target.position);
                if (dist <= _collectRange)
                    StartMagnet();
            }

            if (_magnetActive)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    _target.position,
                    Time.deltaTime * 12f);
            }
        }

        /*──────────────────────── MAGNET FX ───────────────────────────*/
        private void StartMagnet()
        {
            _magnetActive = true;
            canAutoCollect = false;

            Vector3 back = transform.position - transform.forward * 0.6f;
            LeanTween.move(gameObject, back, 0.08f)
                     .setEaseOutQuad();
        }
        /*──────────────────────── PICKUP ──────────────────────────────*/
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Character")) return;
            var player = other.GetComponent<CharacterEntity>();
            if (!player || GameplayManager.Singleton.IsPaused()) return;

            if (type is DropType.CollectAll or DropType.CollectGold
                          or DropType.CollectExperience or DropType.CollectHealth)
            {
                AttractDrops(player, type);
            }
            else
            {
                ApplyDrop(player);
            }

            DropPool.Release(this);   
        }

        private void ApplyDrop(CharacterEntity player)
        {
            switch (type)
            {
                case DropType.Gold: GameplayManager.Singleton.IncrementGainGold(value); break;
                case DropType.Experience: GameplayManager.Singleton.IncrementGainXP(value); ; break;
                case DropType.Health: player.ApplyHealHP(value); break;
                case DropType.Shield: player.ApplyShield(value); break;
            }
        }

        /*──────────────────────── COLLECT-ALL ─────────────────────────*/
        private static void AttractDrops(CharacterEntity player, DropType collector)
        {
#if UNITY_6000_0_OR_NEWER
            var drops = FindObjectsByType<DropEntity>(FindObjectsSortMode.None);
#else
            var drops = FindObjectsOfType<DropEntity>();
#endif
            foreach (var d in drops)
                if (ShouldAttract(collector, d.type))
                {
                    d._target = player.transform;
                    d._collectRange = float.MaxValue;
                    d.StartMagnet();
                }
        }
        private static bool ShouldAttract(DropType collector, DropType drop) =>
               collector == DropType.CollectAll && drop < DropType.CollectAll
            || collector == DropType.CollectGold && drop == DropType.Gold
            || collector == DropType.CollectExperience && drop == DropType.Experience
            || collector == DropType.CollectHealth && drop == DropType.Health;
    }
}
