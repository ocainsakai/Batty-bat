//#if UNITY_EDITOR
//using UnityEditor;
//using UnityEngine;

//namespace BulletHellTemplate
//{
//    [CustomEditor(typeof(CharacterModel))]
//    public class CharacterModelEditor : Editor
//    {
//        /* ───────────── Property refs ───────────── */
//        private SerializedProperty usePlayableAnimationsProp;
//        private SerializedProperty animatorProp;
//        private SerializedProperty baseTemplateProp;
//        private SerializedProperty upperBodyMaskProp;

//        /* Locomotion */
//        private SerializedProperty idleClipProp;
//        private SerializedProperty runForwardProp;
//        private SerializedProperty runBackwardProp;
//        private SerializedProperty runLeftProp;
//        private SerializedProperty runRightProp;

//        /* Combat */
//        private SerializedProperty attackProp;
//        private SerializedProperty skillsProp;

//        /* Optional */
//        private SerializedProperty hitProp;
//        private SerializedProperty dashProp;
//        private SerializedProperty stunProp;
//        private SerializedProperty pickUpProp;

//        /* Events */
//        private SerializedProperty onAttackProp;
//        private SerializedProperty onDashProp;
//        private SerializedProperty onReceiveHitProp;
//        private SerializedProperty onReceiveBuffProp;
//        private SerializedProperty onReceiveDebuffProp;
//        private SerializedProperty OnUpgradeCharacterProp;
//        private SerializedProperty OnCharacterInstantiateProp;

//        /* ───────────── Init ───────────── */
//        private void OnEnable()
//        {
//            /* Core refs */
//            usePlayableAnimationsProp = serializedObject.FindProperty("usePlayableAnimations");
//            animatorProp = serializedObject.FindProperty("_anim");
//            baseTemplateProp = serializedObject.FindProperty("baseTemplate");
//            upperBodyMaskProp = serializedObject.FindProperty("upperBodyMask");

//            /* Locomotion */
//            idleClipProp = serializedObject.FindProperty("idleClip");
//            runForwardProp = serializedObject.FindProperty("runForward");
//            runBackwardProp = serializedObject.FindProperty("runBackward");
//            runLeftProp = serializedObject.FindProperty("runLeft");
//            runRightProp = serializedObject.FindProperty("runRight");

//            /* Combat */
//            attackProp = serializedObject.FindProperty("attack");
//            skillsProp = serializedObject.FindProperty("skills");

//            /* Optional */
//            hitProp = serializedObject.FindProperty("hit");
//            dashProp = serializedObject.FindProperty("dash");
//            stunProp = serializedObject.FindProperty("stun");
//            pickUpProp = serializedObject.FindProperty("pickUp");

//            /* Events */
//            onAttackProp = serializedObject.FindProperty("onAttack");
//            onDashProp = serializedObject.FindProperty("onDash");
//            onReceiveHitProp = serializedObject.FindProperty("onReceiveHit");
//            onReceiveBuffProp = serializedObject.FindProperty("onReceiveBuff");
//            onReceiveBuffProp = serializedObject.FindProperty("onReceiveBuff");
//            onReceiveDebuffProp = serializedObject.FindProperty("onReceiveDebuff");

//            OnUpgradeCharacterProp = serializedObject.FindProperty("OnUpgradeCharacter");
//            OnCharacterInstantiateProp = serializedObject.FindProperty("OnCharacterInstantiate");
//        }

//        /* ───────────── Inspector GUI ───────────── */
//        public override void OnInspectorGUI()
//        {
//            serializedObject.Update();

//            /* Core */
//            EditorGUILayout.PropertyField(usePlayableAnimationsProp, new GUIContent("Use PlayableAPI"));
//            EditorGUILayout.PropertyField(animatorProp, new GUIContent("Animator"));
//            EditorGUILayout.PropertyField(baseTemplateProp, new GUIContent("AC"));
//            EditorGUILayout.PropertyField(upperBodyMaskProp, new GUIContent("Upper-Body Mask"));

//            /* Locomotion */
//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField("Locomotion Clips", EditorStyles.boldLabel);
//            EditorGUILayout.PropertyField(idleClipProp, new GUIContent("Idle"));
//            EditorGUILayout.PropertyField(runForwardProp, new GUIContent("Run Forward (Required)"));
//            EditorGUILayout.PropertyField(runBackwardProp, new GUIContent("Run Backward"));
//            EditorGUILayout.PropertyField(runLeftProp, new GUIContent("Run Left"));
//            EditorGUILayout.PropertyField(runRightProp, new GUIContent("Run Right"));

//            /* Combat */
//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField("Combat", EditorStyles.boldLabel);
//            EditorGUILayout.PropertyField(attackProp, true);
//            EditorGUILayout.PropertyField(skillsProp, true);

//            /* Optional */
//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField("Optional Clips", EditorStyles.boldLabel);
//            EditorGUILayout.PropertyField(hitProp);
//            EditorGUILayout.PropertyField(dashProp);
//            EditorGUILayout.PropertyField(stunProp);
//            EditorGUILayout.PropertyField(pickUpProp);

//            /* Events */
//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField("Unity Events", EditorStyles.boldLabel);
//            EditorGUILayout.PropertyField(onAttackProp);
//            EditorGUILayout.PropertyField(onDashProp);
//            EditorGUILayout.PropertyField(onReceiveHitProp);
//            EditorGUILayout.PropertyField(onReceiveBuffProp);
//            EditorGUILayout.PropertyField(onReceiveDebuffProp);

//            EditorGUILayout.LabelField("UI Unity Events", EditorStyles.boldLabel);
//            EditorGUILayout.PropertyField(OnUpgradeCharacterProp);
//            EditorGUILayout.PropertyField(OnCharacterInstantiateProp);

//            serializedObject.ApplyModifiedProperties();
//        }
//    }
//}
//#endif
