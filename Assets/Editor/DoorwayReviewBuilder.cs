using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TheFog.FirstPlayable;
namespace TheFog.EditorTools
{
    public static class DoorwayReviewBuilder
    {
        [MenuItem("THE FOG/Modules/Create doorway review scene")]
        public static void Run()
        {
            const string path="Assets/Scenes/DoorwayReview.unity";
            if(File.Exists(path))throw new System.Exception("Review scene exists; preserving manual edits");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(DoorwayChecks.PrefabPath);
            if(!asset)throw new System.Exception("Validated doorway prefab missing");
            PrefabUtility.InstantiatePrefab(asset,scene);
            var floor=new GameObject("TEST ONLY · invisible collision floor").AddComponent<BoxCollider>();floor.center=new Vector3(0,-.1f,0);floor.size=new Vector3(12,.2f,12);
            var actor=new GameObject("Daniel · module review");actor.SetActive(false);actor.transform.position=new Vector3(0,.1f,-2);
            var cc=actor.AddComponent<CharacterController>();cc.height=1.75f;cc.radius=.24f;cc.center=new Vector3(0,.88f,0);
            var input=actor.AddComponent<FogInput>();input.SetupDefaults();var player=actor.AddComponent<FogPlayerController>();player.input=input;
            var eye=new GameObject("Main Camera");eye.tag="MainCamera";eye.transform.SetParent(actor.transform,false);eye.transform.localPosition=new Vector3(0,1.62f,0);
            var camera=eye.AddComponent<Camera>();camera.nearClipPlane=.04f;camera.farClipPlane=30;camera.fieldOfView=65;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.16f,.18f,.17f);eye.AddComponent<AudioListener>();player.cameraPivot=eye.transform;
            var hud=actor.AddComponent<FogHud>();var pause=actor.AddComponent<FogPauseController>();
            pause.input=input;pause.hud=hud;pause.player=player;
            actor.SetActive(true);
            var light=new GameObject("Review light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.2f;light.transform.rotation=Quaternion.Euler(45,-30,0);
            RenderSettings.ambientLight=new Color(.4f,.4f,.4f);
            EditorSceneManager.SaveScene(scene,path);AssetDatabase.SaveAssets();
            DanielRoomBuilder.Capture(camera,"docs/doorway-unity-review.png");
            Debug.Log("DOORWAY REVIEW saved; invisible test floor, no final floor asset, no connection to house");
        }
    }
}
