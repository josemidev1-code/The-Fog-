using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheFog.FirstPlayable
{
    /// <summary>
    /// Presets gráficos Bajo / Medio / Alto: resolución interna, sombras, distancia de visión y efectos.
    /// Los tres mantienen la identidad retro (resolución interna reducida y escalado sin suavizar).
    /// F1/F2/F3 cambian de preset; la elección se guarda en PlayerPrefs.
    /// Los valores del asset de URP se restauran al desactivarse para no dejar el proyecto modificado.
    /// Los números son puntos de partida: hay que medir en los equipos reales antes de prometer FPS.
    /// </summary>
    public class FogGraphicsSettings : MonoBehaviour
    {
        [Serializable]
        public class Preset
        {
            public string label = "Medio";
            [Header("Resolución")]
            [Range(0.25f, 1f)] public float renderScale = 0.5f;
            [Tooltip("Escalado sin suavizar: píxel visible, identidad retro.")]
            public bool pointUpscale = true;
            [Header("Sombras")]
            public bool sunShadows = true;
            public float shadowDistance = 25f;
            public bool flashlightShadows;
            [Header("Distancia")]
            public float farClipPlane = 75f;
            [Header("Efectos")]
            [Tooltip("Luces decorativas (farolas).")]
            public bool decorativeLights = true;
        }

        public const string PrefsKey = "TheFog.GraphicsPreset";

        public FogInput input;
        public FogHud hud;
        public Camera targetCamera;
        public Light sun;
        public Light flashlightBeam;
        public Light[] decorativeLights = new Light[0];

        public Preset[] presets = DefaultPresets();
        [Tooltip("Preset inicial si el jugador no ha elegido ninguno: 0 Bajo, 1 Medio, 2 Alto.")]
        public int defaultPreset = 1;

        public int Current { get; private set; }

        UniversalRenderPipelineAsset asset;
        float previousScale;
        float previousShadowDistance;
        UpscalingFilterSelection previousFilter;
        bool captured;

        public static Preset[] DefaultPresets()
        {
            return new[]
            {
                new Preset { label = "Bajo", renderScale = 0.4f, pointUpscale = true, sunShadows = false, shadowDistance = 0f,
                             flashlightShadows = false, farClipPlane = 55f, decorativeLights = false },
                new Preset { label = "Medio", renderScale = 0.5f, pointUpscale = true, sunShadows = true, shadowDistance = 25f,
                             flashlightShadows = false, farClipPlane = 75f, decorativeLights = true },
                new Preset { label = "Alto", renderScale = 0.65f, pointUpscale = true, sunShadows = true, shadowDistance = 45f,
                             flashlightShadows = true, farClipPlane = 95f, decorativeLights = true },
            };
        }

        void OnEnable()
        {
            asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (asset != null)
            {
                previousScale = asset.renderScale;
                previousShadowDistance = asset.shadowDistance;
                previousFilter = asset.upscalingFilter;
                captured = true;
            }
        }

        void Start()
        {
            Apply(PlayerPrefs.GetInt(PrefsKey, defaultPreset), false);
        }

        void OnDisable()
        {
            if (!captured || asset == null) return;
            asset.renderScale = previousScale;
            asset.shadowDistance = previousShadowDistance;
            asset.upscalingFilter = previousFilter;
            captured = false;
        }

        void Update()
        {
            if (input == null) return;
            if (input.QualityLowPressed) Apply(0, true);
            else if (input.QualityMediumPressed) Apply(1, true);
            else if (input.QualityHighPressed) Apply(2, true);
        }

        public void Apply(int index, bool announce)
        {
            if (presets == null || presets.Length == 0) return;
            Current = Mathf.Clamp(index, 0, presets.Length - 1);
            Preset p = presets[Current];

            if (asset != null)
            {
                asset.renderScale = p.renderScale;
                asset.upscalingFilter = p.pointUpscale ? UpscalingFilterSelection.Point : UpscalingFilterSelection.Linear;
                asset.shadowDistance = Mathf.Max(0f, p.shadowDistance);
            }
            if (sun != null) sun.shadows = p.sunShadows ? LightShadows.Soft : LightShadows.None;
            if (flashlightBeam != null) flashlightBeam.shadows = p.flashlightShadows ? LightShadows.Hard : LightShadows.None;
            if (targetCamera != null) targetCamera.farClipPlane = p.farClipPlane;
            if (decorativeLights != null)
            {
                for (int i = 0; i < decorativeLights.Length; i++)
                    if (decorativeLights[i] != null) decorativeLights[i].enabled = p.decorativeLights;
            }

            if (announce)
            {
                PlayerPrefs.SetInt(PrefsKey, Current);
                if (hud != null) hud.ShowMessage("Gráficos: " + p.label, 2.5f);
            }
        }
    }
}
