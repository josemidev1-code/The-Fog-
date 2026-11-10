using UnityEngine;

namespace TheFog
{
    internal sealed partial class ValdaraLayout
    {
        void BuildDaniel(Transform zone)
        {
            Room(zone, 140, 12, 14);
            Portal(zone, "Salir a Valdara", new Vector3(140, 0, -6.7f), 180f, new Vector3(-7, 0.1f, -14.9f), 90f);
            Box(zone, "Tabique del dormitorio", new Vector3(137.7f, 1.5f, 0.4f), new Vector3(7.3f, 3f, 0.18f), plaster);
            Sign(zone, "DORMITORIO", new Vector3(142.9f, 2.75f, 0.25f));
            Sign(zone, "COCINA", new Vector3(144, 2.55f, -0.35f), 1.35f, 90f);

            Model(zone, "Interiores/Sofa", new Vector3(136, 0, -4.8f), 2.4f, 90f);
            Model(zone, "Interiores/Mesa", new Vector3(138.8f, 0, -2.5f), 1.7f);
            Model(zone, "Interiores/Silla", new Vector3(138.8f, 0, -3.75f), 1.05f, 180f);
            Model(zone, "Interiores/Silla", new Vector3(138.8f, 0, -1.25f), 1.05f);
            Model(zone, "Interiores/EstanteriaRural", new Vector3(134.75f, 0, -1.45f), 1.85f, -90f);
            Model(zone, "Interiores/FregaderoCerrado", new Vector3(145f, 0, -2.0f), 1.9f, 90f);
            Model(zone, "Interiores/AlacenaCerrada", new Vector3(145f, 1.66f, -2.0f), 1.55f, 90f);
            Model(zone, "Interiores/Nevera", new Vector3(145f, 0, -4.35f), 1.95f, 90f);

            Model(zone, "Interiores/Cama", new Vector3(137.8f, 0, 4.7f), 2.4f);
            var table = Model(zone, "Interiores/Mesita", new Vector3(139.8f, 0, 5.0f), 0.79f);
            float tableHeight = table.GetComponent<BoxCollider>().size.y;
            var torch = Model(zone, "Objetos/Linterna", new Vector3(139.8f, tableHeight, 5f), 0.30f);
            var pickup = torch.AddComponent<ValdaraAction>();
            pickup.kind = ValdaraAction.Kind.Flashlight;
            pickup.prompt = "Recoger la linterna";
            Model(zone, "Interiores/ArmarioAbierto", new Vector3(143.8f, 0, 5.5f), 2.0f);
            Model(zone, "Interiores/ArmarioCerrado", new Vector3(135.0f, 0, 3.15f), 1.95f, -90f);
            Point(zone, "Luz del salón", new Vector3(138.6f, 2.8f, -3.2f), brass, 8.6f, 2.0f);
            Point(zone, "Luz de la cocina", new Vector3(143.9f, 2.65f, -3.2f), brass, 5.5f, 1.7f);
            Point(zone, "Luz del dormitorio", new Vector3(140.4f, 2.8f, 4.0f), brass, 7.8f, 2.0f);
            Point(zone, "Luz sobre la mesita", new Vector3(139.8f, tableHeight + 0.38f, 4.7f), brass, 1.25f, 0.75f);
        }

        void BuildMarket(Transform zone)
        {
            Room(zone, 169, 11, 12);
            Portal(zone, "Salir a Valdara", new Vector3(169, 0, -5.75f), 180f, new Vector3(7.5f, 0.1f, 2f), -90f);
            Model(zone, "Interiores/Mostrador", new Vector3(172.25f, 0, -2.3f), 2.6f, -18f);
            Model(zone, "Interiores/EstanteriaComercial", new Vector3(165.1f, 0, -1.5f), 2.0f, -90f);
            Model(zone, "Interiores/EstanteriaComercial", new Vector3(165.1f, 0, 1.4f), 2.0f, -90f);
            Model(zone, "Interiores/EstanteriaComercial", new Vector3(166.5f, 0, 4.75f), 2.0f);
            Model(zone, "Interiores/EstanteriaComercial", new Vector3(169.0f, 0, 4.75f), 2.0f);
            Model(zone, "Interiores/FregaderoAbierto", new Vector3(172.25f, 0, 4.65f), 1.8f);
            Model(zone, "Interiores/AlacenaAbierta", new Vector3(172.25f, 1.68f, 4.65f), 1.45f);
            Sign(zone, "ULTRAMARINOS VALDARA", new Vector3(168.9f, 2.65f, 5.65f), 4.4f);
            Point(zone, "Luz del mostrador", new Vector3(171.4f, 2.85f, -1.4f), brass, 9f, 2.2f);
            Point(zone, "Luz de las estanterías", new Vector3(166.5f, 2.85f, 3f), new Color(0.8f, 0.87f, 1f), 9f, 1.9f);
        }

        void Room(Transform zone, float x, float width, float depth)
        {
            Box(zone, "Tablones", new Vector3(x, -0.19f, 0), new Vector3(width, 0.38f, depth), wood);
            Box(zone, "Techo", new Vector3(x, 3.36f, 0), new Vector3(width, 0.32f, depth), wood);
            Box(zone, "Pared oeste", new Vector3(x - width / 2, 1.6f, 0), new Vector3(0.22f, 3.2f, depth), plaster);
            Box(zone, "Pared este", new Vector3(x + width / 2, 1.6f, 0), new Vector3(0.22f, 3.2f, depth), plaster);
            Box(zone, "Pared norte", new Vector3(x, 1.6f, depth / 2), new Vector3(width, 3.2f, 0.22f), plaster);
            Box(zone, "Pared sur", new Vector3(x, 1.6f, -depth / 2), new Vector3(width, 3.2f, 0.22f), plaster);
        }

        void BuildMine(Transform zone)
        {
            var shale = new Color(0.12f, 0.115f, 0.105f);
            Box(zone, "Piso de la galería", new Vector3(199, -0.35f, 12), new Vector3(5.6f, 0.7f, 28), shale);
            Box(zone, "Techo de roca", new Vector3(199, 3.38f, 12), new Vector3(5.6f, 0.76f, 28), shale);
            Box(zone, "Roca oeste", new Vector3(196.1f, 1.5f, 12), new Vector3(0.7f, 3.4f, 28), shale);
            Box(zone, "Roca este", new Vector3(201.9f, 1.5f, 12), new Vector3(0.7f, 3.4f, 28), shale);
            Box(zone, "Fondo", new Vector3(199, 1.5f, 25.8f), new Vector3(5.8f, 3.4f, 0.7f), shale);
            Box(zone, "Muro de acceso", new Vector3(199, 1.5f, -1.7f), new Vector3(5.8f, 3.4f, 0.7f), shale);
            Portal(zone, "Volver al camino", new Vector3(199, 0, -1.2f), 180f, new Vector3(0, 0.1f, 48.5f), 180f);

            for (int i = 0; i < 5; i++)
            {
                float z = 2.6f + i * 4.7f;
                Box(zone, "Puntal oeste", new Vector3(196.8f, 1.5f, z), new Vector3(0.32f, 3f, 0.37f), wood);
                Box(zone, "Puntal este", new Vector3(201.2f, 1.5f, z), new Vector3(0.32f, 3f, 0.37f), wood);
                Box(zone, "Viga", new Vector3(199f, 2.86f, z), new Vector3(4.7f, 0.32f, 0.37f), wood);
                Model(zone, "Exterior/Roca", new Vector3(197.15f + (i % 2) * 3.7f, 0, z + 1.9f), 1.25f, 38f * i);
            }
            for (int i = 0; i < 2; i++)
                Box(zone, "Carril", new Vector3(198.62f + i * 0.76f, 0.03f, 12.3f), new Vector3(0.07f, 0.06f, 23.5f), gravel);

            Point(zone, "Luz de emergencia", new Vector3(199, 2.25f, 25), new Color(0.75f, 0.22f, 0.1f), 5.2f, 1.15f);
            var board = Box(zone, "Aviso de la galería", new Vector3(199, 1.55f, 25.24f), new Vector3(2.3f, 0.8f, 0.18f), ink);
            Text(zone, "GALERÍA 01   /   FIN DE TRAMO", new Vector3(199, 1.56f, 25.12f), 0.029f);
            var survey = board.AddComponent<ValdaraAction>();
            survey.kind = ValdaraAction.Kind.Survey;
            survey.prompt = "Leer el aviso";
        }
    }
}
