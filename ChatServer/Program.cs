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
            // Get list of interface IP addresses
            IPAddress[] IPList = Dns.GetHostAddresses(Dns.GetHostName());
            
            // Create list for storing IPv4 IP addresses
            List<IPAddress> useableIPList = new();

            // Iterate through IPList, show each IPv4 address and add them to the useableIPList
            int count = 0;
            foreach (IPAddress IP in IPList)
            {
                if (IP.AddressFamily == AddressFamily.InterNetwork)
                { 
                    Console.WriteLine($"[{count++}]: {IP}");
                    useableIPList.Add(IP);
                }
            }
            
            // Loop until user selects an integer that is within the list
            int interfaceSelection;
            do Console.Write("\nSelect IP/Interface to bind to: ");
            while (!int.TryParse(Console.ReadLine(), out interfaceSelection) || interfaceSelection > useableIPList.Count() - 1 || interfaceSelection < 0);
            
            Console.WriteLine($"Starting server on {useableIPList[interfaceSelection]}...");
            TcpListener listener = new TcpListener(useableIPList[interfaceSelection], 1234);
            listener.Start();

            Console.WriteLine("Listening on port 1234...");

            // Listen for clients 
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected");

                // Add connected client to list of clients
                clients.Add(client);

                NetworkStream stream = client.GetStream();

                // Create buffer byte array
                byte[] buffer = new byte[client.ReceiveBufferSize];

                // Get username encoded in bytes
                int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);

                // Decode stored bytes into string using ASCII decoding
                string username = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine(username + " has joined the chat");

                // Encode welcome message into bytes with ASCII encoding
                byte[] welcomeBuffer = Encoding.ASCII.GetBytes("Welcome to the chat, " + username);

                // Write bytes to socket stream
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
                // Create buffer byte array
                byte[] buffer = new byte[client.ReceiveBufferSize];

                // Get message encoded in bytes
                int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);

                // Decode stored bytes into string using ASCII decoding
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine(username + ": " + message);

                // Relay message to all connected clients
                foreach (TcpClient c in clients)
                {
                    if (c != client)
                    {
                        NetworkStream s = c.GetStream();

                        // Create relayBuffer byte array and encode message into bytes with ASCII encoding
                        byte[] relayBuffer = Encoding.ASCII.GetBytes(username + ": " + message);

                        // Write bytes to socket stream
                        s.Write(relayBuffer, 0, relayBuffer.Length);
                    }
                }
            }
        }
    }
}
