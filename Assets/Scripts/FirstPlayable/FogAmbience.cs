using UnityEngine;

namespace TheFog.FirstPlayable
{
    /// <summary>
    /// Ambiente de la escena: niebla gris verdosa, luz ambiente y fondo de cámara.
    /// Se aplica también en el editor para poder ajustar los valores viendo el resultado.
    /// </summary>
    [ExecuteAlways]
    public class FogAmbience : MonoBehaviour
    {
        [Header("Niebla")]
        public bool fogEnabled = true;
        public Color fogColor = new Color(0.36f, 0.41f, 0.37f, 1f);
        [Range(0.001f, 0.2f)]
        public float fogDensity = 0.04f;

        [Header("Luz ambiente")]
        public Color ambientColor = new Color(0.20f, 0.24f, 0.22f, 1f);

        [Header("Cámara")]
        [Tooltip("Su fondo se iguala al color de la niebla para que el horizonte no se recorte.")]
        public Camera targetCamera;
        public float farClipPlane = 90f;

        void OnEnable() { Apply(); }
        void OnValidate() { Apply(); }

        [ContextMenu("Aplicar ambiente")]
        public void Apply()
        {
            RenderSettings.fog = fogEnabled;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;

            if (targetCamera != null)
            {
                targetCamera.clearFlags = CameraClearFlags.SolidColor;
                targetCamera.backgroundColor = fogColor;
                targetCamera.farClipPlane = farClipPlane;
            }
        }
    }
}
