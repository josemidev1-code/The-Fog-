# Puesto de Daniel — primera versión jugable

Escena: `Assets/Scenes/PuestoDaniel.unity` (primera escena de Build Settings).
Ejecutable de prueba: `Builds/PuestoDaniel/TheFog.exe` (no se sube a Git).

## Qué es
Turno de noche en la cabina de la torre de vigilancia de Valdara, de 22:00 a 06:00 (unos 11 minutos reales).
Daniel vigila cuatro cámaras del pueblo y registra todo lo que cambia. Si aguanta hasta el amanecer, gana:
la luz del sol frena la niebla. Si la niebla llega al 100 %, pierde.

## Controles
| Acción | Teclado / ratón | Mando |
|---|---|---|
| Moverse / mirar | WASD + ratón (Mayús: paso rápido) | sticks |
| Usar | E (mantener en el panel eléctrico) | A |
| Linterna | F | Y |
| Consola: cambiar cámara | A/D, ←/→ o 1-4 | LB/RB |
| Consola: registrar anomalía | Espacio o clic | X |
| Levantarse de la consola | E o Esc | B |
| Pausa | Esc (R reiniciar, Q salir) | Start |
| Pausa: volumen / sensibilidad | ←/→ y ↑/↓ | cruceta |

## Sistemas
- **Cámaras**: MINA (norte), CASA RURAL (este), ENTRADA (sur) y CASA DE DANIEL (oeste). Se ven en el monitor y por las ventanas: es el mismo mundo.
- **Anomalías**: 16 en total, 4 por lugar (farola que se apaga, luz en una casa vacía, niebla que brota, valla o cartel derribados, linterna ajena, resplandor en la mina). La pestaña de la cámara parpadea con `≈` unos segundos cuando empieza algo, y a veces avisa un vecino por radio.
- **Niebla**: sube con el tiempo, el triple de rápido sin luz. Cada anomalía sin registrar suma un 13 %, cada falsa alarma un 5 %, cada registro correcto resta un 3,5 %. Se ve en la densidad real de la niebla, los bancos que se acercan a la torre, la viñeta y el latido.
- **Electricidad**: el diferencial salta hacia las 03:00 y puede saltar al azar. Sin luz no hay cámaras. Se rearma manteniendo E en el panel junto a la puerta.
- **Linterna y pilas**: está en la mesa. Se buscan pilas en la estantería, el armario y bajo el catre. La mesita y la nevera tienen otras cosas.
- **Radio y notas**: mensajes por horas (relevo, vecinos y "operaciones"), parte del turno anterior, plano de cámaras, comunicado de la empresa y libreta de Daniel. No revelan el origen de la niebla.
- **Ajustes**: volumen general (40 % por defecto) y sensibilidad del ratón, en la pausa; se guardan entre partidas (`PuestoSettings`). No hay volumen por categorías.
- **Finales**: al amanecer, con tres valoraciones del informe; o por la niebla. R repite el turno.

## Modelos
Solo modelos de la carpeta del equipo (copias optimizadas en `Assets/Models/Optimized`, originales intactos): mesa, silla, estantería, armario cerrado, cama, mesita, nevera, linterna, entrada de mina, casa rural, casa de Daniel, cartel, farola, rejilla, valla, roble, arbusto y roca (18). Lista exacta en `docs/puesto/construccion.txt`.
No existe un modelo de torre ni de panel eléctrico, así que esas piezas son geometría simple (cubos) con materiales propios en `Assets/Art/Puesto`. Son los primeros candidatos a sustituir por modelos del equipo.

### Modelos generados con Higgsfield (13/11/2026)
Nueve GLB generados (concepto con GPT Image 2.6, malla con Tripo H3.1), copiados a `Assets/Models/Optimized` con prefijo `PROP_` o `ENV_..._01`. Originales, texturas y conceptos en `Desktop/THE FOG/AssetsHiggsfield`.
- **Cabina:** `PROP_Monitor_CRT_01` y `PROP_Radio_Analogica_01` sustituyen a los cubos del monitor y la radio. La interacción sigue en una caja invisible (`Monitor`, `Radio`), así que consola, radio y prueba automática no cambian. `PROP_Radar_Parasitos_01` está sobre la mesa como atrezo, sin mecánica.
- **Pueblo (grupo `Atrezo generado`, sin anomalías):** `ENV_Caseta_Vigilancia_01` y `PROP_Farola_01` al pie de la torre; `ENV_Casa_Rural_01`, tres tramos de `ENV_Muro_Rural_01` y `PROP_Porton_Metalico_01` junto al camino sur; `PROP_Senal_Valdara_01` en la entrada.
- Estos modelos tienen el frontal hacia -X y el pivote en la base: usar `Facing(desde, hacia)` para orientarlos. `Model()` respeta ahora el desplazamiento propio del GLB.
- Son mallas de IA a partir de una sola imagen: las caras no vistas son lisas y cada modelo es una única malla (no se pueden abrir puertas sin separarlas).
- Sin usar todavía: `ENV_Fachada_Modular_01` y `PROP_Ventana_Persiana_Reja_01` (necesitan un volumen de edificio detrás).

### Modelos de Poly Haven (13/11/2026)
Veinte modelos de polyhaven.com, licencia CC0 (uso libre, sin atribución), en glTF con texturas 1K dentro de `Assets/Models/Optimized/PolyHaven/<nombre>/`. `Model()` los busca también ahí.
- **Cabina (grupo `Atrezo PolyHaven`):** `power_box_01` sustituye el cubo del panel eléctrico (el cubo sigue como volumen invisible de interacción; palanca y piloto siguen siendo cubos). `wall_clock`, `desk_lamp_arm_01`, `clipboard`, `binoculars`, `retro_multimeter` (mesita) y caja de herramientas (junto al panel) son atrezo sin mecánica.
- **Cámaras:** `security_camera_01` sustituye el cubo de las cuatro cámaras exteriores.
- **Pueblo (grupo `Atrezo PolyHaven`):** cuadro eléctrico, cubo de basura, bidón y caja junto a la caseta; coche tapado, neumático, dos sillas de plástico, bombona y escalera junto a la casa generada.
- **Recortes hechos a mano** (archivos `PH_*.gltf`): cubo de basura (versión oxidada) y caja de herramientas (sin bandeja). La puerta de `power_box_01` se ha cerrado quitando su giro.
- Probados y retirados por incoherentes: postes de luz (sin cables), valla metálica (tramos sueltos), barreras de hormigón.

### Suelo (13/11/2026)
Texturas CC0 de Poly Haven a 2K en `Assets/Art/Puesto/Suelo`: `grass_ground` (terreno) y `gravel_road` (caminos), color y normales. Cada tramo de camino tiene su propio material para que el mosaico no se estire. La grava original es rojiza y se ha enfriado con el tinte del material.

### Ambientación (13/11/2026)
- **Tipografías** en `Assets/Art/Puesto/Fuentes` (con sus licencias): Barlow Condensed (interfaz y títulos, OFL), VT323 (pantalla de cámaras, OFL) y Special Elite (notas, intro y final; Apache 2.0). Se asignan a `PuestoHud` desde el generador; si falta alguna se usa la de Unity.
- **Textos** de interfaz, avisos y órdenes reescritos con voz de parte de vigilancia. Notas, mensajes de radio e intro no se han tocado: no hay historia nueva.
- **Cabina:** paredes más frías, bombillas de techo a media potencia y un foco cálido en el flexo (`LuzFlexo`, se apaga con el diferencial). Atrezo nuevo de Poly Haven: termo, tabaco, lata, archivador (catre), hervidor (nevera) y botas (junto a la puerta).
- **Maleza** de Poly Haven en lugar de los arbustos low-poly: 220 piezas dispersas a menos de 65 m y unas 146 en las cunetas de los caminos (arbustos, matas, hierbas, ortigas, ramas, tocones, rocas con musgo). Las hojas llevan la transparencia horneada en `*_diff_1ka.png`. Sin sombras para no cargar el rendimiento.

### Árboles secos (13/11/2026)
No se encontraron árboles realistas, gratuitos y ligeros (los de Poly Haven tienen millones de triángulos), así que se generan por código: `TreeMesh` crea seis variantes sin hojas (tronco y ramas como tubos que se bifurcan, 1.700-2.800 triángulos) guardadas en `Assets/Art/Puesto/Arboles`. `Scatter` coloca 46 (`TreeCount`). La corteza reutiliza la textura del tocón de Poly Haven.

### Limpieza de coherencia (13/11/2026)
- Sin robles low-poly: desentonaban. Sustituidos por los árboles secos y la maleza de arriba.
- Un solo cartel de Valdara: el de la anomalía de ENTRADA es ahora `PROP_Senal_Valdara_01`, mirando a la cámara.
- Retirada la farola generada junto a la caseta (ya había una del camino a dos metros).

No se usan los árboles de terceros, la electrónica de Animimo ni los módulos de Hunyuan.

## Regenerar y probar
- Desde Unity: menú **THE FOG → Construir Puesto de Daniel** (sobrescribe la escena: los cambios manuales se pierden; para ajustar a mano, editar la escena y no regenerarla).
- Por línea de comandos (con Unity cerrado):
  `Unity.exe -batchmode -projectPath . -executeMethod TheFog.EditorTools.PuestoDanielBuilder.RunAndBuild -quit`
- Prueba automática de juego dentro del ejecutable:
  `Builds/PuestoDaniel/TheFog.exe -puestoAutotest -puestoOut "<ruta absoluta>/autotest" -screen-fullscreen 0`
  (con ruta relativa las capturas no se guardan). Escribe `autotest/informe.json` y capturas. Último resultado (13/11/2026, con modelos generados, Poly Haven y ajustes de sonido): 24/24 comprobaciones superadas, ~900-1000 FPS de media a 1280×720 en una RTX 5080. Una ejecución anterior del mismo día dio 23/24 (falló "Sobrevivir hasta las 06:00 gana") y no se repitió: posible prueba inestable, sin investigar. Esa cifra no sirve como medición para PCs modestos.

## Código
`Assets/Scripts/Puesto/` (un componente por archivo) y `Assets/Editor/PuestoDanielBuilder.cs`.
El ajuste del juego está en el Inspector del objeto `SISTEMAS/Partida`: duración del turno, ritmo y penalizaciones de anomalías, mensajes de radio.

## Pendiente
- Sustituir la arquitectura provisional de la torre por modelos. El teclado de la consola y la palanca del panel siguen siendo cubos.
- Los árboles son secos y generados por código; si se quieren árboles con hoja hará falta otra fuente.
- Dar una mecánica al radar de parásitos y comprobar en partida que el atrezo nuevo del camino sur no tapa las anomalías de la cámara ENTRADA.
- Medir rendimiento en un PC modesto y añadir presets Bajo/Medio/Alto a esta escena.
- Audio sintetizado por código (zumbido, viento, estática, golpes, latido): sustituir por sonido definitivo.
- Ajustar la dificultad con partidas reales. Por ahora solo se ha probado de forma automatizada.
