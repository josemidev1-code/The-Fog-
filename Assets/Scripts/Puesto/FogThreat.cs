using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace TheFog.Puesto
{
    /// <summary>Nivel global de niebla: sube con el tiempo, con los apagones y con lo que no se registra. A 100% se pierde.</summary>
    public class FogThreat : MonoBehaviour
    {
        [Range(0, 1)] public float level = 0.08f;
        public float baseRisePerSecond = 0.00075f;
        public float blackoutMultiplier = 3.5f;
        public float minDensity = 0.022f, maxDensity = 0.085f;
        public Color nightColor = new Color(0.20f, 0.23f, 0.22f), denseColor = new Color(0.33f, 0.36f, 0.33f), dawnColor = new Color(0.62f, 0.60f, 0.55f);
        public ParticleSystem[] banks;
        public Transform bankRing;
        public float ringFar = 1f, ringNear = 0.35f;
        public Volume volume;

        public float Displayed { get; private set; }
        public float DawnFactor { get; private set; }
        Vignette vignette;
        float baseVignette;

        void Start()
        {
            if (volume != null && volume.profile.TryGet(out vignette)) baseVignette = vignette.intensity.value;
            Displayed = level;
            Apply();
        }

        public void Add(float amount)
        {
            if (DawnFactor > 0.5f && amount > 0) return;
            level = Mathf.Clamp01(level + amount);
        }

        void Update()
        {
            var g = PuestoGame.I; if (g == null) return;
            bool live = g.clock.running && g.State != GameState.Paused && g.State != GameState.Ended;
            DawnFactor = Mathf.InverseLerp(0.9f, 1f, g.clock.Progress);
            if (live && DawnFactor <= 0f)
                level = Mathf.Clamp01(level + baseRisePerSecond * Time.deltaTime * (g.power.On ? 1f : blackoutMultiplier));
            Displayed = Mathf.MoveTowards(Displayed, level, Time.deltaTime * 0.08f);
            Apply();
            if (live && level >= 1f && Displayed >= 0.995f)
                g.Lose("La niebla ha cubierto la torre. Desde dentro ya no se ve nada de Valdara.\nNadie sabrá lo que ocurrió esta noche.");
        }

        void Apply()
        {
            float k = Displayed * Displayed * 0.6f + Displayed * 0.4f;
            float density = Mathf.Lerp(minDensity, maxDensity, k);
            Color c = Color.Lerp(nightColor, denseColor, k);
            density = Mathf.Lerp(density, minDensity * 0.35f, DawnFactor);
            c = Color.Lerp(c, dawnColor, DawnFactor);
            RenderSettings.fogDensity = density; RenderSettings.fogColor = c;
            var cam = Camera.main; if (cam != null) cam.backgroundColor = c * 0.9f;
            if (bankRing != null) bankRing.localScale = Vector3.one * Mathf.Lerp(ringFar, ringNear, k) * Mathf.Lerp(1f, 1.6f, DawnFactor);
            if (banks != null)
                foreach (var b in banks)
                {
                    if (b == null) continue;
                    var main = b.main; main.startColor = new Color(c.r * 1.25f, c.g * 1.25f, c.b * 1.25f, Mathf.Lerp(0.35f, 0.7f, k) * (1f - DawnFactor));
                }
            if (vignette != null) vignette.intensity.value = baseVignette + Mathf.Clamp01((Displayed - 0.6f) / 0.4f) * (0.18f + 0.04f * Mathf.Sin(Time.time * 4f));
        }
    }
}
