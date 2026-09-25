// ============================================================
// Step5_SetupCollidersAndCollision.cs
// Editor-only script.
//
// Run via:  VR Furniture Shop > Step 5 - Setup Colliders & Player Collision
//
// What it does:
//   1. Adds or updates Box Colliders on all 5 cupboard prefabs
//      (Cupboard_0 through Cupboard_4) in Assets/Prefabs/, sized and
//      centered to match each piece of furniture's exact mesh bounds.
//   2. Ensures the XR Origin (XR Rig) has a Character Controller
//      configured (Height = 1.8m, Radius = 0.2m, Center = 0.9m).
//   3. Attaches the XRHeadCollisionDriver component to XR Origin (XR Rig)
//      so physical / Device Simulator head movement is blocked by colliders
//      (cupboards and walls) rather than clipping through.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class Step5_SetupCollidersAndCollision
{
    private struct CupboardBoundsData
    {
        public string prefabPath;
        public Vector3 center;
        public Vector3 size;

        public CupboardBoundsData(string path, Vector3 c, Vector3 s)
        {
            prefabPath = path;
            center = c;
            size = s;
        }
    }

    private static readonly CupboardBoundsData[] s_Cupboards = new[]
    {
        new CupboardBoundsData(
            "Assets/Prefabs/Cupboard_0.prefab",
            new Vector3(0.07826f, -0.02079f, 0.52135f),
            new Vector3(1.77324f, 0.46791f, 1.10439f)
        ),
        new CupboardBoundsData(
            "Assets/Prefabs/Cupboard_1.prefab",
            new Vector3(0.08974f, -0.04306f, 0.52134f),
            new Vector3(1.04084f, 0.46630f, 1.10440f)
        ),
        new CupboardBoundsData(
            "Assets/Prefabs/Cupboard_2.prefab",
            new Vector3(0.06684f, -0.04186f, 0.52134f),
            new Vector3(1.79608f, 0.46792f, 1.10440f)
        ),
        new CupboardBoundsData(
            "Assets/Prefabs/Cupboard_3.prefab",
            new Vector3(0.23290f, 0.03548f, 0.52134f),
            new Vector3(0.75452f, 0.43246f, 1.10440f)
        ),
        new CupboardBoundsData(
            "Assets/Prefabs/Cupboard_4.prefab",
            new Vector3(0.23290f, -0.02452f, 0.52134f),
            new Vector3(0.75452f, 0.43182f, 1.10440f)
        )
    };

    [MenuItem("VR Furniture Shop/Step 5 - Setup Colliders & Player Collision")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step5] Exit Play Mode before running this script.");
            return;
        }

        // --------------------------------------------------------
        // 1. Setup BoxColliders on all 5 Cupboard Prefabs
        // --------------------------------------------------------
        foreach (var cupboard in s_Cupboards)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(cupboard.prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"[Step5] Failed to load prefab at {cupboard.prefabPath}");
                continue;
            }

            BoxCollider collider = prefabRoot.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = prefabRoot.AddComponent<BoxCollider>();
            }

            collider.center = cupboard.center;
            collider.size = cupboard.size;
            collider.isTrigger = false;

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, cupboard.prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            Debug.Log($"[Step5] Configured BoxCollider on {cupboard.prefabPath}: Center={cupboard.center}, Size={cupboard.size}");
        }

        AssetDatabase.SaveAssets();

        // --------------------------------------------------------
        // 2. Setup CharacterController & XRHeadCollisionDriver on XR Origin
        // --------------------------------------------------------
        GameObject xrRig = GameObject.Find("XR Origin (XR Rig)");
        if (xrRig == null)
        {
            Debug.LogError("[Step5] 'XR Origin (XR Rig)' not found in active scene! Run Step 1 first.");
            return;
        }

        Undo.RecordObject(xrRig, "Configure XR Origin Collision");

        // Ensure rig Y is constantly -0.5
        Vector3 rigPos = xrRig.transform.position;
        rigPos.y = -0.5f;
        xrRig.transform.position = rigPos;

        // Character Controller:
        // When rig is at Y = -0.5, the floor (world Y=0) is at local Y = +0.5.
        // Capsule center Y = 0.5 + 1.8 * 0.5 = 1.4f, so capsule bottom sits on the floor.
        CharacterController cc = xrRig.GetComponent<CharacterController>();
        if (cc == null)
        {
            cc = Undo.AddComponent<CharacterController>(xrRig);
        }

        cc.height = 1.8f;
        cc.radius = 0.2f;
        cc.center = new Vector3(0f, 1.4f, 0f);
        cc.skinWidth = 0.05f;
        cc.minMoveDistance = 0.001f;
        cc.stepOffset = 0.3f;

        // XRHeadCollisionDriver
        XRHeadCollisionDriver driver = xrRig.GetComponent<XRHeadCollisionDriver>();
        if (driver == null)
        {
            driver = Undo.AddComponent<XRHeadCollisionDriver>(xrRig);
        }
        driver.FixedY = -0.5f;

        // Update all TeleportAnchors in the scene to Y = -0.5f
        for (int i = 1; i <= 5; i++)
        {
            GameObject station = GameObject.Find($"Station_0{i}");
            if (station != null)
            {
                Transform anchor = station.transform.Find("TeleportAnchor");
                if (anchor != null)
                {
                    Undo.RecordObject(anchor, "Update TeleportAnchor height");
                    Vector3 ap = anchor.localPosition;
                    ap.y = -0.5f;
                    anchor.localPosition = ap;
                }
            }
        }

        EditorUtility.SetDirty(xrRig);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[Step5] XR Origin Y locked to -0.5, CharacterController & TeleportAnchors updated successfully.");
        Debug.Log("[Step5] Task 1 fix complete! You can now press Play to test collision against cupboard stations.");
    }
}
