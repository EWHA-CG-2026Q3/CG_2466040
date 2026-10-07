using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class S10_MoonAssignmentSetup
{
    const string ScenePath = "Assets/S10_HierarchyAndMatrixComposition/S10_BouncingBallAnimation.unity";
    const string OrbitClipPath = "Assets/S10_HierarchyAndMatrixComposition/Moon_Orbit.anim";
    const string OrbitControllerPath = "Assets/S10_HierarchyAndMatrixComposition/Moon_Orbit.controller";

    [MenuItem("Tools/S10/Fix and Validate Moons")]
    public static void FixAndValidateMoons()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        AnimationClip orbitClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(OrbitClipPath);
        if (orbitClip == null)
            throw new InvalidOperationException($"Missing animation clip: {OrbitClipPath}");

        ConfigureTwoSecondYOrbit(orbitClip);

        RuntimeAnimatorController orbitController =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(OrbitControllerPath);
        if (orbitController == null)
            throw new InvalidOperationException($"Missing animator controller: {OrbitControllerPath}");

        ConfigureBallMoon(scene, orbitController);
        ConfigureDiamondMoon(scene, orbitController);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Validate(scene);
        Debug.Log("S10 moon repair complete: both moons use a 0.15 world-unit orbit radius, " +
                  "0.05 world scale, and a 120-frame Y-axis orbit.");
    }

    static void ConfigureTwoSecondYOrbit(AnimationClip clip)
    {
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
            AnimationUtility.SetEditorCurve(clip, binding, null);

        AnimationCurve constant = AnimationCurve.Linear(0f, 0f, 2f, 0f);
        AnimationCurve rotation = AnimationCurve.Linear(0f, 0f, 2f, 360f);
        SetLinear(constant);
        SetLinear(rotation);

        AnimationUtility.SetEditorCurve(clip, TransformBinding("localEulerAnglesRaw.x"), constant);
        AnimationUtility.SetEditorCurve(clip, TransformBinding("localEulerAnglesRaw.y"), rotation);
        AnimationUtility.SetEditorCurve(clip, TransformBinding("localEulerAnglesRaw.z"), constant);

        SerializedObject serializedClip = new SerializedObject(clip);
        SerializedProperty settings = serializedClip.FindProperty("m_AnimationClipSettings");
        settings.FindPropertyRelative("m_LoopTime").boolValue = true;
        serializedClip.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(clip);
    }

    static EditorCurveBinding TransformBinding(string propertyName) => new EditorCurveBinding
    {
        path = string.Empty,
        type = typeof(Transform),
        propertyName = propertyName,
    };

    static void SetLinear(AnimationCurve curve)
    {
        for (int i = 0; i < curve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        }
    }

    static void ConfigureBallMoon(Scene scene, RuntimeAnimatorController controller)
    {
        Transform ballSphere = FindSceneTransform(scene, "Ball_Sphere_My")
                            ?? FindSceneTransform(scene, "Ball_Sphere");
        if (ballSphere == null)
            throw new InvalidOperationException("Could not find Ball_Sphere_My or Ball_Sphere.");

        Transform orbit = RequireTransform(scene, "Moon_Orbit");
        Transform offset = RequireTransform(scene, "Moon_Offset");
        Transform sphere = RequireTransform(scene, "Moon_Sphere");
        Transform mesh = RequireTransform(scene, "Moon_Mesh");

        SetParentAndReset(orbit, ballSphere);
        SetParentAndReset(offset, orbit);
        SetParentAndReset(sphere, offset);
        SetParentAndReset(mesh, sphere);

        // Ball_Sphere is scaled to 0.1. These local values therefore become
        // a 0.15 world radius and a 0.05 world scale.
        offset.localPosition = new Vector3(0f, 0f, 1.5f);
        sphere.localScale = Vector3.one * 0.5f;
        mesh.localScale = Vector3.one;

        Animator orbitAnimator = orbit.GetComponent<Animator>();
        if (!orbitAnimator)
            orbitAnimator = orbit.gameObject.AddComponent<Animator>();
        orbitAnimator.runtimeAnimatorController = controller;

        // The assignment only animates Moon_Orbit. An Animator on Moon_Sphere
        // adds a second, unintended translation and makes the orbit wobble.
        Animator sphereAnimator = sphere.GetComponent<Animator>();
        if (sphereAnimator != null)
            UnityEngine.Object.DestroyImmediate(sphereAnimator);
    }

    static void ConfigureDiamondMoon(Scene scene, RuntimeAnimatorController controller)
    {
        Transform orbit = GetOrCreateRoot(scene, "Diamond_Moon_Orbit");
        Transform offset = GetOrCreateRoot(scene, "Diamond_Moon_Offset");
        Transform sphere = GetOrCreateRoot(scene, "Diamond_Moon_Sphere");
        Transform mesh = GetOrCreateRoot(scene, "Diamond_Moon_Mesh");

        ResetRoot(orbit);
        ResetRoot(offset);
        ResetRoot(sphere);
        ResetRoot(mesh);

        offset.localPosition = new Vector3(0f, 0f, 1.5f);
        sphere.localScale = Vector3.one * 0.5f;

        Animator orbitAnimator = orbit.GetComponent<Animator>();
        if (!orbitAnimator)
            orbitAnimator = orbit.gameObject.AddComponent<Animator>();
        orbitAnimator.runtimeAnimatorController = controller;

        RemoveAnimatorIfPresent(offset);
        RemoveAnimatorIfPresent(sphere);
        RemoveAnimatorIfPresent(mesh);

        DiamondMesh_Rewinded diamond = mesh.GetComponent<DiamondMesh_Rewinded>()
                                    ?? mesh.gameObject.AddComponent<DiamondMesh_Rewinded>();
        S10_DiamondChain_Finish chain = mesh.GetComponent<S10_DiamondChain_Finish>()
                                     ?? mesh.gameObject.AddComponent<S10_DiamondChain_Finish>();

        Transform[] nodes =
        {
            RequireTransform(scene, "DiamondStage"),
            RequireTransform(scene, "Diamond_Orbit"),
            RequireTransform(scene, "Diamond_Offset"),
            RequireTransform(scene, "Diamond_Ball"),
            orbit,
            offset,
            sphere,
        };

        SerializedObject serializedChain = new SerializedObject(chain);
        SerializedProperty array = serializedChain.FindProperty("chain");
        array.arraySize = nodes.Length;
        for (int i = 0; i < nodes.Length; i++)
            array.GetArrayElementAtIndex(i).objectReferenceValue = nodes[i];
        serializedChain.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(diamond);
        EditorUtility.SetDirty(chain);
    }

    static void Validate(Scene scene)
    {
        Transform ballSphere = FindSceneTransform(scene, "Ball_Sphere_My")
                            ?? FindSceneTransform(scene, "Ball_Sphere");
        Transform ballOrbit = RequireTransform(scene, "Moon_Orbit");
        Transform ballOffset = RequireTransform(scene, "Moon_Offset");
        Transform ballMesh = RequireTransform(scene, "Moon_Mesh");

        if (ballOrbit.parent != ballSphere || ballOffset.parent != ballOrbit)
            throw new InvalidOperationException("Ball moon hierarchy is incorrect.");

        float ballRadius = Vector3.Distance(ballSphere.position, ballMesh.position);
        float ballScale = ballMesh.lossyScale.x;
        if (!Approximately(ballRadius, 0.15f) || !Approximately(ballScale, 0.05f))
            throw new InvalidOperationException($"Ball moon is radius {ballRadius}, scale {ballScale}.");

        Transform diamondMesh = RequireTransform(scene, "Diamond_Moon_Mesh");
        SerializedObject serializedChain = new SerializedObject(
            diamondMesh.GetComponent<S10_DiamondChain_Finish>());
        if (serializedChain.FindProperty("chain").arraySize != 7)
            throw new InvalidOperationException("Diamond moon chain must contain seven transforms.");
    }

    static bool Approximately(float a, float b) => Mathf.Abs(a - b) < 0.0001f;

    static void SetParentAndReset(Transform child, Transform parent)
    {
        child.SetParent(parent, false);
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
    }

    static void ResetRoot(Transform transform)
    {
        transform.SetParent(null, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    static void RemoveAnimatorIfPresent(Transform transform)
    {
        Animator animator = transform.GetComponent<Animator>();
        if (animator != null)
            UnityEngine.Object.DestroyImmediate(animator);
    }

    static Transform GetOrCreateRoot(Scene scene, string objectName)
    {
        Transform existing = FindSceneTransform(scene, objectName);
        if (existing != null)
            return existing;

        GameObject created = new GameObject(objectName);
        SceneManager.MoveGameObjectToScene(created, scene);
        return created.transform;
    }

    static Transform RequireTransform(Scene scene, string objectName) =>
        FindSceneTransform(scene, objectName)
        ?? throw new InvalidOperationException($"Missing scene object: {objectName}");

    static Transform FindSceneTransform(Scene scene, string objectName) =>
        scene.GetRootGameObjects()
             .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
             .FirstOrDefault(transform => transform.name == objectName);
}
