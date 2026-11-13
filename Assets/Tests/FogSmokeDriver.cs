#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheFog.FirstPlayable;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace TheFog.EditorTools
{
    [InitializeOnLoad]
    public static class FogSmokeChecks
    {
        const string Running = "TheFog.SmokeRunning";
        static FogSmokeChecks()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Running, false)) return;
                SessionState.SetBool(Running, false);
                new GameObject("Automated smoke check").AddComponent<FogSmokeDriver>();
            };
        }
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/FirstPlayable.unity");
            SessionState.SetBool(Running, true);
            EditorApplication.EnterPlaymode();
        }
    }

    public class FogSmokeDriver : MonoBehaviour
    {
        [Serializable] public class Report { public string unityVersion; public bool passed; public List<string> checks = new(); public List<string> failures = new(); }
        Report report = new();
        Keyboard keyboard;
        FogPlayerController player;
        FogGraphicsSettings graphics;
        FogFlashlight flashlight;
        FogPauseController pause;
        FogTestRoute route;
        FogInput input;
        int phase, frames, totalFrames, waypoint;
        Vector3 pausedPosition;
        bool originalFlashlight, hadPreference;
        int previousPreference;
        readonly Vector3[] path = { new(1.5f,0,12), new(-2,0,24), new(2,0,36), new(3,0,43) };

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            report.unityVersion = Application.unityVersion;
            hadPreference = PlayerPrefs.HasKey(FogGraphicsSettings.PrefsKey);
            previousPreference = PlayerPrefs.GetInt(FogGraphicsSettings.PrefsKey,1);
            keyboard = InputSystem.AddDevice<Keyboard>("FogSmokeKeyboard");
            Application.logMessageReceived += CaptureError;
            Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            Time.captureFramerate = 60;
            Bind();
        }
        void Bind()
        {
            player = FindAnyObjectByType<FogPlayerController>();
            graphics = FindAnyObjectByType<FogGraphicsSettings>();
            flashlight = FindAnyObjectByType<FogFlashlight>();
            pause = FindAnyObjectByType<FogPauseController>();
            route = FindAnyObjectByType<FogTestRoute>();
            input = FindAnyObjectByType<FogInput>();
        }
        void CaptureError(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.failures.Add(message);
        }
        void QueueKeys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys)); }
        void Check(bool condition,string text)
        {
            (condition ? report.checks : report.failures).Add(text);
        }
        void Next() { phase++; frames=0; QueueKeys(); }
        void LateUpdate()
        {
            try { Tick(); }
            catch (Exception e) { report.failures.Add(e.ToString()); Finish(); }
        }
        void Tick()
        {
            frames++; totalFrames++;
            if (totalFrames>2400) { report.failures.Add("Timed out navigating the test route"); Finish(); return; }
            if (phase==0 && frames==30)
            {
                Check(player && graphics && flashlight && pause && route && input,"All gameplay modules present");
                Check(player.GetComponent<CharacterController>().isGrounded,"Player settles on the ground");
                Check(Mathf.Abs(player.transform.position.y)<0.25f,"Spawn is on the walkable floor");
                Check(FindObjectsByType<Transform>().All(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0),"No missing scripts in scene");
                ScreenCapture.CaptureScreenshot("docs/first-playable-gameplay.png");
                originalFlashlight=flashlight.IsOn;
                Next(); QueueKeys(Key.W);
            }
            else if (phase==1 && frames==60)
            {
                Check(player.transform.position.z>2.5f,"W action moves player through CharacterController");
                Next(); QueueKeys(Key.F);
            }
            else if (phase==2 && frames==3)
            {
                Check(flashlight.IsOn!=originalFlashlight,"F action toggles flashlight"); Next(); QueueKeys(Key.F1);
            }
            else if (phase==3 && frames==3)
            {
                Check(graphics.Current==0 && Mathf.Approximately(Camera.main.farClipPlane,55),"F1 applies Low graphics preset"); Next(); QueueKeys(Key.F3);
            }
            else if (phase==4 && frames==3)
            {
                Check(graphics.Current==2 && Mathf.Approximately(Camera.main.farClipPlane,95),"F3 applies High graphics preset"); Next(); QueueKeys(Key.F2);
            }
            else if (phase==5 && frames==3)
            {
                Check(graphics.Current==1,"F2 applies Medium graphics preset"); Next(); QueueKeys(Key.Escape);
            }
            else if (phase==6 && frames==3)
            {
                Check(pause.Paused && Time.timeScale==0,"Escape pauses gameplay"); pausedPosition=player.transform.position; Next(); QueueKeys(Key.W);
            }
            else if (phase==7 && frames==15)
            {
                Check(Vector3.Distance(player.transform.position,pausedPosition)<0.01f,"Movement is blocked while paused"); Next(); QueueKeys(Key.Escape);
            }
            else if (phase==8 && frames==3)
            {
                Check(!pause.Paused && Time.timeScale==1,"Escape resumes gameplay"); Next();
            }
            else if (phase==9)
            {
                Vector3 delta=path[waypoint]-player.transform.position; delta.y=0;
                if(delta.magnitude<0.5f)
                {
                    waypoint++;
                    if(waypoint==path.Length) { Check(true,"Player traverses all route waypoints with collisions enabled"); Next(); return; }
                    delta=path[waypoint]-player.transform.position; delta.y=0;
                }
                player.transform.rotation=Quaternion.LookRotation(delta);
                QueueKeys(Key.W);
            }
            else if (phase==10)
            {
                var mine=FindObjectsByType<FogInteractable>().First(i=>i.completesRoute!=null);
                var bounds=mine.GetComponent<Collider>().bounds;
                Vector3 target=bounds.center-Vector3.forward*(bounds.extents.z+1.5f); target.y=0;
                Vector3 delta=target-player.transform.position; delta.y=0;
                if(delta.magnitude>0.15f) { player.transform.rotation=Quaternion.LookRotation(delta); QueueKeys(Key.W); }
                else { player.transform.rotation=Quaternion.Euler(0,0,0); Next(); }
            }
            else if (phase==11 && frames==5) { Next(); QueueKeys(Key.E); }
            else if (phase==12 && frames==4)
            {
                Check(route.Completed,"E raycast interaction completes provisional route"); Next(); QueueKeys(Key.Escape);
            }
            else if (phase==13 && frames==3) { Next(); QueueKeys(Key.R); }
            else if (phase==14 && frames>=30)
            {
                Bind();
                Check(!route.Completed && player.transform.position.z<1,"R from pause reloads scene and resets route");
                Finish();
            }
        }
        void Finish()
        {
            enabled=false;
            QueueKeys();
            Application.logMessageReceived-=CaptureError;
            Time.captureFramerate=0;
            Time.timeScale=1;
            if(hadPreference) PlayerPrefs.SetInt(FogGraphicsSettings.PrefsKey,previousPreference);
            else PlayerPrefs.DeleteKey(FogGraphicsSettings.PrefsKey);
            if(keyboard!=null) InputSystem.RemoveDevice(keyboard);
            report.passed=report.failures.Count==0;
            File.WriteAllText("docs/first-playable-smoke-test.json",JsonUtility.ToJson(report,true));
            Debug.Log("THE FOG SMOKE: "+(report.passed?"PASS":"FAIL"));
            EditorApplication.Exit(report.passed?0:1);
        }
    }
}

#endif
