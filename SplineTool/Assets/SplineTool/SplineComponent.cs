using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineComponent : MonoBehaviour {

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
        if (point.connectedIndex < 0) {
            connectedPoints.Add(point);
            
        }
        connectedPoints.Add(splines[splines.Count - 1].points[0]);
        int index = connectedPoints.IndexOf(point);
        point.connectedIndex = index;
        splines[splines.Count - 1].points[0].connectedIndex = index;
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
}
