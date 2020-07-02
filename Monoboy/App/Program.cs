using System;

namespace Monoboy
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using(App game = new App())
            {
                game.Run();
            }
        }
    }
}