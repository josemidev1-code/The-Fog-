using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>La puerta de la cabina: el puesto no se abandona de noche.</summary>
    public class TowerDoor : Interactable
    {
        int tries;
        public override string Prompt => "Puerta de la cabina";
        public override void Interact()
        {
            tries++;
            string[] lines =
            {
                "Al otro lado solo hay escalera, frío y niebla. Tu puesto está aquí hasta el amanecer.",
                "Apoyas la mano en el pomo. Está helado. Mejor no.",
                "No. No vas a bajar."
            };
            PuestoGame.I.hud.Toast(lines[Mathf.Min(tries - 1, lines.Length - 1)]);
        }
    }
}
