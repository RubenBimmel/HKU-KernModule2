﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Spline {

    public List<ControlPoint> points;
    
    public Spline() {
        points = new List<ControlPoint> {
            new ControlPoint(Vector3.forward)
        };
    }

    public void AddControlPoint () {
        points.Add(new ControlPoint(points[points.Count - 1].GetAnchorPosition() + Vector3.forward));
    }

    public void InsertControlPoint (int index) {
        points.Insert(index, new ControlPoint(points[index].GetAnchorPosition() + Vector3.back));
    }

}
