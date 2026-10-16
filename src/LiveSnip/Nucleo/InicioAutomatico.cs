using Microsoft.Win32;

namespace LiveSnip.Nucleo;

// Controla si la app arranca junto con Windows
public interface IInicioAutomatico {
    bool Activo { get; }
    void Establecer(bool activo);
}

// Usa la clave de inicio del usuario 
public sealed class InicioAutomatico : IInicioAutomatico {
    const string Clave = @"Software\Microsoft\Windows\CurrentVersion\Run";
    const string Nombre = "LiveSnip";

    public bool Activo {
        get {
            using var clave = Registry.CurrentUser.OpenSubKey(Clave);
            return clave?.GetValue(Nombre) is string;
        }
    }

    public void Establecer(bool activo) {
        try {
            using var clave = Registry.CurrentUser.OpenSubKey(Clave, writable: true);
            if (clave is null) return;

            if (activo && Environment.ProcessPath is string ruta)
                clave.SetValue(Nombre, $"\"{ruta}\""); // comillas por si la ruta tiene espacios
            else
                clave.DeleteValue(Nombre, throwOnMissingValue: false);
        } catch { /* sin permiso para el registro: se ignora */ }
    }
}
