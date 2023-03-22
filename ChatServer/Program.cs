using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer { 
class Program
{
    static void Main(string[] args)
    {
        // Start listening for incoming connections.
        TcpListener listener = new TcpListener(IPAddress.Any, 8080);
        listener.Start();
        Console.WriteLine("Chat server started.");

        // Create a new thread for each client.
        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Thread thread = new Thread(new ParameterizedThreadStart(HandleClient));
            thread.Start(client);
        }
    }

    static void HandleClient(object obj)
    {
        // Get the client object and create a network stream.
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        // Prompt the client for their username.
        byte[] data = new byte[256];
        int bytes = stream.Read(data, 0, data.Length);
        string username = Encoding.ASCII.GetString(data, 0, bytes);

        // Add the client to the chat room.
        Console.WriteLine("{0} has joined the chat room.", username);
        byte[] welcomeMessage = Encoding.ASCII.GetBytes("Welcome to the chat room, " + username + "!");
        BroadcastMessage(welcomeMessage);

        // Start receiving messages from the client.
        while (true)
        {
            bytes = stream.Read(data, 0, data.Length);
            string message = Encoding.ASCII.GetString(data, 0, bytes);
            Console.WriteLine("{0}: {1}", username, message);
            BroadcastMessage(data);
        }

        // Clean up.
        stream.Close();
        client.Close();
    }

    static void BroadcastMessage(byte[] message)
    {
        // Send the message to all connected clients.
        foreach (TcpClient client in clients)
        {
            NetworkStream stream = client.GetStream();
            stream.Write(message, 0, message.Length);
        }
    }

    static List<TcpClient> clients = new List<TcpClient>();
}}