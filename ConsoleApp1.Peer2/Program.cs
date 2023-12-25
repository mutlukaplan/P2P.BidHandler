using Shared;

namespace ConsoleApp1.Peer2
{
    class Program : NodeBase
    {
        static Program()
        {
            NodeHost = "localhost";
            NodePort = 50052;
        }

        static async Task Main(string[] args)
        {
            RunPeer();
        }
    }

}