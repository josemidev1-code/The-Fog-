using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TheFog.FirstPlayable;
namespace TheFog.EditorTools
{
 public static class PlayableStageBuilder
 {
  public const string ScenePath="Assets/Scenes/PlayableStage.unity";
  [MenuItem("THE FOG/Staging/Create playable exterior")]
  public static void Run()
  {
   if(File.Exists(ScenePath))throw new System.Exception("Existing staging scene preserved; edit it directly");
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/FirstPlayable.unity");
   foreach(var intro in Object.FindObjectsByType<FogIntroduction>(FindObjectsSortMode.None))intro.playOnStart=false;
   var house=GameObject.Find("CasaDaniel");if(!house)throw new System.Exception("Verified house instance not found");
   // Fit collision to the real exterior instead of a whole-building bounding box.
   foreach(var c in house.GetComponents<Collider>())Object.DestroyImmediate(c);
   foreach(var mesh in house.GetComponentsInChildren<MeshFilter>())
   {if(!mesh.sharedMesh)continue;var c=mesh.gameObject.AddComponent<MeshCollider>();c.sharedMesh=mesh.sharedMesh;mesh.gameObject.isStatic=true;}
   var interaction=house.GetComponent<FogInteractable>()??house.AddComponent<FogInteractable>();interaction.prompt="Examinar la casa de Daniel";interaction.message="Casa de Daniel. El interior conectado está pendiente en este staging.";
   var staging=new GameObject("STAGING · exterior de la casa y recorrido");
   var src=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Optimized/ENV_CasaRural01.glb");
   var cottage=(GameObject)PrefabUtility.InstantiatePrefab(src);cottage.transform.SetParent(staging.transform,false);var rs=cottage.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
   float scale=5.5f/b.size.y;cottage.transform.localScale*=scale;cottage.transform.position=new Vector3(18,0,30)-new Vector3(b.center.x,b.min.y,b.center.z)*scale;
   foreach(var f in cottage.GetComponentsInChildren<MeshFilter>()){var c=f.gameObject.AddComponent<MeshCollider>();c.sharedMesh=f.sharedMesh;f.gameObject.isStatic=true;}
   // Existing ground is explicitly temporary until the requested Hunyuan tile has been reconstructed and validated.
   var floor=GameObject.Find("Suelo");if(floor)floor.name="Suelo PROVISIONAL · pendiente de GLB Hunyuan";
   RenderSettings.ambientLight=new Color(.3f,.33f,.3f);
   var sun=RenderSettings.sun;if(sun)sun.intensity=.7f;
   RenderSettings.fogDensity=.016f;
   EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
   // Capture at human height, without saving this camera change into the scene.
   var camera=Object.FindFirstObjectByType<FogPlayerController>().GetComponentInChildren<Camera>();camera.transform.position=new Vector3(-4,1.65f,-5);camera.transform.LookAt(house.transform.position+Vector3.up*2.3f);DanielRoomBuilder.Capture(camera,"docs/playable-stage-house.png");
   File.WriteAllText("docs/playable-stage-build.json","{\"saved\":true,\"scene\":\""+ScenePath+"\",\"houseExteriorMeshColliders\":true,\"connectedInterior\":false,\"groundModelGenerated\":false,\"browserBlock\":\"Local browser use disabled in Work settings\"}");
  }
 }
}
