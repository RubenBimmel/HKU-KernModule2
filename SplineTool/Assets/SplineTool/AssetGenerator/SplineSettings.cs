using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "SplineSettings", menuName = "Spline settings", order = 1)]
public class SplineSettings : ScriptableObject {
    public GeneratedMesh[] generated = new GeneratedMesh[1];
    public ObjectPlacer[] objects = new ObjectPlacer[1];

    public string getName (int index) {
        if (index < generated.Length) {
            return generated[index].name;
        }
        if (index < objects.Length + generated.Length) {
            return objects[index - generated.Length].name;
        }
        return "";
    }
}
