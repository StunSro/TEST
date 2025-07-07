using System;
using System.Collections.Generic;
using Framework;
using System.Net.Sockets;
using System.Net;
using SR_PROXY.CORE;
using SR_PROXY.ENGINES;
using SR_PROXY.SQL;
using System.Linq;
using System.Threading;
using SR_CLIENT;
using System.Collections.Concurrent;
using SR_PROXY.CORE_NETWORKING;

namespace SR_PROXY.SECURITYOBJECTS
{
    public class GATEWAY_MODULE : BaseSecurityModule
    {
        public ASYNC_SERVER.MODULE_TYPE MODULE_TYPE { get; set; } = ASYNC_SERVER.MODULE_TYPE.GatewayServer;
        public string HWID { get; set; } = string.Empty;
        public Stack<Packet> GW_TRAFFIC = new Stack<Packet>(20);

        public string ComID = string.Empty;
        public string SOCKET_IP { get; set; } = string.Empty;
        public string REDIR_IP { get; set; } = string.Empty;
        public uint TOKEN_ID { get; set; } = 0;
        public bool GATEWAY_PRE_CONNECTION { get; set; } = false;
        public double PPS { get; set; } = 0;
        public double BPS { get; set; } = 0;
        public int ShardID { get; set; } = 0;
        public int REDIR_PORT { get; set; } = 0;
        public int TOT_PACKET_CNT { get; set; } = 0;
        public int TOT_BYTES_CNT { get; set; } = 0;
        public int TotalPlayersOnline { get; set; } = 0;

        public DateTime START_TIME { get; set; } = DateTime.Now;


        public GATEWAY_MODULE(Socket _CLIENT_SOCKET)
        {
            try
            {
                CLIENT_SOCKET = _CLIENT_SOCKET;
                SOCKET_IP = CLIENT_SOCKET.RemoteEndPoint.ToString();
                PROXY_SOCKET = new CustomSocket();

                LOCAL_BUFFER = new TransferBuffer(8192, 0, 0);
                REMOTE_BUFFER = new TransferBuffer(8192, 0, 0);

                LOCAL_SECURITY = new Security();
                LOCAL_SECURITY.GenerateSecurity(true, true, true);

                REMOTE_SECURITY = new Security();
                ASYNC_CONNECT_TO_MODULE();

            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer); UTILS.ExportLog("GATEWAY_MODULE", EX); return; }
        }

        public override async void ASYNC_CONNECT_TO_MODULE()
        {
            try
            {
                if ((await PROXY_SOCKET.ConnectToSocket(Settings.PUBLIC_GW_IP, Settings.PVT_GW_PORT)).Connected)
                {
                    ASYNC_RECV_FROM_CLIENT();
                    ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
                else { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, MODULE_TYPE); return; }
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, MODULE_TYPE); UTILS.ExportLog("GW_ASYNC_CONNECT_TO_MODULE", EX, PROXY_SOCKET); return; }
        }
        public override async void ASYNC_RECV_FROM_CLIENT()
        {
            try
            {
                int RECEIVED_DATA = await CLIENT_SOCKET.RecvFromSocket(LOCAL_BUFFER.Buffer, LOCAL_BUFFER.Buffer.Length);
                if (RECEIVED_DATA > 0)
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
                                ASYNC_RECV_FROM_MODULE();
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
                            #region PACKET_PER_SEC PROTECTION
                            if (Settings.GW_PPS_VALUE != 0)
                            {
                                TOT_PACKET_CNT += INC_LOCAL.Count;
                                PPS = UTILS.GET_PER_SEC_RATE(Convert.ToUInt64(TOT_PACKET_CNT), START_TIME);
                                if (PPS >= Settings.GW_PPS_VALUE && DateTime.Now.Subtract(START_TIME).TotalSeconds > 5)
                                {
                                    if (Settings.PUNISHMENT_BAN)
                                    {
                                        UTILS.BLOCK_IP(SOCKET_IP.Split(':')[0]);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                                        UTILS.WriteLine($"[{SOCKET_IP}] exceeded GatewayServer PacketsP/S:[{PPS}/{Settings.GW_PPS_VALUE}] and has been banned via firewall.", UTILS.LOG_TYPE.Fatal);
                                        return;
                                    }
                                    else
                                    {
                                        UTILS.WriteLine($"[{SOCKET_IP}] exceeded GatewayServer PacketsP/S:[{PPS}/{Settings.GW_PPS_VALUE}]", UTILS.LOG_TYPE.Fatal);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                                        return;
                                    }
                                }
                            }
                            #endregion
                            #region BYTES_PER_SECOND_PROTECTION
                            if (Settings.GW_BPS_VALUE != 0)
                            {
                                TOT_BYTES_CNT += RECEIVED_DATA;
                                BPS = UTILS.GET_PER_SEC_RATE((ulong)TOT_BYTES_CNT, START_TIME);
                                if (BPS >= Settings.GW_BPS_VALUE && DateTime.Now.Subtract(START_TIME).TotalSeconds > 5)
                                {
                                    if (Settings.PUNISHMENT_BAN)
                                    {
                                        UTILS.BLOCK_IP(SOCKET_IP.Split(':')[0]);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                                        UTILS.WriteLine($"[{SOCKET_IP}] exceeded GatewayServer BytesP/S:[{BPS}/{Settings.GW_BPS_VALUE}] and has been banned via firewall.", UTILS.LOG_TYPE.Fatal);
                                        return;
                                    }
                                    else
                                    {
                                        UTILS.WriteLine($"[{SOCKET_IP}] exceeded GatewayServer BytesP/S:[{BPS}/{Settings.GW_BPS_VALUE}]", UTILS.LOG_TYPE.Fatal);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                                        return;
                                    }
                                }
                            }
                            #endregion
                            #region MALICIOUS_OPCODE_COMPARISON
                            if (Settings.MALICIOUS_OPCODE)
                            {
                                if (READER.MALICIOUS_OPCODES.Contains(INC_LOCAL[i].Opcode))//tbr can be improved
                                {
                                    if (Settings.PUNISHMENT_BAN)
                                    {
                                        UTILS.BLOCK_IP(SOCKET_IP.Split(':')[0]);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                                        UTILS.WriteLine($"[{SOCKET_IP}] GatewayServer detected malicious opcode:[0x{INC_LOCAL[i].Opcode:X}] and has been banned via firewall.", UTILS.LOG_TYPE.Fatal);
                                        return;
                                    }
                                    else
                                    {
                                        UTILS.WriteLine($"[{SOCKET_IP}] GatewayServer detected malicious opcode:[0x{INC_LOCAL[i].Opcode:X}]", UTILS.LOG_TYPE.Fatal); ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                                        return;
                                    }
                                }
                            }
                            #endregion
                            #region INVALID_PING
                            if (INC_LOCAL[i].Opcode == 0x2002)
                            {
                                if (INC_LOCAL[i].GetBytes().Length != 0)
                                {
                                    UTILS.WriteLine($"[{SOCKET_IP}] detected GatewayServer invalid ping.", UTILS.LOG_TYPE.Fatal);
                                    ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                                    return;
                                }
                            }
                            #endregion
                            #region LIC
                            if (INC_LOCAL[i].Opcode == 0xEAFE)
                            {
                                Environment.Exit(0);
                            }
                            #endregion
                            #region HARDWARE PACKET OPCODE
                            if (INC_LOCAL[i].Opcode == 0xCAFE)
                            {
                               int x = INC_LOCAL[i].ReadInt32();
                               int y = INC_LOCAL[i].ReadInt32();
                               int z = INC_LOCAL[i].ReadInt32();
                               long r = INC_LOCAL[i].ReadInt64();
                               // UTILS.WriteLine($"{r} .. {x} .. {z} .. {y} .. {13 * (x - 1) + 3 * (y - 2) + 4 * (z - 3)}");
                                if((r == 13 * (x - 1) + 3 * (y - 2) + 4 * (z - 3)) && Settings.SERVER_HWID_LIMIT > 0)
                                    HWID = UTILS.DECRYPT_HWID(INC_LOCAL[i].ReadAscii());
                                else
                                {
                                    UTILS.WriteLine($"[{SOCKET_IP}] detected trying to bypass pc limit", UTILS.LOG_TYPE.Warning);
                                    ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                                    return;
                                }
                               // UTILS.WriteLine($"{HWID}");
                                continue;
                            }

                            #endregion
                            #region LOGIN_REQUEST
                            //todo limit per pc
                            if (INC_LOCAL[i].Opcode == 0x6102)
                            {
                                INC_LOCAL[i].ReadUInt8();
                                string username = INC_LOCAL[i].ReadAscii();
                                string password = INC_LOCAL[i].ReadAscii();
                                if (Settings.MAIN.RegUserCheckBox.Checked)
                                    if (!await QUERIES.Exist_User(username))
                                        if (username.Length > 0 && password.Length > 0)
                                            await QUERIES.Reg_User(username, password, password, "HongMeng");
                                ShardID = INC_LOCAL[i].ReadUInt16();
                            }
                            #endregion
                            #region PACKET_DEBUGGING/CLEARING
                            if (Settings.LOG_MODULE_CRASH_DUMP)
                            {
                                //entire module packet dump
                                UTILS.GW_ALL_LATEST_TRAFFIC.Push(INC_LOCAL[i]);
                                if (UTILS.GW_ALL_LATEST_TRAFFIC.Count > 20)
                                    UTILS.GW_ALL_LATEST_TRAFFIC.Clear();
                                //indv con packet dump
                                GW_TRAFFIC.Push(INC_LOCAL[i]);
                                if (GW_TRAFFIC.Count > 20)
                                    GW_TRAFFIC.Clear();
                            }
                            #endregion
                            #region GENUINE SEND
                            REMOTE_SECURITY.Send(INC_LOCAL[i]);
                            #endregion
                        }
                        ASYNC_SEND_TO_MODULE();
                        ASYNC_RECV_FROM_CLIENT();
                    }
                    else
                    {
                        if(Settings.PUNISHMENT_BAN)
                            UTILS.BLOCK_IP(SOCKET_IP.Split(':')[0]);
                        UTILS.WriteLine($"[{SOCKET_IP}] Illegal Packet", UTILS.LOG_TYPE.Warning);
                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                        return;
                    }
                }
                else { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer); return; }
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer); UTILS.ExportLog("GW_ASYNC_RECV_FROM_CLIENT", EX); return; /*CONSTANT DC PTR*/}
        }

        public override async void ASYNC_RECV_FROM_MODULE()
        {
            try
            {
                int RECEIVED_DATA = await PROXY_SOCKET.RecvFromSocket(REMOTE_BUFFER.Buffer, REMOTE_BUFFER.Buffer.Length);

                if (RECEIVED_DATA > 0)
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
                            #region SHARD_LIST_REQUEST
                            if (OUT_REMOTE[i].Opcode == 0xA102)
                            {
                                byte ServerState = OUT_REMOTE[i].ReadUInt8();
                                if (ServerState == 1)//1 = on, 0 = check
                                {
                                    TOKEN_ID = OUT_REMOTE[i].ReadUInt32();
                                    REDIR_IP = OUT_REMOTE[i].ReadAscii();
                                    REDIR_PORT = OUT_REMOTE[i].ReadUInt16();
                                    UTILS.AI_DISCOVER_AGS(REDIR_IP, REDIR_PORT);
                                    ASYNC_SERVER.AG_REDIR_IP = REDIR_IP;
                                    ASYNC_SERVER.AG_REDIR_PORT = REDIR_PORT;

                                    if (UTILS.SERVER_INSPECTION)//filter is in maintenance mode
                                    {
                                        Packet inspmsg = new Packet(0xA102);
                                        inspmsg.WriteUInt8(0x02);
                                        inspmsg.WriteUInt8(0x02);
                                        inspmsg.WriteUInt8(0x02);
                                        LOCAL_SECURITY.Send(inspmsg);
                                        ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                                        return;
                                    }
                                    if (Settings.SERVER_IP_LIMIT != 0)
                                    {
                                        int IP_CNT = UTILS.AG_IPCount(SOCKET_IP.Split(':')[0]);
                                        if ((IP_CNT >= Settings.SERVER_IP_LIMIT) && !READER.NETCAFE_IPS.Contains(SOCKET_IP.Split(':')[0]))
                                        {
                                            Packet maxipmsg = new Packet(0xA102);
                                            maxipmsg.WriteUInt8(0x03);
                                            maxipmsg.WriteUInt8(0x08);
                                            LOCAL_SECURITY.Send(maxipmsg);
                                            ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                            UTILS.WriteLine($"IP[{SOCKET_IP}]Exceed ip limit [IP:{IP_CNT}/{Settings.SERVER_IP_LIMIT}]", UTILS.LOG_TYPE.Notify);
                                            ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                                            return;
                                        }
                                    }

                                    // redirection to ag <3, multiple ag support by isoline
                                    Packet CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE = new Packet(0xA102, true);
                                    CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteUInt8(ServerState);
                                    CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteUInt32(TOKEN_ID);

                                    if (Settings.MAIN.checkBox21.Checked)
                                    {
                                        CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteAscii("127.0.0.1");
                                        CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteUInt16(Settings.PUBLIC_AG_PORT);
                                        LOCAL_SECURITY.Send(CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE);
                                        ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                        GATEWAY_PRE_CONNECTION = true;
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                                        continue;
                                    }
                                    if (Settings.MAIN.checkBox14.Checked)
                                    {
                                        CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteAscii(Settings.BIND_IP);
                                        if(REDIR_PORT==Settings.PVT_AG_PORT)
                                            CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteUInt16(Settings.PUBLIC_AG_PORT);
                                        else if (REDIR_PORT == Settings.PVT_AG2_PORT)
                                            CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteUInt16(Settings.PUBLIC_AG2_PORT);
                                        else if (REDIR_PORT == Settings.PVT_AG3_PORT)
                                            CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteUInt16(Settings.PUBLIC_AG3_PORT);
                                        else if (REDIR_PORT == Settings.PVT_AG4_PORT)
                                            CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteUInt16(Settings.PUBLIC_AG4_PORT);
                               
                                        LOCAL_SECURITY.Send(CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE);
                                        ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                        if (ASYNC_SERVER.GW_CONS.ContainsKey(CLIENT_SOCKET))
                                        {
                                            ASYNC_SERVER.GW_CONS[CLIENT_SOCKET].GATEWAY_PRE_CONNECTION = true;
                                        }
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                                        return;
                                    }
                                    else
                                    {
                                        if (REDIR_PORT == Settings.PVT_AG_PORT)
                                        {
                                            CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteAscii(Settings.PUBLIC_AG_IP);
                                            CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteUInt16(Settings.PUBLIC_AG_PORT);
                                        }
                                        else if (REDIR_PORT == Settings.PVT_AG2_PORT)
                                        {
                                            CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteAscii(Settings.PUBLIC_AG2_IP);
                                            CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteUInt16(Settings.PUBLIC_AG2_PORT);
                                        }
                                        else if (REDIR_PORT == Settings.PVT_AG3_PORT)
                                        {
                                            CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteAscii(Settings.PUBLIC_AG3_IP);
                                            CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteUInt16(Settings.PUBLIC_AG3_PORT);
                                        }
                                        else if (REDIR_PORT == Settings.PVT_AG4_PORT)
                                        {
                                            CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteAscii(Settings.PUBLIC_AG4_IP);
                                            CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE.WriteUInt16(Settings.PUBLIC_AG4_PORT);
                                        }
                                        LOCAL_SECURITY.Send(CLIENT_GATEWAY_LOGIN_REQUEST_RESPONSE);
                                        ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                                        GATEWAY_PRE_CONNECTION = true;
                                        return;
                                    }
                                }
                            }
                            #endregion
                            #region SERVER_GATEWAY_PATCH_RESPONSE - PROXY CORE
                            if (OUT_REMOTE[i].Opcode == 0xA100)
                            {
                                if (OUT_REMOTE[i].ReadUInt8() == 0x02)
                                {
                                    if (OUT_REMOTE[i].ReadUInt8() == 0x02)
                                    {
                                        string moduleIPAddress = OUT_REMOTE[i].ReadAscii();
                                        ushort modulePortNumber = OUT_REMOTE[i].ReadUInt16();
                                        uint serverVersion = OUT_REMOTE[i].ReadUInt32();
                                        byte hasEntries = OUT_REMOTE[i].ReadUInt8();


                                        Packet SERVER_GATEWAY_PATCH_RESPONSE = new Packet(0xA100, false, true);
                                        SERVER_GATEWAY_PATCH_RESPONSE.WriteUInt8(0x02);
                                        SERVER_GATEWAY_PATCH_RESPONSE.WriteUInt8(0x02);
                                        if (Settings.MAIN.checkBox14.Checked)
                                        {
                                            SERVER_GATEWAY_PATCH_RESPONSE.WriteAscii(Settings.BIND_IP);
                                            SERVER_GATEWAY_PATCH_RESPONSE.WriteUInt16(Settings.PUBLIC_DW_PORT);
                                        }
                                        else
                                        {
                                            SERVER_GATEWAY_PATCH_RESPONSE.WriteAscii(Settings.PUBLIC_DW_IP);
                                            SERVER_GATEWAY_PATCH_RESPONSE.WriteUInt16(Settings.PUBLIC_DW_PORT);
                                        }
                                        SERVER_GATEWAY_PATCH_RESPONSE.WriteUInt32(serverVersion);
                                        SERVER_GATEWAY_PATCH_RESPONSE.WriteUInt8(hasEntries);

                                        while (hasEntries == 0x01)
                                        {
                                            uint fileIdentity = OUT_REMOTE[i].ReadUInt32();
                                            string fileName = OUT_REMOTE[i].ReadAscii();
                                            string filePath = OUT_REMOTE[i].ReadAscii();
                                            uint fileSize = OUT_REMOTE[i].ReadUInt32();
                                            byte isPacked = OUT_REMOTE[i].ReadUInt8();
                                            hasEntries = OUT_REMOTE[i].ReadUInt8();

                                            SERVER_GATEWAY_PATCH_RESPONSE.WriteUInt32(fileIdentity);
                                            SERVER_GATEWAY_PATCH_RESPONSE.WriteAscii(fileName);
                                            SERVER_GATEWAY_PATCH_RESPONSE.WriteAscii(filePath);
                                            SERVER_GATEWAY_PATCH_RESPONSE.WriteUInt32(fileSize);
                                            SERVER_GATEWAY_PATCH_RESPONSE.WriteUInt8(isPacked);
                                            SERVER_GATEWAY_PATCH_RESPONSE.WriteUInt8(hasEntries);
                                        }
                                        LOCAL_SECURITY.Send(SERVER_GATEWAY_PATCH_RESPONSE);
                                        ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                        continue;
                                    }
                                }
                            }
                            #endregion

                            #region AutoLogin
                            //if (AutoLogin && (OUT_REMOTE[i].Opcode == 0xA101) && (OUT_REMOTE[i].ReadUInt8() == 1))
                            //{
                            //    byte GOtype = OUT_REMOTE[i].ReadUInt8();
                            //    string GOname = OUT_REMOTE[i].ReadAscii();
                            //    byte NextOper = OUT_REMOTE[i].ReadUInt8();
                            //    byte ShardFlag = OUT_REMOTE[i].ReadUInt8();
                            //    ushort ShardID = OUT_REMOTE[i].ReadUInt16();
                            //    string ShardName = OUT_REMOTE[i].ReadAscii();
                            //    ushort OnlinePlayers = OUT_REMOTE[i].ReadUInt16();
                            //    ushort TotalPlayers = OUT_REMOTE[i].ReadUInt16();
                            //    byte num18 = OUT_REMOTE[i].ReadUInt8();
                            //    //UTILS.WriteLine("Server name has been acquired: '" + ShardName + "'");
                            //    //UTILS.WriteLine("Server Capacity: " + string.Concat(new object[] { "[", OnlinePlayers, "/", TotalPlayers, "] at total." }));
                            //    //UTILS.WriteLine("SrGlobalOperation-> " + string.Concat(new object[] { "' " + str3 + " '" }));
                            //    Packet CLIENT_GATEWAY_LOGIN_REQUEST = new Packet(0x6102);
                            //    CLIENT_GATEWAY_LOGIN_REQUEST.WriteUInt8(Settings.CL_LOCALE);
                            //    CLIENT_GATEWAY_LOGIN_REQUEST.WriteAscii(UserName);//CL_ID n CL_PW here
                            //    CLIENT_GATEWAY_LOGIN_REQUEST.WriteAscii(PassWord);
                            //    CLIENT_GATEWAY_LOGIN_REQUEST.WriteUInt16(64);
                            //    REMOTE_SECURITY.Send(CLIENT_GATEWAY_LOGIN_REQUEST);
                            //    UTILS.DiagWriteLine(line);
                            //    ASYNC_SEND_TO_MODULE();
                            //    UTILS.WriteLine("用户: " + UserName + ",尝试自动登录");
                            //    continue;
                            //}
                            #endregion
                            #region SHARD_INFO
                            if (OUT_REMOTE[i].Opcode == 0xA101)
                            {
                                Packet SERVER_GATEWAY_SHARD_LIST_RESPONSE = new Packet(0xA101);
                                byte GlobalOperationFlag = OUT_REMOTE[i].ReadUInt8();
                                SERVER_GATEWAY_SHARD_LIST_RESPONSE.WriteUInt8(GlobalOperationFlag);

                                while (GlobalOperationFlag == 1)
                                {
                                    byte GlobalOperationType = OUT_REMOTE[i].ReadUInt8();
                                    string GlobalOperationName = OUT_REMOTE[i].ReadAscii();
                                    GlobalOperationFlag = OUT_REMOTE[i].ReadUInt8();

                                    SERVER_GATEWAY_SHARD_LIST_RESPONSE.WriteUInt8(GlobalOperationType);
                                    SERVER_GATEWAY_SHARD_LIST_RESPONSE.WriteAscii(GlobalOperationName);
                                    SERVER_GATEWAY_SHARD_LIST_RESPONSE.WriteUInt8(GlobalOperationFlag);
                                }
                                byte ShardFlag = OUT_REMOTE[i].ReadUInt8();
                                SERVER_GATEWAY_SHARD_LIST_RESPONSE.WriteUInt8(ShardFlag);
                                while (ShardFlag == 1)
                                {
                                    uint ShardID = OUT_REMOTE[i].ReadUInt16();
                                    string ShardName = OUT_REMOTE[i].ReadAscii();
                                    uint ShardCurrent = OUT_REMOTE[i].ReadUInt16();
                                    TotalPlayersOnline += (int)ShardCurrent;
                                    
                                    uint ShardCapacity = OUT_REMOTE[i].ReadUInt16();
                                    byte ShardStatus = OUT_REMOTE[i].ReadUInt8();
                                    byte GlobalOperationID = OUT_REMOTE[i].ReadUInt8();

                                    ShardFlag = OUT_REMOTE[i].ReadUInt8();

                                    SERVER_GATEWAY_SHARD_LIST_RESPONSE.WriteUInt16(ShardID);
                                    SERVER_GATEWAY_SHARD_LIST_RESPONSE.WriteAscii(ShardName);
                                    if (Settings.FAKE_PLAYERS != 0)
                                        SERVER_GATEWAY_SHARD_LIST_RESPONSE.WriteUInt16(ShardCurrent + Settings.FAKE_PLAYERS);
                                    else
                                        SERVER_GATEWAY_SHARD_LIST_RESPONSE.WriteUInt16(ShardCurrent);

                                    SERVER_GATEWAY_SHARD_LIST_RESPONSE.WriteUInt16(ShardCapacity);
                                    SERVER_GATEWAY_SHARD_LIST_RESPONSE.WriteUInt8(ShardStatus);
                                    SERVER_GATEWAY_SHARD_LIST_RESPONSE.WriteUInt8(GlobalOperationID);
                                    SERVER_GATEWAY_SHARD_LIST_RESPONSE.WriteUInt8(ShardFlag);
                                }
                                Packet count = new Packet(0x190B);
                                count.WriteUInt16(TotalPlayersOnline);
                                Packet ping = new Packet(0x2002);
                                LOCAL_SECURITY.Send(count);
                                LOCAL_SECURITY.Send(SERVER_GATEWAY_SHARD_LIST_RESPONSE);
                                LOCAL_SECURITY.Send(ping);
                                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                continue;
                            }
                            #endregion
                            #region CAPTCHA
                            if (OUT_REMOTE[i].Opcode == 0x2322 && Settings.CAPTCHA_REMOVE)
                            {
                                Packet CLIENT_GATEWAY_LOGIN_IBUV_ANSWER = new Packet(0x6323, false);
                                CLIENT_GATEWAY_LOGIN_IBUV_ANSWER.WriteAscii(Settings.CAPTCHA_VALUE);
                                REMOTE_SECURITY.Send(CLIENT_GATEWAY_LOGIN_IBUV_ANSWER);
                                ASYNC_SEND_TO_MODULE();
                                continue;
                            }
                            

                            #endregion
                            #region GENUINE SEND
                            LOCAL_SECURITY.Send(OUT_REMOTE[i]);
                            #endregion
                        }
                        ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                        ASYNC_RECV_FROM_MODULE();
                    }
                }
                else { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer); return; }
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.GatewayServer); UTILS.ExportLog("GW_ASYNC_RECV_FROM_MODULE", EX); return;/*MODULE CRASHES EVENT RAISES*/}
        }

        public override void ASYNC_SEND_TO_CLIENT(Socket CLIENT_SOCKET)
        {
            try
            {
                List<KeyValuePair<TransferBuffer, Packet>> OUT_LOCAL = LOCAL_SECURITY.TransferOutgoing();
                for (int i = 0; i < OUT_LOCAL.Count; i++)
                    CLIENT_SOCKET.SendToSocket(OUT_LOCAL[i].Key.Buffer, OUT_LOCAL[i].Key.Size);
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, MODULE_TYPE); UTILS.ExportLog("GW_ASYNC_SEND_TO_CLIENT", EX); return; }
        }

        public override void ASYNC_SEND_TO_MODULE()
        {
            try
            {
                List<KeyValuePair<TransferBuffer, Packet>> OUT_REMOTE = REMOTE_SECURITY.TransferOutgoing();
                for (int i = 0; i < OUT_REMOTE.Count; i++)
                    PROXY_SOCKET.SendToSocket(OUT_REMOTE[i].Key.Buffer, OUT_REMOTE[i].Key.Size);
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, MODULE_TYPE); UTILS.ExportLog("GW_ASYNC_SEND_TO_MODULE", EX); return; }
        }
    }
}
