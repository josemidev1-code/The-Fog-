using UnityEngine;

namespace TheFog
{
    internal sealed partial class ValdaraLayout
    {
        static readonly (string path, string label)[] catalogModels =
        {
            ("Exterior/Roble", "Roble"), ("Exterior/Arbusto", "Arbusto"),
            ("Exterior/CartelValdara", "Cartel de Valdara"), ("Exterior/CasaDaniel", "Casa de Daniel"),
            ("Exterior/CasaRural", "Casa rural"), ("Exterior/EntradaMina", "Entrada de la mina"),
            ("Exterior/Farola", "Farola"), ("Exterior/Rejilla", "Rejilla"),
            ("Exterior/Roca", "Roca"), ("Exterior/Valla", "Valla"),
            ("Interiores/Nevera", "Nevera"), ("Interiores/ArmarioAbierto", "Armario abierto"),
            ("Interiores/ArmarioCerrado", "Armario cerrado"), ("Interiores/AlacenaAbierta", "Alacena abierta"),
            ("Interiores/AlacenaCerrada", "Alacena cerrada"), ("Interiores/Cama", "Cama"),
            ("Interiores/FregaderoAbierto", "Fregadero abierto"), ("Interiores/FregaderoCerrado", "Fregadero cerrado"),
            ("Interiores/EstanteriaRural", "Estantería rural"), ("Interiores/EstanteriaComercial", "Estantería de tienda"),
            ("Interiores/Mesa", "Mesa"), ("Interiores/Mesita", "Mesita"),
            ("Interiores/Silla", "Silla"), ("Interiores/Sofa", "Sofá"),
            ("Interiores/Mostrador", "Mostrador"), ("Objetos/Linterna", "Linterna")
        };

        public void BuildCatalog()
        {
            Environment(ink, false);
            Transform room = Group("Biblioteca de modelos");
            Box(room, "Suelo", new Vector3(0, -0.35f, 20), new Vector3(25f, 0.7f, 52f), ink);
            Sign(room, "VALDARA   /   BIBLIOTECA 3D", new Vector3(0, 2.65f, 1.1f), 6.5f);
            for (int i = 0; i < catalogModels.Length; i++)
            {
                var (path, label) = catalogModels[i];
                var point = new Vector3((i % 4 - 1.5f) * 5.3f, 0.35f, 4.7f + (i / 4) * 5.5f);
                Box(room, "Peana " + label, point - Vector3.up * 0.175f, new Vector3(3.65f, 0.35f, 3.65f), gravel);
                Model(room, path, point, path == "Objetos/Linterna" ? 0.84f : 2.4f, 0f);
                Sign(room, label, point + new Vector3(0, 0.39f, -1.86f), 3.1f);
            }
            Player(new Vector3(0, 0.1f, -3.2f), 0f, true);
        }
    }
}
