using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.CORE_NETWORKING
{
    public static class SOCKET_EXTENDER
    {
        public static Task<Socket> ConnectToSocket(this Socket TARGET_SOCKET, string Host, int Port)
        {
            var tcs = new TaskCompletionSource<Socket>();
            if (TARGET_SOCKET != null && !TARGET_SOCKET.Connected)
                TARGET_SOCKET.BeginConnect(Host, Port, result =>
                {
                    Socket CURRENT_CONNECTED_SOCKET = result.AsyncState as Socket;
                    if (CURRENT_CONNECTED_SOCKET.Connected)
                    {
                        CURRENT_CONNECTED_SOCKET.EndConnect(result);
                        tcs.SetResult(CURRENT_CONNECTED_SOCKET);
                    }
                    else tcs.SetResult(null);
                }, TARGET_SOCKET);
            else tcs.SetResult(null);
            return tcs.Task;
        }
        public static Task<int> SendToSocket(this Socket TARGET_SOCKET, byte[] TransferBuffer, int Length)
        {
            var tcs = new TaskCompletionSource<int>();
            if (TARGET_SOCKET != null && TARGET_SOCKET.Connected)
                TARGET_SOCKET.BeginSend(TransferBuffer, 0, Length, SocketFlags.None, out SocketError SOCKET_ERROR_HANDLER, result =>
                {
                    Socket CURRENT_SEND_SOCKET = result.AsyncState as Socket;
                    if (CURRENT_SEND_SOCKET.Connected)//After Dispose() is called .Connected property is set to false.
                        tcs.SetResult(CURRENT_SEND_SOCKET.EndSend(result, out SOCKET_ERROR_HANDLER));
                    else tcs.SetResult(0);
                }, TARGET_SOCKET);
            else tcs.SetResult(0);
            return tcs.Task;
        }
        public static Task<int> RecvFromSocket(this Socket RECV_SOCKET, byte[] TransferBuffer, int Length)
        {
            var tcs = new TaskCompletionSource<int>();
            if (RECV_SOCKET != null && RECV_SOCKET.Connected)
                RECV_SOCKET.BeginReceive(TransferBuffer, 0, Length, SocketFlags.None, out SocketError SOCKET_ERROR_HANDLER, result =>
                {
                    try
                    {
                        Socket CLIENT_SOCKET = result.AsyncState as Socket;
                        if (CLIENT_SOCKET.Connected)//After Dispose() is called .Connected property is set to false.
                            tcs.SetResult(CLIENT_SOCKET.EndReceive(result, out SOCKET_ERROR_HANDLER));
                        else tcs.SetResult(0);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                }, RECV_SOCKET);
            else tcs.SetResult(0);
            return tcs.Task;
        }
    }
}
