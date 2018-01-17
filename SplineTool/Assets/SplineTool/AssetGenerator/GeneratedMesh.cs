using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GeneratedMesh {
    public string name;
    [Range(0.05f, 2f)]
    public float length = .1f;
    [Range(3, 12)]
    public int sides = 3;
    public bool smoothEdges = false;
    [Range(0f, 360f)]
    public float rotation = 0;
    public Vector2 scale = Vector2.one;
    public Vector2 offset = Vector2.zero;
    public bool cap = true;
    public Material material;

    public Mesh generate (Spline spline) {
        Mesh mesh = new Mesh();
        mesh.name = string.Concat(spline.name, "_Mesh");

        int meshLength = Mathf.FloorToInt(spline.GetArcLength() / length) + 1;
        int vertexRows = smoothEdges ? sides : sides * 2;
        int capVertices = cap ? sides * 2 : 0;
        int capTriangles = cap ? (sides - 2) * 12 : 0;

        Vector3[] vertices = new Vector3[meshLength * vertexRows + capVertices];
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

        if (cap) {
            for (int i = 0; i < sides; i++) {
                vertices[meshLength * vertexRows + i] = vertices[i * meshLength];
                vertices[meshLength * vertexRows + sides + i] = vertices[(i + 1) * meshLength - 1];
            }
        }

        int[] triangles = new int[(meshLength - 1) * sides * 6 + capTriangles];
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

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}
