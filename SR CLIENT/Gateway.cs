using System;
using System.Collections.Generic;
using Framework;
using System.Net.Sockets;
using SR_PROXY;
using SR_PROXY.ENGINES;
using SR_PROXY.SQL;

namespace SR_CLIENT
{
    public class Gateway
    {
        public Security gw_security;
        public Socket gw_socket;
        public Agent m_Agent;
        public string _username, _password, _charname;
        byte[] gw_buffer = new byte[4096];
        public Gateway(string IP, int Port, string _username, string _password, string _charname)
        {
            try
            {
                this._username = _username;
                this._password = _password;
                this._charname = _charname;
                gw_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                gw_socket.NoDelay = true;
                gw_security = new Security();
                gw_socket.BeginConnect(IP, Port, result =>
                {
                    Socket CLIENTLESS_SOCKET = result.AsyncState as Socket;
                    if (gw_socket.Connected)
                    {
                        CLIENTLESS_SOCKET.EndConnect(result);
                        gw_socket.BeginReceive(gw_buffer, 0, gw_buffer.Length, SocketFlags.None, out SocketError SOCKET_ERROR, new AsyncCallback(CLIENTLESS_RECV_FROM_SERVER), gw_socket);
                    }
                }, gw_socket);
            }
            catch (Exception Ex)
            {
                UTILS.WriteLine($"假人GA连接失败：{Ex.ToString()}", UTILS.LOG_TYPE.Fatal);
                Client.SR_CLIENT_Close(_username);
            }
        }

        async void CLIENTLESS_RECV_FROM_SERVER(IAsyncResult result)
        {
            try
            {
                int Data = (result.AsyncState as Socket).EndReceive(result, out SocketError SOCKET_ERROR);
                if (Data != 0 && SOCKET_ERROR == SocketError.Success)
                {
                    gw_security.Recv(gw_buffer, 0, Data);
                    List<Packet> RemotePackets = gw_security.TransferIncoming();
                    if (RemotePackets != null)
                    {
                        foreach (Packet packet in RemotePackets)
                        {
                            #region CLIENTLESS_LOG
                            //if (Settings.MAIN.checkBox13.Checked)
                            //{
                            //    byte[] packet_bytes = packet.GetBytes();
                            //    string line = (string.Format("[S=>C][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet_bytes.Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet_bytes), Environment.NewLine));
                            //    UTILS.CSWriteLine(line);
                            //}
                            #endregion
                            #region SERVER_GLOBAL_HANDSHAKE - SR_CLIENT CORE
                            if (packet.Opcode == 0x5000 || packet.Opcode == 0x9000)
                            {
                                CLIENTLESS_SEND_TO_SERVER();
                                continue;
                            }
                            #endregion
                            #region X_GLOBAL_IDENTIFICATION - SR_CLIENT CORE
                            if (packet.Opcode == 0x2001)
                            {
                                if (packet.ReadAscii() == "GatewayServer")
                                {
                                    Packet CLIENT_ID_RESPONSE = new Packet(0x6100, true, false);

                                    CLIENT_ID_RESPONSE.WriteUInt8(Settings.CL_LOCALE);
                                    CLIENT_ID_RESPONSE.WriteAscii("SR_Client");
                                    CLIENT_ID_RESPONSE.WriteUInt32(await QUERIES.获取版本号());
                                    gw_security.Send(CLIENT_ID_RESPONSE);
                                    CLIENTLESS_SEND_TO_SERVER();
                                }
                            }
                            #endregion
                            #region SERVER_GATEWAY_PATCH_RESPONSE - SR_CLIENT CORE
                            if (packet.Opcode == 0xA100)
                            {
                                byte status = packet.ReadUInt8();
                                if (status == 1)
                                {
                                    //UTILS.WriteLine("1) 成功连接到 GatewayServer.");
                                    Packet CLIENT_GATEWAY_SERVERLIST_REQUEST = new Packet(0x6101, true);

                                    gw_security.Send(CLIENT_GATEWAY_SERVERLIST_REQUEST);
                                    CLIENTLESS_SEND_TO_SERVER();
                                }
                                else
                                {
                                    status = packet.ReadUInt8();
                                    if (status == 0x02) // Updates available
                                    {
                                        String ip = packet.ReadAscii();
                                        ushort port = packet.ReadUInt16();
                                        uint new_version = packet.ReadUInt32();

                                    }
                                    else if (status == 0x04) // Server down
                                    {
                                        UTILS.WriteLine("服务器维护中.");
                                    }
                                    else if (status == 0x05) // CL_VERSION too old
                                    {
                                        UTILS.WriteLine("Client CL_VERSION is too old.");
                                    }
                                    else if (status == 0x01) // CL_VERSION too new
                                    {
                                        UTILS.WriteLine("Client CL_VERSION is too new.");
                                    }
                                    else
                                    {
                                        UTILS.WriteLine("Unknown response {0}." + result);
                                    }
                                }
                            }
                            #endregion
                            #region SERVER_GATEWAY_SERVERLIST_RESPONSE CORE
                            if ((packet.Opcode == 0xA101) && (packet.ReadUInt8() == 1))
                            {
                                byte GOtype = packet.ReadUInt8();
                                string GOname = packet.ReadAscii();
                                byte NextOper = packet.ReadUInt8();
                                byte ShardFlag = packet.ReadUInt8();
                                ushort ShardID = packet.ReadUInt16();
                                string ShardName = packet.ReadAscii();
                                ushort OnlinePlayers = packet.ReadUInt16();
                                ushort TotalPlayers = packet.ReadUInt16();
                                byte num18 = packet.ReadUInt8();
                                //UTILS.WriteLine("Server name has been acquired: '" + ShardName + "'");
                                //UTILS.WriteLine("Server Capacity: " + string.Concat(new object[] { "[", OnlinePlayers, "/", TotalPlayers, "] at total." }));
                                //UTILS.WriteLine("SrGlobalOperation-> " + string.Concat(new object[] { "' " + str3 + " '" }));
                                Packet CLIENT_GATEWAY_LOGIN_REQUEST = new Packet(0x6102);
                                CLIENT_GATEWAY_LOGIN_REQUEST.WriteUInt8(22);
                                CLIENT_GATEWAY_LOGIN_REQUEST.WriteAscii(_username);//CL_ID n CL_PW here
                                CLIENT_GATEWAY_LOGIN_REQUEST.WriteAscii(_password);
                                CLIENT_GATEWAY_LOGIN_REQUEST.WriteUInt16(ShardID);
                                gw_security.Send(CLIENT_GATEWAY_LOGIN_REQUEST);
                                CLIENTLESS_SEND_TO_SERVER();
                            }
                            #endregion
                            #region SERVER_GATEWAY_LOGIN_IBUV_CHALLENGE - SR_CLIENT CORE
                            if (packet.Opcode == 0x2322)
                            {
                                Packet CLIENT_GATEWAY_LOGIN_IBUV_ANSWER = new Packet(0x6323);
                                CLIENT_GATEWAY_LOGIN_IBUV_ANSWER.WriteAscii(Settings.CL_CAPTCHA_VALUE);
                                gw_security.Send(CLIENT_GATEWAY_LOGIN_IBUV_ANSWER);
                                CLIENTLESS_SEND_TO_SERVER();
                                //UInt32[] pixels = Captcha.GeneratePacketCaptcha(packet);
                                //Random rnd = new Random();
                                //Captcha.SaveCaptchaToBMP(pixels, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + rnd.Next(000000, 999999) + ".bmp");

                            }
                            #endregion
                            #region SERVER_GATEWAY_LOGIN_RESPONSE - SR_CLIENT CORE
                            if (packet.Opcode == 0xA102)
                            {
                                byte status = packet.ReadUInt8();
                                if (status == 1)//Success!
                                {
                                    uint LoginID = packet.ReadUInt32();
                                    string ip = packet.ReadAscii();
                                    int port = packet.ReadUInt16();

                                    m_Agent = new Agent(ip, port, LoginID, _username, _password, _charname, this);
                                    // gw module will dc after ag is connected.

                                }
                                if (status == 2)//Failure
                                {
                                    switch (packet.ReadUInt8())
                                    {
                                        case 1:
                                            {
                                                byte totalattempts = packet.ReadUInt8();
                                                byte num7 = packet.ReadUInt8();
                                                byte num8 = packet.ReadUInt8();
                                                byte num9 = packet.ReadUInt8();
                                                byte attempts = packet.ReadUInt8();
                                                Client.SR_CLIENT_Close(_username);
                                                break;
                                            }
                                        case 2:
                                            if (packet.ReadUInt8() == 1)
                                            { UTILS.WriteLine("You have been blocked, Reason: " + packet.ReadAscii()); }
                                            Client.SR_CLIENT_Close(_username);
                                            break;
                                        case 3:
                                            UTILS.WriteLine("Character is already logged on.");
                                            Client.SR_CLIENT_Close(_username);
                                            break;

                                        default:
                                            {
                                                UTILS.WriteLine("Login refsued server is offline/full/GM-Privliges or some kind of another err.");
                                                break;
                                            }
                                    }
                                }
                            }
                            #endregion
                            #region CLIENT_GATEWAY_LOGIN_IBUV_ANSWER_RESPONSE
                            if (packet.Opcode == 0xA323)
                            {
                                byte capnum = packet.ReadUInt8();
                                if (capnum == 1)
                                {
                                    //UTILS.WriteLine("2) 验证码填写成功.");
                                }
                                else
                                { UTILS.WriteLine("Captcha is incorrect, please try again."); }
                            }
                            #endregion
                        }
                    }
                    gw_socket.BeginReceive(gw_buffer, 0, gw_buffer.Length, SocketFlags.None, out SOCKET_ERROR, new AsyncCallback(CLIENTLESS_RECV_FROM_SERVER), gw_socket);
                }
            }
            catch (Exception ex)
            {
                UTILS.WriteLine($"假人GS：{ex.ToString()}", UTILS.LOG_TYPE.Fatal);
                Client.SR_CLIENT_Close(_username, false);
            }
        }

        public void CLIENTLESS_SEND_TO_SERVER()
        {
            try
            {
                List<KeyValuePair<TransferBuffer, Packet>> List = gw_security.TransferOutgoing();
                if (List != null)
                {
                    foreach (var kvp in List)
                    {
                        gw_socket.BeginSend(kvp.Key.Buffer, 0, kvp.Key.Size, SocketFlags.None, out SocketError SOCKET_ERROR, result => { (result.AsyncState as Socket).EndSend(result); }, gw_socket);
                        #region CLIENTLESS_LOG
                        //if (Settings.MAIN.checkBox13.Checked)
                        //{
                        //    byte[] packet_bytes = kvp.Value.GetBytes();
                        //    string line = (string.Format("[C=>S][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", kvp.Value.Opcode, packet_bytes.Length, kvp.Value.Encrypted ? "[Encrypted]" : "", kvp.Value.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet_bytes), Environment.NewLine));
                        //    UTILS.CSWriteLine(line);
                        //}
                        #endregion
                    }
                }
            }
            catch { }
        }

        public void CLIENTLESS_DISCONNECT()
        {
            try
            {

                if (gw_socket != null && gw_socket.Connected)
                    gw_socket.BeginDisconnect(false, result => { (result.AsyncState as Socket).EndDisconnect(result); }, gw_socket);
            }
            catch { }
        }
    }
}

