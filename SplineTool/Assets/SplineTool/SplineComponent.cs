using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SplineComponent : MonoBehaviour, ISerializationCallbackReceiver {

    [SerializeField]
    private List<Spline> splines;
    [SerializeField]
    private List<ControlPoint> connectedPoints;
    private new Transform transform;
    private List<Transform> generatedContent;

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
        return splines[index].settings;
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
        splines[index].settings = settings;
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
        if (splineCount != generatedContent.Count)
            ResetGeneratedContent();
        splines[index].ResetArcLengthTable();
        if (splines[index].settings) {
            ApplySettings(index);
        } else {
            ClearBranch(index);
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
        if (generatedContent != null) {
            for (int i = 0; i < generatedContent.Count; i++) {
                if (generatedContent[i])
                    DestroyImmediate(generatedContent[i].gameObject);
            }
        }

        Resources.UnloadUnusedAssets();
        generatedContent = new List<Transform>();

        for (int i = 0; i < splineCount; i++) {
            Transform newTransform = new GameObject().transform;
            newTransform.parent = transform;
            newTransform.name = GetSplineName(i);
            newTransform.hideFlags = HideFlags.NotEditable;
            generatedContent.Add(newTransform);
        }
    }

    private void AddGeneratedBranch() {
        Transform newTransform = new GameObject().transform;
        newTransform.parent = transform;
        newTransform.name = GetSplineName(splineCount - 1);
        newTransform.hideFlags = HideFlags.NotEditable;
        generatedContent.Add(newTransform);
    }

    private void RemoveGeneratedBranch(int index) {
        if (generatedContent[index])
            DestroyImmediate(generatedContent[index].gameObject);
        Resources.UnloadUnusedAssets();
        generatedContent.RemoveAt(index);
    }

    private void ApplySettings (int index) {
    }

    private void ClearBranch (int index) {
        foreach( Transform child in generatedContent[index]) {
            DestroyImmediate(child.gameObject);
            Resources.UnloadUnusedAssets();
        }
    }
}
