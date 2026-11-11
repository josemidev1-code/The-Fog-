using UnityEngine;

namespace TheFog
{
    internal sealed partial class ValdaraLayout
    {
        void BuildHouseDocuments(Transform interior)
        {
            var marker = new GameObject("Detalles de lectura");
            marker.transform.SetParent(interior, false);
            foreach (var action in interior.GetComponentsInChildren<ValdaraAction>())
            {
                if (action.kind != ValdaraAction.Kind.Read) continue;
                var bounds = action.GetComponent<BoxCollider>();
                if (bounds == null) continue;
                bool onTable = action.gameObject.name == "Mesa";
                Vector3 point = onTable
                    ? new Vector3(0f, bounds.center.y + bounds.size.y * 0.5f + 0.008f, 0f)
                    : new Vector3(0f, Mathf.Min(1.35f, bounds.size.y * 0.72f), -bounds.size.z * 0.5f - 0.009f);
                var paper = WorldDocument(action.transform, point, onTable ? new Vector3(90f, 0f, -12f) : Vector3.zero,
                    new Vector2(0.23f, 0.30f));
                paper.name = "Papel · " + action.prompt;
            }
        }

        GameObject WorldDocument(Transform parent, Vector3 point, Vector3 angles, Vector2 size)
        {
            var paper = GameObject.CreatePrimitive(PrimitiveType.Quad);
            paper.name = "Documento";
            paper.transform.SetParent(parent, false);
            paper.transform.localPosition = point;
            paper.transform.localRotation = Quaternion.Euler(angles);
            paper.transform.localScale = new Vector3(size.x, size.y, 1f);
            paper.layer = 2;
            var material = new Material(shader) { name = "Papel envejecido", color = new Color(0.86f, 0.83f, 0.72f),
                mainTexture = Resources.Load<Texture2D>("Daniel/PapelNota") };
            material.mainTextureScale = new Vector2(0.95f, 0.95f);
            material.mainTextureOffset = new Vector2(0.025f, 0.025f);
            material.SetFloat("_Smoothness", 0f);
            paper.GetComponent<Renderer>().sharedMaterial = material;
            // Ink strokes remain legible as a written document at the game's render resolution.
            for (int i = 0; i < 7; i++)
            {
                var line = Box(null, "Trazo de tinta", Vector3.zero, Vector3.one, new Color(0.19f, 0.18f, 0.13f));
                line.transform.SetParent(paper.transform, false);
                line.transform.localPosition = new Vector3(-0.03f, 0.32f - i * 0.085f, -0.003f);
                line.transform.localScale = new Vector3(i == 6 ? 0.34f : 0.70f - (i % 3) * 0.05f, 0.009f, 0.001f);
                line.layer = 2;
            }
            return paper;
        }
    }
}
