using System.Collections;
using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>Linterna de Daniel: hay que encontrarla, gasta pilas y parpadea cuando se agotan.</summary>
    public class PlayerFlashlight : MonoBehaviour
    {
        public Light spot;
        public GameObject viewModel;
        public bool owned;
        public bool on;
        [Range(0, 1)] public float battery = 0.45f;
        public int spareBatteries;
        public float secondsPerCharge = 150f;

        float baseIntensity;

        void Start()
        {
            baseIntensity = spot.intensity;
            spot.enabled = false;
            if (viewModel != null) viewModel.SetActive(owned);
        }

        void Update()
        {
            var g = PuestoGame.I; if (g == null || g.State == GameState.Paused) return;
            if (owned && PInput.Flashlight && (g.State == GameState.Playing || g.State == GameState.Console)) Toggle();
            if (on)
            {
                battery -= Time.deltaTime / secondsPerCharge;
                if (battery <= 0f)
                {
                    if (spareBatteries > 0) { spareBatteries--; battery = 1f; g.hud.Toast("Pilas nuevas. La linterna vuelve a alumbrar."); ProcAudio.Play2D(ProcAudio.Click, 0.8f, 0.7f); }
                    else { battery = 0f; on = false; g.hud.Toast("La linterna se apaga. Tiene que haber pilas en algún cajón."); ProcAudio.Play2D(ProcAudio.Click, 0.8f, 0.6f); }
                }
                float flicker = battery < 0.15f && Random.value < 0.12f ? Random.Range(0.05f, 0.5f) : 1f;
                spot.intensity = baseIntensity * Mathf.Lerp(0.45f, 1f, Mathf.Clamp01(battery * 4f)) * flicker;
            }
            spot.enabled = on;
        }

        public void Toggle()
        {
            if (!owned) return;
            if (!on && battery <= 0f) { PuestoGame.I.hud.Toast("Ni un parpadeo. Sin pilas."); ProcAudio.Play2D(ProcAudio.Click, 0.6f, 0.6f); return; }
            on = !on; ProcAudio.Play2D(ProcAudio.Click, 0.7f, on ? 1.2f : 0.9f);
        }

        public void Give()
        {
            owned = true; if (viewModel != null) viewModel.SetActive(true);
            on = true;
        }

        public void AddBatteries(int count) => spareBatteries += count;
    }
}
