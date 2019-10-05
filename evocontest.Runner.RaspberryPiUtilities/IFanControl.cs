using System;
using System.Collections.Generic;
using System.Text;

namespace evocontest.Runner.RaspberryPiUtilities
{
    public interface IFanControl
    {
        void FanPower(bool isOn);

        IDisposable TurnOnTemporarily();
    }
}
