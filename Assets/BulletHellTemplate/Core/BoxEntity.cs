#if FUSION2
using Fusion;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using BulletHellTemplate.VFX;

namespace BulletHellTemplate
{
#if FUSION2
    [RequireComponent(typeof(NetworkObject))]
    public class BoxEntity : NetworkBehaviour
#else
    public class BoxEntity : MonoBehaviour
#endif
    {
        [Header("HP")]
        public int maxHP = 100;
        private int currentHP;

        [System.Serializable] public class DropChance { public DropEntity drop; public int chance; }
        [Header("Drops")]
        public List<DropChance> dropChances;
        public int hpValue, xpValue, goldValue, shieldValue;

        [Header("UI (igual monstro)")]
        public GameObject hpContainer;
        public Image hpBar, hpBarDecrease;
        public float decreaseDelay = 0.12f, decreaseSpeed = 1f;
        private Coroutine hpAnimCo;

        [Header("UnityEvents")]
        public UnityEvent OnEnableEvent, OnDisableEvent;
        [System.Serializable] public class IntEvent : UnityEvent<int> { }
        public IntEvent OnReceiveHit;

#if FUSION2
        [Networked, OnChangedRender(nameof(OnNetHpChanged))] private int NetHp { get; set; }
#endif

        void Start()
        {
            currentHP = Mathf.Max(1, maxHP);
#if FUSION2
            if (Object && HasStateAuthority) NetHp = currentHP;
#endif
            SetBothBars(1f);
            if (hpContainer) hpContainer.SetActive(false);
        }

        void OnEnable()
        {
            GameplayManager.Singleton?.ActiveBoxesList.Add(transform);
            OnEnableEvent?.Invoke();
        }
        void OnDisable()
        {
            GameplayManager.Singleton?.ActiveBoxesList.Remove(transform);
            OnDisableEvent?.Invoke();
        }

        public void ReceiveDamage(int damage)
        {
            if (damage <= 0) return;

#if FUSION2
            if (Runner && Runner.IsRunning && Object && !HasStateAuthority)
            {
                RPC_RequestDamage(damage);   // pede para a autoridade aplicar
                return;
            }
#endif
            ApplyDamageInternal(damage);
#if FUSION2
            if (Runner && Runner.IsRunning && Object && HasStateAuthority)
                NetHp = currentHP;
#endif
        }

#if FUSION2
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestDamage(int damage)
        {
            ApplyDamageInternal(damage);
            NetHp = currentHP;
        }

        private void OnNetHpChanged()
        {
            LocalApplyHp(currentHP, NetHp);
            currentHP = NetHp;
        }
#endif

        private void ApplyDamageInternal(int damage)
        {
            int old = currentHP;
            currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
            OnReceiveHit?.Invoke(damage);
            LocalApplyHp(old, currentHP);
            if (currentHP <= 0) DestroyBox();
        }

        private void LocalApplyHp(int oldValue, int newValue)
        {
            float n = maxHP > 0 ? (float)newValue / maxHP : 0f;

            if (hpContainer && newValue < maxHP)
                hpContainer.SetActive(true);

            if (hpBar) hpBar.fillAmount = n;
            if (newValue < oldValue)
            {
                if (hpAnimCo != null) StopCoroutine(hpAnimCo);
                hpAnimCo = StartCoroutine(DecreaseBarSmooth(n));
            }
            else SetBothBars(n);
        }
        private System.Collections.IEnumerator DecreaseBarSmooth(float target)
        {
            yield return new WaitForSeconds(decreaseDelay);
            while (hpBarDecrease && hpBarDecrease.fillAmount > target)
            {
                hpBarDecrease.fillAmount -= Time.deltaTime * decreaseSpeed;
                yield return null;
            }
            if (hpBarDecrease) hpBarDecrease.fillAmount = target;
            hpAnimCo = null;
        }
        private void SetBothBars(float n)
        {
            if (hpBar) hpBar.fillAmount = n;
            if (hpBarDecrease) hpBarDecrease.fillAmount = n;
        }

        private bool NetActive =>
#if FUSION2
      Runner && Runner.IsRunning;
#else
    false;
#endif

        private void DestroyBox()
        {
            if (TryPickDrop(out int idx))
            {
                var entry = dropChances[idx];
                if (entry?.drop)
                {
                    var type = entry.drop.type;
                    int amount = ResolveValueByType(type);

#if FUSION2
                    if (NetActive && HasStateAuthority)
                    {
                        RPC_SpawnDrop(idx, amount, transform.position);
                    }
                    else
#endif
                    {
                        var drop = DropPool.Spawn(entry.drop, transform.position);
                        drop.SetValue(amount);
                    }
                }
            }

            Destroy(gameObject);
        }

        private bool TryPickDrop(out int index)
        {
            index = -1;
            if (dropChances == null || dropChances.Count == 0) return false;

            int total = 0;
            foreach (var c in dropChances) if (c?.drop) total += Mathf.Max(0, c.chance);
            if (total <= 0) return false;

            int pick = Random.Range(0, total), acc = 0;
            for (int i = 0; i < dropChances.Count; i++)
            {
                var c = dropChances[i];
                if (c == null || c.drop == null || c.chance <= 0) continue;
                acc += c.chance;
                if (pick < acc) { index = i; return true; }
            }
            return false;
        }

        private int ResolveValueByType(DropEntity.DropType t) => t switch
        {
            DropEntity.DropType.Gold => goldValue,
            DropEntity.DropType.Experience => xpValue,
            DropEntity.DropType.Health => hpValue,
            DropEntity.DropType.Shield => shieldValue,
            _ => 0
        };

#if FUSION2
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SpawnDrop(int dropIndex, int amount, Vector3 pos)
        {
            if (dropIndex < 0 || dropIndex >= dropChances.Count) return;
            var entry = dropChances[dropIndex];
            if (!entry?.drop) return;

            var drop = DropPool.Spawn(entry.drop, pos);
            drop.SetValue(amount);
        }
#endif
    }
}
