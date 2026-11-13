#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using TheFog.DanielRoom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace TheFog.EditorTools
{
    [InitializeOnLoad] public static class DanielRoomChecks
    {
        static DanielRoomChecks()
        {
            EditorApplication.playModeStateChanged += s=> {
                if(s==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("DanielRoomCheck",false)) {
                    SessionState.SetBool("DanielRoomCheck",false);
                    new GameObject("Room checks").AddComponent<DanielRoomDriver>();
                }
            };
        }
        public static void Run() {
            EditorSceneManager.OpenScene("Assets/Scenes/DanielRoom.unity");
            SessionState.SetBool("DanielRoomCheck",true); EditorApplication.EnterPlaymode();
        }
    }
    public class DanielRoomDriver : MonoBehaviour
    {
        [Serializable] class Report { public bool passed; public List<string> checks=new(); public List<string> errors=new(); }
        Report report=new();
        string savePath, backup; bool hadSave; int oldQuality; float oldSensitivity,oldVolume;
        void Check(bool value,string description) { if(value)report.checks.Add(description);else report.errors.Add(description); }
        void Error(string message,string stack,LogType type) { if(type==LogType.Exception || type==LogType.Error)report.errors.Add(message); }
        static void Capture(Camera camera,string path)
        {
            Canvas.ForceUpdateCanvases();var rt=new RenderTexture(1280,720,24);var old=camera.targetTexture;var active=RenderTexture.active;
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
            camera.targetTexture=old;RenderTexture.active=active;Destroy(rt);Destroy(texture);
        }
        IEnumerator Start()
        {
            Application.runInBackground=true; Time.captureFramerate=60;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            Application.logMessageReceived+=Error;
            var terminal=FindAnyObjectByType<DanielTerminal>();
            savePath=terminal.SavePath;hadSave=File.Exists(savePath);if(hadSave)backup=File.ReadAllText(savePath);
            oldQuality=PlayerPrefs.GetInt("TheFog.GraphicsPreset",1);
            oldSensitivity=PlayerPrefs.GetFloat("TheFog.MouseSensitivity",.09f);oldVolume=PlayerPrefs.GetFloat("TheFog.Volume",.7f);
            yield return null;yield return null;
            Check(terminal.InTerminal && !terminal.player.controlEnabled,"Menu starts at physical terminal and blocks walking");
            Check(terminal.view.transform.parent==terminal.terminalPose,"Camera uses terminal pose");
            Check(FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>()!=null,"Input System UI event system present");
            Check(FindObjectsByType<Transform>().All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0),"No missing scripts in scene");
            terminal.Settings(); Check(terminal.settingsPage.activeSelf && !terminal.homePage.activeSelf,"Settings opens");
            terminal.Low(); Check(terminal.graphics.Current==0,"Low quality applies");
            terminal.High(); Check(terminal.graphics.Current==2,"High quality applies");
            terminal.SetSensitivity(.12f); Check(Mathf.Approximately(terminal.player.mouseSensitivity,.12f),"Mouse sensitivity applies");
            terminal.SetVolume(.4f);Check(Mathf.Approximately(AudioListener.volume,.4f),"Volume applies");
            terminal.Back();
            var newGame=terminal.homePage.GetComponentsInChildren<Button>().First(b=>b.name=="NUEVA PARTIDA");
            Canvas.ForceUpdateCanvases();
            var pointer=new PointerEventData(EventSystem.current) { position=terminal.view.WorldToScreenPoint(newGame.transform.position) };
            var hits=new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer,hits);
            Check(hits.Any(h=>h.gameObject==newGame.gameObject),"Physical CRT menu receives pointer raycasts");
            newGame.onClick.Invoke();
            yield return new WaitForSecondsRealtime(1.4f);
            Check(!terminal.InTerminal && terminal.player.controlEnabled,"New game enters playable room");
            Check(File.Exists(savePath) && terminal.continueButton.interactable,"Checkpoint enables Continue");
            yield return new WaitForSecondsRealtime(.2f);
            for(int i=0;i<15;i++) yield return null;
            Check(terminal.player.GetComponent<CharacterController>().isGrounded,"Player grounded on room floor at " + terminal.player.transform.position);
            Vector3 before=terminal.player.transform.position;
            // Queue a real Input System device state over multiple frames, preserving release ordering.
            var keyboard=InputSystem.AddDevice<Keyboard>("DanielRoomTest");

            for(int i=0;i<25;i++){ InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.S)); InputSystem.Update(); yield return null; }
            report.checks.Add("Input diagnostics: key="+keyboard.sKey.isPressed+" action="+terminal.input.Move+" active="+terminal.player.isActiveAndEnabled+" delta="+Time.deltaTime+" enabled="+terminal.input.move.enabled+" bindings="+terminal.input.move.bindings.Count);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Check(Vector3.Distance(before,terminal.player.transform.position)>.05f,"Keyboard moves Daniel in room: "+before+" -> "+terminal.player.transform.position);
            InputSystem.RemoveDevice(keyboard);
            var reader=FindAnyObjectByType<DanielRoomInvestigation>();
            var clues=FindObjectsByType<DanielRoomClue>();
            Check(reader!=null && clues.Length==3,"Three room clues available");
            reader.Examine(clues[0]);Check(reader.Reading && !terminal.player.controlEnabled && reader.panel.activeSelf,"Examining a clue blocks movement and displays document");
            reader.Close();Check(!reader.Reading && terminal.player.controlEnabled,"Closing document restores first person control");
            terminal.OpenTerminal();Check(terminal.InTerminal && !terminal.player.controlEnabled,"Return to PC blocks movement");
            terminal.ContinueGame();yield return new WaitForSecondsRealtime(1.4f);
            Check(!terminal.InTerminal,"Continue resumes room checkpoint");
            terminal.OpenTerminal();
            Capture(terminal.view,"docs/daniel-room-runtime.png");
            if(hadSave)File.WriteAllText(savePath,backup);else if(File.Exists(savePath))File.Delete(savePath);
            PlayerPrefs.SetInt("TheFog.GraphicsPreset",oldQuality);PlayerPrefs.SetFloat("TheFog.MouseSensitivity",oldSensitivity);PlayerPrefs.SetFloat("TheFog.Volume",oldVolume);
            terminal.graphics.Apply(oldQuality,false);PlayerPrefs.Save();
            Application.logMessageReceived-=Error;
            report.passed=report.errors.Count==0;
            File.WriteAllText("docs/daniel-room-checks.json",JsonUtility.ToJson(report,true));
            EditorApplication.Exit(report.passed?0:1);
        }
    }
}
#endif
