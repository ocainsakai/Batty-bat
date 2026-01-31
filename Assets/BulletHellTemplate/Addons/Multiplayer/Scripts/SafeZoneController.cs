using Cysharp.Threading.Tasks;
#if FUSION2
using Fusion;
#endif
using System;
using System.Linq;
using UnityEngine;
using BulletHellTemplate.PVP;

namespace BulletHellTemplate
{
#if FUSION2
    [RequireComponent(typeof(NetworkObject))]
#endif
    public class SafeZoneController :
#if FUSION2
        NetworkBehaviour   
#else
        MonoBehaviour
#endif
    {
        [SerializeField] private SphereCollider sphere; // isTrigger
        [SerializeField] private Transform visual;

#if FUSION2
        [Networked] public Vector3 Center { get; private set; }
        [Networked] public float InitialRadius { get; private set; }
        [Networked] public float Radius { get; private set; }

        private BattleRoyaleModeData _data;
        private TickTimer _tick;

        public void InitializeFrom(BattleRoyaleModeData data, Vector3 center)
        {
            if (!HasStateAuthority) return;
            _data = data;
            Center = center;
            InitialRadius = data.initialRadius;
            Radius = data.initialRadius;
            UpdateLocalVisuals();
            ShrinkRoutine().Forget();
        }

        public override void FixedUpdateNetwork()
        {
            UpdateLocalVisuals(); 

            if (!HasStateAuthority || _data == null) return;

            if (_tick.ExpiredOrNotRunning(Runner))
            {
                _tick = TickTimer.CreateFromSeconds(Runner, Mathf.Max(0.05f, _data.tickInterval));
                ApplyDamageOutside();
            }
        }

        private void UpdateLocalVisuals()
        {
            if (sphere) sphere.radius = Radius;
            if (visual) visual.localScale = Vector3.one * (Radius / InitialRadius); 
            transform.position = Center;
        }

        private async UniTaskVoid ShrinkRoutine()
        {
            if (!HasStateAuthority || _data == null) return;

            int stages = Mathf.Max(1, _data.stageDurations != null ? _data.stageDurations.Length : 0);
            float current = Radius;

            for (int i = 0; i < stages; i++)
            {
                float duration = (_data.stageDurations != null && _data.stageDurations.Length > i)
                                     ? _data.stageDurations[i]
                                     : 30f;

                float target = (_data.stageTargetRadius != null && _data.stageTargetRadius.Length > i)
                                     ? _data.stageTargetRadius[i]
                                     : Mathf.Lerp(_data.initialRadius, _data.finalRadius, (i + 1f) / stages);

                float t = 0f;
                while (t < duration)
                {
                    t += Runner.DeltaTime;
                    Radius = Mathf.Lerp(current, target, Mathf.Clamp01(t / duration));
                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
                }
                current = target;

                if (_data.pauseBetweenStages > 0)
                    await UniTask.Delay(TimeSpan.FromSeconds(_data.pauseBetweenStages));
            }
        }

        private void ApplyDamageOutside()
        {
#if UNITY_6000_0_OR_NEWER
            var chars = FindObjectsByType<CharacterEntity>(FindObjectsSortMode.None);
#else
            var chars = FindObjectsOfType<CharacterEntity>();
#endif
            float chunk = Mathf.Max(0.01f, _data.damagePerSecondOutside) * Mathf.Max(0.05f, _data.tickInterval);

            foreach (var ce in chars)
            {
                if (!ce || ce.IsDead) continue;

                float dist = Vector3.Distance(ce.transform.position, Center);
                if (dist > Radius)
                {
                    int dmg = Mathf.CeilToInt(chunk);
     
                    ce.ReceiveDamage(dmg);
                }
            }
        }
#endif
    }
}

