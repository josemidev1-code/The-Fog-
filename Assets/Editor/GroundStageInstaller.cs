using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TheFog.FirstPlayable;
namespace TheFog.EditorTools
{
 public static class GroundStageInstaller
 {
  [Serializable] class Report {public bool saved;public int tiles,trianglesPerTile;public string source,prefab;public Vector3 size;public bool originalPreserved=true;public bool playModeVerified;}
  public static void Run()
  {
   if(!File.Exists("docs/ground-validation.json"))throw new Exception("Ground source validation missing");
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/PlayableStage.unity");
   if(GameObject.Find("SUELO · Hunyuan validado"))throw new Exception("Ground already installed; preserving scene edits");
   const string source="Assets/Models/Optimized/ENV_SueloRural_A.glb";
   var asset=AssetDatabase.LoadAssetAtPath<GameObject>(source);if(!asset)throw new Exception("Hunyuan ground not imported");
   var prototype=(GameObject)PrefabUtility.InstantiatePrefab(asset);var rs=prototype.GetComponentsInChildren<Renderer>();if(rs.Length==0)throw new Exception("Ground missing renderers");
   var b=rs[0].bounds;foreach(var r in rs){b.Encapsulate(r.bounds);if(r.sharedMaterials.Any(m=>!m||!m.shader||m.shader.name.Contains("Error")))throw new Exception("Ground material error");}
   if(Mathf.Abs(b.size.x-4)>.001f||Mathf.Abs(b.size.z-4)>.001f)throw new Exception("Ground not prepared as 4m module");
   int tris=0;foreach(var f in prototype.GetComponentsInChildren<MeshFilter>()){tris+=f.sharedMesh.triangles.Length/3;var c=f.gameObject.AddComponent<MeshCollider>();c.sharedMesh=f.sharedMesh;f.gameObject.isStatic=true;}
   const string prefab="Assets/Prefabs/Modules/ENV_SueloRural_A.prefab";PrefabUtility.SaveAsPrefabAsset(prototype,prefab);UnityEngine.Object.DestroyImmediate(prototype);
   var group=new GameObject("SUELO · Hunyuan validado").transform;var prefabAsset=AssetDatabase.LoadAssetAtPath<GameObject>(prefab);int tiles=0;
   for(int z=-31;z<=65;z+=4)for(int x=-48;x<=48;x+=4){var tile=(GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);tile.transform.SetParent(group,false);tile.transform.localPosition=new Vector3(x,0,z);tile.name="Suelo_"+x+"_"+z;tiles++;}
   // The real generated mesh supplies visual and physical ground; no hidden collision plane masks holes.
   var old=GameObject.Find("Suelo PROVISIONAL · pendiente de GLB Hunyuan");if(!old)throw new Exception("Expected temporary floor missing; inspect scene");UnityEngine.Object.DestroyImmediate(old);
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
   File.WriteAllText("docs/ground-unity-install.json",JsonUtility.ToJson(new Report{saved=true,tiles=tiles,trianglesPerTile=tris,source=source,prefab=prefab,size=b.size},true));
   var camera=UnityEngine.Object.FindFirstObjectByType<FogPlayerController>().GetComponentInChildren<Camera>();camera.transform.position=new Vector3(-4,1.65f,-5);camera.transform.LookAt(GameObject.Find("CasaDaniel").transform.position+Vector3.up*2.3f);DanielRoomBuilder.Capture(camera,"docs/playable-stage-generated-ground.png");
  }
 }
}
