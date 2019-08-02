using System;
using System.IO;
using SixLabors.Fonts;

namespace evorace.Runner.RaspberryPiUtilities
{
    public static class RpiFonts
    {
        public static FontFamily Roboto => myRobotoLazy.Value;

        private static readonly Lazy<FontFamily> myRobotoLazy = new Lazy<FontFamily>(() =>
        {
            var fontCollection = new FontCollection();
            var fontFamily = fontCollection.Install(Path.Combine("res", "Roboto-Regular.ttf"));

            return fontFamily;
        });
    }
}
