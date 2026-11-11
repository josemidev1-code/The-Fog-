# The Fog — Dirección del primer tramo

## Alcance actual
Prototipo en Unity. Daniel comienza en su dormitorio. El recorrido de trabajo es dormitorio → salón/cocina → exterior de la casa → senda de Valdara → entrada de la mina. La tienda y vigilancia son desvíos de exploración. No se considera terminado el mapa.

## Criterio visual
La casa original marca la referencia. Yeso envejecido, madera oscura, crema sucio, oliva y latón apagado. Se conserva la geometría y textura de los GLB originales de Hunyuan. Los nuevos recursos tienen que funcionar juntos en el escenario, con escala humana y accesos despejados.

La interfaz usa la marca propia de The Fog y IBM Plex Mono. Pausa, objetivos y documentos comparten paleta y tipografía. Los textos narrativos se dibujan en Unity para que sigan siendo legibles y editables.

## Recurso integrado en esta iteración
`Assets/TheFog/Resources/Daniel/PapelNota.jpg`: textura generada con Gemini, modo Pro e Imágenes, 11/11/2026. Conversación `https://gemini.google.com/app/e8e6de3b15c9a449`. Se utiliza para las hojas físicas y el fondo de lectura. El texto se superpone en Unity. Imagen original conservada, el margen blanco se excluye mediante coordenadas de textura.

## Cambios de esta iteración
- Papel visible sobre la mesa y en la nevera, asociado a sus interacciones existentes.
- Lectura con espacio calculado según el texto y contraste oscuro sobre papel claro.
- Objetivos y avisos con altura calculada para evitar cortes.
- Pausa con estilo coherente con la pantalla de inicio.
- Suelo y camino principal de Valdara con los materiales de tierra de Daniel.
- Carteles del mapa con la misma familia tipográfica.
- Aviso de cierre de la galería como documento narrativo. La nota de Daniel funciona como apunte personal del protagonista.

## Siguiente prioridad
Recorrer a pie cada acceso y revisar colisiones y escala. Resolver el modelo definitivo de vigilancia antes de vestir su zona. Completar puerta, lámpara, ventana, radio y tablón a partir de las fichas de referencias 3D, evitando gastar generaciones en objetos ya disponibles. Sonido ambiental y eventos deben apoyar la exploración sin contradecir el GDD del equipo.

## Revisión reproducible
En CasaDaniel, menú The Fog → Revisar lectura y pausa. Comprueba ambos documentos, apertura/cierre de los muebles y suelo del camino de Valdara en cuatro puntos. Guarda resultados y capturas en `docs/prototype-review`. Esto no sustituye una prueba completa del recorrido con teclado y ratón.

## Revisión de tipografía
La fuente actual es provisional. El responsable del proyecto pide mejorar la tipografía. Esta es la siguiente prioridad visual, incluida en el registro de avances del 11/11/2026.
