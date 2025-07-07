using SR_PROXY.CORE_NETWORKING;
using SR_PROXY.ENGINES;
using SR_PROXY.SECURITYOBJECTS;
using SR_PROXY.SQL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SR_PROXY.CORE
{
    public static class ASYNC_SERVER
    {
        //Threads Signals - Notifies one or more waiting threads that an event has occurred.
        public static AutoResetEvent ARE = new AutoResetEvent(false);
        public static AutoResetEvent ARE2 = new AutoResetEvent(false);
        public static bool ACTIVATION = true;
        public enum MODULE_TYPE { DownloadServer, GatewayServer, AgentServer };

        //Thread-safe connections manager collections
        public static ConcurrentDictionary<Socket, DOWNLOAD_MODULE> DW_CONS = new ConcurrentDictionary<Socket, DOWNLOAD_MODULE>();
        public static ConcurrentDictionary<Socket, GATEWAY_MODULE> GW_CONS = new ConcurrentDictionary<Socket, GATEWAY_MODULE>();
        public static ConcurrentDictionary<Socket, AGENT_MODULE> AG_CONS = new ConcurrentDictionary<Socket, AGENT_MODULE>();

        //Using a list of disposed gw sessions to assign the disposed gw session
        //to its matching ag session by their matching TokenIDs and RedirIP (in case of multiple AgentServers)
        public static ConcurrentDictionary<Socket, GATEWAY_MODULE> DISPOSED_GW_SESSIONS = new ConcurrentDictionary<Socket, GATEWAY_MODULE>();
        public static List<string> AGENT_MODULES = new List<string>();//Learn AG modules AI, fail safe mechanism

        //Next latest agent inner module redirection
        public static volatile string AG_REDIR_IP;
        public static volatile int AG_REDIR_PORT;

        #region INIT_ASYNC_SERVERS & ACCEPTED_CONNECTION CALLBACKS
        public static void InitializeSingleEngine(string BIND_IP, int GW_PORT, int DW_PORT, int AG_PORT)
        {
            try
            {
                //UTILS.WriteLine($"Thread ID: {Thread.CurrentThread.ManagedThreadId.ToString()}");
                CustomSocket GATEWAY_LISTENER = new CustomSocket();
                //binding socket for our GatewayServer...
                GATEWAY_LISTENER.Bind(new IPEndPoint(IPAddress.Parse(BIND_IP), GW_PORT));
                GATEWAY_LISTENER.Listen(5);

                CustomSocket DOWNLOAD_LISTENER = new CustomSocket();
                //binding socket for our DownloadServer...
                DOWNLOAD_LISTENER.Bind(new IPEndPoint(IPAddress.Parse(BIND_IP), DW_PORT));
                DOWNLOAD_LISTENER.Listen(5);

                CustomSocket AGENT_LISTENER = new CustomSocket();
                //binding socket for our AgentServer...
                AGENT_LISTENER.Bind(new IPEndPoint(IPAddress.Parse(BIND_IP), AG_PORT));
                AGENT_LISTENER.Listen(5);

                //Successful endpoint bound checks
                if (GATEWAY_LISTENER.IsBound && DOWNLOAD_LISTENER.IsBound && AGENT_LISTENER.IsBound)
                {
                    Thread ListenerThread = new Thread(() =>//We are spinning up a dedicated thread for both async server, because tasks will tie up threads in the threadpool...
                    {
                        while (ACTIVATION)
                        {
                            //ARE.Reset();// Set the event to nonsignaled state.
                            DOWNLOAD_LISTENER.AcceptDownloadServerConnections();
                            GATEWAY_LISTENER.AcceptGatewayServerConnections();
                            AGENT_LISTENER.AcceptAgentServerConnections();
                            ARE.WaitOne();// Wait until a connection is made before continuing.
                        }
                    });
                    ListenerThread.Name = "ListenerThread";
                    ListenerThread.Start();
                }
                //Log Output
                UTILS.WriteLine($"Default asynchronous [{MODULE_TYPE.DownloadServer}] initialized [{BIND_IP}:{DW_PORT}]", UTILS.LOG_TYPE.Notify);
                UTILS.WriteLine($"Default asynchronous [{MODULE_TYPE.GatewayServer}] initialized [{BIND_IP}:{GW_PORT}]", UTILS.LOG_TYPE.Notify);
                UTILS.WriteLine($"Default asynchronous [{MODULE_TYPE.AgentServer}] initialized [{BIND_IP}:{AG_PORT}]", UTILS.LOG_TYPE.Notify);
                
            }
            catch (Exception EX) { UTILS.WriteLine($"Cannot initialize asynchronous server, proxy initialization failed. {EX.Message}", UTILS.LOG_TYPE.Fatal); return; }
        }

        public static void InitializeSecondEngine(string BIND_IP, int AG2_PORT, int AG3_PORT, int AG4_PORT)
        {
            CustomSocket AGENT2_LISTENER = new CustomSocket();
            //binding socket for our AgentServer...
            if (Settings.MAIN.checkBox5.Checked)
            {
                AGENT2_LISTENER.Bind(new IPEndPoint(IPAddress.Parse(BIND_IP), AG2_PORT));
                AGENT2_LISTENER.Listen(5);
            }
           
            CustomSocket AGENT3_LISTENER = new CustomSocket();
            //binding socket for our AgentServer...
            if (Settings.MAIN.checkBox6.Checked)
            {
                AGENT3_LISTENER.Bind(new IPEndPoint(IPAddress.Parse(BIND_IP), AG3_PORT));
                AGENT3_LISTENER.Listen(5);
            }

            CustomSocket AGENT4_LISTENER = new CustomSocket();
            //binding socket for our AgentServer...
            if (Settings.MAIN.checkBox27.Checked)
            {
                AGENT4_LISTENER.Bind(new IPEndPoint(IPAddress.Parse(BIND_IP), AG4_PORT));
                AGENT4_LISTENER.Listen(5);
            }

            if (AGENT2_LISTENER.IsBound || AGENT3_LISTENER.IsBound || AGENT4_LISTENER.IsBound)
            {
                Thread Listener2Thread = new Thread(() =>//We are spinning up a dedicated thread for both async server, because tasks will tie up threads in the threadpool...
                {
                    while (ACTIVATION)
                    {
                        if(Settings.MAIN.checkBox5.Checked)
                            AGENT2_LISTENER.AcceptAgent2ServerConnections();
                        if (Settings.MAIN.checkBox6.Checked)
                            AGENT3_LISTENER.AcceptAgent2ServerConnections();
                        if (Settings.MAIN.checkBox27.Checked)
                            AGENT4_LISTENER.AcceptAgent2ServerConnections();
                            ARE2.WaitOne();// Wait until a connection is made before continuing.
                    }
                });
                Listener2Thread.Name = "Listener2Thread";
                Listener2Thread.Start();
            }
            if (Settings.MAIN.checkBox5.Checked)
                UTILS.WriteLine($"Default asynchronous [{MODULE_TYPE.AgentServer}2] initialized [{BIND_IP}:{AG2_PORT}]", UTILS.LOG_TYPE.Notify);
            if (Settings.MAIN.checkBox6.Checked)
                UTILS.WriteLine($"Default asynchronous [{MODULE_TYPE.AgentServer}3] initialized [{BIND_IP}:{AG3_PORT}]", UTILS.LOG_TYPE.Notify);
            if (Settings.MAIN.checkBox27.Checked)
                UTILS.WriteLine($"Default asynchronous [{MODULE_TYPE.AgentServer}4] initialized [{BIND_IP}:{AG4_PORT}]", UTILS.LOG_TYPE.Notify);
        }

        public static async void DISCONNECT(Socket KEY_CLIENT_SOCKET, MODULE_TYPE MT)
        {
            try
            {
                switch (MT)
                {
                    case MODULE_TYPE.DownloadServer:
                        if (DW_CONS.TryRemove(KEY_CLIENT_SOCKET, out DOWNLOAD_MODULE CURRENT_DW_SESSION))
                        {
                            //Disconnecting the current key PROXY socket
                            if (KEY_CLIENT_SOCKET != null && KEY_CLIENT_SOCKET.Connected)
                            {
                                KEY_CLIENT_SOCKET.Shutdown(SocketShutdown.Both);
                                KEY_CLIENT_SOCKET.Close();
                            }
                            //Handling abortive disconnection
                            if (CURRENT_DW_SESSION.PROXY_SOCKET != null && CURRENT_DW_SESSION.PROXY_SOCKET.Connected)
                            {
                                CURRENT_DW_SESSION.PROXY_SOCKET.Shutdown(SocketShutdown.Both);
                                CURRENT_DW_SESSION.PROXY_SOCKET.Close();
                            }
                            //Updating the UI counters
                            Settings.MAIN.DW_DEF_CC.Text = DW_CONS.Count().ToString();
                        }
                        break;
                    case MODULE_TYPE.GatewayServer:
                        if (GW_CONS.TryRemove(KEY_CLIENT_SOCKET, out GATEWAY_MODULE CURRENT_GW_SESSION))
                        {
                            //Disconnecting the current key PROXY socket
                            if (KEY_CLIENT_SOCKET != null && KEY_CLIENT_SOCKET.Connected)
                            {
                                KEY_CLIENT_SOCKET.Shutdown(SocketShutdown.Both);
                                KEY_CLIENT_SOCKET.Close();
                            }
                            //Handling abortive disconnection
                            if (CURRENT_GW_SESSION.PROXY_SOCKET != null && CURRENT_GW_SESSION.PROXY_SOCKET.Connected)
                            {
                                CURRENT_GW_SESSION.PROXY_SOCKET.Shutdown(SocketShutdown.Both);
                                CURRENT_GW_SESSION.PROXY_SOCKET.Close();
                            }
                            //Updating the UI counters
                            Settings.MAIN.GW_DEF_CC.Text = GW_CONS.Count().ToString();
                            // PC limit GatewaySession assign by TokenID
                            if (Settings.SERVER_HWID_LIMIT > 0 && (!string.IsNullOrEmpty(CURRENT_GW_SESSION.HWID) || CURRENT_GW_SESSION.SOCKET_IP.Split(':')[0] == Settings.BIND_IP || CURRENT_GW_SESSION.SOCKET_IP.Split(':')[0] == Settings.PUBLIC_AG_IP))
                                DISPOSED_GW_SESSIONS.TryAdd(KEY_CLIENT_SOCKET, CURRENT_GW_SESSION);
                        }
                        break;
                    case MODULE_TYPE.AgentServer:
                        AGENT_MODULE CURRENT_AG_SESSION = AG_CONS[KEY_CLIENT_SOCKET];
                        if (Settings.MAIN.textBox112.Text.ToLower() == "true")
                        {
                            if (CURRENT_AG_SESSION.RIDING_PET)
                            {
                                CURRENT_AG_SESSION.DISSCONNECT_REQUEST = true;
                                CURRENT_AG_SESSION.DISMOUNT_PET();
                                UTILS.WriteLine($"Detected a pet and has been disbanded. >> {CURRENT_AG_SESSION.CHARNAME16}");
                            }
                        }
                        if (AG_CONS.TryRemove(KEY_CLIENT_SOCKET, out CURRENT_AG_SESSION))
                        {

                            //Disconnecting the current key PROXY socket
                            if (KEY_CLIENT_SOCKET != null && KEY_CLIENT_SOCKET.Connected)
                            {
                                KEY_CLIENT_SOCKET.Shutdown(SocketShutdown.Both);
                                KEY_CLIENT_SOCKET.Close();
                            }
                            //Handling abortive disconnection
                            if (CURRENT_AG_SESSION.PROXY_SOCKET != null && CURRENT_AG_SESSION.PROXY_SOCKET.Connected)
                            {
                                CURRENT_AG_SESSION.PROXY_SOCKET.Shutdown(SocketShutdown.Both);
                                CURRENT_AG_SESSION.PROXY_SOCKET.Close();
                            }
                            //Updating the UI counters
                            Settings.MAIN.AG_DEF_CC.Text = AG_CONS.Count().ToString();
                            //Updating _Players Table
                            if (Settings.LOG_PLAYERS_STATUS)
                            {
                                await QUERIES.UPDATE_PLAYER_STATUS(CURRENT_AG_SESSION.CHARNAME16, false);
                            }
                            foreach (var x in UTILS.SURVregisterActivelist)
                            {
                                if (x.CHARNAME16 == CURRENT_AG_SESSION.CHARNAME16)
                                    UTILS.SURVregisterActivelist.Remove(x);
                            }
                            foreach (var x in UTILS.DRUNKregisterActivelist)
                            {
                                if (x.CHARNAME16 == CURRENT_AG_SESSION.CHARNAME16)
                                    UTILS.DRUNKregisterActivelist.Remove(x);
                            }
                            if (UTILS.Teleport_To_Town.Contains(CURRENT_AG_SESSION.CUR_REGION))
                                await QUERIES.INSERT_INSTANT_TELEPORT(CURRENT_AG_SESSION.CharID, 1, 1, 25000, 982, 140, 140, CURRENT_AG_SESSION.ShardID);
                            if (UTILS.UNIQUEregisterlist.Contains(CURRENT_AG_SESSION.CHARNAME16))
                                UTILS.UNIQUEregisterlist.Remove(CURRENT_AG_SESSION.CHARNAME16);
                            if (UTILS.UNIQUEactivelist.Contains(CURRENT_AG_SESSION.CHARNAME16))
                                UTILS.UNIQUEactivelist.Remove(CURRENT_AG_SESSION.CHARNAME16);
                            if (UTILS.PVPregisterlist.Contains(CURRENT_AG_SESSION.CHARNAME16))
                                UTILS.PVPregisterlist.Remove(CURRENT_AG_SESSION.CHARNAME16);
                            if (UTILS.PVPactivelist.Contains(CURRENT_AG_SESSION.CHARNAME16))
                                UTILS.PVPactivelist.Remove(CURRENT_AG_SESSION.CHARNAME16);
                            if (UTILS.JobFightEvent.Contains(CURRENT_AG_SESSION.CHARNAME16))
                                UTILS.JobFightEvent.Remove(CURRENT_AG_SESSION.CHARNAME16);
                            if (UTILS.GuildWarEvent.ContainsKey(CURRENT_AG_SESSION.GUILDNAME))
                                if (UTILS.GuildWarEvent[CURRENT_AG_SESSION.GUILDNAME].Contains(CURRENT_AG_SESSION.CHARNAME16))
                                    UTILS.GuildWarEvent[CURRENT_AG_SESSION.GUILDNAME].Remove(CURRENT_AG_SESSION.CHARNAME16);
                        }
                        break;
                }
            }
            catch (Exception EX) { UTILS.ExportLog($"DISCONNECT() failed: {EX}", EX); }
        }
        #endregion
    }
}
