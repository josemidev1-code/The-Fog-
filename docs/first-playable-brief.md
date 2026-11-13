# THE FOG — Primera escena jugable

## Estado comprobado

Repositorio: `josemidev1-code/The-Fog-`. Rama de trabajo: `codex/first-playable`, basada en la integración de OneDrive. Unity 6000.6.8f1 con URP y glTFast 6.20.0. Los 26 GLB originales están en `Assets/Models`; su inventario y la importación verificada de 74 texturas están en `docs/`.

El juego anterior de Godot y el proyecto `My-project` son referencias separadas: no sobrescribirlos ni confundirlos con una conversión terminada. No modificar los GLB originales ni sus GUID.

## Dirección y contexto

Terror psicológico rural retro PS1/PS2, Valdara, finales de los 80 o principios de los 90. Daniel es el protagonista en primera persona, vigilante en una casa aislada junto a una torre. No inventar biografía, armas, enemigos ni mecánicas canónicas.

Paleta gris verdosa, oliva y marrón desaturado; madera oscura húmeda, metal oxidado, niebla fría y puntos de luz cálida. Geometría legible y escala humana. La referencia visual histórica más reciente del entorno es MOLDRISE: referencia de atmósfera, sin copiar mapas ni extraer sus recursos. Evitar cartoon y fotorrealismo moderno.

Confirmación del usuario durante esta iteración: la referencia principal se escribe **MOLDRISE**. El juego debe adaptarse a una gama amplia de PCs mediante ajustes Bajo, Medio y Alto, manteniendo identidad visual en todos ellos. Los PCs de desarrollo pueden llevar GPU tipo RTX 5060/5070/5070 Ti, pero esa gama no es un requisito mínimo. Ajustar resolución, sombras, distancia y efectos, y medir antes de prometer compatibilidad o FPS. No confundir estética retro con bajo coste automático de renderizado.

Codex ha preparado seis copias de uso jugable en `Assets/Models/Optimized` con Blender 5.2.3; conservan sus nombres. Consultar `docs/optimization-report.json`. Se mantienen los originales. Las copias requieren revisión visual antes de considerarlas finales.

Fuentes de referencia: `josemidev1-code/thefogbotdiscord/fichas/CHAR_Daniel_Ficha_Discord.md` y la documentación histórica `docs/art_direction.md` y `docs/next_iteration.md` en la copia local antigua de THE-FOG. Las decisiones históricas no prueban que sus sistemas estén implementados en Unity.

## Entrega de esta iteración

Una escena persistente y editable `Assets/Scenes/FirstPlayable.unity`, visible en el editor sin ejecutar un generador, con:

1. Movimiento FPS, ratón, gravedad, colisión y pausa/captura del cursor. Usar el Input System ya instalado; evitar el antiguo Input Manager deshabilitado.
2. Linterna con encendido y apagado; reutilizar `OBJ_Linterna.glb` sin inventar brazos definitivos.
3. Exterior rural pequeño con suelo transitable, casa de Daniel, vegetación, rocas y entrada de mina como hito. Usar modelos existentes, normalizar escala sobre las instancias y conservar sus materiales.
4. Niebla e iluminación legibles; estética retro moderada y controles cómodos.
5. Un recorrido de prueba con objetivo y una interacción sencilla, explícitamente provisional; sin dar por aprobada historia nueva.
6. HUD sencillo con controles e indicación de interacción; reinicio y salida de pausa coherentes.

Módulos separados para jugador, interacción, linterna, ambiente y UI. Colisiones simplificadas en el entorno; no bloquear al jugador dentro de las cajas de los modelos. Si una casa es una carcasa cerrada, documentar el límite en lugar de fingir que tiene interior jugable. No prometer torre, escaleras o criaturas acabadas si faltan modelos.

## Comprobación y entrega

Compilar con el Unity instalado, comprobar referencias, colisiones, inicio de escena y controles. Capturar una vista real del resultado cuando sea posible. Guardar instrucciones y límites. No publicar builds ni hacer merge en main; la entrega debe quedar revisable.

## Claude, Codex y Higgsfield

El usuario ha solicitado trabajar con Fable 5.2 y el plugin `openai/codex-plugin-cc`. El plugin instalado se describe como una integración para usar Codex desde Claude Code; no prueba por sí solo que Codex pueda llamar a Claude.

Comprobar la conexión de Higgsfield y la promoción de la cuenta mediante consultas de solo lectura antes de generar. No asumir que tres días de promoción equivalen a créditos ilimitados ni contratar servicios. Reutilizar lo que existe. Preparar una lista de recursos faltantes antes de gastar generaciones; separar referencia 2D, modelo 3D, rig, animación e integración.

## Iteraciones posteriores

- Consolidar la escena y el movimiento con revisión visual del equipo.
- Casa con distribución, ventanas y accesos realmente transitables.
- Torre con escalera continua, barandillas y cabina.
- Interacción con objetos y audio, guardado y progresión.
- Personajes y criaturas solo a partir de diseños y comportamientos aprobados.
- Optimización, iluminación, pruebas de recorrido y builds compartidas.
