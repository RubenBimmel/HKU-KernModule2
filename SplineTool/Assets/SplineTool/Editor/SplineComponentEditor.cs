using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineComponent))]
public class SplineComponentEditor : Editor {
    private static SplineComponent component;
    private Transform componentTransform;
    private Quaternion componentRotation;
    private Quaternion handleRotation;
    private Quaternion pointRotation;

    private PivotRotation currentPivotRotation;

    private int tool;

    private const float handleSize = 0.05f;
    private const float pickSize = 0.07f;
    private int selectedIndex = -1;
    private int selectedHandle = 0;
    private int activeSpline = -1;

    private const float stepSize = .2f;
    private const float finSize = .03f;

    [MenuItem("CONTEXT/SplineComponent/Reset generated content")]
    public static void ResetGeneratedContent () {
        component.ResetGeneratedContent();
    }

    // Draw splines and handles
    private void OnSceneGUI() {
        component = target as SplineComponent;
        componentTransform = component.transform;
        componentRotation = Tools.pivotRotation == PivotRotation.Local ? componentTransform.rotation : Quaternion.identity;

        DrawBeziers();

        if (Tools.pivotRotation != currentPivotRotation) {
            currentPivotRotation = Tools.pivotRotation;
            handleRotation = Quaternion.identity;
            if (activeSpline >= 0 && selectedIndex >= 0)
                pointRotation = component.GetRotation(activeSpline, selectedIndex);
        }
    }

    private void DrawBeziers() {
        for (int i = 0; i < component.splineCount; i++) {
            Color bezierColor = Color.cyan;
            float bezierWidth = 2f;
            if (i == activeSpline & tool != 1 && tool != 2) {
                bezierColor = Color.white;
                bezierWidth = 2f;
            }
            for (int j = 1; j < component.PointCount(i); j++) {
                Handles.DrawBezier(
                    component.GetPoint(i, j - 1),
                    component.GetPoint(i, j),
                    component.GetHandle(i, j - 1, 1),
                    component.GetHandle(i, j, 0),
                    bezierColor,
                    null,
                    bezierWidth);
            }

            for (int j = 0; j < component.PointCount(i); j++) {
                switch (tool) {
                    case -1:
                        ShowControlPoint(i, j);
                        break;
                    case 0:
                        ShowControlPoint(i, j);
                        break;
                    case 1:
                        ShowSimplePoint(i, j);
                        break;
                    case 2:
                        if (component.GetConnectedIndex(i, j) >= 0)
                            ShowSimplePoint(i, j);
                        break;
                    case 3:
                        ShowSplinePoint(i);
                        break;
                }
            }
        }

        if (activeSpline >= 0 && tool != 1 && tool != 2)
            ShowAngles(activeSpline);
    }

    // Generate ControlPoint positions
    private void ShowControlPoint(int spline, int index) {
        Vector3[] points = new Vector3[3] {
            component.GetPoint(spline, index),
            component.GetHandle(spline, index, 0),
            component.GetHandle(spline, index, 1)
        };

        if (selectedIndex == index && spline == activeSpline) {
            Handles.color = Color.gray;
            Handles.DrawLine(points[0], points[1]);
            Handles.DrawLine(points[0], points[2]);
            for (int i = 0; i < 3; i++) {
                ShowControlPointButton(spline, index, i, points[i], true);
            }
        }
        else {
            int connectedIndex = component.GetConnectedIndex(spline, index);
            if (connectedIndex < 0) {
                ShowControlPointButton(spline, index, 0, points[0], false);
            }
            else if (selectedIndex < 0 || activeSpline < 0 || connectedIndex != component.GetConnectedIndex(activeSpline, selectedIndex)) {
                ShowControlPointButton(spline, index, 0, points[0], false);
            }
        }
    }

    // Draw ControlPoints
    private void ShowControlPointButton(int spline, int index, int handle, Vector3 position, bool selected) {
        float size = HandleUtility.GetHandleSize(position);
        Handles.color = selected ? Color.white : Color.blue;

        if (Handles.Button(position, componentRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
            selectedIndex = index;
            selectedHandle = handle;
            activeSpline = spline;
            handleRotation = Quaternion.identity;
            pointRotation = component.GetRotation(spline, index);
            Repaint();
        }
        if (selected) {
            switch (Tools.current) {
                case Tool.Move:
                    if (selectedHandle == handle)
                        MoveHandle(index, handle, position);
                    break;
                case Tool.Rotate:
                    if (handle == 0)
                        RotationHandle(index, position);
                    break;
                case Tool.Scale:
                    if (handle == 0)
                        ScaleHandle(index, position);
                    break;
            }
        }
    }

    // Generate ControlPoint positions
    private void ShowSimplePoint(int spline, int index) {
        Vector3 point = component.GetPoint(spline, index);

        if (selectedIndex == index && spline == activeSpline) {
            ShowControlPointButton(spline, index, 0, point, true);
        }
        else {
            int connectedIndex = component.GetConnectedIndex(spline, index);
            if (connectedIndex < 0) {
                ShowSimplePointButton(spline, index, point, false);
            }
            else if (selectedIndex < 0 || activeSpline < 0 || connectedIndex != component.GetConnectedIndex(activeSpline, selectedIndex)) {
                ShowSimplePointButton(spline, index, point, false);
            }
        }
    }

    // Draw ControlPoints
    private void ShowSimplePointButton(int spline, int index, Vector3 position, bool selected) {
        float size = HandleUtility.GetHandleSize(position);
        Handles.color = selected ? Color.white : Color.blue;

        if (Handles.Button(position, componentRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
            selectedIndex = index;
            selectedHandle = 0;
            activeSpline = spline;
            handleRotation = Quaternion.identity;
            pointRotation = component.GetRotation(spline, index);
            Repaint();
        }
        if (selected) {
            switch (Tools.current) {
                case Tool.Move:
                    MoveHandle(index, 0, position);
                    break;
                case Tool.Rotate:
                    RotationHandle(index, position);
                    break;
                case Tool.Scale:
                    ScaleHandle(index, position);
                    break;
            }
        }
    }

    // Generate ControlPoint positions
    private void ShowSplinePoint(int spline) {
        Vector3 point = component.GetPoint(spline, .5f * component.GetArcLength(spline));
        bool selected = (spline == activeSpline);

        float size = HandleUtility.GetHandleSize(point);
        Handles.color = selected ? Color.white : Color.blue;

        if (Handles.Button(point, componentRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
            selectedIndex = -1;
            selectedHandle = 0;
            activeSpline = spline;
            Repaint();
        }
    }

    // Draw movement handles
    private void MoveHandle(int index, int handle, Vector3 position) {
        EditorGUI.BeginChangeCheck();
        Quaternion rotation = Tools.pivotRotation == PivotRotation.Global ? componentRotation : component.GetRotation(activeSpline, index);
        position = Handles.DoPositionHandle(position, rotation);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(component, "Move Point");
            EditorUtility.SetDirty(component);
            if (handle == 0) {
                component.SetAnchorPosition(activeSpline, index, position);
            }
            else {
                component.SetHandlePosition(activeSpline, index, handle - 1, position);
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
                if (tool == 0)
                    component.SetRotation(activeSpline, index, handleRotation * pointRotation);
                else if (tool == 2)
                    component.RotateConnection(activeSpline, index, handleRotation * pointRotation);
            }
        }
        else {
            EditorGUI.BeginChangeCheck();
            Quaternion rotation = component.GetRotation(activeSpline, index);
            rotation = Handles.RotationHandle(rotation, position);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Rotate Point");
                EditorUtility.SetDirty(component);
                if (tool == 0)
                    component.SetRotation(activeSpline, index, rotation);
                else if (tool == 2)
                    component.RotateConnection(activeSpline, index, rotation);
            }
        }
    }

    // Draw scale handle
    private void ScaleHandle(int index, Vector3 position) {
        if (tool == 0) {
            EditorGUI.BeginChangeCheck();
            Quaternion rotation = component.GetRotation(activeSpline, index);
            float scale = component.GetHandleMagnitude(activeSpline, index, 1);
            Handles.color = new Color(.4f, .4f, .8f);
            scale = Handles.ScaleSlider(scale, position, rotation * Vector3.forward, rotation, HandleUtility.GetHandleSize(position), 0f);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Scale Point");
                EditorUtility.SetDirty(component);
                float scale2 = scale / component.GetHandleMagnitude(activeSpline, index, 1) * component.GetHandleMagnitude(activeSpline, index, 0);
                component.SetHandleMagnitude(activeSpline, index, 1, scale);
                component.SetHandleMagnitude(activeSpline, index, 0, scale2);
            }
        }
        else {
            EditorGUI.BeginChangeCheck();
            Quaternion rotation = Quaternion.identity;
            float scale = component.GetHandleMagnitude(activeSpline, index, 1);
            Vector3 scale3D = scale * Vector3.one;
            Handles.color = new Color(.4f, .4f, .8f);
            scale3D = Handles.ScaleHandle(scale3D, position, rotation, HandleUtility.GetHandleSize(position));
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Scale Point");
                EditorUtility.SetDirty(component);
                component.ScaleConnection(activeSpline, index, scale3D / scale);
            }
        }
    }

    // Draw the fins that show the angle and orientation of a spline
    private void ShowAngles(int spline) {
        /*Vector3 lineStart = component.GetPoint(spline, 0f);
        Gizmos.color = Color.cyan;
        for (float i = stepSize; i <= component.GetArcLength(spline); i += stepSize) {
            Vector3 lineEnd = component.GetPoint(spline, i);
            Handles.color = new Color(.2f, 1, .2f);
            Vector3 up = component.GetUp(spline, i - stepSize);
            Handles.DrawLine(lineStart + finSize * up, lineEnd);
            Handles.DrawLine(lineStart, lineStart + finSize * up);
            Handles.color = new Color(1, .2f, .2f);
            Vector3 forward = component.GetDirection(spline, i - stepSize);
            Vector3 right = Vector3.Cross(up, forward).normalized;
            Handles.DrawLine(lineStart + finSize * right, lineEnd);
            Handles.DrawLine(lineStart, lineStart + finSize * right);
            lineStart = lineEnd;
        }*/
    }

    public override void OnInspectorGUI() {
        component = target as SplineComponent;
        int newtool = GUILayout.Toolbar(tool, new string[] { "Edit", "Multi", "Junction", "Spline" });
        if (tool != newtool) {
            tool = newtool;
            if (tool >= 2) {
                activeSpline = -1;
                selectedIndex = -1;
            }
            SceneView.RepaintAll();
        }
        EditorGUILayout.Space();

        switch (tool) {
            case 0:
                DrawEditInspector();
                break;
            case 1:
                GUILayout.Label("Not available");
                break;
            case 2:
                DrawJunctionInspector();
                break;
            case 3:
                DrawSplineInspector();
                break;
        }
    }

    private void DrawEditInspector() {
        if (selectedIndex >= 0) {
            if (component.GetConnectedIndex(activeSpline, selectedIndex) >= 0) {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Previous", GUILayout.Height(EditorGUIUtility.singleLineHeight))) {
                    ControlPoint newPoint = component.GetConnectedPoint(activeSpline, selectedIndex, -1);
                    activeSpline = component.GetSpline(newPoint);
                    selectedIndex = component.GetIndex(activeSpline, newPoint);
                    selectedHandle = 0;
                    SceneView.RepaintAll();
                }
                int connectedIndex = component.GetConnectedIndex(activeSpline, selectedIndex);
                string label = string.Concat("( Handle ", component.GetIndexInConnection(activeSpline, selectedIndex) + 1, " of ", component.GetConnectionPointCount(connectedIndex), " )");
                GUILayout.Label(label, GUILayout.Width(100));
                if (GUILayout.Button("  Next  ", GUILayout.Height(EditorGUIUtility.singleLineHeight))) {
                    ControlPoint newPoint = component.GetConnectedPoint(activeSpline, selectedIndex, 1);
                    activeSpline = component.GetSpline(newPoint);
                    selectedIndex = component.GetIndex(activeSpline, newPoint);
                    selectedHandle = 0;
                    SceneView.RepaintAll();
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Anchor:");
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            Vector3 anchor = EditorGUILayout.Vector3Field("Position", component.GetPoint(activeSpline, selectedIndex));
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Move Anchor");
                EditorUtility.SetDirty(component);
                component.SetAnchorPosition(activeSpline, selectedIndex, anchor);
            }

            EditorGUI.BeginChangeCheck();
            Vector3 rotation = EditorGUILayout.Vector3Field("Rotation", component.GetEulerAngles(activeSpline, selectedIndex));
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Rotate Anchor");
                EditorUtility.SetDirty(component);
                component.SetRotation(activeSpline, selectedIndex, Quaternion.Euler(rotation));
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            GUILayout.Label("Handles:");
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", component.GetMode(activeSpline, selectedIndex));
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Change Point Mode");
                EditorUtility.SetDirty(component);
                component.SetMode(activeSpline, selectedIndex, mode);
            }

            EditorGUI.BeginChangeCheck();
            float handle_0 = EditorGUILayout.FloatField(mode == BezierControlPointMode.Mirrored ? "Scale" : "Scale (Back)",
                component.GetHandleMagnitude(activeSpline, selectedIndex, 0));
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Move handle");
                EditorUtility.SetDirty(component);
                component.SetHandleMagnitude(activeSpline, selectedIndex, 0, handle_0);
            }

            if (mode == BezierControlPointMode.Aligned) {
                EditorGUI.BeginChangeCheck();
                float handle_1 = EditorGUILayout.FloatField("Scale (Forward)",
                    component.GetHandleMagnitude(activeSpline, selectedIndex, 1));
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(component, "Move handle");
                    EditorUtility.SetDirty(component);
                    component.SetHandleMagnitude(activeSpline, selectedIndex, 1, handle_1);
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            Texture before = Resources.Load<Texture>("AddPoint_MB");
            Texture after = Resources.Load<Texture>("AddPoint_MA");
            Texture remove = Resources.Load<Texture>("AddPoint_MR");
            if (selectedIndex == 0) {
                before = Resources.Load<Texture>("AddPoint_SB");
                after = Resources.Load<Texture>("AddPoint_SA");
                remove = Resources.Load<Texture>("AddPoint_SR");
            }
            else if (selectedIndex == component.PointCount(activeSpline) - 1) {
                before = Resources.Load<Texture>("AddPoint_EB");
                after = Resources.Load<Texture>("AddPoint_EA");
                remove = Resources.Load<Texture>("AddPoint_ER");
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(before, GUILayout.Height(EditorGUIUtility.singleLineHeight + 3))) {
                Undo.RecordObject(component, "Add point before current");
                EditorUtility.SetDirty(component);
                component.InsertControlPoint(activeSpline, selectedIndex);
            }
            if (GUILayout.Button(after, GUILayout.Height(EditorGUIUtility.singleLineHeight + 3))) {
                Undo.RecordObject(component, "Add point after current");
                EditorUtility.SetDirty(component);
                if (selectedIndex == component.PointCount(activeSpline) - 1)
                    component.AddControlPoint(activeSpline);
                else
                    component.InsertControlPoint(activeSpline, selectedIndex + 1);
                selectedIndex++;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(remove, GUILayout.Height(EditorGUIUtility.singleLineHeight + 3))) {
                Undo.RecordObject(component, "Remove point");
                EditorUtility.SetDirty(component);
                component.RemovePoint(activeSpline, selectedIndex);
                activeSpline = -1;
                selectedIndex = -1;
                selectedHandle = 0;
            }
            if (GUILayout.Button(Resources.Load<Texture>("AddPoint_NS"), GUILayout.Height(EditorGUIUtility.singleLineHeight + 3))) {
                Undo.RecordObject(component, "Start new curve");
                EditorUtility.SetDirty(component);
                component.AddSpline(activeSpline, selectedIndex);
                activeSpline = component.splineCount - 1;
                selectedIndex = 1;
                selectedHandle = 0;
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
    }

    private void DrawJunctionInspector() {
        if (selectedIndex >= 0) {
            GUILayout.Label("Anchor:");
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            Vector3 anchor = EditorGUILayout.Vector3Field("Position", component.GetPoint(activeSpline, selectedIndex));
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Move Anchor");
                EditorUtility.SetDirty(component);
                component.SetAnchorPosition(activeSpline, selectedIndex, anchor);
            }

            EditorGUI.BeginChangeCheck();
            Vector3 rotation = EditorGUILayout.Vector3Field("Rotation", component.GetEulerAngles(activeSpline, selectedIndex));
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Move Anchor");
                EditorUtility.SetDirty(component);
                component.RotateConnection(activeSpline, selectedIndex, Quaternion.Euler(rotation));
            }

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Flatten X")) {
                Undo.RecordObject(component, "Flatten X");
                EditorUtility.SetDirty(component);
                component.ScaleConnection(activeSpline, selectedIndex, new Vector3(0, 1, 1));
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Flatten Y")) {
                Undo.RecordObject(component, "Flatten Y");
                EditorUtility.SetDirty(component);
                component.ScaleConnection(activeSpline, selectedIndex, new Vector3(1, 0, 1));
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Flatten Z")) {
                Undo.RecordObject(component, "Flatten Z");
                EditorUtility.SetDirty(component);
                component.ScaleConnection(activeSpline, selectedIndex, new Vector3(1, 1, 0));
                SceneView.RepaintAll();
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }
    }

    private void DrawSplineInspector() {
        if (activeSpline >= 0) {
            GUILayout.Label("Spline:");
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            string name = EditorGUILayout.TextField("Name", component.GetSplineName(activeSpline));
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Change spline name");
                EditorUtility.SetDirty(component);
                component.SetSplineName(activeSpline, name);
            }

            EditorGUI.BeginChangeCheck();
            SplineSettings settings = (SplineSettings) EditorGUILayout.ObjectField("Settings", component.GetSplineSettings(activeSpline), typeof(SplineSettings), true);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(component, "Change spline settings");
                EditorUtility.SetDirty(component);
                component.SetSplineSettings(activeSpline, settings);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }
    }
}
