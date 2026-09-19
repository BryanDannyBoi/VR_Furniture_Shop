// ============================================================
// Step3_AddStationLabels.cs
// Editor-only one-shot script.
//
// Run via:  VR Furniture Shop > Step 3 - Add Station Labels
//
// What it does:
//   For each station (Station_01 through Station_05) it creates a child
//   GameObject called "Label_Canvas" that contains:
//     - A Canvas in World Space render mode (no screen-overlay, visible in 3D).
//     - A TextMeshProUGUI component showing the furniture name as plain text.
//
//   The canvas is placed 1.8 m above the station, facing the player (-Z side),
//   sized 1.0 m wide x 0.3 m tall at world scale.
//
// IMPORTANT - TMP Essential Resources:
//   If you have never used TextMeshPro in this project before, Unity will show
//   a "TMP Importer" dialog the first time a TMP component is created.
//   Click "Import TMP Essentials" — this installs the default font.
//   Then re-run this script; the text will display correctly.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class Step3_AddStationLabels
{
    // --------------------------------------------------------
    // Mapping: station name -> label text (furniture name).
    // Change the label text here if you ever rename a piece.
    // --------------------------------------------------------
    private static readonly (string station, string label)[] s_Stations =
    {
        ("Station_01", "Cupboard_0"),
        ("Station_02", "Cupboard_1"),
        ("Station_03", "Cupboard_2"),
        ("Station_04", "Cupboard_3"),
        ("Station_05", "Cupboard_4"),
    };

    // Canvas dimensions in canvas-units. The canvas is then scaled by
    // CANVAS_WORLD_SCALE so that 1 canvas-unit = 1 cm in world space,
    // giving a 1 m-wide x 0.3 m-tall label.
    private const float CANVAS_WIDTH_UNITS  = 100f;  // canvas units
    private const float CANVAS_HEIGHT_UNITS = 30f;   // canvas units
    private const float CANVAS_WORLD_SCALE  = 0.01f; // 1 unit = 1 cm  =>  100 units = 1 m

    // Height above the station origin where the label floats.
    private const float LABEL_Y_OFFSET = 1.8f;

    // Font size in canvas-units (auto-sizing is also enabled as fallback).
    private const float FONT_SIZE = 18f;

    // --------------------------------------------------------
    // Menu entry
    // --------------------------------------------------------
    [MenuItem("VR Furniture Shop/Step 3 - Add Station Labels")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step3] Exit Play Mode before running this script.");
            return;
        }

        int created = 0;

        foreach (var (stationName, labelText) in s_Stations)
        {
            // Find the station in the scene.
            GameObject station = GameObject.Find(stationName);
            if (station == null)
            {
                Debug.LogError("[Step3] Could not find GameObject '" + stationName + "' in scene.");
                continue;
            }

            // Skip if a label canvas already exists (safe to re-run).
            if (station.transform.Find("Label_Canvas") != null)
            {
                Debug.LogWarning("[Step3] '" + stationName + "' already has a Label_Canvas. Skipping.");
                continue;
            }

            // ------------------------------------------------
            // 1. Create the Canvas GameObject as a child of the station.
            // ------------------------------------------------
            GameObject canvasGO = new GameObject("Label_Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Label_Canvas for " + stationName);
            canvasGO.transform.SetParent(station.transform, worldPositionStays: false);

            // Position: 1.8 m directly above the station origin.
            // Z = -0.1 nudges it slightly toward the player so it clears the
            // top of the cupboard geometry.
            canvasGO.transform.localPosition = new Vector3(0f, LABEL_Y_OFFSET, -0.1f);

            // Rotation: Y = 180 so the canvas face points toward -Z (the player side).
            canvasGO.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            // Uniform world-scale: shrinks canvas-units down to metres.
            canvasGO.transform.localScale = Vector3.one * CANVAS_WORLD_SCALE;

            // ------------------------------------------------
            // 2. Configure the Canvas component.
            //    World Space = the canvas exists in 3D and is visible from any camera,
            //    including the VR camera. No screen overlay needed.
            // ------------------------------------------------
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            // Note: no Event Camera is needed — we are display-only, no UI clicks.

            // RectTransform size = canvas size in canvas-units.
            RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(CANVAS_WIDTH_UNITS, CANVAS_HEIGHT_UNITS);

            // ------------------------------------------------
            // 3. Create the TextMeshProUGUI child.
            // ------------------------------------------------
            GameObject textGO = new GameObject("LabelText");
            Undo.RegisterCreatedObjectUndo(textGO, "Create LabelText for " + stationName);
            textGO.transform.SetParent(canvasGO.transform, worldPositionStays: false);

            // Stretch the text object to fill the whole canvas.
            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;         // anchor bottom-left
            textRT.anchorMax = Vector2.one;          // anchor top-right (stretch)
            textRT.offsetMin = Vector2.zero;         // no padding
            textRT.offsetMax = Vector2.zero;

            // Add the TextMeshProUGUI component and configure it.
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text            = labelText;
            tmp.fontSize        = FONT_SIZE;
            tmp.enableAutoSizing = true;             // shrink text if it overflows
            tmp.fontSizeMin     = 8f;
            tmp.fontSizeMax     = FONT_SIZE;
            tmp.alignment       = TextAlignmentOptions.Center;
            tmp.color           = Color.white;

            // ------------------------------------------------
            // 4. No CanvasScaler or GraphicRaycaster needed —
            //    we are pure display, no interaction.
            // ------------------------------------------------

            Debug.Log("[Step3] Label '" + labelText + "' created for " + stationName);
            created++;
        }

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Step3] Done - " + created + " label(s) created. Save the scene (Ctrl+S).");
    }
}
