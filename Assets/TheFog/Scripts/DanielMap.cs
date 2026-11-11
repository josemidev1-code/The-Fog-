using System.Collections.Generic;
using UnityEngine;

namespace TheFog
{
    internal sealed partial class ValdaraLayout
    {
        static readonly Vector3 DanielSpawn = new Vector3(-0.47f, 0.12f, -2f);
        static readonly Vector3 TowerBase = new Vector3(16.4f, 0f, 14f);
        static readonly Color danielFog = new Color(0.27f, 0.29f, 0.22f);
        static readonly Color timberTint = new Color(0.74f, 0.68f, 0.53f);
        static readonly Color grassTint = new Color(0.53f, 0.58f, 0.39f);
        static readonly Color dirtTint = new Color(0.67f, 0.58f, 0.41f);
        readonly Dictionary<string, Material> danielSurfaces = new Dictionary<string, Material>();

        public void BuildDanielMap()
        {
            Environment(danielFog, true);
            RenderSettings.fogDensity = 0.046f;
            RenderSettings.ambientLight = new Color(0.37f, 0.36f, 0.29f);
            RenderSettings.sun.color = new Color(0.77f, 0.83f, 0.65f);
            RenderSettings.sun.intensity = 0.58f;

            BuildDanielGround(Group("01 · Linde y claro"));
            BuildDanielHouse(Group("02 · Fachada de Daniel"));
            Player(DanielSpawn, 0f, false, true);
        }

        void BuildDanielGround(Transform zone)
        {
            Surface(zone, "Suelo del claro", new Vector3(0, -0.44f, 3f), new Vector3(96f, 0.88f, 103f),
                "Tierra", grassTint, 32, 35);
            Track(zone, "Senda desde Valdara", new Vector3(0, 0, -39f), new Vector3(0, 0, 0.5f), 3.7f);

            Portal(zone, "Volver a Valdara", new Vector3(0, 0.04f, -27.3f), 180f,
                new Vector3(-7f, 0.12f, -15f), 90f, ValdaraAction.Kind.Travel, "Valdara", showSign: false);
            for (int i = 0; i < 4; i++)
            {
                float x = 2.85f + i * 2.07f;
                Model(zone, "Exterior/Valla", new Vector3(x, 0, -27.3f), 2.08f);
                Model(zone, "Exterior/Valla", new Vector3(-x, 0, -27.3f), 2.08f);
            }
            Model(zone, "Exterior/Valla", new Vector3(-10.10f, 0, -26.25f), 2.1f, 90f);
            Model(zone, "Exterior/Valla", new Vector3(10.10f, 0, -26.25f), 2.1f, 90f);
            foreach (float x in new[] { -0.76f, 0.76f })
                Surface(zone, "Poste de la linde", new Vector3(x, 1.18f, -27.3f), new Vector3(0.22f, 2.36f, 0.26f),
                    "Madera", timberTint, 1, 2);

            // Límites físicos del área de prueba.
            Collision(zone, "Límite oeste", new Vector3(-32, 2.5f, 3), new Vector3(1, 5, 73));
            Collision(zone, "Límite este", new Vector3(32, 2.5f, 3), new Vector3(1, 5, 73));
            Collision(zone, "Límite norte", new Vector3(0, 2.5f, 38), new Vector3(65, 5, 1));
            Collision(zone, "Límite sur", new Vector3(0, 2.5f, -32), new Vector3(65, 5, 1));

            Model(zone, "Exterior/Roca", new Vector3(7.7f, 0, -11f), 1.85f, 42f);
            Model(zone, "Exterior/Roca", new Vector3(-11.8f, 0, 14.7f), 2.15f, 12f);
            Model(zone, "Exterior/Roca", new Vector3(18.9f, 0, 8.7f), 1.35f, 81f);
        }

        void Track(Transform zone, string name, Vector3 start, Vector3 end, float width)
        {
            Vector3 delta = end - start;
            float angle = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
            Surface(zone, name, (start + end) * 0.5f - Vector3.up * 0.018f, new Vector3(width, 0.065f, delta.magnitude),
                "Tierra", dirtTint, 2, Mathf.CeilToInt(delta.magnitude / 2f), angle);
        }

        GameObject Surface(Transform zone, string name, Vector3 point, Vector3 size, string textureName,
            Color tint, int repeatX, int repeatY, float yaw = 0f)
        {
            var block = Box(zone, name, point, size, Color.white, yaw);
            string key = textureName + "-" + ColorUtility.ToHtmlStringRGB(tint) + "-" + repeatX + "x" + repeatY;
            if (!danielSurfaces.TryGetValue(key, out Material material))
            {
                material = new Material(shader)
                {
                    name = "Valdara-Daniel-" + key,
                    color = tint,
                    mainTexture = Resources.Load<Texture2D>("Daniel/" + textureName),
                    mainTextureScale = new Vector2(repeatX, repeatY)
                };
                material.SetFloat("_Smoothness", 0.1f);
                danielSurfaces.Add(key, material);
            }
            block.GetComponent<Renderer>().sharedMaterial = material;
            return block;
        }

        static GameObject Collision(Transform zone, string name, Vector3 point, Vector3 size)
        {
            var shape = new GameObject(name);
            shape.transform.SetParent(zone, false);
            shape.transform.position = point;
            shape.AddComponent<BoxCollider>().size = size;
            return shape;
        }
    }
}
