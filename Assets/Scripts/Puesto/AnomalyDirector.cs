using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>Decide cuándo y dónde aparecen las anomalías, y resuelve registros, fallos y falsas alarmas.</summary>
    public class AnomalyDirector : MonoBehaviour
    {
        public AnomalySite[] sites;
        public float firstAnomalyAt = 32f;
        public float intervalStart = 46f, intervalEnd = 19f;
        public float lifetimeStart = 55f, lifetimeEnd = 38f;
        public float missPenalty = 0.13f, falsePenalty = 0.05f, reportReward = 0.035f;

        float next;
        bool firstDone;

        void Start() { next = firstAnomalyAt; }

        void Update()
        {
            var g = PuestoGame.I; if (g == null || !g.clock.running || g.State == GameState.Ended || g.State == GameState.Paused) return;
            float p = g.clock.Progress;
            float lifetime = Mathf.Lerp(lifetimeStart, lifetimeEnd, p);
            foreach (var s in sites)
            {
                if (!s.IsActive) continue;
                s.ActiveTime += Time.deltaTime;
                if (s.ActiveTime > lifetime) Miss(s);
            }
            if (g.clock.elapsed >= next && p < 0.94f)
            {
                Spawn(firstDone ? -1 : 0);
                next = g.clock.elapsed + Mathf.Lerp(intervalStart, intervalEnd, p) * Random.Range(0.8f, 1.2f);
            }
        }

        public int ActiveCount { get { int n = 0; foreach (var s in sites) if (s.IsActive) n++; return n; } }

        /// <summary>Lanza una anomalía. siteIndex = -1 elige un lugar libre al azar.</summary>
        public AnomalySite Spawn(int siteIndex = -1, int effectIndex = -1)
        {
            var g = PuestoGame.I;
            int maxConcurrent = g.clock.Progress < 0.3f ? 1 : g.clock.Progress < 0.7f ? 2 : 3;
            if (siteIndex < 0 && ActiveCount >= maxConcurrent) return null;
            AnomalySite site = null;
            if (siteIndex >= 0) site = sites[siteIndex];
            else
            {
                var free = new List<AnomalySite>();
                foreach (var s in sites) if (s.Active == null && Time.time - s.EndedAt > 10f) free.Add(s);
                if (free.Count == 0) return null;
                site = free[Random.Range(0, free.Count)];
            }
            if (site.Active != null) return null;
            AnomalyEffect effect;
            if (effectIndex >= 0) effect = site.effects[effectIndex];
            else
            {
                var options = new List<AnomalyEffect>(site.effects);
                if (options.Count > 1 && site.LastEffect != null) options.Remove(site.LastEffect);
                effect = options[Random.Range(0, options.Count)];
            }
            site.Begin(effect);
            bool first = !firstDone; firstDone = true;
            if (first) g.radio.Say("Vecina (radio)", effect.radioHint + " ...Torre, ¿me oye? Mire la cámara, por favor.", true);
            else if (!string.IsNullOrEmpty(effect.radioHint) && Random.value < 0.4f) g.radio.Say("Vecino (radio)", effect.radioHint, false);
            if (g.State == GameState.Console && g.console.Current.site != site) ProcAudio.Play2D(ProcAudio.Static, 0.1f, 0.8f);
            return site;
        }

        public void Report(AnomalySite site)
        {
            var g = PuestoGame.I;
            if (site.IsActive)
            {
                site.Reported = true; g.Registered++;
                g.fog.Add(-reportReward);
                g.hud.Toast(site.label + " — " + site.Active.description + ". Anotado en el parte.");
                g.hud.FlashReport(true);
                ProcAudio.Play2D(ProcAudio.Beep, 0.3f, 1.5f);
                StartCoroutine(EndLater(site, 2.5f));
            }
            else
            {
                g.FalseReports++;
                g.fog.Add(falsePenalty);
                g.hud.Toast(site.label + " — sin novedad. Aviso en falso: también consta en el parte.");
                g.hud.FlashReport(false);
                ProcAudio.Play2D(ProcAudio.Beep, 0.3f, 0.6f);
            }
        }

        IEnumerator EndLater(AnomalySite site, float delay)
        {
            yield return new WaitForSeconds(delay);
            site.End();
        }

        void Miss(AnomalySite site)
        {
            var g = PuestoGame.I;
            g.Missed++;
            g.fog.Add(missPenalty);
            g.hud.Toast(site.label + " — se te ha pasado algo. La niebla gana terreno.");
            ProcAudio.Play2D(ProcAudio.Sting, 0.55f);
            site.End();
        }
    }
}
