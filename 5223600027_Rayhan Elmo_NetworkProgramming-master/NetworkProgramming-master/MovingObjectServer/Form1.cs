using System;
using System.Drawing;
using System.Windows.Forms;

namespace MovingObjectServer
{
    public partial class Form1 : Form
    {
        private int posX = 10;
        private int posY = 10;
        private Timer timer;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.ClientSize = new Size(450, 350);

            // Start Socket server
            SocketHub.Start(11111);

            // Setup timer to move object
            timer = new Timer();
            timer.Interval = 100; // ms
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // simple movement
            posX = (posX + 7) % this.ClientSize.Width;
            posY = (posY + 4) % this.ClientSize.Height;

            // push to clients
            SocketHub.BroadcastPosition(posX, posY);

            // redraw
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.FillRectangle(Brushes.Blue, posX, posY, 30, 30);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timer?.Stop();
            SocketHub.Stop(); // cleanup sockets
            base.OnFormClosing(e);
        }
    }
}
