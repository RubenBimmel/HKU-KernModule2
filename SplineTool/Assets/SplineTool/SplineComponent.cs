﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SplineComponent : MonoBehaviour, ISerializationCallbackReceiver {

    [SerializeField]
    private List<Spline> splines;
    [SerializeField]
    private List<ControlPoint> connectedPoints;
    private new Transform transform;
    private List<List<Transform>> generatedContent;

    public void Reset() {
        splines = new List<Spline> {
            new Spline(Vector3.forward, 0)
        };
        connectedPoints = new List<ControlPoint> { };
        ResetGeneratedContent();
    }

    public void Awake() {
        transform = gameObject.transform;
        ResetGeneratedContent();
    }

    public Vector3 GetPoint (int spline, int point) {
        return transform.TransformPoint(splines[spline].points[point].GetAnchorPosition());
    }

    public Vector3 GetPoint (int spline, float t) {
        return transform.TransformPoint(splines[spline].GetPoint(t));
    }

    public Vector3 GetDirection (int spline, float t) {
        return transform.TransformDirection(splines[spline].GetDirection(t));
    }

    public Vector3 GetUp (int spline, float t) {
        return transform.TransformDirection(splines[spline].GetUp(t));
    }

    public float GetArcLength(int spline) {
        return splines[spline].GetArcLength();
    }

    public int GetSpline (ControlPoint point) {
        for (int i = 0; i < splines.Count; i++) {
            if (splines[i].points.Contains(point)) {
                return i;
            }
        }
        return -1;
    }

    public int GetIndex (int spline, ControlPoint point) {
        return splines[spline].points.IndexOf(point);
    }

    public int splineCount {
        get {
            return splines.Count;
        }
    }

    public int PointCount (int spline) {
        return splines[spline].points.Count;
    }

    public Quaternion GetRotation (int spline, int point) {
        return transform.rotation * splines[spline].points[point].GetRotation();
    }

    public Vector3 GetEulerAngles (int spline, int point) {
        return transform.eulerAngles + splines[spline].points[point].GetEulerAngles();
    }

    public Vector3 GetHandle(int spline, int point, int index) {
        return transform.TransformPoint(splines[spline].points[point].GetHandlePosition(index));
    }

    public float GetHandleMagnitude(int spline, int point, int index) {
        return splines[spline].points[point].GetHandleMagnitude(index);
    }

    public BezierControlPointMode GetMode (int spline, int point) {
        return splines[spline].points[point].GetMode();
    }

    public int GetConnectedIndex (int spline, int point) {
        return splines[spline].points[point].connectedIndex;
    }

    //Get a control point connected to point. If there is none it returns point.
    public ControlPoint GetConnectedPoint(int spline, int point, int offset) {
        int index = connectedPoints.IndexOf(splines[spline].points[point]);
        int newIndex = index + offset;
        if (newIndex >= 0 && newIndex < connectedPoints.Count) {
            if (connectedPoints[newIndex].connectedIndex == connectedPoints[index].connectedIndex) {
                return connectedPoints[newIndex];
            }
        }
        return connectedPoints[index];
    }

    public int connectedPointCount {
        get {
            return connectedPoints.Count;
        }
    }

    public int GetConnectionPointCount(int connectedIndex) {
        int i = connectedIndex;
        int count = 0;
        while (connectedPoints.Count > i && connectedPoints[i].connectedIndex == connectedIndex) {
            i++;
            count++;
        }
        return count;
    }

    public int GetIndexInConnection (int spline, int point) {
        return connectedPoints.IndexOf(splines[spline].points[point]) - splines[spline].points[point].connectedIndex;
    }

    public int GetConnectedPointIndex(int index) {
        return connectedPoints[index].connectedIndex;
    }

    public string GetSplineName(int index) {
        return splines[index].name;
    }

    public SplineSettings GetSplineSettings (int index) {
        return splines[index].GetSettings();
    }

    //This function should be used instead directly in the control point to be able to move connected points
    public void SetAnchorPosition (int spline, int index, Vector3 position) {
        ControlPoint point = splines[spline].points[index];
        position = transform.InverseTransformPoint(position);
        if (point.connectedIndex >= 0) {
            for (int i = 0; i < connectedPoints.Count; i++) {
                if (connectedPoints[i].connectedIndex == point.connectedIndex)
                    connectedPoints[i].SetAnchorPosition(position);
            }

            // QQQ more effective way?
            for (int i = 0; i < splineCount; i++) {
                UpdateSpline(i);
            }
        }
        else {
            point.SetAnchorPosition(position);
            UpdateSpline(spline);
        }
    }

    public void SetHandleMagnitude (int spline, int point, int index, float magnitude) {
        splines[spline].points[point].SetHandleMagnitude(index, magnitude);
        UpdateSpline(spline);
    }

    public void SetHandlePosition (int spline, int point, int index, Vector3 position) {
        ControlPoint controlPoint = splines[spline].points[point];
        position = transform.InverseTransformPoint(position);
        controlPoint.SetHandlePosition(index, position);
        UpdateSpline(spline);
    }

    public void SetMode (int spline, int point, BezierControlPointMode mode) {
        splines[spline].points[point].SetMode(mode);
    }

    public void SetRotation (int spline, int point, Quaternion rotation) {
        ControlPoint controlPoint = splines[spline].points[point];
        controlPoint.SetRotation(Quaternion.Inverse(transform.rotation) * rotation);
        UpdateSpline(spline);
    }

    public void RotateConnection (int spline, int point, Quaternion newRotation) {
        int index = GetConnectedIndex(spline, point);
        Quaternion resetRotation = Quaternion.Inverse(splines[spline].points[point].GetRotation());
        for (int i = 0; i < connectedPoints.Count; i++) {
            if (connectedPoints[i].connectedIndex == index) {
                Quaternion rotation = connectedPoints[i].GetRotation();
                connectedPoints[i].SetRotation(Quaternion.Inverse(transform.rotation) * newRotation * resetRotation * rotation);
            }
        }

        // QQQ more effective way?
        for (int i = 0; i < splineCount; i++) {
            UpdateSpline(i);
        }
    }

    public void ScaleConnection (int spline, int point, Vector3 scale) {
        for (int i = 0; i < 3; i++) {
            if (scale[i] < 0f) {
                scale[i] = 0f;
            }
        }
        int index = GetConnectedIndex(spline, point);
        for(int i = 0; i < connectedPoints.Count; i++) {
            if (connectedPoints[i].connectedIndex == index) {
                connectedPoints[i].Scale(scale);
            }
        }

        // QQQ more effective way?
        for (int j = 0; j < splineCount; j++) {
            UpdateSpline(j);
        }
    }

    public void InsertControlPoint (int spline, int index) {
        splines[spline].InsertControlPoint(index);
        UpdateSpline(spline);
    }

    public void AddControlPoint (int spline) {
        splines[spline].AddControlPoint();
        UpdateSpline(spline);
    }

    // Adds both points to the connected points list (if they aren't already) and gives them the same connectedIndex
    public void ConnectPoints(ControlPoint first, ControlPoint second) {
        if (first.connectedIndex >= 0 && second.connectedIndex >= 0) {
            Debug.LogWarning("Can't connect two junctions!");
        }
        else if (first.connectedIndex >= 0) {
            connectedPoints.Insert(first.connectedIndex, second);
            second.connectedIndex = first.connectedIndex;
        }
        else if(second.connectedIndex >= 0) {
            connectedPoints.Insert(second.connectedIndex, first);
            first.connectedIndex = second.connectedIndex;
        }
        else {
            connectedPoints.Add(first);
            connectedPoints.Add(second);
            first.connectedIndex = connectedPoints.IndexOf(first);
            second.connectedIndex = first.connectedIndex;
        }
    }

    public void SetSplineName(int index, string name) {
        splines[index].name = name;
    }

    public void SetSplineSettings(int index, SplineSettings settings) {
        splines[index].SetSettings(settings);
        UpdateSpline(index);
    }

    public void AddSpline(int spline, int index) {
        ControlPoint point = splines[spline].points[index];
        splines.Add(new Spline(point.GetAnchorPosition(), splineCount));
        ConnectPoints(point, splines[splines.Count - 1].points[0]);
        AddGeneratedBranch();
    }

    public void RemovePoint(int spline, int index) {
        ControlPoint point = splines[spline].points[index];
        if (point != null) {
            int connectedIndex = point.connectedIndex;
            if (connectedIndex >= 0) {
                connectedPoints.Remove(point);

                int count = 0;
                ControlPoint lastPoint = null;
                for (int i = 0; i < connectedPoints.Count; i++) {
                    if (connectedPoints[i].connectedIndex == connectedIndex) {
                        count++;
                        lastPoint = connectedPoints[i];
                    }
                }
                if (count == 1) {
                    lastPoint.connectedIndex = -1;
                    connectedPoints.Remove(lastPoint);
                }
            }
            splines[spline].RemoveControlPoint(point);
            UpdateSpline(spline);
        }

        if (splines[spline].points.Count == 1) {
            RemovePoint(spline, 0);
        }
        else if (splines[spline].points.Count == 0) {
            RemoveSpline(spline);
        }
    }

    public void RemoveSpline (int index) {
        splines.RemoveAt(index);
        RemoveGeneratedBranch(index);
    }

    public void UpdateSpline (int index) {
        splines[index].ResetArcLengthTable();
        if (GetSplineSettings(index)) {
            ApplySettings(index);
        } else {
            ClearBranch(index);
        }
    }

    public void UpdateSpline(SplineSettings settings) {
        for (int i = 0; i < splineCount; i++) {
            if (GetSplineSettings(i) == settings) {
                UpdateSpline(i);
            }
        }
    }

    public void OnBeforeSerialize() {
    }

    public void OnAfterDeserialize() {
        for (int i = 0; i < splineCount; i++) {
            splines[i].ResetArcLengthTable();
        }

        connectedPoints = new List<ControlPoint>();
        for (int i = 0; i < splineCount; i++) {
            for (int j = 0; j < splines[i].points.Count; j++) {
                if (splines[i].points[j].connectedIndex >= 0) {
                    connectedPoints.Add(splines[i].points[j]);
                }
            }
        }
        connectedPoints.Sort(delegate (ControlPoint a, ControlPoint b) { return a.connectedIndex.CompareTo(b.connectedIndex); });
    }

    public void ResetGeneratedContent() {
        foreach (Transform child in transform) {
            if (child.name.StartsWith("Spline_")) {
                DestroyImmediate(child.gameObject);
            }
        }

        Resources.UnloadUnusedAssets();

        for (int i = 0; i < splineCount; i++) {
            UpdateSpline(i);
        }
    }

    private Transform AddGeneratedBranch() {
        return AddGeneratedBranch(splineCount - 1);
    }

    private Transform AddGeneratedBranch(int index) {
        Transform newTransform = new GameObject().transform;
        newTransform.parent = transform;
        newTransform.localPosition = Vector3.zero;
        newTransform.localRotation = Quaternion.identity;
        newTransform.localScale = Vector3.one;
        newTransform.name = GetSplineName(index);
        newTransform.hideFlags = HideFlags.NotEditable;
        return newTransform;
    }

    private void RemoveGeneratedBranch(int index) {
        Transform child = transform.Find(GetSplineName(index));
        DestroyImmediate(child.gameObject);
        Resources.UnloadUnusedAssets();
    }

    private void ApplySettings (int index) {
        Transform splineParent = transform.Find(GetSplineName(index));
        SplineSettings settings = GetSplineSettings(index);
        if (splineParent == null) {
            splineParent = AddGeneratedBranch(index);
        }
        for (int i = 0; i < settings.generated.Length; i++) {
            if (splines[index].AssetIsActive(i)) {
                string name = settings.generated[i].name;
                if (name.Length == 0) {
                    name = string.Concat("Generated Mesh ", i.ToString("D2"));
                }
                name = string.Concat(i.ToString("D2"), "-", name);
                Transform newGenerated = splineParent.Find(name);
                if (newGenerated == null) {
                    newGenerated = new GameObject().transform;
                    newGenerated.parent = splineParent;
                    newGenerated.localPosition = Vector3.zero;
                    newGenerated.localRotation = Quaternion.identity;
                    newGenerated.localScale = Vector3.one;
                    newGenerated.name = name;
                    newGenerated.gameObject.AddComponent<MeshFilter>();
                    newGenerated.gameObject.AddComponent<MeshRenderer>();
                }
                newGenerated.GetComponent<MeshFilter>().mesh = settings.generated[i].generate(splines[index]);
                newGenerated.GetComponent<MeshRenderer>().material = settings.generated[i].material;
            }
        }

        for (int i = 0; i < settings.objects.Length; i++) {
            if (splines[index].AssetIsActive(settings.generated.Length + i) && GetSplineSettings(index).objects[i] != null) {
                string name = settings.objects[i].name;
                if (name.Length == 0) {
                    name = settings.objects[i].objectReference.name;
                }
                name = string.Concat((settings.generated.Length + i).ToString("D2"), "-", name);
                Transform objectParent = null;
                foreach (Transform parent in splineParent) {
                    if (parent.name == name) {
                        objectParent = parent;
                    }
                }
                if (objectParent == null) {
                    objectParent = new GameObject().transform;
                    objectParent.parent = splineParent;
                    objectParent.localPosition = Vector3.zero;
                    objectParent.localRotation = Quaternion.identity;
                    objectParent.localScale = Vector3.one;
                    objectParent.name = name;
                }

                int child = 0;
                Vector3 lastPosition = Vector3.one * float.MaxValue;
                for (float j = settings.objects[i].offset;
                    
                    j < splines[index].GetArcLength() - settings.objects[i].offset;
                    j += settings.objects[i].distance) {

                    Transform newObject;
                    if (objectParent.childCount > child) {
                        newObject = objectParent.GetChild(child);
                    }
                    else {
                        newObject = (Transform)PrefabUtility.InstantiatePrefab(settings.objects[i].objectReference);
                        newObject.parent = objectParent;
                    }

                    Vector3 forward = splines[index].GetDirection(j).normalized;
                    Vector3 up = splines[index].GetUp(j).normalized;
                    Vector3 right = Vector3.Cross(forward, up).normalized;

                    Vector3 position = splines[index].GetPoint(j) + right * settings.objects[i].position.x + up * settings.objects[i].position.y;
                    if (settings.objects[i].type == offsetType.globalDistance) {
                        if (lastPosition.x < float.MaxValue) {
                            float realDistance = (lastPosition - position).magnitude;
                            j += settings.objects[i].distance - realDistance;
                            forward = splines[index].GetDirection(j).normalized;
                            up = splines[index].GetUp(j).normalized;
                            right = Vector3.Cross(forward, up).normalized;
                            position = splines[index].GetPoint(j) + right * settings.objects[i].position.x + up * settings.objects[i].position.y;
                        }
                        lastPosition = position;
                    }

                    Vector3 splineRotation = Spline.GetEulerAngles(up, forward);
                    if (!settings.objects[i].constraints[0]) splineRotation.x = 0;
                    if (!settings.objects[i].constraints[1]) splineRotation.y = 0;
                    if (!settings.objects[i].constraints[2]) splineRotation.z = 0;
                    Quaternion rotation = Quaternion.Euler(splineRotation) * Quaternion.Euler(settings.objects[i].rotation);

                    newObject.position = transform.TransformPoint(position);
                    newObject.rotation = transform.rotation * rotation;
                    newObject.localScale = settings.objects[i].scale;

                    child++;
                }
                while (objectParent.childCount > child) {
                    DestroyImmediate(objectParent.GetChild(child).gameObject);
                }
            }
        }
    }

    private void ClearBranch (int index) {
        Transform splineParent = transform.Find(GetSplineName(index));
        if (splineParent != null) {
            foreach (Transform child in splineParent) {
                DestroyImmediate(child.gameObject);
                Resources.UnloadUnusedAssets();
            }
        }
    }
}
