using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spline))]
public class SplineEditor : Editor {
    private Spline spline;
    private Transform handleTransform;
    private Quaternion handleRotation;

    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;
    private int selectedIndex = -1;

    private void OnSceneGUI() {
        spline = target as Spline;
        handleTransform = spline.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        ShowPoint(0);
        
    }

    private void ShowPoint(int index) {
        Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index).anchor);
        float size = HandleUtility.GetHandleSize(point);
        Handles.color = Color.white;

        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
            selectedIndex = index;
            Repaint();
        }

        if (selectedIndex == index) {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }
    }

    public override void OnInspectorGUI() {
        spline = target as Spline;

        if (GUILayout.Button("Add Curve")) {
            Undo.RecordObject(spline, "Add Curve");
            //spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
    }
}
