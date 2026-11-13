using UnityEngine;

namespace TheFog.FirstPlayable
{
    /// <summary>
    /// Recorrido de prueba PROVISIONAL: un objetivo y un estado de completado.
    /// No representa historia ni progresión aprobadas.
    /// </summary>
    public class FogTestRoute : MonoBehaviour
    {
        public FogHud hud;
        [TextArea(1, 3)]
        public string objectiveText = "Objetivo de prueba: sigue el camino hasta la entrada de la mina y examínala.";
        [TextArea(1, 3)]
        public string completedText = "Recorrido de prueba completado. Pulsa Esc y después R para reiniciar.";

        public bool Completed { get; private set; }

        void Start()
        {
            if (hud != null) hud.SetObjective(objectiveText);
        }

        public void Complete()
        {
            if (Completed) return;
            Completed = true;
            if (hud != null) hud.SetObjective(completedText);
        }
    }
}
