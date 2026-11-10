using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TheFog.FirstPlayable;
namespace TheFog.EditorTools
{
 public static class GroundVisualChecks
 {
  public static void RemoveTemporaryPathAndCapture()
  {
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/PlayableStage.unity");
   var path=GameObject.Find("Camino");if(path)UnityEngine.Object.DestroyImmediate(path);
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Run();
  }
  public static void Run()
  {
   EditorSceneManager.OpenScene("Assets/Scenes/PlayableStage.unity");int frames=0;EditorApplication.CallbackFunction tick=null;
   tick=()=>{if(++frames<90)return;EditorApplication.update-=tick;
    var camera=UnityEngine.Object.FindFirstObjectByType<FogPlayerController>().GetComponentInChildren<Camera>();camera.transform.position=new Vector3(-4,1.65f,-5);camera.transform.LookAt(GameObject.Find("CasaDaniel").transform.position+Vector3.up*2.3f);
    var entries=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).SelectMany(r=>r.sharedMaterials.Select(m=>r.name+" | "+AssetDatabase.GetAssetPath(m)+" | "+(m?m.name:"NULL")+" | "+(m&&m.HasProperty("baseColorTexture")?AssetDatabase.GetAssetPath(m.GetTexture("baseColorTexture")):""))).Distinct();File.WriteAllLines("docs/ground-material-audit.txt",entries);
    Shader.WarmupAllShaders();DanielRoomBuilder.Capture(camera,"docs/playable-stage-generated-ground.png");
    EditorApplication.Exit(0);
   };EditorApplication.update+=tick;
  }
 }
}
