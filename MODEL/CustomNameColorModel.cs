using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    class CustomNameColorModel
    {
        public CustomNameColorModel(int iD, string color, string name)
        {
            ID = iD;
            Color = color;
            Name = name;
        }

        public int ID { get; set; }
        public string Color { get; set; }
        public string Name { get; set; }
    }
}
