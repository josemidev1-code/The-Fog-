using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheFog.Puesto;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheFog.EditorTools
{
    /// <summary>
    /// Construye la escena jugable "Puesto de Daniel" (torre de vigilancia de Valdara) usando solo los modelos 3D de la carpeta del equipo
    /// (copias optimizadas en Assets/Models/Optimized). La arquitectura de la cabina es geometría simple porque no existe modelo de torre.
    /// Batch:  Unity.exe -batchmode -projectPath . -executeMethod TheFog.EditorTools.PuestoDanielBuilder.Run -quit
    /// </summary>
    public static class PuestoDanielBuilder
    {
        public const string ScenePath = "Assets/Scenes/PuestoDaniel.unity";
        const string ArtDir = "Assets/Art/Puesto";
        const string ModelDir = "Assets/Models/Optimized";
        const string PolyHavenDir = ModelDir + "/PolyHaven"; // modelos CC0 de polyhaven.com (glTF con texturas 1K)
        const float Floor = 7f;          // altura del suelo de la cabina
        const float Half = 2.7f;         // media anchura interior
        const float CabinHeight = 2.9f;
        // Props generados (PROP_*): giro para que su frontal mire al jugador sentado (hacia -Z).
        const float PropFrontYaw = -90f;
        const float MonitorFrontZ = 2.12f; // plano frontal del monitor sobre la mesa
        const float ScreenInset = 0.02f;   // la imagen va un poco por detrás del marco

        static readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();
        static readonly List<string> log = new List<string>();
        static readonly HashSet<string> usedModels = new HashSet<string>();
        static float deskTopForCapture;

        [MenuItem("THE FOG/Construir Puesto de Daniel")]
        public static void Run()
        {
            log.Clear(); usedModels.Clear(); mats.Clear(); treeMeshes.Clear(); roadCaps.Clear();
            Directory.CreateDirectory(ArtDir);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateMaterials();
            var feed = CreateFeedTexture();
            var profile = CreateVolumeProfile();

            // ---------------------------------------------------------------- ambiente
            RenderSettings.skybox = null;
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = 0.025f;
            RenderSettings.fogColor = new Color(0.20f, 0.23f, 0.22f);
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.10f, 0.12f, 0.12f);
            RenderSettings.reflectionIntensity = 0.25f;
            Lightmapping.bakedGI = false; Lightmapping.realtimeGI = false;

            var ambience = Group("AMBIENTE");
            var moonGo = new GameObject("Luna"); moonGo.transform.SetParent(ambience); moonGo.transform.rotation = Quaternion.Euler(38, -30, 0);
            var moon = moonGo.AddComponent<Light>(); moon.type = LightType.Directional; moon.color = new Color(0.55f, 0.64f, 0.70f); moon.intensity = 0.35f; moon.shadows = LightShadows.Soft; moon.shadowStrength = 0.75f;
            RenderSettings.sun = moon;
            var volumeGo = new GameObject("PostProceso"); volumeGo.transform.SetParent(ambience);
            var volume = volumeGo.AddComponent<Volume>(); volume.isGlobal = true; volume.sharedProfile = profile; volume.priority = 1;

            // ---------------------------------------------------------------- pueblo
            var village = Group("VALDARA");
            BuildVillage(village, out var sites, out var bankRing, out var banks);

            // ---------------------------------------------------------------- torre
            var tower = Group("TORRE DE VIGILANCIA");
            BuildTowerStructure(tower);
            var cabin = Group("CABINA", tower);
            var refs = BuildCabin(cabin, feed);

            // ---------------------------------------------------------------- jugador
            var player = BuildPlayer(out var cam, out var flashlight);

            // ---------------------------------------------------------------- sistemas
            var systems = Group("SISTEMAS");
            var hudGo = new GameObject("Interfaz"); hudGo.transform.SetParent(systems);
            var hud = hudGo.AddComponent<PuestoHud>(); hud.feedTexture = feed;
            const string fontDir = ArtDir + "/Fuentes/";
            hud.hudFont = AssetDatabase.LoadAssetAtPath<Font>(fontDir + "BarlowCondensed-Medium.ttf");
            hud.titleFont = AssetDatabase.LoadAssetAtPath<Font>(fontDir + "BarlowCondensed-SemiBold.ttf");
            hud.noteFont = AssetDatabase.LoadAssetAtPath<Font>(fontDir + "SpecialElite-Regular.ttf");
            hud.crtFont = AssetDatabase.LoadAssetAtPath<Font>(fontDir + "VT323-Regular.ttf");
            if (!hud.hudFont || !hud.titleFont || !hud.noteFont || !hud.crtFont) Debug.LogWarning("PUESTO DANIEL: falta alguna tipografía en " + fontDir);
            var gameGo = new GameObject("Partida"); gameGo.transform.SetParent(systems);
            var clock = gameGo.AddComponent<ShiftClock>();
            var fog = gameGo.AddComponent<FogThreat>(); fog.volume = volume; fog.bankRing = bankRing; fog.banks = banks;
            var sky = gameGo.AddComponent<SkyCycle>(); sky.moon = moon;
            var director = gameGo.AddComponent<AnomalyDirector>(); director.sites = sites;
            var radio = gameGo.AddComponent<RadioSystem>(); radio.speakerPoint = refs.radio; radio.beats = Beats();
            gameGo.AddComponent<PuestoAmbience>();
            gameGo.AddComponent<PuestoAutoTest>();
            var game = gameGo.AddComponent<PuestoGame>();
            game.player = player; game.hud = hud; game.clock = clock; game.fog = fog; game.power = refs.power; game.director = director;
            game.console = refs.console; game.radio = radio; game.flashlight = flashlight;

            // Consola: cada cámara apunta a su lugar vigilado.
            refs.console.feeds = sites.Select(s => new SecurityFeed { label = s.label, site = s, camera = s.GetComponentInChildren<Camera>(true) }).ToArray();

            // Lo que se mueve en juego no puede ser estático (batching).
            void Dynamic(Transform tr) { foreach (var x in tr.GetComponentsInChildren<Transform>(true)) x.gameObject.isStatic = false; }
            Dynamic(player.transform);
            foreach (var fx in UnityEngine.Object.FindObjectsByType<AnomalyEffect>(FindObjectsInactive.Include))
            { if (fx.target) Dynamic(fx.target); foreach (var go in fx.toggles) if (go) Dynamic(go.transform); }
            foreach (var bp in UnityEngine.Object.FindObjectsByType<BreakerPanel>()) Dynamic(bp.transform);
            Dynamic(bankRing);

            EditorSceneManager.SaveScene(scene, ScenePath);
            var buildScenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            buildScenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();

            Directory.CreateDirectory("docs/puesto");
            Capture(cam, "docs/puesto/editor_jugador.png");
            var tmpGo = new GameObject("CapturaTmp"); var tmp = tmpGo.AddComponent<Camera>(); tmp.CopyFrom(cam); tmp.farClipPlane = 400;
            tmp.transform.SetPositionAndRotation(new Vector3(26, 22, -34), Quaternion.identity); tmp.transform.LookAt(new Vector3(0, 6, 4)); Capture(tmp, "docs/puesto/editor_torre_exterior.png");
            tmp.transform.SetPositionAndRotation(new Vector3(0, 70, -10), Quaternion.Euler(80, 0, 0)); float d = RenderSettings.fogDensity; RenderSettings.fogDensity = 0.002f; Capture(tmp, "docs/puesto/editor_planta.png");
            tmp.transform.SetPositionAndRotation(new Vector3(0, Floor + 6.5f, 0), Quaternion.Euler(90, 0, 0)); tmp.nearClipPlane = 3.4f; Capture(tmp, "docs/puesto/editor_cabina_planta.png"); tmp.nearClipPlane = 0.05f;
            tmp.transform.SetPositionAndRotation(new Vector3(2.3f, Floor + 2.2f, -2.3f), Quaternion.identity); tmp.transform.LookAt(new Vector3(-0.5f, Floor + 0.6f, 1.2f)); Capture(tmp, "docs/puesto/editor_cabina_interior.png");
            tmp.transform.SetPositionAndRotation(new Vector3(-2.3f, Floor + 2.2f, 2.3f), Quaternion.identity); tmp.transform.LookAt(new Vector3(0.8f, Floor + 0.6f, -1.2f)); Capture(tmp, "docs/puesto/editor_cabina_interior2.png");
            tmp.transform.SetPositionAndRotation(new Vector3(0.2f, deskTopForCapture + 0.45f, 1.35f), Quaternion.identity); tmp.transform.LookAt(new Vector3(0, deskTopForCapture + 0.2f, 2.2f)); Capture(tmp, "docs/puesto/editor_mesa_consola.png");
            var fillGo = new GameObject("LuzCapturaTmp"); var fill = fillGo.AddComponent<Light>(); fill.type = LightType.Directional; fill.intensity = 1.1f; fill.color = new Color(0.8f, 0.85f, 0.9f); fillGo.transform.rotation = Quaternion.Euler(35, 60, 0);
            tmp.transform.SetPositionAndRotation(new Vector3(5.5f, 1.7f, -5.5f), Quaternion.identity); tmp.transform.LookAt(new Vector3(11.2f, 1.4f, -9.2f)); Capture(tmp, "docs/puesto/editor_caseta.png");
            tmp.transform.SetPositionAndRotation(new Vector3(6f, 1.7f, -19f), Quaternion.identity); tmp.transform.LookAt(new Vector3(17f, 3.2f, -23f)); Capture(tmp, "docs/puesto/editor_casa_generada.png");
            tmp.transform.SetPositionAndRotation(new Vector3(5.5f, 1.7f, -38f), Quaternion.identity); tmp.transform.LookAt(new Vector3(12.6f, 1.2f, -34.5f)); Capture(tmp, "docs/puesto/editor_muro_porton.png");
            tmp.transform.SetPositionAndRotation(new Vector3(6.5f, 1.7f, -13f), Quaternion.identity); tmp.transform.LookAt(new Vector3(12.5f, 0.8f, -20.5f)); Capture(tmp, "docs/puesto/editor_ph_coche_patio.png");
            tmp.transform.SetPositionAndRotation(new Vector3(7f, 1.7f, -2.5f), Quaternion.identity); tmp.transform.LookAt(new Vector3(11.5f, 0.9f, -7.5f)); Capture(tmp, "docs/puesto/editor_ph_caseta.png");
            tmp.transform.SetPositionAndRotation(new Vector3(5.5f, 5.2f, -16.5f), Quaternion.identity); tmp.transform.LookAt(new Vector3(2.5f, 4.5f, -14.5f)); Capture(tmp, "docs/puesto/editor_ph_camara.png");
            UnityEngine.Object.DestroyImmediate(fillGo);
            tmp.transform.SetPositionAndRotation(new Vector3(1.2f, Floor + 1.6f, -0.9f), Quaternion.identity); tmp.transform.LookAt(new Vector3(0.1f, Floor + 1.1f, -2.7f)); Capture(tmp, "docs/puesto/editor_ph_panel.png");
            tmp.transform.SetPositionAndRotation(new Vector3(0.3f, Floor + 1.75f, 0.6f), Quaternion.identity); tmp.transform.LookAt(new Vector3(0.2f, Floor + 1.5f, 2.6f)); Capture(tmp, "docs/puesto/editor_ph_mesa.png");
            RenderSettings.fogDensity = d;
            for (int i = 0; i < sites.Length; i++) { var sc = sites[i].GetComponentInChildren<Camera>(true); tmp.CopyFrom(sc); tmp.targetTexture = null; tmp.transform.SetPositionAndRotation(sc.transform.position, sc.transform.rotation); Capture(tmp, $"docs/puesto/editor_cam{i + 1}.png"); }
            UnityEngine.Object.DestroyImmediate(tmpGo);

            File.WriteAllText("docs/puesto/construccion.txt", "Escena: " + ScenePath + "\nModelos de la carpeta usados (" + usedModels.Count + "):\n - " + string.Join("\n - ", usedModels.OrderBy(x => x)) + "\n\n" + string.Join("\n", log));
            Debug.Log("PUESTO DANIEL: escena construida con " + usedModels.Count + " modelos del equipo");
        }

        // ==================================================================== pueblo
        static void BuildVillage(Transform root, out AnomalySite[] sites, out Transform bankRing, out ParticleSystem[] banks)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane); ground.name = "Terreno"; ground.transform.SetParent(root);
            ground.transform.localScale = new Vector3(40, 1, 40); ground.GetComponent<Renderer>().sharedMaterial = mats["Suelo"]; ground.isStatic = true;

            var roads = Group("Caminos", root);
            // Cada tramo secundario queda unos milímetros por debajo del principal: en los cruces ya no parpadean (z-fighting).
            // Los finales llegan hasta la boca de la mina y la fachada de cada casa (antes acababan en mitad del campo).
            // El antiguo tramo de acceso a la torre se quitaba: iba encima del camino oeste y parpadeaba con él.
            Road(roads, new Vector3(6, 0, -110), new Vector3(6, 0, 30), 4.2f, 0.030f);
            Road(roads, new Vector3(6, 0, 30), new Vector3(0, 0, 41), 3.4f, 0.027f);
            Road(roads, new Vector3(0, 0, 41), new Vector3(0, 0, 44.2f), 3.4f, 0.027f); // hasta los raíles de la mina
            Road(roads, new Vector3(6, 0, 4), new Vector3(30.2f, 0, 8.6f), 2.8f, 0.024f);
            Road(roads, new Vector3(6, 0, -3), new Vector3(-30.2f, 0, -4.2f), 2.8f, 0.024f);

            var siteList = new List<AnomalySite>();

            // ---------------- MINA (norte)
            var mineSite = Site(root, "MINA", new Vector3(0, 0, 48));
            var mine = Model("ENV_EntradaMina", mineSite.transform, new Vector3(0, 0, 48), 180, 6.5f);
            Vector3[] rocks = { new(-9, 0, 47), new(9.5f, 0, 48), new(-14, 0, 52), new(15, 0, 53), new(0, 0, 56), new(-5, 0, 58), new(6, 0, 60) };
            float[] rockH = { 3.8f, 4.2f, 5f, 4.5f, 6f, 5f, 5.5f };
            for (int i = 0; i < rocks.Length; i++) Model("ENV_Roca", mineSite.transform, rocks[i], 47 * i, rockH[i]);
            // Ladera: la bocamina estaba sola en mitad del llano. Ahora entra en un monte (la mitad trasera queda enterrada).
            var hill = Group("Ladera", mineSite.transform);
            Hill(hill, new Vector3(0, -3f, 59.5f), new Vector3(44, 24, 24));
            Hill(hill, new Vector3(-18f, -4f, 63f), new Vector3(30, 18, 24));
            Hill(hill, new Vector3(18f, -4f, 62f), new Vector3(30, 16, 22));
            var mineLamp = Lamp(mineSite.transform, new Vector3(-6.5f, 0, 40.5f), 160);
            var mineGlow = PointLight(mineSite.transform, "ResplandorMina", new Vector3(0, 1.6f, 45.5f), new Color(0.45f, 1f, 0.55f), 9f, 13f);
            var mineBeam = Beam(mineSite.transform, new Vector3(-4.5f, 0, 41f), 40);
            var mineFog = FogEmitter(mineSite.transform, "NieblaMina", new Vector3(0, 0.5f, 45f), new Vector3(4, 1, 2), 3.5f);
            Effects(mineSite,
                Fx(mineSite, AnomalyKind.Glow, "Resplandor verdoso en la boca de la mina", "Torre... sale una luz verde de la mina. Va y viene, como si respirara.", lights: new[] { mineGlow }, toggles: new[] { mineGlow.gameObject }),
                Fx(mineSite, AnomalyKind.Beam, "Una linterna encendida junto a la mina", "Hay alguien con una linterna en la mina. A estas horas allí no trabaja nadie... ¿verdad?", toggles: new[] { mineBeam.gameObject }, target: mineBeam),
                Fx(mineSite, AnomalyKind.FogBank, "Niebla brotando de la mina", "De la mina sale niebla. Mucha. Y no corre nada de viento.", particles: mineFog),
                Fx(mineSite, AnomalyKind.LightOut, "La farola de la mina se ha apagado", "La farola de la mina parpadea. Se ha quedado a oscuras.", lights: new[] { mineLamp }));
            SecurityCam(mineSite.transform, new Vector3(9f, 5f, 33f), new Vector3(0, 2.2f, 46f));
            siteList.Add(mineSite);

            // ---------------- CASA RURAL (este)
            var ruralSite = Site(root, "CASA RURAL", new Vector3(34, 0, 10));
            Model("ENV_CasaRural01", ruralSite.transform, new Vector3(34, 0, 11), Yaw(new Vector3(34, 0, 11), Vector3.zero), 6.5f);
            var ruralLamp = Lamp(ruralSite.transform, new Vector3(25.5f, 0, 4f), 0);
            var ruralFences = FenceRow(ruralSite.transform, new Vector3(26.5f, 0, 14f), new Vector3(0, 0, 1), 3);
            var porch = PointLight(ruralSite.transform, "LuzPorche", new Vector3(29.6f, 2.6f, 9.4f), new Color(1f, 0.72f, 0.38f), 6f, 7f);
            var ruralFog = FogEmitter(ruralSite.transform, "NieblaPatio", new Vector3(28.5f, 0.4f, 6f), new Vector3(6, 1, 6), 2.5f);
            Effects(ruralSite,
                Fx(ruralSite, AnomalyKind.WindowLight, "Luz encendida en la casa rural vacía", "La casa rural lleva años vacía. ¿Por qué tiene la luz del porche encendida?", lights: new[] { porch }, toggles: new[] { porch.gameObject }),
                Fx(ruralSite, AnomalyKind.LightOut, "La farola de la casa rural se ha apagado", "", lights: new[] { ruralLamp }),
                Fx(ruralSite, AnomalyKind.Displaced, "Valla derribada en la casa rural", "Torre, algo ha hecho ruido junto a la casa rural. Como madera rompiéndose.", target: ruralFences[1], displacedPos: ruralFences[1].localPosition + new Vector3(-0.9f, 0.15f, 0), displacedEuler: new Vector3(0, ruralFences[1].localEulerAngles.y + 12, 78)),
                Fx(ruralSite, AnomalyKind.FogBank, "Niebla acumulándose en el patio de la casa rural", "", particles: ruralFog));
            SecurityCam(ruralSite.transform, new Vector3(20f, 4.6f, 1f), new Vector3(31f, 1.8f, 10f));
            siteList.Add(ruralSite);

            // ---------------- ENTRADA (sur)
            var entrySite = Site(root, "ENTRADA", new Vector3(8, 0, -30));
            var sign = Model("PROP_Senal_Valdara", entrySite.transform, new Vector3(10.5f, 0, -34f), Facing(new Vector3(10.5f, 0, -34f), new Vector3(2.5f, 0, -14.5f)), 2.4f);
            var entryLamp = Lamp(entrySite.transform, new Vector3(9f, 0, -27f), 0);
            var grate = Model("ENV_RejillaAlcantarillado", entrySite.transform, new Vector3(6f, 0.02f, -22f), 0, 0, fitLongest: 0.9f);
            var grateFog = FogEmitter(entrySite.transform, "NieblaAlcantarilla", new Vector3(6f, 0.2f, -22f), new Vector3(0.6f, 0.2f, 0.6f), 2.2f);
            Effects(entrySite,
                Fx(entrySite, AnomalyKind.FogBank, "Niebla saliendo de la alcantarilla", "Sale niebla por la rejilla del camino. Como si debajo hubiera algo caliente... o frío.", particles: grateFog),
                Fx(entrySite, AnomalyKind.Displaced, "El cartel de Valdara está tumbado", "El cartel de la entrada... juraría que antes estaba de pie.", target: sign, displacedPos: sign.localPosition + new Vector3(0.4f, -0.1f, 0.3f), displacedEuler: new Vector3(14, sign.localEulerAngles.y + 20, 24)),
                Fx(entrySite, AnomalyKind.LightOut, "La farola de la entrada se ha apagado", "Se ha ido la luz en la entrada del pueblo. Solo esa farola.", lights: new[] { entryLamp }),
                Fx(entrySite, AnomalyKind.Displaced, "La rejilla de la alcantarilla está fuera de su sitio", "", target: grate, displacedPos: grate.localPosition + new Vector3(0.9f, 0.05f, 0.5f), displacedEuler: new Vector3(0, 37, 0)));
            SecurityCam(entrySite.transform, new Vector3(2.5f, 4.4f, -14.5f), new Vector3(8.5f, 1.2f, -29f));
            siteList.Add(entrySite);

            // ---------------- CASA DE DANIEL (oeste)
            var homeSite = Site(root, "CASA DE DANIEL", new Vector3(-33, 0, -5));
            Model("ENV_CasaDaniel", homeSite.transform, new Vector3(-34, 0, -5), Yaw(new Vector3(-34, 0, -5), Vector3.zero), 6.5f);
            var homeLamp = Lamp(homeSite.transform, new Vector3(-24.5f, 0, 0f), 0);
            var homeFences = FenceRow(homeSite.transform, new Vector3(-26f, 0, -11f), new Vector3(0, 0, 1), 3);
            var homeLight = PointLight(homeSite.transform, "LuzCasaDaniel", new Vector3(-28.8f, 2.4f, -3.6f), new Color(1f, 0.78f, 0.45f), 5.5f, 6.5f);
            var homeBeam = Beam(homeSite.transform, new Vector3(-27.5f, 0, -9.5f), 60);
            Effects(homeSite,
                Fx(homeSite, AnomalyKind.WindowLight, "Luz encendida en casa de Daniel", "Daniel, ¿te has dejado la luz encendida en casa? Juraría que la tenías apagada al subir.", lights: new[] { homeLight }, toggles: new[] { homeLight.gameObject }),
                Fx(homeSite, AnomalyKind.LightOut, "La farola frente a tu casa se ha apagado", "", lights: new[] { homeLamp }),
                Fx(homeSite, AnomalyKind.Beam, "Alguien con linterna rondando tu casa", "Hay una linterna dando vueltas alrededor de tu casa, Daniel.", toggles: new[] { homeBeam.gameObject }, target: homeBeam),
                Fx(homeSite, AnomalyKind.Displaced, "Valla derribada frente a tu casa", "", target: homeFences[1], displacedPos: homeFences[1].localPosition + new Vector3(0.9f, 0.15f, 0), displacedEuler: new Vector3(0, homeFences[1].localEulerAngles.y - 10, -78)));
            SecurityCam(homeSite.transform, new Vector3(-19.5f, 4.6f, 4.5f), new Vector3(-30f, 1.8f, -5f));
            siteList.Add(homeSite);

            // ---------------- Modelos generados (atrezo fijo, sin anomalías asociadas)
            var extra = Group("Atrezo generado", root);
            var boothPos = new Vector3(11.5f, 0, -8.5f);
            Model("ENV_Caseta_Vigilancia", extra, boothPos, Facing(boothPos, new Vector3(6, 0, -8.5f)), 2.6f, collider: true);
            var housePos = new Vector3(17f, 0, -23f);
            Model("ENV_Casa_Rural", extra, housePos, Facing(housePos, new Vector3(6, 0, -23f)), 7f, collider: true);
            Model("ENV_Muro_Rural", extra, new Vector3(12.6f, 0, -29.6f), 0, 1.5f, collider: true);
            Model("ENV_Muro_Rural", extra, new Vector3(12.6f, 0, -33.3f), 180, 1.5f, collider: true);
            Model("PROP_Porton_Metalico", extra, new Vector3(12.6f, 0, -36.7f), 0, 2.4f, collider: true);
            Model("ENV_Muro_Rural", extra, new Vector3(12.6f, 0, -40.1f), 0, 1.5f, collider: true);

            // ---------------- Atrezo de Poly Haven (CC0), fuera de las líneas de visión de las anomalías
            var ph = Group("Atrezo PolyHaven", root);
            Model("utility_box_01", ph, new Vector3(11.7f, 0, -6.75f), 0, 1.15f, collider: true);
            Model("PH_Cubo_Basura", ph, new Vector3(10.7f, 0, -6.7f), 30, 0.95f);
            Model("Barrel_01", ph, new Vector3(13.3f, 0, -7.2f), 15, 0.9f, collider: true);
            Model("wooden_crate_02", ph, new Vector3(12.9f, 0, -10.6f), 70, 0.47f, collider: true);
            Model("covered_car", ph, new Vector3(11.2f, 0, -17.2f), 4, 1.42f, collider: true);
            var tyre = Model("old_tyre", ph, new Vector3(10.4f, 0, -19.9f), 0, 0.6f);
            tyre.SetPositionAndRotation(new Vector3(10.4f, 0.08f, -19.6f), Quaternion.Euler(90, 20, 0));
            Model("plastic_monobloc_chair_01", ph, new Vector3(13.2f, 0, -21.3f), -105, 0.88f);
            Model("plastic_monobloc_chair_01", ph, new Vector3(13.4f, 0, -22.6f), -70, 0.88f);
            Model("small_lpg_tank", ph, new Vector3(13.6f, 0, -25.4f), 0, 0.64f);
            Model("wooden_ladder", ph, new Vector3(14.3f, 0, -19.3f), 80, 1.33f);

            // Farolas del camino (sin anomalía) y vegetación
            var street = Group("Farolas camino", root);
            Lamp(street, new Vector3(9f, 0, -55f), 0); Lamp(street, new Vector3(9f, 0, -8f), 0); Lamp(street, new Vector3(9f, 0, 18f), 0);
            Scatter(root);

            sites = siteList.ToArray();

            // Anillo de bancos de niebla alrededor de la torre: se acerca a medida que sube el nivel.
            bankRing = Group("BancosNiebla", root);
            var list = new List<ParticleSystem>();
            for (int i = 0; i < 10; i++)
            {
                float a = i / 10f * Mathf.PI * 2f;
                var ps = FogEmitter(bankRing, "Banco" + i, new Vector3(Mathf.Cos(a) * 46f, 1.5f, Mathf.Sin(a) * 46f), new Vector3(24, 3, 8), 14f, bank: true);
                ps.transform.localRotation = Quaternion.Euler(0, -a * Mathf.Rad2Deg + 90, 0);
                list.Add(ps);
            }
            banks = list.ToArray();
        }

        const int TreeCount = 46; // árboles secos generados por código (los robles low-poly desentonaban)
        static void Scatter(Transform root)
        {
            var veg = Group("Vegetacion", root);
            var rnd = new System.Random(1998);
            var blocked = new List<(Vector3 p, float r)> { (Vector3.zero, 11f), (new Vector3(0, 0, 48), 14f), (new Vector3(34, 0, 10), 12f), (new Vector3(-34, 0, -5), 12f), (new Vector3(8, 0, -30), 9f),
                (new Vector3(11.5f, 0, -8.5f), 4f), (new Vector3(17, 0, -23), 6f), (new Vector3(12.6f, 0, -35), 8f),
                (new Vector3(11.2f, 0, -17.2f), 3.5f),
                // Monte de la mina: sin árboles ni maleza enterrados a medias en la ladera.
                (new Vector3(0, 0, 60f), 14f), (new Vector3(-18f, 0, 63f), 13f), (new Vector3(18f, 0, 62f), 12f) };
            bool Free(Vector3 p, float r)
            {
                if (Mathf.Abs(p.x - 6) < 5 && p.z < 32) return false; // camino principal
                if (Mathf.Abs(p.z - 6) < 4 && p.x > 6 && p.x < 28) return false;
                if (Mathf.Abs(p.z + 3.5f) < 4 && p.x < 6 && p.x > -27) return false;
                foreach (var b in blocked) if ((b.p - p).sqrMagnitude < (b.r + r) * (b.r + r)) return false;
                return true;
            }
            int trees = 0, bushes = 0, rocks = 0;
            for (int tries = 0; tries < 3000 && (trees < TreeCount || bushes < 20 || rocks < 8); tries++)
            {
                float ang = (float)rnd.NextDouble() * Mathf.PI * 2f, rad = 16f + (float)rnd.NextDouble() * 75f;
                var p = new Vector3(Mathf.Cos(ang) * rad, 0, Mathf.Sin(ang) * rad);
                float yaw = (float)rnd.NextDouble() * 360f;
                if (trees < TreeCount && Free(p, 4f)) { BareTree(veg, p, yaw, 6.5f + (float)rnd.NextDouble() * 5f, trees, rad < 50); blocked.Add((p, 4f)); trees++; }
                else if (bushes < 20 && Free(p, 1.5f)) { bushes = 20; }
                else if (rocks < 8 && Free(p, 2f)) { Model("ENV_Roca", veg, p, yaw, 0.8f + (float)rnd.NextDouble() * 1.4f); blocked.Add((p, 2f)); rocks++; }
            }
            // Maleza realista (Poly Haven, CC0): cerca del pueblo, que es donde alcanzan las cámaras y las ventanas.
            (string prefix, int count, float hMin, float hMax, bool longest, float radius)[] kinds =
            {
                ("PH_Arbusto_", 28, 1.1f, 1.9f, false, 1.6f), ("PH_Mata_", 70, 0.45f, 0.95f, false, 0.6f), ("PH_Hierba_", 70, 0.5f, 0.9f, true, 0.5f),
                ("PH_Ortiga_", 22, 0.6f, 1.1f, false, 0.6f), ("PH_Rama_", 14, 1.1f, 1.8f, true, 1f), ("tree_stump_01", 6, 0.45f, 0.7f, false, 1.4f), ("PH_RocaMusgo_", 10, 0.5f, 1.3f, false, 1.8f),
            };
            string[] abc = { "A", "B", "C" };
            int placed = 0;
            foreach (var k in kinds)
            {
                int variants = k.prefix.EndsWith("_") ? (k.prefix == "PH_Arbusto_" || k.prefix == "PH_RocaMusgo_" ? 3 : 2) : 0;
                for (int n = 0, tries = 0; n < k.count && tries < 4000; tries++)
                {
                    float ang = (float)rnd.NextDouble() * Mathf.PI * 2f, rad = 12.5f + (float)rnd.NextDouble() * 52f;
                    var p = new Vector3(Mathf.Cos(ang) * rad, 0, Mathf.Sin(ang) * rad);
                    if (!Free(p, k.radius)) continue;
                    float size = Mathf.Lerp(k.hMin, k.hMax, (float)rnd.NextDouble());
                    string name = variants > 0 ? k.prefix + abc[rnd.Next(variants)] : k.prefix;
                    Model(name, veg, p, (float)rnd.NextDouble() * 360f, k.longest ? 0 : size, fitLongest: k.longest ? size : 0, shadows: false);
                    blocked.Add((p, k.radius)); n++; placed++;
                }
            }
            // Cunetas: maleza apretada a los lados de los caminos, que es lo que más se ve desde la torre y las cámaras.
            (Vector3 a, Vector3 b, float w)[] roadsides = { (new Vector3(6, 0, -75), new Vector3(6, 0, 30), 4.2f), (new Vector3(6, 0, 30), new Vector3(0, 0, 41), 3.4f), (new Vector3(8, 0, 4.4f), new Vector3(27, 0, 8), 2.8f), (new Vector3(3, 0, -3.1f), new Vector3(-26, 0, -4), 2.8f) };
            string[] verge = { "PH_Mata_A", "PH_Mata_B", "PH_Hierba_A", "PH_Hierba_B", "PH_Mata_A", "PH_Ortiga_A", "PH_Ortiga_B", "PH_Arbusto_B", "PH_Mata_B", "PH_Hierba_A" };
            int vergeCount = 0;
            foreach (var rd in roadsides)
            {
                var dir = (rd.b - rd.a).normalized; var side = Vector3.Cross(Vector3.up, dir); float len = (rd.b - rd.a).magnitude;
                for (float d = 0; d < len; d += 0.55f + (float)rnd.NextDouble() * 0.9f)
                {
                    float off = rd.w / 2f + 0.35f + Mathf.Pow((float)rnd.NextDouble(), 1.7f) * 4.5f;
                    var p = rd.a + dir * d + side * (rnd.Next(2) == 0 ? off : -off);
                    bool clear = true; foreach (var b in blocked) if (b.r > 2.5f && b.p != Vector3.zero && (b.p - p).sqrMagnitude < (b.r * 0.55f) * (b.r * 0.55f)) clear = false;
                    if (p.sqrMagnitude < 5.2f * 5.2f) clear = false; // pies de la torre
                    foreach (var other in roadsides) { var od = (other.b - other.a).normalized; float t = Mathf.Clamp(Vector3.Dot(p - other.a, od), 0, (other.b - other.a).magnitude); if ((other.a + od * t - p).magnitude < other.w / 2f + 0.3f) clear = false; }
                    if (!clear) continue;
                    string name = verge[rnd.Next(verge.Length)];
                    bool herb = name.StartsWith("PH_Hierba"), bush = name.StartsWith("PH_Arbusto");
                    float size = bush ? 1.0f + (float)rnd.NextDouble() * 0.7f : herb ? 0.5f + (float)rnd.NextDouble() * 0.5f : 0.4f + (float)rnd.NextDouble() * 0.6f;
                    Model(name, veg, p, (float)rnd.NextDouble() * 360f, herb ? 0 : size, fitLongest: herb ? size : 0, shadows: false);
                    vergeCount++;
                }
            }
            log.Add($"Vegetación: {trees} robles, {rocks} rocas, {placed} piezas de maleza dispersa, {vergeCount} en cunetas");
        }

        // ==================================================================== torre
        static void BuildTowerStructure(Transform tower)
        {
            var s = Group("Estructura", tower);
            float leg = 3.3f;
            foreach (var x in new[] { -leg, leg }) foreach (var z in new[] { -leg, leg })
                    Box(s, "Pilar", new Vector3(x, Floor / 2f, z), new Vector3(0.38f, Floor, 0.38f), "MaderaOscura");
            // Tirantes en cruz en cada cara
            for (int side = 0; side < 4; side++)
            {
                var rot = Quaternion.Euler(0, side * 90, 0);
                for (int level = 0; level < 2; level++)
                {
                    float y0 = 0.4f + level * 3.3f;
                    Vector3 a = rot * new Vector3(-leg, y0, leg), b = rot * new Vector3(leg, y0 + 3.2f, leg);
                    Strut(s, a, b, 0.16f); Strut(s, rot * new Vector3(leg, y0, leg), rot * new Vector3(-leg, y0 + 3.2f, leg), 0.16f);
                    Box(s, "Travesaño", rot * new Vector3(0, y0 + 3.2f, leg), new Vector3(leg * 2, 0.18f, 0.18f), "MaderaOscura").transform.rotation = rot;
                }
            }
            // Pasarela exterior con barandilla de vallas rurales
            Box(s, "Pasarela", new Vector3(0, Floor - 0.12f, 0), new Vector3(8.2f, 0.24f, 8.2f), "Madera");
            for (int side = 0; side < 4; side++)
            {
                var rot = Quaternion.Euler(0, side * 90, 0);
                var row = FenceRow(s, rot * new Vector3(-3.4f, Floor, 4.0f), rot * Vector3.right, 3, 1.05f, gap: 0.05f);
            }
            // Escalera exterior por el lado este (decorado visible desde las ventanas)
            var stairs = Group("Escalera", s);
            for (int i = 0; i < 24; i++)
                Box(stairs, "Peldaño", new Vector3(4.75f, Floor - 0.28f * (i + 1), 3.6f - i * 0.28f), new Vector3(1.0f, 0.06f, 0.3f), "MaderaOscura");
            Strut(stairs, new Vector3(4.22f, Floor - 0.1f, 3.8f), new Vector3(4.22f, 0f, -3.1f), 0.1f);
            Strut(stairs, new Vector3(5.28f, Floor - 0.1f, 3.8f), new Vector3(5.28f, 0f, -3.1f), 0.1f);
            // Tejado a dos aguas
            var roof = Group("Tejado", s);
            Box(roof, "Alero", new Vector3(0, Floor + CabinHeight + 0.08f, 0), new Vector3(6.6f, 0.16f, 6.6f), "MaderaOscura");
            var r1 = Box(roof, "FaldonN", new Vector3(0, Floor + CabinHeight + 0.75f, 1.65f), new Vector3(6.8f, 0.12f, 3.7f), "Tejado"); r1.transform.rotation = Quaternion.Euler(-21, 0, 0);
            var r2 = Box(roof, "FaldonS", new Vector3(0, Floor + CabinHeight + 0.75f, -1.65f), new Vector3(6.8f, 0.12f, 3.7f), "Tejado"); r2.transform.rotation = Quaternion.Euler(21, 0, 0);
            foreach (var t in s.GetComponentsInChildren<Transform>()) t.gameObject.isStatic = true;
        }

        class CabinRefs { public TowerPower power; public SecurityConsole console; public Transform radio; }

        static CabinRefs BuildCabin(Transform cabin, RenderTexture feed)
        {
            var refs = new CabinRefs();
            var shell = Group("Arquitectura", cabin);
            float t = 0.12f, w = Half + t / 2f, sill = 1.05f, head = 2.35f;
            Box(shell, "Suelo", new Vector3(0, Floor - 0.05f, 0), new Vector3(Half * 2 + 0.3f, 0.1f, Half * 2 + 0.3f), "Tarima");
            Box(shell, "Techo", new Vector3(0, Floor + CabinHeight, 0), new Vector3(Half * 2 + 0.3f, 0.1f, Half * 2 + 0.3f), "Pared");

            // Paredes norte/este/oeste con ventanal corrido; la sur incluye la puerta.
            for (int side = 0; side < 4; side++)
            {
                var rot = Quaternion.Euler(0, side * 90, 0);
                Vector3 P(float x, float y) => rot * new Vector3(x, y, w);
                Vector3 S(float sx, float sy) { var v = new Vector3(sx, sy, t); return side % 2 == 0 ? v : new Vector3(t, sy, sx); }
                bool south = side == 2;
                if (!south)
                {
                    Box(shell, "Antepecho", P(0, Floor + sill / 2), S(Half * 2 + t * 2, sill), "Pared");
                    Box(shell, "Dintel", P(0, Floor + (head + CabinHeight) / 2), S(Half * 2 + t * 2, CabinHeight - head), "Pared");
                    Glass(shell, P(0, Floor + (sill + head) / 2), S(Half * 2, head - sill));
                    for (int m = 0; m <= 4; m++) Box(shell, "Montante", P(-Half + m * Half / 2f, Floor + (sill + head) / 2), S(0.09f, head - sill), "MaderaOscura");
                    Box(shell, "Alfeizar", P(0, Floor + sill), S(Half * 2, 0.05f) + (side % 2 == 0 ? new Vector3(0, 0, 0.22f) : new Vector3(0.22f, 0, 0)), "MaderaOscura");
                }
                else
                {
                    // Sur (z negativa): ventana | muro con panel eléctrico | puerta | ventana
                    float doorL = 0.55f, doorR = 1.55f, wallL = -0.25f;
                    Section(shell, -w, t, -Half - t, wallL, sill, head);
                    Box(shell, "MuroPanel", new Vector3((wallL + doorL) / 2, Floor + CabinHeight / 2, -w), new Vector3(doorL - wallL, CabinHeight, t), "Pared");
                    Section(shell, -w, t, doorR, Half + t, sill, head);
                    Box(shell, "SobrePuerta", new Vector3((doorL + doorR) / 2, Floor + (2.1f + CabinHeight) / 2, -w), new Vector3(doorR - doorL, CabinHeight - 2.1f, t), "Pared");
                    var door = Box(cabin, "Puerta", new Vector3((doorL + doorR) / 2, Floor + 1.05f, -w), new Vector3(doorR - doorL - 0.04f, 2.1f, 0.08f), "MaderaOscura");
                    door.AddComponent<TowerDoor>();
                    var knob = Prim(PrimitiveType.Sphere, cabin, "Pomo", door.transform.position + new Vector3(-0.36f, 0, 0.07f), new Vector3(0.065f, 0.065f, 0.065f), "Metal");
                    UnityEngine.Object.DestroyImmediate(knob.GetComponent<Collider>());
                    // Marco de la puerta: tapa el canto de la hoja y la junta con la pared.
                    float jambDepth = t + 0.05f;
                    Box(shell, "MarcoPuertaIzq", new Vector3(doorL + 0.035f, Floor + 1.05f, -w), new Vector3(0.07f, 2.1f, jambDepth), "MaderaOscura");
                    Box(shell, "MarcoPuertaDer", new Vector3(doorR - 0.035f, Floor + 1.05f, -w), new Vector3(0.07f, 2.1f, jambDepth), "MaderaOscura");
                    Box(shell, "MarcoPuertaSup", new Vector3((doorL + doorR) / 2, Floor + 2.1f - 0.035f, -w), new Vector3(doorR - doorL, 0.07f, jambDepth), "MaderaOscura");
                    var circ = Paper(cabin, "Circular", door.transform.position + new Vector3(0.08f, 0.45f, 0.05f), Quaternion.Euler(90, 0, 0), new Vector2(0.24f, 0.32f));
                    var note = circ.AddComponent<ReadableNote>(); note.title = "Comunicado interno";
                    note.body = "Por orden de la dirección de operaciones, queda restringido el acceso a la zona de extracción entre las 20:00 y las 07:00.\n\nCualquier incidencia observada en la entrada de la mina se comunicará exclusivamente a la empresa.\n\nNo se facilitará información a vecinos ni a terceros.\n\n— Dirección de operaciones";
                }
            }

            // ------------------------------------------------ muebles (modelos del equipo)
            var furniture = Group("Muebles", cabin);
            var desk = Model("INT_MesaMadera", furniture, new Vector3(0, Floor, 2.05f), 0, 0.8f, collider: true, fitWidthX: 2.0f);
            var deskTop = Floor + Bounds(desk.gameObject).size.y; deskTopForCapture = deskTop;
            Model("INT_SillaMadera", furniture, new Vector3(0.05f, Floor, 1.15f), 0, 0.98f, collider: true);
            var shelf = Model("INT_Estanteria", furniture, new Vector3(2.38f, Floor, 0.35f), -90, 1.95f, collider: true);
            var wardrobe = Model("INT_Armario-ARMARIO DE MADERA RURAL (armario cerrado)", furniture, new Vector3(-2.2f, Floor, -2.15f), 90, 2.0f, collider: true);
            var bed = Model("INT_Cama", furniture, new Vector3(-1.85f, Floor, 0.1f), 0, 0.62f, collider: true, fitDepthZ: 1.95f);
            var night = Model("INT_MesitaNoche", furniture, new Vector3(-2.3f, Floor, 1.65f), 90, 0.6f, collider: true);
            var fridge = Model("INR_Nevera", furniture, new Vector3(2.25f, Floor, -2.15f), -90, 1.7f, collider: true);

            Search(shelf, "Registrar la estantería", 1, "Detrás de unos archivadores hay un paquete de pilas.", "");
            Search(wardrobe, "Registrar el armario", 1, "En el bolsillo de un chubasquero: pilas nuevas.", "");
            var nightSpot = Search(night, "Abrir la mesita", 0, "", "Solo hay una libreta.");
            nightSpot.noteTitle = "Libreta de Daniel";
            nightSpot.noteBody = "Tercera noche seguida que la niebla llega antes de lo que dice el parte.\n\nNadie en el pueblo quiere hablar de la mina. Cuando pregunto, cambian de tema.\n\nSi esta noche pasa algo, quiero tenerlo todo apuntado. Todo.";
            Search(fridge, "Abrir la nevera", 0, "", "Un termo de café frío y medio bocadillo. Nada que sirva contra la niebla.");
            Search(bed, "Mirar bajo el catre", 1, "Bajo el catre, una caja con pilas de repuesto.", "");

            // ------------------------------------------------ consola
            var station = Group("Consola", cabin);
            // Monitor CRT real (modelo generado). La caja "Monitor" queda invisible: solo es el volumen de interacción.
            var crt = Model("PROP_Monitor_CRT", station, new Vector3(0, deskTop, 2.4f), PropFrontYaw, 0.42f);
            var cb = Bounds(crt.gameObject); crt.position += new Vector3(0, 0, MonitorFrontZ - cb.min.z); cb = Bounds(crt.gameObject);
            var monitorPos = new Vector3(cb.center.x, cb.min.y + cb.size.y * 0.57f, cb.min.z);
            var monitor = Box(station, "Monitor", cb.center, cb.size, "Carcasa");
            monitor.GetComponent<Renderer>().enabled = false;
            var screen = GameObject.CreatePrimitive(PrimitiveType.Quad); screen.name = "Pantalla"; screen.transform.SetParent(station);
            screen.transform.position = monitorPos + new Vector3(0, 0, ScreenInset); screen.transform.localScale = new Vector3(cb.size.x * 0.72f, cb.size.y * 0.60f, 1);
            UnityEngine.Object.DestroyImmediate(screen.GetComponent<Collider>());
            screen.GetComponent<Renderer>().sharedMaterial = mats["Pantalla"];
            var console = monitor.AddComponent<SecurityConsole>(); console.screen = screen.GetComponent<Renderer>(); console.feedTexture = feed;
            var seat = new GameObject("VistaConsola"); seat.transform.SetParent(station);
            seat.transform.position = new Vector3(0, deskTop + 0.42f, 1.52f); seat.transform.LookAt(monitorPos);
            console.seatView = seat.transform;
            refs.console = console;
            Box(station, "Teclado", new Vector3(0, deskTop + 0.015f, 1.98f), new Vector3(0.44f, 0.03f, 0.15f), "Carcasa");
            // Radio real (modelo generado), con caja invisible para la interacción.
            var radioModel = Model("PROP_Radio_Analogica", station, new Vector3(0.72f, deskTop, 2.22f), PropFrontYaw + 18f, 0.3f);
            var rb = Bounds(radioModel.gameObject);
            var radio = Box(station, "Radio", rb.center, rb.size, "Carcasa");
            radio.GetComponent<Renderer>().enabled = false;
            radio.AddComponent<RadioSet>(); refs.radio = radio.transform;
            // Radar de mano: de momento solo atrezo sobre la mesa, sin mecánica.
            Model("PROP_Radar_Parasitos", station, new Vector3(-0.78f, deskTop, 2.3f), PropFrontYaw - 22f, 0.26f, collider: true);

            var torch = Model("OBJ_Linterna", station, new Vector3(-0.6f, deskTop + 0.035f, 2.05f), 70, 0, fitLongest: 0.27f, collider: true);
            torch.gameObject.AddComponent<FlashlightPickup>();
            var log1 = Paper(station, "ParteTurno", new Vector3(-0.28f, deskTop + 0.004f, 1.95f), Quaternion.Euler(0, 8, 0), new Vector2(0.21f, 0.29f));
            var n1 = log1.AddComponent<ReadableNote>(); n1.title = "Parte del turno anterior";
            n1.body = "21:40 — Cámara 3 con interferencias otra vez.\n21:52 — La niebla ha llegado a la entrada del pueblo antes de las diez.\n\nRecordatorio:\n· Cualquier cambio en las cámaras se registra en el parte desde la consola.\n· No registres por registrar: las falsas alarmas también cuentan.\n· Si salta el diferencial, el panel está junto a la puerta.\n· No se baja de la torre hasta que amanezca.\n\n— El relevo de tarde";
            var map = Paper(station, "PlanoCamaras", new Vector3(0.4f, deskTop + 0.004f, 1.93f), Quaternion.Euler(0, -12, 0), new Vector2(0.29f, 0.21f));
            var n2 = map.AddComponent<ReadableNote>(); n2.title = "Plano de cámaras";
            n2.body = "CAM 1 · MINA — al norte, delante de esta mesa.\nCAM 2 · CASA RURAL — al este.\nCAM 3 · ENTRADA — al sur: cartel, farola y alcantarilla.\nCAM 4 · CASA DE DANIEL — al oeste.\n\nAprende cómo se ve cada cámara cuando todo está en calma.\nLo que cambie, se registra.";

            // ------------------------------------------------ atrezo de Poly Haven (CC0)
            var dressing = Group("Atrezo PolyHaven", cabin);
            Model("desk_lamp_arm_01", dressing, new Vector3(0.9f, deskTop, 2.45f), 200, 0.5f);
            Model("clipboard", dressing, new Vector3(0.78f, deskTop, 1.82f), 24, 0, fitLongest: 0.33f);
            Model("binoculars", dressing, new Vector3(-0.42f, deskTop, 2.36f), 35, 0, fitLongest: 0.22f);
            Model("retro_multimeter", dressing, new Vector3(-2.3f, Bounds(night.gameObject).max.y, 1.65f), 100, 0.2f);
            Model("PH_Caja_Herramientas", dressing, new Vector3(-0.02f, Floor, -Half + 0.2f), 8, 0, fitWidthX: 0.42f);
            Model("modified_thermos", dressing, new Vector3(-0.93f, deskTop, 1.84f), 40, 0.29f);
            Model("cigarette_pack", dressing, new Vector3(0.37f, deskTop, 2.17f), 28, 0.095f);
            Model("can_rusted", dressing, new Vector3(0.5f, deskTop, 2.46f), 0, 0.12f);
            Model("PH_Archivador", dressing, new Vector3(-1.75f, Bounds(bed.gameObject).max.y, -0.45f), 25, 0, fitLongest: 0.3f);
            Model("vintage_electric_kettle", dressing, new Vector3(2.25f, Bounds(fridge.gameObject).max.y, -2.15f), -60, 0.26f);
            Model("PH_Botas", dressing, new Vector3(1.9f, Floor, -2.42f), 165, 0.38f);
            Model("wall_clock", dressing, new Vector3(0, Floor + 2.44f, Half - 0.03f), 180, 0.32f);

            // ------------------------------------------------ electricidad
            var elec = Group("Electricidad", cabin);
            var lamp1 = CeilingLamp(elec, new Vector3(-0.9f, Floor + CabinHeight - 0.35f, 0.6f));
            var lamp2 = CeilingLamp(elec, new Vector3(1.0f, Floor + CabinHeight - 0.35f, -0.9f));
            var deskLight = PointLight(elec, "LuzMonitor", monitorPos + new Vector3(0, 0.05f, -0.4f), new Color(0.6f, 0.85f, 0.75f), 0.6f, 2.2f);
            var panelGo = Box(elec, "PanelElectrico", new Vector3(0.15f, Floor + 1.45f, -Half + 0.07f), new Vector3(0.46f, 0.6f, 0.12f), "Metal");
            // Cuadro eléctrico real (Poly Haven). El cubo queda invisible como volumen de interacción; palanca y piloto siguen visibles.
            panelGo.GetComponent<Renderer>().enabled = false;
            var powerBox = Model("power_box_01", elec, new Vector3(0.15f, Floor + 1.13f, -Half + 0.055f), 0, 0.64f);
            // Palanca y piloto colgaban del cubo escalado (0,46 × 0,6 × 0,12): salían deformados y al girar se cizallaban.
            // Ahora van en un grupo sin escala, pegados al frontal real del cuadro.
            var pb = Bounds(powerBox.gameObject); float pbFront = pb.max.z;
            var controls = Group("MandosPanel", elec);
            var lever = new GameObject("Palanca").transform; lever.SetParent(controls, false); lever.position = new Vector3(pb.center.x + 0.06f, pb.center.y - 0.02f, pbFront + 0.03f);
            var leverBase = Box(controls, "BasePalanca", lever.position, new Vector3(0.08f, 0.06f, 0.03f), "Carcasa");
            UnityEngine.Object.DestroyImmediate(leverBase.GetComponent<Collider>());
            var handle = Box(lever, "Mango", lever.position + new Vector3(0, 0.07f, 0), new Vector3(0.03f, 0.14f, 0.03f), "Ambar");
            UnityEngine.Object.DestroyImmediate(handle.GetComponent<Collider>());
            var lampStatus = Box(controls, "Piloto", new Vector3(pb.center.x - 0.14f, pb.center.y + 0.2f, pbFront + 0.01f), new Vector3(0.035f, 0.035f, 0.02f), "Piloto");
            UnityEngine.Object.DestroyImmediate(lampStatus.GetComponent<Collider>());
            var breaker = panelGo.AddComponent<BreakerPanel>(); breaker.lever = lever; breaker.statusLamp = lampStatus.GetComponent<Renderer>();
            // Arriba = con corriente, abajo = saltado. Ambos ángulos sacan el mango hacia la sala (antes se metía dentro del cuadro).
            breaker.leverOnEuler = new Vector3(20, 0, 0); breaker.leverOffEuler = new Vector3(160, 0, 0); lever.localRotation = Quaternion.Euler(breaker.leverOnEuler);
            var emergency = PointLight(elec, "LuzEmergencia", new Vector3(1.05f, Floor + 2.45f, -Half + 0.2f), new Color(1f, 0.12f, 0.06f), 2f, 6f);
            emergency.enabled = false; // solo se enciende con el apagón (TowerPower)
            var emergencyLamp = Box(elec, "PilotoEmergencia", new Vector3(1.05f, Floor + 2.6f, -Half + 0.08f), new Vector3(0.18f, 0.08f, 0.08f), "PilotoApagado");
            var power = elec.gameObject.AddComponent<TowerPower>();
            var flexoGo = new GameObject("LuzFlexo"); flexoGo.transform.SetParent(elec); flexoGo.transform.position = new Vector3(0.78f, deskTopForCapture + 0.48f, 2.34f);
            flexoGo.transform.LookAt(new Vector3(0.25f, deskTopForCapture, 1.95f));
            var flexo = flexoGo.AddComponent<Light>(); flexo.type = LightType.Spot; flexo.color = new Color(1f, 0.70f, 0.40f); flexo.intensity = 9f; flexo.range = 3.2f; flexo.spotAngle = 82f; flexo.innerSpotAngle = 40f; flexo.shadows = LightShadows.Soft;
            power.interiorLights = new[] { lamp1.light, lamp2.light, deskLight, flexo }; power.emissiveRenderers = new[] { lamp1.bulb, lamp2.bulb };
            power.emergencyLight = emergency; power.panel = panelGo.transform;
            power.bulbOffMaterial = mats["BombillaApagada"];
            power.emergencyLamp = emergencyLamp.GetComponent<Renderer>(); power.emergencyOnMaterial = mats["Piloto"]; power.emergencyOffMaterial = mats["PilotoApagado"];
            refs.power = power;
            return refs;
        }

        static void Section(Transform shell, float w, float t, float x0, float x1, float sill, float head)
        {
            float cx = (x0 + x1) / 2, len = x1 - x0;
            Vector3 P(float x, float y) => new Vector3(x, y, w);
            Box(shell, "Antepecho", P(cx, Floor + sill / 2), new Vector3(len, sill, t), "Pared");
            Box(shell, "Dintel", P(cx, Floor + (head + CabinHeight) / 2), new Vector3(len, CabinHeight - head, t), "Pared");
            Glass(shell, P(cx, Floor + (sill + head) / 2), new Vector3(len, head - sill, t));
            Box(shell, "Montante", P(x0 + 0.05f, Floor + (sill + head) / 2), new Vector3(0.09f, head - sill, t), "MaderaOscura");
            Box(shell, "Montante", P(x1 - 0.05f, Floor + (sill + head) / 2), new Vector3(0.09f, head - sill, t), "MaderaOscura");
        }

        static (Light light, Renderer bulb) CeilingLamp(Transform parent, Vector3 pos)
        {
            // Antes eran cubos (pantalla cuadrada de metal que reflejaba el rojo de emergencia). Ahora: florón, cable, pantalla
            // esmaltada redonda y bombilla esférica.
            float ceiling = Floor + CabinHeight - 0.05f, shadeTop = pos.y + 0.1f, cableLen = Mathf.Max(0.02f, ceiling - shadeTop);
            var parts = new List<GameObject>
            {
                Prim(PrimitiveType.Cylinder, parent, "Floron", new Vector3(pos.x, ceiling - 0.015f, pos.z), new Vector3(0.11f, 0.015f, 0.11f), "Esmalte"),
                Prim(PrimitiveType.Cylinder, parent, "Cable", new Vector3(pos.x, shadeTop + cableLen / 2f, pos.z), new Vector3(0.012f, cableLen / 2f, 0.012f), "Carcasa"),
                Prim(PrimitiveType.Cylinder, parent, "Casquillo", pos + new Vector3(0, 0.11f, 0), new Vector3(0.09f, 0.025f, 0.09f), "Esmalte"),
                Prim(PrimitiveType.Cylinder, parent, "Pantalla", pos + new Vector3(0, 0.05f, 0), new Vector3(0.34f, 0.04f, 0.34f), "Esmalte"),
            };
            var bulb = Prim(PrimitiveType.Sphere, parent, "Bombilla", pos - new Vector3(0, 0.02f, 0), new Vector3(0.1f, 0.1f, 0.1f), "Bombilla");
            parts.Add(bulb);
            foreach (var p in parts) UnityEngine.Object.DestroyImmediate(p.GetComponent<Collider>());
            bulb.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            var l = PointLight(parent, "Luz", pos - new Vector3(0, 0.12f, 0), new Color(1f, 0.80f, 0.58f), 1.5f, 6f);
            l.shadows = LightShadows.Soft;
            return (l, bulb.GetComponent<Renderer>());
        }

        // ==================================================================== jugador
        static PlayerMotor BuildPlayer(out Camera cam, out PlayerFlashlight flashlight)
        {
            var player = new GameObject("Daniel"); player.layer = 2; player.tag = "Player";
            player.transform.position = new Vector3(0.4f, Floor + 0.02f, -0.9f);
            var cc = player.AddComponent<CharacterController>(); cc.height = 1.75f; cc.radius = 0.28f; cc.center = new Vector3(0, 0.875f, 0); cc.stepOffset = 0.25f; cc.skinWidth = 0.03f;
            var head = new GameObject("Cabeza").transform; head.SetParent(player.transform, false); head.localPosition = new Vector3(0, 1.63f, 0);
            var camGo = new GameObject("Camara"); camGo.tag = "MainCamera"; camGo.layer = 2; camGo.transform.SetParent(head, false);
            cam = camGo.AddComponent<Camera>(); cam.nearClipPlane = 0.04f; cam.farClipPlane = 230f; cam.fieldOfView = 68f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = RenderSettings.fogColor;
            var data = cam.GetUniversalAdditionalCameraData(); data.renderPostProcessing = true; data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            camGo.AddComponent<AudioListener>();
            var interactor = camGo.AddComponent<Interactor>(); interactor.cam = cam;

            var spotGo = new GameObject("HazLinterna"); spotGo.transform.SetParent(camGo.transform, false); spotGo.transform.localPosition = new Vector3(0.22f, -0.2f, 0.25f);
            var spot = spotGo.AddComponent<Light>(); spot.type = LightType.Spot; spot.range = 22f; spot.spotAngle = 52f; spot.innerSpotAngle = 22f; spot.intensity = 9f; spot.color = new Color(1f, 0.93f, 0.8f); spot.shadows = LightShadows.Soft;
            var view = Model("OBJ_Linterna", camGo.transform, Vector3.zero, 0, 0, fitLongest: 0.24f);
            view.SetParent(camGo.transform, false); view.localPosition = new Vector3(0.24f, -0.24f, 0.38f); view.localRotation = Quaternion.Euler(0, -90, 0);
            foreach (var r in view.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
            foreach (var c in view.GetComponentsInChildren<Collider>()) UnityEngine.Object.DestroyImmediate(c);
            flashlight = player.AddComponent<PlayerFlashlight>(); flashlight.spot = spot; flashlight.viewModel = view.gameObject;

            var motor = player.AddComponent<PlayerMotor>(); motor.head = head; motor.cam = cam;
            return motor;
        }

        // ==================================================================== piezas
        static Transform Group(string name, Transform parent = null) { var g = new GameObject(name).transform; if (parent) g.SetParent(parent, false); return g; }

        static AnomalySite Site(Transform root, string label, Vector3 pos)
        {
            var go = new GameObject("Lugar · " + label); go.transform.SetParent(root); go.transform.position = Vector3.zero;
            var s = go.AddComponent<AnomalySite>(); s.label = label; return s;
        }

        static AnomalyEffect Fx(AnomalySite site, AnomalyKind kind, string desc, string hint, Light[] lights = null, GameObject[] toggles = null, ParticleSystem particles = null, Transform target = null, Vector3 displacedPos = default, Vector3 displacedEuler = default)
        {
            var go = new GameObject("Anomalia · " + desc); go.transform.SetParent(site.transform);
            var e = go.AddComponent<AnomalyEffect>(); e.kind = kind; e.description = desc; e.radioHint = hint;
            e.lights = lights ?? new Light[0]; e.toggles = toggles ?? new GameObject[0]; e.particles = particles; e.target = target;
            e.displacedLocalPosition = displacedPos; e.displacedLocalEuler = displacedEuler;
            return e;
        }

        static void Effects(AnomalySite site, params AnomalyEffect[] fx) => site.effects = fx;

        const float CamModelYaw = 0f; // giro del modelo de cámara para que el objetivo mire al lugar vigilado
        static void SecurityCam(Transform parent, Vector3 pos, Vector3 lookAt)
        {
            var pole = Box(parent, "PosteCamara", new Vector3(pos.x, pos.y / 2f, pos.z), new Vector3(0.14f, pos.y, 0.14f), "MaderaOscura");
            var body = Model("security_camera_01", parent, pos + new Vector3(0, 0.02f, 0), 0, 0, fitLongest: 0.6f);
            body.rotation = Quaternion.LookRotation(lookAt - pos) * Quaternion.Euler(0, CamModelYaw, 0);
            var go = new GameObject("CamaraVigilancia"); go.transform.SetParent(parent); go.transform.position = pos + (lookAt - pos).normalized * 0.4f; go.transform.LookAt(lookAt);
            var c = go.AddComponent<Camera>(); c.fieldOfView = 62; c.nearClipPlane = 0.2f; c.farClipPlane = 140; c.clearFlags = CameraClearFlags.SolidColor; c.backgroundColor = RenderSettings.fogColor; c.enabled = false;
            var d = c.GetUniversalAdditionalCameraData(); d.renderPostProcessing = false; d.renderShadows = true;
        }

        static Light Lamp(Transform parent, Vector3 pos, float yaw)
        {
            var lamp = Model("ENV_Farola", parent, pos, yaw, 4.3f, collider: false);
            var b = Bounds(lamp.gameObject);
            return PointLight(lamp, "LuzFarola", new Vector3(b.center.x, b.max.y - 0.35f, b.center.z), new Color(1f, 0.72f, 0.42f), 7f, 15f);
        }

        static Transform Beam(Transform parent, Vector3 pos, float yaw)
        {
            var pivot = new GameObject("Linterna ajena").transform; pivot.SetParent(parent); pivot.position = pos + Vector3.up * 1.15f; pivot.rotation = Quaternion.Euler(8, yaw, 0);
            var m = Model("OBJ_Linterna", pivot, Vector3.zero, 0, 0, fitLongest: 0.3f);
            m.SetParent(pivot, false); m.localPosition = Vector3.zero; m.localRotation = Quaternion.Euler(0, -90, 0);
            var l = new GameObject("Haz").AddComponent<Light>(); l.transform.SetParent(pivot, false); l.transform.localPosition = new Vector3(0, 0, 0.2f);
            l.type = LightType.Spot; l.range = 18f; l.spotAngle = 38f; l.innerSpotAngle = 14f; l.intensity = 14f; l.color = new Color(1f, 0.95f, 0.85f);
            var glow = PointLight(pivot, "Brillo", pivot.position + pivot.forward * 0.3f, new Color(1f, 0.95f, 0.85f), 1.2f, 2.5f);
            return pivot;
        }

        static Light PointLight(Transform parent, string name, Vector3 pos, Color color, float intensity, float range)
        {
            var go = new GameObject(name); go.transform.SetParent(parent); go.transform.position = pos;
            var l = go.AddComponent<Light>(); l.type = LightType.Point; l.color = color; l.intensity = intensity; l.range = range; l.shadows = LightShadows.None;
            return l;
        }

        static ParticleSystem FogEmitter(Transform parent, string name, Vector3 pos, Vector3 box, float size, bool bank = false)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = pos;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main; main.loop = true; main.playOnAwake = bank; main.startLifetime = bank ? 26f : 9f; main.startSpeed = bank ? 0.25f : 0.7f;
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.7f, size * 1.3f); main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            main.startColor = new Color(0.55f, 0.6f, 0.57f, bank ? 0.45f : 0.6f); main.maxParticles = bank ? 40 : 90; main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy; main.prewarm = bank; main.gravityModifier = bank ? 0 : -0.01f;
            var em = ps.emission; em.rateOverTime = bank ? 1.6f : 9f;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = box;
            var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f); vel.y = new ParticleSystem.MinMaxCurve(bank ? -0.02f : 0.05f, bank ? 0.04f : 0.35f); vel.z = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
            var col = ps.colorOverLifetime; col.enabled = true;
            var grad = new Gradient(); grad.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.25f), new GradientAlphaKey(1, 0.7f), new GradientAlphaKey(0, 1) });
            col.color = grad;
            var rot = ps.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = mats["Niebla"]; r.renderMode = ParticleSystemRenderMode.Billboard; r.sortMode = ParticleSystemSortMode.Distance;
            r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false; r.maxParticleSize = 2f;
            return ps;
        }

        static readonly List<Vector3> roadCaps = new List<Vector3>();

        static void Road(Transform parent, Vector3 a, Vector3 b, float width, float top = 0.03f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = "Camino"; go.transform.SetParent(parent);
            go.transform.position = (a + b) / 2 + Vector3.up * (top / 2f); go.transform.rotation = Quaternion.LookRotation(b - a);
            go.transform.localScale = new Vector3(width, top, (b - a).magnitude + width * 0.5f);
            var rm = new Material(mats["Camino"]) { name = "Camino (tramo)" };
            rm.SetTextureScale("_BaseMap", new Vector2(Mathf.Max(1f, Mathf.Round(width / 2.2f)), Mathf.Max(1f, Mathf.Round(go.transform.localScale.z / 2.2f))));
            go.GetComponent<Renderer>().sharedMaterial = rm; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); go.isStatic = true;
            RoadCap(parent, a, width, top); RoadCap(parent, b, width, top);
        }

        /// <summary>Remate redondo en cada extremo: los caminos ya no acaban en un rectángulo recortado y los cruces se funden.</summary>
        static void RoadCap(Transform parent, Vector3 p, float width, float roadTop)
        {
            foreach (var c in roadCaps) if ((c - p).sqrMagnitude < 0.01f) return; // un solo remate por punto (sin parpadeo)
            roadCaps.Add(p);
            float h = roadTop - 0.002f; // justo por debajo de su tramo
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); go.name = "CaminoRemate"; go.transform.SetParent(parent);
            go.transform.position = new Vector3(p.x, h / 2f, p.z); go.transform.localScale = new Vector3(width, h / 2f, width);
            var m = new Material(mats["Camino"]) { name = "Camino (remate)" };
            float k = Mathf.Max(1f, Mathf.Round(width / 2.2f)); m.SetTextureScale("_BaseMap", new Vector2(k, k));
            go.GetComponent<Renderer>().sharedMaterial = m; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); go.isStatic = true;
        }

        static void Hill(Transform parent, Vector3 center, Vector3 size)
        {
            var go = Prim(PrimitiveType.Sphere, parent, "Monte", center, size, "Ladera");
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); go.isStatic = true;
        }

        static Transform[] FenceRow(Transform parent, Vector3 start, Vector3 dir, int count, float height = 1.2f, float gap = 0.1f)
        {
            var list = new List<Transform>(); Vector3 cursor = start; float yaw = Quaternion.LookRotation(dir).eulerAngles.y - 90f;
            for (int i = 0; i < count; i++)
            {
                var f = Model("ENV_VallaMadera", parent, cursor, yaw, height);
                var b = Bounds(f.gameObject); float len = Mathf.Max(b.size.x, b.size.z);
                f.position += dir.normalized * len / 2f;
                cursor += dir.normalized * (len + gap); list.Add(f);
            }
            return list.ToArray();
        }

        static float Yaw(Vector3 from, Vector3 to) { var d = to - from; return Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg; }
        /// <summary>Giro para que el frontal de un modelo generado (ENV_*_01 / PROP_*), que mira a -X, apunte hacia un punto.</summary>
        static float Facing(Vector3 from, Vector3 to) => Yaw(from, to) + 90f;

        static GameObject Box(Transform parent, string name, Vector3 pos, Vector3 size, string mat) => Prim(PrimitiveType.Cube, parent, name, pos, size, mat);

        /// <summary>Primitiva de Unity con material propio. Ojo: el cilindro mide 2 de alto con escala 1 (size.y = media altura).</summary>
        static GameObject Prim(PrimitiveType type, Transform parent, string name, Vector3 pos, Vector3 size, string mat)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent);
            go.transform.position = pos; go.transform.localScale = size; go.GetComponent<Renderer>().sharedMaterial = mats[mat];
            return go;
        }

        static void Strut(Transform parent, Vector3 a, Vector3 b, float thickness)
        {
            var go = Box(parent, "Tirante", (a + b) / 2, new Vector3(thickness, thickness, (b - a).magnitude), "MaderaOscura");
            go.transform.rotation = Quaternion.LookRotation(b - a);
        }

        static void Glass(Transform parent, Vector3 pos, Vector3 size)
        {
            var g = Box(parent, "Cristal", pos, size, "Cristal");
            var sc = g.transform.localScale; g.transform.localScale = new Vector3(sc.x <= 0.13f ? 0.02f : sc.x, sc.y, sc.z <= 0.13f ? 0.02f : sc.z);
            g.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        static GameObject Paper(Transform parent, string name, Vector3 pos, Quaternion rot, Vector2 size)
        {
            var go = Box(parent, name, pos, new Vector3(size.x, 0.004f, size.y), "Papel");
            go.transform.rotation = rot;
            var c = go.GetComponent<BoxCollider>(); c.size = new Vector3(1, 12f, 1); // más fácil de apuntar
            return go;
        }

        static SearchSpot Search(Transform furniture, string label, int batteries, string found, string empty)
        {
            var s = furniture.gameObject.AddComponent<SearchSpot>(); s.label = label; s.batteries = batteries; s.foundText = found; s.emptyText = string.IsNullOrEmpty(empty) ? "No hay nada útil." : empty;
            return s;
        }

        static Bounds Bounds(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>(); var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b;
        }

        /// <summary>Instancia un modelo del equipo, lo apoya en el suelo y lo escala a una medida real.</summary>
        static Transform Model(string prefix, Transform parent, Vector3 pos, float yaw, float height, bool collider = false, float fitLongest = 0, float fitWidthX = 0, float fitDepthZ = 0, bool shadows = true)
        {
            var files = Directory.GetFiles(ModelDir, "*.glb").Concat(Directory.Exists(PolyHavenDir) ? Directory.GetFiles(PolyHavenDir, "*.gltf", SearchOption.AllDirectories) : new string[0]).Select(p => p.Replace('\\', '/')).Where(p => Path.GetFileName(p).StartsWith(prefix, StringComparison.Ordinal)).ToArray();
            if (files.Length == 0) throw new Exception("Modelo no encontrado: " + prefix);
            string path = files.OrderBy(p => p.Length).First();
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path) ?? throw new Exception("No se pudo cargar " + path);
            usedModels.Add(Path.GetFileName(path));
            var root = new GameObject(Path.GetFileNameWithoutExtension(path).Split('-')[0].Replace("ENV_", "").Replace("INT_", "").Replace("INR_", "").Replace("OBJ_", "")).transform;
            root.SetParent(parent, false);
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(asset); inst.transform.SetParent(root, false);
            var b = Bounds(inst);
            float scale = fitLongest > 0 ? fitLongest / Mathf.Max(b.size.x, b.size.y, b.size.z)
                        : fitWidthX > 0 ? fitWidthX / b.size.x
                        : fitDepthZ > 0 ? fitDepthZ / b.size.z
                        : height / b.size.y;
            inst.transform.localScale *= scale;
            // Se conserva el desplazamiento propio del modelo (algunos GLB traen el pivote ya movido a la base).
            inst.transform.localPosition = (inst.transform.localPosition - new Vector3(b.center.x, b.min.y, b.center.z)) * scale;
            root.position = pos; root.rotation = Quaternion.Euler(0, yaw, 0);
            foreach (var r in inst.GetComponentsInChildren<Renderer>()) { r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off; }
            foreach (var t in inst.GetComponentsInChildren<Transform>()) t.gameObject.isStatic = true;
            if (collider)
            {
                var bc = root.gameObject.AddComponent<BoxCollider>();
                var lb = b; bc.center = new Vector3(0, lb.size.y * scale / 2f, 0); bc.size = lb.size * scale;
            }
            log.Add($"{root.name}: {path} escala {scale:0.000} tamaño {(b.size * scale)}");
            return root;
        }

        // ==================================================================== assets
        static void CreateMaterials()
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var tablas = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/DanielRoom/Tablas.png");
            var ruido = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/FirstPlayable/SueloRuido.png");
            Mat("Madera", lit, new Color(0.42f, 0.32f, 0.22f), tablas, new Vector2(2, 2), 0.25f);
            Mat("MaderaOscura", lit, new Color(0.22f, 0.17f, 0.12f), tablas, new Vector2(1, 3), 0.2f);
            Mat("Tarima", lit, new Color(0.38f, 0.29f, 0.2f), tablas, new Vector2(3, 3), 0.3f);
            Mat("Pared", lit, new Color(0.33f, 0.35f, 0.31f), tablas, new Vector2(4, 1), 0.12f);
            Mat("Tejado", lit, new Color(0.24f, 0.22f, 0.2f), ruido, new Vector2(3, 3), 0.1f);
            Mat("Metal", lit, new Color(0.32f, 0.33f, 0.31f), null, Vector2.one, 0.55f).SetFloat("_Metallic", 0.7f);
            Mat("Carcasa", lit, new Color(0.13f, 0.135f, 0.13f), null, Vector2.one, 0.45f);
            Mat("Papel", lit, new Color(0.85f, 0.82f, 0.72f), null, Vector2.one, 0.1f);
            // Corteza de los árboles secos: se reutiliza la textura del tocón de Poly Haven.
            Mat("Corteza", lit, new Color(0.40f, 0.38f, 0.35f), AssetDatabase.LoadAssetAtPath<Texture2D>(PolyHavenDir + "/tree_stump_01/textures/tree_stump_01_diff_1k.jpg"), Vector2.one, 0.04f);
            // Suelo y caminos con texturas CC0 de Poly Haven (2K): hierba seca con tierra y grava gastada.
            Ground("Suelo", lit, "grass_ground", new Color(0.62f, 0.66f, 0.56f), new Vector2(170, 170), 0.05f);
            Ground("Camino", lit, "gravel_road", new Color(0.46f, 0.54f, 0.52f), Vector2.one, 0.08f);
            Emissive("Bombilla", new Color(1f, 0.8f, 0.55f), 3f);
            Emissive("Ambar", new Color(1f, 0.6f, 0.2f), 1.2f);
            Emissive("Piloto", new Color(1f, 0.12f, 0.06f), 2f);
            Mat("PilotoApagado", lit, new Color(0.22f, 0.05f, 0.04f), null, Vector2.one, 0.6f);
            Mat("BombillaApagada", lit, new Color(0.30f, 0.29f, 0.26f), null, Vector2.one, 0.75f);
            Mat("Esmalte", lit, new Color(0.16f, 0.21f, 0.19f), null, Vector2.one, 0.55f);
            // Ladera del monte de la mina: misma hierba que el terreno, con un mosaico ajustado a la esfera.
            Ground("Ladera", lit, "grass_ground", new Color(0.55f, 0.59f, 0.50f), new Vector2(40, 14), 0.05f);

            var glass = Mat("Cristal", lit, new Color(0.55f, 0.62f, 0.62f, 0.14f), null, Vector2.one, 0.7f);
            glass.SetFloat("_Surface", 1); glass.SetFloat("_Blend", 0); glass.SetOverrideTag("RenderType", "Transparent");
            glass.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); glass.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); glass.SetFloat("_ZWrite", 0);
            glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); glass.renderQueue = (int)RenderQueue.Transparent;
            glass.SetFloat("_SpecularHighlights", 0); glass.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            glass.SetFloat("_EnvironmentReflections", 0); glass.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");

            var screen = Mat("Pantalla", Shader.Find("Universal Render Pipeline/Unlit"), Color.white, null, Vector2.one, 0);

            var fogTex = SoftTexture();
            var fog = Mat("Niebla", Shader.Find("Universal Render Pipeline/Particles/Unlit"), Color.white, fogTex, Vector2.one, 0);
            fog.SetFloat("_Surface", 1); fog.SetFloat("_Blend", 0); fog.SetOverrideTag("RenderType", "Transparent");
            fog.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); fog.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); fog.SetFloat("_ZWrite", 0);
            fog.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); fog.renderQueue = (int)RenderQueue.Transparent;
            fog.SetFloat("_SoftParticlesEnabled", 0);
            AssetDatabase.SaveAssets();
        }

        // ==================================================================== árboles secos
        const int TreeVariants = 6;
        static readonly Dictionary<int, Mesh> treeMeshes = new Dictionary<int, Mesh>();

        /// <summary>Árbol sin hojas generado por código: tronco y ramas como tubos que se estrechan y se bifurcan.</summary>
        static Mesh TreeMesh(int variant)
        {
            if (treeMeshes.TryGetValue(variant, out var cached) && cached) return cached;
            var rnd = new System.Random(4100 + variant * 37);
            float Rand(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            const int maxDepth = 5;
            void Ring(Vector3 c, Quaternion rot, float r, float vv, int sides)
            {
                for (int i = 0; i <= sides; i++) { float a = i / (float)sides * Mathf.PI * 2f; v.Add(c + rot * new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r)); uv.Add(new Vector2(i / (float)sides * 2f, vv)); }
            }
            void Branch(Vector3 start, Quaternion rot, float len, float r0, int depth, float vv)
            {
                int sides = depth <= 1 ? 8 : depth <= 3 ? 6 : 4, segs = depth == 0 ? 4 : 3;
                var pos = start; var q = rot; int first = v.Count; float r1 = r0 * 0.62f;
                Ring(pos, q, r0, vv, sides);
                for (int sgm = 1; sgm <= segs; sgm++)
                {
                    float wob = 5f + depth * 4f;
                    q = Quaternion.Euler(Rand(-wob, wob), 0, Rand(-wob, wob)) * q;
                    pos += q * Vector3.up * (len / segs); vv += len / segs / 1.6f;
                    Ring(pos, q, Mathf.Lerp(r0, r1, sgm / (float)segs), vv, sides);
                    int a = first + (sgm - 1) * (sides + 1), b = a + sides + 1;
                    for (int i = 0; i < sides; i++) { tri.Add(a + i); tri.Add(b + i); tri.Add(a + i + 1); tri.Add(a + i + 1); tri.Add(b + i); tri.Add(b + i + 1); }
                }
                if (depth >= maxDepth || r1 < 0.012f) return;
                int kids = depth == 0 ? 3 + rnd.Next(2) : 2 + rnd.Next(2);
                float spin = Rand(0, 360);
                for (int k = 0; k < kids; k++)
                {
                    var cq = q * Quaternion.Euler(0, spin + k * (360f / kids) + Rand(-25, 25), 0) * Quaternion.Euler(Rand(24, 52), 0, 0);
                    var dir = (cq * Vector3.up + Vector3.up * 0.22f).normalized; // las ramas tiran un poco hacia arriba
                    Branch(pos, Quaternion.FromToRotation(Vector3.up, dir), len * Rand(0.6f, 0.8f), r1 * Rand(0.62f, 0.85f), depth + 1, vv);
                }
            }
            Branch(new Vector3(0, -0.15f, 0), Quaternion.Euler(Rand(-4, 4), 0, Rand(-4, 4)), Rand(2.2f, 3.2f), Rand(0.2f, 0.27f), 0, 0);
            var mesh = new Mesh { name = "ArbolSeco_" + variant, indexFormat = v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.SetVertices(v); mesh.SetUVs(0, uv); mesh.SetTriangles(tri, 0); mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
            Directory.CreateDirectory(ArtDir + "/Arboles");
            string path = ArtDir + "/Arboles/ArbolSeco_" + variant + ".asset";
            AssetDatabase.DeleteAsset(path); AssetDatabase.CreateAsset(mesh, path);
            treeMeshes[variant] = mesh; log.Add($"Árbol seco {variant}: {tri.Count / 3} triángulos");
            return mesh;
        }

        static void BareTree(Transform parent, Vector3 pos, float yaw, float height, int index, bool shadows)
        {
            var mesh = TreeMesh(index % TreeVariants);
            var go = new GameObject("ArbolSeco"); go.transform.SetParent(parent);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = mats["Corteza"]; r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            float scale = height / mesh.bounds.size.y;
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0)); go.transform.localScale = Vector3.one * scale;
            go.isStatic = true;
        }

        const string GroundDir = "Assets/Art/Puesto/Suelo";
        static Material Ground(string name, Shader shader, string texName, Color tint, Vector2 tiling, float smooth)
        {
            string normalPath = GroundDir + "/" + texName + "_nor_gl.jpg";
            var imp = AssetImporter.GetAtPath(normalPath) as TextureImporter;
            if (imp != null && imp.textureType != TextureImporterType.NormalMap) { imp.textureType = TextureImporterType.NormalMap; imp.SaveAndReimport(); }
            var m = Mat(name, shader, tint, AssetDatabase.LoadAssetAtPath<Texture2D>(GroundDir + "/" + texName + "_Diffuse.jpg"), tiling, smooth);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if (normal != null) { m.SetTexture("_BumpMap", normal); m.SetFloat("_BumpScale", 1f); m.EnableKeyword("_NORMALMAP"); }
            EditorUtility.SetDirty(m); return m;
        }

        static Material Mat(string name, Shader shader, Color color, Texture tex, Vector2 tiling, float smooth)
        {
            string path = ArtDir + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
            m.shader = shader;
            m.SetColor("_BaseColor", color);
            if (tex != null) { m.SetTexture("_BaseMap", tex); m.SetTextureScale("_BaseMap", tiling); }
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
            EditorUtility.SetDirty(m); mats[name] = m; return m;
        }

        static void Emissive(string name, Color c, float intensity)
        {
            var m = Mat(name, Shader.Find("Universal Render Pipeline/Lit"), c, null, Vector2.one, 0.4f);
            m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", c * intensity); m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        }

        static Texture2D SoftTexture()
        {
            string path = ArtDir + "/NieblaParticula.png";
            if (!File.Exists(path))
            {
                var t = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
                    {
                        float dx = (x - 63.5f) / 63.5f, dy = (y - 63.5f) / 63.5f; float r = Mathf.Sqrt(dx * dx + dy * dy);
                        float n = Mathf.PerlinNoise(x * 0.06f, y * 0.06f) * 0.5f + Mathf.PerlinNoise(x * 0.13f + 7, y * 0.13f + 3) * 0.5f;
                        float a = Mathf.Clamp01(1f - r); a = a * a * (3 - 2 * a) * Mathf.Lerp(0.55f, 1f, n);
                        t.SetPixel(x, y, new Color(1, 1, 1, a));
                    }
                File.WriteAllBytes(path, t.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
                var imp = (TextureImporter)AssetImporter.GetAtPath(path); imp.alphaIsTransparency = true; imp.wrapMode = TextureWrapMode.Clamp; imp.mipmapEnabled = true; imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static RenderTexture CreateFeedTexture()
        {
            string path = ArtDir + "/SenalCamaras.renderTexture";
            var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
            if (rt == null) { rt = new RenderTexture(480, 360, 24) { name = "SenalCamaras", filterMode = FilterMode.Point, antiAliasing = 1 }; AssetDatabase.CreateAsset(rt, path); }
            mats["Pantalla"].SetTexture("_BaseMap", rt);
            return rt;
        }

        static VolumeProfile CreateVolumeProfile()
        {
            string path = ArtDir + "/PuestoPostProceso.asset";
            AssetDatabase.DeleteAsset(path);
            var p = ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(p, path);
            void Add<T>(Action<T> setup) where T : VolumeComponent { var c = p.Add<T>(true); setup(c); c.name = typeof(T).Name; AssetDatabase.AddObjectToAsset(c, p); }
            Add<Vignette>(v => { v.intensity.Override(0.36f); v.smoothness.Override(0.45f); v.color.Override(new Color(0.02f, 0.03f, 0.03f)); });
            Add<FilmGrain>(f => { f.type.Override(FilmGrainLookup.Medium3); f.intensity.Override(0.32f); f.response.Override(0.75f); });
            Add<ColorAdjustments>(c => { c.saturation.Override(-28f); c.contrast.Override(14f); c.postExposure.Override(0.55f); c.colorFilter.Override(new Color(0.92f, 1f, 0.95f)); });
            Add<Bloom>(b => { b.intensity.Override(0.55f); b.threshold.Override(1.0f); b.scatter.Override(0.65f); });
            Add<Tonemapping>(t => t.mode.Override(TonemappingMode.Neutral));
            Add<ChromaticAberration>(c => c.intensity.Override(0.12f));
            EditorUtility.SetDirty(p); AssetDatabase.SaveAssets();
            return p;
        }

        public static void Capture(Camera camera, string path)
        {
            var rt = new RenderTexture(1280, 720, 24); var prev = RenderTexture.active; var old = camera.targetTexture; bool wasEnabled = camera.enabled;
            camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            camera.targetTexture = old; RenderTexture.active = prev; UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex); camera.enabled = wasEnabled;
        }

        static List<RadioSystem.Beat> Beats() => new List<RadioSystem.Beat>
        {
            new RadioSystem.Beat { hour = 22.03f, speaker = "Radio · relevo de tarde", text = "Torre de Valdara, te dejo el turno, Daniel. Cuatro cámaras, el parte en la mesa y la linterna junto al monitor. Hasta que salga el sol, esto es tuyo." },
            new RadioSystem.Beat { hour = 22.45f, speaker = "Radio · relevo de tarde", text = "Una cosa más: si algo cambia en las cámaras, lo registras. Lo que sea. Si salta la luz, el panel está junto a la puerta. Y no bajes." },
            new RadioSystem.Beat { hour = 23.5f, speaker = "Vecina (radio)", text = "¿Torre? Soy la de la casa del camino. Los perros llevan una hora sin ladrar. Eso no pasa nunca." },
            new RadioSystem.Beat { hour = 24.0f, speaker = "Radio · relevo de tarde", text = "Medianoche. La niebla está bajando antes que otras noches. Atento a las cámaras." },
            new RadioSystem.Beat { hour = 25.2f, speaker = "Vecino (radio)", text = "Torre, aquí abajo no se ve ni la farola de enfrente. ¿Desde arriba se ve algo?" },
            new RadioSystem.Beat { hour = 26.4f, speaker = "Radio · operaciones", text = "Torre de vigilancia, aquí operaciones. Cualquier incidencia en la zona de extracción se nos comunica a nosotros. A nadie más. Corto." },
            new RadioSystem.Beat { hour = 28.4f, speaker = "Vecina (radio)", text = "Daniel... ¿sigues ahí? Empieza a clarear por el monte. Aguanta un poco más.", objective = "Aguanta hasta las 06:00. Sigue registrando lo que veas." },
            new RadioSystem.Beat { hour = 29.6f, speaker = "Radio · relevo de tarde", text = "Ya sale el sol. Con la luz, la niebla se queda quieta. Buen trabajo... si se le puede llamar así." },
        };

        // ==================================================================== build + prueba
        public static void RunAndBuild() { Run(); BuildExecutable(); }

        [MenuItem("THE FOG/Compilar ejecutable del Puesto")]
        public static void BuildExecutable()
        {
            var opts = new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/PuestoDaniel/TheFog.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.CleanBuildCache };
            PlayerSettings.productName = "THE FOG"; PlayerSettings.companyName = "THE FOG";
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow; PlayerSettings.resizableWindow = true;
            var r = BuildPipeline.BuildPlayer(opts);
            Debug.Log("PUESTO BUILD: " + r.summary.result + " " + r.summary.totalSize / (1024 * 1024) + " MB, errores " + r.summary.totalErrors);
            if (r.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Build fallida");
        }
    }
}
