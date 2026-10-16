using System.Drawing.Imaging;

namespace LiveSnip.Nucleo;

// Saca una imagen de una zona de la pantalla Bitmap
public interface ICapturadorPantalla {
    Bitmap Capturar(Rectangle zona);
}

public sealed class CapturadorPantalla : ICapturadorPantalla {
    public Bitmap Capturar(Rectangle zona) {
        var imagen = new Bitmap(zona.Width, zona.Height, PixelFormat.Format32bppArgb);
        using var grafico = Graphics.FromImage(imagen);
        grafico.CopyFromScreen(zona.Location, Point.Empty, zona.Size);
        return imagen;
    }
}
