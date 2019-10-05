using System;
using System.Linq;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace RpiPower
{
    public class Program
    {
        static void Main(string[] args)
        {
            var pin = 26;
            var isOn = false;
            if (args.Any())
            {
                if (int.TryParse(args[0], out var pinNumber))
                {
                    pin = pinNumber;
                }

                if (int.TryParse(args[1], out var isOnNumber))
                {
                    isOn = isOnNumber > 0;
                }
            }

            Pi.Init<BootstrapWiringPi>();

            Console.WriteLine($"Writing on {pin}: {isOn}");
            var blinkingPin = Pi.Gpio[pin];

            // Configure the pin as an output
            blinkingPin.PinMode = GpioPinDriveMode.Output;
            blinkingPin.Write(isOn);
        }
    }
}
