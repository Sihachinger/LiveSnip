using System.Runtime.InteropServices;

namespace LiveSnip.Interfaz;

// Ventana invisible que solo escucha el atajo global (WM_HOTKEY)
class VentanaAtajo : NativeWindow, IDisposable {
    const int WM_HOTKEY = 0x0312;
    const uint MOD_NOREPEAT = 0x4000; // para que no se repita si se deja pulsado
    const int Id = 1;
    readonly Action _alPresionar;

    public VentanaAtajo(Action alPresionar) {
        _alPresionar = alPresionar;
        CreateHandle(new CreateParams());
    }

    // Reemplaza el atajo anterior, false si otra app ya lo usa
    public bool Registrar(uint modificadores, uint tecla) {
        UnregisterHotKey(Handle, Id);
        return RegisterHotKey(Handle, Id, modificadores | MOD_NOREPEAT, tecla);
    }

    // Windows avisa aca cuando se pulsa el atajo
    protected override void WndProc(ref Message m) {
        if (m.Msg == WM_HOTKEY && m.WParam == Id) _alPresionar();
        base.WndProc(ref m);
    }

    public void Dispose() {
        UnregisterHotKey(Handle, Id);
        DestroyHandle();
    }

    [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
