using LiveSnip.Nucleo;

namespace LiveSnip.Interfaz;

// Cuadro pequeno para escribir el nombre de la captura
class VentanaNombre : Form {
    readonly TextBox _texto = new();

    VentanaNombre(string sugerido) {
        Text = "LiveSnip - Nombre";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(360, 95);
        Font = new Font("Segoe UI", 9);
        TopMost = true;

        Controls.Add(new Label { Text = "Nombre de la captura:", Left = 15, Top = 12, AutoSize = true });
        _texto.SetBounds(15, 34, 330, 25);
        _texto.Text = sugerido;
        Controls.Add(_texto);

        var ok = new Button { Text = "Guardar", DialogResult = DialogResult.OK, Left = 175, Top = 63, Width = 85 };
        var cancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Left = 265, Top = 63, Width = 80 };
        Controls.Add(ok);
        Controls.Add(cancelar);
        AcceptButton = ok;
        CancelButton = cancelar;
    }

    // Pregunta el nombre, si se cancela o queda vacio devuelve el sugerido
    public static string Pedir(string sugerido) {
        using var dlg = new VentanaNombre(sugerido);
        if (dlg.ShowDialog() != DialogResult.OK) return sugerido;
        return AlmacenCapturas.LimpiarNombre(dlg._texto.Text, sugerido);
    }

    // Deja el texto seleccionado para escribir encima directamente asi es más rapido
    protected override void OnShown(EventArgs e) {
        base.OnShown(e);
        Activate();
        _texto.SelectAll();
        _texto.Focus();
    }
}
