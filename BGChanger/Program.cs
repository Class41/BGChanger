using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BGChanger
{
    class Program
    {
        static void Main(string[] args) => BgHandler();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, String pvParam, uint fWinIni);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public static List<string> _triggers;


        private const int SPI_SETDESKWALLPAPER = 0x14;
        private const int SPIF_UPDATEINIFILE = 0x1;
        private const int SPIF_SENDWININICHANGE = 0x2;

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private static void BgHandler()
        {
            if (!System.IO.File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\source.jpg"))
            {
                MessageBox.Show("Source image (your normal image) does not exist: " + System.IO.Directory.GetCurrentDirectory() + "\\source.jpg",
                   "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                Environment.Exit(-1);
            }

            if (!System.IO.File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\other.jpg"))
            {
                MessageBox.Show("Other image (your swap image) does not exist: " + System.IO.Directory.GetCurrentDirectory() + "\\other.jpg",
                   "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                Environment.Exit(-1);
            }

            try
            {
                if (!System.IO.File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\triggers.cfg"))
                {
                    System.IO.File.Create(System.IO.Directory.GetCurrentDirectory() + "\\triggers.cfg");

                    MessageBox.Show("Triggers file created. Please put the titles of applications in that file. One per line. (Case sensitive)" + System.IO.Directory.GetCurrentDirectory() + "\\triggers.cfg",
                   "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                    Environment.Exit(-1);
                }

                _triggers = new List<string>(System.IO.File.ReadAllLines(System.IO.Directory.GetCurrentDirectory() + "\\triggers.cfg"));

            }
            catch (Exception e)
            {
                MessageBox.Show("Could not load triggers file. Aborting: " + System.IO.Directory.GetCurrentDirectory() + "\\triggers.cfg",
                   "Error", MessageBoxButtons.OK,
                   MessageBoxIcon.Exclamation);

                Environment.Exit(-1);
            }
            AppDomain.CurrentDomain.ProcessExit += (s, ev) =>
            {
                DisplayPicture(System.IO.Directory.GetCurrentDirectory() + "\\source.jpg", true);
            };

            ShowWindow(GetConsoleWindow(), SW_HIDE);

            while (true)
            {
                CheckActive();
                Thread.Sleep(500);
            }
        }

        public static bool x = false;

        private static void CheckActive()
        {

            if (_triggers.Count == 0)
            {
                MessageBox.Show("Trigger file empty. Please list window title entries in the file. One per line. " + System.IO.Directory.GetCurrentDirectory() + "\\triggers.cfg",
                "Error", MessageBoxButtons.OK,
                 MessageBoxIcon.Exclamation);

                Environment.Exit(-1);
            }

            if (_triggers.Contains(GetActiveWindowTitle()))
                DisplayPicture(System.IO.Directory.GetCurrentDirectory() + "\\other.jpg", true);
            else
                DisplayPicture(System.IO.Directory.GetCurrentDirectory() + "\\source.jpg", true);

        }

        private static void DisplayPicture(string file_name, bool update_registry)
        {
            try
            {
                // If we should update the registry,
                // set the appropriate flags.
                uint flags = 0;
                if (update_registry)
                    flags = SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE;

                // Set the desktop background to this file.
                if (!SystemParametersInfo(SPI_SETDESKWALLPAPER,
                    0, file_name, flags))
                {
                    MessageBox.Show("SystemParametersInfo failed.",
                        "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error displaying picture " +
                    file_name + ".\n" + ex.Message,
                    "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }
    }
}
