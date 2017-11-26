using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineComponent : MonoBehaviour, ISerializationCallbackReceiver {

    public List<Spline> splines;
    public List<ControlPoint> connectedPoints;

    public void Reset() {
        splines = new List<Spline> {
            new Spline(Vector3.forward)
        };
        connectedPoints = new List<ControlPoint> { };
    }

    public void AddSpline (ControlPoint point) {
        splines.Add(new Spline(point.GetAnchorPosition()));
        ConnectPoints(point, splines[splines.Count - 1].points[0]);
    }

    public void RemovePoint (int spline, int index) {
        ControlPoint point = splines[spline].points[index];
        if (point != null) {
            connectedPoints.Remove(point);
            splines[spline].points.Remove(point);
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
    public void SetControlPoint (ControlPoint point, Vector3 position) {
        if (point.connectedIndex >= 0) {
            for (int i = 0; i < connectedPoints.Count; i++) {
                if (connectedPoints[i].connectedIndex == point.connectedIndex)
                    connectedPoints[i].setAnchorPosition(position);
            }
        } else {
            point.setAnchorPosition(position);
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
}
