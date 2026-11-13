using System;
using System.IO;
using System.Collections;
using TheFog.FirstPlayable;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheFog.DanielRoom
{
    public class DanielTerminal : MonoBehaviour
    {
        public Camera view;
        public FogPlayerController player;
        public FogInput input;
        public FogGraphicsSettings graphics;
        public Transform terminalPose, standingPose, terminalTarget;
        public GameObject homePage, settingsPage;
        public Text status, hint;
        public Button continueButton;
        public Slider sensitivity, volume;
        public bool InTerminal { get; private set; } = true;
        public bool Transitioning { get; private set; }
        public string SavePath => Path.Combine(Application.persistentDataPath, "daniel-room-checkpoint.json");
        [Serializable] class Save { public int version = 1; public string scene = "DanielRoom"; public Vector3 position; public float yaw; }
        Save checkpoint;
        Vector3 eyeLocal; Quaternion eyeRotation;
        bool settings;
        bool reading; int blockedFrame = -1;
        public void SetReading(bool value) { reading=value; blockedFrame=Time.frameCount; player.controlEnabled=!value; }
        void Start()
        {
            eyeLocal = view.transform.localPosition; eyeRotation = view.transform.localRotation;
            player.mouseSensitivity = PlayerPrefs.GetFloat("TheFog.MouseSensitivity", .09f);
            AudioListener.volume = PlayerPrefs.GetFloat("TheFog.Volume", .7f);
            sensitivity.SetValueWithoutNotify(player.mouseSensitivity);
            volume.SetValueWithoutNotify(AudioListener.volume);
            LoadCheckpoint();
            OpenTerminal();
        }
        void Update()
        {
            if (Transitioning || reading || blockedFrame == Time.frameCount) return;
            if (InTerminal)
            {
                if (settings && input.PausePressed) ShowSettings(false);
                return;
            }
            if (input.PausePressed) { OpenTerminal(); return; }
            bool near = Vector3.Distance(player.transform.position, terminalTarget.position) < 2.4f && Vector3.Dot(view.transform.forward,(terminalTarget.position - view.transform.position).normalized) > .6f;
            if (Physics.Raycast(view.transform.position, view.transform.forward, out var clueHit, 2.5f) && clueHit.collider.GetComponentInParent<DanielRoomClue>() != null) return;
            hint.text = near ? "E · USAR TERMINAL     /     ESC · MENÚ" : "WASD · CAMINAR     RATÓN · MIRAR     ESC · MENÚ";
            if (near && input.InteractPressed) OpenTerminal();
        }
        public void Settings() { ShowSettings(true); }
        public void Back() { ShowSettings(false); }
        public void Low() { SetQuality(0); }
        public void Medium() { SetQuality(1); }
        public void High() { SetQuality(2); }
        public void OpenTerminal()
        {
            if (!InTerminal) SaveCheckpoint();
            InTerminal = true; player.controlEnabled = false;
            view.transform.SetParent(terminalPose, false);
            view.transform.localPosition = Vector3.zero; view.transform.localRotation = Quaternion.identity;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            ShowSettings(false); RefreshContinue();
            hint.text = "TERMINAL DE TELECOMUNICACIONES · VALDARA";
        }
        public void NewGame()
        {
            if (Transitioning || reading || blockedFrame == Time.frameCount) return;
            var initial = new Save {position = standingPose.position, yaw = standingPose.eulerAngles.y};
            if (!Store(initial)) return;
            StartCoroutine(EnterRoom(initial));
        }
        public void ContinueGame()
        {
            if (Transitioning || checkpoint == null) return;
            StartCoroutine(EnterRoom(checkpoint));
        }
        IEnumerator EnterRoom(Save save)
        {
            Transitioning = true;
            ShowSettings(false);
            var controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.SetPositionAndRotation(save.position, Quaternion.Euler(0, save.yaw, 0));
            controller.enabled = true;
            Vector3 from = view.transform.position;
            Quaternion rotation = view.transform.rotation;
            view.transform.SetParent(player.transform, false);
            Vector3 to = player.transform.TransformPoint(eyeLocal);
            Quaternion target = player.transform.rotation * eyeRotation;
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime / 1.1f)
            {
                float s = Mathf.SmoothStep(0, 1, t);
                view.transform.SetPositionAndRotation(Vector3.Lerp(from, to, s), Quaternion.Slerp(rotation, target, s));
                yield return null;
            }
            view.transform.localPosition = eyeLocal; view.transform.localRotation = eyeRotation;
            InTerminal = false; Transitioning = false; player.controlEnabled = true;
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
            status.text = "TURNO INICIADO\nMantén la radio a la escucha.";
        }
        public void ShowSettings(bool open)
        {
            settings = open; homePage.SetActive(!open); settingsPage.SetActive(open);
            if (status != null) status.enabled = !open;
        }
        public void SetQuality(int index) { graphics.Apply(index,true); }
        public void SetSensitivity(float value)
        {
            player.mouseSensitivity = value; PlayerPrefs.SetFloat("TheFog.MouseSensitivity",value);
        }
        public void SetVolume(float value)
        {
            AudioListener.volume = value; PlayerPrefs.SetFloat("TheFog.Volume",value);
        }
        public void SaveCheckpoint()
        {
            if (!InTerminal) Store(new Save {position=player.transform.position,yaw=player.transform.eulerAngles.y});
        }
        bool Store(Save save)
        {
            try
            {
                File.WriteAllText(SavePath + ".tmp", JsonUtility.ToJson(save,true));
                File.Copy(SavePath + ".tmp", SavePath,true); File.Delete(SavePath + ".tmp");
                checkpoint = save; RefreshContinue(); return true;
            }
            catch(Exception e) { status.text="NO SE PUDO GUARDAR LA SESIÓN"; Debug.LogWarning(e.Message); return false; }
        }
        void LoadCheckpoint()
        {
            try
            {
                if (!File.Exists(SavePath)) return;
                Save data=JsonUtility.FromJson<Save>(File.ReadAllText(SavePath));
                if (data == null || data.version != 1 || data.scene != "DanielRoom" ||
                    !float.IsFinite(data.position.x) || !float.IsFinite(data.position.y) || !float.IsFinite(data.position.z) ||
                    !float.IsFinite(data.yaw) || Mathf.Abs(data.position.x)>3.8f || Mathf.Abs(data.position.z)>3.3f || data.position.y < -.5f || data.position.y > 2f) return;
                checkpoint=data;
            }
            catch(Exception e) { Debug.LogWarning("Checkpoint no válido: " + e.Message); }
        }
        void RefreshContinue() { continueButton.interactable = checkpoint != null; }
        public void Quit()
        {
            PlayerPrefs.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        void OnApplicationQuit() { if (!Transitioning) SaveCheckpoint(); PlayerPrefs.Save(); }
    }
}
