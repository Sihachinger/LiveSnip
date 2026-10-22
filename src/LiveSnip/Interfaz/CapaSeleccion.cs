using LiveSnip.Nucleo;

namespace LiveSnip.Interfaz;

// Capa casi transparente sobre el monitor que tiene el focus para elegir la zona
// Lo mas importante, no toma ninguna foto de la pantalla, por eso lo de abajo sigue en vivo por asi decirlo
class CapaSeleccion : Form {
    public Rectangle? Resultado { get; private set; } // en coordenadas de pantalla
    public RelacionAspecto Relacion { get; private set; }

    Point _inicio;          // donde se hizo clic para empezar a arrastrar
    Rectangle _seleccion;   // rectangulo actual, en coordenadas de la capa
    bool _arrastrando;      // true mientras el mouse esta presionado
    readonly RelacionAspecto _personalizada;   // la que se elige con la tecla 6
    readonly RotuloAyuda _rotulo = new();      // cartel de ayuda de arriba, con su propio fondo

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
        Opacity = opacidad; // se ve lo de abajo, pero oscurecido segun la transparencia configurada
        Cursor = Cursors.Cross;
        DoubleBuffered = true; // evita el parpadeo al redibujar
        KeyPreview = true;
    }

    protected override void OnShown(EventArgs e) {
        base.OnShown(e);

        // El cartel se muestra sin activarse, asi el teclado sigue llegando a la capa
        ColocarRotulo();
        _rotulo.Show(this);

        Activate(); // para recibir el teclado
    }

    protected override void Dispose(bool disposing) {
        if (disposing) _rotulo.Dispose();
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

    protected override void OnMouseDown(MouseEventArgs e) {
        // Clic derecho cancela
        if (e.Button == MouseButtons.Right) { Close(); return; }
        _inicio = e.Location;
        _seleccion = Rectangle.Empty;
        _arrastrando = true;
        ActualizarRotulo();
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        if (!_arrastrando) return;
        // Recalcula el rectangulo con la posicion del mouse y redibuja
        _seleccion = Relacion.Ajustar(_inicio, e.Location, Size);
        ActualizarRotulo();
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        if (!_arrastrando) return;
        _arrastrando = false;

        // Ignora los clics sueltos
        if (_seleccion.Width < 5 || _seleccion.Height < 5) { Invalidate(); return; }

        // Pasa a coordenadas de pantalla sumando donde empieza el monitor
        Resultado = new Rectangle(
            _seleccion.X + Bounds.X, _seleccion.Y + Bounds.Y,
            _seleccion.Width, _seleccion.Height);
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

            // El cartel resalta el aspecto elegido, asi que hay que rehacerlo
            ColocarRotulo();
            ActualizarRotulo();
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e) {
        var grafico = e.Graphics;

        // Si todavia no hay seleccion, no dibuja nada mas
        if (_seleccion.Width < 1) return;

        using var fuente = new Font("Segoe UI", 11);
        using var lapiz = new Pen(Color.Lime, 3);
        grafico.DrawRectangle(lapiz, _seleccion);
        // Medidas en pixeles encima del rectangulo
        grafico.DrawString($"{_seleccion.Width} x {_seleccion.Height}", fuente, Brushes.Lime,
            _seleccion.X, Math.Max(0, _seleccion.Y - 24));
    }
}
