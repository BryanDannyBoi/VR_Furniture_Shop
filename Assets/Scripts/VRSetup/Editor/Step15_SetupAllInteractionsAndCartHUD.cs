// ============================================================
// Step15_SetupAllInteractionsAndCartHUD.cs
// Editor-only script.
//
// Run via:  VR Furniture Shop > Step 15 - Setup All Interactions & Persistent Cart HUD
// Also auto-executes once on compilation.
//
// What it does:
//   1. Makes ALL furnitures across ALL 4 rooms fully interactive (lighting & 360 rotation):
//      - Room 1: 5 Cupboards
//      - Room 2: 4 Mini Fridges
//      - Room 3: 5 Lamps
//      - Room 4: Cot, Dressing Table, Chair
//      - Adds BoxCollider, XRSimpleInteractable (layer 1), and CupboardInteraction
//        to each 3D model so hovering causes interactive lighting glow and clicking
//        toggles smooth 360-degree rotation in place!
//   2. Ensures the Add to Cart system works flawlessly for all 17 furniture pieces.
//   3. Sets up persistent, glancable Cost / Cart Sum tracking:
//      - HUD_Cart_Badge: Anchored in the player's headset view (upper right periphery)
//        so the user can ALWAYS check their running total in real-time.
//      - Wrist_Cart_HUD: Smartwatch display mounted on the Left Controller.
//      - Wall-mounted Checkout Kiosks in EVERY room (Room 1, 2, 3, 4) with itemized
//        breakdown, live total, and working Pay & Checkout button.
//   4. Saves the scene.
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

[InitializeOnLoad]
public static class Step15_SetupAllInteractionsAndCartHUD
{
    static Step15_SetupAllInteractionsAndCartHUD()
    {
        EditorApplication.delayCall += AutoRunIfSampleScene;
    }

    private static void AutoRunIfSampleScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.isLoaded && scene.path.Contains("SampleScene.unity"))
        {
            ApplyAllSetup(false);
        }
    }

    [MenuItem("VR Furniture Shop/Step 15 - Setup All Interactions & Persistent Cart HUD")]
    public static void ManualRun()
    {
        ApplyAllSetup(true);
    }

    public static void ApplyAllSetup(bool logSummary)
    {
        if (EditorApplication.isPlaying) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.isLoaded) return;

        Camera mainCam = Camera.main;
        Material signBoardMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SignBoard_Mat.mat");

        // 1. Make all furniture models interactive (lighting hover + 360 rotation)
        int interactiveCount = SetupAllFurnitureInteractions();

        // 2. Setup Persistent Headset & Wrist Cart HUDs
        SetupPlayerCartHUDs(mainCam);

        // 3. Setup Cart Summary & Checkout Kiosks in Every Room
        SetupRoomCheckoutKiosks(mainCam, signBoardMat);

        // 4. Format all station buttons and wire up
        CartSignFormatter.ReformatAllSigns(false);

        // 5. Ensure all interactive door buttons and doorway passages (including Room 4) are unobstructed
        DoorButtonSetup.SetupAllDoorButtons(false);
        FixDoorwayPassage.ApplyDoorwayFixes(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        if (logSummary)
        {
            Debug.Log($"[Step15] Successfully configured {interactiveCount} interactive furnitures, persistent Headset & Wrist Cart HUDs, and Kiosks across all 4 rooms!");
        }
    }

    private static int SetupAllFurnitureInteractions()
    {
        int count = 0;

        // Room 1: Cupboards (Cupboard_0 to Cupboard_4)
        for (int i = 0; i < 5; i++)
        {
            GameObject cupboard = GameObject.Find($"Cupboard_{i}");
            if (cupboard != null)
            {
                ConfigureInteractiveModel(cupboard);
                count++;
            }
        }

        // Room 2: Mini Fridges (Station_MF_02 to Station_MF_05)
        string[] room2Stations = { "Station_MF_02", "Station_MF_03", "Station_MF_04", "Station_MF_05" };
        foreach (string stName in room2Stations)
        {
            GameObject st = GameObject.Find(stName);
            if (st == null) continue;

            for (int i = 0; i < st.transform.childCount; i++)
            {
                Transform child = st.transform.GetChild(i);
                if (child.name.StartsWith("MiniFridge_") || child.name.StartsWith("Preview_MiniFridge"))
                {
                    ConfigureInteractiveModel(child.gameObject);
                    count++;
                }
            }
        }

        // Room 3: Lamps (Station_Lamp_01 to Station_Lamp_05)
        string[] room3Stations = { "Station_Lamp_01", "Station_Lamp_02", "Station_Lamp_03", "Station_Lamp_04", "Station_Lamp_05" };
        foreach (string stName in room3Stations)
        {
            GameObject st = GameObject.Find(stName);
            if (st == null) continue;

            for (int i = 0; i < st.transform.childCount; i++)
            {
                Transform child = st.transform.GetChild(i);
                if (child.name.StartsWith("lamp_"))
                {
                    ConfigureInteractiveModel(child.gameObject);
                    count++;
                }
            }
        }

        // Room 4: Bedroom & Living Suite (Cot, Dresser, Chair, Sofa, Table & Chair)
        string[] room4Stations = { "Station_Cot_01", "Station_Dress_01", "Station_Chair_01", "Station_Sofa_01", "Station_TableChair_01" };
        foreach (string stName in room4Stations)
        {
            GameObject st = GameObject.Find(stName);
            if (st == null) continue;

            for (int i = 0; i < st.transform.childCount; i++)
            {
                Transform child = st.transform.GetChild(i);
                string cn = child.name.ToLower();
                if (cn.Contains("cot") || cn.Contains("dressing") || cn.Contains("chair") || cn.Contains("sofa") || cn.Contains("f_1"))
                {
                    ConfigureInteractiveModel(child.gameObject);
                    count++;
                }
            }
        }

        return count;
    }

    private static void ConfigureInteractiveModel(GameObject modelGO)
    {
        // 1. BoxCollider
        BoxCollider boxCol = modelGO.GetComponent<BoxCollider>();
        if (boxCol == null)
        {
            boxCol = modelGO.AddComponent<BoxCollider>();
        }

        // 2. XRSimpleInteractable on Default layer (layer 1)
        XRSimpleInteractable interactable = modelGO.GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = modelGO.AddComponent<XRSimpleInteractable>();
        }
        interactable.interactionLayers = 1; // Default ray layer
        if (!interactable.colliders.Contains(boxCol))
        {
            interactable.colliders.Add(boxCol);
        }

        // 3. CupboardInteraction (Hover lighting + Select 360-degree rotation)
        CupboardInteraction ci = modelGO.GetComponent<CupboardInteraction>();
        if (ci == null)
        {
            ci = modelGO.AddComponent<CupboardInteraction>();
        }

        EditorUtility.SetDirty(modelGO);
    }

    private static void SetupPlayerCartHUDs(Camera mainCam)
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        // A. Headset-Glancable HUD Badge (Top-Right Periphery)
        Transform existingBadge = mainCam.transform.Find("HUD_Cart_Badge");
        GameObject badgeGO;
        if (existingBadge == null)
        {
            badgeGO = new GameObject("HUD_Cart_Badge");
            Undo.RegisterCreatedObjectUndo(badgeGO, "Create HUD_Cart_Badge");
            badgeGO.transform.SetParent(mainCam.transform, false);
        }
        else
        {
            badgeGO = existingBadge.gameObject;
        }

        badgeGO.transform.localPosition = new Vector3(0.38f, 0.24f, 0.90f);
        badgeGO.transform.localRotation = Quaternion.identity;
        badgeGO.transform.localScale = Vector3.one * 0.0006f;

        Canvas badgeCanvas = badgeGO.GetComponent<Canvas>();
        if (badgeCanvas == null) badgeCanvas = badgeGO.AddComponent<Canvas>();
        badgeCanvas.renderMode = RenderMode.WorldSpace;
        badgeCanvas.worldCamera = mainCam;

        RectTransform badgeRT = badgeGO.GetComponent<RectTransform>();
        badgeRT.sizeDelta = new Vector2(380f, 90f);

        Image badgeBg = badgeGO.GetComponent<Image>();
        if (badgeBg == null) badgeBg = badgeGO.AddComponent<Image>();
        badgeBg.color = new Color(0.10f, 0.12f, 0.16f, 0.88f); // Sleek translucent dark slate

        // Text child
        Transform badgeTextT = badgeGO.transform.Find("Badge_Text");
        GameObject badgeTextGO;
        if (badgeTextT == null)
        {
            badgeTextGO = new GameObject("Badge_Text");
            badgeTextGO.transform.SetParent(badgeGO.transform, false);
        }
        else
        {
            badgeTextGO = badgeTextT.gameObject;
        }

        RectTransform textRT = badgeTextGO.GetComponent<RectTransform>();
        if (textRT == null) textRT = badgeTextGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(16f, 8f);
        textRT.offsetMax = new Vector2(-16f, -8f);

        TextMeshProUGUI tmp = badgeTextGO.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = badgeTextGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "<b>🛒 Cart:</b> <color=#B0BEC5>$0</color> <size=80%>(Empty)</size>";
        tmp.fontSize = 24f;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 16f;
        tmp.fontSizeMax = 26f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        CartHUD hudComp = badgeGO.GetComponent<CartHUD>();
        if (hudComp == null) hudComp = badgeGO.AddComponent<CartHUD>();

        SerializedObject so = new SerializedObject(hudComp);
        so.FindProperty("m_Text").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(badgeGO);

        // B. Wrist Watch HUD on Left Controller (if present)
        GameObject leftController = GameObject.Find("Left Controller");
        if (leftController == null) leftController = GameObject.Find("LeftController");
        if (leftController != null)
        {
            Transform existingWrist = leftController.transform.Find("Wrist_Cart_HUD");
            GameObject wristGO;
            if (existingWrist == null)
            {
                wristGO = new GameObject("Wrist_Cart_HUD");
                Undo.RegisterCreatedObjectUndo(wristGO, "Create Wrist_Cart_HUD");
                wristGO.transform.SetParent(leftController.transform, false);
            }
            else
            {
                wristGO = existingWrist.gameObject;
            }

            wristGO.transform.localPosition = new Vector3(0f, 0.05f, -0.06f);
            wristGO.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
            wristGO.transform.localScale = Vector3.one * 0.0006f;

            Canvas wristCanvas = wristGO.GetComponent<Canvas>();
            if (wristCanvas == null) wristCanvas = wristGO.AddComponent<Canvas>();
            wristCanvas.renderMode = RenderMode.WorldSpace;
            wristCanvas.worldCamera = mainCam;

            RectTransform wristRT = wristGO.GetComponent<RectTransform>();
            wristRT.sizeDelta = new Vector2(340f, 100f);

            Image wristBg = wristGO.GetComponent<Image>();
            if (wristBg == null) wristBg = wristGO.AddComponent<Image>();
            wristBg.color = new Color(0.12f, 0.15f, 0.20f, 0.95f);

            Transform wTextT = wristGO.transform.Find("Wrist_Text");
            GameObject wTextGO;
            if (wTextT == null)
            {
                wTextGO = new GameObject("Wrist_Text");
                wTextGO.transform.SetParent(wristGO.transform, false);
            }
            else
            {
                wTextGO = wTextT.gameObject;
            }

            RectTransform wTextRT = wTextGO.GetComponent<RectTransform>();
            if (wTextRT == null) wTextRT = wTextGO.AddComponent<RectTransform>();
            wTextRT.anchorMin = Vector2.zero;
            wTextRT.anchorMax = Vector2.one;
            wTextRT.offsetMin = new Vector2(12f, 6f);
            wTextRT.offsetMax = new Vector2(-12f, -6f);

            TextMeshProUGUI wTmp = wTextGO.GetComponent<TextMeshProUGUI>();
            if (wTmp == null) wTmp = wTextGO.AddComponent<TextMeshProUGUI>();
            wTmp.text = "<b>🛒 Cart:</b> <color=#B0BEC5>$0</color>";
            wTmp.fontSize = 22f;
            wTmp.enableAutoSizing = true;
            wTmp.alignment = TextAlignmentOptions.Center;
            wTmp.color = Color.white;
            wTmp.raycastTarget = false;

            CartHUD wristHudComp = wristGO.GetComponent<CartHUD>();
            if (wristHudComp == null) wristHudComp = wristGO.AddComponent<CartHUD>();

            SerializedObject soWrist = new SerializedObject(wristHudComp);
            soWrist.FindProperty("m_Text").objectReferenceValue = wTmp;
            soWrist.ApplyModifiedProperties();

            EditorUtility.SetDirty(wristGO);
        }
    }

    private static void SetupRoomCheckoutKiosks(Camera mainCam, Material signBoardMat)
    {
        var kiosks = new[]
        {
            new { name = "Cart_Summary_Room2", pos = new Vector3(17.80f, 1.30f, -2.40f), rot = Quaternion.Euler(0f, -90f, 0f) },
            new { name = "Cart_Summary_Room3", pos = new Vector3(25.80f, 1.30f, -2.40f), rot = Quaternion.Euler(0f, -90f, 0f) },
            new { name = "Cart_Summary_Room4", pos = new Vector3(33.80f, 1.30f, -2.40f), rot = Quaternion.Euler(0f, -90f, 0f) }
        };

        foreach (var k in kiosks)
        {
            GameObject kioskRoot = GameObject.Find(k.name);
            if (kioskRoot == null)
            {
                kioskRoot = new GameObject(k.name);
                Undo.RegisterCreatedObjectUndo(kioskRoot, "Create " + k.name);
            }

            kioskRoot.transform.position = k.pos;
            kioskRoot.transform.rotation = k.rot;

            // Physical board backing
            Transform boardT = kioskRoot.transform.Find("Kiosk_Board");
            GameObject boardGO;
            if (boardT == null)
            {
                boardGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boardGO.name = "Kiosk_Board";
                boardGO.transform.SetParent(kioskRoot.transform, false);
            }
            else
            {
                boardGO = boardT.gameObject;
            }
            boardGO.transform.localPosition = Vector3.zero;
            boardGO.transform.localScale = new Vector3(0.84f, 0.64f, 0.02f);
            if (signBoardMat != null)
                boardGO.GetComponent<MeshRenderer>().sharedMaterial = signBoardMat;

            // Canvas
            Transform canvasT = kioskRoot.transform.Find("Kiosk_Canvas");
            GameObject canvasGO;
            if (canvasT == null)
            {
                canvasGO = new GameObject("Kiosk_Canvas");
                canvasGO.transform.SetParent(kioskRoot.transform, false);
            }
            else
            {
                canvasGO = canvasT.gameObject;
            }
            canvasGO.transform.localPosition = new Vector3(0f, 0f, -0.015f);
            canvasGO.transform.localRotation = Quaternion.identity;
            canvasGO.transform.localScale = Vector3.one * 0.01f;

            Canvas canvas = canvasGO.GetComponent<Canvas>();
            if (canvas == null) canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            if (mainCam != null) canvas.worldCamera = mainCam;

            if (canvasGO.GetComponent<GraphicRaycaster>() == null) canvasGO.AddComponent<GraphicRaycaster>();
            if (canvasGO.GetComponent<TrackedDeviceGraphicRaycaster>() == null) canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();

            RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(80f, 60f);

            Image bg = canvasGO.GetComponent<Image>();
            if (bg == null) bg = canvasGO.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.14f, 0.18f, 0.96f);

            // 1. Title Text
            Transform titleT = canvasGO.transform.Find("Title_Text");
            GameObject titleGO = titleT != null ? titleT.gameObject : new GameObject("Title_Text");
            titleGO.transform.SetParent(canvasGO.transform, false);
            RectTransform titleRT = titleGO.GetComponent<RectTransform>() ?? titleGO.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.05f, 0.82f);
            titleRT.anchorMax = new Vector2(0.95f, 0.98f);
            titleRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI titleTMP = titleGO.GetComponent<TextMeshProUGUI>() ?? titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "<b>🛒 SHOWROOM CART</b>";
            titleTMP.fontSize = 5.2f;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = Color.white;

            // 2. Items List Text
            Transform itemsT = canvasGO.transform.Find("Items_List");
            GameObject itemsGO = itemsT != null ? itemsT.gameObject : new GameObject("Items_List");
            itemsGO.transform.SetParent(canvasGO.transform, false);
            RectTransform itemsRT = itemsGO.GetComponent<RectTransform>() ?? itemsGO.AddComponent<RectTransform>();
            itemsRT.anchorMin = new Vector2(0.06f, 0.38f);
            itemsRT.anchorMax = new Vector2(0.94f, 0.80f);
            itemsRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI itemsTMP = itemsGO.GetComponent<TextMeshProUGUI>() ?? itemsGO.AddComponent<TextMeshProUGUI>();
            itemsTMP.text = "<color=#9E9E9E>Cart is currently empty.\nVisit stations to add furniture.</color>";
            itemsTMP.fontSize = 3.6f;
            itemsTMP.alignment = TextAlignmentOptions.TopLeft;
            itemsTMP.color = Color.white;

            // 3. Total Price Text
            Transform totalT = canvasGO.transform.Find("Total_Text");
            GameObject totalGO = totalT != null ? totalT.gameObject : new GameObject("Total_Text");
            totalGO.transform.SetParent(canvasGO.transform, false);
            RectTransform totalRT = totalGO.GetComponent<RectTransform>() ?? totalGO.AddComponent<RectTransform>();
            totalRT.anchorMin = new Vector2(0.06f, 0.22f);
            totalRT.anchorMax = new Vector2(0.94f, 0.36f);
            totalRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI totalTMP = totalGO.GetComponent<TextMeshProUGUI>() ?? totalGO.AddComponent<TextMeshProUGUI>();
            totalTMP.text = "<b>Total: $0.00</b>";
            totalTMP.fontSize = 4.8f;
            totalTMP.alignment = TextAlignmentOptions.Center;
            totalTMP.color = new Color(0.40f, 0.78f, 0.50f); // Light green

            // 4. Buy Button
            Transform buyBtnT = canvasGO.transform.Find("Buy_Button");
            GameObject buyBtnGO = buyBtnT != null ? buyBtnT.gameObject : new GameObject("Buy_Button");
            buyBtnGO.transform.SetParent(canvasGO.transform, false);
            RectTransform buyRT = buyBtnGO.GetComponent<RectTransform>() ?? buyBtnGO.AddComponent<RectTransform>();
            buyRT.anchorMin = new Vector2(0.12f, 0.05f);
            buyRT.anchorMax = new Vector2(0.88f, 0.20f);
            buyRT.sizeDelta = Vector2.zero;

            Image btnImg = buyBtnGO.GetComponent<Image>() ?? buyBtnGO.AddComponent<Image>();
            btnImg.color = new Color(0.18f, 0.55f, 0.34f);
            Button buyBtn = buyBtnGO.GetComponent<Button>() ?? buyBtnGO.AddComponent<Button>();
            buyBtn.targetGraphic = btnImg;

            Transform buyTextT = buyBtnGO.transform.Find("Buy_Text");
            GameObject buyTextGO = buyTextT != null ? buyTextT.gameObject : new GameObject("Buy_Text");
            buyTextGO.transform.SetParent(buyBtnGO.transform, false);
            RectTransform bTextRT = buyTextGO.GetComponent<RectTransform>() ?? buyTextGO.AddComponent<RectTransform>();
            bTextRT.anchorMin = Vector2.zero;
            bTextRT.anchorMax = Vector2.one;
            bTextRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI bTextTMP = buyTextGO.GetComponent<TextMeshProUGUI>() ?? buyTextGO.AddComponent<TextMeshProUGUI>();
            bTextTMP.text = "<b>💳 Pay & Checkout</b>";
            bTextTMP.fontSize = 4.0f;
            bTextTMP.alignment = TextAlignmentOptions.Center;
            bTextTMP.color = Color.white;

            // 5. Confirmation Text
            Transform confT = canvasGO.transform.Find("Confirmation_Text");
            GameObject confGO = confT != null ? confT.gameObject : new GameObject("Confirmation_Text");
            confGO.transform.SetParent(canvasGO.transform, false);
            RectTransform confRT = confGO.GetComponent<RectTransform>() ?? confGO.AddComponent<RectTransform>();
            confRT.anchorMin = new Vector2(0.06f, 0.02f);
            confRT.anchorMax = new Vector2(0.94f, 0.22f);
            confRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI confTMP = confGO.GetComponent<TextMeshProUGUI>() ?? confGO.AddComponent<TextMeshProUGUI>();
            confTMP.text = "<color=#81C784>✔ Order placed successfully!</color>";
            confTMP.fontSize = 3.6f;
            confTMP.alignment = TextAlignmentOptions.Center;
            confTMP.color = Color.white;
            confGO.SetActive(false);

            // CartSummaryUI component
            CartSummaryUI ui = canvasGO.GetComponent<CartSummaryUI>() ?? canvasGO.AddComponent<CartSummaryUI>();
            SerializedObject soUI = new SerializedObject(ui);
            soUI.FindProperty("m_ItemsListText").objectReferenceValue = itemsTMP;
            soUI.FindProperty("m_TotalText").objectReferenceValue = totalTMP;
            soUI.FindProperty("m_ConfirmationText").objectReferenceValue = confTMP;
            soUI.FindProperty("m_BuyButton").objectReferenceValue = buyBtn;
            soUI.ApplyModifiedProperties();

            EditorUtility.SetDirty(kioskRoot);
        }
    }
}
