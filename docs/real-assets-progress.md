# Modelos reales — revisión del 11/11/2026

## Copia de trabajo
ProyectoUnity del escritorio; origin: https://github.com/josemidev1-code/The-Fog-.git; rama codex/daniel-room-menu. Se conservan escenas y originales existentes.

## Hecho y comprobado
- OneDrive local no contiene modelos. Los 26 originales previamente descargados coinciden por SHA-256 con el inventario y Desktop/TheFog; sin duplicados. No se revalidó el inventario de nube en vivo. Informe: real-assets-source-audit.json.
- Se sustituyó la mesa provisional de DanielRoom por INT_MesaMadera propio, sin cambiar sus proporciones, con MeshColliders estáticos. Se conservó el menú y demás objetos. El informe real-asset-scene-audit.json identifica los renderizadores provisionales restantes; NO están terminados.
- Se generó una referencia de cuatro vistas y una imagen frontal separada usando la esquina interior existente como guía. Referencias y prompts en docs/references. Generación de imagen integrada, sin API externa de pago configurada.
- Hunyuan V3.1 Image-to-3D PBR, selección 50k: una generación gratuita, contador visible 5 → 4, saldo visible 0. GLB descargado de 50.000 triángulos, original conservado; copia optimizada 15.999 triángulos, texturas limitadas a 1024.
- Medidas REALES del módulo: 3,7526 × 2,6999 × 0,2437 m en Unity. No cumplió el objetivo inicial 3 × 2,7 × 0,15 m. Se preservaron proporciones con escala uniforme; no se afirma que encaje aún en la casa.
- Prefab: Assets/Prefabs/Modules/INT_Valdara_ParedPuerta_A.prefab. Material sin shader de error, MeshCollider estático que respeta el hueco.

## Pruebas actuales
Unity 6000.6.8f1 compiló el cambio. DanielRoomChecks pasó tras cambiar la mesa: menú, ajustes, movimiento por teclado, apoyo en suelo, lectura e interacción y continuación. Informe: daniel-room-checks.json.
DoorwayChecks pasó en Play Mode: altura 2,70 m, gravedad, paso del CharacterController por el hueco en ambos sentidos, bloqueo contra el paño lateral, caída desde 2 m y velocidad por defecto. Se midieron 3,1745 m/s caminando y 5,2000 m/s corriendo, para valores configurados 3,2 y 5,2. Esta prueba usa un suelo INVISIBLE temporal como infraestructura, no como asset artístico.
La habitación tiene su propio valor de caminar (2,2 m/s); no se ha cambiado. Las mediciones anteriores corresponden al controlador por defecto aislado, no al dormitorio ni a todas las escenas.
El primer intento del test nuevo dio velocidad cero porque el test reiniciaba acciones de entrada ya habilitadas. Se corrigió la creación del actor de prueba y el segundo intento pasó. No se alteró el controlador para forzar resultados.

## Licencia
El primer arranque falló por licencia (exit 198). Se abrió Unity Hub y se actualizó la sesión con la licencia Personal ya existente. Unity volvió a compilar y ejecutar Play Mode. Sin compras, cambios de plan ni aceptación nueva de términos.

## Límites y siguiente trabajo
La pared con hueco es un nuevo módulo reutilizable validado, no una casa completa conectada. No se ha añadido una puerta móvil: faltan hoja real, pivote y comprobación de su barrido. Exterior ENV_CasaDaniel, dormitorio y módulo siguen separados. Revisar distribución y abrir la malla exterior conservando su diseño antes de conectar interior y exterior.
PC, teclado, radio, lámpara, arquitectura del dormitorio, suelo y techo siguen provisionales; no se presentan como modelos terminados. Sustituirlos usando entregas reales o nuevos modelos generados y validados. No se han verificado nuevas aprobaciones de Discord; referencias nuevas son propuestas, no lore ni arte aprobado.
La auditoría de la casa en Blender no prueba habitabilidad. Es una malla de 100.000 triángulos; los contadores de aristas partidas por UV no prueban defectos físicos por sí solos.

## Repetir
- TheFog.EditorTools.RealAssetPass.Run: migración idempotente de mesa y auditoría; no regenera la habitación.
- TheFog.EditorTools.RealAssetPass.CaptureDesk: captura de revisión; no guarda cambios de cámara.
- TheFog.EditorTools.DanielRoomChecks.Run: regresión de habitación.
- TheFog.EditorTools.DoorwayChecks.Run: importa/prepara prefab y prueba aislada; sale al terminar, con límite de 1.200 fotogramas.
- tools/inspect_house.py: inspección de malla en Blender.
- tools/prepare_doorway.py: prepara y evalúa una descarga en docs/candidates antes de importarla.
