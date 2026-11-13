using UnityEngine;

namespace TheFog.FirstPlayable
{
    /// <summary>Movimiento en primera persona: caminar, correr, mirar con ratón o mando, gravedad y colisión.</summary>
    [RequireComponent(typeof(CharacterController))]
    public class FogPlayerController : MonoBehaviour
    {
        public FogInput input;
        [Tooltip("Transform de la cámara; gira en vertical. El cuerpo gira en horizontal.")]
        public Transform cameraPivot;

        [Header("Movimiento")]
        public float walkSpeed = 3.2f;
        public float sprintSpeed = 5.2f;
        public float acceleration = 12f;
        public float gravity = -18f;

        [Header("Mirada")]
        [Tooltip("Grados por píxel de movimiento del ratón.")]
        public float mouseSensitivity = 0.09f;
        [Tooltip("Grados por segundo con el stick derecho.")]
        public float stickLookSpeed = 140f;
        public float pitchLimit = 82f;
        public bool invertY;

        [Header("Estado")]
        [Tooltip("La pausa lo desactiva para bloquear movimiento y mirada.")]
        public bool controlEnabled = true;

        CharacterController controller;
        Vector3 horizontalVelocity;
        float verticalVelocity;
        float pitch;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (cameraPivot != null)
            {
                pitch = cameraPivot.localEulerAngles.x;
                if (pitch > 180f) pitch -= 360f;
            }
        }

        void Update()
        {
            if (input == null || controller == null) return;
            if (controlEnabled) Look();
            Move(controlEnabled);
        }

        void Look()
        {
            Vector2 delta = input.LookMouse * mouseSensitivity + input.LookStick * (stickLookSpeed * Time.deltaTime);
            transform.Rotate(0f, delta.x, 0f, Space.Self);
            pitch += invertY ? delta.y : -delta.y;
            pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);
            if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        void Move(bool acceptInput)
        {
            Vector2 axis = acceptInput ? Vector2.ClampMagnitude(input.Move, 1f) : Vector2.zero;
            float speed = (acceptInput && input.Sprint && axis.y > 0.1f) ? sprintSpeed : walkSpeed;
            Vector3 target = (transform.right * axis.x + transform.forward * axis.y) * speed;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, acceleration * speed * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = horizontalVelocity;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
