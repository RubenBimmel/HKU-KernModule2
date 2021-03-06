﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GeneratedMesh {
    public string name;
    public float length;
    public int sides;
    public bool smoothEdges;
    public float rotation;
    public Vector2 scale;
    public Vector2 offset;
    public bool cap;
    public Material material;

    //Constructor
    public GeneratedMesh() {
        name = "Generated Mesh";
        length = .1f;
        sides = 3;
        smoothEdges = false;
        rotation = 0;
        scale = Vector2.one;
        offset = Vector2.zero;
        cap = true;
        material = null;
    }

    //Copy constructor
    public GeneratedMesh(GeneratedMesh other) {
        name = other.name + " Clone";
        length = other.length;
        sides = other.sides;
        smoothEdges = other.smoothEdges;
        rotation = other.rotation;
        scale = other.scale;
        offset = other.offset;
        cap = other.cap;
        material = other.material;
    }

    public Mesh Generate (Spline spline) {
        Mesh mesh = new Mesh();
        mesh.name = string.Concat(spline.name, "_Mesh");

        //Calculate the amount of vertices
        int meshLength = Mathf.FloorToInt(spline.GetArcLength() / length) + 1;
        int vertexRows = smoothEdges ? sides : sides * 2;
        int capVertices = cap ? sides * 2 : 0;
        int capTriangles = cap ? (sides - 2) * 12 : 0;

        //Create vertices and faces
        Vector3[] vertices = new Vector3[meshLength * vertexRows + capVertices];
        int[] triangles = new int[(meshLength - 1) * sides * 6 + capTriangles];

        //Calculate vertex positions
        for (int i = 0; i < meshLength; i++) {
            Vector3 position = spline.GetPoint(i * length);
            Vector3 forward = spline.GetDirection(i * length).normalized;
            Vector3 up = spline.GetUp(i * length).normalized;
            Vector3 right = Vector3.Cross(forward, up).normalized;
            for (int j = 0; j < sides; j++) {
                float angle = Mathf.Deg2Rad * (360 / sides * j + rotation);
                Vector3 vertex = position;
                vertex += (Mathf.Sin(angle) * scale.x + offset.x) * right;
                vertex += (Mathf.Cos(angle) * scale.y + offset.y) * up;
                vertices[i + meshLength * j] = vertex;
                if (!smoothEdges) {
                    vertices[i + meshLength * (sides + j)] = vertex;
                }
            }
        }

        //Calculate the vertex positions for the cap
        if (cap) {
            for (int i = 0; i < sides; i++) {
                vertices[meshLength * vertexRows + i] = vertices[i * meshLength];
                vertices[meshLength * vertexRows + sides + i] = vertices[(i + 1) * meshLength - 1];
            }
        }

        //Generate faces
        for (int i = 0; i < (meshLength -1); i++) {
            for (int j = 0; j < sides; j++) {
                int offset = 1;
                if (j == sides - 1) offset = 1 - sides;
                if (!smoothEdges)   offset += sides;

                triangles[(i * sides + j) * 6]     = j * meshLength + i + 1;
                triangles[(i * sides + j) * 6 + 1] = j * meshLength + i;
                triangles[(i * sides + j) * 6 + 2] = (j + offset) * meshLength + i;
                triangles[(i * sides + j) * 6 + 3] = j * meshLength + i + 1;
                triangles[(i * sides + j) * 6 + 4] = (j + offset) * meshLength + i;
                triangles[(i * sides + j) * 6 + 5] = (j + offset) * meshLength + i + 1;
            }
        }

        //Generate faces for the cap
        if (cap) {
            int start = (meshLength - 1) * sides * 6;
            int vertexStart = meshLength * vertexRows;
            for (int i = 0; i < sides - 2; i++) {
                triangles[start + i * 3] = vertexStart + 1 + i;
                triangles[start + i * 3 + 1] = vertexStart + 0;
                triangles[start + i * 3 + 2] = vertexStart + 2 + i;
                triangles[start + (sides - 2) * 3 + i * 3] = vertexStart + sides;
                triangles[start + (sides - 2) * 3 + i * 3 + 1] = vertexStart + sides + 1 + i;
                triangles[start + (sides - 2) * 3 + i * 3 + 2] = vertexStart + sides + 2 + i;
            }
        }

        //Apply vertices and faces
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}
