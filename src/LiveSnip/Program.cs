using LiveSnip.Interfaz;
using LiveSnip.Nucleo;

namespace LiveSnip;

static class Program {
    [STAThread]
    static void Main() {
        // Solo una instancia a la vez
        using var mutex = new Mutex(true, "LiveSnip.SingleInstance", out bool esPrimera);
        if (!esPrimera) return;

        ApplicationConfiguration.Initialize();

        // Aca van las piezas y se le pasa a la app
        Application.Run(new AppBandeja(new CapturadorPantalla(), new AlmacenCapturas(), new InicioAutomatico()));
    }
}
