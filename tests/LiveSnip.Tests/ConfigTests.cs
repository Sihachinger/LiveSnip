using LiveSnip.Nucleo;

namespace LiveSnip.Tests;

public class ConfigTests {
    [Fact]
    public void AtajoPorDefecto_EsCtrlAltS() {
        var config = new Config();

        Assert.Equal("Ctrl + Alt + S", config.Atajo);
    }

    [Fact]
    public void Describir_PoneLosModificadoresEnOrden() {
        var texto = Config.Describir(ModificadorTecla.Shift | ModificadorTecla.Ctrl, Keys.F5);

        Assert.Equal("Ctrl + Shift + F5", texto);
    }

    [Fact]
    public void PersonalizadaPorDefecto_Es21a9() {
        var config = new Config();

        Assert.Equal("21:9", config.Personalizada.Nombre);
    }

    [Fact]
    public void Relacion_ConIndiceDeLaPersonalizada_DevuelveLaPersonalizada() {
        var config = new Config { Aspecto = RelacionAspecto.IndicePersonalizada, AnchoPersonalizado = 5, AltoPersonalizado = 4 };

        Assert.Equal("5:4", config.Relacion.Nombre);
    }

    [Fact]
    public void Relacion_ConIndiceDeLaLista_DevuelveLaDeLaLista() {
        var config = new Config { Aspecto = 3 };

        Assert.Equal("16:9", config.Relacion.Nombre);
    }

    [Fact]
    public void AtajoRepetirPorDefecto_EsCtrlAltR() {
        var config = new Config();

        Assert.Equal("Ctrl + Alt + R", config.AtajoRepetir);
    }

    [Fact]
    public void TransparenciaPorDefecto_DejaLaOpacidadDeSiempre() {
        var config = new Config();

        // 60 % de transparencia es la opacidad 0.4 que tenia la capa antes del ajuste
        Assert.Equal(0.4, config.OpacidadCapa, 5);
    }

    [Fact]
    public void Transparencia_ConUnValorMuyAlto_SeCorrigeAlMaximo() {
        var config = new Config { TransparenciaCapa = 200 };

        // El maximo es 90 %, o sea opacidad 0.1
        Assert.Equal(0.1, config.OpacidadCapa, 5);
    }

    [Fact]
    public void Transparencia_ConUnValorNegativo_SeCorrigeAlMinimo() {
        var config = new Config { TransparenciaCapa = -5 };

        // El minimo es 0 %, o sea opacidad 1 (capa negra del todo)
        Assert.Equal(1.0, config.OpacidadCapa, 5);
    }

    [Fact]
    public void Copiar_NoCambiaElOriginal() {
        var original = new Config { Carpeta = "A" };

        var copia = original.Copiar();
        copia.Carpeta = "B";

        Assert.Equal("A", original.Carpeta);
    }
}
