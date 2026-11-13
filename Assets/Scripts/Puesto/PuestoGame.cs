using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace TheFog.Puesto
{
    /// <summary>Director de la partida: estados, pausa, victoria al amanecer, derrota por niebla y reinicio.</summary>
    public class PuestoGame : MonoBehaviour
    {
        public static PuestoGame I { get; private set; }

        [Header("Sistemas")]
        public PlayerMotor player;
        public PuestoHud hud;
        public ShiftClock clock;
        public FogThreat fog;
        public TowerPower power;
        public AnomalyDirector director;
        public SecurityConsole console;
        public RadioSystem radio;
        public PlayerFlashlight flashlight;

        [Header("Introducción")]
        public float introSeconds = 9f;

        public GameState State { get; private set; } = GameState.Intro;
        public int Registered { get; set; }
        public int Missed { get; set; }
        public int FalseReports { get; set; }
        public bool Won { get; private set; }
        public static bool AutoTest { get; private set; }

        GameState beforePause;
        Coroutine introRoutine;
        int stateFrame = -1;
        bool closeReading;
        public void CloseReading() => closeReading = true;
        /// <summary>Fotograma del último cambio de estado: evita que una misma pulsación haga dos cosas.</summary>
        public bool ChangedThisFrame => stateFrame == Time.frameCount;

        void Awake()
        {
            I = this;
            AutoTest = System.Environment.CommandLine.Contains("-puestoAutotest");
            if (AutoTest) Application.runInBackground = true;
            Time.timeScale = 1f;
            PuestoSettings.Load();
        }

        void OnDestroy() { if (I == this) I = null; }

        void Start()
        {
            introRoutine = StartCoroutine(Intro());
        }

        IEnumerator Intro()
        {
            State = GameState.Intro;
            SetCursor(false);
            hud.ShowIntro(true);
            float t = 0f, length = AutoTest ? 0.5f : introSeconds;
            while (t < length)
            {
                t += Time.deltaTime;
                hud.SetIntroProgress(t / length);
                if (t > 1.2f && PInput.Confirm) break; // la introducción se puede saltar
                yield return null;
            }
            hud.ShowIntro(false);
            State = GameState.Playing;
            clock.running = true;
            hud.SetObjective(flashlight.owned ? "Ocupa tu sitio frente al monitor." : "Coge la linterna. Está en la mesa, junto al monitor.");
            SetCursor(true);
            introRoutine = null;
        }

        void Update()
        {
            switch (State)
            {
                case GameState.Playing:
                case GameState.Console:
                case GameState.Reading:
                    if (PInput.Pause && State == GameState.Playing && Time.frameCount != stateFrame) SetPaused(true);
                    break;
                case GameState.Paused:
                    if (PInput.Pause) SetPaused(false);
                    else if (PInput.AdjustX != 0) { PuestoSettings.SetVolume(PuestoSettings.Volume + 0.1f * PInput.AdjustX); hud.RefreshSettings(); }
                    else if (PInput.AdjustY != 0) { PuestoSettings.SetSensitivity(PuestoSettings.Sensitivity + 0.02f * PInput.AdjustY); hud.RefreshSettings(); }
                    else if (PInput.Restart) Restart();
                    else if (PInput.Quit) QuitGame();
                    break;
                case GameState.Ended:
                    if (PInput.Restart) Restart();
                    else if (PInput.Quit) QuitGame();
                    break;
            }
        }

        public void SetPaused(bool paused)
        {
            if (paused && State != GameState.Paused)
            {
                beforePause = State; State = GameState.Paused; Time.timeScale = 0f; AudioListener.pause = true;
                hud.ShowPause(true); SetCursor(false);
            }
            else if (!paused && State == GameState.Paused)
            {
                State = beforePause; Time.timeScale = 1f; AudioListener.pause = false;
                hud.ShowPause(false); SetCursor(true);
            }
        }

        public void EnterConsole() { if (State == GameState.Playing) { State = GameState.Console; stateFrame = Time.frameCount; } }
        public void ExitConsole() { if (State == GameState.Console) { State = GameState.Playing; stateFrame = Time.frameCount; } }

        public void BeginReading(string title, string body)
        {
            if (State != GameState.Playing) return;
            State = GameState.Reading; stateFrame = Time.frameCount; hud.ShowReading(title, body);
            StartCoroutine(ReadingRoutine());
        }

        IEnumerator ReadingRoutine()
        {
            yield return null; // evita que la misma pulsación de E cierre la nota
            while (State == GameState.Reading && !PInput.Back && !closeReading) yield return null;
            closeReading = false;
            if (State == GameState.Reading) { State = GameState.Playing; stateFrame = Time.frameCount; }
            hud.HideReading();
        }

        public void Win()
        {
            if (State == GameState.Ended) return;
            Won = true; Finish();
            string rating = Missed == 0 && FalseReports <= 1 ? "Informe impecable. Nada de lo que ocurrió esta noche se ha perdido."
                : Missed <= 2 ? "Informe con lagunas. Algo pasó en Valdara que nadie ha visto."
                : "Informe incompleto. Demasiadas horas sin vigilancia.";
            hud.ShowEnd(true, "AMANECE EN VALDARA", "La luz del sol frena la niebla. Has aguantado el turno.\n" + rating, Stats());
        }

        public void Lose(string reason)
        {
            if (State == GameState.Ended) return;
            Won = false; Finish();
            hud.ShowEnd(false, "LA NIEBLA", reason, Stats());
        }

        string Stats() => $"Hora: {clock.Text}    Anomalías registradas: {Registered}    Sin registrar: {Missed}    Falsas alarmas: {FalseReports}";

        void Finish()
        {
            if (introRoutine != null) StopCoroutine(introRoutine);
            if (State == GameState.Console) console.ForceExit();
            if (State == GameState.Paused) { Time.timeScale = 1f; AudioListener.pause = false; hud.ShowPause(false); }
            hud.HideReading();
            State = GameState.Ended; clock.running = false; SetCursor(false);
        }

        public void Restart()
        {
            Time.timeScale = 1f; AudioListener.pause = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public static void SetCursor(bool locked)
        {
            if (AutoTest) return;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        void OnApplicationFocus(bool focus)
        {
            if (AutoTest) return;
            // Al cambiar de ventana el turno se pausa solo.
            if (!focus && (State == GameState.Playing || State == GameState.Console)) SetPaused(true);
            else if (focus && (State == GameState.Playing || State == GameState.Console)) SetCursor(true);
        }
    }

    public enum GameState { Intro, Playing, Console, Reading, Paused, Ended }
}
