using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>Radio de la mesa: repite el último mensaje.</summary>
    public class RadioSet : Interactable
    {
        public override bool CanInteract => base.CanInteract && PuestoGame.I != null && PuestoGame.I.radio.HasLast;
        public override string Prompt => "Volver a escuchar el último aviso";
        public override void Interact() => PuestoGame.I.radio.ReplayLast();
    }
}
