// ============================================================
// Step9_PreviewLamps.cs
// Editor-only script.
//
// Run via:  VR Furniture Shop > Step 9 - Preview All (Lamps + New Batch)
//           VR Furniture Shop > Step 9 - Preview Lamps Only
//           VR Furniture Shop > Step 9 - Preview New Batch Only
// Remove:   VR Furniture Shop > Step 9 - Remove All Previews
//
// What it does:
//   1. LAMPS (6 items) - Stage 1 corrections:
//      - Sets ModelImporter globalScale = 0.2 and localRotation = (270, 90, 90).
//      - Blue:   Pos (0.5, 0.0, -2.6)
//      - Cyan:   Pos (1.8, 0.0, -2.6)
//      - Green:  Pos (3.1, 0.0, -2.6)
//      - Purple: Pos (4.4, 0.0, -2.6)
//      - Red:    Pos (5.7, 2.8, -2.6)  [CORRECTION: Y = 2.8]
//      - Yellow: Pos (7.0, 0.0, -1.6)  [CORRECTION: Z = -1.6]
//
//   2. NEW BATCH (8 items) - Stage 1 visual review:
//      - 3 Furniture: Cot, Chair, DressingTable
//      - 5 Mini Fridges: MiniFridge v1 to v5
//      - Scale Factor: 0.2 starting guess
//      - Rotation: (270, 90, 90) starting guess
//      - Instantiated in a clean row at Z = 2.2m (X = 0.5m to 9.6m, spacing 1.3m)
//      - Informational 3D text labels above each item
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class Step9_PreviewLamps
{
    // --------------------------------------------------------
    // Lamp Definitions
    // --------------------------------------------------------
    private static readonly string[] s_LampPaths = new[]
    {
        "Assets/Models/Furniture/lamp_blue.fbx",
        "Assets/Models/Furniture/lamp_cyan.fbx",
        "Assets/Models/Furniture/lamp_green.fbx",
        "Assets/Models/Furniture/lamp_purple.fbx",
        "Assets/Models/Furniture/lamp_red.fbx",
        "Assets/Models/Furniture/lamp_yellow.fbx"
    };

    private static readonly string[] s_LampNames = new[]
    {
        "lamp_blue",
        "lamp_cyan",
        "lamp_green",
        "lamp_purple",
        "lamp_red",
        "lamp_yellow"
    };

    // --------------------------------------------------------
    // New Batch Definitions (8 items)
    // --------------------------------------------------------
    private static readonly string[] s_NewBatchPaths = new[]
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

    private static readonly string[] s_NewBatchNames = new[]
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

    private static readonly Vector3 START_ROTATION_EULER = new Vector3(270f, 90f, 90f);
    private const float TARGET_SCALE_FACTOR = 0.2f;

    // Lamp row settings
    private const float LAMP_ROW_Z = -2.6f;
    private const float LAMP_ROW_Y = 0f;
    private const float LAMP_START_X = 0.5f;
    private const float LAMP_SPACING_X = 1.3f;

    // New batch row settings
    private const float NEW_ROW_Z = 2.2f;
    private const float NEW_ROW_Y = 0f;
    private const float NEW_START_X = 0.5f;
    private const float NEW_SPACING_X = 1.3f;

    static Step9_PreviewLamps()
    {
        EditorApplication.delayCall += AutoRunOnce;
    }

    private static void AutoRunOnce()
    {
        if (SessionState.GetBool("Step9_AutoPreview_V4_Executed", false))
            return;

        SessionState.SetBool("Step9_AutoPreview_V4_Executed", true);
        RunAll();
    }

    [MenuItem("VR Furniture Shop/Step 9 - Preview All (Lamps + New Batch)")]
    public static void RunAll()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step9] Exit Play Mode before running this script.");
            return;
        }

        AssetDatabase.Refresh();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        }

        SetupLampPreviews();
        SetupNewBatchPreviews();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Step9] All previews (Lamps with corrections + New Batch) set up and scene saved successfully!");
    }

    [MenuItem("VR Furniture Shop/Step 9 - Preview Lamps Only")]
    public static void RunLampsOnly()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step9] Exit Play Mode before running this script.");
            return;
        }

        AssetDatabase.Refresh();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        }

        SetupLampPreviews();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Step9] Lamp previews set up successfully!");
    }

    [MenuItem("VR Furniture Shop/Step 9 - Preview New Batch Only")]
    public static void RunNewBatchOnly()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step9] Exit Play Mode before running this script.");
            return;
        }

        AssetDatabase.Refresh();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        }

        SetupNewBatchPreviews();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Step9] New batch previews set up successfully!");
    }

    private static void SetupLampPreviews()
    {
        // 1. Verify / Apply ModelImporter Scale Factor 0.2
        for (int i = 0; i < s_LampPaths.Length; i++)
        {
            string path = s_LampPaths[i];
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                if (Mathf.Abs(importer.globalScale - TARGET_SCALE_FACTOR) > 0.001f)
                {
                    importer.globalScale = TARGET_SCALE_FACTOR;
                    importer.SaveAndReimport();
                    Debug.Log($"[Step9] Reimported {s_LampNames[i]} with globalScale = {TARGET_SCALE_FACTOR}");
                }
            }
            else
            {
                Debug.LogWarning($"[Step9] Could not find ModelImporter for {path}");
            }
        }

        // 2. Clean up previous Lamp_Previews root if present
        GameObject existingRoot = GameObject.Find("Lamp_Previews");
        if (existingRoot != null)
        {
            Undo.DestroyObjectImmediate(existingRoot);
        }

        // 3. Create root "Lamp_Previews"
        GameObject root = new GameObject("Lamp_Previews");
        Undo.RegisterCreatedObjectUndo(root, "Create Lamp_Previews");
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;

        // 4. Instantiate each lamp in preview row with user corrections
        for (int i = 0; i < s_LampPaths.Length; i++)
        {
            string path = s_LampPaths[i];
            string lampName = s_LampNames[i];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError($"[Step9] Failed to load lamp prefab at {path}");
                continue;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
            if (instance == null)
            {
                instance = Object.Instantiate(prefab, root.transform);
            }

            instance.name = $"Preview_{lampName}";
            float posX = LAMP_START_X + i * LAMP_SPACING_X;
            float posY = LAMP_ROW_Y;
            float posZ = LAMP_ROW_Z;

            // User corrections:
            // - Red lamp: adjust Position Y to 2.8 (lines it up level with the others)
            // - Yellow lamp: adjust Position Z to -1.6 (lines it up in line with the others)
            if (lampName == "lamp_red")
            {
                posY = 2.8f;
            }
            else if (lampName == "lamp_yellow")
            {
                posZ = -1.6f;
            }

            Vector3 pos = new Vector3(posX, posY, posZ);
            instance.transform.position = pos;
            instance.transform.rotation = Quaternion.Euler(START_ROTATION_EULER);

            // Add identification label above each lamp
            GameObject labelObj = new GameObject($"{lampName}_Label");
            labelObj.transform.SetParent(instance.transform, false);
            labelObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            labelObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            TextMesh textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.text = $"{lampName}\nPos: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})\n[Scale: 0.2 | Rot: (270, 90, 90)]";
            textMesh.fontSize = 24;
            textMesh.characterSize = 0.035f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;

            Debug.Log($"[Step9] Placed {lampName} at position ({pos.x:F1}, {pos.y:F1}, {pos.z:F1}) with rotation {START_ROTATION_EULER}");
        }
    }

    private static void SetupNewBatchPreviews()
    {
        // 1. Verify / Apply ModelImporter Scale Factor 0.2 on 8 new FBX files
        for (int i = 0; i < s_NewBatchPaths.Length; i++)
        {
            string path = s_NewBatchPaths[i];
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                if (Mathf.Abs(importer.globalScale - TARGET_SCALE_FACTOR) > 0.001f)
                {
                    importer.globalScale = TARGET_SCALE_FACTOR;
                    importer.SaveAndReimport();
                    Debug.Log($"[Step9] Reimported {s_NewBatchNames[i]} with globalScale = {TARGET_SCALE_FACTOR}");
                }
                else
                {
                    Debug.Log($"[Step9] {s_NewBatchNames[i]} already has globalScale = {TARGET_SCALE_FACTOR}");
                }
            }
            else
            {
                Debug.LogWarning($"[Step9] ModelImporter not yet available for {path} (will be imported on asset refresh)");
            }
        }

        // 2. Clean up previous NewBatch_Previews root if present
        GameObject existingRoot = GameObject.Find("NewBatch_Previews");
        if (existingRoot != null)
        {
            Undo.DestroyObjectImmediate(existingRoot);
        }

        // 3. Create root "NewBatch_Previews"
        GameObject root = new GameObject("NewBatch_Previews");
        Undo.RegisterCreatedObjectUndo(root, "Create NewBatch_Previews");
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;

        // 4. Instantiate each new model in preview row
        for (int i = 0; i < s_NewBatchPaths.Length; i++)
        {
            string path = s_NewBatchPaths[i];
            string modelName = s_NewBatchNames[i];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError($"[Step9] Failed to load new batch prefab at {path}");
                continue;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
            if (instance == null)
            {
                instance = Object.Instantiate(prefab, root.transform);
            }

            instance.name = $"Preview_{modelName}";
            Vector3 pos = new Vector3(NEW_START_X + i * NEW_SPACING_X, NEW_ROW_Y, NEW_ROW_Z);
            instance.transform.position = pos;
            instance.transform.rotation = Quaternion.Euler(START_ROTATION_EULER);

            // Add identification label above each model
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

            Debug.Log($"[Step9] Placed {modelName} at position ({pos.x:F1}, {pos.y:F1}, {pos.z:F1}) with rotation {START_ROTATION_EULER}");
        }
    }

    [MenuItem("VR Furniture Shop/Step 9 - Remove Lamp Previews")]
    public static void RemoveLampPreviews()
    {
        GameObject root = GameObject.Find("Lamp_Previews");
        if (root != null)
        {
            Undo.DestroyObjectImmediate(root);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("[Step9] Lamp previews removed.");
        }
    }

    [MenuItem("VR Furniture Shop/Step 9 - Remove New Batch Previews")]
    public static void RemoveNewBatchPreviews()
    {
        GameObject root = GameObject.Find("NewBatch_Previews");
        if (root != null)
        {
            Undo.DestroyObjectImmediate(root);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("[Step9] New batch previews removed.");
        }
    }

    [MenuItem("VR Furniture Shop/Step 9 - Remove All Previews")]
    public static void RemoveAllPreviews()
    {
        RemoveLampPreviews();
        RemoveNewBatchPreviews();
    }
}
