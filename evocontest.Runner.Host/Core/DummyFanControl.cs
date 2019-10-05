using evocontest.Runner.Host.Common.Utility;
using evocontest.Runner.RaspberryPiUtilities;
using System;

namespace evocontest.Runner.Host.Core
{
    public sealed class DummyFanControl : IFanControl
    {
        public void FanPower(bool isOn)
        {
            Console.WriteLine($"No fan control is activate, but would set fan to: {isOn}");
        }

        public IDisposable TurnOnTemporarily()
        {
            FanPower(true);
            return DisposableValue.Create(new object(), _ => FanPower(false));
        }
    }
}
