using LiveSnip.Nucleo;

namespace LiveSnip.Interfaz;

// Capa casi transparente sobre el monitor que tiene el focus para elegir la zona
// Lo mas importante, no toma ninguna foto de la pantalla, por eso lo de abajo sigue en vivo por asi decirlo
class CapaSeleccion : Form {
    public Rectangle? Resultado { get; private set; } // en coordenadas de pantalla
    public RelacionAspecto Relacion { get; private set; }

    // En que momento esta la capa, Eligiendo es dibujar la zona y Ajustando es corregirla antes de capturar
    enum Estado { Eligiendo, Ajustando }
    Estado _estado = Estado.Eligiendo;

    bool _moviendo;                 // true mientras se arrastra la zona abierta para moverla
    Tirador? _tiradorActivo;        // el tirador que se esta arrastrando para cambiar el tamano
    Rectangle _zonaAntesDeAjustar;  // como estaba la zona al empezar a arrastrar el tirador
    Point _desfaseMovimiento;       // distancia entre el clic y la esquina de la zona al empezar a moverla
    Point _inicio;          // donde se hizo clic para empezar a arrastrar
    Rectangle _seleccion;   // rectangulo actual, en coordenadas de la capa
    bool _arrastrando;      // true mientras el mouse esta presionado
    readonly RelacionAspecto _personalizada;   // la que se elige con la tecla 6
    readonly bool _editar;                     // true si al soltar el mouse la zona queda abierta para ajustarla
    readonly RotuloAyuda _rotulo = new();      // cartel de ayuda de arriba, con su propio fondo
    readonly MarcoSeleccion _marco = new();    // marco verde y medidas, en otra ventana para que la oscuridad no los afecte

    public CapaSeleccion(Screen pantalla, RelacionAspecto relacion, RelacionAspecto personalizada, double opacidad, bool editar) {
        Relacion = relacion;
        _personalizada = personalizada;
        _editar = editar;

        // Ventana sin bordes que cubre todo el monitor, siempre encima de lo demas
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = pantalla.Bounds;
        TopMost = true;
        ShowInTaskbar = false;
        BackColor = Color.Black;
        Opacity = opacidad; // se ve lo de abajo, mas o menos oscurecido segun lo configurado
        Cursor = Cursors.Cross;
        KeyPreview = true;
    }

    protected override void OnShown(EventArgs e) {
        base.OnShown(e);

        // El marco y el cartel se muestran sin activarse, asi el teclado sigue llegando a la capa
        _marco.Bounds = Bounds;
        _marco.Show(this);
        ColocarRotulo();
        _rotulo.Show(this);

        Activate(); // para recibir el teclado
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            _rotulo.Dispose();
            _marco.Dispose();
        }
        base.Dispose(disposing);
    }

    // Arma el cartel con una opcion por tecla (0 a 6) y resalta la que esta elegida
    void ColocarRotulo() {
        var opciones = new List<string>();
        for (int i = 0; i < RelacionAspecto.Todas.Count; i++)
            opciones.Add($"{i} {RelacionAspecto.Todas[i].Nombre}");
        opciones.Add($"{RelacionAspecto.IndicePersonalizada} {_personalizada.Nombre}");

        _rotulo.Colocar(opciones, Relacion.Indice, Bounds, _editar);
    }

    // Oculta el cartel mientras la seleccion lo pisa y lo vuelve a mostrar cuando deja de pisarlo
    void ActualizarRotulo() {
        // El cartel esta en coordenadas de pantalla, asi que la seleccion tambien
        var seleccionEnPantalla = new Rectangle(
            _seleccion.X + Bounds.X, _seleccion.Y + Bounds.Y,
            _seleccion.Width, _seleccion.Height);

        bool pisa = seleccionEnPantalla.IntersectsWith(_rotulo.Bounds);
        if (_rotulo.EstaOculto != pisa) _rotulo.Ocultar(pisa);
    }

    // Lo que hace el mouse depende del momento en que este la capa
    protected override void OnMouseDown(MouseEventArgs e) {
        // Clic derecho cancela
        if (e.Button == MouseButtons.Right) { Close(); return; }
        if (_estado == Estado.Eligiendo) EmpezarSeleccion(e.Location);
        else EmpezarAjuste(e.Location);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        if (_estado == Estado.Eligiendo) ArrastrarSeleccion(e.Location);
        else MoverZona(e.Location);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        if (_estado == Estado.Eligiendo) TerminarSeleccion();
        else {
            _moviendo = false;
            _tiradorActivo = null;
        }
    }

    // Los tiradores que se pueden usar ahora, con un aspecto fijo solo las esquinas
    Tirador[] TiradoresActivos() => Relacion.Valor is null ? Tiradores.Todos : Tiradores.Esquinas;

    // Tirador que hay bajo el mouse, con un margen extra para que no sea tan dificil acertarle
    Tirador? TiradorEn(Point punto) {
        foreach (var tirador in TiradoresActivos())
            if (Rectangle.Inflate(Tiradores.Area(_seleccion, tirador), 4, 4).Contains(punto)) return tirador;
        return null;
    }

    // Con la zona abierta, arrastrar un tirador cambia el tamano y arrastrar desde adentro la mueve
    // Un clic afuera no hace nada
    void EmpezarAjuste(Point punto) {
        if (TiradorEn(punto) is Tirador tirador) {
            _tiradorActivo = tirador;
            _zonaAntesDeAjustar = _seleccion;
            return;
        }
        if (!_seleccion.Contains(punto)) return;
        _moviendo = true;
        _desfaseMovimiento = new Point(punto.X - _seleccion.X, punto.Y - _seleccion.Y);
    }

    void MoverZona(Point punto) {
        if (_tiradorActivo is Tirador activo) {
            _seleccion = AjusteDeZona.Redimensionar(_zonaAntesDeAjustar, activo, punto, Relacion, Size);
            _marco.Mostrar(_seleccion);
            ActualizarRotulo();
            return;
        }

        if (!_moviendo) {
            // Sin arrastrar, el cursor avisa que se puede hacer desde ahi
            var cursor = CursorEn(punto);
            if (Cursor != cursor) Cursor = cursor;
            return;
        }

        // La zona sigue al mouse pero sin salirse del monitor
        int x = Math.Clamp(punto.X - _desfaseMovimiento.X, 0, Width - _seleccion.Width);
        int y = Math.Clamp(punto.Y - _desfaseMovimiento.Y, 0, Height - _seleccion.Height);
        _seleccion = new Rectangle(x, y, _seleccion.Width, _seleccion.Height);
        _marco.Mostrar(_seleccion);
        ActualizarRotulo();
    }

    // Cursor que corresponde a lo que se puede hacer en ese punto de la zona abierta
    Cursor CursorEn(Point punto) => TiradorEn(punto) switch {
        Tirador.ArribaIzquierda or Tirador.AbajoDerecha => Cursors.SizeNWSE,
        Tirador.ArribaDerecha or Tirador.AbajoIzquierda => Cursors.SizeNESW,
        Tirador.Arriba or Tirador.Abajo => Cursors.SizeNS,
        Tirador.Izquierda or Tirador.Derecha => Cursors.SizeWE,
        _ => _seleccion.Contains(punto) ? Cursors.SizeAll : Cursors.Cross
    };

    void EmpezarSeleccion(Point punto) {
        _inicio = punto;
        _seleccion = Rectangle.Empty;
        _marco.Mostrar(_seleccion);
        _arrastrando = true;
        ActualizarRotulo();
    }

    void ArrastrarSeleccion(Point punto) {
        if (!_arrastrando) return;
        // Recalcula el rectangulo con la posicion del mouse y lo redibuja en el marco
        _seleccion = Relacion.Ajustar(_inicio, punto, Size);
        _marco.Mostrar(_seleccion);
        ActualizarRotulo();
    }

    void TerminarSeleccion() {
        if (!_arrastrando) return;
        _arrastrando = false;

        // Ignora los clics sueltos
        if (_seleccion.Width < 5 || _seleccion.Height < 5) return;

        // En modo edicion la zona queda abierta para corregirla, y se confirma con Enter o con doble clic
        if (_editar) {
            _estado = Estado.Ajustando;
            _marco.MostrarTiradores(TiradoresActivos());
            return;
        }
        Confirmar();
    }

    // Termina con la zona que hay ahora, pasandola a coordenadas de pantalla sumando donde empieza el monitor
    void Confirmar() {
        Resultado = new Rectangle(
            _seleccion.X + Bounds.X, _seleccion.Y + Bounds.Y,
            _seleccion.Width, _seleccion.Height);
        Close();
    }

    // Doble clic con el boton izquierdo selecciona todo el monitor
    protected override void OnMouseDoubleClick(MouseEventArgs e) {
        base.OnMouseDoubleClick(e);
        if (e.Button != MouseButtons.Left) return;

        // Con la zona abierta, doble clic dentro de ella la confirma
        if (_estado == Estado.Ajustando && _seleccion.Contains(e.Location)) {
            Confirmar();
            return;
        }

        // El resultado va en coordenadas de pantalla, que para el monitor entero son sus limites
        Resultado = Bounds;
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        if (e.KeyCode == Keys.Escape) { Close(); return; }

        // Con la zona abierta, Enter la confirma
        if (e.KeyCode == Keys.Enter && _estado == Estado.Ajustando) { Confirmar(); return; }

        // Teclas 0-5 cambian la relacion de aspecto y la 6 usa la personalizada
        int i = e.KeyCode - Keys.D0;
        if (i >= 0 && i <= RelacionAspecto.IndicePersonalizada) {
            Relacion = i == RelacionAspecto.IndicePersonalizada ? _personalizada : RelacionAspecto.Todas[i];
            // Si ya esta arrastrando, aplica el nuevo aspecto al momento
            if (_arrastrando)
                _seleccion = Relacion.Ajustar(_inicio, PointToClient(Cursor.Position), Size);

            // Con la zona abierta se le aplica el aspecto nuevo desde su esquina de arriba, y los tiradores cambian
            // (con un aspecto fijo solo quedan los de las esquinas)
            if (_estado == Estado.Ajustando) {
                _seleccion = Relacion.Ajustar(_seleccion.Location, new Point(_seleccion.Right, _seleccion.Bottom), Size);
                _marco.MostrarTiradores(TiradoresActivos());
            }

            // El marco se redibuja con el nuevo aspecto
            _marco.Mostrar(_seleccion);

            // El cartel resalta el aspecto elegido, asi que hay que rehacerlo
            ColocarRotulo();
            ActualizarRotulo();
        }
    }
}
