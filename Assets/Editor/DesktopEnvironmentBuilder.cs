using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TheFog.FirstPlayable;
namespace TheFog.EditorTools
{
 public static class DesktopEnvironmentBuilder
 {
  [Serializable] public class Entry {public string id,source,zone;public Vector3 position,size;public float scale;}
  [Serializable] public class Report {public bool saved;public string scene;public List<Entry> additions=new();public string[] missing;public int desktopModels,representedModels;public string limitation="Furniture zones are open review layouts, not finished house interiors. Closed buildings remain exterior-only.";}
  static Report report;
  static GameObject Place(string file,Vector3 position,float height,float yaw,string zone)
  {
   string path="Assets/Models/Optimized/"+file;var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!asset)throw new Exception("Missing optimized desktop model: "+file);
   var root=new GameObject(Path.GetFileNameWithoutExtension(file));var instance=(GameObject)PrefabUtility.InstantiatePrefab(asset);instance.transform.SetParent(root.transform,false);
   var renderers=instance.GetComponentsInChildren<Renderer>();if(renderers.Length==0)throw new Exception("No renderers: "+file);
   if(renderers.SelectMany(r=>r.sharedMaterials).Any(m=>!m||!m.shader||m.shader.name.Contains("Error")))throw new Exception("Invalid material: "+file);
   var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);float scale=height/bounds.size.y;
   instance.transform.localScale*=scale;instance.transform.localPosition=-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)*scale;
   // Detailed static geometry keeps spaces under tables and between furniture legs open.
   foreach(var f in instance.GetComponentsInChildren<MeshFilter>()){var c=f.gameObject.AddComponent<MeshCollider>();c.sharedMesh=f.sharedMesh;f.gameObject.isStatic=true;}
   root.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
   var group=GameObject.Find(zone)??new GameObject(zone);root.transform.SetParent(group.transform,true);
   report.additions.Add(new Entry{id=Path.GetFileNameWithoutExtension(file),source=path,zone=zone,position=position,size=bounds.size*scale,scale=scale});return root;
  }
  public static void Run()
  {
   const string target="Assets/Scenes/DesktopEnvironment.unity";if(File.Exists(target))throw new Exception("Scene exists; refusing to overwrite manual changes");
   report=new Report{scene=target};var scene=EditorSceneManager.OpenScene("Assets/Scenes/FirstPlayable.unity");
   foreach(var intro in UnityEngine.Object.FindObjectsByType<FogIntroduction>(FindObjectsSortMode.None))intro.playOnStart=false;
   // Reserve an open review area beside the existing exterior, preserving the established route.
   foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.parent!=null && (t.name.StartsWith("Roble")||t.name.StartsWith("Arbusto")||t.name.StartsWith("Roca"))).ToArray())
    if(t && t.position.x>10 && t.position.z<30 && t.position.z> -16)UnityEngine.Object.DestroyImmediate(t.gameObject);
   string[] names=Directory.GetFiles("C:/Users/josem/Desktop/TheFog","*.glb").Select(Path.GetFileName).OrderBy(n=>n).ToArray();report.desktopModels=names.Length;
   Place("ENV_CasaRural01.glb",new Vector3(18,0,34),6,180,"EXTERIOR · modelos propios");
   Place(names.Single(n=>n.StartsWith("ENV_Rejilla")),new Vector3(1,0.01f,18),.055f,0,"EXTERIOR · modelos propios");
   var interiors=names.Where(n=>n.StartsWith("INT")||n.StartsWith("INR")).ToArray();
   int bedroom=0,kitchen=0,shop=0;
   foreach(var file in interiors)
   {
    bool isKitchen=file.Contains("Cocina")||file.Contains("Fregadero")||file.Contains("Nevera");bool isShop=file.Contains("Supermercado")||file.Contains("Suepermercado");
    string zone;Vector3 p;float height;
    if(isKitchen){int i=kitchen++;zone="COCINA · disposición abierta provisional";p=new Vector3(15+(i%3)*3.5f,0,10+(i/3)*4);height=file.Contains("SUPERIOR")? .8f:file.Contains("Nevera")?1.85f:.9f;if(file.Contains("SUPERIOR"))p.y=1.5f;}
    else if(isShop){int i=shop++;zone="COMERCIO · disposición abierta provisional";p=new Vector3(17+i*4,0,23);height=file.Contains("ESTANTERIA")?1.9f:1.05f;}
    else{int i=bedroom++;zone="DORMITORIO Y SALÓN · disposición abierta provisional";p=new Vector3(13+(i%3)*3.5f,0,-8+(i/3)*4);height=file.Contains("Cama")?1:file.Contains("Mesita")?.65f:file.Contains("MesaMadera")?.8f:file.Contains("Silla")?1:file.Contains("Sofa")?1:2;}
    Place(file,p,height,0,zone);
   }
   // Track linked GLBs across all prefab instances, including the already placed exterior and flashlight.
   var linked=new HashSet<string>();foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None))
   {string p=PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject);if(!string.IsNullOrEmpty(p)&&p.EndsWith(".glb"))linked.Add(Path.GetFileName(p));}
   report.missing=names.Where(n=>!linked.Contains(n)).ToArray();report.representedModels=names.Count(n=>linked.Contains(n));if(report.missing.Length>0)throw new Exception("Desktop models not represented: "+string.Join(",",report.missing));
   EditorSceneManager.SaveScene(scene,target);report.saved=true;AssetDatabase.SaveAssets();File.WriteAllText("docs/desktop-environment.json",JsonUtility.ToJson(report,true));
   var camera=UnityEngine.Object.FindFirstObjectByType<FogPlayerController>().GetComponentInChildren<Camera>();camera.transform.SetPositionAndRotation(new Vector3(30,25,-22),Quaternion.identity);camera.transform.LookAt(new Vector3(1,0,18));camera.farClipPlane=180;RenderSettings.fogDensity=.007f;DanielRoomBuilder.Capture(camera,"docs/desktop-environment-overview.png");
   Debug.Log("DESKTOP ENVIRONMENT: all "+report.representedModels+" desktop models represented; open furniture review zones are provisional");
  }
 }
}
