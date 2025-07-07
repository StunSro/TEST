using SR_PROXY.ENGINES;
using Framework;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SR_PROXY.SECURITYOBJECTS;
namespace SRO_PROXY.GS
{
    //Manager for stuff coming from the gameserver addon.
    class GameServerManager
    {
        private const int UniqueDmgDisplayQueuePollIntervalMs = 1000;
        private const int UniqueDmgDisplayTimeMs = 10000;
        public static int DMGMeterForm
        { get; set; } = 11446;

        private static readonly ConcurrentQueue<UniqueDamageInfo> UniqueDmgDisplayQueue = new ConcurrentQueue<UniqueDamageInfo>();
        private static Task UniqueDmgDisplayTask = null;
        private static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        static GameServerManager()
        {
            UniqueDmgDisplayQueue = new ConcurrentQueue<UniqueDamageInfo>();
            UniqueDmgDisplayTask = null;
            CancellationTokenSource = new CancellationTokenSource();
        }


        private async static void UniqueDmgDisplayTaskWorker(ManualResetEventSlim startupMRE , int ShardID)
        {
            startupMRE.Set();

            while (!CancellationTokenSource.IsCancellationRequested)
            {
                if (UniqueDmgDisplayQueue.Count == 0)
                {
                    await Task.Delay(UniqueDmgDisplayQueuePollIntervalMs);
                    continue;
                }

                while (UniqueDmgDisplayQueue.TryDequeue(out UniqueDamageInfo record))
                {
                    BroadcastUniqueDmgInfo(record);
                    await Task.Delay(UniqueDmgDisplayTimeMs);
                }

                //Hide the window 10 seconds after broadcast of unique dmg info completes.
                Packet packet = new Packet(0x5101);
                packet.WriteValue<int>(DMGMeterForm);
                packet.WriteValue<byte>(0);
                UTILS.BroadCastToClients(packet , ShardID);
            }
        }
        public static void Start(int ShardID)
        {
            var mre = new ManualResetEventSlim(initialState: false);

            UniqueDmgDisplayTask = Task.Factory.StartNew(() => UniqueDmgDisplayTaskWorker(mre,ShardID),
                CancellationTokenSource.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);

            mre.Wait();
        }

        public static void Shutdown()
        {
            CancellationTokenSource.Cancel();
        }

        public static void HandleUniqueDamageInfo(UniqueDamageInfo record) =>
            UniqueDmgDisplayQueue.Enqueue(record);

        private static void BroadcastUniqueDmgInfo(UniqueDamageInfo record)
        {
            var uniqueInfoMsg = new Packet(0x5103);

            uniqueInfoMsg.WriteValue<int>(record.RefObjID);

            //TODO: Sort, remove items if over max.
            Dictionary<uint, int> records = record.GetSorted(8);

            uniqueInfoMsg.WriteValue<int>(records.Count);

            foreach (var kvp in records)
            {
                uniqueInfoMsg.WriteValue<int>(kvp.Key);
                uniqueInfoMsg.WriteValue<string>(UTILS.FormatNumber(kvp.Value));
            }

            //for only who attacks
            foreach (var kvp in records)
            {
                AGENT_MODULE agentModule = null;
                agentModule = UTILS.GetAgentByGID(kvp.Key);
                if (agentModule != null)
                {
                    agentModule.DMGMeterGui = true;
                    agentModule.LOCAL_SECURITY.Send(uniqueInfoMsg);
                    agentModule.ASYNC_SEND_TO_CLIENT(agentModule.CLIENT_SOCKET);
                }

            }
        }

    }
}
