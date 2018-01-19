using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum BezierControlPointMode {
    Aligned,    //Handles are allowed to have different magnitudes
    Mirrored    //Handles both have the same magnitude
}

[Serializable]
public class ControlPoint {
    [SerializeField]
    private Vector3 anchor;
    [SerializeField]
    private Vector3[] handles;          //handles[0] is the handle before the anchor, handles[1] is the handle after the anchor
    [SerializeField]
    private Vector3 up;                 //Used to store rotations along the splines axis
    [SerializeField]
    private BezierControlPointMode mode;
    public int connectedIndex;          //Used when ControlPoint is part of a junction. Default value = -1

    //Constructor
    public ControlPoint() {
        anchor = Vector3.zero;
        handles = new Vector3[2] {
            -.5f * Vector3.forward,
            .5f * Vector3.forward
        };
        up = Vector3.up;
        mode = BezierControlPointMode.Mirrored;
        connectedIndex = -1;
    }

    //Constructor with position and direction
    public ControlPoint(Vector3 position, Vector3 forward) {
        anchor = position;
        handles = new Vector3[2] {
            -.5f * forward,
            .5f * forward
        };
        up = Vector3.up;
        mode = BezierControlPointMode.Mirrored;
        connectedIndex = -1;
    }

    public Vector3 GetAnchorPosition () {
        return anchor;
    }
    
    public Vector3 GetHandlePosition(int index) {
        return anchor + GetRelativeHandlePosition(index);
    }

    public Vector3 GetRelativeHandlePosition(int index) {
        return handles[index];
    }

    public float GetHandleMagnitude (int index) {
        return handles[index].magnitude;
    }

    public Quaternion GetRotation() {
        return Quaternion.LookRotation(handles[1], up);
    }

    //Euler angles are calculated using the Spline.GetEulerAngles method.
    public Vector3 GetEulerAngles() {
        return Spline.GetEulerAngles(up, handles[1]);
    }

    public BezierControlPointMode GetMode() {
        return mode;
    }

    public void SetAnchorPosition(Vector3 position) {
        anchor = position;
    }

    public void SetHandlePosition(int index, Vector3 position) {
        SetRelativeHandlePosition(index, position - anchor);
    }

    //Updates both handle positions based on the new position of a single handle
    public void SetRelativeHandlePosition (int index, Vector3 position) {
        handles[index] = position;
        switch (mode) {
            case BezierControlPointMode.Aligned:
                Vector3 direction = -position;
                handles[1 - index] = direction.normalized * handles[1 - index].magnitude;
                break;
            case BezierControlPointMode.Mirrored:
                handles[1 - index] = -position;
                break;
        }
    }

    //Set the magnitude of a handle (or both handles when type is mirrored)
    public void SetHandleMagnitude (int index, float magnitude) {
        if (magnitude < .01f)
            magnitude = .01f;

        handles[index] = handles[index].normalized * magnitude;
        if (mode == BezierControlPointMode.Mirrored)
            handles[1 - index] = -handles[index];
    }

    public void Scale (Vector3 scale) {
        handles[0].Scale(scale);
        handles[1].Scale(scale);
    }

    public void SetMode (BezierControlPointMode newMode) {
        mode = newMode;
        SetRelativeHandlePosition(1, GetRelativeHandlePosition(1));
    }

    public void SetRotation (Quaternion rotation) {
        handles[0] = rotation * Vector3.back * handles[0].magnitude;
        handles[1] = rotation* Vector3.forward * handles[1].magnitude;
        up = rotation * Vector3.up;
    }
    
}
