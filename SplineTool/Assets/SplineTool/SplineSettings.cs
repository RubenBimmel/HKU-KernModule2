using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "SplineSettings", menuName = "Spline settings", order = 1)]
public class SplineSettings : ScriptableObject {
    public GeneratedMesh[] generated = new GeneratedMesh[1];
    public ObjectPlacer[] objects = new ObjectPlacer[1];
}
