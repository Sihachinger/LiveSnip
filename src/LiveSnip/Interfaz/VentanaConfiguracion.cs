using LiveSnip.Nucleo;

namespace LiveSnip.Interfaz;

// Ventana de configuracion: atajos, aspecto por defecto, transparencia de la capa, carpeta y nombre
class VentanaConfiguracion : Form {
    public Config Resultado { get; }

    readonly TextBox _atajo = new() { ReadOnly = true, Cursor = Cursors.Hand };
    readonly TextBox _atajoRepetir = new() { ReadOnly = true, Cursor = Cursors.Hand };
    readonly ComboBox _aspecto = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    readonly NumericUpDown _ancho = new() { Minimum = RelacionAspecto.MinimoPersonalizado, Maximum = RelacionAspecto.MaximoPersonalizado };
    readonly NumericUpDown _alto = new() { Minimum = RelacionAspecto.MinimoPersonalizado, Maximum = RelacionAspecto.MaximoPersonalizado };
    readonly TrackBar _transparencia = new() {
        Minimum = Config.TransparenciaMinima, Maximum = Config.TransparenciaMaxima,
        TickFrequency = 10, SmallChange = 1, LargeChange = 10
    };
    readonly Label _transparenciaValor = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
    readonly TextBox _carpeta = new();
    readonly CheckBox _pedirNombre = new() { Text = "Preguntar el nombre al guardar cada captura" };
    readonly CheckBox _iniciar = new() { Text = "Iniciar con Windows" };

    public VentanaConfiguracion(Config actual) {
        // Trabaja sobre una copia: si cancela, la config original no cambia
        Resultado = actual.Copiar();

        Text = "LiveSnip - Configuración";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(440, 428);
        Font = new Font("Segoe UI", 9);
        TopMost = true;

        // Atajo: se captura pulsando la combinacion
        Controls.Add(new Label { Text = "Atajo (haga clic y pulse la combinación):", Left = 15, Top = 15, AutoSize = true });
        _atajo.SetBounds(15, 38, 410, 25);
        _atajo.Text = Resultado.Atajo;
        _atajo.KeyDown += CapturarAtajo;
        Controls.Add(_atajo);

        // Atajo para repetir la ultima captura, se elige igual que el anterior
        Controls.Add(new Label { Text = "Atajo para repetir la última captura:", Left = 15, Top = 78, AutoSize = true });
        _atajoRepetir.SetBounds(15, 101, 410, 25);
        _atajoRepetir.Text = Resultado.AtajoRepetir;
        _atajoRepetir.KeyDown += CapturarAtajo;
        Controls.Add(_atajoRepetir);

        // Aspecto por defecto
        Controls.Add(new Label { Text = "Aspecto por defecto:", Left = 15, Top = 141, AutoSize = true });
        _aspecto.SetBounds(15, 164, 150, 25);
        foreach (var r in RelacionAspecto.Todas) _aspecto.Items.Add(r.Nombre);
        _aspecto.Items.Add("Personalizada"); // ultima opcion: usa los dos numeros de al lado
        _aspecto.SelectedIndex = Math.Clamp(Resultado.Aspecto, 0, RelacionAspecto.IndicePersonalizada);
        _aspecto.SelectedIndexChanged += (_, _) => ActualizarPersonalizada();
        Controls.Add(_aspecto);

        // Numeros de la relacion personalizada (ancho : alto), solo se usan si esta elegida
        _ancho.SetBounds(180, 164, 55, 25);
        // Clamp: un valor fuera de rango en config.json haria fallar la casilla
        _ancho.Value = Math.Clamp(Resultado.AnchoPersonalizado, RelacionAspecto.MinimoPersonalizado, RelacionAspecto.MaximoPersonalizado);
        Controls.Add(_ancho);
        Controls.Add(new Label { Text = ":", Left = 240, Top = 167, AutoSize = true });
        _alto.SetBounds(255, 164, 55, 25);
        _alto.Value = Math.Clamp(Resultado.AltoPersonalizado, RelacionAspecto.MinimoPersonalizado, RelacionAspecto.MaximoPersonalizado);
        Controls.Add(_alto);
        ActualizarPersonalizada();

        // Transparencia de la capa: mas alto se ve mas lo de abajo
        Controls.Add(new Label { Text = "Transparencia de la capa de selección:", Left = 15, Top = 201, AutoSize = true });
        _transparencia.SetBounds(15, 221, 340, 40);
        _transparencia.Value = Math.Clamp(Resultado.TransparenciaCapa, Config.TransparenciaMinima, Config.TransparenciaMaxima);
        _transparencia.ValueChanged += (_, _) => ActualizarTransparencia();
        Controls.Add(_transparencia);
        _transparenciaValor.SetBounds(365, 226, 60, 20);
        Controls.Add(_transparenciaValor);
        ActualizarTransparencia();

        // Carpeta
        Controls.Add(new Label { Text = "Carpeta de guardado (vacío = automática):", Left = 15, Top = 261, AutoSize = true });
        _carpeta.SetBounds(15, 284, 320, 25);
        _carpeta.Text = Resultado.Carpeta;
        Controls.Add(_carpeta);

        var elegir = new Button { Text = "Elegir...", Left = 343, Top = 283, Width = 82, Height = 27 };
        elegir.Click += (_, _) => {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK) _carpeta.Text = dlg.SelectedPath;
        };
        Controls.Add(elegir);

        // Nombre de archivo
        _pedirNombre.SetBounds(15, 320, 410, 24);
        _pedirNombre.Checked = Resultado.PedirNombre;
        Controls.Add(_pedirNombre);

        // Arranque con Windows
        _iniciar.SetBounds(15, 347, 410, 24);
        _iniciar.Checked = Resultado.IniciarConWindows;
        Controls.Add(_iniciar);

        var guardar = new Button { Text = "Guardar", DialogResult = DialogResult.OK, Left = 255, Top = 387, Width = 85 };
        var cancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Left = 345, Top = 387, Width = 80 };
        guardar.Click += (_, _) => {
            Resultado.Aspecto = _aspecto.SelectedIndex;
            Resultado.AnchoPersonalizado = (int)_ancho.Value;
            Resultado.AltoPersonalizado = (int)_alto.Value;
            Resultado.TransparenciaCapa = _transparencia.Value;
            Resultado.Carpeta = _carpeta.Text.Trim();
            Resultado.PedirNombre = _pedirNombre.Checked;
            Resultado.IniciarConWindows = _iniciar.Checked;
        };
        Controls.Add(guardar);
        Controls.Add(cancelar);
        AcceptButton = guardar;
        CancelButton = cancelar;
    }

    // Muestra el porcentaje elegido al lado del deslizador
    void ActualizarTransparencia() {
        _transparenciaValor.Text = $"{_transparencia.Value} %";
    }

    // Los numeros solo se pueden editar si esta elegida la personalizada
    void ActualizarPersonalizada() {
        bool personalizada = _aspecto.SelectedIndex == RelacionAspecto.IndicePersonalizada;
        _ancho.Enabled = personalizada;
        _alto.Enabled = personalizada;
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

        // El mismo evento sirve para los dos campos, sender dice cual se esta editando
        bool esElDeRepetir = sender == _atajoRepetir;

        // Los dos atajos no pueden ser iguales, si lo son se ignora la combinacion
        uint modOtro = esElDeRepetir ? Resultado.Modificadores : Resultado.ModificadoresRepetir;
        int teclaOtro = esElDeRepetir ? Resultado.Tecla : Resultado.TeclaRepetir;
        if (mod == modOtro && (int)tecla == teclaOtro) return;

        if (esElDeRepetir) {
            Resultado.ModificadoresRepetir = mod;
            Resultado.TeclaRepetir = (int)tecla;
            _atajoRepetir.Text = Config.Describir(mod, tecla);
        } else {
            Resultado.Modificadores = mod;
            Resultado.Tecla = (int)tecla;
            _atajo.Text = Config.Describir(mod, tecla);
        }
    }
}
