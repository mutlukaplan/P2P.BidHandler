using Shared;

namespace ConsoleApp2
{
    class Program : NodeBase
    {
        static Program()
        {
            NodeHost = "localhost";
            NodePort = 50051;
        }

        static async Task Main(string[] args)
        {
            RunPeer();
        }
    }
}
