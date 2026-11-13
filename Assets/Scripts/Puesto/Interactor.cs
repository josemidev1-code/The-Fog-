using System.Collections;
using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>Detecta lo que Daniel mira y gestiona pulsar o mantener E.</summary>
    public class Interactor : MonoBehaviour
    {
        public Camera cam;
        public float range = 2.4f;
        public LayerMask mask = ~(1 << 2);

        public Interactable Current { get; private set; }
        float hold;

        void Update()
        {
            var g = PuestoGame.I;
            if (g == null || g.State != GameState.Playing || g.player.Posed) { Current = null; hold = 0; if (g != null) g.hud.SetPrompt(null, 0); return; }

            Interactable found = null;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, range, mask, QueryTriggerInteraction.Collide))
                found = hit.collider.GetComponentInParent<Interactable>();
            if (found != null && !found.CanInteract) found = null;
            if (found != Current) hold = 0;
            Current = found;

            if (Current == null) { g.hud.SetPrompt(null, 0); return; }
            if (Current.holdSeconds > 0)
            {
                if (PInput.InteractHeld) hold += Time.deltaTime; else hold = 0;
                g.hud.SetPrompt("Mantén [E]  " + Current.Prompt, hold / Current.holdSeconds);
                if (hold >= Current.holdSeconds) { hold = 0; Current.Interact(); }
            }
            else
            {
                g.hud.SetPrompt("[E]  " + Current.Prompt, 0);
                if (PInput.Interact && !g.ChangedThisFrame) Current.Interact();
            }
        }
    }
}
