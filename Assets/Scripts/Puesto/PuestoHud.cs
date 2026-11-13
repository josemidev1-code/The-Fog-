using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheFog.Puesto
{
    /// <summary>Toda la interfaz del puesto, construida por código para que la escena no dependa de prefabs de UI.</summary>
    public class PuestoHud : MonoBehaviour
    {
        static readonly Color Ink = new Color(0.86f, 0.88f, 0.80f);
        static readonly Color Dim = new Color(0.62f, 0.66f, 0.58f);
        static readonly Color Amber = new Color(1f, 0.74f, 0.35f);
        static readonly Color Danger = new Color(0.95f, 0.30f, 0.22f);
        static readonly Color CctvGreen = new Color(0.62f, 1f, 0.66f);

        Font font, builtinFont;
        float fontScale = 1f;
        /// <summary>Tipografías del puesto (las asigna el generador de la escena). Si falta alguna se usa la de Unity.</summary>
        public Font hudFont, titleFont, noteFont, crtFont;
        void Use(Font f, float scale) { font = f ? f : builtinFont; fontScale = scale; }
        RectTransform root;
        Text prompt, clock, shiftLabel, objective, subtitle, powerText, lightText, intro1, intro2, intro3, introSkip;
        Image holdBar, holdBack, fogFill, battFill, crosshair, fade, fogVeil, reportFlash;
        GameObject hudGroup, battGroup, consoleGroup, readingGroup, pauseGroup, endGroup, introGroup, subtitleGroup;
        Text readingTitle, readingBody, endTitle, endBody, endStats;
        Text camLabel, camRec, camHelp, camCount, camNoSignal;
        RawImage camFeed, camStatic;
        Text[] camTabs;
        readonly List<(Text text, float until)> toasts = new List<(Text, float)>();
        RectTransform toastRoot;
        float subtitleUntil, reportFlashUntil;
        Texture2D noiseTex;
        public RenderTexture feedTexture;

        void Awake() { Build(feedTexture); }

        public void Build(RenderTexture feed)
        {
            builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Use(hudFont, 1.12f);
            var canvasGo = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 10;
            var scaler = canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            root = canvasGo.GetComponent<RectTransform>();

            noiseTex = new Texture2D(160, 120, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            var px = new Color32[160 * 120];
            for (int i = 0; i < px.Length; i++) { byte v = (byte)Random.Range(0, 255); px[i] = new Color32(v, v, v, 255); }
            noiseTex.SetPixels32(px); noiseTex.Apply();

            fogVeil = Img(root, "VeloNiebla", new Color(0.6f, 0.65f, 0.6f, 0f)); Stretch(fogVeil.rectTransform);
            fogVeil.raycastTarget = false;

            // ---------- HUD de juego
            hudGroup = Group(root, "Juego");
            crosshair = Img(hudGroup.transform, "Punto", new Color(1, 1, 1, 0.55f)); Place(crosshair.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(5, 5));
            prompt = Txt(hudGroup.transform, "Indicacion", "", 30, TextAnchor.MiddleCenter, Ink); Place(prompt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -70), new Vector2(1100, 50));
            holdBack = Img(hudGroup.transform, "BarraFondo", new Color(0, 0, 0, 0.5f)); Place(holdBack.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -105), new Vector2(260, 6));
            holdBar = Img(holdBack.transform, "Barra", Amber); holdBar.rectTransform.anchorMin = Vector2.zero; holdBar.rectTransform.anchorMax = new Vector2(0, 1); holdBar.rectTransform.offsetMin = holdBar.rectTransform.offsetMax = Vector2.zero;

            clock = Txt(hudGroup.transform, "Reloj", "22:00", 64, TextAnchor.UpperRight, Ink); Place(clock.rectTransform, new Vector2(1, 1), new Vector2(-48, -30), new Vector2(400, 80), new Vector2(1, 1));
            shiftLabel = Txt(hudGroup.transform, "Turno", "VALDARA · TORRE DE VIGILANCIA · TURNO DE NOCHE", 20, TextAnchor.UpperRight, Dim); Place(shiftLabel.rectTransform, new Vector2(1, 1), new Vector2(-50, -108), new Vector2(600, 30), new Vector2(1, 1));
            var fogLabel = Txt(hudGroup.transform, "NieblaTxt", "NIEBLA", 20, TextAnchor.MiddleRight, Dim); Place(fogLabel.rectTransform, new Vector2(1, 1), new Vector2(-330, -150), new Vector2(160, 26), new Vector2(1, 1));
            var fogBack = Img(hudGroup.transform, "NieblaFondo", new Color(0, 0, 0, 0.55f)); Place(fogBack.rectTransform, new Vector2(1, 1), new Vector2(-50, -150), new Vector2(270, 12), new Vector2(1, 1));
            fogFill = Img(fogBack.transform, "Nivel", new Color(0.75f, 0.8f, 0.75f)); fogFill.rectTransform.anchorMin = Vector2.zero; fogFill.rectTransform.anchorMax = new Vector2(0, 1); fogFill.rectTransform.offsetMin = fogFill.rectTransform.offsetMax = Vector2.zero;
            powerText = Txt(hudGroup.transform, "Luz", "CORRIENTE  ●", 20, TextAnchor.MiddleRight, Dim); Place(powerText.rectTransform, new Vector2(1, 1), new Vector2(-50, -180), new Vector2(400, 26), new Vector2(1, 1));
            battGroup = Group(hudGroup.transform, "Linterna");
            lightText = Txt(battGroup.transform, "PilasTxt", "LINTERNA", 20, TextAnchor.MiddleRight, Dim); Place(lightText.rectTransform, new Vector2(1, 1), new Vector2(-330, -210), new Vector2(400, 26), new Vector2(1, 1));
            var battBack = Img(battGroup.transform, "PilaFondo", new Color(0, 0, 0, 0.55f)); Place(battBack.rectTransform, new Vector2(1, 1), new Vector2(-50, -210), new Vector2(270, 12), new Vector2(1, 1));
            battFill = Img(battBack.transform, "Carga", Amber); battFill.rectTransform.anchorMin = Vector2.zero; battFill.rectTransform.anchorMax = new Vector2(0, 1); battFill.rectTransform.offsetMin = battFill.rectTransform.offsetMax = Vector2.zero;

            var objTitle = Txt(hudGroup.transform, "ObjetivoTitulo", "ÓRDENES DEL TURNO", 18, TextAnchor.UpperLeft, Dim); Place(objTitle.rectTransform, new Vector2(0, 1), new Vector2(48, -36), new Vector2(700, 26), new Vector2(0, 1));
            objective = Txt(hudGroup.transform, "Objetivo", "", 26, TextAnchor.UpperLeft, Ink); Place(objective.rectTransform, new Vector2(0, 1), new Vector2(48, -62), new Vector2(720, 90), new Vector2(0, 1));

            toastRoot = new GameObject("Avisos", typeof(RectTransform)).GetComponent<RectTransform>(); toastRoot.SetParent(root, false);
            Place(toastRoot, new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(1300, 300), new Vector2(0.5f, 1));

            // ---------- Subtítulos de radio
            subtitleGroup = Group(root, "Subtitulos");
            var subBack = Img(subtitleGroup.transform, "Fondo", new Color(0, 0, 0, 0.6f)); Place(subBack.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 70), new Vector2(1300, 120), new Vector2(0.5f, 0));
            subtitle = Txt(subBack.transform, "Texto", "", 28, TextAnchor.MiddleCenter, Ink); Stretch(subtitle.rectTransform, 24);
            subtitle.supportRichText = true;
            subtitleGroup.SetActive(false);

            // ---------- Consola CCTV
            Use(crtFont, 1.3f);
            consoleGroup = Group(root, "Consola");
            var conBack = Img(consoleGroup.transform, "Fondo", new Color(0.01f, 0.012f, 0.01f, 0.94f)); Stretch(conBack.rectTransform);
            var frame = Img(consoleGroup.transform, "Marco", new Color(0.12f, 0.13f, 0.12f)); Place(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(1240, 930));
            camFeed = Raw(frame.transform, "Imagen", feed); Stretch(camFeed.rectTransform, 14);
            camStatic = Raw(frame.transform, "Estatica", noiseTex); Stretch(camStatic.rectTransform, 14); camStatic.color = new Color(1, 1, 1, 0.08f);
            var scan = Raw(frame.transform, "Lineas", MakeScanlines()); Stretch(scan.rectTransform, 14); scan.uvRect = new Rect(0, 0, 1, 300); scan.color = new Color(1, 1, 1, 0.22f);
            camLabel = Txt(frame.transform, "Camara", "CAM 01", 34, TextAnchor.UpperLeft, CctvGreen); Place(camLabel.rectTransform, new Vector2(0, 1), new Vector2(40, -34), new Vector2(800, 50), new Vector2(0, 1));
            camRec = Txt(frame.transform, "Rec", "● REC", 30, TextAnchor.UpperRight, Danger); Place(camRec.rectTransform, new Vector2(1, 1), new Vector2(-40, -34), new Vector2(500, 50), new Vector2(1, 1));
            camNoSignal = Txt(frame.transform, "SinSenal", "SIN SEÑAL", 64, TextAnchor.MiddleCenter, Ink); Stretch(camNoSignal.rectTransform);
            camCount = Txt(frame.transform, "Registro", "", 24, TextAnchor.LowerLeft, CctvGreen); Place(camCount.rectTransform, new Vector2(0, 0), new Vector2(40, 30), new Vector2(800, 40), new Vector2(0, 0));
            reportFlash = Img(frame.transform, "Destello", new Color(1, 1, 1, 0)); Stretch(reportFlash.rectTransform, 14);
            camTabs = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                camTabs[i] = Txt(consoleGroup.transform, "Tab" + i, "", 22, TextAnchor.MiddleCenter, Dim);
                Place(camTabs[i].rectTransform, new Vector2(0.5f, 0), new Vector2(-465 + i * 310, 54), new Vector2(300, 40), new Vector2(0.5f, 0));
            }
            camHelp = Txt(consoleGroup.transform, "Ayuda", "[A/D] o [1-4] cambiar de cámara     [ESPACIO / clic] anotar en el parte     [E / ESC] levantarse", 22, TextAnchor.MiddleCenter, Dim);
            Place(camHelp.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(1500, 36), new Vector2(0.5f, 0));
            consoleGroup.SetActive(false);

            // ---------- Lectura de notas
            Use(noteFont, 0.95f);
            readingGroup = Group(root, "Lectura");
            var dim = Img(readingGroup.transform, "Oscuro", new Color(0, 0, 0, 0.7f)); Stretch(dim.rectTransform);
            var paper = Img(readingGroup.transform, "Papel", new Color(0.82f, 0.79f, 0.68f)); Place(paper.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 760));
            readingTitle = Txt(paper.transform, "Titulo", "", 38, TextAnchor.UpperLeft, new Color(0.15f, 0.13f, 0.1f)); Place(readingTitle.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(780, 60), new Vector2(0.5f, 1));
            readingBody = Txt(paper.transform, "Cuerpo", "", 28, TextAnchor.UpperLeft, new Color(0.2f, 0.18f, 0.14f)); Place(readingBody.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -130), new Vector2(780, 560), new Vector2(0.5f, 1));
            readingBody.lineSpacing = 1.15f;
            var close = Txt(paper.transform, "Cerrar", "[E / ESC] dejarlo donde estaba", 22, TextAnchor.LowerRight, new Color(0.35f, 0.3f, 0.25f)); Place(close.rectTransform, new Vector2(1, 0), new Vector2(-40, 30), new Vector2(500, 30), new Vector2(1, 0));
            readingGroup.SetActive(false);

            // ---------- Fundido, pausa, final, introducción
            Use(hudFont, 1.12f);
            fade = Img(root, "Fundido", new Color(0, 0, 0, 0)); Stretch(fade.rectTransform);

            pauseGroup = Group(root, "Pausa");
            var pBack = Img(pauseGroup.transform, "Fondo", new Color(0, 0, 0, 0.82f)); Stretch(pBack.rectTransform);
            var pTitle = Txt(pauseGroup.transform, "Titulo", "PAUSA", 80, TextAnchor.MiddleCenter, Ink); Place(pTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(900, 110));
            var pBody = Txt(pauseGroup.transform, "Controles",
                "WASD  moverse        RATÓN  mirar        MAYÚS  paso rápido\n" +
                "E  usar / mantener para el panel        F  linterna\n" +
                "En la consola:  A/D o 1-4 cámaras · ESPACIO registrar · E salir\n\n" +
                "Anota solo lo que haya cambiado. Lo que se te pase espesa la niebla;\nun aviso en falso también cuenta. Sin corriente no hay cámaras.",
                26, TextAnchor.MiddleCenter, Dim);
            Place(pBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 70), new Vector2(1300, 260));
            settingsText = Txt(pauseGroup.transform, "Ajustes", "", 30, TextAnchor.MiddleCenter, Ink); Place(settingsText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -120), new Vector2(1300, 100));
            RefreshSettings();
            var pKeys = Txt(pauseGroup.transform, "Teclas", "[ESC] volver al puesto        [R] empezar el turno de nuevo        [Q] salir", 32, TextAnchor.MiddleCenter, Amber); Place(pKeys.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -230), new Vector2(1300, 50));
            pauseGroup.SetActive(false);

            endGroup = Group(root, "Final");
            var eBack = Img(endGroup.transform, "Fondo", new Color(0, 0, 0, 0.75f)); Stretch(eBack.rectTransform);
            endTitle = Txt(endGroup.transform, "Titulo", "", 96, TextAnchor.MiddleCenter, Ink); Place(endTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 210), new Vector2(1600, 130));
            endBody = Txt(endGroup.transform, "Texto", "", 32, TextAnchor.MiddleCenter, Ink); Place(endBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(1400, 200));
            endStats = Txt(endGroup.transform, "Datos", "", 26, TextAnchor.MiddleCenter, Dim); Place(endStats.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -110), new Vector2(1600, 50));
            var eKeys = Txt(endGroup.transform, "Teclas", "[R] volver a subir a la torre        [Q] salir", 32, TextAnchor.MiddleCenter, Amber); Place(eKeys.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -230), new Vector2(1300, 50));
            endGroup.SetActive(false);

            introGroup = Group(root, "Intro");
            var iBack = Img(introGroup.transform, "Negro", Color.black); Stretch(iBack.rectTransform);
            intro1 = Txt(introGroup.transform, "Titulo", "THE FOG", 150, TextAnchor.MiddleCenter, Ink); Place(intro1.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 90), new Vector2(1600, 200));
            intro2 = Txt(introGroup.transform, "Lugar", "Valdara. Torre de vigilancia. 22:00", 36, TextAnchor.MiddleCenter, Dim); Place(intro2.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(1600, 60));
            intro3 = Txt(introGroup.transform, "Texto", "Cuando cae la noche, la niebla baja al pueblo.\nTu trabajo es mirar. Y anotar todo lo que veas.", 30, TextAnchor.MiddleCenter, Ink); Place(intro3.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(1600, 110));
            introSkip = Txt(introGroup.transform, "Saltar", "[E / ESPACIO] subir a la torre", 22, TextAnchor.LowerRight, Dim); Place(introSkip.rectTransform, new Vector2(1, 0), new Vector2(-40, 30), new Vector2(500, 30), new Vector2(1, 0));
            introGroup.SetActive(false);
            if (titleFont) foreach (var t in new[] { pTitle, endTitle, intro1, clock }) t.font = titleFont;
            if (noteFont) foreach (var t in new[] { intro2, intro3, endBody }) t.font = noteFont;
        }

        // ------------------------------------------------------------ API
        public void SetPrompt(string text, float hold01)
        {
            prompt.text = text ?? "";
            bool showHold = text != null && hold01 > 0f;
            holdBack.gameObject.SetActive(showHold);
            holdBar.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(hold01), 1);
            crosshair.color = new Color(1, 1, 1, text != null ? 0.95f : 0.45f);
        }

        public void SetObjective(string text) { objective.text = text; }

        public void Toast(string text)
        {
            var t = Txt(toastRoot, "Aviso", text, 28, TextAnchor.UpperCenter, Ink);
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 1); t.rectTransform.pivot = new Vector2(0.5f, 1);
            t.rectTransform.sizeDelta = new Vector2(1300, 40);
            toasts.Insert(0, (t, Time.unscaledTime + 5.5f));
            while (toasts.Count > 4) { Destroy(toasts[toasts.Count - 1].text.gameObject); toasts.RemoveAt(toasts.Count - 1); }
        }

        public void ShowSubtitle(string speaker, string text, float seconds)
        {
            subtitle.text = "<color=#E8B565>" + speaker + ":</color>  " + text;
            subtitleGroup.SetActive(true); subtitleUntil = Time.unscaledTime + seconds;
        }

        public void ShowConsole(bool on) { consoleGroup.SetActive(on); }
        public void FlashReport(bool good) { reportFlashUntil = Time.unscaledTime + 0.35f; reportFlash.color = good ? new Color(0.6f, 1f, 0.6f, 0.35f) : new Color(1f, 0.3f, 0.2f, 0.35f); }

        public void ShowReading(string title, string body) { readingTitle.text = title; readingBody.text = body; readingGroup.SetActive(true); }
        public void HideReading() { if (readingGroup != null) readingGroup.SetActive(false); }
        public void ShowPause(bool on) { pauseGroup.SetActive(on); if (on) RefreshSettings(); }

        Text settingsText;
        static string Bar(float t) { int n = Mathf.RoundToInt(Mathf.Clamp01(t) * 10); return "[" + new string('#', n) + new string('-', 10 - n) + "]"; }
        /// <summary>Ajustes de la pausa: volumen general y sensibilidad del ratón.</summary>
        public void RefreshSettings()
        {
            if (!settingsText) return;
            float sens = Mathf.InverseLerp(0.03f, 0.21f, PuestoSettings.Sensitivity);
            settingsText.text = $"VOLUMEN   {Bar(PuestoSettings.Volume)}  {Mathf.RoundToInt(PuestoSettings.Volume * 100)} %      [←] [→]\n" +
                                $"SENSIBILIDAD DEL RATÓN   {Bar(sens)}      [↓] [↑]";
        }

        public void ShowIntro(bool on) { introGroup.SetActive(on); hudGroup.SetActive(!on); }
        public void SetIntroProgress(float p)
        {
            intro1.color = WithA(intro1.color, Mathf.Clamp01(p * 4f) * Mathf.Clamp01((1f - p) * 6f));
            intro2.color = WithA(intro2.color, Mathf.Clamp01(p * 4f - 0.6f) * Mathf.Clamp01((1f - p) * 6f));
            intro3.color = WithA(intro3.color, Mathf.Clamp01(p * 4f - 1.4f) * Mathf.Clamp01((1f - p) * 6f));
            introSkip.color = WithA(introSkip.color, p > 0.15f ? 0.8f : 0f);
            var back = introGroup.transform.GetChild(0).GetComponent<Image>(); back.color = new Color(0, 0, 0, Mathf.Clamp01((1f - p) * 5f));
        }

        public void ShowEnd(bool won, string title, string body, string stats)
        {
            consoleGroup.SetActive(false); readingGroup.SetActive(false); hudGroup.SetActive(false); subtitleGroup.SetActive(false);
            endTitle.text = title; endTitle.color = won ? new Color(1f, 0.88f, 0.7f) : Ink;
            endBody.text = body; endStats.text = stats;
            endGroup.transform.GetChild(0).GetComponent<Image>().color = won ? new Color(0.05f, 0.04f, 0.02f, 0.55f) : new Color(0.55f, 0.58f, 0.55f, 0.92f);
            if (!won) { endBody.color = new Color(0.1f, 0.1f, 0.1f); endStats.color = new Color(0.2f, 0.2f, 0.2f); endTitle.color = new Color(0.1f, 0.1f, 0.1f); }
            endGroup.SetActive(true);
        }

        // ------------------------------------------------------------ frame
        void Update()
        {
            var g = PuestoGame.I; if (g == null || root == null) return;
            clock.text = g.clock.Text;
            float fog = g.fog.Displayed;
            fogFill.rectTransform.anchorMax = new Vector2(fog, 1);
            fogFill.color = fog > 0.75f ? Color.Lerp(new Color(0.75f, 0.8f, 0.75f), Danger, Mathf.PingPong(Time.unscaledTime * 2f, 1f)) : new Color(0.75f, 0.8f, 0.75f);
            powerText.text = g.power.On ? "CORRIENTE  <color=#7CDC86>●</color>" : "CORRIENTE  <color=#F04B38>● CORTADA</color>";
            powerText.supportRichText = true;
            battGroup.SetActive(g.flashlight.owned);
            if (g.flashlight.owned)
            {
                battFill.rectTransform.anchorMax = new Vector2(g.flashlight.battery, 1);
                lightText.text = "LINTERNA" + (g.flashlight.spareBatteries > 0 ? "  · pilas de repuesto: " + g.flashlight.spareBatteries : "");
            }
            fogVeil.color = new Color(RenderSettings.fogColor.r, RenderSettings.fogColor.g, RenderSettings.fogColor.b, Mathf.Clamp01((fog - 0.7f) / 0.3f) * 0.35f);

            for (int i = toasts.Count - 1; i >= 0; i--)
            {
                var (t, until) = toasts[i];
                float a = Mathf.Clamp01(until - Time.unscaledTime);
                t.color = WithA(t.color, a);
                t.rectTransform.anchoredPosition = Vector2.Lerp(t.rectTransform.anchoredPosition, new Vector2(0, -i * 44f), Time.unscaledDeltaTime * 10f);
                if (a <= 0) { Destroy(t.gameObject); toasts.RemoveAt(i); }
            }
            if (subtitleGroup.activeSelf && Time.unscaledTime > subtitleUntil) subtitleGroup.SetActive(false);

            if (consoleGroup.activeSelf) UpdateConsole(g);
        }

        void UpdateConsole(PuestoGame g)
        {
            var con = g.console; var feed = con.Current;
            bool on = g.power.On;
            camLabel.text = $"CAM {con.current + 1:00} · {feed.label}";
            camRec.text = on ? (Mathf.Repeat(Time.unscaledTime, 1f) < 0.6f ? "● REC  " : "      ") + g.clock.Text : g.clock.Text;
            camFeed.color = on ? Color.white : Color.black;
            camNoSignal.gameObject.SetActive(!on);
            camStatic.uvRect = new Rect(Random.value, Random.value, 1.6f, 1.6f);
            float burst = Mathf.Clamp01(1f - con.SinceSwitch / 0.45f);
            camStatic.color = new Color(1, 1, 1, on ? burst * 0.8f : 0.5f);
            camCount.text = $"PARTE DE LA NOCHE    anotadas {g.Registered}    sin anotar {g.Missed}";
            for (int i = 0; i < camTabs.Length && i < con.feeds.Length; i++)
            {
                var site = con.feeds[i].site;
                // La interferencia delata durante unos segundos dónde acaba de pasar algo.
                bool interference = site.IsActive && Time.time - site.StartedAt < 6f && Mathf.Repeat(Time.unscaledTime * 3f, 1f) < 0.5f;
                camTabs[i].text = (i == con.current ? "> " : "") + $"{i + 1} · {con.feeds[i].label}" + (interference ? "  ≈" : "");
                camTabs[i].color = i == con.current ? CctvGreen : interference ? Amber : Dim;
            }
            reportFlash.color = WithA(reportFlash.color, Time.unscaledTime < reportFlashUntil ? reportFlash.color.a * 0.92f : 0f);
        }

        // ------------------------------------------------------------ helpers
        static Color WithA(Color c, float a) { c.a = a; return c; }

        static GameObject Group(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            Stretch(go.GetComponent<RectTransform>()); return go;
        }

        Text Txt(Transform parent, string name, string text, int size, TextAnchor anchor, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Shadow)); go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>(); t.font = font; t.text = text; t.fontSize = Mathf.RoundToInt(size * fontScale); t.alignment = anchor; t.color = color; t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow; t.supportRichText = true;
            var s = go.GetComponent<Shadow>(); s.effectColor = new Color(0, 0, 0, 0.8f); s.effectDistance = new Vector2(2, -2);
            return t;
        }

        static Image Img(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var i = go.GetComponent<Image>(); i.color = color; i.raycastTarget = false; return i;
        }

        static RawImage Raw(Transform parent, string name, Texture tex)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(RawImage)); go.transform.SetParent(parent, false);
            var r = go.GetComponent<RawImage>(); r.texture = tex; r.raycastTarget = false; return r;
        }

        static void Stretch(RectTransform r, float inset = 0) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new Vector2(inset, inset); r.offsetMax = new Vector2(-inset, -inset); }

        static void Place(RectTransform r, Vector2 anchor, Vector2 pos, Vector2 size, Vector2? pivot = null)
        {
            r.anchorMin = r.anchorMax = anchor; r.pivot = pivot ?? new Vector2(0.5f, 0.5f); r.anchoredPosition = pos; r.sizeDelta = size;
        }

        static Texture2D MakeScanlines()
        {
            var t = new Texture2D(1, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            t.SetPixels(new[] { new Color(0, 0, 0, 0.9f), new Color(0, 0, 0, 0), new Color(0, 0, 0, 0), new Color(0, 0, 0, 0.4f) }); t.Apply();
            return t;
        }
    }
}
