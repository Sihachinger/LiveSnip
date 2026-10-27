using System.Drawing.Imaging;

namespace LiveSnip.Nucleo;

// Difumina una zona de la imagen en bloques, cada bloque se pinta con el color promedio de lo que tenia
public static class Pixelador {
    public const int BloqueMinimo = 2;

    // Modifica la imagen directamente, la zona se recorta si se sale de la imagen
    public static void Pixelar(Bitmap imagen, Rectangle zona, int bloque) {
        bloque = Math.Max(bloque, BloqueMinimo);
        zona.Intersect(new Rectangle(Point.Empty, imagen.Size));
        if (zona.Width < 1 || zona.Height < 1) return;

        // Se trabaja sobre los bytes de la imagen porque GetPixel y SetPixel son muy lentos
        var datos = imagen.LockBits(zona, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try {
            var bytes = new byte[datos.Stride * datos.Height];
            System.Runtime.InteropServices.Marshal.Copy(datos.Scan0, bytes, 0, bytes.Length);

            for (int y = 0; y < zona.Height; y += bloque)
                for (int x = 0; x < zona.Width; x += bloque)
                    PintarBloque(bytes, datos.Stride, new Rectangle(x, y, Math.Min(bloque, zona.Width - x), Math.Min(bloque, zona.Height - y)));

            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, datos.Scan0, bytes.Length);
        } finally {
            imagen.UnlockBits(datos);
        }
    }

    // Saca el promedio de cada canal (azul, verde, rojo, alfa) del bloque y lo pinta entero con ese color
    static void PintarBloque(byte[] bytes, int stride, Rectangle bloque) {
        var suma = new long[4];
        for (int y = bloque.Top; y < bloque.Bottom; y++)
            for (int x = bloque.Left; x < bloque.Right; x++)
                for (int canal = 0; canal < 4; canal++)
                    suma[canal] += bytes[y * stride + x * 4 + canal];

        long cantidad = (long)bloque.Width * bloque.Height;
        for (int y = bloque.Top; y < bloque.Bottom; y++)
            for (int x = bloque.Left; x < bloque.Right; x++)
                for (int canal = 0; canal < 4; canal++)
                    bytes[y * stride + x * 4 + canal] = (byte)(suma[canal] / cantidad);
    }
}
