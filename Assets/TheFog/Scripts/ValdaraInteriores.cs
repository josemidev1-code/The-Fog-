using UnityEngine;

namespace TheFog
{
    internal sealed partial class ValdaraLayout
    {
        void BuildMarket(Transform zone)
        {
            Room(zone, 169, 11, 12);
            Portal(zone, "Salir a Valdara", new Vector3(169, 0, -5.75f), 180f, MarketReturn, -90f);
            Model(zone, "Interiores/Mostrador", new Vector3(172.25f, 0, -2.3f), 2.6f, -18f);
            Model(zone, "Interiores/EstanteriaComercial", new Vector3(165.1f, 0, -1.5f), 2.0f, -90f);
            Model(zone, "Interiores/EstanteriaComercial", new Vector3(165.1f, 0, 1.4f), 2.0f, -90f);
            Model(zone, "Interiores/EstanteriaComercial", new Vector3(166.5f, 0, 4.75f), 2.0f);
            Model(zone, "Interiores/EstanteriaComercial", new Vector3(169.0f, 0, 4.75f), 2.0f);
            Model(zone, "Interiores/FregaderoAbierto", new Vector3(172.25f, 0, 4.65f), 1.8f);
            Model(zone, "Interiores/AlacenaAbierta", new Vector3(172.25f, 1.68f, 4.65f), 1.45f);
            var notice = Box(zone, "Aviso de ultramarinos", new Vector3(170.8f, 1.53f, 5.79f),
                new Vector3(0.84f, 0.52f, 0.1f), wood);
            Inspection(notice, "Leer aviso de ultramarinos", "La mina está al final del camino. El puesto de vigilancia queda a la derecha.");
            WorldDocument(notice.transform, new Vector3(0f, 0f, -0.506f), Vector3.zero, new Vector2(0.86f, 0.86f));
            Sign(zone, "ULTRAMARINOS VALDARA", new Vector3(168.9f, 2.65f, 5.65f), 4.4f);
            Point(zone, "Luz del mostrador", new Vector3(171.4f, 2.85f, -1.4f), brass, 9f, 2.2f);
            Point(zone, "Luz de las estanterías", new Vector3(166.5f, 2.85f, 3f), new Color(0.8f, 0.87f, 1f), 9f, 1.9f);
        }

        static void Inspection(GameObject prop, string prompt, string message)
        {
            var action = prop.AddComponent<ValdaraAction>();
            action.kind = ValdaraAction.Kind.Inspect;
            action.prompt = prompt;
            action.message = message;
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
            Portal(zone, "Volver al camino", new Vector3(199, 0, -1.2f), 180f, MineReturn, 180f);

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
            Text(zone, "GALERÍA 01   /   ACCESO CERRADO", new Vector3(199, 1.56f, 25.12f), 0.029f);
            var survey = board.AddComponent<ValdaraAction>();
            survey.kind = ValdaraAction.Kind.Survey;
            survey.prompt = "Leer el aviso de cierre";
            survey.message = "GALERÍA 01\n\nAcceso suspendido hasta nueva inspección. La última ronda no ha regresado al puesto de vigilancia.\n\nNo continúes por las vías. Vuelve al exterior.\n\n— Registro de vigilancia";
        }
    }
}
