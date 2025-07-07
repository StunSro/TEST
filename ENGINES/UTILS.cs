using Framework;
using SR_PROXY.CORE;
using SR_PROXY.SECURITYOBJECTS;
using SR_PROXY.SQL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Collections;
using Newtonsoft.Json;
using SR_PROXY.MODEL;
using NetFwTypeLib;
using Fleck;
using System.Timers;
using System.Net.Security;

namespace SR_PROXY.ENGINES
{
    public static class UTILS
    {
        /*
        break -  stops iterating threw the forloop,will block the current packet and dc the player, 
        continue - block the current packet and keeps iterating threw the for loop
        return - blocks the current packet, dcing the player, ending the current method execution
        we wanna call this once, to create the memoized version of our expensive function
        and then we wanna use this instead of the expensive function...
        public static Func<bool, byte> MemoizedGenerateCountByte = Memoize<bool, byte>(Security.GenerateCountByte);
        public static Func<byte[],byte> MemoizedGenerateCheckByte = Memoize<byte[], byte>(Security.GenerateCheckByte);
        */
        public enum LOG_TYPE { Default, Notify, Warning, Fatal, Error, Event, Debug, System }
        public enum NoticeType : byte
        {
            Notice = 1,
            Notify = 2,
            Green = 3,
            Orange = 4,
            Purble = 5,
            Yellow = 6,
            Guide = 7,
            ColoredSystem = 8,
            ColoredChat = 9,
            Alchemy = 10,
        }
        public static Dictionary<int, int> CUSTOM_UNIQUES = new Dictionary<int, int>();
              public static List<string> BAN_LIST = new List<string>();
        public static int PT_NR_RECORD, MT_ANSWER, AE_REQPLUS, LPTNR_WINNINGNR = 0;
        public static string RT_ANSWER, FT_ANSWER = string.Empty;
        public static string[] TRIVIA_QUESTION;
        //Used to detect Unique kills counting in our Unique auto event...
        public static List<int> KILLED_MOBS_ID = KILLED_MOBS_ID = new List<int>();
        public static bool SERVER_INSPECTION, UNQ_EVENT, TR_EVENT, FT_EVENT, RT_EVENT, MT_EVENT, STALLER_EVENT, AE_EVENT, LT_EVENT, LPNR_EVENT = false;
        //under teleport process list
        public static List<string> TELEPORTING_PROC = new List<string>();
        public static List<string> LOTTERY_LIST = new List<string>();
        //Analyzing 20 latest packets queue
        public static Stack<Packet> GW_ALL_LATEST_TRAFFIC = new Stack<Packet>();
        public static Stack<Packet> AG_ALL_LATEST_TRAFFIC = new Stack<Packet>();
        public static List<string> Available_Uniques = new List<string>();
        public static Dictionary<int, int> Region_Restrection = new Dictionary<int, int>();
        public static List<int> Teleport_To_Town = new List<int>();
        public static List<int> DISABLE_CHAT_REGIONS = new List<int>();
        public static List<int> DISABLE_ZERK_REGIONS = new List<int>();
        public static List<int> DISABLE_PARTY_REGIONS = new List<int>();
        public static List<int> DISABLE_TRACE_REGIONS = new List<int>();
        public static List<int> DISABLE_INVITEFRIENDS_REGIONS = new List<int>();
        public static Dictionary<int, List<int>> DISABLE_SKILLS_REGIONS = new Dictionary<int, List<int>>();
        public static Dictionary<int, List<int>> DISABLE_ITEMS_REGIONS = new Dictionary<int,List<int>>();
        //tite lists
        public static List< _CustomTitleColor> titlescolors = new List<_CustomTitleColor>();
        public static List<_CustomTitle> CustomTitleList = new List<_CustomTitle>();
        //rank lists
        public static ConcurrentDictionary<string, Rank> CustomUniqueRank = new ConcurrentDictionary<string, Rank>();
        public static ConcurrentDictionary<string, Rank> CustomHonorRank = new ConcurrentDictionary<string, Rank>();
        public static ConcurrentDictionary<string, Rank> CustomPVPRank = new ConcurrentDictionary<string, Rank>();
        public static ConcurrentDictionary<string, Rank> CustomTraderRank = new ConcurrentDictionary<string, Rank>();
        public static ConcurrentDictionary<string, Rank> CustomHunterRank = new ConcurrentDictionary<string, Rank>();
        public static ConcurrentDictionary<string, Rank> CustomThiefRank = new ConcurrentDictionary<string, Rank>();
        public static ConcurrentDictionary<string, Rank> CustomCharRank = new ConcurrentDictionary<string, Rank>();
        public static ConcurrentDictionary<string, Rank> CustomJobKillsRank = new ConcurrentDictionary<string, Rank>();
        public static ConcurrentDictionary<string, EventTime> EventTimeList = new ConcurrentDictionary<string, EventTime>();
        public static List<string> RewardSilkHour = new List<string>();

        public static ConcurrentDictionary<int, UniqueMob> UniqueLog = new ConcurrentDictionary<int, UniqueMob>();

        public static ConcurrentDictionary<int, _UniquesLogParam> UniqueLogsParams = new ConcurrentDictionary<int, _UniquesLogParam>();
        public static ConcurrentDictionary<string, _ItemInfo> ItemLinkInfo = new ConcurrentDictionary<string, _ItemInfo>();

        public static List<_CustomNameRank> CustomCharnameRankList = new List<_CustomNameRank>();
        public static List<_CustomName> CustomCharnameList = new List<_CustomName>();
        public static List<long> MagParams = new List<long>();

        public static List<_CustomNameColor> CharnameColorList = new List< _CustomNameColor>();
        public static List<string> PVPregisterlist = new List<string>();
        public static List<string> PVPactivelist = new List<string>();
        public static List<string> UNIQUEregisterlist = new List<string>();
        public static List<string> UNIQUEactivelist = new List<string>();
        public static List<string> ChangeLog = new List<string>();
        public static List<string> JobFightEvent = new List<string>();
        public static List<string> SURVregisterlist = new List<string>();
        public static List<string> DRUNKregisterlist = new List<string>();

        public static List<AGENT_MODULE> SURVregisterActivelist = new List<AGENT_MODULE>();
        public static List<AGENT_MODULE> DRUNKregisterActivelist = new List<AGENT_MODULE>();

        public static ConcurrentDictionary<string, List<string>> GuildWarEvent = new ConcurrentDictionary<string, List<string>>();
        public static ConcurrentDictionary<string, int> PVPeventRegCount = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, int> UNIQUEeventRegCount = new ConcurrentDictionary<string, int>();

        public static ConcurrentDictionary<string, int> FWKills = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, int> SURVKills = new ConcurrentDictionary<string, int>();
        public static List<KeyValuePair<string, List<string>>> EventScheduling = new List<KeyValuePair<string, List<string>>>();
        public static ConcurrentDictionary<int, string> CustomIconsData = new ConcurrentDictionary<int, string>();
        public static List<_CharIcon> CustomIcons = new List<_CharIcon>();
        public static ConcurrentDictionary<string, int> GambleAttempts = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, _DailyReward> DailyReward = new ConcurrentDictionary<string, _DailyReward>();

        public static Dictionary<int, ItemType> Available_Objects = new Dictionary<int, ItemType>();

        public static List<Notification> NotificationList = new List<Notification>();

        public class Notification
        {
            public int UniqueID { get; set; }  // معرّف فريد للإشعار
            public string UniqueName { get; set; }  // اسم فريد للإشعار

            // المنشئ (Constructor) لتعيين القيم عند إنشاء كائن الإشعار
            public Notification(int uniqueID, string uniqueName)
            {
                UniqueID = uniqueID;  // تعيين المعرّف الفريد
                UniqueName = uniqueName;  // تعيين الاسم الفريد
            }
        }

        public static List<MobInfo> mobList = new List<MobInfo>();
        public class MobInfo
        {
            public uint MobID { get; set; }
            public uint MobGameID { get; set; }
            public uint RegionID { get; set; }

        }
        public static List<byte> XSMB = new List<byte>();
        public static List<int> DailyRewardItems = new List<int>();

        public static bool PVP_EVENT_ACTIVE = false;
        public static bool UNIQUE_EVENT_ACTIVE = false;
        public static bool SURVIVAL_ACTIVE = false;
        public static bool DRUNK_ACTIVE = false;
        public static bool DRUNK_REGISTER_ACTIVE = false;
        public static bool GW_EVENT_REGISTER = false;
        public static DateTime LAST_JF_EVENT = new DateTime();
        public static DateTime LAST_SURV_EVENT = new DateTime();
        public static DateTime LAST_DRUNK_EVENT = new DateTime();
        public static bool JOB_FIGHT_REGISTER = false;
        public static bool SURVIVAL_REGISTER = false;
        public static bool FW_EVENT_ACTIVE = false;
        public static bool IS_FORTRESS_INITIALIZED = false;
        public static bool IS_UNIQUE_KILLED = true;
        public static bool IS_UNIQUE_SPAWNED = false;
        public static bool MINIONS_ONLINE = true;
        public static int ListType = 0;
        public static int NoticeIndex = 0;
        public static System.Timers.Timer SeriousModeTimer;
        public static bool isSeriousModeInit;
        public static int GUI_DELAY = 60;
        public static string XSMB_DATE = string.Empty;
        #region MISC
        public static void WriteLine(string Content, LOG_TYPE type = LOG_TYPE.Default)
        {
            try
            {
                Settings.MAIN.listView1.BeginInvoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    Color color = Color.Black;
                    switch (type)
                    {
                        case LOG_TYPE.Default:
                            color = Color.Black;
                            break;

                        case LOG_TYPE.Notify:
                            color = Color.Green;
                            break;

                        case LOG_TYPE.Warning:
                            color = Color.OrangeRed;
                            break;

                        case LOG_TYPE.Fatal:
                            color = Color.Red;
                            break;

                        case LOG_TYPE.Error:
                            color = Color.DarkRed;
                            break;

                        case LOG_TYPE.Event:
                            color = Color.RoyalBlue;
                            break;

                        case LOG_TYPE.Debug:
                            color = Color.Gray;
                            break;

                        case LOG_TYPE.System:
                            color = Color.Purple;
                            break;
                    }
                    ListViewItem line = new ListViewItem(new string[] { DateTime.Now.ToString("h:mm:ss"), type.ToString(), Content });
                    line.ForeColor = color;
                    Settings.MAIN.listView1.Items.Add(line);
                    Settings.MAIN.listView1.EnsureVisible(Settings.MAIN.listView1.Items.Count - 1);
                });
            }
            catch { }
        }
        public static void DiagWriteLine(string Content)
        {
            try
            {
                Settings.MAIN.DiagLog.BeginInvoke((MethodInvoker)delegate
                {
                    Settings.MAIN.DiagLog.AppendText(Content);
                    Settings.MAIN.DiagLog.ScrollToCaret();
                });
            }
            catch { }
        }
        public static void CSWriteLine(string Content)
        {
            //try
            //{
            //    Settings.MAIN.ClientlessLog.BeginInvoke((MethodInvoker)delegate
            //    {
            //        Settings.MAIN.ClientlessLog.AppendText(Content);
            //        Settings.MAIN.ClientlessLog.ScrollToCaret();
            //    });
            //}
            //catch { }
        }
        public static Func<Arg, Ret> Memoize<Arg, Ret>(this Func<Arg, Ret> functor)
        {
            var memo_table = new ConcurrentDictionary<Arg, Ret>();

            return (arg0) =>
            {
                Ret func_return_val;

                if (!memo_table.TryGetValue(arg0, out func_return_val))
                {
                    func_return_val = functor(arg0);
                    memo_table.TryAdd(arg0, func_return_val);
                }
                return func_return_val;
            };
        }
        public static void Measure(string what, int reps, Action action)
        {
            action();//warm up
            double[] results = new double[reps];
            for (int i = 0; i < reps; ++i)
            {
                Stopwatch sw = Stopwatch.StartNew();
                action();
                results[i] = sw.Elapsed.TotalMilliseconds;
            }
            WriteLine($"{what} - AVG = {results.Average()}, MIN = {results.Min()}, MAX = {results.Max()}", LOG_TYPE.Notify);
        }
        public static async void ExportLog(string methodname, Exception EX, Socket SOCKET = null, string CharName16 = "",int OpCode=0)
        {
            try
            {
                if (Settings.LOG_PROXY_ERRORS)
                {
                    if (EX.GetType().ToString() != "System.Net.Sockets.SocketException" && EX.GetType().ToString() != "System.Exception")//Print all but socket and system exceptions
                    {
                        using (StreamWriter file = new StreamWriter("Logs/" + methodname + ".txt", true))
                        {
                            await file.WriteLineAsync($"[行为码[{OpCode}],Time:{DateTime.Now}{methodname}:Sock:{(SOCKET == null ? "nullsock" : SOCKET.RemoteEndPoint.ToString())}:Cn16:{(string.IsNullOrEmpty(CharName16) ? "nulloremptycn16" : CharName16)}]:{Environment.NewLine} {EX.ToString()}");
                            file.Close();
                        }
                    }
                }
            }
            catch (Exception FAILEXC) { WriteLine($"ExportLog() failed. {FAILEXC.Message}"); }
        }
        #endregion
        public static async void SeriousMode(object source, ElapsedEventArgs e)
        {
            try
            {
                SeriousModeTimer.Stop();
                foreach (var item in ASYNC_SERVER.DW_CONS)
                {
                    if (item.Value.TOT_PACKET_CNT == 0)
                    {
                        ASYNC_SERVER.DISCONNECT(item.Key, ASYNC_SERVER.MODULE_TYPE.DownloadServer);
                        ASYNC_SERVER.DW_CONS.TryRemove(item.Key, out DOWNLOAD_MODULE temp);
                    }
                }
                foreach (var item in ASYNC_SERVER.GW_CONS)
                {
                    if (item.Value.TOT_PACKET_CNT == 0)
                    {
                        ASYNC_SERVER.DISCONNECT(item.Key, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                        ASYNC_SERVER.GW_CONS.TryRemove(item.Key, out GATEWAY_MODULE temp);
                    }
                }
                foreach (var a in ASYNC_SERVER.AG_CONS.Where(x => x.Value.TOT_PACKET_CNT == 0 || x.Value.TOT_BYTES_CNT == 0))
                {
                    ASYNC_SERVER.DISCONNECT(a.Key, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                }
                SeriousModeTimer.Start();
            }
            catch (Exception EX) { WriteLine($"SeriousMode failed,  {EX}", LOG_TYPE.Fatal); }
        }
        //判断进程是否64位
        
        public static bool IsDigits(string s)
        {
            try
            {
                if (s == null || s == "") return false;

                for (int i = 0; i < s.Length; i++)
                    if ((s[i] ^ '0') > 9)
                        return false;

                return true;
            }
            catch (Exception EX) { WriteLine($"IsDigits failed. {EX}", LOG_TYPE.Warning); return false; }
        }
        /// <summary>
        /// 判断字符是否是全角符号
        /// </summary>
        /// <param name="ch">判断字符</param>
        /// <returns></returns>
        public static bool IsSBC(char ch)
        {
            if ((ch >= 65281 && ch <= 65374) || ch == 12288)
            {
                return true;
            }
            return false;
        }

        //static bool ContainChinese(string input)
        //{
        //    string pattern = "[\u4e00-\u9fbb]";
        //    return Regex.IsMatch(input, pattern);
        //}
        #region SEND_MESSAGES_FUNCTIONS
        public static void SEND_NOTICE_TO_ALL(string strMessage)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                if (ASYNC_SERVER.AG_CONS.Keys != null && ASYNC_SERVER.AG_CONS.Keys.Count > 0)
                {
                    Packet packet = new Packet(0x3026);
                    packet.WriteUInt8(7);
                    packet.WriteAscii(strMessage);
                    Packet ping = new Packet(0x2002);

                    foreach (var con in ASYNC_SERVER.AG_CONS.Where(x => !string.IsNullOrEmpty(x.Value.CHARNAME16)))// looping via all conns
                    {
                        ASYNC_SERVER.AG_CONS[con.Key].LOCAL_SECURITY.Send(packet);
                        ASYNC_SERVER.AG_CONS[con.Key].LOCAL_SECURITY.Send(ping);

                        if (IsSocketConnected(con.Key))
                            ASYNC_SERVER.AG_CONS[con.Key].ASYNC_SEND_TO_CLIENT(con.Key);
                    }
                }
                sw.Stop();
                //if (Settings.LOG_PROXY_ERRORS)
                //{
                //    WriteLine($"SEND_NOTICE_TO_ALL has been completed in [{sw.ElapsedMilliseconds} ms]");
                //}
            }
            catch (Exception EX) { WriteLine($"SEND_NOTICE_TO_ALL operation has failed. {EX.Message}", LOG_TYPE.Warning); }
        }
        /// <summary>
        /// </summary>
        /// <param name="Num"></param>
        /// <param name="CLIENT_SOCKET"></param>
        public static void SEND_INDV_SLB_UPDATE_NOTICE(int Num, Socket CLIENT_SOCKET)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16))
                {
                    Packet packet = new Packet(0xB034);
                    packet.WriteUInt8(1);
                    packet.WriteUInt8(6);
                    packet.WriteUInt8(0xFE);
                    packet.WriteUInt32(Num);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(packet);
                    Packet ping = new Packet(0x2002);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(ping);

                    if (IsSocketConnected(CLIENT_SOCKET))
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }
            catch (Exception EX) { WriteLine($"SEND_INDV_SLB_UPDATE_NOTICE operation has failed. {EX.Message}", LOG_TYPE.Warning); }
        }
        /// <summary>
        /// </summary>
        /// <param name="Num"></param>
        /// <param name="CLIENT_SOCKET"></param>
        public static void SEND_INDV_SLB_UPDATE(long Num, Socket CLIENT_SOCKET)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16))
                {
                    Packet packet = new Packet(0x304E);
                    packet.WriteUInt8(1);
                    packet.WriteUInt64(Num);
                    packet.WriteUInt8(0);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(packet);
                    Packet ping = new Packet(0x2002);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(ping);

                    if (IsSocketConnected(CLIENT_SOCKET))
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }
            catch (Exception EX) { WriteLine($"SEND_INDV_SLB_UPDATE operation has failed. {EX.Message}", LOG_TYPE.Warning); }
        }

        public static void 召唤怪物(Socket CLIENT_SOCKET,int 怪物ID,int 数量=1)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16))
                {
                    Packet packet = new Packet(0x7010);
                    packet.WriteUInt8(0x6);
                    packet.WriteUInt8(0);
                    packet.WriteUInt32(怪物ID);
                    packet.WriteUInt8(数量);
                    packet.WriteUInt8(0);//类型
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].REMOTE_SECURITY.Send(packet);
                    if (IsSocketConnected(CLIENT_SOCKET))
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_MODULE();
                }
            }
            catch (Exception EX) { WriteLine($"召唤怪物失败:{EX.Message}", LOG_TYPE.Warning); }
        }
        public static void 生成物品(Socket CLIENT_SOCKET, int 物品ID, int 数量 = 1)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16))
                {
                    Packet packet = new Packet(0x7010);
                    packet.WriteUInt8(0x7);
                    packet.WriteUInt8(0);
                    packet.WriteUInt32(物品ID);
                    packet.WriteUInt8(数量);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].REMOTE_SECURITY.Send(packet);
                    if (IsSocketConnected(CLIENT_SOCKET))
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_MODULE();
                }
            }
            catch (Exception EX) { WriteLine($"生成物品失败:{EX.Message}", LOG_TYPE.Warning); }
        }
        public static void 传送至NPC(Socket CLIENT_SOCKET, string Npc)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16))
                {
                    Packet packet = new Packet(0x7010);
                    packet.WriteUInt8(0x1F);
                    packet.WriteUInt8(0);
                    packet.WriteAscii(Npc);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].REMOTE_SECURITY.Send(packet);
                    if (IsSocketConnected(CLIENT_SOCKET))
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_MODULE();
                }
            }
            catch (Exception EX) { WriteLine($"SEND_INDV_SLB_UPDATE operation has failed. {EX.Message}", LOG_TYPE.Warning); }
        }
        public static void 传送到目标(Socket CLIENT_SOCKET, string Name)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16))
                {
                    Packet packet = new Packet(0x7010);
                    packet.WriteUInt16(8);
                    packet.WriteAscii(Name);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].REMOTE_SECURITY.Send(packet);
                    if (IsSocketConnected(CLIENT_SOCKET))
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_MODULE();
                }
            }
            catch (Exception EX) { WriteLine($"SEND_INDV_SLB_UPDATE operation has failed. {EX.Message}", LOG_TYPE.Warning); }
        }
        /// <summary>
        /// 更新用户金珠显示
        /// </summary>
        /// <param name="Num"></param>
        /// <param name="CLIENT_SOCKET"></param>
        public static void SEND_INDV_SILK_UPDATE(int[] Num, Socket CLIENT_SOCKET)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16))
                {

                    Packet packet = new Packet(0x3153);
                    packet.WriteUInt32(Num[0]);
                    packet.WriteUInt32(Num[1]);
                    packet.WriteUInt32(Num[2]);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(packet);

                    //为什么要再发一个心跳包 没搞懂
                    Packet ping = new Packet(0x2002);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(ping);

                    if (IsSocketConnected(CLIENT_SOCKET))
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }
            catch (Exception EX) { WriteLine($"SEND_INDV_SLB_UPDATE operation has failed. {EX.Message}", LOG_TYPE.Warning); }
        }


        /// <summary>
        /// SEND_INDV_NOTICE.向指定Socket发送公告
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="CLIENT_SOCKET"></param>
        public static void SEND_INDV_NOTICE(string strMessage, Socket CLIENT_SOCKET, bool 聊天模式 = false)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16))
                {
                    Packet packet = new Packet(0x3026);
                    packet.WriteUInt8(7);
                    packet.WriteAscii(strMessage);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(packet);


                    Packet ping = new Packet(0x2002);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(ping);

                    if (IsSocketConnected(CLIENT_SOCKET))
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }
            catch (Exception EX) { WriteLine($"SEND_INDV_NOTICE operation has failed. {EX.Message}", LOG_TYPE.Warning); }
        }
        public static void SEND_INDV_ERR_MSG(string errcode, Socket CLIENT_SOCKET, bool INCLUDE_NOITCE = true)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16))
                {
                    if (INCLUDE_NOITCE)
                    {
                        Packet BLUE_NOTICE = new Packet(0x300C);
                        BLUE_NOTICE.WriteUInt16(3100);
                        BLUE_NOTICE.WriteUInt8(1);
                        BLUE_NOTICE.WriteAscii(errcode);
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(BLUE_NOTICE);
                    }

                    Packet BOTTOM_NOTICE = new Packet(0x300C);
                    BOTTOM_NOTICE.WriteUInt16(3100);
                    BOTTOM_NOTICE.WriteUInt8(2);
                    BOTTOM_NOTICE.WriteAscii(errcode);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(BOTTOM_NOTICE);

                    Packet ping = new Packet(0x2002);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(ping);

                    if (IsSocketConnected(CLIENT_SOCKET))
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }
            catch (Exception EX) { WriteLine($"SEND_INDV_ERR_MSG operation has failed. {EX.Message}", LOG_TYPE.Warning); }
        }

        public static void SEND_INDV_PM(string strMessage, Socket CLIENT_SOCKET)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16))
                {
                    Packet packet = new Packet(0x7025);
                    packet.WriteUInt8(2);
                    packet.WriteUInt8(0);
                    packet.WriteAscii(strMessage);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(packet);

                    Packet ping = new Packet(0x2002);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(ping);

                    if (IsSocketConnected(CLIENT_SOCKET))
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }
            catch (Exception EX) { WriteLine($"SEND_INDV_PM operation has failed. {EX.Message}", LOG_TYPE.Warning); }
        }
        #endregion

        #region HELPERS FUNCTIONS: FEATURE/AUTOEVENTS/CONS/PREM/STARTUP
        //CONS TAB
        public static bool CHECK_IS_CHAR_ONLINE(string CharName16)
        {
            try
            {
                bool ONLINE_MARKER = false;
                foreach (AGENT_MODULE SESSION in ASYNC_SERVER.AG_CONS.Values)
                    if (SESSION.CHARNAME16 == CharName16)
                        ONLINE_MARKER = true;
                return ONLINE_MARKER;
            }
            catch (Exception EX) { WriteLine($"CHECK_IS_CHAR_ONLINE() failed and returned false {EX.ToString()}", LOG_TYPE.Warning); return false; }
        }
        public static int COUNT_SPAWNED_CHARS()
        {
            try
            {
                int ctr = 0;
                foreach (AGENT_MODULE SESSION in ASYNC_SERVER.AG_CONS.Values.Where(t => !string.IsNullOrEmpty(t.CHARNAME16)))
                    ctr++;
                return ctr;
            }
            catch (Exception EX) { WriteLine($"COUNT_SPAWNED_CHARS() failed and returned 0 {EX.ToString()}", LOG_TYPE.Warning); return 0; }
        }

        public static int AG_IPCount(string IP)
        {
            try
            {
                if (IP == Settings.BIND_IP) {
                    return 0;
                }
                int cnt = 0;
                if (ASYNC_SERVER.AG_CONS.Values.Count > 0 && ASYNC_SERVER.AG_CONS.Values != null)
                    //ignoring lingering connections!
                    foreach (AGENT_MODULE value in ASYNC_SERVER.AG_CONS.Values.Where(t => (t.PPS != 0 && t.BPS != 0 && t.TOT_PACKET_CNT != 0 && t.TOT_BYTES_CNT != 0 && t.TOKEN_ID != 0)))
                        if (value.SOCKET_IP.Split(':')[0] == IP)
                            cnt++;
                return cnt;
            }
            catch (Exception EX) { WriteLine($"AG_IPCount failed and returned 0. {EX.ToString()}", LOG_TYPE.Warning); return 0; }
        }
        public static int AG_HWIDCount(string HWID)
        {
            try
            {
                int cnt = 0;
                if (ASYNC_SERVER.AG_CONS.Values.Count > 0 && ASYNC_SERVER.AG_CONS.Values != null)
                    //ignoring lingering connections!
                    foreach (AGENT_MODULE value in ASYNC_SERVER.AG_CONS.Values)
                        if (value.CORRESPONDING_GW_SESSION !=null)
                            if(value.CORRESPONDING_GW_SESSION.HWID == HWID)
                            cnt++;
                return cnt;
            }
            catch (Exception EX) { WriteLine($"AG_IPCount failed and returned 0. {EX.ToString()}", LOG_TYPE.Warning); return 0; }
        }

        public static int COUNT_FW_HWID_OCC(string HWID)
        {
            try
            {
                int PC_CNT = 0;
                if (ASYNC_SERVER.AG_CONS != null && ASYNC_SERVER.AG_CONS.Count > 0)
                    foreach (AGENT_MODULE con in ASYNC_SERVER.AG_CONS.Values.Where(x => x.INSIDE_FW))
                        if (con.CORRESPONDING_GW_SESSION.HWID == HWID) PC_CNT++;
                return PC_CNT;
            }
            catch (Exception EX) { WriteLine($"COUNT_FW_HWID_OCC failed and returned 0 {EX}", LOG_TYPE.Warning); return 0; }
        }

        public static int COUNT_CTF_HWID_OCC(string HWID)
        {
            try
            {
                int PC_CNT = 0;
                if (ASYNC_SERVER.AG_CONS != null && ASYNC_SERVER.AG_CONS.Count > 0)
                    foreach (AGENT_MODULE con in ASYNC_SERVER.AG_CONS.Values.Where(x => x.CTF_REG))
                        if (con.CORRESPONDING_GW_SESSION.HWID == HWID) PC_CNT++;
                return PC_CNT;
            }
            catch (Exception EX) { WriteLine($"COUNT_CTF_HWID_OCC failed and returned 0. {EX}", LOG_TYPE.Warning); return 0; }
        }
        public static int COUNT_JOBMODE_HWID_OCC(string HWID,int type)
        {
            try
            {
                int PC_CNT = 0;
                if (type == 0)
                {
                    if (ASYNC_SERVER.AG_CONS != null && ASYNC_SERVER.AG_CONS.Count > 0)
                        foreach (AGENT_MODULE con in ASYNC_SERVER.AG_CONS.Values.Where(x => x.CORRESPONDING_GW_SESSION.HWID !=null && x.CORRESPONDING_GW_SESSION.HWID == HWID))
                        {
                            if(con !=null)
                            {
                                if (con.JOB_YELLOW_LINE || (con.JobType == 2 && con.JOB_FLAG))
                                    PC_CNT = 100;
                                else if ((con.JobType == 1 || con.JobType == 3) && con.JOB_FLAG)
                                    PC_CNT += 1;
                            }
                            
                        }
                }
                else if (type == 1)
                {

                    if (ASYNC_SERVER.AG_CONS != null && ASYNC_SERVER.AG_CONS.Count > 0)
                        foreach (AGENT_MODULE con in ASYNC_SERVER.AG_CONS.Values.Where(x => x.CORRESPONDING_GW_SESSION.HWID != null && x.CORRESPONDING_GW_SESSION.HWID == HWID))
                        {
                            if (con != null)
                            {
                                if (con.JOB_YELLOW_LINE || ((con.JobType == 1 || con.JobType == 3) && con.JOB_FLAG))
                                    PC_CNT = 100;
                                else if (con.JobType == 2 && con.JOB_FLAG)
                                    PC_CNT += 1;
                            }
                        }
                }
                return PC_CNT;
            }
            catch (Exception EX) { WriteLine($"COUNT_JOBMODE_HWID_OCC failed and returned 0. {EX}", LOG_TYPE.Warning); return 0; }
        }

        public static string DECRYPT_HWID(string hwid)
        {
            try
            {
                string salt = "WSAStart";
                char[] key = { 'n', 'a', 'z', 't', 'y', 't', 'o', 'a', 'r', 'n', 'v', 'z', 'r', 'm', 'e', 'n', '1', '2', '3', '4', '5', '6', '7', '8', 'v', 'b', 'r', 'g', 'e', 'a', '1', 'r', 'z', '4', '5', 'g', '7', 'u', 'n', 'a', 'z', 't', 'y', 't' };

                string encryptedHWID = Encoding.UTF8.GetString(Convert.FromBase64String(hwid));
                string HWID = XOR(encryptedHWID, key);

                if (encryptedHWID.Length != key.Length)
                {
                    WriteLine($"HWID key length error! {encryptedHWID.Length}.", LOG_TYPE.Warning);
                    return string.Empty;
                }

                if (!HWID.EndsWith(salt))
                {
                    WriteLine($"HWID key endswith error! {HWID}", LOG_TYPE.Warning);
                    return string.Empty;
                }

                HWID = BitConverter.ToString(Encoding.Default.GetBytes(HWID.Substring(0, 40)));
                //WriteLine("PC_Limit", string.Format("[{0},EL:{1}]", HWID, encryptedHWID.Length));
                return HWID;
            }
            catch (Exception EX) { WriteLine($"DECRYPT_HWID() failed and returned string.Empty. {EX}", LOG_TYPE.Warning); return string.Empty; }
        }

        public static string XOR(string data, char[] key)
        {
            try
            {
                StringBuilder xorstring = new StringBuilder(data);
                for (int i = 0; i < xorstring.Length; i++)
                {
                    xorstring[i] = (char)(data[i] ^ key[i]);
                }
                return xorstring.ToString();
            }
            catch (Exception EX) { WriteLine($"XOR() failed and returned string.Empty. {EX}", LOG_TYPE.Warning); return string.Empty; }
        }
        public static bool CHECK_TRADE_REGION_ALLOWED(short region)
        {
            try
            {
                if (READER.TOWNS_WREGION_ID.Contains(region)) return false; else return true;
            }
            catch (Exception EX) { WriteLine($"CHECK_TRADE_REGION_ALLOWED failed and returned true. {EX.ToString()}", LOG_TYPE.Warning); return true; }
        }
        public static double GET_PER_SEC_RATE(ulong VALUE, DateTime ST)
        {
            //todo: after AI learns this con is legit, stop monitoring him....
            try
            {
                double res = 0.0;
                TimeSpan diff = (DateTime.Now - ST);
                if (VALUE > int.MaxValue)
                {
                    VALUE = 0;
                    WriteLine("GET_PER_SEC_RATE VALUE failed due to a value overflow.", LOG_TYPE.Warning);
                }
                if (VALUE > 0)
                {
                    unchecked
                    {
                        double div = diff.TotalSeconds;
                        if (diff.TotalSeconds > 1.0)
                            res = Math.Round((VALUE / diff.TotalSeconds), 2);
                    }
                }
                return res;
            }
            catch (Exception EX) { WriteLine($"GET_PER_SEC_RATE failed and returned 0. {EX.ToString()}", LOG_TYPE.Warning); return 0; }
        }
        //AUTO EVENTS
        public static int COUNT_KILLED_MOB_ID_OCC(int MOB_ID)
        {
            try
            {
                int ctr = 0;
                foreach (int item in UTILS.KILLED_MOBS_ID.Where(t => t == MOB_ID))
                    ctr++;
                return ctr;
            }
            catch (Exception EX) { WriteLine($"COUNT_KILLED_MOB_ID_OCC failed and returned 0. {EX.ToString()}", LOG_TYPE.Warning); return 0; }
        }
        public static bool IS_DIGITS_ONLY(string str)
        {
            try
            {
                foreach (char c in str)
                {
                    if (c < '0' || c > '9')
                        return false;
                }
                return true;
            }
            catch (Exception EX) { WriteLine($"IS_DIGITS_ONLY failed and returned 0. {EX.ToString()}", LOG_TYPE.Warning); return false; }
        }
        public static string SHUFFLE(string str)
        {
            try
            {
                char[] array = str.ToCharArray();
                Random rng = new Random();
                int n = array.Length;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    var value = array[k];
                    array[k] = array[n];
                    array[n] = value;
                }
                return new string(array);
            }
            catch (Exception EX) { WriteLine($"SHUFFLE() failed and returned string.Empty. {EX.ToString()}", LOG_TYPE.Warning); return string.Empty; }
        }
        public static string SHUFFLE_CASES(string str)
        {
            try
            {
                var randomizer = new Random();
                var final = str.Select(x => randomizer.Next() % 2 == 0 ? (char.IsUpper(x) ? x.ToString().ToLower().FirstOrDefault() : x.ToString().ToUpper().FirstOrDefault()) : x);
                var randomUpperLower = new string(final.ToArray());
                return randomUpperLower;
            }
            catch (Exception EX) { WriteLine($"SHUFFLE_CASES() failed and returned string.Empty. {EX.ToString()}", LOG_TYPE.Warning); return string.Empty; }
        }
        //FEATURES
        public static int Is_ADV_USEABLE(string CodeName128)
        {
            //Items available for adv usage are:
            //Weapons,Set,Acc
            try
            {
                int splitter = 0;
                if (CodeName128.Contains("CLOTHES") || CodeName128.Contains("LIGHT") || CodeName128.Contains("HEAVY"))//set types
                    splitter = 4;
                else if (CodeName128.Contains("SWORD") || CodeName128.Contains("BLADE") || CodeName128.Contains("SPEAR") || CodeName128.Contains("TBLADE") || CodeName128.Contains("BOW") || CodeName128.Contains("TSWORD")
                    || CodeName128.Contains("AXE") || CodeName128.Contains("DARKSTAFF") || CodeName128.Contains("TSTAFF") || CodeName128.Contains("CROSSBOW") || CodeName128.Contains("DAGGER") || CodeName128.Contains("HARP")
                    || CodeName128.Contains("STAFF") || CodeName128.Contains("SHIELD"))//weapon types
                    splitter = 3;
                else if (CodeName128.Contains("EARRING") || CodeName128.Contains("NECKLACE") || CodeName128.Contains("RING"))//acc types
                    splitter = 3;
                else splitter = 0;
                return splitter;
            }
            catch (Exception EX) { WriteLine($"Is_ADV_USEABLE() failed and returned 0. {EX.ToString()}", LOG_TYPE.Warning); return 0; }
        }
        public static bool IsSocketConnected(Socket s)
        {
            try
            {
                if (s == null || !s.Connected)
                    return false;
                bool part1 = s.Poll(1000, SelectMode.SelectRead);
                bool part2 = (s.Available == 0);
                if (part1 && part2)
                    return false;
                else
                    return true;
            }
            catch (Exception EX) { WriteLine($"IsSocketConnected() failed. {EX.ToString()}", LOG_TYPE.Warning); return false; }
        }
        public static void ASSIGN_CORRESPONDING_GW(Socket CLIENT_SOCKET, uint AG_TokenID, string AG_ModuleIP)
        {
            try
            {
                //checking if the Disposed sockets dictionary contains any elements at all
                if (ASYNC_SERVER.DISPOSED_GW_SESSIONS != null && ASYNC_SERVER.DISPOSED_GW_SESSIONS.Count > 0)
                {
                    //looping through all of our gateway module class objects...
                    foreach (GATEWAY_MODULE GW in ASYNC_SERVER.DISPOSED_GW_SESSIONS.Values.Where(x => x != null))
                    {
                        //comparing our current client TokenID to any of our disposed gateway modules dictionary...
                        if (GW.TOKEN_ID == AG_TokenID)
                        {
                            //If we found a matching set of TokenIDs, they are linked together in the same login connection session
                            //In case of multiple agentservers, each of the agentservers can generate the same TokenID
                            //comparing the current matching session redirection IP to the client latest connection to our agent module
                            //UTILS.WriteLine($"[{GW.REDIR_IP}:{AG_ModuleIP}]");
                            if (GW.REDIR_IP == AG_ModuleIP)
                            {
                                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET))
                                ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CORRESPONDING_GW_SESSION = GW;//assigning

                                if (!Settings.LOG_PROXY_ERRORS)
                                {
                                    //Getting the disposed socket dict object to remove it since we assigned it and were done with it.
                                    Socket SocketKey = ASYNC_SERVER.DISPOSED_GW_SESSIONS.Where(x => x.Value == GW).FirstOrDefault().Key;
                                    if (SocketKey != null)
                                        ASYNC_SERVER.DISPOSED_GW_SESSIONS.TryRemove(SocketKey, out GATEWAY_MODULE REMOVED_GW_SESSION);
                                    //Removing the gw session from the disposed gw list. 
                                }
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception EX) { WriteLine($"ASSIGN_CORRESPONDING_GW() failed. {EX.ToString()}", LOG_TYPE.Warning); return; }
        }
        public static void AI_DISCOVER_AGS(string IP, int Port)
        {
            try
            {
                if (!ASYNC_SERVER.AGENT_MODULES.Contains(IP + ":" + Port))
                    ASYNC_SERVER.AGENT_MODULES.Add(IP + ":" + Port);
            }
            catch (Exception EX) { WriteLine($"AI_DISCOVER_AGS() failed. {EX.ToString()}", LOG_TYPE.Warning); return; }
        }

        public static void BLOCK_IP(string IP)
        {
            try
            {
                BAN_LIST.Add(IP);
                INetFwPolicy2 policy2 = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                foreach (INetFwRule item in policy2.Rules)
                {
                    if (item.Name == "FEIXUE BLOCKED: " + IP)
                    {
                        return;
                    }
                }
                INetFwRule netFwRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule"));
                netFwRule.Name = "FEIXUE BLOCKED: " + IP;
                netFwRule.Description = "BLOCKED VIA FEIXUE";
                netFwRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                netFwRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                netFwRule.Enabled = true;
                netFwRule.InterfaceTypes = "All";
                netFwRule.RemoteAddresses = IP;
                policy2.Rules.Add(netFwRule);
                WriteLine($"Banned [IP:{IP}]", LOG_TYPE.Notify);
            }
            catch(Exception ex) { WriteLine($"Failed banning through firewall!:{ex.ToString()}", LOG_TYPE.Fatal); }
        }
        public static void CLEAR_BLOCK_IP()
        {
            try
            {
                INetFwPolicy2 policy2 = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                //检查是否有同名规则
                foreach (INetFwRule item in policy2.Rules)
                {
                    if (item.Name.Contains("FEIXUE BLOCKED:"))
                    {
                        policy2.Rules.Remove(item.Name);
                    }
                }
            }
            catch (Exception ex) { WriteLine($"Clear Firewall bans failed!:{ex.ToString()}", LOG_TYPE.Fatal); }
        }
        public static float TO_GAME_X(float X, byte Xsector)
        {
            return ((Xsector - 135) * 192 + (X / 10));
        }
        public static float TO_GAME_Y(float Y, byte Ysector)
        {
            return ((Ysector - 92) * 192 + (Y / 10));
        }
        //PREMIUM VER HELPERS
        public static void DUMP_MODULE_LATEST_TRAFFIC_DIAG(Stack<Packet> PQ, bool ExportToFile)
        {
            try
            {
                //This function will print the latest recved traffic that reached the asynchrnous security module
                //it could be a collection of numerous clients, which is over-writing each other.
                Settings.MAIN.DiagLog.Clear();
                List<Packet> pl = new List<Packet>();
                for (int i = 0; i < PQ.Count; i++)
                {
                    pl.Add(PQ.Pop());//returns the element on the top of the stack
                }
                foreach (var p in pl.ToArray().Reverse())
                {
                    Settings.MAIN.DiagLog.AppendText(string.Format("[0x{0:X4}][{1} bytes]{2}{3}{4}{5}", p.Opcode, p.GetBytes().Length, p.Encrypted ? "[Encrypted]" : "", p.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(p.GetBytes())) + "\n");
                    //exporting to file
                    if (ExportToFile)
                    {
                        using (StreamWriter file = new StreamWriter("Logs/MODULE_PACKET_DUMP_REPORT.txt", true))
                        {
                            file.WriteLineAsync(string.Format("[0x{0:X4}][{1} bytes]{2}{3}{4}{5}", p.Opcode, p.GetBytes().Length, p.Encrypted ? "[Encrypted]" : "", p.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(p.GetBytes())) + "\n");
                            file.Close();
                        }
                    }
                }
            }
            catch (Exception EX) { WriteLine($"DUMP_MODULE_LATEST_TRAFFIC_DIAG() failed. {EX.ToString()}", LOG_TYPE.Warning); return; }
        }
        //STARTUP HELPERS
        public static async Task INSERT_PLUS_NOTICE_DFEAULT_RECORDS()
        {
            try
            {
                string path = Environment.CurrentDirectory + "\\Prerequisites\\_ItemPoolName.txt";
                string[] lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    string[] items = line.Split('	');
                    if (items[0].ToLower().Contains("null") || items[1].ToLower().Contains("null"))
                        continue;
                    else
                        await QUERIES.FILL_ITEMPOOL_TABLE(items[0], QUERIES.INJECTION_PREFIX(items[1]));
                    //WriteLine("",string.Format("{0} => {1}", items[0], items[1]));
                }
            }
            catch (Exception EX) { WriteLine($"INSERT_PLUS_NOTICE_DFEAULT_RECORDS has failed. {EX.ToString()}", LOG_TYPE.Warning); }
        }
        
        //Security API HELPERS
        public static Socket[] GetCorrespondingSecurityModules(Security sec)
        {
            try
            {
                foreach (var item in ASYNC_SERVER.GW_CONS)
                    if (item.Value.LOCAL_SECURITY == sec)
                    {
                        //UTILS.WriteLine(UTILS.LOG_TYPE.Notify, $"GW: CS:{item.Key},PS:{item.Value.PROXY_SOCKET}");
                        return new Socket[] { item.Key, item.Value.PROXY_SOCKET };
                    }
                    else if (item.Value.REMOTE_SECURITY == sec)
                    {
                        //UTILS.WriteLine(UTILS.LOG_TYPE.Notify, $"GW: CS:{item.Key},PS:{item.Value.PROXY_SOCKET}");
                        return new Socket[] { item.Key, item.Value.PROXY_SOCKET };
                    }
                foreach (var item in ASYNC_SERVER.DW_CONS)
                    if (item.Value.LOCAL_SECURITY == sec)
                    {
                        //UTILS.WriteLine(UTILS.LOG_TYPE.Notify, $"DW: CS:{item.Key},PS:{item.Value.PROXY_SOCKET}");
                        return new Socket[] { item.Key, item.Value.PROXY_SOCKET };
                    }
                foreach (var item in ASYNC_SERVER.AG_CONS)
                    if (item.Value.LOCAL_SECURITY == sec)
                    {
                        //UTILS.WriteLine(UTILS.LOG_TYPE.Notify, $"AG: CS:{item.Key},PS:{item.Value.PROXY_SOCKET}");
                        return new Socket[] { item.Key, item.Value.PROXY_SOCKET };
                    }
                return null;
            }
            catch { return null; }
        }
        #endregion

        #region ETC / CALCULATIONS
        public static long CalculateTotalNetworkUsage()
        {
            try
            {
                long TOTAL_BYTES = 0;
                foreach (var session in ASYNC_SERVER.DW_CONS.Values)
                {
                    TOTAL_BYTES += session.TOT_BYTES_CNT;
                }
                foreach (var session in ASYNC_SERVER.GW_CONS.Values)
                {
                    TOTAL_BYTES += session.TOT_BYTES_CNT;
                }
                foreach (var session in ASYNC_SERVER.AG_CONS.Values)
                {
                    TOTAL_BYTES += session.TOT_BYTES_CNT;
                }
                return TOTAL_BYTES;
            }
            catch { WriteLine("An error occured while calculating total traffic", LOG_TYPE.Warning); return 0; }
        }
        //Botsengine extension related funcs
        public static int RoundUpByTen(int num)
        {
            //rounding the given number up to the nearest 10
            return (int)(Math.Ceiling(num / 10.0d) * 10);
        }
        public static long RoundHighNumber(long num)
        {
            //pow with int params
            int cnt = 0;
            while (num > 100)
            {
                num /= 10;
                cnt++;
            }
            for (int i = 0; i < cnt; i++)
            {
                num *= 10;
            }
            return num;
        }
        public static long LongRandom(long min, long max, Random rand)
        {
            //generating a random number dealing with long number
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand % (max - min)) + min);
        }

        public static AGENT_MODULE GetAgentByGID(uint UniqueID)
        {
            AGENT_MODULE Module = null;
            try
            {
                if (ASYNC_SERVER.AG_CONS.Keys != null && ASYNC_SERVER.AG_CONS.Keys.Count > 0)
                {
                    foreach (KeyValuePair<Socket, AGENT_MODULE> con in ASYNC_SERVER.AG_CONS)
                    {
                        if (con.Value.UNIQUE_ID == UniqueID)
                            Module = con.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Module = null;
                UTILS.WriteLine($"GetAgentByGID : excption {ex}",LOG_TYPE.Fatal);
            }

            return Module;
        }

        public static string FormatNumber(long num)
        {
            try
            {
                if (num >= 100000000)
                {
                    return (num / 1000000D).ToString("0.#M");
                }
                if (num >= 1000000)
                {
                    return (num / 1000000D).ToString("0.##M");
                }
                if (num >= 100000)
                {
                    return (num / 1000D).ToString("0.#k");
                }
                if (num >= 10000)
                {
                    return (num / 1000D).ToString("0.##k");
                }

                return num.ToString("#,0");
            }
            catch (Exception EX) { UTILS.WriteLine($"FormatNumber operation has failed. {EX.Message}", LOG_TYPE.Fatal); return "0"; }

        }
        public static string ArrayRandByChances(Dictionary<string, int> arr)
        {
            //returns a random action by the given chances, wrriten by guyshitz
            int n, range, sum = 0;

            for (int i = 0; i < arr.Count; i++)
                sum += arr.Values.ElementAt(i);

            if (sum != 100)
                throw new Exception("Chances total must be 100%!");

            Random rnd = new Random();

            n = rnd.Next(1, 100);
            range = 100;

            for (int i = 0; i <= arr.Count; i++)
            {
                range -= arr.ElementAt(i).Value;
                if (n > range)
                    return arr.ElementAt(i).Key;
            }

            return arr.LastOrDefault().Key;
        }
        public static int IntPow(int x, uint pow)
        {
            int ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }
            /// <summary>
            /// MD5　32位加密
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
        public static string UserMd5(string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.Default.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符

                if (s[i] <= 0xF)
                {
                    pwd = pwd + "0" + s[i].ToString("X");

                }
                else {
                    pwd = pwd + s[i].ToString("X");

                }


            }
            return pwd;
        }
        public class ListViewItemComparer : IComparer
        {
            ///   
            /// Specifies the column to be sorted  
            ///   
            private int ColumnToSort;
            ///   
            /// Specifies the order in which to sort (i.e. 'Ascending').  
            ///   
            private System.Data.SqlClient.SortOrder OrderOfSort;
            ///   
            /// Case insensitive comparer object  
            ///   
            private CaseInsensitiveComparer ObjectCompare;

            ///   
            /// Class constructor.  Initializes various elements  
            ///   
            public ListViewItemComparer()
            {
                // Initialize the column to '0'  
                ColumnToSort = 0;

                // Initialize the sort order to 'none'  
                OrderOfSort = System.Data.SqlClient.SortOrder.Unspecified;

                // Initialize the CaseInsensitiveComparer object  
                ObjectCompare = new CaseInsensitiveComparer();
            }

            private int comaretInt = 0;
            ///   
            /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.  
            ///   
            /// First object to be compared  
            /// Second object to be compared  
            /// The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'  
            public int Compare(object x, object y)
            {
                int compareResult;
                ListViewItem listviewX, listviewY;

                // Cast the objects to be compared to ListViewItem objects  
                listviewX = (ListViewItem)x;
                listviewY = (ListViewItem)y;

                if (int.TryParse(listviewX.SubItems[ColumnToSort].Text, out comaretInt) &&
                    int.TryParse(listviewY.SubItems[ColumnToSort].Text, out comaretInt))
                {
                    compareResult = int.Parse(listviewX.SubItems[ColumnToSort].Text).CompareTo(int.Parse(listviewY.SubItems[ColumnToSort].Text));
                }
                else
                {
                    // Compare the two items  
                    compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
                }
                // Calculate correct return value based on object comparison  
                if (OrderOfSort == System.Data.SqlClient.SortOrder.Ascending)
                {
                    // Ascending sort is selected, return normal result of compare operation  
                    return compareResult;
                }
                else if (OrderOfSort == System.Data.SqlClient.SortOrder.Descending)
                {
                    // Descending sort is selected, return negative result of compare operation  
                    return (-compareResult);
                }
                else
                {
                    // Return '0' to indicate they are equal  
                    return 0;
                }

            }

            ///   
            /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').  
            ///   
            public int SortColumn
            {
                set
                {
                    ColumnToSort = value;
                }
                get
                {
                    return ColumnToSort;
                }
            }
            ///   
            /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').  
            ///   
            public System.Data.SqlClient.SortOrder Order
            {
                set
                {
                    OrderOfSort = value;
                }
                get
                {
                    return OrderOfSort;
                }
            }
        }
        #endregion
        /// <summary>
        /// </summary>
        /// <param name="srcBytes">被执行查找的 System.Byte[]。</param>
        /// <param name="searchBytes">要查找的 System.Byte[]。</param>
        /// <returns>如果找到该字节数组，则为 searchBytes 的索引位置；如果未找到该字节数组，则为 -1。如果 searchBytes 为 null 或者长度为0，则返回值为 -1。</returns>
        public static int BytesFind(byte[] srcBytes, byte[] searchBytes)
        {
            if (srcBytes == null) { return -1; }
            if (searchBytes == null) { return -1; }
            if (srcBytes.Length == 0) { return -1; }
            if (searchBytes.Length == 0) { return -1; }
            if (srcBytes.Length < searchBytes.Length) { return -1; }
            for (int i = 0; i < srcBytes.Length - searchBytes.Length; i++)
            {
                if (srcBytes[i] == searchBytes[0])
                {
                    if (searchBytes.Length == 1) { return i; }
                    bool flag = true;
                    for (int j = 1; j < searchBytes.Length; j++)
                    {
                        if (srcBytes[i + j] != searchBytes[j])
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag) { return i; }
                }
            }
            return -1;
        }
        public static int CalculateX(byte xSector, float X)
        {
            return (int)((xSector - 135) * 192 + X / 10);
        }
        public static int CalculateY(byte ySector, float Y)
        {
            return (int)((ySector - 92) * 192 + Y / 10);
        }
        ///<summary>
        ///生成随机字符串 
        ///</summary>
        ///<param name="length">目标字符串的长度</param>
        ///<param name="useNum">是否包含数字，1=包含，默认为包含</param>
        ///<param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
        ///<param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
        ///<param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
        ///<param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
        ///<returns>指定长度的随机字符串</returns>
        public static string GetRandomString(int length, string custom="", bool useNum=true, bool useLow=true, bool useUpp=true, bool useSpe=false)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }
        
       
        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="fileName">文件绝对路径</param>
        /// <returns>MD5值</returns>
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
        /// <summary>
        /// 实现字符串反转
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string 颜色反转(string str)
        {

            string R = str.Substring(4, 2);
            string G = str.Substring(2, 2);
            string B = str.Substring(0, 2);
            return R+G+B;
        }

        public static int GetDigits(string a)
        {
            try
            {
                string b = string.Empty;
                int val;

                for (int i = 0; i < a.Length; i++)
                {
                    if (Char.IsDigit(a[i]))
                        b += a[i];
                }

                if (b.Length > 0)
                {
                    val = int.Parse(b);
                    return val;
                }
                else
                    return 0;

            }
            catch
            {
                return 0;
            }
        }

        public static void BroadCastToClients(Packet pck,int ShardID)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.Keys != null && ASYNC_SERVER.AG_CONS.Keys.Count > 0)
                {
                    foreach (KeyValuePair<Socket, AGENT_MODULE> con in ASYNC_SERVER.AG_CONS.Where(x => x.Value.CHARNAME16.Length > 0 && x.Value.ShardID == ShardID))
                    {
                        con.Value.LOCAL_SECURITY.Send(pck);
                        con.Value.ASYNC_SEND_TO_CLIENT(con.Key);
                    }
                }
            }
            catch (Exception ex)
            {
                UTILS.WriteLine($"BroadCastToClients : excption {ex}");
            }
        }

        public static string INJECTION_PREFIX(string str)
        {
            try
            {
                /* This method will escape special character that are consistent with SQL injection (SQLi)
                or Cross site scripting (XSS) vulnerabilites in case you present any of the data in your website aswell */
                if (!string.IsNullOrEmpty(str))
                {
                    str = str.Replace("'", string.Empty);
                    str = str.Replace(";", string.Empty);
                    str = str.Replace("-", string.Empty);
                    str = str.Replace(@"\", string.Empty);
                    str = str.Replace("%", string.Empty);
                    str = str.Replace("<", string.Empty);
                    str = str.Replace(">", string.Empty);
                }
                return str;
            }
            catch (Exception EX) { WriteLine($"INJECTION_PREFIX failed and returned 0.1 {EX}", LOG_TYPE.Warning); return string.Empty; }
        }

        public static async void SendRank(byte type,int ShardID, ConcurrentDictionary<string, Rank> List)
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
                    BroadCastToClients(packet, ShardID);
                }
            }

            catch (Exception EX) { WriteLine($"SendRank operation has failed. {EX.Message}", LOG_TYPE.Fatal); }
        }
        public static async Task GetWinnerNumbers()
        {
            try
            {
                XSMB.Clear();
                ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)((senderX, certificate, chain, sslPolicyErrors) => true);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (WebClient wc = new WebClient())
                {
                    XSMB_DATE = DateTime.Now.ToString("dd:MM:yyyy");
                    wc.Encoding = Encoding.UTF8;
                    string pubIp = wc.DownloadString("https://xskt.com.vn/rss-feed/mien-bac-xsmb.rss");
                    int index = pubIp.IndexOf("ĐB: ");
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 7, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 16, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 25, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 33, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 42, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 50, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 58, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 66, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 74, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 82, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 90, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 97, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 104, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 111, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 119, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 126, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 133, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 140, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 147, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 154, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 161, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 167, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 173, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 179, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 184, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 189, 2)));
                    XSMB.Add(Convert.ToByte(pubIp.Substring(index + 194, 2)));
                }
            }
            catch (Exception ex)
            {
                UTILS.WriteLine($"Failed to get xsmb winners {ex.ToString()}");
            }
        }
        public static async Task FWKillsUpdate()
        {
            try
            {
                if (UTILS.FWKills.Count > 0)
                {
                    var top5 = UTILS.FWKills.OrderByDescending(o => o.Value).Take(5).ToList();
                    Packet logpacket = new Packet(0x5122);
                    logpacket.WriteValue<byte>(top5.Count);
                    foreach (var line in top5)
                    {
                        logpacket.WriteValue<string>(line.Key);
                        logpacket.WriteValue<string>(UTILS.FormatNumber(line.Value));
                    }

                    IEnumerable<AGENT_MODULE> source = from x in ASYNC_SERVER.AG_CONS.Values
                                                      where x.INSIDE_FW
                                                      select x;
                    Parallel.ForEach<AGENT_MODULE>(source, delegate (AGENT_MODULE client)
                    {
                        client.LOCAL_SECURITY.Send(logpacket);
                        client.ASYNC_SEND_TO_CLIENT(client.CLIENT_SOCKET);
                        client.FWKillsGui = true;
                    });
                }
            }
            catch
            {

            }

        }

        public static void SendNoticeForAll(NoticeType Type, string Message, uint Color = 0, string Key = "GM", string Code = "NUll")
        {
            
            try
            {
                //Stopwatch sw = Stopwatch.StartNew();
                Packet Notice = new Packet(0x5102);
                Notice.WriteValue<byte>(Type);
                Notice.WriteAsciiA(Message);

                if (ASYNC_SERVER.AG_CONS.Keys != null && ASYNC_SERVER.AG_CONS.Keys.Count > 0)
                {
                    switch (Type)
                    {
                        case NoticeType.ColoredSystem:
                            Notice.WriteValue<uint>(Color);
                            break;
                        case NoticeType.ColoredChat:
                            Notice.WriteValue<uint>(Color);
                            Notice.WriteValue<string>(Key);
                            break;
                        case NoticeType.Alchemy:
                            Notice.WriteValue<string>(Code);
                            break;
                    }

                    foreach (KeyValuePair<Socket, AGENT_MODULE> con in ASYNC_SERVER.AG_CONS.Where(x => !string.IsNullOrEmpty(x.Value.CHARNAME16)))
                    {
                        ASYNC_SERVER.AG_CONS[con.Key].LOCAL_SECURITY.Send(Notice);
                        ASYNC_SERVER.AG_CONS[con.Key].ASYNC_SEND_TO_CLIENT(con.Key);
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"SendNoticeForAll operation has failed. {EX.Message}", LOG_TYPE.Fatal); }
        }

        public static void SendNotice(NoticeType Type, string Message, Socket CLIENT_SOCKET, uint Color = 0, string Key = "GM", string Code = "NUll")
        {
            
            try
            {
                // Stopwatch sw = Stopwatch.StartNew();
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16_HOLDER))
                {
                    Packet Notice = new Packet(0x5102);
                    Notice.WriteValue<byte>(Type);
                    Notice.WriteAsciiA(Message);

                    switch (Type)
                    {
                        case NoticeType.ColoredSystem:
                            Notice.WriteValue<uint>(Color);
                            break;
                        case NoticeType.ColoredChat:
                            Notice.WriteValue<uint>(Color);
                            Notice.WriteValue<string>(Key);
                            break;
                        case NoticeType.Alchemy:
                            Notice.WriteValue<string>(Code);
                            break;
                    }

                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(Notice);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"SendNotice operation has failed. {EX.Message}", LOG_TYPE.Error); }
        }



        public static void ServerRankConfig(int RankID, string RankName, Socket CLIENT_SOCKET)
        {

            try
            {
                // Stopwatch sw = Stopwatch.StartNew();
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16_HOLDER))
                {
                    Packet packet = new Packet(0x3226);
                    packet.WriteAsciiA(RankName);


                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(packet);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"ServerRankConfig operation has failed. {EX.Message}", LOG_TYPE.Error); }
        }

        public static void ServerRankLoad(int Rank, string CharName,string Guild, string Points , int CharIcon,int imageId,int frameId, Socket CLIENT_SOCKET)
        {

            try
            {
                // Stopwatch sw = Stopwatch.StartNew();
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16_HOLDER))
                {
                    Packet packet = new Packet(0x3225);
                    packet.WriteValue<int>(Rank);
                    packet.WriteValue<string>(CharName);
                    packet.WriteValue<string>(Guild);
                    packet.WriteValue<string>(Points);
                    packet.WriteValue<int>(CharIcon);
                    packet.WriteValue<int>(imageId);
                    packet.WriteValue<int>(frameId);
                    
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(packet);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"ServerRankConfig operation has failed. {EX.Message}", LOG_TYPE.Error); }
        }

        public static void NewEventTimer(int Slot, int BanarID, string EventName, int Leftsecounds, string Date, string Details, int Status, int Registers, Socket CLIENT_SOCKET, bool ToAll)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.Keys != null && ASYNC_SERVER.AG_CONS.Keys.Count > 0)
                {
                    Packet packet = new Packet(0x3232);
                    packet.WriteUInt32(Slot);
                    packet.WriteUInt32(BanarID);
                    packet.WriteAscii(EventName);
                    packet.WriteUInt32(Leftsecounds);
                    packet.WriteAscii(Date);
                    packet.WriteAscii(Details);
                    packet.WriteUInt32(Status);
                    packet.WriteUInt32(Registers);
                    if (!ToAll)
                    {
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(packet);
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);


                    }
                    else
                    {
                        foreach (KeyValuePair<Socket, AGENT_MODULE> con in ASYNC_SERVER.AG_CONS.Where(x => !string.IsNullOrEmpty(x.Value.CHARNAME16)))
                        {
                            ASYNC_SERVER.AG_CONS[con.Key].LOCAL_SECURITY.Send(packet);
                            ASYNC_SERVER.AG_CONS[con.Key].ASYNC_SEND_TO_CLIENT(con.Key);
                        }
                    }


                }

            }
            catch (Exception EX) { WriteLine($"EventTimer operation has failed. {EX.Message}", LOG_TYPE.Error); }
        }

        public static void MonsterUniqueAppeared(int UniqueID, Socket CLIENT_SOCKET)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.Keys != null && ASYNC_SERVER.AG_CONS.Keys.Count > 0)
                {
                    Packet Notice = new Packet(0x3229);
                    Notice.WriteUInt32(UniqueID);

                    if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) && !string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16_HOLDER))
                    {
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(Notice);
                        ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine($"An error occurred in MonsterUniqueAppeared: {ex.Message}", LOG_TYPE.Error);
            }
        }
        public static void MonsterUniqueKilled(int MonstersKillCount, int UniqueID, int KillerObj, string KillerName)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.Keys != null && ASYNC_SERVER.AG_CONS.Keys.Count > 0)
                {
                    Packet Notice = new Packet(0x3230);
                    Notice.WriteUInt32(MonstersKillCount);
                    Notice.WriteUInt32(UniqueID);
                    Notice.WriteUInt32(KillerObj);
                    Notice.WriteAscii(KillerName);

                    foreach (KeyValuePair<Socket, AGENT_MODULE> con in ASYNC_SERVER.AG_CONS.Where(x => x.Value.INEVENT_EVENTID == 0 && x.Value.FortressRegion == 0))
                    {
                        ASYNC_SERVER.AG_CONS[con.Key].LOCAL_SECURITY.Send(Notice);
                        ASYNC_SERVER.AG_CONS[con.Key].ASYNC_SEND_TO_CLIENT(con.Key);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine($"An error occurred in MonsterUniqueAppeared: {ex.Message}", LOG_TYPE.Error);
            }
        }
        public static void SendUniqueLog(Socket CLIENT_SOCKET)
        {
            try
            {
                if (UTILS.UniqueLog.Count == 0 || ASYNC_SERVER.AG_CONS.Count == 0)
                    return;

                DateTime now = DateTime.Now;

                Packet logPacket = new Packet(0x5130);
                logPacket.WriteUInt8(UTILS.UniqueLog.Count);

                foreach (var line in UTILS.UniqueLog)
                {
                    DateTime killedTime = DateTime.Parse(line.Value.Time);
                    TimeSpan span = now - killedTime;

                    int days = span.Days;
                    int hours = span.Hours;
                    int minutes = span.Minutes;

                    string timer = $"{days}day {hours}h {minutes}m before";

                    logPacket.WriteUInt8(line.Value.PageID);
                    logPacket.WriteUInt8(line.Key);
                    logPacket.WriteInt32(line.Value.UniqueID);
                    logPacket.WriteUInt8(line.Value.Status);
                    logPacket.WriteAscii(line.Value.Killer);
                    logPacket.WriteAscii(timer);
                    logPacket.WriteInt32(line.Value.RegionID);
                    logPacket.WriteInt32(line.Value.PostionX);
                    logPacket.WriteInt32(line.Value.PostionZ);
                    logPacket.WriteInt32(line.Value.CircaleRadius);
                    logPacket.WriteInt32(line.Value.Image);
                }

                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET))
                {
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(logPacket);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }
            catch (Exception ex)
            {
                WriteLine($"SendUniqueLog operation failed: {ex.Message}", LOG_TYPE.Error);
            }
        }


        public static void HallOfFameSend(uint ID,uint Rank,string CharName,string Points,uint CharIcon,uint FL_1,uint FL_9,uint FL_49,uint FL_99,uint FL_499,uint FL_999,Socket CLIENT_SOCKET)
        {
            try
            {
                if (ASYNC_SERVER.AG_CONS.ContainsKey(CLIENT_SOCKET) &&!string.IsNullOrEmpty(ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].CHARNAME16_HOLDER))
                {
                    Packet packet = new Packet(0x3227);
                    packet.WriteUInt32(ID);
                    packet.WriteUInt32(Rank);
                    packet.WriteUInt32(CharIcon);
                    packet.WriteAscii(CharName);
                    packet.WriteAscii(Points);
                    packet.WriteUInt32(FL_1);
                    packet.WriteUInt32(FL_9);
                    packet.WriteUInt32(FL_49);
                    packet.WriteUInt32(FL_99);
                    packet.WriteUInt32(FL_499);
                    packet.WriteUInt32(FL_999);

                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].LOCAL_SECURITY.Send(packet);
                    ASYNC_SERVER.AG_CONS[CLIENT_SOCKET].ASYNC_SEND_TO_CLIENT(CLIENT_SOCKET);
                }
            }
            catch (Exception ex)
            {
                UTILS.WriteLine($"HallOfFameSend failed: {ex.Message}", LOG_TYPE.Error);
            }
        }

    }

}