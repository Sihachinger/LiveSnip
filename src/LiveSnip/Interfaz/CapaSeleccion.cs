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

    Point _inicio;          // donde se hizo clic para empezar a arrastrar
    Rectangle _seleccion;   // rectangulo actual, en coordenadas de la capa
    bool _arrastrando;      // true mientras el mouse esta presionado
    readonly RelacionAspecto _personalizada;   // la que se elige con la tecla 6
    readonly RotuloAyuda _rotulo = new();      // cartel de ayuda de arriba, con su propio fondo
    readonly MarcoSeleccion _marco = new();    // marco verde y medidas, en otra ventana para que la oscuridad no los afecte

    public CapaSeleccion(Screen pantalla, RelacionAspecto relacion, RelacionAspecto personalizada, double opacidad) {
        Relacion = relacion;
        _personalizada = personalizada;

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

        _rotulo.Colocar(opciones, Relacion.Indice, Bounds);
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
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        if (_estado == Estado.Eligiendo) ArrastrarSeleccion(e.Location);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        if (_estado == Estado.Eligiendo) TerminarSeleccion();
    }

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

        // El resultado va en coordenadas de pantalla, que para el monitor entero son sus limites
        Resultado = Bounds;
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        if (e.KeyCode == Keys.Escape) { Close(); return; }

        // Teclas 0-5 cambian la relacion de aspecto y la 6 usa la personalizada
        int i = e.KeyCode - Keys.D0;
        if (i >= 0 && i <= RelacionAspecto.IndicePersonalizada) {
            Relacion = i == RelacionAspecto.IndicePersonalizada ? _personalizada : RelacionAspecto.Todas[i];
            // Si ya esta arrastrando, aplica el nuevo aspecto al momento
            if (_arrastrando)
                _seleccion = Relacion.Ajustar(_inicio, PointToClient(Cursor.Position), Size);

            // El marco se redibuja con el nuevo aspecto
            _marco.Mostrar(_seleccion);

            // El cartel resalta el aspecto elegido, asi que hay que rehacerlo
            ColocarRotulo();
            ActualizarRotulo();
        }
    }
}
