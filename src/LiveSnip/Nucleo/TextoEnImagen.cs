using System.Drawing.Drawing2D;
using System.Globalization;
using System.Drawing.Text;

namespace LiveSnip.Nucleo;

// Dibuja un texto en una esquina de la imagen, sirve para la fecha y para la marca de agua
public static class TextoEnImagen {
    const int TamanoMinimo = 12;   // en pixeles, para que se lea aunque la imagen sea chica

    // Formato de la fecha que se dibuja, por ejemplo 27/10/2026 14:35
    public static string FormatoFecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

    // Modifica la imagen directamente, el texto va en blanco sobre un fondo negro semitransparente asi se lee sobre cualquier cosa
    public static void Dibujar(Bitmap imagen, string texto, EsquinaImagen esquina) {
        if (string.IsNullOrWhiteSpace(texto)) return;

        // El tamano acompana a la imagen, asi no queda enorme en una chica ni diminuto en una grande
        float tamano = Math.Max(TamanoMinimo, imagen.Height / 35f);
        int margen = (int)(tamano / 2);

        using var grafico = Graphics.FromImage(imagen);
        grafico.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        grafico.SmoothingMode = SmoothingMode.AntiAlias;
        using var fuente = new Font("Segoe UI", tamano, FontStyle.Regular, GraphicsUnit.Pixel);

        var medida = grafico.MeasureString(texto, fuente);
        var caja = new RectangleF(0, 0, medida.Width + margen, medida.Height + margen / 2f);

        // Esquina elegida, separada del borde de la imagen
        caja.X = esquina is EsquinaImagen.ArribaIzquierda or EsquinaImagen.AbajoIzquierda ? margen : imagen.Width - caja.Width - margen;
        caja.Y = esquina is EsquinaImagen.ArribaIzquierda or EsquinaImagen.ArribaDerecha ? margen : imagen.Height - caja.Height - margen;

        using var fondo = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
        grafico.FillRectangle(fondo, caja);
        grafico.DrawString(texto, fuente, Brushes.White, caja.X + margen / 2f, caja.Y + margen / 4f);
    }
}
