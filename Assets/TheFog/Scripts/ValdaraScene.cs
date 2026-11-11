using System;
using UnityEngine;

namespace TheFog
{
    public sealed class ValdaraScene : MonoBehaviour
    {
        [SerializeField] bool catalog = false;
        [SerializeField] bool daniel = false;
        [SerializeField] Shader standardShader = null;
        [SerializeField] Transform constructed;
        [SerializeField] Transform danielInterior;

        void Awake()
        {
            if (constructed == null)
                Compose(InstantiateModel);
            else if (daniel && danielInterior != null)
                new ValdaraLayout(constructed, standardShader, InstantiateModel).BuildDanielModules(danielInterior);
        }

        public Transform Compose(Func<GameObject, Transform, GameObject> instantiateModel)
        {
            if (constructed != null)
                return null;

            var stage = new GameObject(catalog ? "Catálogo 3D" : daniel ? "Casa de Daniel" : "Valdara");
            stage.transform.SetParent(transform, false);
            var layout = new ValdaraLayout(stage.transform, standardShader, instantiateModel);
            try
            {
                if (catalog)
                    layout.BuildCatalog();
                else if (daniel)
                    layout.BuildDanielMap();
                else
                    layout.BuildValdara();
                constructed = stage.transform;
                return constructed;
            }
            catch
            {
                if (Application.isPlaying)
                    Destroy(stage);
                else
                    DestroyImmediate(stage);
                throw;
            }
        }

        static GameObject InstantiateModel(GameObject prefab, Transform parent)
        {
            return Instantiate(prefab, parent);
        }
    }
}
