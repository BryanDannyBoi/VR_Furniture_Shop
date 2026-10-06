// ============================================================
// FixDoorwayPassage.cs
// Resolves physical and teleportation blocking at doorways:
// 1. Shrinks player CharacterController from oversized 1.0m width / 2.0m height
//    to natural human proportions (0.56m diameter, 1.75m height).
// 2. Adds TeleportationArea to Room 1 Floor and Room 2 Floor so the player
//    can teleport anywhere across the floors and through doorways.
// 3. Adds Doorway Teleport Anchors in the door thresholds.
// 4. Raises door lintels for 1.90m+ vertical clearance.
// 5. Ensures solid Back wall is inactive.
//
// Run via:  VR Furniture Shop > Fix Doorway Passage & Collision
// Also auto-configures on script reload.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[InitializeOnLoad]
public static class FixDoorwayPassage
{
    static FixDoorwayPassage()
    {
        EditorApplication.delayCall += AutoFixDoorway;
    }

    private static void AutoFixDoorway()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.isLoaded && scene.path.Contains("SampleScene.unity"))
        {
            ApplyDoorwayFixes(false);
        }
    }

    [MenuItem("VR Furniture Shop/Fix Doorway Passage & Collision")]
    public static void ManualFix()
    {
        ApplyDoorwayFixes(true);
    }

    public static void ApplyDoorwayFixes(bool logSummary)
    {
        if (EditorApplication.isPlaying) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.isLoaded) return;

        int fixCount = 0;

        // --------------------------------------------------------
        // 1. Adjust CharacterController dimensions on XR Origin
        // --------------------------------------------------------
        GameObject xrRig = GameObject.Find("XR Origin (XR Rig)");
        if (xrRig != null)
        {
            CharacterController cc = xrRig.GetComponent<CharacterController>();
            if (cc != null)
            {
                Undo.RecordObject(cc, "Adjust CharacterController size");
                cc.radius = 0.28f; // 0.56m diameter (was 1.0m - too wide for doorframes)
                cc.height = 1.75f; // 1.75m height (was 2.0m - taller than doorframes)
                cc.center = new Vector3(0f, 0.875f, 0f);
                cc.stepOffset = 0.35f;
                EditorUtility.SetDirty(cc);
                fixCount++;
            }
        }

        // --------------------------------------------------------
        // 2. Enable TeleportationArea on Room 1 Floor and Room 2 Floor
        // --------------------------------------------------------
        GameObject floor1 = GameObject.Find("Floor");
        if (floor1 != null)
        {
            TeleportationArea area = floor1.GetComponent<TeleportationArea>();
            if (area == null) area = Undo.AddComponent<TeleportationArea>(floor1);
            area.interactionLayers = 1 << 31; // Teleport layer
            EditorUtility.SetDirty(floor1);
            fixCount++;
        }

        GameObject floor2 = GameObject.Find("Room_2_Floor");
        if (floor2 != null)
        {
            TeleportationArea area = floor2.GetComponent<TeleportationArea>();
            if (area == null) area = Undo.AddComponent<TeleportationArea>(floor2);
            area.interactionLayers = 1 << 31; // Teleport layer
            EditorUtility.SetDirty(floor2);
            fixCount++;
        }

        // --------------------------------------------------------
        // 3. Add Doorway Teleportation Anchors in thresholds
        // --------------------------------------------------------
        SetupDoorwayAnchor("Doorway_Anchor_Room1_To_Room2", new Vector3(10.11f, -0.5f, -1.50f), Quaternion.Euler(0f, 90f, 0f));
        SetupDoorwayAnchor("Doorway_Anchor_Room2_To_Room3", new Vector3(18.11f, -0.5f, -1.50f), Quaternion.Euler(0f, 90f, 0f));

        // --------------------------------------------------------
        // 4. Ensure solid Back wall is inactive
        // --------------------------------------------------------
        GameObject backWall = GameObject.Find("Back wall");
        if (backWall != null && backWall.activeSelf)
        {
            Undo.RecordObject(backWall, "Disable solid Back wall");
            backWall.SetActive(false);
            EditorUtility.SetDirty(backWall);
            fixCount++;
        }

        // --------------------------------------------------------
        // 5. Raise doorway lintels for generous 1.90m+ vertical clearance
        // --------------------------------------------------------
        GameObject lintel1 = GameObject.Find("BackWall_Segment_Lintel");
        if (lintel1 != null)
        {
            Undo.RecordObject(lintel1.transform, "Raise Lintel 1");
            lintel1.transform.position = new Vector3(10.11f, 1.95f, -1.50f);
            lintel1.transform.localScale = new Vector3(0.20f, 0.10f, 0.88f);
            EditorUtility.SetDirty(lintel1);
            fixCount++;
        }

        GameObject lintel2 = GameObject.Find("EastWall_Segment_Lintel");
        if (lintel2 != null)
        {
            Undo.RecordObject(lintel2.transform, "Raise Lintel 2");
            lintel2.transform.position = new Vector3(18.11f, 1.95f, -1.50f);
            lintel2.transform.localScale = new Vector3(0.20f, 0.10f, 0.88f);
            EditorUtility.SetDirty(lintel2);
            fixCount++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        if (logSummary)
        {
            Debug.Log($"[FixDoorwayPassage] Successfully applied doorway collision & teleportation fixes ({fixCount} updates)!");
        }
    }

    private static void SetupDoorwayAnchor(string anchorName, Vector3 worldPos, Quaternion rot)
    {
        GameObject anchorGO = GameObject.Find(anchorName);
        if (anchorGO == null)
        {
            anchorGO = new GameObject(anchorName);
            Undo.RegisterCreatedObjectUndo(anchorGO, "Create " + anchorName);
        }

        anchorGO.transform.position = worldPos;
        anchorGO.transform.rotation = rot;

        BoxCollider col = anchorGO.GetComponent<BoxCollider>();
        if (col == null) col = anchorGO.AddComponent<BoxCollider>();
        col.size = new Vector3(1.2f, 0.02f, 1.0f);
        col.center = Vector3.zero;

        TeleportationAnchor anchor = anchorGO.GetComponent<TeleportationAnchor>();
        if (anchor == null) anchor = anchorGO.AddComponent<TeleportationAnchor>();
        anchor.matchOrientation = MatchOrientation.TargetUpAndForward;
        anchor.interactionLayers = 1 << 31; // Teleport layer

        EditorUtility.SetDirty(anchorGO);
    }
}
