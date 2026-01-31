using BulletHellTemplate.Core.Events;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles temporary or passive characterBuffsComponent that modify character characterStatsComponent.
    /// </summary>
    public partial class CharacterBuffsComponent : MonoBehaviour
    {
        private CharacterStatsComponent characterStatsComponent;
        private CharacterControllerComponent characterControllerComponent;
        private CharacterEntity characterOwner;

        // Active buffs and debuffs tracking
        private readonly List<ActiveBuff> activeBuffs = new();
        private readonly List<ActiveBuff> activeDebuffs = new();

        private void Awake()
        {
            characterStatsComponent = GetComponent<CharacterStatsComponent>();
            characterControllerComponent = GetComponent<CharacterControllerComponent>();
            characterOwner = GetComponent<CharacterEntity>();
        }

        public void Initialize()
        {
           if (characterStatsComponent == null) characterStatsComponent = GetComponent<CharacterStatsComponent>();
           if (characterControllerComponent == null) characterControllerComponent = GetComponent<CharacterControllerComponent>();
           if (characterOwner == null) characterOwner = GetComponent<CharacterEntity>();
        }

        #region Move Speed

        /// <summary>
        /// Applies a temporary move speed buff (increases speed).
        /// </summary>
        public void ApplyMoveSpeedBuff(float amount, float duration)
        {
            float buffAmount = Mathf.Abs(amount);
            string id = Guid.NewGuid().ToString();

            characterStatsComponent.AlterMoveSpeed(buffAmount);
            RemoveModifierAfterDelayAsync(id, StatType.MoveSpeed, BuffCategory.Buff, buffAmount, duration, this.GetCancellationTokenOnDestroy()).Forget();

            activeBuffs.Add(new ActiveBuff(id, StatType.MoveSpeed, BuffCategory.Buff, buffAmount, duration));
            EventBus.Publish(new BuffReceivedEvent(characterOwner, StatType.MoveSpeed, buffAmount, duration, activeBuffs.Count));
        }

        /// <summary>
        /// Applies a temporary move speed debuff (decreases speed).
        /// </summary>
        public void ApplyMoveSpeedDebuff(float amount, float duration)
        {
            float debuffAmount = -Mathf.Abs(amount);
            string id = Guid.NewGuid().ToString();

            characterStatsComponent.AlterMoveSpeed(debuffAmount);
            RemoveModifierAfterDelayAsync(id, StatType.MoveSpeed, BuffCategory.Debuff, debuffAmount, duration, this.GetCancellationTokenOnDestroy()).Forget();

            activeDebuffs.Add(new ActiveBuff(id, StatType.MoveSpeed, BuffCategory.Debuff, debuffAmount, duration));
            EventBus.Publish(new DebuffReceivedEvent(characterOwner, StatType.MoveSpeed, debuffAmount, duration, activeDebuffs.Count));
        }


        #endregion

        #region Attack Speed

        /// <summary>
        /// Applies a temporary attack speed buff (increases attack speed).
        /// </summary>
        public void ApplyAttackSpeedBuff(float amount, float duration)
        {
            float buffAmount = Mathf.Abs(amount);
            string id = Guid.NewGuid().ToString();

            characterStatsComponent.AlterAttackSpeed(buffAmount);
            RemoveModifierAfterDelayAsync(id, StatType.AttackSpeed, BuffCategory.Buff, buffAmount, duration, this.GetCancellationTokenOnDestroy()).Forget();

            activeBuffs.Add(new ActiveBuff(id, StatType.AttackSpeed, BuffCategory.Buff, buffAmount, duration));
            EventBus.Publish(new BuffReceivedEvent(characterOwner, StatType.AttackSpeed, buffAmount, duration, activeBuffs.Count));
        }

        /// <summary>
        /// Applies a temporary attack speed debuff (decreases attack speed).
        /// </summary>
        public void ApplyAttackSpeedDebuff(float amount, float duration)
        {
            float debuffAmount = -Mathf.Abs(amount);
            string id = Guid.NewGuid().ToString();

            characterStatsComponent.AlterAttackSpeed(debuffAmount);
            RemoveModifierAfterDelayAsync(id, StatType.AttackSpeed, BuffCategory.Debuff, debuffAmount, duration, this.GetCancellationTokenOnDestroy()).Forget();

            activeDebuffs.Add(new ActiveBuff(id, StatType.AttackSpeed, BuffCategory.Debuff, debuffAmount, duration));
            EventBus.Publish(new DebuffReceivedEvent(characterOwner, StatType.AttackSpeed, debuffAmount, duration, activeDebuffs.Count));
        }

        #endregion

        #region Defense

        /// <summary>
        /// Applies a temporary defense buff (increases defense).
        /// </summary>
        public void ApplyDefenseBuff(float amount, float duration)
        {
            float buffAmount = Mathf.Abs(amount);
            string id = Guid.NewGuid().ToString();

            characterStatsComponent.AlterDefense(buffAmount);
            RemoveModifierAfterDelayAsync(id, StatType.Defense, BuffCategory.Buff, buffAmount, duration, this.GetCancellationTokenOnDestroy()).Forget();

            activeBuffs.Add(new ActiveBuff(id, StatType.Defense, BuffCategory.Buff, buffAmount, duration));
            EventBus.Publish(new BuffReceivedEvent(characterOwner, StatType.Defense, buffAmount, duration, activeBuffs.Count));
        }

        /// <summary>
        /// Applies a temporary defense debuff (decreases defense).
        /// </summary>
        public void ApplyDefenseDebuff(float amount, float duration)
        {
            float debuffAmount = -Mathf.Abs(amount);
            string id = Guid.NewGuid().ToString();

            characterStatsComponent.AlterDefense(debuffAmount);
            RemoveModifierAfterDelayAsync(id, StatType.Defense, BuffCategory.Debuff, debuffAmount, duration, this.GetCancellationTokenOnDestroy()).Forget();

            activeDebuffs.Add(new ActiveBuff(id, StatType.Defense, BuffCategory.Debuff, debuffAmount, duration));
            EventBus.Publish(new DebuffReceivedEvent(characterOwner, StatType.Defense, debuffAmount, duration, activeDebuffs.Count));
        }

        #endregion

        #region Damage

        /// <summary>
        /// Applies a temporary damage buff (increases damage).
        /// </summary>
        public void ApplyDamageBuff(float amount, float duration)
        {
            float buffAmount = Mathf.Abs(amount);
            string id = Guid.NewGuid().ToString();

            characterStatsComponent.AlterDamage(buffAmount);
            RemoveModifierAfterDelayAsync(id, StatType.Damage, BuffCategory.Buff, buffAmount, duration, this.GetCancellationTokenOnDestroy()).Forget();

            activeBuffs.Add(new ActiveBuff(id, StatType.Damage, BuffCategory.Buff, buffAmount, duration));
            EventBus.Publish(new BuffReceivedEvent(characterOwner, StatType.Damage, buffAmount, duration, activeBuffs.Count));
        }

        /// <summary>
        /// Applies a temporary damage debuff (decreases damage).
        /// </summary>
        public void ApplyDamageDebuff(float amount, float duration)
        {
            float debuffAmount = -Mathf.Abs(amount);
            string id = Guid.NewGuid().ToString();

            characterStatsComponent.AlterDamage(debuffAmount);
            RemoveModifierAfterDelayAsync(id, StatType.Damage, BuffCategory.Debuff, debuffAmount, duration, this.GetCancellationTokenOnDestroy()).Forget();

            activeDebuffs.Add(new ActiveBuff(id, StatType.Damage, BuffCategory.Debuff, debuffAmount, duration));
            EventBus.Publish(new DebuffReceivedEvent(characterOwner, StatType.Damage, debuffAmount, duration, activeDebuffs.Count));
        }

        #endregion

        #region Shield

        /// <summary>
        /// Applies shield to the character. If temporary, acts as a buff and reverts after duration.
        /// </summary>
        public void ApplyShield(int amount, float duration = 0f, bool isTemporary = false)
        {
            if (amount <= 0 || characterStatsComponent == null)
                return;

            characterStatsComponent.AddShield(amount);

            if (isTemporary && duration > 0f)
            {
                string id = Guid.NewGuid().ToString();
                StatType statType = StatType.Shield;
                BuffCategory buffCategory = BuffCategory.Buff;

                RemoveShieldAfterDelayAsync(id, statType, buffCategory, amount, duration, this.GetCancellationTokenOnDestroy()).Forget();
                activeBuffs.Add(new ActiveBuff(id, statType, buffCategory, amount, duration));

                int remainingBuffsCount = activeBuffs.Count;
                EventBus.Publish(new BuffReceivedEvent(characterOwner, statType, amount, duration, remainingBuffsCount));
            }
        }

        /// <summary>
        /// Async task that waits for the specified duration and then removes the shield buff.
        /// </summary>
        private async UniTask RemoveShieldAfterDelayAsync(
            string id,
            StatType statType,
            BuffCategory buffCategory,
            int amount,
            float duration,
            CancellationToken token = default)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();

                if (GameplayManager.Singleton.IsPaused())
                {
                    await UniTask.WaitWhile(() => GameplayManager.Singleton.IsPaused(), cancellationToken: token);
                }

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            characterStatsComponent.RemoveShield(amount);
            activeBuffs.RemoveAll(b => b.Id == id);

            int remainingBuffsCount = activeBuffs.Count;
            EventBus.Publish(new BuffRemovedEvent(characterOwner, statType, remainingBuffsCount));
        }

        #endregion

        public void ApplyHealHp(int amount)
        {
            amount = Mathf.Abs(amount);
            characterStatsComponent.HealHP(amount);
        }

        /// <summary>
        /// Waits for a duration (while handling pause) and then removes the stat modifier.
        /// </summary>
        /// <param name="id">Unique ID of the modifier.</param>
        /// <param name="statType">Stat affected by the modifier.</param>
        /// <param name="buffCategory">Type of modifier (Buff or Debuff).</param>
        /// <param name="amount">Value applied by the modifier.</param>
        /// <param name="duration">Duration in seconds before removal.</param>
        /// <param name="token">Optional cancellation token.</param>
        private async UniTask RemoveModifierAfterDelayAsync(
     string id, StatType statType, BuffCategory buffCategory, float amount, float duration, CancellationToken token = default)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();
                if (GameplayManager.Singleton.IsPaused())
                    await UniTask.WaitWhile(() => GameplayManager.Singleton.IsPaused(), cancellationToken: token);

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            RemoveModifier(statType, amount);

            if (buffCategory == BuffCategory.Buff)
            {
                activeBuffs.RemoveAll(b => b.Id == id);
                int remaining = activeBuffs.Count;
                EventBus.Publish(new BuffRemovedEvent(characterOwner, statType, remaining)); 
            }
            else
            {
                activeDebuffs.RemoveAll(b => b.Id == id);
                int remaining = activeDebuffs.Count;
                EventBus.Publish(new DebuffRemovedEvent(characterOwner, statType, remaining));
            }
        }

        public void RemoveModifier(StatType statType, float amount)
        {
            switch (statType)
            {
                case StatType.MoveSpeed:
                    characterStatsComponent.AlterMoveSpeed(-amount);
                    break;
                case StatType.AttackSpeed:
                    characterStatsComponent.AlterAttackSpeed(-amount);
                    break;
                case StatType.Damage:
                    characterStatsComponent.AlterDamage(-amount);
                    break;
                case StatType.Defense:
                    characterStatsComponent.AlterDefense(-amount);
                    break;
            }
        }
    }
}
