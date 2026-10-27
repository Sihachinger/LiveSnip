using LiveSnip.Nucleo;

namespace LiveSnip.Tests;

public class TextoEnImagenTests {
    static Bitmap ImagenBlanca() {
        var imagen = new Bitmap(400, 300);
        using var grafico = Graphics.FromImage(imagen);
        grafico.Clear(Color.White);
        return imagen;
    }

    [Fact]
    public void Dibujar_PintaLaEsquinaElegida() {
        using var imagen = ImagenBlanca();

        TextoEnImagen.Dibujar(imagen, "Hola", EsquinaImagen.AbajoDerecha);

        Assert.NotEqual(Color.White.ToArgb(), imagen.GetPixel(385, 285).ToArgb());
    }

    [Fact]
    public void Dibujar_NoTocaLaEsquinaOpuesta() {
        using var imagen = ImagenBlanca();

        TextoEnImagen.Dibujar(imagen, "Hola", EsquinaImagen.AbajoDerecha);

        Assert.Equal(Color.White.ToArgb(), imagen.GetPixel(5, 5).ToArgb());
    }

    [Fact]
    public void Dibujar_ConTextoVacio_NoCambiaNada() {
        using var imagen = ImagenBlanca();

        TextoEnImagen.Dibujar(imagen, "   ", EsquinaImagen.AbajoDerecha);

        Assert.Equal(Color.White.ToArgb(), imagen.GetPixel(385, 285).ToArgb());
    }

    [Fact]
    public void FormatoFecha_UsaDiaMesAnioYHora() {
        var texto = TextoEnImagen.FormatoFecha(new DateTime(2026, 10, 27, 14, 5, 0));

        Assert.Equal("27/10/2026 14:05", texto);
    }
}
