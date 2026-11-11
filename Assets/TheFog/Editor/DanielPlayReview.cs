using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TheFog
{
    [InitializeOnLoad]
    public static class DanielPlayReview
    {
        static DanielPlayReview()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("TheFog.Review", false)) return;
                SessionState.SetBool("TheFog.Review", false);
                new GameObject("Revisión del prototipo").AddComponent<DanielReviewDriver>();
            };
        }

        [MenuItem("The Fog/Revisar lectura y pausa")]
        public static void Run()
        {
            if (EditorApplication.isPlaying) return;
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "CasaDaniel")
            {
                Debug.LogError("Abre CasaDaniel para revisar el primer tramo.");
                return;
            }
            SessionState.SetBool("TheFog.Review", true);
            EditorApplication.EnterPlaymode();
        }
    }

    public sealed class DanielReviewDriver : MonoBehaviour
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        readonly List<string> results = new();
        readonly List<string> errors = new();
        void OnError(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception) errors.Add(message);
        }
        object Call(ValdaraPlayer player, string method, params object[] args)
            => typeof(ValdaraPlayer).GetMethod(method, flags).Invoke(player, args);
        IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);
            Application.logMessageReceived += OnError;
            Directory.CreateDirectory("docs/prototype-review");
            yield return null;
            var player = FindAnyObjectByType<ValdaraPlayer>();
            Call(player, "BeginGame");
            Call(player, "SetPaused", true);
            if (Resources.Load<Texture2D>("Daniel/PapelNota") == null) errors.Add("Missing note texture");
            var notes = new List<ValdaraAction>();
            foreach (var action in FindObjectsByType<ValdaraAction>(FindObjectsSortMode.None))
            {
                if (action.kind == ValdaraAction.Kind.Read) notes.Add(action);
                if (action.kind == ValdaraAction.Kind.Furniture)
                {
                    var originalPrompt = action.prompt;
                    Call(player, "Use", action);
                    Call(player, "Use", action);
                    if (action.prompt != originalPrompt) errors.Add("Furniture did not close: " + action.name);
                    else results.Add("Furniture opens and closes: " + action.name);
                }
            }
            if (notes.Count < 2) errors.Add("Expected both house documents");
            var papers = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            int count = 0;
            foreach (var prop in papers) if (prop.name.StartsWith("Papel · ")) count++;
            if (count != notes.Count) errors.Add("Physical papers do not match readable notes");
            else results.Add("Both readable notes have physical paper props");
            foreach (var note in notes)
            {
                Call(player, "Use", note);
                if ((ValdaraAction)typeof(ValdaraPlayer).GetField("reading", flags).GetValue(player) != note)
                    errors.Add("Reading did not open: " + note.name);
                else results.Add("Reading opened: " + note.prompt);
                yield return null;
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot("docs/prototype-review/nota-" + note.name + ".png");
                yield return null;
            }
            typeof(ValdaraPlayer).GetField("reading", flags).SetValue(player, null);
            Call(player, "SetPaused", true);
            yield return null;
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot("docs/prototype-review/pausa.png");
            yield return null;
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Valdara");
            yield return null;
            player = FindAnyObjectByType<ValdaraPlayer>();
            Call(player, "Warp", new Vector3(0f, 0.15f, 33f), 0f);
            Call(player, "SetPaused", false);
            foreach (float z in new[] { -30f, 0f, 33f, 46f })
            {
                bool ground = Physics.Raycast(new Vector3(0f, 2f, z), Vector3.down, 3f);
                if (!ground) errors.Add("Missing path ground at z=" + z);
                else results.Add("Walkable ground on main path at z=" + z);
            }
            yield return null;
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot("docs/prototype-review/camino-mina.png");
            yield return null;
            Call(player, "SetPaused", true);
            File.WriteAllText("docs/prototype-review/resultados.txt", "RESULT: " + (errors.Count == 0 ? "PASS" : "FAIL")
                + "\n" + string.Join("\n", results) + "\n" + string.Join("\n", errors));
            Application.logMessageReceived -= OnError;
            Debug.Log("THE FOG PROTOTYPE REVIEW: " + (errors.Count == 0 ? "PASS" : "FAIL"));
            Destroy(gameObject);
        }
    }
}
