using Dapper;
using SR_PROXY.ENGINES;
using SR_PROXY.MODEL;
using SR_PROXY.SECURITYOBJECTS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using static SR_PROXY.ENGINES.UTILS;

namespace SR_PROXY.MSSQL_SERVER
{
    internal class SG_QUERIES
    {
        private static string ConnectionString => SR_PROXY.Settings.MAIN.checkBox19.Checked
            ? $"Data Source={SR_PROXY.Settings.MSSQL_SVR_NAME}\\SQLEXPRESS,1433;Network Library=DBMSSOCN;max pool size=1000;Initial Catalog={Settings.FILTER_DB};User ID={SR_PROXY.Settings.MSSQL_SVR_ID};Password={SR_PROXY.Settings.MSSQL_SVR_PW};"
            : $"Data Source={SR_PROXY.Settings.MSSQL_SVR_NAME};Initial Catalog={Settings.FILTER_DB};max pool size=1000;Integrated Security=false;UID={SR_PROXY.Settings.MSSQL_SVR_ID};PWD={SR_PROXY.Settings.MSSQL_SVR_PW};";

        #region Veldora SQL
        #endregion

        #region Loading
        public static async Task PreloadFilterDatabaseAsync()
        {
            try
            {
                await LoadUniqueMonsterNoticeAsync();

                await UpdateUniqueHistory();

                //UTILS.Available_Objects =
                await GetAvailableObjectsAsync();
                UTILS.WriteLine("Filter database preload completed.", LOG_TYPE.Notify);
            }
            catch (Exception ex)
            {
                UTILS.WriteLine($"PreloadFilterDatabaseAsync failed: {ex.Message}", LOG_TYPE.Error);
            }
        }
        #endregion

        #region LoadObjects
        public static async Task<Dictionary<int, ItemType>> GetAvailableObjectsAsync()
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    string query = $@"SELECT _RefObjCommon.ID, TypeID1, TypeID2, TypeID3, TypeID4, CodeName128, NameStrID128, Bionic, Rarity, ReqLevel1, ItemClass FROM {Settings.MSSQL_SHARD_DB}.dbo._RefObjCommon LEFT JOIN {Settings.MSSQL_SHARD_DB}.dbo._RefObjItem ON _RefObjCommon.Link = _RefObjItem.ID WHERE [Service] > 0";

                    var result = await connection.QueryAsync<ItemType>(query);

                    return result.ToDictionary(x => x.ID, x => (ItemType)x);
                }
            }
            catch (Exception ex)
            {
                UTILS.WriteLine($"GetAvailableObjectsAsync failed: {ex.Message}", LOG_TYPE.Error);
                return new Dictionary<int, ItemType>();
            }
        }

        #endregion

        #region EventBoard

        public class EventInfo
        {
            public int BannerID { get; set; }
            public string EventName { get; set; }
            public string Day { get; set; }

            public string NoticeTime { get; set; } 

            public string StartTime { get; set; }
            public string FinishTime { get; set; }
            public int Running { get; set; }
            public string Details { get; set; }
            public int IsRegistered { get; set; }
        }

        public static async Task GetNextEvent(bool toAll, AGENT_MODULE info)
        {
            try
            {
                string charName = toAll ? null : info.CHARNAME16;
                var events = await FetchEventDataAsync(charName);
                DateTime now = DateTime.Now;

                // <اسم الحدث, تفاصيله الأقرب فقط>
                var deduplicated = new Dictionary<string, (EventInfo ev, DateTime fullTime, int secondsLeft)>();

                foreach (var ev in events)
                {
                    TimeSpan noticeTime = TimeSpan.Parse(ev.NoticeTime);
                    TimeSpan start = TimeSpan.Parse(ev.StartTime);
                    TimeSpan finish = TimeSpan.Parse(ev.FinishTime);

                    DateTime fullTime;

                    if (ev.Running == 1 || ev.Running == 2)
                    {
                        fullTime = now.Date.Add(noticeTime);
                        if (start < noticeTime) start = start.Add(TimeSpan.FromDays(1));
                        if (finish < start) finish = finish.Add(TimeSpan.FromDays(1));

                        int secondsLeft = ev.Running == 1
                            ? (int)(start - noticeTime).TotalSeconds
                            : (int)(finish - start).TotalSeconds;

                        // إذا وجدنا نسخة أسبق من نفس الحدث، نحتفظ بها فقط
                        if (!deduplicated.ContainsKey(ev.EventName) || deduplicated[ev.EventName].fullTime > fullTime)
                            deduplicated[ev.EventName] = (ev, fullTime, secondsLeft);

                        continue;
                    }

                    if (ev.Day == "Allday")
                    {
                        fullTime = now.Date.Add(noticeTime);
                        if (fullTime <= now)
                            fullTime = fullTime.AddDays(1);
                    }
                    else if (Enum.TryParse(ev.Day, true, out DayOfWeek dayOfWeek))
                    {
                        int targetDay = (int)dayOfWeek;
                        int currentDay = (int)now.DayOfWeek;
                        int daysToAdd = (targetDay - currentDay + 7) % 7;
                        fullTime = now.Date.AddDays(daysToAdd).Add(noticeTime);

                        if (daysToAdd == 0 && fullTime <= now)
                            fullTime = fullTime.AddDays(7);
                    }
                    else continue;

                    int seconds = (int)(fullTime - now).TotalSeconds;

                    // نفس الشيء، نضيف فقط أقرب نسخة من نفس الحدث
                    if (!deduplicated.ContainsKey(ev.EventName) || deduplicated[ev.EventName].fullTime > fullTime)
                        deduplicated[ev.EventName] = (ev, fullTime, seconds);
                }

                // الترتيب حسب أقرب توقيت
                var ordered = deduplicated.Values.OrderBy(x => x.fullTime).ToList();

                for (int i = 0; i < ordered.Count; i++)
                {
                    var entry = ordered[i];
                    int slotId = i + 1;

                    UTILS.NewEventTimer(
                        slotId,
                        entry.ev.BannerID,
                        entry.ev.EventName,
                        entry.secondsLeft,
                        entry.fullTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        entry.ev.Details,
                        entry.ev.Running,
                        toAll ? 2 : entry.ev.IsRegistered,
                        toAll ? null : info.CLIENT_SOCKET,
                        toAll
                    );
                }
            }
            catch (Exception ex)
            {
                LogEventError(ex);
            }
        }
        private static async Task<List<EventInfo>> FetchEventDataAsync(string charName)
        {
            string query = $@"
        SELECT 
            EH.RegisterPhoto AS BannerID,
            ET.EventName,
            ET.Day,
            ET.NoticeTime,
            ET.StartTime,
            ET.FinishTime,
            ET.Running,
            EH.Details,
            CASE 
                WHEN @Charname IS NOT NULL AND EXISTS (
                    SELECT 1 
                    FROM [{Settings.FILTER_DB}].[dbo].[_Event_Registration] R 
                    WHERE R.EventName = ET.EventName AND R.CharName = @Charname
                ) THEN 1 
                ELSE 0 
            END AS IsRegistered
        FROM [{Settings.FILTER_DB}].[dbo].[_Event_Time] ET
        LEFT JOIN [{Settings.FILTER_DB}].[dbo].[_Event_Handle] EH ON EH.EventName = ET.EventName
        WHERE ET.Service = 1;";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                return (await connection.QueryAsync<EventInfo>(query, new { Charname = charName })).ToList();
            }
        }

        private static bool HandleRunningEvent(List<EventInfo> eventList, DateTime now, bool toAll, AGENT_MODULE info)
        {
            int slotId = 1;

            foreach (var ev in eventList)
            {
                if (ev.Running != 1 && ev.Running != 2)
                    continue;

                TimeSpan noticeTime = TimeSpan.Parse(ev.NoticeTime);
                TimeSpan start = TimeSpan.Parse(ev.StartTime);
                TimeSpan finish = TimeSpan.Parse(ev.FinishTime);

                if (start < noticeTime) start = start.Add(TimeSpan.FromDays(1));
                if (finish < start) finish = finish.Add(TimeSpan.FromDays(1));

                int secondsLeft = ev.Running == 1
                    ? (int)(start - noticeTime).TotalSeconds
                    : (int)(finish - start).TotalSeconds;

                DateTime fullDate = now.Date.Add(noticeTime);

                UTILS.NewEventTimer(
                    slotId++, // ✅ هذا هو SlotID
                    ev.BannerID,
                    ev.EventName,
                    secondsLeft,
                    fullDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ev.Details,
                    ev.Running,
                    toAll ? 2 : ev.IsRegistered,
                    toAll ? null : info.CLIENT_SOCKET,
                    toAll
                );
            }

            return false;
        }
        private static void HandleUpcomingEvent(List<EventInfo> eventList, DateTime now, bool toAll, AGENT_MODULE info)
        {
            var upcoming = new List<(EventInfo ev, DateTime fullTime, int secondsLeft)>();

            foreach (var ev in eventList)
            {
                TimeSpan noticeTime = TimeSpan.Parse(ev.NoticeTime);
                DateTime fullDate;

                if (ev.Day == "Allday")
                {
                    fullDate = now.Date.Add(noticeTime);
                    if (fullDate <= now)
                        fullDate = fullDate.AddDays(1);
                }
                else if (Enum.TryParse(ev.Day, true, out DayOfWeek dayOfWeek))
                {
                    int targetDay = (int)dayOfWeek;
                    int currentDay = (int)now.DayOfWeek;
                    int daysToAdd = (targetDay - currentDay + 7) % 7;
                    fullDate = now.Date.AddDays(daysToAdd).Add(noticeTime);

                    if (daysToAdd == 0 && fullDate <= now)
                        fullDate = fullDate.AddDays(7);
                }
                else continue;

                int secondsLeft = (int)(fullDate - now).TotalSeconds;
                upcoming.Add((ev, fullDate, secondsLeft));
            }

            var ordered = upcoming.OrderBy(e => e.fullTime).ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                var ev = ordered[i];
                int slotId = i + 1;

                UTILS.NewEventTimer(
                    slotId,
                    ev.ev.BannerID,
                    ev.ev.EventName,
                    ev.secondsLeft,
                    ev.fullTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ev.ev.Details,
                    ev.ev.Running,
                    toAll ? 2 : ev.ev.IsRegistered,
                    toAll ? null : info.CLIENT_SOCKET,
                    toAll
                );
            }
        }
        private static void LogEventError(Exception ex)
        {
            UTILS.WriteLine("GetNextEvent failed!");
            UTILS.WriteLine($"Message: {ex.Message}");
            UTILS.WriteLine($"Source: {ex.Source}");
            UTILS.WriteLine($"TargetSite: {ex.TargetSite}");
            UTILS.WriteLine($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                UTILS.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }

        private static void SendEvent(int slotId, EventInfo ev, int secondsLeft, DateTime fullTime, bool toAll, AGENT_MODULE info)
        {
            UTILS.NewEventTimer(
                slotId,
                ev.BannerID,
                ev.EventName,
                secondsLeft,
                fullTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ev.Details,
                ev.Running,
                toAll ? 2 : ev.IsRegistered,
                toAll ? null : info.CLIENT_SOCKET,
                toAll
            );
        }
        #endregion
        
        #region ServerRank
        public static async Task SendAllServerRanksAsync(Socket CLIENT_SOCKET)
        {
            try
            {
                using (var con = new SqlConnection(ConnectionString))
                {
                    var ranks = await con.QueryAsync<(int ID, string RankName)>(
                        $"SELECT ID, RankName FROM {Settings.FILTER_DB}.dbo._ServerRank"
                    );

                    foreach (var rank in ranks)
                    {
                        UTILS.ServerRankConfig(rank.ID, rank.RankName, CLIENT_SOCKET);
                    }
                }
            }
            catch (Exception ex)
            {
                UTILS.WriteLine("SendAllServerRanksAsync failed: " + ex.Message);
            }
        }
        public static async Task LoadServerRank(string rankName, Socket CLIENT_SOCKET)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                string query = $@"
            SELECT 
                C.CharName16 AS CharName,
                ISNULL(G.Name, 'No Guild') AS Guild,
                CAST(R.Points AS nvarchar) AS Points,
                C.RefObjID AS CharIcon,
                SR.ImageID,
                SR.FrameID
            FROM 
                SRO_VT_SHARD.._Char C
            LEFT JOIN 
                SRO_VT_SHARD.._Guild G ON C.GuildID = G.ID
            INNER JOIN 
                {Settings.FILTER_DB}.dbo._ServerRankRecords R 
                    ON R.Charname COLLATE SQL_Latin1_General_CP1_CI_AS = C.CharName16 COLLATE SQL_Latin1_General_CP1_CI_AS
            INNER JOIN 
                {Settings.FILTER_DB}.dbo._ServerRank SR ON SR.ID = R.ServerRank_ID
            WHERE 
                SR.RankName = @RankName
            ORDER BY 
                R.Points DESC;
        ";

                var result = await connection.QueryAsync<dynamic>(query, new { RankName = rankName });

                int rank = 1;
                foreach (var row in result)
                {
                    string charName = row.CharName;
                    string guild = row.Guild;
                    string points = row.Points;
                    int charIcon = row.CharIcon;
                    int imageId = row.ImageID;
                    int frameId = row.FrameID;

                    UTILS.ServerRankLoad(rank, charName, guild, points, charIcon, imageId, frameId, CLIENT_SOCKET);
                    rank++;
                }
            }
        }
        #endregion

        #region UNIQUE_HISTORY
        private static System.Timers.Timer debounceTimer = null;
        private static readonly object debounceLock = new object();

        public static void RequestUpdateUniqueHistory()
        {
            lock (debounceLock)
            {
                // لو المؤقت موجود، نعيد تشغيله
                if (debounceTimer != null)
                {
                    debounceTimer.Stop();
                    debounceTimer.Dispose();
                }

                debounceTimer = new System.Timers.Timer(1000); // 1000ms = 1 ثانية
                debounceTimer.AutoReset = false;
                debounceTimer.Elapsed += async (sender, e) =>
                {
                    await UpdateUniqueHistory();
                };
                debounceTimer.Start();
            }
        }

        public static async Task<bool> UpdateUniqueHistory()
        {
            try
            {
                UTILS.UniqueLog.Clear();

                string query = $@"
            SELECT TOP 100 
                ROW_NUMBER() OVER (ORDER BY K.[State] DESC) AS LineID,
                K.ID AS PageID,
                K.KillerName AS Killer,
                K.UniqueName,
                K.KilledTime AS Time,
                K.State AS Status,
                K.UniqueID,
                ISNULL(M.RegionID, 0) AS RegionID,
                ISNULL(M.PostionX, 0) AS PostionX,
                ISNULL(M.PostionZ, 0) AS PostionZ,
                ISNULL(M.CircaleRadius, 0) AS CircaleRadius,
                ISNULL(M.Image, 0) AS Image
            FROM {Settings.FILTER_DB}.[dbo].[_LogUniqueKills] K
            LEFT JOIN {Settings.FILTER_DB}.[dbo].[_MonsterNotice] M
                ON K.UniqueName = M.CodeName
            ORDER BY K.State DESC, K.KilledTime DESC";

                using (var con = new SqlConnection(ConnectionString))
                {
                    var result = (await con.QueryAsync<UniqueMob>(query)).ToList();

                    foreach (var mob in result)
                    {
                        UTILS.UniqueLog.TryAdd(mob.LineID, mob);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                UTILS.WriteLine($"UpdateUniqueHistory failed: {ex.Message}", LOG_TYPE.Error);
                return false;
            }
        }

        public static async Task<List<Notification>> LoadUniqueMonsterNoticeAsync()
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    string query = $@"SELECT ID AS UniqueID, [Name] AS UniqueName FROM {Settings.FILTER_DB}.dbo._MonsterNotice WHERE Service > 0";

                    var result = await connection.QueryAsync<Notification>(query);

                    UTILS.NotificationList = result.ToList();

                    UTILS.WriteLine("Unique monster notices loaded successfully.", LOG_TYPE.System);

                    return UTILS.NotificationList;
                }
            }
            catch (Exception ex)
            {
                UTILS.WriteLine($"Failed to load unique monster notices: {ex.Message}", LOG_TYPE.Error);
                return new List<Notification>(); // Return empty list on failure
            }
        }
        #endregion

        #region Hall_Of_Fame
        public class HallOfFameEntry
        {
            public string CharName { get; set; }
            public int CharIcon { get; set; }
            public int Points { get; set; }
            public int Rank { get; set; }
            public int FL_1 { get; set; }
            public int FL_9 { get; set; }
            public int FL_49 { get; set; }
            public int FL_99 { get; set; }
            public int FL_499 { get; set; }
            public int FL_999 { get; set; }
        }
        public static async Task LoadHallOfFameAsync(string charName, Socket CLIENT_SOCKET)
        {
            try
            {
                using (var con = new SqlConnection(ConnectionString))
                {
                    string query = @"
                WITH RankedPlayers AS (
                    SELECT ID, ROW_NUMBER() OVER (ORDER BY Points DESC) AS NewRank 
                    FROM [SHADOW_GARDEN].[dbo].[_Hall_Of_Fame]
                )
                UPDATE h 
                SET h.Rank = rp.NewRank 
                FROM [SHADOW_GARDEN].[dbo].[_Hall_Of_Fame] h 
                JOIN RankedPlayers rp ON h.ID = rp.ID;

                SELECT CharName, CharIcon, Points, [Rank], 0 AS FL_1, 0 AS FL_9, 0 AS FL_49, 
                       0 AS FL_99, 0 AS FL_499, 0 AS FL_999 
                FROM (
                    SELECT TOP 8 CharName, CharIcon, Points, [Rank]
                    FROM [SHADOW_GARDEN].[dbo].[_Hall_Of_Fame] 
                    ORDER BY Points DESC
                ) AS TopPlayers

                UNION ALL 

                SELECT CharName, CharIcon, Points, [Rank], FL_1, FL_9, FL_49, FL_99, FL_499, FL_999 
                FROM [SHADOW_GARDEN].[dbo].[_Hall_Of_Fame] 
                WHERE CharName = @CharName;
            ";

                    var result = await con.QueryAsync<HallOfFameEntry>(query, new { CharName = charName });

                    uint id = 1;
                    foreach (var row in result)
                    {
                        HallOfFameSend(
                            id++,
                            (uint)row.Rank,
                            row.CharName,
                            row.Points.ToString(),
                            (uint)row.CharIcon,
                            (uint)row.FL_1,
                            (uint)row.FL_9,
                            (uint)row.FL_49,
                            (uint)row.FL_99,
                            (uint)row.FL_499,
                            (uint)row.FL_999,
                            CLIENT_SOCKET
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                UTILS.WriteLine($"LoadHallOfFameAsync failed: {ex.Message}", LOG_TYPE.Error);
            }
        }
        public static async Task<(string SenderMessage, string ReceiverMessage)> HallOfFameSendPoints(string sender, string receiver, uint points)
        {
            string senderMessage = string.Empty;
            string receiverMessage = string.Empty;

            try
            {
                using (var con = new SqlConnection(ConnectionString))
                {
                    var parameters = new DynamicParameters();

                    parameters.Add("@Sender", UTILS.INJECTION_PREFIX(sender), DbType.String, size: 64);
                    parameters.Add("@Receiver", UTILS.INJECTION_PREFIX(receiver), DbType.String, size: 64);
                    parameters.Add("@Points", points, DbType.Int32);

                    parameters.Add("@SenderMessage", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);
                    parameters.Add("@ReceiverMessage", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

                    await con.ExecuteAsync("SHADOW_GARDEN.._SYS_Hall_Of_Fame", parameters, commandType: CommandType.StoredProcedure);

                    senderMessage = parameters.Get<string>("@SenderMessage") ?? "No response from procedure.";
                    receiverMessage = parameters.Get<string>("@ReceiverMessage") ?? "No response from procedure.";
                }
            }
            catch (Exception ex)
            {
                UTILS.WriteLine($"HallOfFameSendPoints failed: {ex.Message}", LOG_TYPE.Error);
                senderMessage = "⚠️ Failed to send points.";
                receiverMessage = "⚠️ Failed to receive points.";
            }

            return (senderMessage, receiverMessage);
        }
        public static async Task SearchHallOfFameAsync(string charNamePartial, Socket CLIENT_SOCKET)
        {
            try
            {
                using (var con = new SqlConnection(ConnectionString))
                {
                    // 1. تحديث الترتيب
                    string updateRanks = @"
                WITH RankedPlayers AS (
                    SELECT ID, ROW_NUMBER() OVER (ORDER BY Points DESC) AS NewRank 
                    FROM [SHADOW_GARDEN].[dbo].[_Hall_Of_Fame]
                )
                UPDATE h 
                SET h.Rank = rp.NewRank 
                FROM [SHADOW_GARDEN].[dbo].[_Hall_Of_Fame] h 
                JOIN RankedPlayers rp ON h.ID = rp.ID;
            ";
                    await con.ExecuteAsync(updateRanks);

                    // 2. البحث بالاسم
                    string searchQuery = @"
                SELECT CharName, CharIcon, Points, [Rank], FL_1, FL_9, FL_49, FL_99, FL_499, FL_999 
                FROM [SHADOW_GARDEN].[dbo].[_Hall_Of_Fame] 
                WHERE CharName LIKE @Search";

                    var result = await con.QueryAsync<HallOfFameEntry>(searchQuery, new { Search = $"%{charNamePartial}%" });

                    // 3. إرسال النتائج إلى CLIENT_SOCKET
                    uint id = 1;
                    foreach (var row in result)
                    {
                        HallOfFameSend(
                            id++,
                            (uint)row.Rank,
                            row.CharName,
                            row.Points.ToString(),
                            (uint)row.CharIcon,
                            (uint)row.FL_1,
                            (uint)row.FL_9,
                            (uint)row.FL_49,
                            (uint)row.FL_99,
                            (uint)row.FL_499,
                            (uint)row.FL_999,
                            CLIENT_SOCKET
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                UTILS.WriteLine($"SearchHallOfFameAsync failed: {ex.Message}", LOG_TYPE.Error);
            }
        }

        #endregion

    }
}
