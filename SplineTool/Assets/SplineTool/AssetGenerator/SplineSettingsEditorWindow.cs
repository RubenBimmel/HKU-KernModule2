using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SplineSettingsEditorWindow : EditorWindow {
    public static SplineSettings settings;
    private int viewIndex = -1;

    [MenuItem("Window/Spline Settings Editor")]
    public static void Init() {
        EditorWindow.GetWindow(typeof(SplineSettingsEditorWindow));
    }

    public static void Init(SplineSettings _settings) {
        EditorWindow.GetWindow(typeof(SplineSettingsEditorWindow));
        settings = _settings;
    }

    private void OnEnable() {
        if (EditorPrefs.HasKey("ObjectPath")) {
            string objectPath = EditorPrefs.GetString("ObjectPath");
            settings = AssetDatabase.LoadAssetAtPath(objectPath, typeof(SplineSettings)) as SplineSettings;
        }
    }

    private void OnGUI() {
        GUILayout.BeginHorizontal();
        string name = "No asset selected";
        if (settings != null) {
            name = settings.name;
        }
        GUILayout.Label(name);
        if (GUILayout.Button("Open", GUILayout.Width(100f))) {
            OpenSplineSettings();
        }
        if (GUILayout.Button("Close", GUILayout.Width(100f))) {
            settings = null;
        }
        GUILayout.EndHorizontal();
    }

    private void OpenSplineSettings() {
        string absPath = EditorUtility.OpenFilePanel("Select Spline Settings", "Assets/", "asset");
        if (absPath.StartsWith(Application.dataPath)) {
            string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
            settings = AssetDatabase.LoadAssetAtPath(relPath, typeof(SplineSettings)) as SplineSettings;
        }
    }

}
