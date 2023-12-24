using Grpc.Core;
using System.Collections.Concurrent;

namespace P2P.SeedNode
{
    internal class Program
    {

        static void Main(string[] args)
        {
            const int Port = 50055;

            var server = new Server
            {
                Services = { SeedNodeService.BindService(new SeedNodeServiceImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine($"Seed node is running on port {Port}");
            Console.WriteLine("Press any key to stop the seed node...");
            Console.ReadKey();


            server.ShutdownAsync().Wait();
        }
    }

    internal static class RegistDict
    {
        public static readonly ConcurrentDictionary<string, bool> RegisteredNodes = new ConcurrentDictionary<string, bool>();
    }
}