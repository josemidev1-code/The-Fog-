#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace TheFog.EditorTools
{
 [InitializeOnLoad] public static class PlayableStageChecks
 {
  static PlayableStageChecks(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("PlayableStageChecks",false)){SessionState.SetBool("PlayableStageChecks",false);new GameObject("Playable stage route checks").AddComponent<FogSmokeDriver>();}};}
  public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/PlayableStage.unity");SessionState.SetBool("PlayableStageChecks",true);EditorApplication.EnterPlaymode();}
 }
}
#endif

