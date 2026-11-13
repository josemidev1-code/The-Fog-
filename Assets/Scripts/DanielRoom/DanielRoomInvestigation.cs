using UnityEngine;
using UnityEngine.UI;
namespace TheFog.DanielRoom
{
    [DefaultExecutionOrder(-20)]
    public class DanielRoomInvestigation : MonoBehaviour
    {
        public DanielTerminal terminal;
        public GameObject panel;
        public Text heading, body;
        public bool Reading { get; private set; }
        void Update()
        {
            if(terminal.InTerminal || terminal.Transitioning) return;
            if(Reading)
            {
                if(terminal.input.InteractPressed || terminal.input.PausePressed) Close();
                return;
            }
            if(Physics.Raycast(terminal.view.transform.position,terminal.view.transform.forward,out var hit,2.5f))
            {
                var clue=hit.collider.GetComponentInParent<DanielRoomClue>();
                if(clue!=null)
                {
                    terminal.hint.text="E · EXAMINAR " + clue.title;
                    if(terminal.input.InteractPressed) Examine(clue);
                }
            }
        }
        public void Examine(DanielRoomClue clue)
        {
            Reading=true;panel.SetActive(true);heading.text=clue.title;body.text=clue.body;
            terminal.SetReading(true);terminal.hint.text="E / ESC · DEJAR DE EXAMINAR";
        }
        public void Close()
        {
            Reading=false;panel.SetActive(false);terminal.SetReading(false);
        }
    }
}
