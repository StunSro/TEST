using SR_PROXY.MODEL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
namespace SR_PROXY
{
    public static class Settings
    {
        public static MAIN MAIN;
        public static DETAILS DETAILS;
        public static Dictionary<string, AppConfigItemModel> AppConfig = new Dictionary<string, AppConfigItemModel>();
        public static ConcurrentDictionary<int, ShardModel> ShardInfos = new ConcurrentDictionary<int, ShardModel>();
        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;
            Application.Run(new MAIN());
        }
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            MessageBox.Show(string.Format("捕获到未处理异常：{0}\r\n异常信息：{1}\r\n异常堆栈：{2}", ex.GetType(), ex.Message, ex.StackTrace));
        }
        // BINDING FRAMEWORK
        public static string BIND_IP = string.Empty;
        public static bool OUTSOURCE_NETWORKING = false;
        // DEFAULT DOWNLOAD
        public static string PUBLIC_DW_IP = string.Empty;
        public static int PUBLIC_DW_PORT = 0;
        public static int PVT_DW_PORT = 0;
        // DEFAULT GATEWAY
        public static string PUBLIC_GW_IP = string.Empty;
        public static int PUBLIC_GW_PORT = 0;
        public static int PVT_GW_PORT = 0;
        // DEFAULT AGENT
        public static string PUBLIC_AG_IP = string.Empty;
        public static int PUBLIC_AG_PORT = 0;
        public static int PVT_AG_PORT = 0;

        public static string PUBLIC_AG2_IP = string.Empty;
        public static int PUBLIC_AG2_PORT = 0;
        public static int PVT_AG2_PORT = 0;

        public static string PUBLIC_AG3_IP = string.Empty;
        public static int PUBLIC_AG3_PORT = 0;
        public static int PVT_AG3_PORT = 0;

        public static string PUBLIC_AG4_IP = string.Empty;
        public static int PUBLIC_AG4_PORT = 0;
        public static int PVT_AG4_PORT = 0;
        // ADDITIONAL AGENT
        public static string PUBLIC_AG_ADD_IP = string.Empty;
        public static int PUBLIC_AG_ADD_PORT = 0;
        public static int PVT_AG_ADD_PORT = 0;
        // MSSQL
        public static string MSSQL_SVR_NAME = string.Empty;
        public static string MSSQL_SVR_ID = string.Empty;
        public static string MSSQL_SVR_PW = string.Empty;
        public static string MSSQL_LOG_DB = string.Empty;
        public static string MSSQL_ACC_DB = string.Empty;
        public static string MSSQL_SHARD_DB = string.Empty;
        public static string MSSQL_BACKUP_PATH = string.Empty;
        public static int MSSQL_AUTO_BACKUP_H = 0;
        // 杂项
        public static bool PLAYER_LOGON_MSG = false;
        public static bool WELCOME_MSG = false;
        public static string SHARD_NAME = string.Empty;
        public static int FAKE_PLAYERS = 0;
        public static bool LOG_GLOBAL_CHAT = false;
        public static bool REGION_READER = true;
        public static bool GMS_ANTI_INVIS = false;
        public static bool CHAT_FILTER = false;
        public static bool BOT_DETECTOR = false;
        public static int SERVER_HWID_LIMIT = 0;
        public static bool CAPTCHA_REMOVE = false;
        public static string CAPTCHA_VALUE = string.Empty;
        public static bool LOG_UNQ_KILLS = true;
        public static bool RESTART_CHAR_SELECTION = false;
        public static bool GAME_GUIDE_DISABLE = false;
        public static bool UNQ_KILL_SILK_REWARD = false;
        public static int SILK_P_H = 0;
        public static int SILK_P_H_TIME = 0;
        public static int FW_PC_LIMIT = 0;
        public static int JOB_PC_LIMIT = 0;
        public static int CTF_PC_LIMIT = 0;
        public static int BA_PC_LIMIT = 0;
        //public static int TG_REWARD = 0;
        //public static int CERB_REWARD = 0;
        //public static int CI_REWARD = 0;
        //public static int URU_REWARD = 0;
        //public static int ISY_REWARD = 0;
        //public static int LY_REWARD = 0;
        //public static int DS_REWARD = 0;
        //public static int BY_REWARD = 0;
        //public static int ROC_REWARD = 0;
        //public static int WK_90_REWARD = 0;
        //public static int WK_100_REWARD = 0;
        //public static int SOSO_BK_REWARD = 0;
        public static int SILK_PH_REQLVL = 0;
        public static bool DISABLE_SILK_PH_REWARD_AFK = false;
        public static bool AFK_DETECTION = false;
        public static bool DC_AFKERS_CTF = false;
        public static bool DC_AFKERS_FW = false;
        public static bool DC_AFKERS_BA = false;
        public static int SCHEDULED_NOTICE_EVERY = 0;
        public static string SCHEDULED_NOTICE_MESSAGE = string.Empty;
        public static bool UPLOAD_TO_MEGA = false;
        public static string MEGA_USERNAME = string.Empty;
        public static string MEGA_PASSWORD = string.Empty;
        public static string LOGIN_WELCOME_MSG = string.Empty;//登录欢迎消息
        // SECURITY
        public static int SERVER_IP_LIMIT = 0;
        public static bool SCA_COUNTRY = false;
        public static string SCA_COUNTRY_NAME = string.Empty;
        public static int GW_BPS_VALUE = 0;
        public static int AG_BPS_VALUE = 0;
        public static int GW_PPS_VALUE = 0;
        public static int AG_PPS_VALUE = 0;
        public static bool PACKET_PROCESSOR = false;
        public static bool MALICIOUS_OPCODE = false;
        public static bool PUNISHMENT_BAN = false;
        public static bool GM_PRIVG_LVL = false;
        // ACTION DELAYS
        public static int STALL_DELAY = 0;
        public static int GUILD_REQ_DELAY = 0;
        public static int UNION_REQ_DELAY = 0;
        public static int EXCHANGE_REQ_DELAY = 0;
        public static int GLOBAL_CHAT_DELAY = 0;
        public static int EXIT_DELAY = 0;
        public static int RESTART_DELAY = 0;
        public static int ZERK_DELAY = 0;
        // REGULAR MODE
        public static int GUILD_MAX_LIMIT = 0;
        public static int UNION_MAX_LIMIT = 0;
        public static int GLOBAL_REQ_LVL = 0;
        public static int PLUS_MAX_LIMIT = 0;
        public static int CTF_REQ_LVL = 0;
        public static int BA_REQ_LVL = 0;
        public static bool ACADEMY_CREATION = false;
        public static int PLUS_REQ_NOTICE = 0;
        public static bool SKILL_PREVENTION = false;
        public static bool FW_SKILL_PREVENTION = false;
        public static bool DISABLE_TAX_RATE_CHANGE = false;
        public static bool DISABLE_ITEM_OR_GOLD_DROP_INTOWN = false;
        public static bool DISABLE_SUMMON_FW_PETS = false;
        public static bool DISABLE_FELLOW_UNDER_ZERK = false;
        public static bool LOCK_SYSTEM = false;
        public static bool FW_RES_SCROLL = false;
        public static bool FW_TRACE_PREVENTION = false;
        public static bool CTF_SKILL_PREVENTION = false;
        public static bool BA_SKILL_PREVENTION = false;
        public static bool RESTART_DISABLE = false;
        public static bool CUSTOM_YELLOW_TITLE_SYSTEM = false;
        public static bool DISABLE_JOB_MODE = false;
        public static int DISABLE_ADVANCED_ELIXIR_ON_DEGREE = 0;
        public static int[] MAXPLUSINFO = null;
        // JOB MODE
        public static bool JOB_REVERSE_DEATH_POINT = false;
        public static bool JOB_REVERSE_LAST_RECALL_POINT = false;
        public static bool JOB_RESS_SCROLL = false;
        public static bool JOB_ANTI_TRACE = false;
        public static int JOB_RESS_ACCEPTION_DELAY = 0;
        public static bool JOB_GOODS_DROPOUT = false;
        public static bool DISABLE_THIEF_REWARD_MENU_ACCESS = false;
        public static bool DISABLE_FELLOW_UNDER_JOB = false;
        public static bool ONE_CHAR_ALLOWED_JOBFLAG_INSIDE_ACC = false;
        public static bool JOB_REVERSE_MAP_POINT = false;
        public static bool JOB_SKILL_PREVENTION = false;
        // SR_CLIENT
        public static string CL_HOST_IP = string.Empty;
        public static int CL_GW_PORT = 0;
        public static uint CL_VERSION = 0;
        public static uint CL_LOCALE = 0;
        public static string CL_CAPTCHA_VALUE = string.Empty;
        public static string CL_ID = string.Empty;
        public static string CL_PW = string.Empty;
        public static string CL_CharName = string.Empty;
        // BOTS POLICY 
        public static bool BOT_ALLOW = true;
        public static bool BOT_ALLOW_ALCHEMY_ELIXIR = true;
        public static bool BOT_ALLOW_ALCHEMY_STONE = true;
        public static bool BOT_ALLOW_CREATE_PARTY = true;
        public static bool BOT_ALLOW_INVITE_PARTY = true;
        public static bool BOT_ALLOW_EXCHANGE = true;
        public static bool BOT_ALLOW_STALL = true;
        public static bool BOT_ALLOW_ARENA = true;
        public static bool BOT_ALLOW_CTF = true;
        public static bool BOT_ALLOW_FORTRESS = true;
        public static bool BOT_ALLOW_PVP = true;
        public static bool BOT_ALLOW_TRACE = true;
        // LOGS MANAGER
        public static bool LOG_PLAYERS_ALL_CHAT = false;
        public static bool LOG_PLAYERS_PM_CHAT = false;
        public static bool LOG_PLAYERS_GM_CHAT = false;
        public static bool LOG_PLAYERS_PT_CHAT = false;
        public static bool LOG_PLAYERS_GUILD_CHAT = false;
        public static bool LOG_PLAYERS_UNI_CHAT = false;
        public static bool LOG_PLAYERS_LOADIMAGE_TIMESPAN = false;
        public static bool LOG_PROXY_ERRORS = false;
        public static bool LOG_MODULE_CRASH_DUMP = false;
        public static bool LOG_MAGIC_POP_PLAY = false;
        //AUTO EVENTS
        public static int START_EVENT_EVERY_MINUTES = 0;
        public static bool ENABLE_RETYPE = true;
        public static int RETYPE_SILK_REWARD = 0;
        public static bool ENABLE_TRIVIA = true;
        public static int TRIVIA_SILK_REWARD = 0;
        public static bool ENABLE_LUCKY_STALLER = true;
        public static int LUCKY_STALLER_SILK_REWARD = 0;
        public static bool ENABLE_ALCHEMY = true;
        public static int ALCHEMY_SILK_REWARD = 0;
        public static bool ENABLE_FIRST_TYPE = true;
        public static int FIRST_TYPE_SILK_REWARD = 0;
        public static bool ENABLE_LONGEST_ONLINE = true;
        public static int LONGEST_ONLINE_SILK_REWARD = 0;
        public static bool ENABLE_MATH = true;
        public static int MATH_SILK_REWARD = 0;
        public static bool ENABLE_LOTTO = true;
        public static int LOTTO_SILK_REWARD = 0;
        public static bool ENABLE_LUCKY_PARTY_NUMBER = true;
        public static int LUCKY_PARTY_NUMBER_SILK_REWARD = 0;
        public static bool ENABLE_HIDE_AND_SEEK = true;
        public static int HIDE_AND_SEEK_SILK_REWARD = 0;
        public static bool ENABLE_UNIQUE_EVENT = true;
        public static bool ADJUST_UNQ_EVENT_QUANITITY = true;
        //AUTO EVENTS TIMEOUTS MISC
        public static int RETYPE_TO = 0;
        public static int TRIVIA_TO = 0;
        public static int LUCKY_STALLER_TO = 0;
        public static int ALCHEMY_TO = 0;
        public static int ALCHEMY_MINPLUS = 0;
        public static int ALCHEMY_MAXPLUS = 0;
        public static int FIRST_TYPE_TO = 0;
        public static int LONGEST_ONLINE_TO = 0;
        public static int MATH_TO = 0;
        public static int LOTTO_TO = 0;
        public static int LUCKY_PARTY_NUMBER_TO = 0;
        public static int HIDE_AND_SEEK_TO = 0;
        /* WARNING WARNING WARNINNG !!! */
        //NOT IN GUI or hardcoded, Before compiling:
        //change GIVE_TROPHIES to GIVE_SILKS in custom unqiues reward (ag module class)
        //Set license time amount, license IP, set settings below
        public static string INFORMER_VERSION = "1.0.0";
        public static bool INFORMER_LICENSE_TYPE = true; //True = Premium, False = Free
        public static bool REMOTE_CMD = true;
        public static bool LOG_PLAYERS_STATUS = true;
        public static bool FAKE_PALYERS_BASED_ON_TABLE = false;
        public static bool CHECK_WEB_PROF_PRIV_FOR_PLUS_NOTICE = false;
        public static bool POINTS_SYSTEM = false;
        public static bool DISABLE_OUR_SCROLLS_IN_TOWNS = false;
        public static bool BOTS_ENGINE = false;


        public static string FILTER_DB = "xQc_FILTER";

    }
}
