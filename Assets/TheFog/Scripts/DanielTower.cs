using UnityEngine;

namespace TheFog
{
    internal sealed partial class ValdaraLayout
    {
        const float FlightRise = 2.25f;
        const float FlightRun = 3.4f;
        const float TowerLane = 0.86f;
        const int StepsPerFlight = 12;

        static readonly Color towerWood = new Color(0.60f, 0.53f, 0.40f);
        static readonly Color towerRail = new Color(0.77f, 0.66f, 0.48f);

        void BuildDanielTower(Transform zone)
        {
            foreach (float x in new[] { -2.22f, 2.22f })
            foreach (float z in new[] { -2.62f, 2.62f })
            {
                Surface(zone, "Dado de piedra", TowerBase + new Vector3(x, 0.11f, z),
                    new Vector3(0.58f, 0.22f, 0.58f), "Piedra", towerWood, 1, 1);
                TowerBeam(zone, "Pilar", new Vector3(x, 0.18f, z), new Vector3(x, 9.04f, z), 0.26f, towerWood);
            }

            for (int tier = 0; tier < 3; tier++)
            {
                float bottom = tier * FlightRise + 0.24f;
                float top = (tier + 1) * FlightRise - 0.18f;
                foreach (float side in new[] { -1f, 1f })
                {
                    float x = side * 2.22f;
                    TowerBrace(zone, new Vector3(x, bottom, -2.62f), new Vector3(x, top, 2.62f));
                    if (tier > 0 || side > 0f)
                    {
                        float z = side * 2.62f;
                        TowerBrace(zone, new Vector3(-2.22f, bottom, z), new Vector3(2.22f, top, z));
                    }
                }
            }

            for (int flight = 0; flight < 4; flight++)
                BuildTowerFlight(zone, flight);

            TowerFloor(zone, "Balcón oeste", -2.25f, 6.75f, 0f, 1.33f, 6.06f);
            TowerFloor(zone, "Balcón este", 2.25f, 6.75f, 0f, 1.33f, 6.06f);
            TowerFloor(zone, "Balcón frontal", 0f, 6.75f, -2.35f, 3.26f, 1.35f);
            TowerRail(zone, -2.85f, -2.96f, 2.85f, -2.96f, 6.75f);
            foreach (float side in new[] { -1f, 1f })
            {
                TowerRail(zone, side * 2.85f, -2.96f, side * 2.85f, 2.96f, 6.75f);
                TowerRail(zone, side * 2.19f, 2.96f, side * 2.85f, 2.96f, 6.75f);
                TowerRail(zone, side * 1.59f, -1.66f, side * 1.59f, 1.66f, 6.75f);
            }
            TowerRail(zone, -1.59f, -1.66f, 1.59f, -1.66f, 6.75f);

            // La cubierta superior deja un hueco continuo encima del último tramo.
            TowerFloor(zone, "Cubierta oeste", -1.15f, 9f, 0f, 2.54f, 5.53f);
            TowerFloor(zone, "Cubierta este", 2.02f, 9f, 0f, 0.84f, 5.53f);
            TowerFloor(zone, "Cubierta trasera", 0.86f, 9f, 2.24f, 1.54f, 1.06f);
            TowerRail(zone, -2.35f, 2.70f, 2.35f, 2.70f, 9f);
            foreach (float side in new[] { -1f, 1f })
            {
                TowerRail(zone, side * 2.35f, -2.90f, side * 2.35f, 2.70f, 9f);
                TowerRail(zone, side * 2.15f, -2.90f, side * 2.35f, -2.90f, 9f);
            }
            TowerRail(zone, 0.10f, -1.70f, 0.10f, 1.70f, 9f);
            TowerRail(zone, 1.62f, -1.70f, 1.62f, 1.70f, 9f);
            TowerRail(zone, 0.10f, 1.70f, 1.62f, 1.70f, 9f);

            Point(zone, "Luz de la plataforma", TowerBase + new Vector3(-0.55f, 8.55f, 2.25f), brass, 6f, 1.45f);
            Sign(zone, "PUESTO DE VIGILANCIA", TowerBase + new Vector3(1.82f, 1.3f, -3.4f), 2.85f);
        }

        void BuildTowerFlight(Transform zone, int flight)
        {
            float direction = flight % 2 == 0 ? 1f : -1f;
            float x = -direction * TowerLane;
            float startZ = -direction * FlightRun * 0.5f;
            float bottom = flight * FlightRise;
            float top = bottom + FlightRise;
            float stepDepth = FlightRun / StepsPerFlight;
            float stepRise = FlightRise / StepsPerFlight;

            for (int i = 0; i < StepsPerFlight; i++)
            {
                float stepTop = bottom + (i + 1) * stepRise;
                float z = startZ + direction * (i + 0.5f) * stepDepth;
                Surface(zone, "Peldaño", TowerBase + new Vector3(x, stepTop - 0.08f, z),
                    new Vector3(1.36f, 0.16f, stepDepth + 0.012f), "Madera", towerWood, 2, 1);
            }

            foreach (float edge in new[] { -0.72f, 0.72f })
            {
                float railX = x + edge;
                TowerBeam(zone, "Zanca", new Vector3(railX, bottom + 0.02f, startZ),
                    new Vector3(railX, top - 0.13f, -startZ), 0.15f, towerWood);
                for (int i = 0; i <= 3; i++)
                {
                    float t = i / 3f;
                    float z = Mathf.Lerp(startZ, -startZ, t);
                    float y = Mathf.Lerp(bottom, top, t);
                    TowerBeam(zone, "Barrote de escalera", new Vector3(railX, y, z),
                        new Vector3(railX, y + 1.02f, z), 0.095f, towerRail);
                }
                TowerBeam(zone, "Pasamanos", new Vector3(railX, bottom + 1.01f, startZ),
                    new Vector3(railX, top + 1.01f, -startZ), 0.095f, towerRail);
            }

            float landingZ = direction * 2.34f;
            float landingWidth = flight == 2 ? 4.44f : 4.3f;
            TowerFloor(zone, "Descansillo", 0f, top, landingZ, landingWidth, 1.30f);
            float edgeZ = direction * 2.93f;
            TowerRail(zone, -2.15f, edgeZ, 2.15f, edgeZ, top);
            if (flight != 2)
            {
                TowerRail(zone, -2.15f, landingZ - 0.59f, -2.15f, landingZ + 0.59f, top);
                TowerRail(zone, 2.15f, landingZ - 0.59f, 2.15f, landingZ + 0.59f, top);
            }
        }

        void TowerFloor(Transform zone, string label, float x, float top, float z, float width, float depth)
        {
            Surface(zone, label, TowerBase + new Vector3(x, top - 0.10f, z), new Vector3(width, 0.20f, depth),
                "Madera", towerWood, Mathf.CeilToInt(width), Mathf.CeilToInt(depth));
        }

        void TowerRail(Transform zone, float x1, float z1, float x2, float z2, float top)
        {
            foreach (float y in new[] { 0.33f, 0.68f, 1.03f })
                TowerBeam(zone, "Baranda", new Vector3(x1, top + y, z1), new Vector3(x2, top + y, z2), 0.085f, towerRail);
            int spans = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(new Vector2(x1, z1), new Vector2(x2, z2)) / 1.35f));
            for (int i = 0; i <= spans; i++)
            {
                float t = i / (float)spans;
                var foot = new Vector3(Mathf.Lerp(x1, x2, t), top, Mathf.Lerp(z1, z2, t));
                TowerBeam(zone, "Barrote", foot, foot + Vector3.up * 1.04f, 0.105f, towerRail);
            }
        }

        void TowerBrace(Transform zone, Vector3 bottom, Vector3 top)
        {
            TowerBeam(zone, "Arriostramiento", bottom, top, 0.17f, towerWood);
            TowerBeam(zone, "Arriostramiento", new Vector3(top.x, bottom.y, top.z),
                new Vector3(bottom.x, top.y, bottom.z), 0.17f, towerWood);
        }

        void TowerBeam(Transform zone, string label, Vector3 a, Vector3 b, float width, Color tint)
        {
            Vector3 delta = b - a;
            var beam = Surface(zone, label, TowerBase + (a + b) * 0.5f, new Vector3(width, delta.magnitude, width),
                "Madera", tint, 1, Mathf.Max(1, Mathf.CeilToInt(delta.magnitude)));
            beam.transform.rotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        }
    }
}
