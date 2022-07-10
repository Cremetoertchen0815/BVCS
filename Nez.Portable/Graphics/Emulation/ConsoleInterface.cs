using System;
using System.Collections.Generic;
using System.Threading;

namespace Nez
{
    public static class ConsoleInterface
    {

        //internal fields
        internal static char[,] Characters;
        internal static ConsoleColor[,] Colors;
        internal static int Width = 200;
        internal static int Height = 50;
        internal static object _renderlock = new object();
        internal static object _keylock = new object();
        internal static List<ConsoleKey> pressedKeys = new List<ConsoleKey>();
        internal static bool shiftP;
        internal static bool ctrlP;
        internal static bool altP;
        internal static bool readKey;
        private static int x;
        private static int y;
        private const int updateSleeptime = 10;

        public static void Initialize()
        {
            Characters = new char[Width, Height];
            Colors = new ConsoleColor[Width, Height];
        }

        private static char ConsoleKeyToChar(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.D0:
                case ConsoleKey.D1:
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                case ConsoleKey.D4:
                case ConsoleKey.D5:
                case ConsoleKey.D6:
                case ConsoleKey.D7:
                case ConsoleKey.D8:
                case ConsoleKey.D9:
                    return key.ToString()[1];
                case ConsoleKey.NumPad0:
                case ConsoleKey.NumPad1:
                case ConsoleKey.NumPad2:
                case ConsoleKey.NumPad3:
                case ConsoleKey.NumPad4:
                case ConsoleKey.NumPad5:
                case ConsoleKey.NumPad6:
                case ConsoleKey.NumPad7:
                case ConsoleKey.NumPad8:
                case ConsoleKey.NumPad9:
                    return key.ToString()[6];
                case ConsoleKey.Spacebar:
                    return ' ';
                default:
                    return key.ToString()[0];

            }
        }

        //Console emulation properties
        public static bool CursorVisible { get; set; } = true;
        public static ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
        public static int BufferWidth => Width;

        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            ConsoleKey key;
            while (!KeyAvailable) Thread.Sleep(updateSleeptime);
            lock (_keylock)
            {
                key = pressedKeys[0];
                pressedKeys.RemoveAt(0);
            }

            if (!intercept) Write(ConsoleKeyToChar(key).ToString());
            return new ConsoleKeyInfo(ConsoleKeyToChar(key), key, shiftP, altP, ctrlP);
        }

        public static bool KeyAvailable
        {
            get
            {
                lock (_keylock) return pressedKeys.Count > 0;
            }
        }
        public static void SetCursorPosition(int x, int y) { ConsoleInterface.x = x; ConsoleInterface.y = y; }
        public static void Clear()
        {
            x = y = 0;
            lock (_renderlock)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                        if (Characters[x, y] != ' ') Characters[x, y] = ' ';
                }
            }
        }

        //'ll do later
        public static void Beep(int freq, int len) { return; }

        public static string ReadLine()
        {
            string res = "";
            bool leave = false;
            while (!leave)
            {
                var press = ReadKey(true);
                switch (press.Key)
                {
                    case ConsoleKey.Backspace:
                        if (res.Length < 1) continue;
                        res = res.Substring(0, res.Length - 1);
                        x--;
                        Characters[x, y] = ' ';
                        break;
                    case ConsoleKey.Enter:
                        leave = true;
                        y++;
                        x = 0;
                        break;
                    default:
                        string key = shiftP ? press.KeyChar.ToString().ToUpper() : press.KeyChar.ToString().ToLower();
                        Write(key);
                        res += key;
                        break;
                }
            }
            return res;
        }

        public static void Write(string value)
        {
            lock (_renderlock)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] == '\n') { y++; x = 0; continue; }
                    if (value[i] == '\r') continue;
                    Characters[x, y] = value[i];
                    Colors[x, y] = ForegroundColor;
                    x++;
                }
            }

        }


        public static void Write(int value) => Write(value.ToString());
        public static void Write(float value) => Write(value.ToString());
        public static void Write(double value) => Write(value.ToString());
        public static void WriteLine(string value) => Write(value + "\n");
        public static void WriteLine(int value) => Write(value.ToString() + "\n");
        public static void WriteLine(float value) => Write(value.ToString() + "\n");
        public static void WriteLine(double value) => Write(value.ToString() + "\n");

    }
}
