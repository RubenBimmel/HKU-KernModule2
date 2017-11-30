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

    public void Reset() {
        splines = new List<Spline> {
            new Spline(Vector3.forward)
        };
        connectedPoints = new List<ControlPoint> { };
    }

    public void Awake() {
        transform = gameObject.transform;
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

    public int GetConnectedPointCount (int connectedIndex) {
        int i = connectedIndex;
        int count = 0;
        while (connectedPoints.Count > i && connectedPoints[i].connectedIndex == connectedIndex) {
            i++;
            count++;
        }
        return count;
    }

    public int GetConnectedPointIndex (int spline, int point) {
        return connectedPoints.IndexOf(splines[spline].points[point]) - splines[spline].points[point].connectedIndex;
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
        }
        else {
            point.SetAnchorPosition(position);
        }
    }

    public void SetHandleMagnitude (int spline, int point, int index, float magnitude) {
        splines[spline].points[point].SetHandleMagnitude(index, magnitude);
    }

    public void SetHandlePosition (int spline, int point, int index, Vector3 position) {
        position = transform.InverseTransformPoint(position);
        splines[spline].points[point].SetHandlePosition(index, position);
    }

    public void SetMode(int spline, int point, BezierControlPointMode mode) {
        splines[spline].points[point].SetMode(mode);
    }

    public void SetRotation (int spline, int point, Quaternion rotation) {
        splines[spline].points[point].SetRotation(Quaternion.Inverse(transform.rotation) * rotation);
    }

    public void InsertControlPoint (int spline, int index) {
        splines[spline].InsertControlPoint(index);
    }

    public void AddControlPoint (int spline) {
        splines[spline].AddControlPoint();
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

    public void AddSpline(int spline, int index) {
        ControlPoint point = splines[spline].points[index];
        splines.Add(new Spline(point.GetAnchorPosition()));
        ConnectPoints(point, splines[splines.Count - 1].points[0]);
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
            splines[spline].points.Remove(point);
        }
        if (splines[spline].points.Count == 1) {
            RemovePoint(spline, 0);
        }
        else if (splines[spline].points.Count == 0) {
            splines.RemoveAt(spline);
        }
    }

    public void OnBeforeSerialize() {
    }

    public void OnAfterDeserialize() {
        connectedPoints = new List<ControlPoint>();
        for (int i = 0; i < splines.Count; i++) {
            for (int j = 0; j < splines[i].points.Count; j++) {
                if (splines[i].points[j].connectedIndex >= 0) {
                    connectedPoints.Add(splines[i].points[j]);
                }
            }
        }
        connectedPoints.Sort(delegate (ControlPoint a, ControlPoint b) { return a.connectedIndex.CompareTo(b.connectedIndex); });
    }
}
