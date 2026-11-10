using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class FogAssetValidation
{
    [Serializable] public class Entry { public string path; public bool imported; public int meshes; public int materials; public int textures; public string[] errors; }
    [Serializable] public class Report { public string unityVersion; public int expected = 26; public bool passed; public Entry[] assets; }
    [MenuItem("THE FOG/Validate imported models")]
    public static void Run()
    {
        var paths = Directory.GetFiles("Assets/Models", "*.glb", SearchOption.AllDirectories).OrderBy(p => p).ToArray();
        var entries = paths.Select(path => {
            path = path.Replace('\\', '/');
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var materials = assets.OfType<Material>().ToArray();
            var errors = materials.Where(m => m.shader == null || m.shader.name == "Hidden/InternalErrorShader").Select(m => "Invalid shader: " + m.name).ToArray();
            return new Entry { path = path, imported = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null, meshes = assets.OfType<Mesh>().Count(), materials = materials.Length, textures = assets.OfType<Texture>().Count(), errors = errors };
        }).ToArray();
        var report = new Report { unityVersion = Application.unityVersion, assets = entries, passed = paths.Length == 26 && entries.All(e => e.imported && e.meshes > 0 && e.errors.Length == 0) };
        Directory.CreateDirectory("docs");
        File.WriteAllText("docs/unity-import-validation.json", JsonUtility.ToJson(report, true));
        AssetDatabase.SaveAssets();
        Debug.Log("THE FOG IMPORT: " + (report.passed ? "PASS" : "FAIL"));
        if (Application.isBatchMode) EditorApplication.Exit(report.passed ? 0 : 1);
    }
}
