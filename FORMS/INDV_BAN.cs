using SR_PROXY.ENGINES;
using System;
using System.Windows.Forms;

namespace SR_PROXY.FORMS
{
    public partial class INDV_BAN : Form
    {
        public INDV_BAN()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                UTILS.BLOCK_IP(textBox1.Text);
                UTILS.WriteLine($"[{textBox1.Text}] has been manually banned succesfully.", UTILS.LOG_TYPE.Notify);
            }
            catch { }
        }
    }
}
