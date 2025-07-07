using System;
using System.Collections.Generic;
using Framework;
using System.Net.Sockets;
using System.Net;
using SR_PROXY.CORE;
using SR_PROXY.ENGINES;
using System.Linq;

namespace SR_PROXY.SECURITYOBJECTS
{
    public class DOWNLOAD_MODULE
    {
        public ASYNC_SERVER.MODULE_TYPE MODULE_TYPE = ASYNC_SERVER.MODULE_TYPE.DownloadServer;
        public Socket PROXY_SOCKET;
        public Socket CLIENT_SOCKET;
        TransferBuffer LOCAL_BUFFER = new TransferBuffer(4096, 0, 0);
        TransferBuffer REMOTE_BUFFER = new TransferBuffer(4096, 0, 0);
        public double PPS, BPS = 0;
        public int TOT_PACKET_CNT, TOT_BYTES_CNT = 0;
        public string SOCKET_IP = string.Empty;
        public Security LOCAL_SECURITY;
        public Security REMOTE_SECURITY;
        public DateTime START_TIME = DateTime.Now;


        public DOWNLOAD_MODULE(Socket _CLIENT_SOCKET)
        {
            try
            {
                CLIENT_SOCKET = _CLIENT_SOCKET;
                PROXY_SOCKET = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                PROXY_SOCKET.NoDelay = true;
                PROXY_SOCKET.BeginConnect(Settings.PUBLIC_DW_IP, Settings.PVT_DW_PORT, result =>
                {
                    Socket CURRENT_PROXY_SOCKET = result.AsyncState as Socket;
                    if (UTILS.IsSocketConnected(CLIENT_SOCKET) && UTILS.IsSocketConnected(PROXY_SOCKET))
                    {
                        CURRENT_PROXY_SOCKET.EndConnect(result);

                        MODULE_TYPE = ASYNC_SERVER.MODULE_TYPE.DownloadServer;

                        LOCAL_SECURITY = new Security();
                        LOCAL_SECURITY.GenerateSecurity(true, true, true);

                        REMOTE_SECURITY = new Security();
                        CLIENT_SOCKET.BeginReceive(LOCAL_BUFFER.Buffer, 0, LOCAL_BUFFER.Buffer.Length, SocketFlags.None, out SocketError SOCKET_ERROR_HANDLER, new AsyncCallback(ASYNC_RECV_FROM_CLIENT), CLIENT_SOCKET);
                        ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                    }
                    else { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.DownloadServer);   return; }
                }, PROXY_SOCKET);
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.DownloadServer); UTILS.ExportLog("DOWNLOAD_MODULE", EX); return; }
        }

        void ASYNC_RECV_FROM_CLIENT(IAsyncResult result)
        {
            try
            {
                Socket CLIENT_SOCKET = result.AsyncState as Socket;
                int RECEIVED_DATA = CLIENT_SOCKET.EndReceive(result, out SocketError SOCKET_ERROR_HANDLER);
                if (RECEIVED_DATA > 0 && SOCKET_ERROR_HANDLER == SocketError.Success)
                {
                    LOCAL_SECURITY.Recv(LOCAL_BUFFER.Buffer, 0, RECEIVED_DATA);

                    List<Packet> INC_LOCAL = LOCAL_SECURITY.TransferIncoming();
                    if (INC_LOCAL != null)
                    {
                        for (int i = 0; i < INC_LOCAL.Count; i++)
                        {
                            #region X_GLOBAL_IDENTIFICATION RESPONSE - PROXY CORE
                            if (INC_LOCAL[i].Opcode == 0x2001)
                            {
                                PROXY_SOCKET.BeginReceive(REMOTE_BUFFER.Buffer, 0, REMOTE_BUFFER.Buffer.Length, SocketFlags.None, new AsyncCallback(ASYNC_RECV_FROM_MODULE), CLIENT_SOCKET);
                                continue;
                            }
                            #endregion
                            #region SERVER_GLOBAL_HANDSHAKE RESPONSE - PROXY CORE
                            if (INC_LOCAL[i].Opcode == 0x5000 || INC_LOCAL[i].Opcode == 0x9000)
                            {
                                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                continue;
                            }
                            #endregion
                            //#region PACKET_PER_SEC PROTECTION
                            //if (Settings.GW_PPS_VALUE != 0)
                            //{
                            //    TOT_PACKET_CNT += INC_LOCAL.Count;
                            //    PPS = UTILS.GET_PER_SEC_RATE(Convert.ToUInt64(TOT_PACKET_CNT), START_TIME);
                            //    if (PPS >= Settings.GW_PPS_VALUE && DateTime.Now.Subtract(START_TIME).TotalSeconds > 5)
                            //    {
                            //        if (Settings.PUNISHMENT_BAN)
                            //        {
                            //            UTILS.BLOCK_IP(SOCKET_IP.Split(':')[0]);
                            //            ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, PROXY_SOCKET);
                            //            UTILS.WriteLine($"[{SOCKET_IP}] exceeded DownloadServer PacketsP/S:[{PPS}/{Settings.GW_PPS_VALUE}] and has been banned via firewall.", UTILS.LOG_TYPE.Fatal);
                            //            return;
                            //        }
                            //        else
                            //        {
                            //            UTILS.WriteLine($"[{SOCKET_IP}] exceeded DownloadServer PacketsP/S:[{PPS}/{Settings.GW_PPS_VALUE}]", UTILS.LOG_TYPE.Fatal);
                            //            ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, PROXY_SOCKET);
                            //            return;
                            //        }
                            //    }
                            //}
                            //#endregion
                            //#region BYTES_PER_SECOND_PROTECTION
                            //if (Settings.GW_BPS_VALUE != 0)
                            //{
                            //    TOT_BYTES_CNT += RECEIVED_DATA;
                            //    BPS = UTILS.GET_PER_SEC_RATE((ulong)TOT_BYTES_CNT, START_TIME);
                            //    if (BPS >= Settings.GW_BPS_VALUE && DateTime.Now.Subtract(START_TIME).TotalSeconds > 5)
                            //    {
                            //        if (Settings.PUNISHMENT_BAN)
                            //        {
                            //            UTILS.BLOCK_IP(SOCKET_IP.Split(':')[0]);
                            //            ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, PROXY_SOCKET);
                            //            UTILS.WriteLine($"[{SOCKET_IP}] exceeded DownloadServer BytesP/S:[{BPS}/{Settings.GW_BPS_VALUE}] and has been banned via firewall.", UTILS.LOG_TYPE.Fatal);
                            //            return;
                            //        }
                            //        else
                            //        {
                            //            UTILS.WriteLine($"[{SOCKET_IP}] exceeded DownloadServer BytesP/S:[{BPS}/{Settings.GW_BPS_VALUE}]", UTILS.LOG_TYPE.Fatal);
                            //            ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, PROXY_SOCKET);
                            //            return;
                            //        }
                            //    }
                            //}
                            //#endregion
                            #region MALICIOUS_OPCODE_COMPARISON
                            if (Settings.MALICIOUS_OPCODE)
                            {
                                if (READER.MALICIOUS_OPCODES.Contains(INC_LOCAL[i].Opcode))//tbr can be improved
                                {
                                    if (Settings.PUNISHMENT_BAN)
                                    {
                                        UTILS.BLOCK_IP(SOCKET_IP.Split(':')[0]);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.DownloadServer);
                                        UTILS.WriteLine($"[{SOCKET_IP}] DwonloadServer detected malicious opcode:[0x{INC_LOCAL[i].Opcode:X}] and has been banned via firewall.", UTILS.LOG_TYPE.Fatal);
                                        return;
                                    }
                                    else
                                    {
                                        UTILS.WriteLine($"[{SOCKET_IP}] DwonloadServer detected malicious opcode:[0x{INC_LOCAL[i].Opcode:X}]", UTILS.LOG_TYPE.Fatal); ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.DownloadServer);
                                        return;
                                    }
                                }
                            }
                            #endregion
                            #region CLIENT_GLOBAL_KEEP_ALIVE
                            if (INC_LOCAL[i].Opcode == 0x2002)
                            {
                                if (INC_LOCAL[i].GetBytes().Length != 0)
                                {
                                    UTILS.WriteLine($"[{SOCKET_IP}] detected DownloadServer invalid ping.", UTILS.LOG_TYPE.Fatal);
                                    ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.DownloadServer);
                                    return;
                                }
                            }
                            #endregion
                            #region GENUINE SEND
                            REMOTE_SECURITY.Send(INC_LOCAL[i]);
                            #endregion
                        }
                        ASYNC_SEND_TO_MODULE();
                        CLIENT_SOCKET.BeginReceive(LOCAL_BUFFER.Buffer, 0, LOCAL_BUFFER.Buffer.Length, SocketFlags.None, out SOCKET_ERROR_HANDLER, new AsyncCallback(ASYNC_RECV_FROM_CLIENT), CLIENT_SOCKET);
                    }
                }
                else { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.DownloadServer); return; }
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.DownloadServer); UTILS.ExportLog("DW_ASYNC_RECV_FROM_CLIENT", EX); return; /*CONSTANT DC PTR*/}
        }

        void ASYNC_RECV_FROM_MODULE(IAsyncResult result)
        {
            try
            {
                Socket CLIENT_SOCKET = result.AsyncState as Socket;
                int RECEIVED_DATA = PROXY_SOCKET.EndReceive(result, out SocketError SOCKET_ERROR_HANDLER);
                if (RECEIVED_DATA > 0 && SOCKET_ERROR_HANDLER == SocketError.Success)
                {
                    REMOTE_SECURITY.Recv(REMOTE_BUFFER.Buffer, 0, RECEIVED_DATA);
                    List<Packet> OUT_REMOTE = REMOTE_SECURITY.TransferIncoming();
                    if (OUT_REMOTE != null)
                    {
                        for (int i = 0; i < OUT_REMOTE.Count; i++)
                        {
                            #region SERVER_GLOBAL_HANDSHAKE - PROXY CORE
                            if (OUT_REMOTE[i].Opcode == 0x5000 || OUT_REMOTE[i].Opcode == 0x9000)
                            {
                                ASYNC_SEND_TO_MODULE();
                                continue;
                            }
                            #endregion
                            #region GENUINE SEND
                            LOCAL_SECURITY.Send(OUT_REMOTE[i]);
                            #endregion
                        }
                        ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                        PROXY_SOCKET.BeginReceive(REMOTE_BUFFER.Buffer, 0, REMOTE_BUFFER.Buffer.Length, SocketFlags.None, out SOCKET_ERROR_HANDLER, new AsyncCallback(ASYNC_RECV_FROM_MODULE), CLIENT_SOCKET);
                    }
                }
                else { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.DownloadServer); return; }
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.DownloadServer); UTILS.ExportLog("DW_ASYNC_RECV_FROM_MODULE", EX); return;/*MODULE CRASHES EVENT RAISES*/}
        }

        void ASYNC_SEND_TO_CLIENT(Socket CLIENT_SOCKET)
        {
            try
            {
                List<KeyValuePair<TransferBuffer, Packet>> OUT_LOCAL = LOCAL_SECURITY.TransferOutgoing();
                if (OUT_LOCAL != null)
                    for (int i = 0; i < OUT_LOCAL.Count; i++)
                    {
                        CLIENT_SOCKET.BeginSend(OUT_LOCAL[i].Key.Buffer, 0, OUT_LOCAL[i].Key.Size, SocketFlags.None, out SocketError SOCKET_ERROR_HANDLER, result =>
                        {
                            Socket SEND_LOCAL_SOCKET = result.AsyncState as Socket;
                            SEND_LOCAL_SOCKET.EndSend(result, out SOCKET_ERROR_HANDLER);
                        }, CLIENT_SOCKET);
                    }
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.DownloadServer); UTILS.ExportLog("DW_ASYNC_SEND_TO_CLIENT", EX); return; }
        }

        void ASYNC_SEND_TO_MODULE()
        {
            try
            {
                List<KeyValuePair<TransferBuffer, Packet>> OUT_REMOTE = REMOTE_SECURITY.TransferOutgoing();
                if (OUT_REMOTE != null)
                    for (int i = 0; i < OUT_REMOTE.Count; i++)
                    {
                        PROXY_SOCKET.BeginSend(OUT_REMOTE[i].Key.Buffer, 0, OUT_REMOTE[i].Key.Size, SocketFlags.None, out SocketError SOCKET_ERROR_HANDLER, result =>
                        {
                            Socket SEND_REMOTE_SOCKET = result.AsyncState as Socket;
                            SEND_REMOTE_SOCKET.EndSend(result, out SOCKET_ERROR_HANDLER);
                        }, PROXY_SOCKET);
                    }
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.DownloadServer); UTILS.ExportLog("DW_ASYNC_SEND_TO_MODULE", EX); return; }
        }
    }
}
