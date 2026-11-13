using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>Radio de la torre: mensajes por horas y avisos de vecinos, con subtítulos y estática.</summary>
    public class RadioSystem : MonoBehaviour
    {
        [System.Serializable]
        public class Beat { public float hour; public string speaker; [TextArea] public string text; public string objective; [System.NonSerialized] public bool fired; }

        public List<Beat> beats = new List<Beat>();
        public Transform speakerPoint;

        struct Line { public string speaker, text; }
        readonly Queue<Line> queue = new Queue<Line>();
        Line last; bool hasLast; float busyUntil;
        bool flashlightTaken, consoleUsed, knocked;

        public bool HasLast => hasLast;

        void Update()
        {
            var g = PuestoGame.I; if (g == null || g.State == GameState.Paused || g.State == GameState.Ended) return;
            if (g.clock.running)
                foreach (var b in beats)
                    if (!b.fired && g.clock.HourRaw >= b.hour)
                    {
                        b.fired = true;
                        if (!string.IsNullOrEmpty(b.text)) Say(b.speaker, b.text, false);
                        if (!string.IsNullOrEmpty(b.objective)) g.hud.SetObjective(b.objective);
                    }
            // Tres golpes en la puerta, una sola vez en la noche.
            if (!knocked && g.clock.HourRaw >= 25.6f && g.State == GameState.Playing)
            {
                knocked = true;
                var door = FindAnyObjectByType<TowerDoor>();
                if (door != null) ProcAudio.PlayAt(ProcAudio.Knock, door.transform.position, 1f, 14f);
            }
            if (Time.time >= busyUntil && queue.Count > 0) Show(queue.Dequeue());
        }

        public void Say(string speaker, string text, bool urgent)
        {
            var line = new Line { speaker = speaker, text = text };
            if (urgent) { queue.Clear(); Show(line); } else queue.Enqueue(line);
        }

        void Show(Line line)
        {
            last = line; hasLast = true;
            float duration = Mathf.Clamp(2.5f + line.text.Length * 0.055f, 4f, 11f);
            busyUntil = Time.time + duration + 0.6f;
            PuestoGame.I.hud.ShowSubtitle(line.speaker, line.text, duration);
            Vector3 at = speakerPoint != null ? speakerPoint.position : transform.position;
            ProcAudio.PlayAt(ProcAudio.Static, at, 0.5f, 10f);
            ProcAudio.PlayAt(ProcAudio.Beep, at, 0.35f, 10f);
        }

        public void ReplayLast() { if (hasLast) Show(last); }

        public void OnFlashlightTaken()
        {
            if (flashlightTaken) return; flashlightTaken = true;
            if (!consoleUsed) PuestoGame.I.hud.SetObjective("Ocupa tu sitio frente al monitor, en la mesa que mira a la mina.");
        }

        public void OnConsoleUsed()
        {
            if (consoleUsed) return; consoleUsed = true;
            PuestoGame.I.hud.SetObjective("Vigila las cuatro cámaras. Anota todo lo que cambie. Aguanta hasta que amanezca.");
        }
    }
}
