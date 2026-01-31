#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BulletHellTemplate
{
    [CustomEditor(typeof(SkillPerkData))]
    public class SkillPerkDataEditor : Editor
    {
        // Declare all SerializedProperties
        private SerializedProperty icon;
        private SerializedProperty maxLevelIcon;
        private SerializedProperty maxLevelFrame;
        private SerializedProperty maxLevel;
        private SerializedProperty description;
        private SerializedProperty damageType;
        private SerializedProperty initialDamagePrefab;
        private SerializedProperty maxLvDamagePrefab;
        private SerializedProperty spawnEffect;
        private SerializedProperty spawnAudio;
        private SerializedProperty effectDuration;
        private SerializedProperty speed;
        private SerializedProperty lifeTime;
        private SerializedProperty withoutCooldown;
        private SerializedProperty cooldown;
        private SerializedProperty isHpLeech;
        private SerializedProperty isMultiShot;
        private SerializedProperty shots;
        private SerializedProperty angle;
        private SerializedProperty delay;
        private SerializedProperty evolveChanges;
        private SerializedProperty shotsEvolved;
        private SerializedProperty angleEvolved;
        private SerializedProperty delayEvolved;
        private SerializedProperty isOrbital;
        private SerializedProperty orbitalDistance;
        private SerializedProperty isBoomerangSkill;
        private SerializedProperty maxDistance;
        private SerializedProperty isRicochet;
        private SerializedProperty isDash;
        private SerializedProperty dashSpeed;
        private SerializedProperty dashDuration;
        private SerializedProperty isShield;
        private SerializedProperty shieldAmount;
        private SerializedProperty shieldDuration;
        private SerializedProperty isMelee;
        private SerializedProperty applySlow;
        private SerializedProperty slowPercent;
        private SerializedProperty slowDuration;
        private SerializedProperty applyKnockback;
        private SerializedProperty knockbackDistance;
        private SerializedProperty knockbackDuration;
        private SerializedProperty applyStun;
        private SerializedProperty stunDuration;
        private SerializedProperty applyDOT;
        private SerializedProperty dotAmount;
        private SerializedProperty dotDuration;
        private SerializedProperty hasEvolution;
        private SerializedProperty perkRequireToEvolveSkill;
        private SerializedProperty damagePerLevel;
        private SerializedProperty attackerDamageRate;

        private void OnEnable()
        {
            // Load all properties
            icon = serializedObject.FindProperty("icon");
            maxLevelIcon = serializedObject.FindProperty("maxLevelIcon");
            maxLevelFrame = serializedObject.FindProperty("maxLevelFrame");
            maxLevel = serializedObject.FindProperty("maxLevel");
            description = serializedObject.FindProperty("description");
            damageType = serializedObject.FindProperty("damageType");
            initialDamagePrefab = serializedObject.FindProperty("initialDamagePrefab");
            maxLvDamagePrefab = serializedObject.FindProperty("maxLvDamagePrefab");
            spawnEffect = serializedObject.FindProperty("spawnEffect");
            spawnAudio = serializedObject.FindProperty("spawnAudio");
            effectDuration = serializedObject.FindProperty("effectDuration");
            speed = serializedObject.FindProperty("speed");
            lifeTime = serializedObject.FindProperty("lifeTime");
            withoutCooldown = serializedObject.FindProperty("withoutCooldown");
            cooldown = serializedObject.FindProperty("cooldown");
            isHpLeech = serializedObject.FindProperty("isHpLeech");
            isMultiShot = serializedObject.FindProperty("isMultiShot");
            shots = serializedObject.FindProperty("shots");
            angle = serializedObject.FindProperty("angle");
            delay = serializedObject.FindProperty("delay");
            evolveChanges = serializedObject.FindProperty("evolveChanges");
            shotsEvolved = serializedObject.FindProperty("shotsEvolved");
            angleEvolved = serializedObject.FindProperty("angleEvolved");
            delayEvolved = serializedObject.FindProperty("delayEvolved");
            isOrbital = serializedObject.FindProperty("isOrbital");
            orbitalDistance = serializedObject.FindProperty("orbitalDistance");
            isBoomerangSkill = serializedObject.FindProperty("isBoomerangSkill");
            maxDistance = serializedObject.FindProperty("maxDistance");
            isRicochet = serializedObject.FindProperty("isRicochet");
            isDash = serializedObject.FindProperty("isDash");
            dashSpeed = serializedObject.FindProperty("dashSpeed");
            dashDuration = serializedObject.FindProperty("dashDuration");
            isShield = serializedObject.FindProperty("isShield");
            shieldAmount = serializedObject.FindProperty("shieldAmount");
            shieldDuration = serializedObject.FindProperty("shieldDuration");
            isMelee = serializedObject.FindProperty("isMelee");
            applySlow = serializedObject.FindProperty("applySlow");
            slowPercent = serializedObject.FindProperty("slowPercent");
            slowDuration = serializedObject.FindProperty("slowDuration");
            applyKnockback = serializedObject.FindProperty("applyKnockback");
            knockbackDistance = serializedObject.FindProperty("knockbackDistance");
            knockbackDuration = serializedObject.FindProperty("knockbackDuration");
            applyStun = serializedObject.FindProperty("applyStun");
            stunDuration = serializedObject.FindProperty("stunDuration");
            applyDOT = serializedObject.FindProperty("applyDOT");
            dotAmount = serializedObject.FindProperty("dotAmount");
            dotDuration = serializedObject.FindProperty("dotDuration");
            hasEvolution = serializedObject.FindProperty("hasEvolution");
            perkRequireToEvolveSkill = serializedObject.FindProperty("perkRequireToEvolveSkill");
            damagePerLevel = serializedObject.FindProperty("damagePerLevel");
            attackerDamageRate = serializedObject.FindProperty("attackerDamageRate");
        }

        public override void OnInspectorGUI()
        {
            // Update the serialized object
            serializedObject.Update();

            // Title
            GUILayout.Label("Skill Perk Data Editor", EditorStyles.boldLabel);

            // Skill Perk Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Skill Perk Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(icon);
            EditorGUILayout.PropertyField(maxLevelIcon);
            EditorGUILayout.PropertyField(maxLevelFrame);
            EditorGUILayout.PropertyField(maxLevel);
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.PropertyField(damageType);
            if (damageType.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("The Damage Type is required and cannot be empty.", MessageType.Warning);
            }
            EditorGUILayout.EndVertical();

            // Damage Entity Prefab Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Damage Entity Prefab Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(initialDamagePrefab);
            if (initialDamagePrefab.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("The Initial Damage Prefab is required and cannot be empty.", MessageType.Warning);
            }
            EditorGUILayout.PropertyField(maxLvDamagePrefab);
            EditorGUILayout.PropertyField(speed);
            EditorGUILayout.PropertyField(lifeTime);
            EditorGUILayout.EndVertical();

            // Effects on Spawn Skill
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Effects on Spawn Skill", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(spawnEffect);
            EditorGUILayout.PropertyField(effectDuration);
            EditorGUILayout.PropertyField(spawnAudio);
            EditorGUILayout.EndVertical();

            // Cooldown and Lifesteal Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Cooldown and Lifesteal Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(withoutCooldown);
            if (!withoutCooldown.boolValue)
            {
                EditorGUILayout.PropertyField(cooldown);
            }
            EditorGUILayout.PropertyField(isHpLeech);
            EditorGUILayout.LabelField("If activated, this skill perk will leech HP from enemies.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            // Multi-Shot Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Multi-Shot Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(isMultiShot);
            if (isMultiShot.boolValue)
            {
                EditorGUILayout.PropertyField(shots);
                EditorGUILayout.PropertyField(angle);
                EditorGUILayout.PropertyField(delay);
            }
            EditorGUILayout.EndVertical();

            // Orbital Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Orbital Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(isOrbital);
            if (isOrbital.boolValue)
            {
                EditorGUILayout.PropertyField(orbitalDistance);
            }
            EditorGUILayout.LabelField("If 'Orbital' is active, 'Multi-Shot' and other options may not have an effect.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            // Boomerang Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Boomerang Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(isBoomerangSkill);
            if (isBoomerangSkill.boolValue)
            {
                EditorGUILayout.PropertyField(maxDistance);
            }
            EditorGUILayout.EndVertical();

            // Ricochet Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Ricochet Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(isRicochet);
            EditorGUILayout.EndVertical();

            // Dash Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Dash Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(isDash);
            if (isDash.boolValue)
            {
                EditorGUILayout.PropertyField(dashSpeed);
                EditorGUILayout.PropertyField(dashDuration);
            }
            EditorGUILayout.EndVertical();

            // Shield Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Shield Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(isShield);
            if (isShield.boolValue)
            {
                EditorGUILayout.PropertyField(shieldAmount);
                EditorGUILayout.PropertyField(shieldDuration);
            }
            EditorGUILayout.EndVertical();

            // Melee Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Melee Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(isMelee);
            EditorGUILayout.LabelField("For melee skills, set 'Speed' to 0 in the damage settings for best results.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            // Slow Effect Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Slow Effect Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(applySlow);
            if (applySlow.boolValue)
            {
                EditorGUILayout.PropertyField(slowPercent);
                EditorGUILayout.PropertyField(slowDuration);
            }
            EditorGUILayout.EndVertical();

            // Knockback Effect Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Knockback Effect Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(applyKnockback);
            if (applyKnockback.boolValue)
            {
                EditorGUILayout.PropertyField(knockbackDistance);
                EditorGUILayout.PropertyField(knockbackDuration);
            }
            EditorGUILayout.EndVertical();

            // Stun Effect Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Stun Effect Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(applyStun);
            if (applyStun.boolValue)
            {
                EditorGUILayout.PropertyField(stunDuration);
            }
            EditorGUILayout.EndVertical();

            // DOT Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Damage Over Time (DOT) Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(applyDOT);
            if (applyDOT.boolValue)
            {
                EditorGUILayout.PropertyField(dotAmount);
                EditorGUILayout.PropertyField(dotDuration);
            }
            EditorGUILayout.EndVertical();

            // Evolution Settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Evolution Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(hasEvolution);
            if (hasEvolution.boolValue)
            {
                EditorGUILayout.PropertyField(perkRequireToEvolveSkill);
            }
            EditorGUILayout.EndVertical();

            // Damage per Level
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Damage Per Level", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(damagePerLevel, true);
            EditorGUILayout.PropertyField(attackerDamageRate, true);
            EditorGUILayout.EndVertical();

            // Encourage combining options
            EditorGUILayout.HelpBox("Combine various options to create unique and unexpected skill perks!", MessageType.Info);

            // Apply any changes to the serialized object
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
