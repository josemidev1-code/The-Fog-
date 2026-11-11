using UnityEngine;

namespace TheFog
{
    internal sealed partial class ValdaraLayout
    {
        const float HouseFloor = 0.22f;
        static readonly Color housePlaster = new Color(0.79f, 0.77f, 0.63f);
        static readonly Color insidePlaster = new Color(0.69f, 0.71f, 0.51f);
        static readonly Color tileTint = new Color(0.76f, 0.77f, 0.63f);
        static readonly Color stoneTint = new Color(0.58f, 0.59f, 0.49f);

        static readonly Vector3 houseCenter = new Vector3(0f, 0f, 5.9f);
        static readonly Vector3 outsideDoor = new Vector3(-0.47f, 0.22f, 1.30f);
        static readonly Vector3 interiorOffset = new Vector3(144f, 0f, 0f);
        internal static readonly Vector3 BedroomStart = interiorOffset + new Vector3(1.10f, 0.25f, 7.9f);
        internal static readonly Vector3 BedroomTitleCamera = interiorOffset + new Vector3(0.15f, 1.88f, 7.24f);
        internal static readonly Vector3 BedroomTitleFocus = interiorOffset + new Vector3(0.93f, 1.40f, 9.65f);

        void BuildDanielHouse(Transform facade)
        {
            Model(facade, "Exterior/CasaDaniel", houseCenter, 11.8f, 180f, false);
            Collision(facade, "Volumen de la fachada", new Vector3(0, 2.95f, 5.92f),
                new Vector3(9.86f, 5.9f, 8.8f));
            var entry = Collision(facade, "Acceso por la puerta original", outsideDoor + Vector3.up * 1.04f,
                new Vector3(1.24f, 2.08f, 0.24f));
            SetHouseDoor(entry, "Entrar en la casa", interiorOffset + new Vector3(-0.47f, 0.25f, 2.12f), 0f, true);

            Point(facade, "Luz de la entrada", outsideDoor + new Vector3(0f, 2.53f, -0.6f),
                new Color(1f, 0.72f, 0.37f), 7.4f, 2.05f);

            var inside = Group("03 · Interior de Daniel");
            BuildDanielInterior(inside);
            BuildDanielModules(inside);
            inside.position = interiorOffset;
        }

        internal void BuildDanielModules(Transform interior)
        {
            if (interior.Find("Detalles de lectura") != null) return;
            BuildHouseDocuments(interior);
            var doorPanel = Model(null, "Interiores/ParedPuertaDaniel",
                new Vector3(1.945f, HouseFloor, 6.02f), 4.17f, 0f, false);
            doorPanel.transform.SetParent(interior, false);
            foreach (float x in new[] { 1.34f, 2.55f })
            {
                var jamb = Collision(null, "Jamba del dormitorio", new Vector3(x, 1.28f, 6.02f),
                    new Vector3(0.22f, 2.12f, 0.25f));
                jamb.transform.SetParent(interior, false);
            }

            var corner = Model(null, "Interiores/ParedEsquinaDaniel",
                new Vector3(-2.22f, HouseFloor, 10.34f), 3.25f, 180f, false);
            corner.transform.SetParent(interior, false);
        }

        void BuildDanielInterior(Transform zone)
        {
            Surface(zone, "Tablones de la casa", new Vector3(0, 0.165f, 6), new Vector3(10.6f, 0.11f, 9.6f),
                "Madera", timberTint, 10, 11);
            Surface(zone, "Baldosas de la cocina", new Vector3(3.09f, 0.221f, 3.68f), new Vector3(4.18f, 0.014f, 4.72f),
                "Baldosas", tileTint, 4, 4);
            Surface(zone, "Techo interior", new Vector3(0, 3.15f, 6), new Vector3(10.6f, 0.2f, 9.6f),
                "Madera", timberTint, 11, 10);

            Wall(zone, -5.3f, -4.14f, 1.2f);
            Wall(zone, -2.24f, -1.12f, 1.2f);
            Wall(zone, 0.18f, 2.24f, 1.2f);
            Wall(zone, 4.14f, 5.3f, 1.2f);
            Window(zone, -3.19f, 1.2f, 1.9f);
            Window(zone, 3.19f, 1.2f, 1.9f);
            Wall(zone, -1.12f, 0.18f, 1.2f, 2.35f, 3.12f);

            Wall(zone, -5.3f, 0.75f, 10.8f);
            Wall(zone, 2.9f, 5.3f, 10.8f);
            Window(zone, 1.825f, 10.8f, 2.15f);
            foreach (float side in new[] { -5.3f, 5.3f })
                Surface(zone, "Muro lateral", new Vector3(side, 1.67f, 6f), new Vector3(9.6f, 2.9f, 0.24f),
                    "Yeso", insidePlaster, 7, 2, 90f);

            var exit = Surface(zone, "Puerta principal", new Vector3(-0.47f, 1.28f, 1.2f),
                new Vector3(1.3f, 2.12f, 0.15f), "Madera", new Color(0.51f, 0.51f, 0.35f), 2, 3);
            SetHouseDoor(exit, "Salir al claro", new Vector3(outsideDoor.x, 0.09f, outsideDoor.z - 1.37f), 180f);
            Box(zone, "Manilla", new Vector3(-0.04f, 1.22f, 1.29f),
                new Vector3(0.12f, 0.08f, 0.14f), new Color(0.36f, 0.3f, 0.18f)).layer = 2;
            FurnishDanielHouse(zone);
        }

        static void SetHouseDoor(GameObject door, string prompt, Vector3 destination, float facing, bool houseInterior = false)
        {
            var action = door.AddComponent<ValdaraAction>();
            action.kind = ValdaraAction.Kind.Door;
            action.prompt = prompt;
            action.destination = destination;
            action.facing = facing;
            action.houseInterior = houseInterior;
        }

        static void HouseNote(GameObject furniture, string prompt, string message)
        {
            var note = furniture.AddComponent<ValdaraAction>();
            note.kind = ValdaraAction.Kind.Read;
            note.prompt = prompt;
            note.message = message;
        }

        static void OpenableFurniture(GameObject furniture, string label, string openModel, float size)
        {
            var action = furniture.AddComponent<ValdaraAction>();
            action.kind = ValdaraAction.Kind.Furniture;
            action.prompt = "Abrir " + label;
            action.closePrompt = "Cerrar " + label;
            action.openModel = openModel;
            action.modelSize = size;
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
            var livingTable = Model(zone, "Interiores/Mesa", new Vector3(-2.41f, HouseFloor, 4.26f), 1.35f, 6f);
            HouseNote(livingTable, "Leer el apunte de Daniel",
                "He dejado la linterna en la mesita del dormitorio. Tengo que comprobar la entrada de la mina antes del siguiente turno. La niebla ha tapado las marcas de la senda.\n\n— Daniel");
            Model(zone, "Interiores/Silla", new Vector3(-2.45f, HouseFloor, 5.25f), 0.98f, 12f);
            var sink = Model(zone, "Interiores/FregaderoCerrado", new Vector3(4.55f, HouseFloor, 4.65f), 1.92f, 90f);
            OpenableFurniture(sink, "el mueble del fregadero", "Interiores/FregaderoAbierto", 1.92f);
            var cupboard = Model(zone, "Interiores/AlacenaCerrada", new Vector3(4.55f, 1.97f, 4.65f), 1.52f, 90f);
            OpenableFurniture(cupboard, "la alacena", "Interiores/AlacenaAbierta", 1.52f);
            var fridge = Model(zone, "Interiores/Nevera", new Vector3(4.53f, HouseFloor, 2.58f), 1.92f, 90f);
            HouseNote(fridge, "Leer el papel de la nevera",
                "Turno de vigilancia, 23:40. Si vuelve a sonar el teléfono de la garita, apunta la hora. La línea está cortada desde el lunes.");

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

            Point(zone, "Luz del salón", new Vector3(-1.68f, 2.78f, 3.63f), new Color(1, 0.74f, 0.42f), 7.2f, 2.65f);
            Point(zone, "Luz de la cocina", new Vector3(3.31f, 2.68f, 4.04f), new Color(0.76f, 0.84f, 0.60f), 5.7f, 2.15f);
            Point(zone, "Luz del dormitorio", new Vector3(2.0f, 2.64f, 9.36f), new Color(0.98f, 0.70f, 0.38f), 5.9f, 2.35f);
            Point(zone, "Luz sobre la mesita", new Vector3(1.87f, tableTop + 0.44f, 9.52f), brass, 1.4f, 0.72f);
            Point(zone, "Luz del almacén", new Vector3(-2.97f, 2.71f, 8.5f), new Color(0.75f, 0.78f, 0.53f), 4.8f, 1.45f);
        }
    }
}
