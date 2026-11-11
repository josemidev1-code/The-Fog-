# THE FOG — Unity

Proyecto Unity 6000.6.8f1 (URP), preparado para compartir los modelos de OneDrive mediante Git LFS.

Guía para otro ordenador: [ABRIR EN CLASE](docs/ABRIR_EN_CLASE.md). Rama actual: `codex/daniel-room-menu`.

## Abrir en este equipo

1. En Unity Hub, entra en **Projects → Add → Add project from disk**.
2. Selecciona la carpeta `ProyectoUnity` del escritorio (o la raíz del repositorio clonado), la que contiene `Assets`, `Packages` y `ProjectSettings`.
3. Usa **Unity 6000.6.8f1**, ya instalado en este equipo. Espera a que termine la importación de paquetes y modelos.
4. Abre `Assets/Scenes/DanielRoom.unity` y pulsa Play: menú dentro del PC y habitación de Daniel.
5. Nueva partida entra a la habitación. WASD y ratón para moverte; E junto al PC o Esc para volver al terminal. Detalles y límites en `docs/daniel-room-notes.md`.

Los GLB se importan con el paquete oficial Unity glTFast, fijado a 6.20.0. Sus materiales y texturas permanecen dentro de los originales. Documentación: https://github.com/Unity-Technologies/com.unity.cloud.gltfast/blob/main/Packages/com.unity.cloud.gltfast/Documentation~/ImportEditor.md

## Abrir desde otro equipo

Instala Git LFS y Unity Hub con el editor 6000.6.8f1. Después:

```powershell
git lfs install
git clone --branch codex/daniel-room-menu https://github.com/josemidev1-code/The-Fog-.git
cd The-Fog-
git lfs pull
git lfs fsck
```

Añade esa carpeta desde Unity Hub. Necesitarás descargar aproximadamente 2,1 GB de modelos y espacio adicional para la caché de importación. Los archivos GLB deben ser binarios reales, no pequeños archivos de texto de punteros LFS.

## Contenido y procedencia

- `Assets/Models/Environments`: 10 modelos de escenarios.
- `Assets/Models/Interiors`: 15 modelos de mobiliario e interiores.
- `Assets/Models/Props`: una linterna.
- `Characters` y `Creatures`: reservadas, sin modelos disponibles en esta descarga.
- `docs/asset-inventory.json`: nombres originales, tamaño, SHA-256, geometrías, materiales, imágenes y dependencias de cada modelo.
- `docs/unity-import-validation.json`: resultado de la comprobación real con Unity.

Se descargaron los 26 archivos disponibles en la carpeta compartida TheFog de OneDrive el 10 de noviembre de 2026. El ZIP conserva sus originales en Descargas. Se verificaron la integridad ZIP, cabeceras GLB y dependencias: 26 hashes distintos, 74 imágenes integradas y ninguna referencia a archivos externos.

El repositorio de destino solo contenía README y reglas de exclusión antes de esta rama (commit `0f6fd5d8aae0ea6d855de1452858ec92b12fce50`). Por ello se incorporó la plantilla oficial URP Blank 17.2.6 incluida con el editor instalado. No se modificó el proyecto privado `My-project`, ni la copia antigua local de Godot `THE-FOG`, ni el proyecto local `UnityCheck`. La rama incluye la integración de recursos y un prototipo nuevo en Unity; no convierte escenas ni sistemas de Godot a Unity.

## Colaboración

Conservad los `.meta` de Unity junto a los assets para mantener sus referencias. No subáis `Library`, `Temp`, `Logs` o `UserSettings`. Los modelos binarios están configurados con Git LFS; todos los colaboradores deben tenerlo instalado. Los nombres originales, incluidas sus erratas, se conservan para poder rastrear cada entrega.

La validación comprueba que los 26 modelos se importan como GameObject, contienen mallas y tienen materiales sin el shader interno de error. No sustituye una revisión artística, de escala, colisiones, rendimiento o navegación del juego.

Revisión de modelos reales y pruebas del 11/11/2026: [progreso y límites](docs/real-assets-progress.md). Mesa propia integrada en DanielRoom; módulo de pared con hueco de puerta generado y probado como prefab independiente. Casa completa y puerta móvil pendientes.

Entorno con los 26 modelos del escritorio: Assets/Scenes/DesktopEnvironment.unity. Exterior y zonas abiertas de revisión de mobiliario; no interiores terminados. [Estado y pruebas](docs/desktop-environment-notes.md).

PlayableStage.unity incorpora ahora suelo 3D generado en Hunyuan, con original de 1,5M conservado y copia jugable validada. [Proceso, pruebas y pendientes](docs/ground-integration-notes.md).

PlayableStage: 18 árboles sustituidos por tres variantes externas de Elegant Crow, originales conservados. [Fuentes, licencia y pruebas](docs/tree-replacement-notes.md).
