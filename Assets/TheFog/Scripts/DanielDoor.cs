using UnityEngine;

namespace TheFog
{
    public sealed class DanielDoor : MonoBehaviour
    {
        [SerializeField] float openAngle;
        float goal;
        float current;

        public void Toggle(ValdaraAction action, Vector3 playerPosition)
        {
            if (!Mathf.Approximately(goal, current))
                return;

            if (goal == 0f)
            {
                float side = Vector3.Dot(playerPosition - transform.position, transform.forward);
                goal = side < 0f ? -openAngle : openAngle;
            }
            else
            {
                Vector3 threshold = transform.position + transform.parent.right * 0.63f;
                Vector3 delta = playerPosition - threshold;
                if (new Vector2(delta.x, delta.z).sqrMagnitude < 0.75f)
                    return;
                goal = 0f;
            }
            action.prompt = goal == 0f ? "Abrir puerta" : "Cerrar puerta";
        }

        public void Configure(float angle)
        {
            openAngle = angle;
        }

        void Update()
        {
            if (Mathf.Approximately(current, goal))
                return;
            current = Mathf.MoveTowards(current, goal, Time.deltaTime * 200f);
            transform.localRotation = Quaternion.Euler(0f, current, 0f);
        }
    }
}
