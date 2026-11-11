using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheFog
{
    public static class CasaRenderSetup
    {
        const string Folder = "Assets/TheFog/Rendering";
        public const string ScenePath = "Assets/TheFog/Scenes/CasaDaniel.unity";

        [MenuItem("The Fog/Preparar casa con URP")]
        public static void Prepare()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/TheFog", "Rendering");
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(Folder + "/CasaRenderer.asset");
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, Folder + "/CasaRenderer.asset");
                var retro = ScriptableObject.CreateInstance<DanielRetroFeature>();
                retro.name = "Daniel Retro URP";
                retro.passMaterial = new Material(Shader.Find("Hidden/TheFog/DanielRetro"));
                AssetDatabase.CreateAsset(retro.passMaterial, Folder + "/DanielRetro.mat");
                AssetDatabase.AddObjectToAsset(retro, renderer);
                renderer.rendererFeatures.Add(retro);
                renderer.SetDirty();
                EditorUtility.SetDirty(renderer);
            }
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Folder + "/CasaURP.asset");
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.msaaSampleCount = 1;
                pipeline.supportsHDR = false;
                pipeline.shadowDistance = 45f;
                AssetDatabase.CreateAsset(pipeline, Folder + "/CasaURP.asset");
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            int originalQuality = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(originalQuality, false);
            AssetDatabase.SaveAssets();

            foreach (string file in Directory.GetFiles("Assets/TheFog/Resources/Modelos", "*.glb", SearchOption.AllDirectories))
                AssetDatabase.ImportAsset(file.Replace('\\', '/'), ImportAssetOptions.ForceUpdate);

            EditorSceneManager.OpenScene(ScenePath);
            var composer = UnityEngine.Object.FindAnyObjectByType<ValdaraScene>();
            var hierarchy = composer.Compose((asset, parent) => (GameObject)PrefabUtility.InstantiatePrefab(asset, parent));
            if (hierarchy != null) SaveSceneMaterials(hierarchy);
            var camera = Camera.main;
            if (camera != null) camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            EditorSceneManager.MarkSceneDirty(composer.gameObject.scene);
            EditorSceneManager.SaveScene(composer.gameObject.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("THE FOG: CasaDaniel materializada y URP configurado.");
        }

        static void SaveSceneMaterials(Transform root)
        {
            if (!AssetDatabase.IsValidFolder("Assets/TheFog/Materiales")) AssetDatabase.CreateFolder("Assets/TheFog", "Materiales");
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var material = renderer.sharedMaterial;
                if (material == null || EditorUtility.IsPersistent(material)) continue;
                var path = "Assets/TheFog/Materiales/" + material.name + ".mat";
                var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (existing == null) AssetDatabase.CreateAsset(material, path);
                else renderer.sharedMaterial = existing;
            }
        }

        public static void PrepareBatch()
        {
            try { Prepare(); EditorApplication.Exit(0); }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }

        [MenuItem("The Fog/Generar versión para Windows")]
        static void BuildWindows()
        {
            string output = EditorUtility.SaveFilePanel("Generar versión para Windows", "", "TheFog", "exe");
            if (string.IsNullOrEmpty(output))
                return;

            if (BuildWindows(output) == BuildResult.Succeeded)
                EditorUtility.RevealInFinder(output);
        }

        public static void BuildWindowsBatch()
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-theFogBuildPath");
            if (index < 0 || index + 1 >= args.Length)
            {
                Debug.LogError("Falta la ruta del ejecutable en -theFogBuildPath.");
                EditorApplication.Exit(1);
                return;
            }
            EditorApplication.Exit(BuildWindows(args[index + 1]) == BuildResult.Succeeded ? 0 : 1);
        }

        static BuildResult BuildWindows(string output)
            => BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.CompressWithLz4HC
            }).summary.result;

        public static void RestoreOriginalsBatch()
        {
            try
            {
                EditorSceneManager.OpenScene(ScenePath);
                var composer = UnityEngine.Object.FindAnyObjectByType<ValdaraScene>();
                var serialized = new SerializedObject(composer);
                var constructed = serialized.FindProperty("constructed");
                if (constructed.objectReferenceValue is Transform oldStage)
                    UnityEngine.Object.DestroyImmediate(oldStage.gameObject);
                constructed.objectReferenceValue = null;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.SaveScene(composer.gameObject.scene);
                Prepare();
                // Compare source geometry and textures without the pixelation effect.
                var camera = Camera.main;
                if (camera != null && camera.TryGetComponent<DanielRetroCamera>(out var retro)) retro.enabled = false;
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                EditorApplication.Exit(0);
            }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }

        [MenuItem("The Fog/Abrir casa, torre y bosque")]
        public static void OpenForWork()
        {
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.delayCall += () =>
            {
                var view = EditorWindow.GetWindow<SceneView>();
                view.in2DMode = false;
                view.sceneLighting = true;
                view.sceneViewState.showFog = false;
                SceneVisibilityManager.instance.ShowAll();
                view.LookAt(new Vector3(2, 3, 6), Quaternion.Euler(24, 25, 0), 25);
                view.Repaint();
                view.Focus();
            };
        }
    }
}
