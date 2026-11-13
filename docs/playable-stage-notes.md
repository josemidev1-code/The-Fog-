# Staging jugable — 11/11/2026

Escena: Assets/Scenes/PlayableStage.unity. Zona exterior jugable de casa, camino y mina con modelos propios. Conserva FirstPlayable, DanielRoom y la exposición DesktopEnvironment. No tiene muebles expuestos al aire libre. Se mantienen el controlador, linterna, presets gráficos, pausa y recorrido existente. Casa con MeshColliders ajustados a la malla en vez de una caja alrededor del edificio. Esto NO crea un interior ni abre la puerta fusionada: acceso interior y torre jugable siguen pendientes. Segunda casa propia añadida como exterior.

Suelo: superficie provisional identificada en la escena; no es el modelo solicitado terminado. Referencia nueva ENV_SueloRural_A de cuatro vistas, generada con herramienta de imagen integrada, en docs/references; prompt en ground-generation.md. No se envió a Hunyuan ni se gastaron créditos Hunyuan en esta revisión.

Bloqueo de navegador verificado: inventario de herramientas devuelve Local browser use is disabled in Work settings. Inspección directa de Ajustes > Navegador y Uso del ordenador muestra ambos permisos (integrado y Chrome) activados. Discrepancia de conexión, causa no diagnosticada; no se cambió configuración ni cuenta ni se intentó evitar la restricción.

Constructor compiló y guardó la escena tras corregir una referencia de la prueba a un ensamblado de Editor. No añadir lore, diálogos ni torres inventadas. Falta generar el suelo 3D, validar medidas reales, normales, materiales, superficie transitable y uniones entre piezas; luego sustituir la superficie provisional. Interior, suelo y techo de casa, hoja de puerta y torre requieren trabajos posteriores, con recursos propios o generados y validados.

Prueba de Play Mode actual: PlayableStageChecks pasó movimiento, apoyo en suelo, linterna, tres presets gráficos, pausa, cuatro puntos del recorrido exterior, interacción y reinicio. Informe playable-stage-play-mode.json. No cubre el interior ni el futuro suelo Hunyuan.

Actualización posterior: el acceso a Chrome se recuperó. Una generación de suelo a 1,5M completada e integrada; quedan tres usos según último contador. Se retiraron suelo provisional y franjas de camino. Ver ground-integration-notes.md para medidas, textura, pruebas actuales y límites; el bloqueo y suelo pendiente descritos arriba corresponden a la fase anterior.
