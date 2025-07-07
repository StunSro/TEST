using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SRO_PROXY.GS
{
    class UniqueDamageInfo
    {
        public readonly int RefObjID;

        //player gid : damage
        private readonly Dictionary<uint, int> _damageInfos;

        public UniqueDamageInfo(int refObjID)
        {
            this.RefObjID = refObjID;
            _damageInfos = new Dictionary<uint, int>();
        }

        public void AddRecord(uint gid, int dmg) => _damageInfos[gid] = dmg;

        public Dictionary<uint, int> GetSorted(int maxRecords)
        {
            IOrderedEnumerable<KeyValuePair<uint, int>> sorted =
                _damageInfos.OrderByDescending(kvp => kvp.Value);

            //Only take maximum of defined records. The rest wont 
            //be displayed at client, so no need to return more.
            int count = (sorted.Count() > maxRecords) ?
                maxRecords : sorted.Count();

            Dictionary<uint, int> sortedDict = sorted.ToDictionary(
                kvp => kvp.Key, kvp => kvp.Value
                );

            return sortedDict.Take(count).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }

    class GameServerMsgHandler
    {

        public void HandleUniqueMonsterDamageInfo(Packet msg,int ShardID)
        {
            int MobID = msg.ReadInt32();
            int recordCount = msg.ReadInt32();

            var info = new UniqueDamageInfo(MobID);

            for (int i = 0; i < recordCount; i++)
            {
                uint gid = msg.ReadUInt32();
                int dmg = msg.ReadInt32();

                info.AddRecord(gid, dmg);
            }

            GameServerManager.HandleUniqueDamageInfo(info);
            GameServerManager.Start(ShardID);
        }
    }
}
