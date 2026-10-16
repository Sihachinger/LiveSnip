using System.Drawing.Imaging;

namespace LiveSnip.Nucleo;

// Guarda las capturas en disco eligiendo carpeta y nombre sin pisar otros archivos
public interface IAlmacenCapturas {
    string Carpeta { get; }
    void Configurar(string carpetaPreferida);
    string NombrePorDefecto();
    string Guardar(Bitmap imagen, string nombre);
}

public sealed class AlmacenCapturas : IAlmacenCapturas {
    public string Carpeta { get; private set; } = Path.GetTempPath();

    // Usamos la carpeta elegida si se puede escribir, si no, Imagenes, Escritorio o una local
    public void Configurar(string carpetaPreferida) {
        var candidatas = new List<string>();
        if (carpetaPreferida != "") candidatas.Add(carpetaPreferida);
        candidatas.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "LiveSnip"));
        candidatas.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Capturas LiveSnip")); 
        candidatas.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LiveSnip", "Capturas"));

        foreach (var ruta in candidatas) {
            if (SePuedeEscribir(ruta)) {
                Carpeta = ruta;
                return;
            }
        }
        Carpeta = Path.GetTempPath();
    }

    // Fecha y hora en formato ano-mes-dia-hora-minutos-segundos
    public string NombrePorDefecto() => $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";

    // Guardado como PNG y devuelve la ruta final (agregando -2, -3... si el nombre ya existe)
    public string Guardar(Bitmap imagen, string nombre) {
        var ruta = Path.Combine(Carpeta, nombre + ".png");
        for (int i = 2; File.Exists(ruta); i++)
            ruta = Path.Combine(Carpeta, $"{nombre}-{i}.png");

        // Via stream, Save directo a la ruta falla en carpetas de OneDrive
        using var fs = new FileStream(ruta, FileMode.Create, FileAccess.Write);
        imagen.Save(fs, ImageFormat.Png);
        return ruta;
    }

    // Quitamos caracteres invalidos y la extension .png si el usuario la escribio
    public static string LimpiarNombre(string texto, string sugerido) {
        var limpio = string.Concat(texto.Split(Path.GetInvalidFileNameChars())).Trim();
        if (limpio.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) limpio = limpio[..^4].Trim();
        return limpio == "" ? sugerido : limpio;
    }

    // Prueba real de escritura
    static bool SePuedeEscribir(string ruta) {
        try {
            Directory.CreateDirectory(ruta);
            var prueba = Path.Combine(ruta, ".prueba");
            File.WriteAllText(prueba, "");
            File.Delete(prueba);
            return true;
        }
        catch {
            return false;
        }
    }
}
