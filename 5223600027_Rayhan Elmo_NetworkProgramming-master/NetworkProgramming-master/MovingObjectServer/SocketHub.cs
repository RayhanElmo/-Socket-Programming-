using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace MovingObjectServer
{
    public static class SocketHub
    {
        private static Socket listener;
        private static readonly List<Socket> Clients = new List<Socket>();
        private static readonly object Gate = new object();

        public static void Start(int port = 11111)
        {
            if (listener != null) return;
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Any, port));
            listener.Listen(10);

            var t = new Thread(AcceptLoop) { IsBackground = true };
            t.Start();
            Console.WriteLine($"[SocketHub] Listening on {port}");
        }

        private static void AcceptLoop()
        {
            while (true)
            {
                try
                {
                    var client = listener.Accept();
                    lock (Gate) { Clients.Add(client); }
                    Console.WriteLine("[SocketHub] Client connected.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("AcceptLoop error: " + ex.Message);
                    break;
                }
            }
        }

        public static void BroadcastPosition(int x, int y)
        {
            Broadcast($"{x},{y}\n");
        }

        public static void Broadcast(string msg)
        {
            var data = Encoding.UTF8.GetBytes(msg);
            List<Socket> dead = null;

            lock (Gate)
            {
                foreach (var c in Clients)
                {
                    try { c.Send(data); }
                    catch
                    {
                        if (dead == null) dead = new List<Socket>();
                        dead.Add(c);
                    }
                }

                if (dead != null)
                {
                    foreach (var d in dead)
                    {
                        try { d.Shutdown(SocketShutdown.Both); d.Close(); } catch { }
                        Clients.Remove(d);
                    }
                }
            }
        }

        public static void Stop()
        {
            try { listener?.Close(); } catch { }
            listener = null;
            lock (Gate)
            {
                foreach (var c in Clients)
                {
                    try { c.Shutdown(SocketShutdown.Both); c.Close(); } catch { }
                }
                Clients.Clear();
            }
        }
    }
}
