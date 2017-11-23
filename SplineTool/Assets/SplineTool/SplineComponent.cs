using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineComponent : MonoBehaviour {

    public List<Spline> splines;

    public void Reset() {
        splines = new List<Spline> {
            new Spline(Vector3.forward)
        };
    }

    public void AddSpline (ControlPoint point) {
        splines.Add(new Spline(point.GetAnchorPosition()));
        splines[splines.Count - 1].points[0].Connect(point);
        splines[splines.Count - 1].AddControlPoint();
    }
}
