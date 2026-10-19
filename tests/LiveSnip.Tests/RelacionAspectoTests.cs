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

    [Fact]
    public void Personalizada_TieneComoNombreLosDosNumeros() {
        var relacion = RelacionAspecto.Personalizada(21, 9);

        Assert.Equal("21:9", relacion.Nombre);
    }

    [Fact]
    public void Personalizada_SuIndiceEsElDeLaPersonalizada() {
        var relacion = RelacionAspecto.Personalizada(21, 9);

        Assert.Equal(RelacionAspecto.IndicePersonalizada, relacion.Indice);
    }

    [Fact]
    public void Personalizada_IgualQueUnaDeLaLista_NoSeConfundeConElla() {
        var relacion = RelacionAspecto.Personalizada(4, 3);

        Assert.Equal(RelacionAspecto.IndicePersonalizada, relacion.Indice);
    }

    [Fact]
    public void Personalizada_2a1_QuedaConElDobleDeAnchoQueDeAlto() {
        var dosAUno = RelacionAspecto.Personalizada(2, 1);

        var resultado = dosAUno.Ajustar(new Point(0, 0), new Point(200, 10), monitor);

        Assert.Equal(new Rectangle(0, 0, 200, 100), resultado);
    }

    [Fact]
    public void Personalizada_ConCeroEnElAlto_UsaElMinimo() {
        var relacion = RelacionAspecto.Personalizada(16, 0);

        Assert.Equal("16:1", relacion.Nombre);
    }

    [Fact]
    public void Personalizada_ConNumerosMuyGrandes_UsaElMaximo() {
        var relacion = RelacionAspecto.Personalizada(5000, 9);

        Assert.Equal("32:9", relacion.Nombre);
    }
}
