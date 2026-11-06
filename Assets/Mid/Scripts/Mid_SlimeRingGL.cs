using UnityEngine;

// S06_OpenGL_Finish.cs의 GL 그리기 방식을 사용
public class Mid_SlimeRingGL : MonoBehaviour
{
    [SerializeField] private Material glMaterial;
    [SerializeField, Range(8, 128)] private int segments = 64;
    [SerializeField, Min(0.01f)] private float radius = 0.24f;
    [SerializeField, Min(0f)] private float bumpSize = 0.025f;
    [SerializeField, Range(1, 12)] private int bumpCount = 6;
    [SerializeField, Min(0.001f)] private float ringWidth = 0.012f;
    [SerializeField] private Color ringColor = new Color(0.3f, 1f, 0.45f);

    private Material generatedMaterial;

    void Start()
    {
        if (glMaterial == null)
        {
            generatedMaterial = new Material(Shader.Find("Custom/S06_OpenGL_Shader"));
            glMaterial = generatedMaterial;
        }
    }

    void OnRenderObject()
    {
        if (!Application.isPlaying || glMaterial == null) return;
        if (Camera.current != null &&
            (Camera.current.cullingMask & (1 << gameObject.layer)) == 0) return;
        if (!glMaterial.SetPass(0)) return;

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.TRIANGLES);
        GL.Color(ringColor);

        for (int i = 0; i < segments; i++)
        {
            float angleA = 2f * Mathf.PI * i / segments;
            float angleB = 2f * Mathf.PI * (i + 1) / segments;

            Vector3 innerA = RingPoint(angleA, -ringWidth * 0.5f);
            Vector3 outerA = RingPoint(angleA, ringWidth * 0.5f);
            Vector3 innerB = RingPoint(angleB, -ringWidth * 0.5f);
            Vector3 outerB = RingPoint(angleB, ringWidth * 0.5f);

            // 고리 한 구간을 삼각형 두 개로 연결
            GL.Vertex3(innerA.x, innerA.y, innerA.z);
            GL.Vertex3(outerA.x, outerA.y, outerA.z);
            GL.Vertex3(outerB.x, outerB.y, outerB.z);

            GL.Vertex3(innerA.x, innerA.y, innerA.z);
            GL.Vertex3(outerB.x, outerB.y, outerB.z);
            GL.Vertex3(innerB.x, innerB.y, innerB.z);
        }

        GL.End();
        GL.PopMatrix();
    }

    Vector3 RingPoint(float angle, float widthOffset)
    {
        float r = radius + bumpSize * Mathf.Sin(angle * bumpCount) + widthOffset;
        return new Vector3(r * Mathf.Cos(angle), 0f, r * Mathf.Sin(angle));
    }

    void OnDestroy()
    {
        if (generatedMaterial != null) Destroy(generatedMaterial);
    }
}
