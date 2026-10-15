using System.Collections.Generic;
using UnityEngine;

// S03-S04에서 연습한 것처럼 정점(vertices)과 삼각형(triangles)을 직접 지정해 메시를 만든다.
public static class S12_CustomMeshLibrary
{
    public static Mesh CreatePyramid()
    {
        Vector3[] vertices =
        {
            new Vector3(-0.7f, 0f, -0.7f), new Vector3(0.7f, 0f, -0.7f),
            new Vector3(0.7f, 0f, 0.7f), new Vector3(-0.7f, 0f, 0.7f),
            new Vector3(0f, 1.7f, 0f)
        };
        int[] triangles =
        {
            0, 2, 1, 0, 3, 2,
            0, 1, 4, 1, 2, 4, 2, 3, 4, 3, 0, 4
        };
        return Finish("S12_CustomPyramid", vertices, triangles);
    }

    public static Mesh CreateDiamond()
    {
        Vector3[] vertices =
        {
            new Vector3(0f, 1.1f, 0f), new Vector3(0f, -1.1f, 0f),
            new Vector3(0.75f, 0f, 0f), new Vector3(0f, 0f, 0.75f),
            new Vector3(-0.75f, 0f, 0f), new Vector3(0f, 0f, -0.75f)
        };
        int[] triangles =
        {
            0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 2,
            1, 3, 2, 1, 4, 3, 1, 5, 4, 1, 2, 5
        };
        return Finish("S12_CustomDiamond", vertices, triangles);
    }

    public static Mesh CreateVase(int sides = 12)
    {
        // 높이별 반지름을 정한 뒤 원 둘레의 정점을 연결한 회전체 메시.
        float[] heights = { 0f, 0.18f, 0.75f, 1.15f, 1.45f };
        float[] radii = { 0.42f, 0.58f, 0.52f, 0.25f, 0.28f };
        var vertices = new List<Vector3>();
        for (int ring = 0; ring < heights.Length; ring++)
        {
            for (int side = 0; side < sides; side++)
            {
                float angle = side * Mathf.PI * 2f / sides;
                vertices.Add(new Vector3(Mathf.Cos(angle) * radii[ring], heights[ring],
                    Mathf.Sin(angle) * radii[ring]));
            }
        }

        var triangles = new List<int>();
        for (int ring = 0; ring < heights.Length - 1; ring++)
        {
            for (int side = 0; side < sides; side++)
            {
                int next = (side + 1) % sides;
                int a = ring * sides + side;
                int b = ring * sides + next;
                int c = (ring + 1) * sides + side;
                int d = (ring + 1) * sides + next;
                triangles.Add(a); triangles.Add(d); triangles.Add(b);
                triangles.Add(a); triangles.Add(c); triangles.Add(d);
            }
        }
        return Finish("S12_CustomVase", vertices.ToArray(), triangles.ToArray());
    }

    static Mesh Finish(string meshName, Vector3[] vertices, int[] triangles)
    {
        var mesh = new Mesh { name = meshName };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
