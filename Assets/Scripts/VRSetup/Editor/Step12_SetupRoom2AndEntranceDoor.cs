// ============================================================
// Step12_SetupRoom2AndEntranceDoor.cs
// Editor-only script.
//
// Run via:  VR Furniture Shop > Step 12 - Setup Room 2 & Entrance Door (Option A)
//
// What it does:
//   1. Part 1 - Two Small Changes:
//      - Creates an InteractiveDoor at the Room 1 Entrance doorway
//        (X = -1.72, Z = -3.48) with exterior approach trigger zone and
//        world-space "Open [Ray Select]" prompt.
//      - Builds an outside entrance porch/patio floor slab with TeleportAnchor.
//      - Sets the player's starting spawn point to OUTSIDE Room 1
//        at (-3.2, -0.5, -3.48) facing +X toward the Entrance Sign and closed door.
//   2. Part 2 - Room 2 (Mini Fridges) with Option A:
//      - Builds the 8m x 8m Room 2 shell connected to Room 1 from X = 10.11 to 18.11.
//      - Installs East wall doorway (X = 18.11, Z = -1.5) with InteractiveDoor leading to Room 3.
//      - Builds the 4 Mini Fridge stations inside Room 2 along Z = 0 facing -Z:
//          * Classic Mini ($250) at X = 12.0
//          * Standard Cooler ($300) at X = 13.5
//          * Freeze Cool ($320) at X = 15.0
//          * Ultra Cold ($400) at X = 16.5
//      - Each station has: Cylinder pedestal (0.5, 0.03, 0.5), mini fridge model with BoxCollider,
//        TeleportAnchor at local (0, -0.5, -1.5), Label_Canvas with strictly "Name - $Price",
//        and Add-to-Cart sign integrated with ShopCartManager.
//      - Cleans up mini fridge preview instances from NewBatch_Previews (leaves Cot, Chair, DressingTable).
//   3. Saves the scene.
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.UI;

[InitializeOnLoad]
public static class Step12_SetupRoom2AndEntranceDoor
{
    private const string ROOM1_NAME = "Room_1_Cupboards";
    private const string ROOM2_NAME = "Room_2_MiniFridges";
    private const string ENTRANCE_DOOR_NAME = "Entrance_Door_Room1";
    private const string PATIO_NAME = "Entrance_Patio";
    private const string SIGNBOARD_MAT_PATH = "Assets/Materials/SignBoard_Mat.mat";

    static Step12_SetupRoom2AndEntranceDoor()
    {
        EditorApplication.delayCall += AutoRunOnce;
    }

    private static void AutoRunOnce()
    {
        if (SessionState.GetBool("Step12_AutoRun_Executed", false))
            return;

        SessionState.SetBool("Step12_AutoRun_Executed", true);
        Run();
    }

    private struct MiniFridgeData
    {
        public string stationName;
        public string displayName;
        public float price;
        public float posX;
        public string modelPath;

        public MiniFridgeData(string stName, string name, float itemPrice, float x, string path)
        {
            stationName = stName;
            displayName = name;
            price = itemPrice;
            posX = x;
            modelPath = path;
        }
    }

    private static readonly MiniFridgeData[] s_MiniFridges = new[]
    {
        new MiniFridgeData("Station_MF_02", "Classic Mini", 250f, 12.0f, "Assets/Models/Furniture/MiniFridge_v2_cube_white.fbx"),
        new MiniFridgeData("Station_MF_03", "Standard Cooler", 300f, 13.5f, "Assets/Models/Furniture/MiniFridge_v3_bar_silver.fbx"),
        new MiniFridgeData("Station_MF_04", "Freeze Cool", 320f, 15.0f, "Assets/Models/Furniture/MiniFridge_v4_retro_mint.fbx"),
        new MiniFridgeData("Station_MF_05", "Ultra Cold", 400f, 16.5f, "Assets/Models/Furniture/MiniFridge_v5_wine_cooler.fbx")
    };

    [MenuItem("VR Furniture Shop/Step 12 - Setup Room 2 & Entrance Door (Option A)")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step12] Exit Play Mode before running this script.");
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        }

        // Shared materials
        Material doorMat = AssetDatabase.LoadAssetAtPath<Material>(SIGNBOARD_MAT_PATH);
        Material wallMat = null;
        GameObject existingWall = GameObject.Find("Left wall");
        if (existingWall == null) existingWall = GameObject.Find("Title wall");
        if (existingWall != null)
        {
            MeshRenderer mr = existingWall.GetComponent<MeshRenderer>();
            if (mr != null) wallMat = mr.sharedMaterial;
        }

        Material floorMat = null;
        GameObject floorObj = GameObject.Find("Floor");
        if (floorObj != null)
        {
            MeshRenderer mr = floorObj.GetComponent<MeshRenderer>();
            if (mr != null) floorMat = mr.sharedMaterial;
        }

        Material pedestalMat = null;
        GameObject cyl01 = GameObject.Find("Cylinder_01");
        if (cyl01 != null)
        {
            MeshRenderer mr = cyl01.GetComponent<MeshRenderer>();
            if (mr != null) pedestalMat = mr.sharedMaterial;
        }

        Camera mainCam = Camera.main;

        // ========================================================
        // 1. Setup Room 1 Entrance Patio & Player Outside Starting Position
        // ========================================================
        SetupEntrancePatioAndPlayer(floorMat);

        // ========================================================
        // 2. Setup Room 1 Interactive Entrance Door
        // ========================================================
        SetupRoom1EntranceDoor(doorMat);

        // ========================================================
        // 3. Build Room 2 Shell (8m x 8m, X = 10.11 to 18.11)
        // ========================================================
        GameObject room2Root = SetupRoom2Shell(wallMat, floorMat, doorMat);

        // ========================================================
        // 4. Build Room 2 Mini Fridge Stations (Option A)
        // ========================================================
        SetupMiniFridgeStations(room2Root, pedestalMat, mainCam);

        // ========================================================
        // 5. Clean up Mini Fridge preview instances from NewBatch_Previews
        // ========================================================
        CleanMiniFridgePreviews();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Step12] Setup Room 2 (Mini Fridges Option A) and Entrance Door completed successfully!");
    }

    private static void SetupEntrancePatioAndPlayer(Material floorMat)
    {
        // 1. Outside Entrance Patio
        GameObject patio = GameObject.Find(PATIO_NAME);
        if (patio == null)
        {
            patio = GameObject.CreatePrimitive(PrimitiveType.Cube);
            patio.name = PATIO_NAME;
            Undo.RegisterCreatedObjectUndo(patio, "Create " + PATIO_NAME);
            patio.transform.position = new Vector3(-3.5f, -0.01f, -3.48f);
            patio.transform.localScale = new Vector3(3.6f, 0.02f, 2.6f);
            if (floorMat != null)
                patio.GetComponent<MeshRenderer>().sharedMaterial = floorMat;
        }

        // Outside TeleportAnchor on patio
        Transform outsideAnchorT = patio.transform.Find("Outside_Entrance_Anchor");
        if (outsideAnchorT == null)
        {
            GameObject anchorGO = new GameObject("Outside_Entrance_Anchor");
            Undo.RegisterCreatedObjectUndo(anchorGO, "Create Outside_Entrance_Anchor");
            anchorGO.transform.SetParent(patio.transform, false);
            // Local offset: patio center is (-3.5, -0.01, -3.48). Anchor at (-3.2, -0.5, -3.48)
            anchorGO.transform.localPosition = new Vector3(0.3f / 3.6f, -0.5f / 0.02f, 0f);
            anchorGO.transform.localScale = new Vector3(1f / 3.6f, 1f / 0.02f, 1f / 2.6f);
            anchorGO.transform.localRotation = Quaternion.Euler(0f, 90f, 0f); // Facing +X

            BoxCollider col = anchorGO.AddComponent<BoxCollider>();
            col.size = new Vector3(1f, 0.01f, 1f);
            col.center = Vector3.zero;

            TeleportationAnchor anchor = anchorGO.AddComponent<TeleportationAnchor>();
            anchor.matchOrientation = MatchOrientation.TargetUpAndForward;
            anchor.interactionLayers = 1 << 31;
        }

        // 2. Set player starting position to outside Room 1
        GameObject xrRig = GameObject.Find("XR Origin (XR Rig)");
        if (xrRig != null)
        {
            Undo.RecordObject(xrRig.transform, "Set player start outside Room 1");
            xrRig.transform.position = new Vector3(-3.2f, -0.5f, -3.48f);
            xrRig.transform.rotation = Quaternion.Euler(0f, 90f, 0f); // Facing +X towards entrance
            Debug.Log("[Step12] Set XR Origin starting position to outside Room 1: (-3.2, -0.5, -3.48), Y-rot: 90.");
        }
    }

    private static void SetupRoom1EntranceDoor(Material doorMat)
    {
        GameObject room1Root = GameObject.Find(ROOM1_NAME);
        GameObject doorRoot = GameObject.Find(ENTRANCE_DOOR_NAME);
        if (doorRoot == null)
        {
            doorRoot = new GameObject(ENTRANCE_DOOR_NAME);
            Undo.RegisterCreatedObjectUndo(doorRoot, "Create " + ENTRANCE_DOOR_NAME);
            if (room1Root != null) doorRoot.transform.SetParent(room1Root.transform, false);

            // Doorway gap between Right wall (Z = -3.93) and Title wall (Z = -3.03) at X = -1.72
            // Hinge at South jamb: X = -1.72, Y = 0, Z = -3.93
            doorRoot.transform.position = new Vector3(-1.72f, 0f, -3.93f);
            doorRoot.transform.rotation = Quaternion.identity;

            // Hinge child
            GameObject hingeObj = new GameObject("Hinge");
            hingeObj.transform.SetParent(doorRoot.transform, false);
            hingeObj.transform.localPosition = Vector3.zero;
            hingeObj.transform.localRotation = Quaternion.identity;

            // Door Panel (width 0.88m along Z, height 1.80m, thickness 0.05m)
            GameObject doorPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorPanel.name = "Door_Panel";
            doorPanel.transform.SetParent(hingeObj.transform, false);
            doorPanel.transform.localPosition = new Vector3(0f, 0.90f, 0.44f);
            doorPanel.transform.localScale = new Vector3(0.05f, 1.80f, 0.88f);
            if (doorMat != null)
                doorPanel.GetComponent<MeshRenderer>().sharedMaterial = doorMat;

            // XR Ray interactable
            XRSimpleInteractable interactable = doorPanel.AddComponent<XRSimpleInteractable>();

            // UI Prompt Canvas on outside face (-X) facing outside approach (-X)
            GameObject canvasObj = new GameObject("Prompt_Canvas");
            canvasObj.transform.SetParent(doorPanel.transform, false);
            canvasObj.transform.localPosition = new Vector3(-0.035f, 0.20f, 0f); // eye level outer face
            canvasObj.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            RectTransform rt = canvasObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(70f, 25f);

            GameObject textObj = new GameObject("Prompt_Text");
            textObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Open [Ray Select]";
            tmp.fontSize = 11;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            // Trigger zone positioned on outside approach in front of the door
            GameObject triggerObj = new GameObject("Trigger_Zone");
            triggerObj.transform.SetParent(doorRoot.transform, false);
            // Local pos relative to doorRoot: -0.8m along X (outside), Y=1.0, Z=0.45m
            triggerObj.transform.localPosition = new Vector3(-0.80f, 1.0f, 0.45f);
            BoxCollider trigCol = triggerObj.AddComponent<BoxCollider>();
            trigCol.isTrigger = true;
            trigCol.size = new Vector3(1.6f, 2.0f, 1.4f);

            // InteractiveDoor script
            InteractiveDoor doorComp = triggerObj.AddComponent<InteractiveDoor>();
            SerializedObject so = new SerializedObject(doorComp);
            so.FindProperty("m_HingeTransform").objectReferenceValue = hingeObj.transform;
            so.FindProperty("m_OpenAngle").floatValue = 90f; // swings open into Room 1 (+X)
            so.FindProperty("m_OpenDuration").floatValue = 1.0f;
            so.FindProperty("m_PromptCanvas").objectReferenceValue = canvas;
            so.FindProperty("m_PromptText").objectReferenceValue = tmp;
            so.FindProperty("m_PromptMessage").stringValue = "Open [Ray Select]";
            so.FindProperty("m_Interactable").objectReferenceValue = interactable;
            so.ApplyModifiedProperties();

            Debug.Log("[Step12] Installed InteractiveDoor 'Entrance_Door_Room1' at shop entrance.");
        }
    }

    private static GameObject SetupRoom2Shell(Material wallMat, Material floorMat, Material doorMat)
    {
        GameObject room2 = GameObject.Find(ROOM2_NAME);
        if (room2 == null)
        {
            room2 = new GameObject(ROOM2_NAME);
            Undo.RegisterCreatedObjectUndo(room2, "Create " + ROOM2_NAME);
            room2.transform.position = Vector3.zero;
            room2.transform.rotation = Quaternion.identity;
        }

        // South wall: X = 14.11, Y = 1.0, Z = -3.92, scale (8, 2, 0.2)
        Transform southWallT = room2.transform.Find("Room_2_South_Wall");
        if (southWallT == null)
        {
            GameObject sw = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sw.name = "Room_2_South_Wall";
            sw.transform.SetParent(room2.transform, false);
            sw.transform.position = new Vector3(14.11f, 1.0f, -3.92f);
            sw.transform.localScale = new Vector3(8.0f, 2.0f, 0.2f);
            if (wallMat != null) sw.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
        }

        // North wall: X = 14.11, Y = 1.0, Z = 3.92, scale (8, 2, 0.2)
        Transform northWallT = room2.transform.Find("Room_2_North_Wall");
        if (northWallT == null)
        {
            GameObject nw = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nw.name = "Room_2_North_Wall";
            nw.transform.SetParent(room2.transform, false);
            nw.transform.position = new Vector3(14.11f, 1.0f, 3.92f);
            nw.transform.localScale = new Vector3(8.0f, 2.0f, 0.2f);
            if (wallMat != null) nw.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
        }

        // Roof: X = 14.11, Y = 2.07, Z = 0, scale (8, 0.1, 8)
        Transform roofT = room2.transform.Find("Room_2_Roof");
        if (roofT == null)
        {
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Room_2_Roof";
            roof.transform.SetParent(room2.transform, false);
            roof.transform.position = new Vector3(14.11f, 2.07f, 0f);
            roof.transform.localScale = new Vector3(8.0f, 0.1f, 8.0f);
            if (wallMat != null) roof.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
        }

        // Floor: X = 14.11, Y = 0, Z = 0, scale (8, 0.02, 8)
        Transform floorT = room2.transform.Find("Room_2_Floor");
        if (floorT == null)
        {
            GameObject fl = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fl.name = "Room_2_Floor";
            fl.transform.SetParent(room2.transform, false);
            fl.transform.position = new Vector3(14.11f, 0f, 0f);
            fl.transform.localScale = new Vector3(8.0f, 0.02f, 8.0f);
            if (floorMat != null) fl.GetComponent<MeshRenderer>().sharedMaterial = floorMat;
        }

        // East wall with doorway at Z = -1.5m, X = 18.11m (opens into future Room 3: Lamps)
        Transform eastWallGroupT = room2.transform.Find("Room_2_EastWall_Doorway");
        if (eastWallGroupT == null)
        {
            GameObject eastWallGroup = new GameObject("Room_2_EastWall_Doorway");
            eastWallGroup.transform.SetParent(room2.transform, false);

            // South segment
            GameObject segSouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segSouth.name = "EastWall_Segment_South";
            segSouth.transform.SetParent(eastWallGroup.transform, false);
            segSouth.transform.position = new Vector3(18.11f, 1.0f, -2.995f);
            segSouth.transform.localScale = new Vector3(0.20f, 2.0f, 2.09f);
            if (wallMat != null) segSouth.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // North segment
            GameObject segNorth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segNorth.name = "EastWall_Segment_North";
            segNorth.transform.SetParent(eastWallGroup.transform, false);
            segNorth.transform.position = new Vector3(18.11f, 1.0f, 1.455f);
            segNorth.transform.localScale = new Vector3(0.20f, 2.0f, 5.01f);
            if (wallMat != null) segNorth.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Lintel segment
            GameObject segLintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segLintel.name = "EastWall_Segment_Lintel";
            segLintel.transform.SetParent(eastWallGroup.transform, false);
            segLintel.transform.position = new Vector3(18.11f, 1.925f, -1.50f);
            segLintel.transform.localScale = new Vector3(0.20f, 0.15f, 0.90f);
            if (wallMat != null) segLintel.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Interactive Door: Door_To_Lamps
            GameObject doorRoot = new GameObject("Door_To_Lamps");
            doorRoot.transform.SetParent(room2.transform, false);
            doorRoot.transform.position = new Vector3(18.11f, 0f, -1.95f);
            doorRoot.transform.rotation = Quaternion.identity;

            GameObject hingeObj = new GameObject("Hinge");
            hingeObj.transform.SetParent(doorRoot.transform, false);
            hingeObj.transform.localPosition = Vector3.zero;
            hingeObj.transform.localRotation = Quaternion.identity;

            GameObject doorPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorPanel.name = "Door_Panel";
            doorPanel.transform.SetParent(hingeObj.transform, false);
            doorPanel.transform.localPosition = new Vector3(0f, 0.915f, 0.44f);
            doorPanel.transform.localScale = new Vector3(0.05f, 1.83f, 0.88f);
            if (doorMat != null) doorPanel.GetComponent<MeshRenderer>().sharedMaterial = doorMat;

            XRSimpleInteractable interactable = doorPanel.AddComponent<XRSimpleInteractable>();

            GameObject canvasObj = new GameObject("Prompt_Canvas");
            canvasObj.transform.SetParent(doorPanel.transform, false);
            canvasObj.transform.localPosition = new Vector3(-0.035f, 0.20f, 0f);
            canvasObj.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            RectTransform rt = canvasObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(70f, 25f);

            GameObject textObj = new GameObject("Prompt_Text");
            textObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Open [Ray Select]";
            tmp.fontSize = 11;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            GameObject triggerObj = new GameObject("Trigger_Zone");
            triggerObj.transform.SetParent(doorRoot.transform, false);
            triggerObj.transform.localPosition = new Vector3(-0.80f, 1.0f, 0.45f);
            BoxCollider trigCol = triggerObj.AddComponent<BoxCollider>();
            trigCol.isTrigger = true;
            trigCol.size = new Vector3(1.6f, 2.0f, 1.4f);

            InteractiveDoor doorComp = triggerObj.AddComponent<InteractiveDoor>();
            SerializedObject so = new SerializedObject(doorComp);
            so.FindProperty("m_HingeTransform").objectReferenceValue = hingeObj.transform;
            so.FindProperty("m_OpenAngle").floatValue = 90f; // swings open into Room 3 (+X)
            so.FindProperty("m_OpenDuration").floatValue = 1.0f;
            so.FindProperty("m_PromptCanvas").objectReferenceValue = canvas;
            so.FindProperty("m_PromptText").objectReferenceValue = tmp;
            so.FindProperty("m_PromptMessage").stringValue = "Open [Ray Select]";
            so.FindProperty("m_Interactable").objectReferenceValue = interactable;
            so.ApplyModifiedProperties();

            Debug.Log("[Step12] Installed InteractiveDoor 'Door_To_Lamps' on East doorway.");
        }

        return room2;
    }

    private static void SetupMiniFridgeStations(GameObject room2Root, Material pedestalMat, Camera mainCam)
    {
        foreach (var data in s_MiniFridges)
        {
            Transform existingStationT = room2Root.transform.Find(data.stationName);
            if (existingStationT != null)
            {
                Debug.Log($"[Step12] Station {data.stationName} already exists. Skipping.");
                continue;
            }

            GameObject station = new GameObject(data.stationName);
            Undo.RegisterCreatedObjectUndo(station, "Create " + data.stationName);
            station.transform.SetParent(room2Root.transform, false);
            station.transform.position = new Vector3(data.posX, 0f, 0f); // Option A: Z = 0
            station.transform.rotation = Quaternion.identity;

            // 1. Pedestal: Cylinder (scale 0.5, 0.03, 0.5)
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(station.transform, false);
            pedestal.transform.localPosition = Vector3.zero;
            pedestal.transform.localScale = new Vector3(0.5f, 0.03f, 0.5f);
            if (pedestalMat != null)
                pedestal.GetComponent<MeshRenderer>().sharedMaterial = pedestalMat;

            // 2. Mini Fridge 3D Model
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.modelPath);
            if (prefab != null)
            {
                GameObject modelObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, station.transform);
                if (modelObj == null) modelObj = Object.Instantiate(prefab, station.transform);

                modelObj.name = prefab.name;
                modelObj.transform.localPosition = Vector3.zero;
                // Upright orientation facing -Z
                modelObj.transform.localRotation = Quaternion.Euler(270f, 90f, 90f);

                // Add BoxCollider for physical/ray collision
                Renderer[] renderers = modelObj.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds b = renderers[0].bounds;
                    for (int r = 1; r < renderers.Length; r++) b.Encapsulate(renderers[r].bounds);

                    BoxCollider col = modelObj.AddComponent<BoxCollider>();
                    col.center = modelObj.transform.InverseTransformPoint(b.center);
                    Vector3 localSize = modelObj.transform.InverseTransformVector(b.size);
                    col.size = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
                }
            }
            else
            {
                Debug.LogError($"[Step12] Could not load prefab at {data.modelPath}");
            }

            // 3. Teleport Anchor: 1.5m in front along aisle (Z = -1.5m), facing +Z
            GameObject anchorGO = new GameObject("TeleportAnchor");
            anchorGO.transform.SetParent(station.transform, false);
            anchorGO.transform.localPosition = new Vector3(0f, -0.5f, -1.5f);
            anchorGO.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); // Facing +Z toward fridge

            BoxCollider anchorCol = anchorGO.AddComponent<BoxCollider>();
            anchorCol.size = new Vector3(1f, 0.01f, 1f);
            anchorCol.center = Vector3.zero;

            TeleportationAnchor anchorComp = anchorGO.AddComponent<TeleportationAnchor>();
            anchorComp.matchOrientation = MatchOrientation.TargetUpAndForward;
            anchorComp.interactionLayers = 1 << 31; // Teleport layer

            // 4. Label_Canvas: strictly "Name - $Price"
            GameObject canvasGO = new GameObject("Label_Canvas");
            canvasGO.transform.SetParent(station.transform, false);
            canvasGO.transform.localPosition = new Vector3(0f, 1.8f, -0.1f);
            canvasGO.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // Facing -Z
            canvasGO.transform.localScale = Vector3.one * 0.01f;

            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(100f, 30f);

            GameObject textGO = new GameObject("LabelText");
            textGO.transform.SetParent(canvasGO.transform, false);
            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = $"{data.displayName} - ${data.price:F0}";
            tmp.fontSize = 18f;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 8f;
            tmp.fontSizeMax = 18f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            // 5. Add to Cart Sign & Button
            SetupStationCartSign(station, data, mainCam);

            Debug.Log($"[Step12] Created station '{data.stationName}' ({data.displayName} - ${data.price}) at X = {data.posX}");
        }
    }

    private static void SetupStationCartSign(GameObject station, MiniFridgeData data, Camera mainCam)
    {
        GameObject signGO = new GameObject("Cart_Sign");
        signGO.transform.SetParent(station.transform, false);
        // Position on the right side of the fridge
        signGO.transform.localPosition = new Vector3(0.85f, 0.85f, -0.25f);
        signGO.transform.localRotation = Quaternion.Euler(0f, -15f, 0f);
        signGO.transform.localScale = Vector3.one * 0.01f;

        Canvas canvas = signGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        if (mainCam != null) canvas.worldCamera = mainCam;
        RectTransform canvasRT = signGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(44f, 26f);

        signGO.AddComponent<GraphicRaycaster>();
        signGO.AddComponent<TrackedDeviceGraphicRaycaster>();

        Image bgImage = signGO.AddComponent<Image>();
        bgImage.color = new Color(0.12f, 0.14f, 0.18f, 0.95f); // Dark slate

        // Sign Header
        GameObject headerGO = new GameObject("Sign_Header");
        headerGO.transform.SetParent(signGO.transform, false);
        RectTransform headerRT = headerGO.AddComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0f, 0.50f);
        headerRT.anchorMax = new Vector2(1f, 1f);
        headerRT.offsetMin = new Vector2(2f, 2f);
        headerRT.offsetMax = new Vector2(-2f, -2f);

        TextMeshProUGUI headerTMP = headerGO.AddComponent<TextMeshProUGUI>();
        headerTMP.text = $"<b>{data.displayName}</b>\n<color=#81C784>${data.price:F0}</color>";
        headerTMP.fontSize = 11f;
        headerTMP.alignment = TextAlignmentOptions.Center;
        headerTMP.color = Color.white;
        headerTMP.raycastTarget = false;

        // Button
        GameObject buttonGO = new GameObject("Cart_Button");
        buttonGO.transform.SetParent(signGO.transform, false);
        RectTransform buttonRT = buttonGO.AddComponent<RectTransform>();
        buttonRT.anchorMin = new Vector2(0.08f, 0.08f);
        buttonRT.anchorMax = new Vector2(0.92f, 0.48f);
        buttonRT.offsetMin = Vector2.zero;
        buttonRT.offsetMax = Vector2.zero;

        Image btnImage = buttonGO.AddComponent<Image>();
        btnImage.color = new Color(0.18f, 0.55f, 0.34f, 1f); // Forest green

        Button btn = buttonGO.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        btn.colors = cb;

        // Button Text
        GameObject btnTextGO = new GameObject("Button_Text");
        btnTextGO.transform.SetParent(buttonGO.transform, false);
        RectTransform btnTextRT = btnTextGO.AddComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmpText = btnTextGO.AddComponent<TextMeshProUGUI>();
        tmpText.text = "Add to Cart";
        tmpText.fontSize = 13f;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;
        tmpText.raycastTarget = false;

        // StationCartButton component
        StationCartButton stationBtn = buttonGO.AddComponent<StationCartButton>();
        stationBtn.SetItemDetails(data.displayName, data.price);

        SerializedObject so = new SerializedObject(stationBtn);
        so.FindProperty("m_ItemName").stringValue = data.displayName;
        so.FindProperty("m_Price").floatValue = data.price;
        so.FindProperty("m_Button").objectReferenceValue = btn;
        so.FindProperty("m_LabelText").objectReferenceValue = tmpText;
        so.ApplyModifiedProperties();
    }

    private static void CleanMiniFridgePreviews()
    {
        GameObject newBatch = GameObject.Find("NewBatch_Previews");
        if (newBatch == null) return;

        string[] previewNamesToRemove = {
            "Preview_MiniFridge_v2_cube_white",
            "Preview_MiniFridge_v3_bar_silver",
            "Preview_MiniFridge_v4_retro_mint",
            "Preview_MiniFridge_v5_wine_cooler"
        };

        foreach (string pName in previewNamesToRemove)
        {
            Transform t = newBatch.transform.Find(pName);
            if (t != null)
            {
                Undo.DestroyObjectImmediate(t.gameObject);
                Debug.Log($"[Step12] Cleaned up preview object '{pName}' (now active in Room 2).");
            }
        }
    }
}
