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

    // Draw splines and handles
    private void OnSceneGUI() {
        component = target as SplineComponent;
        handleTransform = component.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        for (int i = 0; i < component.splines.Count; i++) {
            Color bezierColor = i == activeSpline ? Color.white : Color.cyan;
            for (int j = 1; j < component.splines[i].points.Count; j++) {
                Handles.DrawBezier(
                    handleTransform.TransformPoint(
                        component.splines[i].points[j - 1].GetAnchorPosition()),
                    handleTransform.TransformPoint(
                        component.splines[i].points[j].GetAnchorPosition()),
                    handleTransform.TransformPoint(
                        component.splines[i].points[j - 1].GetHandlePosition(1)),
                    handleTransform.TransformPoint(
                        component.splines[i].points[j].GetHandlePosition(0)),
                    bezierColor,
                    null,
                    2f);
                ShowPoint(i, j - 1);
            }
            ShowPoint(i, component.splines[i].points.Count - 1);
        }
    }

    // Generate ControlPoint positions
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
                ShowControlPoint(spline, index, i, points[i], true);
            }
        } else {
            int connectedIndex = component.splines[spline].points[index].connectedIndex;
            if (connectedIndex < 0 
                || selectedIndex[0] < 0 
                || activeSpline < 0
                || component.splines[spline].points[index].connectedIndex != component.splines[activeSpline].points[selectedIndex[0]].connectedIndex)
                ShowControlPoint(spline, index, 0, points[0], false);
        }
    }

    // Draw ControlPoints
    private void ShowControlPoint(int spline, int index, int handle, Vector3 position, bool selected) {
        float size = HandleUtility.GetHandleSize(position);
        Handles.color = selected ? Color.white : Color.blue;

        if (Handles.Button(position, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
            selectedIndex = new int[] { index, handle };
            activeSpline = spline;
            Repaint();
        }
        if (selected) {
            switch(Tools.current) {
                case Tool.Move:
                    if (selectedIndex[1] == handle) MoveHandle(index, handle, position);
                    break;
                case Tool.Rotate:
                    if (handle == 0) RotationHandle (index, position);
                    break;
                case Tool.Scale:
                    if (handle == 0) ScaleHandle(index, position);
                    break;
            }
        }
    }

    // Draw movement handles
    private void MoveHandle (int index, int handle, Vector3 position) {
        EditorGUI.BeginChangeCheck();
        position = Handles.DoPositionHandle(position, handleRotation);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(component, "Move Point");
            EditorUtility.SetDirty(component);
            if (handle == 0) {
                component.SetControlPoint(
                    component.splines[activeSpline].points[index],
                    handleTransform.InverseTransformPoint(position));
            }
            else {
                component.splines[activeSpline].points[index].setHandlePosition(handle - 1,
                    handleTransform.InverseTransformPoint(position));
            }
        }
    }

    // Draw rotation handle
    private void RotationHandle(int index, Vector3 position) {
        if (Tools.pivotRotation == PivotRotation.Global) {
            /*EditorGUI.BeginChangeCheck();
            Quaternion rotation = component.splines[activeSpline].points[index].GetHandleRotation();
            Quaternion handleRotation = (Tools.pivotRotation == PivotRotation.Global) ? Quaternion.identity : rotation;
            handleRotation = Handles.RotationHandle(handleRotation, position);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Rotate Point");
                EditorUtility.SetDirty(component);
                component.splines[activeSpline].points[index].SetHandleRotation(rotation * handleRotation);
            }*/
        } else {
            EditorGUI.BeginChangeCheck();
            Quaternion rotation = component.splines[activeSpline].points[index].GetHandleRotation();
            rotation = Handles.RotationHandle(rotation, position);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Rotate Point");
                EditorUtility.SetDirty(component);
                component.splines[activeSpline].points[index].SetHandleRotation(rotation);
            }
        }
    }

    // Draw scale handle
    private void ScaleHandle(int index, Vector3 position) {
        EditorGUI.BeginChangeCheck();
        Quaternion rotation = component.splines[activeSpline].points[index].GetHandleRotation();
        float scale = component.splines[activeSpline].points[index].GetHandleScale();
        Handles.color = new Color(.4f, .4f, .8f);
        scale = Handles.ScaleSlider(scale, position, Vector3.forward, rotation, HandleUtility.GetHandleSize(position), 0f);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(component, "Scale Point");
            EditorUtility.SetDirty(component);
            component.splines[activeSpline].points[index].SetHandleScale(scale);
        }
    }

    public override void OnInspectorGUI() {
        component = target as SplineComponent;
        if (activeSpline >= 0) {
            if (selectedIndex[0] >= 0 && selectedIndex[0] < component.splines[activeSpline].points.Count) {
                GUILayout.Label("Selected Point");
                int connectedIndex = component.splines[activeSpline].points[selectedIndex[0]].connectedIndex;
                if (connectedIndex >= 0) {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Previous")) {
                        ControlPoint newPoint = component.GetConnectedPoint(component.splines[activeSpline].points[selectedIndex[0]], -1);
                        activeSpline = component.GetSpline(newPoint);
                        selectedIndex[0] = component.splines[activeSpline].points.IndexOf(newPoint);
                        selectedIndex[1] = 0;
                        //OnSceneGUI();
                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("Next")) {
                        ControlPoint newPoint = component.GetConnectedPoint(component.splines[activeSpline].points[selectedIndex[0]], 1);
                        activeSpline = component.GetSpline(newPoint);
                        selectedIndex[0] = component.splines[activeSpline].points.IndexOf(newPoint);
                        selectedIndex[1] = 0;
                        //OnSceneGUI();
                        SceneView.RepaintAll();
                    }
                    GUILayout.EndHorizontal();
                }

                if (selectedIndex[1] == 0) {
                    EditorGUI.BeginChangeCheck();
                    Vector3 anchor = EditorGUILayout.Vector3Field("Position (anchor)",
                        component.splines[activeSpline].points[selectedIndex[0]].GetAnchorPosition());
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(component, "Move Anchor");
                        EditorUtility.SetDirty(component);
                        component.SetControlPoint(component.splines[activeSpline].points[selectedIndex[0]], anchor);
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

                if (GUILayout.Button("Remove point")) {
                    Undo.RecordObject(component, "Remove point");
                    component.RemovePoint(activeSpline, selectedIndex[0]);
                    activeSpline = -1;
                    selectedIndex = new int[] { -1, 0 };
                    EditorUtility.SetDirty(component);
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
