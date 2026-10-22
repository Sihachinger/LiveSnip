using LiveSnip.Nucleo;

namespace LiveSnip.Tests;

public class AlmacenCapturasTests {
    [Fact]
    public void LimpiarNombre_DejaElNombreNormal() {
        var nombre = AlmacenCapturas.LimpiarNombre("foto", "sugerido");

        Assert.Equal("foto", nombre);
    }

    [Fact]
    public void LimpiarNombre_QuitaLaExtensionPng() {
        var nombre = AlmacenCapturas.LimpiarNombre("foto.png", "sugerido");

        Assert.Equal("foto", nombre);
    }

    [Fact]
    public void LimpiarNombre_QuitaCaracteresInvalidos() {
        var nombre = AlmacenCapturas.LimpiarNombre("a:b?c", "sugerido");

        Assert.Equal("abc", nombre);
    }

    [Fact]
    public void LimpiarNombre_SiQuedaVacio_UsaElSugerido() {
        var nombre = AlmacenCapturas.LimpiarNombre("   ", "sugerido");

        Assert.Equal("sugerido", nombre);
    }

    [Fact]
    public void Configurar_ConRutaRelativa_LaDescartaYUsaUnaCompleta() {
        var almacen = new AlmacenCapturas();

        almacen.Configurar("LiveSnipRelativa");

        Assert.True(Path.IsPathFullyQualified(almacen.Carpeta));
    }
}
