using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Spline : MonoBehaviour {

    [SerializeField]
    private List<ControlPoint> points;

    public int ControlPointCount {
        get {
            return points.Count;
        }
    }
    
    public void Reset() {
        points = new List<ControlPoint> {
            new ControlPoint(Vector3.forward)
        };
    }

    public ControlPoint GetControlPoint (int index) {
        return points[index];
    }

    public void AddControlPoint () {
        points.Add(new ControlPoint(points[points.Count - 1].GetAnchorPosition() + Vector3.forward));
    }

    public void InsertControlPoint (int index) {
        points.Insert(index, new ControlPoint(points[index].GetAnchorPosition() + Vector3.back));
    }

}
