using Shared;

namespace P2P.Node3
{
    internal class Program : NodeBase
    {
        public override string NodeHost => "localhost";
        public override int NodePort => 50053;
        public static Program Instance => new();

        static async Task Main(string[] args)
        {
            Instance.RunPeer();
        }
    }
}