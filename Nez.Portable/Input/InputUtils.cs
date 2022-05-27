using Microsoft.Xna.Framework.Input;
using System;

namespace Nez
{
    public static class InputUtils
    {
        public static bool IsMac;
        public static bool IsWindows;
        public static bool IsLinux;


        static InputUtils()
        {
            IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            IsLinux = Environment.OSVersion.Platform == PlatformID.Unix;
            IsMac = Environment.OSVersion.Platform == PlatformID.MacOSX;
        }


        public static bool IsShiftDown() => Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift);


        public static bool IsAltDown() => Input.IsKeyDown(Keys.LeftAlt) || Input.IsKeyDown(Keys.RightAlt);


        public static bool IsControlDown()
        {
            if (IsMac)
                return Input.IsKeyDown(Keys.LeftWindows) || Input.IsKeyDown(Keys.RightWindows);

            return Input.IsKeyDown(Keys.LeftControl) || Input.IsKeyDown(Keys.RightControl);
        }
    }
}