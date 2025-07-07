using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class NoticeModel
    {
        public NoticeModel(int ID, DateTime StartDateTime, DateTime EndDateTime, string Content,string Color)
        {
            this.ID = ID;
            this.StartDateTime = StartDateTime;
            this.EndDateTime = EndDateTime;
            this.Content = Content;
            this.Color = Color;
        }

        public int ID { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Content { get; set; }
        public string Color { get; set; }

    }
}
