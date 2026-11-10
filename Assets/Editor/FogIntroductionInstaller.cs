using System;
using System.IO;
using TheFog.FirstPlayable;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheFog.EditorTools
{
    public static class FogIntroductionInstaller
    {
        [MenuItem("THE FOG/Introduction/Install or update animatic")]
        public static void Install()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/FirstPlayable.unity");
            var intro=UnityEngine.Object.FindAnyObjectByType<FogIntroduction>();
            if (!intro) intro=new GameObject("INTRODUCCION_MISTERIO").AddComponent<FogIntroduction>();
            intro.pause=UnityEngine.Object.FindAnyObjectByType<FogPauseController>();
            intro.hud=UnityEngine.Object.FindAnyObjectByType<FogHud>();
            intro.playerCamera=Camera.main;
            intro.flashlightModel=UnityEngine.Object.FindAnyObjectByType<FogFlashlight>().gameObject;
            intro.shots=new []
            {
                new FogIntroduction.Shot { from=new Vector3(12,5,-9),to=new Vector3(8,4,-5),lookAt=new Vector3(0,1,14),seconds=7,caption="VALDARA\nAl caer la noche" },
                new FogIntroduction.Shot { from=new Vector3(-6,3,-5),to=new Vector3(-6,2.6f,-1),lookAt=new Vector3(-13,2.5f,3),seconds=7,caption="Daniel vigila el pueblo.\nEsta noche, la niebla vuelve a avanzar." },
                new FogIntroduction.Shot { from=new Vector3(-3,2.1f,32),to=new Vector3(-1,1.8f,37),lookAt=new Vector3(3,2,50),seconds=7,caption="Algo permanece oculto en Valdara." },
                new FogIntroduction.Shot { from=new Vector3(0,1.9f,-2),to=new Vector3(0,1.62f,0),lookAt=new Vector3(2,1.6f,20),seconds=6,caption="THE FOG" }
            };
            EditorUtility.SetDirty(intro);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            File.WriteAllText("docs/introduction-installation.json","{\"sceneSaved\":true,\"shots\":4,\"durationSeconds\":27,\"provisional\":true}");
            Debug.Log("THE FOG INTRODUCTION: INSTALLED");
            if(Application.isBatchMode) EditorApplication.Exit(0);
        }
    }
}
