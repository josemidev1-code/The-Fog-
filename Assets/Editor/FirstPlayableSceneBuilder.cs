using System;
using System.Collections.Generic;
using System.IO;
using TheFog.FirstPlayable;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheFog.EditorTools
{
    /// <summary>
    /// Construye y guarda Assets/Scenes/FirstPlayable.unity con los modelos existentes.
    /// La escena resultante es persistente y editable: este constructor solo hace falta para regenerarla.
    /// - Menú: THE FOG > First Playable > Rebuild scene (sobrescribe la escena).
    /// - Si la escena todavía no existe, se construye una sola vez al cargar el editor.
    /// - Lote: -executeMethod TheFog.EditorTools.FirstPlayableSceneBuilder.BuildBatch
    /// No modifica los GLB ni sus .meta: instancia los modelos y normaliza la escala en las instancias.
    /// </summary>
    public static class FirstPlayableSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/FirstPlayable.unity";
        const string ArtFolder = "Assets/Art/FirstPlayable";
        const string ReportPath = "docs/first-playable-build-report.json";
        const string PreviewPlayerPath = "docs/first-playable-preview-player.png";
        const string PreviewOverviewPath = "docs/first-playable-preview-overview.png";

        const string Optimized = "Assets/Models/Optimized/";
        const string Environments = "Assets/Models/Environments/";
        const string Props = "Assets/Models/Props/";

        const string HouseFile = "ENV_CasaDaniel.glb";
        const string MineFile = "ENV_EntradaMina.glb";
        const string TreeFile = "ENV_Arbofrondoso-ROBLO DE VALDARA.glb";
        const string BushFile = "ENV_Arbusto-ARBUSTO DE VALDARA.glb";
        const string RockFile = "ENV_Roca-ROCA NATURAL DE VALDARA.glb";
        const string FenceFile = "ENV_VallaMadera-VALLA RURAL DE VALDARA.glb";
        const string LampFile = "ENV_Farola-FAROLA DE VALDARA.glb";
        const string SignFile = "ENV_CartelValdara-CARTEL DE ENTRADA A VALDARA.glb";
        const string FlashlightFile = "OBJ_Linterna.glb";

        static readonly Color FogColor = new Color(0.36f, 0.41f, 0.37f, 1f);
        static readonly Color AmbientColor = new Color(0.20f, 0.24f, 0.22f, 1f);
        static readonly Color WarmLight = new Color(1f, 0.74f, 0.42f, 1f);

        // Camino de prueba (x, z) desde la casa hasta la mina.
        static readonly Vector2[] PathPoints =
        {
            new Vector2(-6f, 2f), new Vector2(0f, 0f), new Vector2(1.5f, 12f),
            new Vector2(-2f, 24f), new Vector2(2f, 36f), new Vector2(3f, 45f)
        };
        static readonly Vector3 SpawnPosition = new Vector3(0f, 0.05f, 0f);
        static readonly Vector3 HousePosition = new Vector3(-13f, 0f, 3f);
        static readonly Vector3 MinePosition = new Vector3(3f, 0f, 50f);

        enum ColliderKind { None, Box, Trunk }

        [Serializable]
        public class PlacedEntry
        {
            public string name;
            public string asset;
            public Vector3 sourceSize;
            public float appliedScale;
            public Vector3 finalSize;
            public string collider;
        }

        [Serializable]
        public class BuildReport
        {
            public string unityVersion;
            public string builtAtUtc;
            public string scene = ScenePath;
            public bool saved;
            public bool previewPlayer;
            public bool previewOverview;
            public List<string> warnings = new List<string>();
            public List<PlacedEntry> placed = new List<PlacedEntry>();
        }

        class Placed
        {
            public GameObject root;
            public Vector3 size;
        }

        static BuildReport report;

        // ------------------------------------------------------------------ entradas

        [InitializeOnLoadMethod]
        static void AutoBuildOnce()
        {
            if (Application.isBatchMode) return;
            if (File.Exists(ScenePath)) return;
            EditorApplication.delayCall += delegate
            {
                if (File.Exists(ScenePath)) return;
                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
                if (SceneManager.GetActiveScene().isDirty)
                {
                    Debug.LogWarning("THE FOG: la escena abierta tiene cambios sin guardar; usa THE FOG > First Playable > Rebuild scene cuando quieras crear FirstPlayable.");
                    return;
                }
                Build();
            };
        }

        [MenuItem("THE FOG/First Playable/Rebuild scene (overwrites FirstPlayable.unity)")]
        public static void RebuildFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("THE FOG",
                    "Se va a regenerar " + ScenePath + " y se perderán los cambios hechos a mano en esa escena.", "Regenerar", "Cancelar"))
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        public static void BuildBatch()
        {
            bool ok = Build();
            if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1);
        }

        // ------------------------------------------------------------------ construcción

        public static bool Build()
        {
            report = new BuildReport { unityVersion = Application.unityVersion, builtAtUtc = DateTime.UtcNow.ToString("o") };
            bool ok = false;
            try
            {
                AssetDatabase.Refresh();
                ok = BuildInternal();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                report.warnings.Add("EXCEPTION: " + e);
            }
            try
            {
                Directory.CreateDirectory("docs");
                File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
            }
            catch (Exception e) { Debug.LogException(e); }
            Debug.Log("THE FOG FIRST PLAYABLE: " + (ok ? "PASS" : "FAIL") + " (" + report.warnings.Count + " avisos). Informe: " + ReportPath);
            return ok;
        }

        static bool BuildInternal()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Transform ambienceRoot = Group("AMBIENTE");
            Transform environmentRoot = Group("ENTORNO");
            Transform gameRoot = Group("JUEGO");

            // ---- ambiente
            RenderSettings.skybox = null;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = FogColor;
            RenderSettings.fogDensity = 0.04f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = AmbientColor;
            RenderSettings.reflectionIntensity = 0.3f;

            GameObject sunGo = new GameObject("LuzAmbiente_Direccional");
            sunGo.transform.SetParent(ambienceRoot, false);
            sunGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.62f, 0.70f, 0.66f, 1f);
            sun.intensity = 0.45f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.6f;
            RenderSettings.sun = sun;

            // ---- suelo y camino
            Texture2D groundTexture = CreateGroundTexture();
            Material groundMaterial = CreateMaterial("Suelo", new Color(0.33f, 0.36f, 0.26f, 1f), groundTexture, 50f);
            Material pathMaterial = CreateMaterial("Camino", new Color(0.30f, 0.25f, 0.19f, 1f), groundTexture, 2f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Suelo";
            ground.transform.SetParent(environmentRoot, false);
            ground.transform.position = new Vector3(0f, 0f, 17f);
            ground.transform.localScale = new Vector3(10f, 1f, 10f);
            ground.GetComponent<MeshRenderer>().sharedMaterial = groundMaterial;
            ground.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            Transform pathRoot = Group("Camino", environmentRoot);
            for (int i = 0; i < PathPoints.Length - 1; i++) CreatePathSegment(pathRoot, PathPoints[i], PathPoints[i + 1], 2.4f, pathMaterial, i);

            CreateBoundary(environmentRoot, -42f, 42f, -26f, 60f);

            // ---- hitos
            Transform landmarks = Group("Hitos", environmentRoot);
            Placed house = Place(Resolve(HouseFile, Environments), "CasaDaniel", landmarks, HousePosition, 90f, 6f, 14f, ColliderKind.Box, 0.92f);
            Placed mine = Place(Resolve(MineFile, Environments), "EntradaMina", landmarks, MinePosition, 180f, 4.5f, 12f, ColliderKind.Box, 0.85f);
            Placed sign = Place(Resolve(SignFile, Environments), "CartelValdara", landmarks, new Vector3(4.2f, 0f, 9f), 200f, 2.6f, 4f, ColliderKind.Box, 0.9f);

            // ---- farolas con luz cálida
            Transform lamps = Group("Farolas", environmentRoot);
            List<Light> decorative = new List<Light>();
            Vector3[] lampPositions = { new Vector3(-3.2f, 0f, 4.5f), new Vector3(5.6f, 0f, 39f) };
            for (int i = 0; i < lampPositions.Length; i++)
            {
                Placed lamp = Place(Resolve(LampFile, Environments), "Farola_" + (i + 1), lamps, lampPositions[i], 0f, 4.2f, 3f, ColliderKind.Trunk, 1f);
                Transform parent = lamp != null ? lamp.root.transform : lamps;
                GameObject lightGo = new GameObject("LuzFarola");
                lightGo.transform.SetParent(parent, false);
                float height = lamp != null ? lamp.size.y : 4.2f;
                lightGo.transform.localPosition = new Vector3(0f, height * 0.9f, 0.5f);
                if (lamp == null) lightGo.transform.position = lampPositions[i] + new Vector3(0f, 3.8f, 0f);
                Light l = lightGo.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = WarmLight;
                l.intensity = 22f;
                l.range = 13f;
                l.shadows = LightShadows.None;
                decorative.Add(l);
            }

            // ---- valla junto al camino y junto a la casa
            Transform fences = Group("Vallas", environmentRoot);
            PlaceRow(Resolve(FenceFile, Environments), "Valla_Camino", fences, new Vector3(-5.5f, 0f, 13f), new Vector3(-0.25f, 0f, 1f), 5, 1.25f);
            PlaceRow(Resolve(FenceFile, Environments), "Valla_Casa", fences, new Vector3(-8f, 0f, -6f), new Vector3(1f, 0f, 0f), 4, 1.25f);

            // ---- rocas grandes alrededor de la mina (sugieren ladera, sin inventar geometría nueva)
            Transform rocks = Group("Rocas", environmentRoot);
            string rockPath = Resolve(RockFile, Environments);
            Vector3[] bigRocks =
            {
                MinePosition + new Vector3(-8f, 0f, 1.5f), MinePosition + new Vector3(8.5f, 0f, 2f),
                MinePosition + new Vector3(-12f, 0f, 4.5f), MinePosition + new Vector3(13f, 0f, 5f),
                MinePosition + new Vector3(0f, 0f, 8f)
            };
            float[] bigRockHeights = { 3.2f, 3.6f, 4.2f, 3.9f, 5f };
            for (int i = 0; i < bigRocks.Length; i++)
                Place(rockPath, "RocaGrande_" + (i + 1), rocks, bigRocks[i], 37f * (i + 1), bigRockHeights[i], 9f, ColliderKind.Box, 0.75f);

            // ---- vegetación y rocas dispersas (semilla fija: el resultado es reproducible)
            System.Random random = new System.Random(1989);
            List<Vector3> occupied = new List<Vector3>();
            Transform trees = Group("Arboles", environmentRoot);
            string treePath = Resolve(TreeFile, Environments);
            foreach (Vector3 p in Scatter(random, 18, 5.5f, 7f, occupied))
                Place(treePath, "Roble", trees, p, Range(random, 0f, 360f), Range(random, 8f, 11f), 12f, ColliderKind.Trunk, 1f);

            Transform bushes = Group("Arbustos", environmentRoot);
            string bushPath = Resolve(BushFile, Environments);
            foreach (Vector3 p in Scatter(random, 26, 2.2f, 2.5f, occupied))
                Place(bushPath, "Arbusto", bushes, p, Range(random, 0f, 360f), Range(random, 0.9f, 1.5f), 3f, ColliderKind.None, 1f);

            foreach (Vector3 p in Scatter(random, 14, 2.6f, 3f, occupied))
            {
                float h = Range(random, 0.5f, 1.9f);
                Place(rockPath, "Roca", rocks, p, Range(random, 0f, 360f), h, 4f, h > 0.8f ? ColliderKind.Box : ColliderKind.None, 0.7f);
            }

            // ---- jugador
            GameObject player = new GameObject("Jugador");
            player.transform.position = SpawnPosition;
            player.transform.rotation = Quaternion.Euler(0f, 5f, 0f);
            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.3f;
            cc.slopeLimit = 45f;
            FogInput input = player.AddComponent<FogInput>();
            input.SetupDefaults();
            FogPlayerController controller = player.AddComponent<FogPlayerController>();
            controller.input = input;

            GameObject cameraGo = new GameObject("Camara");
            cameraGo.tag = "MainCamera";
            cameraGo.transform.SetParent(player.transform, false);
            cameraGo.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.fieldOfView = 65f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 75f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = FogColor;
            cameraGo.AddComponent<AudioListener>();
            camera.GetUniversalAdditionalCameraData();
            controller.cameraPivot = cameraGo.transform;

            // ---- linterna: modelo existente + luz focal independiente de la orientación del modelo
            GameObject flashlightGo = new GameObject("Linterna");
            flashlightGo.transform.SetParent(cameraGo.transform, false);
            flashlightGo.transform.localPosition = new Vector3(0.26f, -0.24f, 0.42f);
            PlaceFlashlightModel(Resolve(FlashlightFile, Props), flashlightGo.transform);

            GameObject beamGo = new GameObject("LuzLinterna");
            beamGo.transform.SetParent(flashlightGo.transform, false);
            beamGo.transform.localPosition = new Vector3(0f, 0.02f, 0.16f);
            Light beam = beamGo.AddComponent<Light>();
            beam.type = LightType.Spot;
            beam.color = new Color(1f, 0.93f, 0.78f, 1f);
            beam.intensity = 45f;
            beam.range = 24f;
            beam.spotAngle = 58f;
            beam.innerSpotAngle = 28f;
            beam.shadows = LightShadows.None;

            FogFlashlight flashlight = flashlightGo.AddComponent<FogFlashlight>();
            flashlight.input = input;
            flashlight.beam = beam;

            // ---- UI
            FogHud hud = BuildHud();

            FogInteractor interactor = cameraGo.AddComponent<FogInteractor>();
            interactor.input = input;
            interactor.hud = hud;
            interactor.rayOrigin = cameraGo.transform;

            // ---- juego: ambiente, gráficos, pausa y recorrido provisional
            FogAmbience ambience = ambienceRoot.gameObject.AddComponent<FogAmbience>();
            ambience.fogColor = FogColor;
            ambience.fogDensity = 0.04f;
            ambience.ambientColor = AmbientColor;
            ambience.targetCamera = camera;
            ambience.farClipPlane = 75f;

            FogGraphicsSettings graphics = gameRoot.gameObject.AddComponent<FogGraphicsSettings>();
            graphics.input = input;
            graphics.hud = hud;
            graphics.targetCamera = camera;
            graphics.sun = sun;
            graphics.flashlightBeam = beam;
            graphics.decorativeLights = decorative.ToArray();

            FogPauseController pause = gameRoot.gameObject.AddComponent<FogPauseController>();
            pause.input = input;
            pause.hud = hud;
            pause.player = controller;
            pause.flashlight = flashlight;
            pause.interactor = interactor;

            FogTestRoute route = gameRoot.gameObject.AddComponent<FogTestRoute>();
            route.hud = hud;

            // Interacción provisional 1: examinar la entrada de la mina (completa el recorrido de prueba).
            float mineDepth = mine != null ? mine.size.z : 6f;
            float mineWidth = mine != null ? mine.size.x : 6f;
            GameObject mineTrigger = new GameObject("Interaccion_Mina_PROVISIONAL");
            mineTrigger.transform.SetParent(gameRoot, false);
            mineTrigger.transform.position = MinePosition + new Vector3(0f, 1.3f, -(mineDepth * 0.5f) - 0.4f);
            BoxCollider mineBox = mineTrigger.AddComponent<BoxCollider>();
            mineBox.isTrigger = true;
            mineBox.size = new Vector3(Mathf.Max(3f, mineWidth), 2.6f, 2.2f);
            FogInteractable mineInteractable = mineTrigger.AddComponent<FogInteractable>();
            mineInteractable.prompt = "Examinar la entrada de la mina";
            mineInteractable.message = "[Provisional] Entrada de la mina. Aquí irá una interacción real cuando esté aprobada.";
            mineInteractable.singleUse = true;
            mineInteractable.completesRoute = route;

            // Interacción provisional 2: mirar el cartel.
            if (sign != null)
            {
                FogInteractable signInteractable = sign.root.AddComponent<FogInteractable>();
                signInteractable.prompt = "Mirar el cartel";
                signInteractable.message = "[Provisional] Cartel de entrada a Valdara. Texto pendiente.";
                signInteractable.messageSeconds = 4f;
            }

            if (house != null) CheckSpawnClear(house.root, "CasaDaniel");

            // ---- guardar
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            report.saved = saved;
            if (saved) AddToBuildSettings();
            AssetDatabase.SaveAssets();

            report.previewPlayer = Capture(camera, PreviewPlayerPath, true);
            GameObject tempGo = new GameObject("TEMP_Overview") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                Camera overview = tempGo.AddComponent<Camera>();
                overview.transform.position = new Vector3(-30f, 42f, -28f);
                overview.transform.LookAt(new Vector3(0f, 0f, 22f));
                overview.fieldOfView = 55f;
                overview.farClipPlane = 300f;
                overview.clearFlags = CameraClearFlags.SolidColor;
                overview.backgroundColor = FogColor;
                report.previewOverview = Capture(overview, PreviewOverviewPath, false);
            }
            finally { UnityEngine.Object.DestroyImmediate(tempGo); }

            return saved;
        }

        // ------------------------------------------------------------------ utilidades de escena

        static Transform Group(string name, Transform parent = null)
        {
            GameObject go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            return go.transform;
        }

        static string Resolve(string file, string originalFolder)
        {
            string optimized = Optimized + file;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(optimized) != null) return optimized;
            report.warnings.Add("Sin copia optimizada importada para " + file + "; se usa el original.");
            return originalFolder + file;
        }

        static float Range(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        static bool TryMeasure(GameObject go, out Bounds bounds)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            bounds = new Bounds(go.transform.position, Vector3.zero);
            if (renderers.Length == 0) return false;
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds.size.y > 0.0001f;
        }

        /// <summary>
        /// Instancia un GLB dentro de un objeto contenedor, normaliza su altura (limitando la huella) sobre la instancia,
        /// lo apoya en el suelo centrado en el contenedor y añade un colisionador simplificado al contenedor.
        /// </summary>
        static Placed Place(string assetPath, string name, Transform parent, Vector3 position, float yaw,
                            float targetHeight, float maxFootprint, ColliderKind collider, float shrink)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset == null)
            {
                report.warnings.Add("No se pudo cargar " + assetPath + " (" + name + ").");
                return null;
            }

            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;

            Bounds b;
            Vector3 size = new Vector3(1f, targetHeight, 1f);
            float scale = 1f;
            if (TryMeasure(model, out b))
            {
                scale = targetHeight / b.size.y;
                float footprint = Mathf.Max(b.size.x, b.size.z);
                if (footprint * scale > maxFootprint) scale = maxFootprint / footprint;
                Vector3 offset = b.center - root.transform.position;
                offset.y = b.min.y - root.transform.position.y;
                model.transform.localScale = model.transform.localScale * scale;
                model.transform.localPosition = -offset * scale;
                size = b.size * scale;
            }
            else
            {
                report.warnings.Add(name + ": no se pudieron medir los límites de " + assetPath + "; escala sin normalizar.");
            }

            if (collider == ColliderKind.Box)
            {
                BoxCollider box = root.AddComponent<BoxCollider>();
                box.center = new Vector3(0f, size.y * 0.5f, 0f);
                box.size = new Vector3(size.x * shrink, size.y, size.z * shrink);
            }
            else if (collider == ColliderKind.Trunk)
            {
                CapsuleCollider capsule = root.AddComponent<CapsuleCollider>();
                capsule.direction = 1;
                capsule.radius = Mathf.Clamp(Mathf.Min(size.x, size.z) * 0.07f, 0.2f, 0.7f);
                capsule.height = Mathf.Max(size.y, capsule.radius * 2f);
                capsule.center = new Vector3(0f, capsule.height * 0.5f, 0f);
            }

            root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            report.placed.Add(new PlacedEntry
            {
                name = name, asset = assetPath, sourceSize = b.size, appliedScale = scale, finalSize = size, collider = collider.ToString()
            });
            return new Placed { root = root, size = size };
        }

        /// <summary>Fila de piezas (vallas) alineadas con una dirección, usando la longitud medida de la pieza.</summary>
        static void PlaceRow(string assetPath, string name, Transform parent, Vector3 start, Vector3 direction, int count, float height)
        {
            direction.y = 0f;
            direction.Normalize();
            float heading = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Vector3 cursor = start;
            for (int i = 0; i < count; i++)
            {
                Placed piece = Place(assetPath, name + "_" + (i + 1), parent, cursor, 0f, height, 6f, ColliderKind.Box, 1f);
                if (piece == null) return;
                bool alongX = piece.size.x >= piece.size.z;
                float length = Mathf.Max(0.5f, alongX ? piece.size.x : piece.size.z);
                piece.root.transform.rotation = Quaternion.Euler(0f, alongX ? heading - 90f : heading, 0f);
                piece.root.transform.position = cursor + direction * (length * 0.5f);
                cursor += direction * (length * 0.98f);
            }
        }

        static void PlaceFlashlightModel(string assetPath, Transform parent)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset == null)
            {
                report.warnings.Add("No se pudo cargar la linterna " + assetPath + ".");
                return;
            }
            GameObject pivot = new GameObject("ModeloLinterna");
            pivot.transform.SetParent(parent, false);
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            model.transform.SetParent(pivot.transform, false);
            model.transform.localPosition = Vector3.zero;

            Bounds b;
            float scale = 1f;
            Vector3 sourceSize = Vector3.zero;
            if (TryMeasure(model, out b))
            {
                sourceSize = b.size;
                float longest = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
                scale = 0.24f / longest;
                model.transform.localScale = model.transform.localScale * scale;
                model.transform.localPosition = -(b.center - pivot.transform.position) * scale;
                // El eje más largo del modelo se orienta hacia delante. El sentido (cabeza o culata) hay que revisarlo a ojo.
                if (b.size.x >= b.size.y && b.size.x >= b.size.z) pivot.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                else if (b.size.y >= b.size.x && b.size.y >= b.size.z) pivot.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            else
            {
                report.warnings.Add("Linterna: no se pudieron medir sus límites.");
            }
            foreach (Renderer r in model.GetComponentsInChildren<Renderer>(true))
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            report.placed.Add(new PlacedEntry
            {
                name = "ModeloLinterna", asset = assetPath, sourceSize = sourceSize, appliedScale = scale, finalSize = sourceSize * scale, collider = "None"
            });
        }

        static float DistanceToPath(Vector2 p)
        {
            float best = float.MaxValue;
            for (int i = 0; i < PathPoints.Length - 1; i++)
            {
                Vector2 a = PathPoints[i], b = PathPoints[i + 1];
                Vector2 ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
                best = Mathf.Min(best, Vector2.Distance(p, a + ab * t));
            }
            return best;
        }

        static List<Vector3> Scatter(System.Random random, int count, float pathClearance, float spacing, List<Vector3> occupied)
        {
            List<Vector3> result = new List<Vector3>();
            int attempts = 0;
            while (result.Count < count && attempts < 4000)
            {
                attempts++;
                Vector3 p = new Vector3(Range(random, -38f, 38f), 0f, Range(random, -22f, 56f));
                if (DistanceToPath(new Vector2(p.x, p.z)) < pathClearance) continue;
                if (Vector3.Distance(p, HousePosition) < 11f) continue;
                if (Vector3.Distance(p, MinePosition) < 9f) continue;
                if (p.z > MinePosition.z - 1f && Mathf.Abs(p.x - MinePosition.x) < 15f) continue;
                if (Vector3.Distance(p, SpawnPosition) < 4f) continue;
                bool tooClose = false;
                for (int i = 0; i < occupied.Count; i++)
                {
                    if (Vector3.Distance(p, occupied[i]) < spacing) { tooClose = true; break; }
                }
                if (tooClose) continue;
                occupied.Add(p);
                result.Add(p);
            }
            return result;
        }

        static void CheckSpawnClear(GameObject root, string name)
        {
            Collider c = root.GetComponent<Collider>();
            if (c == null) return;
            Physics.SyncTransforms();
            Bounds b = c.bounds;
            b.Expand(1.5f);
            if (b.Contains(SpawnPosition + Vector3.up))
                report.warnings.Add(name + ": su colisionador queda demasiado cerca del punto de inicio; mueve el modelo o el jugador.");
        }

        static void CreatePathSegment(Transform parent, Vector2 from, Vector2 to, float width, Material material, int index)
        {
            Vector3 a = new Vector3(from.x, 0.02f, from.y);
            Vector3 b = new Vector3(to.x, 0.02f, to.y);
            Vector3 d = b - a;
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Tramo_" + (index + 1);
            UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());
            quad.transform.SetParent(parent, false);
            quad.transform.position = (a + b) * 0.5f;
            quad.transform.rotation = Quaternion.Euler(90f, Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg, 0f);
            quad.transform.localScale = new Vector3(width, d.magnitude + width * 0.5f, 1f);
            MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static void CreateBoundary(Transform parent, float minX, float maxX, float minZ, float maxZ)
        {
            Transform root = Group("Limites_Invisibles", parent);
            float cx = (minX + maxX) * 0.5f, cz = (minZ + maxZ) * 0.5f;
            Wall(root, "Limite_Norte", new Vector3(cx, 2f, maxZ), new Vector3(maxX - minX + 2f, 4f, 1f));
            Wall(root, "Limite_Sur", new Vector3(cx, 2f, minZ), new Vector3(maxX - minX + 2f, 4f, 1f));
            Wall(root, "Limite_Este", new Vector3(maxX, 2f, cz), new Vector3(1f, 4f, maxZ - minZ + 2f));
            Wall(root, "Limite_Oeste", new Vector3(minX, 2f, cz), new Vector3(1f, 4f, maxZ - minZ + 2f));
        }

        static void Wall(Transform parent, string name, Vector3 position, Vector3 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.AddComponent<BoxCollider>().size = size;
        }

        // ------------------------------------------------------------------ recursos propios de la escena

        static Texture2D CreateGroundTexture()
        {
            Directory.CreateDirectory(ArtFolder);
            string path = ArtFolder + "/SueloRuido.png";
            const int n = 64;
            Texture2D texture = new Texture2D(n, n, TextureFormat.RGBA32, false);
            System.Random random = new System.Random(7);
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float v = 0.72f + (float)random.NextDouble() * 0.28f;
                    if (random.NextDouble() < 0.06) v -= 0.18f;
                    texture.SetPixel(x, y, new Color(v, v, v * 0.96f, 1f));
                }
            }
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = true;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Material CreateMaterial(string name, Color color, Texture2D texture, float tiling)
        {
            Directory.CreateDirectory(ArtFolder);
            string path = ArtFolder + "/" + name + ".mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                report.warnings.Add("No se encontró el shader URP Lit; el material " + name + " usa el shader por defecto.");
                shader = Shader.Find("Standard");
            }
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
                material.SetTextureScale("_BaseMap", new Vector2(tiling, tiling));
            }
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.05f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        // ------------------------------------------------------------------ HUD

        static FogHud BuildHud()
        {
            GameObject canvasGo = new GameObject("UI_HUD");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            FogHud hud = canvasGo.AddComponent<FogHud>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Color pale = new Color(0.86f, 0.88f, 0.80f, 1f);
            Color warm = new Color(0.95f, 0.86f, 0.62f, 1f);

            hud.objectiveText = MakeText(canvasGo.transform, "Objetivo", font, 20, TextAnchor.UpperCenter, warm,
                new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(900f, 60f), "");
            hud.controlsText = MakeText(canvasGo.transform, "Controles", font, 15, TextAnchor.LowerLeft, pale,
                new Vector2(0f, 0f), new Vector2(18f, 14f), new Vector2(620f, 70f),
                "WASD moverse · Ratón mirar · Mayús correr\nF linterna · E interactuar · Esc pausa · F1/F2/F3 gráficos");
            hud.promptText = MakeText(canvasGo.transform, "Indicacion", font, 22, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(800f, 40f), "");
            hud.messageText = MakeText(canvasGo.transform, "Mensaje", font, 20, TextAnchor.MiddleCenter, pale,
                new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(900f, 80f), "");

            GameObject crosshair = new GameObject("Punto");
            crosshair.transform.SetParent(canvasGo.transform, false);
            Image dot = crosshair.AddComponent<Image>();
            dot.color = new Color(0.9f, 0.9f, 0.82f, 0.75f);
            dot.raycastTarget = false;
            RectTransform dotRect = dot.rectTransform;
            dotRect.anchorMin = dotRect.anchorMax = dotRect.pivot = new Vector2(0.5f, 0.5f);
            dotRect.anchoredPosition = Vector2.zero;
            dotRect.sizeDelta = new Vector2(4f, 4f);
            hud.crosshair = crosshair;

            GameObject panel = new GameObject("PanelPausa");
            panel.transform.SetParent(canvasGo.transform, false);
            Image background = panel.AddComponent<Image>();
            background.color = new Color(0.04f, 0.06f, 0.05f, 0.80f);
            background.raycastTarget = false;
            RectTransform panelRect = background.rectTransform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
            MakeText(panel.transform, "TextoPausa", font, 26, TextAnchor.MiddleCenter, pale,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 320f),
                "PAUSA\n\nEsc · Continuar\nR · Reiniciar la escena\nQ · Salir\n\nGráficos: F1 Bajo · F2 Medio · F3 Alto");
            panel.SetActive(false);
            hud.pausePanel = panel;

            return hud;
        }

        static Text MakeText(Transform parent, string name, Font font, int size, TextAnchor alignment, Color color,
                             Vector2 anchor, Vector2 position, Vector2 sizeDelta, string content)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.text = content;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = sizeDelta;
            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            return text;
        }

        // ------------------------------------------------------------------ ajustes y capturas

        static void AddToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (int i = 0; i < scenes.Count; i++)
                if (scenes[i].path == ScenePath) return;
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static bool Capture(Camera camera, string path, bool withFog)
        {
            RenderTexture rt = null;
            Texture2D texture = null;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            bool previousFog = RenderSettings.fog;
            try
            {
                RenderSettings.fog = withFog;
                rt = new RenderTexture(1280, 720, 24);
                camera.targetTexture = rt;
                camera.Render();
                RenderTexture.active = rt;
                texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                texture.Apply();
                Directory.CreateDirectory("docs");
                File.WriteAllBytes(path, texture.EncodeToPNG());
                return true;
            }
            catch (Exception e)
            {
                report.warnings.Add("Captura no disponible (" + path + "): " + e.Message);
                return false;
            }
            finally
            {
                RenderSettings.fog = previousFog;
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                if (rt != null) UnityEngine.Object.DestroyImmediate(rt);
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
