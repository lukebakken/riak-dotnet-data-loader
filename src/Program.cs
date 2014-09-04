namespace riak_data_loader
{
    using System.Text;
    using CorrugatedIron;
    using CorrugatedIron.Models;
    using Newtonsoft.Json.Linq;
    using Tweetinvi.Core.Interfaces;

    static class Program
    {
        private static readonly IRiakClient client;

        static Program()
        {
            var cluster = RiakCluster.FromConfig("riakClusterConfiguration");
            client = cluster.CreateClient();
        }

        static void Main(string[] args)
        {
            var twitterStreamer = new TwitterStreamer(OnTweetReceived);
        }

        static void OnTweetReceived(string tweetJson)
        {
            var json = JObject.Parse(tweetJson);
            string id_str = (string)json["id_str"];
            var value = new RiakObject("tweets-type", "tweets", id_str,
                Encoding.UTF8.GetBytes(tweetJson), "application/json", Encoding.UTF8.EncodingName);
            var result = client.Put(value);
            if (result.IsSuccess == false)
            {
                System.Console
            }
        }
    }
}
