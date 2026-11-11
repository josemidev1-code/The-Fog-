using UnityEngine;

namespace TheFog.FirstPlayable
{
    /// <summary>Detecta con un rayo desde la cámara el FogInteractable que se mira y lo activa con la acción Interact.</summary>
    public class FogInteractor : MonoBehaviour
    {
        public FogInput input;
        public FogHud hud;
        [Tooltip("Origen y dirección del rayo; normalmente la cámara.")]
        public Transform rayOrigin;
        public float distance = 3f;
        public LayerMask layers = ~0;
        public string keyLabel = "E";
        public bool controlEnabled = true;

        FogInteractable current;

        void Update()
        {
            current = controlEnabled ? Find() : null;
            // El texto puede cambiar sin cambiar el objeto (por ejemplo, una puerta).
            RefreshPrompt();

            if (current != null && input != null && input.InteractPressed)
            {
                current.Interact(hud);
                if (current == null || !current.CanInteract)
                {
                    current = null;
                }
                RefreshPrompt();
            }
        }

        void RefreshPrompt()
        {
            if (hud != null) hud.SetPrompt(current != null ? "[" + keyLabel + "] " + current.prompt : "");
        }

        FogInteractable Find()
        {
            Transform origin = rayOrigin != null ? rayOrigin : transform;
            RaycastHit hit;
            if (Physics.Raycast(origin.position, origin.forward, out hit, distance, layers, QueryTriggerInteraction.Collide))
            {
                FogInteractable target = hit.collider.GetComponentInParent<FogInteractable>();
                if (target != null && target.CanInteract) return target;
            }
            return null;
        }

        void OnDisable()
        {
            current = null;
            if (hud != null) hud.SetPrompt("");
        }
    }
}
