using UnityEngine.Rendering.Universal;
namespace TheFog
{
    public sealed class DanielRetroFeature : FullScreenPassRendererFeature
    {
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var marker = renderingData.cameraData.camera.GetComponent<DanielRetroCamera>();
            if (marker != null && marker.isActiveAndEnabled)
                base.AddRenderPasses(renderer, ref renderingData);
        }
    }
}
