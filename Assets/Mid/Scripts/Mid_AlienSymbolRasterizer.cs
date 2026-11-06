using UnityEngine;

// S06_SoftwareRasterizer_Finish.cs를 바탕으로 수정
[RequireComponent(typeof(MeshRenderer))]
public class Mid_AlienSymbolRasterizer : MonoBehaviour
{
    [SerializeField, Range(32, 512)] private int resolution = 256;
    [SerializeField] private Color backgroundColor = new Color(0.025f, 0.015f, 0.06f);

    private Texture2D canvasTexture;
    private Material screenMaterial;
    private Material originalMaterial;

    void Start()
    {
        canvasTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        canvasTexture.name = "Software Rasterized Alien Symbol";
        canvasTexture.filterMode = FilterMode.Point;
        canvasTexture.wrapMode = TextureWrapMode.Clamp;

        FillBackground(backgroundColor);
        DrawSlime();

        canvasTexture.Apply();

        MeshRenderer screen = GetComponent<MeshRenderer>();
        originalMaterial = screen.sharedMaterial;
        screenMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        screenMaterial.name = "Alien Symbol Screen";
        screenMaterial.SetTexture("_BaseMap", canvasTexture);
        screenMaterial.SetColor("_BaseColor", Color.white);
        screenMaterial.SetFloat("_Cull", 0f); // 양면
        screen.sharedMaterial = screenMaterial;
    }

    void DrawSlime()
    {
        // 몸통과 뾰족한 돌기의 테두리
        Vector2[] outline = new Vector2[]
        {
            new Vector2(0.12f, 0.20f), new Vector2(0.20f, 0.34f),
            new Vector2(0.11f, 0.50f), new Vector2(0.29f, 0.47f),
            new Vector2(0.25f, 0.66f), new Vector2(0.37f, 0.60f),
            new Vector2(0.40f, 0.83f), new Vector2(0.49f, 0.69f),
            new Vector2(0.59f, 0.92f), new Vector2(0.64f, 0.65f),
            new Vector2(0.78f, 0.73f), new Vector2(0.74f, 0.51f),
            new Vector2(0.90f, 0.55f), new Vector2(0.81f, 0.34f),
            new Vector2(0.92f, 0.19f), new Vector2(0.77f, 0.12f),
            new Vector2(0.65f, 0.17f), new Vector2(0.53f, 0.10f),
            new Vector2(0.40f, 0.15f), new Vector2(0.27f, 0.10f)
        };
        Vector2 center = new Vector2(0.5f, 0.4f);
        Color centerColor = new Color(0.35f, 1f, 0.55f);

        for (int i = 0; i < outline.Length; i++)
        {
            int next = (i + 1) % outline.Length;
            Color colorA = Color.HSVToRGB(0.12f + outline[i].x * 0.65f, 0.85f, 1f);
            Color colorB = Color.HSVToRGB(0.12f + outline[next].x * 0.65f, 0.85f, 1f);
            DrawTriangle(center * resolution, outline[i] * resolution,
                outline[next] * resolution, centerColor, colorA, colorB);
        }

        // 기울어진 눈
        DrawTriangle(new Vector2(0.27f, 0.53f) * resolution,
            new Vector2(0.44f, 0.46f) * resolution,
            new Vector2(0.41f, 0.36f) * resolution,
            backgroundColor, backgroundColor, backgroundColor);
        DrawTriangle(new Vector2(0.56f, 0.46f) * resolution,
            new Vector2(0.75f, 0.55f) * resolution,
            new Vector2(0.59f, 0.36f) * resolution,
            backgroundColor, backgroundColor, backgroundColor);

        // 입과 송곳니
        DrawTriangle(new Vector2(0.35f, 0.29f) * resolution,
            new Vector2(0.66f, 0.29f) * resolution,
            new Vector2(0.5f, 0.19f) * resolution,
            backgroundColor, backgroundColor, backgroundColor);
        DrawTriangle(new Vector2(0.40f, 0.29f) * resolution,
            new Vector2(0.46f, 0.29f) * resolution,
            new Vector2(0.43f, 0.23f) * resolution,
            Color.white, Color.white, Color.white);
        DrawTriangle(new Vector2(0.55f, 0.29f) * resolution,
            new Vector2(0.61f, 0.29f) * resolution,
            new Vector2(0.58f, 0.23f) * resolution,
            Color.white, Color.white, Color.white);
    }

    // S06의 판정 함수에서 색 보간에 쓸 가중치도 반환
    private bool IsInsideTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c,
        out float w1, out float w2, out float w3)
    {
        float denom = a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y);
        w1 = w2 = w3 = 0f;
        if (Mathf.Abs(denom) < 0.0001f) return false;

        w1 = (p.x * (b.y - c.y) + b.x * (c.y - p.y) + c.x * (p.y - b.y)) / denom;
        w2 = (a.x * (p.y - c.y) + p.x * (c.y - a.y) + c.x * (a.y - p.y)) / denom;
        w3 = 1f - w1 - w2;

        return w1 >= 0f && w2 >= 0f && w3 >= 0f;
    }

    private void FillBackground(Color color)
    {
        for (int x = 0; x < resolution; x++)
            for (int y = 0; y < resolution; y++)
                canvasTexture.SetPixel(x, y, color);
    }

    private void DrawTriangle(Vector2 a, Vector2 b, Vector2 c,
        Color colorA, Color colorB, Color colorC)
    {
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                Vector2 pixelCenter = new Vector2(x + 0.5f, y + 0.5f);
                if (IsInsideTriangle(pixelCenter, a, b, c, out float w1, out float w2, out float w3))
                {
                    Color color = w1 * colorA + w2 * colorB + w3 * colorC;
                    canvasTexture.SetPixel(x, y, color);
                }
            }
        }
    }

    void OnDestroy()
    {
        MeshRenderer screen = GetComponent<MeshRenderer>();
        if (screen != null && screen.sharedMaterial == screenMaterial)
            screen.sharedMaterial = originalMaterial;
        if (screenMaterial != null) Destroy(screenMaterial);
        if (canvasTexture != null) Destroy(canvasTexture);
    }
}
