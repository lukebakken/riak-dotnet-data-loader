namespace riak_data_loader
{
    using System;
#if DEBUG
    using System.Diagnostics;
#endif
    using Tweetinvi;
    using Tweetinvi.Core.Events.EventArguments;
    using Tweetinvi.Core.Interfaces.Streaminvi;

    public class TwitterStreamer : IDisposable
    {
        private readonly ISampleStream sampleStream;
        private readonly Action<string> onTweetJsonHandler;

        public TwitterStreamer(Action<string> onTweetJsonHandler)
        {
            if (onTweetJsonHandler == null)
            {
                throw new ArgumentNullException("onTweetHandler");
            }

            var twitterAuth = new TwitterAuth();
            TwitterCredentials.SetCredentials(
                twitterAuth.AccessToken, twitterAuth.AccessTokenSecret,
                twitterAuth.ApiKey, twitterAuth.ApiSecret);
#if DEBUG
            var user = User.GetLoggedUser();
            Debug.WriteLine("[info] Twitter user screen name: {0}", user.ScreenName);
#endif

            sampleStream = Stream.CreateSampleStream();
            sampleStream.JsonObjectReceived += sampleStream_JsonObjectReceived;

            this.onTweetJsonHandler = onTweetJsonHandler;
        }

        public void StartStreaming()
        {
            sampleStream.StartStream();
        }

        public void Dispose()
        {
            sampleStream.JsonObjectReceived -= sampleStream_JsonObjectReceived;
        }

        private void sampleStream_JsonObjectReceived(object sender, JsonObjectEventArgs e)
        {
            onTweetJsonHandler(e.Json);
        }
    }
}
