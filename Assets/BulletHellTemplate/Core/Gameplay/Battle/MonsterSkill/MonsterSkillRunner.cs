using BulletHellTemplate.VFX;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BulletHellTemplate
{
    [DisallowMultipleComponent]
    public sealed class MonsterSkillRunner : MonoBehaviour
    {
        private MonsterSkill[] skills;
        private MonsterEntity owner;
        private MonsterMovementComponent movement;
        private Transform shootOrigin;
        private float[] cooldown;
        private float startDelay;
        private bool delayDone;
        private bool firstShot;
        public int LastSkillIndex { get; private set; }

        private CancellationTokenSource skillCts;
        public System.Action OnUseSkill;

        private bool _isNetworked; 
        private bool _isHost;

        /* ------------- PUBLIC SETUP ------------ */
        public void Setup(MonsterEntity _owner, MonsterSkill[] _skills, MonsterMovementComponent mv, Transform origin, float initialDelay, bool isNetworked = false, bool isHost = true)      
        {
            owner = _owner;
            skills = _skills;
            movement = mv;
            shootOrigin = origin ? origin : transform;
            startDelay = Mathf.Max(0f, initialDelay);
            delayDone = startDelay <= 0f;

            _isNetworked = isNetworked;
            _isHost = isHost;

            cooldown = new float[skills.Length];
            for (int i = 0; i < skills.Length; ++i)
                cooldown[i] = skills[i] ? skills[i].cooldown : 0f;
        }

        private void Update()
        {           
            if (owner == null || GameplayManager.Singleton.IsPaused()) return;
            if (skills == null || skills.Length == 0) return;

            if (_isNetworked && !_isHost)
                return;

            if (!delayDone)
            {
                startDelay -= Time.deltaTime;
                if (startDelay > 0f) return;
                delayDone = true;
            }

            for (int i = 0; i < skills.Length; ++i)
            {
                MonsterSkill sk = skills[i];
                if (sk == null) continue;

                cooldown[i] -= Time.deltaTime;
                if (cooldown[i] > 0f) continue;

                if (!owner.TryGetTarget(out _, out Vector3 dir, sk.minDistance))
                    continue;

                LastSkillIndex = i;
                ExecuteSkill_Local(sk, dir);
                cooldown[i] = sk.cooldown;
#if FUSION2
                if (_isNetworked)
                {
                    GameplaySync.Instance?.SyncMonsterSkillCast(owner, i, dir);
                }
#endif
            }
        }

        private void ExecuteSkill_Local(MonsterSkill sk, Vector3 dir)
        {
            _ = ExecuteSkillRoutine(sk, dir, applyTargetingDelays: true);
        }

        public void ExecuteSkill_Remote(int skillIndex, Vector3 dir)
        {
            if (skillIndex < 0 || skillIndex >= skills.Length) return;
            MonsterSkill sk = skills[skillIndex];
            if (!sk) return;

            bool applyDelays = false;   
            _ = ExecuteSkillRoutine(sk, dir, applyDelays);
        }

        private async UniTaskVoid ExecuteSkillRoutine(
            MonsterSkill sk,
            Vector3 dir,
            bool applyTargetingDelays)
        {
            LastSkillIndex = Array.IndexOf(skills, sk);
            OnUseSkill?.Invoke();  

            firstShot = true;

            if (sk.spawnEffect)
                GameEffectsManager.SpawnEffect(
                    sk.spawnEffect,
                    owner.transform.position + sk.spawnOffset,
                    Quaternion.identity);

            if (sk.spawnAudio)
                AudioManager.Singleton.PlayAudio(
                    sk.spawnAudio, "vfx", owner.transform.position);

            // stun host + client (visual)
            if (sk.delayToMove > 0f)
                movement.ApplyStun(sk.delayToMove);

            if (applyTargetingDelays)
            {
                if (sk.delayToMove > 0f)
                    await UniTask.Delay(TimeSpan.FromSeconds(sk.delayToMove));

                if (sk.delayToLaunch > 0f)
                    await UniTask.Delay(TimeSpan.FromSeconds(sk.delayToLaunch));
            }

            if (sk.isDash)
            {
                movement.ApplyDash(dir * (sk.isReverseDash ? -1f : 1f),
                                   sk.dashSpeed, sk.dashDuration);
                return;
            }

            if (sk.isTrapSkill && sk.trapPrefab)
            {
                var trap = Instantiate(sk.trapPrefab,
                                       transform.position + sk.spawnOffset,
                                       Quaternion.identity);
                trap.ConfigureFromSkill(sk, owner);
                return;
            }

            int shotCount = sk.isOrbital
                ? Mathf.Max(1, sk.shots)
                : sk.isMultiShot
                    ? Mathf.Max(1, sk.shots)
                    : 1;

            float spread = sk.isOrbital ? 360f : sk.angle;

            await FireWaveAsync(sk, dir, shotCount, spread, null, false, 0f, sk.delayBetweenShots);
        }

        private async UniTask FireWaveAsync(
                                 MonsterSkill sk,
                                 Vector3 baseDir,
                                 int count,
                                 float totalAngle,
                                 List<float> customAngles,
                                 bool useInit,
                                 float initAngle,
                                 float perShotDelay)
        {
            if (count <= 0) return;
            List<float> angles = new(count);

            bool full360 = sk.isOrbital || totalAngle >= 359.5f;
            if (customAngles is { Count: > 1 })
            {
                for (int i = 0; i < count; ++i)
                    angles.Add(customAngles[i % customAngles.Count]);
            }
            else if (full360)
            {
                float step = 360f / count;
                for (int i = 0; i < count; ++i)
                    angles.Add(i * step);              
            }
            else
            {
                float step = count > 1 ? totalAngle / (count - 1) : 0f;
                float start = -totalAngle * .5f;
                for (int i = 0; i < count; ++i)
                    angles.Add(useInit && i == 0 ? initAngle : start + step * i);
            }

            for (int i = 0; i < angles.Count; ++i)
            {
                Vector3 dir = sk.isOrbital
                    ? Quaternion.Euler(0f, angles[i], 0f) * Vector3.forward  
                    : Quaternion.AngleAxis(angles[i], Vector3.up) * baseDir;

                SpawnShot(sk, dir);

                if (perShotDelay > 0f && i < angles.Count - 1)
                    await UniTask.Delay(TimeSpan.FromSeconds(perShotDelay));
            }
        }


        private void SpawnShot(MonsterSkill sk, Vector3 dir)
        {
            Vector3 pos = shootOrigin.position + sk.spawnOffset;
            if (sk.isOrbital)
                pos += dir.normalized * sk.orbitalDistance;

            GameObject go = GameEffectsManager.SpawnEffect(
                                sk.damagePrefab.gameObject,
                                pos,
                                Quaternion.LookRotation(sk.isOrbital ? -dir : dir));

            var proj = go.GetComponent<MonsterDamageEntity>();
            proj.ConfigureFromSkill(sk, owner, dir.normalized * sk.launchSpeed);

            if (sk.isOrbital)         
            {
                float ang = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                proj.SetOrbitInitialAngle(ang);
            }

            /* Audio per shot -------------------------------------------- */
            if (firstShot)
            {
                firstShot = false;

                if (sk.skillAudio != null && !sk.playSkillAudioEaShot && sk.playSkillAudioAfter <= 0f)
                    AudioManager.Singleton.PlayAudio(sk.skillAudio, "vfx",
                                                     pos);
                else if (sk.skillAudio != null && !sk.playSkillAudioEaShot && sk.playSkillAudioAfter > 0f)
                    DelayedAudio(sk.skillAudio, sk.playSkillAudioAfter, pos).Forget();
            }

            if (sk.playSpawnAudioEaShot && sk.spawnAudio != null)
                AudioManager.Singleton.PlayAudio(sk.spawnAudio, "vfx", pos);

            if (sk.playSkillAudioEaShot && sk.skillAudio != null)
                AudioManager.Singleton.PlayAudio(sk.skillAudio, "vfx", pos);
        }

        private async UniTaskVoid DelayedAudio(AudioClip clip, float delay, Vector3 pos)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            if (clip != null)
                AudioManager.Singleton.PlayAudio(clip, "vfx", pos);
        }

    }
}
