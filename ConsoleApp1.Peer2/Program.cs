using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;
using System.Threading.Channels;

namespace ConsoleApp1.Peer2
{
    class Program
    {

        static async Task Main(string[] args)
        {
            const int NodePort = 50052;
            const string NodeHost = "localhost";

            string nodeId = Guid.NewGuid().ToString();

            var serviceProvider = new ServiceCollection()
                    .AddSingleton<IAuctionCache, AuctionCache>() // Register your local service implementation.
                    .BuildServiceProvider();

            var serviceCache= serviceProvider.GetRequiredService<IAuctionCache>();

            var server = new Server
            {
                Services = { AuctionService.BindService(new AutionServiceImpl(serviceCache)) },
                Ports = { new ServerPort(NodeHost, NodePort, ServerCredentials.Insecure) }
            };

            RegisterClientAndStart(NodePort, nodeId, server, NodeHost);

            //var channel = GrpcChannel.ForAddress($"http://localhost:{50052}");



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
            Console.WriteLine("                    2. Make a Bid");
            Console.WriteLine("                    3. Exit");
            Console.WriteLine("------------------------------------------------------------");
        }

        private static void RegisterClientAndStart(int NodePort, string nodeId, Server server, string NodeHost)
        {
            server.Start();
            const int MainChannelPort = 50055;

            Console.WriteLine($"Server listening on port {NodePort}");

            var mainChannel = GrpcChannel.ForAddress($"http://localhost:{MainChannelPort}");

            var seedClient = new SeedNodeService.SeedNodeServiceClient(mainChannel);

            var nodeAddress = $"http://{NodeHost}:{NodePort}";

            var registerRequest = new RegisterRequest { NodeId = nodeId, NodeAddress= nodeAddress };

            var response = seedClient.RegisterNode(registerRequest);

            Console.WriteLine($"Registration response: {response}");

            mainChannel.ShutdownAsync().Wait();
        }

        private static void GetInput()
        {
            while (true)
            {
                string selection = Console.ReadLine();
                switch (selection)
                {
                    case "1":
                        GetAllAuctions();
                        break;
                    case "2":
                        MakeBid();
                        break;
                    case "3":
                        // DoSendCoin();
                        break;

                    case "4":
                        DoExit();
                        break;
                }
            }

        }

        private static void DoExit()
        {
            throw new NotImplementedException();
        }

        private static void MakeBid()
        {
            throw new NotImplementedException();
        }

        private static void GetAllAuctions()
        {
            throw new NotImplementedException();
        }

    }
}