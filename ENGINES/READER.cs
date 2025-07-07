using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SR_PROXY.ENGINES
{
    class READER
    {
        public static string[] LINES;
        public static List<ushort> MALICIOUS_OPCODES = new List<ushort>();
        public static List<string> FILTER_KEYWORDS = new List<string>();
        public static List<string> NETCAFE_IPS = new List<string>();

        public static List<string> BLOCKED_SKILL_IDS = new List<string>();
        public static List<string> FW_BLOCKED_SKILL_IDS = new List<string>();
        public static List<string> CTF_BLOCKED_SKILL_IDS = new List<string>();
        public static List<string> BA_BLOCKED_SKILL_IDS = new List<string>();
        public static List<string> JOB_BLOCKED_SKILL_IDS = new List<string>();
        //region IDs
        public static List<short> FW_WREGION_ID = new List<short>();
        public static List<short> FGW_WREGION_ID = new List<short>();
        public static List<short> CTF_WREGION_ID = new List<short>();
        public static List<short> BA_WREGION_ID = new List<short>();
        public static List<short> TOWNS_WREGION_ID = new List<short>();
        //teleport IDs are gate IDs
        public static List<short> FW_TELEPORT_ID = new List<short>();
        public static List<short> JC_TELEPORT_ID = new List<short>();

        public static List<string> RETYPE_NAMES = new List<string>();
        public static List<string> TRIVIA_QA = new List<string>();
        public static List<string> FIRSTTYPE_NAMES = new List<string>();

        public static int GET_LINE_NR_BY_STRING(string[] LINES, string FIND)
        {
            try
            {
                int counter = 0;
                foreach (var x in LINES)
                {
                    counter++;
                    if (x.Contains(FIND))
                    {
                        break;
                    }
                }
                return counter;
            }
            catch { return 0; }
        }
        public static bool READ_REQ_INIT()
        {
            try
            {
                if (!File.Exists(".\\proxy_cfg.ini"))
                {
                    UTILS.WriteLine("NETWORK/MSSQL configurations in proxy_cfg.ini doesn't exist.");
                    return false;
                }
                LINES = File.ReadAllLines(".\\proxy_cfg.ini");
                string[] index;
                for (int i = 0; i < LINES.Length; i++)
                {
                    #region [PROXY FRAMEWORK]
                    if (LINES[i] == "[PROXY FRAMEWORK]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        string BIND_IP = index[1];
                        index = LINES[i + 2].Split(new char[] { '=' });
                        bool OUTSOURCE_NETWORKING = bool.Parse(index[1]);
                        Settings.BIND_IP = BIND_IP;
                        Settings.MAIN.textBox1.Text = BIND_IP;
                        Settings.OUTSOURCE_NETWORKING = Convert.ToBoolean(OUTSOURCE_NETWORKING);
                        Settings.MAIN.checkBox14.Checked = Convert.ToBoolean(OUTSOURCE_NETWORKING);
                    }
                    #endregion
                    #region [GATEWAYSERVER_DEFAULT]
                    if (LINES[i] == "[GATEWAYSERVER_DEFAULT]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        string PUBLIC_GW_IP = index[1];
                        index = LINES[i + 2].Split(new char[] { '=' });
                        int PUBLIC_GW_PORT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 3].Split(new char[] { '=' });
                        int PVT_GW_PORT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        Settings.PUBLIC_GW_IP = PUBLIC_GW_IP;
                        Settings.PUBLIC_GW_PORT = PUBLIC_GW_PORT;
                        Settings.MAIN.textBox3.Text = PUBLIC_GW_PORT.ToString();
                        Settings.PVT_GW_PORT = PVT_GW_PORT;
                        Settings.MAIN.textBox4.Text = PVT_GW_PORT.ToString();

                    }
                    #endregion
                    #region [GATEWAYSERVER_DEFAULT]
                    if (LINES[i] == "[DOWNLOADSERVER_DEFAULT]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        string PUBLIC_DW_IP = index[1];
                        index = LINES[i + 2].Split(new char[] { '=' });
                        int PUBLIC_DW_PORT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 3].Split(new char[] { '=' });
                        int PVT_DW_PORT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        Settings.PUBLIC_DW_IP = PUBLIC_DW_IP;
                        Settings.PUBLIC_DW_PORT = PUBLIC_DW_PORT;
                        Settings.MAIN.textBox10.Text = PUBLIC_DW_PORT.ToString();
                        Settings.PVT_DW_PORT = PVT_DW_PORT;
                        Settings.MAIN.textBox9.Text = PVT_DW_PORT.ToString();

                    }
                    #endregion
                    #region [AGENTSERVER_DEFAULT]
                    if (LINES[i] == "[AGENTSERVER_DEFAULT]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        string PUBLIC_AG_IP = index[1];
                        index = LINES[i + 2].Split(new char[] { '=' });
                        int PUBLIC_AG_PORT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 3].Split(new char[] { '=' });
                        int PVT_AG_PORT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        Settings.PUBLIC_AG_IP = PUBLIC_AG_IP;
                        Settings.MAIN.textBox17.Text = PUBLIC_AG_IP;
                        Settings.PUBLIC_AG_PORT = PUBLIC_AG_PORT;
                        Settings.MAIN.textBox6.Text = PUBLIC_AG_PORT.ToString();
                        Settings.PVT_AG_PORT = PVT_AG_PORT;
                        Settings.MAIN.textBox7.Text = PVT_AG_PORT.ToString();
                    }
                    #endregion
                    #region [AGENTSERVER_DEFAULT2]
                    if (LINES[i] == "[AGENTSERVER_DEFAULT2]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        bool AG2_ALLOW = Convert.ToBoolean(index[1]);
                        index = LINES[i + 2].Split(new char[] { '=' });
                        string PUBLIC_AG_IP = index[1];
                        index = LINES[i + 3].Split(new char[] { '=' });
                        int PUBLIC_AG_PORT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 4].Split(new char[] { '=' });
                        int PVT_AG_PORT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        Settings.PUBLIC_AG2_IP = PUBLIC_AG_IP;
                        Settings.MAIN.textBox91.Text = PUBLIC_AG_IP;
                        Settings.PUBLIC_AG2_PORT = PUBLIC_AG_PORT;
                        Settings.MAIN.textBox94.Text = PUBLIC_AG_PORT.ToString();
                        Settings.PVT_AG2_PORT = PVT_AG_PORT;
                        Settings.MAIN.textBox93.Text = PVT_AG_PORT.ToString();
                        Settings.MAIN.checkBox5.Checked = AG2_ALLOW;
                    }

                    #endregion
                    #region [AGENTSERVER_DEFAULT3]
                    if (LINES[i] == "[AGENTSERVER_DEFAULT3]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        bool AG3_ALLOW = Convert.ToBoolean(index[1]);
                        index = LINES[i + 2].Split(new char[] { '=' });
                        string PUBLIC_AG_IP = index[1];
                        index = LINES[i + 3].Split(new char[] { '=' });
                        int PUBLIC_AG_PORT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 4].Split(new char[] { '=' });
                        int PVT_AG_PORT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        Settings.PUBLIC_AG3_IP = PUBLIC_AG_IP;
                        Settings.MAIN.textBox92.Text = PUBLIC_AG_IP;
                        Settings.PUBLIC_AG3_PORT = PUBLIC_AG_PORT;
                        Settings.MAIN.textBox100.Text = PUBLIC_AG_PORT.ToString();
                        Settings.PVT_AG3_PORT = PVT_AG_PORT;
                        Settings.MAIN.textBox97.Text = PVT_AG_PORT.ToString();
                        Settings.MAIN.checkBox6.Checked = AG3_ALLOW;

                    }
                    #endregion
                    #region [AGENTSERVER_DEFAULT4]
                    if (LINES[i] == "[AGENTSERVER_DEFAULT4]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        bool AG4_ALLOW = Convert.ToBoolean(index[1]);
                        index = LINES[i + 2].Split(new char[] { '=' });
                        string PUBLIC_AG_IP = index[1];
                        index = LINES[i + 3].Split(new char[] { '=' });
                        int PUBLIC_AG_PORT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 4].Split(new char[] { '=' });
                        int PVT_AG_PORT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        Settings.PUBLIC_AG4_IP = PUBLIC_AG_IP;
                        Settings.MAIN.textBox111.Text = PUBLIC_AG_IP;
                        Settings.PUBLIC_AG4_PORT = PUBLIC_AG_PORT;
                        Settings.MAIN.textBox96.Text = PUBLIC_AG_PORT.ToString();
                        Settings.PVT_AG4_PORT = PVT_AG_PORT;
                        Settings.MAIN.textBox95.Text = PVT_AG_PORT.ToString();
                        Settings.MAIN.checkBox27.Checked = AG4_ALLOW;

                    }
                    #endregion
                    #region [MSSQL Server]
                    if (LINES[i] == "[MSSQL Server]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        string MSSQL_SVR_NAME = index[1];
                        index = LINES[i + 2].Split(new char[] { '=' });
                        string MSSQL_SVR_ID = index[1];
                        index = LINES[i + 3].Split(new char[] { '=' });
                        string MSSQL_SVR_PW = index[1];
                        index = LINES[i + 4].Split(new char[] { '=' });
                        string MSSQL_LOG_DB = index[1];
                        index = LINES[i + 5].Split(new char[] { '=' });
                        string MSSQL_ACC_DB = index[1];
                        index = LINES[i + 6].Split(new char[] { '=' });
                        string MSSQL_SHARD_DB = index[1];


                        Settings.MSSQL_SVR_NAME = MSSQL_SVR_NAME;
                        Settings.MAIN.textBox11.Text = MSSQL_SVR_NAME;
                        Settings.MSSQL_SVR_ID = MSSQL_SVR_ID;
                        Settings.MAIN.textBox12.Text = MSSQL_SVR_ID;
                        Settings.MSSQL_SVR_PW = MSSQL_SVR_PW;
                        Settings.MAIN.textBox13.Text = MSSQL_SVR_PW;
                        Settings.MSSQL_LOG_DB = MSSQL_LOG_DB;
                        Settings.MAIN.textBox14.Text = MSSQL_LOG_DB;
                        Settings.MSSQL_ACC_DB = MSSQL_ACC_DB;
                        Settings.MAIN.textBox15.Text = MSSQL_ACC_DB;
                        Settings.MSSQL_SHARD_DB = MSSQL_SHARD_DB;
                        Settings.MAIN.textBox16.Text = MSSQL_SHARD_DB;
                        //UTILS.WriteLine(Settings.MSSQL_SVR_NAME+ Settings.MSSQL_SVR_ID+ Settings.MSSQL_SVR_PW+ Settings.MSSQL_LOG_DB+ Settings.MSSQL_ACC_DB+ Settings.MSSQL_SHARD_DB+ Settings.MSSQL_BACKUP_PATH);
                    }
                    #endregion
                }
                //UTILS.WriteLine("ASYNC_SVR_PARAMS loaded in [{0} ms]", s2.ElapsedMilliseconds);
                return true;
            }
            catch { return false; }
        }
        public static bool READ_FEATURES()
        {
            try
            {
                if (!File.Exists(".\\proxy_cfg.ini"))
                {
                    UTILS.WriteLine("Settings configurations in proxy_cfg.ini doesn't exist!");
                    return false;
                }
                LINES = File.ReadAllLines(".\\proxy_cfg.ini");
                int TOTAL_LINES_NR = (LINES.Count());
                int CUSTOM_UNIQUES_SPECIFIC_LINES = (GET_LINE_NR_BY_STRING(LINES, "[CUSTOM UNIQUES]"));
                int CUSTOM_UNIQUES_EVENT_SPECIFIC_LINES = (GET_LINE_NR_BY_STRING(LINES, "[CUSTOM UNIQUE EVENT]"));
                string[] index;
                for (int i = 0; i < LINES.Length; i++)
                {
                    #region [MISCELLANEOUS]
                    if (LINES[i] == "[MISCELLANEOUS]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        bool PLAYER_LOGON_MSG = bool.Parse(index[1]);
                        index = LINES[i + 2].Split(new char[] { '=' });
                        bool WELCOME_MSG = bool.Parse(index[1]);
                        index = LINES[i + 3].Split(new char[] { '=' });
                        string SHARD_NAME = (index[1]);
                        index = LINES[i + 4].Split(new char[] { '=' });
                        int FAKE_PLAYERS = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 5].Split(new char[] { '=' });
                        bool REGION_READER = bool.Parse(index[1]);
                        index = LINES[i + 6].Split(new char[] { '=' });
                        bool GMS_ANTI_INVIS = bool.Parse(index[1]);
                        index = LINES[i + 7].Split(new char[] { '=' });
                        bool CHAT_FILTER = bool.Parse(index[1]);
                        index = LINES[i + 8].Split(new char[] { '=' });
                        bool BOT_DETECTOR = bool.Parse(index[1]);
                        index = LINES[i + 9].Split(new char[] { '=' });
                        bool CAPTCHA_REMOVE = bool.Parse(index[1]);
                        index = LINES[i + 10].Split(new char[] { '=' });
                        string CAPTCHA_VALUE = (index[1]);
                        index = LINES[i + 11].Split(new char[] { '=' });
                        bool GAME_GUIDE_DISABLE = bool.Parse(index[1]);
                        index = LINES[i + 12].Split(new char[] { '=' });
                        bool UNQ_KILL_SILK_REWARD = bool.Parse(index[1]);
                        //index = LINES[i + 18].Split(new char[] { '=' });
                        //int TG_REWARD = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        //index = LINES[i + 19].Split(new char[] { '=' });
                        //int CERB_REWARD = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        //index = LINES[i + 20].Split(new char[] { '=' });
                        //int CI_REWARD = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        //index = LINES[i + 21].Split(new char[] { '=' });
                        //int URU_REWARD = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        //index = LINES[i + 22].Split(new char[] { '=' });
                        //int ISY_REWARD = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        //index = LINES[i + 23].Split(new char[] { '=' });
                        //int LY_REWARD = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        //index = LINES[i + 24].Split(new char[] { '=' });
                        //int DS_REWARD = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        //index = LINES[i + 25].Split(new char[] { '=' });
                        //int BY_REWARD = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        //index = LINES[i + 26].Split(new char[] { '=' });
                        //int ROC_REWARD = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        //index = LINES[i + 27].Split(new char[] { '=' });
                        //int WK_90_REWARD = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        //index = LINES[i + 28].Split(new char[] { '=' });
                        //int WK_100_REWARD = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        //index = LINES[i + 29].Split(new char[] { '=' });
                        //int SOSO_BK_REWARD = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 13].Split(new char[] { '=' });
                        int SCHEDULED_NOTICE_EVERY = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        //index = LINES[i + 41].Split(new char[] { '=' });
                        //string SCHEDULED_NOTICE_MESSAGE = (index[1]);
           
                        index = LINES[i + 14].Split(new char[] { '=' });
                        string LOGIN_WELCOME_MSG = (index[1]);


                        Settings.PLAYER_LOGON_MSG = PLAYER_LOGON_MSG;
                        Settings.MAIN.textBox19.Text = PLAYER_LOGON_MSG.ToString();
                        Settings.WELCOME_MSG = WELCOME_MSG;
                        Settings.MAIN.textBox20.Text = WELCOME_MSG.ToString();
                        Settings.SHARD_NAME = SHARD_NAME;
                        Settings.MAIN.textBox21.Text = SHARD_NAME;
                        Settings.FAKE_PLAYERS = FAKE_PLAYERS;
                        Settings.MAIN.textBox22.Text = FAKE_PLAYERS.ToString();
                        Settings.REGION_READER = REGION_READER;
                        //Settings.MAIN.textBox24.Text = REGION_READER.ToString();
                        Settings.GMS_ANTI_INVIS = GMS_ANTI_INVIS;
                        Settings.MAIN.textBox25.Text = GMS_ANTI_INVIS.ToString();
                        Settings.CHAT_FILTER = CHAT_FILTER;
                        Settings.MAIN.textBox26.Text = CHAT_FILTER.ToString();
                        Settings.BOT_DETECTOR = BOT_DETECTOR;
                        Settings.MAIN.textBox27.Text = BOT_DETECTOR.ToString();
                        Settings.CAPTCHA_REMOVE = CAPTCHA_REMOVE;
                        Settings.MAIN.textBox29.Text = CAPTCHA_REMOVE.ToString();
                        Settings.CAPTCHA_VALUE = CAPTCHA_VALUE;
                        Settings.MAIN.textBox30.Text = CAPTCHA_VALUE.ToString();
                        Settings.GAME_GUIDE_DISABLE = GAME_GUIDE_DISABLE;
                        Settings.MAIN.textBox33.Text = GAME_GUIDE_DISABLE.ToString();
                        Settings.UNQ_KILL_SILK_REWARD = UNQ_KILL_SILK_REWARD;
                        Settings.MAIN.textBox34.Text = UNQ_KILL_SILK_REWARD.ToString();
                        //Settings.TG_REWARD = TG_REWARD;
                        //Settings.MAIN.textBox111.Text = TG_REWARD.ToString();
                        //Settings.CERB_REWARD = CERB_REWARD;
                        //Settings.MAIN.textBox112.Text = CERB_REWARD.ToString();
                        //Settings.CI_REWARD = CI_REWARD;
                        //Settings.MAIN.textBox113.Text = CI_REWARD.ToString();
                        //Settings.URU_REWARD = URU_REWARD;
                        //Settings.MAIN.textBox114.Text = URU_REWARD.ToString();
                        //Settings.ISY_REWARD = ISY_REWARD;
                        //Settings.MAIN.textBox115.Text = ISY_REWARD.ToString();
                        //Settings.LY_REWARD = LY_REWARD;
                        //Settings.MAIN.textBox116.Text = LY_REWARD.ToString();
                        //Settings.DS_REWARD = DS_REWARD;
                        //Settings.MAIN.textBox117.Text = DS_REWARD.ToString();
                        //Settings.BY_REWARD = BY_REWARD;
                        //Settings.MAIN.textBox118.Text = BY_REWARD.ToString();
                        //Settings.ROC_REWARD = ROC_REWARD;
                        //Settings.MAIN.textBox119.Text = ROC_REWARD.ToString();
                        //Settings.WK_90_REWARD = WK_90_REWARD;
                        //Settings.MAIN.textBox120.Text = WK_90_REWARD.ToString();
                        //Settings.WK_100_REWARD = WK_100_REWARD;
                        //Settings.MAIN.textBox121.Text = WK_100_REWARD.ToString();
                        //Settings.SOSO_BK_REWARD = SOSO_BK_REWARD;
                        //Settings.MAIN.textBox122.Text = SOSO_BK_REWARD.ToString();
                        Settings.SCHEDULED_NOTICE_EVERY = SCHEDULED_NOTICE_EVERY;
                        Settings.MAIN.textBox171.Text = SCHEDULED_NOTICE_EVERY.ToString();
                        //Settings.SCHEDULED_NOTICE_MESSAGE = SCHEDULED_NOTICE_MESSAGE;
                        Settings.LOGIN_WELCOME_MSG = LOGIN_WELCOME_MSG;
                        Settings.MAIN.textBox24.Text = LOGIN_WELCOME_MSG;
                    }
                    #endregion
                    #region [PROXY SECURITY]
                    else if (LINES[i] == "[PROXY SECURITY]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        int SERVER_IP_LIMIT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 2].Split(new char[] { '=' });
                        int SERVER_HWID_LIMIT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 3].Split(new char[] { '=' });
                        bool SCA_COUNTRY = bool.Parse(index[1]);
                        index = LINES[i + 4].Split(new char[] { '=' });
                        string SCA_COUNTRY_NAME = index[1];
                        index = LINES[i + 5].Split(new char[] { '=' });
                        int GW_BPS_VALUE = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 6].Split(new char[] { '=' });
                        int AG_BPS_VALUE = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 7].Split(new char[] { '=' });
                        int GW_PPS_VALUE = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 8].Split(new char[] { '=' });
                        int AG_PPS_VALUE = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 9].Split(new char[] { '=' });
                        bool PACKET_PROCESSOR = bool.Parse(index[1]);
                        index = LINES[i + 10].Split(new char[] { '=' });
                        bool MALICIOUS_OPCODE = bool.Parse(index[1]);
                        index = LINES[i + 11].Split(new char[] { '=' });
                        bool PUNISHMENT_BAN = bool.Parse(index[1]);
                        index = LINES[i + 12].Split(new char[] { '=' });
                        bool GM_PRIVG_LVL = bool.Parse(index[1]);

                        Settings.SERVER_IP_LIMIT = SERVER_IP_LIMIT;
                        Settings.MAIN.textBox36.Text = SERVER_IP_LIMIT.ToString();
                        Settings.SERVER_HWID_LIMIT = SERVER_HWID_LIMIT;
                        Settings.MAIN.textBox2.Text = SERVER_HWID_LIMIT.ToString();
                        Settings.SCA_COUNTRY = SCA_COUNTRY;
                        //Settings.MAIN.textBox37.Text = SCA_COUNTRY.ToString();
                        Settings.SCA_COUNTRY_NAME = SCA_COUNTRY_NAME;
                        //Settings.MAIN.textBox38.Text = SCA_COUNTRY_NAME;
                        Settings.GW_BPS_VALUE = GW_BPS_VALUE;
                        Settings.MAIN.textBox39.Text = GW_BPS_VALUE.ToString();
                        Settings.AG_BPS_VALUE = AG_BPS_VALUE;
                        Settings.MAIN.textBox40.Text = AG_BPS_VALUE.ToString();
                        Settings.GW_PPS_VALUE = GW_PPS_VALUE;
                        Settings.MAIN.textBox41.Text = GW_PPS_VALUE.ToString();
                        Settings.AG_PPS_VALUE = AG_PPS_VALUE;
                        Settings.MAIN.textBox42.Text = AG_PPS_VALUE.ToString();
                        Settings.PACKET_PROCESSOR = PACKET_PROCESSOR;
                        Settings.MAIN.textBox43.Text = PACKET_PROCESSOR.ToString();
                        Settings.MALICIOUS_OPCODE = MALICIOUS_OPCODE;
                        Settings.MAIN.textBox44.Text = MALICIOUS_OPCODE.ToString();
                        Settings.PUNISHMENT_BAN = PUNISHMENT_BAN;
                        Settings.MAIN.textBox45.Text = PUNISHMENT_BAN.ToString();
                        Settings.GM_PRIVG_LVL = GM_PRIVG_LVL;
                        Settings.MAIN.textBox46.Text = GM_PRIVG_LVL.ToString();
                        // UTILS.WriteLine(Settings.SERVER_IP_LIMIT.ToString()+Settings.SCA_COUNTRY.ToString()+Settings.SCA_COUNTRY_NAME);

                    }
                    #endregion
                    #region [ACTION DELAYS]
                    else if (LINES[i] == "[ACTION DELAYS]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        int STALL_DELAY = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 2].Split(new char[] { '=' });
                        int GUILD_REQ_DELAY = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 3].Split(new char[] { '=' });
                        int UNION_REQ_DELAY = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 4].Split(new char[] { '=' });
                        int EXCHANGE_REQ_DELAY = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 5].Split(new char[] { '=' });
                        int GLOBAL_CHAT_DELAY = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 6].Split(new char[] { '=' });
                        int EXIT_DELAY = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 7].Split(new char[] { '=' });
                        int RESTART_DELAY = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 8].Split(new char[] { '=' });
                        int ZERK_DELAY = int.Parse(index[1], System.Globalization.NumberStyles.Integer);


                        Settings.STALL_DELAY = STALL_DELAY;
                        Settings.MAIN.textBox47.Text = STALL_DELAY.ToString();
                        Settings.GUILD_REQ_DELAY = GUILD_REQ_DELAY;
                        Settings.MAIN.textBox48.Text = GUILD_REQ_DELAY.ToString();
                        Settings.UNION_REQ_DELAY = UNION_REQ_DELAY;
                        Settings.MAIN.textBox49.Text = UNION_REQ_DELAY.ToString();
                        Settings.EXCHANGE_REQ_DELAY = EXCHANGE_REQ_DELAY;
                        Settings.MAIN.textBox50.Text = EXCHANGE_REQ_DELAY.ToString();
                        Settings.GLOBAL_CHAT_DELAY = GLOBAL_CHAT_DELAY;
                        Settings.MAIN.textBox51.Text = GLOBAL_CHAT_DELAY.ToString();
                        Settings.EXIT_DELAY = EXIT_DELAY;
                        Settings.MAIN.textBox102.Text = EXIT_DELAY.ToString();
                        Settings.RESTART_DELAY = RESTART_DELAY;
                        Settings.MAIN.textBox103.Text = RESTART_DELAY.ToString();
                        Settings.ZERK_DELAY = ZERK_DELAY;
                        Settings.MAIN.textBox104.Text = ZERK_DELAY.ToString();
                    }
                    #endregion
                    #region [REGULAR MODE - LIMITS\REQUIRED LEVEL]
                    else if (LINES[i] == "[REGULAR MODE - LIMITS\\REQUIRED LEVEL]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        int GUILD_MAX_LIMIT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 2].Split(new char[] { '=' });
                        int UNION_MAX_LIMIT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 3].Split(new char[] { '=' });
                        int GLOBAL_REQ_LVL = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 4].Split(new char[] { '=' });
                        int PLUS_MAX_LIMIT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 5].Split(new char[] { '=' });
                        int CTF_REQ_LVL = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 6].Split(new char[] { '=' });
                        int BA_REQ_LVL = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 7].Split(new char[] { '=' });
                        bool ACADEMY_CREATION = bool.Parse(index[1]);
                        index = LINES[i + 8].Split(new char[] { '=' });
                        bool DISABLE_TAX_RATE_CHANGE = bool.Parse(index[1]);
                        index = LINES[i + 9].Split(new char[] { '=' });
                        bool DISABLE_ITEM_OR_GOLD_DROP_INTOWN = bool.Parse(index[1]);
                        index = LINES[i + 10].Split(new char[] { '=' });
                        bool DISABLE_SUMMON_FW_PETS = bool.Parse(index[1]);
                        index = LINES[i + 11].Split(new char[] { '=' });
                        bool DISABLE_FELLOW_UNDER_ZERK = bool.Parse(index[1]);
                        index = LINES[i + 12].Split(new char[] { '=' });
                        bool FW_RES_SCROLL = bool.Parse(index[1]);
                        index = LINES[i + 13].Split(new char[] { '=' });
                        bool FW_TRACE_PREVENTION = bool.Parse(index[1]);
                        index = LINES[i + 14].Split(new char[] { '=' });
                        bool RESTART_DISABLE = bool.Parse(index[1]);
                        index = LINES[i + 15].Split(new char[] { '=' });
                        bool DISABLE_JOB_MODE = bool.Parse(index[1]);
                        index = LINES[i + 16].Split(new char[] { '=' });
                        int DISABLE_ADVANCED_ELIXIR_ON_DEGREE = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 17].Split(new char[] { '=' });
                        int WEAP_MAX_PLUS = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 18].Split(new char[] { '=' });
                        int SET_MAX_PLUS = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 19].Split(new char[] { '=' });
                        int ACC_MAX_PLUS = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 20].Split(new char[] { '=' });
                        int SHIELD_MAX_PLUS = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 21].Split(new char[] { '=' });
                        int DEVIL_MAX_PLUS = int.Parse(index[1], System.Globalization.NumberStyles.Integer);


                        Settings.GUILD_MAX_LIMIT = GUILD_MAX_LIMIT;
                        Settings.MAIN.textBox52.Text = GUILD_MAX_LIMIT.ToString();
                        Settings.UNION_MAX_LIMIT = UNION_MAX_LIMIT;
                        Settings.MAIN.textBox53.Text = UNION_MAX_LIMIT.ToString();
                        Settings.GLOBAL_REQ_LVL = GLOBAL_REQ_LVL;
                        Settings.MAIN.textBox54.Text = GLOBAL_REQ_LVL.ToString();
                        Settings.PLUS_MAX_LIMIT = PLUS_MAX_LIMIT;
                        Settings.MAIN.textBox55.Text = PLUS_MAX_LIMIT.ToString();
                        Settings.CTF_REQ_LVL = CTF_REQ_LVL;
                        Settings.MAIN.textBox56.Text = CTF_REQ_LVL.ToString();
                        Settings.BA_REQ_LVL = BA_REQ_LVL;
                        Settings.MAIN.textBox57.Text = BA_REQ_LVL.ToString();
                        Settings.ACADEMY_CREATION = ACADEMY_CREATION;
                        Settings.MAIN.textBox58.Text = ACADEMY_CREATION.ToString();
                        Settings.DISABLE_TAX_RATE_CHANGE = DISABLE_TAX_RATE_CHANGE;
                        Settings.MAIN.textBox105.Text = DISABLE_TAX_RATE_CHANGE.ToString();
                        Settings.DISABLE_ITEM_OR_GOLD_DROP_INTOWN = DISABLE_ITEM_OR_GOLD_DROP_INTOWN;
                        Settings.MAIN.textBox106.Text = DISABLE_ITEM_OR_GOLD_DROP_INTOWN.ToString();
                        Settings.DISABLE_SUMMON_FW_PETS = DISABLE_SUMMON_FW_PETS;
                        Settings.MAIN.textBox107.Text = DISABLE_SUMMON_FW_PETS.ToString();
                        Settings.DISABLE_FELLOW_UNDER_ZERK = DISABLE_FELLOW_UNDER_ZERK;
                        Settings.MAIN.textBox108.Text = DISABLE_FELLOW_UNDER_ZERK.ToString();
                        Settings.FW_RES_SCROLL = FW_RES_SCROLL;
                        Settings.MAIN.textBox130.Text = FW_RES_SCROLL.ToString();
                        Settings.FW_TRACE_PREVENTION = FW_TRACE_PREVENTION;
                        Settings.MAIN.textBox131.Text = FW_TRACE_PREVENTION.ToString();
                        Settings.RESTART_DISABLE = RESTART_DISABLE;
                        Settings.MAIN.textBox162.Text = RESTART_DISABLE.ToString();
                        Settings.DISABLE_JOB_MODE = DISABLE_JOB_MODE;
                        Settings.MAIN.textBox169.Text = DISABLE_JOB_MODE.ToString();
                        Settings.DISABLE_ADVANCED_ELIXIR_ON_DEGREE = DISABLE_ADVANCED_ELIXIR_ON_DEGREE;
                        Settings.MAIN.textBox170.Text = DISABLE_ADVANCED_ELIXIR_ON_DEGREE.ToString();
                        Settings.MAXPLUSINFO = new int[5] { (WEAP_MAX_PLUS),(SET_MAX_PLUS),(ACC_MAX_PLUS),(SHIELD_MAX_PLUS),(DEVIL_MAX_PLUS)};
                        Settings.MAIN.numericUpDown22.Value = WEAP_MAX_PLUS;
                        Settings.MAIN.numericUpDown23.Value = SET_MAX_PLUS;
                        Settings.MAIN.numericUpDown24.Value = ACC_MAX_PLUS;
                        Settings.MAIN.numericUpDown25.Value = SHIELD_MAX_PLUS;
                        Settings.MAIN.numericUpDown28.Value = DEVIL_MAX_PLUS;
                    }
                    #endregion
                    #region [JOB MODE - LIMITS\DELAYS]
                    else if (LINES[i] == "[JOB MODE - LIMITS\\DELAYS]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        bool JOB_REVERSE_DEATH_POINT = bool.Parse(index[1]);
                        index = LINES[i + 2].Split(new char[] { '=' });
                        bool JOB_REVERSE_LAST_RECALL_POINT = bool.Parse(index[1]);
                        index = LINES[i + 3].Split(new char[] { '=' });
                        bool JOB_RESS_SCROLL = bool.Parse(index[1]);
                        index = LINES[i + 4].Split(new char[] { '=' });
                        bool JOB_ANTI_TRACE = bool.Parse(index[1]);
                        index = LINES[i + 5].Split(new char[] { '=' });
                        int JOB_RESS_ACCEPTION_DELAY = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                        index = LINES[i + 6].Split(new char[] { '=' });
                        bool JOB_GOODS_DROPOUT = bool.Parse(index[1]);
                        index = LINES[i + 7].Split(new char[] { '=' });
                        bool DISABLE_THIEF_REWARD_MENU_ACCESS = bool.Parse(index[1]);
                        index = LINES[i + 8].Split(new char[] { '=' });
                        bool DISABLE_FELLOW_UNDER_JOB = bool.Parse(index[1]);
                        index = LINES[i + 9].Split(new char[] { '=' });
                        bool ONE_CHAR_ALLOWED_JOBFLAG_INSIDE_ACC = bool.Parse(index[1]);
                        index = LINES[i + 10].Split(new char[] { '=' });
                        bool JOB_REVERSE_MAP_POINT = bool.Parse(index[1]);
                        index = LINES[i + 11].Split(new char[] { '=' });
                        bool DISABLE_JOB_BETWEEN_SPECIFIC_TIME = bool.Parse(index[1]);
                        index = LINES[i + 12].Split(new char[] { '=' });
                        string DISABLE_JOB_START_TIME = index[1];
                        index = LINES[i + 13].Split(new char[] { '=' });
                        string DISABLE_JOB_END_TIME = index[1];
                        index = LINES[i + 14].Split(new char[] { '=' });
                        int DISABLE_JOB_PC_LIMIT = int.Parse(index[1], System.Globalization.NumberStyles.Integer);

                        Settings.JOB_REVERSE_DEATH_POINT = JOB_REVERSE_DEATH_POINT;
                        Settings.MAIN.textBox62.Text = JOB_REVERSE_DEATH_POINT.ToString();
                        Settings.JOB_REVERSE_LAST_RECALL_POINT = JOB_REVERSE_LAST_RECALL_POINT;
                        Settings.MAIN.textBox63.Text = JOB_REVERSE_LAST_RECALL_POINT.ToString();
                        Settings.JOB_RESS_SCROLL = JOB_RESS_SCROLL;
                        Settings.MAIN.textBox64.Text = JOB_RESS_SCROLL.ToString();
                        Settings.JOB_ANTI_TRACE = JOB_ANTI_TRACE;
                        Settings.MAIN.textBox65.Text = JOB_ANTI_TRACE.ToString();
                        Settings.JOB_RESS_ACCEPTION_DELAY = JOB_RESS_ACCEPTION_DELAY;
                        Settings.MAIN.textBox66.Text = JOB_RESS_ACCEPTION_DELAY.ToString();
                        Settings.JOB_GOODS_DROPOUT = JOB_GOODS_DROPOUT;
                        Settings.MAIN.textBox67.Text = JOB_GOODS_DROPOUT.ToString();
                        Settings.DISABLE_THIEF_REWARD_MENU_ACCESS = DISABLE_THIEF_REWARD_MENU_ACCESS;
                        Settings.MAIN.textBox109.Text = DISABLE_THIEF_REWARD_MENU_ACCESS.ToString();
                        Settings.DISABLE_FELLOW_UNDER_JOB = DISABLE_FELLOW_UNDER_JOB;
                        Settings.MAIN.textBox110.Text = DISABLE_FELLOW_UNDER_JOB.ToString();
                        Settings.ONE_CHAR_ALLOWED_JOBFLAG_INSIDE_ACC = ONE_CHAR_ALLOWED_JOBFLAG_INSIDE_ACC;
                        Settings.MAIN.textBox134.Text = ONE_CHAR_ALLOWED_JOBFLAG_INSIDE_ACC.ToString();
                        Settings.JOB_REVERSE_MAP_POINT = JOB_REVERSE_MAP_POINT;
                        Settings.MAIN.textBox135.Text = JOB_REVERSE_MAP_POINT.ToString();
                        Settings.MAIN.checkBox26.Checked = DISABLE_JOB_BETWEEN_SPECIFIC_TIME;
                        Settings.MAIN.RbLimitStartDTP.Text = DISABLE_JOB_START_TIME;
                        Settings.MAIN.RbLimitEndDTP.Text = DISABLE_JOB_END_TIME;
                        Settings.MAIN.numericUpDown16.Value = DISABLE_JOB_PC_LIMIT;

                    }
                    #endregion   
                    #region [BOTS POLICY]
                    if (LINES[i] == "[BOTS POLICY]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        bool BOT_ALLOW = bool.Parse(index[1]);
                        index = LINES[i + 2].Split(new char[] { '=' });
                        bool BOT_ALLOW_ALCHEMY_ELIXIR = bool.Parse(index[1]);
                        index = LINES[i + 3].Split(new char[] { '=' });
                        bool BOT_ALLOW_ALCHEMY_STONE = bool.Parse(index[1]);
                        index = LINES[i + 4].Split(new char[] { '=' });
                        bool BOT_ALLOW_CREATE_PARTY = bool.Parse(index[1]);
                        index = LINES[i + 5].Split(new char[] { '=' });
                        bool BOT_ALLOW_INVITE_PARTY = bool.Parse(index[1]);
                        index = LINES[i + 6].Split(new char[] { '=' });
                        bool BOT_ALLOW_EXCHANGE = bool.Parse(index[1]);
                        index = LINES[i + 7].Split(new char[] { '=' });
                        bool BOT_ALLOW_STALL = bool.Parse(index[1]);
                        index = LINES[i + 8].Split(new char[] { '=' });
                        bool BOT_ALLOW_ARENA = bool.Parse(index[1]);
                        index = LINES[i + 9].Split(new char[] { '=' });
                        bool BOT_ALLOW_CTF = bool.Parse(index[1]);
                        index = LINES[i + 10].Split(new char[] { '=' });
                        bool BOT_ALLOW_FORTRESS = bool.Parse(index[1]);
                        index = LINES[i + 11].Split(new char[] { '=' });
                        bool BOT_ALLOW_PVP = bool.Parse(index[1]);
                        index = LINES[i + 12].Split(new char[] { '=' });
                        bool BOT_ALLOW_TRACE = bool.Parse(index[1]);

                        Settings.BOT_ALLOW = BOT_ALLOW;
                        Settings.MAIN.textBox68.Text = BOT_ALLOW.ToString();
                        Settings.BOT_ALLOW_ALCHEMY_ELIXIR = BOT_ALLOW_ALCHEMY_ELIXIR;
                        Settings.MAIN.textBox69.Text = BOT_ALLOW_ALCHEMY_ELIXIR.ToString();
                        Settings.BOT_ALLOW_ALCHEMY_STONE = BOT_ALLOW_ALCHEMY_STONE;
                        Settings.MAIN.textBox70.Text = BOT_ALLOW_ALCHEMY_STONE.ToString();
                        Settings.BOT_ALLOW_CREATE_PARTY = BOT_ALLOW_CREATE_PARTY;
                        Settings.MAIN.textBox71.Text = BOT_ALLOW_CREATE_PARTY.ToString();
                        Settings.BOT_ALLOW_INVITE_PARTY = BOT_ALLOW_INVITE_PARTY;
                        Settings.MAIN.textBox72.Text = BOT_ALLOW_INVITE_PARTY.ToString();
                        Settings.BOT_ALLOW_EXCHANGE = BOT_ALLOW_EXCHANGE;
                        Settings.MAIN.textBox73.Text = BOT_ALLOW_EXCHANGE.ToString();
                        Settings.BOT_ALLOW_STALL = BOT_ALLOW_STALL;
                        Settings.MAIN.textBox74.Text = BOT_ALLOW_STALL.ToString();
                        Settings.BOT_ALLOW_ARENA = BOT_ALLOW_ARENA;
                        Settings.MAIN.textBox75.Text = BOT_ALLOW_ARENA.ToString();
                        Settings.BOT_ALLOW_CTF = BOT_ALLOW_CTF;
                        Settings.MAIN.textBox76.Text = BOT_ALLOW_CTF.ToString();
                        Settings.BOT_ALLOW_FORTRESS = BOT_ALLOW_FORTRESS;
                        Settings.MAIN.textBox77.Text = BOT_ALLOW_FORTRESS.ToString();
                        Settings.BOT_ALLOW_PVP = BOT_ALLOW_PVP;
                        Settings.MAIN.textBox78.Text = BOT_ALLOW_PVP.ToString();
                        Settings.BOT_ALLOW_TRACE = BOT_ALLOW_TRACE;
                        Settings.MAIN.textBox79.Text = BOT_ALLOW_TRACE.ToString();

                    }
                    #endregion
                    #region [LOGS MANAGER]
                    if (LINES[i] == "[LOGS MANAGER]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        bool LOG_PLAYERS_ALL_CHAT = bool.Parse(index[1]);
                        index = LINES[i + 2].Split(new char[] { '=' });
                        bool LOG_PLAYERS_PM_CHAT = bool.Parse(index[1]);
                        index = LINES[i + 3].Split(new char[] { '=' });
                        bool LOG_PLAYERS_PT_CHAT = bool.Parse(index[1]);
                        index = LINES[i + 4].Split(new char[] { '=' });
                        bool LOG_PLAYERS_GUILD_CHAT = bool.Parse(index[1]);
                        index = LINES[i + 5].Split(new char[] { '=' });
                        bool LOG_PLAYERS_UNI_CHAT = bool.Parse(index[1]);
                        index = LINES[i + 6].Split(new char[] { '=' });
                        bool LOG_PLAYERS_LOADIMAGE_TIMESPAN = bool.Parse(index[1]);
                        index = LINES[i + 7].Split(new char[] { '=' });
                        bool LOG_PROXY_ERRORS = bool.Parse(index[1]);
                        index = LINES[i + 8].Split(new char[] { '=' });
                        bool LOG_MODULE_CRASH_DUMP = bool.Parse(index[1]);
                        index = LINES[i + 9].Split(new char[] { '=' });
                        bool LOG_MAGIC_POP_PLAY = bool.Parse(index[1]);

                        Settings.LOG_PLAYERS_ALL_CHAT = LOG_PLAYERS_ALL_CHAT;
                        Settings.MAIN.textBox80.Text = LOG_PLAYERS_ALL_CHAT.ToString();
                        Settings.LOG_PLAYERS_PM_CHAT = LOG_PLAYERS_PM_CHAT;
                        Settings.MAIN.textBox81.Text = LOG_PLAYERS_PM_CHAT.ToString();
                        Settings.LOG_PLAYERS_PT_CHAT = LOG_PLAYERS_PT_CHAT;
                        Settings.MAIN.textBox83.Text = LOG_PLAYERS_PT_CHAT.ToString();
                        Settings.LOG_PLAYERS_GUILD_CHAT = LOG_PLAYERS_GUILD_CHAT;
                        Settings.MAIN.textBox84.Text = LOG_PLAYERS_GUILD_CHAT.ToString();
                        Settings.LOG_PLAYERS_UNI_CHAT = LOG_PLAYERS_UNI_CHAT;
                        Settings.MAIN.textBox85.Text = LOG_PLAYERS_UNI_CHAT.ToString();
                        Settings.LOG_PLAYERS_LOADIMAGE_TIMESPAN = LOG_PLAYERS_LOADIMAGE_TIMESPAN;
                        Settings.MAIN.textBox86.Text = LOG_PLAYERS_LOADIMAGE_TIMESPAN.ToString();
                        Settings.LOG_PROXY_ERRORS = LOG_PROXY_ERRORS;
                        Settings.MAIN.textBox87.Text = LOG_PROXY_ERRORS.ToString();
                        Settings.LOG_MODULE_CRASH_DUMP = LOG_MODULE_CRASH_DUMP;
                        Settings.MAIN.textBox88.Text = LOG_MODULE_CRASH_DUMP.ToString();
                        Settings.LOG_MAGIC_POP_PLAY = LOG_MAGIC_POP_PLAY;
                        Settings.MAIN.textBox89.Text = LOG_MAGIC_POP_PLAY.ToString();

                    }
                    #endregion
                    #region [NEW SETTINGS]
                    if (LINES[i] == "[NEW SETTINGS]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        string SHARD_PATH = index[1];
                        index = LINES[i + 2].Split(new char[] { '=' });
                        bool DISABLE_TITLE_MANAGER = Convert.ToBoolean(index[1]);
                        index = LINES[i + 3].Split(new char[] { '=' });
                        bool DISABLE_GRANT_NAME = Convert.ToBoolean(index[1]);
                        index = LINES[i + 4].Split(new char[] { '=' });
                        bool DISABLE_UNIQUE_HISTORY = Convert.ToBoolean(index[1]);
                        index = LINES[i + 5].Split(new char[] { '=' });
                        bool DISABLE_CHARACTER_LOCK = Convert.ToBoolean(index[1]);
                        index = LINES[i + 6].Split(new char[] { '=' });
                        bool DISABLE_RANKING = Convert.ToBoolean(index[1]);
                        index = LINES[i + 7].Split(new char[] { '=' });
                        bool DISABLE_EVENTS_SCHEDULE = Convert.ToBoolean(index[1]);
                        index = LINES[i + 8].Split(new char[] { '=' });
                        bool DISABLE_CUSTOM_TITLE = Convert.ToBoolean(index[1]);
                        index = LINES[i + 9].Split(new char[] { '=' });
                        bool DISABLE_EVENTS_REGISTER = Convert.ToBoolean(index[1]);
                        index = LINES[i + 10].Split(new char[] { '=' });
                        bool DISABLE_CHANGELOG = Convert.ToBoolean(index[1]);
                        index = LINES[i + 11].Split(new char[] { '=' });
                        int DISABLE_TELEPORT_BUTTON_FROM_LEVEL = Convert.ToInt32(index[1]);
                        index = LINES[i + 12].Split(new char[] { '=' });
                        int ALLOW_UPDATING_HONOR_BUFFS_EVERY = Convert.ToInt32(index[1]);
                        index = LINES[i + 13].Split(new char[] { '=' });
                        int ALLOW_UPDATING_RANKINGS_EVERY = Convert.ToInt32(index[1]);
                        index = LINES[i + 14].Split(new char[] { '=' });
                        bool DISABLE_CUSTOM_NAME = Convert.ToBoolean(index[1]);
                        index = LINES[i + 15].Split(new char[] { '=' });
                        bool DISABLE_CUSTOM_RANKNAME = Convert.ToBoolean(index[1]);
                        index = LINES[i + 16].Split(new char[] { '=' });
                        bool DISABLE_CUSTOM_ICON = Convert.ToBoolean(index[1]);
                        index = LINES[i + 17].Split(new char[] { '=' });
                        bool DISABLE_SPECIAL_REVERSE = Convert.ToBoolean(index[1]);
                        index = LINES[i + 18].Split(new char[] { '=' });
                        bool DISABLE_PLAYERS_COUNT_AT_LOGIN_SCREEN = Convert.ToBoolean(index[1]);
                        index = LINES[i + 19].Split(new char[] { '=' });
                        bool DISABLE_PLAYERS_COUNT_AT_LOGIN_LIST = Convert.ToBoolean(index[1]);
                        index = LINES[i + 20].Split(new char[] { '=' });
                        int CUSTOM_TITLE_PRICE = Convert.ToInt32(index[1]);
                        index = LINES[i + 21].Split(new char[] { '=' });
                        int CRIMISON_GLOBAL_ID = Convert.ToInt32(index[1]);
                        index = LINES[i + 22].Split(new char[] { '=' });
                        int BLUE_GLOBAL_ID = Convert.ToInt32(index[1]);
                        index = LINES[i + 23].Split(new char[] { '=' });
                        int LIGHTGREEN_GLOBAL_ID = Convert.ToInt32(index[1]);
                        index = LINES[i + 24].Split(new char[] { '=' });
                        int RED_GLOBAL_ID = Convert.ToInt32(index[1]);
                        index = LINES[i + 25].Split(new char[] { '=' });
                        int ORANGE_GLOBAL_ID = Convert.ToInt32(index[1]);
                        index = LINES[i + 26].Split(new char[] { '=' });
                        int GREEN_GLOBAL_ID = Convert.ToInt32(index[1]);
                        index = LINES[i + 27].Split(new char[] { '=' });
                        int PURPLE_GLOBAL_ID = Convert.ToInt32(index[1]);
                        index = LINES[i + 28].Split(new char[] { '=' });
                        int PINK_GLOBAL_ID = Convert.ToInt32(index[1]);
                        index = LINES[i + 29].Split(new char[] { '=' });
                        bool DISABLE_TELEPORT_BUTTON = Convert.ToBoolean(index[1]);
                        index = LINES[i + 30].Split(new char[] { '=' });
                        bool ALLOW_UPDATING_HONOR_BUFFS = Convert.ToBoolean(index[1]);
                        index = LINES[i + 31].Split(new char[] { '=' });
                        bool ALLOW_UPDATING_RANKING = Convert.ToBoolean(index[1]);
                        index = LINES[i + 32].Split(new char[] { '=' });
                        bool DISABLE_CUSTOM_TITLE_COLOR = Convert.ToBoolean(index[1]);
                        index = LINES[i + 33].Split(new char[] { '=' });
                        bool ENABLE_STALL_REWARD_HOUR = Convert.ToBoolean(index[1]);
                        index = LINES[i + 34].Split(new char[] { '=' });
                        int RESTRCTION_STALL_REWARD_LEVEL = Convert.ToInt32(index[1]);
                        index = LINES[i + 35].Split(new char[] { '=' });
                        int STALL_REWARD_GOLD = Convert.ToInt32(index[1]);
                        index = LINES[i + 36].Split(new char[] { '=' });
                        int STALL_REWARD_SILK = Convert.ToInt32(index[1]);
                        index = LINES[i + 37].Split(new char[] { '=' });
                        bool ENABLE_PARTY_REWARD_HOUR = Convert.ToBoolean(index[1]);
                        index = LINES[i + 38].Split(new char[] { '=' });
                        int RESTRCTION_PARTY_REWARD_LEVEL = Convert.ToInt32(index[1]);
                        index = LINES[i + 39].Split(new char[] { '=' });
                        int PARTY_REWARD_GOLD = Convert.ToInt32(index[1]);
                        index = LINES[i + 40].Split(new char[] { '=' });
                        int PARTY_REWARD_SILK = Convert.ToInt32(index[1]);
                        index = LINES[i + 41].Split(new char[] { '=' });
                        bool ENABLE_SILKGOLD_REWARD_HOUR = Convert.ToBoolean(index[1]);
                        index = LINES[i + 42].Split(new char[] { '=' });
                        int RESTRCTION_SILKGOLD_REWARD_LEVEL = Convert.ToInt32(index[1]);
                        index = LINES[i + 43].Split(new char[] { '=' });
                        int SILKGOLD_REWARD_GOLD = Convert.ToInt32(index[1]);
                        index = LINES[i + 44].Split(new char[] { '=' });
                        int SILKGOLD_REWARD_SILK = Convert.ToInt32(index[1]);
                        index = LINES[i + 45].Split(new char[] { '=' });
                        bool ALLOW_DROP_GOODS_WHEN_GO_OFFLINE = bool.Parse(index[1]);
                        index = LINES[i + 46].Split(new char[] { '=' });
                        string FACEBOOK_LINK = index[1];
                        index = LINES[i + 47].Split(new char[] { '=' });
                        string DISCORD_LINK = index[1];
                        index = LINES[i + 48].Split(new char[] { '=' });
                        string WEBSITE_LINK = index[1];
                        index = LINES[i + 49].Split(new char[] { '=' });
                        bool ALLOW_DISCORD_RPC = Convert.ToBoolean(index[1]);
                        index = LINES[i + 50].Split(new char[] { '=' });
                        string DISCORD_RPC = index[1];
                        index = LINES[i + 51].Split(new char[] { '=' });
                        int FW_PC_LIMIT = Convert.ToInt32(index[1]);
                        index = LINES[i + 52].Split(new char[] { '=' });
                        int CTF_PC_LIMIT = Convert.ToInt32(index[1]);
                        index = LINES[i + 53].Split(new char[] { '=' });
                        int BA_PC_LIMIT = Convert.ToInt32(index[1]);
                        index = LINES[i + 54].Split(new char[] { '=' });
                        int JOB_PC_LIMIT = Convert.ToInt32(index[1]);
                        index = LINES[i + 55].Split(new char[] { '=' });
                        bool DISABLE_ZERK_AT_FW = Convert.ToBoolean(index[1]);
                        index = LINES[i + 56].Split(new char[] { '=' });
                        int PLUS_NOTICE_REQ_VALUE = Convert.ToInt32(index[1]);
                        index = LINES[i + 57].Split(new char[] { '=' });
                        bool ALLOW_GAMBLING_SYSTEM = Convert.ToBoolean(index[1]);
                        index = LINES[i + 58].Split(new char[] { '=' });
                        int WIN_PERCENTAGE = Convert.ToInt32(index[1]);
                        index = LINES[i + 59].Split(new char[] { '=' });
                        int MAX_ATTEMPTS = Convert.ToInt32(index[1]);
                        index = LINES[i + 60].Split(new char[] { '=' });
                        string START = index[1];
                        index = LINES[i + 61].Split(new char[] { '=' });
                        string END = index[1];

                        Settings.MAIN.textBox23.Text = SHARD_PATH;
                        Settings.MAIN.JobTitle.Checked = DISABLE_TITLE_MANAGER;
                        Settings.MAIN.checkBox11.Checked = DISABLE_GRANT_NAME;
                        Settings.MAIN.checkBox12.Checked = DISABLE_UNIQUE_HISTORY;
                        Settings.MAIN.checkBox16.Checked = DISABLE_CHARACTER_LOCK;
                        Settings.MAIN.checkBox15.Checked = DISABLE_RANKING;
                        Settings.MAIN.checkBox13.Checked = DISABLE_EVENTS_SCHEDULE;
                        Settings.MAIN.checkBox24.Checked = DISABLE_CUSTOM_TITLE;
                        Settings.MAIN.checkBox20.Checked = DISABLE_EVENTS_REGISTER;
                        Settings.MAIN.checkBox17.Checked = DISABLE_CHANGELOG;
                        Settings.MAIN.checkBox7.Checked = DISABLE_TELEPORT_BUTTON;
                        Settings.MAIN.checkBox29.Checked = ALLOW_UPDATING_HONOR_BUFFS;
                        Settings.MAIN.checkBox30.Checked = ALLOW_UPDATING_RANKING;
                        Settings.MAIN.numericUpDown27.Value = Convert.ToInt32(DISABLE_TELEPORT_BUTTON_FROM_LEVEL);
                        Settings.MAIN.textBox5.Text = ALLOW_UPDATING_HONOR_BUFFS_EVERY.ToString();
                        Settings.MAIN.textBox28.Text = ALLOW_UPDATING_RANKINGS_EVERY.ToString();
                        Settings.MAIN.checkBox31.Checked = DISABLE_CUSTOM_NAME;
                        Settings.MAIN.checkBox32.Checked = DISABLE_CUSTOM_RANKNAME;
                        Settings.MAIN.checkBox33.Checked = DISABLE_CUSTOM_TITLE_COLOR;
                        Settings.MAIN.checkBox34.Checked = DISABLE_CUSTOM_ICON;
                        Settings.MAIN.checkBox35.Checked = DISABLE_SPECIAL_REVERSE;
                        Settings.MAIN.checkBox36.Checked = DISABLE_PLAYERS_COUNT_AT_LOGIN_SCREEN;
                        Settings.MAIN.checkBox37.Checked = DISABLE_PLAYERS_COUNT_AT_LOGIN_LIST;
                        Settings.MAIN.textBox31.Text = CRIMISON_GLOBAL_ID.ToString();
                        Settings.MAIN.textBox32.Text = BLUE_GLOBAL_ID.ToString();
                        Settings.MAIN.textBox35.Text = LIGHTGREEN_GLOBAL_ID.ToString();
                        Settings.MAIN.textBox37.Text = RED_GLOBAL_ID.ToString();
                        Settings.MAIN.textBox38.Text = ORANGE_GLOBAL_ID.ToString();
                        Settings.MAIN.textBox59.Text = GREEN_GLOBAL_ID.ToString();
                        Settings.MAIN.textBox60.Text = PURPLE_GLOBAL_ID.ToString();
                        Settings.MAIN.textBox61.Text = PINK_GLOBAL_ID.ToString();
                        Settings.MAIN.textBox82.Text = CUSTOM_TITLE_PRICE.ToString();
                        Settings.MAIN.checkBox18.Checked = ENABLE_STALL_REWARD_HOUR;
                        Settings.MAIN.numericUpDown1.Value = RESTRCTION_STALL_REWARD_LEVEL;
                        Settings.MAIN.numericUpDown4.Value = STALL_REWARD_GOLD;
                        Settings.MAIN.numericUpDown5.Value = STALL_REWARD_SILK;

                        Settings.MAIN.checkBox23.Checked = ENABLE_PARTY_REWARD_HOUR;
                        Settings.MAIN.numericUpDown2.Value = RESTRCTION_PARTY_REWARD_LEVEL;
                        Settings.MAIN.numericUpDown7.Value = PARTY_REWARD_GOLD;
                        Settings.MAIN.numericUpDown6.Value = PARTY_REWARD_SILK;

                        Settings.MAIN.checkBox25.Checked = ENABLE_SILKGOLD_REWARD_HOUR;
                        Settings.MAIN.numericUpDown3.Value = RESTRCTION_SILKGOLD_REWARD_LEVEL;
                        Settings.MAIN.numericUpDown9.Value = SILKGOLD_REWARD_GOLD;
                        Settings.MAIN.numericUpDown8.Value = SILKGOLD_REWARD_SILK;
                        Settings.MAIN.textBox112.Text = ALLOW_DROP_GOODS_WHEN_GO_OFFLINE.ToString();

                        Settings.MAIN.textBox113.Text = FACEBOOK_LINK;
                        Settings.MAIN.textBox114.Text = DISCORD_LINK;
                        Settings.MAIN.textBox115.Text = WEBSITE_LINK;
                        Settings.MAIN.checkBox28.Checked = ALLOW_DISCORD_RPC;
                        Settings.MAIN.textBox116.Text = DISCORD_RPC;

                        Settings.MAIN.numericUpDown10.Value = FW_PC_LIMIT;
                        Settings.MAIN.numericUpDown11.Value = CTF_PC_LIMIT;
                        Settings.MAIN.numericUpDown12.Value = BA_PC_LIMIT;
                        Settings.MAIN.numericUpDown13.Value = JOB_PC_LIMIT;
                        Settings.MAIN.textBox120.Text = DISABLE_ZERK_AT_FW.ToString();
                        Settings.MAIN.numericUpDown14.Value = PLUS_NOTICE_REQ_VALUE;
                        Settings.PLUS_REQ_NOTICE = PLUS_NOTICE_REQ_VALUE;

                        Settings.MAIN.checkBox38.Checked = ALLOW_GAMBLING_SYSTEM;
                        Settings.MAIN.numericUpDown15.Value = WIN_PERCENTAGE;
                        Settings.MAIN.numericUpDown20.Value = MAX_ATTEMPTS;
                        Settings.MAIN.dateTimePicker2.Text = START;
                        Settings.MAIN.dateTimePicker1.Text = END;
                    }
                    #endregion
                    #region [FILTER TEXTS]
                    if (LINES[i] == "[FILTER TEXTS]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        string DRUNK_10MINS = index[1];
                        index = LINES[i + 2].Split(new char[] { '=' });
                        string DRUNK_5MINS = index[1];
                        index = LINES[i + 3].Split(new char[] { '=' });
                        string DRUNK_1MINS = index[1];
                        index = LINES[i + 4].Split(new char[] { '=' });
                        string DRUNK_END = index[1];
                        index = LINES[i + 5].Split(new char[] { '=' });
                        string DRUNK_WINNER1 = index[1];
                        index = LINES[i + 6].Split(new char[] { '=' });
                        string DRUNK_REGISTER = index[1];
                        index = LINES[i + 7].Split(new char[] { '=' });
                        string DRUNK_REGISTERED = index[1];
                        index = LINES[i + 8].Split(new char[] { '=' });
                        string DRUNK_UNREGISTER = index[1];
                        index = LINES[i + 9].Split(new char[] { '=' });
                        string DRUNK_NOTREGISTERED = index[1];
                        index = LINES[i + 10].Split(new char[] { '=' });
                        string SURVIVAL_10MINS = index[1];
                        index = LINES[i + 11].Split(new char[] { '=' });
                        string SURVIVAL_5MINS = index[1];
                        index = LINES[i + 12].Split(new char[] { '=' });
                        string SURVIVAL_1MINS = index[1];
                        index = LINES[i + 13].Split(new char[] { '=' });
                        string SURVIVAL_END = index[1];
                        index = LINES[i + 14].Split(new char[] { '=' });
                        string SURVIVAL_WINNER1 = index[1];
                        index = LINES[i + 15].Split(new char[] { '=' });
                        string SURVIVAL_WINNER2 = index[1];
                        index = LINES[i + 16].Split(new char[] { '=' });
                        string SURVIVAL_WINNER3 = index[1];
                        index = LINES[i + 17].Split(new char[] { '=' });
                        string SURVIVAL_REGISTER = index[1];
                        index = LINES[i + 18].Split(new char[] { '=' });
                        string SURVIVAL_REGISTERED = index[1];
                        index = LINES[i + 19].Split(new char[] { '=' });
                        string SURVIVAL_UNREGISTER = index[1];
                        index = LINES[i + 20].Split(new char[] { '=' });
                        string SURVIVAL_NOTREGISTERED = index[1];
                        index = LINES[i + 21].Split(new char[] { '=' });
                        string ZERK_DELAY = index[1];
                        index = LINES[i + 22].Split(new char[] { '=' });
                        string ZERK_UNDER_PET = index[1];
                        index = LINES[i + 23].Split(new char[] { '=' });
                        string ZERK_FORTRESSWAR = index[1];
                        index = LINES[i + 24].Split(new char[] { '=' });
                        string ZERK_REGION = index[1];
                        index = LINES[i + 25].Split(new char[] { '=' });
                        string DISABLE_RESTART = index[1];
                        index = LINES[i + 26].Split(new char[] { '=' });
                        string RESTART_DELAY = index[1];
                        index = LINES[i + 27].Split(new char[] { '=' });
                        string EXIT_DELAY = index[1];
                        index = LINES[i + 28].Split(new char[] { '=' });
                        string DROP_GOODS_TOWN = index[1];
                        index = LINES[i + 29].Split(new char[] { '=' });
                        string BOT_ALLOW_STALL = index[1];
                        index = LINES[i + 30].Split(new char[] { '=' });
                        string CHAT_FILTER_STALL = index[1];
                        index = LINES[i + 31].Split(new char[] { '=' });
                        string STALL_DELAY = index[1];
                        index = LINES[i + 32].Split(new char[] { '=' });
                        string GUILD_INVITE_DELAY = index[1];
                        index = LINES[i + 33].Split(new char[] { '=' });
                        string GUILD_LIMIT = index[1];
                        index = LINES[i + 34].Split(new char[] { '=' });
                        string UNION_REQ_DELAY = index[1];
                        index = LINES[i + 35].Split(new char[] { '=' });
                        string UNION_LIMIT = index[1];
                        index = LINES[i + 36].Split(new char[] { '=' });
                        string BOT_ALLOW_EXCHANGE = index[1];
                        index = LINES[i + 37].Split(new char[] { '=' });
                        string EXCHANGE_DELAY = index[1];
                        index = LINES[i + 38].Split(new char[] { '=' });
                        string JOB_SKILL_PET = index[1];
                        index = LINES[i + 39].Split(new char[] { '=' });
                        string FW_TRACE_DISABLE = index[1];
                        index = LINES[i + 40].Split(new char[] { '=' });
                        string REGION_TRACE_DISABLE = index[1];
                        index = LINES[i + 41].Split(new char[] { '=' });
                        string DISABLE_SKILL_REGION = index[1];
                        index = LINES[i + 42].Split(new char[] { '=' });
                        string JOB_ACCEPT_RES = index[1];
                        index = LINES[i + 43].Split(new char[] { '=' });
                        string ITEM_BLOCK_REGION = index[1];
                        index = LINES[i + 44].Split(new char[] { '=' });
                        string GLOBAL_CHAT_FILTER = index[1];
                        index = LINES[i + 45].Split(new char[] { '=' });
                        string GLOBAL_LEVEL_FILTER = index[1];
                        index = LINES[i + 46].Split(new char[] { '=' });
                        string GLOBAL_DELAY = index[1];
                        index = LINES[i + 47].Split(new char[] { '=' });
                        string REVERSE_DEATH_POINT = index[1];
                        index = LINES[i + 48].Split(new char[] { '=' });
                        string GAMBLE_DELAY = index[1];
                        index = LINES[i + 49].Split(new char[] { '=' });
                        string GAMBLE_DISABLED = index[1];
                        index = LINES[i + 50].Split(new char[] { '=' });
                        string GAMBLE_BETWEEN = index[1];
                        index = LINES[i + 51].Split(new char[] { '=' });
                        string ENOUGH_SILK = index[1];
                        index = LINES[i + 52].Split(new char[] { '=' });
                        string ENOUGH_GOLD = index[1];
                        index = LINES[i + 53].Split(new char[] { '=' });
                        string WON_GAMBLE = index[1];
                        index = LINES[i + 54].Split(new char[] { '=' });
                        string LOSE_GAMBLE = index[1];
                        index = LINES[i + 55].Split(new char[] { '=' });
                        string XSMB_BETWEEN = index[1];
                        index = LINES[i + 56].Split(new char[] { '=' });
                        string NUM_00_99 = index[1];
                        index = LINES[i + 57].Split(new char[] { '=' });
                        string MIN_XSMB_SILK = index[1];
                        index = LINES[i + 58].Split(new char[] { '=' });
                        string MIN_XSMB_GOLD = index[1];
                        index = LINES[i + 59].Split(new char[] { '=' });
                        string XSMB_RESTRECTION = index[1];
                        index = LINES[i + 60].Split(new char[] { '=' });
                        string XSMB_SUCCESS = index[1];
                        index = LINES[i + 61].Split(new char[] { '=' });
                        string JOB_REVERSE_LAST_POINT = index[1];
                        index = LINES[i + 62].Split(new char[] { '=' });
                        string JOB_MAP_POINT = index[1];
                        index = LINES[i + 63].Split(new char[] { '=' });
                        string SUMMON_PET_JOB = index[1];
                        index = LINES[i + 64].Split(new char[] { '=' });
                        string CHAT_REGION = index[1];
                        index = LINES[i + 65].Split(new char[] { '=' });
                        string INVITE_PARTY = index[1];
                        index = LINES[i + 66].Split(new char[] { '=' });
                        string CH_EU_PARTY = index[1];
                        index = LINES[i + 67].Split(new char[] { '=' });
                        string PARTY_REGION = index[1];
                        index = LINES[i + 68].Split(new char[] { '=' });
                        string LIMIT_GOODS = index[1];
                        index = LINES[i + 69].Split(new char[] { '=' });
                        string BUY_SELL_GOODS = index[1];
                        index = LINES[i + 70].Split(new char[] { '=' });
                        string DISABLE_JOB = index[1];
                        index = LINES[i + 71].Split(new char[] { '=' });
                        string JOB_LIMIT = index[1];
                        index = LINES[i + 72].Split(new char[] { '=' });
                        string SPAWN_PET_ON_JOB = index[1];
                        index = LINES[i + 73].Split(new char[] { '=' });
                        string ONE_CHARACTER_JOB = index[1];
                        index = LINES[i + 74].Split(new char[] { '=' });
                        string DROP_ITEMS_IN_TOWN = index[1];
                        index = LINES[i + 75].Split(new char[] { '=' });
                        string THIEF_REWARD_ACCESS = index[1];
                        index = LINES[i + 76].Split(new char[] { '=' });
                        string DISABLE_TAX_RATE = index[1];
                        index = LINES[i + 77].Split(new char[] { '=' });
                        string JOB_PET_MOVE = index[1];
                        index = LINES[i + 78].Split(new char[] { '=' });
                        string MAX_PLUS_LIMIT = index[1];
                        index = LINES[i + 79].Split(new char[] { '=' });
                        string DISABLED_FEATURES = index[1];
                        index = LINES[i + 80].Split(new char[] { '=' });
                        string ITEM_CHEST_SLOTS = index[1];
                        index = LINES[i + 81].Split(new char[] { '=' });
                        string UPDATE_TITLE_DELAY = index[1];
                        index = LINES[i + 82].Split(new char[] { '=' });
                        string PURCHASE_CUSTOM_TITLE = index[1];
                        index = LINES[i + 83].Split(new char[] { '=' });
                        string SAME_CUSTOM_TITLE = index[1];
                        index = LINES[i + 84].Split(new char[] { '=' });
                        string AUT0_EQUIPT_DELAY = index[1];
                        index = LINES[i + 85].Split(new char[] { '=' });
                        string XSMB_LOG_REFRESH = index[1];
                        index = LINES[i + 86].Split(new char[] { '=' });
                        string NEW_REVERSE_MOVE = index[1];
                        index = LINES[i + 87].Split(new char[] { '=' });
                        string NEW_REVERSE_DELAY = index[1];
                        index = LINES[i + 88].Split(new char[] { '=' });
                        string NEW_REVERSE_LOCATION = index[1];
                        index = LINES[i + 89].Split(new char[] { '=' });
                        string CHARACTER_UNLOCK = index[1];
                        index = LINES[i + 90].Split(new char[] { '=' });
                        string ALREADY_UNLOCKED = index[1];
                        index = LINES[i + 91].Split(new char[] { '=' });
                        string LOCK_INCORRECTPW = index[1];
                        index = LINES[i + 92].Split(new char[] { '=' });
                        string LOCKED_SUCCESSFULLY = index[1];
                        index = LINES[i + 93].Split(new char[] { '=' });
                        string ALREADY_LOCKED = index[1];
                        index = LINES[i + 94].Split(new char[] { '=' });
                        string LOCK_REMOVED = index[1];
                        index = LINES[i + 95].Split(new char[] { '=' });
                        string ALREADY_HAVE_LOCK = index[1];
                        index = LINES[i + 96].Split(new char[] { '=' });
                        string PASSWORD_DIGITS_ONLY = index[1];
                        index = LINES[i + 97].Split(new char[] { '=' });
                        string LOCK_BLOCK = index[1];
                        index = LINES[i + 98].Split(new char[] { '=' });
                        string DAILY_REWARD = index[1];
                        index = LINES[i + 99].Split(new char[] { '=' });
                        string PLUS_LIMIT = index[1];
                        index = LINES[i + 100].Split(new char[] { '=' });
                        string PLUS_LIMIT_ADVANCE = index[1];
                        Settings.MAIN.textBox136.Text = DRUNK_10MINS;
                        Settings.MAIN.textBox133.Text = DRUNK_5MINS;
                        Settings.MAIN.textBox132.Text = DRUNK_1MINS;
                        Settings.MAIN.textBox129.Text = DRUNK_END;
                        Settings.MAIN.textBox128.Text = DRUNK_WINNER1;
                        Settings.MAIN.textBox203.Text = DRUNK_REGISTER;
                        Settings.MAIN.textBox202.Text = DRUNK_REGISTERED;
                        Settings.MAIN.textBox201.Text = DRUNK_UNREGISTER;
                        Settings.MAIN.textBox200.Text = DRUNK_NOTREGISTERED;
                        Settings.MAIN.textBox118.Text = SURVIVAL_10MINS;
                        Settings.MAIN.textBox119.Text = SURVIVAL_5MINS;
                        Settings.MAIN.textBox121.Text = SURVIVAL_1MINS;
                        Settings.MAIN.textBox124.Text = SURVIVAL_END;
                        Settings.MAIN.textBox123.Text = SURVIVAL_WINNER1;
                        Settings.MAIN.textBox122.Text = SURVIVAL_WINNER2;
                        Settings.MAIN.textBox125.Text = SURVIVAL_WINNER3;
                        Settings.MAIN.textBox199.Text = SURVIVAL_REGISTER;
                        Settings.MAIN.textBox198.Text = SURVIVAL_REGISTERED;
                        Settings.MAIN.textBox197.Text = SURVIVAL_UNREGISTER;
                        Settings.MAIN.textBox196.Text = SURVIVAL_NOTREGISTERED;
                        Settings.MAIN.textBox139.Text = ZERK_DELAY;
                        Settings.MAIN.textBox138.Text = ZERK_UNDER_PET;
                        Settings.MAIN.textBox137.Text = ZERK_FORTRESSWAR;
                        Settings.MAIN.textBox127.Text = ZERK_REGION;
                        Settings.MAIN.textBox126.Text = DISABLE_RESTART;
                        Settings.MAIN.textBox140.Text = RESTART_DELAY;
                        Settings.MAIN.textBox141.Text = EXIT_DELAY;
                        Settings.MAIN.textBox144.Text = DROP_GOODS_TOWN;
                        Settings.MAIN.textBox143.Text = BOT_ALLOW_STALL;
                        Settings.MAIN.textBox142.Text = CHAT_FILTER_STALL;
                        Settings.MAIN.textBox147.Text = STALL_DELAY;
                        Settings.MAIN.textBox146.Text = GUILD_INVITE_DELAY;
                        Settings.MAIN.textBox145.Text = GUILD_LIMIT;
                        Settings.MAIN.textBox151.Text = UNION_REQ_DELAY;
                        Settings.MAIN.textBox150.Text = UNION_LIMIT;
                        Settings.MAIN.textBox149.Text = BOT_ALLOW_EXCHANGE;
                        Settings.MAIN.textBox148.Text = EXCHANGE_DELAY;
                        Settings.MAIN.textBox152.Text = JOB_SKILL_PET;
                        Settings.MAIN.textBox160.Text = FW_TRACE_DISABLE;
                        Settings.MAIN.textBox159.Text = REGION_TRACE_DISABLE;
                        Settings.MAIN.textBox158.Text = DISABLE_SKILL_REGION;
                        Settings.MAIN.textBox154.Text = JOB_ACCEPT_RES;
                        Settings.MAIN.textBox153.Text = ITEM_BLOCK_REGION;
                        Settings.MAIN.textBox165.Text = GLOBAL_CHAT_FILTER;
                        Settings.MAIN.textBox164.Text = GLOBAL_LEVEL_FILTER;
                        Settings.MAIN.textBox163.Text = GLOBAL_DELAY;
                        Settings.MAIN.textBox161.Text = REVERSE_DEATH_POINT;
                        Settings.MAIN.textBox217.Text = GAMBLE_DELAY;
                        Settings.MAIN.textBox216.Text = GAMBLE_DISABLED;
                        Settings.MAIN.textBox214.Text = GAMBLE_BETWEEN;
                        Settings.MAIN.textBox221.Text = ENOUGH_SILK;
                        Settings.MAIN.textBox220.Text = ENOUGH_GOLD;
                        Settings.MAIN.textBox219.Text = WON_GAMBLE;
                        Settings.MAIN.textBox218.Text = LOSE_GAMBLE;
                        Settings.MAIN.textBox223.Text = XSMB_BETWEEN;
                        Settings.MAIN.textBox222.Text = NUM_00_99;
                        Settings.MAIN.textBox230.Text = MIN_XSMB_SILK;
                        Settings.MAIN.textBox229.Text = MIN_XSMB_GOLD;
                        Settings.MAIN.textBox225.Text = XSMB_RESTRECTION;
                        Settings.MAIN.textBox224.Text = XSMB_SUCCESS;
                        Settings.MAIN.textBox180.Text = JOB_REVERSE_LAST_POINT;
                        Settings.MAIN.textBox179.Text = JOB_MAP_POINT;
                        Settings.MAIN.textBox178.Text = SUMMON_PET_JOB;
                        Settings.MAIN.textBox177.Text = CHAT_REGION;
                        Settings.MAIN.textBox176.Text = INVITE_PARTY;
                        Settings.MAIN.textBox175.Text = CH_EU_PARTY;
                        Settings.MAIN.textBox174.Text = PARTY_REGION;
                        Settings.MAIN.textBox173.Text = LIMIT_GOODS;
                        Settings.MAIN.textBox172.Text = BUY_SELL_GOODS;
                        Settings.MAIN.textBox168.Text = DISABLE_JOB;
                        Settings.MAIN.textBox167.Text = JOB_LIMIT;
                        Settings.MAIN.textBox166.Text = SPAWN_PET_ON_JOB;
                        Settings.MAIN.textBox192.Text = ONE_CHARACTER_JOB;
                        Settings.MAIN.textBox191.Text = DROP_ITEMS_IN_TOWN;
                        Settings.MAIN.textBox190.Text = THIEF_REWARD_ACCESS;
                        Settings.MAIN.textBox189.Text = DISABLE_TAX_RATE;
                        Settings.MAIN.textBox188.Text = JOB_PET_MOVE;
                        Settings.MAIN.textBox187.Text = MAX_PLUS_LIMIT;
                        Settings.MAIN.textBox186.Text = DISABLED_FEATURES;
                        Settings.MAIN.textBox185.Text = ITEM_CHEST_SLOTS;
                        Settings.MAIN.textBox184.Text = UPDATE_TITLE_DELAY;
                        Settings.MAIN.textBox183.Text = PURCHASE_CUSTOM_TITLE;
                        Settings.MAIN.textBox182.Text = SAME_CUSTOM_TITLE;
                        Settings.MAIN.textBox181.Text = AUT0_EQUIPT_DELAY;
                        Settings.MAIN.textBox195.Text = XSMB_LOG_REFRESH;
                        Settings.MAIN.textBox194.Text = NEW_REVERSE_MOVE;
                        Settings.MAIN.textBox193.Text = NEW_REVERSE_DELAY;
                        Settings.MAIN.textBox207.Text = NEW_REVERSE_LOCATION;
                        Settings.MAIN.textBox206.Text = CHARACTER_UNLOCK;
                        Settings.MAIN.textBox205.Text = ALREADY_UNLOCKED;
                        Settings.MAIN.textBox204.Text = LOCK_INCORRECTPW;
                        Settings.MAIN.textBox210.Text = LOCKED_SUCCESSFULLY;
                        Settings.MAIN.textBox209.Text = ALREADY_LOCKED;
                        Settings.MAIN.textBox208.Text = LOCK_REMOVED;
                        Settings.MAIN.textBox212.Text = ALREADY_HAVE_LOCK;
                        Settings.MAIN.textBox211.Text = PASSWORD_DIGITS_ONLY;
                        Settings.MAIN.textBox213.Text = LOCK_BLOCK;
                        Settings.MAIN.textBox236.Text = DAILY_REWARD;
                        Settings.MAIN.textBox235.Text = PLUS_LIMIT;
                        Settings.MAIN.textBox234.Text = PLUS_LIMIT_ADVANCE;
                    }
                    #endregion
                    #region [ADVANCED III]
                    if (LINES[i] == "[ADVANCED III]")
                    {
                        index = LINES[i + 1].Split(new char[] { '=' });
                        Settings.MAIN.checkBox39.Checked = bool.Parse(index[1]);
                        index = LINES[i + 2].Split(new char[] { '=' });
                        Settings.MAIN.checkBox40.Checked = bool.Parse(index[1]);
                        index = LINES[i + 3].Split(new char[] { '=' });
                        Settings.MAIN.checkBox41.Checked = bool.Parse(index[1]);
                        index = LINES[i + 4].Split(new char[] { '=' });
                        Settings.MAIN.checkBox42.Checked = bool.Parse(index[1]);
                        index = LINES[i + 5].Split(new char[] { '=' });
                        Settings.MAIN.checkBox43.Checked = bool.Parse(index[1]);

                        index = LINES[i + 6].Split(new char[] { '=' });
                        Settings.MAIN.textBox215.Text = index[1];
                        index = LINES[i + 7].Split(new char[] { '=' });
                        Settings.MAIN.textBox226.Text = index[1];
                        index = LINES[i + 8].Split(new char[] { '=' });
                        Settings.MAIN.textBox227.Text = index[1];

                        index = LINES[i + 9].Split(new char[] { '=' });
                        Settings.MAIN.checkBox44.Checked = bool.Parse(index[1]);
                        index = LINES[i + 10].Split(new char[] { '=' });
                        Settings.MAIN.textBox232.Text = index[1];
                    }
                    #endregion
                }
                return true;
            }
            catch { UTILS.WriteLine("Your configuration file is corrupted, please correct the errors or restore it!!!"); return false; }
        }
        #region FEATURES_&_AUTO_EVENTS_LOADING
        public static bool LoadMOpcodes()
        {
            MALICIOUS_OPCODES.Clear();
            if (!File.Exists("Features/MALICIOUS_OPCODES.txt"))
            {
                UTILS.WriteLine("Features/MALICIOUS_OPCODES.txt doesn't exist.");
                return false;
            }
            else
            {
                foreach (var line in File.ReadAllLines("Features/MALICIOUS_OPCODES.txt"))
                {
                    if (line != null)
                    {
                        string[] split = line.Split('x');
                        MALICIOUS_OPCODES.Add(ushort.Parse(split[1], System.Globalization.NumberStyles.HexNumber));
                        //UTILS.WriteLine("", split[1].ToString());
                    }
                }
                //UTILS.WriteLine("","Features/BLOCKED_OPCODES.txt loaded.");
                return true;
            }
        }
        public static bool LoadChatFilter()
        {
            FILTER_KEYWORDS.Clear();
            if (!File.Exists("Features/FILTER_KEYWORDS.txt"))
            {
                UTILS.WriteLine("Features/FILTER_KEYWORDS.txt doesn't exist.");
                return false;
            }
            else
            {
                foreach (string line in File.ReadAllLines("Features/FILTER_KEYWORDS.txt"))
                {
                    if (line != string.Empty && line != null)
                    { FILTER_KEYWORDS.Add(line.ToLower()); }
                }
                // UTILS.WriteLine("Features/FILTER_KEYWORDS.txt loaded.");
                return true;
            }
        }
        public static bool LoadBlockedSkillsIDs()
        {
            BLOCKED_SKILL_IDS.Clear();
            if (!File.Exists("Features/BLOCKED_SKILL_IDS.txt"))
            {
                UTILS.WriteLine("Features/BLOCKED_SKILL_IDS.txt doesn't exist.");
                return false;
            }
            else
            {
                foreach (string line in File.ReadAllLines("Features/BLOCKED_SKILL_IDS.txt"))
                {
                    if (line != string.Empty && line != null)
                    { BLOCKED_SKILL_IDS.Add(line.ToLower()); }
                }
                // UTILS.WriteLine(Features/BLOCKED_SKILL_IDS.txt loaded.");
                return true;
            }
        }
        public static bool LoadBlockedFWSkillsIDs()
        {
            FW_BLOCKED_SKILL_IDS.Clear();
            if (!File.Exists("Features/FW_BLOCKED_SKILL_IDS.txt"))
            {
                UTILS.WriteLine("Features/FW_BLOCKED_SKILL_IDS.txt doesn't exist.");
                return false;
            }
            else
            {
                foreach (string line in File.ReadAllLines("Features/FW_BLOCKED_SKILL_IDS.txt"))
                {
                    if (line != string.Empty && line != null)
                    { FW_BLOCKED_SKILL_IDS.Add(line.ToLower()); }
                }
                // UTILS.WriteLine("Features/FW_BLOCKED_SKILL_IDS.txt loaded.");
                return true;
            }
        }
        public static bool LoadBlockedCTFSkillsIDs()
        {
            CTF_BLOCKED_SKILL_IDS.Clear();
            if (!File.Exists("Features/CTF_BLOCKED_SKILL_IDS.txt"))
            {
                UTILS.WriteLine("Features/CTF_BLOCKED_SKILL_IDS.txt doesn't exist.");
                return false;
            }
            else
            {
                foreach (string line in File.ReadAllLines("Features/CTF_BLOCKED_SKILL_IDS.txt"))
                {
                    if (line != string.Empty && line != null)
                    { CTF_BLOCKED_SKILL_IDS.Add(line.ToLower()); }
                }
                // UTILS.WriteLine("Features/FW_BLOCKED_SKILL_IDS.txt loaded.");
                return true;
            }
        }
        public static bool LoadBlockedBASkillsIDs()
        {
            BA_BLOCKED_SKILL_IDS.Clear();
            if (!File.Exists("Features/BA_BLOCKED_SKILL_IDS.txt"))
            {
                UTILS.WriteLine("Features/BA_BLOCKED_SKILL_IDS.txt doesn't exist.");
                return false;
            }
            else
            {
                foreach (string line in File.ReadAllLines("Features/BA_BLOCKED_SKILL_IDS.txt"))
                {
                    if (line != string.Empty && line != null)
                    { BA_BLOCKED_SKILL_IDS.Add(line.ToLower()); }
                }
                // UTILS.WriteLine("Features/FW_BLOCKED_SKILL_IDS.txt loaded.");
                return true;
            }
        }
        public static bool LoadBlockedJobSkillsIDs()
        {
            JOB_BLOCKED_SKILL_IDS.Clear();
            if (!File.Exists("Features/JOB_BLOCKED_SKILL_IDS.txt"))
            {
                UTILS.WriteLine("Features/JOB_BLOCKED_SKILL_IDS.txt doesn't exist.");
                return false;
            }
            else
            {
                foreach (string line in File.ReadAllLines("Features/JOB_BLOCKED_SKILL_IDS.txt"))
                {
                    if (line != string.Empty && line != null)
                    { JOB_BLOCKED_SKILL_IDS.Add(line.ToLower()); }
                }
                //UTILS.WriteLine("Features/JOB_BLOCKED_SKILL_IDS.txt loaded.");
                return true;
            }
        }

        public static bool LoadNetCafeIPs()
        {
            NETCAFE_IPS.Clear();
            if (!File.Exists("Features/NETCAFE_IPS.txt"))
            {
                UTILS.WriteLine("Features/NETCAFE_IPS.txt doesn't exist.");
                return false;
            }
            else
            {
                foreach (string line in File.ReadAllLines("Features/NETCAFE_IPS.txt"))
                {
                    if (line != string.Empty && line != null)
                    { NETCAFE_IPS.Add(line.ToLower()); }
                }
                // UTILS.WriteLine("Features/NETCAFE_IPS.txt loaded.");
                return true;
            }
        }
        public static bool LoadRetypeNames()
        {
            RETYPE_NAMES.Clear();
            if (!File.Exists("Auto Events/EVENT_RETYPE_NAMES.txt"))
            {
                UTILS.WriteLine("Auto Events/EVENT_RETYPE_NAMES.txt doesn't exist.");
                return false;
            }
            else
            {
                foreach (string line in File.ReadAllLines("Auto Events/EVENT_RETYPE_NAMES.txt"))
                {
                    if (line != string.Empty && line != null)
                    { RETYPE_NAMES.Add(line); }
                }
                // UTILS.WriteLine(UTILS.LOG_TYPE.Notify, "Auto Events/EVENT_RETYPE_NAMES.txt loaded.");
                return true;
            }
        }
        public static bool LoadTriviaQA()
        {
            TRIVIA_QA.Clear();
            if (!File.Exists("Auto Events/TRIVIA_QUESTION_ANSWERS.txt"))
            {
                UTILS.WriteLine("Auto Events/TRIVIA_QUESTION_ANSWERS.txt doesn't exist.");
                return false;
            }
            else
            {
                foreach (string line in File.ReadAllLines("Auto Events/TRIVIA_QUESTION_ANSWERS.txt"))
                {
                    if (line != string.Empty && line != null)
                    { TRIVIA_QA.Add(line); }
                }
                // UTILS.WriteLine(UTILS.LOG_TYPE.Notify, "Auto Events/TRIVIA_QUESTION.txt loaded.");
                return true;
            }
        }
        public static void LoadRegionsRestrctions()
        {
            try
            {
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "/Features/REGION_RESCTRECTION.ini");
                string[][] JaggedArray = lines.Skip(2).Select(l => l.Split('	')).ToArray();//1,8
                foreach (string[] fa in JaggedArray)
                {
                    if (!UTILS.Region_Restrection.ContainsKey(Convert.ToInt32(fa[1])))
                        UTILS.Region_Restrection.Add(Convert.ToInt32(fa[1]), Convert.ToInt32(fa[0]));
                    //Console.WriteLine($"{fa[1]}     {fa[0]}");
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadRegionsRestrctions has failed. {EX}", UTILS.LOG_TYPE.Warning); }
        }

        public static void LoadTeleportToTown()
        {
            try
            {
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "/Features/TELEPORT_TO_TOWN_REGIONS.ini");
                string[][] JaggedArray = lines.Skip(2).Select(l => l.Split('	')).ToArray();//1,8
                foreach (string[] fa in JaggedArray)
                {
                    if (!UTILS.Teleport_To_Town.Contains(Convert.ToInt32(fa[1])))
                        UTILS.Teleport_To_Town.Add(Convert.ToInt32(fa[1]));
                    //Console.WriteLine($"{fa[1]}     {fa[0]}");
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadRegionsRestrctions has failed. {EX}", UTILS.LOG_TYPE.Warning); }
        }

        public static void LoadDisableZerkRegions()
        {
            try
            {
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "/Features/DISABLE_ZERK_REGIONS.ini");
                foreach (string fa in lines)
                {
                    if (!UTILS.DISABLE_ZERK_REGIONS.Contains(Convert.ToInt32(fa)))
                        UTILS.DISABLE_ZERK_REGIONS.Add(Convert.ToInt32(fa));
                    //Console.WriteLine($"{fa[1]}     {fa[0]}");
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadDisableZerkRegions has failed. {EX}", UTILS.LOG_TYPE.Warning); }
        }

        public static void LoadDisableChatRegions()
        {
            try
            {
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "/Features/DISABLE_CHAT_REGIONS.ini");
                foreach (string fa in lines)
                {
                    if (!UTILS.DISABLE_CHAT_REGIONS.Contains(Convert.ToInt32(fa)))
                        UTILS.DISABLE_CHAT_REGIONS.Add(Convert.ToInt32(fa));
                    //Console.WriteLine($"{fa[1]}     {fa[0]}");
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadDisableChatRegions has failed. {EX}", UTILS.LOG_TYPE.Warning); }
        }
        public static void LoadDisablePartyRegions()
        {
            try
            {
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "/Features/DISABLE_PARTY_REGIONS.ini");
                foreach (string fa in lines)
                {
                    if (!UTILS.DISABLE_PARTY_REGIONS.Contains(Convert.ToInt32(fa)))
                        UTILS.DISABLE_PARTY_REGIONS.Add(Convert.ToInt32(fa));
                    //Console.WriteLine($"{fa[1]}     {fa[0]}");
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadDisablePartyRegions has failed. {EX}", UTILS.LOG_TYPE.Warning); }
        }
        public static void LoadDisableTraceRegions()
        {
            try
            {
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "/Features/DISABLE_TRACE_REGIONS.ini");
                foreach (string fa in lines)
                {
                    if (!UTILS.DISABLE_TRACE_REGIONS.Contains(Convert.ToInt32(fa)))
                        UTILS.DISABLE_TRACE_REGIONS.Add(Convert.ToInt32(fa));
                    //Console.WriteLine($"{fa[1]}     {fa[0]}");
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadDisableTraceRegions has failed. {EX}", UTILS.LOG_TYPE.Warning); }
        }
        public static void LoadDisableInviteFriendsRegions()
        {
            try
            {
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "/Features/DISABLE_INVITEFRIENDS_REGIONS.ini");
                foreach (string fa in lines)
                {
                    if (!UTILS.DISABLE_INVITEFRIENDS_REGIONS.Contains(Convert.ToInt32(fa)))
                        UTILS.DISABLE_INVITEFRIENDS_REGIONS.Add(Convert.ToInt32(fa));
                    //Console.WriteLine($"{fa[1]}     {fa[0]}");
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadDisableInviteFriendsRegions has failed. {EX}", UTILS.LOG_TYPE.Warning); }
        }

        public static void LoadDisableItemsRegions()
        {
            try
            {
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "/Features/DISABLE_ITEMS_REGIONS.ini");
                string[][] JaggedArray = lines.Skip(2).Select(l => l.Split('	')).ToArray();//1,8
                foreach (string[] fa in JaggedArray)
                {
                    if (!UTILS.DISABLE_ITEMS_REGIONS.ContainsKey(Convert.ToInt32(fa[0])))
                        UTILS.DISABLE_ITEMS_REGIONS.Add(Convert.ToInt32(fa[0]), new List<int>());
                    UTILS.DISABLE_ITEMS_REGIONS[Convert.ToInt32(fa[0])].Add(Convert.ToInt32(fa[1]));
                    //Console.WriteLine($"{fa[1]}     {fa[0]}");
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadDisableItemsRegions has failed. {EX}", UTILS.LOG_TYPE.Warning); }
        }

        public static void LoadDisableSkillsRegions()
        {
            try
            {
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "/Features/DISABLE_SKILLS_REGIONS.ini");
                string[][] JaggedArray = lines.Skip(2).Select(l => l.Split('	')).ToArray();//1,8
                foreach (string[] fa in JaggedArray)
                {

                    if (!UTILS.DISABLE_SKILLS_REGIONS.ContainsKey(Convert.ToInt32(fa[0])))
                        UTILS.DISABLE_SKILLS_REGIONS.Add(Convert.ToInt32(fa[0]), new List<int>());
                    UTILS.DISABLE_SKILLS_REGIONS[Convert.ToInt32(fa[0])].Add(Convert.ToInt32(fa[1]));

                    //Console.WriteLine($"{fa[1]}     {fa[0]}");
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadRegionsRestrctions has failed. {EX}", UTILS.LOG_TYPE.Warning); }
        }
        public static bool LoadFirstTypeNames()
        {
            FIRSTTYPE_NAMES.Clear();
            if (!File.Exists("Auto Events/EVENT_FIRSTTYPE_NAMES.txt"))
            {
                UTILS.WriteLine("Auto Events/EVENT_FIRSTTYPE_NAMES.txt doesn't exist.");
                return false;
            }
            else
            {
                foreach (string line in File.ReadAllLines("Auto Events/EVENT_FIRSTTYPE_NAMES.txt"))
                {
                    if (line != string.Empty && line != null)
                    { FIRSTTYPE_NAMES.Add(line); }
                }
                // UTILS.WriteLine(UTILS.LOG_TYPE.Notify, "Auto Events/EVENT_FIRSTTYPE_NAMES.txt loaded.");
                return true;
            }
        }
        #endregion

    }
}
