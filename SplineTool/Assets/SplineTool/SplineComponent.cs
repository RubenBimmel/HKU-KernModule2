using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SplineComponent : MonoBehaviour, ISerializationCallbackReceiver {

    public List<Spline> splines;
    public List<ControlPoint> connectedPoints;
    private new Transform transform;

    public void Reset() {
        splines = new List<Spline> {
            new Spline(Vector3.forward, transform)
        };
        connectedPoints = new List<ControlPoint> { };
    }

    public void Awake() {
        transform = gameObject.transform;
    }

    public void AddSpline (ControlPoint point) {
        splines.Add(new Spline(point.GetAnchorPosition(), transform));
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

    // Adds both points to the connected points list (if they aren't already) and gives them the same connectedIndex
    public void ConnectPoints (ControlPoint first, ControlPoint second) {
        if (first.connectedIndex < 0) {
            connectedPoints.Add(first);
            first.connectedIndex = connectedPoints.IndexOf(first);
        }

        if (second.connectedIndex < 0) {
            connectedPoints.Add(second);
            second.connectedIndex = connectedPoints.IndexOf(second);
        }

        int newIndex = Mathf.Min(first.connectedIndex, second.connectedIndex);
        for (int i = 0; i < connectedPoints.Count; i++) {
            if (connectedPoints[i].connectedIndex == first.connectedIndex || connectedPoints[i].connectedIndex == second.connectedIndex) {
                connectedPoints[i].connectedIndex = newIndex;
            }
        }
    }

    //This function should be used instead directly in the control point to be able to move connected points
    public void SetPointAnchorPosition (ControlPoint point, Vector3 position) {
        if (point.connectedIndex >= 0) {
            for (int i = 0; i < connectedPoints.Count; i++) {
                if (connectedPoints[i].connectedIndex == point.connectedIndex)
                    connectedPoints[i].SetAnchorPosition(position);
            }
        } else {
            point.SetAnchorPosition(position);
        }
    }

    //Get a control point connected to point. If there is none it returns point.
    public ControlPoint GetConnectedPoint (ControlPoint point, int offset) {
        int start = connectedPoints.IndexOf(point);
        for (int i = start + offset; i < connectedPoints.Count && i >= 0; i += offset) {
            if (connectedPoints[i].connectedIndex == point.connectedIndex) {
                return connectedPoints[i];
            }
        }
        return point;
    }

    public Vector3 GetPoint (int spline, float t) {
        return transform.TransformPoint(splines[spline].GetPoint(t));
    }

    public Vector3 GetDirection (int spline, float t) {
        return transform.TransformPoint(splines[spline].GetDirection(t));
    }

    public Vector3 GetUp (int spline, float t) {
        return transform.TransformPoint(splines[spline].GetUp(t));
    }

    public int GetSpline (ControlPoint point) {
        for (int i = 0; i < splines.Count; i++) {
            if (splines[i].points.Contains(point)) {
                return i;
            }
        }
        return -1;
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

    /*public void OnDrawGizmos() {
        for (int i = 0; i < splines.Count; i++) {
            splines[i].Draw(transform);
        }
    }*/
}
