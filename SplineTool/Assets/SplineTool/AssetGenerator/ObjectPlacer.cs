using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum offsetType {
    arcDistance,
    globalDistance
}

[Serializable]
public class ObjectPlacer {
    public string name;
    public Transform objectReference;
    public Vector2 position;
    public float distance;
    public float offset;
    public offsetType type;
    public Vector3 rotation;
    public Vector3 scale;
    public bool[] constraints;

    //Constructor
    public ObjectPlacer() {
        name = "Object Placer";
        objectReference = null;
        position = Vector2.zero;
        distance = .1f;
        offset = 0f;
        type = offsetType.arcDistance;
        rotation = Vector3.zero;
        scale = Vector3.one;
        constraints = new bool[] { true, true, true };
    }

    //Copy constructor
    public ObjectPlacer(ObjectPlacer other) {
        name = other.name + " Clone";
        objectReference = other.objectReference;
        position = other.position;
        distance = other.distance;
        offset = other.offset;
        type = other.type;
        rotation = other.rotation;
        scale = other.scale;
        constraints = other.constraints;
    }
}
