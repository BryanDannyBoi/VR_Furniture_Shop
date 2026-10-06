// ============================================================
// Step10_PreviewNewBatch.cs
// Editor-only script.
//
// Run via:    VR Furniture Shop > Step 10 - Preview New Batch (Scale 0.2, Rot 270 90 90)
// Remove via: VR Furniture Shop > Step 10 - Remove New Batch Previews
//
// What it does:
//   1. Sets ModelImporter Scale Factor to 0.2 on all 8 new FBX models:
//      - Cot:                    Assets/Models/Furniture/cot.fbx
//      - Chair:                  Assets/Models/Furniture/chair.fbx
//      - DressingTable:          Assets/Models/Furniture/Dressing_Table_3D.fbx
//      - MiniFridge v1 original: Assets/Models/Furniture/MiniFridge_v1_original.fbx
//      - MiniFridge v2 cube:     Assets/Models/Furniture/MiniFridge_v2_cube_white.fbx
//      - MiniFridge v3 bar:      Assets/Models/Furniture/MiniFridge_v3_bar_silver.fbx
//      - MiniFridge v4 retro:    Assets/Models/Furniture/MiniFridge_v4_retro_mint.fbx
//      - MiniFridge v5 wine:     Assets/Models/Furniture/MiniFridge_v5_wine_cooler.fbx
//   2. Instantiates each model in SampleScene under root "NewBatch_Previews".
//   3. Sets initial rotation to Euler(270, 90, 90).
//   4. Arranges them in a clean preview row (Z = 2.2m, Y = 0m, X = 0.5m to 9.6m).
//   5. Adds a floating text label above each item displaying its name, position,
//      scale, and rotation for visual check.
//   6. Saves the scene.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class Step10_PreviewNewBatch
{
    private static readonly string[] s_ModelPaths = new[]
    {
        "Assets/Models/Furniture/cot.fbx",
        "Assets/Models/Furniture/chair.fbx",
        "Assets/Models/Furniture/Dressing_Table_3D.fbx",
        "Assets/Models/Furniture/MiniFridge_v1_original.fbx",
        "Assets/Models/Furniture/MiniFridge_v2_cube_white.fbx",
        "Assets/Models/Furniture/MiniFridge_v3_bar_silver.fbx",
        "Assets/Models/Furniture/MiniFridge_v4_retro_mint.fbx",
        "Assets/Models/Furniture/MiniFridge_v5_wine_cooler.fbx"
    };

    private static readonly string[] s_ModelNames = new[]
    {
        "Cot",
        "Chair",
        "DressingTable",
        "MiniFridge_v1_original",
        "MiniFridge_v2_cube_white",
        "MiniFridge_v3_bar_silver",
        "MiniFridge_v4_retro_mint",
        "MiniFridge_v5_wine_cooler"
    };

    private const float ROW_Z = 2.2f;
    private const float ROW_Y = 0f;
    private const float START_X = 0.5f;
    private const float SPACING_X = 1.3f;
    private static readonly Vector3 START_ROTATION_EULER = new Vector3(270f, 90f, 90f);
    private const float TARGET_SCALE_FACTOR = 0.2f;

    static Step10_PreviewNewBatch()
    {
        EditorApplication.delayCall += AutoRunOnce;
    }

    private static void AutoRunOnce()
    {
        if (SessionState.GetBool("Step10_AutoPreviewExecuted", false))
            return;

        SessionState.SetBool("Step10_AutoPreviewExecuted", true);
        Run();
    }

    [MenuItem("VR Furniture Shop/Step 10 - Preview New Batch (Scale 0.2, Rot 270 90 90)")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step10] Exit Play Mode before running this script.");
            return;
        }

        AssetDatabase.Refresh();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        }

        // --------------------------------------------------------
        // 1. Verify / Apply ModelImporter Scale Factor 0.2
        // --------------------------------------------------------
        for (int i = 0; i < s_ModelPaths.Length; i++)
        {
            string path = s_ModelPaths[i];
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                if (Mathf.Abs(importer.globalScale - TARGET_SCALE_FACTOR) > 0.001f)
                {
                    importer.globalScale = TARGET_SCALE_FACTOR;
                    importer.SaveAndReimport();
                    Debug.Log($"[Step10] Reimported {s_ModelNames[i]} with globalScale = {TARGET_SCALE_FACTOR}");
                }
                else
                {
                    Debug.Log($"[Step10] {s_ModelNames[i]} already has globalScale = {TARGET_SCALE_FACTOR}");
                }
            }
            else
            {
                Debug.LogWarning($"[Step10] Could not find ModelImporter for {path}");
            }
        }

        // --------------------------------------------------------
        // 2. Clean up previous NewBatch_Previews root if present
        // --------------------------------------------------------
        GameObject existingRoot = GameObject.Find("NewBatch_Previews");
        if (existingRoot != null)
        {
            Undo.DestroyObjectImmediate(existingRoot);
        }

        // --------------------------------------------------------
        // 3. Create root "NewBatch_Previews"
        // --------------------------------------------------------
        GameObject root = new GameObject("NewBatch_Previews");
        Undo.RegisterCreatedObjectUndo(root, "Create NewBatch_Previews");
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;

        // --------------------------------------------------------
        // 4. Instantiate each new model in preview row
        // --------------------------------------------------------
        for (int i = 0; i < s_ModelPaths.Length; i++)
        {
            string path = s_ModelPaths[i];
            string modelName = s_ModelNames[i];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError($"[Step10] Failed to load prefab at {path}");
                continue;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
            if (instance == null)
            {
                instance = Object.Instantiate(prefab, root.transform);
            }

            instance.name = $"Preview_{modelName}";
            Vector3 pos = new Vector3(START_X + i * SPACING_X, ROW_Y, ROW_Z);
            instance.transform.position = pos;
            instance.transform.rotation = Quaternion.Euler(START_ROTATION_EULER);

            // Add identification label above each model for easy visual review
            GameObject labelObj = new GameObject($"{modelName}_Label");
            labelObj.transform.SetParent(instance.transform, false);
            labelObj.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            labelObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            TextMesh textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.text = $"{modelName}\nPos: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})\n[Scale: 0.2 | Rot: (270, 90, 90)]";
            textMesh.fontSize = 24;
            textMesh.characterSize = 0.035f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;

            Debug.Log($"[Step10] Placed {modelName} at position ({pos.x:F1}, {pos.y:F1}, {pos.z:F1}) with rotation {START_ROTATION_EULER}");
        }

        // --------------------------------------------------------
        // 5. Save Scene
        // --------------------------------------------------------
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Step10] New batch preview setup completed successfully!");
    }

    [MenuItem("VR Furniture Shop/Step 10 - Remove New Batch Previews")]
    public static void RemovePreviews()
    {
        GameObject root = GameObject.Find("NewBatch_Previews");
        if (root != null)
        {
            Undo.DestroyObjectImmediate(root);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("[Step10] New batch previews removed.");
        }
    }
}
