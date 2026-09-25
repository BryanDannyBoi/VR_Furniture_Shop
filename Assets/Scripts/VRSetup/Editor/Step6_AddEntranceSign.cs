// ============================================================
// Step6_AddEntranceSign.cs
// Editor-only script.
//
// Run via:  VR Furniture Shop > Step 6 - Add Entrance Sign
//
// What it does:
//   1. Creates/loads the solid-color material in Assets/Materials/SignBoard_Mat.mat
//   2. Creates the "Entrance_Sign" GameObject mounted above the entrance doorway gap
//      between "Title wall" (Z = -3.03) and "Right wall" (Z = -3.93).
//   3. Adds a flat panel (Cube) sized to span the doorway width (0.90m wide,
//      0.25m tall, 0.06m thick) at Y = 1.875m (clearance 1.75m underneath).
//   4. Adds a World Space Canvas with CanvasRenderer + TextMeshProUGUI
//      displaying "Furniture Shop" in bold white text, positioned on the outer
//      face and facing outward (-X) toward approaching visitors.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class Step6_AddEntranceSign
{
    private const string SIGN_NAME = "Entrance_Sign";
    private const string MATERIAL_PATH = "Assets/Materials/SignBoard_Mat.mat";

    private static readonly Vector3 SIGN_POSITION = new Vector3(-1.72f, 1.875f, -3.48f);
    private static readonly Vector3 BOARD_SCALE   = new Vector3(0.06f, 0.25f, 0.90f);

    [MenuItem("VR Furniture Shop/Step 6 - Add Entrance Sign")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step6] Exit Play Mode before running this script.");
            return;
        }

        // --------------------------------------------------------
        // 1. Remove any existing Entrance_Sign (safe to re-run)
        // --------------------------------------------------------
        GameObject existingSign = GameObject.Find(SIGN_NAME);
        if (existingSign != null)
        {
            Undo.DestroyObjectImmediate(existingSign);
            Debug.Log("[Step6] Replacing existing Entrance_Sign...");
        }

        // --------------------------------------------------------
        // 2. Load or create the solid-color material
        // --------------------------------------------------------
        Material boardMat = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
        if (boardMat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            boardMat = new Material(shader);
            Color charcoal = new Color(0.13f, 0.14f, 0.17f, 1.0f);
            if (boardMat.HasProperty("_BaseColor")) boardMat.SetColor("_BaseColor", charcoal);
            else if (boardMat.HasProperty("_Color")) boardMat.SetColor("_Color", charcoal);
            if (boardMat.HasProperty("_Smoothness")) boardMat.SetFloat("_Smoothness", 0.1f);

            AssetDatabase.CreateAsset(boardMat, MATERIAL_PATH);
            AssetDatabase.SaveAssets();
        }

        // --------------------------------------------------------
        // 3. Root Entrance_Sign object
        // --------------------------------------------------------
        GameObject rootSign = new GameObject(SIGN_NAME);
        Undo.RegisterCreatedObjectUndo(rootSign, "Create Entrance Sign");
        rootSign.transform.position = SIGN_POSITION;
        rootSign.transform.rotation = Quaternion.identity;
        rootSign.transform.localScale = Vector3.one;

        // --------------------------------------------------------
        // 4. Panel (Cube)
        // --------------------------------------------------------
        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(panel, "Create Sign Panel");
        panel.name = "Sign_Board";
        panel.transform.SetParent(rootSign.transform, false);
        panel.transform.localPosition = Vector3.zero;
        panel.transform.localRotation = Quaternion.identity;
        panel.transform.localScale = BOARD_SCALE;

        MeshRenderer panelRenderer = panel.GetComponent<MeshRenderer>();
        if (panelRenderer != null)
        {
            panelRenderer.sharedMaterial = boardMat;
        }

        // --------------------------------------------------------
        // 5. WorldSpace Canvas for TextMeshProUGUI
        // --------------------------------------------------------
        GameObject canvasGO = new GameObject("Sign_Canvas");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Sign Canvas");
        canvasGO.transform.SetParent(rootSign.transform, false);

        // Position on the outer (-X) face: outer face is at X = -0.03m.
        // Nudge 5mm outward to X = -0.035m to prevent Z-fighting.
        canvasGO.transform.localPosition = new Vector3(-0.035f, 0f, 0f);

        // Rotate Y = -90 so the canvas faces -X (outward toward outside approach)
        canvasGO.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

        // 0.01 scale: 90 units = 0.90m, 25 units = 0.25m
        canvasGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1
                                        | AdditionalCanvasShaderChannels.Normal
                                        | AdditionalCanvasShaderChannels.Tangent;

        RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(90f, 25f);

        // --------------------------------------------------------
        // 6. TextMeshProUGUI Child Object
        // --------------------------------------------------------
        GameObject textGO = new GameObject("Sign_Text");
        Undo.RegisterCreatedObjectUndo(textGO, "Create Sign Text");
        textGO.transform.SetParent(canvasGO.transform, false);

        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        textGO.AddComponent<CanvasRenderer>();

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Furniture Shop";
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 6f;
        tmp.fontSizeMax = 20f;
        tmp.fontSize = 15f;

        // Assign default SDF font if available
        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (defaultFont != null)
        {
            tmp.font = defaultFont;
        }

        // --------------------------------------------------------
        // 7. Save Scene
        // --------------------------------------------------------
        EditorUtility.SetDirty(rootSign);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[Step6] Entrance sign with TextMeshProUGUI successfully created and saved!");
    }
}
