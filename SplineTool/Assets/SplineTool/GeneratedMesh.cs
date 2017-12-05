using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GeneratedMesh {
    [Range(3, 10)]
    public int sides = 3;
    [Range(0.001f, 1f)]
    public float length = .1f;
    [Range(0f, 360f)]
    public float rotation = 0;
    public float scale = .5f;
    public Vector2 offset;
    public Material material;

    public Mesh generate (Spline spline) {
        Mesh mesh = new Mesh();
        mesh.name = string.Concat(spline.name, "_Mesh");

        int meshLength = Mathf.FloorToInt(spline.GetArcLength() / length);

        Vector3[] vertices = new Vector3[(meshLength + 1) * sides];
        for (int i = 0; i < meshLength + 1; i++) {
            Vector3 position = spline.GetPoint(i * length);
            Vector3 forward = spline.GetDirection(i * length);
            Vector3 up = spline.GetUp(i * length);
            for (int j = 0; j < sides; j++) {
                Quaternion rotate = Quaternion.AngleAxis(360 / sides * j + rotation, forward);
                vertices[i * sides + j] = position + rotate * up * scale;
            }
        }

        int[] triangles = new int[meshLength * sides * 6 + (sides - 2) * 6];
        for (int i = 0; i < meshLength; i++) {
            for (int j = 0; j < sides; j++) {
                int offset = (j < sides - 1) ? 1 : 1 - sides;
                triangles[(i * sides + j) * 6] = i * sides + j;
                triangles[(i * sides + j) * 6 + 1] = i * sides + j + offset;
                triangles[(i * sides + j) * 6 + 2] = (i + 1) * sides + j;
                triangles[(i * sides + j) * 6 + 3] = i * sides + j + offset;
                triangles[(i * sides + j) * 6 + 4] = (i + 1) * sides + j + offset;
                triangles[(i * sides + j) * 6 + 5] = (i + 1) * sides + j;
            }
        }

        for (int i = 0; i < sides - 2; i++) {
            int start = (meshLength - 1) * sides * 6;
            triangles[start + i * 3] = 1 + i;
            triangles[start + i * 3 + 1] = 0;
            triangles[start + i * 3 + 2] = 2 + i;
            triangles[start + (sides - 2) * 3 + i * 3] = meshLength* sides;
            triangles[start + (sides - 2) * 3 + i * 3 + 1] = meshLength * sides + 1 + i;
            triangles[start + (sides - 2) * 3 + i * 3 + 2] = meshLength * sides + 2 + i;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}
