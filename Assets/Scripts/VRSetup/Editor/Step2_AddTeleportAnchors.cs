// ============================================================
// Step2_AddTeleportAnchors.cs
// Editor-only one-shot script.
//
// Run via:  VR Furniture Shop > Step 2 - Add Teleport Anchors
//
// What it does:
//   For each of the 5 station GameObjects (Station_01 through Station_05)
//   it creates a child GameObject called "TeleportAnchor" that contains:
//     - A flat BoxCollider (1m x 0.01m x 1m) — the "landing pad" the
//       teleport ray must hit.
//     - A TeleportationAnchor component — snaps the player to the exact
//       position and facing direction defined by this object's transform.
//
//   The anchor is placed at local offset (0, 0, +1.5) from the station —
//   1.5 m in front of the furniture (toward +Z) — and rotated 180 degrees
//   on Y so the player lands facing the cupboard (facing -Z).
//
//   Teleport orientation: TargetUpAndForward — the player will always face
//   the same direction as the anchor, regardless of which direction they
//   aimed the ray from.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public static class Step2_AddTeleportAnchors
{
    // --------------------------------------------------------
    // Station names to process — must match GameObject names in scene exactly.
    // --------------------------------------------------------
    private static readonly string[] s_StationNames =
    {
        "Station_01",
        "Station_02",
        "Station_03",
        "Station_04",
        "Station_05",
    };

    // Local offset from the station origin to where the player lands.
    // (0, 0, -1.5) = 1.5 metres in front of the cupboard doors along -Z.
    // The cupboards face -Z, so the player needs to stand on the -Z side.
    private const float ANCHOR_Z_OFFSET = -1.5f;

    // Y-rotation of the anchor: 0 degrees means the player faces +Z
    // — back toward the station origin where the cupboard sits.
    private const float ANCHOR_Y_ROT = 0f;

    // --------------------------------------------------------
    // Menu entry
    // --------------------------------------------------------
    [MenuItem("VR Furniture Shop/Step 2 - Add Teleport Anchors")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step2] Exit Play Mode before running this script.");
            return;
        }

        int created = 0;

        foreach (string stationName in s_StationNames)
        {
            // Find the station in the scene.
            GameObject station = GameObject.Find(stationName);
            if (station == null)
            {
                Debug.LogError("[Step2] Could not find GameObject '" + stationName
                    + "' in the scene. Is it named exactly as listed above?");
                continue;
            }

            // Skip if an anchor already exists (safe to re-run the script).
            if (station.transform.Find("TeleportAnchor") != null)
            {
                Debug.LogWarning("[Step2] '" + stationName + "' already has a TeleportAnchor child. Skipping.");
                continue;
            }

            // ------------------------------------------------
            // Create the anchor GameObject as a child of the station.
            // ------------------------------------------------
            GameObject anchorGO = new GameObject("TeleportAnchor");
            Undo.RegisterCreatedObjectUndo(anchorGO, "Create TeleportAnchor for " + stationName);

            // Parent it to the station so it moves with it.
            anchorGO.transform.SetParent(station.transform, worldPositionStays: false);

            // Position: in front of the cupboard doors along local -Z.
            anchorGO.transform.localPosition = new Vector3(0f, 0f, ANCHOR_Z_OFFSET);

            // Rotation: face +Z so the player looks toward the cupboard doors.
            anchorGO.transform.localRotation = Quaternion.Euler(0f, ANCHOR_Y_ROT, 0f);

            // ------------------------------------------------
            // Add a flat BoxCollider — this is the surface the teleport
            // ray hits to detect this anchor.
            //   Width (X): 1 m, Height (Y): 0.01 m (almost flat), Depth (Z): 1 m
            // ------------------------------------------------
            BoxCollider col = anchorGO.AddComponent<BoxCollider>();
            col.size   = new Vector3(1f, 0.01f, 1f);
            col.center = Vector3.zero;

            // ------------------------------------------------
            // Add the TeleportationAnchor component.
            //
            // matchOrientation = TargetUpAndForward:
            //   When the player teleports here, their character will be
            //   rotated to match BOTH the up axis and the forward axis of
            //   this anchor — so they always land facing the furniture.
            // ------------------------------------------------
            TeleportationAnchor anchor = anchorGO.AddComponent<TeleportationAnchor>();
            anchor.matchOrientation = MatchOrientation.TargetUpAndForward;

            Debug.Log("[Step2] TeleportAnchor created for " + stationName
                + "  world pos ~" + anchorGO.transform.position);
            created++;
        }

        // Mark scene dirty so Unity prompts to save.
        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Step2] Done — " + created + " anchor(s) created. Save the scene (Ctrl+S).");
    }
}
