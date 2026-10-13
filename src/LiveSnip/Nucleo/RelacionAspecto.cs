using System.Collections.ObjectModel;

namespace LiveSnip.Nucleo;

// Relacion de aspecto (ancho/alto). Valor null = libre.
public sealed record RelacionAspecto(string Nombre, double? Valor)
{
    public static readonly ReadOnlyCollection<RelacionAspecto> Todas = Array.AsReadOnly(new RelacionAspecto[]
    {
        new("Libre", null),
        new("1:1", 1.0),
        new("4:3", 4.0 / 3),
        new("16:9", 16.0 / 9),
        new("9:16", 9.0 / 16),
        new("3:2", 3.0 / 2),
    });

    // Lo busca por indice sin salirse de la lista
    public static RelacionAspecto Desde(int indice) => Todas[Math.Clamp(indice, 0, Todas.Count - 1)];

    public int Indice => Todas.IndexOf(this);

    // Rectangulo entre los dos puntos respetando esta relacion y los limites del monitor
    public Rectangle Ajustar(Point ancla, Point actual, Size limites)
    {
        int dx = actual.X - ancla.X, dy = actual.Y - ancla.Y;
        double w = Math.Abs(dx), h = Math.Abs(dy);

        if (Valor is double r)
        {
            if (w > h * r) h = w / r; else w = h * r;

            // Si se sale del monitor, reduce manteniendo la proporcion
            double maxW = dx >= 0 ? limites.Width - ancla.X : ancla.X;
            double maxH = dy >= 0 ? limites.Height - ancla.Y : ancla.Y;
            if (w > maxW) { h *= maxW / w; w = maxW; }
            if (h > maxH) { w *= maxH / h; h = maxH; }
        }

        double x = dx >= 0 ? ancla.X : ancla.X - w;
        double y = dy >= 0 ? ancla.Y : ancla.Y - h;
        return new Rectangle((int)x, (int)y, (int)w, (int)h);
    }
}
