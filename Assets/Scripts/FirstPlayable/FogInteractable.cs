using UnityEngine;
using UnityEngine.Events;

namespace TheFog.FirstPlayable
{
    /// <summary>
    /// Objeto con el que se puede interactuar. PROVISIONAL: solo muestra un texto en el HUD,
    /// opcionalmente completa el recorrido de prueba y lanza un UnityEvent editable.
    /// Necesita un Collider (puede ser trigger) en este objeto o en sus hijos.
    /// </summary>
    public class FogInteractable : MonoBehaviour
    {
        [Tooltip("Texto tras la tecla, por ejemplo: Examinar la entrada.")]
        public string prompt = "Examinar";
        [TextArea(2, 5)]
        [Tooltip("Mensaje provisional que aparece en el HUD al interactuar.")]
        public string message = "";
        public float messageSeconds = 5f;
        [Tooltip("Si está marcado, solo se puede usar una vez.")]
        public bool singleUse;
        [Tooltip("Si se asigna, esta interacción completa el recorrido de prueba.")]
        public FogTestRoute completesRoute;
        public UnityEvent onInteract = new UnityEvent();

        public bool Used { get; private set; }
        public bool CanInteract { get { return isActiveAndEnabled && !(singleUse && Used); } }

        public void Interact(FogHud hud)
        {
            if (!CanInteract) return;
            Used = true;
            if (hud != null && !string.IsNullOrEmpty(message)) hud.ShowMessage(message, messageSeconds);
            if (completesRoute != null) completesRoute.Complete();
            if (onInteract != null) onInteract.Invoke();
        }
    }
}
