using UnityEngine;
using UnityEngine.UI;

namespace TheFog.FirstPlayable
{
    /// <summary>HUD sencillo: controles, objetivo, indicación de interacción, mensajes y panel de pausa.</summary>
    public class FogHud : MonoBehaviour
    {
        public Text controlsText;
        public Text objectiveText;
        public Text promptText;
        public Text messageText;
        public GameObject crosshair;
        public GameObject pausePanel;

        float messageUntil = -1f;

        void Awake()
        {
            SetPrompt("");
            if (messageText != null) messageText.text = "";
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        void Update()
        {
            if (messageUntil >= 0f && Time.unscaledTime >= messageUntil)
            {
                messageUntil = -1f;
                if (messageText != null) messageText.text = "";
            }
        }

        public void SetObjective(string text)
        {
            if (objectiveText != null) objectiveText.text = text;
        }

        public void SetPrompt(string text)
        {
            if (promptText != null) promptText.text = text;
        }

        public void ShowMessage(string text, float seconds)
        {
            if (messageText == null) return;
            messageText.text = text;
            messageUntil = Time.unscaledTime + Mathf.Max(0.5f, seconds);
        }

        public void SetPaused(bool paused)
        {
            if (pausePanel != null) pausePanel.SetActive(paused);
            if (crosshair != null) crosshair.SetActive(!paused);
            if (promptText != null) promptText.enabled = !paused;
        }
    }
}
