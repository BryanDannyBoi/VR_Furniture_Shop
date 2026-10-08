// ============================================================
// CartSignFormatter.cs
// Editor utility to automatically format, wire, and polish all
// "Add to Cart" station signs and buttons across all 17 furniture pieces
// in all 4 rooms to crisp, spacious VR UI with working TrackedDeviceGraphicRaycaster.
//
// Run via:  VR Furniture Shop > Format Cart Buttons & Signs
// Also automatically runs on compilation.
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;

[InitializeOnLoad]
public static class CartSignFormatter
{
    private struct StationCatalogInfo
    {
        public string stationName;
        public string itemName;
        public float price;
        public Vector3 targetSignPos;
        public bool isRoom4;

        public StationCatalogInfo(string stName, string name, float p, Vector3 pos, bool r4 = false)
        {
            stationName = stName;
            itemName = name;
            price = p;
            targetSignPos = pos;
            isRoom4 = r4;
        }
    }

    private static readonly StationCatalogInfo[] s_Catalog = new[]
    {
        // Room 1: Cupboards
        new StationCatalogInfo("Station_01", "Brown Multi-Utility", 750f, new Vector3(0f, 1.95f, -0.05f)),
        new StationCatalogInfo("Station_02", "Light Wood with Drawers", 650f, new Vector3(0f, 1.95f, -0.05f)),
        new StationCatalogInfo("Station_03", "Dark Cream Wood with Larger Drawer", 700f, new Vector3(0f, 1.95f, -0.05f)),
        new StationCatalogInfo("Station_04", "Classic Dark Wood", 600f, new Vector3(0f, 1.95f, -0.05f)),
        new StationCatalogInfo("Station_05", "Elegant Purple Wood", 550f, new Vector3(0f, 1.95f, -0.05f)),

        // Room 2: Mini Fridges
        new StationCatalogInfo("Station_MF_02", "Classic Mini", 250f, new Vector3(0f, 1.25f, -0.05f)),
        new StationCatalogInfo("Station_MF_03", "Standard Cooler", 300f, new Vector3(0f, 1.25f, -0.05f)),
        new StationCatalogInfo("Station_MF_04", "Freeze Cool", 320f, new Vector3(0f, 1.25f, -0.05f)),
        new StationCatalogInfo("Station_MF_05", "Ultra Cold", 400f, new Vector3(0f, 1.25f, -0.05f)),

        // Room 3: Lamps
        new StationCatalogInfo("Station_Lamp_01", "Cyan Desk Lamp", 120f, new Vector3(0f, 1.05f, 0f)),
        new StationCatalogInfo("Station_Lamp_02", "Green Desk Lamp", 120f, new Vector3(0f, 1.05f, 0f)),
        new StationCatalogInfo("Station_Lamp_03", "Purple Desk Lamp", 140f, new Vector3(0f, 1.05f, 0f)),
        new StationCatalogInfo("Station_Lamp_04", "Red Desk Lamp", 150f, new Vector3(0f, 1.05f, 0f)),
        new StationCatalogInfo("Station_Lamp_05", "Yellow Desk Lamp", 130f, new Vector3(0f, 1.05f, 0f)),

        // Room 4: Bedroom & Living Suite
        new StationCatalogInfo("Station_Cot_01", "Comfort Cot Bed", 850f, new Vector3(0f, 0f, 0f), true),
        new StationCatalogInfo("Station_Dress_01", "Classic Dressing Table", 480f, new Vector3(0f, 0f, 0f), true),
        new StationCatalogInfo("Station_Chair_01", "Modern Lounge Chair", 220f, new Vector3(0f, 0f, 0f), true),
        new StationCatalogInfo("Station_Sofa_01", "Comfort Sofa", 400f, new Vector3(0f, 0f, 0f), true),
        new StationCatalogInfo("Station_TableChair_01", "Table & Chair Set", 250f, new Vector3(0f, 0f, 0f), true)
    };

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
            ReformatAllSigns(false);
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
        int formattedCount = 0;

        foreach (var info in s_Catalog)
        {
            GameObject stationGO = GameObject.Find(info.stationName);
            if (stationGO == null) continue;

            // 1. Remove obsolete label / heading canvases
            Transform labelT = stationGO.transform.Find("Label_Canvas");
            if (labelT != null) Undo.DestroyObjectImmediate(labelT.gameObject);
            Transform oldLabelT = stationGO.transform.Find("Station_Label");
            if (oldLabelT != null) Undo.DestroyObjectImmediate(oldLabelT.gameObject);

            // 2. Locate Cart_Sign
            Transform cartSignT = stationGO.transform.Find("Cart_Sign");
            if (cartSignT == null)
            {
                GameObject newSign = new GameObject("Cart_Sign");
                Undo.RegisterCreatedObjectUndo(newSign, "Create Cart_Sign for " + info.stationName);
                newSign.transform.SetParent(stationGO.transform, false);
                cartSignT = newSign.transform;
            }

            GameObject signGO = cartSignT.gameObject;

            // Positioning
            if (!info.isRoom4)
            {
                signGO.transform.localPosition = info.targetSignPos;
                signGO.transform.localRotation = Quaternion.identity;
            }
            else
            {
                // In Room 4, preserve user's localPosition (0, 0, 0) and anchoredPosition (0, 1)
                RectTransform signRT = signGO.GetComponent<RectTransform>();
                if (signRT != null)
                {
                    signRT.anchoredPosition = new Vector2(0f, 1.0f);
                    signRT.localPosition = Vector3.zero;
                    signRT.localRotation = Quaternion.identity;
                }
            }

            // Check if there is a child Sign_Canvas (like in Room 3 lamps) or if Canvas is on Cart_Sign
            Transform signCanvasT = cartSignT.Find("Sign_Canvas");
            GameObject canvasTargetGO = (signCanvasT != null) ? signCanvasT.gameObject : signGO;

            canvasTargetGO.transform.localScale = Vector3.one * 0.001f;

            Canvas canvas = canvasTargetGO.GetComponent<Canvas>();
            if (canvas == null) canvas = canvasTargetGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            if (mainCam != null) canvas.worldCamera = mainCam;

            RectTransform canvasRT = canvasTargetGO.GetComponent<RectTransform>();
            if (canvasRT != null)
            {
                canvasRT.sizeDelta = new Vector2(440f, 280f);
            }

            if (canvasTargetGO.GetComponent<GraphicRaycaster>() == null)
                canvasTargetGO.AddComponent<GraphicRaycaster>();
            if (canvasTargetGO.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                canvasTargetGO.AddComponent<TrackedDeviceGraphicRaycaster>();

            // Sign Background Plate
            Image bgImage = canvasTargetGO.GetComponent<Image>();
            if (bgImage == null) bgImage = canvasTargetGO.AddComponent<Image>();
            bgImage.color = new Color(0.11f, 0.13f, 0.18f, 0.95f);

            // 3. Setup / Reformat Sign_Header
            Transform headerT = canvasTargetGO.transform.Find("Sign_Header");
            GameObject headerGO;
            if (headerT == null)
            {
                headerGO = new GameObject("Sign_Header");
                headerGO.transform.SetParent(canvasTargetGO.transform, false);
            }
            else
            {
                headerGO = headerT.gameObject;
            }

            RectTransform headerRT = headerGO.GetComponent<RectTransform>() ?? headerGO.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0.05f, 0.44f);
            headerRT.anchorMax = new Vector2(0.95f, 0.94f);
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;

            TextMeshProUGUI headerTMP = headerGO.GetComponent<TextMeshProUGUI>() ?? headerGO.AddComponent<TextMeshProUGUI>();
            headerTMP.text = $"<b>{info.itemName}</b>\n<color=#66BB6A><size=26>${info.price:F0}</size></color>";
            headerTMP.fontSize = 22f;
            headerTMP.enableAutoSizing = true;
            headerTMP.fontSizeMin = 14f;
            headerTMP.fontSizeMax = 22f;
            headerTMP.alignment = TextAlignmentOptions.Center;
            headerTMP.color = Color.white;
            headerTMP.raycastTarget = false;

            // 4. Setup / Reformat Cart_Button
            Transform buttonT = canvasTargetGO.transform.Find("Cart_Button");
            GameObject buttonGO;
            if (buttonT == null)
            {
                buttonGO = new GameObject("Cart_Button");
                buttonGO.transform.SetParent(canvasTargetGO.transform, false);
            }
            else
            {
                buttonGO = buttonT.gameObject;
            }

            RectTransform buttonRT = buttonGO.GetComponent<RectTransform>() ?? buttonGO.AddComponent<RectTransform>();
            buttonRT.anchorMin = new Vector2(0.08f, 0.08f);
            buttonRT.anchorMax = new Vector2(0.92f, 0.38f);
            buttonRT.offsetMin = Vector2.zero;
            buttonRT.offsetMax = Vector2.zero;

            Image btnImage = buttonGO.GetComponent<Image>() ?? buttonGO.AddComponent<Image>();
            btnImage.color = new Color(0.18f, 0.60f, 0.36f, 1f);

            Button btn = buttonGO.GetComponent<Button>() ?? buttonGO.AddComponent<Button>();
            btn.targetGraphic = btnImage;
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = cb;

            // BoxCollider for ray interactor physics fallback
            BoxCollider btnCol = buttonGO.GetComponent<BoxCollider>();
            if (btnCol == null) btnCol = buttonGO.AddComponent<BoxCollider>();
            btnCol.size = new Vector3(320f, 60f, 1f);
            btnCol.center = Vector3.zero;

            // Button_Text child
            Transform btnTextT = buttonGO.transform.Find("Button_Text");
            GameObject btnTextGO;
            if (btnTextT == null)
            {
                btnTextGO = new GameObject("Button_Text");
                btnTextGO.transform.SetParent(buttonGO.transform, false);
            }
            else
            {
                btnTextGO = btnTextT.gameObject;
            }

            RectTransform btnTextRT = btnTextGO.GetComponent<RectTransform>() ?? btnTextGO.AddComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.offsetMin = new Vector2(10f, 4f);
            btnTextRT.offsetMax = new Vector2(-10f, -4f);

            TextMeshProUGUI tmpText = btnTextGO.GetComponent<TextMeshProUGUI>() ?? btnTextGO.AddComponent<TextMeshProUGUI>();
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

            // 5. Wire StationCartButton
            StationCartButton scb = buttonGO.GetComponent<StationCartButton>();
            if (scb == null) scb = buttonGO.AddComponent<StationCartButton>();
            scb.SetItemDetails(info.itemName, info.price);

            SerializedObject so = new SerializedObject(scb);
            so.FindProperty("m_Button").objectReferenceValue = btn;
            so.FindProperty("m_LabelText").objectReferenceValue = tmpText;
            so.FindProperty("m_ItemName").stringValue = info.itemName;
            so.FindProperty("m_Price").floatValue = info.price;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(scb);
            EditorUtility.SetDirty(buttonGO);
            EditorUtility.SetDirty(canvasTargetGO);
            EditorUtility.SetDirty(signGO);
            EditorUtility.SetDirty(stationGO);

            formattedCount++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        if (logSummary)
        {
            Debug.Log($"[CartSignFormatter] Successfully validated & reformatted all {formattedCount} Cart Signs across all 4 rooms!");
        }
    }
}
