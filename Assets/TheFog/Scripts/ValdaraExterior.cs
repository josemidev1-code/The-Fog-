using UnityEngine;

namespace TheFog
{
    internal sealed partial class ValdaraLayout
    {
        static readonly Vector3 MarketReturn = new Vector3(10f, 0.15f, 3.8f);
        internal static readonly Vector3 MineReturn = new Vector3(0f, 0.15f, 49f);

        void BuildExterior(Transform zone)
        {
            Surface(zone, "Suelo", new Vector3(0, -0.6f, 8), new Vector3(86, 1.2f, 114), "Tierra", grassTint, 29, 38);
            Surface(zone, "Camino principal", new Vector3(0, -0.04f, 6), new Vector3(5.6f, 0.12f, 101), "Tierra", dirtTint, 3, 40);
            Surface(zone, "Acceso a casa de Daniel", new Vector3(-4.7f, -0.039f, -13), new Vector3(6, 0.12f, 2.5f), "Tierra", dirtTint, 3, 2);
            Surface(zone, "Acceso a ultramarinos", new Vector3(6.6f, -0.039f, 3.8f), new Vector3(8.3f, 0.12f, 2.5f), "Tierra", dirtTint, 4, 2);
            Box(zone, "Límite oeste", new Vector3(-41, 0.65f, 8), new Vector3(2, 1.3f, 113), soil);
            Box(zone, "Límite este", new Vector3(41, 0.65f, 8), new Vector3(2, 1.3f, 113), soil);
            Box(zone, "Límite norte", new Vector3(0, 0.65f, 64), new Vector3(83, 1.3f, 2), soil);
            Box(zone, "Límite sur", new Vector3(0, 0.65f, -48), new Vector3(83, 1.3f, 2), soil);

            Model(zone, "Exterior/CartelValdara", new Vector3(-3.9f, 0, -33), 2.5f, -17f);
            for (int i = 0; i < 3; i++)
                Model(zone, "Exterior/Valla", new Vector3(-9.1f + i * 1.8f, 0, -11.65f), 1.8f);
            Portal(zone, "Senda de Daniel", new Vector3(-7, 0, -13), 0f, DanielSpawn, 0f,
                ValdaraAction.Kind.Travel, "CasaDaniel");

            var market = Model(zone, "Exterior/CasaRural", new Vector3(10, 0, 9.7f), 8.5f);
            float marketDoorZ = market.transform.position.z - market.GetComponent<BoxCollider>().size.z / 2f - 0.18f;
            Box(zone, "Umbral de ultramarinos", new Vector3(10f, -0.043f, (MarketReturn.z + marketDoorZ) * 0.5f),
                new Vector3(2.3f, 0.12f, marketDoorZ - MarketReturn.z), gravel);
            Portal(zone, "Ultramarinos", new Vector3(10f, 0.04f, marketDoorZ), 0f, new Vector3(169, 0.1f, -4.5f), 0f);
            Model(zone, "Exterior/CasaRural", new Vector3(-11, 0, 18), 7.9f, 16f);
            Model(zone, "Exterior/CasaRural", new Vector3(12, 0, 30), 6.7f, 28f);
            var mine = Model(zone, "Exterior/EntradaMina", new Vector3(0, 0, 58), 10f);
            float mineDoorZ = mine.transform.position.z - mine.GetComponent<BoxCollider>().size.z / 2f - 0.18f;
            Portal(zone, "Entrar en la mina", new Vector3(0, 0.02f, mineDoorZ), 0f,
                new Vector3(199, 0.1f, 0.6f), 0f, ValdaraAction.Kind.MineGate);

            // Puesto de vigilancia junto a la explotación, accesible desde el camino.
            Box(zone, "Acceso al puesto de vigilancia", new Vector3(5.1f, -0.047f, 48f),
                new Vector3(5.4f, 0.12f, 2.2f), gravel);
            Box(zone, "Explanada de vigilancia", new Vector3(10f, -0.05f, 49.8f),
                new Vector3(7f, 0.12f, 8f), gravel);
            var tower = Model(zone, "Exterior/TorreVigilancia", new Vector3(10f, 0.01f, 50.6f), 8.4f, -90f);
            float postX = tower.transform.position.x - tower.GetComponent<BoxCollider>().size.z / 2f - 0.55f;
            var post = Box(zone, "Registro del puesto", new Vector3(postX, 0.9f, 49f),
                new Vector3(0.45f, 1.8f, 0.68f), wood);
            Inspection(post, "Leer registro de vigilancia", "Última ronda: galería 01. El aviso de cierre sigue en el fondo.");
            WorldDocument(post.transform, new Vector3(0f, 0f, -0.506f), Vector3.zero, new Vector2(0.54f, 0.36f));
            Point(zone, "Luz del registro", new Vector3(postX - 0.6f, 2.3f, 49f), brass, 4.5f, 1.6f);

            Model(zone, "Exterior/Rejilla", new Vector3(1.5f, 0.025f, -8.1f), 1.05f, 0f, false);
            Model(zone, "Exterior/Rejilla", new Vector3(-1.6f, 0.025f, 14), 1.05f, 0f, false);

            for (int i = 0; i < 9; i++)
            {
                float side = i % 2 == 0 ? 1f : -1f;
                float z = -27f + i * 9.2f;
                Model(zone, "Exterior/Farola", new Vector3(side * 3.4f, 0, z), 3.8f, side * 90f, false);
                Point(zone, "Farola", new Vector3(side * 3f, 3.05f, z), brass, 8.3f, 1.8f);
            }

            for (int i = 0; i < 6; i++)
            {
                Model(zone, "Exterior/Valla", new Vector3(-13 + i * 1.8f, 0, 13f), 1.8f);
                Model(zone, "Exterior/Valla", new Vector3(7 + i * 1.8f, 0, 25f), 1.8f);
            }

            // Agrupa la vegetación en los bordes y deja libres los accesos y las vistas desde el camino.
            foreach (var (x, z, height, angle) in new[]
            {
                (-11f, -7f, 7.2f, 14f), (-16f, -10f, 6.6f, 67f), (-18f, -4f, 6.1f, 130f),
                (16f, 14f, 6.8f, 105f), (21f, 18f, 7.6f, 38f), (22f, 24f, 6.2f, 195f),
                (-16f, 34f, 7.4f, 160f), (-12f, 39f, 6.4f, 235f), (-18f, 43f, 7.1f, 85f)
            })
            {
                var point = new Vector3(x, 0f, z);
                Model(zone, "Exterior/Roble", point, height, angle, false);
                Box(zone, "Tronco · colisión", point + Vector3.up, new Vector3(0.55f, 2f, 0.55f), wood)
                    .GetComponent<Renderer>().enabled = false;
                Model(zone, "Exterior/Arbusto", point + new Vector3(1.4f, 0, 1.1f), 1.25f, angle, false);
            }
            Model(zone, "Exterior/Roca", new Vector3(-7f, 0, 52.5f), 2.2f, 33f);
            Model(zone, "Exterior/Roca", new Vector3(-6.5f, 0, 55f), 1.65f, 78f);

            Sign(zone, "DANIEL   ←", new Vector3(-2.6f, 1.4f, -16), 1.75f);
            Sign(zone, "TIENDA   →", new Vector3(2.5f, 1.4f, 0.7f), 1.75f);
            Sign(zone, "MINA   ↑", new Vector3(4.1f, 1.4f, 35), 1.75f);
        }
    }
}
