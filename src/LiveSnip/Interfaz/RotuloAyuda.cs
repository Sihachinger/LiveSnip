using System.Drawing.Drawing2D;

namespace LiveSnip.Interfaz;

// Cartel con la ayuda que se muestra arriba de la capa de seleccion.
// Es una ventana aparte porque la capa es muy transparente y el texto casi no se veria,
// asi este cartel tiene su propio fondo negro con muy poca transparencia.
class RotuloAyuda : Form {
    // Estilos de ventana de Windows: el cartel no roba el foco, no aparece en Alt+Tab y deja pasar los clics
    const int WS_EX_TRANSPARENT = 0x00000020;
    const int WS_EX_TOOLWINDOW = 0x00000080;
    const int WS_EX_NOACTIVATE = 0x08000000;

    const int MargenHorizontal = 14;
    const int MargenVertical = 8;
    const int DistanciaArriba = 20;     // separacion con el borde superior del monitor
    const int Separacion = 8;           // espacio entre un elemento y el siguiente
    const int RellenoEtiquetaHorizontal = 9;
    const int RellenoEtiquetaVertical = 4;
    const int RadioCartel = 12;
    const int RadioEtiqueta = 8;
    const double OpacidadVisible = 0.85;   // casi opaco, deja ver apenas lo de abajo

    static readonly Color ColorBorde = Color.FromArgb(95, 95, 95);
    static readonly Color ColorTexto = Color.White;
    static readonly Color ColorApagado = Color.FromArgb(175, 175, 175);
    static readonly Color ColorActivo = Color.Lime;   // el mismo verde del rectangulo de seleccion

    // Cada trozo del cartel: su texto, donde va y de que tipo es
    record Pieza(string Texto, Rectangle Area, bool EsOpcion, bool Activa);

    readonly Font _fuente = new("Segoe UI", 10);
    readonly List<Pieza> _piezas = new();
    Region? _regionCartel;

    public RotuloAyuda() {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = Color.Black;
        Opacity = OpacidadVisible;
        DoubleBuffered = true;
    }

    // Se muestra sin activarse para que el teclado siga yendo a la capa
    protected override bool ShowWithoutActivation => true;

    // Se oculta poniendolo totalmente transparente en vez de esconder la ventana,
    // porque al esconderla y mostrarla Windows le pasa el foco y la capa deja de recibir el teclado
    public bool EstaOculto => Opacity == 0;

    public void Ocultar(bool oculto) => Opacity = oculto ? 0 : OpacidadVisible;

    protected override CreateParams CreateParams {
        get {
            var parametros = base.CreateParams;
            parametros.ExStyle |= WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            return parametros;
        }
    }

    // Arma el cartel con las opciones de aspecto (la activa va resaltada) y lo deja centrado arriba del monitor
    public void Colocar(IReadOnlyList<string> opciones, int activa, Rectangle monitor) {
        _piezas.Clear();

        int altoTexto = TextRenderer.MeasureText("Ag", _fuente, Size.Empty, TextFormatFlags.NoPadding).Height;
        int altoEtiqueta = altoTexto + RellenoEtiquetaVertical * 2;
        int x = MargenHorizontal;

        void Agregar(string texto, bool esOpcion, bool esActiva) {
            int ancho = TextRenderer.MeasureText(texto, _fuente, Size.Empty, TextFormatFlags.NoPadding).Width;
            if (esOpcion) ancho += RellenoEtiquetaHorizontal * 2;
            _piezas.Add(new Pieza(texto, new Rectangle(x, MargenVertical, ancho, altoEtiqueta), esOpcion, esActiva));
            x += ancho + Separacion;
        }

        Agregar("Arrastre para elegir", false, false);
        for (int i = 0; i < opciones.Count; i++)
            Agregar(opciones[i], true, i == activa);
        Agregar("Esc cancela", false, false);

        // x quedo despues de la ultima pieza mas la separacion, que se cambia por el margen
        Size = new Size(x - Separacion + MargenHorizontal, altoEtiqueta + MargenVertical * 2);
        Location = new Point(monitor.X + (monitor.Width - Width) / 2, monitor.Y + DistanciaArriba);

        // Esquinas redondeadas
        using var contorno = Redondeado(new Rectangle(0, 0, Width, Height), RadioCartel);
        // La region tiene que seguir viva porque Windows la aplica recien cuando se crea la ventana
        var anterior = _regionCartel;
        _regionCartel = new Region(contorno);
        Region = _regionCartel;
        anterior?.Dispose();

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);
        var grafico = e.Graphics;
        grafico.SmoothingMode = SmoothingMode.AntiAlias;

        // Borde fino alrededor
        using (var contorno = Redondeado(new Rectangle(0, 0, Width - 1, Height - 1), RadioCartel))
        using (var lapiz = new Pen(ColorBorde))
            grafico.DrawPath(lapiz, contorno);

        foreach (var pieza in _piezas) {
            Color colorTexto = ColorTexto;

            if (pieza.EsOpcion) {
                // La opcion elegida va con fondo verde y texto negro, las demas en gris
                if (pieza.Activa) {
                    using var forma = Redondeado(pieza.Area, RadioEtiqueta);
                    using var relleno = new SolidBrush(ColorActivo);
                    grafico.FillPath(relleno, forma);
                    colorTexto = Color.Black;
                } else {
                    colorTexto = ColorApagado;
                }
            }

            TextRenderer.DrawText(grafico, pieza.Texto, _fuente, pieza.Area, colorTexto,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    static GraphicsPath Redondeado(Rectangle area, int radio) {
        int diametro = radio * 2;
        var ruta = new GraphicsPath();
        ruta.AddArc(area.X, area.Y, diametro, diametro, 180, 90);
        ruta.AddArc(area.Right - diametro, area.Y, diametro, diametro, 270, 90);
        ruta.AddArc(area.Right - diametro, area.Bottom - diametro, diametro, diametro, 0, 90);
        ruta.AddArc(area.X, area.Bottom - diametro, diametro, diametro, 90, 90);
        ruta.CloseFigure();
        return ruta;
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            _fuente.Dispose();
            _regionCartel?.Dispose();
        }
        base.Dispose(disposing);
    }
}
