namespace LiveSnip.Interfaz;

// Los 8 puntos de una zona que se pueden arrastrar para cambiar su tamano
enum Tirador {
    ArribaIzquierda, Arriba, ArribaDerecha,
    Derecha,
    AbajoDerecha, Abajo, AbajoIzquierda,
    Izquierda
}

// Calcula donde esta cada tirador de una zona, sirve para dibujarlos y para saber si el mouse esta encima
static class Tiradores {
    public const int Tamano = 10;   // lado del cuadrito que se dibuja

    public static readonly Tirador[] Todos = Enum.GetValues<Tirador>();

    // Punto de la zona donde va el centro del tirador, en las esquinas y en la mitad de cada lado
    static Point Punto(Rectangle zona, Tirador tirador) => tirador switch {
        Tirador.ArribaIzquierda => new Point(zona.Left, zona.Top),
        Tirador.Arriba => new Point(zona.Left + zona.Width / 2, zona.Top),
        Tirador.ArribaDerecha => new Point(zona.Right, zona.Top),
        Tirador.Derecha => new Point(zona.Right, zona.Top + zona.Height / 2),
        Tirador.AbajoDerecha => new Point(zona.Right, zona.Bottom),
        Tirador.Abajo => new Point(zona.Left + zona.Width / 2, zona.Bottom),
        Tirador.AbajoIzquierda => new Point(zona.Left, zona.Bottom),
        _ => new Point(zona.Left, zona.Top + zona.Height / 2)   // Izquierda
    };

    // Cuadrito del tirador, centrado en su punto
    public static Rectangle Area(Rectangle zona, Tirador tirador) {
        var punto = Punto(zona, tirador);
        return new Rectangle(punto.X - Tamano / 2, punto.Y - Tamano / 2, Tamano, Tamano);
    }
}
