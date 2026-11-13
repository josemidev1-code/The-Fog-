using System.Collections;
using UnityEngine;

namespace TheFog.Puesto
{
    /// <summary>Daniel en primera persona: movimiento pausado, mirada con ratón, balanceo y pasos sobre madera.</summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        public Transform head;
        public Camera cam;
        public float walkSpeed = 1.7f, sprintSpeed = 2.7f, gravity = 18f;
        public float bobAmount = 0.035f, bobFrequency = 1.9f;

        CharacterController cc;
        float yaw, pitch, verticalVelocity, bobPhase, stepDistance;
        bool posed;
        Coroutine poseRoutine;
        Vector3 camRest;
        AudioSource stepSource;

        public bool Posed => posed;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            yaw = transform.eulerAngles.y;
            pitch = head.localEulerAngles.x > 180 ? head.localEulerAngles.x - 360 : head.localEulerAngles.x;
            camRest = cam.transform.localPosition;
            stepSource = gameObject.AddComponent<AudioSource>(); stepSource.spatialBlend = 0; stepSource.playOnAwake = false;
        }

        void Update()
        {
            var g = PuestoGame.I;
            if (g == null || posed) return;
            if (g.State != GameState.Playing) { cc.Move(Vector3.down * 0.01f); return; }

            Vector2 look = PInput.Look();
            yaw += look.x; pitch = Mathf.Clamp(pitch - look.y, -82f, 82f);
            transform.rotation = Quaternion.Euler(0, yaw, 0);
            head.localRotation = Quaternion.Euler(pitch, 0, 0);

            Vector2 input = PInput.Move();
            float speed = PInput.Sprint ? sprintSpeed : walkSpeed;
            Vector3 move = (transform.right * input.x + transform.forward * input.y) * speed;
            verticalVelocity = cc.isGrounded ? -1f : verticalVelocity - gravity * Time.deltaTime;
            Vector3 before = transform.position;
            cc.Move((move + Vector3.up * verticalVelocity) * Time.deltaTime);

            float moved = Vector3.ProjectOnPlane(transform.position - before, Vector3.up).magnitude;
            if (moved > 0.0005f)
            {
                bobPhase += moved / speed * bobFrequency * speed * Mathf.PI;
                stepDistance += moved;
                if (stepDistance > (PInput.Sprint ? 0.85f : 0.7f)) { stepDistance = 0; stepSource.pitch = Random.Range(0.85f, 1.1f); stepSource.PlayOneShot(ProcAudio.Step, 0.55f); }
            }
            float amount = Mathf.Clamp01(move.magnitude / walkSpeed);
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, camRest + new Vector3(Mathf.Cos(bobPhase * 0.5f) * bobAmount * 0.5f, Mathf.Abs(Mathf.Sin(bobPhase * 0.5f)) * bobAmount, 0) * amount, Time.deltaTime * 10f);
        }

        /// <summary>Lleva la cámara a una pose del mundo (por ejemplo, frente al monitor).</summary>
        public void PoseCamera(Transform target, float duration)
        {
            posed = true;
            if (poseRoutine != null) StopCoroutine(poseRoutine);
            poseRoutine = StartCoroutine(MoveCamera(target.position, target.rotation, duration, false));
        }

        public void ReleaseCamera(float duration)
        {
            if (poseRoutine != null) StopCoroutine(poseRoutine);
            poseRoutine = StartCoroutine(MoveCamera(head.TransformPoint(camRest), head.rotation, duration, true));
        }

        IEnumerator MoveCamera(Vector3 pos, Quaternion rot, float duration, bool release)
        {
            Vector3 p0 = cam.transform.position; Quaternion r0 = cam.transform.rotation;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.SmoothStep(0, 1, t / duration);
                cam.transform.SetPositionAndRotation(Vector3.Lerp(p0, pos, k), Quaternion.Slerp(r0, rot, k));
                yield return null;
            }
            cam.transform.SetPositionAndRotation(pos, rot);
            if (release) { cam.transform.localPosition = camRest; cam.transform.localRotation = Quaternion.identity; posed = false; }
            poseRoutine = null;
        }

        /// <summary>Solo para la prueba automática: desplaza a Daniel como si se pulsara una dirección.</summary>
        public void TestMove(Vector3 worldDirection, float seconds) => StartCoroutine(TestMoveRoutine(worldDirection, seconds));
        IEnumerator TestMoveRoutine(Vector3 dir, float seconds)
        {
            for (float t = 0; t < seconds; t += Time.deltaTime) { cc.Move(dir.normalized * walkSpeed * Time.deltaTime + Vector3.down * 0.05f); yield return null; }
        }

        public void Teleport(Vector3 position)
        {
            cc.enabled = false; transform.position = position; cc.enabled = true;
        }

        public void LookAtPoint(Vector3 point)
        {
            Vector3 d = point - head.position;
            yaw = Quaternion.LookRotation(Vector3.ProjectOnPlane(d, Vector3.up)).eulerAngles.y;
            pitch = -Mathf.Asin(d.normalized.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, yaw, 0); head.localRotation = Quaternion.Euler(pitch, 0, 0);
        }
    }
}
