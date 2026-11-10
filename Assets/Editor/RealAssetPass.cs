using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheFog.EditorTools
{
    public static class RealAssetPass
    {
        [Serializable] class Entry { public string scene, hierarchy, source; public bool provisional; }
        [Serializable] class Report { public string date; public string deskSource; public bool deskReplaced; public List<Entry> renderers = new(); public List<string> pending = new(); }
        [Serializable] class BoundsReport { public Vector3 center, size; }
        public static void CaptureDesk()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DanielRoom.unity");
            var camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            camera.transform.SetPositionAndRotation(new Vector3(2.9f,2.3f,-2.8f),Quaternion.identity);
            camera.transform.LookAt(new Vector3(-.3f,1.1f,.7f));
            DanielRoomBuilder.Capture(camera,"docs/real-desk-preview.png");
            var desk = GameObject.Find("Mesa de trabajo · modelo propio");
            var rs=desk.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
            File.WriteAllText("docs/real-desk-bounds.json",JsonUtility.ToJson(new BoundsReport { center=b.center, size=b.size },true));
        }
        public static void Run()
        {
            var report = new Report { date = DateTime.UtcNow.ToString("o") };
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/DanielRoom.unity");
            var station = scene.GetRootGameObjects().First(o => o.name == "PUESTO DE TELECOMUNICACIONES").transform;
            const string deskName = "Mesa de trabajo · modelo propio";
            if (station.Find(deskName) == null)
            {
                var oldTop = station.Find("Mesa de trabajo");
                if (oldTop == null) throw new Exception("Expected provisional desk missing; refusing ambiguous migration");
                report.deskSource = Directory.GetFiles("Assets/Models/Optimized", "INT_MesaMadera-*.glb").Single().Replace('\\','/');
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(report.deskSource);
                if (asset == null) throw new Exception("Original team table has not imported");
                var parent = new GameObject(deskName).transform; parent.SetParent(station, false);
                var model = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene); model.transform.SetParent(parent, false);
                var rs = model.GetComponentsInChildren<Renderer>(); var bounds = rs[0].bounds;
                foreach (var r in rs) bounds.Encapsulate(r.bounds);
                // Keep the real model's proportions. Align tabletop to the existing terminal's support plane.
                float scale = .845f / bounds.size.y;
                model.transform.localScale *= scale;
                model.transform.localPosition = -new Vector3(bounds.center.x, bounds.min.y, bounds.center.z) * scale;
                parent.position = new Vector3(0, 0, 2.35f);
                foreach (var filter in model.GetComponentsInChildren<MeshFilter>())
                {
                    if (filter.sharedMesh == null) continue;
                    var c = filter.gameObject.AddComponent<MeshCollider>(); c.sharedMesh = filter.sharedMesh;
                    filter.gameObject.isStatic = true;
                }
                foreach (var t in station.Cast<Transform>().Where(t => t.name == "Mesa de trabajo" || t.name == "Pata").ToArray())
                    UnityEngine.Object.DestroyImmediate(t.gameObject);
                report.deskReplaced = true;
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            foreach (string path in new[]{"Assets/Scenes/DanielRoom.unity", "Assets/Scenes/FirstPlayable.unity"})
            {
                var inspected = EditorSceneManager.OpenScene(path);
                foreach (var r in inspected.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<MeshRenderer>(true)))
                {
                    var f = r.GetComponent<MeshFilter>(); if (f == null || f.sharedMesh == null) continue;
                    string source = AssetDatabase.GetAssetPath(f.sharedMesh);
                    string hierarchy = r.name; for(var p = r.transform.parent; p != null; p = p.parent) hierarchy = p.name + "/" + hierarchy;
                    report.renderers.Add(new Entry { scene=path, hierarchy=hierarchy, source=source,
                        provisional = !source.StartsWith("Assets/Models/", StringComparison.Ordinal) });
                }
            }
            report.pending.AddRange(new[]{"CasaDaniel: exterior and interior are separate; no verified traversable connection", "Door leaf, frame openings, floor and ceiling modules require real generated models and validation", "PC, keyboard, radio, lamp and visible room architecture still provisional; not final assets", "No Discord approval evidence consulted in this pass; new references are proposals"});
            File.WriteAllText("docs/real-asset-scene-audit.json", JsonUtility.ToJson(report, true));
            EditorSceneManager.OpenScene("Assets/Scenes/DanielRoom.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("REAL ASSET PASS: table replaced with team GLB; remaining provisional geometry inventoried");
        }
    }
}
