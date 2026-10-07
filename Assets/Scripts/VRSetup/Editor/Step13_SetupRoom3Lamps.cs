// ============================================================
// Step13_SetupRoom3Lamps.cs
// Editor script to build Room 3 (Lamps) and finalize Room 2 (Mini Fridges)
//
// What it does:
// 1. Finalizes Room 2 (Mini Fridges) stations:
//    - Ensures the 4 mini fridges (v2, v3, v4, v5) have the confirmed transforms:
//      Rotation: (-90, 0, -180), Scale: (7, 10, 7)
//    - Ensures each station has: Cylinder pedestal (0.5, 0.03, 0.5), TeleportAnchor,
//      and Label_Canvas strictly showing "Name - $Price":
//      * Station_MF_02: "Classic Mini - $250"
//      * Station_MF_03: "Standard Cooler - $300"
//      * Station_MF_04: "Freeze Cool - $320"
//      * Station_MF_05: "Ultra Cold - $400"
// 2. Builds Room 3 (Lamps) shell:
//    - Connected to East wall of Room 2 (X = 18.11 to 26.11, Z = -4.0 to +4.0, height 2.0m).
//    - South Wall (X = 22.11, Z = -3.92, scale 8, 2, 0.2)
//    - North Wall (X = 22.11, Z = 3.92, scale 8, 2, 0.2)
//    - East Wall (outer back wall: X = 26.11, Z = 0, scale 0.2, 2, 8.0)
//    - Floor (X = 22.11, Z = 0, scale 8, 0.02, 8.0) with TeleportationArea (layer 31)
//    - Roof (X = 22.11, Z = 0, scale 8, 0.1, 8.0)
// 3. Connects Room 3 via existing East doorway InteractiveDoor "Door_To_Lamps" (X = 18.11, Z = -1.5).
// 4. Builds 5 Lamp stations in Room 3:
//    - Station_Lamp_01 (Cyan):   Pos (19.5, 0, 0), "Cyan Desk Lamp - $120"
//    - Station_Lamp_02 (Green):  Pos (21.0, 0, 0), "Green Desk Lamp - $120"
//    - Station_Lamp_03 (Purple): Pos (22.5, 0, 0), "Purple Desk Lamp - $140"
//    - Station_Lamp_04 (Red):    Pos (24.0, 0, 0), "Red Desk Lamp - $150"
//    - Station_Lamp_05 (Yellow): Pos (25.5, 0, 0), "Yellow Desk Lamp - $130"
//    - Each station includes:
//      * Cylinder pedestal (scale 0.5, 0.03, 0.5) beneath the lamp
//      * Lamp model placed on pedestal at local (0, 0.03, 0), Rot (-89.98, 0, -180), Scale (1, 1, 1)
//      * BoxCollider on lamp model
//      * TeleportAnchor at local (0, -0.5, -1.5) facing +Z
//      * Label_Canvas at local (0, 1.8, 0) strictly displaying "Name - $Price"
//      * Cart_Sign with Add to Cart button integrated with ShopCartManager
// 5. Cleans up preview lamp instances from Lamp_Previews.
// 6. Saves the scene.
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
public static class Step13_SetupRoom3Lamps
{
    private const string ROOM2_NAME = "Room_2_MiniFridges";
    private const string ROOM3_NAME = "Room_3_Lamps";

    static Step13_SetupRoom3Lamps()
    {
        EditorApplication.delayCall += AutoRunOnce;
    }

    private static void AutoRunOnce()
    {
        if (SessionState.GetBool("Step13_AutoRun_Executed", false))
            return;

        SessionState.SetBool("Step13_AutoRun_Executed", true);
        Run();
    }

    private struct LampData
    {
        public string stationName;
        public string displayName;
        public float price;
        public float posX;
        public string modelPath;

        public LampData(string stName, string name, float itemPrice, float x, string path)
        {
            stationName = stName;
            displayName = name;
            price = itemPrice;
            posX = x;
            modelPath = path;
        }
    }

    private static readonly LampData[] s_Lamps = new[]
    {
        new LampData("Station_Lamp_01", "Cyan Desk Lamp", 120f, 19.5f, "Assets/Models/Furniture/lamp_cyan.fbx"),
        new LampData("Station_Lamp_02", "Green Desk Lamp", 120f, 21.0f, "Assets/Models/Furniture/lamp_green.fbx"),
        new LampData("Station_Lamp_03", "Purple Desk Lamp", 140f, 22.5f, "Assets/Models/Furniture/lamp_purple.fbx"),
        new LampData("Station_Lamp_04", "Red Desk Lamp", 150f, 24.0f, "Assets/Models/Furniture/lamp_red.fbx"),
        new LampData("Station_Lamp_05", "Yellow Desk Lamp", 130f, 25.5f, "Assets/Models/Furniture/lamp_yellow.fbx")
    };

    [MenuItem("VR Furniture Shop/Step 13 - Setup Room 3 (Lamps)")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step13] Exit Play Mode before running this script.");
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        }

        // 1. Shared Materials
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

        // 2. Finalize Room 2 Mini Fridges (Rotation -90, 0, -180 and Scale 7, 10, 7)
        FinalizeRoom2MiniFridges();

        // 3. Build Room 3 Shell (8m x 8m from X = 18.11 to 26.11)
        GameObject room3Root = SetupRoom3Shell(wallMat, floorMat);

        // 4. Build Lamp Stations in Room 3
        SetupLampStations(room3Root, pedestalMat, mainCam);

        // 5. Clean up Lamp previews from Lamp_Previews
        CleanLampPreviews();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Step13] Room 3 (Lamps) and Room 2 (Mini Fridges) setup completed successfully!");
    }

    private static void FinalizeRoom2MiniFridges()
    {
        string[] fridgeStations = { "Station_MF_02", "Station_MF_03", "Station_MF_04", "Station_MF_05" };
        foreach (string stName in fridgeStations)
        {
            GameObject st = GameObject.Find(stName);
            if (st == null) continue;

            for (int i = 0; i < st.transform.childCount; i++)
            {
                Transform child = st.transform.GetChild(i);
                if (child.name.StartsWith("MiniFridge_"))
                {
                    Undo.RecordObject(child, "Update fridge transform");
                    child.localRotation = Quaternion.Euler(-90f, 0f, -180f);
                    child.localScale = new Vector3(7f, 10f, 7f);
                    EditorUtility.SetDirty(child.gameObject);
                }
            }
        }

        // Clean up preview fridge v1 if present
        GameObject prevV1 = GameObject.Find("Preview_MiniFridge_v1_original");
        if (prevV1 != null)
        {
            Undo.DestroyObjectImmediate(prevV1);
        }
    }

    private static GameObject SetupRoom3Shell(Material wallMat, Material floorMat)
    {
        GameObject room3 = GameObject.Find(ROOM3_NAME);
        if (room3 == null)
        {
            room3 = new GameObject(ROOM3_NAME);
            Undo.RegisterCreatedObjectUndo(room3, "Create " + ROOM3_NAME);
            room3.transform.position = Vector3.zero;
            room3.transform.rotation = Quaternion.identity;
        }

        // South wall: X = 22.11, Y = 1.0, Z = -3.92, scale (8, 2, 0.2)
        Transform southWallT = room3.transform.Find("Room_3_South_Wall");
        if (southWallT == null)
        {
            GameObject sw = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sw.name = "Room_3_South_Wall";
            sw.transform.SetParent(room3.transform, false);
            sw.transform.position = new Vector3(22.11f, 1.0f, -3.92f);
            sw.transform.localScale = new Vector3(8.0f, 2.0f, 0.2f);
            if (wallMat != null) sw.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
        }

        // North wall: X = 22.11, Y = 1.0, Z = 3.92, scale (8, 2, 0.2)
        Transform northWallT = room3.transform.Find("Room_3_North_Wall");
        if (northWallT == null)
        {
            GameObject nw = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nw.name = "Room_3_North_Wall";
            nw.transform.SetParent(room3.transform, false);
            nw.transform.position = new Vector3(22.11f, 1.0f, 3.92f);
            nw.transform.localScale = new Vector3(8.0f, 2.0f, 0.2f);
            if (wallMat != null) nw.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
        }

        // East wall (outer back wall of shop): X = 26.11, Y = 1.0, Z = 0, scale (0.2, 2, 8.0)
        Transform eastWallT = room3.transform.Find("Room_3_East_Wall");
        if (eastWallT == null)
        {
            GameObject ew = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ew.name = "Room_3_East_Wall";
            ew.transform.SetParent(room3.transform, false);
            ew.transform.position = new Vector3(26.11f, 1.0f, 0f);
            ew.transform.localScale = new Vector3(0.20f, 2.0f, 8.0f);
            if (wallMat != null) ew.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
        }

        // Roof: X = 22.11, Y = 2.07, Z = 0, scale (8, 0.1, 8)
        Transform roofT = room3.transform.Find("Room_3_Roof");
        if (roofT == null)
        {
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Room_3_Roof";
            roof.transform.SetParent(room3.transform, false);
            roof.transform.position = new Vector3(22.11f, 2.07f, 0f);
            roof.transform.localScale = new Vector3(8.0f, 0.1f, 8.0f);
            if (wallMat != null) roof.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
        }

        // Floor: X = 22.11, Y = 0, Z = 0, scale (8, 0.02, 8)
        Transform floorT = room3.transform.Find("Room_3_Floor");
        if (floorT == null)
        {
            GameObject fl = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fl.name = "Room_3_Floor";
            fl.transform.SetParent(room3.transform, false);
            fl.transform.position = new Vector3(22.11f, 0f, 0f);
            fl.transform.localScale = new Vector3(8.0f, 0.02f, 8.0f);
            if (floorMat != null) fl.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

            // Add TeleportationArea on layer 31
            TeleportationArea ta = fl.AddComponent<TeleportationArea>();
            ta.interactionLayers = 1 << 31;
        }

        return room3;
    }

    private static void SetupLampStations(GameObject room3Root, Material pedestalMat, Camera mainCam)
    {
        Material signBoardMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SignBoard_Mat.mat");

        foreach (var data in s_Lamps)
        {
            Transform existingStationT = room3Root.transform.Find(data.stationName);
            if (existingStationT != null)
            {
                Debug.Log($"[Step13] Station {data.stationName} already exists. Skipping.");
                continue;
            }

            GameObject station = new GameObject(data.stationName);
            Undo.RegisterCreatedObjectUndo(station, "Create " + data.stationName);
            station.transform.SetParent(room3Root.transform, false);
            station.transform.position = new Vector3(data.posX, 0f, 0f); // Along Z = 0 facing -Z
            station.transform.rotation = Quaternion.identity;

            // 1. Pedestal: Cylinder (scale 0.5, 0.03, 0.5)
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(station.transform, false);
            pedestal.transform.localPosition = Vector3.zero;
            pedestal.transform.localScale = new Vector3(0.5f, 0.03f, 0.5f);
            if (pedestalMat != null)
                pedestal.GetComponent<MeshRenderer>().sharedMaterial = pedestalMat;

            // 2. Lamp 3D Model
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.modelPath);
            if (prefab != null)
            {
                GameObject modelObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, station.transform);
                if (modelObj == null) modelObj = Object.Instantiate(prefab, station.transform);

                modelObj.name = prefab.name;
                modelObj.transform.localPosition = new Vector3(0f, 0.03f, 0f); // Sits on top of pedestal
                modelObj.transform.localRotation = Quaternion.Euler(-89.98f, 0f, -180f);
                modelObj.transform.localScale = Vector3.one;

                // Add BoxCollider for ray collision / physics
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
                Debug.LogError($"[Step13] Could not load prefab at {data.modelPath}");
            }

            // 3. Teleport Anchor: 1.5m in front along aisle (Z = -1.5m), facing +Z
            GameObject anchorGO = new GameObject("TeleportAnchor");
            anchorGO.transform.SetParent(station.transform, false);
            anchorGO.transform.localPosition = new Vector3(0f, -0.5f, -1.5f);
            anchorGO.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); // Facing +Z toward lamp

            BoxCollider anchorCol = anchorGO.AddComponent<BoxCollider>();
            anchorCol.size = new Vector3(1f, 0.01f, 1f);
            anchorCol.center = Vector3.zero;

            TeleportationAnchor anchorComp = anchorGO.AddComponent<TeleportationAnchor>();
            anchorComp.matchOrientation = MatchOrientation.TargetUpAndForward;
            anchorComp.interactionLayers = 1 << 31; // Teleport layer

            // 4. Station Label Canvas: exactly "Name - $Price"
            SetupStationLabel(station, data.displayName, data.price, mainCam);

            // 5. Add to Cart Sign & Button
            SetupStationCartSign(station, data.displayName, data.price, signBoardMat, mainCam);
        }
    }

    private static void SetupStationLabel(GameObject station, string name, float price, Camera mainCam)
    {
        GameObject canvasGO = new GameObject("Label_Canvas");
        canvasGO.transform.SetParent(station.transform, false);
        canvasGO.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        canvasGO.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // Facing player at -Z
        canvasGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        if (mainCam != null) canvas.worldCamera = mainCam;

        canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();

        RectTransform rt = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100f, 30f);

        GameObject textGO = new GameObject("LabelText");
        textGO.transform.SetParent(canvasGO.transform, false);

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = $"{name} - ${price:F0}";
        tmp.fontSize = 18f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 8f;
        tmp.fontSizeMax = 20f;

        RectTransform textRt = textGO.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
    }

    private static void SetupStationCartSign(GameObject station, string itemName, float price, Material signBoardMat, Camera mainCam)
    {
        GameObject signGO = new GameObject("Cart_Sign");
        signGO.transform.SetParent(station.transform, false);
        signGO.transform.localPosition = new Vector3(0.55f, 0.85f, -0.30f); // Right side angled towards player
        signGO.transform.localRotation = Quaternion.Euler(0f, -15f, 0f);

        // Sign Board physical backing
        GameObject boardGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boardGO.name = "Sign_Board";
        boardGO.transform.SetParent(signGO.transform, false);
        boardGO.transform.localPosition = Vector3.zero;
        boardGO.transform.localScale = new Vector3(0.44f, 0.28f, 0.02f);
        if (signBoardMat != null)
            boardGO.GetComponent<MeshRenderer>().sharedMaterial = signBoardMat;

        // Sign Canvas
        GameObject signCanvasGO = new GameObject("Sign_Canvas");
        signCanvasGO.transform.SetParent(signGO.transform, false);
        signCanvasGO.transform.localPosition = new Vector3(0f, 0f, -0.015f);
        signCanvasGO.transform.localRotation = Quaternion.identity;
        signCanvasGO.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        Canvas canvas = signCanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        if (mainCam != null) canvas.worldCamera = mainCam;
        signCanvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();

        RectTransform canvasRT = signCanvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(440f, 280f);

        // Header Text: Item Name and Price
        GameObject headerGO = new GameObject("Sign_Header");
        headerGO.transform.SetParent(signCanvasGO.transform, false);
        TextMeshProUGUI headerTMP = headerGO.AddComponent<TextMeshProUGUI>();
        headerTMP.text = $"<b>{itemName}</b>\n<color=#66BB6A><size=26>${price:F0}</size></color>";
        headerTMP.fontSize = 24f;
        headerTMP.alignment = TextAlignmentOptions.Center;
        headerTMP.color = Color.white;

        RectTransform headerRT = headerGO.GetComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0.05f, 0.45f);
        headerRT.anchorMax = new Vector2(0.95f, 0.95f);
        headerRT.sizeDelta = Vector2.zero;

        // Add to Cart Button
        GameObject buttonGO = new GameObject("Cart_Button");
        buttonGO.transform.SetParent(signCanvasGO.transform, false);

        Image btnImg = buttonGO.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.54f, 0.34f, 0.95f);

        Button btn = buttonGO.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.18f, 0.54f, 0.34f, 0.95f);
        cb.highlightedColor = new Color(0.24f, 0.70f, 0.44f, 1f);
        cb.pressedColor = new Color(0.12f, 0.38f, 0.24f, 1f);
        cb.selectedColor = new Color(0.18f, 0.54f, 0.34f, 0.95f);
        btn.colors = cb;

        BoxCollider btnCol = buttonGO.AddComponent<BoxCollider>();
        btnCol.size = new Vector3(320f, 60f, 1f);

        RectTransform btnRT = buttonGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.12f, 0.10f);
        btnRT.anchorMax = new Vector2(0.88f, 0.38f);
        btnRT.sizeDelta = Vector2.zero;

        // Button Text
        GameObject btnTextGO = new GameObject("Button_Text");
        btnTextGO.transform.SetParent(buttonGO.transform, false);
        TextMeshProUGUI btnTMP = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnTMP.text = "Add to Cart";
        btnTMP.fontSize = 20f;
        btnTMP.fontStyle = FontStyles.Bold;
        btnTMP.alignment = TextAlignmentOptions.Center;
        btnTMP.color = Color.white;

        RectTransform btnTextRT = btnTextGO.GetComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.sizeDelta = Vector2.zero;

        // Wire up StationCartButton logic
        StationCartButton scb = buttonGO.AddComponent<StationCartButton>();
        scb.SetItemDetails(itemName, price);
        SerializedObject so = new SerializedObject(scb);
        so.FindProperty("m_ItemName").stringValue = itemName;
        so.FindProperty("m_Price").floatValue = price;
        so.FindProperty("m_Button").objectReferenceValue = btn;
        so.FindProperty("m_LabelText").objectReferenceValue = btnTMP;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(scb);
        EditorUtility.SetDirty(buttonGO);
    }

    private static void CleanLampPreviews()
    {
        GameObject lampPreviews = GameObject.Find("Lamp_Previews");
        if (lampPreviews != null)
        {
            Undo.RecordObject(lampPreviews, "Deactivate lamp previews");
            lampPreviews.SetActive(false);
            Debug.Log("[Step13] Deactivated Lamp_Previews root in favor of Room 3 stations.");
        }
    }
}
