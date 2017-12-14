using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineSettings))]
public class SplineSettingsEditor : Editor {
    public override void OnInspectorGUI() {
        SplineSettings settings = target as SplineSettings;
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck()) {
            SplineComponent[] components = FindObjectsOfType<SplineComponent>();
            foreach(SplineComponent component in components) {
                component.UpdateSpline(settings);
            }
        }
    }
}
