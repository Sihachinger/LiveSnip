namespace LiveSnip.Nucleo;

// A donde va cada captura, el numero es el que se guarda en config.json
public enum DestinoCaptura {
    CopiarYGuardar = 0,   // portapapeles y archivo, como siempre
    SoloCopiar = 1,       // solo portapapeles
    SoloGuardar = 2       // solo archivo
}
