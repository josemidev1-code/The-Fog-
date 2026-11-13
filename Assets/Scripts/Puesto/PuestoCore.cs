using UnityEngine;
using UnityEngine.InputSystem;

namespace TheFog.Puesto
{
    /// <summary>Ajustes del jugador que se guardan entre partidas: volumen general y sensibilidad del ratón.</summary>
    public static class PuestoSettings
    {
        const string VolumeKey = "puesto.volumen", SensKey = "puesto.sensibilidad";
        public const float DefaultVolume = 0.4f, DefaultSensitivity = 0.09f;
        public static float Volume { get; private set; } = DefaultVolume;
        public static float Sensitivity { get; private set; } = DefaultSensitivity;

        public static void Load()
        {
            SetVolume(PlayerPrefs.GetFloat(VolumeKey, DefaultVolume), false);
            SetSensitivity(PlayerPrefs.GetFloat(SensKey, DefaultSensitivity), false);
        }
        public static void SetVolume(float v, bool save = true)
        {
            Volume = Mathf.Clamp01(Mathf.Round(v * 10f) / 10f); AudioListener.volume = Volume;
            if (save) { PlayerPrefs.SetFloat(VolumeKey, Volume); PlayerPrefs.Save(); }
        }
        public static void SetSensitivity(float v, bool save = true)
        {
            Sensitivity = Mathf.Clamp(Mathf.Round(v * 100f) / 100f, 0.03f, 0.21f); PInput.MouseSensitivity = Sensitivity;
            if (save) { PlayerPrefs.SetFloat(SensKey, Sensitivity); PlayerPrefs.Save(); }
        }
    }

    /// <summary>Lectura directa de teclado, ratón y mando (Input System). Centraliza todos los controles del puesto.</summary>
    public static class PInput
    {
        public static float MouseSensitivity = 0.09f;

        static bool Down(Key key) { var k = Keyboard.current; return k != null && k[key].wasPressedThisFrame; }
        static bool Held(Key key) { var k = Keyboard.current; return k != null && k[key].isPressed; }

        public static Vector2 Move()
        {
            Vector2 v = Vector2.zero;
            if (Held(Key.W) || Held(Key.UpArrow)) v.y += 1;
            if (Held(Key.S) || Held(Key.DownArrow)) v.y -= 1;
            if (Held(Key.D) || Held(Key.RightArrow)) v.x += 1;
            if (Held(Key.A) || Held(Key.LeftArrow)) v.x -= 1;
            var g = Gamepad.current; if (g != null) v += g.leftStick.ReadValue();
            return Vector2.ClampMagnitude(v, 1f);
        }

        public static Vector2 Look()
        {
            Vector2 d = Vector2.zero;
            var m = Mouse.current; if (m != null) d += m.delta.ReadValue() * MouseSensitivity;
            var g = Gamepad.current; if (g != null) d += g.rightStick.ReadValue() * 140f * Time.unscaledDeltaTime;
            return d;
        }

        public static bool Interact => Down(Key.E) || (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false);
        public static bool InteractHeld => Held(Key.E) || (Gamepad.current?.buttonSouth.isPressed ?? false);
        public static bool Flashlight => Down(Key.F) || (Gamepad.current?.buttonNorth.wasPressedThisFrame ?? false);
        public static bool Pause => Down(Key.Escape) || (Gamepad.current?.startButton.wasPressedThisFrame ?? false);
        public static bool Back => Down(Key.Escape) || Down(Key.E) || (Gamepad.current?.buttonEast.wasPressedThisFrame ?? false);
        public static bool Report => Down(Key.Space) || (Mouse.current?.leftButton.wasPressedThisFrame ?? false) || (Gamepad.current?.buttonWest.wasPressedThisFrame ?? false);
        public static bool NextCam => Down(Key.D) || Down(Key.RightArrow) || (Gamepad.current?.rightShoulder.wasPressedThisFrame ?? false);
        public static bool PrevCam => Down(Key.A) || Down(Key.LeftArrow) || (Gamepad.current?.leftShoulder.wasPressedThisFrame ?? false);
        public static bool Restart => Down(Key.R);
        public static int AdjustX => (Down(Key.RightArrow) || (Gamepad.current?.dpad.right.wasPressedThisFrame ?? false) ? 1 : 0) - (Down(Key.LeftArrow) || (Gamepad.current?.dpad.left.wasPressedThisFrame ?? false) ? 1 : 0);
        public static int AdjustY => (Down(Key.UpArrow) || (Gamepad.current?.dpad.up.wasPressedThisFrame ?? false) ? 1 : 0) - (Down(Key.DownArrow) || (Gamepad.current?.dpad.down.wasPressedThisFrame ?? false) ? 1 : 0);
        public static bool Quit => Down(Key.Q);
        public static bool Confirm => Down(Key.Enter) || Down(Key.Space) || Down(Key.E) || (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false);
        public static bool Sprint => Held(Key.LeftShift) || (Gamepad.current?.leftStickButton.isPressed ?? false);

        public static int NumberPressed()
        {
            if (Down(Key.Digit1) || Down(Key.Numpad1)) return 0;
            if (Down(Key.Digit2) || Down(Key.Numpad2)) return 1;
            if (Down(Key.Digit3) || Down(Key.Numpad3)) return 2;
            if (Down(Key.Digit4) || Down(Key.Numpad4)) return 3;
            return -1;
        }
    }

    /// <summary>Genera todos los sonidos del puesto por síntesis: no requiere archivos de audio.</summary>
    public static class ProcAudio
    {
        const int Rate = 44100;
        static AudioClip hum, wind, staticNoise, beep, click, knock, step, heart, alarm, sting;

        static AudioClip Make(string name, float seconds, System.Func<float, int, float> f)
        {
            int n = Mathf.CeilToInt(seconds * Rate); var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)Rate, i), -1f, 1f);
            var clip = AudioClip.Create(name, n, 1, Rate, false); clip.SetData(data, 0); return clip;
        }

        static float Noise(int i) { uint x = (uint)i * 747796405u + 2891336453u; x = ((x >> (int)((x >> 28) + 4)) ^ x) * 277803737u; x = (x >> 22) ^ x; return (x / (float)uint.MaxValue) * 2f - 1f; }

        public static AudioClip Hum => hum ??= Make("hum", 2f, (t, i) => 0.18f * Mathf.Sin(2 * Mathf.PI * 50 * t) + 0.08f * Mathf.Sin(2 * Mathf.PI * 100 * t) + 0.03f * Mathf.Sin(2 * Mathf.PI * 150 * t) + 0.01f * Noise(i));
        public static AudioClip Wind
        {
            get
            {
                if (wind != null) return wind;
                float lp = 0, lp2 = 0;
                return wind = Make("wind", 8f, (t, i) =>
                {
                    float gust = 0.55f + 0.45f * Mathf.Sin(2 * Mathf.PI * t / 8f) * Mathf.Sin(2 * Mathf.PI * t * 3f / 8f);
                    lp += (Noise(i) - lp) * (0.02f + 0.03f * gust); lp2 += (lp - lp2) * 0.08f;
                    // Fundido cíclico para que el bucle no tenga corte.
                    float edge = Mathf.Min(1f, Mathf.Min(t, 8f - t) * 4f);
                    return lp2 * 2.6f * gust * edge + (1 - edge) * 0f;
                });
            }
        }
        public static AudioClip Static
        {
            get
            {
                if (staticNoise != null) return staticNoise;
                float lp = 0;
                return staticNoise = Make("static", 1.5f, (t, i) => { lp += (Noise(i) - lp) * 0.45f; float crackle = Noise(i * 7) > 0.995f ? Noise(i * 3) : 0; return lp * 0.35f + crackle * 0.6f; });
            }
        }
        public static AudioClip Beep => beep ??= Make("beep", 0.14f, (t, i) => 0.35f * Mathf.Sign(Mathf.Sin(2 * Mathf.PI * 880 * t)) * Mathf.Clamp01((0.14f - t) * 30f));
        public static AudioClip Click => click ??= Make("click", 0.05f, (t, i) => Noise(i) * Mathf.Exp(-t * 160f) * 0.8f);
        public static AudioClip Knock => knock ??= Make("knock", 1.6f, (t, i) =>
        {
            float s = 0; float[] at = { 0f, 0.42f, 0.84f };
            foreach (float a in at) { float d = t - a; if (d > 0 && d < 0.25f) s += (Mathf.Sin(2 * Mathf.PI * 95 * d) * 0.9f + Noise(i) * 0.35f) * Mathf.Exp(-d * 28f); }
            return s;
        });
        public static AudioClip Step => step ??= Make("step", 0.16f, (t, i) => (Noise(i) * 0.6f + Mathf.Sin(2 * Mathf.PI * 70 * t) * 0.5f) * Mathf.Exp(-t * 38f) * 0.55f);
        public static AudioClip Heart => heart ??= Make("heart", 1f, (t, i) =>
        {
            float s = 0; float[] at = { 0f, 0.22f };
            foreach (float a in at) { float d = t - a; if (d > 0) s += Mathf.Sin(2 * Mathf.PI * 48 * d) * Mathf.Exp(-d * 22f); }
            return s * 0.9f;
        });
        public static AudioClip Alarm => alarm ??= Make("alarm", 1.2f, (t, i) => 0.25f * Mathf.Sin(2 * Mathf.PI * (520 + 180 * Mathf.Sin(2 * Mathf.PI * 2.5f * t)) * t) * Mathf.Clamp01((1.2f - t) * 6));
        public static AudioClip Sting => sting ??= Make("sting", 3.5f, (t, i) =>
        {
            float env = Mathf.Clamp01(t * 6f) * Mathf.Exp(-t * 0.9f);
            return env * (0.22f * Mathf.Sin(2 * Mathf.PI * 55 * t) + 0.16f * Mathf.Sin(2 * Mathf.PI * 58.3f * t) + 0.12f * Mathf.Sin(2 * Mathf.PI * 233f * t + Mathf.Sin(t * 9f)) + 0.05f * Noise(i));
        });

        static AudioSource oneShot;
        public static void Play2D(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (oneShot == null)
            {
                var go = new GameObject("Audio2D"); Object.DontDestroyOnLoad(go);
                oneShot = go.AddComponent<AudioSource>(); oneShot.spatialBlend = 0; oneShot.playOnAwake = false;
            }
            oneShot.pitch = pitch; oneShot.PlayOneShot(clip, volume);
        }

        public static void PlayAt(AudioClip clip, Vector3 position, float volume = 1f, float maxDistance = 25f)
        {
            var go = new GameObject("Audio3D_" + clip.name); go.transform.position = position;
            var s = go.AddComponent<AudioSource>(); s.clip = clip; s.volume = volume; s.spatialBlend = 1; s.rolloffMode = AudioRolloffMode.Linear; s.maxDistance = maxDistance; s.minDistance = 1f; s.Play();
            Object.Destroy(go, clip.length + 0.1f);
        }

        public static AudioSource Loop(GameObject host, AudioClip clip, float volume, bool spatial, float maxDistance = 15f)
        {
            var s = host.AddComponent<AudioSource>(); s.clip = clip; s.loop = true; s.volume = volume; s.playOnAwake = false;
            s.spatialBlend = spatial ? 1 : 0; s.rolloffMode = AudioRolloffMode.Linear; s.maxDistance = maxDistance; s.minDistance = 1f; s.Play();
            return s;
        }
    }
}
