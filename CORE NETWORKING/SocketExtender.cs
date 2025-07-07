using SR_PROXY.CORE;
using SR_PROXY.ENGINES;
using SR_PROXY.SECURITYOBJECTS;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SR_PROXY.CORE_NETWORKING
{
    public static class SocketExtender
    {
        //Listening operations...
        public static Task<Socket> AcceptDownloadServerConnections(this Socket LISTENER_SOCKET)
        {
            var tcs = new TaskCompletionSource<Socket>();
            LISTENER_SOCKET.BeginAccept(result =>
            {
                ASYNC_SERVER.ARE.Set();
                try
                {
                    Socket CLIENT_SOCKET = (result.AsyncState as Socket).EndAccept(result);//obtaining our client socket
                    tcs.SetResult(CLIENT_SOCKET);
                    //Handling Connections Manager
                    if (ASYNC_SERVER.DW_CONS.TryAdd(CLIENT_SOCKET, new DOWNLOAD_MODULE(CLIENT_SOCKET)))
                        Settings.MAIN.DW_DEF_CC.Text = ASYNC_SERVER.DW_CONS.Count().ToString();

                    if (ASYNC_SERVER.DW_CONS.Count() > 1000)
                        InitializeSeriousMode();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, LISTENER_SOCKET);
            return tcs.Task;
        }
        public static Task<Socket> AcceptGatewayServerConnections(this Socket LISTENER_SOCKET)
        {
            var tcs = new TaskCompletionSource<Socket>();
            LISTENER_SOCKET.BeginAccept(result =>
            {
                ASYNC_SERVER.ARE.Set();
                try
                {
                    Socket CLIENT_SOCKET = (result.AsyncState as Socket).EndAccept(result);//obtaining our client socket
                    tcs.SetResult(CLIENT_SOCKET);
                    //Handling Connections Manager
                    if (ASYNC_SERVER.GW_CONS.TryAdd(CLIENT_SOCKET, new GATEWAY_MODULE(CLIENT_SOCKET)))
                        Settings.MAIN.GW_DEF_CC.Text = ASYNC_SERVER.GW_CONS.Count().ToString();
                    if(ASYNC_SERVER.GW_CONS.Count() > 1000)
                        InitializeSeriousMode();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, LISTENER_SOCKET);
            return tcs.Task;
        }
        public static Task<Socket> AcceptAgentServerConnections(this Socket LISTENER_SOCKET)
        {

            var tcs = new TaskCompletionSource<Socket>();
            LISTENER_SOCKET.BeginAccept(result =>
            {

                ASYNC_SERVER.ARE.Set();
                try
                {
                    Socket CLIENT_SOCKET = (result.AsyncState as Socket).EndAccept(result);//obtaining our client socket
                    string ip = ((IPEndPoint)(CLIENT_SOCKET.RemoteEndPoint)).Address.ToString();
                    tcs.SetResult(CLIENT_SOCKET);
                    //Handling Connections Manager
                    if (ASYNC_SERVER.AG_CONS.TryAdd(CLIENT_SOCKET, new AGENT_MODULE(CLIENT_SOCKET)))
                        Settings.MAIN.AG_DEF_CC.Text = ASYNC_SERVER.AG_CONS.Count().ToString();

                    if (ASYNC_SERVER.AG_CONS.Count() > 1000)
                        InitializeSeriousMode();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, LISTENER_SOCKET);
            return tcs.Task;
        }

        public static Task<Socket> AcceptAgent2ServerConnections(this Socket LISTENER_SOCKET)
        {

            var tcs = new TaskCompletionSource<Socket>();
            LISTENER_SOCKET.BeginAccept(result =>
            {

                ASYNC_SERVER.ARE2.Set();
                try
                {
                    Socket CLIENT_SOCKET = (result.AsyncState as Socket).EndAccept(result);//obtaining our client socket
                    string ip = ((IPEndPoint)(CLIENT_SOCKET.RemoteEndPoint)).Address.ToString();
                    tcs.SetResult(CLIENT_SOCKET);
                    //Handling Connections Manager
                    if (ASYNC_SERVER.AG_CONS.TryAdd(CLIENT_SOCKET, new AGENT_MODULE(CLIENT_SOCKET)))
                        Settings.MAIN.AG_DEF_CC.Text = ASYNC_SERVER.AG_CONS.Count().ToString();

                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, LISTENER_SOCKET);
            return tcs.Task;
        }
        //Recv and Send to Socket operations
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
        //Connect to socket 
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

        public static void InitializeSeriousMode()
        {
            if(!UTILS.isSeriousModeInit)
            {
                UTILS.isSeriousModeInit = true;
                if (UTILS.SeriousModeTimer != null)
                    if (UTILS.SeriousModeTimer.Enabled)
                        UTILS.SeriousModeTimer.Dispose();
                UTILS.SeriousModeTimer = new System.Timers.Timer(60000);
                UTILS.SeriousModeTimer.Elapsed += UTILS.SeriousMode;
                UTILS.SeriousModeTimer.AutoReset = true;
                UTILS.SeriousModeTimer.Enabled = true;
            }
            
        }
        public static void DisconnectFromSocket(this Socket TARGET_SOCKET)
        {
            if (TARGET_SOCKET != null && TARGET_SOCKET.Connected)
            {
                TARGET_SOCKET.Shutdown(SocketShutdown.Both);
                TARGET_SOCKET.Close();
            }
        }
    }
}