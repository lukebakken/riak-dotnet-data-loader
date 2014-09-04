namespace riak_data_loader
{
    using System;
    using System.Globalization;
    using System.Text;
    using Newtonsoft.Json.Linq;

    public class TwitterParser
    {
        private readonly string tweetJson;

        private bool is_tweet = false;
        private string id_str = null;
        private byte[] bytes = null;

        public TwitterParser(string tweetJson)
        {
            if (tweetJson.IsNullOrWhitespace())
            {
                throw new ArgumentNullException("tweetJson");
            }
            this.tweetJson = tweetJson;

            this.ToByteArray();
        }

        public bool IsTweet
        {
            get { return is_tweet; }
        }

        public string IdStr
        {
            get { return id_str; }
        }

        public byte[] Bytes
        {
            get { return bytes; }
        }

        private void ToByteArray()
        {
            var json = JObject.Parse(tweetJson);

            id_str = (string)json["id_str"];
            string created_at_str = (string)json["created_at"];

            if (id_str.IsNullOrWhitespace() || created_at_str.IsNullOrWhitespace())
            {
                bytes = null;
                is_tweet = false;
            }
            else
            {
                var created_at = ParseTwitterDateTime(created_at_str);
                var created_at_xmlschema = created_at.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

                json["created_at"] = created_at_xmlschema;

                bytes = Encoding.UTF8.GetBytes(json.ToString());
                is_tweet = true;
            }
        }

        private static DateTime ParseTwitterDateTime(string date)
        {
            const string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
            return DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);
        }
    }
}
