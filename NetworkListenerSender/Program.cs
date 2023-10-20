using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static UdpClient client;
    static IPAddress multicastAddress;
    static int port;
    static string name;

    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the Multicast Chat App!");

        Console.Write("Enter your name: ");
        name = Console.ReadLine();
        string groupAddress = "224.224.224.224";
        port = 8081;

        try
        {
            multicastAddress = IPAddress.Parse(groupAddress);
            client = new UdpClient();

            // Join the multicast group
            client.JoinMulticastGroup(multicastAddress);

            // Start sending and receiving messages
            Thread sendThread = new Thread(SendMessages);
            Thread receiveThread = new Thread(ReceiveMessages);

            sendThread.Start();
            receiveThread.Start();

            Console.WriteLine("Type 'exit' to quit.");

            sendThread.Join();
            receiveThread.Join();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            if (client != null)
                client.Close();
        }
    }

    static void SendMessages()
    {
        try
        {
            while (true)
            {
                string message = Console.ReadLine();

                if (message.ToLower() == "exit")
                    break;

                message = $"{name}: {message}";

                byte[] buffer = Encoding.ASCII.GetBytes(message);
                IPEndPoint endPoint = new IPEndPoint(multicastAddress, port);
                client.Send(buffer, buffer.Length, endPoint);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void ReceiveMessages()
    {
        try
        {
            UdpClient receiver = new UdpClient(port);
            receiver.JoinMulticastGroup(multicastAddress);

            while (true)
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                byte[] buffer = receiver.Receive(ref endPoint);
                string message = Encoding.ASCII.GetString(buffer);

                if (!message.StartsWith(name)) // Avoid bouncing back own message
                {
                    Console.WriteLine(message);
                }

                if (message.ToLower() == "exit")
                    break;
            }

            receiver.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
