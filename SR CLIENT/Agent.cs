using System;
using System.Collections.Generic;
using Framework;
using System.Net.Sockets;
using SR_PROXY;
using SR_PROXY.ENGINES;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SR_PROXY.CORE_NETWORKING;

namespace SR_CLIENT
{
    public class Agent
    {
        public Security ag_security;
        public Socket ag_socket;
        public Gateway gateway;
        byte[] ag_buffer = new byte[4096];
        static DateTime lastpingedtime = new DateTime();
        public string _username, _password, _charname;
        int lastX, lastY, Last_Party;
        uint loginID, UNIQUE_ID;

        public Agent(string IP, int Port, uint _loginID, string _username, string _password, string _charname, Gateway _Gateway)
        {
            try
            {
                this._username = _username;
                this._password = _password;
                this._charname = _charname;
                ag_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                gateway = _Gateway;
                ag_socket.NoDelay = true;
                ag_security = new Security();
                loginID = _loginID;
                ag_socket.BeginConnect(IP, Port, result =>
                {
                    Socket CLIENTLESS_SOCKET = result.AsyncState as Socket;
                    if (ag_socket.Connected)
                    {
                        CLIENTLESS_SOCKET.EndConnect(result);
                        gateway.CLIENTLESS_DISCONNECT();
                        CLIENTLESS_RECV_FROM_SERVER();
                    }
                }, ag_socket);
            }
            catch (Exception Ex)
            {
                UTILS.WriteLine($"Dummy Connection to AG failed：{Ex.ToString()}", UTILS.LOG_TYPE.Fatal);
                Client.SR_CLIENT_Close(_username);
            }
        }
        public void Ping()
        {
            Packet ping = new Packet(0x2002);
            ag_security.Send(ping);
            CLIENTLESS_SEND_TO_SERVER();
        }
        async void CLIENTLESS_RECV_FROM_SERVER()
        {
            try
            {
                int RECEIVED_DATA = await ag_socket.RecvFromSocket(ag_buffer, ag_buffer.Length);
                if (RECEIVED_DATA > 0)
                {
                    ag_security.Recv(ag_buffer, 0, RECEIVED_DATA);
                    List<Packet> RemotePackets = ag_security.TransferIncoming();
                    if (RemotePackets != null)
                    {
                        foreach (Packet packet in RemotePackets.ToList())
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
                                if (packet.ReadAscii() == "AgentServer")
                                {
                                    Packet p = new Packet(0x6103, true);
                                    p.WriteUInt32(loginID);
                                    p.WriteAscii(_username.ToLower());
                                    p.WriteAscii(_password);
                                    p.WriteUInt8(22);
                                    p.WriteUInt32(0);//mac
                                    p.WriteUInt16(0);//mac pt 2
                                    ag_security.Send(p);
                                    CLIENTLESS_SEND_TO_SERVER();
                                }
                            }
                            #endregion
                            #region SERVER_AGENT_LOGIN_RESPONSE - SR_CLIENT CORE
                            if (packet.Opcode == 0xA103)
                            {
                                if (packet.ReadUInt8() == 1)
                                {
                                    Packet CLIENT_AGENT_CHARACTER_SELECTION_REQUEST = new Packet(0x7007);
                                    CLIENT_AGENT_CHARACTER_SELECTION_REQUEST.WriteUInt8(2);
                                    ag_security.Send(CLIENT_AGENT_CHARACTER_SELECTION_REQUEST);
                                    CLIENTLESS_SEND_TO_SERVER();

                                }
                            }
                            #endregion
                            #region CLIENT_AGENT_CHARACTER_SELECTION_REQUEST_RESPONSE - SR_CLIENT CORE
                            if (packet.Opcode == 0xB007)
                            {
                                int Type = packet.ReadInt16();
                                if (Type == 258)
                                {
                                    int Num = packet.ReadInt8();
                                    if (Num > 0)
                                    {
                                        if (UTILS.BytesFind(packet.GetBytes(), Encoding.Default.GetBytes(_charname)) == -1)
                                        {
                                            if (Num < 4)
                                            {
                                                UTILS.WriteLine($"No Character Found Starting Creating New: [{_charname}]", UTILS.LOG_TYPE.Default);
                                                CreateChar(_charname);
                                            }
                                            else
                                            {
                                                UTILS.WriteLine($"Account [{_username}] Cannot be created.", UTILS.LOG_TYPE.Fatal);
                                                Client.SR_CLIENT_Close(_username, false);
                                            }
                                        }
                                        else
                                        {
                                            Packet CLIENT_AGENT_CHARACTER_SELECTION_JOIN_REQUEST = new Packet(0x7001);
                                            CLIENT_AGENT_CHARACTER_SELECTION_JOIN_REQUEST.WriteAscii(_charname);//char name here
                                            ag_security.Send(CLIENT_AGENT_CHARACTER_SELECTION_JOIN_REQUEST);
                                            CLIENTLESS_SEND_TO_SERVER();

                                        }

                                    }
                                    else
                                    {
                                        UTILS.WriteLine($"No characters was found started creating char :[{_charname}]", UTILS.LOG_TYPE.Default);
                                        CreateChar(_charname);
                                    }
                                }
                                else if (Type == 257)
                                {
                                    UTILS.WriteLine($"Created Char Successfully [{_charname}]", UTILS.LOG_TYPE.Default);
                                    Packet CLIENT_AGENT_CHARACTER_SELECTION_JOIN_REQUEST = new Packet(0x7001);
                                    CLIENT_AGENT_CHARACTER_SELECTION_JOIN_REQUEST.WriteAscii(_charname);//char name here
                                    ag_security.Send(CLIENT_AGENT_CHARACTER_SELECTION_JOIN_REQUEST);
                                    CLIENTLESS_SEND_TO_SERVER();
                                }

                            }
                            #endregion
                            #region SERVER_AGENT_CHARACTER_CELESTIAL_POSITION
                            if (packet.Opcode == 0x3020)
                            {
                                if (packet.GetBytes().Length >= 4)
                                {
                                    UNIQUE_ID = packet.ReadUInt32();
                                    //UTILS.WriteLine("",$"unqid:{UNIQUE_ID}");
                                }

                                Packet CLIENT_SPAWN_SUCCESSS = new Packet(0x3012); //CLIENT_SPAWN_SUCCESSS
                                ag_security.Send(CLIENT_SPAWN_SUCCESSS);
                                //Client.SR_CLIENT_ADD(gateway);
                                if(Settings.MAIN.checkBox8.Checked)
                                    OpenParty();

                            }
                            #endregion
                            #region ENTERED?
                            if (packet.Opcode == 0x3305)
                            {
                                    int dstX = new Random(Guid.NewGuid().GetHashCode()).Next(6417, 6450);
                                    int dstY = new Random(Guid.NewGuid().GetHashCode()).Next(1062, 1127);
                                    if (Settings.MAIN.checkBox10.Checked)
                                        DoMovement(dstX, dstY);
                                    if (Settings.MAIN.checkBox8.Checked)
                                        OpenParty();
                                    if (Settings.MAIN.checkBox9.Checked)
                                        OpenStall();
                            }
                            #endregion
                            #region SERVER_PARTY_RESPONSE
                            if (packet.Opcode == 0xB069)
                            {
                                Party_Response(packet);
                            }
                            #endregion
                            #region DELETE_PARTY_MATCHING
                            if (packet.Opcode == 0xB06B)
                            {
                                DeleteParty(packet);
                            }
                            #endregion
                            #region MOVEMENT_RESPONSE
                            if (packet.Opcode == 0xB021)
                            {
                                Movement(packet);
                            }
                            #endregion
                            #region OPENED_SUCCESSFULLY
                            if (packet.Opcode == 0xB0B1)
                            {
                                WelcomeStallMsg();
                            }
                            #endregion
                            #region SPAWN CONFIRMATION
                            if (packet.Opcode == 0x34B5) //spawn confirmation request
                            {
                                Packet p = new Packet(0x34b6);//approve
                                ag_security.Send(p);
                                CLIENTLESS_SEND_TO_SERVER();
                                //UTILS.WriteLine("[AT]", "2 - spawn request approved");
                            }
                            #endregion
                            #region EOT ACR SYSTEM
                            if (packet.Opcode == 0x3809)//End of char teleportation
                            {
                                if (!string.IsNullOrEmpty(_charname))
                                {
                                    Packet p2 = new Packet(0x7010);
                                    p2.WriteUInt16(17);
                                    p2.WriteUnicode(_charname);
                                    ag_security.Send(p2); //UTILS.WriteLine("[AT]", "3 - /recalluser packet sent.");
                                    CLIENTLESS_SEND_TO_SERVER();
                                    string CHARNAME16_2 = _charname;
                                    //_charname = string.Empty;
                                }
                            }
                            #endregion
                            #region PREVENT OVERFLOW
                            if (RemotePackets.Count > 100)//prevent overflow
                            {
                                RemotePackets.Clear();
                            }
                            #endregion
                        }
                        CLIENTLESS_RECV_FROM_SERVER();
                    }
                }
            }
            catch (Exception ex)
            {
                UTILS.WriteLine($"Dummy AG：{ex.ToString()}", UTILS.LOG_TYPE.Fatal);
                Client.SR_CLIENT_Close(_username, false);
            }
        }
        private void Party_Response(Packet packet)
        {
            byte Type = packet.ReadUInt8();
            if (Type == 1)
            {
                Last_Party = packet.ReadInt32();
            }
        }
        private void DeleteParty(Packet packet)
        {
            byte Type = packet.ReadUInt8();
            if (Type == 1)
            {
                Last_Party = 0;
                OpenParty();
            }
        }
        private void Movement(Packet packet)
        {
            uint id = packet.ReadUInt32();
            if (id == UNIQUE_ID)
            {
                byte movement_type = packet.ReadUInt8();
                if (movement_type == 1)
                {
                    byte xsec = packet.ReadUInt8();
                    byte ysec = packet.ReadUInt8();
                    float xcoord = 0;
                    float zcoord = 0;
                    float ycoord = 0;
                    if (ysec == 0x80)
                    {
                        xcoord = packet.ReadSingle();
                        zcoord = packet.ReadSingle();
                        ycoord = packet.ReadSingle();
                    }
                    else
                    {
                        xcoord = packet.ReadUInt16();
                        zcoord = packet.ReadUInt16();
                        ycoord = packet.ReadUInt16();
                    }
                    int x = UTILS.CalculateX(xsec, xcoord);
                    int y = UTILS.CalculateY(ysec, ycoord);
                    lastX = x;
                    lastY = y;
                }
            }
        }

        public void CLIENTLESS_SEND_TO_SERVER()
        {
            try
            {
                List<KeyValuePair<TransferBuffer, Packet>> List = ag_security.TransferOutgoing();
                if (List != null)
                    foreach (var kvp in List)
                        ag_socket.SendToSocket(kvp.Key.Buffer, kvp.Key.Size);
            }
            catch { }
        }

        public void CLIENTLESS_DISCONNECT()
        {
            try
            {

                if (ag_socket != null && ag_socket.Connected) ag_socket.BeginDisconnect(false, result => { (result.AsyncState as Socket).EndDisconnect(result); }, ag_socket);
            }
            catch { }
        }
        public void OpenParty()
        {
            Packet OpenParty = new Packet(0x7069);
            OpenParty.WriteUInt32(0);
            OpenParty.WriteUInt32(0);
            OpenParty.WriteUInt8(0x08);
            OpenParty.WriteInt8(0);
            OpenParty.WriteInt8(1);
            OpenParty.WriteInt8(120);
            OpenParty.WriteAsciiA(Settings.MAIN.textBox228.Text);
            ag_security.Send(OpenParty);
            CLIENTLESS_SEND_TO_SERVER();
        }
        public void RemoveParty()
        {
            Packet RemoveParty = new Packet(0x706B);
            ag_security.Send(RemoveParty);
            CLIENTLESS_SEND_TO_SERVER();
        }

        public void OpenStall()
        {
            Packet stall = new Packet(0x70B1);
            stall.WriteAsciiA(Settings.MAIN.textBox231.Text);
            ag_security.Send(stall);
            CLIENTLESS_SEND_TO_SERVER();

        }
        public void CloseStall()
        {
            Packet close = new Packet(0x70B2);
            ag_security.Send(close);
            CLIENTLESS_SEND_TO_SERVER();

        }
        public void WelcomeStallMsg()
        {
            Packet Welcome = new Packet(0x70BA);
            Welcome.WriteAscii($"Welcome kanaka to [{_charname}] stall!");
            ag_security.Send(Welcome);
            CLIENTLESS_SEND_TO_SERVER();
        }
        public void CreateChar(string Name)
        {
            Packet CharPackage = new Packet(0x7007);
            CharPackage.WriteUInt8(1);
            CharPackage.WriteAscii(Name);
            CharPackage.WriteInt16(1908);
            CharPackage.WriteInt16(0);
            CharPackage.WriteUInt8(0x22);
            int Set = new Random(Guid.NewGuid().GetHashCode()).Next(0, 2);
            CharPackage.WriteUInt32(3637 + Set * 3);
            CharPackage.WriteUInt32(3638 + Set * 3);
            CharPackage.WriteUInt32(3639 + Set * 3);
            int Weapon = new Random(Guid.NewGuid().GetHashCode()).Next(0, 4);
            CharPackage.WriteUInt32(3632 + Weapon);
            ag_security.Send(CharPackage);
            CLIENTLESS_SEND_TO_SERVER();
        }
        public void DoMovement(int X, int Y)
        {
            int xPos = 0;
            int yPos = 0;

            if (X > 0 && Y > 0)
            {
                xPos = (int)((X % 192) * 10);
                yPos = (int)((Y % 192) * 10);
            }
            else
            {
                if (X < 0 && Y > 0)
                {
                    xPos = (int)((192 + (X % 192)) * 10);
                    yPos = (int)((Y % 192) * 10);
                }
                else
                {
                    if (X > 0 && Y < 0)
                    {
                        xPos = (int)((X % 192) * 10);
                        yPos = (int)((192 + (Y % 192)) * 10);
                    }
                }
            }


            byte xSector = (byte)((X - (int)(xPos / 10)) / 192 + 135);
            byte ySector = (byte)((Y - (int)(yPos / 10)) / 192 + 92);

            Packet packet = new Packet(0x7021);
            packet.WriteInt8(0x01);
            int xposition = (int)((X - (int)((xSector - 135) * 192)) * 10);
            int yposition = (int)((Y - (int)((ySector - 92) * 192)) * 10);
            packet.WriteInt8(xSector);
            packet.WriteInt8(ySector);

            packet.WriteUInt16((ushort)xposition);
            packet.WriteUInt16(0x0000);
            packet.WriteUInt16((ushort)yposition);
            ag_security.Send(packet);
            CLIENTLESS_SEND_TO_SERVER();
        }
    }
}
