using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheFog
{
    public sealed class ValdaraPlayer : MonoBehaviour
    {
        [SerializeField] Camera eyes;
        [SerializeField] Light flashlight;
        [SerializeField] bool catalog;
        [SerializeField] bool daniel;
        [SerializeField] Vector3 spawn;
        [SerializeField] float spawnFacing;

        CharacterController controller;
        ValdaraAction target;
        bool ownsFlashlight;
        bool mineReached;
        bool finished;
        bool paused;
        bool crossingDoor;
        bool insideHouse;
        bool titleScreen;
        Vector3 gameplayCameraPosition;
        Quaternion gameplayCameraRotation;
        float doorFade;
        float pitch;
        float fallSpeed;
        float messageUntil;
        string message;
        ValdaraAction reading;
        GUIStyle titleStyle;
        GUIStyle textStyle;
        GUIStyle centeredStyle;
        Texture2D titleShade;
        Texture2D titleMark;
        Texture2D playMark;
        Texture2D exitMark;
        Texture2D notePaper;
        GUIStyle noteStyle;
        GUIStyle noteTitleStyle;
        GUIStyle menuStyle;

        struct Arrival
        {
            public string scene;
            public Vector3 position;
            public float facing;
            public bool ownsFlashlight;
            public bool flashlightOn;
            public bool mineReached;
            public bool finished;
        }

        static Arrival arrival;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearArrival()
        {
            arrival = default;
        }

        public void Configure(Camera view, Light lamp, bool isCatalog, Vector3 start, float facing, bool isDaniel)
        {
            eyes = view;
            flashlight = lamp;
            catalog = isCatalog;
            daniel = isDaniel;
            spawn = start;
            spawnFacing = facing;
        }

        void Start()
        {
            controller = GetComponent<CharacterController>();
            if (arrival.scene == gameObject.scene.name)
            {
                ownsFlashlight = arrival.ownsFlashlight;
                flashlight.enabled = arrival.flashlightOn;
                mineReached = arrival.mineReached;
                finished = arrival.finished;
                Warp(arrival.position, arrival.facing);
                ClearArrival();
                if (ownsFlashlight)
                {
                    foreach (var action in transform.root.GetComponentsInChildren<ValdaraAction>())
                    {
                        if (action.kind == ValdaraAction.Kind.Flashlight)
                            action.gameObject.SetActive(false);
                    }
                }
            }
            else if (daniel)
            {
                titleScreen = true;
                gameplayCameraPosition = eyes.transform.localPosition;
                gameplayCameraRotation = eyes.transform.localRotation;
                eyes.transform.position = ValdaraLayout.BedroomTitleCamera;
                eyes.transform.LookAt(ValdaraLayout.BedroomTitleFocus);
            }
            CaptureMouse(!titleScreen);
        }

        void OnDisable()
        {
            StopAllCoroutines();
            crossingDoor = false;
            doorFade = 0f;
            if (titleScreen)
                RestoreGameplayCamera();
            CaptureMouse(false);
        }

        void OnDestroy()
        {
            if (titleShade != null)
                Destroy(titleShade);
        }

        void RestoreGameplayCamera()
        {
            titleScreen = false;
            eyes.transform.localPosition = gameplayCameraPosition;
            eyes.transform.localRotation = gameplayCameraRotation;
        }

        void BeginGame()
        {
            RestoreGameplayCamera();
            insideHouse = true;
            Warp(ValdaraLayout.BedroomStart, 22f);
            pitch = 23f;
            eyes.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            SetPaused(false);
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && controller != null && !titleScreen)
                SetPaused(true);
        }

        void Update()
        {
            if (titleScreen)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    BeginGame();
                return;
            }

            if (crossingDoor)
                return;

            if (reading != null)
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
                    reading = null;
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
                SetPaused(!paused);
            if (paused)
                return;

            Look();
            Move();
            if (transform.position.y < -12f)
            {
                insideHouse = false;
                Warp(spawn, spawnFacing);
            }

            target = null;
            if (Physics.Raycast(eyes.transform.position, eyes.transform.forward, out RaycastHit hit, 3.5f))
                target = hit.collider.GetComponentInParent<ValdaraAction>();

            if (target != null && Input.GetKeyDown(KeyCode.E))
                Use(target);
            if (!crossingDoor && ownsFlashlight && Input.GetKeyDown(KeyCode.F))
                flashlight.enabled = !flashlight.enabled;
        }

        void Look()
        {
            transform.Rotate(0f, Input.GetAxisRaw("Mouse X") * 2.1f, 0f);
            pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * 2.1f, -82f, 82f);
            eyes.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        void Move()
        {
            float lateral = Axis(KeyCode.A, KeyCode.D, KeyCode.LeftArrow, KeyCode.RightArrow);
            float forward = Axis(KeyCode.S, KeyCode.W, KeyCode.DownArrow, KeyCode.UpArrow);
            Vector3 walk = Vector3.ClampMagnitude(transform.right * lateral + transform.forward * forward, 1f);
            float speed = Input.GetKey(KeyCode.LeftShift) ? 6.1f : 3.7f;
            fallSpeed = controller.isGrounded ? -2f : Mathf.Max(fallSpeed - 23f * Time.deltaTime, -32f);
            controller.Move((walk * speed + Vector3.up * fallSpeed) * Time.deltaTime);
        }

        static float Axis(KeyCode negative, KeyCode positive, KeyCode alternateNegative, KeyCode alternatePositive)
        {
            float right = Input.GetKey(positive) || Input.GetKey(alternatePositive) ? 1f : 0f;
            float left = Input.GetKey(negative) || Input.GetKey(alternateNegative) ? 1f : 0f;
            return right - left;
        }

        void Use(ValdaraAction action)
        {
            switch (action.kind)
            {
                case ValdaraAction.Kind.Door:
                    insideHouse = action.houseInterior;
                    StartCoroutine(CrossDoor(action.destination, action.facing));
                    break;
                case ValdaraAction.Kind.Travel:
                    Travel(action.destinationScene, action.destination, action.facing);
                    break;
                case ValdaraAction.Kind.HingedDoor:
                    action.GetComponentInParent<DanielDoor>().Toggle(action, transform.position);
                    break;
                case ValdaraAction.Kind.Flashlight:
                    ownsFlashlight = true;
                    flashlight.enabled = true;
                    action.gameObject.SetActive(false);
                    Notify("Linterna recogida. Pulsa F para encenderla o apagarla.");
                    break;
                case ValdaraAction.Kind.MineGate:
                    if (!ownsFlashlight)
                    {
                        Notify("Necesitas la linterna de Daniel para entrar.");
                        break;
                    }
                    mineReached = true;
                    StartCoroutine(CrossDoor(action.destination, action.facing));
                    break;
                case ValdaraAction.Kind.Survey:
                    if (finished)
                    {
                        Travel("Catalogo", ValdaraLayout.CreditsSpawn, 0f);
                        break;
                    }
                    finished = true;
                    if (!string.IsNullOrEmpty(action.message)) reading = action;
                    Notify("Galería 01: aviso de cierre localizado. Pulsa E para ver los créditos.");
                    break;
                case ValdaraAction.Kind.Inspect:
                    Notify(action.message);
                    break;
                case ValdaraAction.Kind.Read:
                    reading = action;
                    break;
                case ValdaraAction.Kind.Furniture:
                    action.ToggleFurniture();
                    break;
            }
        }

        void Travel(string scene, Vector3 position, float facing)
        {
            arrival = new Arrival
            {
                scene = scene, position = position, facing = facing,
                ownsFlashlight = ownsFlashlight, flashlightOn = flashlight.enabled,
                mineReached = mineReached, finished = finished
            };
            SceneManager.LoadScene(scene);
        }

        IEnumerator CrossDoor(Vector3 position, float facing)
        {
            crossingDoor = true;
            target = null;
            yield return FadeDoor(1f, 0.24f);
            yield return null;
            Warp(position, facing);
            yield return new WaitForSecondsRealtime(0.08f);
            yield return FadeDoor(0f, 0.28f);
            crossingDoor = false;
        }

        IEnumerator FadeDoor(float opacity, float duration)
        {
            float speed = 1f / duration;
            while (doorFade != opacity)
            {
                doorFade = Mathf.MoveTowards(doorFade, opacity, speed * Time.unscaledDeltaTime);
                yield return null;
            }
        }

        void Warp(Vector3 position, float facing)
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, facing, 0f));
            pitch = 0f;
            fallSpeed = 0f;
            eyes.transform.localRotation = Quaternion.identity;
            controller.enabled = true;
            target = null;
        }

        void Notify(string text)
        {
            message = text;
            messageUntil = Time.unscaledTime + 4.5f;
        }

        void SetPaused(bool value)
        {
            paused = value;
            CaptureMouse(!paused);
        }

        static void CaptureMouse(bool capture)
        {
            Cursor.lockState = capture ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !capture;
        }

        void OnGUI()
        {
            if (crossingDoor)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    var color = GUI.color;
                    GUI.color = new Color(0f, 0f, 0f, doorFade);
                    GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
                    GUI.color = color;
                }
                return;
            }

            float scale = Mathf.Min(Screen.width / 900f, Screen.height / 500f);
            var matrix = GUI.matrix;
            GUI.matrix = matrix * Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            try { DrawHud(Screen.width / scale, Screen.height / scale); }
            finally { GUI.matrix = matrix; }
        }

        void DrawHud(float viewWidth, float viewHeight)
        {
            if (titleStyle == null)
            {
                var bodyFont = Resources.Load<Font>("Tipografia/IBMPlexMono-Regular");
                titleStyle = new GUIStyle(GUI.skin.label) { font = bodyFont, fontSize = 21, fontStyle = FontStyle.Bold };
                textStyle = new GUIStyle(GUI.skin.label) { font = bodyFont, fontSize = 16, wordWrap = true };
                titleStyle.normal.textColor = new Color(1f, 0.86f, 0.60f);
                textStyle.normal.textColor = new Color(0.98f, 0.96f, 0.91f);
                centeredStyle = new GUIStyle(textStyle) { alignment = TextAnchor.MiddleCenter };
                noteStyle = new GUIStyle(textStyle) { fontSize = 17 };
                noteStyle.normal.textColor = new Color(0.16f, 0.14f, 0.10f);
                noteTitleStyle = new GUIStyle(noteStyle) { fontSize = 21, fontStyle = FontStyle.Bold };
                menuStyle = new GUIStyle(textStyle) { alignment = TextAnchor.MiddleLeft };
                notePaper = Resources.Load<Texture2D>("Daniel/PapelNota");
            }

            if (titleScreen)
            {
                DrawTitle(viewWidth, viewHeight);
                return;
            }

            if (reading != null)
            {
                Fill(new Rect(0f, 0f, viewWidth, viewHeight), new Color(0.015f, 0.018f, 0.013f, 0.87f));
                float paperWidth = Mathf.Min(500f, viewWidth - 64f);
                float contentWidth = paperWidth - 64f;
                float headingHeight = noteTitleStyle.CalcHeight(new GUIContent(reading.prompt), contentWidth);
                float bodyHeight = noteStyle.CalcHeight(new GUIContent(reading.message), contentWidth);
                float paperHeight = Mathf.Min(viewHeight - 40f, headingHeight + bodyHeight + 122f);
                var note = new Rect((viewWidth - paperWidth) * 0.5f, (viewHeight - paperHeight) * 0.5f, paperWidth, paperHeight);
                Fill(new Rect(note.x + 5f, note.y + 6f, note.width, note.height), new Color(0f, 0f, 0f, 0.35f));
                if (notePaper != null) GUI.DrawTextureWithTexCoords(note, notePaper, new Rect(0.025f, 0.025f, 0.95f, 0.95f));
                else Fill(note, new Color(0.78f, 0.74f, 0.61f));
                GUI.Label(new Rect(note.x + 32f, note.y + 27f, contentWidth, headingHeight), reading.prompt, noteTitleStyle);
                Fill(new Rect(note.x + 32f, note.y + 40f + headingHeight, contentWidth, 1f), new Color(0.39f, 0.32f, 0.21f, 0.45f));
                GUI.Label(new Rect(note.x + 32f, note.y + 55f + headingHeight, contentWidth, bodyHeight), reading.message, noteStyle);
                GUI.Label(new Rect(note.x + 32f, note.yMax - 43f, contentWidth, 30f), "[ E / Esc ]  Guardar el papel", noteStyle);
                return;
            }

            const float width = 435f;

            string place = catalog ? "THE FOG  /  CRÉDITOS" : daniel ? "THE FOG  /  CASA DE DANIEL" : "THE FOG  /  VALDARA";


            string objective = "Encuentra la linterna en casa de Daniel.";
            if (catalog) objective = "Recorre la exposición. Para salir, usa E en la puerta de entrada.";
            else if (daniel) objective = ownsFlashlight
                ? insideHouse ? "Sal por la puerta principal." : "Sigue la senda hasta la valla para ir a Valdara."
                : insideHouse ? "Explora la casa. La linterna está en el dormitorio." : "Mira hacia la puerta y pulsa E para entrar.";
            else if (finished) objective = "Recorrido completado. Los créditos están disponibles en el menú de pausa.";
            else if (mineReached) objective = "Busca el aviso al fondo de la galería.";
            else if (ownsFlashlight) objective = "Sigue el camino hasta la mina.";
            float objectiveHeight = textStyle.CalcHeight(new GUIContent(objective), width - 24f);
            Panel(new Rect(16f, 16f, width, 48f + objectiveHeight));
            Fill(new Rect(16f, 16f, 2f, 48f + objectiveHeight), new Color(0.57f, 0.43f, 0.25f));
            GUI.Label(new Rect(28f, 20f, width - 24f, 32f), place, titleStyle);
            GUI.Label(new Rect(28f, 56f, width - 24f, objectiveHeight), objective, textStyle);

            if (paused)
            {
                Fill(new Rect(0f, 0f, viewWidth, viewHeight), new Color(0.015f, 0.018f, 0.013f, 0.77f));
                float height = catalog || finished ? 248f : 204f;
                Rect panel = new Rect((viewWidth - 340f) / 2f, (viewHeight - height) / 2f, 340f, height);
                Panel(panel);
                Fill(new Rect(panel.x, panel.y, 2f, height), new Color(0.66f, 0.48f, 0.27f));
                GUI.Label(new Rect(panel.x + 24f, panel.y + 20f, 290f, 32f), "THE FOG / PAUSA", titleStyle);
                if (MenuButton(new Rect(panel.x + 24f, panel.y + 70f, 292f, 40f), "Continuar"))
                    SetPaused(false);
                if (MenuButton(new Rect(panel.x + 24f, panel.y + 114f, 292f, 40f), "Reiniciar escena"))
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                if (catalog && MenuButton(new Rect(panel.x + 24f, panel.y + 158f, 292f, 40f), "Volver a Valdara"))
                    Travel("Valdara", ValdaraLayout.MineReturn, 180f);
                if (!catalog && finished && MenuButton(new Rect(panel.x + 24f, panel.y + 158f, 292f, 40f), "Ver créditos"))
                    Travel("Catalogo", ValdaraLayout.CreditsSpawn, 0f);
                return;
            }

            GUI.Label(new Rect(viewWidth / 2f - 12f, viewHeight / 2f - 12f, 24f, 24f), "+", centeredStyle);
            if (target != null)
                Banner("E  ·  " + (finished && target.kind == ValdaraAction.Kind.Survey ? "Ver créditos" : target.prompt),
                    viewWidth, viewHeight - 104f);
            if (Time.unscaledTime < messageUntil)
                Banner(message, viewWidth, viewHeight - 164f);

            string controls = ownsFlashlight ? "WASD  Moverse    E  Interactuar    F  Linterna    Esc  Pausa"
                : "WASD  Moverse    Ratón  Mirar    E  Interactuar    Esc  Pausa";
            var footer = new Rect((viewWidth - 590f) * 0.5f, viewHeight - 47f, 590f, 32f);
            Panel(footer);
            GUI.Label(footer, controls, centeredStyle);
        }

        void DrawTitle(float width, float height)
        {
            if (titleShade == null)
            {
                titleMark = Resources.Load<Texture2D>("Tipografia/TheFog-Marca");
                playMark = Resources.Load<Texture2D>("Tipografia/TheFog-Jugar");
                exitMark = Resources.Load<Texture2D>("Tipografia/TheFog-Salir");
                titleShade = new Texture2D(64, 1, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
                for (int i = 0; i < titleShade.width; i++)
                {
                    float progress = i / (titleShade.width - 1f);
                    float alpha = 1f - Mathf.SmoothStep(0f, 1f, progress);
                    titleShade.SetPixel(i, 0, new Color(0.035f, 0.033f, 0.025f, 0.97f * alpha));
                }
                titleShade.Apply(false, true);
            }

            GUI.DrawTexture(new Rect(0f, 0f, width, height), titleShade);
            float x = width * 0.072f;
            float y = height * 0.235f;
            if (titleMark != null)
                GUI.DrawTexture(new Rect(x, y, 370f, 136f), titleMark, ScaleMode.ScaleToFit);
            else
                GUI.Label(new Rect(x, y, 370f, 136f), "THE FOG", titleStyle);
            Fill(new Rect(x + 8f, y + 148f, 54f, 1f), new Color(0.66f, 0.48f, 0.27f));
            GUI.Label(new Rect(x + 5f, y + 159f, 330f, 32f), "Dormitorio de Daniel", textStyle);
            if (TitleButton(new Rect(x + 8f, y + 211f, 186f, 42f), playMark, "Jugar"))
                BeginGame();
            if (!Application.isEditor && TitleButton(new Rect(x + 8f, y + 261f, 186f, 42f), exitMark, "Salir"))
                Application.Quit();
        }

        bool TitleButton(Rect area, Texture2D mark, string label)
        {
            bool hover = area.Contains(Event.current.mousePosition);
            var brass = new Color(0.66f, 0.48f, 0.27f, hover ? 1f : 0.65f);
            if (hover)
                Fill(area, new Color(0.24f, 0.19f, 0.12f, 0.9f));
            Fill(new Rect(area.x, area.y + 9f, 2f, area.height - 18f), brass);
            Fill(new Rect(area.x + 2f, area.y + 9f, 6f, 1f), brass);
            Fill(new Rect(area.x + 2f, area.y + area.height - 10f, 6f, 1f), brass);
            var content = new Rect(area.x + 20f, area.y + 13f, 74f, 16f);
            if (mark != null)
                GUI.DrawTexture(content, mark, ScaleMode.ScaleToFit);
            else
                GUI.Label(content, label, textStyle);
            return GUI.Button(area, new GUIContent(string.Empty, label), GUIStyle.none);
        }

        bool MenuButton(Rect area, string label)
        {
            bool hover = area.Contains(Event.current.mousePosition);
            if (hover) Fill(area, new Color(0.24f, 0.19f, 0.12f));
            GUI.Label(new Rect(area.x + 12f, area.y, area.width - 24f, area.height),
                (hover ? "> " : "  ") + label, menuStyle);
            return GUI.Button(area, new GUIContent(string.Empty, label), GUIStyle.none);
        }

        void Banner(string text, float viewWidth, float y)
        {
            float height = Mathf.Max(52f, centeredStyle.CalcHeight(new GUIContent(text), 536f) + 14f);
            var area = new Rect((viewWidth - 560f) * 0.5f, y - (height - 52f), 560f, height);
            Panel(area);
            GUI.Label(new Rect(area.x + 12f, area.y, 536f, area.height), text, centeredStyle);
        }

        static void Panel(Rect area)
            => Fill(area, new Color(0.055f, 0.062f, 0.05f, 0.94f));

        static void Fill(Rect area, Color tint)
        {
            var color = GUI.color;
            GUI.color = tint;
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = color;
        }
    }
}
