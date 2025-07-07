using Framework;
using System.Net.Sockets;


namespace SR_PROXY.SECURITYOBJECTS
{
    public abstract class BaseSecurityModule
    {
        public Socket PROXY_SOCKET { get; set; }
        public Socket CLIENT_SOCKET { get; set; }
        public Security LOCAL_SECURITY { get; set; }
        public Security REMOTE_SECURITY { get; set; }
        public TransferBuffer LOCAL_BUFFER { get; set; }
        public TransferBuffer REMOTE_BUFFER { get; set; }

        public abstract void ASYNC_CONNECT_TO_MODULE();
        public abstract void ASYNC_RECV_FROM_CLIENT();
        public abstract void ASYNC_RECV_FROM_MODULE();
        public abstract void ASYNC_SEND_TO_CLIENT(Socket CLIENT_SOCKET);
        public abstract void ASYNC_SEND_TO_MODULE();
    }
}
