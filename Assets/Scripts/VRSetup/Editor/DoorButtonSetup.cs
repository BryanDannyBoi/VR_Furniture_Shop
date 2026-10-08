// ============================================================
// DoorButtonSetup.cs
// Adds a visible, clickable VR UI button ("[ 🚪 Open Door ]") to all
// InteractiveDoors across the scene so players can open doors with a simple
// ray click or mouse left-click, in addition to controller Grip/Trigger.
//
// Run via:  VR Furniture Shop > Setup Door Open Buttons
// Also auto-configures on script reload.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;

[InitializeOnLoad]
public static class DoorButtonSetup
{
    static DoorButtonSetup()
    {
        EditorApplication.delayCall += AutoSetupDoors;
    }

    private static void AutoSetupDoors()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.isLoaded && scene.path.Contains("SampleScene.unity"))
        {
            SetupAllDoorButtons(false);
        }
    }

    [MenuItem("VR Furniture Shop/Setup Door Open Buttons")]
    public static void ManualSetupDoors()
    {
        SetupAllDoorButtons(true);
    }

    public static void SetupAllDoorButtons(bool logSummary)
    {
        if (EditorApplication.isPlaying) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.isLoaded) return;

        Camera mainCam = Camera.main;

        string[] doorNames = { "Entrance_Door_Room1", "Door_To_MiniFridges", "Door_To_Lamps", "Door_To_Bedroom" };
        int configuredCount = 0;

        foreach (string dName in doorNames)
        {
            GameObject doorRoot = GameObject.Find(dName);
            if (doorRoot == null) continue;

            InteractiveDoor doorComp = doorRoot.GetComponentInChildren<InteractiveDoor>();
            if (doorComp == null) continue;

            Transform hingeT = doorRoot.transform.Find("Hinge");
            if (hingeT == null) continue;

            Transform panelT = hingeT.Find("Door_Panel");

            // Clean up any old prompt canvas that was nested under Door_Panel (which had non-uniform scale)
            if (panelT != null)
            {
                Transform oldCanvasOnPanel = panelT.Find("Prompt_Canvas");
                if (oldCanvasOnPanel != null)
                {
                    Undo.DestroyObjectImmediate(oldCanvasOnPanel.gameObject);
                }
            }

            // Create or configure Front Button (facing -X, e.g. Room 1 approach)
            Button frontBtn = SetupButtonCanvas(
                hingeT,
                "Prompt_Canvas_Front",
                new Vector3(-0.035f, 1.05f, 0.44f),
                Quaternion.Euler(0f, -90f, 0f),
                mainCam,
                dName + "_Front"
            );

            // Create or configure Back Button (facing +X, e.g. approaching from inside)
            Button backBtn = SetupButtonCanvas(
                hingeT,
                "Prompt_Canvas_Back",
                new Vector3(0.035f, 1.05f, 0.44f),
                Quaternion.Euler(0f, 90f, 0f),
                mainCam,
                dName + "_Back"
            );

            // Wire up InteractiveDoor serialized properties
            SerializedObject so = new SerializedObject(doorComp);
            so.FindProperty("m_PromptCanvas").objectReferenceValue = frontBtn != null ? frontBtn.GetComponentInParent<Canvas>() : null;
            so.FindProperty("m_OpenButton").objectReferenceValue = frontBtn;
            so.FindProperty("m_PromptMessage").stringValue = "Open Door";
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(doorComp);
            EditorUtility.SetDirty(hingeT.gameObject);
            configuredCount++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        if (logSummary)
        {
            Debug.Log($"[DoorButtonSetup] Successfully setup '{configuredCount}' doors with clickable VR UI buttons on both faces!");
        }
    }

    private static Button SetupButtonCanvas(Transform parent, string canvasName, Vector3 localPos, Quaternion localRot, Camera mainCam, string undoLabel)
    {
        Transform canvasT = parent.Find(canvasName);
        GameObject canvasObj;
        if (canvasT == null)
        {
            canvasObj = new GameObject(canvasName);
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create " + canvasName + " for " + undoLabel);
            canvasObj.transform.SetParent(parent, false);
        }
        else
        {
            canvasObj = canvasT.gameObject;
        }

        canvasObj.transform.localPosition = localPos;
        canvasObj.transform.localRotation = localRot;
        canvasObj.transform.localScale = Vector3.one * 0.0015f; // Crisp uniform VR scale

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        if (canvas == null) canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        if (mainCam != null) canvas.worldCamera = mainCam;

        RectTransform canvasRT = canvasObj.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(260f, 80f);

        if (canvasObj.GetComponent<GraphicRaycaster>() == null)
            canvasObj.AddComponent<GraphicRaycaster>();
        if (canvasObj.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
            canvasObj.AddComponent<TrackedDeviceGraphicRaycaster>();

        // Button GameObject
        Transform btnT = canvasObj.transform.Find("Open_Button");
        GameObject btnObj;
        if (btnT == null)
        {
            btnObj = new GameObject("Open_Button");
            Undo.RegisterCreatedObjectUndo(btnObj, "Create Open_Button for " + undoLabel);
            btnObj.transform.SetParent(canvasObj.transform, false);
        }
        else
        {
            btnObj = btnT.gameObject;
        }

        RectTransform btnRT = btnObj.GetComponent<RectTransform>();
        if (btnRT == null) btnRT = btnObj.AddComponent<RectTransform>();
        btnRT.anchorMin = Vector2.zero;
        btnRT.anchorMax = Vector2.one;
        btnRT.offsetMin = new Vector2(4f, 4f);
        btnRT.offsetMax = new Vector2(-4f, -4f);

        Image btnImage = btnObj.GetComponent<Image>();
        if (btnImage == null) btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.14f, 0.52f, 0.30f, 0.95f); // Vibrant showroom emerald green

        Button btn = btnObj.GetComponent<Button>();
        if (btn == null) btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;

        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = cb;

        // Button Text
        Transform textT = btnObj.transform.Find("Button_Text");
        GameObject textObj;
        if (textT == null)
        {
            textObj = new GameObject("Button_Text");
            Undo.RegisterCreatedObjectUndo(textObj, "Create Button_Text for " + undoLabel);
            textObj.transform.SetParent(btnObj.transform, false);
        }
        else
        {
            textObj = textT.gameObject;
        }

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        if (textRT == null) textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "<b>Open Door</b>";
        tmp.fontSize = 24f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 14f;
        tmp.fontSizeMax = 24f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        EditorUtility.SetDirty(btnObj);
        EditorUtility.SetDirty(canvasObj);

        return btn;
    }
}
