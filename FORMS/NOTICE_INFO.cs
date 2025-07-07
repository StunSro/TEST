using SR_PROXY.ENGINES;
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
    public partial class NOTICE_INFO : Form
    {
        NoticeModel Notice = null;
        public NOTICE_INFO()
        {
            InitializeComponent();
        }
        public NOTICE_INFO(NoticeModel Notice)
        {
            this.Notice = Notice;
            InitializeComponent();
        }

        private async void Save_Click(object sender, EventArgs e)
        {
            if (Notice==null)
            {
                if ((bool)await QUERIES.Add_Notice(new NoticeModel(-1, dateTimePicker1.Value, dateTimePicker2.Value, richTextBox1.Text,comboBox3.SelectedItem.ToString())))
                    UTILS.WriteLine($"Notice has been added successfully.", UTILS.LOG_TYPE.Notify);
                else
                    UTILS.WriteLine($"Notice has been failed to be added.", UTILS.LOG_TYPE.Fatal);
            }
            else {
                if ((bool)await QUERIES.Update_Notice(new NoticeModel(Notice.ID, dateTimePicker1.Value, dateTimePicker2.Value, richTextBox1.Text, comboBox3.SelectedItem.ToString())))
                    UTILS.WriteLine($"Notice has been modifiend successfully", UTILS.LOG_TYPE.Notify);
                else
                    UTILS.WriteLine($"Notice has been failed to be modified", UTILS.LOG_TYPE.Fatal);
            }
            Close();


        }

        private void NOTICE_INFO_Load(object sender, EventArgs e)
        {
            if (Notice != null)
            {
                dateTimePicker1.Value = Notice.StartDateTime;
                dateTimePicker2.Value = Notice.EndDateTime;
                richTextBox1.Text = Notice.Content;
                comboBox3.SelectedItem = Notice.Color;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure that you want to delete it?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
            {
                if(Notice.ID>=0)
                {
                    await QUERIES.Delete_Notice(Notice.ID);
                    MessageBox.Show("Successfully deleted!");
                }
                
            }
            Close();
        }
    }
}
