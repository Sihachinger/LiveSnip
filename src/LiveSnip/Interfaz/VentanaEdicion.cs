namespace LiveSnip.Interfaz;

// Ventana para retocar la captura antes de guardarla, muestra la imagen y se guarda o se cancela
// Las herramientas de edicion se agregan en los pasos siguientes
class VentanaEdicion : Form {
    const int AltoBotones = 50;
    const int MargenVentana = 80;   // espacio que se deja libre alrededor de la ventana en el monitor

    readonly Bitmap _imagen;                       // copia de trabajo, la original no se toca
    readonly Panel _zonaImagen = new();            // con barras de desplazamiento si la imagen no entra
    readonly PictureBox _vista = new();
    readonly FlowLayoutPanel _barraBotones = new();   // los botones se acomodan desde la derecha

    VentanaEdicion(Bitmap original) {
        _imagen = new Bitmap(original);

        Text = "LiveSnip - Editar captura";
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;
        Font = new Font("Segoe UI", 9);
        TopMost = true;
        ClientSize = TamanoInicial();
        MinimumSize = new Size(380, 200);

        _vista.SizeMode = PictureBoxSizeMode.AutoSize;
        _vista.Image = _imagen;
        _zonaImagen.Dock = DockStyle.Fill;
        _zonaImagen.AutoScroll = true;
        _zonaImagen.BackColor = Color.FromArgb(40, 40, 40);
        _zonaImagen.Controls.Add(_vista);

        _barraBotones.Dock = DockStyle.Bottom;
        _barraBotones.Height = AltoBotones;
        AgregarBotones();

        // La zona de la imagen se agrega primero asi los botones quedan abajo sin tapar nada
        Controls.Add(_zonaImagen);
        Controls.Add(_barraBotones);
    }

    // Tamaño que muestra la imagen entera si entra en el monitor y si no, lo que deja el margen
    Size TamanoInicial() {
        var disponible = Screen.FromPoint(Cursor.Position).WorkingArea.Size;
        int ancho = Math.Min(_imagen.Width + 2, disponible.Width - MargenVentana);
        int alto = Math.Min(_imagen.Height + 2 + AltoBotones, disponible.Height - MargenVentana);
        return new Size(Math.Max(ancho, 380), Math.Max(alto, 200));
    }

    void AgregarBotones() {
        var guardar = new Button { Text = "Guardar", DialogResult = DialogResult.OK, Width = 90, Height = 30 };
        var cancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Width = 90, Height = 30 };

        // Los botones van pegados a la derecha, el primero que se agrega queda mas a la derecha
        _barraBotones.FlowDirection = FlowDirection.RightToLeft;
        _barraBotones.Padding = new Padding(10, 10, 10, 0);

        _barraBotones.Controls.Add(cancelar);
        _barraBotones.Controls.Add(guardar);
        AcceptButton = guardar;
        CancelButton = cancelar;
    }

    // Muestra la ventana con la captura, si se guarda devuelve la imagen retocada y si se cancela devuelve null
    // Quien la recibe se encarga de liberarla
    public static Bitmap? Editar(Bitmap captura) {
        using var ventana = new VentanaEdicion(captura);
        if (ventana.ShowDialog() != DialogResult.OK) return null;
        return new Bitmap(ventana._imagen);
    }

    protected override void OnShown(EventArgs e) {
        base.OnShown(e);
        Activate();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            _vista.Image = null;
            _imagen.Dispose();
        }
        base.Dispose(disposing);
    }
}
