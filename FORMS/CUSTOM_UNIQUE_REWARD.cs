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

namespace SR_PROXY.FORMS
{
    public partial class CUSTOM_UNIQUE_REWARD : Form
    {
        public CUSTOM_UNIQUE_REWARD()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
            }
            catch { UTILS.WriteLine("Invalid input, please try again.", UTILS.LOG_TYPE.Warning); Close(); }
        }
    }
}
