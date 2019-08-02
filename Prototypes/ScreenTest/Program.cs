using System;
using System.Linq;
using System.Threading.Tasks;
using evorace.Runner.Host.Extensions;
using evorace.Runner.RaspberryPiUtilities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ScreenTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await using (var screen = new Waveshare213EpaperDisplay())
            {
                await screen.InitializeAsync(RefreshMode.Full);

                args = args.Any() ? args : new[] { string.Empty };
                switch (args[0])
                {
                    case "type":
                        var size = Convert.ToInt32(args[1]);
                        var text = args[2];

                        using (var image = new Image<Rgba32>(Configuration.Default, screen.Width, screen.Height, Rgba32.White))
                        {
                            image.Mutate(x =>
                            {
                                x.DrawText(new TextGraphicsOptions
                                {
                                    Antialias = false,
                                    WrapTextWidth = screen.Width,
                                    HorizontalAlignment = HorizontalAlignment.Center
                                }, text, RpiFonts.Roboto.CreateFont(size), Rgba32.Black, new PointF(0, 10));
                            });
                            await screen.DrawAsync(image.ToBlackAndWhitePixels());
                        }
                        break;

                    case "draw":
                        var pixels = new byte[screen.Width, screen.Height];
                        for (int y = 0; y < screen.Height; y++)
                        {
                            for (int x = 0; x < screen.Width; x++)
                            {
                                pixels[x, y] = (byte)(x % 10 == 0 || y % 10 == 0 ? 0x00 : 0xFF);
                            }
                        }
                        await screen.DrawAsync(pixels);
                        break;

                    case "clear":
                    default:
                        await screen.ClearAsync();
                        break;
                }
            }
        }
    }
}
