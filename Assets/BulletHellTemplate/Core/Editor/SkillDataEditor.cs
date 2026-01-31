// SkillDataEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="SkillData"/>.
    /// Keeps the original logic/flow, but adds styling boxes and an \"Other Settings\" area
    /// that automatically renders any newly added serialized fields.
    /// </summary>
    [CustomEditor(typeof(SkillData))]
    public class SkillDataEditor : Editor
    {
        // ─────────────────────────────────────────────────────────────────────────────
        // Serialized Properties (exactly as in your original inspector)
        // ─────────────────────────────────────────────────────────────────────────────

        // Skill Details
        private SerializedProperty icon;
        private SerializedProperty iconEvolved;
        private SerializedProperty frameEvolved;
        private SerializedProperty skillName;
        private SerializedProperty skillNameTranslated;
        private SerializedProperty skillDescription;
        private SerializedProperty skillDescriptionTranslated;
        private SerializedProperty damageType;
        private SerializedProperty cooldown;
        private SerializedProperty delayToLaunch;
        private SerializedProperty canRotateWhileStopped;
        private SerializedProperty delayToMove;
        private SerializedProperty autoAttackDelay;
        private SerializedProperty manaCost;
        private SerializedProperty AimMode;
        private SerializedProperty damageEntityPrefab;

        // Effects
        private SerializedProperty spawnEffect;
        private SerializedProperty spawnAudio;
        private SerializedProperty skillAudio;
        private SerializedProperty playSkillAudioEaShot;
        private SerializedProperty playSkillAudioAfter;
        private SerializedProperty playSpawnAudioEaShot;
        private SerializedProperty hitEffect;
        private SerializedProperty HitAudio;

        // New Skill Settings
        private SerializedProperty rangeIndicatorType;
        private SerializedProperty radialIndicatorSettings;
        private SerializedProperty radialAoEIndicatorSettings;
        private SerializedProperty arrowIndicatorSettings;
        private SerializedProperty coneIndicatorSettings;
        private SerializedProperty launchType;
        private SerializedProperty airStrikeHeight;

        // Skill Settings (existing)
        private SerializedProperty isRotateToEnemy;
        private SerializedProperty rotateDuration;
        private SerializedProperty destroyOnFirstHit;
        private SerializedProperty sizeChangeConfig;
        private SerializedProperty applyLifeSteal;
        private SerializedProperty explodeOnDestroy;
        private SerializedProperty explodeDamageEntity;
        private SerializedProperty explodeEntitySettings;

        // Multi-Shot
        private SerializedProperty isMultiShot;
        private SerializedProperty shots;
        private SerializedProperty angle;
        private SerializedProperty delay;

        // Advanced Multi-Shot
        private SerializedProperty isAdvancedMultiShot;
        private SerializedProperty playAnimationEaShot;
        private SerializedProperty shotWaves;
        private SerializedProperty shotWavesEvolved;

        // Orbital
        private SerializedProperty isOrbital;
        private SerializedProperty orbitalDistance;

        // Ricochet
        private SerializedProperty isRicochet;

        // Boomerang
        private SerializedProperty isBoomerangSkill;
        private SerializedProperty maxDistance;

        // Dash
        private SerializedProperty isDash;
        private SerializedProperty isReverseDash;
        private SerializedProperty dashSpeed;
        private SerializedProperty dashDuration;

        // Advanced Dash
        private SerializedProperty advancedDashSettings;
        private SerializedProperty ads_enableAdvancedDash;
        private SerializedProperty ads_dashWaves;
        private SerializedProperty ads_animationTriggerEachDash;
        private SerializedProperty ads_delayBetweenWaves;
        private SerializedProperty ads_dashMode;
        private SerializedProperty ads_dashSpeed;
        private SerializedProperty ads_dashDuration;

        // Melee
        private SerializedProperty isMelee;

        // Slow
        private SerializedProperty applySlow;
        private SerializedProperty slowPercent;
        private SerializedProperty slowDuration;

        // Knockback
        private SerializedProperty applyKnockback;
        private SerializedProperty knockbackDistance;
        private SerializedProperty knockbackDuration;

        // Stun
        private SerializedProperty applyStun;
        private SerializedProperty stunDuration;

        // DOT
        private SerializedProperty applyDOT;
        private SerializedProperty dotAmount;
        private SerializedProperty dotDuration;

        // Buffs/Debuffs
        private SerializedProperty receiveHeal;
        private SerializedProperty healAmount;
        private SerializedProperty receiveShield;
        private SerializedProperty shieldAmount;
        private SerializedProperty shieldDuration;
        private SerializedProperty receiveMoveSpeed;
        private SerializedProperty moveSpeedAmount;
        private SerializedProperty moveSpeedDuration;
        private SerializedProperty receiveAttackSpeed;
        private SerializedProperty attackSpeedAmount;
        private SerializedProperty attackSpeedDuration;
        private SerializedProperty receiveDefense;
        private SerializedProperty defenseAmount;
        private SerializedProperty defenseDuration;
        private SerializedProperty receiveDamage;
        private SerializedProperty damageAmount;
        private SerializedProperty damageDuration;
        private SerializedProperty isInvincible;
        private SerializedProperty invincibleDuration;
        private SerializedProperty receiveSlow;
        private SerializedProperty receiveSlowAmount;
        private SerializedProperty receiveSlowDuration;

        // Evolve changes
        private SerializedProperty evolveChanges;
        private SerializedProperty evolvedDamageEntityPrefab;
        private SerializedProperty shotsEvolved;
        private SerializedProperty angleEvolved;
        private SerializedProperty delayEvolved;
        private SerializedProperty skillLevels;
        private SerializedProperty requireStatForEvolve;

        // ─────────────────────────────────────────────────────────────────────────────
        // Styling & handled properties
        // ─────────────────────────────────────────────────────────────────────────────
        private HashSet<string> _handledProps;
        private readonly Color headerColor = new Color(0.12f, 0.45f, 0.80f);
        private readonly Color labelColor = new Color(0.255f, 0.412f, 0.882f);

        private void RegisterHandled(params SerializedProperty[] props)
        {
            foreach (var p in props)
            {
                if (p != null)
                    _handledProps.Add(p.name);
            }
        }

        private void OnEnable()
        {
            _handledProps = new HashSet<string> { "m_Script" };

            // Skill Details
            icon = serializedObject.FindProperty("icon");
            iconEvolved = serializedObject.FindProperty("iconEvolved");
            frameEvolved = serializedObject.FindProperty("frameEvolved");
            skillName = serializedObject.FindProperty("skillName");
            skillNameTranslated = serializedObject.FindProperty("skillNameTranslated");
            skillDescription = serializedObject.FindProperty("skillDescription");
            skillDescriptionTranslated = serializedObject.FindProperty("skillDescriptionTranslated");
            damageType = serializedObject.FindProperty("damageType");
            cooldown = serializedObject.FindProperty("cooldown");
            delayToLaunch = serializedObject.FindProperty("delayToLaunch");
            canRotateWhileStopped = serializedObject.FindProperty("canRotateWhileStopped");
            delayToMove = serializedObject.FindProperty("delayToMove");
            autoAttackDelay = serializedObject.FindProperty("autoAttackDelay");
            manaCost = serializedObject.FindProperty("manaCost");
            AimMode = serializedObject.FindProperty("AimMode");
            damageEntityPrefab = serializedObject.FindProperty("damageEntityPrefab");

            // Effects
            spawnEffect = serializedObject.FindProperty("spawnEffect");
            spawnAudio = serializedObject.FindProperty("spawnAudio");
            skillAudio = serializedObject.FindProperty("skillAudio");
            playSkillAudioAfter = serializedObject.FindProperty("playSkillAudioAfter");
            playSkillAudioEaShot = serializedObject.FindProperty("playSkillAudioEaShot");
            playSpawnAudioEaShot = serializedObject.FindProperty("playSpawnAudioEaShot");
            hitEffect = serializedObject.FindProperty("HitEffect");
            HitAudio = serializedObject.FindProperty("HitAudio");

            // New Skill Settings
            rangeIndicatorType = serializedObject.FindProperty("rangeIndicatorType");
            radialIndicatorSettings = serializedObject.FindProperty("radialIndicatorSettings");
            radialAoEIndicatorSettings = serializedObject.FindProperty("radialAoEIndicatorSettings");
            arrowIndicatorSettings = serializedObject.FindProperty("arrowIndicatorSettings");
            coneIndicatorSettings = serializedObject.FindProperty("coneIndicatorSettings");
            launchType = serializedObject.FindProperty("launchType");
            airStrikeHeight = serializedObject.FindProperty("airStrikeHeight");

            // Skill Settings (existing)
            isRotateToEnemy = serializedObject.FindProperty("isRotateToEnemy");
            rotateDuration = serializedObject.FindProperty("rotateDuration");
            sizeChangeConfig = serializedObject.FindProperty("sizeChangeConfig");
            destroyOnFirstHit = serializedObject.FindProperty("destroyOnFirstHit");
            applyLifeSteal = serializedObject.FindProperty("isHpLeech");
            explodeOnDestroy = serializedObject.FindProperty("explodeOnDestroy");
            explodeDamageEntity = serializedObject.FindProperty("explodeDamageEntity");
            explodeEntitySettings = serializedObject.FindProperty("explodeEntitySettings");

            // Multi-Shot
            isMultiShot = serializedObject.FindProperty("isMultiShot");
            shots = serializedObject.FindProperty("shots");
            angle = serializedObject.FindProperty("angle");
            delay = serializedObject.FindProperty("delay");

            // Advanced Multi-Shot
            isAdvancedMultiShot = serializedObject.FindProperty("isAdvancedMultiShot");
            playAnimationEaShot = serializedObject.FindProperty("playAnimationEaShot");
            shotWaves = serializedObject.FindProperty("shotWaves");
            shotWavesEvolved = serializedObject.FindProperty("shotWavesEvolved");

            // Orbital
            isOrbital = serializedObject.FindProperty("isOrbital");
            orbitalDistance = serializedObject.FindProperty("orbitalDistance");

            // Ricochet
            isRicochet = serializedObject.FindProperty("isRicochet");

            // Boomerang
            isBoomerangSkill = serializedObject.FindProperty("isBoomerangSkill");
            maxDistance = serializedObject.FindProperty("maxDistance");

            // Dash
            isDash = serializedObject.FindProperty("isDash");
            isReverseDash = serializedObject.FindProperty("isReverseDash");
            dashSpeed = serializedObject.FindProperty("dashSpeed");
            dashDuration = serializedObject.FindProperty("dashDuration");

            // Advanced Dash
            advancedDashSettings = serializedObject.FindProperty("advancedDashSettings");
            ads_enableAdvancedDash = advancedDashSettings.FindPropertyRelative("enableAdvancedDash");
            ads_dashWaves = advancedDashSettings.FindPropertyRelative("dashWaves");
            ads_animationTriggerEachDash = advancedDashSettings.FindPropertyRelative("AnimationTriggerEachDash");
            ads_delayBetweenWaves = advancedDashSettings.FindPropertyRelative("delayBetweenWaves");
            ads_dashMode = advancedDashSettings.FindPropertyRelative("dashMode");
            ads_dashSpeed = advancedDashSettings.FindPropertyRelative("dashSpeed");
            ads_dashDuration = advancedDashSettings.FindPropertyRelative("dashDuration");

            // Melee
            isMelee = serializedObject.FindProperty("isMelee");

            // Slow
            applySlow = serializedObject.FindProperty("applySlow");
            slowPercent = serializedObject.FindProperty("slowPercent");
            slowDuration = serializedObject.FindProperty("slowDuration");

            // Knockback
            applyKnockback = serializedObject.FindProperty("applyKnockback");
            knockbackDistance = serializedObject.FindProperty("knockbackDistance");
            knockbackDuration = serializedObject.FindProperty("knockbackDuration");

            // Stun
            applyStun = serializedObject.FindProperty("applyStun");
            stunDuration = serializedObject.FindProperty("stunDuration");

            // DOT
            applyDOT = serializedObject.FindProperty("applyDOT");
            dotAmount = serializedObject.FindProperty("dotAmount");
            dotDuration = serializedObject.FindProperty("dotDuration");

            // Buffs/Debuffs
            receiveHeal = serializedObject.FindProperty("receiveHeal");
            healAmount = serializedObject.FindProperty("healAmount");
            receiveShield = serializedObject.FindProperty("receiveShield");
            shieldAmount = serializedObject.FindProperty("shieldAmount");
            shieldDuration = serializedObject.FindProperty("shieldDuration");
            receiveMoveSpeed = serializedObject.FindProperty("receiveMoveSpeed");
            moveSpeedAmount = serializedObject.FindProperty("moveSpeedAmount");
            moveSpeedDuration = serializedObject.FindProperty("moveSpeedDuration");
            receiveAttackSpeed = serializedObject.FindProperty("receiveAttackSpeed");
            attackSpeedAmount = serializedObject.FindProperty("attackSpeedAmount");
            attackSpeedDuration = serializedObject.FindProperty("attackSpeedDuration");
            receiveDefense = serializedObject.FindProperty("receiveDefense");
            defenseAmount = serializedObject.FindProperty("defenseAmount");
            defenseDuration = serializedObject.FindProperty("defenseDuration");
            receiveDamage = serializedObject.FindProperty("receiveDamage");
            damageAmount = serializedObject.FindProperty("damageAmount");
            damageDuration = serializedObject.FindProperty("damageDuration");
            isInvincible = serializedObject.FindProperty("isInvincible");
            invincibleDuration = serializedObject.FindProperty("invincibleDuration");
            receiveSlow = serializedObject.FindProperty("receiveSlow");
            receiveSlowAmount = serializedObject.FindProperty("receiveSlowAmount");
            receiveSlowDuration = serializedObject.FindProperty("receiveSlowDuration");

            // Evolve changes
            evolveChanges = serializedObject.FindProperty("evolveChanges");
            evolvedDamageEntityPrefab = serializedObject.FindProperty("evolvedDamageEntityPrefab");
            shotsEvolved = serializedObject.FindProperty("shotsEvolved");
            angleEvolved = serializedObject.FindProperty("angleEvolved");
            delayEvolved = serializedObject.FindProperty("delayEvolved");
            skillLevels = serializedObject.FindProperty("skillLevels");
            requireStatForEvolve = serializedObject.FindProperty("requireStatForEvolve");

            // Register handled top-level properties to hide them from \"Other Settings\".
            RegisterHandled(
                icon, iconEvolved, frameEvolved, skillName, skillNameTranslated, skillDescription, skillDescriptionTranslated, damageType, cooldown, delayToLaunch,
                canRotateWhileStopped, delayToMove, autoAttackDelay, manaCost, AimMode, damageEntityPrefab,
                spawnEffect, spawnAudio, skillAudio, playSkillAudioEaShot, playSkillAudioAfter, playSpawnAudioEaShot,
                hitEffect, HitAudio, rangeIndicatorType, radialIndicatorSettings, radialAoEIndicatorSettings,
                arrowIndicatorSettings, coneIndicatorSettings, launchType, airStrikeHeight, isRotateToEnemy,
                rotateDuration, destroyOnFirstHit, sizeChangeConfig, applyLifeSteal, explodeOnDestroy,
                explodeDamageEntity, explodeEntitySettings, isMultiShot, shots, angle, delay, isAdvancedMultiShot,
                playAnimationEaShot, shotWaves, shotWavesEvolved, isOrbital, orbitalDistance, isRicochet,
                isBoomerangSkill, maxDistance, isDash, isReverseDash, dashSpeed, dashDuration, advancedDashSettings,
                isMelee, applySlow, slowPercent, slowDuration, applyKnockback, knockbackDistance, knockbackDuration,
                applyStun, stunDuration, applyDOT, dotAmount, dotDuration, receiveHeal, healAmount, receiveShield,
                shieldAmount, shieldDuration, receiveMoveSpeed, moveSpeedAmount, moveSpeedDuration, receiveAttackSpeed,
                attackSpeedAmount, attackSpeedDuration, receiveDefense, defenseAmount, defenseDuration, receiveDamage,
                damageAmount, damageDuration, isInvincible, invincibleDuration, receiveSlow, receiveSlowAmount,
                receiveSlowDuration, evolveChanges, evolvedDamageEntityPrefab, shotsEvolved, angleEvolved, delayEvolved,
                skillLevels, requireStatForEvolve
            );
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            const string logoPath = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";
            EditorUIUtils.DrawTitleHeader("Skill Data", logoPath, headerColor);

            DrawSkillDetailsBox();
            DrawEffectsBox();
            DrawSkillSettingsBox();
            DrawMultiShotBox();
            DrawAdvancedMultiShotBox();
            DrawOrbitalBox();
            DrawRicochetBox();
            DrawBoomerangBox();
            DrawDashBox();
            DrawAdvancedDashBox();
            DrawMeleeBox();
            DrawSlowBox();
            DrawKnockbackBox();
            DrawStunBox();
            DrawDotBox();
            DrawBuffsDebuffsBox();
            DrawEvolveBox();
            DrawSkillLevelsBox();
            DrawOtherSettingsBox();

            EditorGUILayout.HelpBox("Combine various options to create unique abilities!", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Draw Sections
        // ─────────────────────────────────────────────────────────────────────────────

        private void DrawSkillDetailsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Skill Details", labelColor);
            EditorGUILayout.PropertyField(icon);
            EditorGUILayout.PropertyField(iconEvolved);
            EditorGUILayout.PropertyField(frameEvolved);
            EditorGUILayout.PropertyField(skillName);
            EditorGUILayout.PropertyField(skillNameTranslated);
            EditorGUILayout.PropertyField(skillDescription);
            EditorGUILayout.PropertyField(skillDescriptionTranslated);
            EditorGUILayout.PropertyField(damageType);
            EditorGUILayout.PropertyField(cooldown);
            EditorGUILayout.PropertyField(delayToLaunch);
            EditorGUILayout.PropertyField(canRotateWhileStopped);
            EditorGUILayout.PropertyField(delayToMove);
            EditorGUILayout.PropertyField(autoAttackDelay);
            EditorGUILayout.PropertyField(manaCost);
            EditorGUILayout.PropertyField(AimMode);
            EditorGUILayout.PropertyField(damageEntityPrefab);
            if (damageEntityPrefab.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("DamageEntity Prefab is required. Please assign one.", MessageType.Warning);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawEffectsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Effects (VFX / SFX)", labelColor);
            EditorGUILayout.PropertyField(spawnEffect);
            EditorGUILayout.PropertyField(spawnAudio);
            EditorGUILayout.PropertyField(playSpawnAudioEaShot);
            EditorGUILayout.PropertyField(skillAudio);
            EditorGUILayout.PropertyField(playSkillAudioEaShot);
            EditorGUILayout.PropertyField(playSkillAudioAfter);
            EditorUIUtils.DrawSubHeader("On Hit", labelColor);
            EditorGUILayout.PropertyField(hitEffect);
            EditorGUILayout.PropertyField(HitAudio);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawSkillSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Skill Settings", labelColor);

            // Range Indicator & Launch Settings
            EditorGUILayout.PropertyField(rangeIndicatorType);
            switch ((RangeIndicatorType)rangeIndicatorType.enumValueIndex)
            {
                case RangeIndicatorType.Radial:
                    EditorGUILayout.PropertyField(radialIndicatorSettings);
                    break;
                case RangeIndicatorType.RadialAoE:
                    EditorGUILayout.PropertyField(radialAoEIndicatorSettings);
                    break;
                case RangeIndicatorType.Arrow:
                    EditorGUILayout.PropertyField(arrowIndicatorSettings);
                    break;
                case RangeIndicatorType.Cone:
                    EditorGUILayout.PropertyField(coneIndicatorSettings);
                    break;
            }

            EditorGUILayout.PropertyField(launchType);
            if ((LaunchType)launchType.enumValueIndex == LaunchType.TargetedAirStrike)
            {
                EditorGUILayout.PropertyField(airStrikeHeight);
            }

            // Existing
            EditorGUILayout.PropertyField(isRotateToEnemy);
            if (isRotateToEnemy.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(rotateDuration);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(applyLifeSteal, new GUIContent("Life Steal (isHpLeech)"));
            EditorGUILayout.PropertyField(destroyOnFirstHit);

            // Size Change
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Size Change Settings", labelColor);
            var enableSizeChange = sizeChangeConfig.FindPropertyRelative("enableSizeChange");
            EditorGUILayout.PropertyField(enableSizeChange, new GUIContent("Enable Size Change"));
            if (enableSizeChange.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorUIUtils.DrawSubHeader("X Axis", labelColor);
                EditorGUILayout.PropertyField(sizeChangeConfig.FindPropertyRelative("initialSizeX"));
                EditorGUILayout.PropertyField(sizeChangeConfig.FindPropertyRelative("finalSizeX"));
                EditorGUILayout.PropertyField(sizeChangeConfig.FindPropertyRelative("sizeChangeTimeX"));

                EditorUIUtils.DrawSubHeader("Y Axis", labelColor);
                EditorGUILayout.PropertyField(sizeChangeConfig.FindPropertyRelative("initialSizeY"));
                EditorGUILayout.PropertyField(sizeChangeConfig.FindPropertyRelative("finalSizeY"));
                EditorGUILayout.PropertyField(sizeChangeConfig.FindPropertyRelative("sizeChangeTimeY"));

                EditorUIUtils.DrawSubHeader("Z Axis", labelColor);
                EditorGUILayout.PropertyField(sizeChangeConfig.FindPropertyRelative("initialSizeZ"));
                EditorGUILayout.PropertyField(sizeChangeConfig.FindPropertyRelative("finalSizeZ"));
                EditorGUILayout.PropertyField(sizeChangeConfig.FindPropertyRelative("sizeChangeTimeZ"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            // Explosion
            EditorGUILayout.Space();
            EditorUIUtils.DrawSubHeader("Explosion On Destroy", labelColor);
            EditorGUILayout.PropertyField(explodeOnDestroy);
            if (explodeOnDestroy.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(explodeDamageEntity);
                EditorGUILayout.PropertyField(explodeEntitySettings);
                EditorGUILayout.HelpBox("Configure the explosion settings on destroy.", MessageType.Info);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawMultiShotBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Multi-Shot Settings", labelColor);
            EditorGUILayout.PropertyField(isMultiShot);
            if (isMultiShot.boolValue)
            {
                // Force Advanced off
                isAdvancedMultiShot.boolValue = false;

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(shots);
                EditorGUILayout.PropertyField(angle);
                EditorGUILayout.PropertyField(delay);
                EditorGUILayout.HelpBox("Simple Multi-Shot: divides shots in an angular spread.", MessageType.Info);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawAdvancedMultiShotBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Advanced Multi-Shot Settings", labelColor);
            EditorGUILayout.PropertyField(isAdvancedMultiShot);
            if (isAdvancedMultiShot.boolValue)
            {
                // Force Simple off
                isMultiShot.boolValue = false;

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(playAnimationEaShot);
                EditorGUILayout.PropertyField(shotWaves, true);
                EditorGUILayout.PropertyField(shotWavesEvolved, true);
                EditorGUILayout.HelpBox("Advanced Multi-Shot uses waves and custom angles for complex patterns.", MessageType.Info);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawOrbitalBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Orbital Settings", labelColor);
            EditorGUILayout.PropertyField(isOrbital);
            if (isOrbital.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(orbitalDistance);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawRicochetBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Ricochet Settings", labelColor);
            EditorGUILayout.PropertyField(isRicochet);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawBoomerangBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Boomerang Settings", labelColor);
            EditorGUILayout.PropertyField(isBoomerangSkill);
            if (isBoomerangSkill.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(maxDistance);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawDashBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Dash Settings", labelColor);
            EditorGUILayout.PropertyField(isDash);
            if (isDash.boolValue)
            {
                // Force Advanced off
                ads_enableAdvancedDash.boolValue = false;

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(isReverseDash);
                EditorGUILayout.PropertyField(dashSpeed);
                EditorGUILayout.PropertyField(dashDuration);
                EditorGUILayout.HelpBox("Simple dash forwards or reversed direction.", MessageType.Info);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawAdvancedDashBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Advanced Dash Settings", labelColor);
            EditorGUILayout.PropertyField(ads_enableAdvancedDash, new GUIContent("Enable Advanced Dash"));
            if (ads_enableAdvancedDash.boolValue)
            {
                // Force Simple off
                isDash.boolValue = false;

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(ads_animationTriggerEachDash, new GUIContent("Trigger Animation Each Dash"));
                EditorGUILayout.PropertyField(ads_dashWaves, new GUIContent("Dash Waves"));
                EditorGUILayout.PropertyField(ads_delayBetweenWaves, new GUIContent("Delay Between Waves"));
                EditorGUILayout.PropertyField(ads_dashMode, new GUIContent("Dash Mode"));
                EditorGUILayout.PropertyField(ads_dashSpeed, new GUIContent("Dash Speed"));
                EditorGUILayout.PropertyField(ads_dashDuration, new GUIContent("Dash Duration"));
                EditorGUI.indentLevel--;
                EditorGUILayout.HelpBox("Advanced Dash allows multiple consecutive dashes with custom modes.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawMeleeBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Melee Settings", labelColor);
            EditorGUILayout.PropertyField(isMelee);
            if (isMelee.boolValue)
            {
                EditorGUILayout.HelpBox("For melee, consider Speed = 0 in the SkillLevel to keep the hit close.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawSlowBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Slow Effect Settings", labelColor);
            EditorGUILayout.PropertyField(applySlow);
            if (applySlow.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(slowPercent);
                EditorGUILayout.PropertyField(slowDuration);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawKnockbackBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Knockback Effect Settings", labelColor);
            EditorGUILayout.PropertyField(applyKnockback);
            if (applyKnockback.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(knockbackDistance);
                EditorGUILayout.PropertyField(knockbackDuration);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawStunBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Stun Effect Settings", labelColor);
            EditorGUILayout.PropertyField(applyStun);
            if (applyStun.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(stunDuration);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawDotBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Damage Over Time (DOT) Settings", labelColor);
            EditorGUILayout.PropertyField(applyDOT);
            if (applyDOT.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(dotAmount);
                EditorGUILayout.PropertyField(dotDuration);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawBuffsDebuffsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Buffs & Debuffs (Self)", labelColor);

            // Heal
            EditorGUILayout.PropertyField(receiveHeal);
            if (receiveHeal.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(healAmount);
                EditorGUI.indentLevel--;
            }

            // Shield
            EditorGUILayout.PropertyField(receiveShield);
            if (receiveShield.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(shieldAmount);
                EditorGUILayout.PropertyField(shieldDuration);
                EditorGUI.indentLevel--;
            }

            // Move Speed
            EditorGUILayout.PropertyField(receiveMoveSpeed);
            if (receiveMoveSpeed.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(moveSpeedAmount);
                EditorGUILayout.PropertyField(moveSpeedDuration);
                EditorGUI.indentLevel--;
            }

            // Attack Speed
            EditorGUILayout.PropertyField(receiveAttackSpeed);
            if (receiveAttackSpeed.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(attackSpeedAmount);
                EditorGUILayout.PropertyField(attackSpeedDuration);
                EditorGUI.indentLevel--;
            }

            // Defense
            EditorGUILayout.PropertyField(receiveDefense);
            if (receiveDefense.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(defenseAmount);
                EditorGUILayout.PropertyField(defenseDuration);
                EditorGUI.indentLevel--;
            }

            // Extra Damage
            EditorGUILayout.PropertyField(receiveDamage);
            if (receiveDamage.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(damageAmount);
                EditorGUILayout.PropertyField(damageDuration);
                EditorGUI.indentLevel--;
            }

            // Invincible
            EditorGUILayout.PropertyField(isInvincible);
            if (isInvincible.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(invincibleDuration);
                EditorGUI.indentLevel--;
            }

            // Debuff (Self Slow)
            EditorUIUtils.DrawSubHeader("Debuff (Self)", labelColor);
            EditorGUILayout.PropertyField(receiveSlow);
            if (receiveSlow.boolValue)
            {
                EditorGUI.indentLevel++;
                receiveSlowAmount.floatValue = EditorGUILayout.Slider("Slow Amount (%)", receiveSlowAmount.floatValue, 0f, 1f);
                EditorGUILayout.PropertyField(receiveSlowDuration, new GUIContent("Slow Duration (s)"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawEvolveBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Evolve Changes", labelColor);
            EditorGUILayout.PropertyField(evolveChanges);
            if (evolveChanges.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(evolvedDamageEntityPrefab);
                EditorGUILayout.PropertyField(shotsEvolved);
                EditorGUILayout.PropertyField(angleEvolved);
                EditorGUILayout.PropertyField(delayEvolved);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(requireStatForEvolve);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawSkillLevelsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Skill Levels", labelColor);
            EditorGUILayout.PropertyField(skillLevels, true);
            EditorGUILayout.HelpBox("Configure baseDamage, speed, lifetime, etc. for each level.", MessageType.Info);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Automatically draws any new serialized fields not explicitly handled above.
        /// </summary>
        private void DrawOtherSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Other Settings", labelColor);

            var prop = serializedObject.GetIterator();
            prop.NextVisible(true); // Skip script reference

            while (prop.NextVisible(false))
            {
                if (_handledProps.Contains(prop.name))
                    continue;

                EditorGUILayout.PropertyField(prop, true);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
}
#endif
