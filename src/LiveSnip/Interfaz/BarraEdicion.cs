using LiveSnip.Nucleo;

namespace LiveSnip.Interfaz;

// Barra de arriba del editor con las opciones: tamano del difuminado, fecha, marca de agua y deshacer
class BarraEdicion : FlowLayoutPanel {
    public const int Alto = 40;
    const int BloqueMinimo = 4;
    const int BloqueMaximo = 60;
    const int BloquePorDefecto = 10;

    readonly NumericUpDown _bloque = new() { Minimum = BloqueMinimo, Maximum = BloqueMaximo, Value = BloquePorDefecto, Width = 55 };
    readonly CheckBox _fecha = new() { Text = "Fecha", AutoSize = true };
    readonly CheckBox _marca = new() { Text = "Marca de agua", AutoSize = true };
    readonly Button _deshacer = new() { Text = "Deshacer", AutoSize = true };
    readonly ToolTip _ayuda = new();   // se guarda en un campo para que no lo recoja el recolector de basura

    // Se avisa cuando cambia la fecha o la marca de agua, para redibujar la vista previa
    public event EventHandler? OpcionesCambiadas;
    public event EventHandler? DeshacerPedido;

    public int TamanoBloque => (int)_bloque.Value;
    public bool ConFecha => _fecha.Checked;
    public bool ConMarca => _marca.Checked;

    // Las casillas arrancan como esta en la configuracion, y la marca de agua solo se puede usar si hay un texto
    public BarraEdicion(Config config) {
        Dock = DockStyle.Top;
        Height = Alto;
        Padding = new Padding(10, 8, 10, 0);

        _fecha.Checked = config.AgregarFecha;
        bool hayMarca = !string.IsNullOrWhiteSpace(config.MarcaDeAgua);
        _marca.Checked = hayMarca;
        _marca.Enabled = hayMarca;
        if (!hayMarca) _ayuda.SetToolTip(_marca, "Escriba el texto en Configuración, pestaña Edición");
        _ayuda.SetToolTip(_bloque, "Tamaño de los cuadritos al difuminar");
        _ayuda.SetToolTip(_deshacer, "Deshace el último difuminado (Ctrl + Z)");

        _fecha.CheckedChanged += (_, _) => OpcionesCambiadas?.Invoke(this, EventArgs.Empty);
        _marca.CheckedChanged += (_, _) => OpcionesCambiadas?.Invoke(this, EventArgs.Empty);
        _deshacer.Click += (_, _) => DeshacerPedido?.Invoke(this, EventArgs.Empty);

        Controls.Add(new Label { Text = "Difuminar con bloques de:", AutoSize = true, Margin = new Padding(0, 5, 3, 0) });
        Controls.Add(_bloque);
        Controls.Add(new Label { Text = "px", AutoSize = true, Margin = new Padding(0, 5, 20, 0) });
        Controls.Add(_fecha);
        Controls.Add(_marca);
        Controls.Add(_deshacer);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) _ayuda.Dispose();
        base.Dispose(disposing);
    }
}
