using LiveSnip.Nucleo;

namespace LiveSnip.Tests;

public class AjusteDeZonaTests {
    static readonly Rectangle Zona = new(100, 100, 300, 200);   // de (100,100) a (400,300)
    static readonly Size Limites = new(1920, 1080);
    static readonly RelacionAspecto Libre = RelacionAspecto.Desde(0);
    static readonly RelacionAspecto DieciseisNueve = RelacionAspecto.Desde(3);

    [Fact]
    public void Libre_EsquinaAbajoDerecha_AgrandaSinMoverLaEsquinaDeArriba() {
        var nueva = AjusteDeZona.Redimensionar(Zona, Tirador.AbajoDerecha, new Point(600, 500), Libre, Limites);

        Assert.Equal(new Rectangle(100, 100, 500, 400), nueva);
    }

    [Fact]
    public void Libre_LadoDerecho_SoloCambiaElAncho() {
        var nueva = AjusteDeZona.Redimensionar(Zona, Tirador.Derecha, new Point(700, 999), Libre, Limites);

        Assert.Equal(new Rectangle(100, 100, 600, 200), nueva);
    }

    [Fact]
    public void Libre_SiLosBordesSeCruzan_LaZonaSeDaVuelta() {
        var nueva = AjusteDeZona.Redimensionar(Zona, Tirador.Derecha, new Point(40, 0), Libre, Limites);

        Assert.Equal(new Rectangle(40, 100, 60, 200), nueva);
    }

    [Fact]
    public void Libre_ConElMouseFueraDelMonitor_SeFrenaEnElBorde() {
        var nueva = AjusteDeZona.Redimensionar(Zona, Tirador.AbajoDerecha, new Point(5000, 5000), Libre, Limites);

        Assert.Equal(new Rectangle(100, 100, 1820, 980), nueva);
    }

    [Fact]
    public void Libre_SiQuedaDemasiadoChica_SeDejaComoEstaba() {
        var nueva = AjusteDeZona.Redimensionar(Zona, Tirador.Derecha, new Point(102, 0), Libre, Limites);

        Assert.Equal(Zona, nueva);
    }

    [Fact]
    public void AspectoFijo_MantieneLaEsquinaDeEnfrente() {
        var nueva = AjusteDeZona.Redimensionar(Zona, Tirador.ArribaIzquierda, new Point(20, 20), DieciseisNueve, Limites);

        Assert.Equal(new Point(400, 300), new Point(nueva.Right, nueva.Bottom));
    }
}
