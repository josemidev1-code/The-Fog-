using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>
    /// Prueba de juego automática dentro del ejecutable (argumento -puestoAutotest).
    /// Recorre el bucle completo con las mismas funciones que usan los controles y escribe un informe JSON con capturas.
    /// </summary>
    public class PuestoAutoTest : MonoBehaviour
    {
        [System.Serializable] class Check { public string name; public bool pass; public string detail; }
        [System.Serializable] class Report { public string date; public bool pass; public List<Check> checks = new List<Check>(); public float averageFps; public int frames; }

        static Report report;
        static int phase;
        static string outDir;

        static float fpsTime; static int fpsFrames;

        void Start()
        {
            if (!PuestoGame.AutoTest) { enabled = false; return; }
            if (outDir == null)
            {
                var args = System.Environment.GetCommandLineArgs();
                outDir = Path.Combine(Application.dataPath, "..", "autotest");
                for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-puestoOut") outDir = args[i + 1];
                Directory.CreateDirectory(outDir);
                report = new Report { date = System.DateTime.Now.ToString("o") };
            }
            StartCoroutine(phase == 0 ? Main() : LossPhase());
        }

        void Update() { fpsTime += Time.unscaledDeltaTime; fpsFrames++; }

        void Add(string name, bool pass, string detail = "") { report.checks.Add(new Check { name = name, pass = pass, detail = detail }); Debug.Log($"[AUTOTEST] {(pass ? "OK " : "FALLO")} {name} {detail}"); }

        IEnumerator Shot(string file)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(outDir, file));
            yield return null; yield return null;
        }

        IEnumerator WaitFor(System.Func<bool> condition, float seconds)
        {
            int frames = 0; // el primer fotograma tras la carga puede durar varios segundos
            for (float t = 0; (t < seconds || frames < 120) && !condition(); t += Mathf.Min(Time.unscaledDeltaTime, 0.1f), frames++) yield return null;
        }

        IEnumerator Main()
        {
            var g = PuestoGame.I;
            yield return WaitFor(() => g.State == GameState.Playing, 10f);
            Add("La introducción termina y empieza el turno", g.State == GameState.Playing && g.clock.running);
            yield return new WaitForSeconds(0.5f);
            Vector3 p0 = g.player.transform.position;
            Add("Daniel está de pie en la cabina", p0.y > 6.5f && p0.y < 9f, p0.ToString());

            g.player.TestMove(g.player.transform.forward, 1f);
            yield return new WaitForSeconds(1.2f);
            float moved = Vector3.Distance(p0, g.player.transform.position);
            Add("Movimiento con colisiones", moved > 0.5f && g.player.transform.position.y > 6.5f, "desplazamiento " + moved.ToString("0.00"));

            var pickup = FindAnyObjectByType<FlashlightPickup>();
            g.player.LookAtPoint(g.console.screen.transform.position);
            yield return Shot("01_cabina_consola.png");
            pickup.Interact();
            yield return null;
            Add("Coger la linterna", g.flashlight.owned && g.flashlight.on);

            var interactor = g.player.GetComponentInChildren<Interactor>();
            g.player.LookAtPoint(g.console.transform.position);
            yield return new WaitForSeconds(0.2f);
            Add("Mirar la consola muestra su indicación", interactor.Current == g.console, interactor.Current ? interactor.Current.name : "nada");

            g.player.LookAtPoint(g.player.transform.position + new Vector3(0, 6.5f, 30f) * 0 + new Vector3(0, -2f, 40f));
            yield return new WaitForSeconds(0.3f);
            yield return Shot("02_ventana_mina.png");
            g.player.LookAtPoint(g.player.transform.position + new Vector3(-35f, -5f, -10f));
            yield return Shot("03_ventana_casa_daniel.png");

            g.console.Interact();
            yield return new WaitForSeconds(0.8f);
            Add("Sentarse en la consola", g.State == GameState.Console);
            for (int i = 0; i < g.console.feeds.Length; i++)
            {
                g.console.Switch(i);
                yield return new WaitForSeconds(1f);
                yield return Shot($"04_cam{i + 1}.png");
                Add("Cámara " + (i + 1) + " activa", g.console.feeds[i].camera.enabled && g.console.current == i, g.console.feeds[i].label);
            }

            var site = g.director.Spawn(0, 0);
            g.console.Switch(0);
            yield return new WaitForSeconds(1.5f);
            yield return Shot("05_anomalia_cam1.png");
            float fogBefore = g.fog.level;
            g.console.Report();
            Add("Registrar una anomalía real", g.Registered == 1 && site != null && site.Reported, "niebla " + fogBefore.ToString("0.000") + " -> " + g.fog.level.ToString("0.000"));

            g.console.Switch(1);
            yield return new WaitForSeconds(1f);
            g.console.Report();
            Add("Falsa alarma penaliza", g.FalseReports == 1 && g.fog.level > fogBefore - 0.035f);

            g.console.Exit();
            yield return new WaitForSeconds(0.6f);
            Add("Levantarse de la consola", g.State == GameState.Playing && !g.player.Posed);

            g.power.Trip();
            yield return new WaitForSeconds(1.2f);
            Add("Apagón: cámaras sin señal", !g.power.On && !g.console.feeds[g.console.current].camera.enabled);
            var panel = FindAnyObjectByType<BreakerPanel>();
            g.player.Teleport(panel.transform.position + new Vector3(0.1f, -1.45f, 1.3f));
            yield return null;
            g.player.LookAtPoint(panel.transform.position);
            yield return new WaitForSeconds(0.2f);
            yield return Shot("06_apagon_panel.png");
            Add("El panel ofrece rearmar", interactor.Current == panel, interactor.Current ? interactor.Current.name : "nada");
            panel.Interact();
            yield return null;
            Add("Rearmar el diferencial", g.power.On);

            SearchSpot withBatteries = null;
            foreach (var s in FindObjectsByType<SearchSpot>()) if (s.batteries > 0 && string.IsNullOrEmpty(s.noteTitle)) { withBatteries = s; break; }
            int spare = g.flashlight.spareBatteries;
            if (withBatteries != null) withBatteries.Interact();
            Add("Registrar un mueble da pilas", withBatteries != null && g.flashlight.spareBatteries > spare);

            var note = FindAnyObjectByType<ReadableNote>();
            note.Interact();
            yield return null;
            yield return Shot("07_nota.png");
            Add("Leer una nota", g.State == GameState.Reading, note.title);
            g.CloseReading();
            yield return new WaitForSeconds(0.2f);
            Add("Cerrar la nota", g.State == GameState.Playing);

            var missSite = g.director.Spawn(2, -1);
            int missed = g.Missed; float fogMiss = g.fog.level;
            if (missSite != null) missSite.ActiveTime = 9999f;
            yield return new WaitForSeconds(0.3f);
            Add("Anomalía no registrada espesa la niebla", g.Missed == missed + 1 && g.fog.level > fogMiss, (g.fog.level - fogMiss).ToString("0.000"));

            g.SetPaused(true);
            yield return new WaitForSecondsRealtime(0.3f);
            yield return Shot("08_pausa.png");
            Add("Pausa detiene el tiempo", Time.timeScale == 0f && g.State == GameState.Paused);
            g.SetPaused(false);
            yield return null;

            g.clock.SetHour(29.72f);
            g.player.LookAtPoint(g.player.transform.position + new Vector3(30f, -4f, 20f));
            yield return new WaitForSeconds(6f);
            yield return Shot("09_amanecer_previo.png");
            yield return WaitFor(() => g.State == GameState.Ended, 40f);
            yield return new WaitForSeconds(0.5f);
            yield return Shot("10_final_amanecer.png");
            Add("Sobrevivir hasta las 06:00 gana", g.State == GameState.Ended && g.Won);

            phase = 1;
            g.Restart();
        }

        IEnumerator LossPhase()
        {
            var g = PuestoGame.I;
            yield return WaitFor(() => g.State == GameState.Playing, 10f);
            Add("Reiniciar el turno", g.State == GameState.Playing && g.Registered == 0 && g.clock.elapsed < 5f);
            g.fog.level = 1f;
            yield return WaitFor(() => g.State == GameState.Ended, 25f);
            yield return new WaitForSeconds(0.5f);
            yield return Shot("11_final_niebla.png");
            Add("La niebla al 100% termina la partida", g.State == GameState.Ended && !g.Won);

            report.frames = fpsFrames; report.averageFps = fpsFrames / Mathf.Max(0.01f, fpsTime);
            report.pass = report.checks.TrueForAll(c => c.pass);
            File.WriteAllText(Path.Combine(outDir, "informe.json"), JsonUtility.ToJson(report, true));
            Debug.Log("[AUTOTEST] RESULTADO " + (report.pass ? "PASS" : "FAIL"));
            yield return new WaitForSecondsRealtime(0.5f);
            Application.Quit();
        }
    }
}
