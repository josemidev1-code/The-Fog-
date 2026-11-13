using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>Un lugar vigilado por una cámara. Solo puede tener una anomalía activa.</summary>
    public class AnomalySite : MonoBehaviour
    {
        public string label;
        public AnomalyEffect[] effects;
        public AnomalyEffect Active { get; private set; }
        public float ActiveTime { get; set; }
        public float StartedAt { get; private set; } = -100f;
        public float EndedAt { get; private set; } = -100f;
        public bool Reported { get; set; }
        public AnomalyEffect LastEffect { get; private set; }

        public bool IsActive => Active != null && !Reported;

        public void Begin(AnomalyEffect e)
        {
            Active = e; LastEffect = e; ActiveTime = 0; Reported = false; StartedAt = Time.time;
            e.Begin();
        }

        public void End()
        {
            if (Active != null) Active.End();
            Active = null; Reported = false; EndedAt = Time.time;
        }
    }
}
