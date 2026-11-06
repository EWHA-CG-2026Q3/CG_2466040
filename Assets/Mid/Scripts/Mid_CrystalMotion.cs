using UnityEngine;

[RequireComponent(typeof(Mid_CrystalMesh))]
public class Mid_CrystalMotion : MonoBehaviour
{
    [SerializeField, Min(0f)] private float floatHeight = 0.12f;
    [SerializeField, Min(0.1f)] private float floatPeriod = 4f;
    [SerializeField] private float rotationSpeed = 25f;
    [SerializeField, Min(0.1f)] private float startDuration = 2f;

    private Mesh crystalMesh;
    private Vector3[] originalVertices;
    private Vector3[] animatedVertices;
    private Vector3 pivot;
    private float elapsed;
    private float angle;

    void LateUpdate()
    {
        // 생성된 메시의 원본 정점 저장
        if (crystalMesh == null)
        {
            crystalMesh = GetComponent<MeshFilter>().sharedMesh;
            if (crystalMesh == null) return;

            originalVertices = crystalMesh.vertices;
            animatedVertices = new Vector3[originalVertices.Length];
            pivot = crystalMesh.bounds.center;
        }

        elapsed += Time.deltaTime;

        // 시작 속도 보간
        float progress = Mathf.Clamp01(elapsed / startDuration);
        float ease = Mathf.SmoothStep(0f, 1f, progress);
        angle = (angle + rotationSpeed * ease * Time.deltaTime) % 360f;

        // 이동 범위: 0~floatHeight
        float lift = floatHeight * 0.5f *
            (1f - Mathf.Cos(elapsed * 2f * Mathf.PI / floatPeriod)) * ease;

        Matrix4x4 T = Matrix4x4.Translate(Vector3.up * lift);
        Matrix4x4 R = Matrix4x4.Rotate(Quaternion.Euler(0f, angle, 0f));
        Matrix4x4 toPivot = Matrix4x4.Translate(pivot);
        Matrix4x4 fromPivot = Matrix4x4.Translate(-pivot);

        // 중심 기준 회전 후 위로 이동
        Matrix4x4 M = T * toPivot * R * fromPivot;
        for (int i = 0; i < originalVertices.Length; i++)
            animatedVertices[i] = M.MultiplyPoint3x4(originalVertices[i]);

        crystalMesh.vertices = animatedVertices;
        crystalMesh.RecalculateNormals();
        crystalMesh.RecalculateBounds();
    }

    void OnDisable()
    {
        if (crystalMesh != null && originalVertices != null)
        {
            crystalMesh.vertices = originalVertices;
            crystalMesh.RecalculateNormals();
            crystalMesh.RecalculateBounds();
        }
        elapsed = 0f;
        angle = 0f;
    }
}
