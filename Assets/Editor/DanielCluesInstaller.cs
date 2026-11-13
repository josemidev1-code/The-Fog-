using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TheFog.DanielRoom;
namespace TheFog.EditorTools
{
    public static class DanielCluesInstaller
    {
        public static void Run()
        {
            var scene=EditorSceneManager.OpenScene(DanielRoomBuilder.ScenePath);
            var terminal=Object.FindAnyObjectByType<DanielTerminal>();
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(terminal.gameObject);
            var investigation=terminal.gameObject.GetComponent<DanielRoomInvestigation>();
            if(investigation==null)investigation=terminal.gameObject.AddComponent<DanielRoomInvestigation>();
            investigation.terminal=terminal;
            Add("Cuaderno de Daniel","CUADERNO DE DANIEL","Antes de empezar el turno:\n\n— Revisar la radio.\n— Comprobar el terminal.\n— Anotar cualquier señal extraña.\n\nLa casa está en silencio. Todavía puedes revisar la habitación.");
            Add("Tablero de notas","PARTE DE GUARDIA","VALDARA · PUESTO DE TELECOMUNICACIONES\n\nMantener la radio a la escucha durante el turno de noche.\n\nRegistrar los cortes de señal. Comprobar el equipo antes de abandonar el puesto.");
            Add("Radio de guardia","RADIO DEL PUESTO","El equipo está encendido, pero no recibes ninguna voz.\n\nEl indicador de señal apenas responde. Tendrás que comprobar qué está ocurriendo en Valdara.");
            if(investigation.panel==null)
            {
                var panel=new GameObject("Lectura · documentos",typeof(RectTransform),typeof(Image)); panel.transform.SetParent(terminal.hint.transform.parent,false);
                panel.GetComponent<RectTransform>().sizeDelta=new Vector2(850,510);panel.GetComponent<Image>().color=new Color(.025f,.045f,.037f,.98f);panel.GetComponent<Image>().raycastTarget=false;
                investigation.panel=panel;investigation.heading=TextAt(panel.transform,"Título",new Vector2(0,175),32,new Vector2(740,70));
                investigation.body=TextAt(panel.transform,"Documento",new Vector2(0,-20),25,new Vector2(740,300));panel.SetActive(false);
            }
            foreach(var t in Object.FindObjectsByType<Transform>()) if(t.name=="Parte de guardia") { var collider=t.GetComponent<Collider>(); if(collider!=null)Object.DestroyImmediate(collider); }
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();EditorApplication.Exit(0);
        }
        static void Add(string name,string title,string body)
        {
            var go=GameObject.Find(name);if(go==null)throw new System.Exception("Missing clue prop "+name);
            var clue=go.GetComponent<DanielRoomClue>();if(clue==null)clue=go.AddComponent<DanielRoomClue>();clue.title=title;clue.body=body;
        }
        static Text TextAt(Transform parent,string name,Vector2 pos,int size,Vector2 dimensions)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchoredPosition=pos;rect.sizeDelta=dimensions;
            var text=go.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;text.color=new Color(.68f,.85f,.73f);text.alignment=TextAnchor.UpperLeft;text.raycastTarget=false;return text;
        }
    }
}
