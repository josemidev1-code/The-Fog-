using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace TheFog.Puesto
{
    /// <summary>Luz de luna nocturna que vira a amanecer en el último tramo del turno.</summary>
    public class SkyCycle : MonoBehaviour
    {
        public Light moon;
        public Color moonColor = new Color(0.55f, 0.64f, 0.70f), sunColor = new Color(1f, 0.82f, 0.62f);
        public float moonIntensity = 0.35f, sunIntensity = 1.3f;
        public Color nightAmbient = new Color(0.10f, 0.12f, 0.12f), dawnAmbient = new Color(0.42f, 0.40f, 0.36f);

        void Update()
        {
            var g = PuestoGame.I; if (g == null || moon == null) return;
            float d = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.86f, 1f, g.clock.Progress));
            moon.color = Color.Lerp(moonColor, sunColor, d);
            moon.intensity = Mathf.Lerp(moonIntensity, sunIntensity, d);
            moon.transform.rotation = Quaternion.Euler(Mathf.Lerp(38f, 9f, d), Mathf.Lerp(-30f, 105f, d), 0);
            RenderSettings.ambientLight = Color.Lerp(nightAmbient, dawnAmbient, d);
        }
    }
}
