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
    public void OscuridadPorDefecto_DejaLaOpacidadDeSiempre() {
        var config = new Config();

        // 40 % de oscuridad es la opacidad 0.4 que tenia la capa antes del ajuste
        Assert.Equal(0.4, config.OpacidadCapa, 5);
    }

    [Fact]
    public void Oscuridad_ConUnValorMuyAlto_SeCorrigeAlMaximo() {
        var config = new Config { OscuridadCapa = 200 };

        // El maximo es 90 %, o sea casi negro
        Assert.Equal(0.9, config.OpacidadCapa, 5);
    }

    [Fact]
    public void Oscuridad_ConUnValorNegativo_SeCorrigeAlMinimo() {
        var config = new Config { OscuridadCapa = -5 };

        // El minimo es 0 %, que se queda en 0.01 para que la capa siga recibiendo el mouse
        Assert.Equal(0.01, config.OpacidadCapa, 5);
    }

    [Fact]
    public void AvisoPorDefecto_EstaActivado() {
        var config = new Config();

        Assert.True(config.MostrarAviso);
    }

    [Fact]
    public void DestinoPorDefecto_CopiaAlPortapapeles() {
        var config = new Config();

        Assert.True(config.CopiaAlPortapapeles);
    }

    [Fact]
    public void DestinoPorDefecto_GuardaEnArchivo() {
        var config = new Config();

        Assert.True(config.GuardaEnArchivo);
    }

    [Fact]
    public void SoloCopiar_NoGuardaEnArchivo() {
        var config = new Config { Destino = DestinoCaptura.SoloCopiar };

        Assert.False(config.GuardaEnArchivo);
    }

    [Fact]
    public void SoloGuardar_NoCopiaAlPortapapeles() {
        var config = new Config { Destino = DestinoCaptura.SoloGuardar };

        Assert.False(config.CopiaAlPortapapeles);
    }

    [Fact]
    public void Destino_ConUnNumeroQueNoExiste_CopiaYGuarda() {
        var config = new Config { Destino = (DestinoCaptura)7 };

        // Se comporta como la opcion por defecto, asi no se pierde la captura
        Assert.True(config.CopiaAlPortapapeles && config.GuardaEnArchivo);
    }

    [Fact]
    public void Copiar_NoCambiaElOriginal() {
        var original = new Config { Carpeta = "A" };

        var copia = original.Copiar();
        copia.Carpeta = "B";

        Assert.Equal("A", original.Carpeta);
    }

    [Fact]
    public void LaFechaYLaMarcaDeAgua_VienenDesactivadasPorDefecto() {
        var config = new Config();

        Assert.False(config.AgregarFecha || config.MarcaDeAgua != "");
    }

    [Fact]
    public void Copiar_TambienCopiaLaMarcaDeAgua() {
        var copia = new Config { MarcaDeAgua = "LiveSnip" }.Copiar();

        Assert.Equal("LiveSnip", copia.MarcaDeAgua);
    }
}
