using System.Collections.Generic;
using System.Windows.Media;

namespace MultiRobotSimulator.WPF.Utils
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Help:Distinguishable_colors
    /// </summary>
    public static class DistinctColors
    {
        private static readonly List<(byte, byte, byte)> _colorAlphabet = new List<(byte, byte, byte)>()
        {
            (240,163,255),
            (0,117,220),
            (153,63,0),
            (76,0,92),
            (25,25,25),
            (0,92,49),
            (43,206,72),
            (255,204,153),
            (128,128,128),
            (148,255,181),
            (143,124,0),
            (157,204,0),
            (194,0,136),
            (0,51,128),
            (255,164,5),
            (255,168,187),
            (66,102,0),
            (255,0,16),
            (94,241,242),
            (0,153,143),
            (224,255,102),
            (116,10,255),
            (153,0,0),
            (255,255,128),
            (255,255,0),
            (255,80,5)
        };

        public static Brush GetBrush(int i)
        {
            var (r, g, b) = _colorAlphabet[i % _colorAlphabet.Count];
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
    }
}
