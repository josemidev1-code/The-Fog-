using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheFog
{
    public static class CasaRetroArt
    {
        const string Packs = "Assets/ThirdParty/ElegantCrow";
        const string Materials = "Assets/TheFog/Materiales";

        public static void Install()
        {
            var scene = EditorSceneManager.OpenScene(CasaRenderSetup.ScenePath);
            var stage = GameObject.Find("Casa de Daniel").transform;
            var grove = stage.Find("01 · Linde y claro");
            var tower = stage.Find("04 · Torre de vigilancia");
            if (grove.Find("Vegetación PSX") != null || tower.Find("Cabina de seguridad") != null)
                throw new InvalidOperationException("La actualización ya está aplicada.");

            var plants = Group(grove, "Vegetación PSX", Vector3.zero);
            var archive = Group(grove, "Vegetación anterior", Vector3.zero);
            archive.gameObject.SetActive(false);
            string[] trees = { "tree01", "tree02", "tree24" };
            int index = 0;
            foreach (var old in grove.Cast<Transform>().Where(t => t.name == "Roble" || t.name == "Arbusto").ToArray())
            {
                var bounds = BoundsOf(old.gameObject);
                bool tree = old.name == "Roble";
                string id = tree ? trees[index++ % trees.Length] : "bush03";
                string source = tree ? $"/Trees/models/{id}.fbx" : "/Nature/Models/bushes/bush03.fbx";
                string texture = tree ? $"/Trees/textures/{id}.png" : "/Nature/textures/bushes/bush3_fall.png";
                Place(plants, source, MaterialFor(id, new Color(.81f,.84f,.74f), texture, true),
                    old.position, old.rotation, bounds.size.y);
                old.SetParent(archive, true);
            }
            var grass = MaterialFor("Hierba-otono", new Color(.76f,.80f,.69f), "/Nature/textures/grass/grass_fall.png", true);
            var random = new System.Random(96);
            for (int i = 0; i < 44; i++)
            {
                float x = (i % 2 == 0 ? -1 : 1) * (3.7f + (float)random.NextDouble() * 13);
                float z = -24 + (float)random.NextDouble() * 15;
                Place(plants, "/Nature/Models/grass/grass01.fbx", grass, new Vector3(x,0,z),
                    Quaternion.Euler(0,random.Next(360),0), .25f + (float)random.NextDouble() * .24f);
            }
            var house = Place(grove, "/Houses/Models/house1.glb",
                MaterialFor("Casa-retro-256", Color.white, "/Houses/textures/256x256/house1_tex1.png"),
                new Vector3(-21,0,17), Quaternion.identity, 5.8f);
            house.name = "Casa auxiliar retro · exterior";
            var houseBounds = BoundsOf(house);
            var wall = house.AddComponent<BoxCollider>();
            wall.center = house.transform.InverseTransformPoint(houseBounds.center);
            wall.size = houseBounds.size;
            house.transform.rotation = Quaternion.Euler(0,145,0);

            DressTower(tower);
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("No se pudo guardar CasaDaniel.");
        }

        static void DressTower(Transform tower)
        {
            Vector3 center = new Vector3(16.4f,0,14);
            var steel = MaterialFor("Torre-acero-oxidado", new Color(.48f,.38f,.30f), "Chapa");
            var olive = MaterialFor("Torre-chapa-oliva", new Color(.50f,.52f,.39f), "Chapa");
            var dark = MaterialFor("Torre-marcos", new Color(.17f,.19f,.17f));
            var glass = MaterialFor("Torre-cristal-sucio", new Color(.24f,.30f,.29f));
            var concrete = MaterialFor("Torre-hormigon", new Color(.52f,.51f,.46f), "Piedra");
            var cameraPaint = MaterialFor("Torre-CCTV-marfil", new Color(.70f,.67f,.56f));
            foreach (var mesh in tower.GetComponentsInChildren<MeshRenderer>())
                if (mesh.sharedMaterial != null && mesh.sharedMaterial.name.StartsWith("Valdara-Daniel-"))
                    mesh.sharedMaterial = mesh.name == "Dado de piedra" ? concrete : steel;

            var cabin = Group(tower, "Cabina de seguridad", center + new Vector3(-1.12f,9,-.30f));
            Cube(cabin, "Techo de chapa", new Vector3(0,2.36f,0), new Vector3(2.60f,.15f,3.55f), steel);
            for (int i = 0; i < 13; i++)
                Cube(cabin, "Nervio de cubierta", new Vector3(-1.2f+i*.20f,2.45f,0), new Vector3(.035f,.055f,3.55f), olive, false);
            foreach (float side in new[] {-1f,1f})
            {
                Cube(cabin, "Panel lateral inferior", new Vector3(side*1.09f,.49f,0), new Vector3(.10f,.98f,2.95f), olive);
                Cube(cabin, "Ventana lateral", new Vector3(side*1.09f,1.55f,0), new Vector3(.075f,1.13f,2.94f), glass);
                for (int i = -2; i <= 2; i++)
                    Cube(cabin, "Montante", new Vector3(side*1.10f,1.55f,i*.71f), new Vector3(.13f,1.18f,.075f), dark, false);
            }
            Cube(cabin, "Panel frontal", new Vector3(0,.49f,-1.46f), new Vector3(2.20f,.98f,.10f), olive);
            Cube(cabin, "Ventana frontal", new Vector3(0,1.55f,-1.46f), new Vector3(2.20f,1.13f,.075f), glass);
            Cube(cabin, "Marco frontal", new Vector3(0,1.56f,-1.51f), new Vector3(.08f,1.18f,.12f), dark, false);
            foreach (float x in new[] {-.83f,.83f})
                Cube(cabin, "Panel trasero", new Vector3(x,1.13f,1.46f), new Vector3(.55f,2.26f,.10f), olive);
            Cube(cabin, "Dintel", new Vector3(0,2.21f,1.46f), new Vector3(1.13f,.16f,.12f), dark);
            Cube(cabin, "Puerta abierta", new Vector3(-.56f,1.05f,1.02f), new Vector3(.07f,2.10f,.90f), steel);
            Cube(cabin, "Mesa de control", new Vector3(-.66f,.89f,-.83f), new Vector3(.62f,.12f,.75f), steel);
            Cube(cabin, "Monitor CRT", new Vector3(-.67f,1.12f,-.91f), new Vector3(.38f,.34f,.37f), dark, false);
            Cube(cabin, "Pantalla CRT", new Vector3(-.67f,1.12f,-.718f), new Vector3(.30f,.24f,.018f), glass, false);
            Cube(cabin, "Mástil de antena", new Vector3(-.92f,2.93f,.93f), new Vector3(.05f,1.03f,.05f), steel, false);
            Cube(cabin, "Antena", new Vector3(-.92f,3.36f,.93f), new Vector3(.82f,.035f,.035f), dark, false);
            Cube(cabin, "Soporte CCTV", new Vector3(.84f,1.96f,-1.75f), new Vector3(.09f,.09f,.64f), steel, false);
            Cube(cabin, "Caja de conexiones", new Vector3(.84f,1.74f,-1.54f), new Vector3(.20f,.23f,.14f), steel, false);
            Cube(cabin, "Conduit CCTV", new Vector3(.84f,1.89f,-1.57f), new Vector3(.028f,.28f,.03f), dark, false);
            var cctv = Group(cabin, "Cámara CCTV analógica", new Vector3(.84f,2.07f,-2.08f));
            cctv.localRotation = Quaternion.Euler(-23, 15, 0);
            Cube(cctv, "Carcasa", Vector3.zero, new Vector3(.27f,.23f,.55f), cameraPaint, false);
            Cube(cctv, "Visera", new Vector3(0,.13f,-.055f), new Vector3(.33f,.05f,.67f), steel, false);
            Cube(cctv, "Cristal del objetivo", new Vector3(0,0,-.285f), new Vector3(.18f,.16f,.022f), dark, false);
            Cube(cctv, "Lente", new Vector3(0,0,-.300f), new Vector3(.085f,.085f,.016f), glass, false);
        }

        static Transform Group(Transform parent, string name, Vector3 localPosition)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            group.localPosition = localPosition;
            return group;
        }

        static void Cube(Transform parent, string name, Vector3 position, Vector3 size, Material material, bool solid = true)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = position;
            cube.transform.localScale = size;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) Object.DestroyImmediate(cube.GetComponent<Collider>());
        }

        static GameObject Place(Transform parent, string source, Material material, Vector3 position, Quaternion rotation, float height)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Packs + source);
            if (prefab == null) throw new FileNotFoundException(Packs + source);
            var root = Group(parent, prefab.name, Vector3.zero);
            var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
            var bounds = BoundsOf(model);
            float scale = height / bounds.size.y;
            model.transform.localScale *= scale;
            model.transform.localPosition -= (bounds.center - Vector3.up * bounds.extents.y) * scale;
            root.SetPositionAndRotation(position, rotation);
            foreach (var mesh in model.GetComponentsInChildren<Renderer>())
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.sharedMaterials.Length).ToArray();
            return root.gameObject;
        }

        static Bounds BoundsOf(GameObject model)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new InvalidOperationException("Modelo sin geometría: " + model.name);
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        static Material MaterialFor(string name, Color color, string texture = null, bool foliage = false)
        {
            string path = Materials + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            if (texture != null)
            {
                string texturePath = texture.StartsWith("/") ? Packs + texture : $"Assets/TheFog/Resources/Daniel/{texture}.png";
                if (AssetImporter.GetAtPath(texturePath) is TextureImporter importer && importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    importer.alphaIsTransparency = foliage;
                    importer.SaveAndReimport();
                }
                material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (material.mainTexture == null) throw new FileNotFoundException(texturePath);
            }
            material.SetFloat("_Smoothness", .12f);
            if (foliage)
            {
                material.SetFloat("_AlphaClip", 1);
                material.SetFloat("_Cutoff", .4f);
                material.SetFloat("_Cull", 0);
                material.EnableKeyword("_ALPHATEST_ON");
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.renderQueue = 2450;
            }
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        public static void InstallBatch()
        {
            try { Install(); EditorApplication.Exit(0); }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }
    }
}
