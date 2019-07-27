using System;
using System.Collections.Generic;
using System.Text;

namespace evorace.WebApp.Common
{
    public enum ValidationStateEnum
    {
        None = 0,
        File = 1,
        Static = 2,
        UnitTest = 3,
        Completed = 100
    }
}
