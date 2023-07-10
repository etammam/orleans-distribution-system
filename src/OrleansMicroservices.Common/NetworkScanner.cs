using System.Net.Sockets;
using System.Net;

namespace OrleansMicroservices.Common;

public class NetworkScanner
{
    public static int GetPort()
    {
        var port = 0;
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            var localEp = new IPEndPoint(IPAddress.Any, 0);
            socket.Bind(localEp);
            localEp = (IPEndPoint)socket.LocalEndPoint!;
            port = localEp.Port;
        }
        finally
        {
            socket.Close();
        }
        return port;
    }
}
