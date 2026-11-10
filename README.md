# The Fog

Prototipo 3D de Valdara en Unity. La casa de Daniel ocupa un mapa propio con una linde arbolada, un claro, una leñera y una torre de vigilancia. Se puede jugar desde esa escena o llegar por la senda desde Valdara.

## Abrir el proyecto

1. Clona el repositorio y cambia a la rama `unity/casa-daniel`.
2. En Unity Hub, añade la carpeta clonada como proyecto y ábrela con **Unity 6.3 LTS (6000.3.28f1)**. La primera importación descarga glTFast y convierte los modelos en prefabs reutilizables.
3. Abre `Assets/TheFog/Scenes/CasaDaniel.unity` y pulsa **Play**. Si empiezas desde `Valdara.unity`, usa **E** en la puerta de la senda de Daniel para llegar al claro. El catálogo de los 26 modelos de partida se abre desde el menú de pausa.

Usa **WASD** o las flechas para moverte, el **ratón** para mirar, **Mayús** para correr, **E** para interactuar, **F** para controlar la linterna y **Esc** para pausar.

## Editar Valdara

Las escenas ensamblan el escenario al pulsar Play. Para mover los objetos desde el editor, abre la escena, elige **The Fog → Materializar escena para editar** y guárdala. Esa escena usará la jerarquía guardada al pulsar Play. Los modelos seguirán vinculados a los prefabs importados. Las escenas de partida también están incluidas en la lista de compilación de Unity.

Las copias optimizadas viven en `Assets/TheFog/Resources/Modelos`. Incluyen el mobiliario con variantes abiertas y cerradas, los elementos del exterior y la linterna. Las 26 copias suman unos 35 MiB. Los archivos originales se conservan en la carpeta de origen.

La casa usa geometría modular provisional y texturas de 64 píxeles. La cámara del mapa usa un filtro de baja resolución y una paleta de verdes, madera y luz cálida inspirada en la dirección retro de [MOLDRISE](https://picster.itch.io/moldrise). La casa se puede atravesar por la puerta principal. La torre tiene su malla en `Assets/TheFog/Resources/Modelos/Exterior/TorreVigilancia.glb` y las cuatro vistas separadas en `ArtReference/TorreVigilancia`. Sus colisiones son piezas separadas en `DanielMap.cs`, así podéis reemplazar el modelo conservando los peldaños, el mirador y las barandas si mantenéis sus medidas.

## Trabajo en equipo

Podéis repartir las cinco áreas iniciales entre exterior, interiores, interacción y jugador, catálogo y modelos, e iluminación y pruebas. Cread una rama por cambio y revisadla antes de integrarla. Acordad quién guarda cada escena materializada para evitar que dos personas editen el mismo archivo a la vez. Subid siempre cada archivo `.meta` junto a su modelo, escena o script.

### Recorrido de comprobación

- En `CasaDaniel`, sigue el sendero, abre la puerta del porche con **E**, cruza el salón y recoge la linterna en la mesita del dormitorio.
- Sal por la puerta del porche, sigue la señal de la torre, sube hasta el mirador y vuelve a bajar por la escalera.
- Vuelve al camino. Puedes visitar la tienda y continuar hacia la mina. Antes de recoger la linterna, la puerta de la mina debe indicar que la necesitas.
- Prueba los dos sentidos de la transición Valdara ↔ CasaDaniel. Al volver a Valdara con la linterna, debe funcionar con **F** y desbloquear la mina.
- Entra en la mina, prueba **F** y usa **E** frente al aviso del fondo. El objetivo debe cambiar a «Recorrido completado».
- En el catálogo, comprueba que aparecen los 26 modelos con su nombre.
