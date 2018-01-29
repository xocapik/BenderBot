
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace xphps
{
    class Program
    {

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static void Scan()
        {
            var gmail = new gmail();
            gmail.test();

            //xphps.Run("31.214.160.104", 27019);
            //xphps.Run("85.190.155.67", 27015);
            //xphps.Run("31.214.160.214", 27021);
        }

        static void Start()
        {
            var b = new BattleMetrics();

            using (DataContext contexto = new DataContext())
            {
                var dbservers = contexto.Servers.ToList();
                if (dbservers.Count < 4)
                {
                    b.FillServerListJsonParse();

                    b.FillUsers();
                } 
            } 
        }

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
#if DEBUG
#else
            // Hide
            ShowWindow(handle, SW_HIDE);
#endif

            //Start();

            Task.Run(async () =>
            {
                while (true)
                {
                    Scan();

                    await Task.Delay(60000*5);
                }
            }).Wait();

        }
    }
}


