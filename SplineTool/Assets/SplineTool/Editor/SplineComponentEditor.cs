using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineComponent))]
public class SplineComponentEditor : Editor {
    private SplineComponent component;
    private Transform componentTransform;
    private Quaternion componentRotation;
    private Quaternion handleRotation;
    private Quaternion pointRotation;

    private PivotRotation currentPivotRotation;

    private int tool;

    private const float handleSize = 0.05f;
    private const float pickSize = 0.07f;
    private int[] selectedIndex = { -1, 0};
    private int activeSpline = -1;

    private static int lineSteps = 10;
    private const float finSize = .03f;

    // Draw splines and handles
    private void OnSceneGUI() {
        component = target as SplineComponent;
        componentTransform = component.transform;
        componentRotation = Tools.pivotRotation == PivotRotation.Local ? componentTransform.rotation : Quaternion.identity;

        for (int i = 0; i < component.splines.Count; i++) {
            Color bezierColor = i == activeSpline ? Color.white : Color.cyan;
            float bezierWidth = i == activeSpline ? 3f : 2f;
            for (int j = 1; j < component.splines[i].points.Count; j++) {
                Handles.DrawBezier(
                    componentTransform.TransformPoint(
                        component.splines[i].points[j - 1].GetAnchorPosition()),
                    componentTransform.TransformPoint(
                        component.splines[i].points[j].GetAnchorPosition()),
                    componentTransform.TransformPoint(
                        component.splines[i].points[j - 1].GetHandlePosition(1)),
                    componentTransform.TransformPoint(
                        component.splines[i].points[j].GetHandlePosition(0)),
                    bezierColor,
                    null,
                    bezierWidth);
                ShowPoint(i, j - 1);
            }
            ShowPoint(i, component.splines[i].points.Count - 1);
        }

        if (activeSpline >= 0)
            ShowAngles(activeSpline);

        if (Tools.pivotRotation != currentPivotRotation) {
            currentPivotRotation = Tools.pivotRotation;
            handleRotation = Quaternion.identity;
            pointRotation = component.splines[activeSpline].points[selectedIndex[0]].GetRotation();
        }
    }

    private void ShowAngles (int spline) {
        Vector3 lineStart = component.GetPoint(spline, 0f);
        Gizmos.color = Color.cyan;
        for (int i = 1; i < lineSteps * (component.splines[spline].points.Count - 1) + 1; i++) {
            Vector3 lineEnd = component.GetPoint(spline, i / (float)lineSteps);
            Handles.color = new Color(.2f, 1, .2f);
            Vector3 up = component.GetUp(spline, (i - 1) / (float)lineSteps);
            Handles.DrawLine(lineStart + finSize * up, lineEnd);
            Handles.DrawLine(lineStart, lineStart + finSize * up);
            Handles.color = new Color(1, .2f, .2f);
            Vector3 forward = lineEnd - lineStart;
            Vector3 right = Vector3.Cross(up, forward).normalized;
            Handles.DrawLine(lineStart + finSize * right, lineEnd);
            Handles.DrawLine(lineStart, lineStart + finSize * right);
            lineStart = lineEnd;
        }
    }

    // Generate ControlPoint positions
    private void ShowPoint(int spline, int index) {
        Vector3[] points = new Vector3[3] {
            componentTransform.TransformPoint(
                component.splines[spline].points[index].GetAnchorPosition()),
            componentTransform.TransformPoint(
                component.splines[spline].points[index].GetHandlePosition(0)),
            componentTransform.TransformPoint(
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

        if (Handles.Button(position, componentRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
            selectedIndex = new int[] { index, handle };
            activeSpline = spline;
            handleRotation = Quaternion.identity;
            pointRotation = component.splines[activeSpline].points[index].GetRotation();
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
        position = Handles.DoPositionHandle(position, componentRotation);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(component, "Move Point");
            EditorUtility.SetDirty(component);
            if (handle == 0) {
                component.SetPointAnchorPosition(
                    component.splines[activeSpline].points[index],
                    componentTransform.InverseTransformPoint(position));
            }
            else {
                component.splines[activeSpline].points[index].SetHandlePosition(handle - 1,
                    componentTransform.InverseTransformPoint(position));
            }
        }
    }

    // Draw rotation handle
    private void RotationHandle(int index, Vector3 position) {
        if (Tools.pivotRotation == PivotRotation.Global) {
           EditorGUI.BeginChangeCheck();
            handleRotation = Handles.RotationHandle(handleRotation, position);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Rotate Point");
                EditorUtility.SetDirty(component);
                component.splines[activeSpline].points[index].SetRotation(handleRotation * pointRotation);
            }
        } else {
            EditorGUI.BeginChangeCheck();
            Quaternion rotation = component.splines[activeSpline].points[index].GetRotation();
            rotation = Handles.RotationHandle(rotation, position);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Rotate Point");
                EditorUtility.SetDirty(component);
                component.splines[activeSpline].points[index].SetRotation(rotation);
            }
        }
    }

    // Draw scale handle
    private void ScaleHandle(int index, Vector3 position) {
        EditorGUI.BeginChangeCheck();
        Quaternion rotation = component.splines[activeSpline].points[index].GetRotation();
        float scale = component.splines[activeSpline].points[index].GetHandleScale();
        Handles.color = new Color(.4f, .4f, .8f);
        scale = Handles.ScaleSlider(scale, position, rotation * Vector3.forward, rotation, HandleUtility.GetHandleSize(position), 0f);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(component, "Scale Point");
            EditorUtility.SetDirty(component);
            component.splines[activeSpline].points[index].SetHandleScale(scale);
        }
    }

    public override void OnInspectorGUI() {
        component = target as SplineComponent;
        tool = GUILayout.Toolbar(tool, new string[] { "Edit", "Multi", "Junction", "Spline" });
        EditorGUILayout.Space();

        switch (tool) {
            case 0:
                DrawEditInspector();
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
        }
    }

    public void DrawEditInspector() {
        if (activeSpline >= 0) {
            if (selectedIndex[0] >= 0 && selectedIndex[0] < component.splines[activeSpline].points.Count) {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Anchor:");
                if (component.splines[activeSpline].points[selectedIndex[0]].connectedIndex >= 0) {
                    if (GUILayout.Button("Previous", GUILayout.Height(EditorGUIUtility.singleLineHeight))) {
                        ControlPoint newPoint = component.GetConnectedPoint(component.splines[activeSpline].points[selectedIndex[0]], -1);
                        activeSpline = component.GetSpline(newPoint);
                        selectedIndex[0] = component.splines[activeSpline].points.IndexOf(newPoint);
                        selectedIndex[1] = 0;
                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("Next", GUILayout.Height(EditorGUIUtility.singleLineHeight))) {
                        ControlPoint newPoint = component.GetConnectedPoint(component.splines[activeSpline].points[selectedIndex[0]], 1);
                        activeSpline = component.GetSpline(newPoint);
                        selectedIndex[0] = component.splines[activeSpline].points.IndexOf(newPoint);
                        selectedIndex[1] = 0;
                        SceneView.RepaintAll();
                    }
                }
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                Vector3 anchor = EditorGUILayout.Vector3Field("Position",
                    component.splines[activeSpline].points[selectedIndex[0]].GetAnchorPosition());
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(component, "Move Anchor");
                    EditorUtility.SetDirty(component);
                    component.SetPointAnchorPosition(component.splines[activeSpline].points[selectedIndex[0]], anchor);
                }

                EditorGUI.BeginChangeCheck();
                Vector3 rotation = EditorGUILayout.Vector3Field("Rotation",
                    component.splines[activeSpline].points[selectedIndex[0]].GetEulerAngles());
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(component, "Move Anchor");
                    EditorUtility.SetDirty(component);
                    component.splines[activeSpline].points[selectedIndex[0]].SetRotation(Quaternion.Euler(rotation));
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                GUILayout.Label("Handles:");
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", component.splines[activeSpline].points[selectedIndex[0]].GetMode());
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(component, "Change Point Mode");
                    component.splines[activeSpline].points[selectedIndex[0]].SetMode(mode);
                    EditorUtility.SetDirty(component);
                }

                EditorGUI.BeginChangeCheck();
                float handle_0 = EditorGUILayout.FloatField(mode == BezierControlPointMode.Mirrored ? "Velocity" : "Velocity(Back)",
                    component.splines[activeSpline].points[selectedIndex[0]].GetHandleMagnitude(0));
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(component, "Move handle");
                    EditorUtility.SetDirty(component);
                    component.splines[activeSpline].points[selectedIndex[0]].SetHandleMagnitude(0, handle_0);
                }

                if (mode == BezierControlPointMode.Aligned) {
                    EditorGUI.BeginChangeCheck();
                    float handle_1 = EditorGUILayout.FloatField("Velocity (Forward)",
                        component.splines[activeSpline].points[selectedIndex[0]].GetHandleMagnitude(1));
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(component, "Move handle");
                        EditorUtility.SetDirty(component);
                        component.splines[activeSpline].points[selectedIndex[0]].SetHandleMagnitude(1, handle_1);
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                Texture before = Resources.Load<Texture>("AddPoint_MB");
                Texture after = Resources.Load<Texture>("AddPoint_MA");
                Texture remove = Resources.Load<Texture>("AddPoint_MR");
                if (selectedIndex[0] == 0) {
                    before = Resources.Load<Texture>("AddPoint_SB");
                    after = Resources.Load<Texture>("AddPoint_SA");
                    remove = Resources.Load<Texture>("AddPoint_SR");
                }
                else if (selectedIndex[0] == component.splines[activeSpline].points.Count - 1) {
                    before = Resources.Load<Texture>("AddPoint_EB");
                    after = Resources.Load<Texture>("AddPoint_EA");
                    remove = Resources.Load<Texture>("AddPoint_ER");
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(before, GUILayout.Height(EditorGUIUtility.singleLineHeight + 3))) {
                    Undo.RecordObject(component, "Add point before current");
                    EditorUtility.SetDirty(component);
                    component.splines[activeSpline].InsertControlPoint(selectedIndex[0]);
                }
                if (GUILayout.Button(after, GUILayout.Height(EditorGUIUtility.singleLineHeight + 3))) {
                    Undo.RecordObject(component, "Add point after current");
                    EditorUtility.SetDirty(component);
                    if (selectedIndex[0] == component.splines[activeSpline].points.Count - 1)
                        component.splines[activeSpline].AddControlPoint();
                    else
                        component.splines[activeSpline].InsertControlPoint(selectedIndex[0] + 1);
                    selectedIndex[0]++;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(remove, GUILayout.Height(EditorGUIUtility.singleLineHeight + 3))) {
                    Undo.RecordObject(component, "Remove point");
                    component.RemovePoint(activeSpline, selectedIndex[0]);
                    activeSpline = -1;
                    selectedIndex = new int[] { -1, 0 };
                    EditorUtility.SetDirty(component);
                }
                if (GUILayout.Button(Resources.Load<Texture>("AddPoint_NS"), GUILayout.Height(EditorGUIUtility.singleLineHeight + 3))) {
                    Undo.RecordObject(component, "Start new curve");
                    component.AddSpline(component.splines[activeSpline].points[selectedIndex[0]]);
                    activeSpline = component.splines.Count - 1;
                    selectedIndex = new int[] { 1, 0 };
                    EditorUtility.SetDirty(component);
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
        }
    }
}
