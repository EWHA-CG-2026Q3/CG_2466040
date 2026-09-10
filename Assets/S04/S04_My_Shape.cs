using UnityEngine;

public class S04_My_Shape : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0f, 0f, 0f), // 0
            new Vector3(1f, 0f, 0f), // 1
            new Vector3(1f, 1f, 0f), // 2
            new Vector3(0f, 1f, 0f), // 3
            new Vector3(0f, 0f, 1f), // 4
            new Vector3(1f, 0f, 1f), // 5
            new Vector3(1f, 1f, 1f), // 6
            new Vector3(0f, 1f, 1f), // 7
            new Vector3(0.5f, 1f, 0.5f) // 8
        };

        int[] triangles = new int[]
        {
            0, 1, 4,
            5, 4, 1,
            0, 1, 8,
            1, 5, 8, 
            5, 4, 8,
            4, 0, 8,
        };
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
