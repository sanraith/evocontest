using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace evocontest.Runner.RaspberryPiUtilities
{
    public static class ImageExtensions
    {
        public static byte[,] ToBlackAndWhitePixels(this Image<Rgba32> image)
        {
            var pixels = new byte[image.Width, image.Height];
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    pixels[x, y] = (byte)(image[x, y] == Rgba32.White ? 0xFF : 0x00);
                }
            }
            return pixels;
        }
    }
}
