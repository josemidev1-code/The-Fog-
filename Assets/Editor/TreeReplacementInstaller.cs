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
 public static class TreeReplacementInstaller
 {
  const string Folder="Assets/ThirdParty/ElegantCrowTrees";
  [Serializable] class Item {public string model;public int triangles;public float height;public Vector3 position;public bool foliageDoubleSided,alphaClip;}
  [Serializable] class Report {public int replaced;public bool originalsPreserved;public List<Item> trees=new();public bool visualReviewPending=true;}
  public static void Run()
  {
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/PlayableStage.unity");
   if(GameObject.Find("ÁRBOLES · Elegant Crow"))throw new Exception("Tree replacement already applied; preserving manual changes");
   var old=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Roble"&&t.GetComponent<CapsuleCollider>()!=null).ToArray();if(old.Length==0)throw new Exception("No expected original trees found");
   Directory.CreateDirectory(Folder+"/Materials");Directory.CreateDirectory("Assets/Prefabs/Trees");
   var report=new Report{originalsPreserved=true};var group=new GameObject("ÁRBOLES · Elegant Crow").transform;var archive=new GameObject("ÁRBOLES ANTERIORES · desactivados").transform;archive.gameObject.SetActive(false);
   string[] variants={"tree01","tree02","tree24"};
   for(int i=0;i<old.Length;i++)
   {
    var t=old[i];var oldRs=t.GetComponentsInChildren<Renderer>();var before=oldRs[0].bounds;foreach(var r in oldRs)before.Encapsulate(r.bounds);
    string id=variants[i%variants.Length];var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/Textures/"+id+".png");
    var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));if(importer.filterMode!=FilterMode.Point||!importer.alphaIsTransparency){importer.filterMode=FilterMode.Point;importer.alphaIsTransparency=true;importer.mipmapEnabled=true;importer.SaveAndReimport();}
    var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/Models/"+id+".fbx");if(!asset||!texture)throw new Exception("External tree missing source or texture");
    string mp=Folder+"/Materials/"+id+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(mp);if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,mp);}
    material.SetTexture("_BaseMap",texture);material.SetColor("_BaseColor",new Color(.8f,.84f,.76f,1));material.SetFloat("_AlphaClip",1);material.SetFloat("_Cutoff",.4f);material.SetFloat("_Cull",0);material.SetFloat("_Smoothness",0);material.SetFloat("_Metallic",0);material.EnableKeyword("_ALPHATEST_ON");material.SetOverrideTag("RenderType","TransparentCutout");material.renderQueue=2450;material.enableInstancing=true;
    var root=new GameObject("Arbol_"+id+"_"+i);root.transform.SetParent(group,false);var model=(GameObject)PrefabUtility.InstantiatePrefab(asset);model.transform.SetParent(root.transform,false);
    var rs=model.GetComponentsInChildren<Renderer>();if(rs.Length==0)throw new Exception("External FBX has no renderers");var bounds=rs[0].bounds;foreach(var r in rs){bounds.Encapsulate(r.bounds);r.sharedMaterials=Enumerable.Repeat(material,r.sharedMaterials.Length).ToArray();}
    float scale=before.size.y/bounds.size.y;model.transform.localScale*=scale;model.transform.localPosition=-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)*scale;
    root.transform.SetPositionAndRotation(t.position,t.rotation);var collider=root.AddComponent<CapsuleCollider>();collider.radius=.28f;collider.height=2.4f;collider.center=new Vector3(0,1.2f,0);
    int triangles=model.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);if(triangles==0)throw new Exception("External source is not a 3D mesh");
    report.trees.Add(new Item{model=id,triangles=triangles,height=before.size.y,position=t.position,foliageDoubleSided=true,alphaClip=true});
    t.SetParent(archive,true);report.replaced++;
    if(i<3)PrefabUtility.SaveAsPrefabAsset(root,"Assets/Prefabs/Trees/"+id+".prefab");
   }
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();File.WriteAllText("docs/tree-replacement.json",JsonUtility.ToJson(report,true));Capture();
  }
  public static void Capture()
  {
   EditorSceneManager.OpenScene("Assets/Scenes/PlayableStage.unity");int frames=0;EditorApplication.CallbackFunction tick=null;
   tick=()=>{if(++frames<90)return;EditorApplication.update-=tick;Shader.WarmupAllShaders();
    var camera=UnityEngine.Object.FindFirstObjectByType<FogPlayerController>().GetComponentInChildren<Camera>();camera.transform.position=new Vector3(-4,1.65f,-5);camera.transform.LookAt(GameObject.Find("CasaDaniel").transform.position+Vector3.up*2.3f);DanielRoomBuilder.Capture(camera,"docs/trees-after.png");
    var tree=GameObject.Find("ÁRBOLES · Elegant Crow").transform.GetChild(0);var rs=tree.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);camera.transform.position=b.center+new Vector3(0,-b.size.y*.15f,-b.size.y*1.3f);camera.transform.LookAt(b.center);DanielRoomBuilder.Capture(camera,"docs/tree-detail.png");EditorApplication.Exit(0);};EditorApplication.update+=tick;
  }
 }
}
