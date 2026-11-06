using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_CrystalMesh : MonoBehaviour
{
    [Header("크리스털 모양")]
    [SerializeField, Min(3)] private int sides = 6;
    [SerializeField, Min(0.01f)] private float radius = 0.18f;
    [SerializeField, Min(0.01f)] private float bodyHeight = 0.4f;
    [SerializeField, Min(0.01f)] private float topHeight = 0.25f;
    [SerializeField, Min(0.01f)] private float bottomHeight = 0.12f;
    [SerializeField] private Vector2 tipOffset = new Vector2(0.04f, 0.02f);
    [SerializeField] private Color crystalColor = new Color(0.3f, 0.95f, 0.75f);

    private Mesh crystalMesh;
    private Material generatedMaterial;

    void Start()
    {
        CreateCrystal();
    }

    void CreateCrystal()
    {
        int n = Mathf.Max(3, sides);

        Vector3[] points = new Vector3[2 * n + 2];
        for (int i = 0; i < n; i++)
        {
            float angle = (360f / n * i) * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);

            points[i] = new Vector3(x, bottomHeight, z);
            points[n + i] = new Vector3(x, bottomHeight + bodyHeight, z);
        }

        int top = 2 * n;
        int bottom = top + 1;
        points[top] = new Vector3(tipOffset.x,
            bottomHeight + bodyHeight + topHeight, tipOffset.y);
        points[bottom] = Vector3.zero;

        int[] triangles = new int[n * 4 * 3];
        int index = 0;
        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;

            triangles[index++] = i;
            triangles[index++] = n + i;
            triangles[index++] = next;

            triangles[index++] = next;
            triangles[index++] = n + i;
            triangles[index++] = n + next;

            triangles[index++] = n + i;
            triangles[index++] = top;
            triangles[index++] = n + next;

            triangles[index++] = i;
            triangles[index++] = next;
            triangles[index++] = bottom;
        }

        Vector3[] vertices = new Vector3[triangles.Length];
        int[] faceTriangles = new int[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            vertices[i] = points[triangles[i]];
            faceTriangles[i] = i;
        }

        crystalMesh = new Mesh();
        crystalMesh.name = "Procedural Crystal";
        crystalMesh.vertices = vertices;
        crystalMesh.triangles = faceTriangles;
        crystalMesh.RecalculateNormals();
        crystalMesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = crystalMesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer.sharedMaterial == null)
        {
            generatedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            generatedMaterial.color = crystalColor;
            meshRenderer.sharedMaterial = generatedMaterial;
        }
    }

    void OnDestroy()
    {
        if (crystalMesh != null) Destroy(crystalMesh);
        if (generatedMaterial != null) Destroy(generatedMaterial);
    }
}
