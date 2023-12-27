using Shared;

namespace ConsoleApp2
{
    internal class Program : NodeBase
    {
        public override string NodeHost => "localhost";
        public override int NodePort => 50051;
        public static Program Instance => new();

        static async Task Main(string[] args)
        {
            Instance.RunPeer();
        }
    }
}
