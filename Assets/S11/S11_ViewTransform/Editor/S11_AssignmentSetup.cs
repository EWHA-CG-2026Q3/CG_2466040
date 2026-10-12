using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class S11_AssignmentSetup
{
    const string DemoScenePath = "Assets/S11/S11_ViewTransform/S11_VirtualCamera_Finish.unity";
    const string SourceScenePath = "Assets/S10/S10_HierarchyAndMatrixComposition/S10_BouncingBallAnimation.unity";
    const string DiamondScenePath = "Assets/S11/S11_DiamondView.unity";

    [MenuItem("Tools/S11/Build and Validate Assignment")]
    public static void BuildAndValidateAssignment()
    {
        ConfigureDemoScene();
        ConfigureDiamondScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ValidateAll();
        Debug.Log("S11 assignment setup complete: demo scene and animated diamond view are ready.");
    }

    static void ConfigureDemoScene()
    {
        Scene scene = EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Single);

        Transform virtualCamera = RequireTransform(scene, "VirtualCamera");
        SetTransform(virtualCamera, new Vector3(2f, 0f, 0f), new Vector3(0f, 90f, 0f));

        S11_VirtualCamera_Finish renderer =
            virtualCamera.GetComponent<S11_VirtualCamera_Finish>()
            ?? virtualCamera.gameObject.AddComponent<S11_VirtualCamera_Finish>();
        renderer.size = 2f;
        renderer.near = 0.3f;
        renderer.far = 20f;
        renderer.targets = new[]
        {
            RequireMeshFilter(scene, "Ball"),
            RequireMeshFilter(scene, "Cube"),
            RequireMeshFilter(scene, "Diamond"),
            RequireMeshFilter(scene, "Pyramid"),
        };
        renderer.canvasImage = RequireTransform(scene, "RawImage").GetComponent<RawImage>();

        Camera mainCamera = RequireTransform(scene, "Main Camera").GetComponent<Camera>();
        SetTransform(mainCamera.transform, virtualCamera.position, virtualCamera.eulerAngles);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 2f;
        mainCamera.nearClipPlane = 0.3f;
        mainCamera.farClipPlane = 20f;

        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(mainCamera);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void ConfigureDiamondScene()
    {
        if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(DiamondScenePath))
        {
            if (!AssetDatabase.CopyAsset(SourceScenePath, DiamondScenePath))
                throw new InvalidOperationException("Could not copy the S10 scene to S11_DiamondView.");
            AssetDatabase.ImportAsset(DiamondScenePath, ImportAssetOptions.ForceSynchronousImport);
        }

        Scene scene = EditorSceneManager.OpenScene(DiamondScenePath, OpenSceneMode.Single);

        // The assignment video is about the script-driven diamond device.
        // Hide the hierarchy-driven ball copy so it cannot visually overlap it.
        SetActiveIfPresent(scene, "BallStage_My", false);
        SetActiveIfPresent(scene, "BallStage", false);

        Transform virtualCamera = FindTransform(scene, "VirtualCamera");
        if (virtualCamera == null)
        {
            GameObject created = new GameObject("VirtualCamera");
            SceneManager.MoveGameObjectToScene(created, scene);
            virtualCamera = created.transform;
        }

        Vector3 cameraPosition = new Vector3(0f, 0.6f, -3f);
        Vector3 cameraRotation = Vector3.zero;
        SetTransform(virtualCamera, cameraPosition, cameraRotation);

        RawImage canvasImage = GetOrCreateOverlayCanvas(scene);
        S11_VirtualCamera_Finish renderer =
            virtualCamera.GetComponent<S11_VirtualCamera_Finish>()
            ?? virtualCamera.gameObject.AddComponent<S11_VirtualCamera_Finish>();
        renderer.targets = new[]
        {
            RequireMeshFilter(scene, "Diamond_Mesh"),
            RequireMeshFilter(scene, "Diamond_Moon_Mesh"),
        };
        renderer.size = 1f;
        renderer.near = 0.3f;
        renderer.far = 20f;
        renderer.canvasImage = canvasImage;
        renderer.width = 640;
        renderer.height = 360;
        renderer.backgroundColor = new Color(0f, 0f, 0f, 0f);
        renderer.lineColor = Color.cyan;

        Camera mainCamera = RequireTransform(scene, "Main Camera").GetComponent<Camera>();
        SetTransform(mainCamera.transform, cameraPosition, cameraRotation);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = renderer.size;
        mainCamera.nearClipPlane = renderer.near;
        mainCamera.farClipPlane = renderer.far;

        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(mainCamera);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static RawImage GetOrCreateOverlayCanvas(Scene scene)
    {
        Transform canvasTransform = FindTransform(scene, "S11_ViewCanvas");
        Canvas canvas;
        if (canvasTransform == null)
        {
            GameObject canvasObject = new GameObject(
                "S11_ViewCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            canvasTransform = canvasObject.transform;
        }

        canvas = canvasTransform.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasTransform.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Transform imageTransform = FindChild(canvasTransform, "S11_WireframeOverlay");
        RawImage image;
        if (imageTransform == null)
        {
            GameObject imageObject = new GameObject(
                "S11_WireframeOverlay",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage));
            imageObject.transform.SetParent(canvasTransform, false);
            imageTransform = imageObject.transform;
        }

        image = imageTransform.GetComponent<RawImage>();
        image.raycastTarget = false;
        image.color = Color.white;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // The overlay needs no input, so an EventSystem is intentionally not added.
        return image;
    }

    static void ValidateAll()
    {
        Scene demo = EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Single);
        S11_VirtualCamera_Finish demoRenderer =
            RequireTransform(demo, "VirtualCamera").GetComponent<S11_VirtualCamera_Finish>();
        Camera demoCamera = RequireTransform(demo, "Main Camera").GetComponent<Camera>();

        Require(Approximately(demoRenderer.transform.position, new Vector3(2f, 0f, 0f)),
            "Demo VirtualCamera position is incorrect.");
        Require(Approximately(demoRenderer.transform.eulerAngles, new Vector3(0f, 90f, 0f)),
            "Demo VirtualCamera rotation is incorrect.");
        Require(demoRenderer.targets.Length == 4, "Demo must have four targets.");
        Require(demoCamera.orthographic && Mathf.Approximately(demoCamera.orthographicSize, 2f),
            "Demo Main Camera must be orthographic with size 2.");

        Scene diamond = EditorSceneManager.OpenScene(DiamondScenePath, OpenSceneMode.Single);
        S11_VirtualCamera_Finish diamondRenderer =
            RequireTransform(diamond, "VirtualCamera").GetComponent<S11_VirtualCamera_Finish>();
        Camera diamondCamera = RequireTransform(diamond, "Main Camera").GetComponent<Camera>();

        string[] targetNames = diamondRenderer.targets.Select(target => target.name).ToArray();
        Require(targetNames.SequenceEqual(new[] { "Diamond_Mesh", "Diamond_Moon_Mesh" }),
            "Diamond view targets are incorrect.");
        Require(diamondCamera.orthographic, "Diamond view Main Camera must be orthographic.");
        Require(Approximately(diamondCamera.transform.position, diamondRenderer.transform.position),
            "Diamond view cameras must share a position.");
        Require(Approximately(diamondCamera.transform.eulerAngles, diamondRenderer.transform.eulerAngles),
            "Diamond view cameras must share a rotation.");
        Require(diamondRenderer.canvasImage != null, "Diamond view canvas image is missing.");

        Debug.Log("S11 validation passed: both scenes, cameras, targets, and canvas references are correct.");
    }

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    static bool Approximately(Vector3 a, Vector3 b) =>
        Vector3.SqrMagnitude(a - b) < 0.0001f;

    static void SetTransform(Transform transform, Vector3 position, Vector3 eulerAngles)
    {
        transform.SetParent(null, true);
        transform.position = position;
        transform.rotation = Quaternion.Euler(eulerAngles);
        transform.localScale = Vector3.one;
    }

    static void SetActiveIfPresent(Scene scene, string objectName, bool active)
    {
        Transform transform = FindTransform(scene, objectName);
        if (transform != null) transform.gameObject.SetActive(active);
    }

    static MeshFilter RequireMeshFilter(Scene scene, string objectName)
    {
        MeshFilter filter = RequireTransform(scene, objectName).GetComponent<MeshFilter>();
        if (filter == null)
            throw new InvalidOperationException($"{objectName} has no MeshFilter.");
        return filter;
    }

    static Transform RequireTransform(Scene scene, string objectName) =>
        FindTransform(scene, objectName)
        ?? throw new InvalidOperationException($"Missing scene object: {objectName}");

    static Transform FindTransform(Scene scene, string objectName) =>
        scene.GetRootGameObjects()
             .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
             .FirstOrDefault(transform => transform.name == objectName);

    static Transform FindChild(Transform parent, string objectName) =>
        parent.GetComponentsInChildren<Transform>(true)
              .FirstOrDefault(transform => transform.name == objectName);
}
