using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Spline {

    public List<ControlPoint> points;
    private static int lineSteps = 20;

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

    public Vector3 GetPoint(Transform component, float t) {
        int curve = (int)t;
        t = t % 1;
        if (curve == points.Count - 1) {
            curve = points.Count - 2;
            t = 1;
        }
        return component.TransformPoint(Bezier.GetPoint(points[curve].GetAnchorPosition(), points[curve].GetHandlePosition(1), 
            points[curve + 1].GetHandlePosition(0), points[curve + 1].GetAnchorPosition(), t));
    }

    /*public void Draw (Transform component) {
        Vector3 lineStart = GetPoint(component, 0f);
        Gizmos.color = Color.cyan;
        for (int i = 1; i < lineSteps * (points.Count - 1) + 1; i++) {
            Vector3 lineEnd = GetPoint(component, i / (float)lineSteps);
            Gizmos.DrawLine(lineStart, lineEnd);
            lineStart = lineEnd;
        }
    }*/
}
