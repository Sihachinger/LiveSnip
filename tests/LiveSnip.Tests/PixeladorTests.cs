using LiveSnip.Nucleo;

namespace LiveSnip.Tests;

public class PixeladorTests {
    // Imagen de 4x4 con la mitad izquierda blanca y la derecha negra
    static Bitmap ImagenDeDosColores() {
        var imagen = new Bitmap(4, 4);
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                imagen.SetPixel(x, y, x < 2 ? Color.White : Color.Black);
        return imagen;
    }

    [Fact]
    public void Pixelar_ConUnBloqueGrande_PintaTodoConElPromedio() {
        using var imagen = ImagenDeDosColores();

        Pixelador.Pixelar(imagen, new Rectangle(0, 0, 4, 4), 4);

        Assert.Equal(127, imagen.GetPixel(0, 0).R);
    }

    [Fact]
    public void Pixelar_NoTocaLoQueQuedaFueraDeLaZona() {
        using var imagen = ImagenDeDosColores();

        Pixelador.Pixelar(imagen, new Rectangle(2, 0, 2, 4), 2);

        Assert.Equal(Color.White.ToArgb(), imagen.GetPixel(0, 0).ToArgb());
    }

    [Fact]
    public void Pixelar_ConUnaZonaFueraDeLaImagen_NoFalla() {
        using var imagen = ImagenDeDosColores();

        Pixelador.Pixelar(imagen, new Rectangle(10, 10, 5, 5), 2);

        Assert.Equal(Color.White.ToArgb(), imagen.GetPixel(0, 0).ToArgb());
    }
}
