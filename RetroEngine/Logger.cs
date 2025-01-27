using RetroEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RetroEngine
{
    public static class Logger
    {

        static TcpSender tcpSender = new TcpSender(2004);

        public static void Log(object s)
        {

            s = $"[{System.DateTime.Now.ToLongTimeString()}] {s}";

            Console.WriteLine(s);
            if(GameMain.Instance.devMenu is not null)
                GameMain.Instance.devMenu.Log(s==null? "null" : s.ToString());

            tcpSender.AddMessage(s.ToString());

        }

        // Class to handle sending string messages over TCP
        public class TcpSender
        {
            private readonly int _port;

            TcpClient client;

            Queue<string> queue = new Queue<string>();

            public TcpSender(int port)
            {
                _port = port;

                

                new Thread(() => { SendLoop(); }).Start();

            }

            void SendLoop()
            {
                while (true)
                {
                    int count = 0;
                    lock (queue)
                        count = queue.Count;

                    if (count == 0)
                    {
                        Thread.Sleep(33);
                        continue;
                    }

                    string msg;
                    lock (queue)
                    msg = queue.Dequeue();
                    SendMessage(msg);

                }
            }

            public void AddMessage(string message)
            {
                lock (queue)
                    queue.Enqueue(message);
            }

            void SendMessage(string message)
            {

                if(client == null)
                    try { client = new TcpClient("localhost", _port); } catch { }

                try
                {

                    NetworkStream stream = client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(message+ "¶");

                    stream.Write(data, 0, data.Length);

                }
                catch (Exception ex)
                {
                    client?.Close();
                    client = new TcpClient("localhost", _port);
                }
            }
        }

    }
}
