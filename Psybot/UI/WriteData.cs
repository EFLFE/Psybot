using System;
using System.Diagnostics;

namespace Psybot.UI
{
    [DebuggerDisplay("Text = \"{Text}\", ({X},{Y}), {Color.ToString()}")]
    internal struct WriteData
    {
        public readonly string Text;

        public readonly ConsoleColor? ForeColor;
        public readonly ConsoleColor? BackColor;

        public readonly int? X;

        public readonly int? Y;

        public WriteData(string text, ConsoleColor? fcolor = null, int? x = null, int? y = null)
        {
            Text = text;
            ForeColor = fcolor;
            X = x;
            Y = y;
            BackColor = null;
        }

        public WriteData(string text, ConsoleColor? fcolor = null, ConsoleColor? bcolor = null, int? x = null, int? y = null)
        {
            Text = text;
            ForeColor = fcolor;
            BackColor = bcolor;
            X = x;
            Y = y;
        }
    }
}
