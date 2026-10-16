using LiveSnip.Nucleo;

namespace LiveSnip.Tests;

public class RelacionAspectoTests {
    // Monitor de 1000 x 1000 para los ejemplos
    Size monitor = new Size(1000, 1000);

    [Fact]
    public void Libre_ArrastrandoHaciaAbajo() {
        var libre = RelacionAspecto.Desde(0);

        var resultado = libre.Ajustar(new Point(100, 100), new Point(200, 150), monitor);

        Assert.Equal(new Rectangle(100, 100, 100, 50), resultado);
    }

    [Fact]
    public void Libre_ArrastrandoHaciaArriba() {
        var libre = RelacionAspecto.Desde(0);

        var resultado = libre.Ajustar(new Point(200, 200), new Point(100, 150), monitor);

        // El rectangulo empieza donde termino el mouse
        Assert.Equal(new Rectangle(100, 150, 100, 50), resultado);
    }

    [Fact]
    public void Cuadrada_QuedaConAnchoIgualAlAlto() {
        var cuadrada = RelacionAspecto.Desde(1);

        var resultado = cuadrada.Ajustar(new Point(100, 100), new Point(300, 150), monitor);

        Assert.Equal(new Rectangle(100, 100, 200, 200), resultado);
    }

    [Fact]
    public void Cuadrada_NoSeSaleDelMonitor() {
        var cuadrada = RelacionAspecto.Desde(1);

        var resultado = cuadrada.Ajustar(new Point(900, 100), new Point(1100, 150), monitor);

        Assert.Equal(new Rectangle(900, 100, 100, 100), resultado);
    }

    [Fact]
    public void Desde_ConIndiceMuyGrande_DevuelveLaUltima() {
        var relacion = RelacionAspecto.Desde(99);

        Assert.Equal("3:2", relacion.Nombre);
    }

    [Fact]
    public void Desde_ConIndiceNegativo_DevuelveLibre() {
        var relacion = RelacionAspecto.Desde(-5);

        Assert.Equal("Libre", relacion.Nombre);
    }
}
