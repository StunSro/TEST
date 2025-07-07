using Framework;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace SR_PROXY.SR_MODULE
{
    public class SR_MODULE
    {
        public static Security module_security;
        public static Socket module_socket;
        public static byte[] module_buffer = new byte[4096];

        #region CLIENT_CON
        public void Start(string IP, int Port)
        {
            try
            {
                module_security = new Security();
                module_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                module_socket.BeginConnect(IP, Port, result => { module_socket.EndConnect(result); if (module_socket.Connected) module_socket.BeginReceive(module_buffer, 0, module_buffer.Length, SocketFlags.None, new AsyncCallback(AsyncRecvFromServerAsync), module_socket); }, module_socket);
            }
            catch { }
        }
        #endregion

        #region CLIENT_RECV
        void AsyncRecvFromServerAsync(IAsyncResult result)
        {
            try
            {

                int Data = module_socket.EndReceive(result);
                if (Data != 0)
                {
                    module_security.Recv(module_buffer, 0, Data);
                    List<Packet> RemotePackets = module_security.TransferIncoming();
                    if (RemotePackets != null)
                    {
                        foreach (Packet packet in RemotePackets)
                        {
                            byte[] packet_bytes = packet.GetBytes();
                            Settings.MAIN.richTextBox2.BeginInvoke((MethodInvoker)delegate { Settings.MAIN.richTextBox2.AppendText(string.Format("[S=>C][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet_bytes.Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet_bytes), Environment.NewLine)); });
                            #region SERVER_GLOBAL_HANDSHAKE - SR_CLIENT CORE
                            if (packet.Opcode == 0x5000 || packet.Opcode == 0x9000)
                            {
                                AsyncSendToServer();
                                continue;
                            }
                            #endregion
                            #region X_GLOBAL_IDENTIFICATION - SR_CLIENT CORE
                            if (packet.Opcode == 0x2001)
                            {
                                Settings.MAIN.richTextBox2.AppendText(string.Format("\nSECURITY MODULE ID VERIFIED [{0}]", packet.ReadAscii()));
                                if (Settings.MAIN.checkBox22.Checked)
                                {
                                
                                    while (true) {
                                        Packet p = new Packet(ushort.Parse(Settings.MAIN.textBox157.Text, System.Globalization.NumberStyles.HexNumber));
                                        module_security.Send(p);
                                        AsyncSendToServer();
                                        Thread.Sleep(1);
                                    }
                  
                                    
                                }
                                else {
                                    Packet p = new Packet(ushort.Parse(Settings.MAIN.textBox157.Text, System.Globalization.NumberStyles.HexNumber));
                                    module_security.Send(p);
                                    p.WriteAscii("sadsaddddddddddsadddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsadddddddddddddsaddddddddddsadddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddddddddddddsadddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddsaddddddddddddd");
                                    AsyncSendToServer();
                                }
                            }
                            #endregion
                        }
                    }
                    module_socket.BeginReceive(module_buffer, 0, module_buffer.Length, SocketFlags.None, new AsyncCallback(AsyncRecvFromServerAsync), module_socket);
                }
            }
            catch { }
        }
        #endregion

        #region CLIENT_SEND
        public static void AsyncSendToServer()
        {
            try
            {
                List<KeyValuePair<TransferBuffer, Packet>> List = module_security.TransferOutgoing();
                if (List != null)
                {
                    foreach (var kvp in List)
                    {
                        module_socket.BeginSend(kvp.Key.Buffer, 0, kvp.Key.Size, SocketFlags.None, result => { module_socket.EndSend(result); }, module_socket);
                        byte[] packet_bytes = kvp.Value.GetBytes();
                        Settings.MAIN.richTextBox2.BeginInvoke((MethodInvoker)delegate { Settings.MAIN.richTextBox2.AppendText(string.Format("[C=>S][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", kvp.Value.Opcode, packet_bytes.Length, kvp.Value.Encrypted ? "[Encrypted]" : "", kvp.Value.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet_bytes), Environment.NewLine)); Settings.MAIN.richTextBox2.ScrollToCaret(); });
                    }
                }
            }
            catch { }
        }
        #endregion

        #region CLIENT_DC
        public static void DC_SR_CLIENT_SOCKET()
        {
            try { module_socket.BeginDisconnect(true, result => { module_socket.EndDisconnect(result); }, module_socket); } catch { }
        }
        #endregion
    }
}
