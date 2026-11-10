using UnityEngine;

namespace TheFog
{
    public sealed class ValdaraAction : MonoBehaviour
    {
        public enum Kind { Door, Flashlight, MineGate, Survey, Travel, HingedDoor }

        public Kind kind;
        public string prompt;
        public Vector3 destination;
        public float facing;
        public string destinationScene;
    }
}
