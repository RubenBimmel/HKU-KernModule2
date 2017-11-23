using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineComponent))]
public class SplineComponentEditor : Editor {
    private SplineComponent component;
    private Transform handleTransform;
    private Quaternion handleRotation;

    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;
    private int[] selectedIndex = { -1, 0};
    private int activeSpline = -1;

    private void OnSceneGUI() {
        component = target as SplineComponent;
        handleTransform = component.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        for (int i = 0; i < component.splines.Count; i++) {
            ShowPoint(i, 0);
            for (int j = 1; j < component.splines[i].points.Count; j++) {
                ShowPoint(i, j);
                Handles.DrawBezier(
                    handleTransform.TransformPoint(
                        component.splines[i].points[j - 1].GetAnchorPosition()),
                    handleTransform.TransformPoint(
                        component.splines[i].points[j].GetAnchorPosition()),
                    handleTransform.TransformPoint(
                        component.splines[i].points[j - 1].GetHandlePosition(1)),
                    handleTransform.TransformPoint(
                        component.splines[i].points[j].GetHandlePosition(0)),
                    Color.white,
                    null,
                    2f);
            }
        }
    }

    private void ShowPoint(int spline, int index) {
        Vector3[] points = new Vector3[3] {
            handleTransform.TransformPoint(
                component.splines[spline].points[index].GetAnchorPosition()),
            handleTransform.TransformPoint(
                component.splines[spline].points[index].GetHandlePosition(0)),
            handleTransform.TransformPoint(
                component.splines[spline].points[index].GetHandlePosition(1))
        };

        if (selectedIndex[0] == index && spline == activeSpline) {
            Handles.color = Color.gray;
            Handles.DrawLine(points[0], points[1]);
            Handles.DrawLine(points[0], points[2]);
            for (int i = 0; i < 3; i++) {
                ShowSelectedControlPoint(index, i, points[i]);
            }
        } else {
            if (component.splines[spline].points[index].GetIndex() == 0)
                ShowControlPoint(spline, index, 0, points[0]);
        }
    }

    public void ShowControlPoint(int spline, int index, int handle, Vector3 position) {
        float size = HandleUtility.GetHandleSize(position);
        Handles.color = Color.blue;

        if (Handles.Button(position, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
            selectedIndex = new int[] { index, handle };
            activeSpline = spline;
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
                Undo.RecordObject(component, "Move Point");
                EditorUtility.SetDirty(component);
                if (handle == 0) {
                    component.splines[activeSpline].points[index].setAnchorPosition(
                        handleTransform.InverseTransformPoint(position));
                }
                else {
                    component.splines[activeSpline].points[index].setHandlePosition(handle - 1,
                        handleTransform.InverseTransformPoint(position));
                }
            }
        }
    }

    public override void OnInspectorGUI() {
        component = target as SplineComponent;
        if (activeSpline >= 0) {
            if (selectedIndex[0] >= 0 && selectedIndex[0] < component.splines[activeSpline].points.Count) {
                if (component.splines[activeSpline].points[selectedIndex[0]].connectedPoints.Count > 1) {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Previous")) {
                        //component.splines[activeSpline].InsertControlPoint(selectedIndex[0]);
                    }
                    if (GUILayout.Button("Next")) {
                        //component.splines[activeSpline].InsertControlPoint(selectedIndex[0]);
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Label("Selected Point");
                if (selectedIndex[1] == 0) {
                    EditorGUI.BeginChangeCheck();
                    Vector3 anchor = EditorGUILayout.Vector3Field("Position (anchor)",
                        component.splines[activeSpline].points[selectedIndex[0]].GetAnchorPosition());
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(component, "Move Anchor");
                        EditorUtility.SetDirty(component);
                        component.splines[activeSpline].points[selectedIndex[0]].setAnchorPosition(anchor);
                    }
                }
                else {
                    EditorGUI.BeginChangeCheck();
                    Vector3 handle = EditorGUILayout.Vector3Field("Position (handle)",
                        component.splines[activeSpline].points[selectedIndex[0]].GetHandlePosition(selectedIndex[1] - 1));
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(component, "Move handle");
                        EditorUtility.SetDirty(component);
                        component.splines[activeSpline].points[selectedIndex[0]].setHandlePosition(selectedIndex[1] - 1, handle);
                    }
                }

                EditorGUI.BeginChangeCheck();
                BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", component.splines[activeSpline].points[selectedIndex[0]].mode);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(component, "Change Point Mode");
                    component.splines[activeSpline].points[selectedIndex[0]].mode = mode;
                    EditorUtility.SetDirty(component);
                }

                if (selectedIndex[0] == 0 || selectedIndex[0] == component.splines[activeSpline].points.Count - 1) {
                    if (GUILayout.Button("Extend Curve")) {
                        Undo.RecordObject(component, "Extend Curve");
                        if (component.splines[activeSpline].points.Count == 1 || selectedIndex[0] > 0) {
                            component.splines[activeSpline].AddControlPoint();
                            selectedIndex = new int[] { component.splines[activeSpline].points.Count - 1, 0 };
                        }
                        else {
                            component.splines[activeSpline].InsertControlPoint(0);
                        }
                        EditorUtility.SetDirty(component);
                    }
                }
                else {
                    if (GUILayout.Button("Insert point")) {
                        Undo.RecordObject(component, "Insert point");
                        component.splines[activeSpline].InsertControlPoint(selectedIndex[0]);
                        EditorUtility.SetDirty(component);
                    }
                }

                if (GUILayout.Button("Start new curve")) {
                    Undo.RecordObject(component, "Start new curve");
                    component.AddSpline(component.splines[activeSpline].points[selectedIndex[0]]);
                    activeSpline = component.splines.Count - 1;
                    selectedIndex = new int[] { 1, 0 };
                    EditorUtility.SetDirty(component);
                }
            }
        }
    }
}
