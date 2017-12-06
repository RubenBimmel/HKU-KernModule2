using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ObjectPlacer {
    public Transform objectReference;
    public Vector2 position = Vector2.zero;
    [Range(0.05f, 2f)]
    public float distance = .1f;
    public Vector3 rotation = Vector3.zero;
    public Vector3 scale = Vector3.one;
}
