// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documnetation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to  whom the Software is
// furished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS OR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace evocontest.Runner.RaspberryPiUtilities
{
    /// <summary>
    /// Driver for the Waveshare 2.13 inch e-paper display (V1) on the Raspberry Pi.
    /// Based on demo code from: https://www.waveshare.com/wiki/2.13inch_e-Paper_HAT
    /// </summary>
    public class Waveshare213EpaperDisplay : IEpaperDisplay, IAsyncDisposable
    {
        public bool IsInitialized { get; private set; }

        public int Width => EPD_HEIGHT;

        public int Height => EPD_WIDTH;

        public async Task InitializeAsync(RefreshMode refreshMode)
        {
            Console.WriteLine("Initialize begin");

            // Configure raspberry pi
            Pi.Init<BootstrapWiringPi>();
            gpio = Pi.Gpio;
            gpio[BUSY_PIN].PinMode = GpioPinDriveMode.Input;
            gpio[DC_PIN].PinMode = GpioPinDriveMode.Output;
            gpio[RST_PIN].PinMode = GpioPinDriveMode.Output;
            gpio[CS_PIN].PinMode = GpioPinDriveMode.Output;
            Pi.Spi.Channel0Frequency = 2000000;
            spi = Pi.Spi.Channel0;

            await InitScreenAsync(refreshMode);

            Console.WriteLine("Initialize end");
        }

        public async Task ClearAsync()
        {
            await ClearAsync(0xFF);
        }

        public async Task DrawAsync(byte[,] pixels)
        {
            await Display(GetBuffer(pixels));
        }

        public async Task SleepAsync()
        {
            Console.WriteLine("Sleep begin");
            send_command(0x10);
            await Delay(100);
            IsInitialized = false;
            Console.WriteLine("Sleep end");
        }

        public async ValueTask DisposeAsync()
        {
            await SleepAsync();
        }

        private async Task InitScreenAsync(RefreshMode refreshMode)
        {
            Console.WriteLine("InitScreen begin");

            await Reset();

            send_command(0x01); // DRIVER_OUTPUT_CONTROL
            send_data(EPD_HEIGHT - 1 & 0xFF);
            send_data(EPD_HEIGHT - 1 >> 8 & 0xFF);
            send_data(0x00); // GD = 0 SM = 0 TB = 0

            send_command(0x0C); // BOOSTER_SOFT_START_CONTROL
            send_data(0xD7);
            send_data(0xD6);
            send_data(0x9D);

            send_command(0x2C); // WRITE_VCOM_REGISTER
            send_data(0xA8); // VCOM 7C

            send_command(0x3A); // SET_DUMMY_LINE_PERIOD
            send_data(0x1A); // 4 dummy lines per gate

            send_command(0x3B); // SET_GATE_TIME
            send_data(0x08); // 2us per line

            send_command(0X3C); // BORDER_WAVEFORM_CONTROL
            send_data(0x03);

            send_command(0X11); // DATA_ENTRY_MODE_SETTING
            send_data(0x03); // X increment; Y increment

            // WRITE_LUT_REGISTER
            send_command(0x32);
            switch (refreshMode)
            {
                case RefreshMode.Full: send_data(lut_full_update); break;
                case RefreshMode.Partial: send_data(lut_partial_update); break;
                default: throw new ArgumentException(nameof(refreshMode));
            }

            IsInitialized = true;

            Console.WriteLine("InitScreen end");
        }

        private async Task Display(byte[] image)
        {
            Console.WriteLine("Display begin");
            int linewidth = GetLineWidth();

            SetWindows(0, 0, EPD_WIDTH, EPD_HEIGHT);
            for (int j = 0; j < EPD_HEIGHT; j++)
            {
                await SetCursorAsync(0, j);
                send_command(0x24);
                for (int i = 0; i < linewidth; i++)
                {
                    send_data(image[i + j * linewidth]);
                }
            }

            await TurnOnDisplay();

            Console.WriteLine("Display end");
        }

        private async Task ClearAsync(byte color)
        {
            Console.WriteLine("Clear begin");

            int linewidth = GetLineWidth();
            SetWindows(0, 0, EPD_WIDTH, EPD_HEIGHT);
            for (int j = 0; j < EPD_HEIGHT; j++)
            {
                await SetCursorAsync(0, j);
                send_command(0x24);
                for (int i = 0; i < linewidth; i++)
                {
                    send_data(color);
                }
            }

            await TurnOnDisplay();

            Console.WriteLine("Clear end");
        }

        private async Task Reset()
        {
            CheckInitialized();
            Console.WriteLine("Reset begin");

            // Reset
            gpio![RST_PIN].Write(GpioPinValue.High);
            await Delay(200);
            gpio[RST_PIN].Write(GpioPinValue.Low);
            await Delay(200);
            gpio[RST_PIN].Write(GpioPinValue.High);
            await Delay(200);

            Console.WriteLine("Reset end");
        }

        private async Task TurnOnDisplay()
        {
            Console.WriteLine("TurnOn begin");

            send_command(0x22); // DISPLAY_UPDATE_CONTROL_2
            send_data(0xC4);
            send_command(0x20); // MASTER_ACTIVATION
            send_command(0xFF); // TERMINATE_FRAME_READ_WRITE

            await wait_until_idle();

            Console.WriteLine("TurnOn end");
        }

        private async Task SetCursorAsync(int x, int y)
        {
            send_command(0x4E); // SET_RAM_X_ADDRESS_COUNTER
            // x point must be the multiple of 8 or the last 3 bits will be ignored
            send_data(x >> 3 & 0xFF);
            send_command(0x4F); // SET_RAM_Y_ADDRESS_COUNTER
            send_data(y & 0xFF);
            send_data(y >> 8 & 0xFF);
            await wait_until_idle();
        }

        private void SetWindows(byte x_start, byte y_start, byte x_end, byte y_end)
        {
            send_command(0x44); // SET_RAM_X_ADDRESS_START_END_POSITION
            send_data(x_start >> 3 & 0xFF);
            send_data(x_end >> 3 & 0xFF);
            send_command(0x45); // SET_RAM_Y_ADDRESS_START_END_POSITION
            send_data(y_start & 0xFF);
            send_data(y_start >> 8 & 0xFF);
            send_data(y_end & 0xFF);
            send_data(y_end >> 8 & 0xFF);
        }

        private async Task wait_until_idle()
        {
            CheckInitialized();
            while (gpio![BUSY_PIN].Read())
            {
                await Delay(100);
            }
        }

        private void send_command(byte command)
        {
            CheckInitialized();
            gpio![DC_PIN].Write(GpioPinValue.Low);
            spi_writebytes(command);
        }

        private void send_data(int data) => send_data((byte)data);
        private void send_data(params byte[] data)
        {
            CheckInitialized();
            gpio![DC_PIN].Write(GpioPinValue.High);
            spi_writebytes(data);
        }

        private void spi_writebytes(params byte[] data)
        {
            CheckInitialized();
            spi!.Write(data);
        }

        private void CheckInitialized()
        {
            if (gpio == null || spi == null)
            {
                throw new InvalidOperationException("Display is not initialized!");
            }
        }

        private static byte[] GetBuffer(byte[,] pixels)
        {
            var linewidth = GetLineWidth();

            byte[] buf = Enumerable.Repeat<byte>(0xFF, linewidth * EPD_HEIGHT).ToArray();
            var (imwidth, imheight) = (pixels.GetLength(0), pixels.GetLength(1));

            if (imwidth == EPD_WIDTH && imheight == EPD_HEIGHT)
            {
                for (int y = 0; y < imheight; y++)
                {
                    for (int x = 0; x < imwidth; x++)
                    {
                        if (pixels[x, y] == 0)
                        {
                            buf[x / 8 + y * linewidth] &= (byte)~(0x80 >> x % 8);
                        }
                    }
                }
            }
            else if (imwidth == EPD_HEIGHT && imheight == EPD_WIDTH)
            {
                for (int y = 0; y < imheight; y++)
                {
                    for (int x = 0; x < imwidth; x++)
                    {
                        var newx = y;
                        var newy = EPD_HEIGHT - x - 1;
                        if (pixels[x, y] == 0)
                        {
                            buf[newx / 8 + newy * linewidth] &= (byte)~(0x80 >> y % 8);
                        }
                    }
                }
            }

            return buf;
        }

        private static int GetLineWidth()
        {
            int linewidth;
            //if (EPD_WIDTH % 8 == 0)
            //{
            //    linewidth = EPD_WIDTH / 8;
            //}
            //else
            //{
            //    linewidth = EPD_WIDTH / 8 + 1;
            //}

            linewidth = EPD_WIDTH / 8 + 1;

            return linewidth;
        }

        private static Task Delay(int delayMillis)
        {
            return Task.Delay(delayMillis);
        }

        private IGpioController? gpio;
        private ISpiChannel? spi;

        // Screen Size
        private const int EPD_WIDTH = 122;
        private const int EPD_HEIGHT = 250;

        // Pins
        private const int RST_PIN = 17;
        private const int DC_PIN = 25;
        private const int CS_PIN = 8;
        private const int BUSY_PIN = 24;

        private readonly byte[] lut_full_update = new byte[] {
            0x22, 0x55, 0xAA, 0x55, 0xAA, 0x55, 0xAA, 0x11,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x1E, 0x1E, 0x1E, 0x1E, 0x1E, 0x1E, 0x1E, 0x1E,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        private readonly byte[] lut_partial_update = new byte[] {
            0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0F, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
    }
}
