using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Spline {

    public List<ControlPoint> points;

    public Spline(Vector3 position) {
        points = new List<ControlPoint> {
            new ControlPoint(position, Vector3.forward),
            new ControlPoint(position + Vector3.forward, Vector3.forward)
        };
    }

    public void AddControlPoint () {
        points.Add(new ControlPoint(points[points.Count - 1].GetAnchorPosition() + points[points.Count - 1].GetRelativeHandlePosition(1).normalized, .5f * points[points.Count - 1].GetRelativeHandlePosition(1).normalized));
    }

    public void InsertControlPoint (int index) {
        points.Insert(index, new ControlPoint(points[index].GetAnchorPosition() + points[index].GetRelativeHandlePosition(0).normalized, .5f * points[index].GetRelativeHandlePosition(1).normalized));
    }

}
