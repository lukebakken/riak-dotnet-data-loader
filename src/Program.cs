namespace riak_data_loader
{
    using CorrugatedIron;

    static class Program
    {
        private static readonly IRiakEndPoint cluster;

        static Program()
        {
            cluster = RiakCluster.FromConfig("riakClusterConfiguration");
        }

        static void Main(string[] args)
        {
        }
    }
}
