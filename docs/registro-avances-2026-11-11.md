# THE FOG — Estado del prototipo y registro de avances

Fecha: 11 de noviembre de 2026
Estado: prototipo en desarrollo. La tipografía actual sigue siendo provisional.

## Dirección del juego
La casa original de Daniel es la referencia visual. Se busca terror rural de estética PS1/PS2, con yeso envejecido, madera oscura, crema sucio, oliva y latón apagado. Los modelos originales de Hunyuan conservan su geometría y texturas. Los recursos nuevos deben respetar materiales, escala y colocación funcional.

El recorrido previsto para el primer tramo es dormitorio → salón/cocina → exterior de Daniel → senda de Valdara → mina. Tienda y vigilancia funcionan como zonas de exploración. El mapa completo, su ritmo y sus colisiones aún requieren revisión.

## Base implementada
- Pantalla de inicio situada dentro del dormitorio de Daniel, con una marca propia de The Fog y botón Jugar.
- Movimiento en primera persona, interacción con E, linterna con F y pausa con Esc.
- Casa amueblada con modelos originales. Se retiraron de la escena de Daniel elementos provisionales que no encajaban con la dirección visual.
- Transiciones con fundido entre exterior e interiores separados. Viaje entre la casa y Valdara.
- Recogida de linterna y acceso a la mina condicionado a tenerla.
- Muebles interactivos con variantes abiertas y cerradas.
- Tienda, recorrido hacia la mina y exposición de modelos reservada para los créditos al completar el tramo del prototipo.

## Cambios de la última iteración
### Documentos e interfaz
Se generó una textura de papel envejecido con Gemini en modo Pro e Imágenes. Está integrada como fondo de lectura y como papel físico sobre la mesa y en la nevera. El texto se dibuja en Unity para conservar legibilidad y permitir correcciones. La imagen original se conserva y su margen blanco se excluye con coordenadas de textura.

La altura de objetivos, avisos y textos de lectura se calcula según su contenido. Se mejoró el contraste de lectura y se unificó la presentación de la pausa con la paleta del juego. La nota de Daniel pasó a ser un apunte personal coherente con su papel de protagonista.

### Mapa y ambientación
Se aplicaron los materiales de tierra de Daniel al suelo y al camino principal de Valdara. Se retiraron los letreros flotantes con instrucciones de los accesos. Las señales de dirección recibieron soporte de madera y se ajustó la colocación de la señal hacia la mina.

El cartel de fin de tramo de la mina pasó a ser un aviso de cierre narrativo que se puede leer. Su contenido es una propuesta provisional de ambientación y debe mantenerse alineado con el GDD del equipo.

## Tipografía — pendiente prioritaria
Comentario del responsable del proyecto: la dirección mejora, pero falta una tipografía más adecuada.

IBM Plex Mono es la fuente provisional actual. Queda pendiente escoger y probar una combinación que encaje mejor con el terror retro, diferenciando menú, indicaciones y documentos cuando sea útil. Debe conservar tildes, ñ y signos españoles, así como legibilidad a la resolución del juego. Revisar también los carteles del escenario. No se considera aprobada la tipografía definitiva.

## Comprobaciones realizadas
- Compilación de los scripts del juego correcta.
- Comprobación de diferencias sin errores de formato.
- Revisión en Unity de las dos notas y sus capturas.
- Apertura y cierre del fregadero y la alacena correctos.
- Existencia de papel físico para ambas notas.
- Existencia de suelo en cuatro puntos del camino principal de Valdara, z = -30, 0, 33 y 46.

Resultado de la revisión automatizada: PASS. Estas comprobaciones no equivalen a recorrer completamente el mapa con teclado y ratón, ni verifican todas las colisiones, accesos o el ritmo de juego.

## Próximas prioridades
1. Mejorar y validar la tipografía.
2. Recorrer a pie el tramo completo y corregir colisiones, escala y orientación.
3. Resolver la torre de vigilancia definitiva. La torre actual continúa siendo provisional.
4. Completar los recursos de interior que faltan, contrastándolos con referencias-3d: puerta, lámpara, ventana, radio y tablón.
5. Incorporar sonido y eventos después de consolidar el recorrido y contrastar la narrativa con el GDD.

## Organización de Discord
Se crearon avances, errores-y-pruebas y mapas-y-jugabilidad dentro de Desarrollo. Referencias-3d conserva la inspiración, arte-y-modelos-3d el trabajo visual y entregas los materiales listos para revisar. El resumen anterior con captura de inicio y logotipo ya está publicado en avances.

## Evidencias adjuntas
- TheFog-Nota-integrada.png: captura real del panel de lectura en Unity.
- TheFog-Pausa.png: captura real de la pausa en el dormitorio.
- TheFog-Camino-mina.png: vista de trabajo del acceso a la mina, con vigilancia provisional.
- Resultados-revision.txt: salida de la comprobación del prototipo.

Los cambios están en el proyecto local de Unity. Este registro y su publicación en Discord no implican que se hayan subido los cambios del proyecto a GitHub ni que exista una nueva versión de Windows distribuida.

## Estado comunicado al equipo
La beta va cogiendo forma. Aún quedan por implementar más modelos 3D, más decoración, una tipografía mejor y más pulido visual y jugable. Los avances mostrados son una base en desarrollo.
