using LiveSnip.Nucleo;

namespace LiveSnip.Interfaz;

// Dibuja el marco verde de la seleccion y sus medidas encima de la capa.
// Es una ventana aparte porque la oscuridad se aplica a toda la ventana de la capa,
// y asi el marco siempre se ve con su color pleno sin importar cuanto oscurezca el fondo.
class MarcoSeleccion : Form {
    // Estilos de ventana de Windows: no roba el foco, no aparece en Alt+Tab y deja pasar los clics
    const int WS_EX_TRANSPARENT = 0x00000020;
    const int WS_EX_TOOLWINDOW = 0x00000080;
    const int WS_EX_NOACTIVATE = 0x08000000;

    const int Grosor = 3;

    // Todo lo que tenga este color es invisible, asi solo se ve lo que se dibuja encima
    static readonly Color ColorClave = Color.Magenta;

    readonly Font _fuente = new("Segoe UI", 11);
    Rectangle _seleccion;   // en coordenadas de esta ventana, que coinciden con las de la capa
    Tirador[] _tiradores = Array.Empty<Tirador>();   // los que se dibujan, ninguno mientras no se pueda ajustar

    public MarcoSeleccion() {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = ColorClave;
        TransparencyKey = ColorClave;
        DoubleBuffered = true;
    }

    // Se muestra sin activarse para que el teclado siga yendo a la capa
    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams {
        get {
            var parametros = base.CreateParams;
            parametros.ExStyle |= WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            return parametros;
        }
    }

    // Muestra u oculta los tiradores de las esquinas y los bordes, solo hacen falta cuando la zona se puede ajustar
    public void MostrarTiradores(Tirador[] tiradores) {
        _tiradores = tiradores;
        Invalidate(ZonaDeDibujo(_seleccion));
    }

    // Cambia la seleccion y redibuja solo lo que hace falta, el marco viejo y el nuevo
    public void Mostrar(Rectangle seleccion) {
        var anterior = _seleccion;
        _seleccion = seleccion;
        Invalidate(ZonaDeDibujo(anterior));
        Invalidate(ZonaDeDibujo(seleccion));
    }

    // Area que ocupan el marco y las medidas de arriba, con un margen de sobra
    static Rectangle ZonaDeDibujo(Rectangle seleccion) =>
        new(seleccion.X - 8, seleccion.Y - 50, Math.Max(seleccion.Width, 220) + 16, seleccion.Height + 58);

    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);

        // Si todavia no hay seleccion, no dibuja nada
        if (_seleccion.Width < 1) return;

        var grafico = e.Graphics;
        using var lapiz = new Pen(Color.Lime, Grosor);
        grafico.DrawRectangle(lapiz, _seleccion);

        // Con la zona abierta se dibujan los tiradores, cuadritos verdes con borde negro para que se vean sobre cualquier fondo
        foreach (var tirador in _tiradores) {
            var area = Tiradores.Area(_seleccion, tirador);
            grafico.FillRectangle(Brushes.Lime, area);
            grafico.DrawRectangle(Pens.Black, area);
        }

        // Las medidas van sobre un cuadro negro, asi se leen bien y no queda borde rosado del color transparente
        string medidas = $"{_seleccion.Width} x {_seleccion.Height}";
        var tamano = TextRenderer.MeasureText(medidas, _fuente, Size.Empty, TextFormatFlags.NoPadding);
        var caja = new Rectangle(_seleccion.X, Math.Max(0, _seleccion.Y - tamano.Height - 8), tamano.Width + 12, tamano.Height + 4);
        grafico.FillRectangle(Brushes.Black, caja);
        TextRenderer.DrawText(grafico, medidas, _fuente, caja, Color.Lime,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) _fuente.Dispose();
        base.Dispose(disposing);
    }
}
