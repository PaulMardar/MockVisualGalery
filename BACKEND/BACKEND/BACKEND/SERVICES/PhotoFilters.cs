// BACKEND.SERVICES / PhotoFilters.cs
using SkiaSharp;
using System;

namespace BACKEND.SERVICES
{
    public static class PhotoFilters
    {
        public const string VariantExtension = "png";

        public static readonly (string Suffix, Func<byte, byte, byte, (byte R, byte G, byte B)> Apply)[] Variants =
        {
            ("inverted", Invert),
            ("sepia", Sepia),
            ("grayscale", Grayscale)
        };

        private static (byte R, byte G, byte B) Invert(byte r, byte g, byte b) =>
            ((byte)(255 - r), (byte)(255 - g), (byte)(255 - b));

        // Standard sepia matrix.
        private static (byte R, byte G, byte B) Sepia(byte r, byte g, byte b)
        {
            var newR = (int)(0.393 * r + 0.769 * g + 0.189 * b);
            var newG = (int)(0.349 * r + 0.686 * g + 0.168 * b);
            var newB = (int)(0.272 * r + 0.534 * g + 0.131 * b);
            return ((byte)Math.Min(255, newR), (byte)Math.Min(255, newG), (byte)Math.Min(255, newB));
        }

        private static (byte R, byte G, byte B) Grayscale(byte r, byte g, byte b)
        {
            var gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
            return (gray, gray, gray);
        }

        public static byte[] Apply(byte[] originalContent, Func<byte, byte, byte, (byte R, byte G, byte B)> transform)
        {
            using var bitmap = SKBitmap.Decode(originalContent) ?? throw new InvalidOperationException("Could not decode image content - unsupported or corrupt image format.");

            var pixels = bitmap.Pixels;
            for (int i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                var (r, g, b) = transform(p.Red, p.Green, p.Blue);
                pixels[i] = new SKColor(r, g, b, p.Alpha);
            }
            bitmap.Pixels = pixels;

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}