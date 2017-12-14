using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum offsetType {
    arcDistance,
    globalDistance
}

[Serializable]
public class ObjectPlacer : SplineAsset {
    public Transform objectReference;
    public Vector2 position = Vector2.zero;
    [Range(0.05f, 4f)]
    public float distance = .1f;
    public float offset = 0f;
    public offsetType type = offsetType.arcDistance;
    public Vector3 rotation = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public bool[] constraints = { true, true, true };
}
