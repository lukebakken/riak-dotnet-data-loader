namespace riak_data_loader
{
    using System;
    using System.Globalization;
    using System.Text;
    using CorrugatedIron;
    using CorrugatedIron.Models;
    using Newtonsoft.Json.Linq;

    static class Program
    {
        private static readonly IRiakEndPoint cluster = null;

        private static ushort tweetCount = 1024;
        private static ushort tweetsReceived = 0;

        private static TwitterStreamer twitterStreamer = null;

        static Program()
        {
            cluster = RiakCluster.FromConfig("riakClusterConfiguration");
        }

        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                tweetCount = Convert.ToUInt16(args[0]);
            }

            var client = cluster.CreateClient();
            var pingResult = client.Ping();
            if (pingResult.IsSuccess)
            {
                Console.Out.WriteLine("[info] Streaming tweets. Hit any key to stop.");

                using (twitterStreamer = new TwitterStreamer(OnTweetReceived))
                {
                    twitterStreamer.StartStreaming();
                    int read = Console.Read();
                    twitterStreamer.StopStreaming();
                }
            }
            else
            {
                Console.Error.WriteLine("[error] cluster PING error: {0}", pingResult.ErrorMessage);
            }
        }

        static void Exit()
        {
            twitterStreamer.StopStreaming();
            twitterStreamer.Dispose();
            Environment.Exit(0);
        }

        static void OnTweetReceived(string tweetJson)
        {
            var twitterParser = new TwitterParser(tweetJson);

            if (twitterParser.IsTweet)
            {
                var value = new RiakObject("tweets-type", "tweets", twitterParser.IdStr,
                    twitterParser.Bytes, "application/json", Encoding.UTF8.EncodingName);

                var client = cluster.CreateClient();
                var result = client.Put(value);
                if (result.IsSuccess == false)
                {
                    Console.Error.WriteLine("[error] PUT error: {0}", result.ErrorMessage);
                }
                else
                {
                    ++tweetsReceived;
                    if (tweetsReceived % 64 == 0)
                    {
                        Console.Out.WriteLine("[info] tweets stored: {0}", tweetsReceived);
                    }
                }

                if (tweetsReceived > tweetCount)
                {
                    Console.Out.WriteLine("[info] tweets stored: {0}, EXITING", tweetsReceived);
                    Exit();
                }
            }
        }
    }
}
