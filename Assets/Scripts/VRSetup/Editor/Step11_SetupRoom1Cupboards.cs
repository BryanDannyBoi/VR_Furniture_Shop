// ============================================================
// Step11_SetupRoom1Cupboards.cs
// Editor-only script.
//
// Run via:  VR Furniture Shop > Step 11 - Setup Room 1 (Cupboards)
//
// What it does:
//   1. Organizes existing Cupboard stations (Station_01 to Station_05)
//      under a clean root GameObject "Room_1_Cupboards" without deleting anything.
//   2. Creates a clean doorway opening in the Back wall (X = 10.11, Z = -1.5m)
//      aligned with the customer walking aisle (where teleport anchors sit).
//   3. Installs an InteractiveDoor filling the doorway:
//      - Hinged door panel matching doorway dimensions (0.88m wide x 1.82m high).
//      - Trigger zone (BoxCollider) in front of the door that displays a world-space
//        "Open [Ray Select]" prompt when player approaches.
//      - InteractiveDoor script that smoothly swings open upon ray interaction.
//   4. Preserves entrance sign, cart system, and audio untouched.
//   5. Saves the scene.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[InitializeOnLoad]
public static class Step11_SetupRoom1Cupboards
{
    private const string ROOM_NAME = "Room_1_Cupboards";
    private const string BACK_WALL_NAME = "Back wall";

    static Step11_SetupRoom1Cupboards()
    {
        EditorApplication.delayCall += AutoRunOnce;
    }

    private static void AutoRunOnce()
    {
        if (SessionState.GetBool("Step11_AutoRun_Executed", false))
            return;

        SessionState.SetBool("Step11_AutoRun_Executed", true);
        Run();
    }

    // Doorway placement along aisle at Z = -1.5m, X = 10.11m (Back wall position)
    private const float WALL_X = 10.11f;
    private const float DOOR_CENTER_Z = -1.50f;
    private const float DOOR_WIDTH = 0.90f;
    private const float DOOR_HEIGHT = 1.85f;
    private const float WALL_HEIGHT = 2.00f;
    private const float WALL_THICKNESS = 0.20f;

    [MenuItem("VR Furniture Shop/Step 11 - Setup Room 1 (Cupboards)")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step11] Exit Play Mode before running this script.");
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        }

        // --------------------------------------------------------
        // 1. Root Room_1_Cupboards GameObject
        // --------------------------------------------------------
        GameObject roomRoot = GameObject.Find(ROOM_NAME);
        if (roomRoot == null)
        {
            roomRoot = new GameObject(ROOM_NAME);
            Undo.RegisterCreatedObjectUndo(roomRoot, "Create " + ROOM_NAME);
            roomRoot.transform.position = Vector3.zero;
            roomRoot.transform.rotation = Quaternion.identity;
        }

        // --------------------------------------------------------
        // 2. Reparent existing Cupboard stations under Room_1_Cupboards
        // --------------------------------------------------------
        string[] stationNames = { "Station_01", "Station_02", "Station_03", "Station_04", "Station_05" };
        foreach (string stName in stationNames)
        {
            GameObject st = GameObject.Find(stName);
            if (st != null && st.transform.parent != roomRoot.transform)
            {
                Undo.SetTransformParent(st.transform, roomRoot.transform, "Parent station to " + ROOM_NAME);
                Debug.Log($"[Step11] Grouped {stName} under {ROOM_NAME}");
            }
        }

        // --------------------------------------------------------
        // 3. Find Wall Material from existing walls
        // --------------------------------------------------------
        Material wallMat = null;
        GameObject existingBackWall = GameObject.Find(BACK_WALL_NAME);
        if (existingBackWall != null)
        {
            MeshRenderer mr = existingBackWall.GetComponent<MeshRenderer>();
            if (mr != null) wallMat = mr.sharedMaterial;
        }

        // --------------------------------------------------------
        // 4. Create Doorway in Back Wall (leads into Room 2: Mini Fridges)
        // --------------------------------------------------------
        // Back wall originally: X = 10.11, Y = 1, Z = -0.04, scale (0.2, 2, 8)
        // Spanned Z from -4.04 to +3.96
        // Split into:
        // - Wall_Segment_South: Z from -4.04 to -1.95 (center Z = -2.995, length = 2.09)
        // - Wall_Segment_North: Z from -1.05 to +3.96 (center Z = 1.455, length = 5.01)
        // - Wall_Lintel_Top:    Y from 1.85 to 2.00 (center Y = 1.925, height = 0.15), Z = -1.5, length = 0.90
        GameObject backWallGroup = GameObject.Find("Room_1_BackWall_Doorway");
        if (backWallGroup == null)
        {
            backWallGroup = new GameObject("Room_1_BackWall_Doorway");
            Undo.RegisterCreatedObjectUndo(backWallGroup, "Create Room 1 BackWall Doorway");
            backWallGroup.transform.SetParent(roomRoot.transform, false);

            // South segment
            GameObject segSouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segSouth.name = "BackWall_Segment_South";
            segSouth.transform.SetParent(backWallGroup.transform, false);
            segSouth.transform.position = new Vector3(WALL_X, WALL_HEIGHT * 0.5f, -2.995f);
            segSouth.transform.localScale = new Vector3(WALL_THICKNESS, WALL_HEIGHT, 2.09f);
            if (wallMat != null) segSouth.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // North segment
            GameObject segNorth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segNorth.name = "BackWall_Segment_North";
            segNorth.transform.SetParent(backWallGroup.transform, false);
            segNorth.transform.position = new Vector3(WALL_X, WALL_HEIGHT * 0.5f, 1.455f);
            segNorth.transform.localScale = new Vector3(WALL_THICKNESS, WALL_HEIGHT, 5.01f);
            if (wallMat != null) segNorth.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Lintel segment
            GameObject segLintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segLintel.name = "BackWall_Segment_Lintel";
            segLintel.transform.SetParent(backWallGroup.transform, false);
            segLintel.transform.position = new Vector3(WALL_X, 1.925f, DOOR_CENTER_Z);
            segLintel.transform.localScale = new Vector3(WALL_THICKNESS, 0.15f, DOOR_WIDTH);
            if (wallMat != null) segLintel.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Deactivate original solid Back wall rather than deleting it (per constraint: do not delete)
            if (existingBackWall != null)
            {
                Undo.RecordObject(existingBackWall, "Disable solid Back wall");
                existingBackWall.SetActive(false);
                existingBackWall.transform.SetParent(roomRoot.transform, true);
                Debug.Log("[Step11] Existing solid Back wall disabled in favor of doorway wall.");
            }
        }

        // --------------------------------------------------------
        // 5. Install Interactive Door
        // --------------------------------------------------------
        GameObject doorRoot = GameObject.Find("Door_To_MiniFridges");
        if (doorRoot == null)
        {
            doorRoot = new GameObject("Door_To_MiniFridges");
            Undo.RegisterCreatedObjectUndo(doorRoot, "Create Door_To_MiniFridges");
            doorRoot.transform.SetParent(roomRoot.transform, false);
            // Hinge position at south jamb: Z = -1.95, X = 10.11, Y = 0
            doorRoot.transform.position = new Vector3(WALL_X, 0f, -1.95f);
            doorRoot.transform.rotation = Quaternion.identity;

            // Hinge transform that rotates
            GameObject hingeObj = new GameObject("Hinge");
            hingeObj.transform.SetParent(doorRoot.transform, false);
            hingeObj.transform.localPosition = Vector3.zero;
            hingeObj.transform.localRotation = Quaternion.identity;

            // Door panel attached to hinge
            GameObject doorPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorPanel.name = "Door_Panel";
            doorPanel.transform.SetParent(hingeObj.transform, false);
            // Center of panel is +0.44m along Z from hinge, height 0.91m
            doorPanel.transform.localPosition = new Vector3(0f, DOOR_HEIGHT * 0.5f, 0.44f);
            doorPanel.transform.localScale = new Vector3(0.05f, DOOR_HEIGHT - 0.02f, 0.88f);

            // Door material - give it wood/signboard style
            Material doorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SignBoard_Mat.mat");
            if (doorMat != null)
            {
                doorPanel.GetComponent<MeshRenderer>().sharedMaterial = doorMat;
            }

            // XR Ray interaction on door panel
            XRSimpleInteractable interactable = doorPanel.AddComponent<XRSimpleInteractable>();

            // Trigger Zone in front of door (in Room 1)
            GameObject triggerObj = new GameObject("Trigger_Zone");
            triggerObj.transform.SetParent(doorRoot.transform, false);
            triggerObj.transform.localPosition = new Vector3(-0.80f, 1.0f, 0.45f);
            BoxCollider trigCol = triggerObj.AddComponent<BoxCollider>();
            trigCol.isTrigger = true;
            trigCol.size = new Vector3(1.6f, 2.0f, 1.4f);

            // World Space UI Prompt Canvas
            GameObject canvasObj = new GameObject("Prompt_Canvas");
            canvasObj.transform.SetParent(doorPanel.transform, false);
            canvasObj.transform.localPosition = new Vector3(-0.035f, 0.20f, 0f); // eye level on Room 1 face
            canvasObj.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            RectTransform rt = canvasObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector3(70f, 25f, 0f);

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

            // Setup InteractiveDoor component on triggerObj or doorRoot
            InteractiveDoor doorComp = triggerObj.AddComponent<InteractiveDoor>();
            // Use SerializedObject to wire up private serialized fields cleanly
            SerializedObject so = new SerializedObject(doorComp);
            so.FindProperty("m_HingeTransform").objectReferenceValue = hingeObj.transform;
            so.FindProperty("m_OpenAngle").floatValue = 90f; // swings into Room 2
            so.FindProperty("m_OpenDuration").floatValue = 1.0f;
            so.FindProperty("m_PromptCanvas").objectReferenceValue = canvas;
            so.FindProperty("m_PromptText").objectReferenceValue = tmp;
            so.FindProperty("m_PromptMessage").stringValue = "Open [Ray Select]";
            so.FindProperty("m_Interactable").objectReferenceValue = interactable;
            so.ApplyModifiedProperties();

            Debug.Log("[Step11] Installed InteractiveDoor 'Door_To_MiniFridges' at Back Wall doorway.");
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Step11] Room 1 (Cupboards) setup complete!");
    }
}
