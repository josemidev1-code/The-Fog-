using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>Ambiente sonoro: viento exterior y latido cuando la niebla está muy cerca.</summary>
    public class PuestoAmbience : MonoBehaviour
    {
        AudioSource wind, heart;

        void Start()
        {
            wind = ProcAudio.Loop(gameObject, ProcAudio.Wind, 0.35f, false);
            heart = ProcAudio.Loop(gameObject, ProcAudio.Heart, 0f, false);
        }

        void Update()
        {
            var g = PuestoGame.I; if (g == null) return;
            float fog = g.fog.Displayed;
            wind.volume = Mathf.Lerp(0.25f, 0.55f, fog) * (1f - g.fog.DawnFactor * 0.7f);
            float danger = Mathf.Clamp01((fog - 0.7f) / 0.3f);
            heart.volume = g.State == GameState.Ended ? 0f : danger * 0.8f;
            heart.pitch = Mathf.Lerp(0.85f, 1.35f, danger);
        }
    }
}
