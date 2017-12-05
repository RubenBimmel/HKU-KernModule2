using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Spline {

    public List<ControlPoint> points;
    public string name;
    public SplineSettings settings;

    private float[] arcLengthTable;
    private static int tableSize = 100;

    public Spline(Vector3 position, int index) {
        points = new List<ControlPoint> {
            new ControlPoint(position, Vector3.forward),
            new ControlPoint(position + Vector3.forward, Vector3.forward)
        };
        ResetArcLengthTable();
        name = string.Concat("Spline_", index.ToString("D2"));
    }

    public void AddControlPoint () {
        points.Add(new ControlPoint(points[points.Count - 1].GetAnchorPosition() + points[points.Count - 1].GetRelativeHandlePosition(1).normalized, .5f * points[points.Count - 1].GetRelativeHandlePosition(1).normalized));
        ResetArcLengthTable();
    }

    public void RemoveControlPoint (ControlPoint point) {
        points.Remove(point);
        ResetArcLengthTable();
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
        ResetArcLengthTable();
    }

    private float GetArcPos (float t) {
        for (int i = 0; i < arcLengthTable.Length; i++) {
            //Debug.Log(arcLengthTable[i]);
            if (arcLengthTable[i] > t) {
                return (float)(i - 1) / (float)tableSize;
            }
        }
        return points.Count - 1;
    }


    public Vector3 GetPoint(float t) {
        t = GetArcPos(t);

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
        t = GetArcPos(t);

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
        t = GetArcPos(t);

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

    public float GetArcLength() {
        return arcLengthTable[arcLengthTable.Length - 1];
    }

    public void ResetArcLengthTable () {
        arcLengthTable = new float[(points.Count - 1) * tableSize + 1];
        arcLengthTable[0] = 0f;
        Vector3 lastPos = points[0].GetAnchorPosition();
        for (int i = 0; i < points.Count - 1; i++) {
            for (int j = 0; j < tableSize; j++) {
                if (i + j != 0) {
                    Vector3 nextPos = Bezier.GetPoint(points[i].GetAnchorPosition(), points[i].GetHandlePosition(1), points[i + 1].GetHandlePosition(0), points[i + 1].GetAnchorPosition(), (float)j / (float)tableSize);
                    arcLengthTable[i * tableSize + j] = arcLengthTable[i * tableSize + j - 1] + (nextPos - lastPos).magnitude;
                    lastPos = nextPos;
                }
            }
        }
        arcLengthTable[arcLengthTable.Length - 1] = arcLengthTable[arcLengthTable.Length - 2] + (points[points.Count - 1].GetAnchorPosition() - lastPos).magnitude;
    }
}
