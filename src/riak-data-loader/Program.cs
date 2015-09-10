namespace riak_data_loader
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using RiakClient;
    using RiakClient.Models;
    using TwitterLib;

    static class Program
    {
        private static readonly IRiakEndPoint cluster = null;

        private static bool running = true;
        private static uint tweetCount = 1024;
        private static int tweetsReceived = 0;

        private const ushort streamerCount = 8;
        private const ushort saverCount = 8;

        private static List<Task> streamerTasks = new List<Task>();
        private static List<TwitterStreamer> streamers = new List<TwitterStreamer>();

        private static List<Task> saverTasks = new List<Task>();

        private static ConcurrentQueue<string> tweetsQueue = new ConcurrentQueue<string>();
        private static AutoResetEvent tweetEvent = new AutoResetEvent(false);

        static Program()
        {
            cluster = RiakCluster.FromConfig("riakClusterConfiguration");
        }

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            if (args != null && args.Length > 0)
            {
                tweetCount = Convert.ToUInt32(args[0]);
            }

            var client = cluster.CreateClient();
            var pingResult = client.Ping();

            if (pingResult.IsSuccess)
            {
                Console.Out.WriteLine("[info] Streaming tweets. CTRL-C stops.");

                for (ushort i = 0; i < saverCount; i++)
                {
                    Console.Out.WriteLine("[info] starting saver {0}", i);
                    saverTasks.Add(Task.Run(() => TweetSaver()));
                }

                for (ushort i = 0; i < streamerCount; i++)
                {
                    Console.Out.WriteLine("[info] starting streamer {0}", i);
                    streamerTasks.Add(Task.Run(() =>
                    {
                        var streamer = new TwitterStreamer(OnTweetReceived);
                        streamers.Add(streamer);
                        streamer.StartStreaming();
                    }));
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

            Console.Out.WriteLine("[info] stopping streamers...");

            streamers.ForEach(s =>
            {
                s.StopStreaming();
                s.Dispose();
            });

            Console.Out.WriteLine("[info] waiting for saver tasks...");
            Task.WaitAll(saverTasks.ToArray());

            Console.Out.WriteLine("[info] waiting for streamer tasks...");
            Task.WaitAll(streamerTasks.ToArray());

            Console.Out.WriteLine("[info] All done! - EXITING");
            if (exit)
            {
                Environment.Exit(0);
            }
        }

        static void TweetSaver()
        {
            string tweetJson;
            IRiakClient client = cluster.CreateClient();

            while (running && tweetsReceived <= tweetCount)
            {
                tweetEvent.WaitOne(TimeSpan.FromMilliseconds(250));
                if (tweetsQueue.TryDequeue(out tweetJson))
                {
                    var twitterParser = new TwitterParser(tweetJson);
                    if (twitterParser.IsTweet)
                    {
                        // Console.Out.WriteLine("[info] thread {0} tweet {1}", Thread.CurrentThread.ManagedThreadId, twitterParser.IdStr);
                        var id = new RiakObjectId("tweets-type", "tweets", twitterParser.IdStr);
                        var value = new RiakObject(id, twitterParser.Bytes, RiakConstants.ContentTypes.ApplicationJson, Encoding.UTF8.EncodingName);

                        var result = client.Put(value);
                        if (result.IsSuccess == false)
                        {
                            Console.Error.WriteLine("[error] PUT error: {0}", result.ErrorMessage);
                        }
                        else
                        {
                            Interlocked.Increment(ref tweetsReceived);
                            if (tweetsReceived % 64 == 0)
                            {
                                Console.Out.WriteLine("[info] tweets stored: {0}", tweetsReceived);
                            }
                        }

                        if (tweetsReceived >= tweetCount)
                        {
                            Console.Out.WriteLine("[info] tweets stored: {0}, EXITING", tweetsReceived);
                            return;
                        }
                    }
                }
            }
        }

        static void OnTweetReceived(string tweetJson)
        {
            tweetsQueue.Enqueue(tweetJson);
            tweetEvent.Set();
        }
    }
}