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
    private int[] selectedIndex = { -1, 0};

    private void OnSceneGUI() {
        spline = target as Spline;
        handleTransform = spline.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        ShowPoint(0);
        for (int i = 1; i < spline.ControlPointCount; i++) {
            ShowPoint(i);
            Handles.DrawBezier(
                handleTransform.TransformPoint(
                    spline.GetControlPoint(i - 1).GetAnchorPosition()),
                handleTransform.TransformPoint(
                    spline.GetControlPoint(i).GetAnchorPosition()),
                handleTransform.TransformPoint(
                    spline.GetControlPoint(i - 1).GetHandlePosition(1)),
                handleTransform.TransformPoint(
                    spline.GetControlPoint(i).GetHandlePosition(0)),
                Color.white,
                null,
                2f);
        }
    }

    private void ShowPoint(int index) {
        Vector3[] points = new Vector3[3] {
            handleTransform.TransformPoint(
                spline.GetControlPoint(index).GetAnchorPosition()),
            handleTransform.TransformPoint(
                spline.GetControlPoint(index).GetHandlePosition(0)),
            handleTransform.TransformPoint(
                spline.GetControlPoint(index).GetHandlePosition(1))
        };

        if (selectedIndex[0] == index) {
            Handles.color = Color.gray;
            Handles.DrawLine(points[0], points[1]);
            Handles.DrawLine(points[0], points[2]);
            for (int i = 0; i < 3; i++) {
                ShowSelectedControlPoint(index, i, points[i]);
            }
        } else {
            ShowControlPoint(index, 0, points[0]);
        }
    }

    public void ShowControlPoint(int index, int handle, Vector3 position) {
        float size = HandleUtility.GetHandleSize(position);
        Handles.color = Color.blue;

        if (Handles.Button(position, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
            selectedIndex = new int[] { index, handle };
            Repaint();
        }
    }

    public void ShowSelectedControlPoint(int index, int handle, Vector3 position) {
        float size = HandleUtility.GetHandleSize(position);
        Handles.color = Color.white;

        if (Handles.Button(position, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
            selectedIndex = new int[] { index, handle };
            Repaint();
        }

        if (selectedIndex[0] == index && selectedIndex[1] == handle) {
            EditorGUI.BeginChangeCheck();
            position = Handles.DoPositionHandle(position, handleRotation);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                if (handle == 0) {
                    spline.GetControlPoint(index).setAnchorPosition(
                        handleTransform.InverseTransformPoint(position));
                }
                else {
                    spline.GetControlPoint(index).setHandlePosition(handle - 1,
                        handleTransform.InverseTransformPoint(position));
                }
            }
        }
    }

    public override void OnInspectorGUI() {
        spline = target as Spline;

        if (selectedIndex[0] >= 0 && selectedIndex[0] < spline.ControlPointCount) {
            GUILayout.Label("Selected Point");
            if (selectedIndex[1] == 0) {
                EditorGUI.BeginChangeCheck();
                Vector3 anchor = EditorGUILayout.Vector3Field("Position (anchor)",
                    spline.GetControlPoint(selectedIndex[0]).GetAnchorPosition());
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(spline, "Move Anchor");
                    EditorUtility.SetDirty(spline);
                    spline.GetControlPoint(selectedIndex[0]).setAnchorPosition(anchor);
                }
            }
            else {
                EditorGUI.BeginChangeCheck();
                Vector3 handle = EditorGUILayout.Vector3Field("Position (handle)",
                    spline.GetControlPoint(selectedIndex[0]).GetHandlePosition(selectedIndex[1] - 1));
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(spline, "Move handle");
                    EditorUtility.SetDirty(spline);
                    spline.GetControlPoint(selectedIndex[0]).setHandlePosition(selectedIndex[1] - 1, handle);
                }
            }

            EditorGUI.BeginChangeCheck();
            BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPoint(selectedIndex[0]).mode);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(spline, "Change Point Mode");
                spline.GetControlPoint(selectedIndex[0]).mode = mode;
                EditorUtility.SetDirty(spline);
            }

            if (selectedIndex[0] == 0 || selectedIndex[0] == spline.ControlPointCount -1) {
                if (GUILayout.Button("Extend Curve")) {
                    Undo.RecordObject(spline, "Extend Curve");
                    if (selectedIndex[0] == 0) {
                        spline.InsertControlPoint(0);
                    }
                    else {
                        spline.AddControlPoint();
                        selectedIndex = new int[] { spline.ControlPointCount - 1, 0};
                    }
                    EditorUtility.SetDirty(spline);
                }
            } else {
                if (GUILayout.Button("Insert point")) {
                    Undo.RecordObject(spline, "Insert point");
                    spline.InsertControlPoint(selectedIndex[0]);
                    EditorUtility.SetDirty(spline);
                }
            }
        }
    }
}
