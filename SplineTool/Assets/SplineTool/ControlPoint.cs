using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BezierControlPointMode {
    Free,
    Aligned,
    Mirrored
}

[Serializable]
public class ControlPoint {

    public Vector3 anchor;

    [SerializeField]
    private Vector3[] handles;

    //Simple constructor
    public ControlPoint() {
        anchor = Vector3.zero;
        handles = new Vector3[] {};
    }

    //Constructor with position
    public ControlPoint(Vector3 position) {
        anchor = position;
        handles = new Vector3[] { };
    }

    //Move entire controlpoint
    public void MoveControlPoint (Vector3 position) {
        Vector3 translation = position - anchor;
        anchor = position;
        for (int i = 0; i < handles.Length; i++) {
            handles[i] += translation;
        }
    }

    //Move a single handle
    public void MoveHandle (int index, Vector3 position) {
        handles[index] = position;
    }

    //Add a handle
    public void AddHandle () {
        AddHandle(Vector3.forward);
    }

    //Add a handle at a relative position
    public void AddHandle(Vector3 relativePosition) {
        Array.Resize(ref handles, handles.Length + 1);
        handles[handles.Length - 1] = anchor + relativePosition;
    }
}
