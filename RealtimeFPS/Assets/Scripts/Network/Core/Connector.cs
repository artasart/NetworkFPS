using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Framework.Network
{
    public static class Connector
    {
        public static async Task<bool> Connect( IPEndPoint endPoint, Connection connection )
        {
            try
            {
                Socket socket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(endPoint);

                socket.NoDelay = true;

                ServerSession session = new();
                connection.Session = session;
                connection.Session.Start(socket);
                connection.Session.OnConnected(endPoint);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return false;
            }
        }
    }
}