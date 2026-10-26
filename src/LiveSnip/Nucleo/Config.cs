using System.Text.Json;
using System.Text.Json.Serialization;

namespace LiveSnip.Nucleo;

// Ajustes del usuario, guardado en %LOCALAPPDATA%\LiveSnip\config.json
public class Config {
    // Combinacion de Modificador + Tecla (Ctrl + Alt por defecto)
    public uint Modificadores { get; set; } = ModificadorTecla.Ctrl | ModificadorTecla.Alt;
    public int Tecla { get; set; } = (int)Keys.S;

    // Atajo para repetir la ultima captura (Ctrl + Alt + R por defecto)
    public uint ModificadoresRepetir { get; set; } = ModificadorTecla.Ctrl | ModificadorTecla.Alt;
    public int TeclaRepetir { get; set; } = (int)Keys.R;

    // Atajo para capturar la pantalla completa (Ctrl + Alt + F por defecto)
    public uint ModificadoresPantallaCompleta { get; set; } = ModificadorTecla.Ctrl | ModificadorTecla.Alt;
    public int TeclaPantallaCompleta { get; set; } = (int)Keys.F;

    public string Carpeta { get; set; } = "";       // vacio = automatica
    public int Aspecto { get; set; } = 0;           // indice en RelacionAspecto.Todas (6 = personalizada)
    public int AnchoPersonalizado { get; set; } = 21;  // numeros de la relacion personalizada (21:9)
    public int AltoPersonalizado { get; set; } = 9;
    public int OscuridadCapa { get; set; } = 40;   // porcentaje, a mas alto mas oscuro el fondo negro (40 = opacidad 0.4)
    public DestinoCaptura Destino { get; set; } = DestinoCaptura.CopiarYGuardar;   // a donde va cada captura
    public bool MostrarAviso { get; set; } = true;  // muestra el globo "Copiada y guardada en..."
    public bool PedirNombre { get; set; } = false;  // pregunta el nombre al guardar
    public bool IniciarConWindows { get; set; } = false; // arrancar con Windows

    // Limites de la oscuridad de la capa, en porcentaje
    public const int OscuridadMinima = 0;
    public const int OscuridadMaxima = 90;

    static readonly string Ruta = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LiveSnip", "config.json");

    [JsonIgnore] public string Atajo => Describir(Modificadores, (Keys)Tecla);
    [JsonIgnore] public string AtajoRepetir => Describir(ModificadoresRepetir, (Keys)TeclaRepetir);
    [JsonIgnore] public string AtajoPantallaCompleta => Describir(ModificadoresPantallaCompleta, (Keys)TeclaPantallaCompleta);
    // Si el config.json trae un numero que no existe, se comporta como copiar y guardar
    [JsonIgnore] public bool CopiaAlPortapapeles => Destino != DestinoCaptura.SoloGuardar;
    [JsonIgnore] public bool GuardaEnArchivo => Destino != DestinoCaptura.SoloCopiar;
    [JsonIgnore] public RelacionAspecto Personalizada => RelacionAspecto.Personalizada(AnchoPersonalizado, AltoPersonalizado);
    // Con opacidad 0 Windows deja pasar el mouse a lo que hay debajo y no se podria seleccionar,
    // asi que se deja un minimo que no se nota pero que sigue recibiendo el mouse
    const double OpacidadMinimaReal = 0.01;

    // Opacidad que se le pone a la capa (de 0.01 a 0.9), corrige cualquier valor fuera de rango del config.json
    [JsonIgnore] public double OpacidadCapa =>
        Math.Max(OpacidadMinimaReal, Math.Clamp(OscuridadCapa, OscuridadMinima, OscuridadMaxima) / 100.0);
    [JsonIgnore] public RelacionAspecto Relacion =>
        Aspecto == RelacionAspecto.IndicePersonalizada ? Personalizada : RelacionAspecto.Desde(Aspecto);

    // Devuelve la config, si es la primera vez que se abre la app y si el archivo estaba danado
    public static (Config Config, bool PrimeraVez, bool Danado) Cargar() {
        try {
            if (File.Exists(Ruta))
                return (JsonSerializer.Deserialize<Config>(File.ReadAllText(Ruta)) ?? new Config(), false, false);
        } catch {
            // archivo danado: se usan los valores por defecto y se avisa
            return (new Config(), true, true);
        }
        return (new Config(), true, false);
    }

    public void Guardar() {
        try {
            Directory.CreateDirectory(Path.GetDirectoryName(Ruta)!);
            File.WriteAllText(Ruta, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        } catch { /* si no se puede guardar, sigue funcionando con lo actual */ }
    }

    public Config Copiar() => (Config)MemberwiseClone();

    // "Ctrl + Alt + S" de default 
    public static string Describir(uint modificadores, Keys tecla) {
        var partes = new List<string>();
        if ((modificadores & ModificadorTecla.Ctrl) != 0) partes.Add("Ctrl");
        if ((modificadores & ModificadorTecla.Alt) != 0) partes.Add("Alt");
        if ((modificadores & ModificadorTecla.Shift) != 0) partes.Add("Shift");
        partes.Add(tecla.ToString());
        return string.Join(" + ", partes);
    }
}
