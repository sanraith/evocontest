using System;
using System.Threading;
using System.Threading.Tasks;

namespace LimiterTest
{
    internal sealed class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () => await MainAsync(args)).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            for (int i = 0; i < 10; i++)
            {
                var spinner = new Spinner();
                var count = await spinner.Run();
                Console.WriteLine($"Count: {count}");
            }
        }
    }

    internal sealed class Spinner
    {
        public async Task<long> Run()
        {
            var tokenSource = new CancellationTokenSource();
            myToken = tokenSource.Token;
            _ = Task.Run(Spin); // TODO add multiple tasks using parallel library
            await Task.Delay(TimeSpan.FromSeconds(1));
            tokenSource.Cancel();
            return myCounter;
        }

        private void Spin()
        {
            const double c = 987.654;
            double number = 12345.6789;
            while (!myToken.IsCancellationRequested)
            {
                for (int i = 0; i < 1000; i++)
                {
                    number *= c;
                    number /= c;
                }
                myCounter++;
            }
        }

        private CancellationToken myToken;
        private long myCounter = 0;
    }
}
