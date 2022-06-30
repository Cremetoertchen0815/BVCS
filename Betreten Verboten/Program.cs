using System;

namespace Betreten_Verboten
{
    /// <summary>
    /// Program entry point.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // execute GeonBit with Game1 instance.
            using (var g = new Nez.GeonBit.UI.Example.GeonBitUI_Examples())
            {
                g.Run();
            }
        }
    }
}
