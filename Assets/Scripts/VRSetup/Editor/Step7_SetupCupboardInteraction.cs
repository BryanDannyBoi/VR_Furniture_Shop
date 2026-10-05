// ============================================================
// Step7_SetupCupboardInteraction.cs
// Editor-only script.
//
// Run via:  VR Furniture Shop > Step 7 - Setup Cupboard Interaction
//
// What it does:
//   1. Adds XRSimpleInteractable and CupboardInteraction components to
//      all 5 cupboard prefabs (Cupboard_0 through Cupboard_4) in Assets/Prefabs/.
//      - XRSimpleInteractable is configured with interactionLayers = Default (1),
//        linking its interaction collider to the existing BoxCollider.
//      - CupboardInteraction manages hover material tint/glow and trigger-toggle
//        auto-rotation (30 deg/sec around the world Y axis).
//   2. Updates the 5 TeleportationAnchor components in the active scene to
//      use interactionLayers = Teleport (1 << 31) so teleportation and pointer
//      rays are cleanly isolated without cross-talk.
//   3. Saves all modified prefabs and the active scene.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public static class Step7_SetupCupboardInteraction
{
    private static readonly string[] s_CupboardPrefabPaths = new[]
    {
        "Assets/Prefabs/Cupboard_0.prefab",
        "Assets/Prefabs/Cupboard_1.prefab",
        "Assets/Prefabs/Cupboard_2.prefab",
        "Assets/Prefabs/Cupboard_3.prefab",
        "Assets/Prefabs/Cupboard_4.prefab"
    };

    private const int INTERACTION_LAYER_DEFAULT = 1;         // Bit 0 = Default
    private const int INTERACTION_LAYER_TELEPORT = 1 << 31;   // Bit 31 = Teleport

    [MenuItem("VR Furniture Shop/Step 7 - Setup Cupboard Interaction")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step7] Exit Play Mode before running this script.");
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        }

        // --------------------------------------------------------
        // 1. Configure XRSimpleInteractable & CupboardInteraction on all 5 Cupboard prefabs
        // --------------------------------------------------------
        foreach (string prefabPath in s_CupboardPrefabPaths)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"[Step7] Failed to load prefab at {prefabPath}");
                continue;
            }

            BoxCollider boxCollider = prefabRoot.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                Debug.LogWarning($"[Step7] {prefabPath} does not have a BoxCollider! Adding one...");
                boxCollider = prefabRoot.AddComponent<BoxCollider>();
            }

            // Add or configure XRSimpleInteractable
            XRSimpleInteractable interactable = prefabRoot.GetComponent<XRSimpleInteractable>();
            if (interactable == null)
            {
                interactable = prefabRoot.AddComponent<XRSimpleInteractable>();
            }

            // Restrict interaction layer to Default only (ignores Teleport ray)
            interactable.interactionLayers = INTERACTION_LAYER_DEFAULT;

            // Associate the BoxCollider
            if (!interactable.colliders.Contains(boxCollider))
            {
                interactable.colliders.Add(boxCollider);
            }

            // Add or configure CupboardInteraction
            CupboardInteraction cupboardInteraction = prefabRoot.GetComponent<CupboardInteraction>();
            if (cupboardInteraction == null)
            {
                cupboardInteraction = prefabRoot.AddComponent<CupboardInteraction>();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            Debug.Log($"[Step7] Successfully configured XRSimpleInteractable and CupboardInteraction on {prefabPath}");
        }

        AssetDatabase.SaveAssets();

        // --------------------------------------------------------
        // 2. Update TeleportationAnchors in the active scene to the Teleport layer
        // --------------------------------------------------------
        int updatedAnchors = 0;
        for (int i = 1; i <= 5; i++)
        {
            GameObject station = GameObject.Find($"Station_0{i}");
            if (station != null)
            {
                Transform anchorT = station.transform.Find("TeleportAnchor");
                if (anchorT != null)
                {
                    TeleportationAnchor anchor = anchorT.GetComponent<TeleportationAnchor>();
                    if (anchor != null)
                    {
                        Undo.RecordObject(anchor, "Set TeleportationAnchor Interaction Layer");
                        anchor.interactionLayers = INTERACTION_LAYER_TELEPORT;
                        EditorUtility.SetDirty(anchor);
                        updatedAnchors++;
                    }
                }
            }
        }

        // Update Step2 script as well for future consistency
        Debug.Log($"[Step7] Updated {updatedAnchors} TeleportationAnchor(s) in active scene to Teleport interaction layer.");

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[Step7] Setup complete! All 5 cupboard prefabs now have XRSimpleInteractable + CupboardInteraction, and TeleportationAnchors are isolated.");
    }
}
