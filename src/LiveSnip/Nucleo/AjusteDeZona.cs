namespace LiveSnip.Nucleo;

// Calcula como queda una zona cuando se arrastra uno de sus tiradores
public static class AjusteDeZona {
    public const int TamanoMinimo = 5;   // una zona mas chica no se puede capturar

    // Con aspecto libre cada tirador mueve solo los bordes que le tocan, con un aspecto fijo
    // solo se usan las esquinas y la zona crece desde la esquina de enfrente respetando la proporcion
    public static Rectangle Redimensionar(Rectangle zona, Tirador tirador, Point mouse, RelacionAspecto relacion, Size limites) {
        // El mouse no puede salirse del monitor
        var punto = new Point(Math.Clamp(mouse.X, 0, limites.Width), Math.Clamp(mouse.Y, 0, limites.Height));

        Rectangle nueva;
        if (relacion.Valor is not null) {
            nueva = relacion.Ajustar(EsquinaOpuesta(zona, tirador), punto, limites);
        } else {
            int izquierda = zona.Left, derecha = zona.Right, arriba = zona.Top, abajo = zona.Bottom;
            if (tirador is Tirador.ArribaIzquierda or Tirador.Izquierda or Tirador.AbajoIzquierda) izquierda = punto.X;
            if (tirador is Tirador.ArribaDerecha or Tirador.Derecha or Tirador.AbajoDerecha) derecha = punto.X;
            if (tirador is Tirador.ArribaIzquierda or Tirador.Arriba or Tirador.ArribaDerecha) arriba = punto.Y;
            if (tirador is Tirador.AbajoIzquierda or Tirador.Abajo or Tirador.AbajoDerecha) abajo = punto.Y;

            // Si los bordes se cruzan la zona se da vuelta, en vez de quedar con un tamano negativo
            nueva = Rectangle.FromLTRB(
                Math.Min(izquierda, derecha), Math.Min(arriba, abajo),
                Math.Max(izquierda, derecha), Math.Max(arriba, abajo));
        }

        // Si queda demasiado chica se deja como estaba
        return nueva.Width < TamanoMinimo || nueva.Height < TamanoMinimo ? zona : nueva;
    }

    // La esquina que no se mueve cuando se arrastra una esquina
    static Point EsquinaOpuesta(Rectangle zona, Tirador tirador) => tirador switch {
        Tirador.ArribaIzquierda => new Point(zona.Right, zona.Bottom),
        Tirador.ArribaDerecha => new Point(zona.Left, zona.Bottom),
        Tirador.AbajoDerecha => new Point(zona.Left, zona.Top),
        Tirador.AbajoIzquierda => new Point(zona.Right, zona.Top),
        _ => zona.Location
    };
}
