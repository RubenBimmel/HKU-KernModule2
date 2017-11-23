using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum BezierControlPointMode {
    Free,
    Aligned,
    Mirrored
}

[Serializable]
public class ControlPoint {
    [SerializeField]
    private Vector3 anchor;
    [SerializeField]
    private Vector3[] handles;
    public BezierControlPointMode mode;
    public List<ControlPoint> connectedPoints;

    //Simple constructor
    public ControlPoint() {
        anchor = Vector3.zero;
        handles = new Vector3[2] {
            .5f * Vector3.back,
            .5f * Vector3.forward
        };
        mode = BezierControlPointMode.Mirrored;
        connectedPoints = new List<ControlPoint> { this};
    }

    //Constructor with position
    public ControlPoint(Vector3 position) {
        anchor = position;
        handles = new Vector3[2] {
            .5f * Vector3.back,
            .5f * Vector3.forward
        };
        mode = BezierControlPointMode.Mirrored;
        connectedPoints = new List<ControlPoint> { this };
    }
    
    //Get position
    public Vector3 GetAnchorPosition () {
        return anchor;
    }
    
    //Get handle position
    public Vector3 GetHandlePosition(int index) {
        return anchor + handles[index];
    }

    //Set position
    public void setAnchorPosition(Vector3 position) {
        for (int i = 0; i < connectedPoints.Count; i++) {
            connectedPoints[i].anchor = position;
        }
    }

    //Set handleposition
    public void setHandlePosition(int index, Vector3 position) {
        handles[index] = position - anchor;
        switch (mode) {
            case BezierControlPointMode.Free:
                break;
            case BezierControlPointMode.Aligned:
                Vector3 direction = anchor - position;
                handles[1 - index] = direction.normalized * handles[1 - index].magnitude;
                break;
            case BezierControlPointMode.Mirrored:
                handles[1 - index] = anchor - position;
                break;
        }
    }

    //Connect to other ControlPoint
    public void Connect (ControlPoint other) {
        other.connectedPoints.AddRange(connectedPoints);
        other.connectedPoints = other.connectedPoints.Distinct().ToList();
        for (int i = 0; i < other.connectedPoints.Count; i++) {
            other.connectedPoints[i].connectedPoints = other.connectedPoints;
        }
    }

    //Check index in connected anchors
    public int GetIndex () {
        for (int i = 0; i < connectedPoints.Count; i++) {
            if (connectedPoints[i] == this) {
                return i;
            }
        }
        return 0;
    }
}
