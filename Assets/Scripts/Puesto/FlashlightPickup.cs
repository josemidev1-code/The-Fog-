using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>La linterna sobre la mesa (modelo OBJ_Linterna).</summary>
    public class FlashlightPickup : Interactable
    {
        public override string Prompt => "Coger la linterna";
        public override void Interact()
        {
            var g = PuestoGame.I;
            g.flashlight.Give();
            g.hud.Toast("Linterna de petaca. [F] para encenderla. Las pilas ya venían gastadas.");
            ProcAudio.Play2D(ProcAudio.Click, 0.8f);
            g.radio.OnFlashlightTaken();
            gameObject.SetActive(false);
        }
    }
}
