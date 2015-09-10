namespace riak_data_loader
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using RiakClient;
    using RiakClient.Models;

    static class Program
    {
        private static readonly IRiakEndPoint cluster = null;

        private static bool running = true;
        private static uint objectCount = 1024;
        private static int objectsSaved = 0;

        private const ushort saverCount = 48;

        private static List<Task> saverTasks = new List<Task>();

        static Program()
        {
            cluster = RiakCluster.FromConfig("riakClusterConfiguration");
        }

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            if (args != null && args.Length > 0)
            {
                objectCount = Convert.ToUInt32(args[0]);
            }

            var client = cluster.CreateClient();
            var pingResult = client.Ping();

            if (pingResult.IsSuccess)
            {
                Console.Out.WriteLine("[info] saving random data. CTRL-C stops.");

                for (ushort i = 0; i < saverCount; i++)
                {
                    Console.Out.WriteLine("[info] starting saver {0}", i);
                    saverTasks.Add(Task.Run(() => ObjectSaver()));
                }

                Task.WaitAll(saverTasks.ToArray());
            }
            else
            {
                Console.Error.WriteLine("[error] cluster PING error: {0}", pingResult.ErrorMessage);
            }

            Exit();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Exit(false);
        }

        static void Exit(bool exit = true)
        {
            running = false;

            Console.Out.WriteLine("[info] waiting for saver tasks...");
            Task.WaitAll(saverTasks.ToArray());

            Console.Out.WriteLine("[info] All done! - EXITING");
            if (exit)
            {
                Environment.Exit(0);
            }
        }

        static void ObjectSaver()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            IRiakClient client = cluster.CreateClient();

            Random r = new Random(threadId);
            var bytes = new byte[65536];
            r.NextBytes(bytes);

            while (running && objectsSaved <= objectCount)
            {
                string idStr = String.Format("{0}_{1}", threadId, objectsSaved);

                var id = new RiakObjectId("data-type", "data", idStr);
                var value = new RiakObject(id, bytes, RiakConstants.ContentTypes.ApplicationOctetStream, RiakConstants.CharSets.Binary);

                var result = client.Put(value);
                if (result.IsSuccess == false)
                {
                    Console.Error.WriteLine("[error] PUT error: {0}", result.ErrorMessage);
                }
                else
                {
                    Interlocked.Increment(ref objectsSaved);
                    if (objectsSaved % 64 == 0)
                    {
                        Console.Out.WriteLine("[info] data stored: {0}", objectsSaved);
                    }
                }

                if (objectsSaved >= objectCount)
                {
                    Console.Out.WriteLine("[info] data stored: {0}, EXITING", objectsSaved);
                    return;
                }
            }
        }
    }
}