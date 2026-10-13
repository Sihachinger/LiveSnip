namespace LiveSnip;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Solo una instancia a la vez
        using var mutex = new Mutex(true, "LiveSnip.SingleInstance", out bool esPrimera);
        if (!esPrimera) return;

        ApplicationConfiguration.Initialize();
    }
}
