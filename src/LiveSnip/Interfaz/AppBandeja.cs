using System.Diagnostics;
using LiveSnip.Nucleo;

namespace LiveSnip.Interfaz;

// App en la bandeja de windows, toma el atajo, el menu y el flujo de captura
// La captura y el guardado los hacen las clases de Nucleo directamente
class AppBandeja : ApplicationContext {
    readonly ICapturadorPantalla _capturador;
    readonly IAlmacenCapturas _almacen;
    readonly IInicioAutomatico _inicio;
    readonly NotifyIcon _icono;
    readonly VentanaAtajo _atajo;
    Config _config;

    bool _capturando;        // evita abrir dos selecciones a la vez
    bool _avisoDeGuardado;   // true si el ultimo aviso fue "Copiada y guardada" (seria el unico que abre la carpeta al tocarlo)
    bool _configAbierta;     // evita abrir dos ventanas de configuracion

    public AppBandeja(ICapturadorPantalla capturador, IAlmacenCapturas almacen, IInicioAutomatico inicio) {
        _capturador = capturador;
        _almacen = almacen;
        _inicio = inicio;

        var (config, primeraVez, danado) = Config.Cargar();
        _config = config;
        _almacen.Configurar(_config.Carpeta);

        // Si esta activo, refresca la ruta por si movieron el .exe
        if (_inicio.Activo) _inicio.Establecer(true);

        _icono = new NotifyIcon {
            Icon = CargarIcono(),
            Visible = true,
            ContextMenuStrip = CrearMenu()
        };
        _icono.DoubleClick += (_, _) => Capturar();
        _icono.BalloonTipClicked += (_, _) => { if (_avisoDeGuardado) AbrirCarpeta(); };

        _atajo = new VentanaAtajo(Capturar);
        AplicarAtajo();

        // Si no se pudo leer el archivo de ajustes, avisa antes de abrir la configuracion
        if (danado)
            MessageBox.Show("No se pudo leer el archivo de configuración, se encontraba dañado.\nSe usarán los valores por defecto.",
                "LiveSnip", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        // La primera vez deja elegir el atajo (y guarda los valores por defecto para no volverlo a preguntar al usuario)
        if (primeraVez) {
            _config.Guardar();
            AbrirConfiguracion();
        }
    }

    // Usa el icono del propio .exe
    static Icon CargarIcono() {
        try { return Icon.ExtractAssociatedIcon(Environment.ProcessPath!) ?? SystemIcons.Application; }
        catch { return SystemIcons.Application; }
    }

    // Registra el atajo de la config, si esta ocupado avisa
    bool AplicarAtajo() {
        bool ok = _atajo.Registrar(_config.Modificadores, (uint)_config.Tecla);
        _icono.Text = $"LiveSnip ({_config.Atajo})";
        if (!ok)
            MessageBox.Show($"No se pudo registrar {_config.Atajo}, otra app ya lo usa.\nElija otro en Configuración.",
                "LiveSnip", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return ok;
    }

    ContextMenuStrip CrearMenu() {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Capturar", null, (_, _) => Capturar());

        // Submenu de relaciones de aspecto
        var aspecto = new ToolStripMenuItem("Relación de aspecto");
        foreach (var relacion in RelacionAspecto.Todas) {
            var item = new ToolStripMenuItem(relacion.Nombre);
            item.Click += (_, _) => GuardarAspecto(relacion);
            aspecto.DropDownItems.Add(item);
        }
        // La personalizada va al final y muestra los numeros que hay en la configuracion
        var personalizada = new ToolStripMenuItem();
        personalizada.Click += (_, _) => GuardarAspecto(_config.Personalizada);
        aspecto.DropDownItems.Add(personalizada);

        // Marca la opcion activa y actualiza el texto de la personalizada cada vez que se abre el submenu
        aspecto.DropDownOpening += (_, _) => {
            personalizada.Text = $"Personalizada ({_config.Personalizada.Nombre})";
            for (int i = 0; i < aspecto.DropDownItems.Count; i++)
                ((ToolStripMenuItem)aspecto.DropDownItems[i]).Checked = i == _config.Aspecto;
        };
        menu.Items.Add(aspecto);

        menu.Items.Add("Configuración...", null, (_, _) => AbrirConfiguracion());
        menu.Items.Add("Abrir carpeta", null, (_, _) => AbrirCarpeta());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Salir", null, (_, _) => Salir());
        return menu;
    }

    // Abre la carpeta de capturas en el Explorador, si no existe la crea y si falla avisa
    void AbrirCarpeta() {
        try {
            Directory.CreateDirectory(_almacen.Carpeta);
            Process.Start(new ProcessStartInfo(_almacen.Carpeta) { UseShellExecute = true });
        } catch (Exception ex) {
            _avisoDeGuardado = false;
            _icono.ShowBalloonTip(3000, "LiveSnip", "No se pudo abrir la carpeta: " + ex.Message, ToolTipIcon.Error);
        }
    }

    void GuardarAspecto(RelacionAspecto relacion) {
        if (_config.Aspecto == relacion.Indice) return;
        _config.Aspecto = relacion.Indice;
        _config.Guardar();
    }

    void AbrirConfiguracion() {
        if (_configAbierta) return;
        _configAbierta = true;
        try {
            // La casilla refleja lo que realmente hay registrado en Windows
            _config.IniciarConWindows = _inicio.Activo;

            using var ventana = new VentanaConfiguracion(_config);
            if (ventana.ShowDialog() != DialogResult.OK) return;

            var anterior = _config;
            _config = ventana.Resultado;

            // Si el atajo nuevo esta ocupado, vuelve al anterior
            if (!AplicarAtajo()) {
                _config.Modificadores = anterior.Modificadores;
                _config.Tecla = anterior.Tecla;
                AplicarAtajo();
            }

            _almacen.Configurar(_config.Carpeta);
            _inicio.Establecer(_config.IniciarConWindows);
            _config.Guardar();
        } finally {
            _configAbierta = false;
        }
    }

    void Capturar() {
        if (_capturando) return;
        _capturando = true;
        try {
            // La seleccion se hace solo en el monitor donde esta el mouse
            var pantalla = Screen.FromPoint(Cursor.Position);

            using var capa = new CapaSeleccion(pantalla, _config.Relacion, _config.Personalizada, _config.OpacidadCapa);
            capa.ShowDialog();

            // Recuerda el aspecto elegido con el teclado
            GuardarAspecto(capa.Relacion);

            if (capa.Resultado is not Rectangle zona) return;

            // Pequena pausa para que la capa desaparezca antes de capturar
            Thread.Sleep(120);

            using var imagen = _capturador.Capturar(zona);
            Clipboard.SetImage(imagen);

            // Nombre: fecha y hora, o el que escriba el usuario si lo activo en ajustes
            string nombre = _almacen.NombrePorDefecto();
            if (_config.PedirNombre) {
                var elegido = VentanaNombre.Pedir(nombre);

                // Si cancela no se guarda el archivo, la imagen queda solo en el portapapeles
                if (elegido is null) {
                    _avisoDeGuardado = false;
                    _icono.ShowBalloonTip(1500, "LiveSnip", "Copiada al portapapeles, no se guardó", ToolTipIcon.None);
                    return;
                }
                nombre = elegido;
            }

            _almacen.Guardar(imagen, nombre);
            _avisoDeGuardado = true;
            _icono.ShowBalloonTip(1500, "LiveSnip", "Copiada y guardada en " + _almacen.Carpeta, ToolTipIcon.None);
        } catch (Exception ex) {
            _avisoDeGuardado = false;
            _icono.ShowBalloonTip(3000, "LiveSnip", "Error: " + ex.Message, ToolTipIcon.Error);
        } finally {
            _capturando = false;
        }
    }

    void Salir() {
        _atajo.Dispose();
        _icono.Visible = false;
        _icono.Dispose();
        ExitThread();
    }
}
