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
    public void Copiar_NoCambiaElOriginal() {
        var original = new Config { Carpeta = "A" };

        var copia = original.Copiar();
        copia.Carpeta = "B";

        Assert.Equal("A", original.Carpeta);
    }
}
