using LiveSnip.Interfaz;
using LiveSnip.Nucleo;

namespace LiveSnip;

static class Program {
    [STAThread]
    static void Main() {
        ApplicationConfiguration.Initialize();

        // Solo una instancia a la vez
        using var mutex = new Mutex(true, "LiveSnip.SingleInstance", out bool esPrimera);
        if (!esPrimera) {
            // Avisa que ya hay una instancia abierta
            MessageBox.Show("LiveSnip ya está abierto.\nBusque su icono en la bandeja del sistema, junto al reloj.",
                "LiveSnip", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Aca van las piezas y se le pasa a la app
        Application.Run(new AppBandeja(new CapturadorPantalla(), new AlmacenCapturas(), new InicioAutomatico()));
    }
}
