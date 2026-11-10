using UnityEngine;

namespace TheFog.FirstPlayable
{
    /// <summary>Linterna con encendido y apagado. El modelo OBJ_Linterna se mantiene visible; solo cambia la luz.</summary>
    public class FogFlashlight : MonoBehaviour
    {
        public FogInput input;
        [Tooltip("Luz focal de la linterna.")]
        public Light beam;
        public bool startsOn = true;
        [Tooltip("Mientras la pausa está activa no se puede alternar.")]
        public bool controlEnabled = true;

        public bool IsOn { get; private set; }

        void Start()
        {
            SetOn(startsOn);
        }

        void Update()
        {
            if (input == null || !controlEnabled) return;
            if (input.FlashlightPressed) SetOn(!IsOn);
        }

        public void SetOn(bool on)
        {
            IsOn = on;
            if (beam != null) beam.enabled = on;
        }
    }
}
