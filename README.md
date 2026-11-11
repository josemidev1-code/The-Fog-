# The Fog

Prototipo 3D de Valdara en Unity. La casa de Daniel ocupa un mapa propio con una linde arbolada, un claro, una leñera y una torre de vigilancia. Se puede jugar desde esa escena o llegar por la senda desde Valdara.

## Base activa de desarrollo

La rama de referencia es `unity/casa-daniel`. El primer escenario es **CasaDaniel: casa, torre y bosque**, y ocupa la primera posición de la lista de compilación. DanielRoom y su menú de habitación no forman parte de esta base. Las ramas anteriores se conservan como historial; `develop` y `feature/gameplay-foundation` pertenecen a la propuesta descartada de DanielRoom y no son la base actual.

Las próximas tareas de jugabilidad deben partir de esta rama, con nombres `feature/casa-<tarea>` o `fix/casa-<problema>`, y volver a ella mediante PR. Primero se consolidará el recorrido casa → bosque → torre → regreso, comprobando colisiones, subida y bajada, interacción y linterna. No se añaden nuevas mecánicas ni historia como parte de este cambio.

## Abrir el proyecto

1. Clona el repositorio y cambia a la rama `unity/casa-daniel`.
2. En Unity Hub, abre **TheFog** en `Documents/Codex/TheFog` con **Unity 6.6 (6000.6.8f1)**. El proyecto usa **URP 17.6.0**, glTFast y materiales compatibles con ese renderizado.
3. Abre `Assets/TheFog/Scenes/CasaDaniel.unity` y pulsa **Play**. Si empiezas desde `Valdara.unity`, usa **E** en la puerta de la senda de Daniel para llegar al claro. El catálogo de los 26 modelos de partida se abre desde el menú de pausa.

Usa **WASD** o las flechas para moverte, el **ratón** para mirar, **Mayús** para correr, **E** para interactuar, **F** para controlar la linterna y **Esc** para pausar.

## Editar Valdara

**CasaDaniel ya tiene la jerarquía guardada:** la casa, el bosque, el interior y la torre se pueden ver y editar sin pulsar Play. Usa **The Fog → Abrir casa, torre y bosque** para abrirla y encuadrar el exterior. Los modelos siguen vinculados a los prefabs importados. Las otras escenas conservan su constructor y se pueden materializar con **The Fog → Materializar escena para editar**.

El renderizado global y todos los niveles de calidad usan `Assets/TheFog/Rendering/CasaURP.asset`. El efecto retro es un pase de URP, sin `OnRenderImage`. El aviso de renderizado integrado obsoleto ya no corresponde a esta configuración. La migración no completa el arte provisional de la torre ni sustituye una prueba completa de subida y bajada.

`Assets/TheFog/Resources/Modelos` usa ahora los **26 GLB originales**, sin decimación ni reducción de texturas. Se restauraron a petición del usuario para comparar la calidad visual. Sus contenidos SHA-256 coinciden con los originales de OneDrive: consulta `docs/original-models-restoration.json`. Las versiones optimizadas anteriores siguen recuperables en el historial de Git y en una copia local de comparación. Los GLB se guardan con Git LFS.

La fachada usa el **original de `ENV_CasaDaniel.glb`**. Su puerta conecta con el interior amueblado de la misma escena al pulsar **E**. El filtro retro está desactivado en la cámara guardada de CasaDaniel para valorar geometría y texturas sin pixelación. La iluminación y la niebla se conservan; la vista Scene del menú de trabajo oculta la niebla para editar.

La torre provisional se construye por piezas en `DanielTower.cs`, con cuatro tramos alternos, descansillos, un balcón y una plataforma superior. Al sustituir su arte por una malla definitiva, conservad el trazado y el ancho libre de los tramos y la abertura de la cubierta para que se pueda seguir subiendo.

## Trabajo en equipo

Podéis repartir las cinco áreas iniciales entre exterior, interiores, interacción y jugador, catálogo y modelos, e iluminación y pruebas. Cread una rama por cambio y revisadla antes de integrarla. Acordad quién guarda cada escena materializada para evitar que dos personas editen el mismo archivo a la vez. Subid siempre cada archivo `.meta` junto a su modelo, escena o script.

### Recorrido de comprobación

- En `CasaDaniel`, sigue el sendero, usa **E** en la puerta de la fachada, cruza el salón y recoge la linterna en la mesita del dormitorio.
- Sal por la misma puerta, sigue la señal de la torre, sube por los cuatro tramos y vuelve a bajar desde la cubierta.
- Vuelve al camino. Puedes visitar la tienda y continuar hacia la mina. Antes de recoger la linterna, la puerta de la mina debe indicar que la necesitas.
- Prueba los dos sentidos de la transición Valdara ↔ CasaDaniel. Al volver a Valdara con la linterna, debe funcionar con **F** y desbloquear la mina.
- Entra en la mina, prueba **F** y usa **E** frente al aviso del fondo. El objetivo debe cambiar a «Recorrido completado».
- En el catálogo, comprueba que aparecen los 26 modelos con su nombre.
