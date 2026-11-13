using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace TheFog.EditorTools
{
    public static class DanielRoomFinish
    {
        public static void Run()
        {
            var scene=EditorSceneManager.OpenScene(DanielRoomBuilder.ScenePath);
            var parent=GameObject.Find("HABITACION · estructura editable").transform;
            var wood=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/DanielRoom/Madera.mat");
            var metal=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/DanielRoom/Metal.mat");
            var paper=new Material(Shader.Find("Universal Render Pipeline/Lit"));paper.color=new Color(.48f,.44f,.31f);
            const string paperPath="Assets/Art/DanielRoom/Papel.mat";
            var existing=AssetDatabase.LoadAssetAtPath<Material>(paperPath);if(existing!=null){Object.DestroyImmediate(paper);paper=existing;}else AssetDatabase.CreateAsset(paper,paperPath);
            if(GameObject.Find("DETALLES HABITACION")==null)
            {
                var details=new GameObject("DETALLES HABITACION").transform;details.SetParent(parent,false);
                Box("Puerta · acceso a torre (sin conexión todavía)",new Vector3(3.87f,1.13f,-.2f),new Vector3(.08f,2.26f,1.0f),wood,details);
                Box("Tirador puerta",new Vector3(3.8f,1.05f,.14f),new Vector3(.08f,.05f,.17f),metal,details);
                Box("Zócalo izquierdo",new Vector3(-3.87f,.14f,0),new Vector3(.08f,.28f,7),wood,details);
                Box("Zócalo posterior",new Vector3(0,.14f,-3.37f),new Vector3(8,.28f,.08f),wood,details);
                Box("Zócalo derecho",new Vector3(3.87f,.14f,0),new Vector3(.08f,.28f,7),wood,details);
                for(int i=-3;i<=3;i++) Box("Viga pared posterior",new Vector3(i,1.5f,-3.35f),new Vector3(.04f,3,.045f),wood,details);
                Box("Tablero de notas",new Vector3(-3.84f,1.72f,1.6f),new Vector3(.08f,1.05f,1.6f),wood,details);
                for(int i=0;i<4;i++) Box("Parte de guardia",new Vector3(-3.79f,1.52f+(i/2)*.43f,1.25f+(i%2)*.67f),new Vector3(.01f,.34f,.49f),paper,details);
                Box("Cuaderno de Daniel",new Vector3(-1.4f,.7f,-2.4f),new Vector3(.25f,.03f,.34f),paper,details);
                Box("Lámpara techo",new Vector3(0,2.99f,0),new Vector3(.7f,.08f,.35f),metal,details);
            }
            // Weathered plank texture: a small project-owned raster, no external dependencies.
            const string texPath="Assets/Art/DanielRoom/Tablas.png";
            if(!File.Exists(texPath))
            {
                var texture=new Texture2D(128,128);var random=new System.Random(42);
                for(int y=0;y<128;y++)for(int x=0;x<128;x++)
                {
                    bool seam=x%32==0 || (y==0 && x<64) || (y==64 && x>=64);
                    float v=seam?.35f:.8f+(float)random.NextDouble()*.25f+Mathf.Sin(y*.17f+x*1.2f)*.04f;
                    texture.SetPixel(x,y,new Color(v,v*.92f,v*.77f));
                }
                texture.Apply();File.WriteAllBytes(texPath,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(texPath);
                var importer=(TextureImporter)AssetImporter.GetAtPath(texPath);importer.filterMode=FilterMode.Point;importer.wrapMode=TextureWrapMode.Repeat;importer.SaveAndReimport();
            }
            wood.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);wood.mainTextureScale=new Vector2(4,4);EditorUtility.SetDirty(wood);
            var terminal=Object.FindAnyObjectByType<TheFog.DanielRoom.DanielTerminal>();
            terminal.continueButton.interactable=false;
            var camera=terminal.view;var p=camera.transform.position;var q=camera.transform.rotation;
            camera.transform.SetPositionAndRotation(terminal.terminalPose.position,terminal.terminalPose.rotation);DanielRoomBuilder.Capture(camera,"docs/daniel-room-menu.png");
            camera.transform.SetPositionAndRotation(new Vector3(1.5f,2.45f,-3.12f),Quaternion.identity);camera.transform.LookAt(new Vector3(-.3f,1.0f,.8f));DanielRoomBuilder.Capture(camera,"docs/daniel-room-overview.png");
            camera.transform.SetPositionAndRotation(p,q);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            EditorApplication.Exit(0);
        }
        static void Box(string name,Vector3 p,Vector3 size,Material material,Transform parent)
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(parent,false);o.transform.position=p;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=material;
        }
    }
}
