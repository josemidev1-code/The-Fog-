using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheFog.EditorTools
{
    public static class FogSceneFinalizer
    {
        [Serializable] class Report { public bool sceneBuilt; public bool occlusionBaked; public long occlusionBytes; public int occludees; public int occluders; }
        public static void Run()
        {
            var report = new Report();
            try
            {
                report.sceneBuilt = FirstPlayableSceneBuilder.Build();
                if (!report.sceneBuilt) throw new Exception("Scene generation failed");
                foreach (var renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>())
                {
                    if (!renderer.transform.root.name.Equals("ENTORNO")) continue;
                    var flags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                    flags |= StaticEditorFlags.OccludeeStatic;
                    report.occludees++;
                    for (var node = renderer.transform; node != null; node = node.parent)
                    {
                        if (node.name == "CasaDaniel" || node.name == "EntradaMina")
                        {
                            flags |= StaticEditorFlags.OccluderStatic;
                            report.occluders++;
                            break;
                        }
                    }
                    GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,flags);
                }
                foreach(var camera in UnityEngine.Object.FindObjectsByType<Camera>()) camera.useOcclusionCulling=true;
                var area = new GameObject("Zona de oclusion del recorrido").AddComponent<OcclusionArea>();
                area.transform.position=new Vector3(0,5,17);
                area.size=new Vector3(90,12,90);
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                report.occlusionBaked=StaticOcclusionCulling.Compute();
                report.occlusionBytes=StaticOcclusionCulling.umbraDataSize;
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                AssetDatabase.SaveAssets();
            }
            catch(Exception e) { Debug.LogException(e); }
            File.WriteAllText("docs/occlusion-validation.json",JsonUtility.ToJson(report,true));
            Debug.Log("THE FOG OCCLUSION: "+(report.occlusionBaked?"PASS":"FAIL"));
            EditorApplication.Exit(report.sceneBuilt && report.occlusionBaked ? 0 : 1);
        }
    }
}
