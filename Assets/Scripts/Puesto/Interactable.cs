using UnityEngine;
using UnityEngine.InputSystem;

namespace TheFog.Puesto
{
    /// <summary>Base de todo objeto con el que Daniel puede interactuar mirando y pulsando E.</summary>
    public abstract class Interactable : MonoBehaviour
    {
        [Tooltip("Segundos que hay que mantener E. 0 = pulsación simple.")]
        public float holdSeconds;
        public virtual bool CanInteract => isActiveAndEnabled;
        public abstract string Prompt { get; }
        public abstract void Interact();
    }
}
