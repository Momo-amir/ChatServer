using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    class Program
    {
        static List<TcpClient> clients = new List<TcpClient>();

        static void Main(string[] args)
        {
            Console.WriteLine("Starting server...");
            TcpListener listener = new TcpListener(IPAddress.Any, 1234);
            listener.Start();

            Console.WriteLine("Listening on port 1234...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected");

                clients.Add(client);

                NetworkStream stream = client.GetStream();

                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
                string username = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine(username + " has joined the chat");

                byte[] welcomeBuffer = Encoding.ASCII.GetBytes("Welcome to the chat, " + username);
                stream.Write(welcomeBuffer, 0, welcomeBuffer.Length);

                // Start a new thread to handle client messages
                System.Threading.Thread t = new System.Threading.Thread(() => HandleClient(client, username));
                t.Start();
            }
        }

        static void HandleClient(TcpClient client, string username)
        {
            NetworkStream stream = client.GetStream();

            while (true)
            {
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine(username + ": " + message);

                // Relay message to all connected clients
                foreach (TcpClient c in clients)
                {
                    if (c != client)
                    {
                        NetworkStream s = c.GetStream();
                        byte[] relayBuffer = Encoding.ASCII.GetBytes(username + ": " + message);
                        s.Write(relayBuffer, 0, relayBuffer.Length);
                    }
                }
            }
        }
    }
}
