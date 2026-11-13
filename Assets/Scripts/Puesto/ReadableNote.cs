using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>Papel o documento que se puede leer.</summary>
    public class ReadableNote : Interactable
    {
        public string title;
        [TextArea(4, 12)] public string body;
        public override string Prompt => "Leer: " + title;
        public override void Interact() { ProcAudio.Play2D(ProcAudio.Click, 0.4f, 1.5f); PuestoGame.I.BeginReading(title, body); }
    }
}
