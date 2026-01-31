using UnityEngine;
using BulletHellTemplate.Core.Events;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles character UI display logic (HP, MP, Buffs, Invincibility icons).
    /// Listens to events via EventBus to remain decoupled from gameplay logic.
    /// </summary>
    [DisallowMultipleComponent]
    public partial class CharacterUIHandlerComponent : MonoBehaviour
    {
        private CharacterEntity _entity;
        private CancellationTokenSource _hpCTS;
        private CancellationTokenSource _mpCTS;

        #region Public ---------------------------------------------------------------------------
        /// <summary>
        /// Binds this UI handler to a character instance.
        /// </summary>
        public void Initialize(CharacterEntity entity)
        {
            _entity = entity;
            // Subscribe to all relevant events (they are fired independently).
            EventBus.Subscribe<PlayerHealthChangedEvent>(OnHp);
            EventBus.Subscribe<PlayerEnergyChangedEvent>(OnMp);
            EventBus.Subscribe<PlayerShieldChangedEvent>(OnShield);
            EventBus.Subscribe<PlayerEXPChangeEvent>(OnLevel);
            EventBus.Subscribe<PlayerInvincibleEvent>(OnInvincible);
            EventBus.Subscribe<PlayerDiedEvent>(OnDeath);
            EventBus.Subscribe<PlayerRevivedEvent>(OnRevive);
            EventBus.Subscribe<BuffReceivedEvent>(OnBuffReceived);
            EventBus.Subscribe<BuffRemovedEvent>(OnBuffRemoved);
            EventBus.Subscribe<DebuffReceivedEvent>(OnDebuffReceived);
            EventBus.Subscribe<DebuffRemovedEvent>(OnDebuffRemoved);

            // Prime UI
            var stats = entity.GetComponent<CharacterStatsComponent>();
            if (stats != null)
            {
                SetHpImmediate(stats.CurrentHP, stats.MaxHp);
                SetMpImmediate(stats.CurrentMP, stats.MaxMp);
                SetShieldImmediate(stats.CurrentShield, stats.MaxHp);
            }
        }

        private void Awake()
        {
            _entity = GetComponent<CharacterEntity>();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHp);
            EventBus.Unsubscribe<PlayerEnergyChangedEvent>(OnMp);
            EventBus.Unsubscribe<PlayerShieldChangedEvent>(OnShield);
            EventBus.Unsubscribe<PlayerEXPChangeEvent>(OnLevel);
            EventBus.Unsubscribe<PlayerInvincibleEvent>(OnInvincible);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnDeath);
            EventBus.Unsubscribe<PlayerRevivedEvent>(OnRevive);
            EventBus.Unsubscribe<BuffReceivedEvent>(OnBuffReceived);
            EventBus.Unsubscribe<BuffRemovedEvent>(OnBuffRemoved);
            EventBus.Unsubscribe<DebuffReceivedEvent>(OnDebuffReceived);
            EventBus.Unsubscribe<DebuffRemovedEvent>(OnDebuffRemoved);

            _hpCTS?.Cancel(); _mpCTS?.Cancel();
        }
        #endregion

        #region Event Callbacks ------------------------------------------------------------------
        private void OnHp(PlayerHealthChangedEvent e)
        {
            if (e.Target != _entity) return;

            _hpCTS?.Cancel();
            _hpCTS = new();
            SetHpImmediate(e.CurrentHP, e.MaxHP);
            ShrinkDelayedBarAsync(e.CurrentHP / e.MaxHP, _hpCTS.Token).Forget();
        }

        private void OnMp(PlayerEnergyChangedEvent e)
        {
            if (e.Target != _entity) return;

            _mpCTS?.Cancel();
            _mpCTS = new();
            LerpMpAsync(e.CurrentMP / e.MaxMP, _mpCTS.Token).Forget();
        }

        private void OnShield(PlayerShieldChangedEvent e)
        {
            if (e.Target != _entity) return;

            SetShieldImmediate(e.CurrentShield, e.MaxHP);
            _entity.shieldIcon?.gameObject.SetActive(e.CurrentShield > 0f);
        }

        private void OnLevel(PlayerEXPChangeEvent e)
        {
            if (e.Target != _entity) return;
            if (_entity.level) _entity.level.text = e.NewLevel.ToString();
        }

        private void OnInvincible(PlayerInvincibleEvent e)
        {
            if (e.character != _entity) return;

            if (_entity.invincibleIcon == null) return;
            _entity.invincibleIcon.gameObject.SetActive(true);
            CancelInvoke(nameof(HideInv));
            Invoke(nameof(HideInv), e.duration);
        }

        private void OnDeath(PlayerDiedEvent e)
        {
            if (e.Target != _entity) return;

            _hpCTS?.Cancel(); _mpCTS?.Cancel();

            SetHpImmediate(0, Mathf.Max(1f, _entity.GetMaxHP()));
            SetMpImmediate(0, Mathf.Max(1f, _entity.GetMaxMP()));
            SetShieldImmediate(0, Mathf.Max(1f, _entity.GetMaxHP()));

            _entity.invincibleIcon?.gameObject.SetActive(false);
            _entity.buffIcon?.gameObject.SetActive(false);
            _entity.debuffIcon?.gameObject.SetActive(false);

            if (GameplayManager.Singleton.ActiveCharactersList.Count <= 0)
            {
                GameplayManager.Singleton.PauseGame();
                UIGameplay.Singleton.DisplayEndGameScreen(false);
            }
        }

        private void OnRevive(PlayerRevivedEvent e)
        {
            if (e.character != _entity) return;

            _hpCTS?.Cancel(); _mpCTS?.Cancel();
            SetHpImmediate(_entity.GetCurrentHP(), _entity.GetMaxHP());
            SetMpImmediate(_entity.GetCurrentMP(), _entity.GetMaxMP());
            SetShieldImmediate(_entity.GetCurrentShield(), _entity.GetMaxHP());
            _entity.invincibleIcon?.gameObject.SetActive(false);
            _entity.buffIcon?.gameObject.SetActive(false);
            _entity.debuffIcon?.gameObject.SetActive(false);
        }

        private void OnBuffReceived(BuffReceivedEvent e) { if (e.character == _entity && e.activeBuffsAmount > 0) _entity.buffIcon?.gameObject.SetActive(true); }
        private void OnBuffRemoved(BuffRemovedEvent e) { if (e.Target == _entity && e.RemainingBuffsCount == 0) _entity.buffIcon?.gameObject.SetActive(false); }
        private void OnDebuffReceived(DebuffReceivedEvent e) { if (e.character == _entity && e.activeDebuffsAmount > 0) _entity.debuffIcon?.gameObject.SetActive(true); }
        private void OnDebuffRemoved(DebuffRemovedEvent e) { if (e.Target == _entity && e.RemainingDebuffsCount == 0) _entity.debuffIcon?.gameObject.SetActive(false); }
        #endregion

        #region UI Helpers -----------------------------------------------------------------------
        public void SetHpImmediate(float cur, float max)
        {
            _entity.hpBar.fillAmount = cur / max;
            _entity.hpDecreaseBar.fillAmount = cur / max;
            if (_entity.HpText) _entity.HpText.text = Mathf.CeilToInt(cur).ToString();
        }
        public void SetMpImmediate(float cur, float max) =>
            _entity.mpBar.fillAmount = max > 0 ? cur / max : 0f;

        public void SetShieldImmediate(float shield, float max) =>
            _entity.shieldBar.fillAmount = max > 0 ? shield / max : 0f;

        private async UniTaskVoid ShrinkDelayedBarAsync(float target, CancellationToken ct)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_entity.decreaseHpBarDelay), cancellationToken: ct);

                while (_entity.hpDecreaseBar.fillAmount - target > 0.001f)
                {
                    _entity.hpDecreaseBar.fillAmount -= Time.deltaTime * _entity.decreaseHPBarSpeed;
                    await UniTask.Yield(ct);
                }
                _entity.hpDecreaseBar.fillAmount = target;
            }
            catch (OperationCanceledException) { }
        }

        private async UniTaskVoid LerpMpAsync(float target, CancellationToken ct)
        {
            try
            {
                while (Mathf.Abs(_entity.mpBar.fillAmount - target) > 0.001f)
                {
                    _entity.mpBar.fillAmount =
                        Mathf.MoveTowards(_entity.mpBar.fillAmount, target, Time.deltaTime * _entity.mpLerpSpeed);
                    await UniTask.Yield(ct);
                }
                _entity.mpBar.fillAmount = target;
            }
            catch (OperationCanceledException) { }
        }

        private void HideInv() => _entity.invincibleIcon?.gameObject.SetActive(false);
        #endregion

    }
}
