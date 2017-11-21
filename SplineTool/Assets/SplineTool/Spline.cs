using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Spline : MonoBehaviour {

    [SerializeField]
    private ControlPoint[] points;
    
    public void Reset() {
        points = new ControlPoint[] {
            new ControlPoint(Vector3.forward)
        };
    }

    public ControlPoint GetControlPoint (int index) {
        return points[index];
    }

    public void SetControlPoint (int index, Vector3 position) {
        points[index].MoveControlPoint(position);
    }

}
