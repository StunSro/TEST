using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.GameSpawn
{
    class SpawnParser
    {
        public static string ParseChar(Packet packet, uint model)
        {

            int trade = 0;
            int stall = 0;
            packet.ReadUInt8(); // Volume/Height
            packet.ReadUInt8(); // Rank
            packet.ReadUInt8(); // Icons
            packet.ReadUInt8(); // Unknown
            packet.ReadUInt8(); // Max Slots
            int items_count = packet.ReadUInt8();
            for (int a = 0; a < items_count; a++)
            {
                uint itemid = packet.ReadUInt32();
                int itemindex = GameInfo.Items.id.IndexOf(itemid);
                if (GameInfo.Items.type[itemindex].StartsWith("ITEM_CH") || GameInfo.Items.type[itemindex].StartsWith("ITEM_EU") || 
                    GameInfo.Items.type[itemindex].StartsWith("ITEM_FORT") || GameInfo.Items.type[itemindex].StartsWith("ITEM_ROC_CH") || GameInfo.Items.type[itemindex].StartsWith("ITEM_ROC_EU"))
                {
                    byte plus = packet.ReadUInt8(); // Item Plus
                }
                if (GameInfo.Items.type[itemindex].StartsWith("ITEM_EU_M_TRADE") || GameInfo.Items.type[itemindex].StartsWith("ITEM_EU_F_TRADE") || 
                    GameInfo.Items.type[itemindex].StartsWith("ITEM_CH_M_TRADE") || GameInfo.Items.type[itemindex].StartsWith("ITEM_CH_W_TRADE"))
                {
                    trade = 1;
                }
            }
            packet.ReadUInt8(); // Max Avatars Slot
            int avatar_count = packet.ReadUInt8();
            for (int a = 0; a < avatar_count; a++)
            {
                uint avatarid = packet.ReadUInt32();
                int avatarindex = GameInfo.Items.id.IndexOf(avatarid);
                byte plus = packet.ReadUInt8(); // Avatar Plus

            }
            int mask = packet.ReadUInt8();
            if (mask == 1)
            {
                uint id = packet.ReadUInt32();
                string type = GameInfo.Mobs.type[GameInfo.Mobs.id.IndexOf(id)];
                if (type.StartsWith("CHAR"))
                {
                    packet.ReadUInt8();
                    byte count = packet.ReadUInt8();
                    for (int i = 0; i < count; i++)
                    {
                        packet.ReadUInt32();
                    }
                }
            }
            packet.ReadUInt32();

            byte xsec = packet.ReadUInt8();
            byte ysec = packet.ReadUInt8();
            float xcoord = packet.ReadSingle();
            packet.ReadSingle();
            float ycoord = packet.ReadSingle();

            packet.ReadUInt16(); // Position
            byte move = packet.ReadUInt8(); // Moving
            packet.ReadUInt8(); // Running

            if (move == 1)
            {
                xsec = packet.ReadUInt8();
                ysec = packet.ReadUInt8();
                if (ysec == 0x80)
                {
                    xcoord = packet.ReadUInt16() - packet.ReadUInt16();
                    packet.ReadUInt16();
                    packet.ReadUInt16();
                    ycoord = packet.ReadUInt16() - packet.ReadUInt16();
                }
                else
                {
                    xcoord = packet.ReadUInt16();
                    packet.ReadUInt16();
                    ycoord = packet.ReadUInt16();
                }
            }
            else
            {
                packet.ReadUInt8(); // No Destination
                packet.ReadUInt16(); // Angle
            }

            packet.ReadUInt8(); // Alive
            packet.ReadUInt8(); // Unknown
            packet.ReadUInt8(); // Unknown
            packet.ReadUInt8(); // Unknown

            packet.ReadUInt32(); // Walking speed
            packet.ReadUInt32(); // Running speed
            packet.ReadUInt32(); // Berserk speed

            byte active_buffs = packet.ReadUInt8(); // Buffs count
            for (int a = 0; a < active_buffs; a++)
            {
                uint skillid = packet.ReadUInt32();
                string type = GameInfo.Skills.type[GameInfo.Skills.id.IndexOf(skillid)];
                packet.ReadUInt32(); // Temp ID
                if (type.StartsWith("SKILL_EU_CLERIC_RECOVERYA_GROUP") || type.StartsWith("SKILL_EU_BARD_BATTLAA_GUARD") || type.StartsWith("SKILL_EU_BARD_DANCEA") || type.StartsWith("SKILL_EU_BARD_SPEEDUPA_HITRATE"))
                {
                   packet.ReadUInt8();
                }
            }
            string name = packet.ReadAscii();
            packet.ReadUInt8(); // Unknown
            packet.ReadUInt8(); // Job type
            packet.ReadUInt8(); // Job level
            int cnt = packet.ReadUInt8();
            packet.ReadUInt8();
            if (cnt == 1)
            {
                packet.ReadUInt32();
            }
            packet.ReadUInt8(); // Unknown
            stall = packet.ReadUInt8(); // Stall flag
            packet.ReadUInt8(); // Unknown
            string guild = packet.ReadAscii(); // Guild
            if (trade == 1)
            {
                packet.ReadUInt16();
            }
            else
            {
                packet.ReadUInt32(); // Guild ID
                packet.ReadAscii(); // Grant Name
                packet.ReadUInt32();
                packet.ReadUInt32();
                packet.ReadUInt32();
                packet.ReadUInt16();
                if (stall == 4)
                {
                    packet.ReadAscii();
                    packet.ReadUInt32();
                    packet.ReadUInt16();
                }
                else
                {
                    packet.ReadUInt16();
                }
            }
            return name;
        }
        public static void ParsePortal(Packet packet, uint model)
        {
            uint id = packet.ReadUInt32();
            byte xsec = packet.ReadUInt8();
            byte ysec = packet.ReadUInt8();
            float xcoord = packet.ReadSingle();
            packet.ReadSingle();
            float ycoord = packet.ReadSingle();
            packet.ReadUInt16(); // Position
            packet.ReadUInt32();
            packet.ReadUInt64();

        }
        public static void ParseOther(Packet packet, uint model)
        {
            string type = GameInfo.Mobs.type[GameInfo.Mobs.id.IndexOf(model)];
            if (type == "INS_QUEST_TELEPORT")
            {
                packet.ReadUInt32(); // MOB ID
                packet.ReadUInt8();
                packet.ReadUInt8();
                packet.ReadSingle();
                packet.ReadSingle();
                packet.ReadSingle();
                packet.ReadUInt16(); // Position
                packet.ReadUInt8(); // Unknwon
                packet.ReadUInt8(); // Unknwon
                packet.ReadUInt16(); // Unknwon
                packet.ReadAscii();
                packet.ReadUInt32();
            }
        }
        public static void ParseItems(Packet packet, uint model)
        {
            string type = GameInfo.Items.type[GameInfo.Items.id.IndexOf(model)];
            if (type.StartsWith("ITEM_ETC_GOLD"))
            {
                packet.ReadUInt32(); // Ammount
            }
            if (type.StartsWith("ITEM_QSP"))
            {
                packet.ReadAscii(); // Name
            }
            if (type.StartsWith("ITEM_CH") || type.StartsWith("ITEM_EU"))
            {
                packet.ReadUInt8(); // Plus
            }
            uint id = packet.ReadUInt32(); // ID
            packet.ReadUInt8(); //XSEC
            packet.ReadUInt8(); //YSEC
            packet.ReadSingle(); //X
            packet.ReadSingle(); //Z
            packet.ReadSingle(); //Y
            packet.ReadUInt16(); //POS
            if (packet.ReadUInt8() == 1) // Owner exist
            {
                packet.ReadUInt32();
            }
            packet.ReadUInt8(); //Item Blued
        }
        public static void ParseMob(Packet packet, uint model)
        {
            uint id = packet.ReadUInt32(); // MOB ID
            byte xsec = packet.ReadUInt8();
            byte ysec = packet.ReadUInt8();
            float xcoord = packet.ReadSingle();
            packet.ReadSingle();
            float ycoord = packet.ReadSingle();
            packet.ReadUInt16(); // Position
            byte move = packet.ReadUInt8(); // Moving
            packet.ReadUInt8(); // Running
            if (move == 1)
            {
                xsec = packet.ReadUInt8();
                ysec = packet.ReadUInt8();
                if (ysec == 0x80)
                {
                    xcoord = packet.ReadUInt16() - packet.ReadUInt16();
                    packet.ReadUInt16();
                    packet.ReadUInt16();
                    ycoord = packet.ReadUInt16() - packet.ReadUInt16();
                }
                else
                {
                    xcoord = packet.ReadUInt16();
                    packet.ReadUInt16();
                    ycoord = packet.ReadUInt16();
                }
            }
            else
            {
                packet.ReadUInt8(); // Unknown
                packet.ReadUInt16(); // Unknwon
            }
            byte alive = packet.ReadUInt8(); // Alive
            packet.ReadUInt8(); // Unknown
            packet.ReadUInt8(); // Unknown
            packet.ReadUInt8(); // Zerk Active
            packet.ReadSingle(); // Walk Speed
            packet.ReadSingle(); // Run Speed
            packet.ReadSingle(); // Zerk Speed
            packet.ReadUInt32(); // Unknown
            byte type = packet.ReadUInt8();

        }
        public static void ParseNPC(Packet packet, uint model)
        {
            uint id = packet.ReadUInt32();
            byte xsec = packet.ReadUInt8();
            byte ysec = packet.ReadUInt8();
            float xcoord = packet.ReadSingle();
            packet.ReadSingle();
            float ycoord = packet.ReadSingle();

            packet.ReadUInt16(); // Position
            byte move = packet.ReadUInt8(); // Moving
            packet.ReadUInt8(); // Running

            if (move == 1)
            {
                xsec = packet.ReadUInt8();
                ysec = packet.ReadUInt8();
                if (ysec == 0x80)
                {
                    xcoord = packet.ReadUInt16() - packet.ReadUInt16();
                    packet.ReadUInt16();
                    packet.ReadUInt16();
                    ycoord = packet.ReadUInt16() - packet.ReadUInt16();
                }
                else
                {
                    xcoord = packet.ReadUInt16();
                    packet.ReadUInt16();
                    ycoord = packet.ReadUInt16();
                }
            }
            else
            {
                packet.ReadUInt8(); // Unknown
                packet.ReadUInt16(); // Unknwon
            }

            packet.ReadUInt64(); //Unknown
            packet.ReadUInt64(); //Unknown
            ushort check = packet.ReadUInt16();
            if (check != 0)
            {
                byte count = packet.ReadUInt8();
                for (byte i = 0; i < count; i++)
                {
                    packet.ReadUInt8();
                }
            }
        }
        public static void ParsePets(Packet packet, uint model)
        {
            uint pet_id = packet.ReadUInt32(); // PET ID
            byte xsec = packet.ReadUInt8();
            byte ysec = packet.ReadUInt8();
            float xcoord = packet.ReadSingle();
            packet.ReadSingle();
            float ycoord = packet.ReadSingle();

            packet.ReadUInt16(); // Position
            byte move = packet.ReadUInt8(); // Moving
            packet.ReadUInt8(); // Running

            if (move == 1)
            {
                xsec = packet.ReadUInt8();
                ysec = packet.ReadUInt8();
                if (ysec == 0x80)
                {
                    xcoord = packet.ReadUInt16() - packet.ReadUInt16();
                    packet.ReadUInt16();
                    packet.ReadUInt16();
                    ycoord = packet.ReadUInt16() - packet.ReadUInt16();
                }
                else
                {
                    xcoord = packet.ReadUInt16();
                    packet.ReadUInt16();
                    ycoord = packet.ReadUInt16();
                }
            }
            else
            {
                packet.ReadUInt8(); // Unknown
                packet.ReadUInt16(); // Unknwon
            }
            packet.ReadUInt8();
            packet.ReadUInt8();
            packet.ReadUInt8();
            packet.ReadUInt8();
            packet.ReadSingle();
            packet.ReadSingle();
            packet.ReadUInt16();
            string type = GameInfo.Mobs.type[GameInfo.Mobs.id.IndexOf(model)];
            //if (Mobs_Info.mobstypelist[index].StartsWith("COS_C_PEGASUS") || Mobs_Info.mobstypelist[index].StartsWith("COS_C_OSTRICH") || Mobs_Info.mobstypelist[index].StartsWith("COS_C_SCARABAEUS") || Mobs_Info.mobstypelist[index].StartsWith("COS_C_TIGER") || Mobs_Info.mobstypelist[index].StartsWith("COS_C_WILDPIG") || Mobs_Info.mobstypelist[index].StartsWith("COS_C_HORSE") || Mobs_Info.mobstypelist[index].StartsWith("COS_C_CAMEL") || Mobs_Info.mobstypelist[index].StartsWith("COS_T_DHORSE") || Mobs_Info.mobstypelist[index].StartsWith("COS_C_DHORSE"))
            if (type.StartsWith("COS_C") || type.StartsWith("COS_T_DHORSE"))
            {
            }
            else
            {
                if (type.StartsWith("COS_U_UNKNOWN"))
                {
                    packet.ReadUInt16();
                    packet.ReadUInt8();
                }
                else
                {
                    packet.ReadUnicode();
                    if (type.StartsWith("COS_T_COW") || type == "COS_T_DONKEY" | type.StartsWith("COS_T_HORSE") || type.StartsWith("COS_T_CAMEL") || type.StartsWith("COS_T_DHORSE") || type.StartsWith("COS_T_BUFFALO") || type.StartsWith("COS_T_WHITEELEPHANT") || type.StartsWith("COS_T_RHINOCEROS"))
                    {
                        if (type.StartsWith("COS_T_BUFFALO") || type.StartsWith("COS_T_WHITEELEPHANT") || type.StartsWith("COS_T_RHINOCEROS"))
                        {
                            packet.ReadUInt8();
                        }
                        packet.ReadUInt16();
                        packet.ReadUInt32();
                    }
                    else
                    {
                        packet.ReadUnicode();
                        if (type.StartsWith("COS_P_RAVEN"))
                        {
                            packet.ReadUInt8();
                        }
                        if (type.StartsWith("COS_P_WOLF"))
                        {
                            packet.ReadUInt8();
                        }
                        if (type.StartsWith("COS_P_BROWNIE"))
                        {
                            packet.ReadUInt32();
                            packet.ReadUInt8();
                        }
                        else
                        {
                            if (type.StartsWith("COS_P_JINN") || type.StartsWith("COS_P_KANGAROO") || type.StartsWith("COS_P_BEAR") || type.StartsWith("COS_P_FOX") || type.StartsWith("COS_P_PENGUIN"))
                            {
                                packet.ReadUInt8();
                            }
                            packet.ReadUInt8();
                            packet.ReadUInt32();
                        }
                    }
                }
            }


        }
    }
}
