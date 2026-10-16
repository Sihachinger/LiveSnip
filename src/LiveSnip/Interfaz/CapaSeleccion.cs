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

    public CapaSeleccion(Screen pantalla, RelacionAspecto relacion) {
        Relacion = relacion;

        // Ventana sin bordes que cubre todo el monitor, siempre encima de lo demas
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = pantalla.Bounds;
        TopMost = true;
        ShowInTaskbar = false;
        BackColor = Color.Black;
        Opacity = 0.4; // se ve lo de abajo, pero oscurecido
        Cursor = Cursors.Cross;
        DoubleBuffered = true; // evita el parpadeo al redibujar
        KeyPreview = true;
    }

    protected override void OnShown(EventArgs e) {
        base.OnShown(e);
        Activate(); // para recibir el teclado
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        // Clic derecho cancela
        if (e.Button == MouseButtons.Right) { Close(); return; }
        _inicio = e.Location;
        _seleccion = Rectangle.Empty;
        _arrastrando = true;
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        if (!_arrastrando) return;
        // Recalcula el rectangulo con la posicion del mouse y redibuja
        _seleccion = Relacion.Ajustar(_inicio, e.Location, Size);
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

        // Teclas 0-5 cambian la relacion de aspecto
        int i = e.KeyCode - Keys.D0;
        if (i >= 0 && i < RelacionAspecto.Todas.Count) {
            Relacion = RelacionAspecto.Todas[i];
            // Si ya esta arrastrando, aplica el nuevo aspecto al momento
            if (_arrastrando)
                _seleccion = Relacion.Ajustar(_inicio, PointToClient(Cursor.Position), Size);
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e) {
        var grafico = e.Graphics;

        // Mensaje arriba sobre el overlay
        string ayuda = $"Arrastra para elegir  |  0 Libre | 1 1:1 | 2 4:3 | 3 16:9 | 4 9:16 | 5 3:2  |  Esc cancela  |  Aspecto: {Relacion.Nombre}";
        using var fuente = new Font("Segoe UI", 11);
        var tam = grafico.MeasureString(ayuda, fuente);
        grafico.DrawString(ayuda, fuente, Brushes.White, (Width - tam.Width) / 2, 20);

        // Si todavia no hay seleccion, no dibuja nada mas
        if (_seleccion.Width < 1) return;

        using var lapiz = new Pen(Color.Lime, 3);
        grafico.DrawRectangle(lapiz, _seleccion);
        // Medidas en pixeles encima del rectangulo
        grafico.DrawString($"{_seleccion.Width} x {_seleccion.Height}", fuente, Brushes.Lime,
            _seleccion.X, Math.Max(0, _seleccion.Y - 24));
    }
}
