using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LoggerHost
{
    // Class to handle receiving string messages over TCP
    public class TcpReceiver
    {
        public delegate void MessageReceivedHandler(string message);
        public event MessageReceivedHandler OnMessageReceived;

        private readonly int _port;
        private bool _isRunning;
        private Thread _listenerThread;
        private TcpListener _listener;

        public TcpReceiver(int port)
        {
            _port = port;
        }

        public void Start()
        {
            _isRunning = true;
            _listenerThread = new Thread(ListenForMessages);
            _listenerThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
            if (_listenerThread != null && _listenerThread.IsAlive)
            {
                _listenerThread.Join();
            }
            Console.WriteLine("Receiver stopped.");
        }

        private void ListenForMessages()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), _port);
                _listener.Start();
                Console.WriteLine("Receiver is listening for messages...");

                while (_isRunning)
                {
                    if (_listener.Pending())
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        Console.WriteLine("Client connected.");

                        Thread clientThread = new Thread(() => HandleClient(client));
                        clientThread.Start();
                    }

                    Thread.Sleep(10); // Prevent tight loop
                }

            }
            catch (SocketException ex) when (!_isRunning)
            {
                Console.WriteLine("Listener stopped due to shutdown.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving message: {ex.Message}");
            }
            finally
            {
                _listener?.Stop();
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (_isRunning && client.Connected)
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);

                        if (bytesRead > 0)
                        {
                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            Console.WriteLine($"Message received: {message}");

                            var messages = message.Split("¶");

                            foreach( var m in messages )
                            {
                                if (m.Length < 1) continue;

                                OnMessageReceived?.Invoke(m);
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(10); // Prevent tight loop
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing client: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Client disconnected.");
                client.Close();
            }
        }
    }

}
