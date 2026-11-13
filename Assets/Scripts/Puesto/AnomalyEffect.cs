using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>Efecto visible de una anomalía. Reversible: al terminar todo vuelve a su sitio.</summary>
    public class AnomalyEffect : MonoBehaviour
    {
        public AnomalyKind kind;
        public string description = "Algo no encaja";
        [TextArea] public string radioHint;
        public Light[] lights;
        public GameObject[] toggles;
        public ParticleSystem particles;
        public Transform target;
        public Vector3 displacedLocalPosition;
        public Vector3 displacedLocalEuler;
        public float sweepAngle = 35f;

        Vector3 origPos; Quaternion origRot;
        float[] origIntensity;
        bool active; float t;

        void Awake()
        {
            if (target != null) { origPos = target.localPosition; origRot = target.localRotation; }
            if (lights != null) { origIntensity = new float[lights.Length]; for (int i = 0; i < lights.Length; i++) origIntensity[i] = lights[i].intensity; }
            if (toggles != null) foreach (var go in toggles) if (go != null) go.SetActive(false);
        }

        public void Begin()
        {
            active = true; t = 0;
            switch (kind)
            {
                case AnomalyKind.WindowLight:
                case AnomalyKind.Glow:
                case AnomalyKind.Beam:
                    foreach (var go in toggles) go.SetActive(true);
                    break;
                case AnomalyKind.FogBank:
                    particles.Play(true);
                    break;
                case AnomalyKind.Displaced:
                    target.localPosition = displacedLocalPosition; target.localRotation = Quaternion.Euler(displacedLocalEuler);
                    ProcAudio.PlayAt(ProcAudio.Knock, target.position, 0.7f, 30f);
                    break;
            }
        }

        public void End()
        {
            active = false;
            if (lights != null) for (int i = 0; i < lights.Length; i++) { lights[i].enabled = true; lights[i].intensity = origIntensity[i]; }
            if (toggles != null) foreach (var go in toggles) go.SetActive(false);
            if (particles != null) particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (target != null && kind != AnomalyKind.Beam) { target.localPosition = origPos; target.localRotation = origRot; }
        }

        void Update()
        {
            if (!active) return;
            t += Time.deltaTime;
            switch (kind)
            {
                case AnomalyKind.LightOut:
                    // Parpadeo nervioso durante unos segundos y luego oscuridad.
                    for (int i = 0; i < lights.Length; i++)
                    {
                        bool flick = t < 4f ? Random.value > 0.45f : (Random.value < 0.01f);
                        lights[i].enabled = flick; lights[i].intensity = origIntensity[i] * Random.Range(0.3f, 1f);
                    }
                    break;
                case AnomalyKind.WindowLight:
                    for (int i = 0; i < lights.Length; i++) lights[i].intensity = origIntensity[i] * (0.85f + 0.15f * Mathf.PerlinNoise(t * 3f, i));
                    break;
                case AnomalyKind.Glow:
                    for (int i = 0; i < lights.Length; i++) lights[i].intensity = origIntensity[i] * (0.55f + 0.45f * Mathf.Sin(t * 1.7f));
                    break;
                case AnomalyKind.Beam:
                    if (target != null) target.localRotation = origRot * Quaternion.Euler(0, Mathf.Sin(t * 0.8f) * sweepAngle, 0);
                    break;
            }
        }
    }

    public enum AnomalyKind { LightOut, WindowLight, FogBank, Displaced, Beam, Glow }
}
