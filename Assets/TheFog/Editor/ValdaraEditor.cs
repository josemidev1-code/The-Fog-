using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheFog
{
    static class ValdaraEditor
    {
        const string MaterialFolder = "Assets/TheFog/Materiales";
        const string BakeMenu = "The Fog/Materializar escena para editar";

        [MenuItem(BakeMenu, true)]
        static bool CanMaterialize()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode && FindComposer() != null;
        }

        [MenuItem(BakeMenu)]
        static void Materialize()
        {
            var composer = FindComposer();
            if (composer == null)
                return;

            Undo.RecordObject(composer, "Materializar escena de The Fog");
            Transform hierarchy = composer.Compose((asset, parent) => (GameObject)PrefabUtility.InstantiatePrefab(asset, parent));
            if (hierarchy == null)
            {
                Debug.Log("La escena ya está materializada.", composer);
                return;
            }

            SaveMaterials(hierarchy);
            Undo.RegisterCreatedObjectUndo(hierarchy.gameObject, "Materializar escena de The Fog");
            EditorUtility.SetDirty(composer);
            EditorSceneManager.MarkSceneDirty(composer.gameObject.scene);
            Selection.activeGameObject = hierarchy.gameObject;
            Debug.Log("Escena lista para editar. Guarda la escena para conservar la jerarquía.", composer);
        }

        static ValdaraScene FindComposer()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
                return null;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.TryGetComponent(out ValdaraScene composer))
                    return composer;
            }
            return null;
        }

        static void SaveMaterials(Transform hierarchy)
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
                AssetDatabase.CreateFolder("Assets/TheFog", "Materiales");

            foreach (var renderer in hierarchy.GetComponentsInChildren<MeshRenderer>())
            {
                var material = renderer.sharedMaterial;
                if (material == null || !material.name.StartsWith("Valdara-", StringComparison.Ordinal)
                    || EditorUtility.IsPersistent(material))
                    continue;

                string path = MaterialFolder + "/" + material.name + ".mat";
                Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (existing != null)
                    renderer.sharedMaterial = existing;
                else
                    AssetDatabase.CreateAsset(material, path);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
