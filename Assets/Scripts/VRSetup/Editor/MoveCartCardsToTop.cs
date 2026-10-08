// ============================================================
// MoveCartCardsToTop.cs
// Replaces the heading ("the text above the object") with the
// Add to Cart card ("the card at the right to the object") so
// the card containing the button and item name/price sits directly
// on top of each object across all 3 rooms.
//
// Run via: VR Furniture Shop > Move Cart Cards To Top & Replace Headings
// Also auto-executes once on compilation.
// ============================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class MoveCartCardsToTop
{
    static MoveCartCardsToTop()
    {
        EditorApplication.delayCall += ExecuteIfSampleScene;
    }

    private static void ExecuteIfSampleScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.isLoaded && scene.path.Contains("SampleScene.unity"))
        {
            ApplyCardReplacement(true);
        }
    }

    [MenuItem("VR Furniture Shop/Move Cart Cards To Top & Replace Headings")]
    public static void ManualExecute()
    {
        ApplyCardReplacement(true);
    }

    public static void ApplyCardReplacement(bool logSummary)
    {
        if (EditorApplication.isPlaying) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.isLoaded) return;

        int replacedCount = 0;

        // 1. Room 1: Cupboards (Station_01 to Station_05)
        // Tall cupboards (~1.73m height). Card center at Y = 1.95m directly above top ledge.
        string[] room1Stations = { "Station_01", "Station_02", "Station_03", "Station_04", "Station_05" };
        foreach (string stName in room1Stations)
        {
            if (ProcessStation(stName, new Vector3(0f, 1.95f, -0.05f)))
                replacedCount++;
        }

        // 2. Room 2: Mini Fridges (Station_MF_02 to Station_MF_05)
        // Low appliances (~0.85m height). Card center at Y = 1.25m directly above fridge at eye level.
        string[] room2Stations = { "Station_MF_02", "Station_MF_03", "Station_MF_04", "Station_MF_05" };
        foreach (string stName in room2Stations)
        {
            if (ProcessStation(stName, new Vector3(0f, 1.25f, -0.05f)))
                replacedCount++;
        }

        // 3. Room 3: Lamps (Station_Lamp_01 to Station_Lamp_05)
        // Table lamps (~0.45m height). Card center at Y = 1.05m directly above lamp at comfortable interaction height.
        string[] room3Stations = { "Station_Lamp_01", "Station_Lamp_02", "Station_Lamp_03", "Station_Lamp_04", "Station_Lamp_05" };
        foreach (string stName in room3Stations)
        {
            if (ProcessStation(stName, new Vector3(0f, 1.05f, 0f)))
                replacedCount++;
        }

        // Format and polish all cart sign UI
        CartSignFormatter.ReformatAllSigns(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        if (logSummary)
        {
            Debug.Log($"[MoveCartCardsToTop] Successfully replaced headings with Add to Cart cards on top of {replacedCount} objects across all 3 rooms!");
        }
    }

    private static bool ProcessStation(string stationName, Vector3 targetLocalPos)
    {
        GameObject stationGO = GameObject.Find(stationName);
        if (stationGO == null) return false;

        // 1. Remove/Destroy the old heading ("Label_Canvas") above the object
        Transform labelT = stationGO.transform.Find("Label_Canvas");
        if (labelT != null)
        {
            Undo.DestroyObjectImmediate(labelT.gameObject);
        }

        // Also check if any secondary label canvas exists
        Transform oldLabelT = stationGO.transform.Find("Station_Label");
        if (oldLabelT != null)
        {
            Undo.DestroyObjectImmediate(oldLabelT.gameObject);
        }

        // 2. Move the Add to Cart card ("Cart_Sign") to the top of the object
        Transform cartT = stationGO.transform.Find("Cart_Sign");
        if (cartT != null)
        {
            Undo.RecordObject(cartT.gameObject, $"Move Cart_Sign to top of {stationName}");
            cartT.localPosition = targetLocalPos;
            cartT.localRotation = Quaternion.identity; // Facing straight towards the front/player

            RectTransform rt = cartT.GetComponent<RectTransform>();
            if (rt != null)
            {
                Undo.RecordObject(rt, $"Update RectTransform for {stationName}");
                rt.anchoredPosition = new Vector2(targetLocalPos.x, targetLocalPos.y);
                rt.localPosition = targetLocalPos;
                rt.localRotation = Quaternion.identity;
            }

            EditorUtility.SetDirty(cartT.gameObject);
            EditorUtility.SetDirty(stationGO);
            return true;
        }

        return false;
    }
}
