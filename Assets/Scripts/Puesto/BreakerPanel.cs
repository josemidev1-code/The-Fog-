using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>Panel eléctrico junto a la puerta: mantener E para rearmar el diferencial.</summary>
    public class BreakerPanel : Interactable
    {
        public Transform lever;
        public Vector3 leverOnEuler = new Vector3(-35, 0, 0), leverOffEuler = new Vector3(35, 0, 0);
        public Renderer statusLamp;

        void Awake() { holdSeconds = 1.6f; }
        public override bool CanInteract => base.CanInteract && PuestoGame.I != null && !PuestoGame.I.power.On;
        public override string Prompt => "Subir el diferencial";
        public override void Interact() => PuestoGame.I.power.Restore();

        void Update()
        {
            var g = PuestoGame.I; if (g == null) return;
            if (lever != null) lever.localRotation = Quaternion.Slerp(lever.localRotation, Quaternion.Euler(g.power.On ? leverOnEuler : leverOffEuler), Time.deltaTime * 12f);
            if (statusLamp != null)
            {
                var c = g.power.On ? new Color(0.2f, 1f, 0.3f) : (Mathf.Repeat(Time.time, 0.8f) < 0.4f ? new Color(1f, 0.1f, 0.05f) : Color.black);
                statusLamp.material.SetColor("_BaseColor", c); statusLamp.material.SetColor("_EmissionColor", c * 2f);
            }
        }
    }
}
