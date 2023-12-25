using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Shared;
using Shared.Services;
using System.Collections.Concurrent;

namespace ConsoleApp2
{
    class Program : NodeBase
    {
        static Program()
        {
            mainChannel = GrpcChannel.ForAddress($"http://localhost:{MainChannelPort}");
            seedClient = new SeedNodeService.SeedNodeServiceClient(mainChannel);
            broadcastClient = new BroadcastService.BroadcastServiceClient(mainChannel);
            NodeId = Guid.NewGuid().ToString();
            NodeHost = "localhost";
            NodePort = 50051;
        }

        static async Task Main(string[] args)
        {
            Server server = RegisterServices();
            RegisterClientAndStart(NodePort, NodeId, server, NodeHost);
            StartListeningBroadcastMessages();
            ShowMenu();
            GetInput();
            server.ShutdownAsync().Wait();
        }

        private static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("\n\n\n");
            Console.WriteLine("                    P2p bidder ");
            Console.WriteLine("============================================================");
            Console.WriteLine("============================================================");
            Console.WriteLine("                    1. Get All Auctions");
            Console.WriteLine("                    2. Make a Bid for an auction");
            Console.WriteLine("                    3. Create Auction");
            Console.WriteLine("                    4. My Auctions");
            Console.WriteLine("                    5. Accept the bid");
            Console.WriteLine("                    6. Exit");
            Console.WriteLine("------------------------------------------------------------");
        }
    }
}
