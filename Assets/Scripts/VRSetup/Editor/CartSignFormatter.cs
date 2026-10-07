// ============================================================
// CartSignFormatter.cs
// Editor utility to automatically format and polish all "Add to Cart"
// station signs and buttons across the scene to crisp, spacious VR UI.
//
// Run via:  VR Furniture Shop > Format Cart Buttons & Signs
// Also automatically runs on compilation.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;

[InitializeOnLoad]
public static class CartSignFormatter
{
    static CartSignFormatter()
    {
        EditorApplication.delayCall += AutoFormatIfSampleScene;
    }

    private static void AutoFormatIfSampleScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.isLoaded && scene.path.Contains("SampleScene.unity"))
        {
            ReformatAllSigns(true);
        }
    }

    [MenuItem("VR Furniture Shop/Format Cart Buttons & Signs")]
    public static void ManualReformat()
    {
        ReformatAllSigns(true);
    }

    public static void ReformatAllSigns(bool logSummary)
    {
        if (EditorApplication.isPlaying) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.isLoaded) return;

        Camera mainCam = Camera.main;

        // 1. Check mini-fridge model scale (5, 5, 10) for Station_MF_02 .. 05
        string[] fridgeStations = { "Station_MF_02", "Station_MF_03", "Station_MF_04", "Station_MF_05" };
        foreach (string stName in fridgeStations)
        {
            GameObject st = GameObject.Find(stName);
            if (st == null) continue;

            for (int i = 0; i < st.transform.childCount; i++)
            {
                Transform child = st.transform.GetChild(i);
                if (child.name.StartsWith("MiniFridge_") || child.name.StartsWith("Preview_MiniFridge"))
                {
                    child.localScale = new Vector3(7f, 10f, 7f);
                    EditorUtility.SetDirty(child.gameObject);
                }
            }
        }

        // 2. Find all Cart_Sign objects in the scene
        StationCartButton[] allButtons = Object.FindObjectsByType<StationCartButton>(FindObjectsSortMode.None);
        int formattedCount = 0;

        foreach (var stationBtn in allButtons)
        {
            GameObject buttonGO = stationBtn.gameObject;
            Transform signT = buttonGO.transform.parent;
            if (signT == null) continue;

            GameObject signGO = signT.gameObject;

            // Reformat Canvas
            signGO.transform.localScale = Vector3.one * 0.001f; // 0.001m per UI unit
            Canvas canvas = signGO.GetComponent<Canvas>();
            if (canvas == null) canvas = signGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            if (mainCam != null) canvas.worldCamera = mainCam;

            RectTransform canvasRT = signGO.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(440f, 280f);

            if (signGO.GetComponent<GraphicRaycaster>() == null)
                signGO.AddComponent<GraphicRaycaster>();
            if (signGO.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                signGO.AddComponent<TrackedDeviceGraphicRaycaster>();

            // Sign Background Plate
            Image bgImage = signGO.GetComponent<Image>();
            if (bgImage == null) bgImage = signGO.AddComponent<Image>();
            bgImage.color = new Color(0.11f, 0.13f, 0.18f, 0.95f); // Deep sleek dark slate

            // Reformat Sign_Header
            Transform headerT = signGO.transform.Find("Sign_Header");
            if (headerT != null)
            {
                RectTransform headerRT = headerT.GetComponent<RectTransform>();
                headerRT.anchorMin = new Vector2(0.05f, 0.44f);
                headerRT.anchorMax = new Vector2(0.95f, 0.94f);
                headerRT.offsetMin = Vector2.zero;
                headerRT.offsetMax = Vector2.zero;

                TextMeshProUGUI headerTMP = headerT.GetComponent<TextMeshProUGUI>();
                if (headerTMP != null)
                {
                    headerTMP.text = $"<b>{stationBtn.ItemName}</b>\n<color=#66BB6A><size=26>${stationBtn.Price:F0}</size></color>";
                    headerTMP.fontSize = 22f;
                    headerTMP.enableAutoSizing = true;
                    headerTMP.fontSizeMin = 14f;
                    headerTMP.fontSizeMax = 22f;
                    headerTMP.alignment = TextAlignmentOptions.Center;
                    headerTMP.color = Color.white;
                    headerTMP.raycastTarget = false;
                    EditorUtility.SetDirty(headerTMP);
                }
                EditorUtility.SetDirty(headerT.gameObject);
            }

            // Reformat Cart_Button
            RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
            buttonRT.anchorMin = new Vector2(0.08f, 0.08f);
            buttonRT.anchorMax = new Vector2(0.92f, 0.38f);
            buttonRT.offsetMin = Vector2.zero;
            buttonRT.offsetMax = Vector2.zero;

            Image btnImage = buttonGO.GetComponent<Image>();
            if (btnImage == null) btnImage = buttonGO.AddComponent<Image>();
            btnImage.color = new Color(0.18f, 0.60f, 0.36f, 1f); // Vibrant emerald green

            Button btn = buttonGO.GetComponent<Button>();
            if (btn == null) btn = buttonGO.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = cb;

            // Reformat Button_Text
            Transform btnTextT = buttonGO.transform.Find("Button_Text");
            if (btnTextT != null)
            {
                RectTransform btnTextRT = btnTextT.GetComponent<RectTransform>();
                btnTextRT.anchorMin = Vector2.zero;
                btnTextRT.anchorMax = Vector2.one;
                btnTextRT.offsetMin = new Vector2(10f, 4f);
                btnTextRT.offsetMax = new Vector2(-10f, -4f);

                TextMeshProUGUI tmpText = btnTextT.GetComponent<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = "Add to Cart";
                    tmpText.fontSize = 20f;
                    tmpText.fontStyle = FontStyles.Bold;
                    tmpText.enableAutoSizing = true;
                    tmpText.fontSizeMin = 14f;
                    tmpText.fontSizeMax = 20f;
                    tmpText.textWrappingMode = TextWrappingModes.NoWrap;
                    tmpText.overflowMode = TextOverflowModes.Ellipsis;
                    tmpText.alignment = TextAlignmentOptions.Center;
                    tmpText.color = Color.white;
                    tmpText.raycastTarget = false;
                    EditorUtility.SetDirty(tmpText);
                }
                EditorUtility.SetDirty(btnTextT.gameObject);
            }

            SerializedObject so = new SerializedObject(stationBtn);
            so.FindProperty("m_Button").objectReferenceValue = btn;
            if (btnTextT != null)
            {
                so.FindProperty("m_LabelText").objectReferenceValue = btnTextT.GetComponent<TextMeshProUGUI>();
            }
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(stationBtn);
            EditorUtility.SetDirty(buttonGO);
            EditorUtility.SetDirty(signGO);
            formattedCount++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        if (logSummary)
        {
            Debug.Log($"[CartSignFormatter] Successfully reformatted {formattedCount} Cart Signs with high-res, non-wrapping VR UI layout!");
        }
    }
}
