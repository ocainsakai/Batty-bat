using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents the data for a coin in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCurrency", menuName = "Monetization/Currency", order = 51)]
    public class Currency : ScriptableObject
    {
        [Header("Coin Information")]

        [Tooltip("The name of the Currency.")]
        public string coinName;

        [Tooltip("The unique identifier for the Currency.")]
        public string coinID;

        [Tooltip("The icon representing the Currency.")]
        public Sprite icon;

        [Tooltip("The initial amount of this Currency the player starts with.")]
        public int initialAmount;

        [Tooltip("Option to use a maximum amount limit for the Currency.")]
        public bool useMaxAmount;

        [Tooltip("The maximum amount of Currency allowed.")]
        public int maxAmount;

        [Tooltip("Determines if the Currency value can exceed the maximum limit in purchases and gifts.")]
        public bool canExceedMaxValue;

        [Tooltip("Indicates if the Currency is rechargeable.")]
        public bool isRechargeableCurrency;

        [Tooltip("Time scale used for recharging the Currency.")]
        public rechargeableTimeScale rechargeableTimeScale;

        [Tooltip("Time duration for the Currency to recharge.")]
        public float rechargeableTime;

        [Tooltip("The amount of Currency recharged after the recharge time.")]
        public int rechargeAmount;

        [Tooltip("Specifies if the Currency should recharge while offline.")]
        public bool RechargeWhileOffline;
    }

    /// <summary>
    /// Enum to define time scale options for recharging.
    /// </summary>
    public enum rechargeableTimeScale
    {
        Seconds,
        Minutes,
        Hours
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom editor for the Currency ScriptableObject.
    /// This editor displays grouped fields in boxes and conditionally shows/hides fields based on certain boolean values.
    /// </summary>
    [CustomEditor(typeof(Currency))]
    public class CurrencyEditor : Editor
    {
        // Serialized properties for Currency fields.
        SerializedProperty coinNameProp;
        SerializedProperty coinIDProp;
        SerializedProperty iconProp;
        SerializedProperty initialAmountProp;
        SerializedProperty useMaxAmountProp;
        SerializedProperty maxAmountProp;
        SerializedProperty canExceedMaxValueProp;
        SerializedProperty isRechargeableCurrencyProp;
        SerializedProperty rechargeableTimeScaleProp;
        SerializedProperty rechargeableTimeProp;
        SerializedProperty rechargeAmountProp;
        SerializedProperty RechargeWhileOfflineProp;

        /// <summary>
        /// Initializes the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            coinNameProp = serializedObject.FindProperty("coinName");
            coinIDProp = serializedObject.FindProperty("coinID");
            iconProp = serializedObject.FindProperty("icon");
            initialAmountProp = serializedObject.FindProperty("initialAmount");
            useMaxAmountProp = serializedObject.FindProperty("useMaxAmount");
            maxAmountProp = serializedObject.FindProperty("maxAmount");
            canExceedMaxValueProp = serializedObject.FindProperty("canExceedMaxValue");
            isRechargeableCurrencyProp = serializedObject.FindProperty("isRechargeableCurrency");
            rechargeableTimeScaleProp = serializedObject.FindProperty("rechargeableTimeScale");
            rechargeableTimeProp = serializedObject.FindProperty("rechargeableTime");
            rechargeAmountProp = serializedObject.FindProperty("rechargeAmount");
            RechargeWhileOfflineProp = serializedObject.FindProperty("RechargeWhileOffline");
        }

        /// <summary>
        /// Custom inspector GUI for the Currency ScriptableObject.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Update the serialized object representation.
            serializedObject.Update();

            // Box area for Coin Information.
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Coin Information", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(coinNameProp);
            EditorGUILayout.PropertyField(coinIDProp);
            EditorGUILayout.PropertyField(iconProp);
            EditorGUILayout.PropertyField(initialAmountProp);
            EditorGUILayout.EndVertical();

            // Box area for Maximum Amount Settings.
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Maximum Amount Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(useMaxAmountProp);
            if (useMaxAmountProp.boolValue)
            {
                EditorGUILayout.PropertyField(maxAmountProp);
                EditorGUILayout.PropertyField(canExceedMaxValueProp);
            }
            EditorGUILayout.EndVertical();

            // Box area for Rechargeable Settings.
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Rechargeable Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(isRechargeableCurrencyProp);
            if (isRechargeableCurrencyProp.boolValue)
            {
                EditorGUILayout.PropertyField(rechargeableTimeScaleProp);
                EditorGUILayout.PropertyField(rechargeableTimeProp);
                EditorGUILayout.PropertyField(rechargeAmountProp);
                EditorGUILayout.PropertyField(RechargeWhileOfflineProp);
            }
            EditorGUILayout.EndVertical();

            // Apply the modifications to the serialized object.
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
