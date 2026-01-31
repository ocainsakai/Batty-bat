#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BulletHellTemplate.EditorTools
{
    [CustomEditor(typeof(MonsterSkill), true)]
    public class MonsterSkillEditor : Editor
    {
        private SerializedProperty prop = null;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSection("Basic Info", "skillName", "icon", "damageType", "cooldown");
            DrawSection("Launch Basics", "damagePrefab", "spawnOffset", "baseDamage",
                                          "launchSpeed", "lifeTime", "minDistance",
                                          "animationIndex", "delayToLaunch", "delayToMove");

            DrawSection("Effects", "spawnEffect", "spawnAudio", "playSpawnAudioEaShot", "skillAudio",
                                        "playSkillAudioEaShot", "playSkillAudioAfter", "HitEffect", "HitAudio");

            DrawSection("Multi-Shot", "isMultiShot", "shots", "angle", "delayBetweenShots");
            DrawSection("Advanced Multi-Shot", "isAdvancedMultiShot", "shotWaves");

            DrawSection("Special Modes", "isOrbital", "orbitalDistance",
                                          "isDash", "isReverseDash", "dashSpeed", "dashDuration",
                                          "isTrapSkill", "trapPrefab", "trapLifetime",
                                          "isRicochet", "isBoomerang", "maxBoomerangDistance",
                                          "isMelee", "isRanged");

            DrawSection("Status Effects", "slow", "stun", "knockback", "dot");

            DrawSection("Healing", "healSelf", "healTrap");

            // Catch-all: draw any property we forgot or new ones added via partial
            DrawRemainingProperties();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSection(string title, params string[] props)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            foreach (string p in props)
            {
                prop = serializedObject.FindProperty(p);
                if (prop != null) EditorGUILayout.PropertyField(prop, true);
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>Ensures new fields added in partial classes still show.</summary>
        private void DrawRemainingProperties()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                if (iterator.name == "m_Script") continue;         // skip
                if (AlreadyDrawn(iterator.name)) continue;
                EditorGUILayout.PropertyField(iterator, true);
                enterChildren = false;
            }
        }

        private bool AlreadyDrawn(string propName)
        {
            // quick hash – match names we drew manually
            string[] drawn =
            {
                "skillName","icon","damageType","cooldown",
                "damagePrefab","spawnOffset","baseDamage","launchSpeed","lifeTime","minDistance","animationIndex",
                "isMultiShot","shots","angle","delayBetweenShots","delayToLaunch", "delayToMove",
                "Effects", "spawnEffect", "spawnAudio", "playSpawnAudioEaShot", "skillAudio",
                "playSkillAudioEaShot", "playSkillAudioAfter", "HitEffect", "HitAudio",
                "isAdvancedMultiShot","shotWaves",
                "isOrbital","orbitalDistance",
                "isDash","isReverseDash","dashSpeed","dashDuration",
                "isTrapSkill","trapPrefab","trapLifetime",
                "isRicochet","isBoomerang","maxBoomerangDistance",
                "isMelee","isRanged",
                "slow","stun","knockback","dot",
                "healSelf","healTrap"
            };
            foreach (string s in drawn) if (s == propName) return true;
            return false;
        }
    }
}
#endif
