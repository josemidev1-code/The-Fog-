# Habitación de Daniel — primer bloque jugable

## Abrir
Unity Hub → ProyectoUnity (copia del escritorio) → Unity 6000.6.8f1.
Escena: Assets/Scenes/DanielRoom.unity. Doble clic en la escena y Play.
Es la primera escena de Build Settings. FirstPlayable.unity sigue disponible y no se ha regenerado.

## Comportamiento
El menú vive en la pantalla física del ordenador: THE FOG, Nueva partida, Continuar, Ajustes, Salir.
Nueva partida inicia el turno dentro de la habitación y crea un punto de continuación local.
WASD mueve, ratón mira. E cerca del ordenador abre el terminal. Escape vuelve al terminal.
Continuar recupera la posición de esta habitación. No es un sistema de guardado de la historia completa.
Al volver al terminal o salir se guarda la posición. Nueva partida sustituye este punto de continuación.
Ajustes: Bajo/Medio/Alto, sensibilidad y volumen global. Preferencias persistentes entre sesiones.
El deslizador de volumen está preparado; esta escena aún no tiene audio de ambiente.

## Recursos y límites
Cama, mesita, estantería y silla proceden de los modelos propios de OneDrive, en copias optimizadas.
Los GLB originales permanecen intactos. Informe: docs/room-optimization.json.
El PC, teclado, radio, lámpara y arquitectura son geometría provisional editable.
Revisada la carpeta compartida TheFog el 10/11/2026: 26 modelos, ninguno llamado ordenador/PC/monitor.
Falta sustituir el PC cuando se facilite o encuentre su modelo.
La habitación aún no está conectada al exterior ni tiene misiones, diálogos o cámaras de vigilancia.

## Seguir en clase
Editar DanielRoom.unity directamente; no regenerar la escena para conservar cambios manuales.
Objetos agrupados en HABITACION, MUEBLES y PUESTO DE TELECOMUNICACIONES.
Terminal · menú y sesión expone las referencias del menú y las poses de cámara en Inspector.
PC PROVISIONAL identifica dónde colocar el modelo definitivo, conservando el Canvas de pantalla.
Scripts propios: Assets/Scripts/DanielRoom/DanielTerminal.cs.
Constructor inicial: Assets/Editor/DanielRoomBuilder.cs (rechaza sobrescribir una escena existente).
Comprobación automatizada: TheFog.EditorTools.DanielRoomChecks.Run.
Guardado de usuario: Application.persistentDataPath/daniel-room-checkpoint.json; no se sube a Git.

## Inicio confirmado
El juego es en primera persona. El primer lugar jugable es la habitación de Daniel en su casa; la torre queda para una fase posterior.
Tres objetos examinables con E mirando hacia ellos: cuaderno, parte de guardia y radio. E o Esc cierra la lectura.
Los textos son una primera propuesta ambiental sin revelar el origen de la niebla, no un capítulo completo de historia.
