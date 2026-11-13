using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TheFog.FirstPlayable
{
    /// <summary>
    /// Pausa y captura del cursor. Esc pausa o reanuda; en pausa, R reinicia la escena y Q sale.
    /// Si el cursor se libera sin pausa (por ejemplo, al perder el foco), un clic lo vuelve a capturar.
    /// </summary>
    public class FogPauseController : MonoBehaviour
    {
        public FogInput input;
        public FogHud hud;
        public FogPlayerController player;
        public FogFlashlight flashlight;
        public FogInteractor interactor;

        public bool Paused { get; private set; }

        void Start()
        {
            SetPaused(false);
        }

        void Update()
        {
            if (input == null) return;

            if (input.PausePressed)
            {
                SetPaused(!Paused);
                return;
            }

            if (Paused)
            {
                if (input.RestartPressed) Restart();
                else if (input.QuitPressed) Quit();
            }
            else if (Cursor.lockState != CursorLockMode.Locked)
            {
                Mouse mouse = Mouse.current;
                if (mouse != null && mouse.leftButton.wasPressedThisFrame) CaptureCursor(true);
            }
        }

        public void SetPaused(bool paused)
        {
            Paused = paused;
            Time.timeScale = paused ? 0f : 1f;
            CaptureCursor(!paused);
            if (player != null) player.controlEnabled = !paused;
            if (flashlight != null) flashlight.controlEnabled = !paused;
            if (interactor != null) interactor.controlEnabled = !paused;
            if (hud != null) hud.SetPaused(paused);
        }

        static void CaptureCursor(bool capture)
        {
            Cursor.lockState = capture ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !capture;
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0) SceneManager.LoadScene(scene.buildIndex);
            else SceneManager.LoadScene(scene.name);
        }

        public void Quit()
        {
            Time.timeScale = 1f;
            CaptureCursor(false);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
