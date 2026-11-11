# The Fog

Beta en desarrollo de un juego de terror rural con estética PS1/PS2. La casa original de Daniel guía la dirección visual. El mapa y el arte siguen en construcción.

## Base activa

La rama de referencia es **`unity/casa-daniel`**. No retomar desde DanielRoom ni desde las ramas antiguas `develop` o `feature/gameplay-foundation`.

Para continuar, consulta primero [CONTINUAR.md](CONTINUAR.md). El registro detallado está en [docs/registro-avances-2026-11-11.md](docs/registro-avances-2026-11-11.md).

## Abrir y jugar

1. Clona el repositorio con **Git LFS** instalado. Cambia a `unity/casa-daniel` y ejecuta `git lfs pull` para descargar los modelos originales.
2. Añade la carpeta del repositorio a Unity Hub y abre con **Unity 6000.6.8f1**. El proyecto usa URP 17.6 y glTFast.
3. Abre `Assets/TheFog/Scenes/CasaDaniel.unity` y pulsa Play. El inicio aparece dentro del dormitorio de Daniel; elige Jugar.

Controles: WASD o flechas para moverse, ratón para mirar, Mayús para correr, E para interactuar, F para la linterna y Esc para pausar.

Unity Hub abre la copia local del repositorio. Antes de empezar otra sesión, actualiza esta misma carpeta con Git para tener la versión compartida.

## Estado jugable

- Inicio en el dormitorio de Daniel, casa amueblada, linterna, notas legibles y muebles que se abren y cierran.
- Entrada y salida de la casa mediante fundido a un interior separado.
- Conexión CasaDaniel ↔ Valdara, tienda y entrada de mina que requiere linterna.
- Aviso de cierre al fondo de la mina y acceso a créditos al completar el tramo del prototipo.
- La exposición de modelos se reserva para los créditos.

Recorrido de trabajo: dormitorio → salón/cocina → exterior de Daniel → senda de Valdara → mina. La tienda y vigilancia son desvíos de exploración.

## Arte y edición

Los 26 modelos originales de partida conservan su geometría y texturas, sin decimación. Se añadieron dos módulos originales de pared. Los GLB usan Git LFS y sus `.meta` deben viajar junto a ellos. La restauración original está documentada en `docs/original-models-restoration.json`.

CasaDaniel contiene una jerarquía materializada. Parte de los detalles recientes, como los módulos interiores y papeles, se añade al entrar en Play. Valdara y Catalogo conservan su constructor. No ejecutar herramientas de regeneración o restauración sin revisar si sobrescriben trabajo manual de la escena.

En CasaDaniel están desactivados los elementos provisionales retirados de la torre, vegetación y carteles. Valdara todavía contiene una torre provisional; su modelo definitivo no está resuelto. El efecto retro de CasaDaniel está activo.

La textura de papel se generó con Gemini Pro. La marca es propia y la fuente provisional es IBM Plex Mono, con su licencia en la carpeta de Tipografia. **La tipografía necesita mejorar y no está aprobada como definitiva.**

## Comprobaciones

Con CasaDaniel abierta, usa **The Fog → Revisar lectura y pausa**. Comprueba notas, apertura/cierre de muebles y suelo en cuatro puntos de Valdara. Guarda capturas y resultados en `docs/prototype-review` y deja Play en Valdara pausada. Al salir de Play se recupera la escena inicial.

La última revisión dio PASS. No sustituye una prueba completa del recorrido ni de todas las colisiones.

Para generar un ejecutable actualizado, usa **The Fog → Generar versión para Windows**. Esta entrega en GitHub contiene el proyecto fuente y no publica una nueva versión ejecutable.

## Próximas prioridades

1. Tipografía más acorde con el juego, legible con tildes y ñ.
2. Más modelos 3D y decoración coherentes con la casa.
3. Recorrido completo, colisiones, escala y orientación de accesos.
4. Torre de vigilancia definitiva y recursos interiores pendientes.
5. Ambientación sonora y eventos alineados con el GDD del equipo.

Crear ramas `feature/casa-<tarea>` o `fix/casa-<problema>` desde la base activa. Acordar quién edita cada escena para evitar conflictos.
