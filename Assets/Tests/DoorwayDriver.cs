#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using TheFog.FirstPlayable;
namespace TheFog.EditorTools
{
    [InitializeOnLoad] public static class DoorwayChecks
    {
        public const string PrefabPath="Assets/Prefabs/Modules/INT_Valdara_ParedPuerta_A.prefab";
        static DoorwayChecks() { EditorApplication.playModeStateChanged += s => { if(s==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("DoorwayChecks",false)) { SessionState.SetBool("DoorwayChecks",false); new GameObject("Doorway physics checks").AddComponent<DoorwayDriver>(); } }; }
        public static void Run()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Optimized/INT_Valdara_ParedPuerta_A.glb");
            if(!asset)throw new Exception("Doorway GLB not imported");
            var root=(GameObject)PrefabUtility.InstantiatePrefab(asset);root.name="INT_Valdara_ParedPuerta_A";
            var materials=root.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).ToArray();
            if(materials.Length==0 || materials.Any(m=>!m || !m.shader || m.shader.name.Contains("Error")))throw new Exception("Doorway materials invalid");
            foreach(var f in root.GetComponentsInChildren<MeshFilter>()) { var c=f.gameObject.AddComponent<MeshCollider>();c.sharedMesh=f.sharedMesh;f.gameObject.isStatic=true; }
            PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);AssetDatabase.SaveAssets();UnityEngine.Object.DestroyImmediate(root);
            SessionState.SetBool("DoorwayChecks",true);EditorApplication.EnterPlaymode();
        }
    }
    public class DoorwayDriver:MonoBehaviour
    {
        [Serializable] class Report { public bool passed; public List<string> checks=new();public List<string> errors=new();public float walkMeasured,sprintMeasured;public Vector3 moduleSize; }
        Report report=new();Keyboard keyboard;CharacterController cc;FogPlayerController player;int frames;bool finished;
        void Check(bool ok,string s) { (ok?report.checks:report.errors).Add(s); }
        void Error(string message,string stack,LogType type) { if(type==LogType.Error || type==LogType.Exception)report.errors.Add(message); }
        void Warp(Vector3 p,float yaw=0) { cc.enabled=false;player.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));cc.enabled=true;Physics.SyncTransforms(); }
        IEnumerator Hold(int count,params Key[] keys) { for(int i=0;i<count;i++) { InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));yield return null; } }
        IEnumerator Start()
        {
            Application.logMessageReceived+=Error;Application.runInBackground=true;Time.captureFramerate=60;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var module=Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(DoorwayChecks.PrefabPath));
            var rs=module.GetComponentsInChildren<Renderer>();var bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);report.moduleSize=bounds.size;
            Check(Mathf.Abs(bounds.size.y-2.7f)<.03f,"Module imported at 2.70m height, no error materials");
            // Invisible floor is test infrastructure only, not a finished art asset.
            var floor=new GameObject("TEST ONLY invisible floor").AddComponent<BoxCollider>();floor.center=new Vector3(0,-.1f,0);floor.size=new Vector3(20,.2f,20);
            var actor=new GameObject("Daniel physics test");cc=actor.AddComponent<CharacterController>();cc.height=1.75f;cc.radius=.24f;cc.center=new Vector3(0,.88f,0);
            var input=actor.AddComponent<FogInput>();player=actor.AddComponent<FogPlayerController>();player.input=input;
            keyboard=InputSystem.AddDevice<Keyboard>("DoorwayChecksKeyboard");
            Warp(new Vector3(0,.1f,-2));yield return Hold(45);Check(cc.isGrounded,"Gravity settles Daniel on floor");
            yield return Hold(15,Key.W);var p=actor.transform.position;yield return Hold(30,Key.W);report.walkMeasured=(actor.transform.position.z-p.z)/.5f;
            Check(Mathf.Abs(report.walkMeasured-player.walkSpeed)<.2f,"Forward walking speed matches configured 3.2m/s");
            yield return Hold(45,Key.W);Check(actor.transform.position.z>1,"Daniel crosses doorway forwards without collision blockage");
            yield return Hold(15);Warp(new Vector3(0,.1f,2),180);yield return Hold(90,Key.W);Check(actor.transform.position.z< -1,"Daniel crosses doorway in reverse direction");
            yield return Hold(15);Warp(new Vector3(-1.2f,.1f,-2));yield return Hold(90,Key.W);Check(actor.transform.position.z< -.15f && actor.transform.position.z> -1,"Wall collider blocks Daniel beside doorway");
            yield return Hold(15);Warp(new Vector3(0,.1f,-5));yield return Hold(15,Key.W,Key.LeftShift);p=actor.transform.position;yield return Hold(30,Key.W,Key.LeftShift);report.sprintMeasured=(actor.transform.position.z-p.z)/.5f;
            Check(Mathf.Abs(report.sprintMeasured-player.sprintSpeed)<.2f,"Sprint speed matches configured 5.2m/s");
            yield return Hold(15);Warp(new Vector3(0,2,-2));yield return Hold(90);Check(cc.isGrounded && actor.transform.position.y<.15f,"Daniel falls from 2m and lands on floor");
            Finish();
        }
        void Update() { if(++frames>1200&&!finished) {report.errors.Add("Play Mode test timeout");Finish();} }
        void Finish() { if(finished)return;finished=true;if(keyboard!=null)InputSystem.RemoveDevice(keyboard);Application.logMessageReceived-=Error;report.passed=report.errors.Count==0;File.WriteAllText("docs/doorway-play-mode.json",JsonUtility.ToJson(report,true));EditorApplication.Exit(report.passed?0:1); }
    }
}
#endif
