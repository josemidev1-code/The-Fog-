using UnityEngine;

namespace TheFog
{
    [RequireComponent(typeof(Camera))]
    public sealed class DanielRetroCamera : MonoBehaviour
    {
        Material material;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (material == null)
            {
                Shader shader = Resources.Load<Shader>("Daniel/DanielRetro");
                if (shader == null || !shader.isSupported)
                {
                    Graphics.Blit(source, destination);
                    return;
                }
                material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            }
            Graphics.Blit(source, destination, material);
        }

        void OnDisable()
        {
            if (material != null)
                Destroy(material);
        }
    }
}
