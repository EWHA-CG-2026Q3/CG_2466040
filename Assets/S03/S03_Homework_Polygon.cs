using UnityEngine;

public class S03_Homework_Polygon : MonoBehaviour
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

            new Vector3(2f, 0f, 0f), // 8
            new Vector3(2f, 1f, 0f), // 9
            new Vector3(2f, 2f, 0f), // 10
            new Vector3(1f, 2f, 0f), // 11
            new Vector3(0f, 2f, 0f), // 12

            new Vector3(2f, 0f, 1f), // 13
            new Vector3(2f, 1f, 1f), // 14
            new Vector3(2f, 2f, 1f), // 15
            new Vector3(1f, 2f, 1f), // 16
            new Vector3(0f, 2f, 1f), // 17

            new Vector3(0f, 0f, 2f), // 18
            new Vector3(1f, 0f, 2f), // 19
            new Vector3(2f, 0f, 2f), // 20
            new Vector3(0f, 1f, 2f), // 21
            new Vector3(1f, 1f, 2f), // 22
            new Vector3(2f, 1f, 2f), // 23
            new Vector3(0f, 2f, 2f), // 24
            new Vector3(1f, 2f, 2f), // 25
            new Vector3(2f, 2f, 2f), // 26
            new Vector3(0.5f, 3f, 0f), // 27
        };
        
        int[] triangles = new int[]
        {
            0, 11, 12,
            0, 2, 11,
            0, 9, 2,
            0, 8, 9,
            0, 1, 2,
            12, 11, 27,
           
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
