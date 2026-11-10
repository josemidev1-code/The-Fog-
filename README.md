# The Fog

Primer prototipo 3D de Valdara en Unity. El recorrido conecta el camino del pueblo, la casa de Daniel, la tienda de ultramarinos y la galería de la mina. La primera misión es encontrar la linterna de Daniel y leer el aviso al final de la galería.

## Abrir el proyecto

1. Clona el repositorio y cambia a la rama `unity/valdara-prototipo` mientras la primera versión se revisa.
2. En Unity Hub, añade la carpeta clonada como proyecto y ábrela con **Unity 6.3 LTS (6000.3.28f1)**. La primera importación descarga glTFast y convierte los 26 modelos en prefabs reutilizables.
3. Abre `Assets/TheFog/Scenes/Valdara.unity` y pulsa **Play**. Para revisar todos los modelos, abre `Catalogo.unity` o elige **Abrir catálogo** en el menú de pausa.

Usa **WASD** o las flechas para moverte, el **ratón** para mirar, **Mayús** para correr, **E** para interactuar, **F** para controlar la linterna y **Esc** para pausar.

## Editar Valdara

Las escenas ensamblan el escenario al pulsar Play. Para mover los objetos desde el editor, abre la escena, elige **The Fog → Materializar escena para editar** y guárdala. Esa escena usará la jerarquía guardada al pulsar Play. Los modelos seguirán vinculados a los prefabs importados. Las escenas de partida también están incluidas en la lista de compilación de Unity.

Las copias optimizadas viven en `Assets/TheFog/Resources/Modelos`. Incluyen el mobiliario con variantes abiertas y cerradas, los elementos del exterior y la linterna. Las 26 copias suman unos 35 MiB. Los archivos originales se conservan en la carpeta de origen.

## Trabajo en equipo

Podéis repartir las cinco áreas iniciales entre exterior, interiores, interacción y jugador, catálogo y modelos, e iluminación y pruebas. Cread una rama por cambio y revisadla antes de integrarla. Acordad quién guarda cada escena materializada para evitar que dos personas editen el mismo archivo a la vez. Subid siempre cada archivo `.meta` junto a su modelo, escena o script.

### Recorrido de comprobación

- Desde el cartel de Valdara, sigue el camino hasta la señal de la casa de Daniel. Entra con **E**, busca la mesita del dormitorio y recoge la linterna.
- Vuelve al camino. Puedes visitar la tienda y continuar hacia la mina. Antes de recoger la linterna, la puerta de la mina debe indicar que la necesitas.
- Entra en la mina, prueba **F** y usa **E** frente al aviso del fondo. El objetivo debe cambiar a «Recorrido completado».
- En el catálogo, comprueba que aparecen los 26 modelos con su nombre.
