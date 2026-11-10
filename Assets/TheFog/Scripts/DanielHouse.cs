using UnityEngine;

namespace TheFog
{
    internal sealed partial class ValdaraLayout
    {
        const float HouseFloor = 0.22f;
        static readonly Color housePlaster = new Color(0.79f, 0.77f, 0.63f);
        static readonly Color insidePlaster = new Color(0.69f, 0.71f, 0.51f);
        static readonly Color roofTint = new Color(0.62f, 0.51f, 0.44f);
        static readonly Color tileTint = new Color(0.76f, 0.77f, 0.63f);
        static readonly Color stoneTint = new Color(0.58f, 0.59f, 0.49f);

        void BuildDanielHouse(Transform zone)
        {
            Surface(zone, "Cimientos", new Vector3(0, 0.08f, 6), new Vector3(10.72f, 0.16f, 9.82f),
                "Piedra", stoneTint, 8, 3);
            Surface(zone, "Tablones de la casa", new Vector3(0, 0.165f, 6), new Vector3(10.6f, 0.11f, 9.6f),
                "Madera", timberTint, 10, 11);
            Surface(zone, "Baldosas de la cocina", new Vector3(3.09f, 0.221f, 3.68f), new Vector3(4.18f, 0.014f, 4.72f),
                "Baldosas", tileTint, 4, 4);
            Surface(zone, "Techo interior", new Vector3(0, 3.15f, 6), new Vector3(10.6f, 0.2f, 9.6f),
                "Madera", timberTint, 11, 10);

            Wall(zone, -5.3f, -4.14f, 1.2f);
            Wall(zone, -2.24f, -0.68f, 1.2f);
            Wall(zone, 0.68f, 2.24f, 1.2f);
            Wall(zone, 4.14f, 5.3f, 1.2f);
            Window(zone, -3.19f, 1.2f, 1.9f);
            Window(zone, 3.19f, 1.2f, 1.9f);
            Wall(zone, -0.68f, 0.68f, 1.2f, 2.38f, 3.12f);

            Wall(zone, -5.3f, 0.75f, 10.8f);
            Wall(zone, 2.9f, 5.3f, 10.8f);
            Window(zone, 1.825f, 10.8f, 2.15f);
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -5.3f : 5.3f;
                Surface(zone, "Muro lateral", new Vector3(side, 1.67f, 6f), new Vector3(9.6f, 2.9f, 0.24f),
                    "Yeso", housePlaster, 7, 2, 90f);
                Surface(zone, "Zócalo de piedra", new Vector3(side, 0.42f, 6f), new Vector3(9.6f, 0.4f, 0.28f),
                    "Piedra", stoneTint, 7, 1, 90f);
                var slope = Surface(zone, "Faldón de chapa", new Vector3(side * 0.52f, 3.98f, 6),
                    new Vector3(5.89f, 0.18f, 10.63f), "Chapa", roofTint, 6, 10);
                slope.transform.rotation = Quaternion.Euler(0f, 0f, i == 0 ? 18f : -18f);
            }

            for (int i = 0; i < 8; i++)
            {
                float y = 3.16f + (i + 0.5f) * 0.21f;
                float width = 9.65f - i * 1.29f;
                foreach (float z in new[] { 1.2f, 10.8f })
                    Surface(zone, "Testero bajo cubierta", new Vector3(0, y, z), new Vector3(width, 0.217f, 0.23f),
                        "Yeso", housePlaster, Mathf.Max(1, Mathf.RoundToInt(width)), 1);
            }
            Surface(zone, "Chimenea", new Vector3(-2.87f, 4.32f, 8.75f), new Vector3(0.76f, 1.92f, 0.72f),
                "Piedra", stoneTint, 2, 2);
            Surface(zone, "Sombrerete", new Vector3(-2.87f, 5.24f, 8.75f), new Vector3(0.94f, 0.16f, 0.9f),
                "Chapa", roofTint, 2, 1);

            Surface(zone, "Escalón", new Vector3(0, 0.058f, -0.58f), new Vector3(2.65f, 0.116f, 0.66f),
                "Piedra", stoneTint, 2, 1);
            Surface(zone, "Porche", new Vector3(0, 0.11f, 0.42f), new Vector3(3.2f, 0.22f, 1.62f),
                "Madera", timberTint, 4, 2);
            Surface(zone, "Marquesina", new Vector3(0, 2.91f, 0.35f), new Vector3(3.51f, 0.16f, 1.97f),
                "Chapa", roofTint, 4, 2);
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                Surface(zone, "Pilar del porche", new Vector3(side * 1.47f, 1.55f, -0.25f),
                    new Vector3(0.17f, 2.65f, 0.17f), "Madera", timberTint, 1, 2);
                Surface(zone, "Jamba de la puerta", new Vector3(side * 0.68f, 1.29f, 1.17f),
                    new Vector3(0.14f, 2.14f, 0.31f), "Madera", timberTint, 1, 2);
            }

            var pivot = new GameObject("Bisagra de la puerta");
            pivot.transform.SetParent(zone, false);
            pivot.transform.position = new Vector3(-0.61f, HouseFloor, 1.2f);
            pivot.AddComponent<DanielDoor>().Configure(106f);
            var leaf = Surface(pivot.transform, "Puerta principal", new Vector3(0, 1.29f, 1.2f),
                new Vector3(1.22f, 2.13f, 0.105f), "Madera", new Color(0.51f, 0.51f, 0.35f), 2, 3);
            var action = leaf.AddComponent<ValdaraAction>();
            action.kind = ValdaraAction.Kind.HingedDoor;
            action.prompt = "Abrir puerta";
            Box(pivot.transform, "Manilla", new Vector3(0.46f, 1.22f, 1.1f),
                new Vector3(0.12f, 0.08f, 0.18f), new Color(0.36f, 0.3f, 0.18f)).layer = 2;

            var porchLamp = new Color(1f, 0.72f, 0.37f);
            Box(zone, "Bombilla del porche", new Vector3(0, 2.66f, 0.63f), new Vector3(0.18f, 0.16f, 0.14f), porchLamp);
            Point(zone, "Luz del porche", new Vector3(0, 2.54f, 0.6f), porchLamp, 7.2f, 2.05f);
            FurnishDanielHouse(zone);
        }

        void Wall(Transform zone, float start, float end, float z, float bottom = HouseFloor, float top = 3.12f,
            bool inside = false)
        {
            Surface(zone, "Pared de yeso", new Vector3((start + end) * 0.5f, (top + bottom) * 0.5f, z),
                new Vector3(end - start, top - bottom, 0.24f), "Yeso", inside ? insidePlaster : housePlaster,
                Mathf.Max(1, Mathf.CeilToInt((end - start) * 0.7f)), 2);
            if (bottom <= HouseFloor && top > 0.62f)
                Surface(zone, "Zócalo", new Vector3((start + end) * 0.5f, 0.42f, z),
                    new Vector3(end - start, 0.4f, 0.28f), inside ? "Madera" : "Piedra",
                    inside ? timberTint : stoneTint, Mathf.Max(1, Mathf.CeilToInt(end - start)), 1);
        }

        void Window(Transform zone, float x, float z, float width)
        {
            Wall(zone, x - width / 2, x + width / 2, z, HouseFloor, 1.12f);
            Wall(zone, x - width / 2, x + width / 2, z, 2.36f, 3.12f);
            Surface(zone, "Vidrio sucio", new Vector3(x, 1.74f, z), new Vector3(width, 1.24f, 0.09f),
                "Yeso", new Color(0.35f, 0.36f, 0.25f), 2, 1);
            Surface(zone, "Alféizar", new Vector3(x, 1.12f, z), new Vector3(width + 0.19f, 0.14f, 0.36f),
                "Madera", timberTint, 2, 1);
            Surface(zone, "Travesaño", new Vector3(x, 2.36f, z), new Vector3(width + 0.16f, 0.12f, 0.32f),
                "Madera", timberTint, 2, 1);
            foreach (float offset in new[] { -width / 2, 0, width / 2 })
                Surface(zone, "Marco", new Vector3(x + offset, 1.74f, z), new Vector3(0.12f, 1.24f, 0.27f),
                    "Madera", timberTint, 1, 2);
        }

        void FurnishDanielHouse(Transform zone)
        {
            Wall(zone, -5.3f, -3.1f, 6.25f, inside: true);
            Wall(zone, -1.72f, 1.23f, 6.25f, inside: true);
            Wall(zone, 2.66f, 5.3f, 6.25f, inside: true);
            Wall(zone, -3.1f, -1.72f, 6.25f, 2.36f, 3.12f, true);
            Wall(zone, 1.23f, 2.66f, 6.25f, 2.36f, 3.12f, true);
            Surface(zone, "Tabique del dormitorio", new Vector3(-0.43f, 1.67f, 8.48f), new Vector3(4.47f, 2.9f, 0.2f),
                "Yeso", insidePlaster, 4, 2, 90f);

            Model(zone, "Interiores/Sofa", new Vector3(-4.0f, HouseFloor, 3.42f), 2.32f, 90f);
            Model(zone, "Interiores/Mesa", new Vector3(-2.41f, HouseFloor, 4.26f), 1.35f, 6f);
            Model(zone, "Interiores/Silla", new Vector3(-2.45f, HouseFloor, 5.25f), 0.98f, 12f);
            Model(zone, "Interiores/FregaderoCerrado", new Vector3(4.55f, HouseFloor, 4.65f), 1.92f, 90f);
            Model(zone, "Interiores/AlacenaCerrada", new Vector3(4.55f, 1.97f, 4.65f), 1.52f, 90f);
            Model(zone, "Interiores/Nevera", new Vector3(4.53f, HouseFloor, 2.58f), 1.92f, 90f);

            Model(zone, "Interiores/ArmarioCerrado", new Vector3(-4.08f, HouseFloor, 9.85f), 2.04f, 180f);
            Model(zone, "Interiores/EstanteriaRural", new Vector3(-4.75f, HouseFloor, 8.05f), 1.89f, -90f);
            Model(zone, "Interiores/Cama", new Vector3(3.5f, HouseFloor, 9.23f), 2.34f, 180f);
            var table = Model(zone, "Interiores/Mesita", new Vector3(1.87f, HouseFloor, 9.82f), 0.79f);
            float tableTop = HouseFloor + table.GetComponent<BoxCollider>().size.y;
            var torch = Model(zone, "Objetos/Linterna", new Vector3(1.87f, tableTop, 9.82f), 0.3f, 27f);
            var pickup = torch.AddComponent<ValdaraAction>();
            pickup.kind = ValdaraAction.Kind.Flashlight;
            pickup.prompt = "Recoger la linterna";
            Model(zone, "Interiores/ArmarioAbierto", new Vector3(4.22f, HouseFloor, 7.24f), 1.94f, 102f);

            Point(zone, "Luz del salón", new Vector3(-1.68f, 2.78f, 3.63f), new Color(1, 0.74f, 0.42f), 6.7f, 1.75f);
            Point(zone, "Luz de la cocina", new Vector3(3.31f, 2.68f, 4.04f), new Color(0.76f, 0.84f, 0.60f), 5.1f, 1.45f);
            Point(zone, "Luz del dormitorio", new Vector3(2.0f, 2.64f, 9.36f), new Color(0.98f, 0.70f, 0.38f), 5.3f, 1.5f);
            Point(zone, "Luz sobre la mesita", new Vector3(1.87f, tableTop + 0.44f, 9.52f), brass, 1.4f, 0.72f);
            Point(zone, "Luz del almacén", new Vector3(-2.97f, 2.71f, 8.5f), new Color(0.75f, 0.78f, 0.53f), 4f, 0.88f);
        }
    }
}
