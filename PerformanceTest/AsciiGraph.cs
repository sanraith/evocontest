using System;
using System.Collections.Generic;
using System.Linq;

namespace PerformanceTest
{
    public class AsciiGraph
    {
        public int Height { get; set; }

        public IReadOnlyList<double> Items { get; set; }

        public void Draw()
        {
            var width = Items.Count;
            var max = Items.Max();
            var min = Items.Min();
            var step = (max - min) / Height;

            for (int y = 0; y < Height; y++)
            {
                var line = new char[width];
                for (int x = 0; x < width; x++)
                {
                    var c = Items[x] >= max - (y + 1) * step ? '*' : ' ';
                    line[x] = c;
                }
                Console.WriteLine(line);
            }
            Console.WriteLine($"{max - min} >< {min}..{max}");
        }

    }
}
