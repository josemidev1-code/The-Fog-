using System.Collections.Generic;
using UnityEngine;

namespace TheFog
{
    internal sealed partial class ValdaraLayout
    {
        static readonly Vector3 DanielSpawn = new Vector3(0f, 0.12f, -23.5f);
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
            BuildDanielTower(Group("04 · Torre de vigilancia"));
            Player(DanielSpawn, 0f, false, true);
        }

        void BuildDanielGround(Transform zone)
        {
            Surface(zone, "Suelo del claro", new Vector3(0, -0.44f, 3f), new Vector3(96f, 0.88f, 103f),
                "Tierra", grassTint, 32, 35);
            Track(zone, "Senda desde Valdara", new Vector3(0, 0, -39f), new Vector3(0, 0, 0.5f), 3.7f);
            Track(zone, "Vereda lateral", new Vector3(0.8f, 0.008f, -6f), new Vector3(11.8f, 0.008f, 3.8f), 2.35f);
            Track(zone, "Acceso a la torre", new Vector3(11.8f, 0.012f, 3.8f), new Vector3(15.54f, 0.012f, 11.15f), 2.1f);
            Track(zone, "Paso a la leñera", new Vector3(-0.9f, 0.016f, -3f), new Vector3(-8.7f, 0.016f, 3.6f), 1.6f);

            Portal(zone, "Volver a Valdara", new Vector3(0, 0.04f, -27.3f), 180f,
                new Vector3(-7f, 0.12f, -15f), 90f, ValdaraAction.Kind.Travel, "Valdara");
            for (int i = 0; i < 4; i++)
            {
                float x = 2.85f + i * 2.07f;
                Model(zone, "Exterior/Valla", new Vector3(x, 0, -27.3f), 2.08f);
                Model(zone, "Exterior/Valla", new Vector3(-x, 0, -27.3f), 2.08f);
            }
            foreach (float x in new[] { -0.76f, 0.76f })
                Surface(zone, "Poste de la linde", new Vector3(x, 1.18f, -27.3f), new Vector3(0.22f, 2.36f, 0.26f),
                    "Madera", timberTint, 1, 2);

            // Los límites físicos quedan detrás de la primera fila de árboles.
            Collision(zone, "Límite oeste", new Vector3(-32, 2.5f, 3), new Vector3(1, 5, 73));
            Collision(zone, "Límite este", new Vector3(32, 2.5f, 3), new Vector3(1, 5, 73));
            Collision(zone, "Límite norte", new Vector3(0, 2.5f, 38), new Vector3(65, 5, 1));
            Collision(zone, "Límite sur", new Vector3(0, 2.5f, -32), new Vector3(65, 5, 1));

            var random = new System.Random(616);
            for (int i = 0; i < 10; i++)
            {
                float z = -28f + 6.5f * i;
                Tree(zone, random, -28.4f, z);
                Tree(zone, random, 28.4f, z + 1.1f);
                float x = -28.1f + 6.2f * i;
                Tree(zone, random, x, 33.5f);
                if (Mathf.Abs(x) > 4f)
                    Tree(zone, random, x, -29.4f);
            }

            foreach (Vector3 position in new[]
            {
                new Vector3(-7, 0, -17), new Vector3(8, 0, -20), new Vector3(-11, 0, -7),
                new Vector3(12, 0, -10), new Vector3(-16, 0, 5), new Vector3(23, 0, 7),
                new Vector3(-9, 0, 15), new Vector3(3, 0, 17), new Vector3(23, 0, 20)
            })
                Tree(zone, random, position.x, position.z);

            // La senda continúa visualmente entre los árboles del lado de Valdara.
            foreach (float x in new[] { -13.5f, -8.6f, -4f, 4f, 8.6f, 13.5f })
                Tree(zone, random, x, -36.5f);
            Tree(zone, random, 0.7f, -42f);

            Model(zone, "Exterior/Roca", new Vector3(7.7f, 0, -11f), 1.85f, 42f);
            Model(zone, "Exterior/Roca", new Vector3(-11.8f, 0, 14.7f), 2.15f, 12f);
            Model(zone, "Exterior/Roca", new Vector3(18.9f, 0, 8.7f), 1.35f, 81f);
            Sign(zone, "TORRE   →", new Vector3(2.45f, 1.31f, -6.2f), 1.55f, -7f);
            Sign(zone, "CASA DE DANIEL", new Vector3(-2.75f, 1.38f, -6.9f), 2.35f, 8f);

            BuildWoodshed(zone);
        }

        void Tree(Transform zone, System.Random random, float x, float z)
        {
            x += (float)random.NextDouble() * 2.7f - 1.35f;
            z += (float)random.NextDouble() * 2.7f - 1.35f;
            float height = 5.5f + (float)random.NextDouble() * 3.3f;
            Model(zone, "Exterior/Roble", new Vector3(x, 0, z), height, (float)random.NextDouble() * 360f, false);
            Collision(zone, "Tronco", new Vector3(x, 1.3f, z), new Vector3(0.65f, 2.6f, 0.65f));
            Model(zone, "Exterior/Arbusto", new Vector3(x + 1.32f, 0, z - 0.95f), 1.55f,
                (float)random.NextDouble() * 360f, false);
        }

        void BuildWoodshed(Transform zone)
        {
            Surface(zone, "Piso de la leñera", new Vector3(-9, 0.06f, 4.6f), new Vector3(3.2f, 0.12f, 2.75f),
                "Madera", timberTint, 2, 3);
            Surface(zone, "Fondo de la leñera", new Vector3(-9, 1.05f, 5.9f), new Vector3(3.2f, 2.1f, 0.17f),
                "Madera", timberTint, 4, 2);
            for (int i = 0; i < 2; i++)
            {
                Surface(zone, "Pilar de la leñera", new Vector3(-10.5f + 3f * i, 1.1f, 3.32f),
                    new Vector3(0.17f, 2.2f, 0.17f), "Madera", timberTint, 1, 2);
                Model(zone, "Exterior/Valla", new Vector3(-9.95f + 2.1f * i, 0, 12.6f), 2.1f);
            }
            var roof = Surface(zone, "Chapa de la leñera", new Vector3(-9, 2.2f, 4.61f), new Vector3(3.6f, 0.16f, 3.07f),
                "Chapa", timberTint, 4, 3);
            roof.transform.rotation = Quaternion.Euler(-6f, 0f, 0f);
            for (int i = 0; i < 3; i++)
                Surface(zone, "Pila de tablas", new Vector3(-9.38f, 0.22f + i * 0.18f, 5.2f),
                    new Vector3(1.65f - i * 0.16f, 0.18f, 0.58f), "Madera", timberTint, 2, 1);
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
                material.SetFloat("_Glossiness", 0.1f);
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
