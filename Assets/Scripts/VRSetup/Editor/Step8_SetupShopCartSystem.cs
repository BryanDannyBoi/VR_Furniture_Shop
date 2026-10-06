// ============================================================
// Step8_SetupShopCartSystem.cs
// Editor-only script.
//
// Run via:  VR Furniture Shop > Step 8 - Setup Add to Cart System
//
// What it does:
//   1. Ensures the scene contains an EventSystem with XRUIInputModule
//      so VR controller ray pointers (and mouse) can click UI buttons.
//   2. Ensures the single-instance ShopCartManager GameObject is in the scene.
//   3. For each of the 5 stations, adds a small sign on the RIGHT side
//      of each cupboard with item details and an "Add to Cart" button.
//   4. Creates the "Cart Summary" world-space Canvas beside the entrance
//      (Option A) with live running total, item list, and "Buy" button with
//      on-screen order confirmation.
//   5. Saves the scene.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;

public static class Step8_SetupShopCartSystem
{
    private struct StationData
    {
        public string stationName;
        public string itemName;
        public float price;

        public StationData(string station, string item, float itemPrice)
        {
            stationName = station;
            itemName = item;
            price = itemPrice;
        }
    }

    private static readonly StationData[] s_Stations = new[]
    {
        new StationData("Station_01", "Brown Multi-Utility", 750f),
        new StationData("Station_02", "Light Wood with Drawers", 650f),
        new StationData("Station_03", "Dark Cream Wood with Larger Drawer", 700f),
        new StationData("Station_04", "Light Wood with Multi Mirrors", 600f),
        new StationData("Station_05", "Elegant Purple Wood", 550f),
    };

    // Right-side sign placement relative to each station origin
    // Position: +1.05m to the right, 0.85m height (chest level), -0.30m forward (angled toward player)
    private static readonly Vector3 SIGN_LOCAL_POS = new Vector3(1.05f, 0.85f, -0.30f);
    private static readonly Quaternion SIGN_LOCAL_ROT = Quaternion.Euler(0f, -15f, 0f);
    private const float SIGN_WIDTH = 440f;
    private const float SIGN_HEIGHT = 280f;
    private const float SIGN_SCALE = 0.001f;

    // Cart Summary Canvas placement beside the entrance (Option A)
    private static readonly Vector3 SUMMARY_POS = new Vector3(-1.65f, 1.30f, -2.40f);
    private static readonly Quaternion SUMMARY_ROT = Quaternion.Euler(0f, 90f, 0f);
    private const float SUMMARY_WIDTH = 80f;
    private const float SUMMARY_HEIGHT = 60f;
    private const float SUMMARY_SCALE = 0.01f;

    [MenuItem("VR Furniture Shop/Step 8 - Setup Add to Cart System")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step8] Exit Play Mode before running this script.");
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        }

        // --------------------------------------------------------
        // 1. Ensure EventSystem with XRUIInputModule exists in scene
        // --------------------------------------------------------
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        GameObject eventSystemGO;
        if (eventSystem == null)
        {
            eventSystemGO = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create EventSystem");
            eventSystem = eventSystemGO.AddComponent<EventSystem>();
        }
        else
        {
            eventSystemGO = eventSystem.gameObject;
        }

        XRUIInputModule xrInputModule = eventSystemGO.GetComponent<XRUIInputModule>();
        if (xrInputModule == null)
        {
            xrInputModule = Undo.AddComponent<XRUIInputModule>(eventSystemGO);
        }
        xrInputModule.enableBuiltinActionsAsFallback = true;
        xrInputModule.enableXRInput = true;
        xrInputModule.enableMouseInput = true;
        xrInputModule.enableTouchInput = true;

        StandaloneInputModule standalone = eventSystemGO.GetComponent<StandaloneInputModule>();
        if (standalone != null)
        {
            Undo.DestroyObjectImmediate(standalone);
        }

        Debug.Log("[Step8] EventSystem verified with XRUIInputModule.");

        // --------------------------------------------------------
        // 2. Ensure ShopCartManager singleton GameObject exists
        // --------------------------------------------------------
        ShopCartManager cartManager = Object.FindFirstObjectByType<ShopCartManager>();
        if (cartManager == null)
        {
            GameObject cartGO = new GameObject("ShopCartManager");
            Undo.RegisterCreatedObjectUndo(cartGO, "Create ShopCartManager");
            cartManager = cartGO.AddComponent<ShopCartManager>();
        }
        Debug.Log("[Step8] ShopCartManager verified in scene.");

        // --------------------------------------------------------
        // 3. Add small sign on the RIGHT side of each cupboard
        // --------------------------------------------------------
        Camera mainCam = Camera.main;
        int signsConfigured = 0;

        foreach (var data in s_Stations)
        {
            GameObject station = GameObject.Find(data.stationName);
            if (station == null)
            {
                Debug.LogError($"[Step8] Station '{data.stationName}' not found in scene!");
                continue;
            }

            // Remove any legacy CartButton_Canvas if it exists from earlier tests
            Transform oldCanvas = station.transform.Find("CartButton_Canvas");
            if (oldCanvas != null)
            {
                Undo.DestroyObjectImmediate(oldCanvas.gameObject);
            }

            // Find or create Cart_Sign child
            Transform signT = station.transform.Find("Cart_Sign");
            GameObject signGO;
            if (signT == null)
            {
                signGO = new GameObject("Cart_Sign");
                Undo.RegisterCreatedObjectUndo(signGO, $"Create Cart_Sign for {data.stationName}");
                signGO.transform.SetParent(station.transform, worldPositionStays: false);
            }
            else
            {
                signGO = signT.gameObject;
            }

            signGO.transform.localPosition = SIGN_LOCAL_POS;
            signGO.transform.localRotation = SIGN_LOCAL_ROT;
            signGO.transform.localScale = Vector3.one * SIGN_SCALE;

            Canvas canvas = signGO.GetComponent<Canvas>();
            if (canvas == null) canvas = signGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            if (mainCam != null) canvas.worldCamera = mainCam;

            RectTransform canvasRT = signGO.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(SIGN_WIDTH, SIGN_HEIGHT);

            if (signGO.GetComponent<GraphicRaycaster>() == null)
                signGO.AddComponent<GraphicRaycaster>();
            if (signGO.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                signGO.AddComponent<TrackedDeviceGraphicRaycaster>();

            // Sign Background Plate
            Image bgImage = signGO.GetComponent<Image>();
            if (bgImage == null) bgImage = signGO.AddComponent<Image>();
            bgImage.color = new Color(0.12f, 0.14f, 0.18f, 0.95f); // Elegant dark slate

            // Sign Header (Item Name + Price)
            Transform headerT = signGO.transform.Find("Sign_Header");
            GameObject headerGO;
            if (headerT == null)
            {
                headerGO = new GameObject("Sign_Header");
                Undo.RegisterCreatedObjectUndo(headerGO, $"Create Sign_Header for {data.stationName}");
                headerGO.transform.SetParent(signGO.transform, worldPositionStays: false);
            }
            else
            {
                headerGO = headerT.gameObject;
            }

            RectTransform headerRT = headerGO.GetComponent<RectTransform>();
            if (headerRT == null) headerRT = headerGO.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0.05f, 0.44f);
            headerRT.anchorMax = new Vector2(0.95f, 0.94f);
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;

            TextMeshProUGUI headerTMP = headerGO.GetComponent<TextMeshProUGUI>();
            if (headerTMP == null) headerTMP = headerGO.AddComponent<TextMeshProUGUI>();
            headerTMP.text = $"<b>{data.itemName}</b>\n<color=#66BB6A><size=26>${data.price:F0}</size></color>";
            headerTMP.fontSize = 22f;
            headerTMP.enableAutoSizing = true;
            headerTMP.fontSizeMin = 14f;
            headerTMP.fontSizeMax = 22f;
            headerTMP.alignment = TextAlignmentOptions.Center;
            headerTMP.color = Color.white;
            headerTMP.raycastTarget = false;

            // Add/Remove Button
            Transform buttonT = signGO.transform.Find("Cart_Button");
            GameObject buttonGO;
            if (buttonT == null)
            {
                buttonGO = new GameObject("Cart_Button");
                Undo.RegisterCreatedObjectUndo(buttonGO, $"Create Cart_Button for {data.stationName}");
                buttonGO.transform.SetParent(signGO.transform, worldPositionStays: false);
            }
            else
            {
                buttonGO = buttonT.gameObject;
            }

            RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
            if (buttonRT == null) buttonRT = buttonGO.AddComponent<RectTransform>();
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

            // Button Label Text
            Transform btnTextT = buttonGO.transform.Find("Button_Text");
            GameObject btnTextGO;
            if (btnTextT == null)
            {
                btnTextGO = new GameObject("Button_Text");
                Undo.RegisterCreatedObjectUndo(btnTextGO, $"Create Button_Text for {data.stationName}");
                btnTextGO.transform.SetParent(buttonGO.transform, worldPositionStays: false);
            }
            else
            {
                btnTextGO = btnTextT.gameObject;
            }

            RectTransform btnTextRT = btnTextGO.GetComponent<RectTransform>();
            if (btnTextRT == null) btnTextRT = btnTextGO.AddComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.offsetMin = new Vector2(10f, 4f);
            btnTextRT.offsetMax = new Vector2(-10f, -4f);

            TextMeshProUGUI tmpText = btnTextGO.GetComponent<TextMeshProUGUI>();
            if (tmpText == null) tmpText = btnTextGO.AddComponent<TextMeshProUGUI>();
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

            // StationCartButton component
            StationCartButton stationBtn = buttonGO.GetComponent<StationCartButton>();
            if (stationBtn == null) stationBtn = buttonGO.AddComponent<StationCartButton>();
            stationBtn.SetItemDetails(data.itemName, data.price);

            SerializedObject so = new SerializedObject(stationBtn);
            so.FindProperty("m_ItemName").stringValue = data.itemName;
            so.FindProperty("m_Price").floatValue = data.price;
            so.FindProperty("m_Button").objectReferenceValue = btn;
            so.FindProperty("m_LabelText").objectReferenceValue = tmpText;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(stationBtn);
            EditorUtility.SetDirty(buttonGO);
            EditorUtility.SetDirty(signGO);
            signsConfigured++;

            Debug.Log($"[Step8] Configured right-side sign for {data.stationName}: Item='{data.itemName}', Price=${data.price}");
        }

        // --------------------------------------------------------
        // 4. Create Cart Summary World-Space Canvas (Option A: Beside Entrance)
        // --------------------------------------------------------
        GameObject summaryGO = GameObject.Find("Cart_Summary_Canvas");
        if (summaryGO == null)
        {
            summaryGO = new GameObject("Cart_Summary_Canvas");
            Undo.RegisterCreatedObjectUndo(summaryGO, "Create Cart_Summary_Canvas");
        }

        summaryGO.transform.position = SUMMARY_POS;
        summaryGO.transform.rotation = SUMMARY_ROT;
        summaryGO.transform.localScale = Vector3.one * SUMMARY_SCALE;

        Canvas summaryCanvas = summaryGO.GetComponent<Canvas>();
        if (summaryCanvas == null) summaryCanvas = summaryGO.AddComponent<Canvas>();
        summaryCanvas.renderMode = RenderMode.WorldSpace;
        if (mainCam != null) summaryCanvas.worldCamera = mainCam;

        RectTransform summaryRT = summaryGO.GetComponent<RectTransform>();
        summaryRT.sizeDelta = new Vector2(SUMMARY_WIDTH, SUMMARY_HEIGHT);

        if (summaryGO.GetComponent<GraphicRaycaster>() == null)
            summaryGO.AddComponent<GraphicRaycaster>();
        if (summaryGO.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
            summaryGO.AddComponent<TrackedDeviceGraphicRaycaster>();

        // Background Panel
        Image summaryBg = summaryGO.GetComponent<Image>();
        if (summaryBg == null) summaryBg = summaryGO.AddComponent<Image>();
        summaryBg.color = new Color(0.10f, 0.12f, 0.16f, 0.95f); // Deep dark kiosk slate

        // Title Header
        Transform titleT = summaryGO.transform.Find("Title_Text");
        GameObject titleGO;
        if (titleT == null)
        {
            titleGO = new GameObject("Title_Text");
            Undo.RegisterCreatedObjectUndo(titleGO, "Create Title_Text on Cart Summary");
            titleGO.transform.SetParent(summaryGO.transform, worldPositionStays: false);
        }
        else
        {
            titleGO = titleT.gameObject;
        }

        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        if (titleRT == null) titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.05f, 0.82f);
        titleRT.anchorMax = new Vector2(0.95f, 0.97f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;

        TextMeshProUGUI titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        if (titleTMP == null) titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "<b><size=20>SHOWROOM CART</size></b>";
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        titleTMP.raycastTarget = false;

        // Items List Display
        Transform itemsT = summaryGO.transform.Find("Items_List");
        GameObject itemsGO;
        if (itemsT == null)
        {
            itemsGO = new GameObject("Items_List");
            Undo.RegisterCreatedObjectUndo(itemsGO, "Create Items_List on Cart Summary");
            itemsGO.transform.SetParent(summaryGO.transform, worldPositionStays: false);
        }
        else
        {
            itemsGO = itemsT.gameObject;
        }

        RectTransform itemsRT = itemsGO.GetComponent<RectTransform>();
        if (itemsRT == null) itemsRT = itemsGO.AddComponent<RectTransform>();
        itemsRT.anchorMin = new Vector2(0.08f, 0.38f);
        itemsRT.anchorMax = new Vector2(0.92f, 0.80f);
        itemsRT.offsetMin = Vector2.zero;
        itemsRT.offsetMax = Vector2.zero;

        TextMeshProUGUI itemsTMP = itemsGO.GetComponent<TextMeshProUGUI>();
        if (itemsTMP == null) itemsTMP = itemsGO.AddComponent<TextMeshProUGUI>();
        itemsTMP.text = "<color=#9E9E9E>Cart is currently empty.\nVisit stations to add furniture.</color>";
        itemsTMP.fontSize = 11f;
        itemsTMP.alignment = TextAlignmentOptions.TopLeft;
        itemsTMP.color = Color.white;
        itemsTMP.raycastTarget = false;

        // Total Text
        Transform totalT = summaryGO.transform.Find("Total_Text");
        GameObject totalGO;
        if (totalT == null)
        {
            totalGO = new GameObject("Total_Text");
            Undo.RegisterCreatedObjectUndo(totalGO, "Create Total_Text on Cart Summary");
            totalGO.transform.SetParent(summaryGO.transform, worldPositionStays: false);
        }
        else
        {
            totalGO = totalT.gameObject;
        }

        RectTransform totalRT = totalGO.GetComponent<RectTransform>();
        if (totalRT == null) totalRT = totalGO.AddComponent<RectTransform>();
        totalRT.anchorMin = new Vector2(0.08f, 0.25f);
        totalRT.anchorMax = new Vector2(0.92f, 0.36f);
        totalRT.offsetMin = Vector2.zero;
        totalRT.offsetMax = Vector2.zero;

        TextMeshProUGUI totalTMP = totalGO.GetComponent<TextMeshProUGUI>();
        if (totalTMP == null) totalTMP = totalGO.AddComponent<TextMeshProUGUI>();
        totalTMP.text = "<b>Total: $0.00</b>";
        totalTMP.fontSize = 15f;
        totalTMP.alignment = TextAlignmentOptions.Left;
        totalTMP.color = new Color(0.51f, 0.78f, 0.52f); // Light green
        totalTMP.raycastTarget = false;

        // Buy Button
        Transform buyBtnT = summaryGO.transform.Find("Buy_Button");
        GameObject buyBtnGO;
        if (buyBtnT == null)
        {
            buyBtnGO = new GameObject("Buy_Button");
            Undo.RegisterCreatedObjectUndo(buyBtnGO, "Create Buy_Button on Cart Summary");
            buyBtnGO.transform.SetParent(summaryGO.transform, worldPositionStays: false);
        }
        else
        {
            buyBtnGO = buyBtnT.gameObject;
        }

        RectTransform buyBtnRT = buyBtnGO.GetComponent<RectTransform>();
        if (buyBtnRT == null) buyBtnRT = buyBtnGO.AddComponent<RectTransform>();
        buyBtnRT.anchorMin = new Vector2(0.20f, 0.08f);
        buyBtnRT.anchorMax = new Vector2(0.80f, 0.22f);
        buyBtnRT.offsetMin = Vector2.zero;
        buyBtnRT.offsetMax = Vector2.zero;

        Image buyBtnImg = buyBtnGO.GetComponent<Image>();
        if (buyBtnImg == null) buyBtnImg = buyBtnGO.AddComponent<Image>();
        buyBtnImg.color = new Color(0.12f, 0.45f, 0.75f, 1f); // Vibrant royal blue

        Button buyBtn = buyBtnGO.GetComponent<Button>();
        if (buyBtn == null) buyBtn = buyBtnGO.AddComponent<Button>();
        buyBtn.targetGraphic = buyBtnImg;

        ColorBlock buyCb = buyBtn.colors;
        buyCb.normalColor = Color.white;
        buyCb.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        buyCb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        buyBtn.colors = buyCb;

        // Buy Button Text
        Transform buyTextT = buyBtnGO.transform.Find("Buy_Text");
        GameObject buyTextGO;
        if (buyTextT == null)
        {
            buyTextGO = new GameObject("Buy_Text");
            Undo.RegisterCreatedObjectUndo(buyTextGO, "Create Buy_Text on Buy Button");
            buyTextGO.transform.SetParent(buyBtnGO.transform, worldPositionStays: false);
        }
        else
        {
            buyTextGO = buyTextT.gameObject;
        }

        RectTransform buyTextRT = buyTextGO.GetComponent<RectTransform>();
        if (buyTextRT == null) buyTextRT = buyTextGO.AddComponent<RectTransform>();
        buyTextRT.anchorMin = Vector2.zero;
        buyTextRT.anchorMax = Vector2.one;
        buyTextRT.offsetMin = Vector2.zero;
        buyTextRT.offsetMax = Vector2.zero;

        TextMeshProUGUI buyTMP = buyTextGO.GetComponent<TextMeshProUGUI>();
        if (buyTMP == null) buyTMP = buyTextGO.AddComponent<TextMeshProUGUI>();
        buyTMP.text = "Buy Now";
        buyTMP.fontSize = 14f;
        buyTMP.fontStyle = FontStyles.Bold;
        buyTMP.alignment = TextAlignmentOptions.Center;
        buyTMP.color = Color.white;
        buyTMP.raycastTarget = false;

        // Order Confirmation Text (hidden initially, shown on click)
        Transform confirmT = summaryGO.transform.Find("Confirmation_Text");
        GameObject confirmGO;
        if (confirmT == null)
        {
            confirmGO = new GameObject("Confirmation_Text");
            Undo.RegisterCreatedObjectUndo(confirmGO, "Create Confirmation_Text on Cart Summary");
            confirmGO.transform.SetParent(summaryGO.transform, worldPositionStays: false);
        }
        else
        {
            confirmGO = confirmT.gameObject;
        }

        RectTransform confirmRT = confirmGO.GetComponent<RectTransform>();
        if (confirmRT == null) confirmRT = confirmGO.AddComponent<RectTransform>();
        confirmRT.anchorMin = new Vector2(0.05f, 0.01f);
        confirmRT.anchorMax = new Vector2(0.95f, 0.08f);
        confirmRT.offsetMin = Vector2.zero;
        confirmRT.offsetMax = Vector2.zero;

        TextMeshProUGUI confirmTMP = confirmGO.GetComponent<TextMeshProUGUI>();
        if (confirmTMP == null) confirmTMP = confirmGO.AddComponent<TextMeshProUGUI>();
        confirmTMP.text = "";
        confirmTMP.fontSize = 11f;
        confirmTMP.alignment = TextAlignmentOptions.Center;
        confirmTMP.color = new Color(0.51f, 0.78f, 0.52f);
        confirmTMP.raycastTarget = false;
        confirmGO.SetActive(false);

        // CartSummaryUI component
        CartSummaryUI summaryUI = summaryGO.GetComponent<CartSummaryUI>();
        if (summaryUI == null) summaryUI = summaryGO.AddComponent<CartSummaryUI>();

        SerializedObject summarySO = new SerializedObject(summaryUI);
        summarySO.FindProperty("m_ItemsListText").objectReferenceValue = itemsTMP;
        summarySO.FindProperty("m_TotalText").objectReferenceValue = totalTMP;
        summarySO.FindProperty("m_ConfirmationText").objectReferenceValue = confirmTMP;
        summarySO.FindProperty("m_BuyButton").objectReferenceValue = buyBtn;
        summarySO.ApplyModifiedProperties();

        EditorUtility.SetDirty(summaryUI);
        EditorUtility.SetDirty(summaryGO);

        // Mark scene dirty and save
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log($"[Step8] Setup complete! Configured {signsConfigured} right-side cupboard signs, EventSystem, and Entrance Cart Summary Canvas.");
    }
}
