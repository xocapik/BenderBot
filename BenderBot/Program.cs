using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace xphp
{
    
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static void Main(string[] args)
        {
            
            var handle = GetConsoleWindow();
            // Hide
#if DEBUG
#else
            // Hide
           ShowWindow(handle, SW_HIDE);
#endif
            DiscordBot bot = new DiscordBot();
        }
    }
}
