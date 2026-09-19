// ============================================================
// Step1_SetupXROrigin.cs
// Editor-only one-shot script.
//
// Run via:  VR Furniture Shop > Step 1 - Setup XR Origin
//
// What it does (in order):
//   1. Disables the existing "Main Camera" GameObject (does NOT delete it).
//   2. Instantiates the XR Origin (XR Rig) prefab from the Starter Assets
//      sample at position (-1, 0, 2) with Y-rotation 90 degrees (facing +X
//      toward the station row).
//   3. Disables the "Move" and "Turn" child GameObjects inside the rig's
//      Locomotion hierarchy so that only teleport locomotion is active.
//
// After running, you can delete this file - it only shows in menus
// while in the Editor and has no runtime cost.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class Step1_SetupXROrigin
{
    // ---------- menu entry ----------
    [MenuItem("VR Furniture Shop/Step 1 - Setup XR Origin")]
    public static void Run()
    {
        // Guard: only run when not in play mode.
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step1] Exit Play Mode before running this script.");
            return;
        }

        // --------------------------------------------------------
        // 1. Disable the existing Main Camera
        // --------------------------------------------------------
        GameObject mainCam = GameObject.Find("Main Camera");
        if (mainCam != null)
        {
            Undo.RecordObject(mainCam, "Disable Main Camera");
            mainCam.SetActive(false);
            Debug.Log("[Step1] Main Camera disabled (not deleted).");
        }
        else
        {
            Debug.LogWarning("[Step1] 'Main Camera' not found in scene (may already be disabled). Continuing.");
        }

        // --------------------------------------------------------
        // 2. Instantiate the XR Origin (XR Rig) prefab
        // --------------------------------------------------------

        // Asset path inside the project.
        const string PREFAB_PATH =
            "Assets/Samples/XR Interaction Toolkit/3.3.2/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
        if (prefab == null)
        {
            Debug.LogError("[Step1] Could not load XR Origin prefab from:\n" + PREFAB_PATH
                + "\nMake sure the Starter Assets sample is imported in Package Manager.");
            return;
        }

        // Spawn at (-1, 0, -2), rotated 90 degrees around Y so it faces +X (toward Station_05).
        // Z = -2 puts the player in front of the cupboard doors (cupboards face -Z).
        Vector3    spawnPos = new Vector3(-1f, 0f, -2f);
        Quaternion spawnRot = Quaternion.Euler(0f, 90f, 0f);

        // PrefabUtility.InstantiatePrefab keeps the prefab connection (shown in blue
        // in the Hierarchy) and is undo-able.
        GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(rig, "Instantiate XR Origin");
        rig.transform.SetPositionAndRotation(spawnPos, spawnRot);
        rig.name = "XR Origin (XR Rig)";

        Debug.Log("[Step1] XR Origin instantiated at " + spawnPos + ", Y-rotation 90 degrees.");

        // --------------------------------------------------------
        // 3. Disable Move and Turn child GameObjects inside Locomotion
        //    so only teleport locomotion remains active.
        //
        //    Prefab hierarchy (relevant nodes):
        //      XR Origin (XR Rig)
        //        Locomotion
        //          Move   <- disable this (continuous movement)
        //          Turn   <- disable this (snap/smooth turn)
        //          (Teleport and Climb nodes are left enabled)
        // --------------------------------------------------------
        DisableLocomotionChild(rig, "Locomotion/Move", "Continuous move");
        DisableLocomotionChild(rig, "Locomotion/Turn", "Snap/smooth turn");

        // --------------------------------------------------------
        // Mark scene dirty so Unity knows it needs saving.
        // --------------------------------------------------------
        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Step1] Done! Review the Hierarchy, then save the scene (Ctrl+S).");
    }

    // --------------------------------------------------------
    // Helper: find a child by relative path and disable it.
    // --------------------------------------------------------
    private static void DisableLocomotionChild(GameObject root, string relativePath, string label)
    {
        Transform child = root.transform.Find(relativePath);
        if (child != null)
        {
            Undo.RecordObject(child.gameObject, "Disable " + label);
            child.gameObject.SetActive(false);
            Debug.Log("[Step1] '" + relativePath + "' (" + label + ") disabled.");
        }
        else
        {
            Debug.LogWarning("[Step1] Could not find child at path '" + relativePath + "' -- "
                + "the prefab hierarchy may have changed. Check manually in Inspector.");
        }
    }
}
