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
4. Para capturar de nuevo **la misma zona** sin volver a arrastrar, pulse **Ctrl + Alt + R** o elija "Repetir última captura" en el menú del icono. La zona se recuerda mientras la aplicación esté abierta.
5. Si se abre LiveSnip cuando ya está abierto, se muestra la ventana de configuración de la copia que ya está en ejecución.
6. Para abrir la captura más reciente, elija "Abrir última captura" en el menú del icono. Se abre en el visor de imágenes de Windows.

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
- **Atajo para repetir la última captura:** se elige igual que el anterior (por defecto Ctrl + Alt + R). No puede ser igual al atajo principal.
- **Aspecto por defecto.** Al elegir "Personalizada" se habilitan dos casillas para escribir el ancho y el alto como números enteros (de 1 a 32), por ejemplo 21 y 9. Esa relación es la que se usa con la tecla `6`.
- **Oscuridad de la capa de selección:** deslizador de 0 a 90 % (por defecto 40 %). Cuanto más alto, más oscuro el fondo negro que aparece al capturar, y con 90 % queda casi negro. El marco verde y las medidas no se ven afectados.
- **Cada captura se:** elige a dónde va cada captura, "Copia y se guarda" (por defecto), "Solo se copia" o "Solo se guarda". Con "Solo se copia" no se pregunta el nombre ni se escribe nada en el disco, y con "Solo se guarda" no se toca el portapapeles.
- **Carpeta de guardado.**
- **Preguntar el nombre:** si se activa, tras cada captura aparece un cuadro para escribir el nombre (con o sin `.png`). Si se deja vacío, se usa la fecha y hora. Si se cancela, la captura no se guarda (queda solo en el portapapeles). Si ya existe un archivo con ese nombre, se agrega `-2`, `-3`...
- **Mostrar un aviso al terminar cada captura:** muestra el mensaje "Copiada y guardada en…". Si se apaga, solo se silencia ese aviso de éxito, los errores y los demás avisos (por ejemplo, que no se guardó al cancelar el nombre) se siguen mostrando.
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
    ├── AppBandeja.cs          Coordina menú, atajos y flujo de captura
    ├── CapaSeleccion.cs       Capa para elegir la zona
    ├── VentanaConfiguracion.cs  Ventana de configuración
    ├── VentanaNombre.cs       Cuadro para el nombre
    └── VentanaAtajo.cs        Atajos globales
tests/LiveSnip.Tests/          Pruebas de la lógica
```

`AppBandeja` recibe el capturador y el almacén por interfaz, así que se puede reemplazar o probar por separado.

Las ventanas se construyen por código, sin el diseñador de Visual Studio. El proyecto se hizo para experimentar realizarlo de esta manera y los formularios son pequeños, lo cúal facilita este fortma de hacerlo.

## Limitaciones

- Solo captura el monitor donde está el mouse al pulsar el atajo.
- Solo Windows.

## Acerca del historial

LiveSnip lo he desarrollado originalmente a principios de 2026. Esta versión es una **reescritura realizada en 2026** a partir de la versión final de ese proyecto, aplicando criterios actuales de organización, nombres y estilo.

El comienzo del historial de commits de este repositorio es una **reconstrucción**: ordene el trabajo en pasos lógicos a partir del código final. **No es el historial original**.

## Licencia

MIT

## Autor

[Victor Sihachinger](https://github.com/Sihachinger)
