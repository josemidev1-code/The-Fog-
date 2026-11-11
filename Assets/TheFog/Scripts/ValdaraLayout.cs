using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace TheFog
{
    internal sealed partial class ValdaraLayout
    {
        readonly Transform stage;
        readonly Shader shader;
        readonly Func<GameObject, Transform, GameObject> instantiateModel;
        readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();

        static readonly Color soil = new Color(0.15f, 0.18f, 0.14f);
        static readonly Color gravel = new Color(0.32f, 0.30f, 0.24f);
        static readonly Color plaster = new Color(0.43f, 0.39f, 0.31f);
        static readonly Color wood = new Color(0.24f, 0.16f, 0.11f);
        static readonly Color ink = new Color(0.075f, 0.088f, 0.082f);
        static readonly Color brass = new Color(1f, 0.81f, 0.46f);
        static readonly Color fog = new Color(0.34f, 0.43f, 0.46f);

        public ValdaraLayout(Transform stage, Shader shader, Func<GameObject, Transform, GameObject> instantiateModel)
        {
            this.stage = stage;
            this.shader = GraphicsSettings.currentRenderPipeline != null
                ? Shader.Find("Universal Render Pipeline/Lit")
                : shader != null ? shader : Shader.Find("Standard");
            this.instantiateModel = instantiateModel;
        }

        public void BuildValdara()
        {
            Environment(fog, true);
            BuildExterior(Group("01  ·  Camino de Valdara"));
            BuildMarket(Group("03  ·  Ultramarinos"));
            BuildMine(Group("04  ·  Galería de la mina"));
            Player(new Vector3(0, 0.15f, -38f), 0f, false);
        }

        void Environment(Color background, bool mist)
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = mist ? new Color(0.33f, 0.38f, 0.40f) : new Color(0.58f, 0.61f, 0.65f);
            RenderSettings.fog = mist;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = background;
            RenderSettings.fogDensity = 0.033f;
            var sun = Group("Luz ambiental").gameObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = mist ? new Color(0.66f, 0.76f, 0.89f) : Color.white;
            sun.intensity = mist ? 0.48f : 0.95f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
            RenderSettings.sun = sun;
        }

        void Player(Vector3 position, float yaw, bool catalog, bool daniel = false)
        {
            var player = Group("Jugador");
            player.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            var controller = player.gameObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.center = Vector3.up * 0.9f;
            controller.radius = 0.31f;
            controller.stepOffset = 0.26f;

            var head = new GameObject("Cámara");
            head.tag = "MainCamera";
            head.transform.SetParent(player, false);
            head.transform.localPosition = Vector3.up * 1.62f;
            var camera = head.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = daniel ? RenderSettings.fogColor : catalog ? ink : fog;
            camera.nearClipPlane = 0.035f;
            camera.farClipPlane = 85f;
            camera.fieldOfView = 67f;
            head.AddComponent<AudioListener>();

            var lamp = new GameObject("Haz de la linterna");
            lamp.transform.SetParent(head.transform, false);
            var beam = lamp.AddComponent<Light>();
            beam.type = LightType.Spot;
            beam.renderMode = LightRenderMode.ForcePixel;
            beam.range = 26f;
            beam.intensity = 4f;
            beam.spotAngle = 49f;
            beam.innerSpotAngle = 28f;
            beam.color = new Color(0.94f, 0.91f, 0.78f);
            beam.shadows = LightShadows.Hard;
            beam.enabled = false;

            if (daniel)
                head.AddComponent<DanielRetroCamera>();
            player.gameObject.AddComponent<ValdaraPlayer>().Configure(camera, beam, catalog, position, yaw, daniel);
        }

        Transform Group(string name)
        {
            var group = new GameObject(name);
            group.transform.SetParent(stage, false);
            return group.transform;
        }

        GameObject Box(Transform parent, string name, Vector3 position, Vector3 size, Color color, float yaw = 0f)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            block.transform.localScale = size;
            if (!materials.TryGetValue(color, out Material material))
            {
                material = new Material(shader) { name = "Valdara-" + ColorUtility.ToHtmlStringRGB(color), color = color };
                material.SetFloat("_Smoothness", 0.13f);
                materials.Add(color, material);
            }
            block.GetComponent<Renderer>().sharedMaterial = material;
            return block;
        }

        GameObject Model(Transform parent, string path, Vector3 foot, float longestSide, float yaw = 0f, bool solid = true)
        {
            var asset = Resources.Load<GameObject>("Modelos/" + path);
            if (asset == null)
                throw new InvalidOperationException("No se pudo importar el modelo " + path + ". Revisa glTFast y la consola.");

            var root = new GameObject(path.Substring(path.LastIndexOf('/') + 1));
            root.transform.SetParent(parent, false);
            var instance = instantiateModel(asset, root.transform);
            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                throw new InvalidOperationException("El modelo " + path + " no contiene ninguna malla visible.");

            var bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
                bounds.Encapsulate(renderer.bounds);
            float max = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (max < 0.0001f)
                throw new InvalidOperationException("El modelo " + path + " tiene dimensiones nulas.");

            float scale = longestSide / max;
            instance.transform.localScale *= scale;
            instance.transform.localPosition = (instance.transform.localPosition - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z)) * scale;
            root.transform.SetPositionAndRotation(foot, Quaternion.Euler(0f, yaw, 0f));
            if (solid)
            {
                var collider = root.AddComponent<BoxCollider>();
                collider.size = bounds.size * scale;
                collider.center = Vector3.up * (collider.size.y * 0.5f);
            }
            return root;
        }

        static void Text(Transform parent, string value, Vector3 point, float size, float yaw = 0f)
        {
            var title = new GameObject(value);
            title.transform.SetParent(parent, false);
            title.transform.SetPositionAndRotation(point, Quaternion.Euler(0f, yaw, 0f));
            var text = title.AddComponent<TextMesh>();
            text.text = value;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 64;
            text.characterSize = size;
            text.color = brass;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
        }

        void Sign(Transform parent, string words, Vector3 position, float width = 2.1f, float yaw = 0f)
        {
            var rotation = Quaternion.Euler(0f, yaw, 0f);
            Box(parent, words, position, new Vector3(width, 0.55f, 0.10f), ink, yaw);
            Text(parent, words, position + rotation * new Vector3(0f, 0f, -0.061f), 0.036f, yaw);
        }

        static void Point(Transform parent, string name, Vector3 point, Color color, float range, float intensity)
        {
            var holder = new GameObject(name);
            holder.transform.SetParent(parent, false);
            holder.transform.position = point;
            var light = holder.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
        }

        void Portal(Transform parent, string label, Vector3 floor, float yaw, Vector3 destination, float facing,
            ValdaraAction.Kind kind = ValdaraAction.Kind.Door, string destinationScene = null)
        {
            var door = Box(parent, label, floor + Vector3.up * 1.1f, new Vector3(1.25f, 2.2f, 0.24f), wood, yaw);
            var action = door.AddComponent<ValdaraAction>();
            action.kind = kind;
            action.prompt = label;
            action.destination = destination;
            action.facing = facing;
            action.destinationScene = destinationScene;
            Sign(parent, label, floor + Vector3.up * 2.43f, 2.35f, yaw);
            Point(parent, "Luz de acceso", floor + Vector3.up * 2.55f + Quaternion.Euler(0f, yaw, 0f) * Vector3.back,
                brass, 4f, 1.25f);
        }
    }
}
