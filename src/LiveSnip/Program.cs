using System.Runtime.InteropServices;
using LiveSnip.Interfaz;
using LiveSnip.Nucleo;

namespace LiveSnip;

static class Program {
    const string NombreMutex = "LiveSnip.SingleInstance";
    const string NombreEvento = "LiveSnip.PedirConfiguracion";
    const int ASFW_ANY = -1;   // deja que la copia abierta pase su ventana al frente

    [STAThread]
    static void Main() {
        // Solo una instancia a la vez
        using var mutex = new Mutex(true, NombreMutex, out bool esPrimera);
        if (!esPrimera) {
            AvisarALaCopiaAbierta();
            return;
        }

        ApplicationConfiguration.Initialize();

        // Aca van las piezas y se le pasa a la app
        using var pedidoDeConfiguracion = new EventWaitHandle(false, EventResetMode.AutoReset, NombreEvento);
        Application.Run(new AppBandeja(new CapturadorPantalla(), new AlmacenCapturas(), new InicioAutomatico(), pedidoDeConfiguracion));
    }

    // La segunda copia no abre nada, le pide a la que ya esta abierta que muestre su configuracion
    static void AvisarALaCopiaAbierta() {
        if (!EventWaitHandle.TryOpenExisting(NombreEvento, out var pedido)) return;

        using (pedido) {
            AllowSetForegroundWindow(ASFW_ANY);
            pedido.Set();
        }
    }

    [DllImport("user32.dll")] static extern bool AllowSetForegroundWindow(int dwProcessId);
}
