// -----------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR           // --------- Editor-only code ------------------------
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.IO;

namespace BulletHellTemplate
{
    /// <summary>
    /// Creates a wizard window that merges meshes by material, bakes them into
    /// new Mesh assets and saves a passEntryPrefab with the combined meshes.
    /// <para>Open via Tools ▸ Mesh Combine Wizard.</para>
    /// </summary>
    public class MeshCombineWizard : ScriptableWizard
    {
        /* ───────────────────── Wizard parameters ───────────────────── */
        [Header("Source")]
        [Tooltip("Root object whose children will be combined.")]
        public GameObject combineParent;

        [Header("Output")]
        [Tooltip("Folder path relative to Assets/ where new meshes & passEntryPrefab go.")]
        public string resultPath = "CombinedMeshes";
        [Tooltip("Use 32-bit index buffer (allows >65K vertices per sub-mesh).")]
        public bool is32bit = true;
        [Tooltip("Generate secondary UVs for lightmapping.")]
        public bool generateSecondaryUVs = false;

        /* ───────────────────── Menu entry ───────────────────── */
        [MenuItem("Tools/Mesh Combine Wizard")]
        private static void CreateWizard()
        {
            var wizard = DisplayWizard<MeshCombineWizard>("Mesh Combine Wizard");

            // Auto-assign selection if exactly one GameObject is selected.
            if (Selection.objects.Length == 1 && Selection.activeGameObject)
                wizard.combineParent = Selection.activeGameObject;
        }

        /* ───────────────────── Core logic ───────────────────── */
        private void OnWizardCreate()
        {
            if (combineParent == null)
            {
                Debug.LogError("Mesh Combine Wizard: No source object assigned.");
                return;
            }

            string assetFolder = Path.Combine("Assets", resultPath);
            if (!Directory.Exists(assetFolder))
            {
                Debug.LogError($"Mesh Combine Wizard: Folder '{assetFolder}' not found.");
                return;
            }

            // Save original position, then reset to origin for correct transforms.
            Vector3 originalPos = combineParent.transform.position;
            combineParent.transform.position = Vector3.zero;

            var matToFilters = new Dictionary<Material, List<MeshFilter>>();
            foreach (MeshFilter mf in combineParent.GetComponentsInChildren<MeshFilter>())
            {
                MeshRenderer mr = mf.GetComponent<MeshRenderer>();
                if (mr == null || mr.sharedMaterials == null || mr.sharedMaterials.Length == 0)
                    continue;

                if (mr.sharedMaterials.Length > 1)
                {
                    Debug.LogError($"Mesh '{mf.name}' uses multiple materials. Abort.");
                    combineParent.transform.position = originalPos;
                    return;
                }

                Material mat = mr.sharedMaterials[0];
                if (!matToFilters.ContainsKey(mat))
                    matToFilters[mat] = new List<MeshFilter>();
                matToFilters[mat].Add(mf);
            }

            var combinedGOs = new List<GameObject>();

            foreach (var pair in matToFilters)
            {
                Material mat = pair.Key;
                List<MeshFilter> filters = pair.Value;

                var combine = new CombineInstance[filters.Count];
                for (int i = 0; i < filters.Count; i++)
                {
                    combine[i].mesh = filters[i].sharedMesh;
                    combine[i].transform = filters[i].transform.localToWorldMatrix;
                }

                Mesh outMesh = new Mesh { indexFormat = is32bit ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                outMesh.CombineMeshes(combine);

                if (generateSecondaryUVs)
                    Unwrapping.GenerateSecondaryUVSet(outMesh);

                string meshName = $"Combined_{mat.name}_{outMesh.GetInstanceID()}";
                AssetDatabase.CreateAsset(outMesh, Path.Combine(assetFolder, $"{meshName}.asset"));

                GameObject go = new GameObject(meshName);
                go.AddComponent<MeshFilter>().sharedMesh = outMesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                combinedGOs.Add(go);
            }

            // Group multiple outputs
            GameObject rootGO = combinedGOs.Count == 1
                ? combinedGOs[0]
                : new GameObject($"Combined_{combineParent.name}");

            if (combinedGOs.Count > 1)
                foreach (GameObject go in combinedGOs) go.transform.parent = rootGO.transform;

            string prefabPath = Path.Combine(assetFolder, $"{rootGO.name}.passEntryPrefab");
            PrefabUtility.SaveAsPrefabAssetAndConnect(rootGO, prefabPath, InteractionMode.UserAction);

            combineParent.SetActive(false);
            combineParent.transform.position = originalPos;
            rootGO.transform.position = originalPos;

            Debug.Log($"Mesh Combine Wizard: Success, passEntryPrefab saved at {prefabPath}");
        }
    }
}
#else                       // --------- Runtime stub (build-safe) -------------
namespace BulletHellTemplate
{
    /// <summary>
    /// Dummy placeholder so references compile in player builds.
    /// </summary>
    public class MeshCombineWizard : MonoBehaviour { }
}
#endif
