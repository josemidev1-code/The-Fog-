using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace TheFog.Puesto
{
    /// <summary>Turno de 22:00 a 06:00 comprimido en unos minutos reales.</summary>
    public class ShiftClock : MonoBehaviour
    {
        public float shiftMinutes = 11f;
        public float startHour = 22f, endHour = 30f;
        public bool running;
        public float elapsed;

        public float Duration => shiftMinutes * 60f;
        public float Progress => Mathf.Clamp01(elapsed / Duration);
        /// <summary>Hora continua: 22..30 (24 = medianoche, 27 = 03:00).</summary>
        public float HourRaw => Mathf.Lerp(startHour, endHour, Progress);
        public string Text { get { float h = HourRaw % 24f; int hh = Mathf.FloorToInt(h); int mm = Mathf.FloorToInt((h - hh) * 60f); return $"{hh:00}:{mm:00}"; } }

        void Update()
        {
            var g = PuestoGame.I;
            if (!running || g == null || g.State == GameState.Paused || g.State == GameState.Ended) return;
            elapsed += Time.deltaTime;
            if (elapsed >= Duration) g.Win();
        }

        public void SetHour(float hourRaw) { elapsed = Mathf.InverseLerp(startHour, endHour, hourRaw) * Duration; }
    }
}
