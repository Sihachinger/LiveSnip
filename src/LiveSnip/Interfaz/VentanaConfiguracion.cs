using LiveSnip.Nucleo;

namespace LiveSnip.Interfaz;

// Ventana de configuracion: atajos, aspecto por defecto, oscuridad de la capa, destino, carpeta y nombre
class VentanaConfiguracion : Form {
    public Config Resultado { get; }

    readonly TextBox _atajo = new() { ReadOnly = true, Cursor = Cursors.Hand };
    readonly TextBox _atajoRepetir = new() { ReadOnly = true, Cursor = Cursors.Hand };
    readonly TextBox _atajoPantallaCompleta = new() { ReadOnly = true, Cursor = Cursors.Hand };
    readonly ComboBox _aspecto = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    readonly NumericUpDown _ancho = new() { Minimum = RelacionAspecto.MinimoPersonalizado, Maximum = RelacionAspecto.MaximoPersonalizado };
    readonly NumericUpDown _alto = new() { Minimum = RelacionAspecto.MinimoPersonalizado, Maximum = RelacionAspecto.MaximoPersonalizado };
    readonly TrackBar _oscuridad = new() {
        Minimum = Config.OscuridadMinima, Maximum = Config.OscuridadMaxima,
        TickFrequency = 10, SmallChange = 1, LargeChange = 10
    };
    readonly Label _oscuridadValor = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
    readonly ComboBox _destino = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    readonly TextBox _carpeta = new();
    readonly CheckBox _editar = new() { Text = "Editar recorte antes de guardar" };
    readonly ToolTip _ayuda = new();   // se guarda en un campo para que no lo recoja el recolector de basura
    readonly CheckBox _mostrarAviso = new() { Text = "Mostrar un aviso al terminar cada captura" };
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
        ClientSize = new Size(440, 605);
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

        // Atajo para capturar la pantalla completa, tambien se elige igual
        Controls.Add(new Label { Text = "Atajo para capturar la pantalla completa:", Left = 15, Top = 141, AutoSize = true });
        _atajoPantallaCompleta.SetBounds(15, 164, 410, 25);
        _atajoPantallaCompleta.Text = Resultado.AtajoPantallaCompleta;
        _atajoPantallaCompleta.KeyDown += CapturarAtajo;
        Controls.Add(_atajoPantallaCompleta);

        // Aspecto por defecto
        Controls.Add(new Label { Text = "Aspecto por defecto:", Left = 15, Top = 204, AutoSize = true });
        _aspecto.SetBounds(15, 227, 150, 25);
        foreach (var r in RelacionAspecto.Todas) _aspecto.Items.Add(r.Nombre);
        _aspecto.Items.Add("Personalizada"); // ultima opcion: usa los dos numeros de al lado
        _aspecto.SelectedIndex = Math.Clamp(Resultado.Aspecto, 0, RelacionAspecto.IndicePersonalizada);
        _aspecto.SelectedIndexChanged += (_, _) => ActualizarPersonalizada();
        Controls.Add(_aspecto);

        // Numeros de la relacion personalizada (ancho : alto), solo se usan si esta elegida
        _ancho.SetBounds(180, 227, 55, 25);
        // Clamp: un valor fuera de rango en config.json haria fallar la casilla
        _ancho.Value = Math.Clamp(Resultado.AnchoPersonalizado, RelacionAspecto.MinimoPersonalizado, RelacionAspecto.MaximoPersonalizado);
        Controls.Add(_ancho);
        Controls.Add(new Label { Text = ":", Left = 240, Top = 230, AutoSize = true });
        _alto.SetBounds(255, 227, 55, 25);
        _alto.Value = Math.Clamp(Resultado.AltoPersonalizado, RelacionAspecto.MinimoPersonalizado, RelacionAspecto.MaximoPersonalizado);
        Controls.Add(_alto);
        ActualizarPersonalizada();

        // Oscuridad del fondo negro de la capa: mas alto, mas oscuro
        Controls.Add(new Label { Text = "Oscuridad de la capa de selección:", Left = 15, Top = 264, AutoSize = true });
        _oscuridad.SetBounds(15, 284, 340, 40);
        _oscuridad.Value = Math.Clamp(Resultado.OscuridadCapa, Config.OscuridadMinima, Config.OscuridadMaxima);
        _oscuridad.ValueChanged += (_, _) => ActualizarOscuridad();
        Controls.Add(_oscuridad);
        _oscuridadValor.SetBounds(365, 289, 60, 20);
        Controls.Add(_oscuridadValor);
        ActualizarOscuridad();

        // Destino de cada captura, el orden de la lista es el mismo que el de DestinoCaptura
        Controls.Add(new Label { Text = "Cada captura se:", Left = 15, Top = 324, AutoSize = true });
        _destino.SetBounds(15, 347, 200, 25);
        _destino.Items.AddRange(new object[] { "Copia y se guarda", "Solo se copia", "Solo se guarda" });
        _destino.SelectedIndex = Math.Clamp((int)Resultado.Destino, 0, _destino.Items.Count - 1);
        Controls.Add(_destino);

        // Carpeta
        Controls.Add(new Label { Text = "Carpeta de guardado (vacío = automática):", Left = 15, Top = 384, AutoSize = true });
        _carpeta.SetBounds(15, 407, 320, 25);
        _carpeta.Text = Resultado.Carpeta;
        Controls.Add(_carpeta);

        var elegir = new Button { Text = "Elegir...", Left = 343, Top = 406, Width = 82, Height = 27 };
        elegir.Click += (_, _) => {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK) _carpeta.Text = dlg.SelectedPath;
        };
        Controls.Add(elegir);

        // Nombre de archivo
        _pedirNombre.SetBounds(15, 443, 410, 24);
        _pedirNombre.Checked = Resultado.PedirNombre;
        Controls.Add(_pedirNombre);

        // Modo edicion, se explica al pasar el mouse porque no entra todo en la casilla
        _editar.SetBounds(15, 470, 410, 24);
        _editar.Checked = Resultado.EditarAntesDeGuardar;
        _ayuda.SetToolTip(_editar, "Permite mover y cambiar el tamaño de la zona, difuminar partes y agregar fecha o marca de agua antes de guardar");
        Controls.Add(_editar);

        // Aviso al terminar la captura
        _mostrarAviso.SetBounds(15, 497, 410, 24);
        _mostrarAviso.Checked = Resultado.MostrarAviso;
        Controls.Add(_mostrarAviso);

        // Arranque con Windows
        _iniciar.SetBounds(15, 524, 410, 24);
        _iniciar.Checked = Resultado.IniciarConWindows;
        Controls.Add(_iniciar);

        var guardar = new Button { Text = "Guardar", DialogResult = DialogResult.OK, Left = 255, Top = 564, Width = 85 };
        var cancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Left = 345, Top = 564, Width = 80 };
        guardar.Click += (_, _) => {
            Resultado.Aspecto = _aspecto.SelectedIndex;
            Resultado.AnchoPersonalizado = (int)_ancho.Value;
            Resultado.AltoPersonalizado = (int)_alto.Value;
            Resultado.OscuridadCapa = _oscuridad.Value;
            Resultado.Carpeta = _carpeta.Text.Trim();
            Resultado.Destino = (DestinoCaptura)_destino.SelectedIndex;
            Resultado.MostrarAviso = _mostrarAviso.Checked;
            Resultado.PedirNombre = _pedirNombre.Checked;
            Resultado.EditarAntesDeGuardar = _editar.Checked;
            Resultado.IniciarConWindows = _iniciar.Checked;
        };
        Controls.Add(guardar);
        Controls.Add(cancelar);
        AcceptButton = guardar;
        CancelButton = cancelar;
    }

    // Muestra el porcentaje elegido al lado del deslizador
    void ActualizarOscuridad() {
        _oscuridadValor.Text = $"{_oscuridad.Value} %";
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

        // El mismo evento sirve para los tres campos, sender dice cual se esta editando
        var campo = (TextBox)sender!;

        // Los tres atajos tienen que ser distintos, si la combinacion ya esta en otro campo se ignora
        if (UsadaPorOtroCampo(campo, mod, (int)tecla)) return;

        if (campo == _atajoRepetir) {
            Resultado.ModificadoresRepetir = mod;
            Resultado.TeclaRepetir = (int)tecla;
        } else if (campo == _atajoPantallaCompleta) {
            Resultado.ModificadoresPantallaCompleta = mod;
            Resultado.TeclaPantallaCompleta = (int)tecla;
        } else {
            Resultado.Modificadores = mod;
            Resultado.Tecla = (int)tecla;
        }
        campo.Text = Config.Describir(mod, tecla);
    }

    // True si otro de los tres campos ya tiene esa combinacion
    bool UsadaPorOtroCampo(TextBox campo, uint mod, int tecla) {
        var combinaciones = new List<(TextBox Campo, uint Mod, int Tecla)> {
            (_atajo, Resultado.Modificadores, Resultado.Tecla),
            (_atajoRepetir, Resultado.ModificadoresRepetir, Resultado.TeclaRepetir),
            (_atajoPantallaCompleta, Resultado.ModificadoresPantallaCompleta, Resultado.TeclaPantallaCompleta)
        };
        return combinaciones.Any(c => c.Campo != campo && c.Mod == mod && c.Tecla == tecla);
    }
}
