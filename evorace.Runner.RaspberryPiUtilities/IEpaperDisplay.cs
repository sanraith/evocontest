using System.Threading.Tasks;

namespace evorace.Runner.RaspberryPiUtilities
{
    public interface IEpaperDisplay
    {
        /// <summary>
        /// True, if the screen is ready for display.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// The width of the display in pixels.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the display in pixels.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Initializes the display to work with the given refresh mode.
        /// Has to be called before any operation, and after sleep.
        /// </summary>
        /// <param name="refreshMode">The refresh mode of the display</param>
        Task InitializeAsync(RefreshMode refreshMode);

        /// <summary>
        /// Clears the screen.
        /// </summary>
        Task ClearAsync();

        /// <summary>
        /// Draws an image to the screen. Each element in the array represents a single pixel.
        /// 0xFF = white, 0x00 = black.
        /// </summary>
        /// <param name="pixels">The pixels to be drawn.</param>
        Task DrawAsync(byte[,] pixels);

        /// <summary>
        /// Puts the screen to a deep sleep.
        /// The screen must be re-initialized to enable drawing again.
        /// </summary>
        Task SleepAsync();
    }
}
