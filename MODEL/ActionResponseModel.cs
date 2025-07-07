using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    class ActionResponseModel
    {
        public string DstCharName="";//目标名称
        public string DstNoticeMessage = "";//通知目标的消息
        public string SrcNoticeMessage = "";//通知触发者的消息
        public string AllNoticeMessage = "";//通知触发者的消息
        public int SrcAddSLB=0;//触发者SLB改变
        public int DstAddSLB=0;//目标SLB改变
        public int ExpelType=0;//踢出游戏类型 0不触发 1踢出触发者 2踢出目标 3 1|2
        public int SilkChangeType=0;//金珠改变类型 0不更新 1更新触发者 2更新目标 3 1|2

        public ActionResponseModel() {
        }

        public ActionResponseModel(string dstCharName, string dstNoticeMessage, string srcNoticeMessage, string allNoticeMessage, int srcAddSLB, int dstAddSLB, int expelType, int silkChangeType)
        {
            DstCharName = dstCharName;
            DstNoticeMessage = dstNoticeMessage;
            SrcNoticeMessage = srcNoticeMessage;
            AllNoticeMessage = allNoticeMessage;
            SrcAddSLB = srcAddSLB;
            DstAddSLB = dstAddSLB;
            ExpelType = expelType;
            SilkChangeType = silkChangeType;
        }
    }
}
