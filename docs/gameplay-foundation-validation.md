# Verificación de la base de jugabilidad — 11/11/2026

Unity 6000.6.8f1: importación y compilación completadas, salida 0. `git lfs fsck` y `git diff --check` correctos.

Se ejecutó `TheFog.EditorTools.PlayableStageChecks.Run` dos veces sobre `PlayableStage`. Pasaron las 15 comprobaciones de módulos, apoyo en suelo, movimiento, linterna, tres presets, pausa, bloqueo de movimiento, recorrido, interacción y reinicio.

**Resultado global fallido en ambos intentos:** `ArgumentOutOfRangeException` en `UnityEditor.Search.SearchDatabase.EnumerateAll`, invocada desde `SearchInit.IndexationOnStartup`. El capturador registra correctamente esta excepción del editor y no se ha silenciado. Informe en `gameplay-foundation-smoke-test.json`. Resolver la indexación del editor antes de considerar verde la suite completa.

La prueba existente cubre la ruta y las regresiones básicas; no comprueba específicamente un cambio dinámico del texto ni un objetivo destruido durante su interacción. Esos dos casos requieren verificación adicional, junto con revisión manual y build ejecutable. No se da por terminada la iteración de jugabilidad.

Se restauraron los cambios incidentales de configuración y metadatos que Unity produjo al importar. No se modificaron escenas en este PR.
