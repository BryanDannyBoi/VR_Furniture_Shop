// ============================================================
// Step4_VerifyDeviceSimulator.cs
// Editor-only verification script.
//
// Run via:  VR Furniture Shop > Step 4 - Verify Device Simulator
//
// XRDeviceSimulatorSettings is an internal Unity class so we
// cannot reference it directly. Instead we load the settings
// ScriptableObject and read its fields via Unity's
// SerializedObject API, which works on any serialized asset
// regardless of access level.
//
// HOW THE DEVICE SIMULATOR WORKS:
//   At Play Mode start Unity reads XRDeviceSimulatorSettings and
//   spawns the XR Device Simulator prefab automatically. The prefab
//   feeds fake HMD + controller inputs into the XR Origin so the
//   rig responds as if a real headset were connected.
//   It is Editor-only and NOT included in any build.
//
// CONTROLS (XRI 3.x default bindings):
//   Mouse move            Look around (rotate HMD)
//   W / A / S / D         Move forward/left/back/right
//   Q / E                 Move down / up
//   Left Shift (hold)     Control LEFT hand controller
//   Space (hold)          Control RIGHT hand controller
//   G                     Toggle Grip on active controller
//   T                     Toggle Trigger (fires teleport ray)
//   Right-click drag      Rotate active controller
//   Middle-click drag     Translate active controller
//
// TELEPORTING:
//   1. Hold Space  (right controller mode)
//   2. Move mouse to aim the arc at a TeleportAnchor
//   3. Hold T — the arc ray appears
//   4. Release T — player teleports to that station
// ============================================================

using UnityEngine;
using UnityEditor;

public static class Step4_VerifyDeviceSimulator
{
    [MenuItem("VR Furniture Shop/Step 4 - Verify Device Simulator")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Step4] Exit Play Mode before running this script.");
            return;
        }

        // --------------------------------------------------------
        // Load the settings asset as a plain ScriptableObject.
        // XRDeviceSimulatorSettings is internal so we use
        // SerializedObject to read its serialized fields by name.
        // --------------------------------------------------------
        const string SETTINGS_PATH =
            "Assets/XRI/Settings/Resources/XRDeviceSimulatorSettings.asset";

        ScriptableObject settingsSO =
            AssetDatabase.LoadAssetAtPath<ScriptableObject>(SETTINGS_PATH);

        if (settingsSO == null)
        {
            Debug.LogError("[Step4] Could not load XRDeviceSimulatorSettings at:\n"
                + SETTINGS_PATH + "\nMake sure the XR Interaction Toolkit package is installed.");
            return;
        }

        // Wrap in SerializedObject so we can read any field by name.
        SerializedObject so = new SerializedObject(settingsSO);

        bool autoOn     = so.FindProperty("m_AutomaticallyInstantiateSimulatorPrefab").boolValue;
        bool editorOnly = so.FindProperty("m_AutomaticallyInstantiateInEditorOnly").boolValue;
        Object prefabRef = so.FindProperty("m_SimulatorPrefab").objectReferenceValue;

        // --------------------------------------------------------
        // Report results.
        // --------------------------------------------------------
        if (autoOn && editorOnly && prefabRef != null)
        {
            Debug.Log("[Step4] XR Device Simulator configured correctly:\n"
                + "  AutoInstantiate   = TRUE\n"
                + "  EditorOnly        = TRUE  (excluded from builds)\n"
                + "  SimulatorPrefab   = " + prefabRef.name);
        }
        else
        {
            // Something is wrong — log what needs attention.
            if (!autoOn)
                Debug.LogError("[Step4] AutoInstantiate is OFF.\n"
                    + "Open '" + SETTINGS_PATH + "' in the Inspector and enable it, "
                    + "or select Edit > Project Settings > XR Interaction Toolkit.");
            if (!editorOnly)
                Debug.LogWarning("[Step4] EditorOnly is OFF — simulator will appear in builds too.");
            if (prefabRef == null)
                Debug.LogError("[Step4] SimulatorPrefab is not assigned.\n"
                    + "Open '" + SETTINGS_PATH + "' in the Inspector and drag in:\n"
                    + "Assets/Samples/XR Interaction Toolkit/3.3.2/XR Device Simulator/"
                    + "XR Device Simulator.prefab");
            return;
        }

        // --------------------------------------------------------
        // Print the control reference to the Console for easy access
        // while testing in Play Mode.
        // --------------------------------------------------------
        Debug.Log("[Step4] -- DEVICE SIMULATOR CONTROLS ------------------\n"
            + "  Mouse move          Look / rotate HMD\n"
            + "  W A S D             Move player (horizontal)\n"
            + "  Q / E               Move player down / up\n"
            + "  Left Shift (hold)   Control LEFT hand controller\n"
            + "  Space (hold)        Control RIGHT hand controller\n"
            + "  G                   Toggle Grip on active controller\n"
            + "  T                   Toggle Trigger (fires teleport ray)\n"
            + "  Right-click drag    Rotate active controller\n"
            + "  Middle-click drag   Translate active controller\n"
            + "----------------------------------------------------\n"
            + "  TO TELEPORT:\n"
            + "    1. Hold Space  (switches to right controller)\n"
            + "    2. Move mouse to aim arc ray at a TeleportAnchor\n"
            + "    3. Hold T  — arc becomes visible\n"
            + "    4. Release T  — player teleports\n"
            + "----------------------------------------------------\n"
            + "  Press Play now to test!");
    }
}
