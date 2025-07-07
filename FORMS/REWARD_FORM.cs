using SR_PROXY.CORE;
using SR_PROXY.ENGINES;
using SR_PROXY.SECURITYOBJECTS;
using SR_PROXY.SQL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SR_PROXY
{
    public partial class REWARD_FORM : Form
    {

        public REWARD_FORM()
        {
            InitializeComponent();
            RW_TYPE.Items.Add("Silk");
            RW_TYPE.Items.Add("Gold");
            RW_TYPE.Items.Add("SP");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (OnlineUser.Checked)
                {
                    //var temp = ASYNC_SERVER.AG_CONS;

                    List<System.Collections.Generic.KeyValuePair<Socket, AGENT_MODULE>> temp = ASYNC_SERVER.AG_CONS.ToList();
                    if (ip.Checked)
                    {
                        temp = ASYNC_SERVER.AG_CONS
                         .GroupBy(p => p.Value.IP)
                         .Select(g => g.First())
                         .ToList();
                    }
                    else if (mac.Checked)
                    {
                        temp = ASYNC_SERVER.AG_CONS
                          .GroupBy(p => p.Value.CORRESPONDING_GW_SESSION.HWID)
                              .Select(g => g.First())
                              .ToList();
                    }
                    foreach (var item in temp)
                    {
                        string target = item.Value.CHARNAME16;
                        if (String.Empty.Equals(target))
                            continue;
                        int ShardID = item.Value.ShardID;
                        if ((string)(RW_TYPE.SelectedItem) == "Silk")
                        {
                            if (!string.IsNullOrWhiteSpace(target))
                            {

                                await QUERIES.UpdateSilk(target, ShardID, 1, int.Parse(RW_AMOUNT.Text));
                                int[] Silk = await QUERIES.Get_TOT_SILK_BALANCE(target, ShardID);
                                UTILS.SEND_INDV_SILK_UPDATE(Silk, item.Key);
                                UTILS.SEND_INDV_NOTICE($"Congratulations on getting[{RW_AMOUNT.Text}]Silk", item.Key);
                            }
                        }
                        if ((string)(RW_TYPE.SelectedItem) == "Gold")
                        {
                            if (!string.IsNullOrWhiteSpace(target))
                            {
                                int result = await QUERIES.GIVE_GOLD(target, int.Parse(RW_AMOUNT.Text), ShardID);
                                long gb = await QUERIES.GOLD_BALANCE(target, ShardID);

                                //UTILS.SEND_INDV_SLB_UPDATE_NOTICE(int.Parse(RW_AMOUNT.Text), item.Key);
                                UTILS.SEND_INDV_SLB_UPDATE(gb, item.Key);
                                if (result != 0)
                                {
                                    UTILS.SEND_INDV_NOTICE($"Congratulations on getting[{RW_AMOUNT.Text}]Gold", item.Key);
                                }
                            }
                        }
                        if ((string)(RW_TYPE.SelectedItem) == "SP")
                        {
                            if (!string.IsNullOrWhiteSpace(target))
                            {
                                int result = await QUERIES.GIVE_SP(target, int.Parse(RW_AMOUNT.Text), ShardID);
                                int spb = await QUERIES.SP_BALANCE(target, ShardID);
                                if (result != 0)
                                {
                                    UTILS.SEND_INDV_NOTICE($"Congratulations on getting[{RW_AMOUNT.Text}]SP", item.Key);
                                }
                            }
                        }
                    }
                    MessageBox.Show("Sent successfully", "Msg", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    string target = Settings.MAIN.CM_LW.SelectedItems[0].SubItems[4].Text;
                    if (String.Empty.Equals(target))
                        return;
                    int ShardID = int.Parse(Settings.MAIN.CM_LW.SelectedItems[0].SubItems[5].Text.Split('|')[0]);
                    if (string.IsNullOrWhiteSpace(target))
                    {
                        MessageBox.Show("No User Selected!", "Msg", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    if ((string)(RW_TYPE.SelectedItem) == "Silk")
                    {
                        if (!string.IsNullOrWhiteSpace(target))
                        {

                            string SockIP = Settings.MAIN.CM_LW.SelectedItems[0].SubItems[1].Text;
                            var keysWithMatchingValues = ASYNC_SERVER.AG_CONS.Where(p => p.Value.SOCKET_IP == SockIP).Select(p => p.Key);

                            await QUERIES.UpdateSilk(target, ShardID, 1, int.Parse(RW_AMOUNT.Text));
                            int[] Silk = await QUERIES.Get_TOT_SILK_BALANCE(target, ShardID);
                            foreach (var key in keysWithMatchingValues)//matching socket Key
                            {
                                UTILS.SEND_INDV_NOTICE($"Congratulations on getting[{RW_AMOUNT.Text}]Silk",key);
                                UTILS.SEND_INDV_SILK_UPDATE(Silk, key);

                            }
                            MessageBox.Show("Result【Success】,[" + target + "]Silk:[" + Silk[0] + "]", "Msg", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        }
                    }
                    if ((string)(RW_TYPE.SelectedItem) == "Gold")
                    {
                        if (!string.IsNullOrWhiteSpace(target))
                        {
                            int result = await QUERIES.GIVE_GOLD(target, int.Parse(RW_AMOUNT.Text), ShardID);
                            long gb = await QUERIES.GOLD_BALANCE(target, ShardID);

                            //todo UTILS.AddSLB(target, int.Parse(RW_AMOUNT.Text), ShardID);

                            string SockIP = Settings.MAIN.CM_LW.SelectedItems[0].SubItems[1].Text;
                            var keysWithMatchingValues = ASYNC_SERVER.AG_CONS.Where(p => p.Value.SOCKET_IP == SockIP).Select(p => p.Key);
                            foreach (var key in keysWithMatchingValues)//matching socket Key
                            {
                                //UTILS.SEND_INDV_SLB_UPDATE_NOTICE(int.Parse(RW_AMOUNT.Text), key);
                                UTILS.SEND_INDV_SLB_UPDATE(gb, key);
                                if (result != 0)
                                {
                                    UTILS.SEND_INDV_NOTICE($"Congratulations on getting[{RW_AMOUNT.Text}]Gold", key);
                                }
                            }
                            MessageBox.Show("Result【Success】! [" + target + "]current gold:[" + gb + "]", "Msg", MessageBoxButtons.OK, MessageBoxIcon.Warning);


                        }
                    }
                    if ((string)(RW_TYPE.SelectedItem) == "SP")
                    {
                        if (!string.IsNullOrWhiteSpace(target))
                        {
                            int result = await QUERIES.GIVE_SP(target, int.Parse(RW_AMOUNT.Text), ShardID);
                            int spb = await QUERIES.SP_BALANCE(target, ShardID);
                            if (result != 0)
                            {
                                string SockIP = Settings.MAIN.CM_LW.SelectedItems[0].SubItems[1].Text;
                                var keysWithMatchingValues = ASYNC_SERVER.AG_CONS.Where(p => p.Value.SOCKET_IP == SockIP).Select(p => p.Key);
                                foreach (var key in keysWithMatchingValues)//matching socket Key
                                {
                                    UTILS.SEND_INDV_NOTICE($"Congratulations on getting[{RW_AMOUNT.Text}]SP", key);

                                }
                                MessageBox.Show("Result【Success】! [" + target + "] SP:[" + spb + "]", "Msg", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            }
                        }
                    }
                }

            }
            catch { }
        }

        private void RW_AMOUNT_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar) || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        private void OnlineUser_CheckedChanged(object sender, EventArgs e)
        {
            groupBox1.Enabled = (sender as CheckBox).Checked;
        }

        private void REWARD_FORM_Load(object sender, EventArgs e)
        {

        }
    }
}
