using System;
using UnityEngine;

namespace TheFog
{
    internal sealed partial class ValdaraLayout
    {
        void BuildExterior(Transform zone)
        {
            Box(zone, "Suelo", new Vector3(0, -0.6f, 8), new Vector3(86, 1.2f, 114), soil);
            Box(zone, "Camino principal", new Vector3(0, -0.04f, 6), new Vector3(5.6f, 0.12f, 101), gravel);
            Box(zone, "Acceso a casa de Daniel", new Vector3(-4.7f, -0.039f, -13), new Vector3(6, 0.12f, 2.5f), gravel);
            Box(zone, "Acceso a ultramarinos", new Vector3(5, -0.039f, 3.8f), new Vector3(6.5f, 0.12f, 2.5f), gravel);
            Box(zone, "Límite oeste", new Vector3(-41, 0.65f, 8), new Vector3(2, 1.3f, 113), soil);
            Box(zone, "Límite este", new Vector3(41, 0.65f, 8), new Vector3(2, 1.3f, 113), soil);
            Box(zone, "Límite norte", new Vector3(0, 0.65f, 64), new Vector3(83, 1.3f, 2), soil);
            Box(zone, "Límite sur", new Vector3(0, 0.65f, -48), new Vector3(83, 1.3f, 2), soil);

            Model(zone, "Exterior/CartelValdara", new Vector3(-3.9f, 0, -33), 2.5f, -17f);
            Sign(zone, "VALDARA", new Vector3(-3.9f, 2.45f, -34), 2.45f);
            Model(zone, "Exterior/CasaDaniel", new Vector3(-9, 0, -7), 8.4f, 0f);
            Portal(zone, "Casa de Daniel", new Vector3(-7, 0, -13), 0f, new Vector3(140, 0.1f, -5.3f), 0f);

            Model(zone, "Exterior/CasaRural", new Vector3(10, 0, 9.7f), 8.5f, -8f);
            Portal(zone, "Ultramarinos", new Vector3(7.5f, 0, 3.8f), 0f, new Vector3(169, 0.1f, -4.5f), 0f);
            Model(zone, "Exterior/CasaRural", new Vector3(-11, 0, 18), 7.9f, 16f);
            Model(zone, "Exterior/CasaRural", new Vector3(12, 0, 30), 6.7f, 28f);
            Model(zone, "Exterior/EntradaMina", new Vector3(0, 0, 58), 10f, 0f);
            Portal(zone, "Entrar en la mina", new Vector3(0, 0, 51), 0f,
                new Vector3(199, 0.1f, 0.6f), 0f, ValdaraAction.Kind.MineGate);

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

            var random = new System.Random(181);
            for (int i = 0; i < 32; i++)
            {
                float side = i % 2 == 0 ? -1 : 1;
                float x = side * (15f + 23f * (float)random.NextDouble());
                float z = -43f + 104f * (float)random.NextDouble();
                float angle = 360f * (float)random.NextDouble();
                var point = new Vector3(x, 0f, z);
                Model(zone, "Exterior/Roble", point, 4.2f + 3.8f * (float)random.NextDouble(), angle, false);
                Box(zone, "Tronco · colisión", point + Vector3.up, new Vector3(0.55f, 2f, 0.55f), wood)
                    .GetComponent<Renderer>().enabled = false;
                Model(zone, i % 4 == 0 ? "Exterior/Roca" : "Exterior/Arbusto",
                    point + new Vector3(side * 1.15f, 0, 1.25f), 0.9f + (float)random.NextDouble(), angle, false);
            }

            Sign(zone, "DANIEL   ←", new Vector3(-2.6f, 1.4f, -16), 1.75f);
            Sign(zone, "TIENDA   →", new Vector3(2.5f, 1.4f, 0.7f), 1.75f);
            Sign(zone, "MINA   ↑", new Vector3(2.6f, 1.4f, 35), 1.75f);
        }
    }
}
