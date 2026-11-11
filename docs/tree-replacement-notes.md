# Árboles externos en PlayableStage — 11/11/2026

Solicitud del usuario: buscar árboles 3D mejores en internet, autorizando esta excepción al inventario propio.

Se sustituyeron 18 instancias activas por tres variantes reales FBX (tree01, tree02, tree24) del Retro PSX Style Tree Pack de Elegant Crow: https://elegantcrow.itch.io/psx-retro-style-tree-pack

Cada variante tiene 52 triángulos: tronco y follaje mediante planos con textura y transparencia recortada, técnica habitual de este estilo. No son árboles volumétricos detallados ni imágenes de referencia colocadas como assets terminados. Texturas originales, filtro Point, material URP Lit de doble cara, alpha cutoff 0.4 y tinte apagado. Escala ajustada a la altura previa (aproximadamente 8–11 m). Cada instancia usa una cápsula de tronco de radio 0.28 m y altura 2.4 m; las hojas no bloquean al jugador.

Los árboles anteriores y sus modelos originales se conservan: instancias desactivadas bajo ÁRBOLES ANTERIORES · desactivados. Solo cambia PlayableStage, sin rehacer las otras escenas. Los prefabs están en Assets/Prefabs/Trees y las fuentes externas en Assets/ThirdParty/ElegantCrowTrees.

El autor declara los modelos CC0 y explica la procedencia de las texturas (CC0Textures y principalmente imágenes de Pixabay transformadas). Se conserva esta precisión en SOURCE_AND_LICENSE.txt, sin atribuir CC0 a las fotos originales por cuenta propia. docs/tree-sources.json conserva hashes y procedencia. El sampler PSX Forest de heyheythere fue comparado y descartado visualmente; no se incluyó en Unity.

Validación: importación Unity 6000.6.8f1; capturas trees-after.png y tree-detail.png revisadas (texturas presentes, follaje recortado); Play Mode automatizado aprobado con 15 comprobaciones y cero fallos, incluido recorrido con colisiones. Informe: tree-stage-play-mode.json. Esta prueba no mide FPS ni certifica colisión contra cada tronco. La adecuación artística queda para revisión del usuario; no se afirma aprobación de Discord.

No hubo pagos ni generaciones Hunyuan. La casa completa, puerta practicable y torre siguen pendientes según la documentación anterior.
