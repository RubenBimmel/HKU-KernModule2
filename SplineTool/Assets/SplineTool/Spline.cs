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
        Vector3 newAnchor = new Vector3();
        Vector3 newDirection = new Vector3();
        if (index == 0) {
            newAnchor = points[index].GetAnchorPosition() + points[index].GetRelativeHandlePosition(0).normalized;
            newDirection = 5f * points[index].GetRelativeHandlePosition(1).normalized;
        } else {
            newAnchor = GetPoint(index - 1, .5f);
            newDirection = GetDirection(index - 1, .5f) * points[index].GetRelativeHandlePosition(0).magnitude * .5f;
            points[index - 1].SetMode(BezierControlPointMode.Aligned);
            points[index - 1].SetRelativeHandlePosition(1, points[index - 1].GetRelativeHandlePosition(1) * .5f);
            points[index].SetMode(BezierControlPointMode.Aligned);
            points[index].SetRelativeHandlePosition(0, points[index].GetRelativeHandlePosition(0) * .5f);
        }
        points.Insert(index, new ControlPoint(newAnchor, newDirection));
    }

    public Vector3 GetPoint(float t) {
        int curve = (int)t;
        t = t % 1;
        if (curve == points.Count - 1) {
            curve = points.Count - 2;
            t = 1;
        }
        return GetPoint(curve, t);
    }

    private Vector3 GetPoint(int curve, float t) {
        return Bezier.GetPoint(points[curve].GetAnchorPosition(), points[curve].GetHandlePosition(1),
            points[curve + 1].GetHandlePosition(0), points[curve + 1].GetAnchorPosition(), t);
    }

    public Vector3 GetDirection(float t) {
        int curve = (int)t;
        t = t % 1;
        if (curve == points.Count - 1) {
            curve = points.Count - 2;
            t = 1;
        }
        return GetDirection(curve, t);
    }

    private Vector3 GetDirection(int curve, float t) {
        return Bezier.GetFirstDerivative(points[curve].GetAnchorPosition(), points[curve].GetHandlePosition(1),
            points[curve + 1].GetHandlePosition(0), points[curve + 1].GetAnchorPosition(), t);
    }

    public Vector3 GetUp(float t) {
        int curve = (int)t;
        t = t % 1;
        if (curve == points.Count - 1) {
            curve = points.Count - 2;
            t = 1;
        }
        Vector3 direction = GetDirection(curve, t);
        Quaternion rotation = Quaternion.Lerp( points[curve].GetRotation(), points[curve + 1].GetRotation(), t);
        return Vector3.ProjectOnPlane(rotation * Vector3.up, direction);
    }
}
