using System;
using Framework;
using SR_PROXY;
using System.Diagnostics;
using System.Collections.Generic;
using SR_PROXY.ENGINES;
using System.Collections.Concurrent;

namespace SR_CLIENT
{
    public class Client
    {

        public static System.Timers.Timer ping_timer;
        public static ConcurrentDictionary<string, Gateway> ClientList = new ConcurrentDictionary<string, Gateway>();
        public static int MinionsNumber;
        static Client(){

        }
        public static void SR_CLIENT_Close(string _username,bool _isrelogin=true)
        {
            try
            {
                if (ClientList.ContainsKey(_username))
                {

                    if (ClientList[_username] != null)
                        ClientList[_username].CLIENTLESS_DISCONNECT();
                    if (ClientList[_username].m_Agent != null)
                        ClientList[_username].m_Agent.CLIENTLESS_DISCONNECT();
                    var 密码 = ClientList[_username]._password;
                    var 角色名 = ClientList[_username]._charname;
                    Gateway ignored;
                    ClientList.TryRemove(_username, out ignored);
                    //if (_isrelogin)
                    //{
                    //    ClientList.TryAdd(_username, new Gateway(Settings.BIND_IP, Settings.PUBLIC_GW_PORT,_username, 密码, 角色名));
                    //}
                }
            }
            catch (Exception Ex) {
                UTILS.WriteLine($" SR_CLIENT_Close失败:{Ex.ToString()}", UTILS.LOG_TYPE.Fatal);
            }
        }
        public static void Close_All()
        {
            UTILS.WriteLine($"Closing [{ClientList.Count}] Online Accounts", UTILS.LOG_TYPE.Notify);
            foreach (var item in ClientList)
            {
                SR_CLIENT_Close(item.Value._username,false);
            }
        }

    }
}