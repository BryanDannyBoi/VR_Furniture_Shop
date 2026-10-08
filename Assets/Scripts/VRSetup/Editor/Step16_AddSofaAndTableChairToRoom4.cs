// ============================================================
// Step16_AddSofaAndTableChairToRoom4.cs
// Editor utility to instantiate and configure the two new furniture pieces:
//   - Sofa (Price: $400) -> Station_Sofa_01
//   - f_1 (Table & Chair Set, Price: $250) -> Station_TableChair_01
// In Room 4 (Bedroom & Living Suite) with full interactivity:
//   - Hover Lighting Highlight
//   - Select 360-degree Rotation
//   - Add to Cart Sign with synchronized state
//   - Teleportation Anchor
//
// Run via:  VR Furniture Shop > Step 16 - Add Sofa & Table-Chair Set to Room 4
// Also auto-runs once on compilation.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[InitializeOnLoad]
public static class Step16_AddSofaAndTableChairToRoom4
{
    private const string ROOM4_NAME = "Room_4";
    private const string SOFA_FBX = "Assets/Models/Furniture/sofa.fbx";
    private const string F1_FBX = "Assets/Models/Furniture/f_1.fbx";

    static Step16_AddSofaAndTableChairToRoom4()
    {
        EditorApplication.delayCall += AutoRunIfSampleScene;
    }

    private static void AutoRunIfSampleScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.isLoaded && scene.path.Contains("SampleScene.unity"))
        {
            if (GameObject.Find("Station_Sofa_01") == null || GameObject.Find("Station_TableChair_01") == null)
            {
                Run();
            }
        }
    }

    [MenuItem("VR Furniture Shop/Step 16 - Add Sofa & Table-Chair Set to Room 4")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.isLoaded) return;

        GameObject room4 = GameObject.Find(ROOM4_NAME);
        if (room4 == null)
        {
            Debug.LogError("[Step16] Room_4 not found in scene!");
            return;
        }

        Material pedestalMat = FindMaterial("Cylinder_01", "Pedestal");
        Camera mainCam = Camera.main;

        int addedCount = 0;

        // 1. Station_Sofa_01 ($400)
        if (GameObject.Find("Station_Sofa_01") == null)
        {
            GameObject stationSofa = new GameObject("Station_Sofa_01");
            Undo.RegisterCreatedObjectUndo(stationSofa, "Create Station_Sofa_01");
            stationSofa.transform.SetParent(room4.transform, false);
            stationSofa.transform.position = new Vector3(28.5f, 0f, -2.4f);
            stationSofa.transform.rotation = Quaternion.identity;

            // Pedestal
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(stationSofa.transform, false);
            pedestal.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            pedestal.transform.localScale = new Vector3(2.4f, 0.03f, 1.4f);
            if (pedestalMat != null) pedestal.GetComponent<MeshRenderer>().sharedMaterial = pedestalMat;

            // Model
            GameObject sofaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SOFA_FBX);
            if (sofaPrefab != null)
            {
                GameObject sofaObj = (GameObject)PrefabUtility.InstantiatePrefab(sofaPrefab, stationSofa.transform);
                if (sofaObj == null) sofaObj = Object.Instantiate(sofaPrefab, stationSofa.transform);

                sofaObj.name = "sofa";
                sofaObj.transform.localPosition = new Vector3(0f, 0.44f, 0f);
                sofaObj.transform.localRotation = Quaternion.identity;
                sofaObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                ConfigureModelInteractions(sofaObj);
            }

            // Teleport Anchor (player stands at Z = -1.1m facing the sofa)
            SetupTeleportAnchor(stationSofa, new Vector3(0f, -0.5f, 1.3f), Quaternion.Euler(0f, 180f, 0f));

            addedCount++;
        }

        // 2. Station_TableChair_01 ($250)
        if (GameObject.Find("Station_TableChair_01") == null)
        {
            GameObject stationDesk = new GameObject("Station_TableChair_01");
            Undo.RegisterCreatedObjectUndo(stationDesk, "Create Station_TableChair_01");
            stationDesk.transform.SetParent(room4.transform, false);
            stationDesk.transform.position = new Vector3(31.2f, 0f, -2.4f);
            stationDesk.transform.rotation = Quaternion.identity;

            // Pedestal
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(stationDesk.transform, false);
            pedestal.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            pedestal.transform.localScale = new Vector3(1.6f, 0.03f, 1.4f);
            if (pedestalMat != null) pedestal.GetComponent<MeshRenderer>().sharedMaterial = pedestalMat;

            // Model
            GameObject f1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(F1_FBX);
            if (f1Prefab != null)
            {
                GameObject f1Obj = (GameObject)PrefabUtility.InstantiatePrefab(f1Prefab, stationDesk.transform);
                if (f1Obj == null) f1Obj = Object.Instantiate(f1Prefab, stationDesk.transform);

                f1Obj.name = "f_1";
                // Offset vertices so chair & table sit upright on the pedestal
                f1Obj.transform.localPosition = new Vector3(-0.38f, -1.30f, -0.15f);
                f1Obj.transform.localRotation = Quaternion.identity;
                f1Obj.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);

                ConfigureModelInteractions(f1Obj);
            }

            // Teleport Anchor (player stands at Z = -1.1m facing the table & chair)
            SetupTeleportAnchor(stationDesk, new Vector3(0f, -0.5f, 1.3f), Quaternion.Euler(0f, 180f, 0f));

            addedCount++;
        }

        // Format all signs and wire up buttons
        CartSignFormatter.ReformatAllSigns(false);

        // Ensure door and doorway passage
        FixDoorwayPassage.ApplyDoorwayFixes(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"[Step16] Successfully added and configured {addedCount} new furniture stations (Sofa $400 & Table-Chair $250) in Room 4!");
    }

    private static void ConfigureModelInteractions(GameObject modelObj)
    {
        // 1. BoxCollider fitted to renderers
        Renderer[] renderers = modelObj.GetComponentsInChildren<Renderer>();
        BoxCollider col = modelObj.GetComponent<BoxCollider>();
        if (col == null) col = modelObj.AddComponent<BoxCollider>();

        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int r = 1; r < renderers.Length; r++) b.Encapsulate(renderers[r].bounds);

            col.center = modelObj.transform.InverseTransformPoint(b.center);
            Vector3 localSize = modelObj.transform.InverseTransformVector(b.size);
            col.size = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
        }

        // 2. XRSimpleInteractable on layer 1
        XRSimpleInteractable interactable = modelObj.GetComponent<XRSimpleInteractable>();
        if (interactable == null) interactable = modelObj.AddComponent<XRSimpleInteractable>();
        interactable.interactionLayers = 1;
        if (!interactable.colliders.Contains(col)) interactable.colliders.Add(col);

        // 3. CupboardInteraction (lighting glow + 360 rotation)
        if (modelObj.GetComponent<CupboardInteraction>() == null)
            modelObj.AddComponent<CupboardInteraction>();

        EditorUtility.SetDirty(modelObj);
    }

    private static void SetupTeleportAnchor(GameObject station, Vector3 localPos, Quaternion rot)
    {
        GameObject anchorGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        anchorGO.name = "TeleportAnchor";
        anchorGO.transform.SetParent(station.transform, false);
        anchorGO.transform.localPosition = localPos;
        anchorGO.transform.localRotation = rot;
        anchorGO.transform.localScale = new Vector3(1f, 0.01f, 1f);

        BoxCollider anchorCol = anchorGO.GetComponent<BoxCollider>();
        anchorCol.size = Vector3.one;
        anchorCol.center = Vector3.zero;

        TeleportationAnchor anchorComp = anchorGO.AddComponent<TeleportationAnchor>();
        anchorComp.matchOrientation = MatchOrientation.TargetUpAndForward;
        anchorComp.interactionLayers = 1 << 31;

        EditorUtility.SetDirty(anchorGO);
    }

    private static Material FindMaterial(params string[] targetNames)
    {
        foreach (string name in targetNames)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr != null && mr.sharedMaterial != null) return mr.sharedMaterial;
            }
        }
        return null;
    }
}
