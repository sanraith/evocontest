﻿using System.Threading.Tasks;
using evocontest.Runner.RaspberryPiUtilities;

namespace evocontest.Runner.Host.Core
{
    public class DummyEpaperDisplay : IEpaperDisplay
    {
        public bool IsInitialized => true;

        public int Width => 250;

        public int Height => 122;

        public Task ClearAsync()
        {
            return Task.CompletedTask;
        }

        public Task DrawAsync(byte[,] pixels)
        {
            return Task.CompletedTask;
        }

        public Task InitializeAsync(RefreshMode refreshMode)
        {
            return Task.CompletedTask;
        }

        public Task SleepAsync()
        {
            return Task.CompletedTask;
        }
    }
}
