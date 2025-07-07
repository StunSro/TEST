using Fleck;
using Framework;
using SR_PROXY.CORE;
using SR_PROXY.CORE_NETWORKING;
using SR_PROXY.ENGINES;
using SR_PROXY.GameSpawn;
using SR_PROXY.MODEL;
using SR_PROXY.MSSQL_SERVER;
using SR_PROXY.SQL;
using SRO_PROXY.GS;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace SR_PROXY.SECURITYOBJECTS
{
    public class AGENT_MODULE : BaseSecurityModule
    {
        #region SESSION_PROPERTIES
        public ASYNC_SERVER.MODULE_TYPE MODULE_TYPE = ASYNC_SERVER.MODULE_TYPE.AgentServer;
        public DateTime LAST_ATTEMPT_TO_RIDE,LAST_PLAYER_SEEN, SPAWNED_START_TIME, TELEPORT_START_TIME, LAST_STALL_TIME, LAST_GLOBAL_TIME, LAST_EXREQ_TIME, LAST_GUILDREQ_TIME, LAST_UNIREQ_TIME, LAST_JOB_REQTIME, LAST_RROREXIT_DELAY, LAST_ZERK_TIME,LAST_PURCHASE_CUSTOMTITLE_DELAY,LAST_UPDATERANKING_DELAY,LAST_UPDATETITLE_DELAY,LAST_RELOAD_DELAY,LAST_NEWREVERSE_DELAY,LAST_DAILYREWARD_CLICK,LAST_UPDATEUNIQUES_DELAY,LAST_UPDATETIMER_DELAY,LAST_CHEST_DELAY,LAST_UPDATE_TITLE,LAST_GAMBLE_TIME,LAST_XSMBLOG_TIMNE = new DateTime();
        public bool HASGMPRIV, GOODS_SELL_FLAG, DEAD_STATUS,HAS_JOB_PET, RIDING_PET, CTF_REG, HELPER_MARK_STATUS, INVISIBLE_STATUS, PVP_FLAG, JOB_FLAG,STORAGE_OPENED, JOB_YELLOW_LINE, NONE_CLIENTLESS_MARKER, FIRST_CLIENT_SPAWN_SUCCESS, FIRST_CLIENT_CHAR_SELECT, ZERKING, INSIDE_JC, INSIDE_TT, INSIDE_FW, INSIDE_BA, INSIDE_FGW, INSIDE_CTF, INSIDE_PT,INSIDE_JF,INSIDE_SURV,INSIDE_DRUNK, STALLING, AFK, DISSCONNECT_REQUEST = false;
        public string ComID = string.Empty;
        public Stack<Packet> AG_TRAFFIC = new Stack<Packet>(20);
        public GATEWAY_MODULE CORRESPONDING_GW_SESSION = null;
        public int CharID { get; set; } = 0;
        public int CUR_LEVEL { get; set; } = 0;
        public int CUR_REGION { get; set; } = 0;
        public int RIDING_PET_UNIQUEID { get; set; } = 0;

        public int JID { get; set; } = 0;
        public string 安全密码 ="666666",NickName16 = string.Empty, CHARNAME16 = string.Empty, CHARNAME16_HOLDER = string.Empty, SOCKET_IP = string.Empty, IP = string.Empty, UserName = string.Empty,GUILDNAME = string.Empty;
        public uint CURRENT_HP, TARGET_TELEPORT, TOKEN_ID = 0, UNIQUE_ID = 0;
        public short LATEST_REGOINING;
        //public System.Timers.Timer SILK_TIMER;
        public double BPS = 0, PPS = 0;
        public int LOCK_PIN, PING_COUNTER, TOT_PACKET_CNT = 0, TOT_BYTES_CNT = 0;
        public uint FELLOW_PET = 0;
        public int TeamStart, StallStart, ShardID = 64, JobLevel = 0,Silk_GOLD;
        public DateTime START_TIME = DateTime.Now;
        public int PartyID=0,JobType = 0;//1商2贼3镖师
        public bool 职业状态 = false,卡号求助中=false;
        public byte Xsec =0, Ysec=0;
        public short x=0,z=0,y=0;
        uint LeaderType; // Type
        uint LeaderRace; // Race
        uint LeaderPurpose; // Purpose of party
        uint LeaderEntryLevel; // Enter level
        uint LeaderMaxLevel; // Max level
        string LeaderTitle; // Title
        #endregion
        #region SESSION_PROPERTIES2
        public bool CharLockStatue = false;
        public string CharLockCurPassword = null;
        public List<uint> PartyMembers;
        public ConcurrentDictionary<string, List<string>> CharTitles;
        public Dictionary<byte, SavedLocation> SavedLocations;
        public Dictionary<int, _CharChest> CharChest;
        public List<_XsmbLog> XsmbLog;
        public bool ISEURO = false;
        public bool ISCHIN = false;
        public bool IsReverseSent = false;
        public bool IsChangeLogSent = false;
        public bool ISXSMBsent = false;
        public bool DMGMeterGui = false;
        public bool FWKillsGui = false;
        public bool SURVKillsGui = false;
        public bool IsDailySent = false;
        #endregion
        public uint ObjCharID { get; set; } = 0;

        public int MonstersKillCount { get; set; } = 0;
        public int PlayersKillCount { get; set; } = 0;
        public DateTime LastMonstersKill { get; set; } = new DateTime();
        public DateTime LastPlayersKill { get; set; } = new DateTime();
        public int INEVENT_EVENTID { get; set; } = 0;
        public int FortressRegion { get; set; } = 0;

        public AGENT_MODULE(Socket _CLIENT_SOCKET)
        {
            try
            {
                CLIENT_SOCKET = _CLIENT_SOCKET;
                SOCKET_IP = CLIENT_SOCKET.RemoteEndPoint.ToString();
                PROXY_SOCKET = new CustomSocket();
                CharTitles = new ConcurrentDictionary<string, List<string>>();
                PartyMembers = new List<uint>();
                SavedLocations = new Dictionary<byte, SavedLocation>();
                CharChest = new Dictionary<int, _CharChest>();
                XsmbLog = new List<_XsmbLog>();
                LOCAL_BUFFER = new TransferBuffer(8192, 0, 0);
                REMOTE_BUFFER = new TransferBuffer(8192, 0, 0);

                LOCAL_SECURITY = new Security();
                LOCAL_SECURITY.GenerateSecurity(true, true, true);
                REMOTE_SECURITY = new Security();
                ASYNC_CONNECT_TO_MODULE();

            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, MODULE_TYPE); UTILS.ExportLog("AGENT_MODULE", EX, PROXY_SOCKET); return; }
        }
        public override async void ASYNC_CONNECT_TO_MODULE()
        {
            try
            {
                if ((await PROXY_SOCKET.ConnectToSocket(ASYNC_SERVER.AG_REDIR_IP, ASYNC_SERVER.AG_REDIR_PORT)).Connected)
                {
                    ASYNC_RECV_FROM_CLIENT();
                    ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
                else { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, MODULE_TYPE); return; }
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, MODULE_TYPE); UTILS.ExportLog("AG_ASYNC_CONNECT_TO_MODULE", EX, PROXY_SOCKET); return; }
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
                            #region ATTACKING?
                            if (INC_LOCAL[i].Opcode != 0x6103 && INC_LOCAL[i].Opcode != 0x5000 && INC_LOCAL[i].Opcode != 0x9000 && INC_LOCAL[i].Opcode != 0x2001)
                            {
                                if (UserName == String.Empty)
                                {
                                    if(Settings.PUNISHMENT_BAN)
                                        UTILS.BLOCK_IP(SOCKET_IP.Split(':')[0]);
                                    ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                    UTILS.WriteLine($"[{SOCKET_IP}] Suspected attacking attemp {INC_LOCAL[i].Opcode}EmptyUserName", UTILS.LOG_TYPE.Warning);
                                    return;
                                }
                                //else if (!await QUERIES.Exist_User(UserName))
                                //{
                                //    if (Settings.PUNISHMENT_BAN)
                                //        UTILS.BLOCK_IP(SOCKET_IP.Split(':')[0]);
                                //    ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, PROXY_SOCKET);
                                //    UTILS.WriteLine($"[{SOCKET_IP}] Suspected attacking attemp {INC_LOCAL[i].Opcode}ErrorUserName{UserName}", UTILS.LOG_TYPE.Warning);
                                //    return;
                                //}
                            }
                            #endregion
                            #region DIAGNOSTICS_LOG
                            if (Settings.MAIN.checkBox4.Checked)
                            {

                                byte[] packet_bytes = INC_LOCAL[i].GetBytes();
                                string line = $"[AGENT][LOCAL][C=>P][{INC_LOCAL[i].Opcode:X4}][{packet_bytes.Length} bytes]{(INC_LOCAL[i].Encrypted ? "[Encrypted]" : "")}{(INC_LOCAL[i].Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(packet_bytes)}";
                                if (!Settings.MAIN.textBox18.Text.Equals(""))
                                {
                                    if (SOCKET_IP.Contains(Settings.MAIN.textBox18.Text))
                                    {
                                        Settings.MAIN.DiagLog.AppendText(line);
                                    }
                                }
                                else
                                {
                                    Settings.MAIN.DiagLog.AppendText(line);
                                }
                            }
                            #endregion

                            #region X_GLOBAL_IDENTIFICATION - PROXY CORE
                            if (INC_LOCAL[i].Opcode == 0x2001)
                            {
                                ASYNC_RECV_FROM_MODULE();
                                continue;
                            }
                            #endregion
                            #region SERVER_GLOBAL_HANDSHAKE CLIENT_GLOBAL_HANDSHAKE_ACCEPT - PROXY CORE
                            if (INC_LOCAL[i].Opcode == 0x5000 || INC_LOCAL[i].Opcode == 0x9000)
                            {
                                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                continue;
                            }
                            #endregion
                            #region PACKET_PER_SEC PROTECTION
                            TOT_PACKET_CNT += INC_LOCAL.Count;
                            PPS = UTILS.GET_PER_SEC_RATE(Convert.ToUInt64(TOT_PACKET_CNT), START_TIME);
                            if (Settings.AG_PPS_VALUE != 0 && PPS >= Settings.AG_PPS_VALUE && DateTime.Now.Subtract(START_TIME).TotalSeconds > 5)
                            {
                                if (Settings.PUNISHMENT_BAN)
                                {
                                    UTILS.BLOCK_IP(SOCKET_IP.Split(':')[0]);
                                    UTILS.WriteLine($"[{INC_LOCAL.Count}]Warnning[{SOCKET_IP}]AgentServer PacketsP/S:[{PPS}/{Settings.AG_PPS_VALUE}] IP.", UTILS.LOG_TYPE.Fatal);
                                }
                                else
                                    UTILS.WriteLine($"[{INC_LOCAL.Count}]Warnning[{SOCKET_IP}]AgentServer PacketsP/S:[{PPS}/{Settings.AG_PPS_VALUE}]", UTILS.LOG_TYPE.Fatal);
                                ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                return;

                            }
                            #endregion
                            #region BYTES_PER_SECOND_PROTECTION
                            TOT_BYTES_CNT += RECEIVED_DATA;
                            BPS = UTILS.GET_PER_SEC_RATE((ulong)TOT_BYTES_CNT, START_TIME);
                            if (Settings.AG_BPS_VALUE != 0 && BPS >= Settings.AG_BPS_VALUE && DateTime.Now.Subtract(START_TIME).TotalSeconds > 5)
                            {
                                if (Settings.PUNISHMENT_BAN)
                                {
                                    UTILS.BLOCK_IP(SOCKET_IP.Split(':')[0]);
                                    UTILS.WriteLine($"[{INC_LOCAL.Count}]Warnning[{SOCKET_IP}]AgentServer BytesP/S:[{BPS}/{Settings.AG_BPS_VALUE}] 尝试封禁IP.", UTILS.LOG_TYPE.Fatal);
                                }
                                else
                                    UTILS.WriteLine($"[{INC_LOCAL.Count}]Warnning[{SOCKET_IP}]AgentServer BytesP/S:[{BPS}/{Settings.AG_BPS_VALUE}]", UTILS.LOG_TYPE.Fatal);
                                ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                return;
                            }
                            #endregion
                            #region MALICIOUS_OPCODE_COMPARISON
                            if (Settings.MALICIOUS_OPCODE)
                            {
                                if (READER.MALICIOUS_OPCODES.Contains(INC_LOCAL[i].Opcode))
                                {
                                    if (Settings.PUNISHMENT_BAN)
                                        UTILS.BLOCK_IP(SOCKET_IP.Split(':')[0]);
                                    ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                    UTILS.WriteLine($"[{SOCKET_IP}]  AgentServer detected malicious opcode:[0x{INC_LOCAL[i].Opcode:X}] and has been banned via firewall.", UTILS.LOG_TYPE.Fatal);
                                    return;
                                }
                            }
                            #endregion
                            #region CLIENT_PACKETS
                            switch (INC_LOCAL[i].Opcode)
                            {
                                #region CLIENT_USER_CONNECTION
                                case 0x6103:
                                    TOKEN_ID = INC_LOCAL[i].ReadUInt32();
                                    UserName = INC_LOCAL[i].ReadAscii();
                                    if (!await QUERIES.Exist_User(UserName))
                                    {
                                        if (Settings.PUNISHMENT_BAN)
                                            UTILS.BLOCK_IP(SOCKET_IP.Split(':')[0]);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                        UTILS.WriteLine($"[{SOCKET_IP}]Banned due to Suspected attacking 0x6103", UTILS.LOG_TYPE.Warning);
                                        return;
                                    }
                                    string AG_MODULE_CONNECTED = PROXY_SOCKET.RemoteEndPoint.ToString();
                                    //assigning this current connection crresponding gw session!
                                    KeyValuePair<Socket, GATEWAY_MODULE> MATCHING_GW_CON = ASYNC_SERVER.DISPOSED_GW_SESSIONS.LastOrDefault(s => s.Value.TOKEN_ID == TOKEN_ID && s.Value.REDIR_IP == AG_MODULE_CONNECTED.Split(':')[0] && s.Value.REDIR_PORT == Convert.ToInt32(AG_MODULE_CONNECTED.Split(':')[1]));
                                    CORRESPONDING_GW_SESSION = MATCHING_GW_CON.Value;
                                    if (CORRESPONDING_GW_SESSION == null)
                                    {
                                        if (!READER.NETCAFE_IPS.Contains(SOCKET_IP.Split(':')[0]) && SOCKET_IP.Split(':')[0] != Settings.BIND_IP && SOCKET_IP.Split(':')[0] != Settings.PUBLIC_AG_IP)
                                        {
                                            UTILS.WriteLine($"[{SOCKET_IP.Split(':')[0]}] GW session is null, assigning corresponding gw session by TokenID failed.", UTILS.LOG_TYPE.Warning);
                                            ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        ASYNC_SERVER.DISPOSED_GW_SESSIONS.TryRemove(MATCHING_GW_CON.Key, out GATEWAY_MODULE REMOVED_GW_SESSION);
                                        //Security measure if someone tries to interact with the ag module before the gw.
                                        if (!CORRESPONDING_GW_SESSION.GATEWAY_PRE_CONNECTION)
                                        {
                                            UTILS.WriteLine($"[{SOCKET_IP}] disconnected detected incomplete gateway session.", UTILS.LOG_TYPE.Warning);
                                            ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                            return;
                                        }
                                        else if (Settings.SERVER_HWID_LIMIT > 0 && !READER.NETCAFE_IPS.Contains(SOCKET_IP.Split(':')[0]) && SOCKET_IP.Split(':')[0] != Settings.BIND_IP && SOCKET_IP.Split(':')[0] != Settings.PUBLIC_AG_IP)
                                        {
                                            //Verifiying that the client reported reported its hwid
                                            if (CORRESPONDING_GW_SESSION.HWID == string.Empty)
                                            {
                                                Packet maxipmsg = new Packet(0xA102);
                                                maxipmsg.WriteUInt8(0x02);
                                                maxipmsg.WriteUInt8(0x09);

                                                LOCAL_SECURITY.Send(maxipmsg);
                                                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                                UTILS.WriteLine($"[{SOCKET_IP.Split(':')[0]}] did not report its HWID while logging in.", UTILS.LOG_TYPE.Warning);
                                                ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                                return;
                                            }
                                            else
                                            {
                                                int PC_CNT = UTILS.AG_HWIDCount(CORRESPONDING_GW_SESSION.HWID);
                                                if (PC_CNT > Settings.SERVER_HWID_LIMIT)
                                                {
                                                    Packet maxipmsg = new Packet(0xA102);
                                                    maxipmsg.WriteUInt8(0x03);
                                                    maxipmsg.WriteUInt8(0x08);
                                                    LOCAL_SECURITY.Send(maxipmsg);

                                                    ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                                    UTILS.WriteLine($"[{SOCKET_IP.Split(':')[0]}] has reached the max pc-limit [{PC_CNT}/{Settings.SERVER_HWID_LIMIT}]", UTILS.LOG_TYPE.Notify);
                                                    ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                                    return;
                                                }
                                            }
                                        }
                                    }

                                    ShardID = CORRESPONDING_GW_SESSION.ShardID;
                                    ComID = CORRESPONDING_GW_SESSION.ComID;
                                    if (ShardID == 0) ShardID = 64;
                                    HASGMPRIV = await QUERIES.Has_GM_Priv(UserName);
                                    break;
                                #endregion
                                #region CLIENT_PING_CHECKER
                                case 0x2002:
                                    if (INC_LOCAL[i].GetBytes().Length != 0)
                                    {
                                        //int Type = INC_LOCAL[i].ReadUInt8();
                                        //if (Type == 1)
                                        //{
                                        //    UTILS.SendNotice(UTILS.NoticeType.Notify,INC_LOCAL[i].ReadAscii(), CLIENT_SOCKET);
                                        //    continue;
                                        //}
                                        //else {
                                        UTILS.WriteLine($"[{SOCKET_IP.Split(':')[0]}] Invalid ping", UTILS.LOG_TYPE.Warning);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                        return;
                                        //}
                                    }
                                    break;
                                #endregion
                                #region CLIENT_PLAYER_BERSERK EXPLOIT_INVISIBLE_INVINCIBLE
                                case 0x70a7://Zerk exploit fix
                                    if (INC_LOCAL[i].ReadUInt8() != 1)
                                    {
                                        UTILS.WriteLine($"[{SOCKET_IP.Split(':')[0]}] detected zerk state exploit.", UTILS.LOG_TYPE.Warning);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                        return;
                                    }
                                    if (Settings.ZERK_DELAY != 0 && Convert.ToInt64((DateTime.Now.Subtract(LAST_ZERK_TIME)).TotalSeconds) < Convert.ToInt64(Settings.ZERK_DELAY))
                                    {
                                        int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_ZERK_TIME)).TotalSeconds) - Convert.ToInt64(Settings.ZERK_DELAY));
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox139.Text, Surplus), CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (Settings.DISABLE_FELLOW_UNDER_ZERK && RIDING_PET && await QUERIES.IS_FellowSummoned_by_CHARNAME16(CHARNAME16, ShardID))
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox138.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (Convert.ToBoolean(Settings.MAIN.textBox120.Text) && INSIDE_FW)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox137.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (UTILS.DISABLE_ZERK_REGIONS.Contains(CUR_REGION))
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox127.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    LAST_ZERK_TIME = DateTime.Now;
                                    break;
                                #endregion
                                #region SR_GAMESERVER CRASH EXPLOIT
                                case 0x3510:
                                    UTILS.WriteLine($"[{SOCKET_IP}] disconnected detected SR_GameServer crash exploit.", UTILS.LOG_TYPE.Warning);
                                    ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                    return;
                                #endregion
                                #region CLIENT_AVATAR_BLUES MAGIC_OPT_EXPLOIT
                                case 0x34A9://Avatar blue_opt exploit fix
                                    string avatar_blue = INC_LOCAL[i].ReadAscii().ToLower();
                                    if (!avatar_blue.Contains("avatar"))
                                    {
                                        UTILS.WriteLine($"[{SOCKET_IP.Split(':')[0]}] detected avatar magic option exploit", UTILS.LOG_TYPE.Warning);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                        return;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_CHAR_SELECT CHARNAME_INJ_FIX
                                case 0x7001:
                                    if (FIRST_CLIENT_CHAR_SELECT)//CL_CharName injection exploit
                                    {
                                        UTILS.WriteLine($"[{SOCKET_IP.Split(':')[0]}] Char selection injection was detected", UTILS.LOG_TYPE.Warning);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                        return;
                                    }
                                    FIRST_CLIENT_CHAR_SELECT = true;//reg as sent one
                                    if (INC_LOCAL[i].GetBytes().Length < 4)
                                        continue;
                                    if (INC_LOCAL[i].GetBytes()[0] == 0)
                                        continue;
                                    if (INC_LOCAL[i].GetBytes()[0] + 2 != INC_LOCAL[i].GetBytes().Length)
                                        continue;
                                    if (INC_LOCAL[i].GetBytes().Length > 2)// must be atleast 2 bytes
                                    {
                                        CHARNAME16_HOLDER = QUERIES.INJECTION_PREFIX(INC_LOCAL[i].ReadAscii());
                                        NickName16 = await QUERIES.Get_NikeName_by_CHARNAME16(CHARNAME16_HOLDER, ShardID);
                                        CharID = await QUERIES.Get_CharID_by_CharName16(CHARNAME16_HOLDER, ShardID);
                                        JID = await QUERIES.Get_JID_by_CharID(CharID, ShardID);
                                        int[] jobdata = await QUERIES.Get_JobType_by_CHARNAME16(CHARNAME16_HOLDER, ShardID);
                                        if (jobdata != null)
                                        {
                                            JobType = jobdata[0];
                                            JobLevel = jobdata[1];
                                        }
                                        if (Settings.MAIN.numericUpDown13.Value > 0)
                                        {
                                            int x = 0;
                                            if (this.JobType == 1 || this.JobType == 3)
                                                x = UTILS.COUNT_JOBMODE_HWID_OCC(CORRESPONDING_GW_SESSION.HWID, 0);
                                            else if (this.JobType == 2)
                                                x = UTILS.COUNT_JOBMODE_HWID_OCC(CORRESPONDING_GW_SESSION.HWID, 1);
                                            if (x == 100 || x >= Settings.MAIN.numericUpDown13.Value)
                                            {
                                                ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                                return;
                                            }

                                        }

                                    }
                                    break;
                                #endregion
                                #region 7007 
                                case 0x7007:
                                    if (INC_LOCAL[i].GetBytes().Length < 1)
                                        continue;
                                    if (INC_LOCAL[i].GetBytes()[0] < 1 || INC_LOCAL[i].GetBytes()[0] > 5)
                                        continue;
                                    if (INC_LOCAL[i].GetBytes()[0] == 3 && INC_LOCAL[i].GetBytes().Length < 5)
                                        continue;
                                    if (INC_LOCAL[i].GetBytes()[0] == 3 && INC_LOCAL[i].GetBytes()[1] + 3 != INC_LOCAL[i].GetBytes().Length)
                                        continue;
                                    if (INC_LOCAL[i].GetBytes()[0] == 2 && INC_LOCAL[i].GetBytes().Length > 1)
                                        continue;
                                    if (INC_LOCAL[i].GetBytes()[0] == 4 && INC_LOCAL[i].GetBytes().Length < 5)
                                        continue;
                                    if (INC_LOCAL[i].GetBytes()[0] == 4 && INC_LOCAL[i].GetBytes()[1] + 3 != INC_LOCAL[i].GetBytes().Length)
                                        continue;
                                    if (INC_LOCAL[i].GetBytes()[0] == 1 && INC_LOCAL[i].GetBytes().Length < 26)
                                        continue;
                                    if (INC_LOCAL[i].GetBytes()[0] == 1 && INC_LOCAL[i].GetBytes()[1] + 24 != INC_LOCAL[i].GetBytes().Length)
                                        continue;
                                    break;
                                #endregion
                                #region CLIENT_LEAVE_REQUEST
                                case 0x7005:
                                    if (INC_LOCAL[i].GetBytes().Length > 1)//DC Players exploit found by IWA
                                    {
                                        UTILS.WriteLine($"[{SOCKET_IP.Split(':')[0]}] detected client spawn exploit", UTILS.LOG_TYPE.Warning);
                                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                    }
                                    uint Action = INC_LOCAL[i].ReadUInt8();
                                    if (Settings.RESTART_DISABLE && Action == 2) // for pc limit or glavie exploit shit
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox126.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (Settings.RESTART_DELAY != 0 && Action == 2 && Convert.ToInt64((DateTime.Now.Subtract(LAST_RROREXIT_DELAY)).TotalSeconds) < Convert.ToInt64(Settings.RESTART_DELAY))//rr
                                    {
                                        int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_RROREXIT_DELAY)).TotalSeconds) - Convert.ToInt64(Settings.RESTART_DELAY));
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox140.Text, Surplus), CLIENT_SOCKET);
                                        continue;
                                    }
                                    else if (Settings.EXIT_DELAY != 0 && Action == 1 && Convert.ToInt64((DateTime.Now.Subtract(LAST_RROREXIT_DELAY)).TotalSeconds) < Convert.ToInt64(Settings.EXIT_DELAY))//exit
                                    {
                                        int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_RROREXIT_DELAY)).TotalSeconds) - Convert.ToInt64(Settings.EXIT_DELAY));
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox141.Text, Surplus), CLIENT_SOCKET);
                                        continue;
                                    }
                                    LAST_RROREXIT_DELAY = DateTime.Now;
                                    break;
                                #endregion
                                #region CLIENT_PET_TERMINATE TRADE_REGION
                                case 0x70c6:
                                    if (Settings.JOB_GOODS_DROPOUT)
                                    {
                                        if (!(UTILS.CHECK_TRADE_REGION_ALLOWED((short)CUR_REGION)) && await QUERIES.IS_TransOrVehicleContainsGoods_by_CHARNAME16(CHARNAME16, ShardID) != 0)
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox144.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                    }
                                    GOODS_SELL_FLAG = false;
                                    break;
                                #endregion
                                #region GM_PRIVILEGE                            
                                case 0x7010:
                                    if (!HASGMPRIV)
                                        continue;
                                    if (Settings.GM_PRIVG_LVL && GM_PRIVG.GM_CMD_VERIFY(CLIENT_SOCKET, PROXY_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer, INC_LOCAL[i]))
                                        continue;
                                    break;
                                #endregion
                                #region CLIENT_STALL_REQUEST
                                case 0x70B1://Opening stall delay
                                    string stallname = INC_LOCAL[i].ReadAscii();
                                    //StallStart = Environment.TickCount;
                                    if (!Settings.BOT_ALLOW_STALL && !NONE_CLIENTLESS_MARKER)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox143.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (Settings.CHAT_FILTER && READER.FILTER_KEYWORDS.Count > 0 && READER.FILTER_KEYWORDS.Contains(stallname))
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox142.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (Convert.ToInt64((DateTime.Now.Subtract(LAST_STALL_TIME)).TotalSeconds) < Convert.ToInt64(Settings.STALL_DELAY))
                                    {
                                        int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_STALL_TIME)).TotalSeconds) - Convert.ToInt64(Settings.STALL_DELAY));

                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox147.Text, Surplus), CLIENT_SOCKET);
                                        continue;
                                    }
                                    LAST_STALL_TIME = DateTime.Now;
                                    break;
                                #endregion
                                #region CLIENT_STALL_STATUS
                                case 0x70BA:
                                    STALLING = true;
                                    break;
                                #endregion
                                #region CLIENT_STALL_CANCEL
                                case 0x70b2:
                                    StallStart = 0;
                                    STALLING = false;
                                    break;
                                #endregion
                                #region CLIENT_CHAR_LOADED
                                case 0x750E:
                                    //byte[] packet_bytes = CharTitleInfoPacket.GetBytes();
                                    //string line = string.Format("新包[{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", CharTitleInfoPacket.Opcode, packet_bytes.Length, CharTitleInfoPacket.Encrypted ? "[Encrypted]" : "", CharTitleInfoPacket.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet_bytes), Environment.NewLine);
                                    //Settings.MAIN.DiagLog.AppendText(line);
                                    break;
                                #endregion
                                #region CLIENT_GUILD_INVITE
                                case 0x70F3: //Guild Join Request Delay
                                    if (INC_LOCAL[i].ReadUInt8() == 7)
                                    {
                                        if (Convert.ToInt64((DateTime.Now.Subtract(LAST_GUILDREQ_TIME)).TotalSeconds) < Settings.GUILD_REQ_DELAY && Settings.GUILD_REQ_DELAY != 0)
                                        {
                                            int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_GUILDREQ_TIME)).TotalSeconds) - Convert.ToInt64(Settings.GUILD_REQ_DELAY));

                                            UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox146.Text, Surplus), CLIENT_SOCKET);
                                            continue;
                                        }
                                        if (await QUERIES.Get_GuildMembersCount_by_CHARNAME16(CHARNAME16, ShardID) >= Settings.GUILD_MAX_LIMIT && Settings.GUILD_MAX_LIMIT != 0)
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox145.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                        LAST_GUILDREQ_TIME = DateTime.Now;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_UNION_INVITE                         
                                case 0x70FB://Union Join Request Delay
                                    if (Convert.ToInt64((DateTime.Now.Subtract(LAST_UNIREQ_TIME)).TotalSeconds) < Settings.UNION_REQ_DELAY && Settings.UNION_REQ_DELAY != 0)
                                    {
                                        int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_UNIREQ_TIME)).TotalSeconds) - Convert.ToInt64(Settings.UNION_REQ_DELAY));
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox151.Text, Surplus), CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (await QUERIES.Get_UnionMembersCount_by_CHARNAME16(CHARNAME16, ShardID) >= Settings.UNION_MAX_LIMIT && Settings.UNION_MAX_LIMIT != 0)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox150.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    LAST_UNIREQ_TIME = DateTime.Now;
                                    break;
                                #endregion
                                #region CLIENT_EXCHANGE_REQUEST                            
                                case 0x7081://Exchange Request Delay
                                    if (!Settings.BOT_ALLOW_EXCHANGE && !NONE_CLIENTLESS_MARKER)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox149.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (Convert.ToInt64((DateTime.Now.Subtract(LAST_EXREQ_TIME)).TotalSeconds) < Convert.ToInt64(Settings.EXCHANGE_REQ_DELAY))
                                    {
                                        int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_EXREQ_TIME)).TotalSeconds) - Convert.ToInt64(Settings.EXCHANGE_REQ_DELAY));
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox148.Text, Surplus), CLIENT_SOCKET);
                                        continue;
                                    }
                                    LAST_EXREQ_TIME = DateTime.Now;
                                    break;
                                    #endregion
                                #region CLIENT_EXCHANGE_ACCEPT/APPROVE/CLOSE
                                    // CLIENT_EXCHANGE_ACCEPT || CLIENT_EXCHANGE_APPROVE ||  CLIENT_EXCHANGE_WINDOWS_CLOSE
                                    break;
                                #endregion
                                #region CLIENT_ACADEMY_CREATE
                                case 0x7470://Academy create req
                                    if (!Settings.ACADEMY_CREATION)
                                    {
                                        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_ACADEMY_CREATION", CLIENT_SOCKET);
                                        continue;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_ACADEMY_INVITE CLIENT_ACADEMY_JOIN CLIENT_ACADEMY_ACCEPT
                                case 0x747E:
                                case 0x347F:
                                    if ((!Settings.ACADEMY_CREATION))//Academy Invite request0x7472
                                    {
                                        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_ACADEMY_INVITE", CLIENT_SOCKET);
                                        continue;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_ARENA_REGISTER
                                case 0x74D3: //Arena Registeration
                                    if (CUR_LEVEL < Convert.ToInt32(Settings.BA_REQ_LVL))
                                    {
                                        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_BA_REQLVL", CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (!Settings.BOT_ALLOW_ARENA && !NONE_CLIENTLESS_MARKER)
                                    {
                                        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_BOT_BA", CLIENT_SOCKET);
                                        continue;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_FLAG_REGISTER
                                case 0x74B2://Capture the flag Registeration
                                    if (CUR_LEVEL < Convert.ToInt32(Settings.CTF_REQ_LVL))
                                    {
                                        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_CTF_REQLVL", CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (!Settings.BOT_ALLOW_CTF && !NONE_CLIENTLESS_MARKER)
                                    {
                                        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_BOT_CTF", CLIENT_SOCKET);
                                        continue;
                                    }
                                    //if (Settings.CTF_PC_LIMIT != 0)
                                    //{
                                    //    CTF_REG = true;
                                    //    if (UTILS.COUNT_CTF_HWID_OCC(CORRESPONDING_GW_SESSION.HWID) > Settings.CTF_PC_LIMIT)
                                    //    {
                                    //        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_HWID_CTF", CLIENT_SOCKET);
                                    //        continue;
                                    //    }
                                    //}
                                    break;
                                #endregion
                                #region CLIENT_MAINACTION SKILL/TRACE/PICK
                                case 0x7074:
                                    if (this.JOB_FLAG && await QUERIES.IS_CosSummoned_by_CHARID(CharID, ShardID))
                                    {
                                        TimeSpan RbLimitStrat = Convert.ToDateTime(Settings.MAIN.RbLimitStartDTP.Value).TimeOfDay;
                                        TimeSpan RbLimitEnd = Convert.ToDateTime(Settings.MAIN.RbLimitEndDTP.Value).TimeOfDay;
                                        TimeSpan NowDate = DateTime.Now.TimeOfDay;
                                        if (!(NowDate > RbLimitStrat && NowDate < RbLimitEnd))
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox152.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                    }
                                    if (INSIDE_DRUNK)
                                        continue;
                                    if (INC_LOCAL[i].ReadUInt8() == 1)// 1 = Attack success,2 = Cancel attack
                                    {
                                        byte action = INC_LOCAL[i].ReadUInt8();
                                        if (action == 3)// TRACE
                                        {
                                            if (Settings.JOB_ANTI_TRACE)
                                            {
                                                if (await QUERIES.Get_Jobstate_by_CHARNAME16(CHARNAME16, ShardID))
                                                {
                                                    UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox159.Text, CLIENT_SOCKET);
                                                    continue;
                                                }
                                            }
                                            if (!Settings.BOT_ALLOW_TRACE && !NONE_CLIENTLESS_MARKER)
                                            {
                                                UTILS.SEND_INDV_ERR_MSG("UIIT_STT_BOT_ACTIONS", CLIENT_SOCKET);
                                                continue;
                                            }
                                            if (Settings.FW_TRACE_PREVENTION && INSIDE_FW)
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox160.Text, CLIENT_SOCKET);
                                                continue;
                                            }
                                            if (UTILS.DISABLE_TRACE_REGIONS.Contains(CUR_REGION))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox159.Text, CLIENT_SOCKET);
                                                continue;
                                            }
                                        }
                                        else if (action == 4 && INC_LOCAL[i].GetBytes().Length >= 4)// SKILL CAST
                                        {
                                            ushort SkillID = INC_LOCAL[i].ReadUInt16();
                                            if (UTILS.DISABLE_SKILLS_REGIONS.ContainsKey(CUR_REGION))
                                                if (UTILS.DISABLE_SKILLS_REGIONS[CUR_REGION].Contains(SkillID))
                                                {
                                                    UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox158.Text, CLIENT_SOCKET);
                                                    continue;
                                                }
                                            if (READER.BLOCKED_SKILL_IDS.Count > 0 && Settings.SKILL_PREVENTION)
                                            {
                                                if (READER.BLOCKED_SKILL_IDS.Contains(SkillID.ToString()))
                                                {
                                                    UTILS.SEND_INDV_ERR_MSG("UIIT_STT_SKILL_PREVENTION", CLIENT_SOCKET);
                                                    continue;
                                                }
                                            }
                                            if (READER.FW_BLOCKED_SKILL_IDS.Count > 0 && Settings.FW_SKILL_PREVENTION && INSIDE_FW)
                                            {
                                                if (READER.FW_BLOCKED_SKILL_IDS.Contains(SkillID.ToString()))
                                                {
                                                    UTILS.SEND_INDV_ERR_MSG("UIIT_STT_FWSKILL_PREVENTION", CLIENT_SOCKET);
                                                    continue;
                                                }
                                            }
                                            if (READER.CTF_BLOCKED_SKILL_IDS.Count > 0 && Settings.CTF_SKILL_PREVENTION && INSIDE_CTF)
                                            {
                                                if (READER.CTF_BLOCKED_SKILL_IDS.Contains(SkillID.ToString()))
                                                {
                                                    UTILS.SEND_INDV_ERR_MSG("UIIT_STT_CTFSKILL_PREVENTION", CLIENT_SOCKET);
                                                    continue;
                                                }
                                            }
                                            if (READER.BA_BLOCKED_SKILL_IDS.Count > 0 && Settings.BA_SKILL_PREVENTION && INSIDE_BA)
                                            {
                                                if (READER.BA_BLOCKED_SKILL_IDS.Contains(SkillID.ToString()))
                                                {
                                                    UTILS.SEND_INDV_ERR_MSG("UIIT_STT_BASKILL_PREVENTION", CLIENT_SOCKET);
                                                    continue;
                                                }
                                            }
                                            if (READER.JOB_BLOCKED_SKILL_IDS.Count > 0 && Settings.JOB_SKILL_PREVENTION && JOB_FLAG)
                                            {
                                                if (READER.JOB_BLOCKED_SKILL_IDS.Contains(SkillID.ToString()))
                                                {
                                                    UTILS.SEND_INDV_ERR_MSG("UIIT_STT_JOBSKILL_PREVENTION", CLIENT_SOCKET);
                                                    continue;
                                                }
                                            }

                                        }
                                    }
                                    break;
                                #endregion
                                #region CLIENT_TELEPORTSTART
                                case 0x705A:// Get teleport data packet
                                    if (INC_LOCAL[i].GetBytes().Length >= 9)
                                    {
                                        INC_LOCAL[i].ReadUInt32(); // Unknown Bytes
                                        if (INC_LOCAL[i].ReadUInt8() == 2)
                                        {
                                            TARGET_TELEPORT = INC_LOCAL[i].ReadUInt32();// Read target teleport    
                                            /*
                                            if (Settings.BLOCK_EVENTROOM_TELEPORT && (TARGET_TELEPORT == 224 || TARGET_TELEPORT == 225)) // events map
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify,"There is no available event at this moment, please try again later.", CLIENT_SOCKET);
                                                continue;
                                            }
                                            if (Settings.BLOCK_TEMPLE_TELEPORT && (TARGET_TELEPORT == 173 || TARGET_TELEPORT == 174)) // job temple
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify,"The job temple is currently disabled, please try again later.", CLIENT_SOCKET);
                                                continue;
                                            }
                                            */
                                            //tp gates logic...
                                            if (READER.FW_TELEPORT_ID.Contains((short)TARGET_TELEPORT))// FW Reg
                                            {
                                                if (Settings.MAIN.numericUpDown10.Value > 0 && UTILS.COUNT_FW_HWID_OCC(CORRESPONDING_GW_SESSION.HWID) >= Settings.MAIN.numericUpDown10.Value && !INSIDE_FW)
                                                {
                                                    UTILS.SEND_INDV_ERR_MSG("UIIT_STT_FW_HWID", CLIENT_SOCKET);
                                                    continue;
                                                }
                                                if (!Settings.BOT_ALLOW_FORTRESS && !NONE_CLIENTLESS_MARKER)
                                                {
                                                    UTILS.SEND_INDV_ERR_MSG("UIIT_STT_BOT_FW", CLIENT_SOCKET);
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                #endregion
                                #region JOB_RES_ACCEPT_DELAY
                                case 0x3080: //Job ressurection acceptor delay
                                    INSIDE_PT = true;
                                    byte a = INC_LOCAL[i].ReadUInt8();
                                    byte yesorno = INC_LOCAL[i].ReadUInt8();
                                    if (yesorno == 1)
                                    {
                                        if (Convert.ToInt64((DateTime.Now.Subtract(LAST_JOB_REQTIME)).TotalSeconds) < Convert.ToInt64(Settings.JOB_RESS_ACCEPTION_DELAY))
                                        {
                                            if (await QUERIES.Get_Jobstate_by_CHARNAME16(CHARNAME16, ShardID))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox154.Text, CLIENT_SOCKET);
                                            }
                                            continue;
                                        }
                                        LAST_JOB_REQTIME = DateTime.Now;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_PLAYER_HANDLE
                                case 0x704C:
                                    if (INC_LOCAL[i].GetBytes().Length >= 3)//item usage packet
                                    {
                                        byte inv_slot = INC_LOCAL[i].ReadUInt8();
                                        byte type4 = INC_LOCAL[i].ReadUInt8();
                                        byte typeid = INC_LOCAL[i].ReadUInt8();
                                        int RefItemID = await QUERIES.Get_ItemID_by_Slot(inv_slot, CharID, ShardID);
                                        if (UTILS.DISABLE_ITEMS_REGIONS.ContainsKey(CUR_REGION))
                                            if (UTILS.DISABLE_ITEMS_REGIONS[CUR_REGION].Contains(RefItemID))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox153.Text, CLIENT_SOCKET);
                                                continue;
                                            }
                                        if (INC_LOCAL[i].GetBytes().Length >= 6 && (type4 == 0xEC || type4 == 0xED) && (typeid == 0x29))
                                        {
                                            string message = INC_LOCAL[i].ReadAscii();
                                            if (READER.FILTER_KEYWORDS.Count > 0 && Settings.CHAT_FILTER && READER.FILTER_KEYWORDS.Any(message.ToLower().Contains))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox165.Text, CLIENT_SOCKET);
                                                continue;
                                            }
                                            if (CUR_LEVEL < Convert.ToInt32(Settings.GLOBAL_REQ_LVL) && (!CHARNAME16.Contains("[")))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox164.Text, Settings.GLOBAL_REQ_LVL), CLIENT_SOCKET);
                                                continue;
                                            }
                                            if (Convert.ToInt64((DateTime.Now.Subtract(LAST_GLOBAL_TIME)).TotalSeconds) < Convert.ToInt64(Settings.GLOBAL_CHAT_DELAY) && (!CHARNAME16.Contains("[")))
                                            {
                                                int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_GLOBAL_TIME)).TotalSeconds) - Convert.ToInt64(Settings.GLOBAL_CHAT_DELAY));
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox163.Text, Surplus), CLIENT_SOCKET);
                                                continue;
                                            }
                                            message = message.Replace(".1`", string.Empty);
                                            message = message.Replace(".2`", string.Empty);
                                            message = message.Replace(".3`", string.Empty);
                                            message = message.Replace(".4`", string.Empty);
                                            message = message.Replace(".5`", string.Empty);
                                            message = message.Replace(".6`", string.Empty);
                                            message = message.Replace(".7`", string.Empty);
                                            message = message.Replace(".8`", string.Empty);
                                            if (RefItemID == Convert.ToInt32(Settings.MAIN.textBox31.Text))
                                                message = ".1`" + message;
                                            else if (RefItemID == Convert.ToInt32(Settings.MAIN.textBox32.Text))
                                                message = ".2`" + message;
                                            else if (RefItemID == Convert.ToInt32(Settings.MAIN.textBox35.Text))
                                                message = ".3`" + message;
                                            else if (RefItemID == Convert.ToInt32(Settings.MAIN.textBox37.Text))
                                                message = ".4`" + message;
                                            else if (RefItemID == Convert.ToInt32(Settings.MAIN.textBox38.Text))
                                                message = ".5`" + message;
                                            else if (RefItemID == Convert.ToInt32(Settings.MAIN.textBox59.Text))
                                                message = ".6`" + message;
                                            else if (RefItemID == Convert.ToInt32(Settings.MAIN.textBox60.Text))
                                                message = ".7`" + message;
                                            else if (RefItemID == Convert.ToInt32(Settings.MAIN.textBox61.Text))
                                                message = ".8`" + message;

                                            SendGlobal(inv_slot, message, type4, typeid);
                                            LAST_GLOBAL_TIME = DateTime.Now;
                                            if (Settings.LOG_GLOBAL_CHAT)
                                                await QUERIES.LOG_GLOBAL_CHAT(CHARNAME16, message);
                                            continue;
                                        }
                                        if (INC_LOCAL[i].GetBytes().Length == 4 && typeid == 0x19)//contains atleast 4 bytes
                                        {
                                            byte identifier = INC_LOCAL[i].ReadUInt8();
                                            if (identifier == 0x3)
                                            {
                                                int DeadRegion = await QUERIES.Get_Cur_DiedRegion(CharID, ShardID);
                                                if (UTILS.Region_Restrection.ContainsKey(DeadRegion))
                                                    continue;
                                                if (Settings.JOB_REVERSE_DEATH_POINT && await QUERIES.Get_Jobstate_by_CHARNAME16(CHARNAME16, ShardID))
                                                {
                                                    UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox161.Text, CLIENT_SOCKET);
                                                    continue;
                                                }
                                            }
                                            if (identifier == 0x2)
                                            {
                                                if (Settings.JOB_REVERSE_LAST_RECALL_POINT && await QUERIES.Get_Jobstate_by_CHARNAME16(CHARNAME16, ShardID))
                                                {
                                                    UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox180.Text, CLIENT_SOCKET);
                                                    continue;
                                                }
                                                int TelRegion = await QUERIES.Get_Cur_RecallRegion(CharID, ShardID);
                                                if (UTILS.Region_Restrection.ContainsKey(TelRegion))
                                                    continue;
                                            }
                                        }
                                        if (INC_LOCAL[i].GetBytes().Length == 8 && typeid == 0x19)//FIRST_CLIENT_CHAR_SELECT
                                        {
                                            byte identifier = INC_LOCAL[i].ReadUInt8();
                                            if (identifier == 0x7 && Settings.JOB_REVERSE_MAP_POINT)
                                            {
                                                if (await QUERIES.Get_Jobstate_by_CHARNAME16(CHARNAME16, ShardID))
                                                {
                                                    UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox179.Text, CLIENT_SOCKET);
                                                    continue;
                                                }
                                            }
                                        }
                                        if (INC_LOCAL[i].GetBytes().Length == 3 && Settings.DISABLE_FELLOW_UNDER_JOB && type4 == 0xCD && typeid == 0x08)//removed type == 0xEC || for the 3 above
                                        {
                                            if (await QUERIES.Get_Jobstate_by_CHARNAME16(CHARNAME16, ShardID))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox178.Text, CLIENT_SOCKET);
                                                continue;
                                            }
                                        }
                                        if (Settings.DISABLE_OUR_SCROLLS_IN_TOWNS && INC_LOCAL[i].GetBytes().Length == 3 && type4 == 0xED && typeid == 0x09)
                                        {
                                            if (!await QUERIES.IS_BattleField_by_CHARNAME16(CHARNAME16, ShardID))
                                            {
                                                string ItemCodeName = await QUERIES.Get_ItemCodeName128_by_Slot(inv_slot, CHARNAME16, ShardID);
                                                if (ItemCodeName == "ITEM_EVENT_AUTOEQUIP_COUPON")
                                                {
                                                    UTILS.SendNotice(UTILS.NoticeType.Notify, "You cannot use the auto equipment scroll outside of towns.", CLIENT_SOCKET);
                                                    continue;
                                                }
                                                else if (ItemCodeName == "ITEM_EVENT_TITLE_09")
                                                {
                                                    UTILS.SendNotice(UTILS.NoticeType.Notify, "You cannot use the random trophies scroll outside of towns.", CLIENT_SOCKET);
                                                    continue;
                                                }
                                            }
                                        }
                                        else if (typeid == 0x36 && Settings.FW_RES_SCROLL && INSIDE_FW)
                                        {
                                            UTILS.SEND_INDV_ERR_MSG("UIIT_STT_FW_RESSSCROLL", CLIENT_SOCKET);
                                            continue;
                                        }
                                        else if (typeid == 0x36 && Settings.JOB_RESS_SCROLL)
                                        {
                                            if (await QUERIES.Get_Jobstate_by_CHARNAME16(CHARNAME16, ShardID))
                                            {
                                                UTILS.SEND_INDV_ERR_MSG("UIIT_STT_JOB_RESSSCROLL", CLIENT_SOCKET);
                                                continue;
                                            }
                                        }

                                    }
                                    break;
                                #endregion                            
                                #region AGENT_COS_UPDATE_RIDESTATE
                                case 0x70CB:
                                    if (INC_LOCAL[i].GetBytes().Length == 5)
                                    {
                                        byte status = INC_LOCAL[i].ReadUInt8();
                                        if (status == 0x01)//attempting to ride a pet
                                            LAST_ATTEMPT_TO_RIDE = DateTime.Now;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_CHAT_INFORMATION
                                case 0x7025:
                                    if (INC_LOCAL[i].GetBytes().Length >= 2)
                                    {
                                        int type3 = INC_LOCAL[i].ReadUInt8();//chat type
                                        byte msgCount = INC_LOCAL[i].ReadUInt8();//msg cnt
                                        string message = INC_LOCAL[i].ReadAscii();// msg content
                                        string PMmessage = String.Empty;// msg content
                                        if (type3 == 0x2)
                                        {
                                            PMmessage = INC_LOCAL[i].ReadAscii();
                                        }
                                        if (type3 == 7 && Settings.GM_PRIVG_LVL)// Pink Notices!!!
                                        {
                                            if (GM_PRIVG.GM_CMD_VERIFY(CLIENT_SOCKET, PROXY_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer, INC_LOCAL[i]))
                                            {
                                                continue;
                                            }
                                        }
                                        if (UTILS.DISABLE_CHAT_REGIONS.Contains(CUR_REGION))
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox177.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                        if ((Settings.CHAT_FILTER && READER.FILTER_KEYWORDS.Count > 0 && READER.FILTER_KEYWORDS.Any(message.Contains)))//Checks if content contains any forbidden keywords
                                        {
                                            if (type3 == 1)// ALL chat
                                            {
                                                UTILS.SEND_INDV_ERR_MSG("UIIT_STT_CHAT_FILTER", CLIENT_SOCKET);
                                                continue;
                                            }
                                            if (type3 == 2) // Private chat
                                            {
                                                UTILS.SEND_INDV_ERR_MSG("UIIT_STT_CHAT_FILTER", CLIENT_SOCKET);
                                                continue;
                                            }
                                            if (type3 == 4)// Party Chat
                                            {
                                                UTILS.SEND_INDV_ERR_MSG("UIIT_STT_CHAT_FILTER", CLIENT_SOCKET);
                                                continue;
                                            }
                                        }
                                        if (Settings.LOG_PLAYERS_ALL_CHAT && type3 == 0x01)
                                        {
                                            await QUERIES.LOG_COMMON_CHAT("_LogAllChat", CHARNAME16, message);
                                        }
                                        if (Settings.LOG_PLAYERS_PM_CHAT && type3 == 0x02)
                                        {

                                            await QUERIES.LOG_PM_CHAT(CHARNAME16, message, PMmessage);
                                        }
                                        if (Settings.LOG_PLAYERS_GM_CHAT && type3 == 0x03)
                                        {
                                            await QUERIES.LOG_COMMON_CHAT("_LogAllChat", CHARNAME16, message);
                                        }
                                        if (Settings.LOG_PLAYERS_PT_CHAT && type3 == 0x04)
                                        {
                                            await QUERIES.LOG_COMMON_CHAT("_LogPTChat", CHARNAME16, message);
                                        }
                                        if (Settings.LOG_PLAYERS_GUILD_CHAT && type3 == 0x05)
                                        {
                                            await QUERIES.LOG_COMMON_CHAT("_LogGUILDChat", CHARNAME16, message);
                                        }
                                        if (Settings.LOG_PLAYERS_UNI_CHAT && (type3 == 0x11 || type3 == 0xb))
                                        {
                                            await QUERIES.LOG_COMMON_CHAT("_LogUNIONChat", CHARNAME16, message);
                                        }
                                    }
                                    break;
                                #endregion
                                #region CLIENT_PM_SEND CHAT_FILTER_CONTINUED
                                case 0x7309:
                                    if (READER.FILTER_KEYWORDS.Count > 0 && Settings.CHAT_FILTER)
                                    {
                                        string ToUser = INC_LOCAL[i].ReadAscii();//target 
                                        string Message = INC_LOCAL[i].ReadAscii().ToString().ToLower();//msg content
                                        if (READER.FILTER_KEYWORDS.Any(Message.Contains))//Checks if content contains any forbidden keywords
                                        {
                                            UTILS.SEND_INDV_ERR_MSG("UIIT_STT_CHAT_FILTER", CLIENT_SOCKET);
                                            continue;
                                        }
                                    }
                                    break;
                                #endregion
                                #region CLIENT_PARTY_CREATE
                                case 0x7069:
                                    if (UTILS.DISABLE_PARTY_REGIONS.Contains(CUR_REGION))
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox176.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    INSIDE_PT = true; // Register pt
                                    uint PartyNumber = INC_LOCAL[i].ReadUInt32(); // Party number
                                    uint Type = INC_LOCAL[i].ReadUInt32(); // Type
                                    uint Race = INC_LOCAL[i].ReadUInt8(); // Race
                                    uint Purpose = INC_LOCAL[i].ReadUInt8(); // Purpose of party
                                    uint EntryLevel = INC_LOCAL[i].ReadUInt8(); // Enter level
                                    uint MaxLevel = INC_LOCAL[i].ReadUInt8(); // Max level
                                    string Title = INC_LOCAL[i].ReadAscii().ToString().ToLower(); // Title
                                    //TeamStart = TeamStart == 0 ? Environment.TickCount : TeamStart;
                                    if (Settings.CHAT_FILTER && READER.FILTER_KEYWORDS.Count > 0 && READER.FILTER_KEYWORDS.Contains(Title))
                                    {
                                        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_CHAT_FILTER", CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (!Settings.BOT_ALLOW_CREATE_PARTY && !NONE_CLIENTLESS_MARKER)
                                    {
                                        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_BOT_PT", CLIENT_SOCKET);
                                        continue;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_ACCEPT_PARTY_MATCHING
                                case 0x306E:
                                    if (UTILS.DISABLE_PARTY_REGIONS.Contains(CUR_REGION))
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox174.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_JOIN_FORMED_PARTY
                                case 0x706d:
                                    INSIDE_PT = true;//reg
                                    break;
                                #endregion
                                #region CLIENT_PARTY_INVITE_REQUEST
                                case 0x7062:
                                    if (!Settings.MAIN.checkBox42.Checked)
                                        break;
                                    uint Target = INC_LOCAL[i].ReadUInt32();
                                    var x3 = ASYNC_SERVER.AG_CONS.Where(x => x.Value.UNIQUE_ID == Target).FirstOrDefault();
                                    if (x3.Value != null)
                                    {
                                        if ((x3.Value.ISEURO && ISEURO) || (x3.Value.ISCHIN && ISCHIN))
                                            break;
                                        else
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox175.Text, CLIENT_SOCKET);
                                            continue;
                                        }

                                    }
                                    break;
                                #endregion
                                #region CLIENT_REQUEST_PARTY
                                case 0x7060:
                                    uint Target2 = INC_LOCAL[i].ReadUInt32();
                                    if (!Settings.BOT_ALLOW_INVITE_PARTY && !NONE_CLIENTLESS_MARKER)
                                    {
                                        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_BOT_PT", CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (UTILS.DISABLE_PARTY_REGIONS.Contains(CUR_REGION))
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox176.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (!Settings.MAIN.checkBox42.Checked)
                                        break;
                                    var x4 = ASYNC_SERVER.AG_CONS.Where(x => x.Value.UNIQUE_ID == Target2).FirstOrDefault();
                                    if (x4.Value != null)
                                    {
                                        if ((x4.Value.ISEURO && ISEURO) || (x4.Value.ISCHIN && ISCHIN))
                                            break;
                                        else
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox175.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                    }
                                    break;
                                #endregion
                                #region CLIENT_ITEM_MOVE                              
                                case 0x7034:
                                    if (INC_LOCAL[i].GetBytes().Length >= 1)
                                    {
                                        int type = INC_LOCAL[i].ReadUInt8();

                                        if (INC_LOCAL[i].GetBytes().Length == 0x0D && (type != 7 || type != 10) && Settings.MAIN.checkBox26.Checked && !DISSCONNECT_REQUEST)
                                        {
                                            var keysWithMatchingValues = ASYNC_SERVER.AG_CONS.Where(p => p.Value.CORRESPONDING_GW_SESSION.HWID == CORRESPONDING_GW_SESSION.HWID && p.Value.GOODS_SELL_FLAG).Select(p => p.Key);
                                            int Count = keysWithMatchingValues.Count();
                                            if (!GOODS_SELL_FLAG && Count >= Convert.ToInt32(Settings.MAIN.numericUpDown16.Value))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox173.Text, CLIENT_SOCKET);
                                                continue;
                                            }
                                            TimeSpan RbLimitStrat = Convert.ToDateTime(Settings.MAIN.RbLimitStartDTP.Value).TimeOfDay;
                                            TimeSpan RbLimitEnd = Convert.ToDateTime(Settings.MAIN.RbLimitEndDTP.Value).TimeOfDay;
                                            TimeSpan NowDate = DateTime.Now.TimeOfDay;
                                            if (!(NowDate > RbLimitStrat && NowDate < RbLimitEnd))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox172.Text, RbLimitStrat.ToString(), RbLimitEnd.ToString()), CLIENT_SOCKET);
                                                continue;
                                            }
                                            else
                                                GOODS_SELL_FLAG = true;
                                        }
                                        if (type == 0x14 && Settings.POINTS_SYSTEM)
                                        {
                                            int a2 = INC_LOCAL[i].ReadUInt8();
                                            int b = INC_LOCAL[i].ReadUInt8();
                                            int c = INC_LOCAL[i].ReadUInt16();
                                            if (c == 0x00)
                                            {
                                                INC_LOCAL[i].ReadUInt8();
                                                int stack = INC_LOCAL[i].ReadUInt8();
                                                int d = INC_LOCAL[i].ReadUInt8();
                                                int e = INC_LOCAL[i].ReadUInt8();
                                                int f = INC_LOCAL[i].ReadUInt8();
                                                int g = INC_LOCAL[i].ReadUInt16();
                                                if (d == 0x00 && e == 0x3E && f == 0x05 && g == 0x00)
                                                {
                                                    if (await QUERIES.Is_StackRecord_Exists(CHARNAME16, ShardID))
                                                    {
                                                        // update
                                                        await QUERIES.Update_Stack_Record(CHARNAME16, stack);
                                                    }
                                                    else
                                                    {
                                                        // insert
                                                        await QUERIES.Insert_Stack_Record(CHARNAME16, stack, ShardID);
                                                    }
                                                }
                                            }
                                        }
                                        if (INC_LOCAL[i].GetBytes().Length == 5)
                                        {
                                            int id = INC_LOCAL[i].ReadUInt8();
                                            int ident = INC_LOCAL[i].ReadUInt8();//0x08 byte, attempting to use jobflag, slot ?

                                            if (ident == 0x08)
                                            {
                                                if (Settings.DISABLE_JOB_MODE)
                                                {
                                                    UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox168.Text, CLIENT_SOCKET);
                                                    continue;
                                                }
                                                if (Settings.MAIN.numericUpDown13.Value > 0)
                                                {

                                                    int x = 0;
                                                    if (this.JobType == 1 || this.JobType == 3)
                                                        x = UTILS.COUNT_JOBMODE_HWID_OCC(CORRESPONDING_GW_SESSION.HWID, 0);
                                                    else if (this.JobType == 2)
                                                        x = UTILS.COUNT_JOBMODE_HWID_OCC(CORRESPONDING_GW_SESSION.HWID, 1);
                                                    JOB_YELLOW_LINE = true;
                                                    if (x == 100 || x >= Settings.MAIN.numericUpDown13.Value)
                                                    {
                                                        JOB_YELLOW_LINE = false;
                                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox167.Text, CLIENT_SOCKET);
                                                        continue;
                                                    }

                                                }
                                            }
                                            if (Settings.DISABLE_FELLOW_UNDER_JOB && ident == 0x08 && await QUERIES.IS_FellowSummoned_by_CHARNAME16(CHARNAME16, ShardID))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox166.Text, CLIENT_SOCKET);
                                                continue;
                                            }
                                            if (Settings.ONE_CHAR_ALLOWED_JOBFLAG_INSIDE_ACC && ident == 0x08 && await QUERIES.IS_OneOrMoreCharsInAcc_WearJob_by_StrUserID(await QUERIES.Get_StrUserID_by_CHARNAME16(CHARNAME16, ShardID), ShardID))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox192.Text, CLIENT_SOCKET);
                                                continue;
                                            }
                                        }
                                        if (Settings.DISABLE_ITEM_OR_GOLD_DROP_INTOWN && (type == 7 || type == 10))
                                        {
                                            if (await QUERIES.IS_BattleField_by_CHARNAME16(CHARNAME16, ShardID))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox191.Text, CLIENT_SOCKET);
                                                continue;
                                            }
                                        }
                                        if (CharLockStatue)
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox213.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                    }
                                    break;
                                #endregion
                                #region THIEF_REWARD_MENU_ACCES
                                case 0x70E5:
                                    if (Settings.DISABLE_THIEF_REWARD_MENU_ACCESS)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox190.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_FORTRESS_NPC SQL_INJ_EXPLOIT / TAXES
                                case 0x705E:
                                    if (INC_LOCAL[i].GetBytes().Length >= 11)
                                    {
                                        byte a2 = INC_LOCAL[i].ReadUInt8();
                                        byte b = INC_LOCAL[i].ReadUInt8();
                                        byte c = INC_LOCAL[i].ReadUInt8();
                                        byte d = INC_LOCAL[i].ReadUInt8();
                                        byte e = INC_LOCAL[i].ReadUInt8();
                                        byte f = INC_LOCAL[i].ReadUInt8();
                                        INC_LOCAL[i].ReadUInt8();
                                        INC_LOCAL[i].ReadUInt8();
                                        INC_LOCAL[i].ReadUInt8();
                                        string message = INC_LOCAL[i].ReadAscii();//tbr can be imroved
                                                                                  //UTILS.WriteLine("",message.ToString());

                                        if (message.Contains("'") || message.Contains("-") || message.Contains(";"))
                                        {
                                            ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                            UTILS.WriteLine($"[{SOCKET_IP}] SQL INJECTION ATTEMPT.", UTILS.LOG_TYPE.Warning);
                                            return;
                                        }
                                        if (Settings.DISABLE_TAX_RATE_CHANGE && INC_LOCAL[i].GetBytes().Length == 11 && e == 0x01 && f == 0x01)
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox189.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                    }
                                    break;
                                #endregion
                                #region CLIENT_MOVEMENT
                                case 0x7021:
                                    if (this.JOB_FLAG && await QUERIES.IS_CosSummoned_by_CHARID(CharID, ShardID))
                                    {
                                        TimeSpan RbLimitStrat = Convert.ToDateTime(Settings.MAIN.RbLimitStartDTP.Value).TimeOfDay;
                                        TimeSpan RbLimitEnd = Convert.ToDateTime(Settings.MAIN.RbLimitEndDTP.Value).TimeOfDay;
                                        TimeSpan NowDate = DateTime.Now.TimeOfDay;
                                        if (!(NowDate > RbLimitStrat && NowDate < RbLimitEnd))
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox188.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                    }
                                    byte click = INC_LOCAL[i].ReadUInt8();
                                    if (click == 1)//Ground-Click.
                                    {
                                        Xsec = INC_LOCAL[i].ReadUInt8();
                                        Ysec = INC_LOCAL[i].ReadUInt8();
                                        x = INC_LOCAL[i].ReadInt16();
                                        z = INC_LOCAL[i].ReadInt16();
                                        y = INC_LOCAL[i].ReadInt16();
                                    }
                                    if (INSIDE_FW && Settings.DISABLE_SUMMON_FW_PETS && await QUERIES.IS_FellowSummoned_by_CHARNAME16(CHARNAME16, ShardID))
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox188.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    JOB_YELLOW_LINE = false;
                                    break;
                                #endregion
                                #region PET_MOVEMENT
                                case 0x70C5:
                                    if (this.JOB_FLAG && this.RIDING_PET)
                                    {
                                        TimeSpan RbLimitStrat = Convert.ToDateTime(Settings.MAIN.RbLimitStartDTP.Value).TimeOfDay;
                                        TimeSpan RbLimitEnd = Convert.ToDateTime(Settings.MAIN.RbLimitEndDTP.Value).TimeOfDay;
                                        TimeSpan NowDate = DateTime.Now.TimeOfDay;
                                        if (!(NowDate > RbLimitStrat && NowDate < RbLimitEnd))
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox188.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                    }
                                    break;
                                #endregion
                                #region CLIENT_ALCHEMY
                                case 0x7150:
                                    if (!Settings.BOT_ALLOW_ALCHEMY_ELIXIR && !NONE_CLIENTLESS_MARKER)
                                    {
                                        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_BOT_ALCHEMY", CLIENT_SOCKET);
                                        continue;
                                    }

                                    if (INC_LOCAL[i].GetBytes().Length >= 4)
                                    {
                                        byte a3 = INC_LOCAL[i].ReadUInt8();
                                        byte b = INC_LOCAL[i].ReadUInt8();
                                        INC_LOCAL[i].ReadUInt8();
                                        byte slot = INC_LOCAL[i].ReadUInt8();
                                        if (a3 == 2 && b == 3)
                                        {
                                            int[] MAXPLUSINFO = new int[5];
                                            MAXPLUSINFO = Settings.MAXPLUSINFO;
                                            int PLUS = 0;
                                            string ITEMNAME = "";
                                            int[] ITEMTYPE;
                                            ITEMTYPE = new int[2];
                                            ITEMTYPE = await QUERIES.ReturnType3AndOptlevel(CharID, slot, ShardID);

                                            if (ITEMTYPE == null)
                                            {
                                                ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                                return;
                                            }

                                            if (ITEMTYPE[0] == 6)
                                            {
                                                PLUS = MAXPLUSINFO[0];
                                                ITEMNAME = "Weapons";
                                            }
                                            else if (ITEMTYPE[0] == 9 || ITEMTYPE[0] == 10 || ITEMTYPE[0] == 11 || ITEMTYPE[0] == 1 || ITEMTYPE[0] == 2 || ITEMTYPE[0] == 3)
                                            {
                                                PLUS = MAXPLUSINFO[1];
                                                ITEMNAME = "Sets";
                                            }
                                            else if (ITEMTYPE[0] == 5 || ITEMTYPE[0] == 12)
                                            {
                                                PLUS = MAXPLUSINFO[2];
                                                ITEMNAME = "Accessories";
                                            }
                                            else if (ITEMTYPE[0] == 4)
                                            {
                                                PLUS = MAXPLUSINFO[3];
                                                ITEMNAME = "Shields";
                                            }
                                            else if (ITEMTYPE[0] == 14)
                                            {
                                                PLUS = MAXPLUSINFO[4];
                                                ITEMNAME = "Devils";
                                            }
                                            else
                                            {
                                                PLUS = Convert.ToInt32(Settings.MAIN.textBox55.Text);
                                                ITEMNAME = "Others";

                                            }
                                            if (ITEMTYPE[1] >= PLUS)
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox187.Text, ITEMNAME), CLIENT_SOCKET);
                                                continue;
                                            }
                                        }
                                        if (a3 == 2 && b == 8 && INC_LOCAL[i].GetBytes().Length == 5 && Settings.DISABLE_ADVANCED_ELIXIR_ON_DEGREE != 0)//using adv elixir
                                        {
                                            string TargetItemCodeName128 = await QUERIES.Get_ItemCodeName128_by_Slot(slot, CHARNAME16, ShardID);
                                            int splitter = UTILS.Is_ADV_USEABLE(TargetItemCodeName128);//generally 3 or 3 is a good result!
                                            if (splitter != 0)
                                            {
                                                if (Convert.ToInt32(TargetItemCodeName128.Split('_')[splitter]) == Settings.DISABLE_ADVANCED_ELIXIR_ON_DEGREE)
                                                {
                                                    UTILS.SEND_INDV_ERR_MSG("UIIT_STT_ADV_DG_DISABLE", CLIENT_SOCKET);
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                #endregion
                                #region CLIENT_ALCHEMY_STONE
                                case 0x7151:
                                    if (!Settings.BOT_ALLOW_ALCHEMY_STONE && !NONE_CLIENTLESS_MARKER)
                                    {
                                        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_BOT_ALCHEMY", CLIENT_SOCKET);
                                        continue;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_REQUEST_GUI_REQUEST
                                case 0x9946:
                                    //todo make it 1 byte instead of ascii
                                    string INFO = INC_LOCAL[i].ReadAscii();
                                    switch (INFO)
                                    {
                                        #region TITLE_STORAGE_REQUEST_LIST
                                        case "!titleopened":
                                            if (Convert.ToInt64((DateTime.Now.Subtract(LAST_UPDATETITLE_DELAY)).TotalSeconds) < Convert.ToInt64(UTILS.GUI_DELAY))
                                                continue;
                                            LAST_UPDATETITLE_DELAY = DateTime.Now;
                                            if (Settings.MAIN.JobTitle.Checked)
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox186.Text, CLIENT_SOCKET);
                                                continue;
                                            }
                                            await QUERIES.LoadHwanList(this);
                                            _CustomTitle xx = new _CustomTitle();
                                            xx = UTILS.CustomTitleList.Find(x => x.CharName == CHARNAME16 && x.ShardID == ShardID);
                                            if (!CharTitles.ContainsKey(CHARNAME16) && xx == null)
                                                continue;
                                            Packet Title2 = new Packet(0x5104);
                                            //send normal titles
                                            if (CharTitles.ContainsKey(CHARNAME16))
                                            {
                                                Title2.WriteInt32(CharTitles[CHARNAME16].Count);
                                                foreach (string key in CharTitles.Keys)
                                                {
                                                    if (key == CHARNAME16)
                                                    {
                                                        foreach (string val in CharTitles[key])
                                                        {
                                                            Title2.WriteAscii(val);
                                                            int titleid = UTILS.GetDigits(val);
                                                            Title2.WriteInt32(titleid);
                                                            _CustomTitleColor xxx = new _CustomTitleColor();
                                                            xxx = UTILS.titlescolors.Find(clr => clr.CharName == CHARNAME16 && clr.ShardID == ShardID);
                                                            if (xxx != null)
                                                            {
                                                                Title2.WriteUInt32(xxx.Color);
                                                            }
                                                            else
                                                                Title2.WriteUInt32(0xffff00);
                                                        }
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                                Title2.WriteInt32(0);

                                            //send custom titles
                                            _CustomTitle asd = new _CustomTitle();
                                            asd = UTILS.CustomTitleList.Find(x => x.CharName == CHARNAME16 && x.ShardID == ShardID);
                                            if (asd != null)
                                            {
                                                Title2.WriteInt32(asd.Titles.Count);
                                                foreach (var x in asd.Titles)
                                                {
                                                    Title2.WriteAscii(x.Key);
                                                    _CustomTitleColor xxx = new _CustomTitleColor();
                                                    xxx = UTILS.titlescolors.Find(clr => clr.CharName == CHARNAME16 && clr.ShardID == ShardID);

                                                    if (xxx != null)
                                                        Title2.WriteUInt32(xxx.Color);
                                                    else
                                                        Title2.WriteUInt32(0xffff00);
                                                }
                                            }
                                            else
                                                Title2.WriteInt32(0);
                                            LOCAL_SECURITY.Send(Title2);
                                            ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                            continue;
                                        #endregion
                                        #region NEW_REVERSE_LOCATIONS_REQUEST
                                        case "!newreverseopened":
                                            if (!IsReverseSent)
                                            {
                                                await QUERIES.LOAD_SAVED_LOCATIONS(this);

                                                Packet UL = new Packet(0x181B);
                                                UL.WriteUInt8(0x2);
                                                UL.WriteUInt8(this.SavedLocations.Count);
                                                foreach (KeyValuePair<byte, SavedLocation> entry in this.SavedLocations)
                                                {
                                                    UL.WriteUInt8(entry.Key);
                                                    UL.WriteUInt16(entry.Value.region);
                                                }
                                                LOCAL_SECURITY.Send(UL);
                                                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                                IsReverseSent = true;
                                            }
                                            break;
                                        #endregion
                                        #region CHEST_REQUEST_LIST
                                        case "!chestopened":
                                            if (Convert.ToInt64((DateTime.Now.Subtract(LAST_CHEST_DELAY)).TotalSeconds) < 20)
                                                continue;
                                            LAST_CHEST_DELAY = DateTime.Now;
                                            await QUERIES.UpdateCharChest(this);
                                            if (CharChest.Count > 0)
                                            {
                                                int RemainSlots = await QUERIES.GetRemainSlots(CharID, ShardID);
                                                Packet Info = new Packet(0x5125);
                                                Info.WriteUInt32(CharChest.Count);
                                                Info.WriteInt32(RemainSlots);
                                                foreach (KeyValuePair<int, _CharChest> line in CharChest)
                                                {
                                                    Info.WriteInt32(line.Value.LineNum);
                                                    Info.WriteInt32(line.Key);
                                                    Info.WriteInt32(line.Value.RefItemID);
                                                    Info.WriteAscii(line.Value.Count.ToString());
                                                    Info.WriteInt8(line.Value.RandomizedStats);
                                                    Info.WriteAscii(line.Value.OptLevel.ToString());
                                                    Info.WriteAscii(line.Value.From);
                                                    Info.WriteAscii(line.Value.RegisterTime.ToShortDateString());
                                                }

                                                LOCAL_SECURITY.Send(Info);
                                                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                            }
                                            break;
                                        #endregion
                                        #region CHAR_RANK_REQUEST_LIST
                                        case "!rankopened":
                                            if (Convert.ToInt64((DateTime.Now.Subtract(LAST_UPDATERANKING_DELAY)).TotalSeconds) < Convert.ToInt64(UTILS.GUI_DELAY))
                                                continue;
                                            LAST_UPDATERANKING_DELAY = DateTime.Now;
                                            if (Settings.MAIN.checkBox15.Checked)
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox186.Text, CLIENT_SOCKET);
                                                continue;
                                            }

                                            // can be improved by requesting lists for each page not all at once
                                            this.SendRank(1, UTILS.CustomUniqueRank);
                                            this.SendRank(2, UTILS.CustomHonorRank);
                                            this.SendRank(3, UTILS.CustomPVPRank);
                                            this.SendRank(4, UTILS.CustomTraderRank);
                                            this.SendRank(5, UTILS.CustomHunterRank);
                                            this.SendRank(6, UTILS.CustomThiefRank);
                                            break;
                                        #endregion
                                        #region EVENT_TIMERS_UPDATE_REQUEST
                                        case "!eventopened":
                                            if (Convert.ToInt64((DateTime.Now.Subtract(LAST_UPDATETIMER_DELAY)).TotalSeconds) < Convert.ToInt64(UTILS.GUI_DELAY))
                                                continue;
                                            LAST_UPDATETIMER_DELAY = DateTime.Now;
                                            if (Settings.MAIN.checkBox13.Checked)
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox186.Text, CLIENT_SOCKET);
                                                continue;
                                            }
                                            if (UTILS.EventTimeList.Count > 0)
                                            {
                                                Packet Info = new Packet(0x5128);
                                                Info.WriteUInt8(UTILS.EventTimeList.Count);
                                                foreach (KeyValuePair<string, EventTime> line in UTILS.EventTimeList.OrderBy(x => x.Value.ID))
                                                {
                                                    Info.WriteAscii(line.Key);
                                                    Info.WriteAscii(line.Value.Day);
                                                    var myDate1 = Convert.ToDateTime(line.Value.Time);
                                                    DateTime myDate2 = DateTime.Now;
                                                    TimeSpan myDateResult;
                                                    myDateResult = myDate1 - myDate2;
                                                    string remaintime = "";
                                                    if (Convert.ToInt32(myDateResult.Days) > 0)
                                                        remaintime = $"{Convert.ToInt32(myDateResult.Days)}d" + $"{Convert.ToInt32(myDateResult.Hours)}h" + $" {Convert.ToInt32(myDateResult.Minutes)}m";
                                                    else if (Convert.ToInt32(myDateResult.Hours) > 0)
                                                        remaintime = $"{Convert.ToInt32(myDateResult.Hours)}h" + $" {Convert.ToInt32(myDateResult.Minutes)}m";
                                                    else
                                                        remaintime = $"{Convert.ToInt32(myDateResult.Minutes)}m";
                                                    Info.WriteAscii(remaintime);
                                                    Info.WriteAscii(line.Value.State);
                                                }
                                                LOCAL_SECURITY.Send(Info);
                                                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                            }
                                            break;
                                        #endregion
                                        #region UNIQUE_REQUEST_UPDATE
                                        #endregion
                                        #region CHANGELOG_REQUEST_LIST
                                        case "!changelogopened":
                                            if (Settings.MAIN.checkBox17.Checked)
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox186.Text, CLIENT_SOCKET);
                                                continue;
                                            }
                                            if (!IsChangeLogSent)
                                            {
                                                Packet Info = new Packet(0x9941);
                                                Info.WriteInt32(UTILS.ChangeLog.Count);
                                                foreach (string info in UTILS.ChangeLog)
                                                {
                                                    Info.WriteAscii(info);
                                                }
                                                LOCAL_SECURITY.Send(Info);
                                                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                                IsChangeLogSent = true;
                                            }
                                            break;
                                        #endregion
                                        #region PVP_MATCHING
                                        case "!registerpvp":
                                            if (ShardID != 64) continue;
                                            if (!UTILS.PVPeventRegCount.ContainsKey(CHARNAME16))
                                            {
                                                UTILS.PVPeventRegCount.TryAdd(CHARNAME16, 0);
                                            }
                                            else
                                            {
                                                if (UTILS.PVPeventRegCount[CHARNAME16] > 1)
                                                {
                                                    UTILS.SendNotice(UTILS.NoticeType.Yellow, "You have reached maximum amount of participations 2/2 try again tomorrow.", CLIENT_SOCKET);
                                                    continue;
                                                }
                                            }
                                            if (!UTILS.PVPregisterlist.Contains(CHARNAME16))
                                            {
                                                UTILS.PVPregisterlist.Add(CHARNAME16);
                                                UTILS.SendNotice(UTILS.NoticeType.Yellow, "You have successfully registered at pvp matching.", CLIENT_SOCKET);
                                            }
                                            else
                                                UTILS.SendNotice(UTILS.NoticeType.Yellow, $"You already registered,you #{UTILS.PVPregisterlist.IndexOf(CHARNAME16) + 1} in the queue.", CLIENT_SOCKET);
                                            break;
                                        case "!unregisterpvp":
                                            if (UTILS.PVPregisterlist.Contains(CHARNAME16))
                                            {
                                                UTILS.PVPregisterlist.Remove(CHARNAME16);
                                                UTILS.SendNotice(UTILS.NoticeType.Yellow, "You have successfully unregistered from pvp matching.", CLIENT_SOCKET);
                                            }
                                            else
                                                UTILS.SendNotice(UTILS.NoticeType.Yellow, $"You arent registered at this event", CLIENT_SOCKET);
                                            break;
                                        #endregion
                                        #region UNIQUE_MATCHING
                                        case "!registerunique":
                                            if (ShardID != 64) continue;

                                            if (!UTILS.UNIQUEeventRegCount.ContainsKey(CHARNAME16))
                                                UTILS.UNIQUEeventRegCount.TryAdd(CHARNAME16, 0);
                                            else
                                            {
                                                if (UTILS.UNIQUEeventRegCount[CHARNAME16] > 1)
                                                {
                                                    UTILS.SendNotice(UTILS.NoticeType.Yellow, "You have reached maximum amount of participations 2/2 try again tomorrow.", CLIENT_SOCKET);
                                                    continue;
                                                }
                                            }
                                            if (!UTILS.UNIQUEregisterlist.Contains(CHARNAME16))
                                            {
                                                UTILS.UNIQUEregisterlist.Add(CHARNAME16);
                                                UTILS.SendNotice(UTILS.NoticeType.Yellow, "You have successfully registered at unique matching.", CLIENT_SOCKET);
                                            }
                                            else
                                                UTILS.SendNotice(UTILS.NoticeType.Yellow, $"You already registered,you #{UTILS.UNIQUEregisterlist.IndexOf(CHARNAME16) + 1} in the queue.", CLIENT_SOCKET);
                                            break;
                                        case "!unregisterunique":
                                            if (UTILS.UNIQUEregisterlist.Contains(CHARNAME16))
                                            {
                                                UTILS.UNIQUEregisterlist.Remove(CHARNAME16);
                                                UTILS.SendNotice(UTILS.NoticeType.Yellow, "You have successfully unregistered from unique matching.", CLIENT_SOCKET);
                                            }
                                            else
                                                UTILS.SendNotice(UTILS.NoticeType.Yellow, $"You arent registered at this event", CLIENT_SOCKET);
                                            break;
                                        #endregion
                                        #region GUILD_WAR_EVENT
                                        case "!registerguildwar":
                                            if (ShardID != 64) continue;

                                            if (UTILS.GW_EVENT_REGISTER)
                                            {
                                                if (this.GUILDNAME != string.Empty)
                                                {
                                                    if (!UTILS.GuildWarEvent.ContainsKey(GUILDNAME))
                                                    {
                                                        if (UTILS.GuildWarEvent.Count < 4)
                                                        {
                                                            var NewRegister = new List<string>();
                                                            NewRegister.Add(CHARNAME16);
                                                            UTILS.GuildWarEvent.TryAdd(GUILDNAME, NewRegister);
                                                            UTILS.SendNoticeForAll(UTILS.NoticeType.Green, $"[{CHARNAME16}] has registered [{GUILDNAME}] at the guild war event.");

                                                        }
                                                        else
                                                            UTILS.SendNotice(UTILS.NoticeType.Yellow, "Event is currently full with 4 guilds!.", CLIENT_SOCKET);
                                                    }
                                                    else
                                                    {
                                                        if (!UTILS.GuildWarEvent[GUILDNAME].Contains(CHARNAME16))
                                                        {
                                                            UTILS.GuildWarEvent[GUILDNAME].Add(CHARNAME16);
                                                            UTILS.SendNotice(UTILS.NoticeType.Yellow, "You have registered successfully!.", CLIENT_SOCKET);
                                                        }
                                                        else
                                                            UTILS.SendNotice(UTILS.NoticeType.Yellow, "You already registered at this event!.", CLIENT_SOCKET);
                                                    }
                                                }
                                                else
                                                    UTILS.SendNotice(UTILS.NoticeType.Yellow, "This event is made for guilds only.", CLIENT_SOCKET);
                                            }
                                            break;
                                        case "!unregisterguildwar":
                                            if (UTILS.GW_EVENT_REGISTER)
                                            {
                                                if (UTILS.GuildWarEvent.ContainsKey(CHARNAME16))
                                                {
                                                    UTILS.GuildWarEvent[GUILDNAME].Remove(CHARNAME16);
                                                    UTILS.SendNotice(UTILS.NoticeType.Yellow, "You have unregistered successfully!.", CLIENT_SOCKET);
                                                }
                                            }
                                            break;
                                        #endregion
                                        #region JOB_FIGHT_EVENT
                                        case "!registerjobfight":
                                            if (ShardID != 64) continue;

                                            if (UTILS.JOB_FIGHT_REGISTER)
                                            {
                                                if (JobType > 0 && JobType < 4)
                                                {
                                                    if (!UTILS.JobFightEvent.Contains(CHARNAME16))
                                                    {
                                                        UTILS.JobFightEvent.Add(CHARNAME16);
                                                        UTILS.SendNotice(UTILS.NoticeType.Yellow, "You have successfully registered at job fight event!.", CLIENT_SOCKET);
                                                    }
                                                    else
                                                        UTILS.SendNotice(UTILS.NoticeType.Yellow, "You already registered at this event!.", CLIENT_SOCKET);
                                                }
                                            }
                                            break;
                                        case "!unregisterjobfight":
                                            if (UTILS.JOB_FIGHT_REGISTER)
                                            {
                                                if (UTILS.JobFightEvent.Contains(CHARNAME16))
                                                {
                                                    UTILS.JobFightEvent.Remove(CHARNAME16);
                                                    UTILS.SendNotice(UTILS.NoticeType.Yellow, "You have successfully unregistered at job fight event!.", CLIENT_SOCKET);
                                                }
                                                else
                                                    UTILS.SendNotice(UTILS.NoticeType.Yellow, "You arent registered at this event!.", CLIENT_SOCKET);
                                            }
                                            break;
                                        #endregion
                                        #region XSMB
                                        case "!xsmbopened":
                                            if (!Settings.MAIN.checkBox39.Checked)
                                                continue;
                                            if (!ISXSMBsent)
                                            {
                                                ISXSMBsent = true;
                                                Packet xsmb = new Packet(0x183A);
                                                xsmb.WriteAscii(UTILS.XSMB_DATE);
                                                foreach (var x in UTILS.XSMB)
                                                    xsmb.WriteUInt8(x);
                                                LOCAL_SECURITY.Send(xsmb);
                                                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                            }
                                            continue;
                                        case "!xsmblogopened":
                                            if (Convert.ToInt64((DateTime.Now.Subtract(LAST_XSMBLOG_TIMNE)).TotalSeconds) < Convert.ToInt64(60))
                                            {
                                                int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_XSMBLOG_TIMNE)).TotalSeconds) - Convert.ToInt64(60));
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox195.Text, Surplus), CLIENT_SOCKET);
                                                continue;
                                            }
                                            LAST_XSMBLOG_TIMNE = DateTime.Now;
                                            await QUERIES.LoadXsmbLogs(this);
                                            if (this.XsmbLog.Count > 0)
                                            {
                                                Packet logpacket = new Packet(0x183B);
                                                logpacket.WriteUInt8(XsmbLog.Count);
                                                foreach (var line in XsmbLog)
                                                {
                                                    logpacket.WriteUInt8(line.LineID + 30);
                                                    logpacket.WriteInt64(line.Amount);
                                                    logpacket.WriteUInt8(line.Type);
                                                    logpacket.WriteInt32(line.Num);
                                                    logpacket.WriteAscii(line.status);
                                                    logpacket.WriteAscii(line.date);

                                                }
                                                LOCAL_SECURITY.Send(logpacket);
                                                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                            }
                                            continue;
                                        #endregion
                                        #region SURVIVAL_ARENA_EVENT
                                        case "!registersurvival":
                                            if (ShardID != 64) continue;
                                            if (UTILS.SURVIVAL_ACTIVE)
                                            {
                                                if (!UTILS.SURVregisterlist.Contains(CHARNAME16))
                                                {
                                                    UTILS.SURVregisterlist.Add(CHARNAME16);
                                                    UTILS.SendNotice(UTILS.NoticeType.Yellow, Settings.MAIN.textBox203.Text, CLIENT_SOCKET);
                                                }
                                                else
                                                    UTILS.SendNotice(UTILS.NoticeType.Yellow, String.Format(Settings.MAIN.textBox202.Text, UTILS.SURVregisterlist.IndexOf(CHARNAME16) + 1), CLIENT_SOCKET);

                                            }
                                            continue;
                                        case "!unregistersurvival":
                                            if (UTILS.SURVregisterlist.Contains(CHARNAME16))
                                            {
                                                UTILS.SURVregisterlist.Remove(CHARNAME16);
                                                UTILS.SendNotice(UTILS.NoticeType.Yellow, Settings.MAIN.textBox200.Text, CLIENT_SOCKET);
                                            }
                                            else
                                                UTILS.SendNotice(UTILS.NoticeType.Yellow, Settings.MAIN.textBox129.Text, CLIENT_SOCKET);
                                            continue;
                                        #endregion
                                        #region DRUNK_EVENT
                                        case "!registerdrunk":
                                            if (ShardID != 64) continue;
                                            if (UTILS.DRUNK_REGISTER_ACTIVE)
                                            {
                                                if (!UTILS.DRUNKregisterlist.Contains(CHARNAME16))
                                                {
                                                    UTILS.DRUNKregisterlist.Add(CHARNAME16);
                                                    UTILS.SendNotice(UTILS.NoticeType.Yellow, Settings.MAIN.textBox203.Text, CLIENT_SOCKET);
                                                }
                                                else
                                                    UTILS.SendNotice(UTILS.NoticeType.Yellow, String.Format(Settings.MAIN.textBox202.Text, UTILS.DRUNKregisterlist.IndexOf(CHARNAME16) + 1), CLIENT_SOCKET);
                                            }
                                            continue;
                                        case "!unregisterdrunk":
                                            if (UTILS.DRUNKregisterlist.Contains(CHARNAME16))
                                            {
                                                UTILS.DRUNKregisterlist.Remove(CHARNAME16);
                                                UTILS.SendNotice(UTILS.NoticeType.Yellow, Settings.MAIN.textBox201.Text, CLIENT_SOCKET);
                                            }
                                            else
                                                UTILS.SendNotice(UTILS.NoticeType.Yellow, Settings.MAIN.textBox200.Text, CLIENT_SOCKET);
                                            continue;
                                        #endregion
                                        #region DAILY_REWARD
                                        case "!dailyrewardopened":
                                            if (!IsDailySent && UTILS.DailyRewardItems.Count == 20)
                                            {
                                                IsDailySent = true;
                                                Packet dailyInfo = new Packet(0x183C);
                                                _DailyReward dailyreward = UTILS.DailyReward[CHARNAME16];
                                                foreach (var x in UTILS.DailyRewardItems)
                                                    dailyInfo.WriteInt32(x);
                                                dailyInfo.WriteInt32(dailyreward.Total);
                                                dailyInfo.WriteUInt8(dailyreward.one_5);
                                                dailyInfo.WriteUInt8(dailyreward.six_10);
                                                dailyInfo.WriteUInt8(dailyreward.eleven_15);
                                                dailyInfo.WriteUInt8(dailyreward.sixteen_20);
                                                dailyInfo.WriteUInt8(dailyreward.twentyone_25);
                                                LOCAL_SECURITY.Send(dailyInfo);
                                                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                            }
                                            continue;
                                            #endregion
                                    }
                                    continue;
                                #endregion
                                #region ITEM_CHEST_RECEIVE&UPDATE
                                case 0x9947:
                                    int IID = INC_LOCAL[i].ReadInt32();
                                    uint refitemid = INC_LOCAL[i].ReadUInt32();
                                    string itemcode = INC_LOCAL[i].ReadAscii();
                                    int remainslots = await QUERIES.GetRemainSlots(CharID, ShardID);
                                    if (this.CharChest.Count() > 0)
                                    {

                                        if (this.CharChest.ContainsKey(IID))
                                        {
                                            var chestinfo = this.CharChest[IID];
                                            if (this.CharChest.Remove(IID))
                                            {
                                                if (remainslots > 0)
                                                {
                                                    await QUERIES.INSERT_INSTANT_ITEM(itemcode, chestinfo.Count, chestinfo.OptLevel, CharID, ShardID);
                                                    await QUERIES.RemoveItemChest(IID);
                                                }

                                                //await this.UpdateItemChestAsync(true);
                                            }
                                        }

                                    }
                                    continue;
                                case 0x9948:
                                    int remainslots2 = await QUERIES.GetRemainSlots(CharID, ShardID);
                                    if (remainslots2 > 0)
                                    {
                                        foreach (KeyValuePair<int, _CharChest> entry in this.CharChest)
                                        {
                                            if (remainslots2 > 0)
                                            {
                                                await QUERIES.INSERT_INSTANT_ITEM(await QUERIES.Get_ItemCode128_byid((int)entry.Value.RefItemID, ShardID), entry.Value.Count, entry.Value.OptLevel, CharID, ShardID);
                                                await QUERIES.RemoveItemChest(entry.Key);
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                            remainslots2 -= 1;
                                        }
                                    }
                                    else
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox185.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    await UpdateItemChestAsync(true);
                                    continue;

                                #endregion
                                #region CLIENT_UPDATE_TITLE
                                case 0x1201:
                                    if (Convert.ToInt64((DateTime.Now.Subtract(LAST_UPDATE_TITLE)).TotalSeconds) < Convert.ToInt64(UTILS.GUI_DELAY))
                                    {
                                        int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_UPDATE_TITLE)).TotalSeconds) - Convert.ToInt64(UTILS.GUI_DELAY));
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox184.Text, Surplus), CLIENT_SOCKET);
                                        continue;
                                    }
                                    LAST_UPDATE_TITLE = DateTime.Now;
                                    if (Settings.MAIN.JobTitle.Checked)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox186.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (INC_LOCAL[i].GetBytes().Length == 1)
                                    {
                                        //remove title
                                        LiveTitleUpdate((byte)0);
                                        foreach (var cTitle in UTILS.CustomTitleList)
                                        {
                                            if (cTitle.CharName == CHARNAME16 && cTitle.ShardID == ShardID)
                                            {
                                                foreach (var x in cTitle.Titles)
                                                {
                                                    cTitle.Titles[x.Key] = 0;
                                                }
                                                await QUERIES.UpdateCurrentActiveTitle(CHARNAME16, "bimbumtest");
                                            }
                                        }
                                        continue;
                                    }
                                    string TITLENAME = INC_LOCAL[i].ReadAscii();
                                    int TITLEID = INC_LOCAL[i].ReadInt32();

                                    //custom title
                                    if (TITLEID == 999)
                                    {
                                        foreach (var cTitle in UTILS.CustomTitleList)
                                        {
                                            if (cTitle.CharName == CHARNAME16 && cTitle.ShardID == ShardID)
                                            {
                                                foreach (var x in cTitle.Titles)
                                                {
                                                    if (x.Key == TITLENAME)
                                                    {
                                                        await QUERIES.UpdateCurrentActiveTitle(CHARNAME16, TITLENAME);
                                                        Packet custom2 = new Packet(0x5106);
                                                        custom2.WriteAscii(CHARNAME16);
                                                        custom2.WriteAscii(TITLENAME);
                                                        UTILS.BroadCastToClients(custom2, ShardID);
                                                        LiveTitleUpdate((byte)1);
                                                        cTitle.Titles[x.Key] = 1;
                                                    }
                                                    cTitle.Titles[x.Key] = 0;
                                                }
                                                continue;
                                            }
                                        }
                                    }
                                    //normal title
                                    else
                                    {
                                        if (!CharTitles.ContainsKey(CHARNAME16) || !CharTitles[CHARNAME16].Contains(TITLENAME))
                                            continue;
                                        foreach (var cTitle in UTILS.CustomTitleList)
                                        {
                                            if (cTitle.CharName == CHARNAME16 && cTitle.ShardID == ShardID)
                                            {
                                                foreach (var x in cTitle.Titles)
                                                {
                                                    if (cTitle.Titles[x.Key] == 1)
                                                    {
                                                        Packet Info = new Packet(0x5107);
                                                        Info.WriteAscii(CHARNAME16);
                                                        Info.WriteAscii(x.Key);
                                                        UTILS.BroadCastToClients(Info, ShardID);
                                                    }
                                                    cTitle.Titles[x.Key] = 0;
                                                }
                                                await QUERIES.UpdateCurrentActiveTitle(CHARNAME16, "bimbumtest");
                                                LiveTitleUpdate((byte)TITLEID);
                                                continue;
                                            }
                                        }
                                    }
                                    continue;
                                #endregion
                                #region CLIENT_PURCHASE_CUSTOM_TITLE
                                case 0x1202:
                                    byte cOt = INC_LOCAL[i].ReadUInt8();
                                    if (Convert.ToInt64((DateTime.Now.Subtract(LAST_PURCHASE_CUSTOMTITLE_DELAY)).TotalSeconds) < Convert.ToInt64(UTILS.GUI_DELAY))
                                    {
                                        int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_PURCHASE_CUSTOMTITLE_DELAY)).TotalSeconds) - Convert.ToInt64(UTILS.GUI_DELAY));
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox183.Text, Surplus), CLIENT_SOCKET);
                                        continue;
                                    }
                                    LAST_PURCHASE_CUSTOMTITLE_DELAY = DateTime.Now;
                                    if (cOt == 0x01)
                                    {
                                        
                                        int[] balance = await QUERIES.Get_TOT_SILK_BALANCE(CHARNAME16, ShardID);
                                        var Cur_Silk2 = balance[0];
                                        if (Cur_Silk2 < Convert.ToInt32(Settings.MAIN.textBox82.Text))
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox221.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                        await QUERIES.GIVE_SILK(UserName, -1 * Convert.ToInt32(Settings.MAIN.textBox82.Text));
                                        if (Settings.MAIN.checkBox24.Checked)
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox186.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                        string Name = INC_LOCAL[i].ReadAscii();
                                        string CustomTI = INC_LOCAL[i].ReadAscii();
                                        if (Name != CHARNAME16 || (READER.FILTER_KEYWORDS.Count > 0 && READER.FILTER_KEYWORDS.Contains(CustomTI.ToLower())))
                                        {
                                            continue;
                                        }
                                        _CustomTitle customTitle = new _CustomTitle();
                                        customTitle = UTILS.CustomTitleList.Find(x => x.CharName == CHARNAME16 && x.ShardID == ShardID);
                                        if (customTitle != null)
                                        {
                                            if (customTitle.Titles.ContainsKey(CustomTI))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox182.Text, CLIENT_SOCKET);
                                                continue;
                                            }
                                            else
                                            {
                                                foreach (var x in customTitle.Titles)
                                                    customTitle.Titles[x.Key] = 0;
                                                customTitle.Titles.TryAdd(CustomTI, 1);
                                            }
                                        }
                                        else
                                        {
                                            _CustomTitle Custom = new _CustomTitle();
                                            Custom.CharName = CHARNAME16;
                                            Custom.Titles.TryAdd(CustomTI, 1);
                                            Custom.ShardID = ShardID;
                                            UTILS.CustomTitleList.Add(Custom);
                                        }

                                        LiveTitleUpdate(1);
                                        await QUERIES.AddCustomTitle(CHARNAME16, CustomTI, ShardID);
                                        Packet custom = new Packet(0x5106);
                                        custom.WriteAscii(Name);
                                        custom.WriteAscii(CustomTI);
                                        UTILS.BroadCastToClients(custom, ShardID);
                                    }
                                    else if (cOt == 0x02)
                                    {
                                        string CharName = INC_LOCAL[i].ReadAscii();
                                        uint Color = INC_LOCAL[i].ReadUInt32();
                                        int[] balance = await QUERIES.Get_TOT_SILK_BALANCE(CHARNAME16, ShardID);
                                        var Cur_Silk2 = balance[0];
                                        if (Cur_Silk2 < 200)
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox221.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                        await QUERIES.GIVE_SILK(UserName, -1 * 200);
                                        
                                        var HEXColor = "#" + Color.ToString("X");
                                        await QUERIES.SQL_Run_Code($"exec xQc_FILTER.[dbo].[_ADDCustomTitleColor] '{CharName}','{HEXColor}',{this.ShardID}");

                                    }
                                    continue;
                                #endregion
                                #region CLIENT_ITEM_LINKING
                                case 0x180C:
                                    var ItemName = INC_LOCAL[i].ReadAscii();
                                    var ItemSlot = INC_LOCAL[i].ReadUInt8();
                                    var ItemID = INC_LOCAL[i].ReadInt32();
                                    _ItemInfo Item = new _ItemInfo();
                                    Item = await QUERIES.Get_ItemInfo_by_Slot(ItemSlot, CharID, ShardID, ItemID);
                                    Item = await QUERIES.Get_BindingInfo_by_Slot(Item, ShardID);
                                    Item.ItemID = ItemID;
                                    if (!UTILS.ItemLinkInfo.ContainsKey($"{CHARNAME16}<{ItemName}>"))
                                        UTILS.ItemLinkInfo.TryAdd($"{CHARNAME16}<{ItemName}>", Item);
                                    else
                                        UTILS.ItemLinkInfo[$"{CHARNAME16}<{ItemName}>"] = Item;
                                    continue;
                                case 0x180F:
                                    byte type6 = INC_LOCAL[i].ReadUInt8();
                                    if(type6 == 0x01)
                                    {
                                        byte SlotID1 = INC_LOCAL[i].ReadUInt8();
                                        var ItemNameKey = INC_LOCAL[i].ReadAscii();
                                        if (UTILS.ItemLinkInfo.ContainsKey(ItemNameKey))
                                            SendB034(UTILS.ItemLinkInfo[ItemNameKey], SlotID1);
                                    }
                                    else if(type6 ==0x02)
                                    {
                                        byte SlotID1 = INC_LOCAL[i].ReadUInt8();
                                        Packet b034 = new Packet(0xB034);
                                        b034.WriteUInt8(0x01);
                                        b034.WriteUInt8(0x07);
                                        b034.WriteUInt8(SlotID1);
                                        LOCAL_SECURITY.Send(b034);
                                        ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                    }
                                    continue;
                                #endregion
                                #region CLIENT_GS_GAMESERVER_ADDOON
                                case 0x3500:
                                    UTILS.WriteLine($"Someone trying to send filter msg which is supposed to be sent by GS addon. Remote address [{SOCKET_IP}].", UTILS.LOG_TYPE.Warning);
                                    ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                    return;
                                break;
                                #endregion
                                #region CLIENT_DEATH_REQUEST
                                case 0x3053:
                                    byte state = INC_LOCAL[i].ReadUInt8();
                                    if (state == 0x1)
                                    {
                                        if (INSIDE_JF && JobType == 2)
                                        {
                                            LifeStats();
                                            await Task.Delay(500);
                                            await QUERIES.INSERT_INSTANT_TELEPORT(CharID, 1, 1, 29399, 1597, 103, 1367,64);
                                            continue;
                                        }
                                        else if (INSIDE_JF && (JobType == 1 || JobType == 3))
                                        {
                                            LifeStats();
                                            await Task.Delay(500);
                                            await QUERIES.INSERT_INSTANT_TELEPORT(CharID, 1, 1, 29655, 62, 107, 945,64);
                                            continue;
                                        }
                                        
                                    }
                                    break;
                                #endregion
                                #region CLIENT_AUTOEQUIP_TELEPORT
                                case 0x3502:
                                    if (Convert.ToInt64((DateTime.Now.Subtract(LAST_RELOAD_DELAY)).TotalSeconds) < Convert.ToInt64(UTILS.GUI_DELAY))
                                    {
                                        int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_RELOAD_DELAY)).TotalSeconds) - Convert.ToInt64(UTILS.GUI_DELAY));
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox181.Text, Surplus), CLIENT_SOCKET);
                                        continue;
                                    }
                                    LAST_RELOAD_DELAY = DateTime.Now;
                                    if (!Settings.MAIN.checkBox7.Checked)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox186.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (CUR_LEVEL > Convert.ToInt32(Settings.MAIN.numericUpDown27.Value))
                                    {
                                        //Utils.SendErrorMsg($"You are not allowed to use this function", CLIENT_SOCKET);
                                        continue;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_NEW_GRANT_NAME_REQUEST
                                case 0x3501:
                                    if (Settings.MAIN.checkBox11.Checked)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox186.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    uint GnUniueID = INC_LOCAL[i].ReadUInt32();
                                    string NewGrantName = INC_LOCAL[i].ReadAscii();
                                    if (Settings.CHAT_FILTER && READER.FILTER_KEYWORDS.Count > 0 && READER.FILTER_KEYWORDS.Contains(NewGrantName.ToLower()))
                                    {
                                        //Utils.SendErrorMsg($"Some expressions are not allowed in our server.", CLIENT_SOCKET);
                                        continue;
                                    }
                                    break;
                                #endregion
                                #region REVERSE_MOVE_TO_PARTY_MEMBER
                                case 0x181A:
                                    if (Settings.MAIN.checkBox35.Checked)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox186.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    byte reverse_slot = INC_LOCAL[i].ReadUInt8();
                                    int jid = INC_LOCAL[i].ReadInt32();
                                    int x1 = INC_LOCAL[i].ReadInt32();
                                    int y1 = INC_LOCAL[i].ReadInt32();
                                    int z1 = INC_LOCAL[i].ReadInt32();
                                    int region = INC_LOCAL[i].ReadInt32();
                                    ushort TID = INC_LOCAL[i].ReadUInt16();

                                    int[] ItemData2= await QUERIES.Get_ItemID_Data(CHARNAME16, reverse_slot, ShardID,CharID);
                                    if (UTILS.Region_Restrection.ContainsKey(region))
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox194.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    //if (Utils.Region_Restrection[region] == 1)
                                    //{
                                    if (this.DEAD_STATUS || this.JOB_FLAG || (ItemData2[0] != 3 && ItemData2[1] != 3 && ItemData2[2] != 3 && ItemData2[3] != 3) ||  !this.PartyMembers.Contains((uint)jid))
                                            continue;
                                    //update item count using new defined type
                                    this.Update_Item_Count(reverse_slot , TID);
                                    await Task.Delay(500);
                                    this.LiveTeleport(1, 1, (ushort)region, x1, y1, z1);
                                    //Console.WriteLine($"{x1} .. {y} .. {z}");
                                    //Console.WriteLine($"reverse_slot:: {reverse_slot}");
                                    //}
                                    continue;
                                #endregion
                                #region REVERSE_MOVE_TO_SAVED_LOCATION
                                case 0x181C:
                                    if (Settings.MAIN.checkBox35.Checked)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox186.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    
                                    byte r_Slot = INC_LOCAL[i].ReadUInt8();
                                    byte btn_ID = INC_LOCAL[i].ReadUInt8();
                                    ushort r_Type = INC_LOCAL[i].ReadUInt16();
                                    int[] ItemData3 = await QUERIES.Get_ItemID_Data(CHARNAME16, r_Slot, ShardID, CharID);
                                    if (this.DEAD_STATUS || this.JOB_FLAG || (ItemData3[0] != 3 && ItemData3[1] != 3 && ItemData3[2] != 3 && ItemData3[3] != 3))
                                        continue;
                                    //update item count

                                    this.Update_Item_Count(r_Slot , r_Type);

                                    //move to location
                                    SavedLocation Loc = new SavedLocation();
                                    Loc = this.SavedLocations[btn_ID];
                                    await Task.Delay(500);

                                   this.LiveTeleport(1, 1, (ushort)Loc.region, Loc.x, Loc.y, Loc.z);
                                    continue;
                                #endregion
                                #region SAVE_REMOVE_REVERSE_LOCATIONS
                                case 0x181B:
                                    if (Convert.ToInt64((DateTime.Now.Subtract(LAST_NEWREVERSE_DELAY)).TotalSeconds) < Convert.ToInt64(UTILS.GUI_DELAY))
                                    {
                                        int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_NEWREVERSE_DELAY)).TotalSeconds) - Convert.ToInt64(UTILS.GUI_DELAY));
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox193.Text, Surplus), CLIENT_SOCKET);
                                        continue;
                                    }
                                    LAST_NEWREVERSE_DELAY = DateTime.Now;
                                    if (Settings.MAIN.checkBox35.Checked)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox186.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    byte Action_Type = INC_LOCAL[i].ReadUInt8();
                                    if (Action_Type == 0x1)
                                    {

                                        byte Button_Tag = INC_LOCAL[i].ReadUInt8();
                                        ushort Region = INC_LOCAL[i].ReadUInt16();
                                        int Current_x = INC_LOCAL[i].ReadInt32();
                                        int Current_y = INC_LOCAL[i].ReadInt32();
                                        int Current_z = INC_LOCAL[i].ReadInt32();
                                        string LocationName = INC_LOCAL[i].ReadAscii();

                                        //check if region isnt restrectid then send confirmation.
                                        if (UTILS.Region_Restrection.ContainsKey(Region))
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox207.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                        //if (Utils.Region_Restrection[Region] == 1)
                                        //{
                                            Packet Confirm = new Packet(0x181B);
                                            Confirm.WriteUInt8(0x1);
                                            Confirm.WriteUInt8(Button_Tag);
                                            Confirm.WriteUInt16(Region);
                                            LOCAL_SECURITY.Send(Confirm);
                                            ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);

                                            //add it to the list
                                            SavedLocation SL = new SavedLocation();
                                            SL.x = Current_x;
                                            SL.y = Current_y;
                                            SL.z = Current_z;
                                            SL.region = Region;
                                            SL.locationName = LocationName;
                                            this.SavedLocations.Add(Button_Tag, SL);

                                            //save it to database
                                            await QUERIES.ADD_NEW_LOCATION(CharID, Button_Tag, Region, Current_x, Current_y, Current_z, LocationName,ShardID);
                                        //}
                                        //else
                                           //Utils.SendErrorMsg($"You cannot save the current location.", CLIENT_SOCKET);
                                    }
                                    else if (Action_Type == 0x2)
                                    {
                                        byte Button_Index = INC_LOCAL[i].ReadUInt8();
                                        this.SavedLocations.Remove(Button_Index);

                                        //remove it from database
                                        await QUERIES.REMOVE_LOCATION(CharID, Button_Index,ShardID);
                                    }
                                    continue;
                                #endregion
                                #region PK
                                case 0x7516:
                                    if (!Settings.BOT_ALLOW_PVP && !NONE_CLIENTLESS_MARKER)
                                    {
                                        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_BOT_PVP", CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (INSIDE_SURV)
                                        continue;
                                    break;
                                #endregion
                                #region PREMIUM_USAGE
                                case 0x715f:
                                        continue;
                                    break;
                                #endregion
                                #region CLIENT_ITEM_LOCK_STATUS
                                case 0x1204:
                                    byte LockType = INC_LOCAL[i].ReadUInt8();
                                    string passcode = INC_LOCAL[i].ReadAscii();
                                    //Logger.Warn($"LockType {LockType} passcode {passcode}");
                                    if (CharLockCurPassword != null)//registerted
                                    {
                                        if (UTILS.IsDigits(passcode))
                                        {
                                            switch (LockType)
                                            {
                                                //unlock
                                                case 1:
                                                    if (passcode.ToLower() == CharLockCurPassword.ToLower())
                                                    {
                                                        if (CharLockStatue)
                                                        {
                                                            CharLockStatue = false;
                                                            await QUERIES.SET_LOCK(UserName, 0);
                                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox206.Text, CLIENT_SOCKET);
                                                        }
                                                        else
                                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox205.Text, CLIENT_SOCKET);

                                                    }
                                                    else
                                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox204.Text, CLIENT_SOCKET);
                                                    break;
                                                //lock
                                                case 2:
                                                    if (passcode.ToLower() == CharLockCurPassword.ToLower())
                                                    {
                                                        if (!CharLockStatue)
                                                        {
                                                            CharLockStatue = true;
                                                            await QUERIES.SET_LOCK(UserName, 1);
                                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox210.Text, CLIENT_SOCKET);

                                                        }
                                                        else
                                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox209.Text, CLIENT_SOCKET);

                                                    }
                                                    else
                                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox204.Text, CLIENT_SOCKET);
                                                    break;
                                                //remove
                                                case 3:
                                                    if (passcode.ToLower() == CharLockCurPassword.ToLower())
                                                    {

                                                        CharLockCurPassword = null;
                                                        CharLockStatue = false;
                                                        if (await QUERIES.RemoveCharLock(UserName))
                                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox208.Text, CLIENT_SOCKET);

                                                    }
                                                    else
                                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox204.Text, CLIENT_SOCKET);
                                                    break;

                                                case 4:
                                                    UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox212.Text, CLIENT_SOCKET);
                                                    break;
                                            }
                                        }
                                        else
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox211.Text, CLIENT_SOCKET);
                                    }
                                    else
                                    {
                                        if (LockType == 4)
                                        {
                                            if (UTILS.IsDigits(passcode))
                                            {
                                                CharLockCurPassword = passcode.ToLower();
                                                CharLockStatue = true;

                                                if (await QUERIES.CreateCharLock(UserName, passcode))
                                                     UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox210.Text, CLIENT_SOCKET);
                                            }
                                            else
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox211.Text, CLIENT_SOCKET);

                                        }
                                        else
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox204.Text, CLIENT_SOCKET);
                                    }
                                    continue;
                                #endregion
                                #region CLIENT_ITEM_LOCK_USAGE
                                //CLIENT_GUILD_DISBAND
                                case 0x70F1:
                                //CLIENT_GUILD_DONATE
                                case 0x7258:
                                //CLIENT_GUILD_KICK
                                case 0x70F4:
                                //CLIENT_GUILD_LEAVE
                                case 0x70F2:
                                //CLIENT_GUILD_UPDATE_PERMISSION
                                case 0x7104:
                                //CLIENT_GUILD_STORAGE_OPEN
                                case 0x7250:
                                //CLIENT_GUILD_STORAGE_LIST
                                case 0x7252:
                                //CLIENT_GUILD_UNION_KICK
                                case 0x70FD:
                                //CLIENT_GUILD_UNION_LEAVE
                                case 0x70FC:
                                //CLIENT_GUILD_CREATE
                                case 0x70F0:
                                //CLIENT_PLAYER_UPDATE_STR
                                case 0x7050:
                                //CLIENT_PLAYER_UPDATE_INT
                                case 0x7051:
                                //CLIENT_SKILL_LEARN
                                case 0x70A1:
                                //CLIENT_ALCHEMY_DISMANTLE
                                case 0x7157:
                                //CLIENT_PREMIUM
                                //CLIENT_STALL_BUY
                                case 0x70B4:
                                //CLIENT_NPC_BUYPACK
                                case 0x7168:
                                    //add check
                                    if (CharLockStatue)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox213.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_EXCHANGE_REQUEST
                                //CLIENT_EXCHANGE_CONFIRM
                                case 0x7082:
                                    if (CharLockStatue)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox213.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    break;
                                //CLIENT_EXCHANGE_APPROVE
                                case 0x7083:
                                    
                                    if (CharLockStatue)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox213.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_GAMBLING_REQUEST
                                case 0x1807:
                                    continue;
                                    if (Convert.ToInt64((DateTime.Now.Subtract(LAST_GAMBLE_TIME)).TotalSeconds) < Convert.ToInt64(20))
                                    {
                                        int Surplus = (int)Math.Abs(Convert.ToInt64((DateTime.Now.Subtract(LAST_GAMBLE_TIME)).TotalSeconds) - Convert.ToInt64(20));
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox217.Text, Surplus), CLIENT_SOCKET);
                                        continue;
                                    }
                                    if(!Settings.MAIN.checkBox38.Checked)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox216.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    TimeSpan GambleStart = Convert.ToDateTime(Settings.MAIN.dateTimePicker2.Value).TimeOfDay;
                                    TimeSpan GambleEnd = Convert.ToDateTime(Settings.MAIN.dateTimePicker1.Value).TimeOfDay;
                                    TimeSpan NowDate2 = DateTime.Now.TimeOfDay; 

                                    if (!(NowDate2 > GambleStart && NowDate2 < GambleEnd))
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox214.Text, GambleStart.ToString(), GambleEnd.ToString()), CLIENT_SOCKET);
                                        continue;
                                    }

                                    if (UTILS.GambleAttempts.ContainsKey(CHARNAME16) && Settings.MAIN.numericUpDown20.Value > 0)
                                    {
                                        if (UTILS.GambleAttempts[CHARNAME16] >= Settings.MAIN.numericUpDown20.Value)
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                        UTILS.GambleAttempts.TryAdd(CHARNAME16, 0);

                                    UTILS.GambleAttempts[CHARNAME16] += 1;

                                    LAST_GAMBLE_TIME = DateTime.Now;
                                    byte isSilk = 0;
                                    byte isGold = 0;
                                    string silk_amount = string.Empty;
                                    string gold_amount = string.Empty;

                                    isSilk = INC_LOCAL[i].ReadUInt8();
                                    if(isSilk==1)
                                        silk_amount = INC_LOCAL[i].ReadAscii();
                                    isGold = INC_LOCAL[i].ReadUInt8();
                                    if(isGold==1)
                                        gold_amount = INC_LOCAL[i].ReadAscii();

                                    if (JOB_FLAG)
                                        continue;

                                    int Cur_Silk = 0;
                                    long Cur_Gold = 0;
                                    bool Won = new Random().Next(0, 100) < Settings.MAIN.numericUpDown15.Value;
                                    bool break2= false;
                                    Task.Factory.StartNew(async () =>
                                    {

                                        Packet window = new Packet(0x1807);
                                        window.WriteUInt8(0);
                                        LOCAL_SECURITY.Send(window);
                                        ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                        await Task.Delay(6000);
                                        if (isSilk == 1)
                                        {
                                            int[] balance = await QUERIES.Get_TOT_SILK_BALANCE(CHARNAME16, ShardID);
                                            Cur_Silk = balance[0];
                                            if (Cur_Silk < Convert.ToInt32(silk_amount))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox221.Text, CLIENT_SOCKET);
                                                break2 = true;
                                            }
                                        }

                                        if (isGold == 1)
                                        {
                                            Cur_Gold = await QUERIES.GOLD_BALANCE(CHARNAME16, ShardID);
                                            if (Cur_Gold < Convert.ToInt64(gold_amount))
                                            {
                                                UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox220.Text, CLIENT_SOCKET);
                                                break2 = true;
                                            }
                                        }
                                        if(!break2)
                                        {
                                            if (Won)
                                            {
                                                Packet win = new Packet(0x1807);
                                                win.WriteUInt8(1);
                                                LOCAL_SECURITY.Send(win);
                                                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                                if (isSilk == 1)
                                                {
                                                    await QUERIES.GIVE_SILK(UserName, Convert.ToInt32(silk_amount));
                                                    await QUERIES.GambeLogs(CHARNAME16, $"Won {silk_amount} silk");
                                                    if (Convert.ToInt32(silk_amount) > 500)
                                                        UTILS.SendNoticeForAll(UTILS.NoticeType.Green, String.Format(Settings.MAIN.textBox219.Text, CHARNAME16, Convert.ToInt32(silk_amount)));

                                                }
                                                if (isGold == 1)
                                                {
                                                    Packet LV = new Packet(0x3502);
                                                    LV.WriteUInt32(UNIQUE_ID);
                                                    REMOTE_SECURITY.Send(LV);
                                                    ASYNC_SEND_TO_MODULE();
                                                    await QUERIES.GIVE_GOLD(CHARNAME16, Convert.ToInt32(gold_amount), ShardID);
                                                    await QUERIES.GambeLogs(CHARNAME16, $"Won {gold_amount} gold");
                                                    if (Convert.ToInt32(gold_amount) > 30000000)
                                                        UTILS.SendNoticeForAll(UTILS.NoticeType.Green, String.Format(Settings.MAIN.textBox219.Text, CHARNAME16, Convert.ToInt32(gold_amount)));
                                                }
                                            }
                                            else
                                            {
                                                Packet lose = new Packet(0x1807);
                                                lose.WriteUInt8(2);
                                                LOCAL_SECURITY.Send(lose);
                                                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                                if (isSilk == 1)
                                                {
                                                    await QUERIES.GIVE_SILK(UserName, -1 * Convert.ToInt32(silk_amount));
                                                    await QUERIES.GambeLogs(CHARNAME16, $"Lose {silk_amount} silk");
                                                    if (Convert.ToInt32(silk_amount) > 500)
                                                        UTILS.SendNoticeForAll(UTILS.NoticeType.Green, String.Format(Settings.MAIN.textBox218.Text, CHARNAME16, Convert.ToInt32(silk_amount)));
                                                }
                                                if (isGold == 1)
                                                {
                                                    Packet LV = new Packet(0x3502);
                                                    LV.WriteUInt32(UNIQUE_ID);
                                                    REMOTE_SECURITY.Send(LV);
                                                    ASYNC_SEND_TO_MODULE();
                                                    await QUERIES.GIVE_GOLD(CHARNAME16, -1 * Convert.ToInt64(gold_amount), ShardID);
                                                    await QUERIES.GambeLogs(CHARNAME16, $"Lose {gold_amount} gold");
                                                    if (Convert.ToInt32(gold_amount) > 30000000)
                                                        UTILS.SendNoticeForAll(UTILS.NoticeType.Green, String.Format(Settings.MAIN.textBox218.Text, CHARNAME16, Convert.ToInt32(gold_amount)));

                                                }

                                            }
                                        }
                                        
                                    });

                                    continue;
                                #endregion
                                #region CLIENT_XSMB_REQUEST
                                case 0x183A:
                                if (!Settings.MAIN.checkBox39.Checked)
                                    continue;
                                TimeSpan XSMBSTART = Convert.ToDateTime("18:00:00").TimeOfDay;
                                TimeSpan XSMBEND = Convert.ToDateTime("20:00:00").TimeOfDay;
                                TimeSpan NOWDATE = DateTime.Now.TimeOfDay; 

                                if ((NOWDATE > XSMBSTART && NOWDATE < XSMBEND))
                                {
                                    UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox223.Text, CLIENT_SOCKET);
                                    continue;
                                }
                              
                                byte xsmbtype = INC_LOCAL[i].ReadUInt8();
                                string number = INC_LOCAL[i].ReadAscii();
                                string amount = INC_LOCAL[i].ReadAscii();
                                if(!string.IsNullOrEmpty(amount) && !string.IsNullOrEmpty(number) && UTILS.IS_DIGITS_ONLY(amount) && UTILS.IS_DIGITS_ONLY(number))
                                {
                                    if (Convert.ToInt32(number) > 99 || Convert.ToInt32(number) < 0)
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox222.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (xsmbtype == 1 &&(Convert.ToInt32(amount) < 10))
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox230.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (xsmbtype == 2 &&(Convert.ToInt32(amount) < 1000000))
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox229.Text, CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (xsmbtype == 1)
                                    {   
                                        if (!Settings.MAIN.checkBox40.Checked)
                                          continue;
                                        //silk
                                        int[] balance = await QUERIES.Get_TOT_SILK_BALANCE(CHARNAME16, ShardID);
                                        Cur_Silk = balance[0];
                                        if (Cur_Silk < Convert.ToInt32(amount))
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox221.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                        await QUERIES.GIVE_SILK(UserName, -1 * Convert.ToInt32(amount));
                                        await QUERIES.INSERT_XSMB_EVENT(CHARNAME16, Convert.ToInt64(amount), xsmbtype,Convert.ToInt32(number));
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox224.Text +" Silk", amount), CLIENT_SOCKET);

                                    }
                                    else
                                    {
                                        if (!Settings.MAIN.checkBox41.Checked)
                                          continue;
                                        long Gold_Amount = await QUERIES.Get_Storage_Gold_Amount(UserName, ShardID);
                                        if(Gold_Amount < Convert.ToInt64(amount))
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox220.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                        if (this.JOB_FLAG || this.STORAGE_OPENED)
                                        {
                                            UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox225.Text, CLIENT_SOCKET);
                                            continue;
                                        }
                                        await QUERIES.Update_Storage_Gold_Amount(UserName, ShardID, Convert.ToInt64(amount) * -1);
                                        await QUERIES.INSERT_XSMB_EVENT(CHARNAME16, Convert.ToInt64(amount), xsmbtype, Convert.ToInt32(number));
                                        Packet LV = new Packet(0x3502);
                                        LV.WriteUInt32(UNIQUE_ID);
                                        REMOTE_SECURITY.Send(LV);
                                        ASYNC_SEND_TO_MODULE();
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, String.Format(Settings.MAIN.textBox224.Text + " Gold", amount), CLIENT_SOCKET);

                                        }
                                    }
                                continue;
                                #endregion
                                #region CLIENT_STORAGE_OPENED
                                case 0x7046:
                                    if (INC_LOCAL[i].GetBytes().Length == 5)
                                    {
                                        uint targetid = INC_LOCAL[i].ReadUInt32();
                                        byte storage = INC_LOCAL[i].ReadUInt8();
                                        if (storage == (byte)3)
                                            this.STORAGE_OPENED = true;
                                    }
                                    break;
                                #endregion
                                #region CLIENT_DAILY_REWARD
                                case 0x183C:
                                    if (Convert.ToInt64((DateTime.Now.Subtract(LAST_DAILYREWARD_CLICK)).TotalSeconds) < Convert.ToInt64(1))
                                        continue;
                                    LAST_DAILYREWARD_CLICK = DateTime.Now;
                                    byte buttonIndex = INC_LOCAL[i].ReadUInt8();
                                    if (UTILS.DailyReward.ContainsKey(CHARNAME16))
                                    {
                                        switch (buttonIndex)
                                        {
                                            case 0x01:
                                                if (UTILS.DailyReward[CHARNAME16].Total >= 5 && !UTILS.DailyReward[CHARNAME16].one_5)
                                                {
                                                    UTILS.DailyReward[CHARNAME16].one_5 = true;
                                                    await QUERIES.SQL_Run_Code($"update xQc_FILTER.dbo._DailyReward set one_5 = 1 where CharName16 = '{CHARNAME16}'");
                                                    for (int j = 0; j <= 3; j++)
                                                        await QUERIES.INSERT_INSTANT_ITEM(await QUERIES.Get_ItemCode128_byid((int)UTILS.DailyRewardItems[j], ShardID), 1,0, CharID, ShardID);
                                                }
                                                continue;
                                            case 0x02:
                                                if (UTILS.DailyReward[CHARNAME16].Total >= 10 && !UTILS.DailyReward[CHARNAME16].six_10)
                                                {
                                                    UTILS.DailyReward[CHARNAME16].six_10 = true;
                                                    await QUERIES.SQL_Run_Code($"update xQc_FILTER.dbo._DailyReward set six_10 = 1 where CharName16 = '{CHARNAME16}'");
                                                    for (int j = 4; j <= 7; j++)
                                                        await QUERIES.INSERT_INSTANT_ITEM(await QUERIES.Get_ItemCode128_byid((int)UTILS.DailyRewardItems[j], ShardID), 1, 0, CharID, ShardID);
                                                }
                                                continue;
                                            case 0x03:
                                                if (UTILS.DailyReward[CHARNAME16].Total >= 15 && !UTILS.DailyReward[CHARNAME16].eleven_15)
                                                {
                                                    UTILS.DailyReward[CHARNAME16].eleven_15 = true;
                                                    await QUERIES.SQL_Run_Code($"update xQc_FILTER.dbo._DailyReward set eleven_15 = 1 where CharName16 = '{CHARNAME16}'");
                                                    for (int j = 8; j <= 11; j++)
                                                        await QUERIES.INSERT_INSTANT_ITEM(await QUERIES.Get_ItemCode128_byid((int)UTILS.DailyRewardItems[j], ShardID), 1, 0, CharID, ShardID);
                                                }
                                                continue;
                                            case 0x04:
                                                if (UTILS.DailyReward[CHARNAME16].Total >= 20 && !UTILS.DailyReward[CHARNAME16].sixteen_20)
                                                {
                                                    UTILS.DailyReward[CHARNAME16].sixteen_20 = true;
                                                    await QUERIES.SQL_Run_Code($"update xQc_FILTER.dbo._DailyReward set sixteen_20 = 1 where CharName16 = '{CHARNAME16}'");
                                                    for (int j = 12; j <= 15; j++)
                                                        await QUERIES.INSERT_INSTANT_ITEM(await QUERIES.Get_ItemCode128_byid((int)UTILS.DailyRewardItems[j], ShardID), 1, 0, CharID, ShardID);
                                                }
                                                continue;
                                            case 0x05:
                                                if (UTILS.DailyReward[CHARNAME16].Total >= 25 && !UTILS.DailyReward[CHARNAME16].twentyone_25)
                                                {
                                                    UTILS.DailyReward[CHARNAME16].twentyone_25 = true;
                                                    await QUERIES.SQL_Run_Code($"update xQc_FILTER.dbo._DailyReward set twentyone_25 = 1 where CharName16 = '{CHARNAME16}'");
                                                    for (int j = 16; j <= 19; j++)
                                                        await QUERIES.INSERT_INSTANT_ITEM(await QUERIES.Get_ItemCode128_byid((int)UTILS.DailyRewardItems[j], ShardID), 1, 0, CharID, ShardID);
                                                }
                                                continue;
                                        }
                                    }
                                    
                                    continue;
                                #endregion
                                #region SHADOWGARDEN_PACKETS
                                case 0x1318:
                                    uint SG_ID = INC_LOCAL[i].ReadUInt32();
                                    switch(SG_ID)
                                    {
                                        case 1:
                                            string LoadRank = INC_LOCAL[i].ReadAscii();
                                            SG_QUERIES.LoadServerRank(LoadRank,CLIENT_SOCKET);
                                            break;
                                        case 2:
                                            string LoadUniqueHistory = INC_LOCAL[i].ReadAscii();
                                            UTILS.SendUniqueLog(CLIENT_SOCKET);
                                            break;
                                        case 3:
                                            string FlowerToCharName = INC_LOCAL[i].ReadAscii();
                                            uint FlowerID = INC_LOCAL[i].ReadUInt32();
                                            
                                            var result = await SG_QUERIES.HallOfFameSendPoints(CHARNAME16, FlowerToCharName, FlowerID);
                                            string resultSenderMessage = result.SenderMessage;
                                            string resultReceiverMessage = result.ReceiverMessage; 
                                            
                                            SG_QUERIES.LoadHallOfFameAsync(CHARNAME16, CLIENT_SOCKET);
                                            UTILS.SEND_INDV_NOTICE(resultSenderMessage, CLIENT_SOCKET);

                                            if (resultReceiverMessage != "Faild")
                                            {
                                                Socket CLIENT_SOCKET1 = ASYNC_SERVER.AG_CONS.FirstOrDefault(x => x.Value.CHARNAME16 == FlowerToCharName && !string.IsNullOrEmpty(x.Value.CHARNAME16)).Key;
                                                if (CLIENT_SOCKET1 != null)
                                                {
                                                    UTILS.SEND_INDV_NOTICE(resultReceiverMessage, CLIENT_SOCKET1);
                                                }
                                            }

                                            break;
                                        case 4:
                                            string SearchCharName = INC_LOCAL[i].ReadAscii();
                                            SG_QUERIES.SearchHallOfFameAsync(SearchCharName, CLIENT_SOCKET);
                                            break;
                                    }


                                    continue;
                                    break;
                                    #endregion
                            }
                            #endregion
                            #region PACKET_DEBUGGING/CLEARING
                            if (Settings.LOG_MODULE_CRASH_DUMP)
                            {
                                //entire module packet dump
                                UTILS.AG_ALL_LATEST_TRAFFIC.Push(INC_LOCAL[i]);
                                if (UTILS.AG_ALL_LATEST_TRAFFIC.Count > 20)
                                    UTILS.AG_ALL_LATEST_TRAFFIC.Clear();
                                //indv con packet dump
                                AG_TRAFFIC.Push(INC_LOCAL[i]);
                                if (AG_TRAFFIC.Count > 20)
                                    AG_TRAFFIC.Clear();
                            }
                            #endregion
                            #region GENUINE_SEND
                            REMOTE_SECURITY.Send(INC_LOCAL[i]);
                            #endregion
                        }
                        ASYNC_SEND_TO_MODULE();
                        ASYNC_RECV_FROM_CLIENT();
                    }
                    else
                    {
                        ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                        return;
                    }
                }
                else
                {
                    ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                    return;
                }
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer); UTILS.WriteLine($"AG_ASYNC_RECV_FROM_CLIENT : {EX}"); return; }
        }
        public override async void ASYNC_RECV_FROM_MODULE()
        {
            int LogOpCode = 0;
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
                            LogOpCode = OUT_REMOTE[i].Opcode;

                            #region DIAGNOSTICS_LOG
                            if (Settings.MAIN.checkBox3.Checked)
                            {

                                byte[] packet_bytes = OUT_REMOTE[i].GetBytes();
                                string line = $"{CHARNAME16}[AGENT][REMOTE][S=>P][{OUT_REMOTE[i].Opcode:X4}][{packet_bytes.Length} bytes]{(OUT_REMOTE[i].Encrypted ? "[Encrypted]" : "")}{(OUT_REMOTE[i].Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(packet_bytes)}";
                                if (!Settings.MAIN.textBox18.Text.Equals(""))
                                {
                                    if (SOCKET_IP.Contains(Settings.MAIN.textBox18.Text))
                                    {
                                        UTILS.DiagWriteLine(line);
                                    }
                                }
                                else
                                {
                                    UTILS.DiagWriteLine(line);
                                }

                            }
                            #endregion
                            #region CLIENT_HEADER1 CLIENT_HEADER2 - PROXY CORE
                            if (OUT_REMOTE[i].Opcode == 0x5000 || OUT_REMOTE[i].Opcode == 0x9000)
                            {
                                ASYNC_SEND_TO_MODULE();
                                continue;
                            }
                            #endregion
                            switch (OUT_REMOTE[i].Opcode)
                            {
                                #region SERVER_UNIQUE_ID
                                case 0x3020:
                                    if (OUT_REMOTE[i].GetBytes().Length >= 4)
                                    {
                                        UNIQUE_ID = OUT_REMOTE[i].ReadUInt32();
                                        //UTILS.WriteLine("",$"unqid:{UNIQUE_ID}");
                                    }
                                    break;
                                #endregion
                                #region SERVER_GROUPSPAWN_START
                                case 0x3017:
                                    //groupSpawn.GroupSpawnBegin(OUT_REMOTE[i]);
                                    break;
                                #endregion
                                #region SERVER_GROUPSPAWN_DATA
                                //case 0x3019:
                                //{
                                //    //groupSpawn.Manager(OUT_REMOTE[i]);
                                //    //string Temp = "";
                                //    //foreach (var item in groupSpawn.GroupeSpawned())
                                //    //{
                                //    //    Temp += item + ",";
                                //    //}
                                //}
                                //#region SERVER_GROUPSPAWN_END
                                ////case == 0x3018:
                                ////{


                                ////}
                                ///break;
                                //#endregion
                                #endregion
                                #region SERVER_TELEPORTIMAGE TELEPORTING_PROC
                                case 0x34B5://spawn request
                                            //TELEPORT_START_TIME = DateTime.Now;
                                            //UTILS.TELEPORTING_PROC.Add(CHARNAME16);
                                            //UTILS.WriteLine("'{0}' under teleport", this.CL_CharName);
                                            //canncelled this shitty system, todo...
                                    var RegionID = OUT_REMOTE[i].ReadUInt16();
                                    CUR_REGION = RegionID;
                                    break;
                                #endregion
                                #region SERVER_GS_GAMESERVER_ADDOON
                                case 0x5010:
                                    GameServerMsgHandler GSAddon = new GameServerMsgHandler();
                                    GSAddon.HandleUniqueMonsterDamageInfo(OUT_REMOTE[i],ShardID);
                                    continue;
                                #endregion
                                #region SERVER_FORTRESS_NOTICE
                                    case 0x385F:
                                    byte FUpdateType = OUT_REMOTE[i].ReadUInt8();
                                    if (FUpdateType == 0x2)
                                    {
                                        if (!UTILS.IS_FORTRESS_INITIALIZED)
                                        {
                                            UTILS.IS_FORTRESS_INITIALIZED = true;
                                            Task.Factory.StartNew(async () =>
                                            {
                                                while (UTILS.FW_EVENT_ACTIVE)
                                                {
                                                    await QUERIES.UpdateFWKillsCounter();
                                                    await UTILS.FWKillsUpdate();
                                                    await Task.Delay(10000);
                                                }
                                            });
                                        }
                                    }
                                    if (FUpdateType == 0x6)// ftw end
                                    {
                                        if (FWKillsGui)
                                        {
                                            FWKillsGui = false;
                                            UTILS.FWKills.Clear();
                                            UTILS.FW_EVENT_ACTIVE = false;
                                            UTILS.IS_FORTRESS_INITIALIZED = false;
                                            ShowControl(11448, 0);
                                        }
                                    }
                                    break;
                                #endregion
                                #region - -
                                case 0x34A6:
                                    NickName16 = await QUERIES.Get_NikeName_by_CHARNAME16(CHARNAME16, ShardID);
                                    //string Data = "GM|GM&";
                                    //if (Settings.AppConfig["FeixueXJL"].Service) { 
                                    //    var TempA = Task.Run(()=> {
                                    //        Packet AppConfigInfo = new Packet(0x19A0);
                                    //        AppConfigInfo.WriteUInt32(int.Parse(Settings.AppConfig["NameColorPrice"].Value));
                                    //        AppConfigInfo.WriteUInt32(int.Parse(Settings.AppConfig["CustomTitleColor"].Value));
                                    //        AppConfigInfo.WriteUInt32(int.Parse(Settings.AppConfig["TitlePrice"].Value));
                                    //        AppConfigInfo.WriteUInt8(int.Parse(Settings.AppConfig["NameColorPriceType"].Value));
                                    //        AppConfigInfo.WriteUInt8(int.Parse(Settings.AppConfig["CustomTitleColorType"].Value));
                                    //        AppConfigInfo.WriteUInt8(int.Parse(Settings.AppConfig["TitlePrice"].Value));
                                    //        LOCAL_SECURITY.Send(AppConfigInfo);
                                    //    });
                                    //}
                                    //ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);

                                    //List<CharTitleInfoModel> CharTitleInfo = await QUERIES.Get_CharTitle();
                                    //List<CustomNameColorModel> CustomNameColors = await QUERIES.Get_CharColor();
                                    //List<CustomNameColorModel> CustomTitleColors = await QUERIES.Get_TitleColor();

                                    //var ResData = "";
                                    //try {
                                    //    await UTILS.发送自定义消息(0x19A1, Settings.AppConfig["JobTitle"].Service?"开":"关", 防御通讯);
                                    //    if (Settings.AppConfig["CustomTitleColor"].Service)
                                    //    {
                                    //        //发送称号颜色信息

                                    //        ResData = "";
                                    //        for (int s = 0; s < CustomTitleColors.Count; s++)
                                    //        {
                                    //            ResData += CustomTitleColors[s].Name + "|0x" + CustomTitleColors[s].Color + "&";
                                    //        }
                                    //        if (ResData.Length > 0)
                                    //        {
                                    //            ResData = ResData.Substring(0, ResData.Length - 1);
                                    //            await UTILS.发送自定义消息(0x199D, ResData, 防御通讯);
                                    //        }
                                    //    }
                                    //    if (Settings.AppConfig["NameColorPrice"].Service)
                                    //    {
                                    //        ResData = "";
                                    //        for (int s = 0; s < CustomNameColors.Count; s++)
                                    //        {
                                    //            ResData += CustomNameColors[s].Name + "|0x" + CustomNameColors[s].Color + "&";
                                    //        }
                                    //        if (ResData.Length > 0)
                                    //        {
                                    //            ResData = ResData.Substring(0, ResData.Length - 1);
                                    //            await UTILS.发送自定义消息(0x199C, ResData, 防御通讯);
                                    //        }
                                    //    }
                                    //    if (Settings.AppConfig["TitlePrice"].Service)
                                    //    {
                                    //        ResData = "";
                                    //        if (Settings.AppConfig["JobTitle"].Service)
                                    //        {
                                    //            for (int s = 0; s < CharTitleInfo.Count; s++)
                                    //            {
                                    //                if (!await QUERIES.Get_Jobstate_by_CHARNAME16(CharTitleInfo[s].CharName, 64))
                                    //                {
                                    //                    ResData += CharTitleInfo[s].CharName + "|" + CharTitleInfo[s].Title + "&";
                                    //                }
                                    //            }
                                    //            foreach (var con in ASYNC_SERVER.AG_CONS.Where(x => !string.IsNullOrEmpty(x.Value.CHARNAME16) && x.Value.职业状态))// looping via all conns
                                    //            {
                                    //                ResData += con.Value.NickName16 + "|" + await QUERIES.获取职业称号(con.Value.JobLevel, con.Value.职业类型) + "&";
                                    //            }
                                    //        }
                                    //        else
                                    //        {
                                    //            for (int s = 0; s < CharTitleInfo.Count; s++)
                                    //            {
                                    //                ResData += CharTitleInfo[s].CharName + "|" + CharTitleInfo[s].Title + "&";
                                    //            }
                                    //        }
                                    //        if (ResData.Length > 0) {
                                    //            ResData = ResData.Substring(0, ResData.Length - 1);
                                    //            await UTILS.发送自定义消息(0x199B, ResData, 防御通讯);
                                    //        }


                                    //    }
                                    //    if (Settings.AppConfig["JobTitle"].Service)
                                    //    {
                                    //        if (string.IsNullOrEmpty(CHARNAME16)) CHARNAME16 = CHARNAME16_HOLDER;
                                    //        int[] 职业数据 = await QUERIES.Get_JobType_by_CHARNAME16(CHARNAME16, ShardID);
                                    //        if (职业数据 != null)
                                    //        {
                                    //            int 本次职业类型 = 职业数据[0];
                                    //            JobLevel = 职业数据[1];
                                    //            bool 本次职业状态 = await QUERIES.Get_Jobstate_by_CHARNAME16(CHARNAME16, ShardID);
                                    //            NickName16 = await QUERIES.Get_NikeName_by_CHARNAME16(CHARNAME16, ShardID);
                                    //            //判断职业状态是否改变
                                    //            var TitleData = "";
                                    //            if (职业状态 != 本次职业状态)
                                    //            {
                                    //                if (本次职业状态)
                                    //                {
                                    //                    //职业称号
                                    //                    await UTILS.发送自定义消息(0x199F, CHARNAME16 + "|" + await QUERIES.获取职业称号(JobLevel, 本次职业类型), 防御通讯);
                                    //                    //CharTitlePacket = new Packet(0x199F);
                                    //                    //CharTitlePacket.WriteUnicode(CHARNAME16 + "|" + await QUERIES.获取职业称号(JobLevel, 本次职业类型));
                                    //                    //LOCAL_SECURITY.Send(CharTitlePacket);
                                    //                    //CharTitlePacket = new Packet(0x199F);
                                    //                    //CharTitlePacket.WriteUnicode(NickName16 + "|" + await QUERIES.获取职业称号(JobLevel, 本次职业类型));
                                    //                    TitleData = NickName16 + "|" + await QUERIES.获取职业称号(JobLevel, 本次职业类型);
                                    //                }
                                    //                else
                                    //                {
                                    //                    //非职业
                                    //                    CharTitleInfoModel CharTitle = await QUERIES.Get_CharTitle(CHARNAME16);
                                    //                    if (CharTitle == null)
                                    //                        TitleData = CHARNAME16 + "|NULL";
                                    //                    else
                                    //                        TitleData = CharTitle.CharName + "|" + CharTitle.Title;

                                    //                }
                                    //                foreach (var con in ASYNC_SERVER.AG_CONS.Where(x => !string.IsNullOrEmpty(x.Value.CHARNAME16)))// looping via all conns
                                    //                {
                                    //                    await UTILS.发送自定义消息(0x199F, TitleData, con.Value.防御通讯);
                                    //                }

                                    //            }
                                    //            else if (本次职业状态)
                                    //            {
                                    //                TitleData = CHARNAME16 + "|" + await QUERIES.获取职业称号(JobLevel, 本次职业类型);
                                    //                await UTILS.发送自定义消息(0x199F, TitleData, 防御通讯);
                                    //            }
                                    //            else {
                                    //                CharTitleInfoModel CharTitle = await QUERIES.Get_CharTitle(CHARNAME16);
                                    //                if (CharTitle == null)
                                    //                    TitleData = CHARNAME16 + "|NULL";
                                    //                else
                                    //                    TitleData = CharTitle.CharName + "|" + CharTitle.Title;
                                    //                await UTILS.发送自定义消息(0x199F, TitleData, 防御通讯);
                                    //            }
                                    //            职业类型 = 本次职业类型;
                                    //            职业状态 = 本次职业状态;
                                    //        }
                                    //        else
                                    //        {
                                    //            UTILS.WriteLine($"[{CHARNAME16_HOLDER}]职业数据为空");
                                    //        }
                                    //    }
                                    //} catch (Exception Ex)
                                    //{
                                    //    UTILS.WriteLine($"向玩家[{CHARNAME16}]发送防御数据发生异常:{Ex.ToString()}");
                                    //}
                                    break;
                                #endregion
                                #region SERVER_FRIEND_REQUEST
                                case 0x7302:
                                    if(UTILS.DISABLE_INVITEFRIENDS_REGIONS.Contains(CUR_REGION))
                                        continue;
                                    break;
                                #endregion
                                #region AGENT_COMMUNITY_FRIEND_INFO Spawned IN GAME
                                case 0x3305:
                                    SPAWNED_START_TIME = DateTime.Now;
                                    DEAD_STATUS = false;
                                    ZERKING = false;
                                    JOB_YELLOW_LINE = false;
                                    PVP_FLAG = false;
                                    if (!FIRST_CLIENT_SPAWN_SUCCESS)
                                    {
                                        //RIDING_PET = false;
                                        FIRST_CLIENT_SPAWN_SUCCESS = true;
                                        ///////////////////////
                                        //assigining our cn16 from his holder as soon as we know that the client reoprted succesful spawn
                                        CHARNAME16 = CHARNAME16_HOLDER;
                                        if (Settings.PLAYER_LOGON_MSG)
                                        {
                                            UTILS.WriteLine($"[{CHARNAME16}] Has Spawned.", UTILS.LOG_TYPE.Notify);
                                        }
                                        if (Settings.WELCOME_MSG)
                                        {
                                            Settings.LOGIN_WELCOME_MSG.Replace("{ShardID}", Settings.ShardInfos[ShardID].Name);
                                            UTILS.SendNotice(UTILS.NoticeType.Notify,String.Format(Settings.LOGIN_WELCOME_MSG, CHARNAME16), CLIENT_SOCKET);
                                        }

                                        if (Settings.LOG_PLAYERS_STATUS)
                                        {
                                            if (!await QUERIES.IS_CHARNAME16_Has_Online_Record(CHARNAME16))
                                                await QUERIES.LOG_PLAYER_STATUS(CHARNAME16,
                                                    SOCKET_IP.Split(':')[0],
                                                    CORRESPONDING_GW_SESSION.HWID,
                                                    true, ShardID);
                                            else
                                                await QUERIES.UPDATE_PLAYER_STATUS(CHARNAME16, true);
                                        }

                                        _ = SG_QUERIES.SendAllServerRanksAsync(CLIENT_SOCKET);
                                        _ = SG_QUERIES.GetNextEvent(false, this);
                                        _ = SG_QUERIES.LoadHallOfFameAsync(CHARNAME16, CLIENT_SOCKET);

                                        if (!Settings.MAIN.checkBox33.Checked)
                                            this.UpdateTitlesColor();
                                        if (!Settings.MAIN.checkBox32.Checked)
                                            this.UpdateCustomNameRank();
                                        if (!Settings.MAIN.checkBox31.Checked)
                                            this.UpdateCharNameColor();
                                        if (!Settings.MAIN.checkBox34.Checked)
                                            this.UpdateCharIcon();
                                        if (!Settings.MAIN.checkBox24.Checked)
                                            this.UpdateCustomTitle();
                                        LAST_PLAYER_SEEN = DateTime.Now;
                                        if (UTILS.DailyReward.ContainsKey(CHARNAME16))
                                        {
                                            double dif = (LAST_PLAYER_SEEN - UTILS.DailyReward[CHARNAME16].last_seen).TotalHours;
                                            if (dif > 48)
                                            {
                                                //clear all
                                                await QUERIES.Update_dailyReward(CHARNAME16, 1, false, false, false, false, false);
                                                _DailyReward Loser = new _DailyReward();
                                                UTILS.DailyReward[CHARNAME16] = Loser;
                                            }
                                            else if (dif > 24 && dif < 48)
                                            {
                                                int total = UTILS.DailyReward[CHARNAME16].Total;
                                                total += 1;
                                                await QUERIES.Update_dailyReward(CHARNAME16, total);
                                                UTILS.DailyReward[CHARNAME16].Total = total;
                                                if (total == 5 || total == 10 || total == 15 || total == 20 || total == 25)
                                                    UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox236.Text, CLIENT_SOCKET);
                                                if(total == 26)
                                                {
                                                    await QUERIES.Update_dailyReward(CHARNAME16, 1, false, false, false, false, false);
                                                    _DailyReward Loser = new _DailyReward();
                                                    UTILS.DailyReward[CHARNAME16] = Loser;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _DailyReward newplayer = new _DailyReward();
                                            UTILS.DailyReward.TryAdd(CHARNAME16, newplayer);
                                            await QUERIES.Insert_dailyReward(CHARNAME16, 1, false, false, false, false, false);
                                            //insert at database
                                        }

                                        CUR_REGION = await QUERIES.Get_RegionID_by_CHARNAME16(CHARNAME16_HOLDER, ShardID);

                                        //todo -> make a list for it
                                        CharLockCurPassword = await QUERIES.ReturnCharLockPassword(UserName) ?? null;
                                        if (CharLockCurPassword != null)
                                        {
                                            int isset = await QUERIES.IS_SET_LOCK(UserName);
                                            if (isset != 0)
                                                CharLockStatue = true;
                                            else
                                                CharLockStatue = false;
                                        }

                                    }
                                    if (!Settings.BOT_ALLOW && !NONE_CLIENTLESS_MARKER)
                                    {
                                        UTILS.SEND_INDV_ERR_MSG("UIIT_STT_BOT_ALLOW", CLIENT_SOCKET);
                                        continue;
                                    }
                                    if (Settings.AFK_DETECTION && !string.IsNullOrEmpty(CHARNAME16)) //check if the char has spawned at least once
                                    {
                                        PING_COUNTER = 0;
                                        if (HELPER_MARK_STATUS)
                                        {
                                            await Task.Delay(500);
                                            Packet helpmark = new Packet(0x7402);
                                            helpmark.WriteUInt8(0x00); //remove the mark for good measure...
                                            REMOTE_SECURITY.Send(helpmark);
                                            ASYNC_SEND_TO_MODULE();
                                            //Utils.WriteLine($"0x7402 removed for {CHARNAME16} @ EVERY TP CORE");
                                        }
                                    }
                                    //CAN BE IMPROVED AND OBTAINED VIA PACKETS!!!
                                    short CurrentRegionID = (short)Convert.ToInt32(CUR_REGION);
                                    //Utils.WriteLine($"curReg {CurrentRegionID}, charid: {CharID}!, charname: {CHARNAME16_HOLDER}");

                                    if (CurrentRegionID == 29399 || CurrentRegionID == 29655)
                                        INSIDE_JF = true;
                                    else
                                        INSIDE_JF = false;
                                    if (CurrentRegionID == 25580)
                                        INSIDE_SURV = true;
                                    else
                                        INSIDE_SURV = false;

                                    if (CurrentRegionID == -32748)
                                        INSIDE_DRUNK = true;
                                    else
                                        INSIDE_DRUNK = false;

                                    if (READER.FW_WREGION_ID.Contains(CurrentRegionID))
                                        INSIDE_FW = true;
                                    else INSIDE_FW = false;

                                    if (READER.CTF_WREGION_ID.Contains(CurrentRegionID))
                                        INSIDE_CTF = true;
                                    else INSIDE_CTF = false;

                                    if (READER.FGW_WREGION_ID.Contains(CurrentRegionID))
                                        INSIDE_FGW = true;
                                    else INSIDE_FGW = false;

                                    if (READER.BA_WREGION_ID.Contains(CurrentRegionID))
                                        INSIDE_BA = true;
                                    else INSIDE_BA = false;

                                    if (CurrentRegionID == 24758)
                                        INSIDE_TT = true;
                                    else INSIDE_TT = false;

                                    if (FWKillsGui && !INSIDE_FW)
                                    {
                                        ShowControl(11448, 0);
                                        FWKillsGui = false;
                                    }

                                    if (SURVKillsGui && !INSIDE_SURV)
                                    {
                                        ShowControl(11449, 0);
                                        SURVKillsGui = false;
                                    }
                                    if (await QUERIES.Get_Jobstate_by_CHARNAME16(CHARNAME16, ShardID)) JOB_FLAG = true;
                                    else JOB_FLAG = false;
                                    int[] jobdata = await QUERIES.Get_JobType_by_CHARNAME16(CHARNAME16_HOLDER, ShardID);
                                    if (jobdata != null)
                                    {
                                        JobType = jobdata[0];
                                        JobLevel = jobdata[1];
                                    }
                                    if (Settings.GMS_ANTI_INVIS && INVISIBLE_STATUS && CHARNAME16 != Settings.CL_CharName)// GM anti-invisible
                                    {
                                        Packet overwriteinvis = new Packet(0x7010);
                                        overwriteinvis.WriteUInt16(14);
                                        REMOTE_SECURITY.Send(overwriteinvis);
                                        ASYNC_SEND_TO_MODULE();
                                    }
                                    break;
                                #endregion
                                #region SERVER_AGENT_SILK_UPDATE - FIRST TP CORE
                                case 0x3153:
                                    //this packet is called for the char first tp only!
                                    //packets is called for GM's/Clientless/Characters (100% spawned)
                                    //this packet can only be used as a first time indicator
                                    //because it is called on other events aswell

                                    if (OUT_REMOTE[i].GetBytes().Length == 12)//12 Bytes - updating silk pack
                                    {
                                        int[] balance = await QUERIES.Get_TOT_SILK_BALANCE(CHARNAME16, ShardID);
                                        if (balance != null)
                                        {
                                            Packet SERVER_AGENT_SILK_UPDATE = new Packet(0x3153);//SERVER_SILKPACK
                                            SERVER_AGENT_SILK_UPDATE.WriteUInt32(Convert.ToUInt32(balance[0]));
                                            SERVER_AGENT_SILK_UPDATE.WriteUInt32(Convert.ToUInt32(balance[1]));
                                            SERVER_AGENT_SILK_UPDATE.WriteUInt32(Convert.ToUInt32(balance[2]));
                                            LOCAL_SECURITY.Send(SERVER_AGENT_SILK_UPDATE);
                                            ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                            continue;
                                        }
                                    }
                                    break;
                                #endregion
                                #region SERVER_AGENT_ENTITY_UPDATE_STATE
                                case 0xB50E://occurs in all events but clientless, 100% spawned.
                                    //this snippet will not execute for clientless connections!!!
                                    NONE_CLIENTLESS_MARKER = true;

                                    //if (Settings.GMS_ANTI_INVIS && INVISIBLE_STATUS)// GM anti-invisible
                                    //{
                                    //    Packet overwriteinvis = new Packet(0x7010);
                                    //    overwriteinvis.WriteUInt16(14);
                                    //    REMOTE_SECURITY.Send(overwriteinvis);
                                    //    ASYNC_SEND_TO_MODULE();
                                    //}
                                    break;
                                #endregion
                                #region SERVER_UPDATE_NOTICE
                                case 0x3154:
                                    OUT_REMOTE[i].ReadUInt8();//unknown
                                    byte types = OUT_REMOTE[i].ReadUInt8();//0x00 normal silk, 0x01 points
                                    if (types == 0x01)
                                    {
                                        int[] res = await QUERIES.Get_ADDED_POINTS(CHARNAME16, ShardID);
                                        if (res != null)
                                        {
                                            int points = Convert.ToInt32(res[0]);
                                            int type2 = Convert.ToInt32(res[1]);
                                            if (points < 0)//why?
                                                continue;

                                            Packet SERVER_SILK_UPDATE_NOTIFY = new Packet(0x3154);
                                            SERVER_SILK_UPDATE_NOTIFY.WriteUInt16(type2 == 1 ? 4 : 0);//points;
                                            SERVER_SILK_UPDATE_NOTIFY.WriteInt32(points);

                                            Packet ping = new Packet(0x2002);
                                            LOCAL_SECURITY.Send(SERVER_SILK_UPDATE_NOTIFY);
                                            LOCAL_SECURITY.Send(ping);
                                            await QUERIES.DELETE_SilkChange_BY_Web_Records();
                                            continue;
                                        }
                                    }
                                    break;
                                #endregion
                                #region SERVER_QUESTMARK
                                case 0xB402:
                                    if (OUT_REMOTE[i].GetBytes().Length == 5)
                                    {
                                        uint CURRENT_UNQ_ID = OUT_REMOTE[i].ReadUInt32();
                                        byte status2 = OUT_REMOTE[i].ReadUInt8();
                                        if (status2 == 0x02 && CURRENT_UNQ_ID == UNIQUE_ID)
                                            HELPER_MARK_STATUS = true;
                                        if (status2 == 0x00 && CURRENT_UNQ_ID == UNIQUE_ID)
                                            HELPER_MARK_STATUS = false;
                                    }
                                    break;
                                #endregion
                                #region SERVER_CHAT
                                case 0x3026: //Chat packet
                                    byte Chattype = OUT_REMOTE[i].ReadUInt8();
                                    break;
                                #endregion
                                #region SERVER_ALCHEMY PR_AUTO_NOTICE/BOT
                                case 0xB150:
                                    if (OUT_REMOTE[i].GetBytes().Length > 12)//alchemy attempt
                                        if (Settings.PLUS_REQ_NOTICE != 0 || UTILS.AE_EVENT)
                                        {
                                            OUT_REMOTE[i].ReadUInt16();
                                            OUT_REMOTE[i].ReadUInt8();
                                            byte slot = OUT_REMOTE[i].ReadUInt8();
                                            OUT_REMOTE[i].ReadUInt64();
                                            byte plusvalue = OUT_REMOTE[i].ReadUInt8();//UTILS.WriteLine(plusvalue);      
                                            if (plusvalue >= Settings.PLUS_REQ_NOTICE)//anything from current plus will be noticed.
                                            {
                                                string ItemName_PlusNotice = await QUERIES.Get_ItemRealName_by_Slot(slot,CHARNAME16, ShardID);

                                                int? advresult = await QUERIES.Get_AdvancedElixirValue(slot, CHARNAME16,ShardID);
                                                if (advresult >= 1 && advresult != null)
                                                    UTILS.SendNoticeForAll(UTILS.NoticeType.ColoredSystem, String.Format(Settings.MAIN.textBox235.Text, CHARNAME16, ItemName_PlusNotice, plusvalue + advresult, advresult), uint.Parse("FFFF00", NumberStyles.HexNumber));
                                                else
                                                    UTILS.SendNoticeForAll(UTILS.NoticeType.ColoredSystem, String.Format(Settings.MAIN.textBox234.Text, CHARNAME16, ItemName_PlusNotice, plusvalue), uint.Parse("FFFF00", NumberStyles.HexNumber));
                                            }
                                        }
                                    break;
                                #endregion
                                #region SERVER_PVP_DATA
                                case 0xB516:
                                    if (OUT_REMOTE[i].GetBytes().Length >= 6) //applying pvp flag(cape) packet
                                    {
                                        OUT_REMOTE[i].ReadUInt32();
                                        OUT_REMOTE[i].ReadUInt8();
                                        byte flagtype = OUT_REMOTE[i].ReadUInt8();
                                        if (flagtype == 1 || flagtype == 2 || flagtype == 3 || flagtype == 4 || flagtype == 5)
                                            PVP_FLAG = true;

                                        if (flagtype == 0)
                                            PVP_FLAG = false;
                                    }
                                    break;
                                #endregion
                                #region SERVER_PLAYER_LEVELUP_EFFECT
                                case 0x3054:
                                    uint PlayerUniqueID = OUT_REMOTE[i].ReadUInt32();
                                    if (PlayerUniqueID == UNIQUE_ID)
                                    {
                                        CUR_LEVEL += 1;
                                        //Utils.WriteLine($"{CHARNAME16} has leveled up.", Utils.LOG_TYPE.Special);
                                    }
                                    break;
                                #endregion
                                #region SERVER_PLAYERDATA
                                case 0x3013:
                                    if (OUT_REMOTE[i].GetBytes().Length >= 59)
                                    {
                                        OUT_REMOTE[i].ReadUInt32(); //ServerTime
                                        ObjCharID = OUT_REMOTE[i].ReadUInt32();//RefObjID
                                        if (ObjCharID < 14875)
                                            ISCHIN = true;
                                        else
                                            ISEURO = true;
                                        OUT_REMOTE[i].ReadUInt8(); //Scale
                                        CUR_LEVEL = OUT_REMOTE[i].ReadUInt8(); //curlvl
                                        OUT_REMOTE[i].ReadUInt8(); //maxlvl
                                        OUT_REMOTE[i].ReadUInt64(); //Expoffset
                                        OUT_REMOTE[i].ReadUInt32();//SExpoffset 
                                        OUT_REMOTE[i].ReadUInt64(); //RemainGold
                                        OUT_REMOTE[i].ReadUInt32(); //Remain Skill point
                                        OUT_REMOTE[i].ReadUInt16(); //Remain Stat point
                                        OUT_REMOTE[i].ReadUInt8(); //Remain zerk point
                                        OUT_REMOTE[i].ReadUInt32(); //GatheredExpPoint
                                        OUT_REMOTE[i].ReadUInt32(); //hp
                                        OUT_REMOTE[i].ReadUInt32(); //mp
                                        byte res = OUT_REMOTE[i].ReadUInt8(); //AutoInvestEXP 51 bytes till here
                                        if (res == 0x02 || res == 0x03)
                                            HELPER_MARK_STATUS = true;
                                        else
                                            HELPER_MARK_STATUS = false;
                                        OUT_REMOTE[i].ReadUInt8(); //DailyPK
                                        OUT_REMOTE[i].ReadUInt8(); //TotalPK
                                        OUT_REMOTE[i].ReadUInt32(); //PkPenatlyPoint
                                        OUT_REMOTE[i].ReadUInt8(); //HwanLevel
                                        OUT_REMOTE[i].ReadUInt8(); //PVPCape

                                    }
                                    break;
                                #endregion
                                #region SERVER_ACTION_DATA
                                case 0xB070:
                                    /* if (OUT_REMOTE[i].ReadUInt8() == 0x01)
                                     {
                                         if (OUT_REMOTE[i].ReadUInt8() == 0x02)
                                         {
                                             if (OUT_REMOTE[i].ReadUInt8() == 0x30)
                                             {
                                                 uint skill_id = OUT_REMOTE[i].ReadUInt32();
                                                 uint attacker_id = OUT_REMOTE[i].ReadUInt32();

                                                 //skill casted ?
                                                 OUT_REMOTE[i].ReadUInt32();
                                                 uint target_uniqueid = OUT_REMOTE[i].ReadUInt32();
                                                 //UTILS.WriteLine("", $"SkillID:{skill_id}, TargetUniqueID:{target_uniqueid} ");
                                                 string Basic_Code = await QUERIES.Get_BasicCode_by_SkillID((int)skill_id);
                                                 //UTILS.WriteLine("", Basic_Code);
                                                 foreach (string unq_name in UTILS.Available_Uniques)
                                                     if (Basic_Code.Contains(unq_name))
                                                     {
                                                         UTILS.WriteLine("", $"Unique Monster Skill Detected !!!");
                                                     }
                                             }
                                         }
                                     }*/
                                    break;
                                #endregion
                                #region SERVER_CHANGE_STATUS
                                case 0x30BF:
                                    if (OUT_REMOTE[i].GetBytes().Length == 6)//zerking response off
                                    {
                                        uint UniqueID = OUT_REMOTE[i].ReadUInt32();
                                        byte StateType = OUT_REMOTE[i].ReadUInt8();
                                        byte State = OUT_REMOTE[i].ReadUInt8();
                                        if (StateType == 0x04 && State == 0x00)
                                        {
                                            ZERKING = false;
                                        }
                                        if (StateType == 0x04 && State == 0x04)//detected invisibilty state! (gm)
                                        {
                                            INVISIBLE_STATUS = true;
                                        }
                                    }
                                    if (OUT_REMOTE[i].GetBytes().Length == 7)//zerking response on
                                    {
                                        uint UniqueID = OUT_REMOTE[i].ReadUInt32();
                                        byte StateType = OUT_REMOTE[i].ReadUInt8();
                                        byte State = OUT_REMOTE[i].ReadUInt8();
                                        byte IsEnhanced = OUT_REMOTE[i].ReadUInt8();//20% dmg buff while blue zerking,was IsEnhcanced == 0x00
                                        if (StateType == 0x04 && State == 0x01)
                                        {
                                            ZERKING = true;
                                        }
                                    }
                                    break;
                                #endregion
                                #region SERVER_COS_INFO
                                case 0x30C8:
                                    uint uid = OUT_REMOTE[i].ReadUInt32(); //Unique ID
                                    uint refID = OUT_REMOTE[i].ReadUInt32(); //Model ID, can skip since the spawn packet comes first.
                                    OUT_REMOTE[i].ReadUInt32(); // MaxHP?
                                    OUT_REMOTE[i].ReadUInt32(); // MaxHP?
                                    int[] Jobpet = await QUERIES.Get_ItemID_Data_ByItemID(CHARNAME16, ShardID, (int)refID);
                                    if(Jobpet[3] == 2)
                                        HAS_JOB_PET = true;
                                    break;
                                #endregion
                                #region SERVER_SPEED_UPDATE
                                case 0x30D0:
                                    //UTILS.WriteLine("", "speed");
                                    //if (DateTime.Now.Subtract(LAST_ATTEMPT_TO_RIDE).TotalSeconds <= 5)
                                    //{
                                    //    UTILS.SendNotice(UTILS.NoticeType.Notify,"Speed blocked", CLIENT_SOCKET);
                                    //    continue;
                                    //}
                                    //if (ZERKING && RIDING_PET)
                                    //{
                                    //    UTILS.SendNotice(UTILS.NoticeType.Notify,"Speed blocked2", CLIENT_SOCKET);
                                    //    continue;
                                    //}
                                    break;
                                #endregion
                                #region AGENT_GUILD_INFO_DATA
                                case 0x3101:
                                    OUT_REMOTE[i].ReadUInt32();
                                    this.GUILDNAME = OUT_REMOTE[i].ReadAscii();
                                    break;
                                #endregion
                                #region AGENT_COS_UPDATE_RIDESTATE
                                case 0xB0CB:
                                    if (OUT_REMOTE[i].GetBytes().Length == 10)
                                        OUT_REMOTE[i].ReadUInt32();
                                     OUT_REMOTE[i].ReadUInt8();
                                    byte status = OUT_REMOTE[i].ReadUInt8();
                                    if (status == 0x01)
                                    {
                                        RIDING_PET = true;
                                        RIDING_PET_UNIQUEID = (int)OUT_REMOTE[i].ReadUInt32();
                                        //UTILS.WriteLine("riding");
                                    }
                                    else if (status == 0x00)
                                    {
                                        RIDING_PET = false;
                                        //UTILS.WriteLine("not riding");
                                    }
                                    break;
                                #endregion
                                #region SERVER_SKILL_DATA
                                case 0xB071:// && OUT_REMOTE[i].GetBytes().Length >= 19
                                            //continue;
                                            //OUT_REMOTE[i].ReadUInt8();
                                            //OUT_REMOTE[i].ReadUInt8();
                                            //OUT_REMOTE[i].ReadUInt8();
                                            //uint SkillID = OUT_REMOTE[i].ReadUInt32();
                                            //uint CasterID = OUT_REMOTE[i].ReadUInt32();
                                            //OUT_REMOTE[i].ReadUInt32();
                                            //uint TargetID = OUT_REMOTE[i].ReadUInt32();
                                            //UTILS.WriteLine("UnqTarget", string.Format("SkillID={0}, CasterID={1}, TargetID={2}", SkillID.ToString("X"), CasterID.ToString(), TargetID.ToString()));
                                    break;
                                #endregion
                                #region SERVER_SKILL_EFFECTS
                                case 0x3057://11bytes 
                                    uint Uinque_ID = OUT_REMOTE[i].ReadUInt32();
                                    if (Uinque_ID == UNIQUE_ID)
                                    {
                                        OUT_REMOTE[i].ReadUInt8();
                                        OUT_REMOTE[i].ReadUInt8();
                                        byte typeofpddate = OUT_REMOTE[i].ReadUInt8();

                                        switch (typeofpddate)
                                        {
                                            case 0x01:
                                                CURRENT_HP = OUT_REMOTE[i].ReadUInt32(); //Current hp
                                                break;
                                            case 0x02:
                                                OUT_REMOTE[i].ReadUInt32(); //Current mp
                                                break;
                                            case 0x03:
                                                CURRENT_HP = OUT_REMOTE[i].ReadUInt32(); //Current hp
                                                OUT_REMOTE[i].ReadUInt32(); //Current mp
                                                break;
                                            case 0x04:
                                                if (OUT_REMOTE[i].ReadUInt32() == 0)
                                                {
                                                    //out of bad status
                                                }
                                                else
                                                {
                                                    //bad status!
                                                }
                                                break;
                                        }
                                        if (CURRENT_HP > 0)
                                            DEAD_STATUS = false;
                                        else DEAD_STATUS = true;
                                    }
                                    break;
                                #endregion
                                #region SERVER_CHARACTER_DIED
                                case 0x3011:
                                    DEAD_STATUS = true;
                                    if (INSIDE_DRUNK && UTILS.DRUNK_ACTIVE)
                                    {
                                        LifeStats();
                                        UTILS.DRUNKregisterActivelist.RemoveAll(e => e.CHARNAME16 == this.CHARNAME16);
                                    }
                                    break;
                                #endregion
                                #region SERVER_MOVE_RESPONSE
                                case 0xB021:
                                    if (OUT_REMOTE[i].GetBytes().Length >= 7)
                                        // uint Target =; // Unique CL_ID from player
                                        if (OUT_REMOTE[i].ReadUInt32() == UNIQUE_ID)//Check if target == this.unique_ID
                                        {
                                            byte click = OUT_REMOTE[i].ReadUInt8();
                                            short Region = OUT_REMOTE[i].ReadInt16();

                                            if (OUT_REMOTE[i].GetBytes().Length == 24) // Make sure region is 5 numbers.
                                                CUR_REGION = Region;
                                            //UTILS.WriteL(ine("Current bytes: " + OUT_REMOTE[i].GetBytes().Length);
                                            //UTILS.WriteLine("Current region: " + Region);
                                        }
                                    break;
                                #endregion
                                #region SERVER_PARTY_INFORMATION
                                case 0x3864:
                                    if (OUT_REMOTE[i].GetBytes().Length >= 5)
                                    {
                                        byte value = OUT_REMOTE[i].ReadUInt8();
                                        if (value == 1)
                                        {
                                            // pt dismissed
                                            INSIDE_PT = false; //reg
                                            PartyID = 0;
                                            PartyMembers.Clear();
                                        }
                                        else if (value == 2)//another player has accepted your pt invite
                                        {
                                            OUT_REMOTE[i].ReadUInt8();
                                            uint PlayerJID = OUT_REMOTE[i].ReadUInt32();
                                            if (!PartyMembers.Contains(PlayerJID))
                                                PartyMembers.Add(PlayerJID);
                                        }
                                        else if (value == 3)
                                        {
                                            uint PT_Member_JID = OUT_REMOTE[i].ReadUInt32();//the target of left/banned
                                            if (PT_Member_JID == (uint)JID)//checking if its the current client
                                            {
                                                INSIDE_PT = false;
                                                PartyMembers.Clear();
                                            }
                                            else
                                                PartyMembers.Remove(PT_Member_JID);
                                        }
                                    }
                                    break;
                                #endregion
                                #region PARTY_INFO
                                case 0x3065:
                                    INSIDE_PT = true;
                                    PartyMembers.Clear();
                                    OUT_REMOTE[i].ReadUInt32();
                                    OUT_REMOTE[i].ReadUInt32();
                                    OUT_REMOTE[i].ReadUInt8();
                                    OUT_REMOTE[i].ReadUInt8();
                                    byte playerCount = (byte)OUT_REMOTE[i].ReadUInt8();
                                    for (int j = 0; j < playerCount; j++)
                                    {
                                        OUT_REMOTE[i].ReadUInt8();
                                        uint JID = OUT_REMOTE[i].ReadUInt32();
                                        string PlayerName = OUT_REMOTE[i].ReadAscii(); //name
                                        OUT_REMOTE[i].ReadUInt32();
                                        OUT_REMOTE[i].ReadUInt8(); //level
                                        OUT_REMOTE[i].ReadUInt8();
                                        ushort region = OUT_REMOTE[i].ReadUInt16(); //region
                                        int x, y, z;
                                        if (region > short.MaxValue)
                                        {
                                            x = OUT_REMOTE[i].ReadInt32();
                                            y = OUT_REMOTE[i].ReadInt32();
                                            z = OUT_REMOTE[i].ReadInt32();
                                        }
                                        else
                                        {
                                            x = (int)OUT_REMOTE[i].ReadUInt16();
                                            y = (int)OUT_REMOTE[i].ReadUInt16();
                                            z = (int)OUT_REMOTE[i].ReadUInt16();
                                        }
                                        OUT_REMOTE[i].ReadUInt8();
                                        OUT_REMOTE[i].ReadUInt8();
                                        OUT_REMOTE[i].ReadUInt8();
                                        OUT_REMOTE[i].ReadUInt8();
                                        OUT_REMOTE[i].ReadAscii();
                                        OUT_REMOTE[i].ReadUInt8();
                                        OUT_REMOTE[i].ReadUInt32();
                                        OUT_REMOTE[i].ReadUInt32();
                                        if (JID != this.JID)
                                        {
                                            if (!PartyMembers.Contains(JID))
                                                PartyMembers.Add(JID);
                                        }
                                    }
                                    break;
                                #endregion
                                #region SERVER_UNIQUE_ANNOUNCE
                                case 0x300C:
                                    try
                                    {
                                        int event_type = OUT_REMOTE[i].ReadInt8();
                                        OUT_REMOTE[i].ReadInt8();
                                        int s_MOBID = OUT_REMOTE[i].ReadInt32();

                                        SG_QUERIES.RequestUpdateUniqueHistory();
                                        //var BlueTeamTower = Utils.BlueTeamTowerID.Contains((uint)s_MOBID);
                                        //var RedTeamTower = Utils.RedTeamTowerID.Contains((uint)s_MOBID);
                                        //var GreenTeamTower = Utils.GreenTeamTowerID.Contains((uint)s_MOBID);
                                        //var NonTeamTower = Utils.NonTeamTowerID.Contains((uint)s_MOBID);
                                        //if (BlueTeamTower || RedTeamTower || GreenTeamTower || NonTeamTower)
                                        //{
                                        //    continue;
                                        //}

                                        /// Unique Notice

                                        var Notification = UTILS.NotificationList.FirstOrDefault(x => x.UniqueID == s_MOBID);
                                        if (Notification != null)
                                        {
                                            string UniqueName = Notification.UniqueName;//Utils.Available_ObjectNames[Utils.Available_Objects[s_MOBID].CodeName128];

                                            if (event_type == 0x05)
                                            {
                                                

                                                //if (BlueTeamTower || RedTeamTower || GreenTeamTower || NonTeamTower)
                                                //{
                                                //    continue;
                                                //}

                                                UTILS.MonsterUniqueAppeared(s_MOBID, CLIENT_SOCKET);

                                                
                                                ///if (Settings.DISCORD_ENABLE)
                                                ///{
                                                ///    if (!Utils.UniquesSpwanList.Contains(UniqueName))
                                                ///    {
                                                ///        Utils.UniquesSpwanList.Add(UniqueName);
                                                ///        DiscordAPI.SendMessage(Settings.SPAWN_CHANNELID, $"[{UniqueName}]: has appeared", "monsterinsert");
                                                ///        UpdateUniqueGUI();
                                                ///    }
                                                ///}

                                                continue;
                                            }
                                            else if (event_type == 0x06)
                                            {
                                                int discordkillvalue;
                                                string KillerName = OUT_REMOTE[i].ReadAscii();
                                                var playerSocket = ASYNC_SERVER.AG_CONS.FirstOrDefault(x => x.Value.CHARNAME16 == KillerName).Value;
                                                if (playerSocket != null && CHARNAME16 == KillerName)
                                                {
                                                    playerSocket.MonstersKillCount++;
                                                    DateTime TimeNow = DateTime.Now;
                                                    if (TimeNow > playerSocket.LastMonstersKill && playerSocket.MonstersKillCount > 1)
                                                    {
                                                        playerSocket.MonstersKillCount = 1;
                                                        discordkillvalue = 6;
                                                        UTILS.MonsterUniqueKilled(6, s_MOBID, (int)playerSocket.ObjCharID, KillerName);
                                                    }
                                                    else if (TimeNow > playerSocket.LastMonstersKill && playerSocket.MonstersKillCount == 1)
                                                    {
                                                        discordkillvalue = 1;
                                                        UTILS.MonsterUniqueKilled(1, s_MOBID, (int)playerSocket.ObjCharID, KillerName);
                                                    }
                                                    else
                                                    {
                                                        if (playerSocket.MonstersKillCount > 5)
                                                        {
                                                            discordkillvalue = 5;
                                                            playerSocket.MonstersKillCount = 5;
                                                        }
                                                        discordkillvalue = playerSocket.MonstersKillCount;
                                                        UTILS.MonsterUniqueKilled(playerSocket.MonstersKillCount, s_MOBID, (int)playerSocket.ObjCharID, KillerName);
                                                    }

                                                    playerSocket.LastMonstersKill = DateTime.Now.AddSeconds(600);
                                                    //if (Settings.DISCORD_ENABLE)
                                                    //{
                                                    //    DiscordAPI.SendKillMessage(Settings.KILL_CHANNELID, KillerName, UniqueName, discordkillvalue);
                                                    //    UpdateUniqueGUI();
                                                    //    if (UTILS.UniquesSpwanList.Contains(UniqueName))
                                                    //    {
                                                    //        UTILS.UniquesSpwanList.Remove(UniqueName);
                                                    //    }
                                                    //}
                                                }
                                                else
                                                {
                                                    continue;
                                                }



                                                continue;
                                            }
                                        }
                                    }
                                    catch { }
                                    ;
                                    break;
                                #endregion
                                #region SERVER_LEAVE_SUCCESS
                                //if (OUT_REMOTE[i].Opcode == 0x300A)//CLIENT HITS RR BUTTON
                                //{
                                //    //if (Settings.RESTART_CHAR_SELECTION)
                                //    //{
                                //    //    CLIENT_RR = true;//RESTART_CHARSLECTION REG
                                //    //}
                                //}
                                #endregion
                                #region SERVER_GACHA_PLAY
                                case 0xB118://client plays gacha
                                    if (Settings.LOG_MAGIC_POP_PLAY)
                                    {
                                        OUT_REMOTE[i].ReadUInt8();
                                        byte winlose = OUT_REMOTE[i].ReadUInt8();
                                        if (winlose == 0x00)
                                        { await QUERIES.LOG_MAGICPOP_STATS("_MagicPop_Stats", CHARNAME16, "Lost"); }
                                        if (winlose == 0x01)
                                        { await QUERIES.LOG_MAGICPOP_STATS("_MagicPop_Stats", CHARNAME16, "Won"); }
                                    }
                                    break;
                                #endregion
                                #region SERVER_FORMED_PARTY_CREATED
                                case 0xB069:
                                    if(OUT_REMOTE[i].GetBytes().Length >= 5)
                                    {
                                        OUT_REMOTE[i].ReadInt8();
                                        PartyID = OUT_REMOTE[i].ReadInt32();
                                        LeaderType = OUT_REMOTE[i].ReadUInt32(); // Type
                                        LeaderRace = OUT_REMOTE[i].ReadUInt8(); // Race
                                        LeaderPurpose = OUT_REMOTE[i].ReadUInt8(); // Purpose of party
                                        LeaderEntryLevel = OUT_REMOTE[i].ReadUInt8(); // Enter level
                                        LeaderMaxLevel = OUT_REMOTE[i].ReadUInt8(); // Max level
                                        LeaderTitle = OUT_REMOTE[i].ReadAscii(); // Title
                                        UTILS.PT_NR_RECORD = PartyID;
                                    }
                                    break;
                                #endregion
                                #region SERVER_DELETE_FORMED_PARTY
                                case 0xB06B:
                                    if (OUT_REMOTE[i].ReadUInt8() == 1) {
                                        PartyID = 0;
                                    }
                                    break;
                                #endregion
                                #region SERVER_PARTY_REQUEST
                                case 0x706D:
                                    if (!Settings.MAIN.checkBox42.Checked)
                                        break;
                                    uint requesterID = OUT_REMOTE[i].ReadUInt32();
                                    uint requestModel = OUT_REMOTE[i].ReadUInt32();
                                    uint partyNumber = OUT_REMOTE[i].ReadUInt32();
                                    uint randomNumber = OUT_REMOTE[i].ReadUInt32();
                                    uint randomNumber2 = OUT_REMOTE[i].ReadUInt32();
                                    byte flag = OUT_REMOTE[i].ReadUInt8();
                                    byte flag2 = OUT_REMOTE[i].ReadUInt8();
                                    uint uniqueID = OUT_REMOTE[i].ReadUInt32();
                                    string charname = UTILS.INJECTION_PREFIX(OUT_REMOTE[i].ReadAscii());
                                    uint race = OUT_REMOTE[i].ReadUInt32();
                                    var x3 = ASYNC_SERVER.AG_CONS.Where<KeyValuePair<Socket, AGENT_MODULE>>((Func<KeyValuePair<Socket, AGENT_MODULE>, bool>)(x => x.Value.CHARNAME16 == charname)).FirstOrDefault<KeyValuePair<Socket, AGENT_MODULE>>();
                                    if ((this.ISCHIN && race > 1933 || !this.ISCHIN && race < 1933))
                                    {
                                        UTILS.SendNotice(UTILS.NoticeType.Notify, Settings.MAIN.textBox175.Text, x3.Value.CLIENT_SOCKET);
                                        continue;
                                    }
                                    break;
                                #endregion
                                #region SERVER_GAME_GUIDE
                                case 0xB0EA://svr game guide send
                                    if (Settings.GAME_GUIDE_DISABLE)
                                    {
                                        continue;
                                    }
                                    break;
                                #endregion
                                #region SEND_CLIENT_SETTINGS
                                case 0xB007:
                                    Packet GUI = new Packet(0x182A);
                                    GUI.WriteUInt8(Convert.ToByte(Settings.MAIN.JobTitle.Checked));
                                    GUI.WriteUInt8(Convert.ToByte(Settings.MAIN.checkBox11.Checked));
                                    GUI.WriteUInt8(Convert.ToByte(Settings.MAIN.checkBox12.Checked));
                                    GUI.WriteUInt8(Convert.ToByte(Settings.MAIN.checkBox16.Checked));
                                    GUI.WriteUInt8(Convert.ToByte(Settings.MAIN.checkBox15.Checked));
                                    GUI.WriteUInt8(Convert.ToByte(Settings.MAIN.checkBox13.Checked));
                                    GUI.WriteUInt8(Convert.ToByte(Settings.MAIN.checkBox24.Checked));
                                    GUI.WriteUInt8(Convert.ToByte(Settings.MAIN.checkBox20.Checked));
                                    GUI.WriteUInt8(Convert.ToByte(Settings.MAIN.checkBox17.Checked));
                                    GUI.WriteUInt8(Convert.ToByte(Settings.MAIN.checkBox7.Checked));
                                    GUI.WriteInt32(Convert.ToInt32(Settings.MAIN.numericUpDown27.Value));
                                    GUI.WriteAscii(Settings.MAIN.textBox113.Text);
                                    GUI.WriteAscii(Settings.MAIN.textBox114.Text);
                                    GUI.WriteAscii(Settings.MAIN.textBox115.Text);
                                    GUI.WriteInt32(Convert.ToInt32(Settings.MAIN.textBox82.Text));
                                    GUI.WriteUInt8(Convert.ToByte(Settings.MAIN.checkBox28.Checked));
                                    if (Settings.MAIN.checkBox28.Checked)
                                        GUI.WriteInt64(Convert.ToInt64(Settings.MAIN.textBox116.Text));
                                    LOCAL_SECURITY.Send(GUI);
                                    ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                                    break;
                                #endregion
                                #region SERVER_STORAGE_CLOSED
                                case 0xB04B:
                                    STORAGE_OPENED = false;
                                    break;
                                #endregion
                                #region SERVER_INVITATION_REQUEST
                                case 0x3080:
                                    byte type5 = OUT_REMOTE[i].ReadUInt8();
                                    uint uniqueid = OUT_REMOTE[i].ReadUInt32();
                                    switch (type5)
                                    {
                                        case 0x01:
                                            if(JOB_FLAG)
                                            {
                                                continue;
                                            }
                                            break;
                                    }
                                    break;
                                #endregion
                                #region SINGLE_SPAWN
                                case 0x3015:
                                    try
                                    {
                                        /// Mob Or Unique Spwen
                                        uint RefObjID = OUT_REMOTE[i].ReadUInt32(); /// Server Item Drop Veldora
                                        if (UTILS.Available_Objects.ContainsKey((int)RefObjID))
                                        {
                                            ItemType Object = UTILS.Available_Objects[(int)RefObjID];
                                            //Utils.WriteLine($"InvItem:[{Object.TypeID1},{Object.TypeID2},{Object.TypeID3},{Object.TypeID4}]item_id:{RefObjID}, type:{Object.CodeName128}");
                                            if (Object.TypeID1 == 1)
                                            {
                                                //if (RefObjID >= 60303845 || RefObjID <= 60303849)
                                                //{
                                                //    Utils.WriteLine($"WWW {RefObjID} // Object.TypeID1 :{Object.TypeID1} // Object.TypeID2 :{Object.TypeID2}// Object.TypeID3 :{Object.TypeID3}");
                                                //
                                                //}

                                                if (Object.TypeID2 == 1) //player
                                                {
                                                    //Char info
                                                    byte Scale = OUT_REMOTE[i].ReadUInt8();
                                                    byte HwanLevel = OUT_REMOTE[i].ReadUInt8();
                                                    byte PVPCape = OUT_REMOTE[i].ReadUInt8();
                                                    byte AutoInverstExp = OUT_REMOTE[i].ReadUInt8();



                                                    //Char Inventory
                                                    byte Inventory_Size = OUT_REMOTE[i].ReadUInt8();
                                                    byte Inventory_ItemCount = OUT_REMOTE[i].ReadUInt8();
                                                    for (int iic = 0; iic < Inventory_ItemCount; iic++)
                                                    {
                                                        uint Item_RefObjID = OUT_REMOTE[i].ReadUInt32();
                                                        ItemType Item = UTILS.Available_Objects[(int)Item_RefObjID];
                                                        if (Item.TypeID1 == 3 && Item.TypeID2 == 1)
                                                        {
                                                            byte ItemOptLevel = OUT_REMOTE[i].ReadUInt8();
                                                        }
                                                    }
                                                    //AvatarInventory
                                                    byte AvaInv_Size = OUT_REMOTE[i].ReadUInt8();
                                                    byte AvaInv_ItemCount = OUT_REMOTE[i].ReadUInt8();
                                                    for (int aii = 0; aii < AvaInv_ItemCount; aii++)
                                                    {
                                                        uint AvaInvItem_RefObjID = OUT_REMOTE[i].ReadUInt32();
                                                        ItemType AvaInvItem = UTILS.Available_Objects[(int)AvaInvItem_RefObjID];
                                                        if (AvaInvItem.TypeID1 == 3 && AvaInvItem.TypeID2 == 1)
                                                        {
                                                            byte ItemOptLevel = OUT_REMOTE[i].ReadUInt8();
                                                        }
                                                    }
                                                    byte Mask = OUT_REMOTE[i].ReadUInt8();
                                                    if (Mask == 0x01)
                                                    {
                                                        uint Mask_RefObjID = OUT_REMOTE[i].ReadUInt32();
                                                        ItemType MaskObject = UTILS.Available_Objects[(int)Mask_RefObjID];
                                                        if (MaskObject.CodeName128.StartsWith("CHAR"))
                                                        {
                                                            byte MaskScale = OUT_REMOTE[i].ReadUInt8();
                                                            byte MaskCount = OUT_REMOTE[i].ReadUInt8();
                                                            for (int mc = 0; mc < MaskCount; mc++)
                                                            {
                                                                uint RefItemID = OUT_REMOTE[i].ReadUInt32();
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (Object.TypeID2 == 2 && Object.TypeID3 == 5)
                                                {
                                                    //NPC_FORTRESS_STRUCT
                                                    uint StructHP = OUT_REMOTE[i].ReadUInt32();
                                                    uint RefEventStructID = OUT_REMOTE[i].ReadUInt32();
                                                    ushort StructState = OUT_REMOTE[i].ReadUInt16();
                                                }
                                                else if (Object.TypeID2 == 2 && Object.TypeID3 == 1)
                                                {
                                                    uint UniqueID2 = OUT_REMOTE[i].ReadUInt32();

                                                    UTILS.MobInfo newMobInfo = new UTILS.MobInfo();
                                                    newMobInfo.MobID = RefObjID;
                                                    newMobInfo.MobGameID = UniqueID2;
                                                    UTILS.mobList.Add(newMobInfo);

                                                   // var BlueTeamTower = UTILS.BlueTeamTowerID.Contains(RefObjID);
                                                   // var RedTeamTower = UTILS.RedTeamTowerID.Contains(RefObjID);
                                                   // var GreenTeamTower = UTILS.GreenTeamTowerID.Contains(RefObjID);
                                                   //
                                                   // if (RedTeamTower)
                                                   // {
                                                   //     UTILS.RedTeamTowerGame.Add(UniqueID2);
                                                   // }
                                                   //
                                                   // if (BlueTeamTower)
                                                   // {
                                                   //     UTILS.BlueTeamTowerGame.Add(UniqueID2);
                                                   // }
                                                   //
                                                   // if (GreenTeamTower)
                                                   // {
                                                   //     UTILS.GreenTeamTowerGame.Add(UniqueID2);
                                                   // }



                                                    //if (!Utils.spawend_uniques_in_server.ContainsKey(UniqueID2))
                                                    //{
                                                    //    Utils.spawend_uniques_in_server.Add(UniqueID2, RefObjID);
                                                    //    Utils.mobList.Add(new Utils.MobInfo { MobID = RefObjID, MobGameID = UniqueID2 });
                                                    //
                                                    //}


                                                    //Utils.WriteLine($"{RefObjID} Single Spawn => UniqueID:{UniqueID2}");

                                                }
                                                uint UniqueID1 = OUT_REMOTE[i].ReadUInt32();
                                                //CharacterVicinity.Add(UniqueID1);

                                                //Position
                                                byte xsec = OUT_REMOTE[i].ReadUInt8();
                                                byte ysec = OUT_REMOTE[i].ReadUInt8();
                                                float xcoord = OUT_REMOTE[i].ReadSingle();
                                                float ycoord = OUT_REMOTE[i].ReadSingle();
                                                float zcoord = OUT_REMOTE[i].ReadSingle();
                                                ushort Angle = OUT_REMOTE[i].ReadUInt16();

                                                //We are performing a union of the two sectors(X,Y) to get the RegionID
                                                //UshortArray cur_RegionID;
                                                //cur_RegionID.Int1 = 0;
                                                //cur_RegionID.Byte1 = xsec;
                                                //cur_RegionID.Byte2 = ysec;
                                                //Utils.WriteLine($"{RefObjID} Single Spawn => UniqueID:{UniqueID1}, RegionID:{cur_RegionID.Int1}, X:{Utils.TO_GAME_X(xcoord, xsec)},Y:{Utils.TO_GAME_Y(ycoord, ysec)}");

                                                //Movement
                                                byte HasDestination = OUT_REMOTE[i].ReadUInt8();
                                                byte Type = OUT_REMOTE[i].ReadUInt8();



                                               //if (HasDestination == 1)
                                               //{
                                               //    ushort MovementDestinationRegion = OUT_REMOTE[i].ReadUInt16();
                                               //    if (cur_RegionID.Int1 < short.MaxValue)
                                               //    {
                                               //        //World
                                               //        ushort DestX = OUT_REMOTE[i].ReadUInt16();
                                               //        ushort DestY = OUT_REMOTE[i].ReadUInt16();
                                               //        ushort DestZ = OUT_REMOTE[i].ReadUInt16();
                                               //    }
                                               //    else
                                               //    {
                                               //        //Dungeon
                                               //        uint DestX = OUT_REMOTE[i].ReadUInt32();
                                               //        uint DestY = OUT_REMOTE[i].ReadUInt32();
                                               //        uint DestZ = OUT_REMOTE[i].ReadUInt32();
                                               //    }
                                               //}
                                               //else
                                               //{
                                               //    byte MovementSource = OUT_REMOTE[i].ReadUInt8();//0 = Spinning, 1 = Sky-/Key-walking
                                               //    ushort MovementAngle = OUT_REMOTE[i].ReadUInt16();//Represents the new angle, character is looking at
                                               //}
                                               //
                                            }
                                            //else if (Object.TypeID1 == 3)
                                            //{

                                            //    if (Object.TypeID2==1) //isEquipable
                                            //    {
                                            //        Byte Plus = OUT_REMOTE[i].ReadUInt8();
                                            //        //Utils.WriteLine($"Plus : {Plus}");
                                            //    }
                                            //    if (Object.TypeID4 == 0) //isgold
                                            //    {
                                            //        uint Golddrop = OUT_REMOTE[i].ReadUInt32();
                                            //        //Utils.WriteLine($"Golddrop : {Golddrop}");
                                            //    }    
                                            //    if (Object.TypeID3 == 9 || Object.TypeID3 == 8) //isquest || trade good
                                            //    {
                                            //        string OwnerName = OUT_REMOTE[i].ReadAscii();
                                            //        //Utils.WriteLine($"OwnerName : {OwnerName}");
                                            //    }    
                                            //    // Position
                                            //    uint DropUniqueID = OUT_REMOTE[i].ReadUInt32();
                                            //    ushort dropRegion = OUT_REMOTE[i].ReadUInt16();
                                            //    float xdrop = OUT_REMOTE[i].ReadSingle();
                                            //    float ydrop = OUT_REMOTE[i].ReadSingle();
                                            //    float zdrop = OUT_REMOTE[i].ReadSingle();

                                            //    ushort dropAngle = OUT_REMOTE[i].ReadUInt16();
                                            //    //Utils.WriteLine($"DropUniqueID : {DropUniqueID} , dropRegion : {dropRegion}, xdrop : {xdrop}, ydrop : {ydrop},  zdrop : {zdrop}");
                                            //    // States
                                            //    bool hasOwner = OUT_REMOTE[i].ReadBool();
                                            //    if (hasOwner)
                                            //    {
                                            //        uint dropOwnerJoinID = OUT_REMOTE[i].ReadUInt32();
                                            //    }    

                                            //    Byte dropRarity = OUT_REMOTE[i].ReadByte();

                                            //}


                                            //byte[] packet_bytes = OUT_REMOTE[i].GetBytes();
                                            //Console.WriteLine($"{CHARNAME16}[AGENT][REMOTE][S=>P][{OUT_REMOTE[i].Opcode:X4}][{packet_bytes.Length} bytes]{(OUT_REMOTE[i].Encrypted ? "[Encrypted]" : "")}{(OUT_REMOTE[i].Massive ? "[Massive]" : "")}{Environment.NewLine}{Utility.HexDump(packet_bytes)}");
                                        }
                                    }
                                    catch { }
                                    ;
                                    break;
                                #endregion
                                #region SINGLE_DESPAWN
                                case 0x3016:
                                    try
                                    {
                                        uint despwn_UniqueID = OUT_REMOTE[i].ReadUInt32();
                                        var mobToRemove = UTILS.mobList.FirstOrDefault(mob => mob.MobGameID == despwn_UniqueID);
                                        if (mobToRemove != null)
                                        {
                                            UTILS.mobList.Remove(mobToRemove);
                                        }
                                    }
                                    catch { }
                                    ;
                                    break;
                                    #endregion
                            }
                            #region SEND_DATA
                            LOCAL_SECURITY.Send(OUT_REMOTE[i]);
                            #endregion
                        }
                        ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                        ASYNC_RECV_FROM_MODULE();
                    }
                }
                else { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer); return; }
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, ASYNC_SERVER.MODULE_TYPE.AgentServer); UTILS.ExportLog("AG_ASYNC_RECV_FROM_CLIENT", EX, CLIENT_SOCKET, CHARNAME16, LogOpCode); return; }
        }

        public override void ASYNC_SEND_TO_CLIENT(Socket CLIENT_SOCKET)
        {
            try
            {
                List<KeyValuePair<TransferBuffer, Packet>> OUT_LOCAL = LOCAL_SECURITY.TransferOutgoing();
                for (int i = 0; i < OUT_LOCAL.Count; i++)
                    CLIENT_SOCKET.SendToSocket(OUT_LOCAL[i].Key.Buffer, OUT_LOCAL[i].Key.Size);
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, MODULE_TYPE); UTILS.ExportLog("AG_ASYNC_SEND_TO_CLIENT", EX, CLIENT_SOCKET, CHARNAME16); return; }
        }

        public override void ASYNC_SEND_TO_MODULE()
        {
            try
            {
                List<KeyValuePair<TransferBuffer, Packet>> OUT_REMOTE = REMOTE_SECURITY.TransferOutgoing();
                for (int i = 0; i < OUT_REMOTE.Count; i++)
                    PROXY_SOCKET.SendToSocket(OUT_REMOTE[i].Key.Buffer, OUT_REMOTE[i].Key.Size);
            }
            catch (Exception EX) { ASYNC_SERVER.DISCONNECT(CLIENT_SOCKET, MODULE_TYPE); UTILS.ExportLog("AG_ASYNC_SEND_TO_MODULE", EX, PROXY_SOCKET, CHARNAME16); return; }
        }

        public void UpdateTitlesColor()
        {
            try
            {
                if (UTILS.titlescolors.Count > 0)
                {
                    Packet Info = new Packet(0x5108);
                    Info.WriteUInt32(UTILS.titlescolors.Count(x=> x.ShardID == ShardID));
                    foreach (var line in UTILS.titlescolors)
                    {
                        if(line.ShardID == ShardID)
                        {
                            Info.WriteAscii(line.CharName);
                            Info.WriteUInt32(line.Color);
                        }
                    }
                    LOCAL_SECURITY.Send(Info);
                    ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }
            catch { }
        }

        public void UpdateParty() {
            if (PartyID != 0)
            {
                try {

                    Packet UpdateParty = new Packet(0x706A);
                    UpdateParty.WriteUInt32(PartyID);
                    UpdateParty.WriteUInt32(LeaderType);
                    UpdateParty.WriteUInt8(LeaderRace);
                    UpdateParty.WriteUInt8(LeaderPurpose);
                    UpdateParty.WriteUInt8(LeaderEntryLevel);
                    UpdateParty.WriteUInt8(LeaderMaxLevel);
                    UpdateParty.WriteAscii(LeaderTitle);
                    REMOTE_SECURITY.Send(UpdateParty);
                    ASYNC_SEND_TO_MODULE();
                    //byte[] packet_bytes = 更新注册组队包.GetBytes();
                    //string line = string.Format("[IP:{7}][AGENT][REMOTE][S=>P][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", 更新注册组队包.Opcode, packet_bytes.Length,
                    //    更新注册组队包.Encrypted ? "[Encrypted]" : "", 更新注册组队包.Massive ? "[Massive]" : "", Environment.NewLine, BitConverter.ToString(packet_bytes),// Utility.HexDump
                    //    Environment.NewLine, SOCKET_IP);
                    //UTILS.DiagWriteLine(line);
                }
                catch (Exception Ex) {
                    UTILS.WriteLine($"[{CHARNAME16}] Update Party Failed.:{Ex.ToString()}");
                }
            }
        }
        public void Update_Item_Count(byte reverse_slot , ushort reverse_type)
        {
            Packet reverse = new Packet(0x704C, true);
            reverse.WriteUInt8(reverse_slot);
            reverse.WriteUInt16(reverse_type);
            reverse.WriteUInt8(0x7);
            reverse.WriteUInt32(0x1C);
            REMOTE_SECURITY.Send(reverse);
            ASYNC_SEND_TO_MODULE();
        }
        public void DISMOUNT_PET()
        {
            Packet Dis = new Packet(0x70C6);
            Dis.WriteUInt32(RIDING_PET_UNIQUEID);
            REMOTE_SECURITY.Send(Dis);
            ASYNC_SEND_TO_MODULE();
        }

        public void SendRank(byte type, ConcurrentDictionary<string, Rank> List)
        {
            try
            {
                if (List.Count > 0)
                {
                    Packet packet = new Packet(0x5124);
                    packet.WriteUInt8(type);
                    packet.WriteUInt8(List.Count(x=> x.Value.ShardID == ShardID));

                    foreach (var line in List)
                    {
                        if(line.Value.ShardID == ShardID)
                        {
                            packet.WriteUInt8(line.Value.LineNum);
                            packet.WriteAscii(line.Key);
                            packet.WriteAscii(line.Value.Guild);
                            packet.WriteAscii(line.Value.Points);
                        }
                    }
                
                    LOCAL_SECURITY.Send(packet);
                    ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"SendRank operation has failed. {EX.Message}", UTILS.LOG_TYPE.Fatal); }
        }

        public void ShowControl(int ID, byte Switch)
        {
            Packet packet = new Packet(0x5101);
            packet.WriteValue<int>(ID);
            packet.WriteValue<byte>(Switch);

            if (ID == 11448)
            {
                IEnumerable<AGENT_MODULE> source = from x in ASYNC_SERVER.AG_CONS.Values
                                                    where x.FWKillsGui
                                                    select x;
                Parallel.ForEach<AGENT_MODULE>(source, delegate (AGENT_MODULE client)
                {
                    client.LOCAL_SECURITY.Send(packet);
                    client.ASYNC_SEND_TO_CLIENT(client.CLIENT_SOCKET);
                });
            }
            else
            {
                LOCAL_SECURITY.Send(packet);
                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
            }
        }

        public void ShowTimer(int timetoset)
        {
            Packet pk = new Packet(0x34D2);
            pk.WriteUInt8((byte)8);
            pk.WriteUInt32(timetoset);
            LOCAL_SECURITY.Send(pk);
            ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
        }

        public void SendGlobal(byte Slot, string Message, byte UseType, byte TypeID)
        {
            Packet packet = new Packet(0x704C, true);
            packet.WriteUInt8(Slot); //here is the global slot
            packet.WriteUInt8(UseType);
            packet.WriteUInt8(TypeID);
            //vietnamese
            packet.WriteAsciiA(Message);
            REMOTE_SECURITY.Send(packet);
            ASYNC_SEND_TO_MODULE();
        }

        public void LifeStats()
        {
            DEAD_STATUS = false;
            Packet packet = new Packet(0x3053);
            packet.WriteUInt8(0x01);
            REMOTE_SECURITY.Send(packet);
            ASYNC_SEND_TO_MODULE();
        }

        public void LiveTitleUpdate(byte ID)
        {
            Packet LiveTitle = new Packet(0x3500);
            LiveTitle.WriteUInt32(UNIQUE_ID);
            LiveTitle.WriteUInt8(2);
            LiveTitle.WriteUInt8(ID);
            REMOTE_SECURITY.Send(LiveTitle);
            ASYNC_SEND_TO_MODULE();
        }
        public void LiveGold(int gold, bool show)
        {
            Packet packet = new Packet(0x3500);
            packet.WriteUInt32(UNIQUE_ID);
            packet.WriteUInt8(6);
            packet.WriteUInt32(gold);
            packet.WriteValue<bool>(show);
            REMOTE_SECURITY.Send(packet);
            ASYNC_SEND_TO_MODULE();
        }
        //public void LiveSilk(int Silk, int Git, int Points, bool show)
        //{
        //    Packet packet = new Packet(0x3500);
        //    packet.WriteUInt32(UNIQUE_ID);
        //    packet.WriteUInt8(5);
        //    packet.WriteInt32(Silk);
        //    packet.WriteInt32(Git);
        //    packet.WriteInt32(Points);
        //    packet.WriteValue<bool>(show);
        //    REMOTE_SECURITY.Send(packet);
        //    ASYNC_SEND_TO_MODULE();
        //}
        public void LiveTeleport(int WorldLayerID, int WorldID, ushort RegionID, float x, float y, float z)
        {
            Packet packet = new Packet(0x3500);
            packet.WriteUInt32(UNIQUE_ID);
            packet.WriteUInt8(8);
            packet.WriteInt32(WorldLayerID);
            packet.WriteInt32(WorldID);
            packet.WriteUInt16(RegionID);
            packet.WriteValue<float>(x);
            packet.WriteValue<float>(y);
            packet.WriteValue<float>(z);
            REMOTE_SECURITY.Send(packet);
            ASYNC_SEND_TO_MODULE();
        }

        public void UpdateCharNameColor()
        {
            if (UTILS.CharnameColorList.Count > 0)
            {
                Packet Info = new Packet(0x5111);
                Info.WriteUInt32(UTILS.CharnameColorList.Count(x => x.ShardID == ShardID));
                foreach (var line in UTILS.CharnameColorList)
                {
                    if(line.ShardID == ShardID)
                    {
                        Info.WriteAscii(line.CharName);
                        Info.WriteUInt32(line.Color);
                    }
                }
                LOCAL_SECURITY.Send(Info);
                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
            }
        }
        public void UpdateCustomTitle()
        {
            if(UTILS.CustomTitleList.Count > 0)
            {
                Packet Info = new Packet(0x5105);
                Info.WriteUInt32(UTILS.CustomTitleList.Count(a=> a.ShardID == ShardID && a.Titles.Count(x=> x.Value == 1) > 0));
                foreach (var x in UTILS.CustomTitleList)
                {
                    if(x.ShardID == ShardID)
                    {
                        foreach (var xa in x.Titles)
                        {
                            if (xa.Value == 1)
                            {
                                Info.WriteAscii(x.CharName);
                                Info.WriteAscii(xa.Key);
                            }
                        }
                    }
                    
                }
                UTILS.BroadCastToClients(Info, ShardID);
            }
        }

        public void UpdateCharIcon()
        {
            Packet icon = new Packet(0x180A);
            icon.WriteUInt8((byte)UTILS.CustomIconsData.Count);
            foreach (var xx in UTILS.CustomIconsData)
            {
                icon.WriteInt32(xx.Key);
                icon.WriteAscii(xx.Value);
            }
            LOCAL_SECURITY.Send(icon);
            ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);

            Packet icon2 = new Packet(0x180B);
            icon2.WriteUInt8((byte)UTILS.CustomIcons.Count(x => x.ShardID == ShardID));
            foreach (var xx in UTILS.CustomIcons)
            {
                if (xx.ShardID == ShardID)
                {
                    icon2.WriteInt32(xx.IconID);
                    icon2.WriteAscii(xx.CharName);
                }
            }
            LOCAL_SECURITY.Send(icon2);
            ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
        }
        public void SendB034(_ItemInfo Item,byte Slot)
        {
            Packet x180F = new Packet(0x180F);
            x180F.WriteUInt8(Slot);

            var response = new Packet(0xB034);
            response.WriteUInt8(0x01); //Success
            response.WriteUInt8(0x06); //pickitem
            response.WriteUInt8(Slot); //slot
            response.WriteUInt32(0); //Unknwon (could be unique id)
            response.WriteUInt32(Item.ItemID); //itemid
            switch (Item.TypeID2)
            {
                case 1:
                    response.WriteUInt8(Item.Plus);
                    response.WriteUInt64(Item.Variance);
                    response.WriteUInt32(Item.Durability);
                    response.WriteUInt8(Item.MagParamNum);
                    if (Item.MagParamNum > 0)
                    {
                        for (var i = 0; i < Item.MagParamNum; i++)
                        {
                            var MagHex = Item.MagicOptions[i].ToString("X");
                            var MagValue = uint.Parse(MagHex.Substring(0, MagHex.Length - 8), NumberStyles.HexNumber);
                            var MagType = uint.Parse(MagHex.Substring(MagHex.Length - 4), NumberStyles.HexNumber);
                            response.WriteUInt32(MagType);
                            response.WriteUInt32(MagValue);
                        }
                    }
                    //1 = Socket
                    response.WriteUInt8(1);
                    response.WriteUInt8(Item.SocketOptions.Count);
                    for (int i =0; i < Item.SocketOptions.Count; i++)
                    {
                        response.WriteUInt8(Item.SocketOptions[i].Slot);
                        response.WriteUInt16(Item.SocketOptions[i].ID);
                        response.WriteUInt16(Item.SocketOptions[i].Value);
                        response.WriteUInt32(Item.SocketOptions[i].nParam);
                    }
                    // 2 = Advanced elixir
                    response.WriteUInt8(2);
                    response.WriteUInt8(Item.AdvanceOptions.Count);
                    for (int i = 0; i < Item.AdvanceOptions.Count; i++)
                    {
                        response.WriteUInt8(Item.AdvanceOptions[i].Slot);
                        response.WriteUInt32(Item.AdvanceOptions[i].ID);
                        response.WriteUInt32(Item.AdvanceOptions[i].Value);
                    }
                    break;
                case 2:
                    switch (Item.TypeID3)
                    {
                        case 1:
                            response.WriteUInt8(1); //State
                            break;

                        case 2:
                            response.WriteUInt32(0); //Monster mask or so
                            break;

                        default:
                            if (Item.TypeID4 == 3) //Magic cube
                                response.WriteInt32(Item.Durability);
                            break;
                    }
                    break;
                case 3:
                    response.WriteUInt16(Item.Durability);

                    if (Item.TypeID3 == 11) //Magic stones
                        if (Item.TypeID4 == 1 || Item.TypeID4 == 2)
                            response.WriteUInt8(0); //AttributeAssimilationProbability
                        else if (Item.TypeID3 == 14 && Item.TypeID4 == 2) //ITEM_MALL_GACHA_CARD_WIN & LOSE
                            response.WriteUInt8(0);
                    break;
            }
               
                LOCAL_SECURITY.Send(response);
                LOCAL_SECURITY.Send(x180F);
                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
       
        }
        public void UpdateCustomNameRank()
        {
            if (UTILS.CustomCharnameRankList.Count > 0)
            {
                Packet Info = new Packet(0x5115);
                Info.WriteUInt32(UTILS.CustomCharnameRankList.Count(x=> x.ShardID == ShardID));
                foreach (var line in UTILS.CustomCharnameRankList)
                {
                    if(line.ShardID == ShardID)
                    {
                        Info.WriteAscii(line.CharName);
                        Info.WriteAscii(line.Rank);
                    }
                }
                LOCAL_SECURITY.Send(Info);
                ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
            }
        }

        public void SendPlusNoticeSignal(byte slot)
        {

            Packet packet = new Packet(0x9960);
            packet.WriteUInt8(slot);
            LOCAL_SECURITY.Send(packet);
            ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
        }
        public async Task UpdateItemChestAsync(bool updatelist = false)
        {
            try
            {
                if (updatelist)
                {
                    await QUERIES.UpdateCharChest(this);
                }
                if (CharChest.Count > 0)
                {
                    int RemainSlots = await QUERIES.GetRemainSlots(CharID,ShardID);
                    Packet Info = new Packet(0x5125);
                    Info.WriteUInt32(CharChest.Count);
                    Info.WriteInt32(RemainSlots);
                    foreach (KeyValuePair<int, _CharChest> line in CharChest)
                    {
                        Info.WriteInt32(line.Value.LineNum);
                        Info.WriteInt32(line.Key);
                        Info.WriteInt32(line.Value.RefItemID);
                        Info.WriteAscii(line.Value.Count.ToString());
                        Info.WriteInt8(line.Value.RandomizedStats);
                        Info.WriteAscii(line.Value.OptLevel.ToString());
                        Info.WriteAscii(line.Value.From);
                        Info.WriteAscii(line.Value.RegisterTime.ToShortDateString());
                    }

                    LOCAL_SECURITY.Send(Info);
                    ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }

            catch { }
        }
    }
}
