#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class S12_StillLifeSceneBuilder
{
    const string Root = "Assets/S12/S12_StillLife";
    const string Generated = Root + "/Generated";

    [MenuItem("CG/S12/Build Still Life Scene")]
    public static void Build()
    {
        EnsureFolders();
        Mesh pyramid = SaveMesh(S12_CustomMeshLibrary.CreatePyramid(), Generated + "/CustomPyramid.asset");
        Mesh diamond = SaveMesh(S12_CustomMeshLibrary.CreateDiamond(), Generated + "/CustomDiamond.asset");
        Mesh vase = SaveMesh(S12_CustomMeshLibrary.CreateVase(), Generated + "/CustomVase.asset");

        Material wood = SaveMaterial("Wood", new Color(0.30f, 0.12f, 0.045f), 0.18f);
        Material red = SaveMaterial("Coral", new Color(0.85f, 0.16f, 0.12f), 0.28f);
        Material blue = SaveMaterial("Blue", new Color(0.08f, 0.34f, 0.78f), 0.42f);
        Material gold = SaveMaterial("Gold", new Color(0.95f, 0.58f, 0.08f), 0.55f);
        Material floorMaterial = SaveMaterial("Floor", new Color(0.16f, 0.18f, 0.22f), 0.08f);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var targets = new List<MeshFilter>();

        GameObject set = new GameObject("Still Life Objects");
        CreateTable(set.transform, wood, targets);
        CreateMeshObject("CustomMesh_01_Pyramid", pyramid, red, new Vector3(-1.55f, 1.15f, 0.05f),
            Quaternion.Euler(0f, 24f, 0f), Vector3.one, set.transform, targets);
        CreateMeshObject("CustomMesh_02_Vase", vase, blue, new Vector3(0.15f, 1.15f, 0.2f),
            Quaternion.identity, Vector3.one, set.transform, targets);
        CreateMeshObject("CustomMesh_03_Diamond_Far", diamond, gold, new Vector3(2.1f, 1.85f, 1.05f),
            Quaternion.Euler(0f, 18f, 12f), new Vector3(0.9f, 0.9f, 0.9f), set.transform, targets);

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor (S02-style object)";
        floor.transform.SetPositionAndRotation(new Vector3(0f, -0.12f, 0.6f), Quaternion.identity);
        floor.transform.localScale = new Vector3(10f, 0.2f, 8f);
        floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;
        targets.Add(floor.GetComponent<MeshFilter>());

        GameObject virtualCamera = new GameObject("VirtualCamera");
        virtualCamera.transform.position = new Vector3(-6.7f, 4.2f, -7.6f);
        virtualCamera.transform.rotation = Quaternion.LookRotation(new Vector3(0.25f, 1.15f, 0.35f) - virtualCamera.transform.position);

        GameObject mainCameraObject = new GameObject("Main Camera");
        mainCameraObject.tag = "MainCamera";
        mainCameraObject.transform.SetParent(virtualCamera.transform, false);
        mainCameraObject.transform.localPosition = Vector3.zero;
        mainCameraObject.transform.localRotation = Quaternion.identity;
        Camera mainCamera = mainCameraObject.AddComponent<Camera>();
        mainCamera.fieldOfView = 60f;
        mainCamera.nearClipPlane = 0.3f;
        mainCamera.farClipPlane = 20f;
        mainCamera.clearFlags = CameraClearFlags.Skybox;
        mainCameraObject.AddComponent<AudioListener>();

        RawImage image = CreateCanvas();
        var virtualCameraScript = virtualCamera.AddComponent<S12_VirtualCamera_Finish>();
        virtualCameraScript.targets = targets.ToArray();
        virtualCameraScript.fieldOfView = 60f;
        virtualCameraScript.near = 0.3f;
        virtualCameraScript.far = 20f;
        virtualCameraScript.width = 800;
        virtualCameraScript.height = 600;
        virtualCameraScript.canvasImage = image;
        virtualCameraScript.lineColor = new Color(0.3f, 1f, 0.75f, 1f);
        virtualCameraScript.clearColor = Color.clear;

        GameObject lightObject = new GameObject("Directional Light");
        lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;

        GameObject fillObject = new GameObject("Fill Light");
        fillObject.transform.position = new Vector3(-2f, 4f, -1f);
        Light fill = fillObject.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.range = 10f;
        fill.intensity = 4f;

        EditorSceneManager.SaveScene(scene, Root + "/S12_StillLife.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"S12 still-life scene created with {targets.Count} VirtualCamera targets.");
    }

    public static void BuildFromCommandLine()
    {
        Build();
        EditorApplication.Exit(0);
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/S12")) AssetDatabase.CreateFolder("Assets", "S12");
        if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets/S12", "S12_StillLife");
        if (!AssetDatabase.IsValidFolder(Generated)) AssetDatabase.CreateFolder(Root, "Generated");
    }

    static Mesh SaveMesh(Mesh mesh, string path)
    {
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    static Material SaveMaterial(string name, Color color, float smoothness)
    {
        string path = Generated + "/" + name + ".mat";
        AssetDatabase.DeleteAsset(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var material = new Material(shader) { name = name, color = color };
        material.SetFloat("_Smoothness", smoothness);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    static void CreateTable(Transform parent, Material material, List<MeshFilter> targets)
    {
        GameObject table = new GameObject("Table (from S02 composition)");
        table.transform.SetParent(parent);
        CreateCubePart("TableTop", new Vector3(0f, 0.95f, 0f), new Vector3(4.8f, 0.28f, 2.5f), table.transform, material, targets);
        Vector3[] legPositions =
        {
            new Vector3(-2.05f, 0.42f, -0.9f), new Vector3(2.05f, 0.42f, -0.9f),
            new Vector3(-2.05f, 0.42f, 0.9f), new Vector3(2.05f, 0.42f, 0.9f)
        };
        for (int i = 0; i < legPositions.Length; i++)
            CreateCubePart("TableLeg_" + (i + 1), legPositions[i], new Vector3(0.26f, 0.95f, 0.26f), table.transform, material, targets);
    }

    static void CreateCubePart(string name, Vector3 position, Vector3 scale, Transform parent,
        Material material, List<MeshFilter> targets)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        targets.Add(part.GetComponent<MeshFilter>());
    }

    static void CreateMeshObject(string name, Mesh mesh, Material material, Vector3 position,
        Quaternion rotation, Vector3 scale, Transform parent, List<MeshFilter> targets)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.SetPositionAndRotation(position, rotation);
        go.transform.localScale = scale;
        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
        targets.Add(filter);
    }

    static RawImage CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800f, 600f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject imageObject = new GameObject("VirtualCamera Canvas (800x600)", typeof(RectTransform), typeof(RawImage));
        imageObject.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        RawImage image = imageObject.GetComponent<RawImage>();
        image.color = Color.white;
        image.raycastTarget = false;

        return image;
    }
}
#endif
