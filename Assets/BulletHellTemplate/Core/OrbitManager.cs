using BulletHellTemplate.VFX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages the orbit of multiple DamageEntity objects around a character.
    /// </summary>
    public class OrbitManager : MonoBehaviour
    {
        private Transform target; // The character to orbit
        private SkillData skillData;
        private List<DamageEntity> orbitingOrbs = new List<DamageEntity>(); // List of orbs
        private List<float> orbAngles = new List<float>(); // Stores the angle of each orb
        private float orbitDistance;
        private float orbitSpeed;
        public int orbCount;
        private float lifeTime;
        private bool isPaused = false;

        /// <summary>
        /// Creates N orbiting DamageEntity instances around the attacker
        /// using the object-pool and IDamageProvider.
        /// </summary>
        public void InitializeOrbital(
            SkillData skillData,
            SkillLevel levelData,
            CharacterEntity attacker,
            int orbCount,
            DamageEntity damagePrefab,
            Vector3 velocity,
            bool isReplica)
        {
            this.skillData = skillData;
            this.target = attacker.transform;
            this.orbCount = orbCount;
            this.orbitDistance = skillData.orbitalDistance;
            this.orbitSpeed = levelData.speed * 10f;
            this.lifeTime = levelData.lifeTime;

            if (damagePrefab == null) { Debug.LogWarning("Orbit passEntryPrefab missing."); return; }

            float angleStep = 360f / orbCount;
            var provider = new SkillDamageProvider(skillData, levelData,isReplica);   // adapter

            for (int i = 0; i < orbCount; ++i)
            {
                float angleDeg = i * angleStep;
                Quaternion q = Quaternion.Euler(0f, angleDeg, 0f);
                Vector3 dir = q * Vector3.forward;

                GameObject orbGO = GameEffectsManager.SpawnEffect(
                    damagePrefab.gameObject,
                    target.position + dir * orbitDistance,
                    Quaternion.identity);

                var orb = orbGO.GetComponent<DamageEntity>();
                orb.Init(provider, attacker, velocity, isReplica);

                orbitingOrbs.Add(orb);
                orbAngles.Add(angleDeg);

            }
        }


        /// <summary>
        /// Pauses or resumes the orbs when the game is paused or unpaused.
        /// </summary>
        private void HandlePauseState()
        {
            if (GameplayManager.Singleton.IsPaused())
            {
                isPaused = true;
            }
            else
            {
                isPaused = false;
            }
        }

        private void Update()
        {
            HandlePauseState(); // Check and handle pause state

            if (target == null || isPaused) return;

            for (int i = 0; i < orbitingOrbs.Count; i++)
            {
                if (orbitingOrbs[i] != null && orbitingOrbs[i].gameObject != null)
                {
                    OrbitAroundTarget(orbitingOrbs[i], orbAngles[i]);
                }
            }
        }

        /// <summary>
        /// Rotates the orbs around the target at a specified speed and keeps them synchronized.
        /// </summary>
        private void OrbitAroundTarget(DamageEntity orb, float initialAngle)
        {
            if (target == null || isPaused || orb == null || orb.gameObject == null) return;

            // Update the angle of rotation over time based on the orbit speed
            float angle = initialAngle + orbitSpeed * Time.time;

            // Calculate the new position based on the angle
            Vector3 offset = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad) * orbitDistance,
                0,
                Mathf.Cos(angle * Mathf.Deg2Rad) * orbitDistance
            );

            // Apply the position relative to the target's position
            orb.transform.position = target.position + offset;

            // Calculate the direction from the target to the orb
            Vector3 directionToOrb = offset.normalized;

            // Set the rotation of the orb to face along its movement path
            // This will make the sword appear to swing as it orbits
            Quaternion rotation = Quaternion.LookRotation(directionToOrb);
            orb.transform.rotation = rotation;

            // Optional: Add spin around the forward axis to simulate swinging
            // float spinSpeed = 360f; // degrees per second
            // orb.transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);
        }

    }
}
