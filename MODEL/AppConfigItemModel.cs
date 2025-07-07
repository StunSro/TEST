using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class AppConfigItemModel
    {
        public bool Service;
        public string Name;
        public string Value;

        public AppConfigItemModel(bool service, string name, string value)
        {
            Service = service;
            Name = name;
            Value = value;
        }
    }
}
