using System;
using System.IO;
using System.Linq;
using TheFog.DanielRoom;
using TheFog.FirstPlayable;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace TheFog.EditorTools
{
    public static class DanielRoomBuilder
    {
        public const string ScenePath="Assets/Scenes/DanielRoom.unity";
        const string Art="Assets/Art/DanielRoom";
        static Font font;
        static Material wood,wall,metal,dark,green;
        static readonly Color Ink=new Color(.56f,.91f,.7f);
        [MenuItem("THE FOG/Daniel/Open room")]
        public static void Open() { EditorSceneManager.OpenScene(ScenePath); }
        public static void BuildBatch()
        {
            try { Build(); EditorApplication.Exit(0); }
            catch(Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }
        public static void Build()
        {
            if(File.Exists(ScenePath)) throw new Exception("DanielRoom exists; refusing to overwrite hand edits.");
            AssetDatabase.Refresh(); Directory.CreateDirectory(Art);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            wood=Mat("Madera",new Color(.22f,.15f,.09f)); wall=Mat("Pared",new Color(.22f,.25f,.22f));
            metal=Mat("Metal",new Color(.24f,.28f,.26f)); dark=Mat("Terminal",new Color(.055f,.065f,.06f));
            green=Mat("Cristal noche",new Color(.07f,.16f,.15f),new Color(.06f,.13f,.13f));
            RenderSettings.skybox=null; RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.16f,.2f,.19f); RenderSettings.fog=false;
            var shell=new GameObject("HABITACION · estructura editable").transform;
            Box("Suelo",new Vector3(0,-.12f,0),new Vector3(8,.24f,7),wood,shell);
            Box("Techo",new Vector3(0,3.1f,0),new Vector3(8,.2f,7),wall,shell);
            Box("Pared izquierda",new Vector3(-4,1.5f,0),new Vector3(.2f,3,7),wall,shell);
            Box("Pared derecha",new Vector3(4,1.5f,0),new Vector3(.2f,3,7),wall,shell);
            Box("Pared posterior",new Vector3(0,1.5f,-3.5f),new Vector3(8,3,.2f),wall,shell);
            // Window opening is built from separate wall sections, so it remains editable.
            Box("Ventana · antepecho",new Vector3(0,.5f,3.5f),new Vector3(8,1,.2f),wall,shell);
            Box("Ventana · dintel",new Vector3(0,2.85f,3.5f),new Vector3(8,.5f,.2f),wall,shell);
            Box("Ventana · lateral izquierdo",new Vector3(-3,1.8f,3.5f),new Vector3(2,1.6f,.2f),wall,shell);
            Box("Ventana · lateral derecho",new Vector3(3,1.8f,3.5f),new Vector3(2,1.6f,.2f),wall,shell);
            Box("Cristal · noche de Valdara",new Vector3(0,1.8f,3.58f),new Vector3(4,1.6f,.07f),green,shell);
            for(int i=-1;i<=1;i++) Box("Marco ventana",new Vector3(i*1.9f,1.8f,3.4f),new Vector3(.055f,1.6f,.09f),wood,shell);
            Box("Marco horizontal",new Vector3(0,1.8f,3.4f),new Vector3(4,.055f,.09f),wood,shell);
            var furniture=new GameObject("MUEBLES · modelos propios optimizados").transform;
            Model("INT_Cama-", "Cama de Daniel",new Vector3(-2.7f,0,-1.3f),1.0f,2.9f,0,furniture);
            Model("INT_MesitaNoche-","Mesita",new Vector3(-1.4f,0,-2.4f),.65f,.7f,0,furniture);
            Model("INT_Estanteria-","Estantería",new Vector3(3.2f,0,-1.9f),2.1f,1.2f,180,furniture);
            Model("INT_SillaMadera-","Silla de trabajo",new Vector3(1.5f,0,1.2f),1.0f,.65f,180,furniture);
            var desk=new GameObject("PUESTO DE TELECOMUNICACIONES").transform;
            Box("Mesa de trabajo",new Vector3(0,.78f,2.35f),new Vector3(2.8f,.13f,1.0f),wood,desk);
            foreach(float x in new[]{-1.25f,1.25f}) foreach(float z in new[]{1.96f,2.74f}) Box("Pata",new Vector3(x,.38f,z),new Vector3(.1f,.76f,.1f),metal,desk);
            var computer=new GameObject("PC PROVISIONAL · sustituir por modelo original").transform; computer.SetParent(desk);
            Box("Monitor CRT",new Vector3(0,1.35f,2.49f),new Vector3(1.28f,.94f,.48f),metal,computer);
            Box("Peana",new Vector3(0,.9f,2.5f),new Vector3(.65f,.14f,.48f),metal,computer);
            Box("Pantalla",new Vector3(0,1.37f,2.237f),new Vector3(1.1f,.72f,.02f),dark,computer);
            Box("Teclado",new Vector3(0,.9f,1.99f),new Vector3(1.0f,.06f,.3f),metal,computer);
            for(int i=0;i<10;i++) for(int j=0;j<3;j++) Box("Tecla",new Vector3(-.43f+i*.095f,.937f,1.9f+j*.08f),new Vector3(.07f,.018f,.055f),dark,computer,false);
            Box("Radio de guardia",new Vector3(-1,.98f,2.5f),new Vector3(.5f,.3f,.32f),dark,desk);
            for(int i=0;i<6;i++) Box("Rejilla radio",new Vector3(-1.14f+i*.045f,.98f,2.33f),new Vector3(.015f,.19f,.015f),metal,desk,false);
            Box("Antena",new Vector3(-.85f,1.43f,2.55f),new Vector3(.015f,.6f,.015f),metal,desk,false);
            Box("Torre PC",new Vector3(.92f,1.06f,2.52f),new Vector3(.28f,.45f,.4f),metal,computer);
            var lamp=Box("Lámpara de mesa",new Vector3(-1.17f,1.48f,2.66f),new Vector3(.24f,.09f,.3f),wood,desk);
            var warm=LightAt("Luz cálida",new Vector3(-1.05f,1.8f,2.0f),new Color(1,.64f,.3f),4,5);
            LightAt("Luz de ventana",new Vector3(0,2.25f,3.1f),new Color(.3f,.65f,.64f),2.2f,7);
            LightAt("Lámpara dormitorio",new Vector3(-1.5f,1.5f,-2),new Color(1,.63f,.29f),1.6f,5);
            LightAt("Resplandor CRT",new Vector3(0,1.4f,2.0f),new Color(.3f,1,.55f),.5f,2);
            var playerGO=new GameObject("Daniel"); playerGO.transform.position=new Vector3(0,.05f,.1f);
            var cc=playerGO.AddComponent<CharacterController>(); cc.height=1.75f; cc.radius=.24f; cc.center=new Vector3(0,.88f,0);
            var input=playerGO.AddComponent<FogInput>(); input.SetupDefaults();
            var player=playerGO.AddComponent<FogPlayerController>(); player.input=input; player.walkSpeed=2.2f;
            var cameraGO=new GameObject("Main Camera"); cameraGO.tag="MainCamera"; cameraGO.transform.SetParent(playerGO.transform,false);
            cameraGO.transform.localPosition=new Vector3(0,1.62f,0);
            var camera=cameraGO.AddComponent<Camera>(); camera.nearClipPlane=.04f; camera.farClipPlane=60; camera.fieldOfView=52;
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.025f,.04f,.04f);
            cameraGO.AddComponent<AudioListener>(); player.cameraPivot=cameraGO.transform;
            var graphics=playerGO.AddComponent<FogGraphicsSettings>(); graphics.input=input; graphics.targetCamera=camera;
            // Room lights remain on in Low: they are required to read the terminal and navigate.
            graphics.decorativeLights=Array.Empty<Light>();
            var terminal=new GameObject("Terminal · menú y sesión").AddComponent<DanielTerminal>();
            terminal.view=camera; terminal.player=player; terminal.input=input; terminal.graphics=graphics; terminal.terminalTarget=computer;
            terminal.standingPose=Pose("Inicio jugable",new Vector3(0,.05f,.05f),Quaternion.identity);
            terminal.terminalPose=Pose("Vista menú",new Vector3(.18f,1.47f,.98f),Quaternion.identity);
            terminal.terminalPose.LookAt(new Vector3(0,1.37f,2.23f));
            var screen=new GameObject("PANTALLA · menú principal",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));
            var canvas=screen.GetComponent<Canvas>(); canvas.renderMode=RenderMode.WorldSpace; canvas.worldCamera=camera;
            screen.transform.SetParent(computer,false); screen.transform.position=new Vector3(0,1.37f,2.22f); screen.transform.localScale=Vector3.one*.001f;
            screen.GetComponent<RectTransform>().sizeDelta=new Vector2(1080,700);
            var bg=Panel("Fondo CRT",screen.transform,new Vector2(1080,700),Vector2.zero,new Color(.018f,.042f,.029f));
            Label("THE FOG",bg.transform,new Vector2(0,236),60,TextAnchor.MiddleCenter);
            Label("VALDARA / TELECOMUNICACIONES",bg.transform,new Vector2(0,172),21,TextAnchor.MiddleCenter);
            Label("ESTACIÓN 04     ·     OPERADOR: DANIEL",bg.transform,new Vector2(0,133),17,TextAnchor.MiddleCenter);
            terminal.homePage=new GameObject("Menú",typeof(RectTransform)); terminal.homePage.transform.SetParent(bg.transform,false);
            ButtonAt("NUEVA PARTIDA",terminal.homePage.transform,65,terminal.NewGame);
            terminal.continueButton=ButtonAt("CONTINUAR",terminal.homePage.transform,-5,terminal.ContinueGame);
            ButtonAt("AJUSTES",terminal.homePage.transform,-75,terminal.Settings);
            ButtonAt("SALIR",terminal.homePage.transform,-145,terminal.Quit);
            terminal.settingsPage=new GameObject("Ajustes",typeof(RectTransform)); terminal.settingsPage.transform.SetParent(bg.transform,false);
            Label("CALIDAD GRÁFICA",terminal.settingsPage.transform,new Vector2(0,85),23,TextAnchor.MiddleCenter);
            ButtonAt("BAJO",terminal.settingsPage.transform,30,terminal.Low,-260,240);
            ButtonAt("MEDIO",terminal.settingsPage.transform,30,terminal.Medium,0,240);
            ButtonAt("ALTO",terminal.settingsPage.transform,30,terminal.High,260,240);
            Label("SENSIBILIDAD",terminal.settingsPage.transform,new Vector2(-280,-40),20,TextAnchor.MiddleCenter);
            terminal.sensitivity=SliderAt("Sensibilidad",terminal.settingsPage.transform,-80,.025f,.2f,.09f,terminal.SetSensitivity);
            Label("VOLUMEN",terminal.settingsPage.transform,new Vector2(-300,-135),20,TextAnchor.MiddleCenter);
            terminal.volume=SliderAt("Volumen",terminal.settingsPage.transform,-175,0,1,.7f,terminal.SetVolume);
            ButtonAt("VOLVER",terminal.settingsPage.transform,-245,terminal.Back);
            terminal.status=Label("SEÑAL ESTABLE\nESPERANDO INICIO DE TURNO",bg.transform,new Vector2(0,-267),17,TextAnchor.MiddleCenter);
            terminal.settingsPage.SetActive(false);
            var overlay=new GameObject("Indicaciones",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
            overlay.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=overlay.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1280,720);
            terminal.hint=Label("TERMINAL DE TELECOMUNICACIONES · VALDARA",overlay.transform,new Vector2(0,-325),16,TextAnchor.MiddleCenter);
            var events=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            EditorSceneManager.SaveScene(scene,ScenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)}.Concat(EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath)).ToArray();
            AssetDatabase.SaveAssets();
            var pos=camera.transform.position; var rot=camera.transform.rotation;
            camera.transform.SetPositionAndRotation(terminal.terminalPose.position,terminal.terminalPose.rotation);
            Capture(camera,"docs/daniel-room-menu.png");
            camera.transform.SetPositionAndRotation(new Vector3(2.9f,2.3f,-2.8f),Quaternion.identity); camera.transform.LookAt(new Vector3(-.3f,1.1f,.7f));
            Capture(camera,"docs/daniel-room-overview.png");
            camera.transform.SetPositionAndRotation(pos,rot);
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            File.WriteAllText("docs/daniel-room-build.json","{\"saved\":true,\"scene\":\""+ScenePath+"\",\"computerIsPlaceholder\":true}");
            Debug.Log("DANIEL ROOM BUILD PASS");
        }
        static Transform Pose(string name,Vector3 p,Quaternion r) { var t=new GameObject(name).transform; t.SetPositionAndRotation(p,r); return t; }
        static Material Mat(string name,Color color,Color emission=default)
        {
            string path=Art+"/"+name+".mat"; var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m,path);}
            m.color=color; m.SetFloat("_Smoothness",.1f);
            if(emission!=default){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission);}
            return m;
        }
        static GameObject Box(string name,Vector3 p,Vector3 s,Material m,Transform parent,bool collision=true)
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Cube); o.name=name; o.transform.SetParent(parent,false); o.transform.position=p; o.transform.localScale=s;
            o.GetComponent<Renderer>().sharedMaterial=m; if(!collision) UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>()); return o;
        }
        static void Model(string prefix,string name,Vector3 p,float height,float footprint,float yaw,Transform parent)
        {
            string path=Directory.GetFiles("Assets/Models/Optimized",prefix+"*.glb").FirstOrDefault();
            if(path==null) throw new Exception("Missing optimized furniture: "+prefix);
            var root=new GameObject(name); root.transform.SetParent(parent); var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path)); model.transform.SetParent(root.transform,false);
            var rs=model.GetComponentsInChildren<Renderer>(); var b=rs[0].bounds; foreach(var r in rs)b.Encapsulate(r.bounds);
            float scale=Mathf.Min(height/b.size.y,footprint/Mathf.Max(b.size.x,b.size.z)); model.transform.localScale*=scale;
            model.transform.localPosition=-new Vector3(b.center.x,b.min.y,b.center.z)*scale;
            var box=root.AddComponent<BoxCollider>(); box.center=Vector3.up*b.size.y*scale*.5f; box.size=b.size*scale;
            root.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));
        }
        static Light LightAt(string name,Vector3 p,Color c,float intensity,float range)
        {
            var l=new GameObject(name).AddComponent<Light>(); l.transform.position=p;l.type=LightType.Point;l.color=c;l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;return l;
        }
        static GameObject Panel(string name,Transform parent,Vector2 size,Vector2 pos,Color color)
        {
            var o=new GameObject(name,typeof(RectTransform),typeof(Image));o.transform.SetParent(parent,false);var r=o.GetComponent<RectTransform>();r.sizeDelta=size;r.anchoredPosition=pos;o.GetComponent<Image>().color=color;return o;
        }
        static Text Label(string value,Transform parent,Vector2 pos,int size,TextAnchor align)
        {
            var o=new GameObject(value.Split('\n')[0],typeof(RectTransform),typeof(Text));o.transform.SetParent(parent,false);var r=o.GetComponent<RectTransform>();r.sizeDelta=new Vector2(1000,size*2.8f);r.anchoredPosition=pos;
            var t=o.GetComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.color=Ink;t.alignment=align;t.raycastTarget=false;return t;
        }
        static Button ButtonAt(string name,Transform parent,float y,UnityAction callback,float x=0,float width=720)
        {
            var o=Panel(name,parent,new Vector2(width,56),new Vector2(x,y),new Color(.06f,.14f,.095f)); var b=o.AddComponent<Button>(); b.targetGraphic=o.GetComponent<Image>();
            var colors=b.colors;colors.highlightedColor=new Color(.4f,.7f,.45f);colors.selectedColor=new Color(.4f,.7f,.45f);colors.disabledColor=new Color(.25f,.27f,.25f);b.colors=colors;
            UnityEventTools.AddPersistentListener(b.onClick,callback);Label(name,o.transform,Vector2.zero,27,TextAnchor.MiddleCenter);return b;
        }
        static Slider SliderAt(string name,Transform parent,float y,float min,float max,float value,UnityAction<float> callback)
        {
            var o=Panel(name,parent,new Vector2(680,22),new Vector2(0,y),new Color(.08f,.18f,.12f)); var s=o.AddComponent<Slider>();s.targetGraphic=o.GetComponent<Image>();s.minValue=min;s.maxValue=max;s.value=value;
            var handle=Panel("Handle",o.transform,new Vector2(24,38),Vector2.zero,Ink);s.handleRect=handle.GetComponent<RectTransform>();UnityEventTools.AddPersistentListener(s.onValueChanged,callback);return s;
        }
        public static void Capture(Camera camera,string path)
        {
            Canvas.ForceUpdateCanvases(); var rt=new RenderTexture(1280,720,24);var previous=RenderTexture.active;var old=camera.targetTexture;
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());
            camera.targetTexture=old;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}
