using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MovingObjectClient
{
    public partial class Form1 : Form
    {
        private int posX = 10;
        private int posY = 10;
        private readonly string clientName;

        public Form1(string name = "CLIENT")
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.ClientSize = new Size(400, 300);
            this.clientName = name;

            // jalankan koneksi di thread background
            var t = new Thread(StartClient) { IsBackground = true };
            t.Start();
        }

        private void StartClient()
        {
            try
            {
                string serverIp = "127.0.0.1"; // ganti dengan IP server jika beda PC
                int port = 11111;

                var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.Connect(new IPEndPoint(IPAddress.Parse(serverIp), port));

                var buffer = new byte[1024];
                var sb = new StringBuilder();

                while (true)
                {
                    int n = sock.Receive(buffer);
                    if (n == 0) break;

                    sb.Append(Encoding.UTF8.GetString(buffer, 0, n));

                    while (true)
                    {
                        var s = sb.ToString();
                        int idx = s.IndexOf('\n');
                        if (idx < 0) break;

                        var line = s.Substring(0, idx);
                        sb.Remove(0, idx + 1);

                        var parts = line.Split(',');
                        if (parts.Length == 2 &&
                            int.TryParse(parts[0], out var x) &&
                            int.TryParse(parts[1], out var y))
                        {
                            posX = x;
                            posY = y;
                            this.Invoke((MethodInvoker)(() => this.Invalidate()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)(() => MessageBox.Show("Client error: " + ex.Message)));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.FillRectangle(Brushes.Blue, posX, posY, 30, 30);
            var font = new Font("Arial", 12);
            e.Graphics.DrawString(clientName, font, Brushes.Black, 10, 10);
        }
    }
}
