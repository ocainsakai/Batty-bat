using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BulletHellTemplate
{
    /// <summary>
    /// Runtime helper that lets developers inspect and modify a single PlayerPrefs key
    /// while the game is running. Attach it to any GameObject in the scene.
    /// </summary>
    public class PlayerPrefsDebugger : MonoBehaviour
    {
        [Tooltip("PlayerPrefs key to inspect or edit.")]
        public string key = string.Empty;

        [Tooltip("The value type stored under this key.")]
        public PrefValueType valueType = PrefValueType.Int;

        [Tooltip("New value to assign when pressing <b>Set Value</b>.")]
        public string newValue = string.Empty;

        /// <summary>
        /// Returns the value currently stored under <see cref="key"/> in PlayerPrefs.
        /// </summary>
        public string CurrentValue
        {
            get
            {
                switch (valueType)
                {
                    case PrefValueType.Int:
                        return PlayerPrefs.GetInt(key, 0).ToString();
                    case PrefValueType.Float:
                        return PlayerPrefs.GetFloat(key, 0f).ToString();
                    default:
                        return PlayerPrefs.GetString(key, string.Empty);
                }
            }
        }

        /// <summary>
        /// The three basic value types supported by Unity's PlayerPrefs API.
        /// </summary>
        public enum PrefValueType { Int, Float, String }

        /// <summary>
        /// Writes <see cref="newValue"/> to PlayerPrefs using <see cref="key"/> and
        /// the currently selected <see cref="valueType"/>. The method includes basic
        /// parsing validation for numeric values.
        /// </summary>
        public void SetValue()
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("Key is empty. Nothing was written.");
                return;
            }

            switch (valueType)
            {
                case PrefValueType.Int:
                    if (int.TryParse(newValue, out int i))
                        PlayerPrefs.SetInt(key, i);
                    else
                        Debug.LogError($"'{newValue}' is not a valid integer.");
                    break;
                case PrefValueType.Float:
                    if (float.TryParse(newValue, out float f))
                        PlayerPrefs.SetFloat(key, f);
                    else
                        Debug.LogError($"'{newValue}' is not a valid float.");
                    break;
                default:
                    PlayerPrefs.SetString(key, newValue);
                    break;
            }

            PlayerPrefs.Save();
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Polished inspector for <see cref="PlayerPrefsDebugger"/>. It is defined inside
    /// a UNITY_EDITOR block so that it is only compiled for the Editor, never for builds.
    /// </summary>
    [CustomEditor(typeof(PlayerPrefsDebugger))]
    public class PlayerPrefsDebuggerInspector : Editor
    {
        SerializedProperty keyProp;
        SerializedProperty typeProp;
        SerializedProperty valueProp;

        void OnEnable()
        {
            keyProp = serializedObject.FindProperty("key");
            typeProp = serializedObject.FindProperty("valueType");
            valueProp = serializedObject.FindProperty("newValue");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(keyProp, new GUIContent("Key"));
            EditorGUILayout.PropertyField(typeProp, new GUIContent("Value Type"));
            EditorGUILayout.PropertyField(valueProp, new GUIContent("New Value"));

            GUILayout.Space(4);

            if (GUILayout.Button("Get Current Value"))
            {
                var dbg = (PlayerPrefsDebugger)target;
                string current = dbg.CurrentValue;
                EditorUtility.DisplayDialog("Current Value", $"Key: {dbg.key}\nValue: {current}", "OK");
            }

            if (GUILayout.Button("Set Value"))
            {
                ((PlayerPrefsDebugger)target).SetValue();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

    /// <summary>
    /// Stand‑alone window that offers a quick way to read, write and delete any
    /// PlayerPrefs key. Open it via <c>Tools ▸ BulletHell Template ▸ PlayerPrefs Editor</c>.
    /// </summary>
    public class PlayerPrefsEditorWindow : EditorWindow
    {
        private string key = string.Empty;
        private PlayerPrefsDebugger.PrefValueType valueType = PlayerPrefsDebugger.PrefValueType.Int;
        private string value = string.Empty;
        private Vector2 scroll;

        [MenuItem("Tools/BulletHell Template/PlayerPrefs Editor", priority = 1)]
        public static void Open()
        {
            GetWindow<PlayerPrefsEditorWindow>("PlayerPrefs Editor");
        }

        void OnGUI()
        {
            GUILayout.Label("Key Editor", EditorStyles.boldLabel);

            key = EditorGUILayout.TextField("Key", key);
            valueType = (PlayerPrefsDebugger.PrefValueType)EditorGUILayout.EnumPopup("Value Type", valueType);
            value = EditorGUILayout.TextField("Value", value);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Get"))
            {
                value = GetCurrentValue();
            }
            if (GUILayout.Button("Set"))
            {
                SetCurrentValue();
            }
            if (GUILayout.Button("Delete"))
            {
                if (EditorUtility.DisplayDialog("Delete Key?", $"Remove '{key}' from PlayerPrefs?", "Yes", "No"))
                {
                    PlayerPrefs.DeleteKey(key);
                    PlayerPrefs.Save();
                    value = string.Empty;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("Known Keys (read‑only)", EditorStyles.boldLabel);

            scroll = GUILayout.BeginScrollView(scroll);
            foreach (var kv in GetPrefixedKeys())
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(kv.Key, GUILayout.Width(300));
                GUILayout.Label(kv.Value);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        string GetCurrentValue()
        {
            switch (valueType)
            {
                case PlayerPrefsDebugger.PrefValueType.Int:
                    return PlayerPrefs.GetInt(key, 0).ToString();
                case PlayerPrefsDebugger.PrefValueType.Float:
                    return PlayerPrefs.GetFloat(key, 0f).ToString();
                default:
                    return PlayerPrefs.GetString(key, string.Empty);
            }
        }

        void SetCurrentValue()
        {
            switch (valueType)
            {
                case PlayerPrefsDebugger.PrefValueType.Int:
                    if (int.TryParse(value, out int i))
                        PlayerPrefs.SetInt(key, i);
                    else
                        ShowParseError("int");
                    break;
                case PlayerPrefsDebugger.PrefValueType.Float:
                    if (float.TryParse(value, out float f))
                        PlayerPrefs.SetFloat(key, f);
                    else
                        ShowParseError("float");
                    break;
                default:
                    PlayerPrefs.SetString(key, value);
                    break;
            }
            PlayerPrefs.Save();
        }

        void ShowParseError(string type)
        {
            EditorUtility.DisplayDialog("Parsing Error", $"Value is not a valid {type}.", "OK");
        }

        /// <summary>
        /// Because Unity does not expose PlayerPrefs enumeration at runtime, we rely on
        /// known prefixes defined in <see cref="PlayerSave"/> to probe for existing keys.
        /// </summary>
        IDictionary<string, string> GetPrefixedKeys()
        {
            var dict = new Dictionary<string, string>();
            string[] prefixes =
            {
                "PLAYERNAME_", "PLAYERICON_", "PLAYERFRAME_", "PLAYERACCOUNTLEVEL_",
                "PLAYERACCOUNTCURRENTEXP_", "TUTORIALISDONE_", "PLAYERSELECTEDCHARACTER_",
                "PLAYERFAVOURITECHARACTER_", "CHARACTERMASTERYLEVEL_", "CHARACTERMASTERYCURRENTEXP_",
                "CHARACTERLEVEL_", "CHARACTERCURRENTEXP_", "ITEMSLOT_", "ITEMLEVEL_",
                "CHARACTERUPGRADELEVEL_", "CHARACTERSKIN_", "CHARACTERUNLOCKEDSKINS_",
                "UNLOCKED_MAPS", "QUEST_PROGRESS_", "QUEST_COMPLETED_", "USED_COUPONS_",
                "LOCAL_DAILYREWARDS_DATA", "LOCAL_NEWPLAYERREWARDS_DATA", "NEXTDAILYRESETTIME"
            };

            foreach (string prefix in prefixes)
            {
                if (PlayerPrefs.HasKey(prefix))
                    dict[prefix] = SafeRead(prefix);

                if (prefix.EndsWith("_"))
                {
                    for (int i = 0; i < 100; i++)
                    {
                        string keyVariant = prefix + i;
                        if (PlayerPrefs.HasKey(keyVariant))
                            dict[keyVariant] = SafeRead(keyVariant);
                    }
                }
            }
            return dict;
        }

        string SafeRead(string k)
        {
            // Try int
            int intVal = PlayerPrefs.GetInt(k, int.MinValue);
            if (intVal != int.MinValue) return intVal.ToString();
            // Try float
            float floatVal = PlayerPrefs.GetFloat(k, float.NaN);
            if (!float.IsNaN(floatVal)) return floatVal.ToString();
            // Fallback string
            return PlayerPrefs.GetString(k, string.Empty);
        }
    }
#endif
}
