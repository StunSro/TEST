using SR_PROXY.CORE;
using SR_PROXY.ENGINES;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SR_PROXY
{
    public partial class INDV_NOTICE : Form
    {
        public INDV_NOTICE()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string SockIP = Settings.MAIN.CM_LW.SelectedItems[0].SubItems[1].Text;
                var keysWithMatchingValues = ASYNC_SERVER.AG_CONS.Where(p => p.Value.SOCKET_IP == SockIP).Select(p => p.Key);
                foreach (var key in keysWithMatchingValues)//matching socket Key
                {
                    UTILS.SEND_INDV_NOTICE(textBox1.Text, key);
                }
                this.Close();
            }
            catch { }

        }

        private void INDV_NOTICE_Load(object sender, EventArgs e)
        {

        }
    }
}
