#if UNITY_EDITOR
using System;
using System.IO;
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
 [InitializeOnLoad] public static class GroundTileChecks
 {
  static GroundTileChecks(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("GroundTileChecks",false)){SessionState.SetBool("GroundTileChecks",false);new GameObject("Ground seam checks").AddComponent<GroundTileDriver>();}};}
  public static void Run(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SessionState.SetBool("GroundTileChecks",true);EditorApplication.EnterPlaymode();}
 }
 public class GroundTileDriver:MonoBehaviour
 {
  [Serializable] class Report {public bool passed;public List<string> checks=new();public List<string> errors=new();public Vector3 start,end;public int seamSamples;public float maxSeamDelta;}
  Report report=new();Keyboard keyboard;int frames;bool done;
  void Check(bool ok,string message){(ok?report.checks:report.errors).Add(message);}
  IEnumerator Start()
  {
   Application.runInBackground=true;Time.captureFramerate=60;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Modules/ENV_SueloRural_A.prefab");if(!prefab){report.errors.Add("Missing real ground prefab");Finish();yield break;}
   foreach(float x in new[]{-2f,2f})foreach(float z in new[]{-2f,2f})Instantiate(prefab,new Vector3(x,0,z),Quaternion.identity);
   Physics.SyncTransforms();bool hits=true;
   for(int i=0;i<15;i++)
   {
    float v=-3.5f+i*.5f;
    foreach(bool vertical in new[]{true,false})
    {
     Vector3 a=vertical?new Vector3(-.002f,.5f,v):new Vector3(v,.5f,-.002f);Vector3 b=vertical?new Vector3(.002f,.5f,v):new Vector3(v,.5f,.002f);
     bool ha=Physics.Raycast(a,Vector3.down,out var ah,1),hb=Physics.Raycast(b,Vector3.down,out var bh,1);hits&=ha&&hb;report.seamSamples+=2;if(ha&&hb)report.maxSeamDelta=Mathf.Max(report.maxSeamDelta,Mathf.Abs(ah.point.y-bh.point.y));
    }
   }
   Check(hits,"60 seam-adjacent surface raycasts find real ground without collision plane");Check(report.maxSeamDelta<.005f,"Adjacent tiles have less than 5mm height difference at seam");
   var actor=new GameObject("Daniel terrain test");actor.transform.position=new Vector3(-3,.1f,-3);actor.transform.rotation=Quaternion.Euler(0,45,0);var cc=actor.AddComponent<CharacterController>();cc.height=1.75f;cc.radius=.24f;cc.center=new Vector3(0,.88f,0);var input=actor.AddComponent<FogInput>();var player=actor.AddComponent<FogPlayerController>();player.input=input;keyboard=InputSystem.AddDevice<Keyboard>("GroundSeamKeyboard");
   for(int i=0;i<45;i++)yield return null;Check(cc.isGrounded,"Gravity settles Daniel on generated terrain");report.start=actor.transform.position;
   for(int i=0;i<145;i++){InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));yield return null;}
   InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;report.end=actor.transform.position;
   Check(report.end.x>1.5f&&report.end.z>1.5f,"CharacterController crosses both tile seams diagonally");Check(cc.isGrounded&&report.end.y>-.1f&&report.end.y<.15f,"Daniel remains grounded after crossing without hidden floor");Finish();
  }
  void Update(){if(++frames>900&&!done){report.errors.Add("Ground Play Mode test timed out");Finish();}}
  void Finish(){if(done)return;done=true;if(keyboard!=null)InputSystem.RemoveDevice(keyboard);report.passed=report.errors.Count==0;File.WriteAllText("docs/ground-play-mode.json",JsonUtility.ToJson(report,true));EditorApplication.Exit(report.passed?0:1);}
 }
}
#endif
