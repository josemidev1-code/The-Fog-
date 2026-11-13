# Abrir THE FOG en el PC de clase

1. Instala Unity Hub, Unity Editor 6000.6.8f1 y GitHub Desktop. Cada compañero usa su propia cuenta de GitHub, con acceso al repositorio.
2. En GitHub Desktop: File → Clone repository → URL → https://github.com/josemidev1-code/The-Fog-.git . Guarda el clon en una carpeta local (mejor fuera de OneDrive).
3. Selecciona la rama codex/daniel-room-menu. Esta es la versión actual; main todavía no contiene el prototipo.
4. Espera a que termine la descarga. Los modelos usan Git LFS. Si ves archivos GLB diminutos o errores de modelos, abre Repository → Open in Terminal y ejecuta:

```powershell
git lfs install
git lfs pull
git lfs fsck
```

5. Unity Hub → Añadir → Añadir proyecto desde el disco → selecciona la raíz del clon, la carpeta que contiene Assets, Packages y ProjectSettings. No selecciones solo Assets.
6. Abre con Unity 6000.6.8f1 y espera la importación. Library se genera de nuevo en cada ordenador y no se descarga de GitHub.
7. En Unity: Assets → Scenes → DanielRoom.unity, doble clic. Pulsa Play → Nueva partida. WASD y ratón; E para examinar objetos o usar el PC, Esc para volver al terminal. Ajustes está en el monitor.

## Compartir el trabajo
Antes de empezar, Unity cerrado: Fetch origin → Pull origin en GitHub Desktop.
Al terminar: guarda escenas y proyecto en Unity (Ctrl+S), cierra Unity, revisa los cambios en GitHub Desktop, incluye los .meta, escribe una descripción → Commit → Push origin.
Antes de cambiar de ordenador comprueba que el último commit se ve en GitHub. Acordad quién edita cada escena para evitar conflictos.
No se sube automáticamente al guardar en Unity. Hace falta Commit y Push.

## Qué se comparte
Se comparten escenas, código, materiales, texturas, los 26 GLB originales, copias optimizadas, el primer módulo Hunyuan y el vídeo de prueba (Assets/Video). El vídeo está archivado como recurso; aún no se reproduce en el juego.
La carpeta del escritorio TheFog se comparó por SHA-256: sus 26 archivos son exactamente los ya incorporados. No se duplicaron.
La habitación es un prototipo editable. PC y lámpara provisionales; falta reemplazarlos por los modelos del equipo. El interior modular está iniciado, no terminado. El exterior está en FirstPlayable.unity y se conserva separado.
Los puntos de continuación de una partida y las preferencias gráficas son locales a cada ordenador: no viajan con GitHub. Esto comparte el desarrollo del proyecto, no las partidas personales.
No copiéis Library, Temp, Logs ni UserSettings. Tampoco hacen falta las cuentas o claves de este ordenador. El acceso de Unity y GitHub se inicia en el PC de clase con las cuentas correspondientes.
