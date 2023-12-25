using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;

namespace ConsoleApp2
{
    class Program
    {
        public static GrpcChannel? mainChannel;
        public static SeedNodeService.SeedNodeServiceClient? seedClient;
        public static BroadcastService.BroadcastServiceClient? broadcastClient;
        const int MainChannelPort = 50055;
        public static IAuctionCache auctionCache;
        public static string NodeId;
        public static string NodeHost;
        public static int NodePort;

        static Program()
        {
            mainChannel = GrpcChannel.ForAddress($"http://localhost:{MainChannelPort}");
            seedClient = new SeedNodeService.SeedNodeServiceClient(mainChannel);
            broadcastClient = new BroadcastService.BroadcastServiceClient(mainChannel);
            NodeId = Guid.NewGuid().ToString();
            auctionCache = new AuctionCache();
            NodeHost = "localhost";
            NodePort = 50051;
        }

        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                    .AddSingleton<IAuctionCache, AuctionCache>() // Register your local service implementation.
                    .BuildServiceProvider();

            var serviceCache = serviceProvider.GetRequiredService<IAuctionCache>();

            var server = new Server
            {
                Services = { AuctionService.BindService(new AutionServiceImpl(serviceCache)) },
                Ports = { new ServerPort(NodeHost, NodePort, ServerCredentials.Insecure) }
            };


            RegisterClientAndStart(NodePort, NodeId, server, NodeHost);

            StartListeningBroadcastMessages();

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
            Console.WriteLine("                    4. Send Message");
            Console.WriteLine("                    5. Exit");
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

            var registerRequest = new RegisterRequest { NodeId = nodeId, NodeAddress = nodeAddress };

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
                        //GetBroadcastMessages();
                        break;

                    case "4":
                        SendAuctionToTheChannel();
                        break;

                    case "5":
                        DoExit();
                        break;
                }
            }

        }

        private static void SendAuctionToTheChannel()
        {

            Console.WriteLine("write your message");
            var message = Console.ReadLine();

            var request = new BroadcastMessage { Text = message, AuctionId = Guid.NewGuid().ToString(), OwnerId = NodeId };
            var response = broadcastClient?.Broadcast(request);
            Console.WriteLine("Message sent to all nodes.");
        }

        private static void StartListeningBroadcastMessages()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var call = broadcastClient?.ReceiveBroadcast(new BroadcastEmpty());

            var messageHandlerTask = Task.Run(async () =>
            {
                try
                {
                    await foreach (var message in call.ResponseStream.ReadAllAsync(cancellationTokenSource.Token))
                    {
                        if (!string.IsNullOrEmpty(message.Text) && !string.IsNullOrEmpty(message.AuctionId) && !string.IsNullOrEmpty(message.OwnerId))
                        {
                            Console.WriteLine($"Received broadcast: {message.Text}");
                            var auctionResponse = new AuctionResponse { AuctionId = message.AuctionId, OwnerNodeId = message.OwnerId, Address = message.Address };
                            auctionCache.AddAuction(auctionResponse);
                        }
                        
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    Console.WriteLine("Broadcast subscription canceled.");
                }
            });

            return;
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

            var getallNodes = seedClient.GetRegisteredNodes(new EmptyMsg());



            throw new NotImplementedException();
        }
    }

}
