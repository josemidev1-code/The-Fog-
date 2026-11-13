using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>Mueble que se puede registrar una vez: puede contener pilas y una nota.</summary>
    public class SearchSpot : Interactable
    {
        public string label = "Registrar";
        public int batteries;
        [TextArea] public string foundText = "Encuentras un paquete de pilas.";
        [TextArea] public string emptyText = "No hay nada útil.";
        public string noteTitle;
        [TextArea(4, 12)] public string noteBody;
        public bool searched;

        public override bool CanInteract => base.CanInteract && !searched;
        public override string Prompt => label;

        public override void Interact()
        {
            var g = PuestoGame.I;
            searched = true;
            ProcAudio.PlayAt(ProcAudio.Step, transform.position, 0.6f, 8f);
            if (batteries > 0) { g.flashlight.AddBatteries(batteries); g.hud.Toast(foundText + "  (Pilas de repuesto: " + g.flashlight.spareBatteries + ")"); }
            else g.hud.Toast(emptyText);
            if (!string.IsNullOrEmpty(noteTitle)) g.BeginReading(noteTitle, noteBody);
        }
    }
}
