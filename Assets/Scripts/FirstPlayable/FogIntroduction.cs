using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheFog.FirstPlayable
{
    /// <summary>Animatic provisional en tiempo real. No revela el origen de la niebla.</summary>
    [DefaultExecutionOrder(200)]
    public sealed class FogIntroduction : MonoBehaviour
    {
        [Serializable] public class Shot
        {
            public Vector3 from;
            public Vector3 to;
            public Vector3 lookAt;
            public float seconds = 7;
            [TextArea] public string caption;
        }
        public Shot[] shots;
        public FogPauseController pause;
        public FogHud hud;
        public Camera playerCamera;
        public GameObject flashlightModel;
        public bool playOnStart = true;
        public static bool SuppressForTests;
        public bool Playing { get; private set; }
        public bool Completed { get; private set; }
        Camera cinematicCamera;
        Canvas overlay;
        Text caption, skip;
        Image fade;
        int shot;
        float elapsed;
        bool oldCameraEnabled, oldFlashlightActive;

        void Start()
        {
            if (playOnStart && !SuppressForTests) Begin();
        }
        public void Begin()
        {
            if (Playing || shots == null || shots.Length == 0 || !playerCamera || !pause) return;
            Playing = true;
            Completed = false;
            shot = 0;
            elapsed = 0;
            oldCameraEnabled = playerCamera.enabled;
            oldFlashlightActive = flashlightModel && flashlightModel.activeSelf;
            pause.SetPaused(false);
            pause.enabled = false;
            pause.player.controlEnabled = false;
            if (pause.flashlight) pause.flashlight.controlEnabled = false;
            if (pause.interactor) pause.interactor.controlEnabled = false;
            if (hud) hud.gameObject.SetActive(false);
            if (flashlightModel) flashlightModel.SetActive(false);
            playerCamera.enabled = false;
            var cameraObject = new GameObject("Camara de introduccion");
            cinematicCamera = cameraObject.AddComponent<Camera>();
            cinematicCamera.CopyFrom(playerCamera);
            cinematicCamera.enabled = true;
            cinematicCamera.fieldOfView = 52;
            cinematicCamera.useOcclusionCulling = true;
            CreateOverlay();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
            ApplyShot();
        }
        void Update()
        {
            if (!Playing) return;
            var keyboard = Keyboard.current;
            var gamepad = Gamepad.current;
            if ((keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
                || (gamepad != null && gamepad.startButton.wasPressedThisFrame))
            { Finish(); return; }
            elapsed += Time.unscaledDeltaTime;
            if (elapsed >= Mathf.Max(1, shots[shot].seconds))
            {
                shot++;
                elapsed = 0;
                if (shot >= shots.Length) { Finish(); return; }
            }
            ApplyShot();
        }
        void ApplyShot()
        {
            Shot current = shots[shot];
            float duration = Mathf.Max(1, current.seconds);
            float t = Mathf.Clamp01(elapsed / duration);
            cinematicCamera.transform.position = Vector3.Lerp(current.from, current.to, Mathf.SmoothStep(0,1,t));
            cinematicCamera.transform.LookAt(current.lookAt);
            caption.text = current.caption;
            // Fundidos independientes del tiempo de juego, sin cortes bruscos.
            float opacity = Mathf.Max(1-Mathf.Clamp01(elapsed/0.8f), 1-Mathf.Clamp01((duration-elapsed)/0.8f));
            fade.color = new Color(0.015f,0.022f,0.018f,opacity);
        }
        public void Finish()
        {
            if (!Playing) return;
            Playing = false;
            Completed = true;
            if (cinematicCamera) { cinematicCamera.enabled = false; Destroy(cinematicCamera.gameObject); }
            if (overlay) { overlay.gameObject.SetActive(false); Destroy(overlay.gameObject); }
            if (playerCamera) playerCamera.enabled = oldCameraEnabled;
            if (flashlightModel) flashlightModel.SetActive(oldFlashlightActive);
            if (hud) hud.gameObject.SetActive(true);
            if (pause) { pause.enabled = true; pause.SetPaused(false); }
        }
        void OnDisable() { if (Playing) Finish(); }
        void CreateOverlay()
        {
            var go = new GameObject("UI_Introduccion");
            overlay = go.AddComponent<Canvas>();
            overlay.renderMode = RenderMode.ScreenSpaceOverlay;
            overlay.sortingOrder = 100;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280,720);
            AddImage("Banda superior",new Vector2(0,0.88f),Vector2.one,Color.black);
            AddImage("Banda inferior",Vector2.zero,new Vector2(1,0.14f),Color.black);
            caption = AddText("Texto",new Vector2(0.1f,0.16f),new Vector2(0.9f,0.29f),25,TextAnchor.MiddleCenter);
            skip = AddText("Saltar",new Vector2(0.63f,0.025f),new Vector2(0.97f,0.09f),15,TextAnchor.MiddleRight);
            skip.text = "ESPACIO / ESC / START · Saltar";
            fade = AddImage("Fundido",Vector2.zero,Vector2.one,new Color(0,0,0,1));
        }
        Image AddImage(string label, Vector2 min,Vector2 max,Color color)
        {
            var go = new GameObject(label); go.transform.SetParent(overlay.transform,false);
            var image=go.AddComponent<Image>(); image.raycastTarget=false; image.color=color;
            Fit(image.rectTransform,min,max); return image;
        }
        Text AddText(string label,Vector2 min,Vector2 max,int size,TextAnchor alignment)
        {
            var go=new GameObject(label); go.transform.SetParent(overlay.transform,false);
            var text=go.AddComponent<Text>(); text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize=size; text.alignment=alignment; text.color=new Color(0.85f,0.87f,0.79f);
            text.raycastTarget=false; Fit(text.rectTransform,min,max); return text;
        }
        static void Fit(RectTransform rect,Vector2 min,Vector2 max)
        { rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero; }
    }
}
