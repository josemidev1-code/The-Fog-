using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace TheFog.Puesto
{
    /// <summary>Electricidad de la torre: luces, monitor y cámaras. El diferencial salta y hay que rearmarlo en el panel.</summary>
    public class TowerPower : MonoBehaviour
    {
        public Light[] interiorLights;
        public Renderer[] emissiveRenderers;
        public Light emergencyLight;
        public Transform panel;
        [Tooltip("Material de las bombillas sin corriente. Si falta, las bombillas se ocultan (comportamiento antiguo).")]
        public Material bulbOffMaterial;
        [Tooltip("Piloto rojo de emergencia: solo brilla durante el apagón.")]
        public Renderer emergencyLamp;
        public Material emergencyOnMaterial, emergencyOffMaterial;
        public float scriptedTripHour = 27.1f; // 03:06
        public int maxRandomTrips = 2;
        public float randomTripChancePerCheck = 0.2f;

        public bool On { get; private set; } = true;
        public int Trips { get; private set; }

        float[] baseIntensity;
        Material[] bulbOnMaterials;
        AudioSource hum;
        bool scriptedDone;
        int randomTrips;
        float nextCheck = 30f, lastRestore = -999f;

        void Start()
        {
            baseIntensity = new float[interiorLights.Length];
            for (int i = 0; i < interiorLights.Length; i++) baseIntensity[i] = interiorLights[i].intensity;
            bulbOnMaterials = new Material[emissiveRenderers != null ? emissiveRenderers.Length : 0];
            for (int i = 0; i < bulbOnMaterials.Length; i++) if (emissiveRenderers[i] != null) bulbOnMaterials[i] = emissiveRenderers[i].sharedMaterial;
            SetEmergencyLamp(false);
            hum = ProcAudio.Loop(gameObject, ProcAudio.Hum, 0.12f, true, 9f);
            if (emergencyLight != null) emergencyLight.enabled = false;
        }

        void Update()
        {
            var g = PuestoGame.I; if (g == null || !g.clock.running || g.State == GameState.Ended || g.State == GameState.Paused) return;
            if (On)
            {
                if (!scriptedDone && g.clock.HourRaw >= scriptedTripHour) { scriptedDone = true; Trip(); return; }
                if (g.clock.elapsed >= nextCheck)
                {
                    nextCheck = g.clock.elapsed + 30f;
                    if (g.clock.HourRaw > 24.5f && randomTrips < maxRandomTrips && Time.time - lastRestore > 70f && Random.value < randomTripChancePerCheck) { randomTrips++; Trip(); return; }
                }
                // Con niebla densa la instalación parpadea: aviso diegético del peligro.
                float unstable = Mathf.Clamp01((g.fog.Displayed - 0.55f) / 0.45f);
                for (int i = 0; i < interiorLights.Length; i++)
                    interiorLights[i].intensity = baseIntensity[i] * (Random.value < unstable * 0.06f ? Random.Range(0.1f, 0.6f) : 1f);
            }
            else if (emergencyLight != null)
            {
                emergencyLight.intensity = 1.6f + 0.5f * Mathf.Sin(Time.time * 3.1f);
            }
        }

        public void Trip()
        {
            if (!On) return;
            On = false; Trips++;
            StartCoroutine(FlickerOut());
            hum.Stop();
            ProcAudio.PlayAt(ProcAudio.Click, transform.position, 1f, 12f);
            ProcAudio.Play2D(ProcAudio.Alarm, 0.25f);
            var g = PuestoGame.I;
            if (g.State == GameState.Console) g.console.ForceExit();
            g.hud.Toast("Se ha ido la luz. Sin corriente no hay cámaras, y a oscuras la niebla corre.");
            g.hud.SetObjective("Sube el diferencial. El cuadro está junto a la puerta.");
        }

        IEnumerator FlickerOut()
        {
            for (int k = 0; k < 6; k++)
            {
                SetLights(k % 2 == 1);
                yield return new WaitForSeconds(Random.Range(0.04f, 0.13f));
            }
            SetLights(false);
            if (emergencyLight != null && !On) emergencyLight.enabled = true;
            if (!On) SetEmergencyLamp(true);
        }

        void SetLights(bool on)
        {
            for (int i = 0; i < interiorLights.Length; i++) { interiorLights[i].enabled = on; interiorLights[i].intensity = baseIntensity[i]; }
            if (emissiveRenderers == null) return;
            for (int i = 0; i < emissiveRenderers.Length; i++)
            {
                var r = emissiveRenderers[i]; if (r == null) continue;
                // La bombilla se queda en su sitio, solo se apaga (antes desaparecía del todo).
                if (bulbOffMaterial != null && bulbOnMaterials != null && i < bulbOnMaterials.Length) { r.enabled = true; r.sharedMaterial = on ? bulbOnMaterials[i] : bulbOffMaterial; }
                else r.enabled = on;
            }
        }

        void SetEmergencyLamp(bool on)
        {
            if (emergencyLamp == null) return;
            var m = on ? emergencyOnMaterial : emergencyOffMaterial;
            if (m != null) emergencyLamp.sharedMaterial = m;
        }

        public void Restore()
        {
            if (On) return;
            StopAllCoroutines();
            On = true; lastRestore = Time.time;
            SetLights(true);
            if (emergencyLight != null) emergencyLight.enabled = false;
            SetEmergencyLamp(false);
            hum.Play();
            ProcAudio.PlayAt(ProcAudio.Click, transform.position, 1f, 12f);
            PuestoGame.I.hud.Toast("Vuelve la corriente. El monitor tarda un momento en calentar.");
            PuestoGame.I.hud.SetObjective(PuestoGame.I.console != null ? "Vigila las cuatro cámaras. Anota todo lo que cambie. Aguanta hasta que amanezca." : "");
        }
    }
}
