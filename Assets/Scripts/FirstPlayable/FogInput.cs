using UnityEngine;
using UnityEngine.InputSystem;

namespace TheFog.FirstPlayable
{
    /// <summary>
    /// Acciones del Input System para la primera escena jugable.
    /// Las acciones se serializan en la escena y se pueden reasignar desde el Inspector.
    /// No usa el Input Manager antiguo.
    /// </summary>
    public class FogInput : MonoBehaviour
    {
        public InputAction move;
        public InputAction lookMouse;
        public InputAction lookStick;
        public InputAction sprint;
        public InputAction interact;
        public InputAction flashlight;
        public InputAction pause;
        public InputAction restart;
        public InputAction quit;
        public InputAction qualityLow;
        public InputAction qualityMedium;
        public InputAction qualityHigh;

        public Vector2 Move { get { return move.ReadValue<Vector2>(); } }
        public Vector2 LookMouse { get { return lookMouse.ReadValue<Vector2>(); } }
        public Vector2 LookStick { get { return lookStick.ReadValue<Vector2>(); } }
        public bool Sprint { get { return sprint.IsPressed(); } }
        public bool InteractPressed { get { return interact.WasPressedThisFrame(); } }
        public bool FlashlightPressed { get { return flashlight.WasPressedThisFrame(); } }
        public bool PausePressed { get { return pause.WasPressedThisFrame(); } }
        public bool RestartPressed { get { return restart.WasPressedThisFrame(); } }
        public bool QuitPressed { get { return quit.WasPressedThisFrame(); } }
        public bool QualityLowPressed { get { return qualityLow.WasPressedThisFrame(); } }
        public bool QualityMediumPressed { get { return qualityMedium.WasPressedThisFrame(); } }
        public bool QualityHighPressed { get { return qualityHigh.WasPressedThisFrame(); } }

        static bool IsEmpty(InputAction action)
        {
            return action == null || action.bindings.Count == 0;
        }

        /// <summary>Crea los controles por defecto. El constructor de la escena lo llama para dejarlos guardados.</summary>
        [ContextMenu("Restaurar controles por defecto")]
        public void SetupDefaults()
        {
            move = new InputAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");

            lookMouse = new InputAction("LookMouse", InputActionType.Value);
            lookMouse.AddBinding("<Mouse>/delta");

            lookStick = new InputAction("LookStick", InputActionType.Value);
            lookStick.AddBinding("<Gamepad>/rightStick");

            sprint = new InputAction("Sprint", InputActionType.Button);
            sprint.AddBinding("<Keyboard>/leftShift");
            sprint.AddBinding("<Gamepad>/leftStickPress");

            interact = new InputAction("Interact", InputActionType.Button);
            interact.AddBinding("<Keyboard>/e");
            interact.AddBinding("<Gamepad>/buttonWest");

            flashlight = new InputAction("Flashlight", InputActionType.Button);
            flashlight.AddBinding("<Keyboard>/f");
            flashlight.AddBinding("<Gamepad>/buttonNorth");

            pause = new InputAction("Pause", InputActionType.Button);
            pause.AddBinding("<Keyboard>/escape");
            pause.AddBinding("<Gamepad>/start");

            restart = new InputAction("Restart", InputActionType.Button);
            restart.AddBinding("<Keyboard>/r");

            quit = new InputAction("Quit", InputActionType.Button);
            quit.AddBinding("<Keyboard>/q");

            qualityLow = new InputAction("QualityLow", InputActionType.Button);
            qualityLow.AddBinding("<Keyboard>/f1");
            qualityMedium = new InputAction("QualityMedium", InputActionType.Button);
            qualityMedium.AddBinding("<Keyboard>/f2");
            qualityHigh = new InputAction("QualityHigh", InputActionType.Button);
            qualityHigh.AddBinding("<Keyboard>/f3");
        }

        void Awake()
        {
            if (IsEmpty(move) || IsEmpty(lookMouse) || IsEmpty(lookStick) || IsEmpty(sprint) || IsEmpty(interact)
                || IsEmpty(flashlight) || IsEmpty(pause) || IsEmpty(restart) || IsEmpty(quit)
                || IsEmpty(qualityLow) || IsEmpty(qualityMedium) || IsEmpty(qualityHigh))
            {
                SetupDefaults();
            }
        }

        void OnEnable()
        {
            move.Enable(); lookMouse.Enable(); lookStick.Enable(); sprint.Enable(); interact.Enable();
            flashlight.Enable(); pause.Enable(); restart.Enable(); quit.Enable();
            qualityLow.Enable(); qualityMedium.Enable(); qualityHigh.Enable();
        }

        void OnDisable()
        {
            move.Disable(); lookMouse.Disable(); lookStick.Disable(); sprint.Disable(); interact.Disable();
            flashlight.Disable(); pause.Disable(); restart.Disable(); quit.Disable();
            qualityLow.Disable(); qualityMedium.Disable(); qualityHigh.Disable();
        }
    }
}
