using Shared;

namespace P2P.Node3
{
    internal class Program: NodeBase
    {
        static Program()
        {
            NodeHost = "localhost";
            NodePort = 50053;
        }

        static async Task Main(string[] args)
        {
            RunPeer();
        }
    }
}