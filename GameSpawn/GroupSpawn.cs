using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.GameSpawn
{
    class GroupSpawn
    {
        private Packet GroupSpawnPacket;
        public byte action = 0;
        public ushort count = 0;

        public void GroupSpawnBegin(Packet packet)
        {
            GroupSpawnPacket = new Packet(0x3019);
            action = packet.ReadUInt8();
            count = packet.ReadUInt16();
        }
        public void Manager(Packet packet)
        {
            for (int i = 0; i < packet.GetBytes().Length; i++)
            {
                GroupSpawnPacket.WriteUInt8(packet.ReadUInt8());
            }
        }
        public List<string> GroupeSpawned()
        {
            return SpawnManager.Parse(GroupSpawnPacket,count,action);
        }
    }
}
