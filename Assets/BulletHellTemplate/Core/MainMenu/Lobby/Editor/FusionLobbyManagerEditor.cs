#if UNITY_EDITOR
#if FUSION2
using UnityEditor;

namespace BulletHellTemplate
{
    [CustomEditor(typeof(FusionLobbyManager))]
    class FusionLobbyManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var mgr = (FusionLobbyManager)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Connected Players", EditorStyles.boldLabel);

            foreach (var (pref, data) in mgr.OrderedPlayers())
                EditorGUILayout.LabelField($"[{pref.RawEncoded}] {data.playerName}");
        }
    }
}
#endif
#endif
