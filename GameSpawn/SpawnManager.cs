using Framework;
using SR_PROXY.ENGINES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.GameSpawn
{
    class SpawnManager
    {
        public static List<string> Parse(Packet packet, ushort count = 1, byte action = 1)
        {
            packet.Lock();
            List<string> Names = new List<string>() ;
            #region Info Of further actions
            #endregion
            if (action == 1) //Spawn
            {
                for (int i = 0; i < count; i++)
                {
                    //System.Threading.Thread.Sleep(3);
                    //Spawn Code Here
                    string Name= Spawn(packet);
                    if (!"".Equals(Name)) {
                        Names.Add(Name);
                    }
                }
            }
            //else //Despawn
            //{
            //    for (int i = 0; i < count; i++)
            //    {
            //        //Despawn Code Here
            //        System.Threading.Thread.Sleep(3);
            //        Despawn(packet.ReadUInt32());
            //    }
            //}
            return Names;
        }
        //public static void SingleDespawn(Packet packet)
        //{
        //    Despawn(packet.ReadUInt32());
        //}



        private static string Spawn(Packet packet)
        {
            uint model = 0;
            try
            {
                model = packet.ReadUInt32();

                int mob_index = GameInfo.Mobs.id.IndexOf(model);
                int item_index = GameInfo.Items.id.IndexOf(model);
                if (mob_index != -1)
                {
                    //UTILS.WriteLine(GameInfo.Mobs.type[GameInfo.Mobs.id.IndexOf(model)]);

                    string type = GameInfo.Mobs.type[mob_index];
                    if (type.Contains("CHAR_"))
                    {
                        return SpawnParser.ParseChar(packet, model);
                    }
                    else if (type.Contains("_GATE"))
                    {
                        SpawnParser.ParsePortal(packet, model);
                    }
                    else if (type.Contains("COS_"))
                    {
                        SpawnParser.ParsePets(packet, model);
                    }
                    else if (type.Contains("MOB_"))
                    {
                        SpawnParser.ParseMob(packet, model);
                    }
                    else if (type.Contains("NPC_"))
                    {
                        SpawnParser.ParseNPC(packet, model);
                    }
                    else
                    {
                        SpawnParser.ParseOther(packet, model);
                    }
                }else if (item_index != -1)
                {
                    //UTILS.WriteLine(GameInfo.Items.type[GameInfo.Items.id.IndexOf(model)]);
                    SpawnParser.ParseItems(packet, model);
                }
                if (item_index == -1 && mob_index == -1)
                {
                }
            }
            catch (Exception ex) {
                //UTILS.WriteLine($"解析数组报错ID{model}:{ex.ToString()}");
            }
            return "";
        }
        //public static void Despawn(uint id)
        //{
        //    if (Monsters.UniqueID.IndexOf(id) != -1)
        //    {
        //        int monster_index = Monsters.UniqueID.IndexOf(id);
        //        Monsters.UniqueID.RemoveAt(monster_index);
        //        Monsters.Model.RemoveAt(monster_index);
        //        Monsters.x.RemoveAt(monster_index);
        //        Monsters.y.RemoveAt(monster_index);
        //        Monsters.Type.RemoveAt(monster_index);
        //        Monsters.Status.RemoveAt(monster_index);
        //        Monsters.Name.RemoveAt(monster_index);
        //        Monsters.Distance.RemoveAt(monster_index);
        //        Monsters.Attack.RemoveAt(monster_index);
        //        Stuck.DeleteMob(id);
        //    }
        //    else if (Drops.UniqueID.IndexOf(id) != -1)
        //    {
        //        int drop_index = Drops.UniqueID.IndexOf(id);
        //        Drops.UniqueID.RemoveAt(drop_index);
        //        Drops.Type.RemoveAt(drop_index);
        //        Drops.Status.RemoveAt(drop_index);
        //    }
        //    else if (Characters.UniqueID.IndexOf(id) != -1)
        //    {
        //        int char_index = Characters.UniqueID.IndexOf(id);
        //        Characters.UniqueID.RemoveAt(char_index);
        //        Characters.Name.RemoveAt(char_index);
        //    }
        //    else if (NPCs.UniqueID.IndexOf(id) != -1)
        //    {
        //        int npc_index = NPCs.UniqueID.IndexOf(id);
        //        NPCs.UniqueID.RemoveAt(npc_index);
        //        NPCs.Type.RemoveAt(npc_index);
        //    }
        //    else if (Pets.UniqueID.IndexOf(id) != -1)
        //    {
        //        int pet_index = Pets.UniqueID.IndexOf(id);
        //        Pets.UniqueID.RemoveAt(pet_index);
        //        Pets.Speed.RemoveAt(pet_index);
        //    }
        //}
    }
}
