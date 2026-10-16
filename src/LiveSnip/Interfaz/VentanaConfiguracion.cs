using LiveSnip.Nucleo;

namespace LiveSnip.Interfaz;

// Ventana de configuracion: atajo, aspecto por defecto, carpeta y nombre
class VentanaConfiguracion : Form {
    public Config Resultado { get; }

    readonly TextBox _atajo = new() { ReadOnly = true, Cursor = Cursors.Hand };
    readonly ComboBox _aspecto = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    readonly TextBox _carpeta = new();
    readonly CheckBox _pedirNombre = new() { Text = "Preguntar el nombre al guardar cada captura" };
    readonly CheckBox _iniciar = new() { Text = "Iniciar con Windows" };

    public VentanaConfiguracion(Config actual) {
        // Trabaja sobre una copia: si cancela, la config original no cambia
        Resultado = actual.Copiar();

        Text = "LiveSnip - Configuracion";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(440, 305);
        Font = new Font("Segoe UI", 9);
        TopMost = true;

        // Atajo: se captura pulsando la combinacion
        Controls.Add(new Label { Text = "Atajo (haga clic y pulse la combinacion):", Left = 15, Top = 15, AutoSize = true });
        _atajo.SetBounds(15, 38, 410, 25);
        _atajo.Text = Resultado.Atajo;
        _atajo.KeyDown += CapturarAtajo;
        Controls.Add(_atajo);

        // Aspecto por defecto
        Controls.Add(new Label { Text = "Aspecto por defecto:", Left = 15, Top = 78, AutoSize = true });
        _aspecto.SetBounds(15, 101, 150, 25);
        foreach (var r in RelacionAspecto.Todas) _aspecto.Items.Add(r.Nombre);
        _aspecto.SelectedIndex = Resultado.Relacion.Indice;
        Controls.Add(_aspecto);

        // Carpeta
        Controls.Add(new Label { Text = "Carpeta de guardado (vacio = automatica):", Left = 15, Top = 138, AutoSize = true });
        _carpeta.SetBounds(15, 161, 320, 25);
        _carpeta.Text = Resultado.Carpeta;
        Controls.Add(_carpeta);

        var elegir = new Button { Text = "Elegir...", Left = 343, Top = 160, Width = 82, Height = 27 };
        elegir.Click += (_, _) => {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK) _carpeta.Text = dlg.SelectedPath;
        };
        Controls.Add(elegir);

        // Nombre de archivo
        _pedirNombre.SetBounds(15, 197, 410, 24);
        _pedirNombre.Checked = Resultado.PedirNombre;
        Controls.Add(_pedirNombre);

        // Arranque con Windows
        _iniciar.SetBounds(15, 224, 410, 24);
        _iniciar.Checked = Resultado.IniciarConWindows;
        Controls.Add(_iniciar);

        var guardar = new Button { Text = "Guardar", DialogResult = DialogResult.OK, Left = 255, Top = 264, Width = 85 };
        var cancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Left = 345, Top = 264, Width = 80 };
        guardar.Click += (_, _) => {
            Resultado.Aspecto = _aspecto.SelectedIndex;
            Resultado.Carpeta = _carpeta.Text.Trim();
            Resultado.PedirNombre = _pedirNombre.Checked;
            Resultado.IniciarConWindows = _iniciar.Checked;
        };
        Controls.Add(guardar);
        Controls.Add(cancelar);
        AcceptButton = guardar;
        CancelButton = cancelar;
    }

    // Lee la combinacion pulsada, exige al menos Ctrl, Alt o Shift para funcionar
    void CapturarAtajo(object? sender, KeyEventArgs e) {
        e.SuppressKeyPress = true;
        e.Handled = true;

        var tecla = e.KeyCode;
        if (tecla is Keys.ControlKey or Keys.ShiftKey or Keys.Menu or Keys.LWin or Keys.RWin) return;

        uint mod = 0;
        if (e.Control) mod |= ModificadorTecla.Ctrl;
        if (e.Alt) mod |= ModificadorTecla.Alt;
        if (e.Shift) mod |= ModificadorTecla.Shift;
        if (mod == 0) return; // una tecla sola molestaria al escribir y no tiene sentido

        Resultado.Modificadores = mod;
        Resultado.Tecla = (int)tecla;
        _atajo.Text = Config.Describir(mod, tecla);
    }
}
