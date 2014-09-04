namespace TwitterLib
{
    using System;
    using System.IO;
    using YamlDotNet.RepresentationModel;

    public class TwitterAuth
    {
        private readonly string apiKey = String.Empty;
        private readonly string apiSecret = String.Empty;
        private readonly string accessToken = String.Empty;
        private readonly string accessTokenSecret = String.Empty;

        private static readonly YamlScalarNode consumer_key = new YamlScalarNode("consumer_key");
        private static readonly YamlScalarNode consumer_secret = new YamlScalarNode("consumer_secret");
        private static readonly YamlScalarNode access_token = new YamlScalarNode("access_token");
        private static readonly YamlScalarNode access_token_secret = new YamlScalarNode("access_token_secret");

        public TwitterAuth()
        {
            string twauth = Path.Combine(HomeDir, ".twauth");
            if (File.Exists(twauth))
            {
                using (var fs = new FileStream(twauth, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        var yaml = new YamlStream();
                        yaml.Load(sr);

                        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                        
                        YamlNode consumer_key_value;
                        mapping.Children.TryGetValue(consumer_key, out consumer_key_value);
                        apiKey = ((YamlScalarNode)consumer_key_value).Value;

                        YamlNode consumer_secret_value;
                        mapping.Children.TryGetValue(consumer_secret, out consumer_secret_value);
                        apiSecret = ((YamlScalarNode)consumer_secret_value).Value;

                        YamlNode access_token_value;
                        mapping.Children.TryGetValue(access_token, out access_token_value);
                        accessToken = ((YamlScalarNode)access_token_value).Value;

                        YamlNode access_token_secret_value;
                        mapping.Children.TryGetValue(access_token_secret, out access_token_secret_value);
                        accessTokenSecret = ((YamlScalarNode)access_token_secret_value).Value;
                    }
                }
            }
        }

        public string ApiKey
        {
            get { return apiKey; }
        }

        public string ApiSecret
        {
            get { return apiSecret; }
        }

        public string AccessToken
        {
            get { return accessToken; }
        }

        public string AccessTokenSecret
        {
            get { return accessTokenSecret; }
        }

        private string HomeDir
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); }
        }
    }
}
