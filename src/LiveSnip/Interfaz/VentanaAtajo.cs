using System.Runtime.InteropServices;

namespace LiveSnip.Interfaz;

// Ventana invisible que solo escucha los atajos globales (WM_HOTKEY)
// Cada atajo tiene un numero propio (id), asi se sabe cual de todos se pulso
class VentanaAtajo : NativeWindow, IDisposable {
    const int WM_HOTKEY = 0x0312;
    const uint MOD_NOREPEAT = 0x4000; // para que no se repita si se deja pulsado
    readonly Action<int> _alPresionar;
    readonly HashSet<int> _registrados = new();   // ids que estan registrados ahora en Windows

    public VentanaAtajo(Action<int> alPresionar) {
        _alPresionar = alPresionar;
        CreateHandle(new CreateParams());
    }

    // Reemplaza el atajo anterior de ese id, false si otra app ya lo usa
    public bool Registrar(int id, uint modificadores, uint tecla) {
        UnregisterHotKey(Handle, id);
        _registrados.Remove(id);

        bool ok = RegisterHotKey(Handle, id, modificadores | MOD_NOREPEAT, tecla);
        if (ok) _registrados.Add(id);
        return ok;
    }

    // Windows avisa aca cuando se pulsa un atajo, y WParam trae el id
    protected override void WndProc(ref Message m) {
        if (m.Msg == WM_HOTKEY) _alPresionar((int)m.WParam);
        base.WndProc(ref m);
    }

    public void Dispose() {
        foreach (int id in _registrados) UnregisterHotKey(Handle, id);
        _registrados.Clear();
        DestroyHandle();
    }

    [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
