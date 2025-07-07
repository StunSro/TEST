using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SR_PROXY.ENGINES
{
    class WRITER
    {
        public static async Task<bool> REWRITE_CFG()
        {
            try
            {
                using (StreamWriter file = new StreamWriter(".\\proxy_cfg.ini", false))
                {
                    await file.WriteLineAsync("[PROXY FRAMEWORK]" + Environment.NewLine +
                    "BIND_IP=" + Settings.MAIN.textBox1.Text + Environment.NewLine +
                    "OUTSOURCE_NETWORKING=" + Convert.ToBoolean(Settings.MAIN.checkBox14.CheckState) + Environment.NewLine +

                    "[GATEWAYSERVER_DEFAULT]" + Environment.NewLine +
                    "PUBLIC_GW_IP=" + Settings.MAIN.textBox17.Text + Environment.NewLine +
                    "PUBLIC_GW_PORT=" + Settings.MAIN.textBox3.Text + Environment.NewLine +
                    "PVT_GW_PORT=" + Settings.MAIN.textBox4.Text + Environment.NewLine +

                    "[DOWNLOADSERVER_DEFAULT]" + Environment.NewLine +
                    "PUBLIC_DW_IP=" + Settings.MAIN.textBox17.Text + Environment.NewLine +
                    "PUBLIC_DW_PORT=" + Settings.MAIN.textBox10.Text + Environment.NewLine +
                    "PVT_DW_PORT=" + Settings.MAIN.textBox9.Text + Environment.NewLine +

                    "[AGENTSERVER_DEFAULT]" + Environment.NewLine +
                    "PUBLIC_AG_IP=" + Settings.MAIN.textBox17.Text + Environment.NewLine +
                    "PUBLIC_AG_PORT=" + Settings.MAIN.textBox6.Text + Environment.NewLine +
                    "PVT_AG_PORT=" + Settings.MAIN.textBox7.Text + Environment.NewLine +

                    "[AGENTSERVER_DEFAULT2]" + Environment.NewLine +
                    "PUBLIC_AG2_IP_ALLOW=" + Convert.ToBoolean(Settings.MAIN.checkBox5.CheckState) + Environment.NewLine +
                    "PUBLIC_AG2_IP=" + Settings.MAIN.textBox91.Text + Environment.NewLine +
                    "PUBLIC_AG2_PORT=" + Settings.MAIN.textBox94.Text + Environment.NewLine +
                    "PVT_AG2_PORT=" + Settings.MAIN.textBox93.Text + Environment.NewLine +

                    "[AGENTSERVER_DEFAULT3]" + Environment.NewLine +
                    "PUBLIC_AG3_IP_ALLOW=" + Convert.ToBoolean(Settings.MAIN.checkBox6.CheckState) + Environment.NewLine +
                    "PUBLIC_AG3_IP=" + Settings.MAIN.textBox92.Text + Environment.NewLine +
                    "PUBLIC_AG3_PORT=" + Settings.MAIN.textBox100.Text + Environment.NewLine +
                    "PVT_AG3_PORT=" + Settings.MAIN.textBox97.Text + Environment.NewLine +


                    "[AGENTSERVER_DEFAULT4]" + Environment.NewLine +
                    "PUBLIC_AG4_IP_ALLOW=" + Convert.ToBoolean(Settings.MAIN.checkBox27.CheckState) + Environment.NewLine +
                    "PUBLIC_AG4_IP=" + Settings.MAIN.textBox111.Text + Environment.NewLine +
                    "PUBLIC_AG4_PORT=" + Settings.MAIN.textBox96.Text + Environment.NewLine +
                    "PVT_AG4_PORT=" + Settings.MAIN.textBox95.Text + Environment.NewLine +

                    "[MSSQL Server]" + Environment.NewLine +
                    "MSSQL_SVR_NAME=" + Settings.MAIN.textBox11.Text + Environment.NewLine +
                    "MSSQL_SVR_ID=" + Settings.MAIN.textBox12.Text + Environment.NewLine +
                    "MSSQL_SVR_PW=" + Settings.MAIN.textBox13.Text + Environment.NewLine +
                    "MSSQL_LOG_DB=" + Settings.MAIN.textBox14.Text + Environment.NewLine +
                    "MSSQL_ACC_DB=" + Settings.MAIN.textBox15.Text + Environment.NewLine +
                    "MSSQL_SHARD_DB=" + Settings.MAIN.textBox16.Text + Environment.NewLine +

                    "[MISCELLANEOUS]" + Environment.NewLine +
                    "PLAYER_LOGON_MSG=" + Settings.MAIN.textBox19.Text + Environment.NewLine +
                    "WELCOME_MSG=" + Settings.MAIN.textBox20.Text + Environment.NewLine +
                    "SHARD_NAME=" + Settings.MAIN.textBox21.Text + Environment.NewLine +
                    "FAKE_PLAYERS=" + Settings.MAIN.textBox22.Text + Environment.NewLine +
                    "REGION_READER=True" + Environment.NewLine +
                    "GMS_ANTI_INVIS=" + Settings.MAIN.textBox25.Text + Environment.NewLine +
                    "CHAT_FILTER=" + Settings.MAIN.textBox26.Text + Environment.NewLine +
                    "BOT_DETECTOR=" + Settings.MAIN.textBox27.Text + Environment.NewLine +
                    "CAPTCHA_REMOVE=" + Settings.MAIN.textBox29.Text + Environment.NewLine +
                    "CAPTCHA_VALUE=" + Settings.MAIN.textBox30.Text + Environment.NewLine +
                    "GAME_GUIDE_DISABLE=" + Settings.MAIN.textBox33.Text + Environment.NewLine +
                    "UNQ_KILL_SILK_REWARD=" + Settings.MAIN.textBox34.Text + Environment.NewLine +
                    //"TG_REWARD=" + Settings.MAIN.textBox111.Text + Environment.NewLine +
                    //"CERB_REWARD=" + Settings.MAIN.textBox112.Text + Environment.NewLine +
                    //"CI_REWARD=" + Settings.MAIN.textBox113.Text + Environment.NewLine +
                    //"URU_REWARD=" + Settings.MAIN.textBox114.Text + Environment.NewLine +
                    //"ISY_REWARD=" + Settings.MAIN.textBox115.Text + Environment.NewLine +
                    //"LY_REWARD=" + Settings.MAIN.textBox116.Text + Environment.NewLine +
                    //"DS_REWARD=" + Settings.MAIN.textBox117.Text + Environment.NewLine +
                    //"BY_REWARD=" + Settings.MAIN.textBox118.Text + Environment.NewLine +
                    //"ROC_REWARD=" + Settings.MAIN.textBox119.Text + Environment.NewLine +
                    //"WK_90_REWARD=" + Settings.MAIN.textBox120.Text + Environment.NewLine +
                    //"WK_100_REWARD=" + Settings.MAIN.textBox121.Text + Environment.NewLine +
                    //"SOSO_BK_REWARD=" + Settings.MAIN.textBox122.Text + Environment.NewLine +
                    "SCHEDULED_NOTICE_EVERY=" + Settings.MAIN.textBox171.Text + Environment.NewLine +
                    "LOGIN_WELCOME_MSG=" + Settings.MAIN.textBox24.Text + Environment.NewLine +
                    "[PROXY SECURITY]" + Environment.NewLine +
                    "SERVER_IP_LIMIT=" + Settings.MAIN.textBox36.Text + Environment.NewLine +
                    "SERVER_HWID_LIMIT=" + Settings.MAIN.textBox2.Text + Environment.NewLine +
                    "SCA_COUNTRY=False" + Environment.NewLine +
                    "SCA_COUNTRY_NAME=Germany" + Environment.NewLine +
                    "GW_BPS_VALUE=" + Settings.MAIN.textBox39.Text + Environment.NewLine +
                    "AG_BPS_VALUE=" + Settings.MAIN.textBox40.Text + Environment.NewLine +
                    "GW_PPS_VALUE=" + Settings.MAIN.textBox41.Text + Environment.NewLine +
                    "AG_PPS_VALUE=" + Settings.MAIN.textBox42.Text + Environment.NewLine +
                    "PACKET_PROCESSOR=" + Settings.MAIN.textBox43.Text + Environment.NewLine +
                    "MALICIOUS_OPCODE=" + Settings.MAIN.textBox44.Text + Environment.NewLine +
                    "PUNISHMENT_BAN=" + Settings.MAIN.textBox45.Text + Environment.NewLine +
                    "GM_PRIVG_LVL=" + Settings.MAIN.textBox46.Text + Environment.NewLine +

                    "[ACTION DELAYS]" + Environment.NewLine +
                    "STALL_DELAY=" + Settings.MAIN.textBox47.Text + Environment.NewLine +
                    "GUILD_REQ_DELAY=" + Settings.MAIN.textBox48.Text + Environment.NewLine +
                    "UNION_REQ_DELAY=" + Settings.MAIN.textBox49.Text + Environment.NewLine +
                    "EXCHANGE_REQ_DELAY=" + Settings.MAIN.textBox50.Text + Environment.NewLine +
                    "GLOBAL_CHAT_DELAY=" + Settings.MAIN.textBox51.Text + Environment.NewLine +
                    "EXIT_DELAY=" + Settings.MAIN.textBox102.Text + Environment.NewLine +
                    "RESTART_DELAY=" + Settings.MAIN.textBox103.Text + Environment.NewLine +
                    "BERSEKER_DELAY=" + Settings.MAIN.textBox104.Text + Environment.NewLine +

                    "[REGULAR MODE - LIMITS\\REQUIRED LEVEL]" + Environment.NewLine +
                    "GUILD_MAX_LIMIT=" + Settings.MAIN.textBox52.Text + Environment.NewLine +
                    "UNION_MAX_LIMIT=" + Settings.MAIN.textBox53.Text + Environment.NewLine +
                    "GLOBAL_REQ_LVL=" + Settings.MAIN.textBox54.Text + Environment.NewLine +
                    "PLUS_MAX_LIMIT=" + Settings.MAIN.textBox55.Text + Environment.NewLine +
                    "CTF_REQ_LVL=" + Settings.MAIN.textBox56.Text + Environment.NewLine +
                    "BA_REQ_LVL=" + Settings.MAIN.textBox57.Text + Environment.NewLine +
                    "ACADEMY_CREATION=" + Settings.MAIN.textBox58.Text + Environment.NewLine +
                    "DISABLE_TAX_RATE_CHANGE=" + Settings.MAIN.textBox105.Text + Environment.NewLine +
                    "DISABLE_ITEM_OR_GOLD_DROP_INTOWN=" + Settings.MAIN.textBox106.Text + Environment.NewLine +
                    "DISABLE_SUMMON_FW_PETS=" + Settings.MAIN.textBox107.Text + Environment.NewLine +
                    "DISABLE_FELLOW_UNDER_ZERK=" + Settings.MAIN.textBox108.Text + Environment.NewLine +
                    "FW_RES_SCROLL=" + Settings.MAIN.textBox130.Text + Environment.NewLine +
                    "FW_TRACE_PREVENTION=" + Settings.MAIN.textBox131.Text + Environment.NewLine +
                    "RESTART_DISABLE=" + Settings.MAIN.textBox162.Text + Environment.NewLine +
                    "DISABLE_JOB_MODE=" + Settings.MAIN.textBox169.Text + Environment.NewLine +
                    "DISABLE_ADVANCED_ELIXIR_ON_DEGREE=" + Settings.MAIN.textBox170.Text + Environment.NewLine +
                    "WEAP_MAX_PLUS=" + Settings.MAIN.numericUpDown22.Value.ToString() + Environment.NewLine +
                    "SET_MAX_PLUS=" + Settings.MAIN.numericUpDown23.Value.ToString() + Environment.NewLine +
                    "ACC_MAX_PLUS=" + Settings.MAIN.numericUpDown24.Value.ToString() + Environment.NewLine +
                    "SHIELD_MAX_PLUS=" + Settings.MAIN.numericUpDown25.Value.ToString() + Environment.NewLine +
                    "DEVIL_MAX_PLUS=" + Settings.MAIN.numericUpDown28.Value.ToString() + Environment.NewLine +
                    "[JOB MODE - LIMITS\\DELAYS]" + Environment.NewLine +
                    "JOB_REVERSE_DEATH_POINT=" + Settings.MAIN.textBox62.Text + Environment.NewLine +
                    "JOB_REVERSE_LAST_RECALL_POINT=" + Settings.MAIN.textBox63.Text + Environment.NewLine +
                    "JOB_RESS_SCROLL=" + Settings.MAIN.textBox64.Text + Environment.NewLine +
                    "JOB_ANTI_TRACE=" + Settings.MAIN.textBox65.Text + Environment.NewLine +
                    "JOB_RESS_ACCEPTION_DELAY=" + Settings.MAIN.textBox66.Text + Environment.NewLine +
                    "JOB_GOODS_DROPOUT=" + Settings.MAIN.textBox67.Text + Environment.NewLine +
                    "DISABLE_THIEF_REWARD_MENU_ACCESS=" + Settings.MAIN.textBox109.Text + Environment.NewLine +
                    "DISABLE_FELLOW_UNDER_JOB=" + Settings.MAIN.textBox110.Text + Environment.NewLine +
                    "ONE_CHAR_ALLOWED_JOBFLAG_INSIDE_ACC=" + Settings.MAIN.textBox134.Text + Environment.NewLine +
                    "JOB_REVERSE_MAP_POINT=" + Settings.MAIN.textBox135.Text + Environment.NewLine +
                    "DISABLE_JOB_BETWEEN_SPECIFIC_TIME=" + Convert.ToBoolean(Settings.MAIN.checkBox26.CheckState) + Environment.NewLine +
                    "DISABLE_JOB_START_TIME=" + Settings.MAIN.RbLimitStartDTP.Text + Environment.NewLine +
                    "DISABLE_JOB_END_TIME=" + Settings.MAIN.RbLimitEndDTP.Text + Environment.NewLine +
                    "DISABLE_JOB_PC_LIMIT=" + Settings.MAIN.numericUpDown16.Value + Environment.NewLine +

                    "[BOTS POLICY]" + Environment.NewLine +
                    "BOT_ALLOW=" + Settings.MAIN.textBox68.Text + Environment.NewLine +
                    "BOT_ALLOW_ALCHEMY_ELIXIR=" + Settings.MAIN.textBox69.Text + Environment.NewLine +
                    "BOT_ALLOW_ALCHEMY_STONE=" + Settings.MAIN.textBox70.Text + Environment.NewLine +
                    "BOT_ALLOW_CREATE_PARTY=" + Settings.MAIN.textBox71.Text + Environment.NewLine +
                    "BOT_ALLOW_INVITE_PARTY=" + Settings.MAIN.textBox72.Text + Environment.NewLine +
                    "BOT_ALLOW_EXCHANGE=" + Settings.MAIN.textBox73.Text + Environment.NewLine +
                    "BOT_ALLOW_STALL=" + Settings.MAIN.textBox74.Text + Environment.NewLine +
                    "BOT_ALLOW_ARENA=" + Settings.MAIN.textBox75.Text + Environment.NewLine +
                    "BOT_ALLOW_CTF=" + Settings.MAIN.textBox76.Text + Environment.NewLine +
                    "BOT_ALLOW_FORTRESS=" + Settings.MAIN.textBox77.Text + Environment.NewLine +
                    "BOT_ALLOW_PVP=" + Settings.MAIN.textBox78.Text + Environment.NewLine +
                    "BOT_ALLOW_TRACE=" + Settings.MAIN.textBox79.Text + Environment.NewLine +

                    "[LOGS MANAGER]" + Environment.NewLine +
                    "LOG_PLAYERS_ALL_CHAT=" + Settings.MAIN.textBox80.Text + Environment.NewLine +
                    "LOG_PLAYERS_PM_CHAT=" + Settings.MAIN.textBox81.Text + Environment.NewLine +
                    "LOG_PLAYERS_PT_CHAT=" + Settings.MAIN.textBox83.Text + Environment.NewLine +
                    "LOG_PLAYERS_GUILD_CHAT=" + Settings.MAIN.textBox84.Text + Environment.NewLine +
                    "LOG_PLAYERS_UNI_CHAT=" + Settings.MAIN.textBox85.Text + Environment.NewLine +
                    "LOG_PLAYERS_LOADIMAGE_TIMESPAN=" + Settings.MAIN.textBox86.Text + Environment.NewLine +
                    "LOG_PROXY_ERRORS=" + Settings.MAIN.textBox87.Text + Environment.NewLine +
                    "LOG_MODULE_CRASH_DUMP=" + Settings.MAIN.textBox88.Text + Environment.NewLine +
                    "LOG_MAGIC_POP_PLAY=" + Settings.MAIN.textBox89.Text + Environment.NewLine +

                    "[NEW SETTINGS]" + Environment.NewLine +
                    "SHARD_PATH=" + Settings.MAIN.textBox23.Text + Environment.NewLine +
                    "DISABLE_TITLE_MANAGER=" + Convert.ToBoolean(Settings.MAIN.JobTitle.CheckState) + Environment.NewLine +
                    "DISABLE_GRANT_NAME=" + Convert.ToBoolean(Settings.MAIN.checkBox11.CheckState) + Environment.NewLine +
                    "DISABLE_UNIQUE_HISTORY=" + Convert.ToBoolean(Settings.MAIN.checkBox12.CheckState) + Environment.NewLine +
                    "DISABLE_CHARACTER_LOCK=" + Convert.ToBoolean(Settings.MAIN.checkBox16.CheckState) + Environment.NewLine +
                    "DISABLE_RANKING=" + Convert.ToBoolean(Settings.MAIN.checkBox15.CheckState) + Environment.NewLine +
                    "DISABLE_EVENTS_SCHEDULE=" + Convert.ToBoolean(Settings.MAIN.checkBox13.CheckState) + Environment.NewLine +
                    "DISABLE_CUSTOM_TITLE=" + Convert.ToBoolean(Settings.MAIN.checkBox24.CheckState) + Environment.NewLine +
                    "DISABLE_EVENTS_REGISTER=" + Convert.ToBoolean(Settings.MAIN.checkBox20.CheckState) + Environment.NewLine +
                    "DISABLE_CHANGELOG=" + Convert.ToBoolean(Settings.MAIN.checkBox17.CheckState) + Environment.NewLine +
                    "DISABLE_TELEPORT_BUTTON_FROM_LEVEL=" + Settings.MAIN.numericUpDown27.Value.ToString() + Environment.NewLine +
                    "ALLOW_UPDATING_HONOR_BUFFS_EVERY=" + Settings.MAIN.textBox5.Text + Environment.NewLine +
                    "ALLOW_UPDATING_RANKINGS_EVERY=" + Settings.MAIN.textBox28.Text + Environment.NewLine +
                    "DISABLE_CUSTOM_NAME=" + Convert.ToBoolean(Settings.MAIN.checkBox31.CheckState) + Environment.NewLine +
                    "DISABLE_CUSTOM_RANKNAME=" + Convert.ToBoolean(Settings.MAIN.checkBox32.CheckState) + Environment.NewLine +
                    "DISABLE_CUSTOM_ICON=" + Convert.ToBoolean(Settings.MAIN.checkBox34.CheckState) + Environment.NewLine +
                    "DISABLE_SPECIAL_REVERSE=" + Convert.ToBoolean(Settings.MAIN.checkBox35.CheckState) + Environment.NewLine +
                    "DISABLE_PLAYERS_COUNT_AT_LOGIN_SCREEN=" + Convert.ToBoolean(Settings.MAIN.checkBox36.CheckState) + Environment.NewLine +
                    "DISABLE_PLAYERS_COUNT_AT_LOGIN_LIST=" + Convert.ToBoolean(Settings.MAIN.checkBox37.CheckState) + Environment.NewLine +
                    "CUSTOM_TITLE_PRICE=" + Settings.MAIN.textBox82.Text + Environment.NewLine +
                    "CRIMISON_GLOBAL_ID=" + Settings.MAIN.textBox31.Text + Environment.NewLine +
                    "BLUE_GLOBAL_ID=" + Settings.MAIN.textBox32.Text + Environment.NewLine +
                    "LIGHTGREEN_GLOBAL_ID=" + Settings.MAIN.textBox35.Text + Environment.NewLine +
                    "RED_GLOBAL_ID=" + Settings.MAIN.textBox37.Text + Environment.NewLine +
                    "ORANGE_GLOBAL_ID=" + Settings.MAIN.textBox38.Text + Environment.NewLine +
                    "GREEN_GLOBAL_ID=" + Settings.MAIN.textBox59.Text + Environment.NewLine +
                    "PURPLE_GLOBAL_ID=" + Settings.MAIN.textBox60.Text + Environment.NewLine +
                    "PINK_GLOBAL_ID=" + Settings.MAIN.textBox61.Text + Environment.NewLine +
                    "DISABLE_TELEPORT_BUTTON=" + Convert.ToBoolean(Settings.MAIN.checkBox7.CheckState) + Environment.NewLine +
                    "ALLOW_UPDATING_HONOR_BUFFS=" + Convert.ToBoolean(Settings.MAIN.checkBox29.CheckState) + Environment.NewLine +
                    "ALLOW_UPDATING_RANKING=" + Convert.ToBoolean(Settings.MAIN.checkBox30.CheckState) + Environment.NewLine +
                    "DISABLE_CUSTOM_TITLE_COLOR=" + Convert.ToBoolean(Settings.MAIN.checkBox33.CheckState) + Environment.NewLine +
                    "ENABLE_STALL_REWARD_HOUR=" + Convert.ToBoolean(Settings.MAIN.checkBox18.CheckState) + Environment.NewLine +
                    "RESTRCTION_STALL_REWARD_LEVEL=" + Settings.MAIN.numericUpDown1.Value.ToString() + Environment.NewLine +
                    "STALL_REWARD_GOLD=" + Settings.MAIN.numericUpDown4.Value.ToString() + Environment.NewLine +
                    "STALL_REWARD_SILK=" + Settings.MAIN.numericUpDown5.Value.ToString() + Environment.NewLine +
                    "ENABLE_PARTY_REWARD_HOUR=" + Convert.ToBoolean(Settings.MAIN.checkBox23.CheckState) + Environment.NewLine +
                    "RESTRCTION_PARTY_REWARD_LEVEL=" + Settings.MAIN.numericUpDown2.Value.ToString() + Environment.NewLine +
                    "PARTY_REWARD_GOLD=" + Settings.MAIN.numericUpDown7.Value.ToString() + Environment.NewLine +
                    "PARTY_REWARD_SILK=" + Settings.MAIN.numericUpDown6.Value.ToString() + Environment.NewLine +
                    "ENABLE_SILKGOLD_REWARD_HOUR=" + Convert.ToBoolean(Settings.MAIN.checkBox25.CheckState) + Environment.NewLine +
                    "RESTRCTION_SILKGOLD_REWARD_LEVEL=" + Settings.MAIN.numericUpDown3.Value.ToString() + Environment.NewLine +
                    "SILKGOLD_REWARD_GOLD=" + Settings.MAIN.numericUpDown9.Value.ToString() + Environment.NewLine +
                    "SILKGOLD_REWARD_SILK=" + Settings.MAIN.numericUpDown8.Value.ToString() + Environment.NewLine +
                    "ALLOW_DROP_GOODS_WHEN_GO_OFFLINE=" + Settings.MAIN.textBox112.Text + Environment.NewLine +
                    "FACEBOOK_LINK=" + Settings.MAIN.textBox113.Text + Environment.NewLine +
                    "DISCORD_LINK=" + Settings.MAIN.textBox114.Text + Environment.NewLine +
                    "WEBSITE_LINK=" + Settings.MAIN.textBox115.Text + Environment.NewLine +
                    "ALLOW_DISCORD_RPC=" + Convert.ToBoolean(Settings.MAIN.checkBox28.CheckState) + Environment.NewLine +
                    "DISCORD_RPC=" + Settings.MAIN.textBox116.Text + Environment.NewLine +
                    "FW_PC_LIMIT=" + Settings.MAIN.numericUpDown10.Value.ToString() + Environment.NewLine +
                    "CTF_PC_LIMIT=" + Settings.MAIN.numericUpDown11.Value.ToString() + Environment.NewLine +
                    "BA_PC_LIMIT=" + Settings.MAIN.numericUpDown12.Value.ToString() + Environment.NewLine +
                    "JOB_PC_LIMIT=" + Settings.MAIN.numericUpDown13.Value.ToString() + Environment.NewLine +
                    "DISABLE_ZERK_AT_FW=" + Settings.MAIN.textBox120.Text + Environment.NewLine +
                    "PLUS_NOTICE_REQ_VALUE=" + Settings.MAIN.numericUpDown14.Value.ToString() + Environment.NewLine +
                    "ALLOW_GAMBLING_SYSTEM=" + Convert.ToBoolean(Settings.MAIN.checkBox38.CheckState) + Environment.NewLine +
                    "WIN_PERCENTAGE=" + Settings.MAIN.numericUpDown15.Value.ToString() + Environment.NewLine +
                    "MAX_ATTEMPTS=" + Settings.MAIN.numericUpDown20.Value.ToString() + Environment.NewLine +
                    "START=" + Settings.MAIN.dateTimePicker2.Text + Environment.NewLine +
                    "END=" + Settings.MAIN.dateTimePicker1.Text + Environment.NewLine +
                    "[FILTER TEXTS]" + Environment.NewLine +
                    "DRUNK_10MINS=" + Settings.MAIN.textBox136.Text + Environment.NewLine +
                    "DRUNK_5MINS=" + Settings.MAIN.textBox133.Text + Environment.NewLine +
                    "DRUNK_1MINS=" + Settings.MAIN.textBox132.Text + Environment.NewLine +
                    "DRUNK_END=" + Settings.MAIN.textBox129.Text + Environment.NewLine +
                    "DRUNK_WINNER1=" + Settings.MAIN.textBox128.Text + Environment.NewLine +
                    "DRUNK_REGISTER=" + Settings.MAIN.textBox203.Text + Environment.NewLine +
                    "DRUNK_REGISTERED=" + Settings.MAIN.textBox202.Text + Environment.NewLine +
                    "DRUNK_UNREGISTER=" + Settings.MAIN.textBox201.Text + Environment.NewLine +
                    "DRUNK_NOTREGISTERED=" + Settings.MAIN.textBox200.Text + Environment.NewLine +
                    "SURVIVAL_10MINS=" + Settings.MAIN.textBox118.Text + Environment.NewLine +
                    "SURVIVAL_5MINS=" + Settings.MAIN.textBox119.Text + Environment.NewLine +
                    "SURVIVAL_1MINS=" + Settings.MAIN.textBox121.Text + Environment.NewLine +
                    "SURVIVAL_END=" + Settings.MAIN.textBox124.Text + Environment.NewLine +
                    "SURVIVAL_WINNER1=" + Settings.MAIN.textBox123.Text + Environment.NewLine +
                    "SURVIVAL_WINNER2=" + Settings.MAIN.textBox122.Text + Environment.NewLine +
                    "SURVIVAL_WINNER3=" + Settings.MAIN.textBox125.Text + Environment.NewLine +
                    "SURVIVAL_REGISTER=" + Settings.MAIN.textBox199.Text + Environment.NewLine +
                    "SURVIVAL_REGISTERED=" + Settings.MAIN.textBox198.Text + Environment.NewLine +
                    "SURVIVAL_UNREGISTER=" + Settings.MAIN.textBox197.Text + Environment.NewLine +
                    "SURVIVAL_NOTREGISTERED=" + Settings.MAIN.textBox196.Text + Environment.NewLine +
                    "ZERK_DELAY=" + Settings.MAIN.textBox139.Text + Environment.NewLine +
                    "ZERK_UNDER_PET=" + Settings.MAIN.textBox138.Text + Environment.NewLine +
                    "ZERK_FORTRESSWAR=" + Settings.MAIN.textBox137.Text + Environment.NewLine +
                    "ZERK_REGION=" + Settings.MAIN.textBox127.Text + Environment.NewLine +
                    "DISABLE_RESTART=" + Settings.MAIN.textBox126.Text + Environment.NewLine +
                    "RESTART_DELAY=" + Settings.MAIN.textBox140.Text + Environment.NewLine +
                    "EXIT_DELAY=" + Settings.MAIN.textBox141.Text + Environment.NewLine +
                    "DROP_GOODS_TOWN=" + Settings.MAIN.textBox144.Text + Environment.NewLine +
                    "BOT_ALLOW_STALL=" + Settings.MAIN.textBox143.Text + Environment.NewLine +
                    "CHAT_FILTER_STALL=" + Settings.MAIN.textBox142.Text + Environment.NewLine +
                    "STALL_DELAY=" + Settings.MAIN.textBox147.Text + Environment.NewLine +
                    "GUILD_INVITE_DELAY=" + Settings.MAIN.textBox146.Text + Environment.NewLine +
                    "GUILD_LIMIT=" + Settings.MAIN.textBox145.Text + Environment.NewLine +
                    "UNION_REQ_DELAY=" + Settings.MAIN.textBox151.Text + Environment.NewLine +
                    "UNION_LIMIT=" + Settings.MAIN.textBox150.Text + Environment.NewLine +
                    "BOT_ALLOW_EXCHANGE=" + Settings.MAIN.textBox149.Text + Environment.NewLine +
                    "EXCHANGE_DELAY=" + Settings.MAIN.textBox148.Text + Environment.NewLine +
                    "JOB_SKILL_PET=" + Settings.MAIN.textBox152.Text + Environment.NewLine +
                    "FW_TRACE_DISABLE=" + Settings.MAIN.textBox160.Text + Environment.NewLine +
                    "REGION_TRACE_DISABLE=" + Settings.MAIN.textBox159.Text + Environment.NewLine +
                    "DISABLE_SKILL_REGION=" + Settings.MAIN.textBox158.Text + Environment.NewLine +
                    "JOB_ACCEPT_RES=" + Settings.MAIN.textBox154.Text + Environment.NewLine +
                    "ITEM_BLOCK_REGION=" + Settings.MAIN.textBox153.Text + Environment.NewLine +
                    "GLOBAL_CHAT_FILTER=" + Settings.MAIN.textBox165.Text + Environment.NewLine +
                    "GLOBAL_LEVEL_FILTER=" + Settings.MAIN.textBox164.Text + Environment.NewLine +
                    "GLOBAL_DELAY=" + Settings.MAIN.textBox163.Text + Environment.NewLine +
                    "REVERSE_DEATH_POINT=" + Settings.MAIN.textBox161.Text + Environment.NewLine +
                    "GAMBLE_DELAY=" + Settings.MAIN.textBox217.Text + Environment.NewLine +
                    "GAMBLE_DISABLED=" + Settings.MAIN.textBox216.Text + Environment.NewLine +
                    "GAMBLE_BETWEEN=" + Settings.MAIN.textBox214.Text + Environment.NewLine +
                    "ENOUGH_SILK=" + Settings.MAIN.textBox221.Text + Environment.NewLine +
                    "ENOUGH_GOLD=" + Settings.MAIN.textBox220.Text + Environment.NewLine +
                    "WON_GAMBLE=" + Settings.MAIN.textBox219.Text + Environment.NewLine +
                    "LOSE_GAMBLE=" + Settings.MAIN.textBox218.Text + Environment.NewLine +
                    "XSMB_BETWEEN=" + Settings.MAIN.textBox223.Text + Environment.NewLine +
                    "NUM_00_99=" + Settings.MAIN.textBox222.Text + Environment.NewLine +
                    "MIN_XSMB_SILK=" + Settings.MAIN.textBox230.Text + Environment.NewLine +
                    "MIN_XSMB_GOLD=" + Settings.MAIN.textBox229.Text + Environment.NewLine +
                    "XSMB_RESTRECTION=" + Settings.MAIN.textBox225.Text + Environment.NewLine +
                    "XSMB_SUCCESS=" + Settings.MAIN.textBox224.Text + Environment.NewLine +
                    "JOB_REVERSE_LAST_POINT=" + Settings.MAIN.textBox180.Text + Environment.NewLine +
                    "JOB_MAP_POINT=" + Settings.MAIN.textBox179.Text + Environment.NewLine +
                    "SUMMON_PET_JOB=" + Settings.MAIN.textBox178.Text + Environment.NewLine +
                    "CHAT_REGION=" + Settings.MAIN.textBox177.Text + Environment.NewLine +
                    "INVITE_PARTY=" + Settings.MAIN.textBox176.Text + Environment.NewLine +
                    "CH_EU_PARTY=" + Settings.MAIN.textBox175.Text + Environment.NewLine +
                    "PARTY_REGION=" + Settings.MAIN.textBox174.Text + Environment.NewLine +
                    "LIMIT_GOODS=" + Settings.MAIN.textBox173.Text + Environment.NewLine +
                    "BUY_SELL_GOODS=" + Settings.MAIN.textBox172.Text + Environment.NewLine +
                    "DISABLE_JOB=" + Settings.MAIN.textBox168.Text + Environment.NewLine +
                    "JOB_LIMIT=" + Settings.MAIN.textBox167.Text + Environment.NewLine +
                    "SPAWN_PET_ON_JOB=" + Settings.MAIN.textBox166.Text + Environment.NewLine +
                    "ONE_CHARACTER_JOB=" + Settings.MAIN.textBox192.Text + Environment.NewLine +
                    "DROP_ITEMS_IN_TOWN=" + Settings.MAIN.textBox191.Text + Environment.NewLine +
                    "THIEF_REWARD_ACCESS=" + Settings.MAIN.textBox190.Text + Environment.NewLine +
                    "DISABLE_TAX_RATE=" + Settings.MAIN.textBox189.Text + Environment.NewLine +
                    "JOB_PET_MOVE=" + Settings.MAIN.textBox188.Text + Environment.NewLine +
                    "MAX_PLUS_LIMIT=" + Settings.MAIN.textBox187.Text + Environment.NewLine +
                    "DISABLED_FEATURES=" + Settings.MAIN.textBox186.Text + Environment.NewLine +
                    "ITEM_CHEST_SLOTS=" + Settings.MAIN.textBox185.Text + Environment.NewLine +
                    "UPDATE_TITLE_DELAY=" + Settings.MAIN.textBox184.Text + Environment.NewLine +
                    "PURCHASE_CUSTOM_TITLE=" + Settings.MAIN.textBox183.Text + Environment.NewLine +
                    "SAME_CUSTOM_TITLE=" + Settings.MAIN.textBox182.Text + Environment.NewLine +
                    "AUT0_EQUIPT_DELAY=" + Settings.MAIN.textBox181.Text + Environment.NewLine +
                    "XSMB_LOG_REFRESH=" + Settings.MAIN.textBox195.Text + Environment.NewLine +
                    "NEW_REVERSE_MOVE=" + Settings.MAIN.textBox194.Text + Environment.NewLine +
                    "NEW_REVERSE_DELAY=" + Settings.MAIN.textBox193.Text + Environment.NewLine +
                    "NEW_REVERSE_LOCATION=" + Settings.MAIN.textBox207.Text + Environment.NewLine +
                    "CHARACTER_UNLOCK=" + Settings.MAIN.textBox206.Text + Environment.NewLine +
                    "ALREADY_UNLOCKED=" + Settings.MAIN.textBox205.Text + Environment.NewLine +
                    "LOCK_INCORRECTPW=" + Settings.MAIN.textBox204.Text + Environment.NewLine +
                    "LOCKED_SUCCESSFULLY=" + Settings.MAIN.textBox210.Text + Environment.NewLine +
                    "ALREADY_LOCKED=" + Settings.MAIN.textBox209.Text + Environment.NewLine +
                    "LOCK_REMOVED=" + Settings.MAIN.textBox208.Text + Environment.NewLine +
                    "ALREADY_HAVE_LOCK=" + Settings.MAIN.textBox212.Text + Environment.NewLine +
                    "PASSWORD_DIGITS_ONLY=" + Settings.MAIN.textBox211.Text + Environment.NewLine +
                    "LOCK_BLOCK=" + Settings.MAIN.textBox213.Text + Environment.NewLine +
                    "DAILY_REWARD=" + Settings.MAIN.textBox236.Text + Environment.NewLine +
                    "PLUS_LIMIT=" + Settings.MAIN.textBox235.Text + Environment.NewLine +
                    "PLUS_LIMIT_ADVANCE=" + Settings.MAIN.textBox234.Text + Environment.NewLine +
                    "[ADVANCED III]" + Environment.NewLine +
                    "ALLOW_XSMB_EVENT=" + Convert.ToBoolean(Settings.MAIN.checkBox39.CheckState) + Environment.NewLine +
                    "ALLOW_XSMB_EVENT_SILK=" + Convert.ToBoolean(Settings.MAIN.checkBox40.CheckState) + Environment.NewLine +
                    "ALLOW_XSMB_EVENT_GOLD=" + Convert.ToBoolean(Settings.MAIN.checkBox41.CheckState) + Environment.NewLine +
                    "DISABLE_CH_EU_SAME_PT=" + Convert.ToBoolean(Settings.MAIN.checkBox42.CheckState) + Environment.NewLine +
                    "ALLOW_SURVIVAL_EVENT=" + Convert.ToBoolean(Settings.MAIN.checkBox43.CheckState) + Environment.NewLine +
                    "TOP_ONE=" + Settings.MAIN.textBox215.Text + Environment.NewLine +
                    "TOP_TWO=" + Settings.MAIN.textBox226.Text + Environment.NewLine +
                    "TOP_THREE=" + Settings.MAIN.textBox227.Text + Environment.NewLine +
                    "ALLOW_DRUNK_EVENT=" + Convert.ToBoolean(Settings.MAIN.checkBox44.CheckState) + Environment.NewLine +
                    "TOP_ONE=" + Settings.MAIN.textBox232.Text

                    );
                    file.Close();
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"Error in writer manager {EX.ToString()}", UTILS.LOG_TYPE.Fatal); }
            return true;
        }
    }
}
