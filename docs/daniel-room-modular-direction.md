# Sustituir la habitación provisional por módulos del equipo

Decisión del usuario: juego en primera persona, inicio en la habitación de la casa de Daniel. Casa, PC, lámpara y ambiente deben seguir los modelos propios; los bloques provisionales actuales no representan la dirección artística final.

## Recursos existentes
- ENV_CasaDaniel.glb: mantener su identidad exterior; la versión usada hasta ahora no ofrece un dormitorio jugable preparado.
- INT_Cama, INT_MesitaNoche, INT_Estanteria, INT_SillaMadera, INT_MesaMadera y resto del mobiliario: reutilizar modelos originales o copias optimizadas, no regenerarlos.
- En las 26 entregas de OneDrive revisadas no aparecen PC, monitor, lámpara de mesa ni radio. Localizarlos si existen en otra entrega antes de sustituirlos por diseños nuevos.

## Generación pendiente: interior modular compatible
Herramienta confirmada por el usuario: Hunyuan 3D.
Acceso oficial: https://hy3d.tencent.ai/ . El usuario completó el acceso. Se inició una primera generación gratuita de pared modular con 3DGeneration-V3.1, PBR y objetivo de 50k caras. El contador visible pasó de 5 a 4 generaciones diarias; saldo de pago visible: 0. Sin compras.

Crear módulos separados para el interior de la casa, usando referencias de ENV_CasaDaniel y del mobiliario propio. No generar una habitación cerrada con muebles fusionados.

Primera pieza propuesta: pared interior recta, 3 m de ancho por 2.7 m de alto, 0.15 m de grosor. Madera rural envejecida y yeso húmedo apagado; oliva, marrón y gris verdoso. Estética retro de terror rural, geometría sencilla, textura legible, desgaste moderado, sin iluminación ni sombras horneadas fuertes. Una sola pared aislada, sin suelo, sin techo, sin mobiliario, sin personas, sin texto, sin fondo escénico. Caras interiores válidas, escala coherente, sin huecos ni decoraciones flotantes. Exportar GLB con texturas.

Después: variante con hueco de ventana, variante con hueco de puerta, suelo y techo independientes. Comprobar medidas y uniones en Blender/Unity antes de generar el siguiente lote.

Los prompts son una propuesta inicial, no recursos generados ni aprobados visualmente.

## Integración
- Guardar originales generados en Assets/Models/Interiors/Generated; copias optimizadas separadas.
- Revisar normales, escala, UV, texturas y colisiones. Evitar geometría que encierre al jugador o bloquee puertas.
- Mantener DanielTerminal y su Canvas: recolocar el Canvas dentro del monitor del modelo definitivo, conservar botones y transición a primera persona.
- No alterar la escena abierta mientras el usuario pueda estar editándola sin comprobar primero sus cambios.
- No contratar planes ni consumir créditos de pago sin un límite autorizado.


## Resultado de la primera generación
Se descargó un GLB PBR con una esquina, no una pared plana: INT_Valdara_Esquina_A.
Original preservado en Assets/Models/Interiors/Generated. Incluye tres imágenes integradas.
Copia optimizada en Assets/Models/Optimized: 11.568 triángulos. Se eliminaron 1.799 triángulos de marcas de medida generadas por error, sin modificar el original.
Prefab reutilizable: Assets/Prefabs/Modules/INT_Valdara_Esquina_A.prefab, con MeshCollider estático.
Medidas comprobadas por Unity: 3.2265 m ancho, 2.6999 m alto y 1.1336 m fondo (incluye el retorno).
Material importado sin shader de error. Informe: docs/hunyuan-unity-validation.json.
La escena DanielRoom permanece igual: todavía no se ha construido el conjunto modular ni sustituido el PC provisional.
Se utilizó una generación gratuita; quedan cuatro según el contador de la web. Sin compras.
