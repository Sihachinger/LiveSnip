# LiveSnip

Captura de pantalla para Windows **que NO congela las pantallas**.

Las herramientas habituales (Win+Shift+S, Herramienta de recortes) sacan una foto de todos los monitores y la muestran congelada para elegir la zona. LiveSnip no, dibuja una capa semitransparente sobre **un unico monitor**, así que todo lo demás (otros monitores, videos, pantallas externas) sigue en vivo.

## Descargar

1. Vaya a [Releases](https://github.com/Sihachinger/LiveSnip/releases/latest).
2. Descargue `LiveSnip.exe`.
3. Ejecútelo. No necesita instalación ni .NET.

Windows SmartScreen puede avisar porque el programa no está firmado, para utilizarlo pulse "Más información" → "Ejecutar de todas formas".

## Uso

1. Ejecute `LiveSnip.exe`. La primera vez se abre la configuración para elegir el atajo, después queda ya en la bandeja del sistema.
2. Pulse el atajo (por defecto **Ctrl + Alt + S**) o haga doble clic en el icono. La capa aparece en el monitor donde está el mouse unicamente.
3. Arrastre para elegir la zona. Al soltar, la captura se **copia al portapapeles** y se **guarda** en `Imágenes\LiveSnip`. Si no se puede escribir ahí (permisos, OneDrive, u otra razon), se prueba `Escritorio\Capturas LiveSnip` y luego `%LOCALAPPDATA%\LiveSnip\Capturas`. El menú del icono tiene "Abrir carpeta" para ir directamente al guardado.

| Tecla | Acción |
|---|---|
| `0` | Aspecto libre |
| `1` | 1:1 |
| `2` | 4:3 |
| `3` | 16:9 |
| `4` | 9:16 |
| `5` | 3:2 |
| `6` | Personalizada (por defecto 21:9) |
| `Esc` o clic derecho | Cancelar |

El aspecto también se puede fijar desde el menú del icono (clic derecho). Este queda guardado para la próxima vez.

## Ejecutar desde el código

Requiere el [SDK de .NET 10](https://dotnet.microsoft.com/download) y Windows.

```bash
dotnet run --project src/LiveSnip
```

## Compilar

```bash
dotnet publish src/LiveSnip -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

Genera `publish/LiveSnip.exe`, un único archivo portable que no necesita instalar nada.

## Pruebas

```bash
dotnet test tests/LiveSnip.Tests
```

Las pruebas (xUnit) cubren la lógica sin ventanas, el cálculo de relaciones de aspecto, la limpieza de nombres de archivo y la configuración. No cubren la interfaz, el atajo global ni el registro de Windows.

## Configuración

Clic derecho en el icono → **Configuración...** para cambiar:

- **Atajo:** haga clic en el campo y pulse la combinación (debe incluir Ctrl, Alt o Shift).
- **Aspecto por defecto.** Al elegir "Personalizada" se habilitan dos casillas para escribir el ancho y el alto como números enteros (de 1 a 32), por ejemplo 21 y 9. Esa relación es la que se usa con la tecla `6`.
- **Carpeta de guardado.**
- **Preguntar el nombre:** si se activa, tras cada captura aparece un cuadro para escribir el nombre (con o sin `.png`). Si se deja vacío o se cancela, se usa la fecha y hora. Si ya existe un archivo con ese nombre, se agrega `-2`, `-3`...
- **Iniciar con Windows:** arranca la app al iniciar sesión (usa la clave `Run` del usuario, no necesita administrador). Si se mueve el `.exe`, hay que abrirlo una vez para que se actualice la ruta.

Se guarda en `%LOCALAPPDATA%\LiveSnip\config.json`.

## Estructura

```
src/LiveSnip/
├── Program.cs                 Punto de entrada y armado de las piezas
├── Nucleo/                    Lógica, sin ventanas
│   ├── Config.cs              Ajustes y su guardado en JSON
│   ├── ModificadorTecla.cs    Constantes de Alt, Ctrl y Shift
│   ├── RelacionAspecto.cs     Relaciones de aspecto y su respectivo cálculo
│   ├── InicioAutomatico.cs    IInicioAutomatico: arranque con Windows
│   ├── CapturadorPantalla.cs  ICapturadorPantalla: saca la imagen de una zona
│   └── AlmacenCapturas.cs     IAlmacenCapturas: carpeta, nombre y guardado del PNG
└── Interfaz/                  Ventanas y bandeja
    ├── AppBandeja.cs          Coordina menú, atajo y flujo de captura
    ├── CapaSeleccion.cs       Capa para elegir la zona
    ├── VentanaConfiguracion.cs  Ventana de configuración
    ├── VentanaNombre.cs       Cuadro para el nombre
    └── VentanaAtajo.cs        Atajo global
tests/LiveSnip.Tests/          Pruebas de la lógica
```

`AppBandeja` recibe el capturador y el almacén por interfaz, así que se puede reemplazar o probar por separado.

Las ventanas se construyen por código, sin el diseñador de Visual Studio. El proyecto se hizo para experimentar realizarlo de esta manera y los formularios son pequeños, lo cúal facilita este fortma de hacerlo.

## Limitaciones

- Solo captura el monitor donde está el mouse al pulsar el atajo.
- Solo Windows.

## Sobre este Remaster

LiveSnip lo he desarrollado originalmente a principios de 2026. Esta versión es una **reescritura realizada en 2026** a partir de la versión final de ese proyecto, aplicando criterios actuales de organización, nombres y estilo.

El comienzo del historial de commits de este repositorio es una **reconstrucción**: ordene el trabajo en pasos lógicos a partir del código final. **No es el historial original**.

## Cambios respecto al original

- **Nombres en español en toda la solución.** Las clases y archivos de `Interfaz/` las habia desarrollado en inglés y ahora siguen el mismo idioma para uniformidad.
- **Constantes con nombre para los modificadores de teclado.** Los valores `0x0001`, `0x0002` y `0x0004` aparecían repetidos como números sueltos, ahora están en `ModificadorTecla`.
- **Comentarios:** se reescribieron y se agregaron algunos comentarios para explicar el flujo y hacer más claro el codigo.
- **Pruebas:** se agregó el proyecto `tests/LiveSnip.Tests` (xUnit 2.9.3, Microsoft.NET.Test.Sdk 17.14.1, xunit.runner.visualstudio 3.1.4). Para experimentar con xUnit.
- **Carpeta de capturas recreada si se borra.** Si se borra con la aplicación abierta, se vuelve a crear al guardar o al usar "Abrir carpeta" (antes el guardado fallaba), y si "Abrir carpeta" falla se muestra un aviso con el motivo.
- **Función nueva:** al tocar el aviso "Copiada y guardada en…" se abre la carpeta de capturas.
- **Función nueva: relación de aspecto personalizada.** El usuario puede escribir su propia relación (ancho y alto enteros) en la configuración, elegirla desde el menú del icono o con la tecla `6`. El original solo tenía las seis relaciones fijas.
- **Aviso si el archivo de configuración está dañado.** Antes se usaban los valores por defecto sin decir nada, ahora se muestra un mensaje antes de abrir la configuración.

Salvo lo indicado en los puntos de la carpeta recreada, las funciones nuevas y el aviso de configuración dañada, el comportamiento de la aplicación es el mismo que el del original.

## Licencia

MIT

## Autor

[Victor Sihachinger](https://github.com/Sihachinger)
