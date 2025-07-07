using Framework;
using SR_PROXY.CORE;
using SR_PROXY.ENGINES;
using SR_PROXY.GameSpawn;
using SR_PROXY.MODEL;
using SR_PROXY.SECURITYOBJECTS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SR_PROXY.ENGINES.UTILS;
using Dapper;

namespace SR_PROXY.SQL
{
    class QUERIES
    {
        public static string connectionstring;
        public static void SetConnectiontType()
        {
            if (Settings.MAIN.checkBox19.Checked)
                connectionstring = "Data Source=" + Settings.MSSQL_SVR_NAME + "\\SQLEXPRESS,1433;Network Library = DBMSSOCN;max pool size=1000; Initial Catalog =xQc_FILTER; User ID = " + Settings.MSSQL_SVR_ID + "; Password=" + Settings.MSSQL_SVR_PW + ";";
            else
                connectionstring = "Data Source=" + Settings.MSSQL_SVR_NAME + ";Initial Catalog=xQc_FILTER;max pool size=1000;Integrated Security=false;UID = " + Settings.MSSQL_SVR_ID + "; PWD = " + Settings.MSSQL_SVR_PW + ";";
        }
        public static async Task<bool> SQL_CONNECTIVITY(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Data Source=" + Settings.MSSQL_SVR_NAME + ";Initial Catalog=" + Settings.MSSQL_LOG_DB + ";Integrated Security=false;UID = " + Settings.MSSQL_SVR_ID + "; PWD = " + Settings.MSSQL_SVR_PW + ";"))
                {
                    await connection.OpenAsync();
                    UTILS.WriteLine("Database Connection is okey!.", UTILS.LOG_TYPE.Notify);
                    connection.Close();
                    MAIN.SQL_STATUS = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                UTILS.WriteLine(ex.ToString(), UTILS.LOG_TYPE.Fatal);
                return false;
            }
        }
        public static string INJECTION_PREFIX(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Replace("'", string.Empty);
                str = str.Replace(";", string.Empty);
                str = str.Replace("-", string.Empty);
                str = str.Replace("\\", string.Empty);
                str = str.Replace("%", string.Empty);
                str = str.Replace("<", string.Empty);
                str = str.Replace(">", string.Empty);
            }
            return str;
        }

        #region LOAD-PREQUESITIES - Startup Mandatory Checks
        public static async Task BACKUP_DB(string dbname, string filepath)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("BACKUP DATABASE " + dbname + "  TO DISK = '" + filepath + "' WITH FORMAT;", con))
                    {
                        await con.OpenAsync();
                        cmd.CommandTimeout = 250;//ms timeout, dbs can potentially be heavy sql operations, hence the bigger timeout
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine(ex.ToString()); }
        }
        public static async Task<bool> TABLE_EXISTENSE(string TN, string DBN)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("IF OBJECT_ID (N'" + DBN + ".dbo." + TN + "', N'U') IS NOT NULL SELECT 1 AS res ELSE SELECT 0 AS res;", con))
                    {
                        await con.OpenAsync();
                        int result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return result == 1 ? true : false;
                    }
                }
            }
            catch { UTILS.WriteLine("TABLE_EXISTENSE returned false and failed."); return false; }
        }
        public static async Task<bool> DATABASE_EXISTENSE(string DBN)
        {
            try
            {
                using (SqlConnection con = new SqlConnection("Data Source=" + Settings.MSSQL_SVR_NAME + ";Initial Catalog=" + Settings.MSSQL_LOG_DB + ";Integrated Security=false;UID = " + Settings.MSSQL_SVR_ID + "; PWD = " + Settings.MSSQL_SVR_PW + ";"))
                {
                    using (SqlCommand cmd = new SqlCommand("select name from master.dbo.sysdatabases where name = '" + DBN + "'", con))
                    {
                        await con.OpenAsync();
                        string result = Convert.ToString(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return !string.IsNullOrEmpty(result) ? true : false;
                    }
                }
            }
            catch { UTILS.WriteLine("DATABASE_EXISTENSE returned false and failed."); return false; }
        }
        public static async Task<bool> DBN_CREATE(string DBN)
        {
            try
            {
                using (SqlConnection con = new SqlConnection("Data Source=" + Settings.MSSQL_SVR_NAME + ";Initial Catalog=" + Settings.MSSQL_LOG_DB + ";Integrated Security=false;UID = " + Settings.MSSQL_SVR_ID + "; PWD = " + Settings.MSSQL_SVR_PW + ";"))
                {
                    using (SqlCommand cmd = new SqlCommand("CREATE DATABASE " + DBN + ";", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteScalarAsync();
                        con.Close();
                        return true;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("DBN_CREATE returned false and failed." + EX.ToString()); return false; }
        }

        public static async Task<bool> SQL_Run_Code(string SqlCode)
        {
            try
            {
                if (String.Empty.Equals(SqlCode))
                {
                    throw new Exception("执行的代码为空");
                }
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    //using (SqlCommand Getcharnamecmd = new SqlCommand("TRUNCATE TABLE xQc_FILTER.dbo._Players;", con))
                    using (SqlCommand cmd = new SqlCommand(SqlCode, con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                UTILS.WriteLine($"SQL执行出现异常{SqlCode}:" + e.Message);
                return false;
            }
        }

        public static async Task<DataTable> GetList(string query)
        {
            try
            {
                using (var con = new SqlConnection(connectionstring))
                {
                    await con.OpenAsync().ConfigureAwait(false);
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        using (SqlDataReader read = await command.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            var dataTable = new DataTable();
                            dataTable.Load(read);
                            return dataTable;
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                return null;
            }
            catch (SqlException Ex)
            {
            }
            return null;
        }
        public static async Task<bool> Clear_Players_Records()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    //using (SqlCommand Getcharnamecmd = new SqlCommand("TRUNCATE TABLE xQc_FILTER.dbo._Players;", con))
                    using (SqlCommand cmd = new SqlCommand("update xQc_FILTER.dbo._Players set cur_status = 0;", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        return true;
                    }
                }
            }
            catch { UTILS.WriteLine("Clear_Players_Records operation has failed"); return false; }
        }
        public static async Task SQL_SVR_BIT()
        {
            await loadCustomTitleList();
            await LoadTitleColorsList();
            await LoadEventTime();
            await LoadChangeLog();
            await LoadCustomNamesRank();
            await LoadCharnameColor();
            await LoadCustomNames();
            await LoadIcons();
            await LoadIconsData();
            await LoadEventsScheduler();
            await LoadDailyReward();
            await LoadDailyRewardItems();
            //required region IDs info for systems
            //regions IDs
            foreach (var item in Settings.ShardInfos)
            {
                await LoadGuildRank(item.Key);
                await LoadHonorRank(item.Key);
                //await LoadCharRank(item.Key);
                await LoadHunterRank(item.Key);
                await LoadTraderRank(item.Key);
                await LoadThiefRank(item.Key);
                await LoadPVPRank(item.Key);
                //await LoadJobKillsRank(item.Key);
                if (!await Get_wRegionID(READER.FW_WREGION_ID, "FORT_", item.Key))
                {
                    UTILS.WriteLine("FAILED:FWwRegionID.", UTILS.LOG_TYPE.Warning);
                }
                if (!await Get_wRegionID(READER.CTF_WREGION_ID, "SIEGE_DUNG", item.Key))
                {
                    UTILS.WriteLine("FAILED:GET_CTFwRegionID.", UTILS.LOG_TYPE.Warning);
                }
                if (!await Get_wRegionID(READER.FGW_WREGION_ID, "GOD_", item.Key))
                {
                    UTILS.WriteLine("FAILED:GET_BAwRegionID.", UTILS.LOG_TYPE.Warning);
                }
                if (!await Get_wRegionID(READER.BA_WREGION_ID, "ARENA_", item.Key))
                {
                    UTILS.WriteLine("FAILED:GET_FGWwRegionID.", UTILS.LOG_TYPE.Warning);
                }
                if (!await Get_wRegionID(READER.TOWNS_WREGION_ID, "TOWN", item.Key))
                {
                    UTILS.WriteLine("FAILED:GET_TownswRegionID.", UTILS.LOG_TYPE.Warning);
                }
                //gate portal IDs
                if (!await Get_TeleportID(READER.FW_TELEPORT_ID, "FORT", item.Key))
                {
                    UTILS.WriteLine("FAILED:GET_FWTeleportID.", UTILS.LOG_TYPE.Warning);
                }
                if (!await Get_TeleportID(READER.JC_TELEPORT_ID, "TEMPLE", item.Key))
                {
                    UTILS.WriteLine("FAILED:GET_JCTeleportID.", UTILS.LOG_TYPE.Warning);
                }
                //clear _Players table everytime we restart, to dispose old records, keeps the integrity of the data intact.
                if (!await Clear_Players_Records())
                {
                    UTILS.WriteLine("FAILED:Clear_Players_Records.", UTILS.LOG_TYPE.Warning);
                }
                UTILS.Available_Uniques = await GET_AvailableUniques(item.Key);
            }
        }
        #endregion

        #region BASE_HELPERS Get/Is Checks
        //BotEngine recent changes
        public static async Task<long?> Get_ItemID_bySlotandCharname16(string CharName16, string ItemCodeName128, int slot, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT ItemID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory WHERE CharID = (Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CharName16 + "') AND ItemID IN (SELECT ID64 FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Items WHERE RefItemID IN (SELECT ID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjCommon WHERE CodeName128 = '" + ItemCodeName128 + "') and slot = '" + slot + "');", con))
                    {
                        await con.OpenAsync();
                        long? result = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return result != null ? result : 0;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("Get_ItemID_bySlotandCharname16() has returned 0 " + EX.ToString()); return 0; }
        }

        public static async Task<int> Get_ItemID_by_Slot(int Slot, int CharID,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select RefItemID from {Settings.ShardInfos[ShardID].DBName}.dbo._Items where ID64 = (Select ItemID from {Settings.ShardInfos[ShardID].DBName}.dbo._Inventory where Slot = {Slot} and CharID = {CharID})", con))
                    {
                        await con.OpenAsync();
                        int x = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"Get_ItemID_by_Slot failed: {EX}"); return 0; }
        }
        public static async Task<int> Get_TidGroupID_By_SlotAndCharname(string CharName16, int slot, int ShardID,int CharID)
        {
            try
            {
                int[] result = await Get_ItemID_Data(CharName16, slot, ShardID,CharID);
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT TidGroupID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefFmnTidGroupMap WHERE Service = 1 AND TypeID1 = " + result[0] + " AND TypeID2 = " + result[1] + " AND TypeID3 = " + result[2] + " AND TypeID4 = " + result[3], con))
                    {
                        await con.OpenAsync();
                        int TidGroupID = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return result == null ? 0 : TidGroupID;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("Get_TidGroupID_By_SlotAndCharname() has returned 0." + EX.ToString()); return 0; }
        }
        public static async Task<int[]> Get_ItemID_Data(string CharName16, int slot, int ShardID ,int CharID)
        {
            try
            {
                int[] result = null;
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 TypeID1, TypeID2, TypeID3, TypeID4 FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjCommon WHERE ID = ( SELECT RefItemID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Items WHERE ID64 = ( SELECT ItemID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory WHERE CharID = '" + CharID + "' AND Slot = " + slot + " ))", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                result = new int[4] { Convert.ToInt32(reader[0]), Convert.ToInt32(reader[1]), Convert.ToInt32(reader[2]), Convert.ToInt32(reader[3]) };
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return result;
            }
            catch (Exception EX) { UTILS.WriteLine("Get_ItemID_Data() has returned 0 " + EX.ToString()); return null; }
        }

        public static async Task<int[]> Get_ItemID_Data_ByItemID(string CharName16, int ShardID , int ItemID)
        {
            try
            {
                int[] result = null;
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 TypeID1, TypeID2, TypeID3, TypeID4 FROM " + Settings.ShardInfos[ShardID].DBName + $".dbo._RefObjCommon WHERE ID like {ItemID}", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                result = new int[4] { Convert.ToInt32(reader[0]), Convert.ToInt32(reader[1]), Convert.ToInt32(reader[2]), Convert.ToInt32(reader[3]) };
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return result;
            }
            catch (Exception EX) { UTILS.WriteLine("Get_ItemID_Data() has returned 0 " + EX.ToString()); return null; }
        }
        public static async Task<int?> Get_Next_InvAvailableSlot(string CharName16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select top 1 Slot from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory where charid = (Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CharName16 + "') and slot >= 13 and slot < (SELECT InventorySize FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char WHERE CharName16 = N'" + CharName16 + "') and itemid = 0 order by slot;", con))
                    {
                        await con.OpenAsync();
                        int result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return result;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("Get_Next_InvAvailableSlot() has returned null " + EX.ToString()); return null; }
        }
        public static async Task<int?> Get_ItemMaxStack_By_CharNameAndSlot(string CharName16, int slot, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT MaxStack FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjItem WHERE ID = (SELECT Link FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjCommon WHERE ID = (SELECT RefItemID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Items WHERE ID64 = ( SELECT ItemID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory WHERE CharID = " + await QUERIES.Get_CharID_by_CharName16(CharName16, ShardID) + " AND Slot = " + slot + " )))", con))
                    {
                        await con.OpenAsync();
                        int result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return result;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("Get_ItemMaxStack_By_CharNameAndSlot() has returned null " + EX.ToString()); return null; }
        }
        public static async Task<int?> Get_ItemStack_Count(string CharName16, int slot, int ShardID)
        {
            if (await Get_ItemMaxStack_By_CharNameAndSlot(CharName16, slot, ShardID) == 1) return 1; // if item max stack is one, return 1
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT Data FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Items WHERE ID64 = ( SELECT ItemID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory WHERE CharID = " + await QUERIES.Get_CharID_by_CharName16(CharName16, ShardID) + " AND Slot = " + slot + " )", con))
                    {
                        await con.OpenAsync();
                        int result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return result;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("Get_ItemStack_Count() has returned null " + EX.ToString()); return null; }
        }
        public static async Task<int?> Get_InvSlot_By_Codename(string CharName16, int ShardID, string codename, int? stack_count = null, int? opt_level = null)
        {
            try
            {
                int CharID = await Get_CharID_by_CharName16(CharName16, ShardID);
                stack_count = (stack_count == 0 ? null : stack_count);
                string sql_cmd = string.Empty;
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    if (stack_count == null && opt_level == null) sql_cmd = "SELECT TOP 1 Slot FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory WHERE CharID = " + CharID + " AND ItemID IN ( SELECT ID64 FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Items WHERE RefItemID IN ( SELECT ID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjCommon WHERE CodeName128 = '" + codename + "' ) )";
                    else if (stack_count == null && opt_level != null) sql_cmd = "SELECT TOP 1 Slot FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory WHERE CharID = " + CharID + " AND ItemID IN ( SELECT ID64 FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Items WHERE RefItemID IN ( SELECT ID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjCommon WHERE CodeName128 = '" + codename + "' ) AND OptLevel = " + opt_level + " )";
                    else sql_cmd = "SELECT TOP 1 Slot FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory WHERE CharID = " + CharID + " AND ItemID IN ( SELECT ID64 FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Items WHERE RefItemID IN ( SELECT ID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjCommon WHERE CodeName128 = '" + codename + "' ) AND Data = " + stack_count + " AND OptLevel = " + opt_level + " )";

                    using (SqlCommand cmd = new SqlCommand(sql_cmd, con))
                    {
                        await con.OpenAsync();
                        int result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return result < 13 ? 404 : result;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("Get_ItemStack_Count() has returned null " + EX.ToString()); return null; }
        }
        public static async Task Add_Item_To_Inventory(string CharName16, string ItemCodeName, int Quantity, int PlusValue, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("exec " + Settings.ShardInfos[ShardID].DBName + ".dbo._ADD_ITEM_EXTERN '" + CharName16 + "','" + ItemCodeName + "'," + Quantity + "," + PlusValue, con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("Add_Item_To_Inventory() has returned " + EX.ToString()); return; }
        }
        public static async Task<List<string>> Get_BotsEngine_CharName16(int ShardID)
        {
            try
            {
                var temp_records = new List<string>();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT CharName16 FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char WHERE CharID IN (SELECT CharID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._User WHERE UserJID IN (SELECT JID FROM " + Settings.MSSQL_ACC_DB + ".dbo.TB_User WHERE StrUserID like 'srbot_%'))", con))
                    {
                        cmd.CommandTimeout = 60;
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                temp_records.Add(reader[0].ToString());
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return temp_records;
            }
            catch (Exception EX) { UTILS.WriteLine("Get_BotsEngine_CharName16() has returned并且返回NULL" + EX.ToString()); return null; }
        }
        public static async Task<List<string>> Get_Online_BotsEngine_Characters()
        {
            try
            {
                var temp_records = new List<string>();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT CharName16 FROM xQc_FILTER.dbo._Players WHERE StrUserID like 'srbot_%' AND cur_status = 1", con))
                    {
                        cmd.CommandTimeout = 60;
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                temp_records.Add(reader[0].ToString());
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return temp_records;
            }
            catch (Exception EX) { UTILS.WriteLine("Get_Online_BotsEngine_Characters() has returned并且返回NULL" + EX.ToString()); return null; }
        }
        // get
        public static async Task<int[]> Get_TOT_SILK_BALANCE(string CharName16, int ShardID)
        {
            string sql = "";
            try
            {
                int[] result = null;
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    sql = "Select silk_own,silk_gift,silk_point from " + Settings.MSSQL_ACC_DB + ".dbo.SK_Silk where JID = (Select UserJID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._User where CharID = (Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CharName16 + "'))";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                                result = new int[3] { Convert.ToInt32(reader[0]), Convert.ToInt32((int)reader[1]), Convert.ToInt32((int)reader[2]) };
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return result;
            }
            catch (Exception EX) { UTILS.WriteLine($"TOT_SILK_BALANCE() has returned,SQL:{sql},{EX.ToString()}"); return null; }
        }
        public static async Task<long> 获取余额(string CharName16,int Type, int ShardID)
        {
            string sql = "";
            try
            {
                long result = 0;
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    switch (Type) {
                        case 0:
                            sql = "select silk_own from " + Settings.MSSQL_ACC_DB + ".dbo.SK_Silk where JID = (Select UserJID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._User where CharID = (Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CharName16 + "'))";
                            break;
                        case 1:
                            sql = $"select RemainGold FROM {Settings.ShardInfos[ShardID].DBName}.[dbo].[_Char] where [CharName16]='{CharName16}'";
                            break;
                        case 2:
                            sql = "select silk_point from " + Settings.MSSQL_ACC_DB + ".dbo.SK_Silk where JID = (Select UserJID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._User where CharID = (Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CharName16 + "'))";
                            break;
                        default:
                            throw new Exception("非法货币类型");
                            break;
                    }
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        await con.OpenAsync();
                        result = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                        con.Close();
                    }
                }
                return result;
            }
            catch (Exception EX) { UTILS.WriteLine($"获取余额 has returned,SQL:{sql},{EX.ToString()}"); return 0; }
        }
        public static async Task<int[]> Get_ADDED_POINTS(string CharName16, int ShardID)
        {
            string sql = "SELECT TOP 1 Silk_Offset, Silk_Type FROM " + Settings.MSSQL_ACC_DB + ".dbo.SK_SilkBuyList WHERE UserJID = (SELECT TOP 1 UserJID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._User WHERE CharID = (SELECT TOP 1 CharID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char WHERE CharName16 = N'" + CharName16 + "')) ORDER BY RegDate DESC;";
            try
            {
                int[] result = null;
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                                result = new int[2] { Convert.ToInt32(reader[0]), Convert.ToInt32(reader[1]) };
                        }
                        con.Close();
                    }
                }
                return result;
            }
            catch (Exception EX) { UTILS.WriteLine($"GET_ADDED_POINTS() has returned,Sql{sql},{EX.ToString()}"); return null; }
        }
        public static async Task<List<string>> GET_AvailableUniques(int ShardID)
        {
            try
            {
                var tempList = new List<string>();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select CodeName128 from " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjCommon where Rarity = 3 and CodeName128 like '%MOB%' and CodeName128 not like '%_EV_%'and CodeName128 not like '%GNGWC%'and CodeName128 not like '%GOD%'and CodeName128 not like '%FW%'and CodeName128 not like '%_L%'and Service = 1", con))
                    {

                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                tempList.Add(reader[0].ToString().Split('_')[2]);
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return tempList;
            }
            catch { UTILS.WriteLine("GET_AvailableUniques returned an empty list and failed."); return new List<string>(); }
        }
        public static async Task<string> Get_BasicCode_by_SkillID(int SkillID, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select Basic_Code from " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefSkill where ID = '" + SkillID + "';", con))
                    {
                        await con.OpenAsync();
                        string Basic_Code = (string)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return Basic_Code;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_BasicCode_by_SkillID operation has has returned string.Empty"); return string.Empty; }
        }
        public static async Task<int?> Get_LastOrderID()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select Max(cast(OrderNumber as int))+1 from " + Settings.MSSQL_ACC_DB + ".dbo.SK_SilkBuyList", con))
                    {
                        await con.OpenAsync();
                        int? sb = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        if (sb == null)
                        {
                            await Insert_LastOrderID_Dummy();
                            return await Get_LastOrderID();
                        }
                        else return sb;
                    }
                }
            }
            catch {  return 0; }
        }
        public static async Task<int> Insert_LastOrderID_Dummy()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT " + Settings.MSSQL_ACC_DB + ".dbo.SK_SilkBuyList(UserJID,Silk_Type,Silk_Reason,Silk_Offset,Silk_Remain,ID,BuyQuantity,OrderNumber,SlipPaper,RegDate) VALUES( 0,0,0,0,0,0,0,0,'dummy',GETDATE())", con))
                    {
                        await con.OpenAsync();
                        int result = await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        return result == 1 ? 0 : 1;
                    }
                }
            }
            catch { UTILS.WriteLine("Insert_LastOrderID_Dummy operation has has returned 1"); return 1; }
        }
        public static async Task<string> Get_CHARNAME16_by_CharID(int CharID, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand Getcharnamecmd = new SqlCommand("Select CHARNAME16 from  " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharID =" + CharID + ";", con))
                    {
                        await con.OpenAsync();
                        string CHARNAME16 = (string)await Getcharnamecmd.ExecuteScalarAsync();
                        con.Close();
                        return CHARNAME16;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_CHARNAME16_by_CharID operation has has returned string.Empty"); return string.Empty; }
        }

        public static async Task<int> Get_CharID_by_CharName16(string CharName16,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = '" + UTILS.INJECTION_PREFIX(CharName16) + "';", con))
                    {
                        await con.OpenAsync();
                        int CHARID = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return CHARID;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_CharID_by_CharName16 operation has has returned 0"); return 0; }
        }


        public static async Task<int> Get_JID_by_CharID(int CharID,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select UserJID from {Settings.ShardInfos[ShardID].DBName}.dbo._User where CharID = {CharID}", con))
                    {
                        await con.OpenAsync();
                        int x = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"Get_JID_by_CharName16 failed: {EX}",UTILS.LOG_TYPE.Fatal); return 0; }
        }

        public static async Task<int> Get_Trophies_Price_By_Codename(string codename, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT Cost FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefPricePolicyOfItem WHERE PaymentDevice = 16 AND RefPackageItemCodeName = '" + codename + "'", con))
                    {
                        await con.OpenAsync();
                        int price = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return price;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_Trophies_Price_By_Codename operation has has returned 0"); return 0; }
        }
        public static async Task<int> Get_Point_balance(string CharName16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select silk_point from " + Settings.MSSQL_ACC_DB + ".dbo.SK_Silk where JID IN (Select UserJID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._User where CharID in (Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CharName16 + "'));", con))
                    {
                        await con.OpenAsync();
                        int currency = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return currency;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_Point_balance operation has has returned 0"); return 0; }
        }
        public static async Task<int> 获取版本号()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 1 [nVersion] FROM [{Settings.MSSQL_ACC_DB}].[dbo].[_ModuleVersion] order by nVersion desc", con))
                    {
                        await con.OpenAsync();
                        int currency = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return currency;
                    }
                }
            }
            catch (Exception Ex) { UTILS.WriteLine($"获取版本号失败:{Ex.ToString()}"); return 0; }
        }
        public static async Task<long> GetLastCertificateNum()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 1 certificate_num FROM {Settings.MSSQL_ACC_DB}.[dbo].[TB_User] order by certificate_num desc", con))
                    {
                        await con.OpenAsync();
                        object temp = await cmd.ExecuteScalarAsync();
                        long currency = Convert.ToInt64(temp == null ? "203103198405100000" : temp) + 1;
                        con.Close();
                        return currency;
                    }
                }
            }
            catch (Exception Ex) { UTILS.WriteLine($"GetLastCertificateNum:{Ex.ToString()}"); return 203103198405100000; }
        }
        public static async Task<int> 获取称号等级(string Name, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT [HwanLevel] FROM {Settings.ShardInfos[ShardID].DBName}.[dbo].[_Char] where [CharName16]='{Name}'", con))
                    {
                        await con.OpenAsync();
                        object temp = await cmd.ExecuteScalarAsync();
                        con.Close();
                        return Convert.ToInt32(temp);
                    }
                }
            }
            catch (Exception Ex) { UTILS.WriteLine($"获取称号等级:{Ex.ToString()}"); return 0; }
        }
        public static async Task<string> Get_StrUserID_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT StrUserID FROM " + Settings.MSSQL_ACC_DB + ".dbo.TB_User WHERE JID = (SELECT UserJID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._User WHERE CharID=(Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "'));", con))
                    {
                        await con.OpenAsync();
                        string Username = Convert.ToString(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return Username;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_StrUserID_by_CHARNAME16 operation has has returned string.Empty"); return string.Empty; }
        }
        public static async Task<int> Get_CurLevel_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            if (ShardID == 0) ShardID = 64;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT CurLevel FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char WHERE CharName16 = N'" + CHARNAME16 + "';", con))
                    {
                        await con.OpenAsync();//do not change to cast, do convert to int, cuasing errors
                        int charlvl = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return charlvl;
                    }
                }
            }
            catch(Exception ex) { UTILS.WriteLine($"Get_CurLevel_by_CHARNAME16 operation has has returned:{ex.ToString()}"); return Settings.GLOBAL_REQ_LVL + 1; }
        }
        public static async Task<string> Get_NikeName_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT NickName16 FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char WHERE CharName16 = N'" + CHARNAME16 + "';", con))
                    {
                        await con.OpenAsync();//do not change to cast, do convert to int, cuasing errors
                        string charlvl = Convert.ToString(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return charlvl;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_NikeName_by_CHARNAME16 operation has has returned 1."); return ""; }
        }

        public static async Task<bool> Get_Jobstate_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT ItemID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory WHERE CharID = (Select CharID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "') AND Slot = 8;", con))
                    {
                        await con.OpenAsync();
                        string result = Convert.ToString(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return (string.IsNullOrEmpty(result) || result == "0") ? false : true;
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine("$Get_Jobstate_by_CHARNAME16 returned 0 and failed." + ex.ToString(), UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<int[]> Get_JobType_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"select JobType,Level from {Settings.ShardInfos[ShardID].DBName}.[dbo].[_CharTrijob] a,{Settings.ShardInfos[ShardID].DBName}.[dbo].[_Char] b where [CharName16]='{CHARNAME16}' and a.CharID=b.CharID", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                foreach (DataRow item in dt.Rows)
                                {
                                    return new int[] {Convert.ToInt32(item["JobType"]), Convert.ToInt32(item["Level"]) };
                                }
                            }
                        }
                        con.Close();
                        return null;
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine("Get_JobType_by_CHARNAME16:" + ex.ToString(), UTILS.LOG_TYPE.Fatal); return null; }
        }
        public static async Task<string> Get_ItemRealName_by_Slot(int Slot, string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select top (1) RealName from xQc_FILTER.dbo._ItemPoolName where CodeName COLLATE DATABASE_DEFAULT in(select CodeName128 from " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjCommon where ID =(select RefItemID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Items where ID64 = (select ItemID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory where Slot = " + Slot + " and CharID =(select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "'))))", con))
                    {
                        await con.OpenAsync();
                        string result = (string)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return result;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_ItemRealName_by_Slot operation has has returned null, if _ItemPoolName table has no records, please fill it up from Prerequisites folder."); return string.Empty; }
        }
        public static async Task<string> Get_ItemCodeName128_by_Slot(int Slot, string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select top (1) CodeName128 from " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjCommon where ID =(select RefItemID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Items where ID64 = (select ItemID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory where Slot = '" + Slot + "' and CharID =(select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "')))", con))
                    {
                        await con.OpenAsync();
                        string result = (string)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return result;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_ItemRealName_by_Slot operation has has returned null, if _ItemPoolName table has no records, please fill it up from Prerequisites folder."); return null; }
        }
        public static async Task<int?> Get_AdvancedElixirValue(int Slot, string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select nOptValue from " + Settings.ShardInfos[ShardID].DBName + ".dbo._BindingOptionWithItem WHERE nItemDBID = (SELECT ItemID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory where CharID = (Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "') AND Slot = '" + Slot + "')", con))
                    {
                        await con.OpenAsync();
                        int? result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return result != null ? result : 0;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("Get_AdavncedElixirValue operation failed " + EX.ToString()); return 0; }
        }
        public static async Task<short> Get_RegionID_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT LatestRegion FROM [" + Settings.ShardInfos[ShardID].DBName + "].[dbo].[_Char] where CharName16 = N'" + CHARNAME16 + "'", con))
                    {
                        await con.OpenAsync();
                        short region = (short)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return region;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_RegionID_by_CHARNAME16 operation has has returned 0"); return 0; }
        }
        public static async Task<int> Get_EmptyInvSlotCount_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(Slot) AS EmptySlots FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory WHERE CharID = (SELECT CharID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "') AND ItemID = 0  AND Slot > 12 AND Slot < (SELECT InventorySize FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharID = (SELECT CharID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "'))", con))
                    {
                        await con.OpenAsync();
                        int sb = (int)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return sb;
                    }
                }
            }
            catch { UTILS.WriteLine("IS_BattleField_by_CHARNAME16 returned 500 and failed."); return 500; }
        }
        public static async Task<string> Get_ItemCodeName_by_SlotAndCHARNAME16(int slot, string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 CodeName128 FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjCommon WHERE ID = (SELECT TOP 1 RefItemID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Items WHERE ID64 = (SELECT TOP 1 ItemID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory WHERE Slot = " + slot.ToString() + " and CharID = (SELECT CharID FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "'))) AND CodeName128 like 'ITEM_CH_%_FRPVP_VOUCHER_%'", con))
                    {
                        await con.OpenAsync();
                        string result = (string)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return result;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_ItemCodeName_by_SlotAndCHARNAME16 returned string.Empty and failed."); return string.Empty; }
        }
        public static async Task<string> Get_Gender_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT Count(Deleted) FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "' AND (RefObjID BETWEEN 1907 AND 1919) OR (RefObjID BETWEEN 14717 AND 14734)", con))
                    {
                        await con.OpenAsync();
                        int result = (int)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return result > 0 ? "male" : "female";
                    }
                }
            }
            catch { UTILS.WriteLine("Get_Gender_by_CHARNAME16 returned string.Empty and failed."); return string.Empty; }
        }
        public static async Task<int> Get_Optlvl_by_SlotAndCHARNAME16(string CHARNAME16, int Slot, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("select OptLevel from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Items where ID64 = (select ItemID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory where Slot = '" + Slot + "'  and CharID =(select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 ='" + CHARNAME16 + "'))", con))
                    {
                        await con.OpenAsync();
                        string optlvl = Convert.ToString(cmd.ExecuteScalar());
                        con.Close();
                        return string.IsNullOrEmpty(optlvl) ? (Settings.ALCHEMY_MAXPLUS + 1) : Convert.ToInt32(optlvl);
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("Get_Optlvl_by_SlotAndCHARNAME16 returned 0 and failed." + EX.ToString()); return 0; }
        }
        public static async Task<int> Get_GuildMembersCount_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select COUNT(CharID) from " + Settings.ShardInfos[ShardID].DBName + ".dbo._GuildMember where CharName = '" + INJECTION_PREFIX(CHARNAME16) + "';", con))
                    {
                        await con.OpenAsync();
                        int memcount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return memcount;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_GuildMembersCount_by_CHARNAME16 returned 0 and failed."); return 0; }
        }
        public static async Task<int> Get_UnionMembersCount_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT CASE WHEN Ally1 != 0 THEN 1 ELSE 0 END + CASE WHEN Ally2 != 0 THEN 1 ELSE 0 END + CASE WHEN Ally3 != 0 THEN 1 ELSE 0 END + CASE WHEN Ally4 != 0 THEN 1 ELSE 0 END + CASE WHEN Ally5 != 0 THEN 1 ELSE 0 END + CASE WHEN Ally6 != 0 THEN 1 ELSE 0 END + CASE WHEN Ally7 != 0 THEN 1 ELSE 0 END + CASE WHEN Ally8 != 0 THEN 1 ELSE 0 END AS TOTAL FROM  " + Settings.ShardInfos[ShardID].DBName + ".dbo._AlliedClans where ID = (Select Alliance from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Guild where ID = (Select GuildID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + INJECTION_PREFIX(CHARNAME16) + "'));", con))
                    {
                        await con.OpenAsync();
                        int UNcnt = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return UNcnt;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_UnionMembersCount_by_CHARNAME16 returned 0 and failed."); return 0; }
        }
        public static async Task<bool> Get_GMPrivileges_by_StrUserID(string StrUserID)
        {
            try
            {
                int[] result = null;
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select sec_primary,sec_content from " + Settings.MSSQL_ACC_DB + ".dbo.TB_User where StrUserID = '" + StrUserID + "';", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                                result = new int[2] { (int)reader[0], (int)reader[1] };
                            reader.Close();
                        }
                        con.Close();
                        return (result[0] == 1 && result[1] == 1) ? true : false;
                    }
                }
            }
            catch { UTILS.WriteLine("Get_GMsPriviliges_by_StrUserID returned false and failed."); return false; }
        }
        public static async Task<List<string>> GET_AvailableHwanIDs(int ShardID)
        {
            try
            {
                var tempList = new List<string>();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("select Title_CH70 from " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefHwanLevel where Title_CH70  like '%_LEVEL_%';", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                tempList.Add(reader[0].ToString().Split(new string[] { "_LEVEL_" }, StringSplitOptions.None)[1]);
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return tempList;
            }
            catch { UTILS.WriteLine("GET_AvailableHwanIDs returned an empty list and failed."); return new List<string>(); }
        }
        public static async Task<bool> Get_TableRowsNumber(string dbname, string tblname)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select COUNT (*) from " + dbname + ".dbo." + tblname + ";", con))
                    {
                        await con.OpenAsync();
                        int rowsnr = (int)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return rowsnr != 0 ? true : false;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine(EX.ToString()); return false; }
        }
        public static async Task<bool> Get_wRegionID(List<short> LIST, string keyword, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select distinct wRegionID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Refregion where ContinentName like '%" + keyword + "%';", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                LIST.Add(Convert.ToInt16(reader[0]));
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch { UTILS.WriteLine("GET_JCTeleportID returned false and failed."); return false; }
        }
        public static async Task<bool> Get_TeleportID(List<short> LIST, string keyword, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select distinct TargetTeleport from " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefTeleLink WHERE OwnerTeleport IN (Select ID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefTeleport where CodeName128 like '%" + keyword + "%' AND Service = 1)", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                LIST.Add(Convert.ToInt16(reader[0]));
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch { UTILS.WriteLine("GET_JCTeleportID returned false and failed."); return false; }
        }
        public static async Task<bool> Get_WebPrivileges_by_CharName16(string CharName16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT [STATUS] FROM " + Settings.MSSQL_ACC_DB + ".dbo.TB_User where JID = (Select UserJID from  " + Settings.ShardInfos[ShardID].DBName + ".dbo._User where CharID = (Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + INJECTION_PREFIX(CharName16) + "'))", con))
                    {
                        await con.OpenAsync();
                        int status = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return status != 2 ? true : false;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("Get_WebPrivileges_by_StrUserID operation has has returned 0" + EX.ToString()); return true; }
        }
        /// <summary>
        /// 判断表是否存在
        /// </summary>
        /// <param name="TabName"></param>
        /// <returns></returns>
        public static async Task<bool> SQL_Exist_Tab(string TabName)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("USE [xQc_FILTER] select name from sys.objects where name = '" + TabName + "' and type='U'", con))
                    {

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                return true;
                            }
                            return false;
                        }

                    }
                    con.Close();
                }
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 判断存储是否存在
        /// </summary>
        /// <param name="ExeName"></param>
        /// <returns></returns>
        public static async Task<bool> SQL_Exist_Exe(string ExeName)
        {
            try
            {

                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("USE [xQc_FILTER] select name from sys.objects where name = '" + ExeName + "' and type='P'", con))
                    {

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                return true;
                            }
                            return false;
                        }

                    }
                    con.Close();
                }
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 获取存储
        /// </summary>
        /// <returns></returns>
        public static async Task<List<string>> Get_Sp_List()
        {
            List<string> SpNames = new List<string>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("use [xQc_FILTER] select name from sysobjects where type='P'", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                foreach (DataRow item in dt.Rows)
                                {
                                    SpNames.Add(item["name"].ToString());
                                }
                            }
                        }
                        con.Close();
                    }
                }
                return SpNames;
            }
            catch (Exception EX) { UTILS.WriteLine("存储列表获取失败" + EX.ToString()); return SpNames; }
        }
        /// <summary>
        /// 获取自定义指令列表
        /// </summary>
        /// <returns></returns>
        public static async Task<ActionResponseModel> SQL_Run_Action(string ActionName, string CharName, List<string> Parameters, string DstCharName = "")
        {
            ActionResponseModel actionResponse = new ActionResponseModel();
            try
            {
                SqlConnection con = new SqlConnection(connectionstring);
                await con.OpenAsync();
                SqlCommand command = new SqlCommand("select * from xQc_FILTER.[dbo].[_CustomAction] where [Name]=N'" + ActionName + "'", con);
                SqlDataReader read = await command.ExecuteReaderAsync();
                if (!read.Read())
                {
                    actionResponse.SrcNoticeMessage = "该功能不存在！";
                    return actionResponse;
                }

                string SpName = read["SpName"].ToString();
                int ParameterNum = (int)read["ParameterNum"];
                int Service = (int)read["Service"];
                con.Close();
                if (Parameters.Count != ParameterNum)
                {
                    actionResponse.SrcNoticeMessage = "参数格式不正确！";
                    return actionResponse;
                }
                else if (Service == 0)
                {
                    actionResponse.SrcNoticeMessage = "服务已被关闭！";
                    return actionResponse;
                }
                actionResponse = await SQL_Run_ActionSp(SpName, CharName, Parameters, DstCharName);
            }
            catch (Exception e)
            {
                string ParametersText = "";
                foreach (var item in Parameters)
                {
                    ParametersText += item + "|";
                }
                UTILS.WriteLine($"玩家[{CharName}]执行[{ActionName}]指令,传入参数[{ParametersText}]发生异常:" + e.ToString(), UTILS.LOG_TYPE.Warning);
                actionResponse.SrcNoticeMessage = "发生异常！";
            }
            return actionResponse;
        }
        public static async Task<Dictionary<string, AppConfigItemModel>> Get_AppConfig()
        {

            Dictionary<string, AppConfigItemModel> AppConfig = new Dictionary<string, AppConfigItemModel>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("select * from xQc_FILTER.[dbo].[_AppConfig]", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                foreach (DataRow item in dt.Rows)
                                {
                                    AppConfig.Add(item["Name"].ToString(), new AppConfigItemModel(
                                        int.Parse(item["Service"].ToString()) == 1,
                                        item["Name"].ToString(),
                                        item["Value"].ToString()
                                        ));
                                }
                            }
                        }
                        con.Close();
                    }
                }
                return AppConfig;
            }
            catch (Exception EX) { UTILS.WriteLine("配置列表读取失败" + EX.ToString()); return AppConfig; }
        }
        /// <summary>
        /// 运行自定义指令存储
        /// </summary>
        /// <param name="SpName"></param>
        /// <param name="CharName"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public static async Task<ActionResponseModel> SQL_Run_ActionSp(string SpName, string CharName, List<string> Parameters,string DstCharName="")
        {
            ActionResponseModel actionResponse = new ActionResponseModel();
            try
            {
                SqlConnection con = new SqlConnection(connectionstring);
                await con.OpenAsync();
                SqlCommand command = new SqlCommand("[xQc_FILTER].[dbo].[" + SpName + "]", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                //系统输入参数
                command.Parameters.Add("@SrcCharName", SqlDbType.NVarChar);
                command.Parameters["@SrcCharName"].Value = CharName;
                //系统输出参数
                command.Parameters.Add("@DstCharName", SqlDbType.NVarChar, 64);
                command.Parameters.Add("@SrcNoticeMessage", SqlDbType.NVarChar, -1);
                command.Parameters.Add("@DstNoticeMessage", SqlDbType.NVarChar, -1);
                command.Parameters.Add("@AllNoticeMessage", SqlDbType.NVarChar, -1);
                command.Parameters.Add("@SilkChangeType", SqlDbType.Int);
                command.Parameters.Add("@ExpelType", SqlDbType.Int);
                command.Parameters.Add("@SrcAddSLB", SqlDbType.Int);
                command.Parameters.Add("@DstAddSLB", SqlDbType.Int);
                command.Parameters["@DstCharName"].Direction = ParameterDirection.InputOutput;
                command.Parameters["@SrcNoticeMessage"].Direction = ParameterDirection.Output;
                command.Parameters["@DstNoticeMessage"].Direction = ParameterDirection.Output;
                command.Parameters["@AllNoticeMessage"].Direction = ParameterDirection.Output;
                command.Parameters["@SilkChangeType"].Direction = ParameterDirection.Output;
                command.Parameters["@ExpelType"].Direction = ParameterDirection.Output;
                command.Parameters["@SrcAddSLB"].Direction = ParameterDirection.Output;
                command.Parameters["@DstAddSLB"].Direction = ParameterDirection.Output;

                command.Parameters["@DstCharName"].Value = DstCharName;
                //自定义参数
                for (int i = 0; i < Parameters.Count; i++)
                {
                    command.Parameters.Add("@Parameter" + (i + 1), SqlDbType.VarChar, 64);
                    command.Parameters["@Parameter" + (i + 1)].Value = Parameters[i];
                }

                command.ExecuteNonQuery();
                //int num = (int)command.Parameters["@return"].Value;
                con.Close();
                actionResponse.DstCharName = command.Parameters["@DstCharName"].Value.ToString();
                actionResponse.SrcNoticeMessage = command.Parameters["@SrcNoticeMessage"].Value.ToString();
                actionResponse.DstNoticeMessage = command.Parameters["@DstNoticeMessage"].Value.ToString();
                actionResponse.AllNoticeMessage = command.Parameters["@AllNoticeMessage"].Value.ToString();
                actionResponse.SilkChangeType = int.Parse(command.Parameters["@SilkChangeType"].Value.ToString());
                actionResponse.ExpelType = int.Parse(command.Parameters["@ExpelType"].Value.ToString());
                actionResponse.SrcAddSLB = int.Parse(command.Parameters["@SrcAddSLB"].Value.ToString());
                actionResponse.DstAddSLB = int.Parse(command.Parameters["@DstAddSLB"].Value.ToString());
            }
            catch (Exception e)
            {
                string ParametersText = "";
                foreach (var item in Parameters)
                {
                    ParametersText += item + "|";
                }
                UTILS.WriteLine($"玩家[{CharName}]执行[{SpName}]存储,传入参数[{ParametersText}]发生异常:" + e.ToString(), UTILS.LOG_TYPE.Warning);
                actionResponse.SrcNoticeMessage = "RunSp发生异常！";
            }
            return actionResponse;
        }
        /// <summary>
        /// 运行自定义限制存储
        /// </summary>
        /// <param name="SpName"></param>
        /// <param name="CharName"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public static async Task<ActionResponseModel> SQL_Run_LimitSp(string SpName, string CharName, string Buff, string HWID)
        {
            ActionResponseModel actionResponse = new ActionResponseModel();
            try
            {
                SqlConnection con = new SqlConnection(connectionstring);
                await con.OpenAsync();
                SqlCommand command = new SqlCommand("[xQc_FILTER].[dbo].[" + SpName + "]", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                //系统输入参数
                command.Parameters.Add("@SrcCharName", SqlDbType.NVarChar);
                command.Parameters["@SrcCharName"].Value = CharName;
                command.Parameters.Add("@Buff", SqlDbType.NVarChar);
                command.Parameters["@Buff"].Value = Buff;
                command.Parameters.Add("@HWID", SqlDbType.NVarChar);
                command.Parameters["@HWID"].Value = HWID;
                //系统输出参数
                command.Parameters.Add("@SrcNoticeMessage", SqlDbType.NVarChar, -1);
                command.Parameters.Add("@AllNoticeMessage", SqlDbType.NVarChar, -1);
                command.Parameters.Add("@SilkChangeType", SqlDbType.Int);
                command.Parameters.Add("@ExpelType", SqlDbType.Int);
                command.Parameters.Add("@SrcAddSLB", SqlDbType.Int);
                command.Parameters.Add("@return", SqlDbType.Int);
                command.Parameters["@SrcNoticeMessage"].Direction = ParameterDirection.Output;
                command.Parameters["@AllNoticeMessage"].Direction = ParameterDirection.Output;
                command.Parameters["@SilkChangeType"].Direction = ParameterDirection.Output;
                command.Parameters["@ExpelType"].Direction = ParameterDirection.Output;
                command.Parameters["@SrcAddSLB"].Direction = ParameterDirection.Output;
                command.Parameters["@return"].Direction = ParameterDirection.ReturnValue;

                command.ExecuteNonQuery();
                //int num = (int)command.Parameters["@return"].Value;
                con.Close();
                actionResponse.SrcNoticeMessage = command.Parameters["@SrcNoticeMessage"].Value.ToString();
                actionResponse.AllNoticeMessage = command.Parameters["@AllNoticeMessage"].Value.ToString();
                actionResponse.SilkChangeType = int.Parse(command.Parameters["@SilkChangeType"].Value.ToString());
                actionResponse.ExpelType = int.Parse(command.Parameters["@ExpelType"].Value.ToString());
                actionResponse.SrcAddSLB = int.Parse(command.Parameters["@SrcAddSLB"].Value.ToString());
                actionResponse.DstCharName = command.Parameters["@return"].Value.ToString();
            }
            catch (Exception e)
            {
                string ParametersText = "";

                UTILS.WriteLine($"玩家[{CharName}]触发限制执行[{SpName}]存储,发生异常:" + e.ToString(), UTILS.LOG_TYPE.Warning);
                actionResponse.SrcNoticeMessage = "RunSp发生异常！";
            }
            return actionResponse;
        }

        /// <summary>
        /// 获取怪物击杀奖励列表
        /// </summary>
        /// <returns></returns>
        public static async Task<List<MonsterKillRewardModel>> Get_MonsterKillReward_List()
        {
            List<MonsterKillRewardModel> MonsterKillRewards = new List<MonsterKillRewardModel>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("select * from xQc_FILTER.[dbo].[_MonsterKillReward]", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                foreach (DataRow item in dt.Rows)
                                {
                                    MonsterKillRewardModel monsterKillReward = new MonsterKillRewardModel(
                                        int.Parse(item["Service"].ToString()),
                                        int.Parse(item["ID"].ToString()),
                                        item["Name"].ToString(),
                                        int.Parse(item["SilkOwnReward"].ToString()),
                                        int.Parse(item["SLBReward"].ToString()),
                                        int.Parse(item["RewardProbability"].ToString()),
                                        int.Parse(item["NoticeType"].ToString()),
                                        item["NoticeMessage"].ToString());
                                    MonsterKillRewards.Add(monsterKillReward);
                                }
                            }
                        }
                        con.Close();
                    }
                }
                return MonsterKillRewards;
            }
            catch (Exception EX) { UTILS.WriteLine("怪物击杀列表获取失败" + EX.ToString()); return MonsterKillRewards; }
        }
        /// <summary>
        /// 获取怪物击杀奖励
        /// </summary>
        /// <returns></returns>
        public static async Task<MonsterKillRewardModel> Get_MonsterKillRewardByID(int ID)
        {
            MonsterKillRewardModel MonsterKillRewards = null;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("select * from xQc_FILTER.[dbo].[_MonsterKillReward] where [ID]=" + ID, con))
                    {
                        await con.OpenAsync();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                if (dt.Rows.Count > 0)
                                {
                                    DataRow item = dt.Rows[0];
                                    MonsterKillRewards = new MonsterKillRewardModel(
                                           int.Parse(item["Service"].ToString()),
                                           int.Parse(item["ID"].ToString()),
                                           item["Name"].ToString(),
                                           int.Parse(item["SilkOwnReward"].ToString()),
                                           int.Parse(item["SLBReward"].ToString()),
                                           int.Parse(item["RewardProbability"].ToString()),
                                           int.Parse(item["NoticeType"].ToString()),
                                           item["NoticeMessage"].ToString());
                                }

                            }
                        }
                        con.Close();
                    }
                }
                return MonsterKillRewards;
            }
            catch (Exception EX) { UTILS.WriteLine($"怪物ID:{ID},奖励获取失败" + EX.ToString()); return MonsterKillRewards; }
        }
        /// <summary>
        /// 添加怪物击杀奖励
        /// </summary>
        /// <param name="MonsterKillReward"></param>
        /// <returns></returns>
        public static async Task<bool> Add_MonsterKillReward(MonsterKillRewardModel MonsterKillReward)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO xQc_FILTER.[dbo].[_MonsterKillReward] " +
                        "values(@Service, @ID, @Name, @SilkOwnReward, @SLBReward, @RewardProbability, @NoticeType, @NoticeMessage)", con))
                    {
                        await con.OpenAsync();
                        SqlParameter[] Parameters = {
                            new SqlParameter("@Service",MonsterKillReward.Service),
                            new SqlParameter("@ID",MonsterKillReward.ID),
                            new SqlParameter("@Name",MonsterKillReward.Name),
                            new SqlParameter("@SilkOwnReward",MonsterKillReward.SilkOwnReward),
                            new SqlParameter("@SLBReward",MonsterKillReward.SLBReward),
                            new SqlParameter("@RewardProbability",MonsterKillReward.RewardProbability),
                            new SqlParameter("@NoticeType",MonsterKillReward.NoticeType),
                            new SqlParameter("@NoticeMessage",MonsterKillReward.NoticeMessage),
                        };
                        cmd.Parameters.AddRange(Parameters);
                        bool x = await cmd.ExecuteNonQueryAsync() > 0;
                        con.Close();
                        return x;

                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("添加击杀奖励失败:" + EX.ToString()); return false; }
        }
        /// <summary>
        /// 修改怪物击杀奖励
        /// </summary>
        /// <param name="MonsterKillReward"></param>
        /// <returns></returns>
        public static async Task<bool> Update_MonsterKillReward(MonsterKillRewardModel MonsterKillReward)
        {
            List<NoticeModel> Notices = new List<NoticeModel>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("update xQc_FILTER.[dbo].[_MonsterKillReward]" +
                        " set [Service]=@Service,[Name]=@Name,[SilkOwnReward]=@SilkOwnReward,[SLBReward]=@SLBReward,[RewardProbability]=@RewardProbability,[NoticeType]=@NoticeType,[NoticeMessage]=@NoticeMessage" +
                        " where [ID]=@ID", con))
                    {
                        await con.OpenAsync();
                        SqlParameter[] Parameters = {
                            new SqlParameter("@Service",MonsterKillReward.Service),
                            new SqlParameter("@ID",MonsterKillReward.ID),
                            new SqlParameter("@Name",MonsterKillReward.Name),
                            new SqlParameter("@SilkOwnReward",MonsterKillReward.SilkOwnReward),
                            new SqlParameter("@SLBReward",MonsterKillReward.SLBReward),
                            new SqlParameter("@RewardProbability",MonsterKillReward.RewardProbability),
                            new SqlParameter("@NoticeType",MonsterKillReward.NoticeType),
                            new SqlParameter("@NoticeMessage",MonsterKillReward.NoticeMessage),
                        };
                        cmd.Parameters.AddRange(Parameters);
                        bool x = await cmd.ExecuteNonQueryAsync() > 0;
                        con.Close();
                        return x;

                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("修改击杀奖励失败:" + EX.ToString()); return false; }
        }
        /// <summary>
        /// 获取公告列表
        /// </summary>
        /// <returns></returns>
        public static async Task<List<NoticeModel>> Get_Notice_List()
        {
            List<NoticeModel> Notices = new List<NoticeModel>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("select * from xQc_FILTER.[dbo].[_Notice]", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                foreach (DataRow item in dt.Rows)
                                {
                                    NoticeModel Notice = new NoticeModel(
                                        int.Parse(item["ID"].ToString()),
                                        Convert.ToDateTime(item["StartDate"].ToString()),
                                        Convert.ToDateTime(item["EndDate"]),
                                        item["Content"].ToString(),
                                        item["Color"].ToString());

                                    Notices.Add(Notice);
                                }
                            }
                        }
                        con.Close();
                    }
                }
                return Notices;
            }
            catch (Exception EX) { UTILS.WriteLine("通知列表获取失败" + EX.ToString()); return Notices; }
        }
        /// <summary>
        /// 添加公告
        /// </summary>
        /// <param name="Notice"></param>
        /// <returns></returns>
        public static async Task<bool> Add_Notice(NoticeModel Notice)
        {
            List<NoticeModel> Notices = new List<NoticeModel>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO xQc_FILTER.[dbo].[_Notice] values(@StartDate,@EndDate,@Content,@Color)", con))
                    {
                        await con.OpenAsync();
                        SqlParameter[] Parameters = {
                            new SqlParameter("@StartDate",Notice.StartDateTime.ToString("yyyy-MM-dd HH:mm:ss")),
                            new SqlParameter("@EndDate",Notice.EndDateTime.ToString("yyyy-MM-dd HH:mm:ss")),
                            new SqlParameter("@Content",Notice.Content),
                            new SqlParameter("@Color",Notice.Color)

                        };
                        cmd.Parameters.AddRange(Parameters);
                        bool x = await cmd.ExecuteNonQueryAsync() > 0;
                        con.Close();
                        return x;

                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("添加公告失败:" + EX.ToString()); return false; }
        }
        /// <summary>
        /// 修改公告
        /// </summary>
        /// <param name="Notice"></param>
        /// <returns></returns>
        public static async Task<bool> Update_Notice(NoticeModel Notice)
        {
            List<NoticeModel> Notices = new List<NoticeModel>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("update xQc_FILTER.[dbo].[_Notice] set [StartDate]=@StartDate,[EndDate]=@EndDate,[Content]=@Content,[Color]=@Color where  [ID]=@ID", con))
                    {
                        await con.OpenAsync();
                        SqlParameter[] Parameters = {
                            new SqlParameter("@StartDate",Notice.StartDateTime.ToString("yyyy-MM-dd HH:mm:ss")),
                            new SqlParameter("@EndDate",Notice.EndDateTime.ToString("yyyy-MM-dd HH:mm:ss")),
                            new SqlParameter("@Content",Notice.Content),
                            new SqlParameter("@Color",Notice.Color),
                            new SqlParameter("@ID",Notice.ID),
                        };
                        cmd.Parameters.AddRange(Parameters);
                        bool x = await cmd.ExecuteNonQueryAsync() > 0;
                        con.Close();
                        return x;

                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("修改公告失败:" + EX.ToString()); return false; }
        }
        public static async Task<bool> Delete_Notice(int ID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("delete xQc_FILTER.[dbo].[_Notice] where [ID]=@ID", con))
                    {
                        await con.OpenAsync();
                        SqlParameter[] Parameters = {
                            new SqlParameter("@ID",ID),
                        };
                        cmd.Parameters.AddRange(Parameters);
                        bool x = await cmd.ExecuteNonQueryAsync() > 0;
                        con.Close();
                        return x;

                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("删除公告失败:" + EX.ToString()); return false; }
        }
        // is
        public static async Task<int> IS_CHARNAME16_Exists(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT CHARNAME16 FROM " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + INJECTION_PREFIX(CHARNAME16) + "';", con))
                    {
                        await con.OpenAsync();
                        int result = await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        return result;
                    }
                }
            }
            catch { UTILS.WriteLine("IS_CHARNAME16_Exists operation has has returned 0"); return 0; }
        }
        public static async Task<bool> IS_OneOrMoreCharsInAcc_WearJob_by_StrUserID(string StrUserID, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("(select CH.CHARNAME16 from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char as CH LEFT JOIN " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory as Inv ON CH.CharID=Inv.CharID LEFT JOIN " + Settings.ShardInfos[ShardID].DBName + ".dbo._User as US ON CH.CharID=US.CharID LEFT JOIN " + Settings.MSSQL_ACC_DB + ".dbo.TB_User as TUS ON US.UserJID=TUS.JID where TUS.StrUserID='" + StrUserID + "' AND Slot ='8' AND ItemID > 0)", con))
                    {
                        await con.OpenAsync();
                        string result = (string)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return !string.IsNullOrEmpty(result) ? true : false;
                    }
                }
            }
            catch { UTILS.WriteLine("IS_OneOrMoreCharsInAcc_WearJob_by_StrUserID returned false and failed."); return false; }
        }
        public static async Task<bool> IS_FellowSummoned_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("select top (1) CanBeVehicle from " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjChar where CanBeVehicle = 1 and ID =(select top (1) Link from " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjCommon where ID = (select top (1) RefCharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._CharCOS as A inner join " + Settings.ShardInfos[ShardID].DBName + ".dbo._Items as B on A.ID = B.Data and A.ID != 0 inner join " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory as C on B.ID64 = C.ItemID inner join " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char as D on C.CharID = D.CharID where D.CharName16 = N'" + CHARNAME16 + "' and A.[State] = 3 order by A.RentEndTime))", con))
                    {
                        await con.OpenAsync();
                        string result = Convert.ToString(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return !string.IsNullOrEmpty(result) ? true : false;
                    }
                }
            }
            catch { UTILS.WriteLine("IS_SummoningFellow_by_CHARNAME16 returned 0 and failed."); return false; }
        }
        public static async Task<int> IS_PetSummoned_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            try
            {//Not used anywhere in the code, it makes some problems
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("select count(ID) from " + Settings.ShardInfos[ShardID].DBName + ".dbo._CharCOS as A inner join " + Settings.ShardInfos[ShardID].DBName + ".dbo._Items as B on A.ID = B.Data and A.ID != 0 inner join " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory as C on B.ID64 = C.ItemID inner join " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char as D on C.CharID = D.CharID where D.CharName16 = N'" + CHARNAME16 + "' and A.[State] = 3", con))
                    {
                        await con.OpenAsync();
                        int result = (int)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return result;
                    }
                }
            }
            catch { UTILS.WriteLine("IS_SummoningAttackOrFellowPets_by_CHARNAME16 returned 0 and failed."); return 0; }
        }

        public static async Task<bool> IS_CosSummoned_by_CHARID(int CharID, int ShardID)
        {
            try
            {//Not used anywhere in the code, it makes some problems
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"select OwnerCharID from " + Settings.ShardInfos[ShardID].DBName + $".dbo._CharCOS WHERE RefCharID IN (SELECT [ID] FROM [{Settings.ShardInfos[ShardID].DBName}].[dbo].[_RefObjCommon] where TypeID1=1 and TypeID2=2 and TypeID3=3 and TypeID4=2 AND Service=1) AND OwnerCharID= {CharID}", con))
                    {
                        await con.OpenAsync();
                        string result = Convert.ToString(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return !string.IsNullOrEmpty(result) ? true : false;
                    }
                }
            }
            catch { UTILS.WriteLine("IS_SummoningAttackOrFellowPets_by_CHARNAME16 returned 0 and failed."); return false; }
        }
        public static async Task<bool> IS_BattleField_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("select B.IsBattleField from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char as A inner join " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefRegion as B on A.LatestRegion = B.wRegionID where CharName16 = N'" + CHARNAME16 + "'", con))
                    {
                        await con.OpenAsync();
                        int result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return result == 0 ? true : false;
                    }
                }
            }
            catch { UTILS.WriteLine("IS_BattleField_by_CHARNAME16 returned false and failed."); return false; }
        }
        public static async Task<int> IS_TransOrVehicleContainsGoods_by_CHARNAME16(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("select count(ItemID) from [" + Settings.ShardInfos[ShardID].DBName + "].[dbo].[_InvCOS] where COSID = (select ID from [" + Settings.ShardInfos[ShardID].DBName + "].[dbo].[_CharCOS] where OwnerCharID = (select CharID from " + Settings.ShardInfos[ShardID].DBName + ".[dbo].[_Char] where CharName16 = N'" + CHARNAME16 + "')) and ItemID != 0", con))
                    {
                        await con.OpenAsync();
                        int result = (int)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return result;
                    }
                }
            }
            catch { UTILS.WriteLine("IS_TransOrVehicleContainsGoods_by_CHARNAME16 returned 0 and failed."); return 0; }
        }
        public static async Task<bool> IS_CHARNAME16_Has_Online_Record(string CHARNAME16)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT CHARNAME16 FROM xQc_FILTER.dbo._Players where CharName16 = N'" + CHARNAME16 + "'; ", con))
                    {
                        await con.OpenAsync();
                        string result = Convert.ToString(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return string.IsNullOrEmpty(result) ? false : true;
                    }
                }
            }
            catch { UTILS.WriteLine("IS_CHARNAME16_Has_Online_Record returned false and failed."); return false; }
        }
        public static async Task<bool> Is_StackRecord_Exists(string CharName16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select Count(LastSoldStack) from xQc_FILTER.dbo._ThievesStackRecords where CharID = (Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CharName16 + "');", con))
                    {
                        await con.OpenAsync();
                        int result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return result != 0 ? true : false;
                    }
                }
            }
            catch { UTILS.WriteLine("Is_StackRecord_Exists returned false and failed."); return false; }
        }
        public static async Task<bool> IS_CHARNAME16_VIP(string CHARNAME16)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT top 1 GivenTime FROM xQc_FILTER.dbo._VIP where CharName16 = N'" + CHARNAME16 + "' order by GivenTime desc; ", con))
                    {
                        await con.OpenAsync();
                        string result = Convert.ToString(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return !string.IsNullOrEmpty(result) ? (DateTime.Parse(result) != null && DateTime.Now.Subtract(DateTime.Parse(result)).TotalDays >= 27) : false;
                    }
                }
            }
            catch { UTILS.WriteLine("IS_CHARNAME16_VIP returned false and failed."); return false; }
        }
        #endregion

        #region 功能相关

        /// <summary>
        /// 保存称号颜色
        /// </summary>
        /// <param name="CharName"></param>
        /// <param name="Color"></param>
        /// <param name="ShardID"></param>
        /// <returns></returns>
        public static async Task<bool> Exist_User(string UserName)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"select JID  from {Settings.MSSQL_ACC_DB}.[dbo].[TB_User] where StrUserID=@StrUserID", con))
                    {
                        cmd.Parameters.Add(new SqlParameter("@StrUserID", UserName));
                        await con.OpenAsync();
                        bool x = (await cmd.ExecuteReaderAsync()).HasRows;
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"查找账号时发生异常:{ex.ToString()}"); return true; }
        }
        public static async Task<bool> Has_GM_Priv(string UserName)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"select sec_content,sec_primary from {Settings.MSSQL_ACC_DB}.[dbo].[TB_User] where StrUserID=@StrUserID", con))
                    {
                        cmd.Parameters.Add(new SqlParameter("@StrUserID", UserName));
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                return Convert.ToInt32(reader[0]) == 1 && Convert.ToInt32(reader[1]) == 1;
                                //UTILS.WriteLine($"{reader[0]},{reader[1]},{reader[2]}");
                            }
                            reader.Close();
                            con.Close();
                            return true;
                        }
                      

                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"是否为GM账号:{ex.ToString()}"); return false; }
        }
        public static async Task<bool> Exist_Limit(int Code)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"select ID from [dbo].[_CustomActionLimit] where ActionCode=@ActionCode and Service=1", con))
                    {
                        cmd.Parameters.Add(new SqlParameter("@ActionCode", Code));
                        await con.OpenAsync();
                        bool x = (await cmd.ExecuteReaderAsync()).HasRows;
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"判断限制时发生异常:{ex.ToString()}"); return true; }
        }
        public static async Task<string> Get_Limit(int Code)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"select SpName from [dbo].[_CustomActionLimit] where ActionCode=@ActionCode and Service=1", con))
                    {
                        cmd.Parameters.Add(new SqlParameter("@ActionCode", Code));
                        await con.OpenAsync();
                        string x = Convert.ToString(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"获取限制时发生异常:{ex.ToString()}"); return String.Empty; }
        }
        public static async Task<List<ActionLimitModel>> Get_LimitList()
        {
            List<ActionLimitModel> actionLimits = new List<ActionLimitModel>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"select * from [dbo].[_CustomActionLimit]", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                foreach (DataRow item in dt.Rows)
                                {
                                    actionLimits.Add(new ActionLimitModel(
                                         int.Parse(item["Service"].ToString()),
                                        int.Parse(item["ID"].ToString()),
                                         int.Parse(item["ActionCode"].ToString()),
                                         item["Name"].ToString(),
                                        item["SpName"].ToString(),
                                        item["Introduce"].ToString()
                                        ));
                                }
                            }
                        }
                        con.Close();
                        return actionLimits;
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"获取限制列表时发生异常:{ex.ToString()}"); return actionLimits; }
        }
        public static async Task<bool> Save_Limit(int ID, int Service)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"update [dbo].[_CustomActionLimit] set Service=@Service where ID=@ID", con))
                    {
                        cmd.Parameters.Add(new SqlParameter("@ID", ID));
                        cmd.Parameters.Add(new SqlParameter("@Service", Service));
                        await con.OpenAsync();
                        bool x = await cmd.ExecuteNonQueryAsync() > 0;
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"保存限制时发生异常:{ex.ToString()}"); return false; }
        }
        public static async Task<List<Dictionary<string, string>>> GET_USERS_LIST()
        {
            List<Dictionary<string, string>> UserList = null;
            try
            {

                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"select StrUserID,Name,Address from {Settings.MSSQL_ACC_DB}.[dbo].[TB_User] where Email='bimbum' order by newid()", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                UserList = new List<Dictionary<string, string>>();
                                foreach (DataRow item in dt.Rows)
                                {
                                    Dictionary<string, string> User = new Dictionary<string, string>();
                                    User.Add("Username", item["StrUserID"].ToString());
                                    User.Add("Password", item["Name"].ToString());
                                    User.Add("Role", item["Address"].ToString());
                                    UserList.Add(User);
                                }
                                con.Close();
                                return UserList;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"查询机器人列表发生异常:{ex.ToString()}"); return UserList; }
        }
        public static async Task<bool> Reg_User(string UserName, string PassWord, string SPassWord, string Mac, string Address = "test")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"insert into {Settings.MSSQL_ACC_DB}.[dbo].[TB_User] (StrUserID, password, Name,Email,address,certificate_num) values (@StrUserID, @Password, @SPassWord,@HWID,@Address,@certificate_num)", con))
                    {
                        cmd.Parameters.Add(new SqlParameter("@StrUserID", UserName));
                        cmd.Parameters.Add(new SqlParameter("@Password", UTILS.UserMd5(PassWord)));
                        cmd.Parameters.Add(new SqlParameter("@SPassWord", SPassWord));
                        cmd.Parameters.Add(new SqlParameter("@HWID", Mac));
                        cmd.Parameters.Add(new SqlParameter("@Address", Address));
                        cmd.Parameters.Add(new SqlParameter("@certificate_num", await GetLastCertificateNum()));
                        await con.OpenAsync();
                        bool x = await cmd.ExecuteNonQueryAsync() > 0;
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"账号注册发生异常:{ex.ToString()}"); return false; }
        }
        public static async Task<bool> Update_User(string UserName, string PassWord, string SPassWord)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"update {Settings.MSSQL_ACC_DB}.[dbo].[TB_User] set password=@Password where StrUserID=@StrUserID and Name=@SPassWord", con))
                    {
                        cmd.Parameters.Add(new SqlParameter("@StrUserID", UserName));
                        cmd.Parameters.Add(new SqlParameter("@Password", UTILS.UserMd5(PassWord)));
                        cmd.Parameters.Add(new SqlParameter("@SPassWord", SPassWord));
                        await con.OpenAsync();
                        bool x = await cmd.ExecuteNonQueryAsync() > 0;
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"账号修改发生异常:{ex.ToString()}"); return false; }
        }
        public static async Task<List<string>> GET_BOTS_NAMES()
        {
            try
            {
                List<string> result = null;
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT [Name] FROM [xQc_FILTER].[dbo].[_MinionsNames] A WHERE NOT EXISTS (SELECT * FROM {Settings.MSSQL_ACC_DB}.[dbo].[TB_User] B  WHERE A.Name=B.[address])", con))
                    {
                        await con.OpenAsync();
                        //cmd.CommandTimeout = 60;
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            result = new List<string>();
                            while (await reader.ReadAsync())
                            {
                                result.Add(reader[0].ToString());
                                //UTILS.WriteLine($"{reader[0]},{reader[1]},{reader[2]}");
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return result;
            }
            catch (Exception EX) { UTILS.WriteLine($"GET_BOTS_NAMES failed!! {EX}"); UTILS.ExportLog("GET_BOTS_NAMES()", EX); return null; }
        }
        public static async Task<object[]> FETCH_NOTICE()
        {
            try
            {
                object[] result = null;
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select Top (1) [ID],[Message],[CHARNAME16] ,[SLBUpdate],[SilkUpdate],[IsBreak],[ShardID] from xQc_FILTER.dbo._AutoNotice with (nolock) where [Sent] = 0 order by [Time] desc;", con))
                    {
                        await con.OpenAsync();
                        //cmd.CommandTimeout = 60;
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                result = new object[7] { reader[0], reader[1], reader[2], reader[3], reader[4], reader[5], reader[6] };
                                //UTILS.WriteLine($"{reader[0]},{reader[1]},{reader[2]}");
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return result;
            }
            catch (Exception EX) { UTILS.WriteLine("FETCH_NOTICE operation has returned NULL"); UTILS.ExportLog("FETCH_NOTICE()", EX); return null; }
        }
        public static async Task NOTICE_UPDATE_STATUS(int NoticeID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE xQc_FILTER.dbo._AutoNotice SET Sent = 1 WHERE ID = " + NoticeID + ";", con))
                    {
                        await con.OpenAsync();//sending a beam of search, to find a CharID to teleport upon conditions met.
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch { UTILS.WriteLine("NOTICE_UPDATE_STATUS operation has has returned false"); }
        }
        public static async Task LOG_GLOBAL_CHAT(string Sender, string Content)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO xQc_FILTER.dbo._LogGlobalChat (Sender,Content,TimeSent) VALUES ('" + INJECTION_PREFIX(Sender) + "','" + INJECTION_PREFIX(Content) + "',GETDATE())", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch { UTILS.WriteLine("LOG_GLOBAL_CHAT operation has failed."); }
        }
        public static async Task LOG_COMMON_CHAT(string TableName, string Sender, string Content)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO xQc_FILTER.dbo." + TableName + " (Sender,Content,TimeSent) VALUES (N'" + INJECTION_PREFIX(Sender) + "',N'" + INJECTION_PREFIX(Content) + "',GETDATE())", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine("LOG_COMMON_CHAT operation has failed." + ex.ToString()); }
        }
        public static async Task LOG_PM_CHAT(string Sender, string To, string Content)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO xQc_FILTER.dbo._LogPMChat (Sender,[To],Content,TimeSent) VALUES ('" + Sender + "','" + INJECTION_PREFIX(To) + "','" + INJECTION_PREFIX(Content) + "',GETDATE())", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch { UTILS.WriteLine("LOG_PM_CHAT operation has failed."); }
        }
        public static async Task LOG_LOADIMAGE_TS(string TableName, string Sender, long delay)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand iq = new SqlCommand("INSERT INTO xQc_FILTER.dbo." + TableName + " (CHARNAME16,LoadingTimeSpanMS,TimeSent) VALUES ('" + INJECTION_PREFIX(Sender) + "','" + delay + "',GETDATE())", con))
                    {
                        await con.OpenAsync();
                        await iq.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine("LOG_LOADIMAGE_TS operation has failed." + ex.ToString()); }
        }
        public static async Task LOG_MAGICPOP_STATS(string TableName, string CHARNAME16, string WonOrLost)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO xQc_FILTER.dbo." + TableName + " (CHARNAME16,WonOrLost,TimeSent) VALUES ('" + CHARNAME16 + "','" + WonOrLost + "',GETDATE())", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine("LOG_MAGICPOP_STATS operation has failed." + ex.ToString()); }
        }
        public static async Task LOG_UNQ_KILLS(string KillerName, string UniqueName)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO xQc_FILTER.dbo._LogUniqueKills (KillerName,UniqueName,KilledTime) values ('" + KillerName + "','" + UniqueName + "',GETDATE())", con))
                    {
                        await con.OpenAsync();
                        int result = await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch { UTILS.WriteLine("LOG_UNQ_KILLS returned false and failed."); }
        }
        public static async Task LOG_PLAYER_STATUS(string CHARNAME16, string IP, string HWID, bool cur_status, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {

                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO xQc_FILTER.dbo._Players (CHARNAME16,StrUserID,[ip],mac,cur_status,last_seen) values (N'" +
                        CHARNAME16 + "',(SELECT StrUserID FROM  " + Settings.MSSQL_ACC_DB + ".dbo.TB_User WHERE JID = (SELECT UserJID FROM " +
                        Settings.ShardInfos[ShardID].DBName + ".dbo._User WHERE CharID = (Select CharID from " + Settings.ShardInfos[ShardID].DBName +
                        ".dbo._Char where CharName16 = N'" + CHARNAME16 + "'))),'" + IP + "','" + HWID + "',1,GETDATE());", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX)
            {
                UTILS.WriteLine("LOG_PLAYER_STATUS operation has failed." + EX.ToString());

            }
        }
        public static async Task UPDATE_PLAYER_STATUS(string CHARNAME16, bool status)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE xQc_FILTER.dbo._Players SET cur_status = " + (status ? 1 : 0) + " WHERE CharName16 = N'" + CHARNAME16 + "';", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch { UTILS.WriteLine("UPDATE_PLAYER_STATUS operation has failed."); }
        }
        public static async Task INSERT_PLAYER_STATISTICS(string CharName16, string IP, string HWID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO xQc_FILTER.dbo._CharsStatistics ([IP],[HWID],[CharName16],[CurrentTime]) values ('" + IP + "','" + HWID + "','" + CharName16 + "',GETDATE());", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch { UTILS.WriteLine("INSERT_PLAYER_STATISTICS operation has failed."); }
        }
        public static async Task DELETE_SilkChange_BY_Web_Records()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM " + Settings.MSSQL_ACC_DB + ".dbo.SK_SilkChange_BY_Web;", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch { UTILS.WriteLine("DELETE_SilkChange_BY_Web_Records operation has failed."); }
        }
        public static async Task<int> DELETE_ITEM_FROM_INVENTORY(string CHARNAME16, int Slot, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE " + Settings.ShardInfos[ShardID].DBName + ".dbo._Inventory SET ItemID = 0 WHERE Slot = " + Slot + " AND CharID = ( SELECT CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "')", con))
                    {
                        await con.OpenAsync();
                        int result = await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        return result;
                    }
                }
            }
            catch { UTILS.WriteLine("DELETE_ITEM_FROM_INVENTORY operation has has returned 0"); return 0; }
        }
        
        public static async Task UPDATE_HWANLEVEL_CHAR(string CHARNAME16, int hwanlevel, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char set HwanLevel = '" + hwanlevel + "' where CharName16 = N'" + CHARNAME16 + "';", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine("UPDATE_HWANLEVEL_Char operation has failed." + ex.ToString()); }
        }
        public static async Task FILL_ITEMPOOL_TABLE(string codename, string realname)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO xQc_FILTER.dbo._ItemPoolName ([CodeName], [RealName]) VALUES ('" + codename + "','" + realname + "');", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine(EX.ToString()); }
        }
        public static async Task Update_Stack_Record(string CharName16, long value)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE xQc_FILTER.dbo._ThievesStackRecords set LastSoldStack = '" + value + "' where CharID = (Select CharID from SRO_VT_SHARD.dbo._Char where CharName16 = N'" + CharName16 + "');", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch { UTILS.WriteLine("Update_Stack_Record returned false and failed."); }
        }
        public static async Task Insert_Stack_Record(string CharName16, long value, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO xQc_FILTER.dbo._ThievesStackRecords (CharID,LastSoldStack) values ((Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CharName16 + "'),'" + value + "');", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch { UTILS.WriteLine("Insert_Stack_Record returned false and failed."); }
        }
        public static async Task LOAD_UNIQUES_TABLE(DataGridView dgv, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select ID,NameStrID128 from " + Settings.ShardInfos[ShardID].DBName + ".dbo._RefObjCommon where Rarity = 3 and CodeName128 LIKE '%MOB%' AND Service = 1;", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                dgv.DataSource = dt;
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception AnyEX) { UTILS.WriteLine("Could not load uniques table, " + AnyEX.Message); }
        }
        #endregion

        #region MISC Queries - GUI workarounds
        public static async Task<string> GIVE_SILK(string StrUserID, int SilkAmount)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("EXEC " + Settings.MSSQL_ACC_DB + ".CGI.CGI_WebPurchaseSilk '" + await Get_LastOrderID() + "','" + StrUserID + "','1','" + SilkAmount + "',1", con))
                    {
                        await con.OpenAsync();
                        string result = (string)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return result;
                    }
                }
            }
            catch { UTILS.WriteLine("GIVE_SILK opeation has has returned string.Empty"); return string.Empty; }
        }
        public static async Task<string> GIVE_TROPHIES(string StrUserID, int amount)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("EXEC " + Settings.MSSQL_ACC_DB + ".CGI.CGI_WebPurchaseGift '" + await Get_LastOrderID() + "','" + StrUserID + "','1','" + amount + "',1", con))
                    {
                        await con.OpenAsync();
                        string result = (string)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return result;
                    }
                }
            }
            catch { UTILS.WriteLine("GIVE_TROPHIES opeation has has returned string.Empty"); return string.Empty; }
        }
        //public static async Task<int> SILK_BALANCE(string CHARNAME16, int ShardID)
        //{
        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(connectionstring))
        //        {
        //            using (SqlCommand cmd = new SqlCommand("(Select silk_own from " + Settings.MSSQL_ACC_DB + ".dbo.SK_Silk where JID = (Select UserJID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._User where CharID = (Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "')))", con))
        //            {
        //                await con.OpenAsync();
        //                int sb = (int)await cmd.ExecuteScalarAsync();
        //                con.Close();
        //                return sb;
        //            }
        //        }
        //    }
        //    catch(Exception ex) { UTILS.WriteLine($"SILK_BALANCE operation has has returned:{ex.ToString()}"); return 0; }
        //}
        public static async Task<bool> UpdateSilk(string CHARNAME16, int ShardID, int Type, int SilkOwn, int SilkGift = 0, int SilkPoint = 0)
        {
            string sql = "";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    string 运算 = Type == 1 ? "+=" : "-=";
                    sql = "update " + Settings.MSSQL_ACC_DB + $".dbo.SK_Silk set [silk_own]{运算}{SilkOwn},[silk_gift]{运算}{SilkGift},[silk_point]{运算}{SilkPoint} where JID = (Select UserJID from " +
                        Settings.ShardInfos[ShardID].DBName + ".dbo._User where CharID = (Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "'))";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        await con.OpenAsync();
                        bool x = await cmd.ExecuteNonQueryAsync() > 0;
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"UpdateSilk operation has has returned{sql}{ex.ToString()}"); return false; }
        }
        public static async Task<bool> 扣费(string CHARNAME16,long Num, int Type, int ShardID)
        {
            string sql = "";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    switch (Type)
                    {
                        case 0:
                            sql = "update " + Settings.MSSQL_ACC_DB + $".dbo.SK_Silk set [silk_own]-={Num} where JID = (Select UserJID from " +
                       Settings.ShardInfos[ShardID].DBName + ".dbo._User where CharID = (Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "'))";
                            break;
                        case 1:
                            sql = $"UPDATE {Settings.ShardInfos[ShardID].DBName}.dbo._Char set RemainGold -={Num} where CharName16 = N'" + CHARNAME16 + "';";
                            break;
                        case 2:
                            sql = "update " + Settings.MSSQL_ACC_DB + $".dbo.SK_Silk set [silk_point]-={Num} where JID = (Select UserJID from " +
              Settings.ShardInfos[ShardID].DBName + ".dbo._User where CharID = (Select CharID from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "'))";
                            break;
                        default:
                            throw new Exception("货币类型不存在");
                            break;
                    }
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        await con.OpenAsync();
                        bool x = await cmd.ExecuteNonQueryAsync() > 0;
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"扣费 has returned{sql}{ex.ToString()}"); return false; }
        }
        public static async Task<LoginRewardModel> LoginReward(string CHARNAME16)
        {
            LoginRewardModel loginRewardModel = null;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("_LoginReward", con))
                    {
                        await con.OpenAsync();
                        cmd.CommandType = CommandType.StoredProcedure;
                        //系统输入参数
                        cmd.Parameters.Add("@CharName", SqlDbType.NVarChar, -1);
                        cmd.Parameters["@CharName"].Value = CHARNAME16;
                        //系统输出参数
                        cmd.Parameters.Add("@SilkReward", SqlDbType.Int);
                        cmd.Parameters.Add("@SLBReward", SqlDbType.Int);
                        cmd.Parameters.Add("@return", SqlDbType.Int);
                        cmd.Parameters["@SilkReward"].Direction = ParameterDirection.Output;
                        cmd.Parameters["@SLBReward"].Direction = ParameterDirection.Output;
                        cmd.Parameters["@return"].Direction = ParameterDirection.ReturnValue;
                        await cmd.ExecuteNonQueryAsync();
                        loginRewardModel = new LoginRewardModel(
                            int.Parse(cmd.Parameters["@return"].Value.ToString()),
                            int.Parse(cmd.Parameters["@SilkReward"].Value.ToString()),
                            int.Parse(cmd.Parameters["@SLBReward"].Value.ToString())
                            );
                        con.Close();
                        return loginRewardModel;
                    }
                }
            }
            catch { UTILS.WriteLine("SILK_BALANCE operation has has returned 0"); return loginRewardModel; }
        }

        public static async Task<int> GIVE_GOLD(string CHARNAME16, long GoldAmount, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char set RemainGold = RemainGold+" + GoldAmount + " where CharName16 = N'" + CHARNAME16 + "';", con))
                    {
                        await con.OpenAsync();
                        int result = await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        return result;
                    }
                }
            }
            catch { UTILS.WriteLine("GIVE_GOLD operation has has returned 0"); return 0; }
        }
        public static async Task<long> GOLD_BALANCE(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select RemainGold from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "';", con))
                    {
                        await con.OpenAsync();
                        long result = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return result;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("GOLD_BALANCE operation has has returned 0" + EX.ToString()); return 0; }
        }
        public static async Task<int> GIVE_SP(string CHARNAME16, int SPAmount, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char set RemainSkillPoint = RemainSkillPoint+" + SPAmount + " where CharName16 = N'" + CHARNAME16 + "';", con))
                    {
                        con.Open();
                        int result = await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        return result;
                    }
                }
            }
            catch { UTILS.WriteLine("GIVE_SP operation has has returned 0"); return 0; }
        }
        public static async Task<int> SP_BALANCE(string CHARNAME16, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Select RemainSkillPoint from " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char where CharName16 = N'" + CHARNAME16 + "';", con))
                    {
                        await con.OpenAsync();
                        int result = (int)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return result;
                    }
                }
            }
            catch { UTILS.WriteLine("SP_BALANCE operation has has returned 0"); return 0; }
        }
        public static async Task RESET_POS_ALL(int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char set LatestRegion='25000',PosX='985.1272',PosY='0.903694',PosZ='1585.906';", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch { UTILS.WriteLine("RESET_POS_ALL operation has failed"); }
        }
        public static async Task SET_CLIENTLESS_LOCATION_TO_JANGAN_SOUTH(int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE " + Settings.ShardInfos[ShardID].DBName + ".dbo._Char set LatestRegion='24744',PosX='1071',PosY='-9.72171',PosZ='1379' where CharName16 = N'" + Settings.CL_CharName + "';", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch { UTILS.WriteLine("SET_CLIENTLESS_LOCATION_TO_JANGAN_SOUTH operation has failed"); }
        }
        public static async Task REMOTE_CMD(string remotecmd)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand(remotecmd, con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch { }
        }
        #endregion

        public static async void LoadRefSkill(int ShardID = 64) {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT [ID],[Basic_Code] TypeName FROM {Settings.ShardInfos[ShardID].DBName}.[dbo].[_RefSkill] where Service=1", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                foreach (DataRow item in dt.Rows)
                                {
                                    GameInfo.Skills.id.Add(Convert.ToUInt32(item["ID"]));
                                    GameInfo.Skills.type.Add(item["TypeName"].ToString());
                                }
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"LoadRefSkill:{ex.ToString()}"); }
        }
        public static async void LoadItems(int ShardID = 64)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT c.[ID] ID,c.[CodeName128] TypeName FROM {Settings.ShardInfos[ShardID].DBName}.[dbo].[_RefObjCommon] c,{Settings.ShardInfos[ShardID].DBName}.[dbo].[_RefObjItem] i where c.Service=1 and CodeName128 like'Item%' and c.Link=i.ID", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                foreach (DataRow item in dt.Rows)
                                {
                                    GameInfo.Items.id.Add(Convert.ToUInt32(item["ID"]));
                                    GameInfo.Items.type.Add(item["TypeName"].ToString());
                                }
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"LoadItems:{ex.ToString()}"); }
        }
        public static async void LoadMobs(int ShardID = 64)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT c.[ID] ID,c.[CodeName128] TypeName FROM {Settings.ShardInfos[ShardID].DBName}.[dbo].[_RefObjCommon] c,{Settings.ShardInfos[ShardID].DBName}.[dbo].[_RefObjChar] i where c.Service=1 and not CodeName128 like'Item%' and c.Link=i.ID", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                foreach (DataRow item in dt.Rows)
                                {
                                    GameInfo.Mobs.id.Add(Convert.ToUInt32(item["ID"]));
                                    GameInfo.Mobs.type.Add(item["TypeName"].ToString());
                                }
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"LoadMobs:{ex.ToString()}"); }
        }
        public static async Task<string> 获取职业称号(int 职业等级,int 职业类型)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT [TradeTitle],[ThiefTitle],[DartsTitle] FROM [xQc_FILTER].[dbo].[_CustomJobTitle] where [JobLevel]={职业等级}", con))
                    {
                        await con.OpenAsync();
                        SqlDataReader sqlReader= await cmd.ExecuteReaderAsync();
                        sqlReader.Read();
                        if (职业类型 > 0 && 职业类型 < 4)
                        {
                            con.Close();
                            return sqlReader[职业类型 - 1].ToString();
                        }
                        else {
                            con.Close();
                            throw new Exception($"尝试查询不存在的类型:{职业类型}");
                        }
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"获取职业称号异常:{ex.ToString()}");return String.Empty; }
        }
        public static async Task UpdateHonorRanking(int ShardID = 64)
        {
            await SQL_Run_Code($"exec {Settings.ShardInfos[ShardID].DBName}.[dbo].[_TRAINING_CAMP_UPDATEHONORRANK]");
        }
        public static async Task<List<ReputationRankModel>> 获取名誉排行(int ShardID = 64)
        {
            List<ReputationRankModel> ReputationRanks = new List<ReputationRankModel>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"select TR.Ranking,TR.[Rank],TM.CharName,TM.CharCurLevel,TM.CharMaxLevel,TC.GraduateCount,(select Name from {Settings.ShardInfos[ShardID].DBName}.[dbo]._Guild where ID =(select GuildID from {Settings.ShardInfos[ShardID].DBName}.[dbo]._Char where CharID = TM.CharID)) as GuildName,(select GuildID from {Settings.ShardInfos[ShardID].DBName}.[dbo]._Char where CharID = TM.CharID) as GuildID from {Settings.ShardInfos[ShardID].DBName}.[dbo]._TrainingCampHonorRank as TR,{Settings.ShardInfos[ShardID].DBName}.[dbo]._TrainingCampMember as TM,{Settings.ShardInfos[ShardID].DBName}.[dbo]._TrainingCamp as TC where TR.CampID = TM.CampID and TC.ID = TR.CampID and TM.MemberClass=0 order by TR.Ranking", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                
                                foreach (DataRow item in dt.Rows)
                                {
                                    ReputationRanks.Add(new ReputationRankModel(
                                        Convert.ToInt32(item["Ranking"]),
                                        Convert.ToInt32(item["Rank"]),
                                        item["CharName"].ToString(),
                                        Convert.ToInt32(item["CharCurLevel"]),
                                        Convert.ToInt32(item["CharMaxLevel"]),
                                        Convert.ToInt32(item["GraduateCount"]),
                                        item["GuildName"].ToString(),
                                        Convert.ToInt32(item["GuildID"])
                                        ));
                                }
                                return ReputationRanks;
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception ex) { UTILS.WriteLine($"获取名誉排行失败:{ex.ToString()}"); return ReputationRanks; }
        }

        public static async Task<int[]> ReturnType3AndOptlevel(int CharID, int Slot,int ShardID)
        {
            try
            {
                int[] temp = null;
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT a.TypeID3, b.OptLevel FROM {Settings.ShardInfos[ShardID].DBName}.._RefObjCommon a JOIN {Settings.ShardInfos[ShardID].DBName}.._Items b ON a.ID = b.RefItemID JOIN {Settings.ShardInfos[ShardID].DBName}.._Inventory c ON b.ID64 = c.ItemID WHERE c.CharID = {CharID} AND c.Slot = {Slot}", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader Utils = await cmd.ExecuteReaderAsync())
                        {
                            while (await Utils.ReadAsync())
                            {
                                temp = new int[2] { Convert.ToInt32(Utils[0]), Convert.ToInt32(Utils[1])};
                            }
                            Utils.Close();
                        }
                        con.Close();
                    }
                }
                return temp;
            }
            catch (Exception EX) { UTILS.WriteLine("Get_Optlvl_by_SlotAndCharName16 returned 0 and failed." + EX.ToString()); return null; }
        }
        public static async Task<bool> GM账号登录(string UserName,string PassWord) {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"select count(JID) from {Settings.MSSQL_ACC_DB}.[dbo].[TB_User] where StrUserID='{UserName}' and password='{UTILS.UserMd5(PassWord)}' and sec_primary=1 and sec_content=1", con))
                    {
                        await con.OpenAsync();
                        bool x = ((int)await cmd.ExecuteScalarAsync()) > 0;
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine("移动端尝试登陆GM账号报错:" + EX.ToString()); return false; }
        }

        public static async Task<bool> loadCustomTitleList()
        {
            try
            {
                UTILS.CustomTitleList.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select * from xQc_FILTER.dbo._CustomTitle", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string charname = Convert.ToString(reader[0]);
                                string Title = Convert.ToString(reader[1]);
                                byte isActive = Convert.ToByte(reader[2]);
                                int ShardID = Convert.ToInt32(reader[3]);

                                _CustomTitle Custom = new _CustomTitle();
                                _CustomTitle Custom2 = new _CustomTitle();

                                Custom = UTILS.CustomTitleList.Find(x => x.CharName == charname && x.ShardID == ShardID);
                                if(Custom == null)
                                {
                                    Custom2.CharName = charname;
                                    Custom2.Titles.TryAdd(Title, isActive);
                                    Custom2.ShardID = ShardID;
                                    UTILS.CustomTitleList.Add(Custom2);
                                }
                                else
                                    Custom.Titles.TryAdd(Title, isActive);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateCustomTitle returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> LoadHwanList(AGENT_MODULE Session)
        {
            try
            {
                Session.CharTitles.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select CH70,EU70 from xQc_FILTER.dbo._TitleStorage where CharName = '{Session.CHARNAME16}' and ShardID = {Session.ShardID};", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string ch70 = Convert.ToString(reader[0]);
                                string eu70 = Convert.ToString(reader[1]);
                                if (!Session.CharTitles.ContainsKey(Session.CHARNAME16))
                                    Session.CharTitles.TryAdd(Session.CHARNAME16, new List<string>());
                                if (Session.ISEURO)
                                    Session.CharTitles[Session.CHARNAME16].Add(eu70);
                                else
                                    Session.CharTitles[Session.CHARNAME16].Add(ch70);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateHwanList returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<string> UpdateCurrentActiveTitle(string Charname, string Title)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"UPDATE xQc_FILTER.dbo._CustomTitle set isActive =0 where CharName = '{Charname}';UPDATE xQc_FILTER.dbo._CustomTitle set isActive =1 where CharName = '{Charname}' and Title = '{UTILS.INJECTION_PREFIX(Title)}'", con))
                    {
                        await con.OpenAsync();
                        string x = Convert.ToString(await cmd.ExecuteScalarAsync()); 
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"_TitleUpdateCurrentActive opeation failed and returned string.Empty: {EX}", UTILS.LOG_TYPE.Fatal) ; return string.Empty; }
        }
        public static async Task<bool> LoadTitleColorsList()
        {
            try
            {
                UTILS.titlescolors.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select * from xQc_FILTER.dbo._CustomTitleColor", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string charname = Convert.ToString(reader[0]);
                                string colorcode = Convert.ToString(reader[1]);
                                int ShardID = Convert.ToInt32(reader[2]);
                                uint color = UInt32.Parse(colorcode.Replace("#", ""), NumberStyles.HexNumber);
                                _CustomTitleColor Custom = new _CustomTitleColor();
                                Custom.CharName = charname;
                                Custom.Color = color;
                                Custom.ShardID = ShardID;
                                UTILS.titlescolors.Add(Custom);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadTitleColorsList returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<string> AddCustomTitle(string Charname, string Title,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"UPDATE xQc_FILTER.dbo._CustomTitle set isActive =0 where CharName ='{Charname}' and ShardID = {ShardID};INSERT INTO xQc_FILTER.dbo._CustomTitle VALUES('{Charname}','{UTILS.INJECTION_PREFIX(Title)}',1,{ShardID})", con))
                    {
                        await con.OpenAsync();
                        string x = Convert.ToString(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"AddCustomTitle opeation failed and returned string.Empty: {EX}",UTILS.LOG_TYPE.Fatal); return string.Empty; }
        }
        public static async Task ADD_NEW_LOCATION(int charid, byte Index, int region, int x, int y, int z, string location,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"INSERT INTO xQc_FILTER.dbo._SavedLocations VALUES ({charid},{Index},{region},{x},{y},{z},'{UTILS.INJECTION_PREFIX(location)}',{ShardID})", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"ADD_NEW_LOCATION returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); }
        }
        public static async Task REMOVE_LOCATION(int charid, byte Index,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"DELETE FROM xQc_FILTER.dbo._SavedLocations where CharID = {charid} and BIndex = {Index} and ShardID = {ShardID}", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"REMOVE_LOCATION returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); }
        }
        public static async Task<bool> LOAD_SAVED_LOCATIONS(AGENT_MODULE Session = null)
        {
            try
            {
                Session.SavedLocations.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT * FROM xQc_FILTER.dbo._SavedLocations WHERE CharID = {Session.CharID} and ShardID = {Session.ShardID}", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                byte BIndex = Convert.ToByte(reader[2]);
                                ushort region = Convert.ToUInt16(reader[3]);
                                int x = Convert.ToInt32(reader[4]);
                                int y = Convert.ToInt32(reader[5]);
                                int z = Convert.ToInt32(reader[6]);
                                string location = Convert.ToString(reader[7]);

                                SavedLocation savedlocation = new SavedLocation
                                {
                                    region = region,
                                    x = x,
                                    y = y,
                                    z = z,
                                    locationName = location
                                };

                                Session.SavedLocations.Add(BIndex, savedlocation);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }

                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"LOAD_SAVED_LOCATIONS returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<int> GetRemainSlots(int CharID,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT Count(Slot) FROM {Settings.ShardInfos[ShardID].DBName}.dbo._Inventory WHERE CharID = {CharID} AND ItemID = 0 and Slot BETWEEN 13 AND 255", con))
                    {
                        await con.OpenAsync();
                        int x = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"GetRemainSlots returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return 0; }
        }
        public static async Task<bool> UpdateCharChest(AGENT_MODULE Session)
        {
            try
            {
                Session.CharChest.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 50 ROW_NUMBER() OVER(ORDER BY ID asc),* from xQc_FILTER.dbo._CharChest where CharID = '{Session.CharID}' and ShardID = {Session.ShardID};", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int linenum = Convert.ToInt32(reader[0]);
                                int ID = Convert.ToInt32(reader[1]);
                                uint RefItemID = Convert.ToUInt32(reader[3]);
                                int Count = Convert.ToInt32(reader[4]);
                                byte OptLevel = Convert.ToByte(reader[5]);
                                bool RandomStat = Convert.ToBoolean(reader[6]);
                                string From = Convert.ToString(reader[7]);
                                int ShardID = Convert.ToInt32(reader[8]);
                                DateTime RegTime = Convert.ToDateTime(reader[9]);
                                if (!Session.CharChest.ContainsKey(ID))
                                {
                                    _CharChest Chest = new _CharChest
                                    {
                                        LineNum = linenum,
                                        RefItemID = RefItemID,
                                        Count = Count,
                                        OptLevel = OptLevel,
                                        RandomizedStats = RandomStat,
                                        From = From,
                                        ShardID = ShardID,
                                        RegisterTime = RegTime
                                    };
                                    Session.CharChest.Add(ID, Chest);
                                }
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateCharChest returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> LoadGuildRank(int ShardID)
        {
            try
            {
                UTILS.CustomPVPRank.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 50 ROW_NUMBER() OVER (ORDER BY ItemPoints DESC), Name,Lvl, ItemPoints FROM {Settings.ShardInfos[ShardID].DBName}.dbo._Guild WITH(NOLOCK) WHERE ItemPoints > 0 ORDER BY ItemPoints DESC", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                byte linenum = Convert.ToByte(reader[0]);
                                string GuildName = Convert.ToString(reader[1]);
                                string GuildLevel = Convert.ToString(reader[2]);
                                string Points = Convert.ToString(reader[3]);
                                Rank rank = new Rank
                                {
                                    LineNum = linenum,
                                    Guild = GuildLevel,
                                    Points = Points,
                                    ShardID = ShardID
                                };

                                if (!UTILS.CustomPVPRank.ContainsKey(GuildName))
                                    UTILS.CustomPVPRank.TryAdd(GuildName, rank);
                                else
                                    UTILS.CustomPVPRank[GuildName] = rank;
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadGuildRank returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<long> prod_int64(string query)
        {
            long value = 0;
            try
            {
                using (var con = new SqlConnection(connectionstring))
                {
                    await con.OpenAsync().ConfigureAwait(false);
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        using (SqlDataReader read = await command.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            await read.ReadAsync().ConfigureAwait(false);
                            if (!read.IsDBNull(0))
                            {
                                value = read.GetInt64(0);
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (NullReferenceException)
            {
                return 0;
            }
            catch (SqlException Ex)
            {
                return 0;
            }
            return value;
        }
        public static async Task<bool> LoadHonorRank(int ShardID)
        {
            try
            {
                UTILS.CustomHonorRank.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 20 ROW_NUMBER() OVER (ORDER BY CurLevel DESC , ExpOffset DESC), CharName16,CurLevel,ExpOffset FROM {Settings.ShardInfos[ShardID].DBName}.dbo._Char WHERE RefObjID > 2000  ORDER BY CurLevel DESC , ExpOffset DESC", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                byte linenum = Convert.ToByte(reader[0]);
                                string Charname = Convert.ToString(reader[1]);
                                string Guild = Convert.ToString(reader[2]);
                                long Explevel = Task.Run(async () => await prod_int64($"SELECT Exp_C FROM {Settings.ShardInfos[ShardID].DBName}.dbo._RefLevel where Lvl = {Convert.ToByte(reader[2])}")).Result;
                                string Points = ((Math.Round(100f * Convert.ToInt64(reader[3]) / Explevel))).ToString() + "%";
                                Rank rank = new Rank
                                {
                                    LineNum = linenum,
                                    Guild = Guild,
                                    Points = Points,
                                    ShardID = ShardID
                                };

                                if (!UTILS.CustomHonorRank.ContainsKey(Charname))
                                    UTILS.CustomHonorRank.TryAdd(Charname, rank);
                                else
                                    UTILS.CustomHonorRank[Charname] = rank;
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateHonorRank returned false and failed: {EX}", UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> LoadPVPRank(int ShardID)
        {
            try
            {
                UTILS.CustomUniqueRank.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 20 ROW_NUMBER() OVER (ORDER BY CurLevel DESC , ExpOffset DESC), CharName16,CurLevel,ExpOffset FROM {Settings.ShardInfos[ShardID].DBName}.dbo._Char WHERE RefObjID <2000  ORDER BY CurLevel DESC , ExpOffset DESC", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                byte linenum = Convert.ToByte(reader[0]);
                                string Charname = Convert.ToString(reader[1]);
                                string Guild = Convert.ToString(reader[2]);
                                long Explevel = Task.Run(async () => await prod_int64($"SELECT Exp_C FROM {Settings.ShardInfos[ShardID].DBName}.dbo._RefLevel where Lvl = {Convert.ToByte(reader[2])}")).Result;
                                string Points = ((Math.Round(100f * Convert.ToInt64(reader[3]) / Explevel))).ToString() + "%";
                                Rank rank = new Rank
                                {
                                    LineNum = linenum,
                                    Guild = Guild,
                                    Points = Points,
                                    ShardID = ShardID
                                };
                                if (!UTILS.CustomUniqueRank.ContainsKey(Charname))
                                    UTILS.CustomUniqueRank.TryAdd(Charname, rank);
                                else
                                    UTILS.CustomUniqueRank[Charname] = rank;
                               
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdatePVPRank returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> LoadCharRank(int ShardID)
        {
            try
            {
                UTILS.CustomCharRank.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 50 ROW_NUMBER() OVER (ORDER BY ItemPoints DESC), CharName16 , GuildName, ItemPoints , ShardID FROM xQc_FILTER.dbo._CharRanking WITH(NOLOCK) WHERE ItemPoints >= 0  AND ShardID = {ShardID} ORDER BY ItemPoints DESC", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                byte linenum = Convert.ToByte(reader[0]);
                                string Charname = Convert.ToString(reader[1]);
                                string Guild = Convert.ToString(reader[2]);
                                string Points = Convert.ToString(reader[3]);
                                int ShardID5 = Convert.ToInt32(reader[4]);
                                Rank rank = new Rank
                                {
                                    LineNum = linenum,
                                    Guild = Guild,
                                    Points = Points,
                                    ShardID = ShardID5
                                };

                                if (!UTILS.CustomCharRank.ContainsKey(Charname))
                                    UTILS.CustomCharRank.TryAdd(Charname, rank);
                                else
                                    UTILS.CustomCharRank[Charname] = rank;

                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateUniqeuRank returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> LoadJobKillsRank(int ShardID)
        {
            try
            {
                UTILS.CustomJobKillsRank.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 50 ROW_NUMBER() OVER (ORDER BY JobPVPPoints DESC), CharName16 , GuildName, JobPVPPoints , ShardID FROM xQc_FILTER.dbo._CharRanking WITH(NOLOCK) WHERE JobPVPPoints >= 0  AND ShardID = {ShardID} ORDER BY JobPVPPoints DESC", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                byte linenum = Convert.ToByte(reader[0]);
                                string Charname = Convert.ToString(reader[1]);
                                string Guild = Convert.ToString(reader[2]);
                                string Points = Convert.ToString(reader[3]);
                                int ShardID6 = Convert.ToInt32(reader[4]);
                                Rank rank = new Rank
                                {
                                    LineNum = linenum,
                                    Guild = Guild,
                                    Points = Points,
                                    ShardID = ShardID6
                                };

                                if (!UTILS.CustomJobKillsRank.ContainsKey(Charname))
                                    UTILS.CustomJobKillsRank.TryAdd(Charname, rank);
                                else
                                    UTILS.CustomJobKillsRank[Charname] = rank;
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateJobKillsRank returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> LoadTraderRank(int ShardID)
        {
            try
            {
                UTILS.CustomTraderRank.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 50 ROW_NUMBER() OVER(ORDER BY B.Exp DESC), A.CharName16, B.Level, B.Exp FROM {Settings.ShardInfos[ShardID].DBName}.dbo._CharTrijob B JOIN {Settings.ShardInfos[ShardID].DBName}.dbo._Char A ON A.CharID = B.CharID WHERE B.JobType = 1 ORDER BY B.Exp DESC", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                byte linenum = Convert.ToByte(reader[0]);
                                string Charname = Convert.ToString(reader[1]);
                                string Guild = Convert.ToString(reader[2]);
                                string Points = Convert.ToString(reader[3]);
                                Rank rank = new Rank
                                {
                                    LineNum = linenum,
                                    Guild = Guild,
                                    Points = Points,
                                    ShardID = ShardID
                                };

                                if (!UTILS.CustomTraderRank.ContainsKey(Charname))
                                    UTILS.CustomTraderRank.TryAdd(Charname, rank);
                                else
                                    UTILS.CustomTraderRank[Charname] = rank;
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateTraderRank returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> LoadHunterRank(int ShardID)
        {
            try
            {
                UTILS.CustomHunterRank.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 50 ROW_NUMBER() OVER(ORDER BY B.Exp DESC), A.CharName16, B.Level, B.Exp FROM {Settings.ShardInfos[ShardID].DBName}.dbo._CharTrijob B JOIN {Settings.ShardInfos[ShardID].DBName}.dbo._Char A ON A.CharID = B.CharID WHERE B.JobType = 3 ORDER BY B.Exp DESC", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                byte linenum = Convert.ToByte(reader[0]);
                                string Charname = Convert.ToString(reader[1]);
                                string Guild = Convert.ToString(reader[2]);
                                string Points = Convert.ToString(reader[3]);
                                Rank rank = new Rank
                                {
                                    LineNum = linenum,
                                    Guild = Guild,
                                    Points = Points,
                                    ShardID = ShardID
                                };

                                if (!UTILS.CustomHunterRank.ContainsKey(Charname))
                                    UTILS.CustomHunterRank.TryAdd(Charname, rank);
                                else
                                    UTILS.CustomHunterRank[Charname] = rank;
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateHunterRank returned false and failed: {EX}", UTILS.LOG_TYPE.Fatal); ; return false; }
        }
        public static async Task<bool> LoadThiefRank(int ShardID)
        {
            try
            {
                UTILS.CustomThiefRank.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 50 ROW_NUMBER() OVER(ORDER BY B.Exp DESC), A.CharName16, B.Level, B.Exp FROM {Settings.ShardInfos[ShardID].DBName}.dbo._CharTrijob B JOIN {Settings.ShardInfos[ShardID].DBName}.dbo._Char A ON A.CharID = B.CharID WHERE B.JobType = 2 ORDER BY B.Exp DESC", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                byte linenum = Convert.ToByte(reader[0]);
                                string Charname = Convert.ToString(reader[1]);
                                string Guild = Convert.ToString(reader[2]);
                                string Points = Convert.ToString(reader[3]);
                                Rank rank = new Rank
                                {
                                    LineNum = linenum,
                                    Guild = Guild,
                                    Points = Points,
                                    ShardID = ShardID
                                };

                                if (!UTILS.CustomThiefRank.ContainsKey(Charname))
                                    UTILS.CustomThiefRank.TryAdd(Charname, rank);
                                else
                                    UTILS.CustomThiefRank[Charname] = rank;
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateThiefRank returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> LoadEventTime()
        {
            try
            {
                UTILS.EventTimeList.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select ROW_NUMBER() OVER(ORDER BY Datetime,cast(Datetime as time(7)) asc) as ID,* from xQc_FILTER.dbo._EventsSchedule ORDER BY Datetime,cast(Datetime as time(7))", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int ID = Convert.ToInt32(reader[0]);
                                string EventName = Convert.ToString(reader[1]);
                                string Day = Convert.ToString(reader[2]);
                                string Time = Convert.ToString(reader[3]);
                                bool State = Convert.ToBoolean(reader[4]);
                                var myDate1 = Convert.ToDateTime(Time);
                                DateTime myDate2 = DateTime.Now;
                                TimeSpan myDateResult;
                                myDateResult = myDate1 - myDate2;
                                EventTime eventt = new EventTime
                                {
                                    Day = Day,
                                    Time = Time,
                                    State = State,
                                    ID = ID
                                };

                                if (!UTILS.EventTimeList.ContainsKey(EventName) && myDateResult.Seconds > 0 && UTILS.EventTimeList.Count < 10)
                                    UTILS.EventTimeList.TryAdd(EventName, eventt);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }

                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateEventTime returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> LoadChangeLog()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select * from xQc_FILTER.dbo._ChangeLog", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string info = Convert.ToString(reader[0]);
                                UTILS.ChangeLog.Add(info);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadChangeLog returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> UpdateFWKillsCounter()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 5 * FROM xQc_FILTER.._CounterFW WITH(NOLOCK) ORDER BY TotalKills DESC", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string Guildname = Convert.ToString(reader[0]);
                                int kills = Convert.ToInt32(reader[1]);

                                if (!UTILS.FWKills.ContainsKey(Guildname))
                                    UTILS.FWKills.TryAdd(Guildname, kills);
                                else
                                    UTILS.FWKills[Guildname] = kills;
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }

                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateSURVKillsCounter returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<string> TRUNCATE_FWKILLS()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"truncate table xQc_FILTER.dbo._CounterFW", con))
                    {
                        await con.OpenAsync();
                        string x = Convert.ToString(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"TRUNCATE_FWKILLS opeation failed and returned string.Empty: {EX}", UTILS.LOG_TYPE.Fatal); return string.Empty; }
        }
        public static async Task INSERT_INSTANT_TELEPORT(int charid, int worldlayerid, int worldid, int region, float x, float y, float z,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"INSERT INTO {Settings.ShardInfos[ShardID].DBName}.dbo._InstantTeleportDelivery VALUES ({charid},{worldlayerid},{worldid},{region},{x},{y},{z})", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"INSERT_INSTANT_TELEPORT returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); }
        }
        public static async Task INSERT_UNIQUE_BYPOS(int serverid, int worldlayerid, int worldid, int region, float x, float y, float z, int mobtype, float range, int refobjid,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"INSERT INTO {Settings.ShardInfos[ShardID].DBName}.dbo._InstantMobSpawnAtPosDelivery VALUES ({serverid},{worldlayerid},{worldid},{region},{x},{y},{z},{mobtype},{range},{refobjid})", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"INSERT_UNIQUE_BYPOS returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); }
        }
        public static async Task INSTANT_PVPCAPE(int CharID, byte State,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"INSERT INTO {Settings.ShardInfos[ShardID].DBName}.dbo._InstantPvpStateDelivery VALUES ({CharID},{State})", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"INSTANT_PVPCAPE returned false and failed: {EX}"); }
        }
        public static async Task<bool> UpdateSURVKillsCounter()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 5 * FROM xQc_FILTER.dbo._CounterSurvival WITH(NOLOCK) ORDER BY TotalKills DESC", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string Charname = Convert.ToString(reader[0]);
                                int kills = Convert.ToInt32(reader[1]);

                                if (!UTILS.SURVKills.ContainsKey(Charname))
                                    UTILS.SURVKills.TryAdd(Charname, kills);
                                else
                                    UTILS.SURVKills[Charname] = kills;
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }

                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateSURVKillsCounter returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<string> TRUNCATE_SURVKILLS()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"truncate table xQc_FILTER.dbo._CounterSurvival", con))
                    {
                        await con.OpenAsync();
                        string x = Convert.ToString(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"TRUNCATE_SURVKILLS opeation failed and returned string.Empty: {EX}", UTILS.LOG_TYPE.Fatal); return string.Empty; }
        }
        public static async Task INSERT_ITEM_CHEST(int CharID, int ItemID, int count, int OptLevel, string From,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"INSERT INTO xQc_FILTER.dbo._CharChest VALUES ({CharID},{ItemID},{count},{OptLevel},0,'{From}',{ShardID},GETDATE())", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"INSERT_ITEM_CHEST returned false and failed: {EX}",UTILS.LOG_TYPE.Fatal); }
        }
        public static async Task<bool> FilterCommands()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"select top 1* from xQc_FILTER.dbo._FilterCommands where IsDone = '0' order by ID asc", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int ID = Convert.ToInt32(reader[0]);
                                byte CommandID = Convert.ToByte(reader[1]);
                                string CommandString = Convert.ToString(reader[2]);
                                string Data1 = Convert.ToString(reader[3]);
                                string Data2 = Convert.ToString(reader[4]);
                                string Data3 = Convert.ToString(reader[5]);

                                try
                                {
                                    switch (CommandID)
                                    {
                                        case 1:
                                            //notice notify
                                            if (Data1.ToLower().Contains("all"))
                                                UTILS.SendNoticeForAll(NoticeType.Notice, Data2);
                                            else
                                                UTILS.SendNotice(NoticeType.Notice,Data2, ASYNC_SERVER.AG_CONS.FirstOrDefault(x => x.Value.CHARNAME16_HOLDER.ToLower() == Data1.ToLower()).Key);
                                            break;
                                        case 2:
                                            //blue notify

                                            if (Data1.ToLower().Contains("all"))
                                                UTILS.SendNoticeForAll(NoticeType.Notify, Data2);
                                            else
                                                UTILS.SendNotice(NoticeType.Notify, Data2, ASYNC_SERVER.AG_CONS.FirstOrDefault(x => x.Value.CHARNAME16_HOLDER.ToLower() == Data1.ToLower()).Key);
                                            break;
                                        case 3:
                                            //quest notify
                                            if (Data1.ToLower().Contains("all"))
                                                UTILS.SendNoticeForAll(NoticeType.Green, Data2);
                                            else
                                                UTILS.SendNotice(NoticeType.Green, Data2, ASYNC_SERVER.AG_CONS.FirstOrDefault(x => x.Value.CHARNAME16_HOLDER.ToLower() == Data1.ToLower()).Key);
                                            break;
                                        case 4:
                                            //yellow notify
                                            if (Data1.ToLower().Contains("all"))
                                                UTILS.SendNoticeForAll(NoticeType.Yellow, Data2);
                                            else
                                                UTILS.SendNotice(NoticeType.Yellow, Data2, ASYNC_SERVER.AG_CONS.FirstOrDefault(x => x.Value.CHARNAME16_HOLDER.ToLower() == Data1.ToLower()).Key);
                                            break;
                                        case 5:
                                            //Guide notify
                                            if (Data1.ToLower().Contains("all"))
                                                UTILS.SendNoticeForAll(NoticeType.Guide, Data2);
                                            else
                                                UTILS.SendNotice(NoticeType.Guide, Data2, ASYNC_SERVER.AG_CONS.FirstOrDefault(x => x.Value.CHARNAME16_HOLDER.ToLower() == Data1.ToLower()).Key);
                                            break;
                                        case 6:
                                            //Colored System
                                            if (Data1.ToLower().Contains("all"))
                                                UTILS.SendNoticeForAll(NoticeType.ColoredSystem, Data2, uint.Parse(Data3.Replace("#", ""), NumberStyles.HexNumber));
                                            else
                                                UTILS.SendNotice(NoticeType.ColoredSystem, Data2, ASYNC_SERVER.AG_CONS.FirstOrDefault(x => x.Value.CHARNAME16_HOLDER.ToLower() == Data1.ToLower()).Key, uint.Parse(Data3.Replace("#", ""), NumberStyles.HexNumber));
                                            break;
                                        case 7:
                                            //Colored Chat
                                            if (Data1.ToLower().Contains("all"))
                                                UTILS.SendNoticeForAll(NoticeType.ColoredChat, Data2, uint.Parse(Data3.Replace("#", ""), NumberStyles.HexNumber));
                                            else
                                                UTILS.SendNotice(NoticeType.ColoredChat, Data2, ASYNC_SERVER.AG_CONS.FirstOrDefault(x => x.Value.CHARNAME16_HOLDER.ToLower() == Data1.ToLower()).Key, uint.Parse(Data3.Replace("#", ""), NumberStyles.HexNumber));
                                            break;
                                        case 8:
                                            //add custom title
                                            _CustomTitle Found = new _CustomTitle();
                                            Found = CustomTitleList.Find(x => x.CharName == Data1 && x.ShardID == Convert.ToInt32(Data3));

                                            if (Found == null)
                                            {
                                                _CustomTitle CustomTitle = new _CustomTitle();
                                                CustomTitle.CharName = Data1;
                                                CustomTitle.Titles.TryAdd(Data2, 1);
                                                CustomTitle.ShardID = Convert.ToInt32(Data3);
                                                CustomTitleList.Add(CustomTitle);
                                            }
                                            else
                                            {
                                                foreach(var x in Found.Titles)
                                                    Found.Titles[x.Key] = 0;
                                                Found.Titles.TryAdd(Data2, 1);
                                            }
                                            Packet CTInfo = new Packet(0x5106);
                                            CTInfo.WriteAscii(Data1);
                                            CTInfo.WriteAscii(Data2);
                                            UTILS.BroadCastToClients(CTInfo, Convert.ToInt32(Data3));
                                            break;
                                        case 9:
                                            //REMOVE custom title
                                            foreach (var CSTitle in CustomTitleList)
                                            {
                                                if ( CSTitle.CharName == Data1 && CSTitle.ShardID == Convert.ToInt32(Data3))
                                                {
                                                    var item = CustomTitleList.Single(y => y.CharName == Data1 && y.ShardID == Convert.ToInt32(Data3));
                                                    CustomTitleList.Remove(item);
                                                    var charn = ASYNC_SERVER.AG_CONS.LastOrDefault(x => x.Value.CHARNAME16_HOLDER.ToLower() == Data1.ToLower()).Value;
                                                    Packet Info = new Packet(0x5107);
                                                    Info.WriteAscii(Data1);
                                                    //1337 title name
                                                    Info.WriteAscii(charn.CharTitles[Data1]);
                                                    UTILS.BroadCastToClients(Info, Convert.ToInt32(Data3));
                                                    break;
                                                }
                                            }
                                            break;
                                        case 11:
                                            //remove title color
                                            foreach(var x in titlescolors)
                                            {
                                                if(x.CharName == Data1 && x.ShardID == Convert.ToInt32(Data3))
                                                {
                                                    var item = titlescolors.Single(y => y.CharName == Data1 && y.ShardID == Convert.ToInt32(Data3));
                                                    titlescolors.Remove(item);
                                                    Packet RtC = new Packet(0x5110);
                                                    RtC.WriteAscii(Data1);
                                                    UTILS.BroadCastToClients(RtC, Convert.ToInt32(Data3));
                                                    break;
                                                }
                                            }
                                            break;
                                        case 12:
                                            //add char color
                                            _CustomNameColor Custom = new _CustomNameColor();
                                            Custom.CharName = Data1;
                                            Custom.Color = uint.Parse(Data2.Replace("#", ""), NumberStyles.HexNumber);
                                            Custom.ShardID = Convert.ToInt32(Data3);
                                            UTILS.CharnameColorList.Add(Custom);
                                            Packet CCInfo = new Packet(0x5112);
                                            CCInfo.WriteAscii(Data1);
                                            CCInfo.WriteUInt32(uint.Parse(Data2.Replace("#", ""), NumberStyles.HexNumber));
                                            UTILS.BroadCastToClients(CCInfo, Convert.ToInt32(Data3));
                                            break;
                                        case 13:
                                            //REMOVE char color
                                            foreach (var CCL in CharnameColorList)
                                            {
                                                if(CCL.CharName == Data1 && CCL.ShardID == Convert.ToInt32(Data3))
                                                {
                                                    var item = CharnameColorList.Single(y => y.CharName == Data1 && y.ShardID == Convert.ToInt32(Data3));
                                                    CharnameColorList.Remove(item);
                                                    Packet Info = new Packet(0x5113);
                                                    Info.WriteAscii(Data1);
                                                    UTILS.BroadCastToClients(Info, Convert.ToInt32(Data3));
                                                    break;
                                                }
                                            }
                                            break;
                                        case 14:
                                            //add custom charname rank
                                            _CustomNameRank CustomRank = new _CustomNameRank();
                                            CustomRank.CharName = Data1;
                                            CustomRank.Rank = Data2;
                                            CustomRank.ShardID = Convert.ToInt32(Data3);
                                            UTILS.CustomCharnameRankList.Add(CustomRank);
                                            Packet CNInfo = new Packet(0x5116);
                                            CNInfo.WriteAscii(Data1);
                                            CNInfo.WriteAscii(Data2);
                                            UTILS.BroadCastToClients(CNInfo, Convert.ToInt32(Data3));
                                            break;
                                        case 15:
                                            //REMOVE custom charname rank
                                            foreach (var CCL in CustomCharnameRankList)
                                            {
                                                if (CCL.CharName == Data1 && CCL.ShardID == Convert.ToInt32(Data3))
                                                {
                                                    var item = CustomCharnameRankList.Single(y => y.CharName == Data1 && y.ShardID == Convert.ToInt32(Data3));
                                                    CustomCharnameRankList.Remove(item);
                                                    Packet Info = new Packet(0x5117);
                                                    Info.WriteAscii(Data1);
                                                    UTILS.BroadCastToClients(Info, Convert.ToInt32(Data3));
                                                    break;
                                                }
                                            }
                                            break;
                                        case 16:
                                            //Purble notify
                                            if (Data1.ToLower().Contains("all"))
                                                UTILS.SendNoticeForAll(NoticeType.Purble, Data2);
                                            else
                                                UTILS.SendNotice(NoticeType.Purble, Data2, ASYNC_SERVER.AG_CONS.FirstOrDefault(x => x.Value.CHARNAME16_HOLDER.ToLower() == Data1.ToLower()).Key);
                                            break;
                                        case 17:
                                            //Orange notify
                                            if (Data1.ToLower().Contains("all"))
                                                UTILS.SendNoticeForAll(NoticeType.Orange, Data2);
                                            else
                                                UTILS.SendNotice(NoticeType.Orange, Data2, ASYNC_SERVER.AG_CONS.FirstOrDefault(x => x.Value.CHARNAME16_HOLDER.ToLower() == Data1.ToLower()).Key);
                                            break;
                                        case 18:
                                            //add title color
                                            var TColor = uint.Parse(Data2.Replace("#", ""), NumberStyles.HexNumber);
                                            _CustomTitleColor ctc = UTILS.titlescolors.Find(c => c.CharName == Data1 && c.ShardID == Convert.ToInt32(Data3));
                                            if (ctc != null)
                                                ctc.Color = TColor;
                                            else
                                            {
                                                ctc = new _CustomTitleColor();
                                                ctc.CharName = Data1;
                                                ctc.Color = TColor;
                                                ctc.ShardID = Convert.ToInt32(Data3);

                                            }
                                            UTILS.titlescolors.Add(ctc);
                                            Packet TInfo = new Packet(0x5108);
                                            TInfo.WriteUInt32(1);
                                            TInfo.WriteAscii(Data1);
                                            TInfo.WriteUInt32(TColor);
                                            UTILS.BroadCastToClients(TInfo, Convert.ToInt32(Data3));
                                            break;
                                        //case 20:
                                        //    UTILS.SURVIVAL_EVENT_ACTIVE = true;
                                        //    Task.Factory.StartNew(async () =>
                                        //    {
                                        //        while (UTILS.SURVIVAL_EVENT_ACTIVE)
                                        //        {
                                        //            await UpdateSURVKillsCounter();
                                        //            await UTILS.SurvialKillsUpdate();
                                        //            await Task.Delay(10000);
                                        //        }
                                        //    });
                                        //    break;
                                        //case 21:
                                        //    UTILS.SURVIVAL_EVENT_ACTIVE = false;
                                        //    UTILS.SURVKills.Clear();
                                        //    break;
                                        case 22:
                                            UTILS.FW_EVENT_ACTIVE = true;
                                            Task.Factory.StartNew(async () =>
                                            {
                                                while (UTILS.FW_EVENT_ACTIVE)
                                                {
                                                    await UpdateFWKillsCounter();
                                                    await UTILS.FWKillsUpdate();
                                                    await Task.Delay(10000);
                                                }
                                            });
                                            break;
                                        case 23:
                                            UTILS.PVP_EVENT_ACTIVE = false;
                                            if (UTILS.PVPactivelist.Contains(Data1))
                                            {
                                                UTILS.SendNoticeForAll(NoticeType.Orange, $"[PVP MATCHING] {Data1} has won againt {Data2}");
                                                UTILS.PVPactivelist.Clear();
                                            }
                                            break;
                                        case 24:
                                            UTILS.UNIQUE_EVENT_ACTIVE = false;
                                            string charname1 = UTILS.UNIQUEactivelist[0];
                                            string charname2 = UTILS.UNIQUEactivelist[1];
                                            if (charname1 == Data1 || charname2 == Data1)
                                            {
                                                if (charname1 == Data1)
                                                    UTILS.SendNoticeForAll(NoticeType.Orange, $"[UNIQUE MATCHING] {Data1} has won againt {charname2}");
                                                else
                                                    UTILS.SendNoticeForAll(NoticeType.Orange, $"[UNIQUE MATCHING] {Data1} has won againt {charname1}");
                                                int charid1 = await Get_CharID_by_CharName16(charname1,64);
                                                int charid2 = await Get_CharID_by_CharName16(charname2,64);
                                                await INSERT_INSTANT_TELEPORT(charid1, 1, 1, 25000, 982, 140, 140,64);
                                                await INSERT_INSTANT_TELEPORT(charid2, 1, 1, 25000, 982, 140, 140,64);
                                                UTILS.UNIQUEactivelist.Clear();
                                                UTILS.IS_UNIQUE_KILLED = true;
                                            }
                                            break;
                                        case 25:
                                            //add new character icon
                                            foreach(var Ico in CustomIcons)
                                                if (Ico.CharName == Data1 && Ico.ShardID == Convert.ToInt32(Data3))
                                                    return true;
                                            _CharIcon Icon = new _CharIcon();
                                            Icon.CharName = Data1;
                                            Icon.IconID = Convert.ToInt32(Data2);
                                            Icon.ShardID = Convert.ToInt32(Data3);

                                            UTILS.CustomIcons.Add(Icon);
                                            Packet icons = new Packet(0x180B);
                                            icons.WriteUInt8(1);
                                            icons.WriteUInt32(Convert.ToInt32(Data2));
                                            icons.WriteAscii(Data1);
                                            UTILS.BroadCastToClients(icons, Convert.ToInt32(Data3));
                                            break;
                                        case 26:
                                            foreach(var x in CustomIcons)
                                            {
                                                if(x.CharName == Data1 && x.ShardID == Convert.ToInt32(Data3))
                                                {
                                                    var item = CustomIcons.Single(y => y.CharName == Data1 && y.ShardID == Convert.ToInt32(Data3));
                                                    CustomIcons.Remove(item);
                                                    Packet icons2 = new Packet(0x190B);
                                                    icons2.WriteAscii(Data1);
                                                    UTILS.BroadCastToClients(icons2, Convert.ToInt32(Data3));
                                                    break;
                                                }
                                            }
                                            break;
                                    }
                                }
                                catch
                                {
                                }

                                DeleteFromFilterCommands(ID);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"FilterCommands returned false and failed: {EX}", UTILS.LOG_TYPE.Fatal); return false; }
        }
        private static async void DeleteFromFilterCommands(int ID)
        {
            await EXEC_QUERY($"DELETE FROM xQc_FILTER.dbo._FilterCommands WHERE ID={ID}");
        }
        public static async Task<string> EXEC_QUERY(string query)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"{query}", con))
                    {
                        await con.OpenAsync();
                        string x = Convert.ToString(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return x;
                    }
                }
            }
            catch { return "N/A"; }
        }
        public static async Task<bool> LoadCustomNamesRank()
        {
            try
            {
                UTILS.CustomCharnameRankList.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select * from xQc_FILTER.dbo._CustomNameRank", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string charname = Convert.ToString(reader[0]);
                                string Rank = Convert.ToString(reader[1]);
                                int ShardID = Convert.ToInt32(reader[2]);

                                _CustomNameRank CustomRank = new _CustomNameRank();
                                CustomRank.CharName = charname;
                                CustomRank.Rank = Rank;
                                CustomRank.ShardID = ShardID;
                                UTILS.CustomCharnameRankList.Add(CustomRank);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateCustomNameRank returned false and failed: {EX}",LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> LoadCharnameColor()
        {
            try
            {
                UTILS.CharnameColorList.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select * from xQc_FILTER.dbo._CustomNameColor", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string charname = Convert.ToString(reader[0]);
                                string colorcode = Convert.ToString(reader[1]);
                                int ShardID = Convert.ToInt32(reader[2]);
                                uint color = UInt32.Parse(colorcode.Replace("#", ""), NumberStyles.HexNumber);
                                _CustomNameColor Custom = new _CustomNameColor();
                                Custom.CharName = charname;
                                Custom.Color = color;
                                Custom.ShardID = ShardID;
                                UTILS.CharnameColorList.Add(Custom);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }

                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateCharnameColor returned false and failed: {EX}",LOG_TYPE.Fatal) ; return false; }
        }
        public static async Task<bool> LoadCustomNames()
        {
            try
            {
                UTILS.CustomCharnameList.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select * from xQc_FILTER.dbo._CustomName", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string charname = Convert.ToString(reader[0]);
                                string Title = Convert.ToString(reader[1]);
                                int ShardID = Convert.ToInt32(reader[2]);
                                _CustomName Custom = new _CustomName();
                                Custom.CharName = charname;
                                Custom.Title = Title;
                                Custom.ShardID = ShardID;
                                UTILS.CustomCharnameList.Add(Custom);

                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateCustomTitle returned false and failed: {EX}",LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> LoadIcons()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select * from xQc_FILTER.dbo._CustomIcon", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string CharName = Convert.ToString(reader[0]);
                                int ID = Convert.ToInt32(reader[1]);
                                int ShardID = Convert.ToInt32(reader[2]);
                                _CharIcon Icon = new _CharIcon();
                                Icon.CharName = CharName;
                                Icon.IconID = ID;
                                Icon.ShardID = ShardID;
                                UTILS.CustomIcons.Add(Icon);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadIcons returned false and failed: {EX}",LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> LoadIconsData()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select * from xQc_FILTER.dbo._CustomIconData", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int ID = Convert.ToInt32(reader[0]);
                                string Path = Convert.ToString(reader[1]);
                                if (!UTILS.CustomIconsData.ContainsKey(ID))
                                    UTILS.CustomIconsData.TryAdd(ID, Path);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadIconsData returned false and failed: {EX}",LOG_TYPE.Fatal); return false; }
        }
        public static async Task<string> ReturnCharLockPassword(string StrUserID)
        {
            try
            {
                string result = null;
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select PinCode From xQc_FILTER.dbo._CharLockPin where StrUserID like '%{StrUserID}%';", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                if (!string.IsNullOrEmpty(reader[0].ToString()))
                                    result = reader[0].ToString();
                            }
                            reader.Close();
                        }
                        con.Close();
                        return result;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"ReturnCharLockPassword returned null and failed: {EX}"); return null; }
        }
        public static async Task<int> IS_SET_LOCK(string struserid)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select isSet from xQc_FILTER.dbo._CharLockPin where StrUserID = '{struserid}' ", con))
                    {
                        await con.OpenAsync();
                        int x = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return x;

                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"IS_SET_LOCK failed and returned 0: {EX}"); return 0; }

        }
        public static async Task SET_LOCK(string struserid, int isset)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"UPDATE xQc_FILTER.dbo._CharLockPin set isSet = {isset} where StrUserID = '{struserid}'", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"SET_LOCK returned false and failed: {EX}"); }
        }
        public static async Task<bool> RemoveCharLock(string StrUserID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Delete From xQc_FILTER.dbo._CharLockPin where StrUserID like '%{StrUserID}%';", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        return true;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"RemoveCharLock returned false and failed: {EX}"); return false; }

        }
        public static async Task<bool> CreateCharLock(string StrUserID, string password)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"insert into xQc_FILTER.dbo._CharLockPin (StrUserID,PinCode,isSet) values ('{StrUserID}','{password}',1);", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        return true;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"CreateCharLock returned false and failed: {EX}"); return false; }
        }
        public static async Task<bool> GambeLogs(string CharName16, string Type)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"insert into xQc_FILTER.dbo._LogGamble values ('{CharName16}','{Type}',GETDATE());", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        return true;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"GambeLogs returned false and failed: {EX}"); return false; }
        }
        public static async Task INSERT_INSTANT_ITEM(string itemcode, int a, int b, int CharID,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"INSERT INTO {Settings.ShardInfos[ShardID].DBName}.dbo._InstantItemDelivery VALUES ({CharID},0,'{UTILS.INJECTION_PREFIX(itemcode)}',{a},{b})", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"INSERT INSTANT ITEM returned false and failed: {EX}"); }
        }
        public static async Task<bool> RemoveItemChest(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Delete From xQc_FILTER.dbo._CharChest where ID = {id};", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        return true;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"RemoveItemChest returned false and failed: {EX}"); return false; }
        }
        public static async Task<string> Get_ItemCode128_byid(int id,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT CodeName128 from {Settings.ShardInfos[ShardID].DBName}.dbo._RefObjCommon where ID = {id};", con))
                    {
                        await con.OpenAsync();
                        string Basic_Code = (string)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return Basic_Code;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"Get_ItemCode128_byid returned false and failed: {EX}"); return string.Empty; }
        }
        public static async Task<_ItemInfo> Get_ItemInfo_by_Slot(int Slot, int CharID , int ShardID,int ItemID)
        {
            try
            {
                List<long> MagParams = new List<long>();
                _ItemInfo Item = new _ItemInfo();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select I.OptLevel,I.Variance,I.Data,I.MagParamNum,I.MagParam1,I.MagParam2,I.MagParam3,I.MagParam4,I.MagParam5,I.MagParam6,I.MagParam7,I.MagParam8,I.MagParam9,I.MagParam10,I.MagParam11,I.MagParam12,R.TypeID2,R.TypeID3,R.TypeID4,I.ID64 from {Settings.ShardInfos[ShardID].DBName}.dbo._Items I JOIN {Settings.ShardInfos[ShardID].DBName}.dbo._Inventory C ON I.ID64 = C.ItemID JOIN {Settings.ShardInfos[ShardID].DBName}.dbo._RefObjCommon R ON R.ID = {ItemID} WHERE c.Slot = {Slot} and C.CharID = {CharID}", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                byte Plus = (reader[0] != DBNull.Value) ? Convert.ToByte(reader[0]) : Convert.ToByte(0);
                                ulong Variance = (reader[1] != DBNull.Value) ? Convert.ToUInt64(reader[1]) : Convert.ToUInt64(0);
                                uint Durability = (reader[2] != DBNull.Value) ? Convert.ToUInt32(reader[2]) : Convert.ToUInt32(0);
                                byte MagParamNum = (reader[3] != DBNull.Value) ? Convert.ToByte(reader[3]) : Convert.ToByte(0);
                                MagParams.Add((reader[4] != DBNull.Value) ? Convert.ToInt64(reader[4]) : 0);
                                MagParams.Add((reader[5] != DBNull.Value) ? Convert.ToInt64(reader[5]) : 0);
                                MagParams.Add((reader[6] != DBNull.Value) ? Convert.ToInt64(reader[6]) : 0);
                                MagParams.Add((reader[7] != DBNull.Value) ? Convert.ToInt64(reader[7]) : 0);
                                MagParams.Add((reader[8] != DBNull.Value) ? Convert.ToInt64(reader[8]) : 0);
                                MagParams.Add((reader[9] != DBNull.Value) ? Convert.ToInt64(reader[9]) : 0);
                                MagParams.Add((reader[10] != DBNull.Value) ? Convert.ToInt64(reader[10]) : 0);
                                MagParams.Add((reader[11] != DBNull.Value) ? Convert.ToInt64(reader[11]) : 0);
                                MagParams.Add((reader[12] != DBNull.Value) ? Convert.ToInt64(reader[12]) : 0);
                                MagParams.Add((reader[13] != DBNull.Value) ? Convert.ToInt64(reader[13]) : 0);
                                MagParams.Add((reader[14] != DBNull.Value) ? Convert.ToInt64(reader[14]) : 0);
                                MagParams.Add((reader[15] != DBNull.Value) ? Convert.ToInt64(reader[15]) : 0);
                                int TypeID2 = (reader[16] != DBNull.Value) ? Convert.ToInt32(reader[16]) : Convert.ToInt32(0);
                                int TypeID3 = (reader[17] != DBNull.Value) ? Convert.ToInt32(reader[17]) : Convert.ToInt32(0);
                                int TypeID4 = (reader[18] != DBNull.Value) ? Convert.ToInt32(reader[18]) : Convert.ToInt32(0);
                                long ID64 = (reader[19] != DBNull.Value) ? Convert.ToInt64(reader[19]) : Convert.ToInt64(0);
                                Item.Plus = Plus;
                                Item.Variance = Variance;
                                Item.Durability = Durability;
                                Item.MagParamNum = MagParamNum;
                                Item.MagicOptions = MagParams;
                                Item.TypeID2 = TypeID2;
                                Item.TypeID3 = TypeID3;
                                Item.TypeID4 = TypeID4;
                                Item.ID64 = ID64;
                            }
                            reader.Close();
                        }
                    }
                }
                return Item;
            }
            catch (Exception EX) { UTILS.WriteLine($"Get_ItemInfo_by_Slot failed: {EX}"); _ItemInfo Item = new _ItemInfo(); return Item; }
        }
        public static async Task<_ItemInfo> Get_BindingInfo_by_Slot(_ItemInfo Item,int ShardID)
        {
            try
            {
                List<_ItemInfoSocket> Sockets = new List<_ItemInfoSocket>();
                List<_ItemInfoAdvance> Advances = new List<_ItemInfoAdvance>();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select bOptType,nSlot,nOptID,nOptLvl,nOptValue,nParam1 from {Settings.ShardInfos[ShardID].DBName}.dbo._BindingOptionWithItem  WHERE nItemDBID = {Item.ID64}", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                
                                byte Type = (reader[0] != DBNull.Value) ? Convert.ToByte(reader[0]) : Convert.ToByte(0);
                                byte nSlot = (reader[1] != DBNull.Value) ? Convert.ToByte(reader[1]) : Convert.ToByte(0);

                                if (Type == 1)
                                {
                                    ushort nOptValue = (reader[3] != DBNull.Value) ? Convert.ToUInt16(reader[3]) : Convert.ToUInt16(0);
                                    ushort nOptID = (reader[2] != DBNull.Value) ? Convert.ToUInt16(reader[2]) : Convert.ToUInt16(0);
                                    uint nParam = (reader[5] != DBNull.Value) ? Convert.ToUInt32(reader[5]) : Convert.ToUInt32(0);
                                    _ItemInfoSocket sock = new _ItemInfoSocket(nSlot, nOptID);
                                    sock.Value = nOptValue;
                                    sock.nParam = nParam;
                                    Sockets.Add(sock);

                                }
                                else if(Type ==2)
                                {
                                    uint nOptID = (reader[2] != DBNull.Value) ? Convert.ToUInt32(reader[2]) : Convert.ToUInt32(0);
                                    uint nOptValue = (reader[4] != DBNull.Value) ? Convert.ToUInt32(reader[4]) : Convert.ToUInt32(0);
                                    _ItemInfoAdvance adv = new _ItemInfoAdvance(nSlot, nOptID);
                                    adv.Value = nOptValue;
                                    Advances.Add(adv);
                                }
                                Item.SocketOptions = Sockets;
                                Item.AdvanceOptions = Advances;
                            }
                            reader.Close();
                        }
                    }
                }
                return Item;
            }
            catch (Exception EX) { UTILS.WriteLine($"Get_ItemInfo_by_Slot failed: {EX}"); return Item; }
        }
        public static async Task INSERT_XSMB_EVENT(string CharName, long Amount, byte Type, int Num)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"INSERT INTO xQc_FILTER.dbo._XsmbEvent VALUES ('{CharName}',{Amount},{Type},{Num});INSERT INTO xQc_FILTER.dbo._XsmbEventLog VALUES ('{CharName}',{Amount},{Type},{Num},'Chê kÕt qu¶',GETDATE())", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"INSERT_XSMB_EVENT returned false and failed: {EX}"); }
        }

        public static async Task<long> Get_Storage_Gold_Amount(string username, int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select Gold from " + Settings.ShardInfos[ShardID].DBName + ".._AccountJID where AccountID = '" + username + "'", con))
                    {
                        await con.OpenAsync();
                        long Basic_Code = (long)await cmd.ExecuteScalarAsync();
                        con.Close();
                        return Basic_Code;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"Get_Storage_Gold_Amount returned false and failed: {EX}"); return 0; }
        }

        public static async Task Update_Storage_Gold_Amount(string username, int ShardID , long amount)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"UPDATE {Settings.ShardInfos[ShardID].DBName}.dbo._AccountJID SET Gold = Gold + {amount} where AccountID = '{username}' ", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"Update_Storage_Gold_Amount returned false and failed: {EX}"); }
        }
        public static async Task<bool> LoadXsmbLogs(AGENT_MODULE Session)
        {
            try
            {
                Session.XsmbLog.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT TOP 10 ROW_NUMBER() OVER (ORDER BY DateRegistered DESC) as ID , * from xQc_FILTER.dbo._XsmbEventLog where CharName = '{Session.CHARNAME16}';", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                byte LineID = Convert.ToByte(reader[0]);
                                string CharName = Convert.ToString(reader[1]);
                                long Amount = Convert.ToInt64(reader[2]);
                                byte Type = Convert.ToByte(reader[3]);
                                int Num = Convert.ToInt32(reader[4]);
                                string Status = Convert.ToString(reader[5]);
                                string time = Convert.ToString(reader[6]);
                                _XsmbLog xsmb = new _XsmbLog();
                                xsmb.LineID = LineID;
                                xsmb.date = time;
                                xsmb.Amount = Amount;
                                xsmb.Num = Num;
                                xsmb.Type = Type;
                                xsmb.status = Status;
                                xsmb.PageID = 1;
                                Session.XsmbLog.Add(xsmb);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"UpdateHwanList returned false and failed: {EX}", UTILS.LOG_TYPE.Fatal); return false; }
        }
        public static async Task<bool> LoadEventsScheduler()
        {
            try
            {
                UTILS.EventScheduling.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select * from xQc_FILTER.dbo._EventsSchedule", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string EventName = Convert.ToString(reader[0]);
                                string Day = Convert.ToString(reader[1]);
                                string Time = Convert.ToString(reader[2]);
                                var Events = new List<string>();
                                Events.Add(Day);
                                Events.Add(Time);
                                var testEvents = new KeyValuePair<string, List<string>>(EventName, Events);
                                UTILS.EventScheduling.Add(testEvents);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadEventsScheduler returned false and failed: {EX}"); return false; }
        }

        public static async Task Update_Xsmb(string Msg)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"UPDATE xQc_FILTER.dbo._XsmbEventLog SET Msg = (case when Num in {Msg} then 'Tróng' else 'Tr­ît' end) WHERE Msg = 'Chê kÕt qu¶'", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"Update_Xsmb returned false and failed: {EX}"); }
        }

        public static async Task Update_dailyReward(string CharName,int Total,bool one_5, bool six_10, bool eleven_15, bool sixteen_20, bool twentyone_25)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"UPDATE xQc_FILTER.dbo._DailyReward SET Total = {Total} ,  one_5 ="+(one_5 ? 1 : 0) + " , six_10 ="+ (six_10 ? 1 : 0) + ", eleven_15 = " +(eleven_15 ? 1 : 0) + ", sixteen_20 = "+(sixteen_20 ? 1 : 0) + ", twentyone_25 = "+(twentyone_25 ? 1 : 0) + ", last_seen = GETDATE() where CharName16 = '{CharName}'", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"Update_dailyReward returned false and failed: {EX}"); }
        }
        public static async Task Update_dailyReward(string CharName, int Total)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"UPDATE xQc_FILTER.dbo._DailyReward SET Total = {Total} , last_seen = GETDATE() where CharName16 = '{CharName}'", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"Update_dailyReward returned false and failed: {EX}"); }
        }
        public static async Task Insert_dailyReward(string CharName, int Total, bool one_5, bool six_10, bool eleven_15, bool sixteen_20, bool twentyone_25)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Insert into xQc_FILTER.dbo._DailyReward Values('{CharName}',{Total}," + (one_5 ? 1 : 0) + "," + (six_10 ? 1 : 0) + "," + (eleven_15 ? 1 :0)+","+(sixteen_20 ? 1:0)+","+(twentyone_25?1:0)+",GETDATE()) ", con))
                    {
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"Insert_dailyReward returned false and failed: {EX}"); }
        }
        public static async Task<bool> LoadDailyReward()
        {
            try
            {
                UTILS.DailyReward.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select * from xQc_FILTER.dbo._DailyReward", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string CharName = Convert.ToString(reader[0]);
                                int Total = Convert.ToInt32(reader[1]);
                                bool x1 = Convert.ToBoolean(reader[2]);
                                bool x2 = Convert.ToBoolean(reader[3]);
                                bool x3 = Convert.ToBoolean(reader[4]);
                                bool x4 = Convert.ToBoolean(reader[5]);
                                bool x5 = Convert.ToBoolean(reader[6]);
                                DateTime Time = Convert.ToDateTime(reader[7]);
                                _DailyReward daily = new _DailyReward();
                                daily.Total = Total;
                                daily.one_5 = x1;
                                daily.six_10 = x2;
                                daily.eleven_15 = x3;
                                daily.sixteen_20 = x4;
                                daily.twentyone_25 = x5;
                                daily.last_seen = Time;
                                if (!UTILS.DailyReward.ContainsKey(CharName))
                                    UTILS.DailyReward.TryAdd(CharName, daily);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadDailyReward returned false and failed: {EX}"); return false; }
        }
        public static async Task<bool> LoadDailyRewardItems()
        {
            try
            {
                UTILS.DailyRewardItems.Clear();
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select * from xQc_FILTER.dbo._DailyRewardItems", con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                for(int i =0; i < 20;i++)
                                    UTILS.DailyRewardItems.Add(Convert.ToInt32(reader[i]));
  
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception EX) { UTILS.WriteLine($"LoadDailyRewardItems returned false and failed: {EX}"); return false; }
        }
        public static async Task<short> Get_Cur_DiedRegion(int CharID,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select DiedRegion from {Settings.ShardInfos[ShardID].DBName}.dbo._Char where CharID = {CharID};", con))
                    {
                        await con.OpenAsync();
                        short x = Convert.ToInt16(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return x;

                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"Get_Cur_DiedRegion failed and returned 0: {EX}"); return 0; }
        }
        public static async Task<short> Get_Cur_RecallRegion(int CharID,int ShardID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand($"Select TelRegion from {Settings.ShardInfos[ShardID].DBName}.dbo._Char where CharID = {CharID};", con))
                    {
                        await con.OpenAsync();
                        short x = Convert.ToInt16(await cmd.ExecuteScalarAsync());
                        con.Close();
                        return x;
                    }
                }
            }
            catch (Exception EX) { UTILS.WriteLine($"Get_Cur_RecallRegion failed and returned 0: {EX}"); return 0; }
        }







    }
}