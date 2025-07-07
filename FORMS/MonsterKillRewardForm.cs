using SR_PROXY.MODEL;
using SR_PROXY.SQL;
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
    public partial class MonsterKillRewardForm : Form
    {
        MonsterKillRewardModel MonsterKillReward=null;
        public MonsterKillRewardForm()
        {
            InitializeComponent();
        }
        public MonsterKillRewardForm(MonsterKillRewardModel MonsterKillReward)
        {
            this.MonsterKillReward = MonsterKillReward;
            InitializeComponent();
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar) || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        private void MonsterKillRewardForm_Load(object sender, EventArgs e)
        {
            if (MonsterKillReward != null) {
                textBox1.Text = MonsterKillReward.ID.ToString();
                textBox2.Text = MonsterKillReward.SilkOwnReward.ToString();
                textBox3.Text = MonsterKillReward.SLBReward.ToString();
                textBox4.Text = MonsterKillReward.NoticeType.ToString();
                textBox5.Text = MonsterKillReward.NoticeMessage;
                textBox6.Text = MonsterKillReward.RewardProbability.ToString();
                textBox7.Text = MonsterKillReward.Name.ToString();
                checkBox1.Checked = MonsterKillReward.Service == 1;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar) || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar) || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar) || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar) || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        private async void Save_Click(object sender, EventArgs e)
        {
            bool IsUpdate = MonsterKillReward!=null;
            if (MonsterKillReward == null)
                MonsterKillReward = new MonsterKillRewardModel();
            MonsterKillReward.ID = int.Parse(textBox1.Text.ToString());
            MonsterKillReward.SilkOwnReward = int.Parse(textBox2.Text.ToString());
            MonsterKillReward.SLBReward = int.Parse(textBox3.Text.ToString());
            MonsterKillReward.NoticeType = int.Parse(textBox4.Text.ToString());
            MonsterKillReward.NoticeMessage = textBox5.Text.ToString();
            MonsterKillReward.RewardProbability = int.Parse(textBox6.Text.ToString());
            MonsterKillReward.Name = textBox7.Text.ToString();
            MonsterKillReward.Service = checkBox1.Checked ? 1 : 0;
            if (IsUpdate)
                await QUERIES.Update_MonsterKillReward(MonsterKillReward);
            else
                await QUERIES.Add_MonsterKillReward(MonsterKillReward);

            Close();
        }
    }
}
