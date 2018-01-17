using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(SplineSettings))]
public class SplineSettingsEditor : Editor {

    public override void OnInspectorGUI() {
        SplineSettings settings = target as SplineSettings;

        string[] assets = settings.GetAssetNames();
        foreach (string asset in assets) {
            GUILayout.Label(asset);
        }
    }

    [UnityEditor.Callbacks.OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line) {
        SplineSettings settings = Selection.activeObject as SplineSettings;
        if (settings != null) {
            SplineSettingsEditorWindow.Init(settings);
            return true; //catch open file
        }

        return false; // let unity open the file
    }
}
