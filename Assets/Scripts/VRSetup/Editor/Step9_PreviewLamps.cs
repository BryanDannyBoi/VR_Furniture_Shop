// ============================================================
// Step9_PreviewLamps.cs
// Editor-only script.
//
// Run via:  VR Furniture Shop > Step 9 - Preview Lamps (Scale 0.2, Rot 270 90 90)
// Remove via: VR Furniture Shop > Step 9 - Remove Lamp Previews
//
// What it does:
//   1. Verifies/sets ModelImporter Scale Factor to 0.2 on all 6 lamp FBX models:
//      - lamp_blue.fbx
//      - lamp_cyan.fbx
//      - lamp_green.fbx
//      - lamp_purple.fbx
//      - lamp_red.fbx
//      - lamp_yellow.fbx
//   2. Instantiates each lamp in SampleScene under a root "Lamp_Previews" object.
//   3. Sets localRotation to Euler(270, 90, 90).
//   4. Arranges the lamps in a clear preview row in the player's view:
//      Z = -2.6m, Y = 0m, spaced along X from 0.5m to 7.0m.
//   5. Adds a simple floating text label above each lamp (e.g. "lamp_blue")
//      so the user can immediately identify each lamp during review.
//   6. Saves the scene.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class Step9_PreviewLamps
{
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

    private const float ROW_Z = -2.6f;
    private const float ROW_Y = 0f;
    private const float START_X = 0.5f;
    private const float SPACING_X = 1.3f;
    private static readonly Vector3 START_ROTATION_EULER = new Vector3(270f, 90f, 90f);
    private const float TARGET_SCALE_FACTOR = 0.2f;

    static Step9_PreviewLamps()
    {
        EditorApplication.delayCall += AutoRunOnce;
    }

    private static void AutoRunOnce()
    {
        if (SessionState.GetBool("Step9_AutoPreviewExecuted", false))
            return;

        SessionState.SetBool("Step9_AutoPreviewExecuted", true);
        Run();
    }

    [MenuItem("VR Furniture Shop/Step 9 - Preview Lamps (Scale 0.2, Rot 270 90 90)")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step9] Exit Play Mode before running this script.");
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        }

        // --------------------------------------------------------
        // 1. Verify / Apply ModelImporter Scale Factor 0.2
        // --------------------------------------------------------
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
                else
                {
                    Debug.Log($"[Step9] {s_LampNames[i]} already has globalScale = {TARGET_SCALE_FACTOR}");
                }
            }
            else
            {
                Debug.LogWarning($"[Step9] Could not find ModelImporter for {path}");
            }
        }

        // --------------------------------------------------------
        // 2. Clean up previous Lamp_Previews root if present
        // --------------------------------------------------------
        GameObject existingRoot = GameObject.Find("Lamp_Previews");
        if (existingRoot != null)
        {
            Undo.DestroyObjectImmediate(existingRoot);
        }

        // --------------------------------------------------------
        // 3. Create root "Lamp_Previews"
        // --------------------------------------------------------
        GameObject root = new GameObject("Lamp_Previews");
        Undo.RegisterCreatedObjectUndo(root, "Create Lamp_Previews");
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;

        // --------------------------------------------------------
        // 4. Instantiate each lamp in preview row
        // --------------------------------------------------------
        for (int i = 0; i < s_LampPaths.Length; i++)
        {
            string path = s_LampPaths[i];
            string lampName = s_LampNames[i];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError($"[Step9] Failed to load prefab at {path}");
                continue;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
            if (instance == null)
            {
                instance = Object.Instantiate(prefab, root.transform);
            }

            instance.name = $"Preview_{lampName}";
            Vector3 pos = new Vector3(START_X + i * SPACING_X, ROW_Y, ROW_Z);
            instance.transform.position = pos;
            instance.transform.rotation = Quaternion.Euler(START_ROTATION_EULER);

            // Add identification label above each lamp for easy visual review
            GameObject labelObj = new GameObject($"{lampName}_Label");
            labelObj.transform.SetParent(instance.transform, false);
            labelObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            // Face player looking along +Z
            labelObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            TextMesh textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.text = $"{lampName}\n[Scale: 0.2 | Rot: (270, 90, 90)]";
            textMesh.fontSize = 24;
            textMesh.characterSize = 0.035f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;

            Debug.Log($"[Step9] Placed {lampName} at position ({pos.x:F1}, {pos.y:F1}, {pos.z:F1}) with rotation {START_ROTATION_EULER}");
        }

        // --------------------------------------------------------
        // 5. Save Scene
        // --------------------------------------------------------
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Step9] Lamp preview setup completed successfully!");
    }

    [MenuItem("VR Furniture Shop/Step 9 - Remove Lamp Previews")]
    public static void RemovePreviews()
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
}
