using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "SplineSettings", menuName = "Spline settings", order = 1)]
public class SplineSettings : ScriptableObject {
    public List<GeneratedMesh> generated = new List<GeneratedMesh>();
    public List<ObjectPlacer> placers = new List<ObjectPlacer>();

    public int assetCount {
        get {
            return generated.Count + placers.Count;
        }
    }

    public void AddGeneratedMesh() {
        GeneratedMesh newMesh = new GeneratedMesh();
        newMesh.name = string.Concat("Generated mesh ", generated.Count + 1);
        generated.Add(newMesh);
    }

    public void CloneGeneratedMesh(int index) {
        GeneratedMesh newMesh = new GeneratedMesh();
        newMesh.name = generated[index].name + " Copy";
        newMesh.length = generated[index].length;
        newMesh.sides = generated[index].sides;
        newMesh.smoothEdges = generated[index].smoothEdges;
        newMesh.rotation = generated[index].rotation;
        newMesh.scale = generated[index].scale;
        newMesh.offset = generated[index].offset;
        newMesh.cap = generated[index].cap;
        newMesh.material = generated[index].material;
        generated.Insert(index + 1, newMesh);
    }

    public void AddObjectPlacer() {
        ObjectPlacer newPlacer = new ObjectPlacer();
        newPlacer.name = string.Concat("Object placer ", placers.Count + 1);
        placers.Add(newPlacer);
    }

    public void CloneObjectPlacer(int index) {
        ObjectPlacer newPlacer = new ObjectPlacer();
        newPlacer.name = placers[index].name + " Copy";
        newPlacer.objectReference = placers[index].objectReference;
        newPlacer.position = placers[index].position;
        newPlacer.distance = placers[index].distance;
        newPlacer.offset = placers[index].offset;
        newPlacer.type = placers[index].type;
        newPlacer.rotation = placers[index].rotation;
        newPlacer.scale = placers[index].scale;
        newPlacer.constraints = placers[index].constraints;
        placers.Insert(index + 1, newPlacer);
}

    public string[] GetAssetNames() {
        string[] names = new string[assetCount];
        for (int i = 0; i < generated.Count; i++) {
            names[i] = generated[i].name;
        }
        for (int i = 0; i < placers.Count; i++) {
            names[i + generated.Count] = placers[i].name;
        }
        return names;
    }

    public string[] GetGeneratedNames() {
        string[] names = new string[assetCount];
        for (int i = 0; i < generated.Count; i++) {
            names[i] = generated[i].name;
        }
        return names;
    }

    public string[] GetPlacerNames() {
        string[] names = new string[assetCount];
        for (int i = 0; i < placers.Count; i++) {
            names[i + generated.Count] = placers[i].name;
        }
        return names;
    }
}
