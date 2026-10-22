using System.Text.Json;
using System.Text.Json.Serialization;

namespace LiveSnip.Nucleo;

// Ajustes del usuario, guardado en %LOCALAPPDATA%\LiveSnip\config.json
public class Config {
    // Combinacion de Modificador + Tecla (Ctrl + Alt por defecto)
    public uint Modificadores { get; set; } = ModificadorTecla.Ctrl | ModificadorTecla.Alt;
    public int Tecla { get; set; } = (int)Keys.S;
    public string Carpeta { get; set; } = "";       // vacio = automatica
    public int Aspecto { get; set; } = 0;           // indice en RelacionAspecto.Todas (6 = personalizada)
    public int AnchoPersonalizado { get; set; } = 21;  // numeros de la relacion personalizada (21:9)
    public int AltoPersonalizado { get; set; } = 9;
    public int TransparenciaCapa { get; set; } = 60;   // porcentaje, a mas alto se ve mas lo de abajo (60 = opacidad 0.4)
    public bool PedirNombre { get; set; } = false;  // pregunta el nombre al guardar
    public bool IniciarConWindows { get; set; } = false; // arrancar con Windows

    // Limites de la transparencia de la capa, en porcentaje
    public const int TransparenciaMinima = 0;
    public const int TransparenciaMaxima = 90;

    static readonly string Ruta = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LiveSnip", "config.json");

    [JsonIgnore] public string Atajo => Describir(Modificadores, (Keys)Tecla);
    [JsonIgnore] public RelacionAspecto Personalizada => RelacionAspecto.Personalizada(AnchoPersonalizado, AltoPersonalizado);
    // Opacidad que se le pone a la capa (de 0.1 a 1), corrige cualquier valor fuera de rango del config.json
    [JsonIgnore] public double OpacidadCapa =>
        1.0 - Math.Clamp(TransparenciaCapa, TransparenciaMinima, TransparenciaMaxima) / 100.0;
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
