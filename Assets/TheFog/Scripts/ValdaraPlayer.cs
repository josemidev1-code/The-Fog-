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
        float doorFade;
        float pitch;
        float fallSpeed;
        float messageUntil;
        string message;
        GUIStyle titleStyle;
        GUIStyle textStyle;
        GUIStyle centeredStyle;

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
            CaptureMouse(true);
        }

        void OnDisable()
        {
            StopAllCoroutines();
            crossingDoor = false;
            doorFade = 0f;
            CaptureMouse(false);
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && controller != null)
                SetPaused(true);
        }

        void Update()
        {
            if (crossingDoor)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
                SetPaused(!paused);
            if (paused)
                return;

            Look();
            Move();
            if (transform.position.y < -12f)
                Warp(spawn, spawnFacing);

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
                    if (daniel)
                        StartCoroutine(CrossDoor(action.destination, action.facing));
                    else
                        Warp(action.destination, action.facing);
                    break;
                case ValdaraAction.Kind.Travel:
                    arrival = new Arrival
                    {
                        scene = action.destinationScene, position = action.destination, facing = action.facing,
                        ownsFlashlight = ownsFlashlight, flashlightOn = flashlight.enabled,
                        mineReached = mineReached, finished = finished
                    };
                    SceneManager.LoadScene(arrival.scene);
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
                    Warp(action.destination, action.facing);
                    break;
                case ValdaraAction.Kind.Survey:
                    finished = true;
                    Notify("Fin del recorrido. Puedes volver a Valdara por la misma salida.");
                    break;
            }
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

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
                textStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true };
                centeredStyle = new GUIStyle(textStyle) { alignment = TextAnchor.MiddleCenter };
            }

            float width = Mathf.Min(410f, Screen.width - 32f);
            Rect hud = new Rect(16f, 16f, width, 88f);
            GUI.Box(hud, GUIContent.none);
            string place = catalog ? "CATÁLOGO  /  THE FOG" : daniel ? "THE FOG  /  CASA DE DANIEL" : "THE FOG  /  VALDARA";
            GUI.Label(new Rect(28f, 20f, width - 24f, 32f), place, titleStyle);

            string objective = "Encuentra la linterna en casa de Daniel.";
            if (catalog) objective = "Biblioteca de modelos de Valdara.";
            else if (daniel) objective = ownsFlashlight
                ? "Explora el claro y sube a la torre de vigilancia."
                : "Entra en la casa y busca la linterna en el dormitorio.";
            else if (finished) objective = "Recorrido completado.";
            else if (mineReached) objective = "Busca el aviso al fondo de la galería.";
            else if (ownsFlashlight) objective = "Sigue el camino hasta la mina.";
            GUI.Label(new Rect(28f, 56f, width - 24f, 38f), objective, textStyle);

            if (paused)
            {
                Rect panel = new Rect((Screen.width - 240f) / 2f, (Screen.height - 148f) / 2f, 240f, 148f);
                GUI.Box(panel, "PAUSA");
                if (GUI.Button(new Rect(panel.x + 20f, panel.y + 30f, 200f, 27f), "Continuar"))
                    SetPaused(false);
                if (GUI.Button(new Rect(panel.x + 20f, panel.y + 65f, 200f, 27f), "Reiniciar escena"))
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                if (GUI.Button(new Rect(panel.x + 20f, panel.y + 100f, 200f, 27f), catalog ? "Ir a Valdara" : "Abrir catálogo"))
                    SceneManager.LoadScene(catalog ? "Valdara" : "Catalogo");
                return;
            }

            GUI.Label(new Rect(Screen.width / 2f - 12f, Screen.height / 2f - 12f, 24f, 24f), "+", centeredStyle);
            if (target != null)
                Banner("E  ·  " + target.prompt, Screen.height - 112f);
            if (Time.unscaledTime < messageUntil)
                Banner(message, Screen.height - 160f);

            string controls = catalog ? "WASD  Moverse      Ratón  Mirar      Esc  Pausa"
                : "WASD  Moverse      Mayús  Correr      E  Usar      F  Linterna      Esc  Pausa";
            GUI.Label(new Rect(12f, Screen.height - 46f, Screen.width - 24f, 40f), controls, centeredStyle);
        }

        void Banner(string text, float y)
        {
            float width = Mathf.Min(450f, Screen.width - 24f);
            Rect area = new Rect((Screen.width - width) / 2f, y, width, 38f);
            GUI.Box(area, GUIContent.none);
            GUI.Label(area, text, centeredStyle);
        }
    }
}
