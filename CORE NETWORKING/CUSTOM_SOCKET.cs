using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.CORE_NETWORKING
{
    class CUSTOM_SOCKET
    {
        Socket Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);



        public CUSTOM_SOCKET()
        {
            Sock.NoDelay = true;
            //return Sock;
        }
    }
}
