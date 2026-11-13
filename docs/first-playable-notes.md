# THE FOG — Primera escena jugable: notas de entrega

Entrega de Claude (Fable 5.2) del 10 de noviembre de 2026 sobre la rama `codex/first-playable`.
**Estado: código escrito, sin compilar ni probar.** Claude no tuvo compilador de C# ni Unity; la compilación, la generación de la escena, las capturas y las pruebas las hace Codex con Unity CLI. Nada de lo que sigue está comprobado en ejecución salvo que se indique.

## Qué se ha añadido

| Ruta | Contenido |
|---|---|
| `Assets/Scripts/FirstPlayable/FogInput.cs` | Acciones del Input System, serializadas en la escena y reasignables en el Inspector |
| `Assets/Scripts/FirstPlayable/FogPlayerController.cs` | Jugador: caminar, correr, mirar, gravedad, colisión (`CharacterController`) |
| `Assets/Scripts/FirstPlayable/FogFlashlight.cs` | Linterna: encendido y apagado de la luz focal |
| `Assets/Scripts/FirstPlayable/FogInteractable.cs`, `FogInteractor.cs` | Interacción por rayo desde la cámara |
| `Assets/Scripts/FirstPlayable/FogTestRoute.cs` | Recorrido de prueba provisional: objetivo y completado |
| `Assets/Scripts/FirstPlayable/FogHud.cs` | HUD: controles, objetivo, indicación, mensajes, panel de pausa |
| `Assets/Scripts/FirstPlayable/FogPauseController.cs` | Pausa, captura del cursor, reinicio y salida |
| `Assets/Scripts/FirstPlayable/FogAmbience.cs` | Niebla, luz ambiente y fondo de cámara |
| `Assets/Scripts/FirstPlayable/FogGraphicsSettings.cs` | Presets gráficos Bajo / Medio / Alto |
| `Assets/Editor/FirstPlayableSceneBuilder.cs` | Constructor que genera y guarda `Assets/Scenes/FirstPlayable.unity` |

No se han tocado los 26 GLB originales, sus `.meta` ni `Assets/Models/Optimized`. No se ha hecho commit, push, merge ni cambio de rama. Faltan los `.meta` de los archivos nuevos: los crea Unity al importar.

## Generar la escena

La escena es persistente y editable una vez guardada; el constructor solo hace falta para crearla o regenerarla.

- Por lotes (no usar `-nographics`, las capturas necesitan GPU; el método cierra el editor con código 0 o 1):
  `Unity.exe -batchmode -projectPath <carpeta> -executeMethod TheFog.EditorTools.FirstPlayableSceneBuilder.BuildBatch -logFile <log>`
- En el editor: menú **THE FOG > First Playable > Rebuild scene**. Sobrescribe la escena y pide confirmación.
- Automático: al abrir el editor (no en modo lote), si la escena no existe y la escena abierta no tiene cambios sin guardar, se construye una vez.

El constructor también:

- crea `Assets/Art/FirstPlayable/` (textura de ruido 64×64 con filtro de punto y materiales `Suelo` y `Camino`);
- inserta la escena en la posición 0 de `ProjectSettings/EditorBuildSettings.asset`, necesario para que el reinicio con R funcione; `SampleScene` se conserva;
- escribe `docs/first-playable-build-report.json` con tamaño de origen, escala aplicada, tamaño final y colisionador de cada modelo, más los avisos;
- intenta guardar `docs/first-playable-preview-player.png` (vista del jugador con niebla) y `docs/first-playable-preview-overview.png` (vista elevada sin niebla, para revisar la distribución). Si `Camera.Render` falla, queda como aviso en el informe y no impide guardar la escena.

## Controles

| Acción | Teclado y ratón | Mando |
|---|---|---|
| Moverse | WASD o flechas | Stick izquierdo |
| Mirar | Ratón | Stick derecho |
| Correr (solo hacia delante) | Mayús izquierda | Pulsar stick izquierdo |
| Linterna | F | Botón norte |
| Interactuar | E | Botón oeste |
| Pausa / continuar | Esc | Start |
| Reiniciar escena (en pausa) | R | — |
| Salir (en pausa) | Q | — |
| Gráficos Bajo / Medio / Alto | F1 / F2 / F3 | — |

La pausa detiene el tiempo, libera el cursor y bloquea movimiento, linterna e interacción. Si el cursor se libera sin pausa, un clic lo vuelve a capturar. No hay salto ni agacharse. El mando no está probado y no tiene reinicio, salida ni cambio de preset.

Valores ajustables en el Inspector: velocidad 3,2 m/s andando y 5,2 m/s corriendo, sensibilidad de ratón 0,09 grados por píxel, inversión del eje Y, alcance de interacción 3 m.

## Presets gráficos

Componente `FogGraphicsSettings` en el objeto `JUEGO`. La elección se guarda en `PlayerPrefs` (`TheFog.GraphicsPreset`); por defecto, Medio.

| | Bajo | Medio | Alto |
|---|---|---|---|
| Escala de render | 0,40 | 0,50 | 0,65 |
| Escalado | Punto | Punto | Punto |
| Sombras de la luz direccional | No | Sí, 25 m | Sí, 45 m |
| Sombras de la linterna | No | No | Sí |
| Distancia de cámara | 55 m | 75 m | 95 m |
| Luces de farola | No | Sí | Sí |

- Los tres mantienen la resolución interna reducida y el escalado sin suavizar: la identidad retro no depende del preset.
- Los valores son puntos de partida sin medir. No hay ninguna cifra de FPS ni de compatibilidad comprobada.
- El componente modifica el asset de URP activo (escala, filtro, distancia de sombras) mientras está activo y lo restaura al desactivarse. Si el editor se cierra de forma anómala en Play, revisar `Assets/Settings/PC_RPAsset.asset` antes de confirmar cambios.
- La niebla no cambia con el preset (densidad 0,04, exponencial cuadrática). En Bajo la distancia de 55 m queda dentro de la niebla densa, pero hay que confirmarlo a ojo.
- En Bajo se apagan las farolas, que son los puntos de luz cálida de la dirección artística. Es una decisión a revisar.
- El preset no cambia la calidad de texturas, el antialiasing ni el nivel de calidad de Unity.

## Contenido de la escena

- **Modelos**: casa de Daniel, entrada de mina, roble, arbusto, roca y linterna desde `Assets/Models/Optimized`; valla, farola y cartel desde los originales. Si una copia optimizada no carga, se usa el original y queda un aviso en el informe.
- **Distribución**: jugador en el origen mirando al norte; casa al oeste; camino de 2,4 m de ancho hasta la mina, a unos 50 m; cartel y dos farolas junto al camino; dos tramos de valla; cinco rocas grandes alrededor de la mina; 18 robles, 26 arbustos y 14 rocas repartidos con semilla fija (1989), fuera del camino.
- **Escala**: se normaliza por altura sobre cada instancia, con un tope de huella. Alturas objetivo: casa 6 m, mina 4,5 m, robles 8–11 m, farola 4,2 m, cartel 2,6 m, valla 1,25 m, linterna 24 cm de largo.
- **Colisiones**: caja reducida en casa, mina, cartel, vallas y rocas de más de 0,8 m; cápsula de tronco en robles y farolas; arbustos y rocas pequeñas sin colisión; cuatro muros invisibles en el perímetro (84 × 86 m).
- **Luz**: direccional fría y tenue, ambiente plano gris verdoso, linterna cálida, farolas anaranjadas.
- **Interacción provisional**: examinar la entrada de la mina completa el recorrido de prueba; mirar el cartel muestra un texto. Ambos mensajes llevan la etiqueta `[Provisional]` y no contienen historia.

## Límites conocidos

1. **Sin compilar.** Puede haber errores de compilación o API distintas en Unity 6000.6 / URP 17.6. Puntos a mirar primero: `UniversalRenderPipelineAsset.upscalingFilter` y `shadowDistance`, `Camera.GetUniversalAdditionalCameraData()`, `UnityEngine.UI.Text` con la fuente `LegacyRuntime.ttf`.
2. **Modelos no vistos.** La orientación de la casa (giro de 90°), de la mina (180°) y del cartel es una suposición sobre dónde está su frente. Las alturas objetivo también son estimaciones.
3. **Linterna.** El eje más largo del modelo se alinea hacia delante, pero el sentido (cabeza o culata) puede quedar invertido; se corrige girando `ModeloLinterna` 180°. La luz es independiente del modelo. No hay brazos ni mano.
4. **Casa y mina son carcasas.** Llevan un colisionador de caja completo: no tienen interior jugable y no se puede entrar en la mina. La interacción de la mina es un volumen delante de la caja.
5. **Colisiones aproximadas.** Las cajas se calculan con los límites del modelo, no con su forma. Las copas de los robles no colisionan. El cartel bloquea como un bloque macizo.
6. **Vallas.** Se encadenan según la longitud medida de la pieza; si el modelo no es un tramo recto, la fila puede quedar mal alineada.
7. **Farolas.** La luz se coloca al 90 % de la altura y 0,5 m hacia delante del eje; puede no coincidir con el farol del modelo.
8. **Suelo plano.** No hay terreno con relieve; las rocas grandes solo sugieren la ladera de la mina.
9. **Sin audio, guardado, torre, personajes ni criaturas.** No se ha añadido lore, armas ni enemigos.
10. **Regenerar sobrescribe.** Los cambios hechos a mano en `FirstPlayable.unity` se pierden al regenerar. Para conservarlos, editar el constructor o no volver a ejecutarlo.
11. **Memoria.** Valla, farola y cartel siguen siendo los originales, de 76 a 86 MB cada uno con tres texturas; no están optimizados.
12. **Capturas en modo lote.** Dependen de que `Camera.Render` funcione con URP fuera de Play; no incluyen el HUD.

## Comprobaciones sugeridas

- Compilación sin errores y `docs/first-playable-build-report.json` con `saved: true` y sin avisos.
- En el informe, `appliedScale` y `finalSize` razonables para cada modelo.
- En Play: inicio sin quedar atrapado, recorrido completo hasta la mina, ambas interacciones, linterna, pausa, reinicio, salida y los tres presets.
- Tras salir de Play, `PC_RPAsset.asset` sin cambios.

## Conexiones

| Conexión | Qué se comprobó | Resultado |
|---|---|---|
| Carpeta local | Listado, lectura y escritura de archivos desde la sesión de Claude | Funciona. Sin terminal en el equipo: no se pudo ejecutar git ni Unity CLI |
| Rama | Lectura de `.git/HEAD` | `refs/heads/codex/first-playable` |
| Plugin `openai/codex-plugin-cc` | Lista de plugins de la cuenta y servidores MCP locales del equipo | Ambas vacías. El plugin no estaba disponible en esta sesión; Claude no pudo llamar a Codex. La coordinación fue por archivos y por mensajes del usuario |
| Unity local | Resolución de aplicaciones instaladas | Unity 6000.6.8f1 y Unity Hub instalados. No se abrieron: el control de ventanas no se concedió y Codex se encarga de Unity |
| Higgsfield | Saldo, espacios de trabajo y movimientos, solo lectura | Conectado. Plan `plus`, 110 créditos, un espacio privado |

Movimientos de Higgsfield: 10 créditos de registro y 2 de reinicio diario el 20 de octubre de 2026; 100 créditos de suscripción el 10 de noviembre de 2026 a las 10:22 UTC. No hay ningún gasto. Claude no generó nada ni gastó créditos.

Sobre la promoción: el saldo es finito, no ilimitado. Estas consultas no indican si el plan Plus es una prueba temporal, cuándo termina ni si se renueva con cobro. Eso queda sin verificar y hay que mirarlo en la cuenta antes de generar.

## Recursos que faltan

Lista previa a cualquier generación; nada de esto está encargado.

| Recurso | Referencia 2D | Modelo 3D | Rig | Animación | Integración |
|---|---|---|---|---|---|
| Torre con escalera y cabina | Falta | Falta | No aplica | No aplica | Falta |
| Interior transitable de la casa de Daniel | Falta distribución | Falta | No aplica | Puertas | Falta |
| Interior de la mina | Falta | Falta | No aplica | No aplica | Falta |
| Brazos o mano para la linterna | Falta | Falta | Falta | Falta | Falta |
| Terreno con relieve y camino | Falta | Falta | No aplica | No aplica | Falta |
| Daniel (si llega a verse) | Existe ficha, sin revisar aquí | Falta | Falta | Falta | Falta |
| Criaturas | Sin diseño aprobado | — | — | — | — |
| Audio ambiente y pasos | No aplica | No aplica | No aplica | No aplica | Falta |
| Copias optimizadas de valla, farola y cartel | Existen | Existen los originales | No aplica | No aplica | Falta optimizar |

Los 15 modelos de interior ya existentes no se usan en esta escena porque no hay interior jugable.
