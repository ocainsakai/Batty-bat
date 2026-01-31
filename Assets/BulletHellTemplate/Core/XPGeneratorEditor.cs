#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Editor script to generate XP progression values for gameplay.
    /// </summary>
    public class XPProgressionGenerator : EditorWindow
    {
        private int level1XP = 40; // XP required for level 1
        private int maxLevelXP = 8230; // XP required for the max level
        private int maxLevel = 40; // Maximum level
        private int[] xpToNextLevel;

        [MenuItem("Tools/XP Progression Generator")]
        public static void ShowWindow()
        {
            GetWindow<XPProgressionGenerator>("XP Progression Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("XP Progression Generator", EditorStyles.boldLabel);

            level1XP = EditorGUILayout.IntField("XP for Level 1", level1XP);
            maxLevelXP = EditorGUILayout.IntField("XP for Max Level", maxLevelXP);
            maxLevel = EditorGUILayout.IntField("Max Level", maxLevel);

            if (GUILayout.Button("Generate XP Progression"))
            {
                GenerateXPProgression();
            }
        }

        private void GenerateXPProgression()
        {
            // Validate inputs
            if (level1XP <= 0 || maxLevelXP <= level1XP || maxLevel <= 1)
            {
                Debug.LogError("Invalid XP values. Ensure XP for Level 1 is greater than 0, Max Level XP is greater, and Max Level is greater than 1.");
                return;
            }

            // Calculate XP progression values
            List<int> xpValues = new List<int>();
            xpValues.Add(level1XP);

            float factor = Mathf.Pow((float)maxLevelXP / level1XP, 1f / (maxLevel - 1));
            int previousXP = level1XP;

            for (int level = 1; level < maxLevel; level++)
            {
                int xpForNextLevel = Mathf.RoundToInt(previousXP * factor);
                xpValues.Add(xpForNextLevel);
                previousXP = xpForNextLevel;
            }

            xpToNextLevel = xpValues.ToArray();

#if UNITY_6000_0_OR_NEWER
            GameplayManager gameplayManager = FindFirstObjectByType<GameplayManager>();
#else
            GameplayManager gameplayManager = FindObjectOfType<GameplayManager>();
#endif
            if (gameplayManager != null)
            {
                gameplayManager.xpToNextLevel = xpToNextLevel;
                EditorUtility.SetDirty(gameplayManager);
                Debug.Log("XP progression values updated successfully.");
            }
            else
            {
                Debug.LogError("GameplayManager not found in the scene.");
            }
        }
    }
}
#endif
