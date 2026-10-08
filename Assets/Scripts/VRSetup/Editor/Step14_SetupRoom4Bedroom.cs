// ============================================================
// Step14_SetupRoom4Bedroom.cs
// Editor-only one-shot script.
//
// Run via:  VR Furniture Shop > Step 14 - Setup Room 4 Bedroom & Living Suite
//
// What it does:
//   1. Replaces Room 3's solid East wall (X = 26.11) with an open doorway at
//      Z = -1.5m and an interactive door ("Door_To_Bedroom").
//   2. Builds Room 4 (Bedroom & Living Suite) shell:
//      - Dimensions: 8m x 8m (X = 26.11 to 34.11, Z = -4.0 to +4.0, height 2.0m).
//      - South Wall, North Wall, Outer East Wall, Floor with TeleportationArea, Roof.
//   3. Builds 3 luxury stations for the remaining furniture:
//      - Station_Cot_01:   "Comfort Cot Bed" ($850) at X = 28.5m, Z = 0.8m
//      - Station_Dress_01: "Classic Dressing Table" ($480) at X = 31.0m, Z = 0.8m
//      - Station_Chair_01: "Modern Lounge Chair" ($220) at X = 33.0m, Z = 0.8m
//      - Each station features:
//        * Architectural display pedestal
//        * 3D model with BoxCollider
//        * TeleportAnchor for VR player navigation
//        * Add to Cart card directly on top of each object, integrated with ShopCartManager!
//   4. Cleans up preview instances from NewBatch_Previews.
//   5. Wires door interaction button and formats UI.
//   6. Saves the scene.
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

[InitializeOnLoad]
public static class Step14_SetupRoom4Bedroom
{
    private const string ROOM3_NAME = "Room_3_Lamps";
    private const string ROOM4_NAME = "Room_4_Bedroom";

    public struct BedroomItemData
    {
        public string stationName;
        public string displayName;
        public float price;
        public float posX;
        public float posZ;
        public string modelPath;
        public Vector3 pedestalScale;
        public bool isCylinderPedestal;
        public float signHeight;

        public BedroomItemData(string stName, string dName, float p, float x, float z, string mPath, Vector3 pedScale, bool isCyl, float sHeight)
        {
            stationName = stName;
            displayName = dName;
            price = p;
            posX = x;
            posZ = z;
            modelPath = mPath;
            pedestalScale = pedScale;
            isCylinderPedestal = isCyl;
            signHeight = sHeight;
        }
    }

    private static readonly BedroomItemData[] s_Items = new[]
    {
        new BedroomItemData("Station_Cot_01", "Comfort Cot Bed", 850f, 28.5f, 0.8f, "Assets/Models/Furniture/cot.fbx", new Vector3(2.2f, 0.03f, 1.8f), false, 1.0f),
        new BedroomItemData("Station_Dress_01", "Classic Dressing Table", 480f, 31.0f, 0.8f, "Assets/Models/Furniture/Dressing_Table_3D.fbx", new Vector3(1.4f, 0.03f, 1.0f), false, 1.0f),
        new BedroomItemData("Station_Chair_01", "Modern Lounge Chair", 220f, 33.0f, 0.8f, "Assets/Models/Furniture/chair.fbx", new Vector3(0.9f, 0.03f, 0.9f), true, 1.0f),
        new BedroomItemData("Station_Sofa_01", "Comfort Sofa", 400f, 28.5f, -2.4f, "Assets/Models/Furniture/sofa.fbx", new Vector3(2.4f, 0.03f, 1.4f), false, 1.0f),
        new BedroomItemData("Station_TableChair_01", "Table & Chair Set", 250f, 31.2f, -2.4f, "Assets/Models/Furniture/f_1.fbx", new Vector3(1.6f, 0.03f, 1.4f), false, 1.0f)
    };

    static Step14_SetupRoom4Bedroom()
    {
        EditorApplication.delayCall += AutoRunIfSampleScene;
    }

    private static void AutoRunIfSampleScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.isLoaded && scene.path.Contains("SampleScene.unity"))
        {
            // Auto run if Room 4 doesn't exist yet
            if (GameObject.Find(ROOM4_NAME) == null)
            {
                Run();
            }
        }
    }

    [MenuItem("VR Furniture Shop/Step 14 - Setup Room 4 Bedroom & Living Suite")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step14] Exit Play Mode before running this script.");
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        }

        // Shared materials
        Material wallMat = FindMaterial("Left wall", "Title wall", "Room_3_South_Wall");
        Material floorMat = FindMaterial("Floor", "Room_3_Floor");
        Material pedestalMat = FindMaterial("Cylinder_01", "Pedestal");
        Material doorMat = FindMaterial("Door_Panel");

        Camera mainCam = Camera.main;

        // 1. Setup Doorway from Room 3 to Room 4
        SetupRoom3ToRoom4Doorway(wallMat, doorMat, mainCam);

        // 2. Build Room 4 Shell (X = 26.11 to 34.11, Z = -4.0 to +4.0)
        GameObject room4Root = SetupRoom4Shell(wallMat, floorMat);

        // 3. Build Bedroom stations
        SetupBedroomStations(room4Root, pedestalMat, mainCam);

        // 4. Clean up preview instances
        CleanupPreviews();

        // 5. Wire door buttons and format signs
        DoorButtonSetup.SetupAllDoorButtons(false);
        CartSignFormatter.ReformatAllSigns(false);

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Step14] Room 4 (Bedroom & Living Suite) setup completed successfully!");
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

    private static void SetupRoom3ToRoom4Doorway(Material wallMat, Material doorMat, Camera mainCam)
    {
        GameObject room3 = GameObject.Find(ROOM3_NAME);
        if (room3 == null) return;

        // Destroy solid East wall of Room 3 if present
        Transform solidEastWall = room3.transform.Find("Room_3_East_Wall");
        if (solidEastWall != null)
        {
            Undo.DestroyObjectImmediate(solidEastWall.gameObject);
        }

        // Create doorway group on X = 26.11m
        Transform doorwayGroupT = room3.transform.Find("Room_3_EastWall_Doorway");
        if (doorwayGroupT == null)
        {
            GameObject doorwayGroup = new GameObject("Room_3_EastWall_Doorway");
            Undo.RegisterCreatedObjectUndo(doorwayGroup, "Create Room_3_EastWall_Doorway");
            doorwayGroup.transform.SetParent(room3.transform, false);

            // South segment: Z = -3.92 to -2.04
            GameObject segSouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segSouth.name = "EastWall_Segment_South";
            segSouth.transform.SetParent(doorwayGroup.transform, false);
            segSouth.transform.position = new Vector3(26.11f, 1.0f, -2.995f);
            segSouth.transform.localScale = new Vector3(0.20f, 2.0f, 2.09f);
            if (wallMat != null) segSouth.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // North segment: Z = -0.96 to 3.92
            GameObject segNorth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segNorth.name = "EastWall_Segment_North";
            segNorth.transform.SetParent(doorwayGroup.transform, false);
            segNorth.transform.position = new Vector3(26.11f, 1.0f, 1.455f);
            segNorth.transform.localScale = new Vector3(0.20f, 2.0f, 5.01f);
            if (wallMat != null) segNorth.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Lintel segment over doorway: Z = -1.95 to -1.05
            GameObject segLintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segLintel.name = "EastWall_Segment_Lintel";
            segLintel.transform.SetParent(doorwayGroup.transform, false);
            segLintel.transform.position = new Vector3(26.11f, 1.925f, -1.50f);
            segLintel.transform.localScale = new Vector3(0.20f, 0.15f, 0.90f);
            if (wallMat != null) segLintel.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Interactive Door: Door_To_Bedroom
            GameObject doorRoot = new GameObject("Door_To_Bedroom");
            doorRoot.transform.SetParent(room3.transform, false);
            doorRoot.transform.position = new Vector3(26.11f, 0f, -1.95f);
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

            doorPanel.AddComponent<XRSimpleInteractable>();

            InteractiveDoor doorComp = doorRoot.AddComponent<InteractiveDoor>();
            SerializedObject so = new SerializedObject(doorComp);
            so.FindProperty("m_HingeTransform").objectReferenceValue = hingeObj.transform;
            so.FindProperty("m_OpenAngle").floatValue = 90f;
            so.FindProperty("m_OpenDuration").floatValue = 1.0f;
            so.ApplyModifiedProperties();
        }
    }

    private static GameObject SetupRoom4Shell(Material wallMat, Material floorMat)
    {
        GameObject room4 = GameObject.Find(ROOM4_NAME);
        if (room4 == null)
        {
            room4 = new GameObject(ROOM4_NAME);
            Undo.RegisterCreatedObjectUndo(room4, "Create " + ROOM4_NAME);
            room4.transform.position = Vector3.zero;
            room4.transform.rotation = Quaternion.identity;
        }

        // South wall: X = 30.11, Y = 1.0, Z = -3.92, scale (8, 2, 0.2)
        Transform southWallT = room4.transform.Find("Room_4_South_Wall");
        if (southWallT == null)
        {
            GameObject sw = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sw.name = "Room_4_South_Wall";
            sw.transform.SetParent(room4.transform, false);
            sw.transform.position = new Vector3(30.11f, 1.0f, -3.92f);
            sw.transform.localScale = new Vector3(8.0f, 2.0f, 0.2f);
            if (wallMat != null) sw.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
        }

        // North wall: X = 30.11, Y = 1.0, Z = 3.92, scale (8, 2, 0.2)
        Transform northWallT = room4.transform.Find("Room_4_North_Wall");
        if (northWallT == null)
        {
            GameObject nw = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nw.name = "Room_4_North_Wall";
            nw.transform.SetParent(room4.transform, false);
            nw.transform.position = new Vector3(30.11f, 1.0f, 3.92f);
            nw.transform.localScale = new Vector3(8.0f, 2.0f, 0.2f);
            if (wallMat != null) nw.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
        }

        // Outer East wall: X = 34.11, Y = 1.0, Z = 0, scale (0.2, 2, 8.0)
        Transform eastWallT = room4.transform.Find("Room_4_East_Wall");
        if (eastWallT == null)
        {
            GameObject ew = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ew.name = "Room_4_East_Wall";
            ew.transform.SetParent(room4.transform, false);
            ew.transform.position = new Vector3(34.11f, 1.0f, 0f);
            ew.transform.localScale = new Vector3(0.20f, 2.0f, 8.0f);
            if (wallMat != null) ew.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
        }

        // Roof: X = 30.11, Y = 2.07, Z = 0, scale (8, 0.1, 8)
        Transform roofT = room4.transform.Find("Room_4_Roof");
        if (roofT == null)
        {
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Room_4_Roof";
            roof.transform.SetParent(room4.transform, false);
            roof.transform.position = new Vector3(30.11f, 2.07f, 0f);
            roof.transform.localScale = new Vector3(8.0f, 0.1f, 8.0f);
            if (wallMat != null) roof.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
        }

        // Floor: X = 30.11, Y = 0, Z = 0, scale (8, 0.02, 8)
        Transform floorT = room4.transform.Find("Room_4_Floor");
        if (floorT == null)
        {
            GameObject fl = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fl.name = "Room_4_Floor";
            fl.transform.SetParent(room4.transform, false);
            fl.transform.position = new Vector3(30.11f, 0f, 0f);
            fl.transform.localScale = new Vector3(8.0f, 0.02f, 8.0f);
            if (floorMat != null) fl.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

            UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea ta = fl.AddComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea>();
            ta.interactionLayers = 1 << 31;
        }

        return room4;
    }

    private static void SetupBedroomStations(GameObject room4Root, Material pedestalMat, Camera mainCam)
    {
        foreach (var data in s_Items)
        {
            Transform existingStationT = room4Root.transform.Find(data.stationName);
            if (existingStationT != null)
            {
                Debug.Log($"[Step14] Station {data.stationName} already exists. Skipping.");
                continue;
            }

            GameObject station = new GameObject(data.stationName);
            Undo.RegisterCreatedObjectUndo(station, "Create " + data.stationName);
            station.transform.SetParent(room4Root.transform, false);
            station.transform.position = new Vector3(data.posX, 0f, data.posZ);
            station.transform.rotation = Quaternion.identity;

            // 1. Pedestal
            GameObject pedestal = GameObject.CreatePrimitive(data.isCylinderPedestal ? PrimitiveType.Cylinder : PrimitiveType.Cube);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(station.transform, false);
            pedestal.transform.localPosition = new Vector3(0f, data.pedestalScale.y * 0.5f, 0f);
            pedestal.transform.localScale = data.pedestalScale;
            if (pedestalMat != null)
                pedestal.GetComponent<MeshRenderer>().sharedMaterial = pedestalMat;

            // 2. 3D Model
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.modelPath);
            if (prefab != null)
            {
                GameObject modelObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, station.transform);
                if (modelObj == null) modelObj = Object.Instantiate(prefab, station.transform);

                modelObj.name = prefab.name;
                modelObj.transform.localPosition = new Vector3(0f, data.pedestalScale.y, 0f);
                modelObj.transform.localRotation = Quaternion.Euler(270f, 90f, 90f);
                modelObj.transform.localScale = Vector3.one;

                BoxCollider col = modelObj.GetComponent<BoxCollider>();
                if (col == null) col = modelObj.AddComponent<BoxCollider>();

                XRSimpleInteractable interactable = modelObj.GetComponent<XRSimpleInteractable>();
                if (interactable == null) interactable = modelObj.AddComponent<XRSimpleInteractable>();
                interactable.interactionLayers = 1;
                if (!interactable.colliders.Contains(col)) interactable.colliders.Add(col);

                if (modelObj.GetComponent<CupboardInteraction>() == null)
                    modelObj.AddComponent<CupboardInteraction>();
            }
            else
            {
                Debug.LogError($"[Step14] Could not load model prefab at '{data.modelPath}'");
            }

            // 3. Teleport Anchor (facing +Z toward the piece)
            GameObject anchorGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            anchorGO.name = "TeleportAnchor";
            anchorGO.transform.SetParent(station.transform, false);
            anchorGO.transform.localPosition = new Vector3(0f, -0.5f, -1.6f);
            anchorGO.transform.localRotation = Quaternion.identity;
            anchorGO.transform.localScale = new Vector3(1f, 0.01f, 1f);

            BoxCollider anchorCol = anchorGO.GetComponent<BoxCollider>();
            anchorCol.size = Vector3.one;
            anchorCol.center = Vector3.zero;

            UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor anchorComp = anchorGO.AddComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor>();
            anchorComp.matchOrientation = UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.MatchOrientation.TargetUpAndForward;
            anchorComp.interactionLayers = 1 << 31;

            // 4. Add to Cart Sign & Button positioned DIRECTLY ON TOP of the piece
            SetupStationCartSign(station, data.displayName, data.price, data.signHeight, mainCam);
        }
    }

    private static void SetupStationCartSign(GameObject station, string itemName, float price, float signY, Camera mainCam)
    {
        GameObject signGO = new GameObject("Cart_Sign");
        signGO.transform.SetParent(station.transform, false);
        signGO.transform.localPosition = new Vector3(0f, signY, 0f); // Directly on top of item
        signGO.transform.localRotation = Quaternion.identity;
        signGO.transform.localScale = Vector3.one * 0.001f;

        Canvas canvas = signGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        if (mainCam != null) canvas.worldCamera = mainCam;

        RectTransform canvasRT = signGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(440f, 280f);

        signGO.AddComponent<GraphicRaycaster>();
        signGO.AddComponent<TrackedDeviceGraphicRaycaster>();

        Image bgImage = signGO.AddComponent<Image>();
        bgImage.color = new Color(0.11f, 0.13f, 0.18f, 0.95f); // Deep sleek dark slate

        // Sign Header Text
        GameObject headerGO = new GameObject("Sign_Header");
        headerGO.transform.SetParent(signGO.transform, false);
        RectTransform headerRT = headerGO.AddComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0.05f, 0.44f);
        headerRT.anchorMax = new Vector2(0.95f, 0.94f);
        headerRT.offsetMin = Vector2.zero;
        headerRT.offsetMax = Vector2.zero;

        TextMeshProUGUI headerTMP = headerGO.AddComponent<TextMeshProUGUI>();
        headerTMP.text = $"<b>{itemName}</b>\n<color=#66BB6A><size=26>${price:F0}</size></color>";
        headerTMP.fontSize = 22f;
        headerTMP.enableAutoSizing = true;
        headerTMP.fontSizeMin = 14f;
        headerTMP.fontSizeMax = 22f;
        headerTMP.alignment = TextAlignmentOptions.Center;
        headerTMP.color = Color.white;
        headerTMP.raycastTarget = false;

        // Cart Button
        GameObject buttonGO = new GameObject("Cart_Button");
        buttonGO.transform.SetParent(signGO.transform, false);
        RectTransform buttonRT = buttonGO.AddComponent<RectTransform>();
        buttonRT.anchorMin = new Vector2(0.08f, 0.08f);
        buttonRT.anchorMax = new Vector2(0.92f, 0.38f);
        buttonRT.offsetMin = Vector2.zero;
        buttonRT.offsetMax = Vector2.zero;

        Image btnImage = buttonGO.AddComponent<Image>();
        btnImage.color = new Color(0.18f, 0.60f, 0.36f, 1f); // Vibrant emerald green

        Button btn = buttonGO.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        btn.colors = cb;

        // Button Text
        GameObject btnTextGO = new GameObject("Button_Text");
        btnTextGO.transform.SetParent(buttonGO.transform, false);
        RectTransform btnTextRT = btnTextGO.AddComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = new Vector2(10f, 4f);
        btnTextRT.offsetMax = new Vector2(-10f, -4f);

        TextMeshProUGUI tmpText = btnTextGO.AddComponent<TextMeshProUGUI>();
        tmpText.text = "Add to Cart";
        tmpText.fontSize = 20f;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.enableAutoSizing = true;
        tmpText.fontSizeMin = 14f;
        tmpText.fontSizeMax = 20f;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;
        tmpText.raycastTarget = false;

        // Wire StationCartButton
        StationCartButton stationBtn = buttonGO.AddComponent<StationCartButton>();
        stationBtn.SetItemDetails(itemName, price);

        SerializedObject so = new SerializedObject(stationBtn);
        so.FindProperty("m_ItemName").stringValue = itemName;
        so.FindProperty("m_Price").floatValue = price;
        so.FindProperty("m_Button").objectReferenceValue = btn;
        so.FindProperty("m_LabelText").objectReferenceValue = tmpText;
        so.ApplyModifiedProperties();
    }

    private static void CleanupPreviews()
    {
        string[] previewNames = { "Preview_Cot", "Preview_Chair", "Preview_DressingTable", "Cot_Label", "Chair_Label", "DressingTable_Label" };
        foreach (string pName in previewNames)
        {
            GameObject go = GameObject.Find(pName);
            if (go != null) Undo.DestroyObjectImmediate(go);
        }

        GameObject previewsRoot = GameObject.Find("NewBatch_Previews");
        if (previewsRoot != null && previewsRoot.transform.childCount == 0)
        {
            Undo.DestroyObjectImmediate(previewsRoot);
        }
    }
}
