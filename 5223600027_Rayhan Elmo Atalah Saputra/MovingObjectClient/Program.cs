using System;
using System.Windows.Forms;

namespace MovingObjectClient
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string name = args.Length > 0 ? args[0] : "CLIENT";
            Application.Run(new Form1(name));
        }
    }
}
