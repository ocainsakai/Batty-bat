using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using BulletHellTemplate.VFX;
using Cysharp.Threading.Tasks;
using System;
using BulletHellTemplate.Core.Events;

#if FUSION2
using Fusion;
#endif

namespace BulletHellTemplate
{
    /// <summary>
    /// Network-ready health component for any monster-type entity.
    /// Keeps the server authoritative, replicates hit-points to all clients,
    /// and triggers UI updates via SyncVar hook or RPCs.
    /// </summary>
    [DisallowMultipleComponent]
#if FUSION2
    public class MonsterHealth : NetworkBehaviour, IDamageable
#else
    public class MonsterHealth : MonoBehaviour, IDamageable
#endif
    {
        /* ════ CONFIGURATION ═════════════════════════════════════════════ */

        [Header("Stats Reference")]
        private CharacterStats stats;       

        [Header("UI")]
        private Transform damagePopupContainer;
        private GameObject hpContainer;
        private Image hpBar;
        private Image hpBarDecrease;

        [Header("UI Timing")]
        [SerializeField] private float decreaseDelay = 0.12f;
        [SerializeField] private float decreaseSpeed = 1f;

        /* ════ PUBLIC EVENTS ════════════════════════════════════════════ */

        public Action<float> OnHealthChanged;
        public Action OnDeath;

        /* ════ NETWORK STATE ════════════════════════════════════════════ */

#if FUSION2
        /// <summary>
        /// HitKey uniquely identifies a hit attempt coming from an attacker.
        /// </summary>
        private struct HitKey : IEquatable<HitKey>
        {
            public NetworkId Attacker;
            public ulong HitId;

            public bool Equals(HitKey other) => Attacker == other.Attacker && HitId == other.HitId;
            public override bool Equals(object obj) => obj is HitKey other && Equals(other);
            public override int GetHashCode() => (Attacker.GetHashCode() * 486187739) ^ HitId.GetHashCode();
        }

        // Dedup storage with short TTL to prevent memory growth.
        private readonly Dictionary<HitKey, float> _hitSeenAt = new();
        private const float HIT_TTL_SECONDS = 3f;

        private void PruneOldHits()
        {
            if (_hitSeenAt.Count == 0) return;
            float now = Time.time;
            _pruneBuffer ??= new List<HitKey>(8);
            _pruneBuffer.Clear();
            foreach (var kv in _hitSeenAt)
                if (now - kv.Value > HIT_TTL_SECONDS)
                    _pruneBuffer.Add(kv.Key);

            for (int i = 0; i < _pruneBuffer.Count; i++)
                _hitSeenAt.Remove(_pruneBuffer[i]);
        }
        private List<HitKey> _pruneBuffer;


        private float _queuedHp = -1f; 

#endif

        /// <summary>
        /// Current HP, replicated to every observing client.
        /// All modifications **must** happen on the server.
        /// </summary>
        private float currentHp;

        
        private bool _initialized;
        public float CurrentHp
        {
#if FUSION2
            get
            {
                if (Runner == null || Object == null)
                    return currentHp;

                return HasStateAuthority ? currentHp : NetHp;
            }
#else
            get => currentHp;
#endif
        }


#if FUSION2
        [Networked, OnChangedRender(nameof(OnNetHpChanged))]
        public float NetHp { get; set; }
#endif

        /* ════ PROPERTIES ═══════════════════════════════════════════════ */

        private MonsterEntity monsterOwner;
        public CharacterStats Stats => stats;
        public float MaxHp => stats.baseHP;

        /* ════ PRIVATE STATE ════════════════════════════════════════════ */

        private Coroutine hpAnimCo;
        private readonly Dictionary<SkillData, Coroutine> dotsSkill = new();
        private readonly Dictionary<SkillPerkData, Coroutine> dotsPerk = new();

        /* ════ INITIALISATION ═══════════════════════════════════════════ */

        private void Awake()
        {
            monsterOwner = GetComponent<MonsterEntity>();
        }

        /// <summary>
        /// Called by the monster entity right after spawning.
        /// Must be executed on both server **and** client, but only the
        /// server changes authoritative values.
        /// </summary>
        public void Setup(
            CharacterStats _stats,
            Transform popupRoot,
            GameObject hpRoot,
            Image hpMain,
            Image hpDelayed,
            float _decreaseDelay,
            float _decreaseSpeed)
        {
            stats = _stats;
            damagePopupContainer = popupRoot;
            hpContainer = hpRoot;
            hpBar = hpMain;
            hpBarDecrease = hpDelayed;
            decreaseDelay = _decreaseDelay;
            decreaseSpeed = _decreaseSpeed;

            currentHp = _stats.baseHP;

#if FUSION2
            if (_queuedHp >= 0f)
            {
                float old = currentHp;
                currentHp = _queuedHp;
                LocalApplyHp(old, currentHp);
                _queuedHp = -1f;
            }
#endif
            SetBothBars(currentHp / MaxHp);
            if (hpContainer) hpContainer.SetActive(false);

            _initialized = true;
            RegenLoopAsync().Forget();
        }

        /* ════ DAMAGE / HEAL / DOT / REGEN  ─ SERVER ONLY ─═════════════ */

        /// <summary>
        /// Applies damage on the **server**. Clients must call CmdRequestDamage().
        /// </summary>
        /// <param name="amount">Raw damage.</param>
        /// <param name="critical">Was it a critical hit?</param>
        public void ReceiveDamage(float amount, bool critical = false)
        {
            if (amount <= 0f || CurrentHp <= 0f) return;

#if FUSION2
            if (Runner != null && Object != null && !HasStateAuthority) return;
#endif

            float oldHp = currentHp;
            currentHp = Mathf.Clamp(currentHp - amount, 0f, MaxHp);

#if FUSION2
            if (Runner != null && Runner.IsRunning && Object && Object.IsValid)
            {
                if (HasStateAuthority)
                {
                    NetHp = currentHp;
                    RPC_ShowDamage((int)amount, critical);
                }
            }
            else
            {
                ShowDamagePopup((int)amount, critical);
            }
#else
            ShowDamagePopup((int)amount, critical);
#endif

            LocalApplyHp(oldHp, currentHp);

            if (currentHp <= 0f)
                OnDeath?.Invoke();
        }

#if FUSION2
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, InvokeLocal = true)] 
        private void RPC_ShowDamage(int amount, bool critical)
        {
            ShowDamagePopup(amount, critical);
        }
#endif

#if FUSION2
        /// <summary>
        /// Server-side RPC that applies damage if and only if:
        /// - The RPC sender matches the input authority of 'attackerId'.
        /// - The (attackerId, hitId) pair was not processed before (dedup).
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestDamage(float amount, bool critical, NetworkId attackerId, ulong hitId, RpcInfo info = default)
        {
            if (!Object || !HasStateAuthority)
                return;

            if (!Runner.TryFindObject(attackerId, out var attackerObj))
                return;

            if (attackerObj.InputAuthority != info.Source)
                return; 

            var key = new HitKey { Attacker = attackerId, HitId = hitId };
            PruneOldHits();
            if (_hitSeenAt.ContainsKey(key))
                return;

            _hitSeenAt[key] = Time.time;

            ReceiveDamage(amount, critical);
        }
#endif

        /// <summary>
        /// Preferred entry point to apply damage in online sessions.
        /// Clients call this with the attacker's NetworkObject and a unique hit id.
        /// Offline, it applies immediately.
        /// </summary>
        public void ApplyDamageFromHit(
#if FUSION2
            NetworkObject attacker,
#else
            object attacker,
#endif
            ulong hitId, float amount, bool critical)
        {
            if (!_initialized || amount <= 0f) return;

#if FUSION2
            if (Runner == null || Object == null)
            {
                // Offline fallback
                ReceiveDamage(amount, critical);
                return;
            }

            if (HasStateAuthority)
            {
                var key = new HitKey { Attacker = attacker.Id, HitId = hitId };
                PruneOldHits();
                if (_hitSeenAt.ContainsKey(key))
                    return;

                _hitSeenAt[key] = Time.time;
                ReceiveDamage(amount, critical);
                return;
            }

            RPC_RequestDamage(amount, critical, attacker.Id, hitId);
#else
            ReceiveDamage(amount, critical);
#endif
        }

        /// <summary>
        /// Legacy entry point: kept for backward-compatibility.
        /// Online callers should migrate to ApplyDamageFromHit(...) to avoid duplicates.
        /// </summary>
        public void ApplyDamageRequest(float amount, bool critical)
        {
            if (!_initialized || amount <= 0f) return;

#if FUSION2
            if (Runner == null || Object == null)
            {
                ReceiveDamage(amount, critical);
                return;
            }

            if (HasStateAuthority)
            {
                ReceiveDamage(amount, critical);
                return;
            }

            RPC_RequestDamage(amount, critical, default, 0UL);
#else
            ReceiveDamage(amount, critical);
#endif
        }


#if FUSION2
        private void OnNetHpChanged(NetworkBehaviourBuffer prev)
        {
            float oldVal = GetPropertyReader<float>(nameof(NetHp)).Read(prev);

            if (!_initialized)
            {
                _queuedHp = NetHp;
                return;
            }
            LocalApplyHp(oldVal, NetHp);
        }
#endif

        public void ApplyRemoteHp(float newHp)
        {
            float old = currentHp;
            currentHp = Mathf.Clamp(newHp, 0f, MaxHp);
            LocalApplyHp(old, currentHp);  
        }

        public void ApplyHeal(float amount)
        {
            if (amount <= 0f || currentHp <= 0f) return;
            currentHp = Mathf.Min(currentHp + amount, MaxHp);
        }

        public void ApplyDot(SkillData source, int totalDmg, float dur)
        {
            RestartDot(dotsSkill, source, DotRoutine(totalDmg, dur, () => dotsSkill.Remove(source)));
        }


        public void ApplyDot(SkillPerkData source, int totalDmg, float dur)
        {
            RestartDot(dotsPerk, source, DotRoutine(totalDmg, dur, () => dotsPerk.Remove(source)));
        }

        /* ════ NETWORK HOOKS & RPCs ════════════════════════════════════ */

        /// <summary>
        /// Called on **all** instances (server + clients) whenever currentHp changes.
        /// Responsible for updating local UI and raising events.
        /// </summary>
        private void LocalApplyHp(float oldValue, float newValue)
        {
            float normalized = MaxHp > 0 ? newValue / MaxHp : 0f;

            if (hpContainer && newValue < MaxHp)
                hpContainer.SetActive(true);

            UpdateMainBar(normalized);

            if (newValue < oldValue)
            {
                if (hpAnimCo != null) StopCoroutine(hpAnimCo);
                hpAnimCo = StartCoroutine(DecreaseBarSmooth(normalized));
            }
            else
            {
                SetBothBars(normalized);
            }

            if (monsterOwner.IsFinalBoss)
                EventBus.Publish(new BossHealthChangedEvent(monsterOwner, newValue, MaxHp));

            OnHealthChanged?.Invoke(newValue);
        }

        /// <summary>
        /// Displays a floating damage popup.
        /// </summary>
        private void ShowDamagePopup(int amount, bool critical)
        {
            Vector3 pos = damagePopupContainer ? damagePopupContainer.position
                                               : transform.position + Vector3.up * 1f;
            DamagePopup.Show(amount, pos, critical);
        }

        /* ════ COROUTINES ─ SERVER ONLY ─═══════════════════════════════ */

        private async UniTaskVoid RegenLoopAsync()
        {
            const float tick = 1f;
            while (this && isActiveAndEnabled)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(tick));
                if (GameplayManager.Singleton.IsPaused()) continue;
                if (CurrentHp < MaxHp)
                    ApplyHeal(Stats.baseHPRegen);
            }
        }

        private IEnumerator DotRoutine(int total, float dur, System.Action onFinish)
        {
            const float tick = 0.5f;
            int ticks = Mathf.CeilToInt(dur / tick);
            int perTick = total / ticks;
            int remainder = total - perTick * (ticks - 1);

            for (int i = 0; i < ticks; i++)
            {
                if (currentHp <= 0f) break;

                int dmg = i == ticks - 1 ? perTick + remainder : perTick;
                dmg = Mathf.Min(dmg, Mathf.RoundToInt(currentHp) - 1);
                ReceiveDamage(dmg);
                yield return new WaitForSeconds(tick);
            }
            onFinish?.Invoke();
        }

        /* ════ UI HELPERS (LOCAL ONLY) ═════════════════════════════════ */

        private void UpdateMainBar(float normalized)
        {
            if (hpBar) hpBar.fillAmount = normalized;
        }

        private void SetBothBars(float normalized)
        {
            if (hpBar) hpBar.fillAmount = normalized;
            if (hpBarDecrease) hpBarDecrease.fillAmount = normalized;
        }

        private IEnumerator DecreaseBarSmooth(float target)
        {
            yield return new WaitForSeconds(decreaseDelay);
            while (hpBarDecrease.fillAmount > target)
            {
                hpBarDecrease.fillAmount -= Time.deltaTime * decreaseSpeed;
                yield return null;
            }
            hpBarDecrease.fillAmount = target;
            hpAnimCo = null;
        }

        /* ════ UTILS ══════════════════════════════════════════════════ */

        /// <summary>
        /// Stops an existing DOT coroutine (if any) and starts a new one.
        /// Helps to avoid stacking identical effects from the same source.
        /// </summary>
        private void RestartDot<TKey>(Dictionary<TKey, Coroutine> dict, TKey key, IEnumerator routine)
        {
            if (dict.TryGetValue(key, out var co) && co != null)
                StopCoroutine(co);

            dict[key] = StartCoroutine(routine);
        }
    }
}
