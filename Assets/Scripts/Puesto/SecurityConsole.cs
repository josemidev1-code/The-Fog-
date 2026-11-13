using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>Consola de cámaras de la torre. Daniel se sienta, cambia de cámara y registra anomalías.</summary>
    public class SecurityConsole : Interactable
    {
        public SecurityFeed[] feeds;
        public Renderer screen;
        public Transform seatView;
        public RenderTexture feedTexture;
        public float liveDistance = 6f;
        public int current;

        Material screenMat;
        int enterFrame = -1;
        float switchTime = -10f;

        public override string Prompt => "Sentarse frente al monitor";
        public override bool CanInteract => base.CanInteract && PuestoGame.I != null && PuestoGame.I.State == GameState.Playing;
        public float SinceSwitch => Time.time - switchTime;
        public SecurityFeed Current => feeds[current];

        void Start()
        {
            screenMat = screen.material;
            foreach (var f in feeds) { f.camera.targetTexture = feedTexture; f.camera.enabled = false; }
        }

        public override void Interact()
        {
            var g = PuestoGame.I;
            g.EnterConsole(); enterFrame = Time.frameCount;
            g.player.PoseCamera(seatView, 0.45f);
            g.hud.ShowConsole(true);
            ProcAudio.Play2D(ProcAudio.Beep, 0.25f, 0.8f);
            g.radio.OnConsoleUsed();
        }

        void Update()
        {
            var g = PuestoGame.I; if (g == null) return;
            bool near = Vector3.Distance(g.player.transform.position, screen.transform.position) < liveDistance;
            bool live = g.power.On && (g.State == GameState.Console || near) && g.State != GameState.Ended;
            for (int i = 0; i < feeds.Length; i++) feeds[i].camera.enabled = live && i == current;
            if (screenMat != null)
            {
                Color c = g.power.On ? Color.white * (0.85f + 0.08f * Mathf.Sin(Time.time * 50f)) : Color.black;
                screenMat.SetColor("_BaseColor", c);
            }

            if (g.State != GameState.Console || Time.frameCount == enterFrame) return;
            if (PInput.Back) { Exit(); return; }
            int n = PInput.NumberPressed();
            if (n >= 0 && n < feeds.Length) Switch(n);
            else if (PInput.NextCam) Switch(current + 1);
            else if (PInput.PrevCam) Switch(current - 1);
            else if (PInput.Report) Report();
        }

        public void Switch(int index)
        {
            int next = (index % feeds.Length + feeds.Length) % feeds.Length;
            if (next == current && SinceSwitch < 0.2f) return;
            current = next; switchTime = Time.time;
            ProcAudio.Play2D(ProcAudio.Beep, 0.18f, 1.25f);
            ProcAudio.Play2D(ProcAudio.Static, 0.12f);
        }

        public void Report()
        {
            var g = PuestoGame.I;
            if (!g.power.On) { g.hud.Toast("Monitor apagado. Sin corriente no hay nada que anotar."); return; }
            if (SinceSwitch < 0.6f) return; // la imagen aún se está estabilizando
            g.director.Report(Current.site);
        }

        public void Exit()
        {
            var g = PuestoGame.I;
            g.ExitConsole();
            g.player.ReleaseCamera(0.35f);
            g.hud.ShowConsole(false);
        }

        public void ForceExit()
        {
            if (PuestoGame.I.State == GameState.Console) Exit();
        }
    }

    [System.Serializable]
    public class SecurityFeed
    {
        public string label;
        public Camera camera;
        public AnomalySite site;
    }
}
