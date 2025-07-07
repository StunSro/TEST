using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class CharTitleInfoModel
    {
        public CharTitleInfoModel(int iD, string charName, string title)
        {
            ID = iD;
            CharName = charName;
            Title = title;
        }

        public int ID { get; set; }
        public string CharName { get; set; }
        public string Title { get; set; }
    }
}
