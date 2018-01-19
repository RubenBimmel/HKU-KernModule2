using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class SplineSettingsEditorWindow : EditorWindow {
    public static SplineSettings settings;
    private int viewIndex = -1;
    private Vector2 scrollposition = Vector2.zero;

    //Initialize from menu
    [MenuItem("Window/Spline Settings Editor")]
    public static void Init() {
        SplineSettingsEditorWindow window = (SplineSettingsEditorWindow) EditorWindow.GetWindow(typeof(SplineSettingsEditorWindow));
        window.minSize = new Vector2(600, 300);
        window.viewIndex = -1;
    }

    //Initialize from file
    public static void Init(SplineSettings _settings) {
        SplineSettingsEditorWindow window = (SplineSettingsEditorWindow) EditorWindow.GetWindow(typeof(SplineSettingsEditorWindow));
        window.minSize = new Vector2(600, 300);
        window.viewIndex = -1;
        settings = _settings;
    }

    private void OnGUI() {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.Space();

        //Draw top bar with name and open & close buttons
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
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //Draw editor if a settings asset is selected
        if (settings != null) {
            GUILayout.BeginHorizontal();

            //Left panel
            scrollposition = EditorGUILayout.BeginScrollView(scrollposition, false, true, GUILayout.Width(position.width / 2), GUILayout.Height(position.height - 30));
            string[] names = settings.GetAssetNames();

            //Generated meshes list
            GUILayout.Label("Generated meshes:");
            for (int i = 0; i < settings.generated.Count; i++) {
                string assetName = "";
                try {
                    assetName = names[i];
                }
                catch (IndexOutOfRangeException) {
                    //Cloning a asset can take longer then one GUI frame. In this case the amount of names given by GetAssetNames is less then the amount of assets.
                    //This problem fixes itselve in the next frame.
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(assetName, GUILayout.Width(position.width / 2 - 215));
                if (GUILayout.Button("Edit", GUILayout.Width(60))) {
                    viewIndex = i;
                    GUIUtility.keyboardControl = 0;
                }
                if (GUILayout.Button("Clone", GUILayout.Width(60))) {
                    settings.CloneGeneratedMesh(i);
                    viewIndex = i + 1;
                    GUIUtility.keyboardControl = 0;
                }
                if (GUILayout.Button("Remove", GUILayout.Width(60))) {
                    settings.generated.RemoveAt(i);
                    viewIndex = -1;
                    GUIUtility.keyboardControl = 0;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(position.width / 2 - 87));
            if (GUILayout.Button("Add", GUILayout.Width(60))) {
                settings.AddGeneratedMesh();
                viewIndex = settings.generated.Count - 1;
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            //Object placers list
            GUILayout.Label("Object Placers:");
            for (int i = settings.generated.Count; i < names.Length ; i++) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(names[i], GUILayout.Width(position.width / 2 - 215));
                if (GUILayout.Button("Edit", GUILayout.Width(60))) {
                    viewIndex = i;
                    GUIUtility.keyboardControl = 0;
                }
                if (GUILayout.Button("Clone", GUILayout.Width(60))) {
                    settings.CloneObjectPlacer(i - settings.generated.Count);
                    viewIndex = i + 1;
                    GUIUtility.keyboardControl = 0;
                }
                if (GUILayout.Button("Remove", GUILayout.Width(60))) {
                    settings.placers.RemoveAt(i - settings.generated.Count);
                    viewIndex = -1;
                    GUIUtility.keyboardControl = 0;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(position.width / 2 - 87));
            if (GUILayout.Button("Add", GUILayout.Width(60))) {
                settings.AddObjectPlacer();
                viewIndex = settings.assetCount - 1;
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();

            //Right panel
            GUILayout.BeginVertical();
            if (viewIndex >= 0 && viewIndex < settings.assetCount) {
                if (viewIndex < settings.generated.Count) {
                    DrawGeneratedMeshSettings(settings.generated[viewIndex]);
                } else {
                    DrawObjectPlacerSettings(settings.placers[viewIndex - settings.generated.Count]);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        if (EditorGUI.EndChangeCheck()) {
            if (settings) {
                EditorUtility.SetDirty(settings);
                SplineComponent[] components = FindObjectsOfType<SplineComponent>();
                foreach (SplineComponent component in components) {
                    component.UpdateSpline(settings);
                }
            }
        }
    }

    // Draw the editor for a Generated Mesh
    private void DrawGeneratedMeshSettings (GeneratedMesh meshSettings) {
        meshSettings.name = EditorGUILayout.TextField("Name", meshSettings.name);
        meshSettings.length = EditorGUILayout.Slider("Length", meshSettings.length, 0.05f, 2f);
        meshSettings.sides = Mathf.FloorToInt(EditorGUILayout.Slider("Sides", meshSettings.sides, 3, 12));
        meshSettings.smoothEdges = EditorGUILayout.Toggle("Smooth Edges", meshSettings.smoothEdges);
        meshSettings.rotation = EditorGUILayout.Slider("Rotation", meshSettings.rotation, 0f, 360f);
        meshSettings.scale = EditorGUILayout.Vector2Field("Scale", meshSettings.scale);
        meshSettings.offset = EditorGUILayout.Vector2Field("Offset", meshSettings.offset);
        meshSettings.cap = EditorGUILayout.Toggle("Cap", meshSettings.cap);
        meshSettings.material = (Material) EditorGUILayout.ObjectField("Material", meshSettings.material, typeof(Material), false);
    }

    // Draw the editor for an Object Placer
    private void DrawObjectPlacerSettings(ObjectPlacer objectSettings) {
        objectSettings.name = EditorGUILayout.TextField("Name", objectSettings.name);
        objectSettings.objectReference = (Transform)EditorGUILayout.ObjectField("Object Reference", objectSettings.objectReference, typeof(Transform), false);
        objectSettings.position = EditorGUILayout.Vector2Field("Position", objectSettings.position);
        objectSettings.distance = EditorGUILayout.Slider("Distance", objectSettings.distance, 0.05f, 4f);
        objectSettings.offset = EditorGUILayout.FloatField("Offset at start", objectSettings.offset);
        objectSettings.type = (offsetType) EditorGUILayout.EnumPopup("Offset type", objectSettings.type);
        objectSettings.rotation = EditorGUILayout.Vector3Field("Rotation", objectSettings.rotation);
        objectSettings.scale = EditorGUILayout.Vector3Field("Scale", objectSettings.scale);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Constraints:  ");
        GUILayout.Label("X");
        objectSettings.constraints[0] = EditorGUILayout.Toggle(objectSettings.constraints[0]);
        GUILayout.Label("Y");
        objectSettings.constraints[1] = EditorGUILayout.Toggle(objectSettings.constraints[1]);
        GUILayout.Label("Z");
        objectSettings.constraints[2] = EditorGUILayout.Toggle(objectSettings.constraints[2]);
        EditorGUILayout.EndHorizontal();
    }

    //Open a new SplineSettings asset
    private void OpenSplineSettings() {
        string absPath = EditorUtility.OpenFilePanel("Select Spline Settings", "Assets/", "asset");
        if (absPath.StartsWith(Application.dataPath)) {
            string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
            settings = AssetDatabase.LoadAssetAtPath(relPath, typeof(SplineSettings)) as SplineSettings;
        }
    }

}
