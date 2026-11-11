using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheFog
{
    [InitializeOnLoad]
    public static class CasaRenderChecks
    {
        static CasaRenderChecks()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("CasaRenderChecks", false)) return;
                SessionState.SetBool("CasaRenderChecks", false);
                new GameObject("Casa render checks").AddComponent<CasaRenderDriver>();
            };
        }
        public static void Run()
        {
            EditorSceneManager.OpenScene(CasaRenderSetup.ScenePath);
            SessionState.SetBool("CasaRenderChecks", true);
            EditorApplication.EnterPlaymode();
        }
    }

    public class CasaRenderDriver : MonoBehaviour
    {
        [Serializable] class Report
        {
            public string unityVersion;
            public bool passed;
            public int renderers, materials;
            public List<string> checks = new();
            public List<string> failures = new();
        }
        readonly Report report = new();
        void Check(bool result, string text) { (result ? report.checks : report.failures).Add(text); }
        IEnumerator Start()
        {
            Directory.CreateDirectory("docs");
            report.unityVersion = Application.unityVersion;
            Application.logMessageReceived += CaptureError;
            yield return null;
            yield return null;
            Check(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset, "URP is active");
            var roots = gameObject.scene.GetRootGameObjects();
            var transforms = roots.SelectMany(root => root.GetComponentsInChildren<Transform>()).ToArray();
            Check(transforms.Any(t => t.name.Contains("Fachada de Daniel")), "House hierarchy is present");
            Check(transforms.Any(t => t.name == "Suelo del claro"), "Walkable clearing is present");
            Check(!transforms.Any(t => t.name is "Roble" or "Arbusto" or "Tronco" || t.name.Contains("Torre de vigilancia")), "Provisional tower and vegetation are inactive");
            var renderers = roots.SelectMany(root => root.GetComponentsInChildren<Renderer>()).ToArray();
            report.renderers = renderers.Length;
            var materials = renderers.SelectMany(r => r.sharedMaterials).Distinct().ToArray();
            report.materials = materials.Length;
            foreach (var material in materials)
                Check(material != null && material.shader != null && material.shader.isSupported && material.shader.name != "Hidden/InternalErrorShader" && material.shader.name != "Standard",
                    "Compatible material: " + (material != null ? material.name + " / " + material.shader.name : "MISSING"));
            Check(Shader.Find("Hidden/TheFog/DanielRetro").isSupported, "URP retro shader is supported");
            var player = FindAnyObjectByType<ValdaraPlayer>();
            var camera = Camera.main;
            Check(player != null && camera != null, "Player and main camera are present");
            var controller = player.GetComponent<CharacterController>();
            player.enabled = false;
            Physics.SyncTransforms();
            for (int i = 0; i < 60; i++) controller.Move(Vector3.down * .05f);
            Check(controller.isGrounded && player.transform.position.y > -.2f, "Player spawn is grounded");
            var start = player.transform.position;
            for (int i = 0; i < 60; i++) controller.Move(new Vector3(0, -.033f, .06f));
            Check(player.transform.position.z > start.z + 2f, "CharacterController can move along the entry path");
            Capture(camera, "docs/casa-urp-player.png");
            yield return null;
            yield return null;
            // Overview is a visual check, not a claim of completing the tower route.
            camera.transform.SetParent(null, true);
            camera.transform.position = new Vector3(29, 22, -26);
            camera.transform.LookAt(new Vector3(0, 3, 7));
            camera.GetComponent<DanielRetroCamera>().enabled = false;
            camera.farClipPlane = 180;
            RenderSettings.fog = false;
            yield return null;
            Capture(camera, "docs/casa-urp-overview.png");
            camera.transform.position = new Vector3(11, 5, -8);
            camera.transform.LookAt(new Vector3(0, 3.8f, 5.9f));
            Capture(camera, "docs/casa-original-closeup.png");
            yield return null;
            yield return null;
            Application.logMessageReceived -= CaptureError;
            report.passed = report.failures.Count == 0;
            File.WriteAllText("docs/casa-urp-checks.json", JsonUtility.ToJson(report, true));
            Debug.Log("THE FOG CASA URP: " + (report.passed ? "PASS" : "FAIL"));
            EditorApplication.Exit(report.passed ? 0 : 1);
        }
        void CaptureError(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                report.failures.Add(message + "\n" + stack);
        }
        void Capture(Camera camera, string path)
        {
            var target = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                target.Create();
                RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = target });
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Check(File.Exists(path), "Rendered screenshot: " + path);
            }
            catch (Exception error) { report.failures.Add(error.ToString()); }
            finally
            {
                RenderTexture.active = previous;
                target.Release();
                Destroy(target);
                Destroy(texture);
            }
        }
    }
}
