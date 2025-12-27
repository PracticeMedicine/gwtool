using System;
using System.Windows.Forms;

namespace GWTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // warm up the thread helper.
            _ = ThreadHelper.Current;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UI.Main());
        }
    }
}
