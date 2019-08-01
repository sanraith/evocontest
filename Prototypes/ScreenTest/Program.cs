using System.Linq;
using System.Threading.Tasks;

namespace ScreenTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var screenHandler = new CustomScreenHandler();
            await screenHandler.InitializeAsync();

            args = args.Any() ? args : new[] { string.Empty };
            switch (args[0])
            {
                case "draw": await screenHandler.DrawSomethingAsync(); break;
                case "clear":
                default:
                    await screenHandler.ClearAsync();
                    break;
            }
        }
    }
}
