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
    const int IdAtajoCapturar = 1;   // numero con el que se registra el atajo de capturar
    const int IdAtajoRepetir = 2;    // numero con el que se registra el atajo de repetir la captura
    const int IdAtajoPantallaCompleta = 3;   // numero con el que se registra el atajo de la pantalla completa
    Config _config;

    bool _capturando;        // evita abrir dos selecciones a la vez
    Rectangle? _ultimaZona;  // zona de la ultima captura en coordenadas de pantalla, se pierde al cerrar la app
    bool _avisoDeGuardado;   // true si el ultimo aviso fue "Copiada y guardada" (seria el unico que abre la carpeta al tocarlo)
    bool _configAbierta;     // evita abrir dos ventanas de configuracion
    VentanaConfiguracion? _ventanaConfiguracion;   // la que esta abierta ahora, para traerla al frente

    // Cuando se abre otra copia de la app, esta senal le pide a esta que muestre la configuracion
    readonly EventWaitHandle _pedidoDeConfiguracion;
    RegisteredWaitHandle? _espera;
    readonly Control _enlaceConLaInterfaz = new();   // permite volver al hilo de la interfaz desde el hilo que espera la senal

    public AppBandeja(ICapturadorPantalla capturador, IAlmacenCapturas almacen, IInicioAutomatico inicio, EventWaitHandle pedidoDeConfiguracion) {
        _capturador = capturador;
        _almacen = almacen;
        _inicio = inicio;
        _pedidoDeConfiguracion = pedidoDeConfiguracion;

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

        _atajo = new VentanaAtajo(AlPresionarAtajo);
        AplicarAtajo();
        AplicarAtajoRepetir();
        AplicarAtajoPantallaCompleta();

        // Se crea el control en este hilo y se queda esperando la senal de otra copia
        _ = _enlaceConLaInterfaz.Handle;
        _espera = ThreadPool.RegisterWaitForSingleObject(
            _pedidoDeConfiguracion, (_, _) => AlRecibirPedido(), null, Timeout.Infinite, false);

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

    // Windows avisa con el id del atajo que se pulso
    void AlPresionarAtajo(int id) {
        if (id == IdAtajoCapturar) Capturar();
        else if (id == IdAtajoRepetir) RepetirCaptura(esperarAntes: false);
        else if (id == IdAtajoPantallaCompleta) CapturarPantallaCompleta(esperarAntes: false);
    }

    // Registra el atajo de la config, si esta ocupado avisa
    bool AplicarAtajo() {
        bool ok = _atajo.Registrar(IdAtajoCapturar, _config.Modificadores, (uint)_config.Tecla);
        _icono.Text = $"LiveSnip ({_config.Atajo})";
        if (!ok)
            MessageBox.Show($"No se pudo registrar {_config.Atajo}, otra app ya lo usa.\nElija otro en Configuración.",
                "LiveSnip", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return ok;
    }

    // Registra el atajo para repetir la captura, si esta ocupado avisa
    bool AplicarAtajoRepetir() {
        bool ok = _atajo.Registrar(IdAtajoRepetir, _config.ModificadoresRepetir, (uint)_config.TeclaRepetir);
        if (!ok)
            MessageBox.Show($"No se pudo registrar {_config.AtajoRepetir}, otra app ya lo usa.\nElija otro en Configuración.",
                "LiveSnip", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return ok;
    }

    // Registra el atajo de la pantalla completa, si esta ocupado avisa
    bool AplicarAtajoPantallaCompleta() {
        bool ok = _atajo.Registrar(IdAtajoPantallaCompleta, _config.ModificadoresPantallaCompleta, (uint)_config.TeclaPantallaCompleta);
        if (!ok)
            MessageBox.Show($"No se pudo registrar {_config.AtajoPantallaCompleta}, otra app ya lo usa.\nElija otro en Configuración.",
                "LiveSnip", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return ok;
    }

    ContextMenuStrip CrearMenu() {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Capturar", null, (_, _) => Capturar());
        menu.Items.Add("Capturar pantalla completa", null, (_, _) => CapturarPantallaCompleta(esperarAntes: true));
        menu.Items.Add("Repetir última captura", null, (_, _) => RepetirCaptura(esperarAntes: true));
        menu.Items.Add("Abrir última captura", null, (_, _) => AbrirUltimaCaptura());

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

    // Abre en el visor de imagenes de Windows el .png mas reciente de la carpeta de capturas
    void AbrirUltimaCaptura() {
        try {
            string? ruta = _almacen.UltimaCaptura();
            if (ruta is null) {
                AvisarSinCaptura("Todavía no hay ninguna captura guardada");
                return;
            }
            Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
        } catch (Exception ex) {
            _avisoDeGuardado = false;
            _icono.ShowBalloonTip(3000, "LiveSnip", "No se pudo abrir la captura: " + ex.Message, ToolTipIcon.Error);
        }
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

    // Esto corre en otro hilo, asi que vuelve al hilo de la interfaz antes de abrir nada
    void AlRecibirPedido() {
        try {
            _enlaceConLaInterfaz.BeginInvoke((Action)MostrarConfiguracion);
        } catch (ObjectDisposedException) {
            // la app se esta cerrando
        } catch (InvalidOperationException) {
            // la interfaz ya no esta disponible
        }
    }

    // La pide la otra copia de la app: abre la configuracion, o la trae al frente si ya estaba abierta
    void MostrarConfiguracion() {
        // No se interrumpe una captura en curso
        if (_capturando) return;

        if (_configAbierta) {
            _ventanaConfiguracion?.Activate();
            return;
        }
        AbrirConfiguracion();
    }

    void AbrirConfiguracion() {
        if (_configAbierta) return;
        _configAbierta = true;
        try {
            // La casilla refleja lo que realmente hay registrado en Windows
            _config.IniciarConWindows = _inicio.Activo;

            using var ventana = new VentanaConfiguracion(_config);
            _ventanaConfiguracion = ventana;
            if (ventana.ShowDialog() != DialogResult.OK) return;

            var anterior = _config;
            _config = ventana.Resultado;

            // Si el atajo nuevo esta ocupado, vuelve al anterior
            if (!AplicarAtajo()) {
                _config.Modificadores = anterior.Modificadores;
                _config.Tecla = anterior.Tecla;
                AplicarAtajo();
            }

            // Lo mismo para el atajo de repetir la captura
            if (!AplicarAtajoRepetir()) {
                _config.ModificadoresRepetir = anterior.ModificadoresRepetir;
                _config.TeclaRepetir = anterior.TeclaRepetir;
                AplicarAtajoRepetir();
            }

            // Y para el de la pantalla completa
            if (!AplicarAtajoPantallaCompleta()) {
                _config.ModificadoresPantallaCompleta = anterior.ModificadoresPantallaCompleta;
                _config.TeclaPantallaCompleta = anterior.TeclaPantallaCompleta;
                AplicarAtajoPantallaCompleta();
            }

            _almacen.Configurar(_config.Carpeta);
            _inicio.Establecer(_config.IniciarConWindows);
            _config.Guardar();
        } finally {
            _ventanaConfiguracion = null;
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

            _ultimaZona = zona;
            CapturarZona(zona);
        } catch (Exception ex) {
            _avisoDeGuardado = false;
            _icono.ShowBalloonTip(3000, "LiveSnip", "Error: " + ex.Message, ToolTipIcon.Error);
        } finally {
            _capturando = false;
        }
    }

    // Captura otra vez la misma zona de la ultima captura, sin mostrar la capa
    // Desde el menu hay que esperar un momento para que el menu termine de cerrarse y no salga en la imagen
    void RepetirCaptura(bool esperarAntes) {
        if (_capturando) return;

        if (_ultimaZona is not Rectangle zona) {
            AvisarSinCaptura("Todavía no hay una captura anterior para repetir");
            return;
        }

        // La pantalla pudo cambiar desde la captura anterior (resolucion, monitores)
        if (!Screen.AllScreens.Any(pantalla => pantalla.Bounds.Contains(zona))) {
            AvisarSinCaptura("La zona de la captura anterior ya no entra en ninguna pantalla");
            return;
        }

        CapturarSinCapa(zona, esperarAntes);
    }

    // Captura todo el monitor donde esta el mouse, sin mostrar la capa
    void CapturarPantallaCompleta(bool esperarAntes) {
        if (_capturando) return;
        CapturarSinCapa(Screen.FromPoint(Cursor.Position).Bounds, esperarAntes);
    }

    // Captura una zona que ya se conoce, sin elegirla en la capa, y la recuerda para poder repetirla
    void CapturarSinCapa(Rectangle zona, bool esperarAntes) {
        _capturando = true;
        try {
            if (esperarAntes) Thread.Sleep(200);
            _ultimaZona = zona;
            CapturarZona(zona);
        } catch (Exception ex) {
            _avisoDeGuardado = false;
            _icono.ShowBalloonTip(3000, "LiveSnip", "Error: " + ex.Message, ToolTipIcon.Error);
        } finally {
            _capturando = false;
        }
    }

    // Aviso corto cuando no se pudo repetir, tocarlo no abre la carpeta
    void AvisarSinCaptura(string mensaje) {
        _avisoDeGuardado = false;
        _icono.ShowBalloonTip(2500, "LiveSnip", mensaje, ToolTipIcon.None);
    }

    // Aviso de captura exitosa, se puede apagar en la configuracion
    // Los errores y los demas avisos no se apagan
    void AvisarExito(string mensaje) {
        if (!_config.MostrarAviso) return;
        _icono.ShowBalloonTip(1500, "LiveSnip", mensaje, ToolTipIcon.None);
    }

    // Saca la imagen de una zona de la pantalla, la copia al portapapeles y la guarda
    // Segun el destino elegido en la configuracion la copia al portapapeles, la guarda o hace las dos cosas
    void CapturarZona(Rectangle zona) {
        using var imagen = _capturador.Capturar(zona);
        if (_config.CopiaAlPortapapeles) Clipboard.SetImage(imagen);

        // Si solo se copia no hay nada que nombrar ni que guardar
        if (!_config.GuardaEnArchivo) {
            _avisoDeGuardado = false;
            AvisarExito("Copiada al portapapeles");
            return;
        }

        // Nombre: fecha y hora, o el que escriba el usuario si lo activo en ajustes
        string nombre = _almacen.NombrePorDefecto();
        if (_config.PedirNombre) {
            var elegido = VentanaNombre.Pedir(nombre);

            // Si cancela no se guarda el archivo, y la imagen queda en el portapapeles solo si se estaba copiando
            if (elegido is null) {
                _avisoDeGuardado = false;
                string mensaje = _config.CopiaAlPortapapeles ? "Copiada al portapapeles, no se guardó" : "No se guardó la captura";
                _icono.ShowBalloonTip(1500, "LiveSnip", mensaje, ToolTipIcon.None);
                return;
            }
            nombre = elegido;
        }

        _almacen.Guardar(imagen, nombre);
        _avisoDeGuardado = true;
        string accion = _config.CopiaAlPortapapeles ? "Copiada y guardada en " : "Guardada en ";
        AvisarExito(accion + _almacen.Carpeta);
    }

    void Salir() {
        _espera?.Unregister(null);
        _enlaceConLaInterfaz.Dispose();
        _atajo.Dispose();
        _icono.Visible = false;
        _icono.Dispose();
        ExitThread();
    }
}
