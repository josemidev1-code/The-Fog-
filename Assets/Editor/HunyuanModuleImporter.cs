using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace TheFog.EditorTools
{
    public static class HunyuanModuleImporter
    {
        [Serializable] class Report { public bool passed; public string prefab; public int meshes, triangles, materials; public Vector3 size; public string[] shaders; public bool currentSceneUnchanged; }
        [MenuItem("THE FOG/Modules/Prepare generated corner")]
        public static void Prepare()
        {
            var report=new Report();var current=EditorSceneManager.GetActiveScene();bool wasDirty=current.isDirty;
            var preview=EditorSceneManager.NewPreviewScene();
            try
            {
                const string path="Assets/Models/Optimized/INT_Valdara_Esquina_A.glb";
                var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset==null)throw new Exception("Generated GLB not imported");
                var root=(GameObject)PrefabUtility.InstantiatePrefab(asset,preview);root.name="INT_Valdara_Esquina_A";
                var renderers=root.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                report.size=bounds.size;
                foreach(var mesh in root.GetComponentsInChildren<MeshFilter>())
                {
                    if(mesh.sharedMesh==null)continue;report.meshes++;report.triangles+=mesh.sharedMesh.triangles.Length/3;
                    var collider=mesh.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=mesh.sharedMesh;mesh.gameObject.isStatic=true;
                }
                var materials=renderers.SelectMany(r=>r.sharedMaterials).Distinct().ToArray();report.materials=materials.Length;
                report.shaders=materials.Select(m=>m==null?"MISSING":m.shader.name).ToArray();
                report.passed=report.meshes>0 && materials.All(m=>m!=null && m.shader!=null && !m.shader.name.Contains("Error")) && Mathf.Abs(bounds.size.y-2.7f)<.03f;
                if(!report.passed)throw new Exception("Generated corner validation failed");
                Directory.CreateDirectory("Assets/Prefabs/Modules");report.prefab="Assets/Prefabs/Modules/INT_Valdara_Esquina_A.prefab";
                PrefabUtility.SaveAsPrefabAsset(root,report.prefab);AssetDatabase.SaveAssets();
            }
            catch(Exception e){report.passed=false;Debug.LogException(e);}
            finally{EditorSceneManager.ClosePreviewScene(preview);}
            report.currentSceneUnchanged=current.isDirty==wasDirty;
            File.WriteAllText("docs/hunyuan-unity-validation.json",JsonUtility.ToJson(report,true));
            Debug.Log("HUNYUAN MODULE "+(report.passed?"PASS":"FAIL"));
        }
    }
}
