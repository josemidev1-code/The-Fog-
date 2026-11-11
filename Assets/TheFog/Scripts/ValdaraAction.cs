using UnityEngine;

namespace TheFog
{
    public sealed class ValdaraAction : MonoBehaviour
    {
        public enum Kind { Door, Flashlight, MineGate, Survey, Travel, HingedDoor, Inspect, Read, Furniture }

        public Kind kind;
        public string prompt;
        public Vector3 destination;
        public float facing;
        public string destinationScene;
        public string message;
        public bool houseInterior;
        public string openModel;
        public float modelSize;
        public string closePrompt;

        GameObject closedFurniture;
        GameObject openFurniture;

        public void ToggleFurniture()
        {
            if (openFurniture == null)
            {
                closedFurniture = transform.GetChild(0).gameObject;
                openFurniture = ValdaraLayout.CreateModel(null, openModel, Vector3.zero, modelSize, 0f, false,
                    (asset, parent) => Instantiate(asset, parent));
                openFurniture.transform.SetParent(transform, false);
                openFurniture.SetActive(false);
            }
            bool opening = !openFurniture.activeSelf;
            openFurniture.SetActive(opening);
            closedFurniture.SetActive(!opening);
            (prompt, closePrompt) = (closePrompt, prompt);
        }
    }
}
