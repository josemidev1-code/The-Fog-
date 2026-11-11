# Desarrollo de THE FOG

## Ramas

- `main`: entregas estables revisadas. Actualmente sigue en el commit inicial; no representa todavía la versión Unity avanzada.
- `develop`: integración de Unity, creada desde `c012581` de `codex/daniel-room-menu` el 11/11/2026. Es la base para las siguientes tareas.
- `feature/gameplay-foundation`: primera iteración de jugabilidad.
- Siguientes tareas: `feature/<tarea>`; correcciones: `fix/<problema>`; recursos: `art/<recurso>`.

Crear las ramas de tarea cuando comience su trabajo, siempre desde `develop` actualizado. Abrir PR contra `develop`; pasar a `main` mediante un PR de entrega cuando la escena y el recorrido estén revisados. No fusionar tareas directamente en `main`.

Las ramas `codex/onedrive-unity-integration`, `codex/daniel-room-menu`, `unity/casa-daniel` y `unity/valdara-prototipo` se conservan como historial. Sus PR existentes no se fusionan automáticamente: el trabajo más reciente de Daniel ya forma la base de `develop`, mientras que las dos ramas `unity/*` requieren comparar escenas y código antes de integrar. No mezclar escenas YAML a ciegas.

La copia antigua `The-Fog-review` conserva trabajo local sin commit en `codex/first-playable`. No incorporarlo en bloque: gran parte de ese prototipo ya tiene sucesores en `develop`. Revisar diferencias por archivo si se recupera algo.

## Forma de trabajar

1. Una tarea concreta por rama y PR; indicar escena, comportamiento esperado y límites.
2. Conservar `.meta` y GUID; mantener modelos grandes en Git LFS. No subir `Library`, `Logs`, `Temp` o datos personales.
3. Evitar dos editores trabajando a la vez sobre una misma escena. Preferir componentes y prefabs pequeños.
4. Antes del PR: compilar con Unity 6000.6.8f1, ejecutar las comprobaciones afectadas y revisar los cambios de escena y de configuración.
5. Para una entrega: probar en Play movimiento, colisión, interacción, linterna, pausa, reinicio y la ruta completa; revisar también una build ejecutable. Registrar qué se ha probado realmente.

Esta organización no instala reglas de protección en GitHub ni convierte una escena provisional en una entrega validada.

## Primera iteración de jugabilidad

Objetivo: consolidar explorar → identificar un objeto → examinarlo → continuar, usando los recursos y escenas existentes. Sin añadir historia, enemigos o sistemas de combate no aprobados.

1. **Interacción base:** indicaciones coherentes al mirar y dejar de mirar objetos; bloquear interacciones durante pausa; verificar alcance y obstáculos.
2. **Habitación de Daniel:** examinar las pistas existentes y volver a moverse; terminal y lectura sin entradas duplicadas; conservar el guardado actual.
3. **Exterior `PlayableStage`:** recorrido transitable con colisiones y linterna. El acceso real a casa y torre sigue siendo trabajo pendiente, no se simula como terminado.
4. **Continuidad:** diseñar y verificar el enlace habitación/exterior antes de cambiar de escena; conservar posición y estado solo cuando esté definido el recorrido.
5. **Sensación de juego:** pasos y ambiente, ajustes de movimiento y legibilidad. Medir rendimiento en Bajo/Medio/Alto antes de prometer FPS.

Criterio de cierre: recorrido acordado repetible sin quedarse atrapado, indicaciones correctas, pausa coherente y sin errores de ejecución. La aprobación visual y la prueba manual siguen pendientes hasta realizarlas.
