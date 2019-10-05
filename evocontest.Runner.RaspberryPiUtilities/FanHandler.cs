using System;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace evocontest.Runner.RaspberryPiUtilities
{
    public sealed class FanHandler : IFanControl
    {
        public FanHandler(int controlGpio)
        {
            Pi.Init<BootstrapWiringPi>();
            myControlGpio = controlGpio;
        }

        public void FanPower(bool isOn)
        {
            Console.WriteLine($"Turning fan on {myControlGpio} to: {isOn}");
            var fanPin = Pi.Gpio[myControlGpio];
            fanPin.PinMode = GpioPinDriveMode.Output;
            fanPin.Write(isOn);
        }

        public IDisposable TurnOnTemporarily()
        {
            FanPower(true);
            return new ActionOnDispose(() => FanPower(false));
        }

        private class ActionOnDispose : IDisposable
        {
            public ActionOnDispose(Action onDispose)
            {
                myOnDispose = onDispose;
            }

            public void Dispose()
            {
                myOnDispose();
            }

            private readonly Action myOnDispose;
        }

        private readonly int myControlGpio;
    }
}
