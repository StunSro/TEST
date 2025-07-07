using Framework;
using SR_PROXY.CORE;
using System.IO;
using System.Net;
using System.Net.Sockets;


namespace SR_PROXY.ENGINES
{
    sealed class GM_PRIVG
    {
        public static string[] m_Lines;
        public struct CommandRules
        {
            public string AllowedIP;
            public int VerifyIPCheck;
            public string AllowedCHARNAME16;
            public int PinkNotice;
            public int invisible;
            public int invincible;
            public int Loadmonster;
            public int Zoes;
            public int mobkills;
            public int makeitem;
            public int recalluser;
            public int moveotuser;
        }

        public static bool GM_CMD_VERIFY(Socket cs, Socket ps, ASYNC_SERVER.MODULE_TYPE HT, Packet packet)
        {
            bool BLOCK_PACKET = false;
            m_Lines = File.ReadAllLines(".\\Privileged_GMs.ini");

            string[] index;
            for (int i = 0; i < m_Lines.Length; i++)
            {
                if (m_Lines[i] == "[GM]")
                {
                    //reading each packet structure of parameters
                    index = m_Lines[i + 1].Split(new char[] { '=' });
                    string AllowedIP = index[1];
                    index = m_Lines[i + 2].Split(new char[] { '=' });
                    int verifyipcheck = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                    index = m_Lines[i + 3].Split(new char[] { '=' });
                    string AllowedCHARNAME16 = index[1];
                    index = m_Lines[i + 4].Split(new char[] { '=' });
                    int PinkNotice = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                    index = m_Lines[i + 5].Split(new char[] { '=' });
                    int invisible = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                    index = m_Lines[i + 6].Split(new char[] { '=' });
                    int invincible = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                    index = m_Lines[i + 7].Split(new char[] { '=' });
                    int Loadmonster = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                    index = m_Lines[i + 8].Split(new char[] { '=' });
                    int Zoes = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                    index = m_Lines[i + 9].Split(new char[] { '=' });
                    int mobkills = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                    index = m_Lines[i + 10].Split(new char[] { '=' });
                    int makeitem = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                    index = m_Lines[i + 11].Split(new char[] { '=' });
                    int recalluser = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                    index = m_Lines[i + 12].Split(new char[] { '=' });
                    int moveotuser = int.Parse(index[1], System.Globalization.NumberStyles.Integer);
                    //allocating a new rule for each
                    CommandRules CR = new CommandRules();
                    //initlizaing obtained parameters
                    CR.AllowedIP = AllowedIP;
                    CR.VerifyIPCheck = verifyipcheck;
                    CR.AllowedCHARNAME16 = AllowedCHARNAME16;
                    CR.PinkNotice = PinkNotice;
                    CR.invisible = invisible;
                    CR.invincible = invincible;
                    CR.Loadmonster = Loadmonster;
                    CR.Zoes = Zoes;
                    CR.mobkills = mobkills;
                    CR.makeitem = makeitem;
                    CR.recalluser = recalluser;
                    CR.moveotuser = moveotuser;

                   // UTILS.WriteLine(CR.AllowedCHARNAME16, ASYNC_SERVER.AG_CONS[cs].CHARNAME16);
                    if (CR.AllowedCHARNAME16 == ASYNC_SERVER.AG_CONS[cs].CHARNAME16)
                    {
                        if (packet.Opcode == 0x7025)
                        {
                            if (CR.PinkNotice == 0)
                            {
                                UTILS.SEND_INDV_NOTICE("You are not privileged to use this command.", cs);
                                BLOCK_PACKET = true;
                            }
                        }
                        if (packet.Opcode == 0x7010)
                        {
                            byte id = packet.ReadUInt8();
                            if (CR.VerifyIPCheck == 1 && CR.AllowedIP != ((IPEndPoint)(cs.RemoteEndPoint)).Address.ToString())
                            {
                                UTILS.SEND_INDV_NOTICE("You are not a privileged [GM], a message has been sent to notify the server owner, bye bye!", cs);
                                ASYNC_SERVER.DISCONNECT(cs, ASYNC_SERVER.MODULE_TYPE.AgentServer);
                                BLOCK_PACKET = true;
                            }
                            if (packet.GetBytes().Length == 2 && id == 0x0E && CR.invisible == 0) // invisible cmd
                            {
                                UTILS.SEND_INDV_NOTICE("You are not privileged to use this command.", cs);
                                BLOCK_PACKET = true;
                            }
                            if (packet.GetBytes().Length == 2 && id == 0x0F && CR.invincible == 0) // invincible cmd
                            {
                                UTILS.SEND_INDV_NOTICE("You are not privileged to use this command.", cs);
                                BLOCK_PACKET = true;
                            }
                            if (id == 0x06 && CR.Loadmonster == 0) // Loadmonster cmd
                            {
                                UTILS.SEND_INDV_NOTICE("You are not privileged to use this command.", cs);
                                BLOCK_PACKET = true;
                            }
                            if (id == 0x0C && CR.Zoes == 0) // Zoes cmd
                            {
                                UTILS.SEND_INDV_NOTICE("You are not privileged to use this command.", cs);
                                BLOCK_PACKET = true;
                            }
                            if (id == 0x14 && CR.mobkills == 0) // mobkills cmd
                            {
                                UTILS.SEND_INDV_NOTICE("You are not privileged to use this command.", cs);
                                BLOCK_PACKET = true;
                            }
                            if (id == 0x07 && CR.makeitem == 0) // makeitem cmd
                            {
                                UTILS.SEND_INDV_NOTICE("You are not privileged to use this command.", cs);
                                BLOCK_PACKET = true;
                            }
                            if (id == 0x08 && CR.moveotuser == 0) // movetouser cmd
                            {
                                UTILS.SEND_INDV_NOTICE("You are not privileged to use this command.", cs);
                                BLOCK_PACKET = true;
                            }
                            if (id == 0x11 && CR.recalluser == 0) // recalluser cmd
                            {
                                UTILS.SEND_INDV_NOTICE("You are not privileged to use this command.", cs);
                                BLOCK_PACKET = true;
                            }






                        }
                    }
                }
            }
            return BLOCK_PACKET;
        }
    }
}
