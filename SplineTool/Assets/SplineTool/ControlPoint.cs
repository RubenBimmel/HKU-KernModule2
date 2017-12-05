using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum BezierControlPointMode {
    //Free,
    Aligned,
    Mirrored
}

[Serializable]
public class ControlPoint {
    [SerializeField]
    private Vector3 anchor;
    [SerializeField]
    private Vector3[] handles;
    [SerializeField]
    private Vector3 up;
    [SerializeField]
    private BezierControlPointMode mode;
    public int connectedIndex;

    //Constructor with position
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

    //Get position
    public Vector3 GetAnchorPosition () {
        return anchor;
    }
    
    //Get handle position
    public Vector3 GetHandlePosition(int index) {
        return anchor + GetRelativeHandlePosition(index);
    }

    //Get relative handle position
    public Vector3 GetRelativeHandlePosition(int index) {
        return handles[index];
    }

    public float GetHandleMagnitude (int index) {
        return handles[index].magnitude;
    }

    public BezierControlPointMode GetMode() {
        return mode;
    }

    //Set position
    public void SetAnchorPosition(Vector3 position) {
        anchor = position;
    }

    //Set handleposition
    public void SetHandlePosition(int index, Vector3 position) {
        SetRelativeHandlePosition(index, position - anchor);
    }

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

    public Quaternion GetRotation () {
        return Quaternion.LookRotation(handles[1], up);
    }

    public void SetRotation (Quaternion rotation) {
        handles[0] = rotation * Vector3.back * handles[0].magnitude;
        handles[1] = rotation* Vector3.forward * handles[1].magnitude;
        up = rotation * Vector3.up;
    }

    public Vector3 GetEulerAngles () {
        Vector3 euler = new Vector3();
        euler.y = Mathf.Rad2Deg * Mathf.Atan2(handles[1].x, handles[1].z);

        Vector3 xzDirection = handles[1];
        xzDirection.y = 0;
        euler.x = -Mathf.Rad2Deg * Mathf.Atan2(handles[1].y, xzDirection.magnitude);

        Vector3 perpendicular = Vector3.Cross(Vector3.up, xzDirection);
        Vector3 normal = Vector3.Cross(handles[1], perpendicular);
        if (Vector3.Angle(perpendicular, up) < 90)
            euler.z = 360 - Vector3.Angle(normal, up);
        else
            euler.z = Vector3.Angle(normal, up);

        return euler;
    }
}
