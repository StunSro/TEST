using System.Windows.Forms;
using System;
using SR_PROXY.ENGINES;
using SR_PROXY.CORE;
using System.Data;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using SR_CLIENT;
using System.Globalization;
using SR_PROXY.SQL;
using System.Threading.Tasks;
using System.Timers;
using Framework;
using SR_PROXY.FORMS;
using System.Net;
using SR_PROXY.SECURITYOBJECTS;
using SR_PROXY.MODEL;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Collections;
using System.Runtime.CompilerServices;
using VMProtect;
using static SR_PROXY.ENGINES.UTILS;
using System.Threading;
using SR_PROXY.GameSpawn;
using System.Collections.Concurrent;
using SR_PROXY.MSSQL_SERVER;

namespace SR_PROXY
{
    public partial class MAIN : Form
    {
        Stopwatch sw = Stopwatch.StartNew();
        //Declaring varibles
        public static System.Timers.Timer 功勋计时器,LingeringCons_Timer, BotsEngineActions_Timer, AutoEvents_Timer, AutoNotice_Timer, AutoDBbackup_Timer, AutoScheduledNotice_Timer, License_Timer, Informer_Timer, RankTimer, FilterCommands, RegisterEventTimer, EventsTimer, ResetTimer,MinionsTimer,HonorTimer;
        public PerformanceCounter BandwidthCounter, MemoryCounter, CPUCounter;
        public static int PROCESSOR_COUNT;
        public static bool SQL_STATUS;
        public static string LIC_HWID;
        public DateTime START_TIME;
        DataTable dt;
        DataView dv;
        #region BotsEngine extension shit
        //a list of all the online botengine in the server.
        public static List<string> online_botengines = new List<string>();
        public static Dictionary<string, int> random_actions = new Dictionary<string, int>();
        public static List<Tuple<string, string, DateTime>> actions_queue = new List<Tuple<string, string, DateTime>>();
        public static Dictionary<string, string> wait_for_relogin = new Dictionary<string, string>();
        //txt file lists
        public static List<Tuple<string, int, int>> global_texts;
        public static List<Tuple<string, int, int, bool>> pt_match_list;
        public static List<string> used_pt_texts = new List<string>();
        public static List<string> used_global_texts = new List<string>();
        public static List<Tuple<string, int, int, ulong, int, int>> stall_items;
        #endregion
        
        public MAIN()
        {
          //  this.Hide();
          //  DialogResult dr = new Login().ShowDialog();
           // if (dr != DialogResult.OK)
           // {
           //     Application.Exit();
           // }
            InitializeComponent();

            //Assigning variables
            Settings.MAIN = this;
            CheckForIllegalCrossThreadCalls = false;
            //Our application initial start time
            START_TIME = Process.GetCurrentProcess().StartTime;
            //Initial state for our sql server, false by deafult
            SQL_STATUS = false;
            //Datatable and DataView for filtering our connections list view results...
            dt = new DataTable();
            dv = new DataView(dt);
            //Adding the listview column headers into our data table
            dt.Columns.Add("CNTER");
            dt.Columns.Add("SOCKET_IP");
            dt.Columns.Add("RAW_MODULE");
            dt.Columns.Add("MODULE_TYPE");
            dt.Columns.Add("CHARNAME16");
            dt.Columns.Add("SHRADNAME");
            dt.Columns.Add("PPS");
            dt.Columns.Add("BPS");
            dt.Columns.Add("TOTALPACKETS");
            dt.Columns.Add("TOTALBYTES");
            dt.Columns.Add("HWID");
            dt.Columns.Add("TOKEN_ID");
            //Performance counters...
            CPUCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
            MemoryCounter = new PerformanceCounter("Process", "Working Set - Private", Process.GetCurrentProcess().ProcessName);
            PROCESSOR_COUNT = Environment.ProcessorCount;
            LIC_HWID = string.Empty;
        }
        [VMProtect.BeginVirtualizationLockByKey]

        private async void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            // Confirm user wants to close
            switch (MessageBox.Show(this, "Are you sure you want to close?", "Closing", MessageBoxButtons.YesNo))
            {
                case DialogResult.No:
                    e.Cancel = true;
                    break;
                default:
                    break;
            }
        }
        private async void MAIN_ShownAsync(object sender, EventArgs e)
        {

            try
            {
         //       label232.Text = Settings.sd.Expires.ToString("yyyy-MM-dd");
                READER.READ_REQ_INIT();
                QUERIES.SetConnectiontType();
               // if (Settings.BIND_IP != "222.255.214.35")
                //    Environment.Exit(0);
                ASYNC_SERVER.InitializeSingleEngine(Settings.BIND_IP, Settings.PUBLIC_GW_PORT, Settings.PUBLIC_DW_PORT, Settings.PUBLIC_AG_PORT);
                if(checkBox5.Checked || checkBox6.Checked || checkBox27.Checked)
                    ASYNC_SERVER.InitializeSecondEngine(Settings.BIND_IP, Settings.PUBLIC_AG2_PORT, Settings.PUBLIC_AG3_PORT, Settings.PUBLIC_AG4_PORT);
                READER.READ_FEATURES();
                READER.LoadMOpcodes();
                READER.LoadChatFilter();
                READER.LoadBlockedSkillsIDs();
                READER.LoadBlockedFWSkillsIDs();
                READER.LoadBlockedCTFSkillsIDs();
                READER.LoadBlockedBASkillsIDs();
                READER.LoadBlockedJobSkillsIDs();
                READER.LoadNetCafeIPs();
                READER.LoadRegionsRestrctions();
                READER.LoadTeleportToTown();
                READER.LoadDisableZerkRegions();
                READER.LoadDisableChatRegions();
                READER.LoadDisablePartyRegions();
                READER.LoadDisableTraceRegions();
                READER.LoadDisableInviteFriendsRegions();
                READER.LoadDisableSkillsRegions();
                READER.LoadDisableItemsRegions();
                await GetWinnerNumbers();
                if (await QUERIES.SQL_CONNECTIVITY(QUERIES.connectionstring))//check if a valid sql con can be acquired
                {
                    LoadMssqlConfigAsync();

                    await QUERIES.SQL_SVR_BIT();
                    await SG_QUERIES.PreloadFilterDatabaseAsync();

                }
                else {
                    UTILS.WriteLine("An error occured while connecting to database！", UTILS.LOG_TYPE.Fatal);
                    return;
                }
                UTILS.BLOCK_IP("180.101.49.12");
                TIMERS_INIT();
                sw.Stop();
                UTILS.WriteLine($"All settings was loaded in: [{sw.ElapsedMilliseconds} ms]", UTILS.LOG_TYPE.Notify);
            }
            catch (Exception EX) { UTILS.WriteLine($"【An error occured in settings loading】 {EX.ToString()}", UTILS.LOG_TYPE.Fatal); UTILS.ExportLog("MAIN_Shown", EX); }
        }
        /// <summary>
        /// </summary>
        [VMProtect.BeginVirtualizationLockByKey]
        private async void LoadMssqlConfigAsync() {

            if (!String.Empty.Equals(textBox23.Text) && File.Exists(textBox23.Text))
            {
                try {
                    ReadShardInfo(textBox23.Text);
                    GameInfo.InitializeLists();
                    QUERIES.LoadRefSkill();
                    QUERIES.LoadItems();
                    QUERIES.LoadMobs();
                }
                catch (Exception Ex)
                {
                }
            }
            else {
                UTILS.WriteLine("Please load the server configuration file or the game will be disconnected！", UTILS.LOG_TYPE.Fatal);
            }
            LoadNoticeList();
            LoadMonsterKillRewardList();
            LoadRestrectedRegionsList();
            LoadTelportToTownRegionsList();
            Loaddisablezerkregions();
            Loaddisablechatregions();
            Loaddisablepartyregions();
            Loaddisableinvitefriendsregions();
            Loaddisabletraceregions();
            Loaddisableskillsregions();
            Loaddisableitemsregions();

        }
       
 
        private async void LoadNoticeList() {
            List<NoticeModel> Notices = (List<NoticeModel>)await QUERIES.Get_Notice_List();
            NoticeListView.BeginUpdate();
            NoticeListView.Items.Clear();
            foreach (var item in Notices)
            {
                string[] Temp = new string[NoticeListView.Columns.Count];
                Temp[0] = item.ID.ToString();
                Temp[1] = item.StartDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                Temp[2] = item.EndDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                Temp[3] = item.Content;
                Temp[4] = item.Color;
                ListViewItem lvi = new ListViewItem(Temp);
                NoticeListView.Items.Add(lvi);
            }
            NoticeListView.EndUpdate();

        }
        private async void LoadRestrectedRegionsList()
        {
            listView2.BeginUpdate();
            listView2.Items.Clear();
            int i = 1;
            foreach (var item in Region_Restrection)
            {
                string[] Temp = new string[listView2.Columns.Count];
                Temp[0] = item.Key.ToString();
                ListViewItem lvi = new ListViewItem(Temp);
                listView2.Items.Add(lvi);
                i++;
            }
            listView2.EndUpdate();
        }

        private async void LoadTelportToTownRegionsList()
        {
            listView3.BeginUpdate();
            listView3.Items.Clear();
            int i = 1;
            foreach (var item in Teleport_To_Town)
            {
                string[] Temp = new string[listView3.Columns.Count];
                Temp[0] = item.ToString();
                ListViewItem lvi = new ListViewItem(Temp);
                listView3.Items.Add(lvi);
                i++;
            }
            listView3.EndUpdate();
        }

        private async void Loaddisablezerkregions()
        {
            listView4.BeginUpdate();
            listView4.Items.Clear();
            int i = 1;
            foreach (var item in DISABLE_ZERK_REGIONS)
            {
                string[] Temp = new string[listView4.Columns.Count];
                Temp[0] = item.ToString();
                ListViewItem lvi = new ListViewItem(Temp);
                listView4.Items.Add(lvi);
                i++;
            }
            listView4.EndUpdate();
        }

        private async void Loaddisablechatregions()
        {
            listView10.BeginUpdate();
            listView10.Items.Clear();
            int i = 1;
            foreach (var item in DISABLE_CHAT_REGIONS)
            {
                string[] Temp = new string[listView10.Columns.Count];
                Temp[0] = item.ToString();
                ListViewItem lvi = new ListViewItem(Temp);
                listView10.Items.Add(lvi);
                i++;
            }
            listView10.EndUpdate();
        }

        private async void Loaddisablepartyregions()
        {
            listView8.BeginUpdate();
            listView8.Items.Clear();
            int i = 1;
            foreach (var item in DISABLE_PARTY_REGIONS)
            {
                string[] Temp = new string[listView8.Columns.Count];
                Temp[0] = item.ToString();
                ListViewItem lvi = new ListViewItem(Temp);
                listView8.Items.Add(lvi);
                i++;
            }
            listView8.EndUpdate();
        }

        private async void Loaddisabletraceregions()
        {
            listView9.BeginUpdate();
            listView9.Items.Clear();
            int i = 1;
            foreach (var item in DISABLE_TRACE_REGIONS)
            {
                string[] Temp = new string[listView9.Columns.Count];
                Temp[0] = item.ToString();
                ListViewItem lvi = new ListViewItem(Temp);
                listView9.Items.Add(lvi);
                i++;
            }
            listView9.EndUpdate();
        }

        private async void Loaddisableinvitefriendsregions()
        {
            listView11.BeginUpdate();
            listView11.Items.Clear();
            int i = 1;
            foreach (var item in DISABLE_INVITEFRIENDS_REGIONS)
            {
                string[] Temp = new string[listView11.Columns.Count];
                Temp[0] = item.ToString();
                ListViewItem lvi = new ListViewItem(Temp);
                listView11.Items.Add(lvi);
                i++;
            }
            listView11.EndUpdate();
        }

        private async void Loaddisableitemsregions()
        {
            listView7.BeginUpdate();
            listView7.Items.Clear();
            foreach (var item in DISABLE_ITEMS_REGIONS)
            {
                foreach(var x in DISABLE_ITEMS_REGIONS[item.Key])
                {
                    string[] Temp = new string[listView7.Columns.Count];
                    Temp[0] = item.Key.ToString();
                    Temp[1] = x.ToString();
                    ListViewItem lvi = new ListViewItem(Temp);
                    listView7.Items.Add(lvi);
                }
            }
            listView7.EndUpdate();
        }

        private async void Loaddisableskillsregions()
        {
            listView6.BeginUpdate();
            listView6.Items.Clear();
            foreach (var item in DISABLE_SKILLS_REGIONS)
            {
                foreach (var x in DISABLE_SKILLS_REGIONS[item.Key])
                {
                    string[] Temp = new string[listView6.Columns.Count];
                    Temp[0] = item.Key.ToString();
                    Temp[1] = x.ToString();
                    ListViewItem lvi = new ListViewItem(Temp);
                    listView6.Items.Add(lvi);
                }
            }
            listView6.EndUpdate();
        }

        private async void LoadMonsterKillRewardList()
        {
            List<MonsterKillRewardModel> MonsterKillRewards = (List<MonsterKillRewardModel>)await QUERIES.Get_MonsterKillReward_List();
            MonsterKillRewardListView.BeginUpdate();
            MonsterKillRewardListView.Items.Clear();
            foreach (var item in MonsterKillRewards)
            {
                string[] Temp = new string[MonsterKillRewardListView.Columns.Count];
                Temp[0] = item.Service.ToString();
                Temp[1] = item.ID.ToString();
                Temp[2] = item.Name;
                Temp[3] = item.SilkOwnReward.ToString();
                Temp[4] = item.SLBReward.ToString();
                Temp[5] = (item.RewardProbability / 100f) +"%";
                Temp[6] = item.NoticeType.ToString();
                Temp[7] = item.NoticeMessage;
                ListViewItem lvi = new ListViewItem(Temp);
                MonsterKillRewardListView.Items.Add(lvi);
            }
            MonsterKillRewardListView.EndUpdate();

        }

        #region Diagnostics Tab Related
        private void LLDiag_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try { DiagLog.Clear(); } catch { }
        }
        private async void button22_Click(object sender, EventArgs e)
        {
            await QUERIES.SQL_SVR_BIT();
            await SG_QUERIES.PreloadFilterDatabaseAsync();

        }
        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                //gw traffc dump
                UTILS.DUMP_MODULE_LATEST_TRAFFIC_DIAG(UTILS.GW_ALL_LATEST_TRAFFIC, false);
            }
            catch { }
        }
        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                //ag traffc dump
                UTILS.DUMP_MODULE_LATEST_TRAFFIC_DIAG(UTILS.AG_ALL_LATEST_TRAFFIC, false);
            }
            catch { }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                int start = 0;
                int end = DiagLog.Text.LastIndexOf(textBox98.Text);

                DiagLog.SelectAll();

                while (start < end)
                {
                    DiagLog.Find(textBox98.Text, start, DiagLog.TextLength, RichTextBoxFinds.MatchCase);

                    DiagLog.SelectionBackColor = Color.Yellow;

                    start = DiagLog.Text.IndexOf(textBox98.Text, start) + 1;
                }
            }
            catch { }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                string filename = "";
                SaveFileDialog sfd = new SaveFileDialog();

                sfd.Title = "Save as a file";
                sfd.Filter = "Text File (.txt) | *.txt";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    filename = sfd.FileName.ToString();
                    if (filename != "")
                    {
                        using (StreamWriter sw = new StreamWriter(filename))
                        {
                            sw.WriteLineAsync(DiagLog.Text);
                            sw.Close();
                        }
                    }
                }
            }
            catch { }
        }
        private void button23_Click(object sender, EventArgs e)
        {
            try
            {
                Settings.MAIN.DiagLog.Clear();
                UTILS.WriteLine($"Raw disposed gw sessions count:{ASYNC_SERVER.DISPOSED_GW_SESSIONS.Count}, Valid disposed gw sessions count:{ASYNC_SERVER.DISPOSED_GW_SESSIONS.Where(x => x.Value != null).Count()}");
                foreach (GATEWAY_MODULE DISPOSED_GATEWAY_SESSION in ASYNC_SERVER.DISPOSED_GW_SESSIONS.Values)
                {
                    if (DISPOSED_GATEWAY_SESSION != null && DISPOSED_GATEWAY_SESSION.TOKEN_ID != 0)
                    {
                        Settings.MAIN.DiagLog.AppendText($"TokenID:{DISPOSED_GATEWAY_SESSION.TOKEN_ID}, [{DISPOSED_GATEWAY_SESSION.REDIR_IP}]" + Environment.NewLine);
                    }
                }
            }
            catch { UTILS.WriteLine("An error occured while dumping disposed gw sessions.", UTILS.LOG_TYPE.Warning); }
        }
        private void button24_Click(object sender, EventArgs e)
        {
            try
            {
                Settings.MAIN.DiagLog.Clear();
                List<GATEWAY_MODULE> temp_list = ASYNC_SERVER.DISPOSED_GW_SESSIONS.Values.Where(x => x != null && x.TOKEN_ID == uint.Parse(textBox98.Text)).ToList();
                if (temp_list != null && temp_list.Count > 0)
                    foreach (var session in temp_list)
                    {
                        Settings.MAIN.DiagLog.AppendText($"TokenID:{session.TOKEN_ID}, IP :[{session.REDIR_IP}]" + Environment.NewLine);
                    }
            }
            catch { }
        }
        #endregion

        #region Clientless Tab Related
        private async void button19_ClickAsync(object sender, EventArgs e)
        {
            try
            {

                await Task.Run(() => { new SR_MODULE.SR_MODULE().Start(Settings.MAIN.textBox156.Text, int.Parse(textBox155.Text)); });
                Settings.MAIN.richTextBox2.Text = "Operation started...\n";

            }
            catch (Exception ex) { Settings.MAIN.richTextBox2.Text = ex.ToString(); }
        }
        private void button20_Click(object sender, EventArgs e)
        {
            try
            {
                SR_MODULE.SR_MODULE.DC_SR_CLIENT_SOCKET();
                Settings.MAIN.richTextBox2.Text = "Operation Aborted !!!";
            }
            catch (Exception ex) { Settings.MAIN.richTextBox2.Text = ex.ToString(); }
        }



        #endregion

        #region Connection Manager Tab Related
        private void textBox99_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboBox1.SelectedItem == null)
                    comboBox1.SelectedItem = comboBox1.Items[0];//Select the first item in the combo box in none selected

                dv.RowFilter = string.Format("{1} LIKE '%{0}%'", textBox99.Text, comboBox1.SelectedItem);
                //filtering results
                dt.Rows.Clear();
                foreach (var player in ASYNC_SERVER.GW_CONS)
                    dt.Rows.Add(
                        "N/A",
                        player.Value.SOCKET_IP, 
                        player.Value.MODULE_TYPE.ToString(),
                        //player.Value.PROXY_SOCKET.RemoteEndPoint.ToString(),
                        "N/A",
                        "N/A",
                        "N/A", 
                        player.Value.PPS.ToString(),
                        player.Value.BPS.ToString(), 
                        player.Value.TOT_PACKET_CNT.ToString(), 
                        player.Value.TOT_BYTES_CNT.ToString(),
                        "N/A",
                        player.Value.TOKEN_ID.ToString());
                foreach (var player in ASYNC_SERVER.AG_CONS)
                    dt.Rows.Add(
                        "N/A", 
                        player.Value.SOCKET_IP,
                        player.Value.MODULE_TYPE.ToString(),
                         //player.Value.PROXY_SOCKET.RemoteEndPoint.ToString(),
                        "Connected",
                        player.Value.CHARNAME16,
                        player.Value.ShardID + "|" + (Settings.ShardInfos.ContainsKey(player.Value.ShardID) ? Settings.ShardInfos[player.Value.ShardID].Name:"异常"),
                        player.Value.PPS.ToString(), 
                        player.Value.BPS.ToString(),
                        player.Value.TOT_PACKET_CNT.ToString(),
                        player.Value.TOT_BYTES_CNT.ToString(),
                        player.Value.CORRESPONDING_GW_SESSION.HWID,
                        player.Value.TOKEN_ID.ToString());

                //clearing the connections list view
                CM_LW.Items.Clear();

                foreach (DataRow row in dv.ToTable().Rows)
                    CM_LW.Items.Add(new ListViewItem(new string[] { row[0].ToString(), row[1].ToString(), row[2].ToString(), row[3].ToString(), row[4].ToString(), row[5].ToString(), row[6].ToString(), row[7].ToString(), row[8].ToString(), row[9].ToString(), row[10].ToString(), row[11].ToString() }));
            }
            catch(Exception Ex) { UTILS.WriteLine($"搜索失败:{Ex.ToString()}", UTILS.LOG_TYPE.Warning); }
        }
        //REFRESH CONS BUTTON
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                Settings.MAIN.button5.Enabled = false;
                if (comboBox1.SelectedItem == null)
                    comboBox1.SelectedItem = comboBox1.Items[0];//Select the first item in the combo box in none selected
                CM_LW.Items.Clear();
                int COUNTER = 1;
                foreach (var player in ASYNC_SERVER.GW_CONS) {
                    try
                    {

                        CM_LW.Items.Add(
                        new ListViewItem(new string[] {
                            COUNTER++.ToString(),
                            player.Value.SOCKET_IP,
                            player.Value.MODULE_TYPE!=null?player.Value.MODULE_TYPE.ToString():"GS异常",
                            //player.Value.PROXY_SOCKET!=null&& player.Value.PROXY_SOCKET.Connected?player.Value.PROXY_SOCKET.RemoteEndPoint.ToString():"失败",
                            "N/A",
                            "N/A",
                            "N/A",
                            player.Value.PPS.ToString(),
                            player.Value.BPS.ToString(),
                            player.Value.TOT_PACKET_CNT.ToString(),
                            player.Value.TOT_BYTES_CNT.ToString(),
                            "N/A",
                            player.Value.TOKEN_ID.ToString() }));
                    }
                    catch (Exception Ex)
                    {
                        ASYNC_SERVER.GW_CONS.TryRemove(player.Key, out GATEWAY_MODULE CURRENT_DW_SESSION);
                    }
                }
                foreach (var player in ASYNC_SERVER.AG_CONS) {
                    if (player.Value == null) continue;
                    try
                    {
                        CM_LW.Items.Add(
                            new ListViewItem(new string[] {
                            COUNTER++.ToString(),
                            player.Value.SOCKET_IP,
                            player.Value.MODULE_TYPE!=null?player.Value.MODULE_TYPE.ToString():"AS abnormal",
                            //player.Value.PROXY_SOCKET!=null&& player.Value.PROXY_SOCKET.Connected?player.Value.PROXY_SOCKET.RemoteEndPoint.ToString():"失败",
                            "Connected",
                            player.Value.CHARNAME16,
                            player.Value.ShardID+"|"+(Settings.ShardInfos.ContainsKey(player.Value.ShardID)?Settings.ShardInfos[player.Value.ShardID].Name:"Abnormal"),
                            player.Value.PPS.ToString(),
                            player.Value.BPS.ToString(),
                            player.Value.TOT_PACKET_CNT.ToString(),
                            player.Value.TOT_BYTES_CNT.ToString(),
                            player.Value.CORRESPONDING_GW_SESSION.HWID,
                            player.Value.TOKEN_ID.ToString() }));
                    }
                    catch (Exception Ex) {
                        ASYNC_SERVER.AG_CONS.TryRemove(player.Key, out AGENT_MODULE CURRENT_DW_SESSION);
                    }
                }
                sw.Stop();
                UTILS.WriteLine($"Total [{ASYNC_SERVER.GW_CONS.Count() + ASYNC_SERVER.AG_CONS.Count()}] Refresh completed in: [{sw.ElapsedMilliseconds} ms]", UTILS.LOG_TYPE.Notify);
            }
            catch (Exception EX) { UTILS.WriteLine($"Refresh connections has been failed. {EX.ToString()}", UTILS.LOG_TYPE.Warning); }
            finally { Settings.MAIN.button5.Enabled = true; }
        }
        #endregion

        #region Main Buttons Related

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                DETAILS DTS = new DETAILS();
                DTS.Show();
                DTS.DetailsLog.AppendText("You can add a chat filter words at FILTER_KEYWORDS.txt , one word by line..");
            }
            catch { }
        }
        private void linkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                DETAILS DTS = new DETAILS();
                DTS.Show();
                DTS.DetailsLog.AppendText("The packet processor system will allow you to restrict a broad spectrum of packets, in case you get attacked by an unknown exploit and in case you have failed to capture the latest packet dump in the diagnostics sessions\n\nCPU usage may increase while using this feature\n\nSpecify your broad spectrum packets in SR_Proxy directory - packet_processor.ini\n\nUsing this system should be as a last resort.");
            }
            catch { }
        }
        private void linkLabel11_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                label113.Text = $"{Math.Round(CPUCounter.NextValue() / PROCESSOR_COUNT, 2)}% - { PROCESSOR_COUNT} Cores";
                label111.Text = $"{Math.Round(MemoryCounter.NextValue() / 1024 / 1024), 2} MB";
                label109.Text = $"{Process.GetCurrentProcess().Threads.Count}";
                label229.Text = $"{(DateTime.Now - START_TIME).ToString(@"dd\.hh\:mm\:ss")}";
                label230.Text = $"{UTILS.COUNT_SPAWNED_CHARS()}";
                label180.Text = $"{UTILS.CalculateTotalNetworkUsage() / 1024} MB";
            }
            catch { }
        }

        private void linkLabel17_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            CM_LW.Items.Clear();
        }

        private void copyMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(listView1.SelectedItems[0].SubItems[2].Text);
        }
        private void dumpParamatersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string SockIP = CM_LW.SelectedItems[0].SubItems[1].Text;
                string MODULE_TYPE = CM_LW.SelectedItems[0].SubItems[2].Text;
                string CHARNAME16 = CM_LW.SelectedItems[0].SubItems[4].Text;

                switch (MODULE_TYPE)
                {
                    case "GatewayServer":
                        GATEWAY_MODULE GW_SESSION = ASYNC_SERVER.GW_CONS.Values.Where(p => p.SOCKET_IP == SockIP).FirstOrDefault();
                        if (GW_SESSION != null)
                        {
                            //obtaining our class fields
                            var fieldValues = GW_SESSION.GetType().GetFields().Select(field => field.GetValue(GW_SESSION)).Where(x => x != null).ToList();

                            DETAILS DTS = new DETAILS();
                            DTS.Show();

                            int ctr = 0;
                            foreach (var field in fieldValues)
                            {
                                ctr++;
                                DTS.DetailsLog.AppendText(ctr + " " + field.GetType().Name + ": " + field.ToString() + "." + Environment.NewLine);
                            }
                        }
                        break;
                    case "AgentServer":
                        AGENT_MODULE AG_SESSION = ASYNC_SERVER.AG_CONS.Values.Where(x => x.CHARNAME16 == CHARNAME16).FirstOrDefault();
                        if (AG_SESSION != null)
                        {
                            //obtaining our class fields
                            var fieldValues = AG_SESSION.GetType().GetFields().Select(field => field.GetValue(AG_SESSION)).Where(x => x != null).ToList();

                            DETAILS DTS = new DETAILS();
                            DTS.Show();

                            int ctr = 0;
                            foreach (var field in fieldValues)
                            {
                                ctr++;
                                DTS.DetailsLog.AppendText(ctr + " " + field.GetType().Name + ": " + field.ToString() + "." + Environment.NewLine);
                            }
                        }
                        break;
                }
            }
            catch { UTILS.WriteLine("An error occured while dumping user info", UTILS.LOG_TYPE.Warning); }
        }

        private async void button17_Click(object sender, EventArgs e)
        {
            if (await QUERIES.SQL_CONNECTIVITY(QUERIES.connectionstring))
            {
                label193.Text = "OK";
            }
            else
            {
                label193.Text = "Error";
            }
            await Task.Delay(5000);
            label193.Text = "Unknown";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            try
            {
                listView1.Items.Clear();
            }
            catch { }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            try
            {
                UTILS.SEND_NOTICE_TO_ALL(textBox101.Text);
                MessageBox.Show("Your message has been sent to the server succesfully.");
            }
            catch { }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Application.Restart();
                Environment.Exit(0);
            }
            catch { }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Environment.Exit(1);
            }
            catch { }
        }
        private void button21_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(@"" + Environment.CurrentDirectory + "");
            }
            catch { }
        }
        private void button25_Click(object sender, EventArgs e)
        {
            try
            {
                INDV_BAN IB = new INDV_BAN();
                IB.Show();
            }
            catch { }
        }
        #endregion

        #region Context Menu Strip
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (CM_LW.SelectedItems.Count <= 0)
                {
                    e.Cancel = true; return;
                }
                string SockIP = CM_LW.SelectedItems[0].SubItems[1].Text;
                string MODULE_TYPE = CM_LW.SelectedItems[0].SubItems[2].Text;
                string CHARNAME16 = CM_LW.SelectedItems[0].SubItems[4].Text;
            }
            catch { }
        }
        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string SockIP = CM_LW.SelectedItems[0].SubItems[1].Text;
                string MODULE_TYPE = CM_LW.SelectedItems[0].SubItems[2].Text;
                string CHARNAME16 = CM_LW.SelectedItems[0].SubItems[4].Text;

                switch (MODULE_TYPE)
                {
                    case "GatewayServer":
                        Socket GW_MatchingClientSocket = ASYNC_SERVER.GW_CONS.Where(p => p.Value.SOCKET_IP == SockIP).FirstOrDefault().Key;
                        if (GW_MatchingClientSocket != null)
                        {
                            Socket GW_MatchingProxySocket = ASYNC_SERVER.GW_CONS[GW_MatchingClientSocket].PROXY_SOCKET;
                            ASYNC_SERVER.DISCONNECT(GW_MatchingClientSocket, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                        }
                        break;
                    case "AgentServer":
                        Socket AG_MatchingClientSocket = ASYNC_SERVER.AG_CONS.Where(p => p.Value.SOCKET_IP == SockIP).FirstOrDefault().Key;
                        if (AG_MatchingClientSocket != null)
                        {
                            Socket AG_MatchingProxySocket = ASYNC_SERVER.AG_CONS[AG_MatchingClientSocket].PROXY_SOCKET;
                            ASYNC_SERVER.DISCONNECT(AG_MatchingClientSocket, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                        }
                        break;
                }
                MessageBox.Show("Selected client DC request has been sent.", "Operation Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { }
        }
        private void banIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string SockIP = CM_LW.SelectedItems[0].SubItems[1].Text;
                UTILS.BLOCK_IP(SockIP);
            }
            catch { }
        }
        private void SOCKET_IPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(CM_LW.SelectedItems[0].SubItems[1].Text);
            }
            catch { }
        }
        private void MODULE_TYPEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try { Clipboard.SetText(CM_LW.SelectedItems[0].SubItems[2].Text); } catch { }
        }
        private void CHARNAME16ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try { Clipboard.SetText(CM_LW.SelectedItems[0].SubItems[4].Text); } catch { }
        }
        private void gatewayServerDumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                tabControl1.SelectedTab = tabPage8;
                string SockIP = CM_LW.SelectedItems[0].SubItems[1].Text;
                Socket CURRENT_GW_SOCKET = ASYNC_SERVER.GW_CONS.Where(p => p.Value.SOCKET_IP == SockIP).FirstOrDefault().Key;
                UTILS.DUMP_MODULE_LATEST_TRAFFIC_DIAG(ASYNC_SERVER.GW_CONS[CURRENT_GW_SOCKET].GW_TRAFFIC, false);
            }
            catch { }
        }
        private void agentServerDumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                tabControl1.SelectedTab = tabPage8;
                string SockIP = CM_LW.SelectedItems[0].SubItems[1].Text;
                Socket CURRENT_AG_SOCKET = ASYNC_SERVER.AG_CONS.Where(p => p.Value.SOCKET_IP == SockIP).FirstOrDefault().Key;
                UTILS.DUMP_MODULE_LATEST_TRAFFIC_DIAG(ASYNC_SERVER.AG_CONS[CURRENT_AG_SOCKET].AG_TRAFFIC, false);
            }
            catch { }
        }
        private void rewardMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                REWARD_FORM RF = new REWARD_FORM();
                RF.ShowDialog();
            }
            catch { }
        }
        private void sendIndToolStripMenuItem_Click(object sender, EventArgs e)
        {
            INDV_NOTICE IN = new INDV_NOTICE();
            IN.Show();
        }
        #endregion

        #region CUSTOM UNIQUE_REWARD AND UNIQUE_EVENT UI



        private void addToListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    Control sourceControl = owner.SourceControl;
                    //UTILS.WriteLine("", sourceControl.Name.ToString());
                    if (sourceControl.Name.ToLower().Contains("gridview1"))
                    {
                        CUSTOM_UNIQUE_REWARD CUR = new CUSTOM_UNIQUE_REWARD();
                        CUR.Show();
                    }
                    else if (sourceControl.Name.ToLower().Contains("gridview2"))
                    {
                        CUSTOM_UNIQUE_EVENT CUE = new CUSTOM_UNIQUE_EVENT();
                        CUE.Show();
                    }
                }
            }
        }

     
        #endregion

        #region TAB Timers inits and callbacks
        void TIMERS_INIT()
        {
            if (Settings.START_EVENT_EVERY_MINUTES != 0)
            {
                if (AutoEvents_Timer != null)
                    if (AutoEvents_Timer.Enabled)
                        AutoEvents_Timer.Dispose();
                AutoEvents_Timer = new System.Timers.Timer(Settings.START_EVENT_EVERY_MINUTES * 60000);//60sec
                AutoEvents_Timer.Elapsed += AUTO_EVENTS_TIMER;
                AutoEvents_Timer.AutoReset = true;
                AutoEvents_Timer.Enabled = true;
            }
            if (Settings.SCHEDULED_NOTICE_EVERY >0)
            {
                if (AutoScheduledNotice_Timer != null)
                    if (AutoScheduledNotice_Timer.Enabled)
                        AutoScheduledNotice_Timer.Dispose();
                AutoScheduledNotice_Timer = new System.Timers.Timer(Settings.SCHEDULED_NOTICE_EVERY * 1000);
                AutoScheduledNotice_Timer.Elapsed += SCHEDULED_NOTICE_TIMER;
                AutoScheduledNotice_Timer.AutoReset = true;
                AutoScheduledNotice_Timer.Enabled = true;
            }
            else
            {
                if (AutoScheduledNotice_Timer != null)
                    if (AutoScheduledNotice_Timer.Enabled)
                        AutoScheduledNotice_Timer.Enabled = false;
            }
            if (LingeringCons_Timer == null || !LingeringCons_Timer.Enabled)
            {
                LingeringCons_Timer = new System.Timers.Timer(900000);//15min
                LingeringCons_Timer.Elapsed += LINGERING_TIMER;
                LingeringCons_Timer.AutoReset = true;
                LingeringCons_Timer.Enabled = true;
            }
            //if (SQL_STATUS)
            //{
            //    if (AutoNotice_Timer != null)
            //        if (AutoNotice_Timer.Enabled)
            //            AutoNotice_Timer.Dispose();
            //    AutoNotice_Timer = new System.Timers.Timer(3000);
            //    AutoNotice_Timer.Elapsed += AUTO_NOTICE_TIMER;
            //    AutoNotice_Timer.AutoReset = true;
            //    AutoNotice_Timer.Enabled = true;
            //}
            if(Settings.MAIN.checkBox29.Checked)
            {
                if (HonorTimer != null)
                    if (HonorTimer.Enabled)
                        HonorTimer.Dispose();
                HonorTimer = new System.Timers.Timer(Convert.ToInt32(textBox5.Text) * 60000);
                HonorTimer.Elapsed += HonorTimer_Tick;
                HonorTimer.AutoReset = true;
                HonorTimer.Enabled = true;
            }
            

            if (ResetTimer != null)
                if (ResetTimer.Enabled)
                    ResetTimer.Dispose();
            ResetTimer = new System.Timers.Timer(24 * 60 * 60000);
            ResetTimer.Elapsed += ResetEvery24h;
            ResetTimer.AutoReset = true;
            ResetTimer.Enabled = true;

            if (RegisterEventTimer != null)
                if (RegisterEventTimer.Enabled)
                    RegisterEventTimer.Dispose();
            RegisterEventTimer = new System.Timers.Timer(1 * 60000);
            RegisterEventTimer.Elapsed += RegisterEvent;
            RegisterEventTimer.AutoReset = true;
            RegisterEventTimer.Enabled = true;

            if (Settings.MAIN.checkBox30.Checked)
            {
                if (RankTimer != null)
                    if (RankTimer.Enabled)
                        RankTimer.Dispose();
                RankTimer = new System.Timers.Timer(Convert.ToInt32(Settings.MAIN.textBox28.Text) * 60000);
                RankTimer.Elapsed += UpdateRank;
                RankTimer.AutoReset = true;
                RankTimer.Enabled = true;
            }
            if (EventsTimer != null)
                if (EventsTimer.Enabled)
                    EventsTimer.Dispose();
            EventsTimer = new System.Timers.Timer(1 * 60000);
            EventsTimer.Elapsed += FetchEvents;
            EventsTimer.AutoReset = true;
            EventsTimer.Enabled = true;

            if (FilterCommands != null)
                if (FilterCommands.Enabled)
                    FilterCommands.Dispose();
            FilterCommands = new System.Timers.Timer(3000);
            FilterCommands.Elapsed += FilterCommandsTimer;
            FilterCommands.AutoReset = true;
            FilterCommands.Enabled = true;

        }

        private void MAIN_Load(object sender, EventArgs e)
        {
            //MessageBox.Show(UTILS.UserMd5("123456"));
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //base.OnFormClosing(e);

            //if (e.CloseReason == CloseReason.WindowsShutDown) return;

            // Confirm user wants to close
            switch (MessageBox.Show(this, "Are you sure you want to close?", "Closing", MessageBoxButtons.YesNo))
            {
                case DialogResult.No:
                    e.Cancel = true;
                    break;
                default:
                    Environment.Exit(1);
                    break;
            }
        }
        public static Image RotateImage(Image img, float a)
        {
            Bitmap b = new Bitmap(img.Width, img.Height);
            Graphics g = Graphics.FromImage(b);
            g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
            g.RotateTransform(a);
            g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, new Point(0, 0));
            g.Dispose();
            return b;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                label113.Text = $"{Math.Round(CPUCounter.NextValue() / PROCESSOR_COUNT, 2)}% - { PROCESSOR_COUNT} Cores";
                label111.Text = $"{Math.Round(MemoryCounter.NextValue() / 1024 / 1024),2} MB";
                label109.Text = $"{Process.GetCurrentProcess().Threads.Count}";
                label229.Text = $"{(DateTime.Now - START_TIME).ToString(@"dd\.hh\:mm\:ss")}";
                label230.Text = $"{UTILS.COUNT_SPAWNED_CHARS()}";
                label180.Text = $"{UTILS.CalculateTotalNetworkUsage() / 1024} MB";
            }
            catch { }
        }

        private void AddNoticeButton_Click(object sender, EventArgs e)
        {
            new NOTICE_INFO().ShowDialog();
            LoadNoticeList();
        }

        private void NoticeListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (NoticeListView.SelectedItems.Count == 0) return;
            NoticeModel Notice = new NoticeModel(
                int.Parse(NoticeListView.SelectedItems[0].SubItems[0].Text),
                Convert.ToDateTime(NoticeListView.SelectedItems[0].SubItems[1].Text),
                Convert.ToDateTime(NoticeListView.SelectedItems[0].SubItems[2].Text),
                NoticeListView.SelectedItems[0].SubItems[3].Text,
                NoticeListView.SelectedItems[0].SubItems[4].Text
                );
            new NOTICE_INFO(Notice).ShowDialog();
            LoadNoticeList();
        }

        private void AUTO_EVENTS_TIMER(object source, ElapsedEventArgs e)
        {
            
        }

        private void AddMonsterKillRewardButton_Click(object sender, EventArgs e)
        {
            new MonsterKillRewardForm().ShowDialog();
            LoadMonsterKillRewardList();
        }

        private void MonsterKillRewardListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (MonsterKillRewardListView.SelectedItems.Count == 0) return;
            var Item=MonsterKillRewardListView.SelectedItems[0];
            MonsterKillRewardModel MonsterKillReward = new MonsterKillRewardModel(
                int.Parse(Item.SubItems[0].Text),
                int.Parse(Item.SubItems[1].Text),
                Item.SubItems[2].Text,
                int.Parse(Item.SubItems[3].Text),
                int.Parse(Item.SubItems[4].Text),
                (int)(float.Parse(Item.SubItems[5].Text.Replace("%",""))*100),
                int.Parse(Item.SubItems[6].Text),
                Item.SubItems[7].Text
                );
            new MonsterKillRewardForm(MonsterKillReward).ShowDialog();
            LoadMonsterKillRewardList();
        }


        private void button26_Click(object sender, EventArgs e)
        {
          
        }

        private void CustomActionLsitView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
           
        }

        private void RunActionNameComboBox_MouseEnter(object sender, EventArgs e)
        {


         
        }

        private async void RunCustomActionBtn_Click(object sender, EventArgs e)
        {
           

        }

        private void button27_Click(object sender, EventArgs e)
        {
        }

        private void ActionLimitListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click_1(object sender, EventArgs e)
        {
        }
        private ListViewItemComparer lvwColumnSorter = new ListViewItemComparer();

        private void CM_LW_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Call the sort method to manually sort.
            CM_LW.Sort();
            foreach (ColumnHeader ch in CM_LW.Columns)
            {
                ch.ImageIndex = -1;
            }

            CM_LW.ListViewItemSorter = lvwColumnSorter;

            //if (listView1.SmallImageList == null)  
            //{  
            //    listView1.SmallImageList = imageList1;  
            //}  

            //listView1.Columns[e.Column].ImageIndex = 0;  

            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.  
                if (lvwColumnSorter.Order == System.Data.SqlClient.SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = System.Data.SqlClient.SortOrder.Descending;
                    //listView1.Columns[e.Column].ImageIndex = 1;  
                }
                else
                {
                    lvwColumnSorter.Order = System.Data.SqlClient.SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.  
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = System.Data.SqlClient.SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.  

            this.CM_LW.Sort();
            CM_LW.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            CM_LW.Update(); 
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox8.Text = "Bimbum";
            textBox117.Text = "64";
            switch (comboBox3.SelectedItem.ToString())
            {
                case "Custom Name Color":
                case "Custom Title Color":
                    textBox90.Text = "#ff00a2";
                    break;
                case "Custom Icon":
                    textBox90.Text = "1";
                    break;
                case "Custom Title":
                    textBox90.Text = "Insane";
                    break;
                case "Custom Name Rank":
                    textBox90.Text = "Warrior Of Courage";
                    break;
            }
        }
        private void button10_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "srNodeData.ini";
            dialog.Filter = "srNodeData.ini(srNodeData.ini)|srNodeData.ini";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var FileName = dialog.FileName;
                int ShardCount = InIHelper.ReadConfig<int>(FileName, "global", "count");
                for (int i = 0; i < ShardCount; i++)
                {
                    int ServiceType = InIHelper.ReadConfig<int>(FileName, "entry" + i, "service_type");
                    int Port = InIHelper.ReadConfig<int>(FileName, "entry" + i, "port");
                    switch (ServiceType) {
                        case 3:
                            textBox9.Text = Port.ToString();
                            break;
                        case 4:
                            textBox4.Text = Port.ToString();
                            break;
                        case 6:
                            textBox7.Text = Port.ToString();
                            break;
                    }
                }
                Save();
            }
        }


        private void OnClick(object sender, EventArgs e)
        {
            (sender as TextBox).Text = (sender as TextBox).Text == "True" ? "False" : "True";
        }

        private void numericUpDown19_ValueChanged(object sender, EventArgs e)
        {

        }
        public static List<string> BotsList = new List<string>();

        private async void button13_Click(object sender, EventArgs e)
        {
            if (MinionsTimer != null)
                if (MinionsTimer.Enabled)
                    MinionsTimer.Dispose();
            MinionsTimer = new System.Timers.Timer(5000);
            MinionsTimer.Elapsed += MinionsPing;
            MinionsTimer.AutoReset = true;
            MinionsTimer.Enabled = true;
            await Task.Factory.StartNew(async () =>
            {
                BotsList = await QUERIES.GET_BOTS_NAMES();
                if(BotsList.Count <=0)
                {
                    UTILS.WriteLine("Please add bots names at _MinionsNames at database!", UTILS.LOG_TYPE.Warning);
                    return;
                }    
                List<Dictionary<string, string>> UserList = await QUERIES.GET_USERS_LIST();
                if (UserList != null)
                {
                    //if count of required logins < our list count then create the difference
                    int BotsCount = (int)numericUpDown19.Value - UserList.Count;
                    for (int i = 0; i < BotsCount; i++)
                    {

                        string PassWord = UTILS.GetRandomString(9);
                        await QUERIES.Reg_User(UTILS.GetRandomString(9), PassWord, PassWord, "bimbum", BotsList[i]);
                    }
                    UserList = await QUERIES.GET_USERS_LIST();
                    var Temp = 0;
                    foreach (var item in UserList)
                    {
                        if (item["Role"].Contains(" ")) continue;
                        if (Temp >= numericUpDown19.Value) break;
                        Temp++;
                        //if (Temp > 0 && Temp % 10 == 0) {
                        //}
                        await Task.Delay(500);
                        if (Client.ClientList.Count > 2000)
                            return;
                        if (!Client.ClientList.ContainsKey(item["Username"]))
                            Client.ClientList.TryAdd(item["Username"], new Gateway(Settings.BIND_IP, Settings.PUBLIC_GW_PORT, item["Username"], item["Password"], item["Role"]));
                        string[] Temp2 = new string[listView5.Columns.Count];
                        Temp2[0] = item["Username"];
                        Temp2[1] = item["Password"];
                        Temp2[2] = item["Role"];
                        Temp2[3] = "Jangan";
                        Temp2[4] = "Connected";
                        ListViewItem lvi = new ListViewItem(Temp2);
                        listView5.Items.Add(lvi);
                    }
                }
                else
                {
                    UTILS.WriteLine($"The list of bots is empty.");
                }

            });
        }

        private async void button16_Click(object sender, EventArgs e)
        {
            await Task.Factory.StartNew(async() =>
            {
                foreach (var item in Client.ClientList)
                {
                    if (item.Value.m_Agent != null)
                    {
                        item.Value.m_Agent.CloseStall();
                        await Task.Delay(10);

                        int dstX = new Random(Guid.NewGuid().GetHashCode()).Next(6417, 6450);
                        int dstY = new Random(Guid.NewGuid().GetHashCode()).Next(1062, 1127);
                        item.Value.m_Agent.DoMovement(dstX, dstY);

                        await Task.Delay(10);

                    }
                    else
                    {
                        UTILS.WriteLine($"{item.Value._charname} : agent null ");
                    }
                }
            });
        }

        private async void button30_Click(object sender, EventArgs e)
        {
            await Task.Factory.StartNew(async () =>
            {
                foreach (var item in Client.ClientList)
                {
                    if (item.Value.m_Agent != null)
                    {
                        item.Value.m_Agent.OpenStall();
                        await Task.Delay(10);
                    }
                }
            });
        }

        private async void button15_Click(object sender, EventArgs e)
        {
            listView5.Items.Clear();
            MinionsTimer.Stop();
            Task.Factory.StartNew(() =>
            {
                Client.Close_All();
            });
        }


        private void numericUpDown21_ValueChanged(object sender, EventArgs e)
        {

        }

        private async void HonorTimer_Tick(object sender, EventArgs e)
        {
            HonorTimer.Stop();
            await QUERIES.UpdateHonorRanking();
            HonorTimer.Start();
        }

        private void button34_Click(object sender, EventArgs e)
        {
            UTILS.BAN_LIST.Clear();
            UTILS.CLEAR_BLOCK_IP();
            foreach (var a in ASYNC_SERVER.AG_CONS.Where(x => x.Value.TOT_PACKET_CNT == 0 || x.Value.TOT_BYTES_CNT == 0))
            {
                try
                {
                    ASYNC_SERVER.AG_CONS.TryRemove(a.Key, out AGENT_MODULE s);
                    ASYNC_SERVER.DISCONNECT(a.Key, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                }
                catch (Exception Ex)
                {
                }
            }
        }

        private async void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
           
                MessageBox.Show("hello xD");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void textBox177_TextChanged(object sender, EventArgs e)
        {

        }

        private void Debug_Click(object sender, EventArgs e)
        {
            try
            {
                string SockIP = Settings.MAIN.CM_LW.SelectedItems[0].SubItems[1].Text;
                var keysWithMatchingValues = ASYNC_SERVER.AG_CONS.Where(p => p.Value.SOCKET_IP == SockIP).Select(p => p.Value);
                foreach (var key in keysWithMatchingValues)//matching socket Key
                {
              //       UTILS.发送自定义消息(0x19A2, "hello world", key.防御通讯);
                    
                }
            }
            catch { }
        }

        private void label100_Click(object sender, EventArgs e)
        {

        }

        private void button29_Click(object sender, EventArgs e)
        {

        }

        private void RegUserCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void UpdateUserCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void groupBox11_Enter(object sender, EventArgs e)
        {

        }

        private void CM_LW_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button28_Click(object sender, EventArgs e)
        {

        }

        private void tabPage10_Click(object sender, EventArgs e)
        {

        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private async void RewardTimer_TickAsync(object sender, EventArgs e)
        {
            foreach (var key in ASYNC_SERVER.AG_CONS)//matching socket Key
            {
                if (key.Value.CHARNAME16 == null) continue;
                int Level = key.Value.CUR_LEVEL;
                //if (checkBox18.Checked && Level >= Convert.ToInt32(numericUpDown1.Value) && key.Value.StallStart > 0 && (Environment.TickCount - key.Value.StallStart) / 1000 > 3600)
                //{
                //    key.Value.StallStart = Environment.TickCount;
                //    if (Convert.ToInt32(numericUpDown4.Value) > 0)
                //    {
                //        key.Value.LiveGold(Convert.ToInt32(numericUpDown4.Value), true);
                //        UTILS.SendNotice(NoticeType.Orange, $"You have received [{Convert.ToInt32(numericUpDown4.Value)}] Silk for being stalling for an hour.", key.Value.CLIENT_SOCKET);
                //    }
                //    if (Convert.ToInt32(numericUpDown5.Value) > 0)
                //    {
                //        await QUERIES.GIVE_SILK(key.Value.UserName, Convert.ToInt32(numericUpDown5.Value));
                //        UTILS.SendNotice(NoticeType.Orange, $"You have received [{Convert.ToInt32(numericUpDown5.Value)}] Gold for being stalling for an hour.", key.Value.CLIENT_SOCKET);
                //    }
                //}
                //if (checkBox23.Checked && key.Value.TeamStart > 0 && (Environment.TickCount - key.Value.TeamStart) / 1000 > 3600 && key.Value.PartyID != 0)
                //{
                //    if (Level >= Convert.ToInt32(numericUpDown2.Value))
                //    {
                //        key.Value.TeamStart = Environment.TickCount;
                //        if (Convert.ToInt32(numericUpDown7.Value) > 0)
                //        {
                //            key.Value.LiveGold(Convert.ToInt32(numericUpDown7.Value), true);
                //            UTILS.SendNotice(NoticeType.Orange, $"You have received [{Convert.ToInt32(numericUpDown7.Value)}] Silk for uping party matching for an hour.", key.Value.CLIENT_SOCKET);
                //        }
                //        if (Convert.ToInt32(numericUpDown6.Value) > 0)
                //        {
                //            await QUERIES.GIVE_SILK(key.Value.UserName, Convert.ToInt32(numericUpDown6.Value));
                //            UTILS.SendNotice(NoticeType.Orange, $"You have received [{Convert.ToInt32(numericUpDown6.Value)}] Gold for uping party matching for an hour.", key.Value.CLIENT_SOCKET);
                //        }
                //    }
                //    key.Value.UpdateParty();
                //}
                RewardSilkHour.Clear();
                if (checkBox25.Checked && ((DateTime.Now - key.Value.LAST_PLAYER_SEEN).TotalSeconds > 3600) && Level >= Convert.ToInt32(numericUpDown3.Value))
                {
                    key.Value.LAST_PLAYER_SEEN = DateTime.Now;
                    if (RewardSilkHour.Contains(key.Value.CORRESPONDING_GW_SESSION.HWID))
                        continue;
                    else
                        RewardSilkHour.Add(key.Value.CORRESPONDING_GW_SESSION.HWID);
                    if (Convert.ToInt32(numericUpDown9.Value) > 0)
                    {
                        key.Value.LiveGold(Convert.ToInt32(numericUpDown9.Value), true);
                        UTILS.SendNotice(NoticeType.Orange, $"Chóc Mõng B¹n §· NhËn §­îc [{Convert.ToInt32(numericUpDown9.Value)}] Gold Khi §· Online 1/h.", key.Value.CLIENT_SOCKET);
                    }
                    if (Convert.ToInt32(numericUpDown8.Value) > 0)
                    {
                        await QUERIES.GIVE_SILK(key.Value.UserName, Convert.ToInt32(numericUpDown8.Value));
                        UTILS.SendNotice(NoticeType.Orange, $"Chóc Mõng B¹n §· NhËn §­îc [{Convert.ToInt32(numericUpDown8.Value)}] Silk Khi §· Online 1/h.", key.Value.CLIENT_SOCKET);
                    }

                }
            }
        }

        private void button31_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "srShard.ini";
            dialog.Filter = "srShard.ini(srShard.ini)|srShard.ini";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox23.Text= dialog.FileName;
            }
            
            try { ReadShardInfo(textBox23.Text);}
            catch(Exception Ex){
            }
        }
        private  void ReadShardInfo(string FileName) {
            int ShardCount = InIHelper.ReadConfig<int>(FileName, "global", "count");
            ShardListView.BeginUpdate();
            ShardListView.Items.Clear();
            Settings.ShardInfos.Clear();
            for (int i = 0; i < ShardCount; i++)
            {
                string Query = InIHelper.ReadConfig<string>(FileName, "entry" + i, "query");
                string DBName = Regex.Split(Query, "DATABASE=", RegexOptions.IgnoreCase)[1];
                int ShradID = InIHelper.ReadConfig<int>(FileName, "entry" + i, "id");
                Settings.ShardInfos[ShradID] = new ShardModel(
                    ShradID,
                    DBName,
                    InIHelper.ReadConfig<string>(FileName, "entry" + i, "name"),
                    InIHelper.ReadConfig<int>(FileName, "entry" + i, "capacity"));
                string[] Temp = new string[ShardListView.Columns.Count];
                Temp[0] = Settings.ShardInfos[ShradID].Id.ToString();
                Temp[1] = Settings.ShardInfos[ShradID].Name;
                Temp[2] = Settings.ShardInfos[ShradID].DBName;
                Temp[3] = Settings.ShardInfos[ShradID].Capacity.ToString();
                ListViewItem lvi = new ListViewItem(Temp);
                ShardListView.Items.Add(lvi);
                _UniquesLogParam xx = new _UniquesLogParam();
                xx.UniqueLogLine = 30;
                xx.UniqueLogPage = 1;
                UTILS.UniqueLogsParams.TryAdd(Settings.ShardInfos[ShradID].Id, xx);
            }
            ShardListView.EndUpdate();

        }

        private void button29_Click_1(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                DETAILS DTS = new DETAILS();
                DTS.Show();
                DTS.DetailsLog.AppendText("This function is made to add live things to characters\nFirst select the thing you want to add to the character then put charname.\n1-For title color put at data color code example : #FFC0CB.\n2-For custom name or custom name rank put a string example: Warrior Of Courage.\n3-For custom icon put the icon id example : 30.");
            }
            catch
            {

            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                DETAILS DTS = new DETAILS();
                DTS.Show();
                DTS.DetailsLog.AppendText("These regions will be disabled for both the new and old reverses\n*Default regions shown are the region which has worldid > 1");
            }
            catch
            {

            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                DETAILS DTS = new DETAILS();
                DTS.Show();
                DTS.DetailsLog.AppendText("When the player goes offline. if he was at one of these regions he will be teleported back to town.");
            }
            catch
            {

            }
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            UTILS.ListType = 1;
            new DISABLE_CUSTOM_REGION().ShowDialog();
            LoadRestrectedRegionsList();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            UTILS.ListType = 2;
            new DISABLE_CUSTOM_REGION().ShowDialog();
            LoadTelportToTownRegionsList();
        }

        private void button26_Click_1(object sender, EventArgs e)
        {
            foreach (var a in ASYNC_SERVER.AG_CONS.Where(x => x.Value.RIDING_PET))
            {
                a.Value.DISMOUNT_PET();
            }
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void groupBox51_Enter(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            UTILS.ListType = 3;
            new DISABLE_CUSTOM_REGION().ShowDialog();
            Loaddisablezerkregions();
        }

        private void button29_Click_2(object sender, EventArgs e)
        {
            UTILS.ListType = 4;
            new DISABLE_CUSTOM_REGION().ShowDialog();
            Loaddisablechatregions();
        }

        private void button27_Click_1(object sender, EventArgs e)
        {
            UTILS.ListType = 5;
            new DISABLE_CUSTOM_REGION().ShowDialog();
            Loaddisablepartyregions();
        }

        private void button28_Click_1(object sender, EventArgs e)
        {
            UTILS.ListType = 6;
            new DISABLE_CUSTOM_REGION().ShowDialog();
            Loaddisabletraceregions();
        }

        private void button32_Click(object sender, EventArgs e)
        {
            UTILS.ListType = 7;
            new DISABLE_CUSTOM_REGION().ShowDialog();
            Loaddisableinvitefriendsregions();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            UTILS.ListType = 9;
            new DISABLE_CUSTOM_REGION().ShowDialog();
            Loaddisableskillsregions();
        }

        private void button26_Click_2(object sender, EventArgs e)
        {
            UTILS.ListType = 8;
            new DISABLE_CUSTOM_REGION().ShowDialog();
            Loaddisableitemsregions();
        }

        private void textBox120_TextChanged(object sender, EventArgs e)
        {

        }

        private void label133_Click(object sender, EventArgs e)
        {

        }

        private async void button7_Click_1(object sender, EventArgs e)
        {
            if(comboBox3.SelectedIndex.ToString() != null)
            {
                switch (comboBox3.SelectedItem.ToString())
                {
                    case "Custom Name Color":
                        _CustomNameColor x = UTILS.CharnameColorList.Find(a => a.CharName == textBox8.Text && a.ShardID == Convert.ToInt32(textBox117.Text));
                        if(x !=null)
                        {
                            UTILS.WriteLine($"[{textBox8.Text}] already got a charname color please remove it first.");
                            return;
                        }
                        await QUERIES.SQL_Run_Code($"exec xQc_FILTER.[dbo].[_ADDCustomNameColor] '{textBox8.Text}','{textBox90.Text}',{Convert.ToInt32(textBox117.Text)}");
                        UTILS.WriteLine($"Successfully added a custom name color to [{textBox8.Text}]");
                        break;
                    case "Custom Title Color":
                        _CustomTitleColor x1 = UTILS.titlescolors.Find(a => a.CharName == textBox8.Text && a.ShardID == Convert.ToInt32(textBox117.Text));
                        if (x1 !=null)
                        {
                            UTILS.WriteLine($"[{textBox8.Text}] already got a title color please remove it first.");
                            return;
                        }
                        await QUERIES.SQL_Run_Code($"exec xQc_FILTER.[dbo].[_ADDCustomTitleColor] '{textBox8.Text}','{textBox90.Text}',{Convert.ToInt32(textBox117.Text)}");
                        UTILS.WriteLine($"Successfully added a custom title color to [{textBox8.Text}]");
                        break;
                    case "Custom Icon":
                        _CharIcon x2 = UTILS.CustomIcons.Find(a => a.CharName == textBox8.Text && a.ShardID == Convert.ToInt32(textBox117.Text));
                        if (x2 != null)
                        {
                            UTILS.WriteLine($"[{textBox8.Text}] already got a custom icon please remove it first.");
                            return;
                        }
                        await QUERIES.SQL_Run_Code($"exec xQc_FILTER.[dbo].[_ADDCustomIcon] '{textBox8.Text}','{textBox90.Text}',{Convert.ToInt32(textBox117.Text)}");
                        UTILS.WriteLine($"Successfully added a custom icon to [{textBox8.Text}]");
                        break;
                    case "Custom Title":
                        await QUERIES.SQL_Run_Code($"insert into {Settings.ShardInfos[Convert.ToInt32(textBox117.Text)].DBName}.dbo._InstantHwanLevelDelivery Values ({await QUERIES.Get_CharID_by_CharName16(textBox8.Text, Convert.ToInt32(textBox117.Text))},1)");
                        await QUERIES.SQL_Run_Code($"exec xQc_FILTER.[dbo].[_ADDCustomTitle] '{textBox8.Text}','{textBox90.Text}',{Convert.ToInt32(textBox117.Text)}");
                        UTILS.WriteLine($"Successfully added a custom title to [{textBox8.Text}]");
                        break;
                    case "Custom Name Rank":
                        _CustomNameRank x3 = UTILS.CustomCharnameRankList.Find(a => a.CharName == textBox8.Text && a.ShardID == Convert.ToInt32(textBox117.Text));
                        if (x3!= null)
                        {
                            UTILS.WriteLine($"[{textBox8.Text}] already got a custom rank please remove it first.");
                            return;
                        }
                        await QUERIES.SQL_Run_Code($"exec xQc_FILTER.[dbo].[_ADDCustomNameRank] '{textBox8.Text}','{textBox90.Text}',{Convert.ToInt32(textBox117.Text)}");
                        UTILS.WriteLine($"Successfully added a custom name rank to [{textBox8.Text}]");
                        break;
                }
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox60_Enter(object sender, EventArgs e)
        {

        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox224_TextChanged(object sender, EventArgs e)
        {

        }

        private void button33_Click(object sender, EventArgs e)
        {
            _ = Task.Run(async () =>
            {
                while (checkBox45.Checked)
                {
                    if (!checkBox45.Checked)
                    {
                        await Task.Delay(1000); // لو مش مفعّل، استنى ثانية وكرر التحقق
                        continue;
                    }

                    foreach (var item in Client.ClientList)
                    {
                        if (item.Value.m_Agent != null)
                        {
                            item.Value.m_Agent.CloseStall();
                            await Task.Delay(10);

                            int dstX = new Random(Guid.NewGuid().GetHashCode()).Next(6417, 6450);
                            int dstY = new Random(Guid.NewGuid().GetHashCode()).Next(1062, 1127);
                            item.Value.m_Agent.DoMovement(dstX, dstY);

                            await Task.Delay(10);
                        }
                        else
                        {
                            UTILS.WriteLine($"{item.Value._charname} : agent null ");
                        }
                    }

                    await Task.Delay(3000); // انتظر قبل تكرار الحركة
                }
            });


        }

        //数据库通知判断
        private async void AUTO_NOTICE_TIMER(object source, ElapsedEventArgs e)
        {
            try
            {
                AutoNotice_Timer.Stop();
                object[] result = await QUERIES.FETCH_NOTICE();
                if (result != null)
                {
                    string 内容 = (string)result[1];
                    if (((string)result[2]).ToLower() == "#所有人") {
                        UTILS.SEND_NOTICE_TO_ALL(内容);//Message
                    }
                    else {
                        //[ID],[Message],[CHARNAME16] ,[SLBUpdate],[SilkUpdate],[IsBreak]
                        int AddSLB = (int)result[3];
                        int AddSilk = (int)result[4];
                        int IsBreak = (int)result[5];
                        int ShardID = (int)result[6];
                        var 扩展数据 = 内容.Split('|'); 
                        var 目标玩家 = ASYNC_SERVER.AG_CONS.Where(x => x.Value.CHARNAME16 == (string)result[2]).FirstOrDefault();
                        if (目标玩家.Key != null)
                        {
                            if (IsBreak<=2)
                            {
                                UTILS.SEND_INDV_NOTICE(内容, 目标玩家.Key);
                            }

                            if (AddSLB != 0)
                            {
                                await QUERIES.GIVE_GOLD(目标玩家.Value.CHARNAME16, AddSLB, ShardID);
                                long gb = await QUERIES.GOLD_BALANCE(目标玩家.Value.CHARNAME16, ShardID);
                                //todo UTILS.AddSLB(目标玩家.Value.CHARNAME16, AddSLB, ShardID);
                                if (AddSLB > 0)
                                    //UTILS.SEND_INDV_SLB_UPDATE_NOTICE(AddSLB, 目标玩家.Key);
                                UTILS.SEND_INDV_SLB_UPDATE(gb, 目标玩家.Key);
                            }
                            if (AddSilk != 0)
                            {
                                await QUERIES.UpdateSilk(目标玩家.Value.CHARNAME16, ShardID, 1, AddSilk);
                                var Silk = await QUERIES.Get_TOT_SILK_BALANCE(目标玩家.Value.CHARNAME16, ShardID);
                                UTILS.SEND_INDV_SILK_UPDATE(Silk, 目标玩家.Key);
                            }
                            switch (IsBreak)
                            {
                                case 1:
                                    ASYNC_SERVER.DISCONNECT(目标玩家.Key, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                    break;
                                case 3:
                                    UTILS.传送至NPC(目标玩家.Key, 内容);
                                    break;
                                case 4:
                                    UTILS.传送到目标(目标玩家.Key, 内容);
                                    break;
                                case 5:
                                    int ID=Convert.ToInt32(扩展数据[0]);
                                    int 数量 = Convert.ToInt32(扩展数据[1]);
                                    UTILS.召唤怪物(目标玩家.Key, ID,数量);
                                    break;
                                case 6:
                                    ID = Convert.ToInt32(扩展数据[0]);
                                    数量 = Convert.ToInt32(扩展数据[1]);
                                    UTILS.生成物品(目标玩家.Key, ID,数量);
                                    break;
                                default:
                                    break;
                            }

                        }
                        else {
                            UTILS.WriteLine($"自定义指令[{ (string)result[2]}]玩家不在线 has returned.", UTILS.LOG_TYPE.Fatal);
                        }

                    }

                    await QUERIES.NOTICE_UPDATE_STATUS((int)result[0]);//ID
                }
                AutoNotice_Timer.Start();
            }
            catch(Exception Ex) {
                AutoNotice_Timer.Start();
                UTILS.WriteLine($"自动通知失败:{Ex.ToString()}", UTILS.LOG_TYPE.Fatal);
            }
            //todo finally ? if its not enabled or null reinitialize it
        }
        private async void SCHEDULED_NOTICE_TIMER(object source, ElapsedEventArgs e)
        {
            try
            {
                AutoScheduledNotice_Timer.Stop();
                if (NoticeListView.Items.Count <= 0) return;
                if (NoticeIndex >= NoticeListView.Items.Count) {
                    NoticeIndex = 0;
                }
                if (RandomNotificationCheckBox.Checked) {
                    NoticeIndex = new Random().Next(0, NoticeListView.Items.Count-1);
                }
                ListViewItem NoticeInfo = NoticeListView.Items[NoticeIndex];
                DateTime StartTime = Convert.ToDateTime(NoticeInfo.SubItems[1].Text);
                DateTime EndTime = Convert.ToDateTime(NoticeInfo.SubItems[2].Text);
                NoticeIndex++;
                if (DateTime.Now > StartTime && DateTime.Now < EndTime)
                {
                    String Content = NoticeInfo.SubItems[3].Text;
                    switch(NoticeInfo.SubItems[4].Text.ToLower())
                    {
                        case "blue":
                            UTILS.SendNoticeForAll(UTILS.NoticeType.Notify,Content);
                            break;
                        case "green":
                            UTILS.SendNoticeForAll(UTILS.NoticeType.Green, Content);
                            break;
                        case "orange":
                            UTILS.SendNoticeForAll(UTILS.NoticeType.Orange, Content);
                            break;
                        case "purple":
                            UTILS.SendNoticeForAll(UTILS.NoticeType.Purble, Content);
                            break;
                        default:
                            UTILS.SendNoticeForAll(UTILS.NoticeType.Notice, Content);
                            break;
                    }
                    UTILS.WriteLine($"Sent notice successfully:{Content}", UTILS.LOG_TYPE.Notify);
                }
                AutoScheduledNotice_Timer.Start();
            }
            catch { UTILS.WriteLine("Error while sending the notice.", UTILS.LOG_TYPE.Warning); }
        }
        private static async void LINGERING_TIMER(object source, ElapsedEventArgs e)
        {
            try
            {
                foreach (var item in ASYNC_SERVER.GW_CONS)
                {
                    if (!UTILS.IsSocketConnected(item.Key))
                    {
                        ASYNC_SERVER.DISCONNECT(item.Key, ASYNC_SERVER.MODULE_TYPE.GatewayServer);
                        //UTILS.WriteLine($"Disposed lingering GW connection.");
                    }
                }
                foreach (var item in ASYNC_SERVER.DW_CONS)
                {
                    if (!UTILS.IsSocketConnected(item.Key))
                    {
                        ASYNC_SERVER.DISCONNECT(item.Key, ASYNC_SERVER.MODULE_TYPE.DownloadServer);
                        //UTILS.WriteLine($"Disposed lingering DW connection.");
                    }
                }
                foreach (var item in ASYNC_SERVER.AG_CONS)
                {
                    if (!UTILS.IsSocketConnected(item.Key))
                    {
                        ASYNC_SERVER.DISCONNECT(item.Key, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                        //UTILS.WriteLine($"Disposed lingering AG connection.");
                    }
                }
            }
            catch { UTILS.WriteLine("A linger unidentified error has occured!", UTILS.LOG_TYPE.Warning); }
        }

        private static async void UpdateRank(object source, ElapsedEventArgs e)
        {
            try
            {
                RankTimer.Stop();
                
                foreach(var x in Settings.ShardInfos.Keys)
                {
                  //  await QUERIES.LoadCharRank(x);
                    await QUERIES.LoadPVPRank(x);
                    await QUERIES.LoadGuildRank(x);
                    await QUERIES.LoadHonorRank(x);
                    //await QUERIES.LoadJobKillsRank(x);
                    await QUERIES.LoadTraderRank(x);
                    await QUERIES.LoadHunterRank(x);
                    await QUERIES.LoadThiefRank(x);
                }
                await QUERIES.LoadEventTime();
                RankTimer.Start();
            }
            catch { UTILS.WriteLine("UpdateRank failed."); }
        }

        private static async void RegisterEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                RegisterEventTimer.Stop();
                if (PVPregisterlist.Count > 1 && !PVP_EVENT_ACTIVE)
                {
                    await PVPEVENT();
                }
                if (UNIQUEregisterlist.Count > 1 && !UNIQUE_EVENT_ACTIVE)
                {
                    await UNIQUEEVENT();
                }
                RegisterEventTimer.Start();
            }
            catch (Exception EX) { UTILS.WriteLine($"RegisterEventTimer failed, please click save settings button in order to try and fix this. {EX}", LOG_TYPE.Fatal); }
        }
        private static async void ResetEvery24h(object source, ElapsedEventArgs e)
        {
            try
            {
                ResetTimer.Stop();
                PVPeventRegCount.Clear();
                UNIQUEeventRegCount.Clear();
                GambleAttempts.Clear();
                ResetTimer.Start();
            }
            catch (Exception EX) { WriteLine($"ResetEvery24h failed,  {EX}", LOG_TYPE.Fatal); }
        }

        private static async void MinionsPing(object source, ElapsedEventArgs e)
        {
            try
            {
                MinionsTimer.Stop();
                foreach (var item in Client.ClientList)
                    if (item.Value.m_Agent != null)
                        item.Value.m_Agent.Ping();
                MinionsTimer.Start();
            }
            catch (Exception EX) { WriteLine($"MinionsPing failed, please click save settings button in order to try and fix this. {EX}", LOG_TYPE.Fatal); }

        }
        private static async void FilterCommandsTimer(object source, ElapsedEventArgs e)
        {
            try
            {
                FilterCommands.Stop();
                await QUERIES.FilterCommands();
                FilterCommands.Start();
            }
            catch (Exception EX) { WriteLine($"FilterCommandsTimer failed, please click save settings button in order to try and fix this. {EX}", LOG_TYPE.Fatal); }
        }
        private static async void FetchEvents(object source, ElapsedEventArgs e)
        {
            try
            {
                EventsTimer.Stop();
                for (int i = 0; i < EventScheduling.Count; i++)
                {
                    string eName = EventScheduling[i].Key;
                    var Info = EventScheduling[i].Value;
                    DateTime Start = DateTime.ParseExact(Info[1], "HH:mm:ss", CultureInfo.InvariantCulture);
                    if (DateTime.Now.Hour == Start.Hour && Start.Minute == DateTime.Now.Minute && (Info[0].ToLower() == DateTime.Now.DayOfWeek.ToString().ToLower() || Info[0].ToLower() == "everyday"))
                    {
                        if (eName == "Guild War")
                            await GWEvent();
                        else if (eName == "Job Fight")
                            await JFEvent();
                        else if (eName == "Survival Arena")
                            await SURVIVALEVENT();
                        else if (eName == "Drunk Event")
                            await DRUNKEVENT();
                    }
                    DateTime XsmbWinners = DateTime.ParseExact("19:00:00", "HH:mm:ss", CultureInfo.InvariantCulture);
                    if(DateTime.Now.Hour == XsmbWinners.Hour && XsmbWinners.Minute == DateTime.Now.Minute)
                    {
                        await UTILS.GetWinnerNumbers();
                        string Msg = "(";
                        foreach (var x in UTILS.XSMB)
                        {
                            if(XSMB.IndexOf(x) ==0)
                                Msg = Msg + $"{x}";
                            else
                                Msg = Msg + $",{x}";
                        }

                        Msg = Msg + ")";
                        await QUERIES.Update_Xsmb(Msg);
                        DataTable commands2 = Task.Run(async () => await QUERIES.GetList($"SELECT * FROM xQc_FILTER.dbo._XsmbEvent")).Result;
                        foreach (DataRow dts in commands2.Rows)
                        {
                            string CharName = dts["CharName"].ToString();
                            long Amount = Convert.ToInt64(dts["Amount"]);
                            byte type =Convert.ToByte(dts["Type"]);
                            int Num = (dts["Num"] != DBNull.Value) ? Convert.ToInt32(dts["Num"]) : 0;
                            if(UTILS.XSMB.Contains((byte)Num))
                            {
                                if (type == 1)
                                {
                                    UTILS.SendNoticeForAll(NoticeType.Green, $"Chóc Mõng Nh©n VËt {CharName} §· Tróng {Amount*3* XSMB.Count(x => x == (byte)Num)} Silk Event XSMB");
                                    await QUERIES.GIVE_SILK(await QUERIES.Get_StrUserID_by_CHARNAME16(CharName, 64), (int)(Amount * 3 * XSMB.Count(x => x == (byte)Num)));
                                }
                                else
                                {
                                    UTILS.SendNoticeForAll(NoticeType.Green, $"Chóc Mõng Nh©n VËt {CharName} §· Tróng {Amount * 3* XSMB.Count(x => x == (byte)Num)} Gold Event XSMB");
                                    await QUERIES.Update_Storage_Gold_Amount(await QUERIES.Get_StrUserID_by_CHARNAME16(CharName, 64), 64, (Amount * 3* XSMB.Count(x => x == (byte)Num)));
                                }
                            }
                        }
                        await QUERIES.SQL_Run_Code("truncate table xQc_FILTER.dbo._XsmbEvent");

                    }
                }

                EventsTimer.Start();
            }
            catch (Exception EX) { WriteLine($"FetchEvents failed,  {EX}", LOG_TYPE.Fatal); }
        }
        public static async Task DRUNKEVENT()
        {
            try
            {
                if (!Settings.MAIN.checkBox44.Checked)
                    return;
                UTILS.DRUNK_REGISTER_ACTIVE = true;
                DRUNKregisterActivelist.Clear();
                UTILS.SendNoticeForAll(NoticeType.Green, Settings.MAIN.textBox136.Text);
                await Task.Delay(5 * 60000);
                UTILS.SendNoticeForAll(NoticeType.Green, Settings.MAIN.textBox133.Text);
                await Task.Delay(4 * 60000);
                UTILS.SendNoticeForAll(NoticeType.Green, Settings.MAIN.textBox132.Text);
                await Task.Delay(1 * 60000);
                UTILS.DRUNK_REGISTER_ACTIVE = false;
                UTILS.DRUNK_ACTIVE = true;
                if (UTILS.DRUNKregisterlist.Count > 1)
                {
                    foreach (KeyValuePair<Socket, AGENT_MODULE> con in ASYNC_SERVER.AG_CONS.Where(x => !string.IsNullOrEmpty(x.Value.CHARNAME16)))
                    {
                        if (UTILS.DRUNKregisterlist.Contains(con.Value.CHARNAME16))
                        {
                            if (!con.Value.JOB_FLAG && !con.Value.DEAD_STATUS && !con.Value.INSIDE_PT)
                                DRUNKregisterActivelist.Add(con.Value);
                        }
                    }

                    foreach (var con in DRUNKregisterActivelist)
                    {
                        await QUERIES.INSERT_INSTANT_TELEPORT(con.CharID, 1, 1, -32748, 956, -134, 1015, 64);
                        con.INSIDE_DRUNK = true;
                    }

                    await Task.Delay(20000);
                    if(!IS_UNIQUE_SPAWNED)
                    {
                        IS_UNIQUE_SPAWNED = true;
                        await QUERIES.INSERT_UNIQUE_BYPOS(1, 1, 1, -32748, 956, -134, 1015, 3, 1, 1945, 64);
                    }
                    LAST_DRUNK_EVENT = DateTime.Now;
                    foreach (var con in DRUNKregisterActivelist)
                        con.ShowTimer(45 * 60000);

                    bool IS_FINISHED = false;
                    while (DRUNKregisterActivelist.Count > 1)
                    {
                        int time = 45 * 60000 - Convert.ToInt32(DateTime.Now.Subtract(UTILS.LAST_DRUNK_EVENT).TotalSeconds) * 1000;
                        if (time < 0)
                        {
                            IS_FINISHED = true;
                            break;
                        }
                        await Task.Delay(30000);
                    }
                    UTILS.DRUNK_ACTIVE = false;
                    if (IS_FINISHED)
                        UTILS.SendNoticeForAll(NoticeType.Green, Settings.MAIN.textBox129.Text);
                    else
                    {
                        foreach(var x in DRUNKregisterActivelist)
                        {
                            UTILS.SendNoticeForAll(NoticeType.Green,String.Format(Settings.MAIN.textBox128.Text, x.CHARNAME16));
                            await QUERIES.UpdateSilk(x.CHARNAME16, 64, 1, Convert.ToInt32(Settings.MAIN.textBox232.Text));
                            await QUERIES.INSERT_INSTANT_TELEPORT(x.CharID, 1, 1, 25000, 982, 140, 140, 64);

                        }
                    }

                }
                    UTILS.SendNoticeForAll(NoticeType.Green, Settings.MAIN.textBox129.Text);
                UTILS.DRUNKregisterlist.Clear();
            }
            catch (Exception EX)
            {
                UTILS.WriteLine($"DRUNK EVENT FAILED {EX.ToString()}");
            }
        }
        public static async Task SURVIVALEVENT()
        {
            try
            {
                if (!Settings.MAIN.checkBox43.Checked)
                    return;
                SURVIVAL_ACTIVE = true;
                SURVregisterActivelist.Clear();
                UTILS.SendNoticeForAll(NoticeType.Green, Settings.MAIN.textBox118.Text);
                await Task.Delay(5 * 60000);
                UTILS.SendNoticeForAll(NoticeType.Green, Settings.MAIN.textBox119.Text);
                await Task.Delay(4 * 60000);
                UTILS.SendNoticeForAll(NoticeType.Green, Settings.MAIN.textBox121.Text);
                await Task.Delay(1 * 60000);
                SURVIVAL_ACTIVE = false;
                SURVKills.Clear();
                await QUERIES.TRUNCATE_SURVKILLS();
                if(UTILS.SURVregisterlist.Count > 1)
                {
                    foreach (KeyValuePair<Socket, AGENT_MODULE> con in ASYNC_SERVER.AG_CONS.Where(x => !string.IsNullOrEmpty(x.Value.CHARNAME16)))
                    {
                        if (UTILS.SURVregisterlist.Contains(con.Value.CHARNAME16))
                        {
                            if (!con.Value.JOB_FLAG && !con.Value.DEAD_STATUS && !con.Value.INSIDE_PT)
                                SURVregisterActivelist.Add(con.Value);
                        }
                    }
                    foreach (var con in SURVregisterActivelist)
                        await QUERIES.INSERT_INSTANT_TELEPORT(con.CharID, 1, 1, 25580, 500, 0, 500,64);

                    await Task.Delay(20000);
                    LAST_SURV_EVENT = DateTime.Now;
                    foreach (var con in SURVregisterActivelist)
                    {
                        await QUERIES.INSTANT_PVPCAPE(con.CharID, 5, 64);
                        con.ShowTimer(15 * 60000);
                    }    
                    
                    while (true)
                    {
                        await QUERIES.UpdateSURVKillsCounter();
                        var top5 = UTILS.SURVKills.OrderByDescending(o => o.Value).Take(5).ToList();
                        Packet logpacket = new Packet(0x5123);
                        logpacket.WriteValue<byte>(top5.Count);
                        foreach (var line in top5)
                        {
                            logpacket.WriteValue<string>(line.Key);
                            logpacket.WriteValue<string>(UTILS.FormatNumber(line.Value));
                        }
                        foreach (var par in SURVregisterActivelist)
                        {
                            par.LOCAL_SECURITY.Send(logpacket);
                            par.ASYNC_SEND_TO_CLIENT(par.CLIENT_SOCKET);
                            par.SURVKillsGui = true;
                            
                        }
                        int time = 15 * 60000 - Convert.ToInt32(DateTime.Now.Subtract(UTILS.LAST_SURV_EVENT).TotalSeconds) * 1000;
                        if (time < 0)
                            break;
                        await Task.Delay(15000);
                    }
                    var Winners = UTILS.SURVKills.OrderByDescending(o => o.Value).ToList();

                    for (int i = 0; i < Winners.Count; i++)
                    {
                        if(i==0)
                        {
                            await QUERIES.UpdateSilk(Winners.ElementAt(i).Key, 64, 1, Convert.ToInt32(Settings.MAIN.textBox215.Text));
                            UTILS.SendNoticeForAll(NoticeType.Green, String.Format(Settings.MAIN.textBox123.Text, Winners.ElementAt(i).Key));
                        }
                        else if(i==1)
                        {
                            UTILS.SendNoticeForAll(NoticeType.Green, String.Format(Settings.MAIN.textBox122.Text, Winners.ElementAt(i).Key));
                            await QUERIES.UpdateSilk(Winners.ElementAt(i).Key, 64, 1, Convert.ToInt32(Settings.MAIN.textBox226.Text));
                        }
                        else if(i==2)
                        {
                            await QUERIES.UpdateSilk(Winners.ElementAt(i).Key, 64, 1, Convert.ToInt32(Settings.MAIN.textBox227.Text));
                            UTILS.SendNoticeForAll(NoticeType.Green, String.Format(Settings.MAIN.textBox125.Text, Winners.ElementAt(i).Key));
                        }
                    }
                    foreach (var con in SURVregisterActivelist)
                        await QUERIES.INSERT_INSTANT_TELEPORT(con.CharID, 1, 1, 25000, 982, 140, 140, 64);
                    UTILS.SURVregisterlist.Clear();
                }
                UTILS.SendNoticeForAll(NoticeType.Green, Settings.MAIN.textBox124.Text);
            }
            catch (Exception EX)
            {
                UTILS.WriteLine($"Survival failed. {EX}");
            }
        }
        public static async Task PVPEVENT()
        {
            try
            {
                PVP_EVENT_ACTIVE = true;
                string charname1 = string.Empty;
                string charname2 = string.Empty;
                AGENT_MODULE chr1 = null, chr2 = null;
                while (true)
                {
                    if (PVPregisterlist.Count < 2)
                        return;
                    charname1 = PVPregisterlist[0];
                    charname2 = PVPregisterlist[1];
                    //save names
                    PVPactivelist.Clear();
                    PVPactivelist.Add(charname1);
                    PVPactivelist.Add(charname2);

                    foreach (KeyValuePair<Socket, AGENT_MODULE> con in ASYNC_SERVER.AG_CONS.Where(x => !string.IsNullOrEmpty(x.Value.CHARNAME16)))
                    {
                        if (con.Value.CHARNAME16 == charname1)
                            chr1 = con.Value;
                        else if (con.Value.CHARNAME16 == charname2)
                            chr2 = con.Value;
                    }
                    if (chr1.JOB_FLAG || chr1.DEAD_STATUS)
                        PVPregisterlist.RemoveAt(0);
                    else if (chr2.JOB_FLAG || chr2.DEAD_STATUS)
                        PVPregisterlist.RemoveAt(1);
                    else
                    {
                        //removing them from list
                        PVPregisterlist.RemoveRange(0, 2);
                        break;
                    }
                }
                PVPeventRegCount[charname1] += 1;
                PVPeventRegCount[charname2] += 1;
                //send a notice
                UTILS.SendNoticeForAll(NoticeType.Orange, $"[PVP MATCHING] {charname1} vs {charname2}.");

                await Task.Delay(400);
                await QUERIES.INSERT_INSTANT_TELEPORT(chr1.CharID, 1, 1, 22966, 1101, 51, 1117,64);
                await QUERIES.INSERT_INSTANT_TELEPORT(chr2.CharID, 1, 1, 22966, 827, 51, 1068,64);

                await Task.Delay(20000);
                if (chr1 != null)
                {
                    chr1.ShowTimer(5 * 60000);
                    SendNotice(NoticeType.Notify, "You have 5 minutes to kill the opposite player!", chr1.CLIENT_SOCKET);
                    await QUERIES.INSTANT_PVPCAPE(chr1.CharID, 2,64);
                }
                if (chr2 != null)
                {
                    chr2.ShowTimer(5 * 60000);
                    SendNotice(NoticeType.Notify, "You have 5 minutes to kill the opposite player!", chr2.CLIENT_SOCKET);
                    await QUERIES.INSTANT_PVPCAPE(chr2.CharID, 3,64);
                }

                int i = 0;
                while (true)
                {
                    if (!PVP_EVENT_ACTIVE)
                        break;
                    await Task.Delay(2000);
                    i++;
                    if (i == 150)
                        break;
                }
                chr1.LifeStats();
                chr2.LifeStats();
                if (i == 150)
                {
                    UTILS.SendNoticeForAll(NoticeType.Orange, $"[PVP MATCHING] {charname1} vs {charname2} has ended without any winners.");
                    PVP_EVENT_ACTIVE = false;
                    await QUERIES.INSERT_INSTANT_TELEPORT(chr1.CharID, 1, 1, 25000, 982, 140, 140,64);
                    await QUERIES.INSERT_INSTANT_TELEPORT(chr2.CharID, 1, 1, 25000, 982, 140, 140,64);
                }
            }
            catch (Exception EX) { PVP_EVENT_ACTIVE = false; WriteLine($"[PVPEVENT] operation has failed. {EX.Message}", LOG_TYPE.Fatal); }
        }
        public static async Task UNIQUEEVENT()
        {
            try
            {
                UNIQUE_EVENT_ACTIVE = true;
                string charname1 = string.Empty;
                string charname2 = string.Empty;
                AGENT_MODULE chr1 = null, chr2 = null;
                while (true)
                {
                    if (UNIQUEregisterlist.Count < 2)
                        return;

                    charname1 = UNIQUEregisterlist[0];
                    charname2 = UNIQUEregisterlist[1];
                    //save names
                    UNIQUEactivelist.Clear();
                    UNIQUEactivelist.Add(charname1);
                    UNIQUEactivelist.Add(charname2);
                    //teleport them to destination
                    foreach (KeyValuePair<Socket, AGENT_MODULE> con in ASYNC_SERVER.AG_CONS.Where(x => !string.IsNullOrEmpty(x.Value.CHARNAME16)))
                    {
                        if (con.Value.CHARNAME16 == charname1)
                            chr1 = con.Value;
                        else if (con.Value.CHARNAME16 == charname2)
                            chr2 = con.Value;
                    }
                    if (chr1.JOB_FLAG || chr1.DEAD_STATUS)
                        UNIQUEregisterlist.RemoveAt(0);
                    else if (chr2.JOB_FLAG || chr2.DEAD_STATUS)
                        UNIQUEregisterlist.RemoveAt(1);
                    else
                    {
                        //removing them from list
                        UNIQUEregisterlist.RemoveRange(0, 2);
                        break;
                    }
                }
                UNIQUEeventRegCount[charname1] += 1;
                UNIQUEeventRegCount[charname2] += 1;
                //send a notice
                UTILS.SendNoticeForAll(NoticeType.Orange, $"[UNIQUE MATCHING] {charname1} vs {charname2}.");

                await Task.Delay(400);
                await QUERIES.INSERT_INSTANT_TELEPORT(chr1.CharID, 1, 1, 25584, 1789, 506, 444,64);
                await QUERIES.INSERT_INSTANT_TELEPORT(chr2.CharID, 1, 1, 25584, 1479, 505, 441,64);
                await Task.Delay(10000);

                if (IS_UNIQUE_KILLED)
                    await QUERIES.INSERT_UNIQUE_BYPOS(1, 1, 1, 25584, 1562, 502, 560, 3, 1, 22528,64);

                if (chr1 != null)
                {
                    chr1.ShowTimer(5 * 60000);
                    SendNotice(NoticeType.Notify, "You have 5 minutes to kill the unique!", chr1.CLIENT_SOCKET);
                }
                if (chr2 != null)
                {
                    chr2.ShowTimer(5 * 60000);
                    SendNotice(NoticeType.Notify, "You have 5 minutes to kill the unique!", chr2.CLIENT_SOCKET);
                }

                int i = 0;
                while (true)
                {
                    if (!UNIQUE_EVENT_ACTIVE)
                        break;

                    i++;
                    if (i == 150)
                        break;

                    await Task.Delay(2000);
                }
                //incase a noob was killed by the unique
                chr1.LifeStats();
                chr2.LifeStats();
                if (i == 150)
                {
                    IS_UNIQUE_KILLED = false;
                    UTILS.SendNoticeForAll(NoticeType.Orange, $"[UNIQUE MATCHING] {charname1} vs {charname2} has ended without any winners.");
                    UNIQUE_EVENT_ACTIVE = false;
                    UTILS.UNIQUEactivelist.Clear();
                    await QUERIES.INSERT_INSTANT_TELEPORT(chr1.CharID, 1, 1, 25000, 982, 140, 140,64);
                    await QUERIES.INSERT_INSTANT_TELEPORT(chr2.CharID, 1, 1, 25000, 982, 140, 140,64);
                }
                #endregion
            }
            catch (Exception EX) { UNIQUE_EVENT_ACTIVE = false; WriteLine($"[UNIQUE EVENT] operation has failed. {EX.Message}", LOG_TYPE.Fatal); }
        }
        public static async Task GWEvent()
        {
            try
            {
                GW_EVENT_REGISTER = true;
                UTILS.SendNoticeForAll(NoticeType.Green, $"[GUILD WAR EVENT] will start in 10 minutes. Registering has been allowed!");
                await Task.Delay(5 * 60000);
                UTILS.SendNoticeForAll(NoticeType.Green, $"[GUILD WAR EVENT] will start in 5 minutes. please make sure that you dont equip job suit to enter");
                await Task.Delay(4 * 60000);
                UTILS.SendNoticeForAll(NoticeType.Green, $"[GUILD WAR EVENT] will start in 1 minute. please make sure that you dont equip job suit to enter");
                await Task.Delay(1 * 60000);
                GW_EVENT_REGISTER = false;
                if (GuildWarEvent.Count == 4)
                {
                    ConcurrentDictionary<AGENT_MODULE, byte> AGS = new ConcurrentDictionary<AGENT_MODULE, byte>();
                    foreach (KeyValuePair<Socket, AGENT_MODULE> con in ASYNC_SERVER.AG_CONS.Where(x => !string.IsNullOrEmpty(x.Value.CHARNAME16)))
                    {
                        if (UTILS.GuildWarEvent.ContainsKey(con.Value.GUILDNAME))
                        {
                            if (UTILS.GuildWarEvent[con.Value.GUILDNAME].Contains(con.Value.CHARNAME16))
                            {
                                if (con.Value.DEAD_STATUS || con.Value.JOB_FLAG)
                                    UTILS.GuildWarEvent[con.Value.GUILDNAME].Remove(con.Value.CHARNAME16);
                                else
                                {
                                    byte index = 0;
                                    if (UTILS.GuildWarEvent.Keys.ElementAt(0) == con.Value.GUILDNAME)
                                        index = 1;
                                    else if (UTILS.GuildWarEvent.Keys.ElementAt(1) == con.Value.GUILDNAME)
                                        index = 2;
                                    else if (UTILS.GuildWarEvent.Keys.ElementAt(2) == con.Value.GUILDNAME)
                                        index = 3;
                                    else if (UTILS.GuildWarEvent.Keys.ElementAt(3) == con.Value.GUILDNAME)
                                        index = 4;
                                    //teleport him
                                    con.Value.LiveTeleport(1, 1, 25580, 500, 0, 500);
                                    AGS.TryAdd(con.Value, index);
                                }
                            }
                        }
                    }

                    await Task.Delay(10000);

                    foreach (var par in AGS)
                    {
                        await QUERIES.INSTANT_PVPCAPE(par.Key.CharID, par.Value,64);
                        par.Key.ShowTimer(10 * 60000);
                    }
                    int i = 0;
                    while (true)
                    {
                        await QUERIES.UpdateSURVKillsCounter();
                        var top5 = UTILS.SURVKills.OrderByDescending(o => o.Value).Take(5).ToList();
                        Packet logpacket = new Packet(0x5123);
                        logpacket.WriteValue<byte>(top5.Count);
                        foreach (var line in top5)
                        {
                            logpacket.WriteValue<string>(line.Key);
                            logpacket.WriteValue<string>(UTILS.FormatNumber(line.Value));
                        }
                        foreach (var par in AGS)
                        {
                            par.Key.LOCAL_SECURITY.Send(logpacket);
                            par.Key.ASYNC_SEND_TO_CLIENT(par.Key.CLIENT_SOCKET);
                            par.Key.SURVKillsGui = true;
                        }
                        if (i == 61)
                            break;
                        await Task.Delay(10000);
                        i++;
                    }
                    //announce winners
                    var top1 = UTILS.SURVKills.OrderByDescending(o => o.Value).Take(1).ToList();
                    UTILS.SendNoticeForAll(NoticeType.Green, $"Congratulations to [{top1.ElementAt(0).Key}] for winning guild war event !. ");
                    foreach (var par in AGS)
                    {
                        par.Key.LiveTeleport(1, 1, 25000, 982, 140, 140);
                        if (par.Key.GUILDNAME == top1.ElementAt(0).Key)
                            par.Key.LiveGold(2500000, true);
                        else
                            par.Key.LiveGold(1000000, true);

                    }
                    //clear everything
                    SURVKills.Clear();
                    GuildWarEvent.Clear();
                    await QUERIES.TRUNCATE_SURVKILLS();
                }
                else
                {
                    UTILS.SendNoticeForAll(NoticeType.Green, $"[GUILD WAR EVENT] has been cancelled.");
                    GuildWarEvent.Clear();
                }
            }
            catch (Exception EX)
            {
                GW_EVENT_REGISTER = false;
                WriteLine($"[GUILD WAR EVENT] operation has failed. {EX.Message}", LOG_TYPE.Fatal);
            }

        }
        public static async Task JFEvent()
        {
            try
            {
                JOB_FIGHT_REGISTER = true;
                UTILS.SendNoticeForAll(NoticeType.Green, $"[JOB FIGHT EVENT] will start in 10 minutes. Registering has been allowed!");
                await Task.Delay(5 * 60000);
                UTILS.SendNoticeForAll(NoticeType.Green, $"[JOB FIGHT EVENT] will start in 5 minutes. please make sure that you equip job suit to enter");
                await Task.Delay(4 * 60000);
                UTILS.SendNoticeForAll(NoticeType.Green, $"[JOB FIGHT EVENT] will start in 1 minute. please make sure that you equip job suit to enter");
                await Task.Delay(1 * 60000);
                JOB_FIGHT_REGISTER = false;
                List<AGENT_MODULE> AGS = new List<AGENT_MODULE>();
                foreach (KeyValuePair<Socket, AGENT_MODULE> con in ASYNC_SERVER.AG_CONS.Where(x => !string.IsNullOrEmpty(x.Value.CHARNAME16)))
                    if (JobFightEvent.Contains(con.Value.CHARNAME16) && con.Value.JobType > 0 && con.Value.JobType < 4 && con.Value.JOB_FLAG)
                    {
                        AGS.Add(con.Value);
                        if (con.Value.JobType == 2)
                            await QUERIES.INSERT_INSTANT_TELEPORT(con.Value.CharID, 1, 1, 29399, 1597, 103, 1367,64);
                        else
                            await QUERIES.INSERT_INSTANT_TELEPORT(con.Value.CharID, 1, 1, 29655, 62, 107, 945,64);
                    }
                if (AGS.Where(x => x.JobType == 2).Count() > 0 && (AGS.Where(x => x.JobType == 1).Count() + AGS.Where(x => x.JobType == 3).Count()) > 0)
                {
                    await Task.Delay(10000);
                    LAST_JF_EVENT = DateTime.Now;
                    foreach (var par in AGS)
                        par.ShowTimer(20 * 60000);
                    int i = 0;
                    SURVKills.Clear();
                    await QUERIES.TRUNCATE_SURVKILLS();
                    while (true)
                    {
                        await QUERIES.UpdateSURVKillsCounter();
                        var top5 = UTILS.SURVKills.OrderByDescending(o => o.Value).Take(5).ToList();
                        Packet logpacket = new Packet(0x5123);
                        logpacket.WriteValue<byte>(top5.Count);
                        foreach (var line in top5)
                        {
                            logpacket.WriteValue<string>(line.Key);
                            logpacket.WriteValue<string>(UTILS.FormatNumber(line.Value));
                        }
                        foreach (var par in AGS)
                        {
                            par.LOCAL_SECURITY.Send(logpacket);
                            par.ASYNC_SEND_TO_CLIENT(par.CLIENT_SOCKET);
                            par.SURVKillsGui = true;
                            int time = 20 * 60000 - Convert.ToInt32(DateTime.Now.Subtract(UTILS.LAST_JF_EVENT).TotalSeconds) * 1000;
                            if (time > 0)
                                par.ShowTimer(time);
                            else
                                break;
                        }
                        if (i == 121)
                            break;
                        await Task.Delay(10000);
                        i++;
                    }
                    //announce winners
                    var top1 = UTILS.SURVKills.OrderByDescending(o => o.Value).Take(1).ToList();
                    UTILS.SendNoticeForAll(NoticeType.Green, $"Congratulations to [{top1.ElementAt(0).Key}] for winning job war event !. ");
                    foreach (var par in AGS)
                    {
                        await QUERIES.INSERT_INSTANT_TELEPORT(par.CharID, 1, 1, 25000, 982, 140, 140,64);
                        if (top1.ElementAt(0).Key.Contains("Thie"))
                        {
                            if (par.JobType == 2)
                                await QUERIES.INSERT_ITEM_CHEST(par.CharID, 25834, 20, 0, "Job Fight",par.ShardID);
                        }
                        else
                        if (par.JobType == 1 || par.JobType == 3)
                            await QUERIES.INSERT_ITEM_CHEST(par.CharID, 25834, 20, 0, "Job Fight",par.ShardID);
                    }
                    //clear everything
                    SURVKills.Clear();
                    JobFightEvent.Clear();
                    await QUERIES.TRUNCATE_SURVKILLS();
                }
                else
                {
                    UTILS.SendNoticeForAll(NoticeType.Green, $"[JOB FIGHT EVENT] has been cancelled.");
                    //teleport them to town
                    foreach (var par in AGS)
                    {
                        par.LiveTeleport(1, 1, 25000, 982, 140, 140);
                        //if (par.JobType == x)
                        //    par.LiveGold(2500000, true);
                        //else
                        //    par.LiveGold(1000000, true);

                    }
                    JobFightEvent.Clear();
                }

            }
            catch (Exception EX)
            {
                JOB_FIGHT_REGISTER = false;
                WriteLine($"JFEvent operation has failed. {EX.Message}", LOG_TYPE.Fatal); }
            }


        #region MISC FUNCS
        public async void Save()
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                await WRITER.REWRITE_CFG();
                READER.READ_REQ_INIT();
                READER.READ_FEATURES();
                READER.LoadMOpcodes();
                READER.LoadChatFilter();
                READER.LoadBlockedSkillsIDs();
                READER.LoadBlockedFWSkillsIDs();
                READER.LoadBlockedCTFSkillsIDs();
                READER.LoadBlockedBASkillsIDs();
                READER.LoadBlockedJobSkillsIDs();
                READER.LoadNetCafeIPs();
                READER.LoadDisableChatRegions();
                READER.LoadDisableZerkRegions();
                READER.LoadDisablePartyRegions();
                READER.LoadDisableTraceRegions();
                READER.LoadDisableInviteFriendsRegions();
                READER.LoadDisableSkillsRegions();
                READER.LoadDisableItemsRegions();
                QUERIES.SetConnectiontType();

                await QUERIES.SQL_SVR_BIT();
                await SG_QUERIES.PreloadFilterDatabaseAsync();

                //Reset our timers
                TIMERS_INIT();
                sw.Stop();
                UTILS.WriteLine($"All settings was saved in[{sw.ElapsedMilliseconds} ms]", UTILS.LOG_TYPE.Notify);
            }
            catch(Exception ex) { UTILS.WriteLine($"An error occured while saving settings.{ex.ToString()}", UTILS.LOG_TYPE.Fatal); }
        }
        public static void UPLOAD_TO_MEGA()
        {
            //try
            //{
            //    await Task.Factory.StartNew(() =>
            //    {
            //        UTILS.WriteLine("Started uploading to mega, be patient.");
            //        string[] databases = new string[3] { Settings.MSSQL_LOG_DB, Settings.MSSQL_ACC_DB, Settings.MSSQL_SHARD_DB };
            //        string folderName = DateTime.Now.ToString("dd.MM.yyyy HH-mm-ss tt", CultureInfo.InvariantCulture);
            //        string Backup_directory = Settings.MSSQL_BACKUP_PATH + "MEGA" + "\\";
            //        Directory.CreateDirectory(Backup_directory);

            //        //Mega Api 
            //        CG.Web.MegaApiClient.MegaApiClient client = new CG.Web.MegaApiClient.MegaApiClient();
            //        client.Login(Settings.MEGA_USERNAME, Settings.MEGA_PASSWORD);
            //        var nodes = client.GetNodes();
            //        CG.Web.MegaApiClient.INode root = nodes.Single(n => n.Type == CG.Web.MegaApiClient.NodeType.Root);
            //        CG.Web.MegaApiClient.INode backupFolder = client.CreateFolder("SR_Proxy Backup - " + folderName, root);

            //        UTILS.WriteLine($"Started to upload your databases to MEGA drive, this operation might take a while, do not close the application!");

            //        foreach (string database in databases)
            //        {
            //            string filePath = Backup_directory + database + ".bak";
            //            CG.Web.MegaApiClient.INode myFile = client.UploadFile(filePath, backupFolder);
            //            UTILS.WriteLine($"Finished uploading {database}");
            //        }
            //        UTILS.WriteLine($"Finished uploading databases to MEGA drive.");
            //    });
            //}
            //catch (Exception EX) { UTILS.WriteLine($"UPLOAD_TO_MEGA has failed. {EX.ToString()}"); }
        }

        #endregion
    }
}
