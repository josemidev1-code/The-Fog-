# Continuar The Fog

Estado guardado el 11/11/2026. Rama base: `unity/casa-daniel`.

## Retomar

1. Actualizar esta rama desde origin y ejecutar `git lfs pull`.
2. Abrir la misma carpeta del repositorio en Unity 6000.6.8f1.
3. Abrir `Assets/TheFog/Scenes/CasaDaniel.unity`, pulsar Play y elegir Jugar.
4. Leer `docs/direccion-primer-tramo.md` y `docs/registro-avances-2026-11-11.md` antes de generar nuevos recursos.

## Petición más reciente del responsable

La beta va cogiendo forma. Aún faltan más modelos 3D, más decoración, mejor tipografía y pulido visual y jugable. La tipografía es la prioridad visual inmediata. Conservar la estética de la casa y los modelos originales de Hunyuan sin optimizarlos.

El responsable autoriza avanzar en el proyecto y generar recursos con Gemini Pro cuando sean útiles. No confundir una imagen de referencia con un modelo 3D ya integrado. Los cinco modelos de interior propuestos no se han generado todavía en Hunyuan y no se han gastado sus generaciones en esta tanda.

## Qué se ha guardado

- Pantalla inicial en el dormitorio, marca propia, controles, interacción, linterna y pausa.
- Casa y modelos originales, dos módulos adicionales de pared y cambios de distribución/iluminación.
- Notas físicas con textura generada en Gemini Pro y lectura de texto en Unity.
- Objetivos y avisos con altura ajustada al texto; pausa personalizada.
- Apertura y cierre del fregadero y alacena mediante modelos alternativos.
- Transiciones a interiores separados y viaje entre CasaDaniel y Valdara.
- Materiales del camino de Valdara, señales y aviso narrativo de cierre de la mina.
- Créditos y catálogo reservados para completar el tramo del prototipo.
- Herramienta de generación de ejecutable Windows desde el menú The Fog.

## Archivos que guían los próximos cambios

- `Assets/TheFog/Scripts/ValdaraPlayer.cs`: controles, inicio, pausa, lectura, objetivos y viaje.
- `DanielHouse.cs`, `DanielDocuments.cs`: interior de Daniel, módulos y papeles.
- `DanielMap.cs`, `ValdaraExterior.cs`, `ValdaraInteriores.cs`: recorrido y escenarios.
- `ValdaraScene.cs`: distingue construcción en tiempo de ejecución de la escena materializada.
- `Assets/TheFog/Resources/Tipografia`: marca y fuente provisional con licencia OFL.
- `Assets/TheFog/Resources/Daniel/PapelNota.jpg`: original generado por Gemini.
- `Assets/TheFog/Editor/DanielPlayReview.cs`: revisión reproducible de esta tanda.

## Pendientes concretos

- Elegir mejor tipografía para menú, indicaciones y documentos; comprobar tildes, ñ y lectura en resolución retro.
- Probar a pie casa → Valdara → mina y regreso, además de tienda, puertas y muebles. Revisar colisiones, escala y guía visual.
- La torre de Valdara sigue siendo provisional. No presentarla como modelo definitivo aprobado ni como recorrido de subida validado.
- Completar puerta, lámpara de techo, ventana, radio y tablón usando las fichas del canal referencias-3d. Hay una imagen de radio generada anteriormente, pero no un nuevo modelo de radio integrado.
- Añadir decoración de forma deliberada y coherente. Evitar reintroducir los packs descartados por su estética.
- Contrastar los textos de ambientación con el GDD antes de ampliar la historia.

## Validación y límites

Compilación de scripts correcta y diferencias sin errores de formato. `docs/prototype-review/resultados.txt` da PASS para ambos documentos, los dos muebles y cuatro comprobaciones de suelo en Valdara. Las capturas muestran lectura, pausa y camino hacia la mina. No se ha validado todo el mapa ni se ha distribuido un ejecutable con todos estos cambios recientes.

El menú de revisión deja el juego en Valdara pausado; salir de Play devuelve CasaDaniel. No regenerar la escena materializada sin revisar qué se perdería.

## Discord

Servidor THE FOG. Se crearon avances, errores-y-pruebas y mapas-y-jugabilidad dentro de Desarrollo.

Registro con tres capturas y resultados publicado en avances:
https://discord.com/channels/1561791993972269110/1570145544221757451/1570150398524653630

Publicación anterior del menú y marca:
https://discord.com/channels/1561791993972269110/1570145544221757451/1570146328611258389

El registro de Discord describe el estado anterior a esta subida del proyecto a GitHub. La rama remota actualizada será la referencia para continuar.
