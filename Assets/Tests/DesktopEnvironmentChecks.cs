#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace TheFog.EditorTools
{
 [InitializeOnLoad] public static class DesktopEnvironmentChecks
 {
  static DesktopEnvironmentChecks(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("DesktopEnvironmentChecks",false)){SessionState.SetBool("DesktopEnvironmentChecks",false);new GameObject("Desktop environment route checks").AddComponent<FogSmokeDriver>();}};}
  public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/DesktopEnvironment.unity");SessionState.SetBool("DesktopEnvironmentChecks",true);EditorApplication.EnterPlaymode();}
 }
}
#endif
