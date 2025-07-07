using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class DataItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Md5 { get; set; }
    }

    public class SqlInfoModel
    {
        /// <summary>
        /// 
        /// </summary>
        public List<DataItem> Data { get; set; }
    }
}
