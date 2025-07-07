using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SR_PROXY.ENGINES;
using SR_PROXY.MODEL;
using SR_PROXY.SQL;
namespace SR_PROXY.FORMS
{
    public partial class DISABLE_CUSTOM_REGION : Form
    {
        public DISABLE_CUSTOM_REGION()
        {
            InitializeComponent();
        }

        private void PNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar) || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        private async void Save_ClickAsync(object sender, EventArgs e)
        {
            if(UTILS.ListType==1)
            {
                if (!UTILS.Region_Restrection.ContainsKey(Convert.ToInt32(ActionName.Text)))
                    UTILS.Region_Restrection.Add(Convert.ToInt32(ActionName.Text), 123);
                string createText = Environment.NewLine + $"123\t{Convert.ToInt32(ActionName.Text)}";
                File.AppendAllText(Environment.CurrentDirectory + "/Features/REGION_RESCTRECTION.ini", createText);
            }
            else if(UTILS.ListType == 2)
            {
                if (!UTILS.Teleport_To_Town.Contains(Convert.ToInt32(ActionName.Text)))
                    UTILS.Teleport_To_Town.Add(Convert.ToInt32(ActionName.Text));
                string createText = Environment.NewLine + $"123\t{Convert.ToInt32(ActionName.Text)}";
                File.AppendAllText(Environment.CurrentDirectory + "/Features/TELEPORT_TO_TOWN_REGIONS.ini", createText);
            }
            else if (UTILS.ListType == 3)
            {
                if (!UTILS.DISABLE_ZERK_REGIONS.Contains(Convert.ToInt32(ActionName.Text)))
                    UTILS.DISABLE_ZERK_REGIONS.Add(Convert.ToInt32(ActionName.Text));
                string createText = Environment.NewLine + $"{Convert.ToInt32(ActionName.Text)}";
                File.AppendAllText(Environment.CurrentDirectory + "/Features/DISABLE_ZERK_REGIONS.ini", createText);
            }
            else if (UTILS.ListType == 4)
            {
                if (!UTILS.DISABLE_CHAT_REGIONS.Contains(Convert.ToInt32(ActionName.Text)))
                    UTILS.DISABLE_CHAT_REGIONS.Add(Convert.ToInt32(ActionName.Text));
                string createText = Environment.NewLine + $"{Convert.ToInt32(ActionName.Text)}";
                File.AppendAllText(Environment.CurrentDirectory + "/Features/DISABLE_CHAT_REGIONS.ini", createText);
            }
            else if (UTILS.ListType == 5)
            {
                if (!UTILS.DISABLE_PARTY_REGIONS.Contains(Convert.ToInt32(ActionName.Text)))
                    UTILS.DISABLE_PARTY_REGIONS.Add(Convert.ToInt32(ActionName.Text));
                string createText = Environment.NewLine + $"{Convert.ToInt32(ActionName.Text)}";
                File.AppendAllText(Environment.CurrentDirectory + "/Features/DISABLE_PARTY_REGIONS.ini", createText);
            }
            else if (UTILS.ListType == 6)
            {
                if (!UTILS.DISABLE_TRACE_REGIONS.Contains(Convert.ToInt32(ActionName.Text)))
                    UTILS.DISABLE_TRACE_REGIONS.Add(Convert.ToInt32(ActionName.Text));
                string createText = Environment.NewLine + $"{Convert.ToInt32(ActionName.Text)}";
                File.AppendAllText(Environment.CurrentDirectory + "/Features/DISABLE_TRACE_REGIONS.ini", createText);
            }
            else if (UTILS.ListType == 7)
            {
                if (!UTILS.DISABLE_INVITEFRIENDS_REGIONS.Contains(Convert.ToInt32(ActionName.Text)))
                    UTILS.DISABLE_INVITEFRIENDS_REGIONS.Add(Convert.ToInt32(ActionName.Text));
                string createText = Environment.NewLine + $"{Convert.ToInt32(ActionName.Text)}";
                File.AppendAllText(Environment.CurrentDirectory + "/Features/DISABLE_INVITEFRIENDS_REGIONS.ini", createText);
            }
            else if (UTILS.ListType == 8)
            {
                if (!UTILS.DISABLE_ITEMS_REGIONS.ContainsKey(Convert.ToInt32(ActionName.Text)))
                    UTILS.DISABLE_ITEMS_REGIONS.Add(Convert.ToInt32(ActionName.Text), new List<int>());
                UTILS.DISABLE_ITEMS_REGIONS[Convert.ToInt32(ActionName.Text)].Add(Convert.ToInt32(textBox1.Text));
                string createText = Environment.NewLine + $"{Convert.ToInt32(ActionName.Text)}\t{Convert.ToInt32(textBox1.Text)}";
                File.AppendAllText(Environment.CurrentDirectory + "/Features/DISABLE_ITEMS_REGIONS.ini", createText);
            }
            else if (UTILS.ListType == 9)
            {
                if (!UTILS.DISABLE_SKILLS_REGIONS.ContainsKey(Convert.ToInt32(ActionName.Text)))
                    UTILS.DISABLE_SKILLS_REGIONS.Add(Convert.ToInt32(ActionName.Text), new List<int>());
                UTILS.DISABLE_SKILLS_REGIONS[Convert.ToInt32(ActionName.Text)].Add(Convert.ToInt32(textBox1.Text));
                string createText = Environment.NewLine + $"{Convert.ToInt32(ActionName.Text)}\t{Convert.ToInt32(textBox1.Text)}";
                File.AppendAllText(Environment.CurrentDirectory + "/Features/DISABLE_SKILLS_REGIONS.ini", createText);
            }
            
            Close();
        }



        private void CustomActionForm_Load(object sender, EventArgs e)
        {
            if(UTILS.ListType==8)
            {
                label2.Text = "ItemID";
            }
            else if(UTILS.ListType ==9)
            {
                label2.Text = "SkillID";
            }
            else
            {
                label2.Enabled = false;
                textBox1.Enabled = false;
            }
        }
        private async void LoadSpList() {
           
        }
    }
}
