using UnityEngine;

// 과제: y 높이에 비례해 x축 방향으로 미는 shear를 4x4 행렬로 구현
// 학번 2466040의 끝자리는 0이므로 자기 k값은 (0 + 1) / 5 = 0.2
[ExecuteAlways]
[RequireComponent(typeof(DiamondMesh))]
public class S09_Shear : MonoBehaviour
{
    [Header("x' = x + k y")]
    [SerializeField] float k = 0.2f;

    DiamondMesh diamondMesh;

    void OnEnable()
    {
        diamondMesh = GetComponent<DiamondMesh>();
        ApplyShear();
        LogTopVertex();
    }

    void Update()
    {
        ApplyShear();
    }

    void OnValidate()
    {
        // Inspector에서 k를 0.2와 -0.2로 바꿀 때마다 제출용 결과를 출력한다.
        LogTopVertex();
    }

    void ApplyShear()
    {
        if (diamondMesh == null)
            diamondMesh = GetComponent<DiamondMesh>();
        if (diamondMesh == null || diamondMesh.BaseVertices == null)
            return;

        float[,] shear = ShearMatrixRaw(k);
        Vector3[] baseVertices = diamondMesh.BaseVertices;
        Vector3[] verts = new Vector3[baseVertices.Length];

        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector4 h = ToHomogeneous(baseVertices[i]);
            verts[i] = FromHomogeneous(MultiplyMatrixVectorRaw(shear, h));
        }

        diamondMesh.SetVertices(verts);
    }

    // e1 -> (1, 0, 0), e2 -> (k, 1, 0), e3 -> (0, 0, 1), 원점은 그대로다.
    // 각 결과 벡터를 차례로 1~4열에 넣으면 아래의 shear 행렬이 된다.
    public float[,] ShearMatrixRaw(float shearK)
    {
        return new float[,]
        {
            { 1f, shearK, 0f, 0f },
            { 0f,      1f, 0f, 0f },
            { 0f,      0f, 1f, 0f },
            { 0f,      0f, 0f, 1f }
        };
    }

    Vector4 ToHomogeneous(Vector3 v)
    {
        return new Vector4(v.x, v.y, v.z, 1f);
    }

    Vector3 FromHomogeneous(Vector4 h)
    {
        return new Vector3(h.x, h.y, h.z);
    }

    Vector4 MultiplyMatrixVectorRaw(float[,] matrix, Vector4 v)
    {
        float[] input = { v.x, v.y, v.z, v.w };
        float[] result = new float[4];

        for (int row = 0; row < 4; row++)
            for (int col = 0; col < 4; col++)
                result[row] += matrix[row, col] * input[col];

        return new Vector4(result[0], result[1], result[2], result[3]);
    }

    void LogTopVertex()
    {
        Vector3 top = new Vector3(0.5f, 1f, 0.5f);
        Vector3 result = FromHomogeneous(
            MultiplyMatrixVectorRaw(ShearMatrixRaw(k), ToHomogeneous(top)));

        Debug.Log($"[Shear k={k:F1}] 꼭대기 정점 {top} -> {result}", this);
    }
}
