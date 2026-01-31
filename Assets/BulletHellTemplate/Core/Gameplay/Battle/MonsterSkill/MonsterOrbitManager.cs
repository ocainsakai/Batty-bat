using BulletHellTemplate;
using BulletHellTemplate.VFX;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    public sealed class MonsterOrbitManager : MonoBehaviour
    {
        private readonly List<MonsterDamageEntity> list = new();
        private MonsterEntity owner;

        public void Init(MonsterSkill sk, MonsterEntity o, Transform origin)
        {
            owner = o;
            int shots = Mathf.Max(1, sk.shots);
            float step = 360f / shots;

            for (int i = 0; i < shots; ++i)
            {
                float ang = i * step * Mathf.Deg2Rad;
                Vector3 dir = new(Mathf.Sin(ang), 0, Mathf.Cos(ang));
                GameObject go = GameEffectsManager.SpawnEffect(
                        sk.damagePrefab.gameObject,
                        origin.position + dir * sk.orbitalDistance,
                        Quaternion.LookRotation(-dir));

                var proj = go.GetComponent<MonsterDamageEntity>();
                proj.ConfigureFromSkill(sk, owner, Vector3.zero);
                list.Add(proj);
            }
        }

        private void Update()
        {
            if (!owner) { Destroy(gameObject); return; }
            list.RemoveAll(p => !p);
            if (list.Count == 0) Destroy(gameObject);
        }
    }
}