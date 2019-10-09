using System;
using System.Threading.Tasks;

namespace evocontest.Runner.Host.Common.Utility
{
    public static class ConsoleUtilities
    {
        public static async Task CountDown(int countDownSeconds, Func<int, string> countingMessageGenerator, string completedMessage)
        {
            var maxLength = 0;
            for (int i = countDownSeconds; i > 0; i--)
            {
                var message = countingMessageGenerator(i);
                maxLength = Math.Max(maxLength, message.Length);
                Console.Write(message + new string(' ', maxLength - message.Length) + "\r");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            maxLength = Math.Max(maxLength, completedMessage.Length);
            Console.WriteLine(completedMessage + new string(' ', maxLength - completedMessage.Length));
        }
    }
}
