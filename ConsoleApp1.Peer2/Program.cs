using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;
using System.Threading.Channels;

namespace ConsoleApp1.Peer2
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
            NodePort = 50052;
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
            Console.WriteLine("                    2. Make a Bid for an auction");
            Console.WriteLine("                    3. Create Auction");
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
                        CreateAuction();
                        break;

                    case "4":
                        MyAuctions();
                        break;

                    case "5":
                        AcceptTheBid();
                        break;

                    case "6":
                        DoExit();
                        break;
                }
            }

        }

        private static void AcceptTheBid()
        {
            Console.WriteLine("type Auction Id to be selected");
            
            var auctionId = Console.ReadLine();

            Console.WriteLine("Type 'Y' to accept or 'N' to decline it");

            var acceptOrNot = Console.ReadLine();

            switch (acceptOrNot)
            {
                case "Y":
                    AcceptTheBidbyId(auctionId);

                    break;
                case "N":
                    //Decline();
                    break;
            }

        }

        private static void Decline()
        {
            throw new NotImplementedException();
        }

        private static void AcceptTheBidbyId(string? auctionId)
        {
            throw new NotImplementedException();
        }

        private static void MyAuctions()
        {
            var getMyAuctions = auctionCache.GetAuctions().Where(p=>p.OwnerNodeId==NodeId).ToList();

            foreach (var auction in getMyAuctions)
            {
                Console.WriteLine($"AuctionId: {auction.AuctionId} Item Name: {auction.AuctionRequest.ItemName}, with a current bid is{auction.AuctionRequest.StartingPrice}");
            }        
            return;
        }

        private static void CreateAuction()
        {
            Console.WriteLine("Write Item Name");
            var ItemName = Console.ReadLine();

            Console.WriteLine("Write Starting Price");
            var startingPrice = Console.ReadLine();

            var request = new BroadcastMessage
            {
                Text =  "Add",
                AuctionId = Guid.NewGuid().ToString(),
                OwnerId = NodeId,
                Address = $"http://{NodeHost}:{NodePort}",
                ItemName = ItemName,
                StartingPrice = Convert.ToDouble(startingPrice),
                Bidder = string.Empty
            };
            _ = broadcastClient?.Broadcast(request);
            Console.WriteLine("Auction is sent the the network!");
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
                            var auctionResponse = new AuctionResponse
                            {
                                AuctionId = message.AuctionId,
                                OwnerNodeId = message.OwnerId,
                                Address = message.Address,
                                AuctionRequest = new InitiateAuctionRequest
                                {
                                    StartingPrice = message.StartingPrice,
                                    ItemName = message.ItemName
                                },
                                Bidder= message.Bidder
                            };
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
            var getAllAuctions = auctionCache.GetAuctions();

            foreach (var auction in getAllAuctions)
            {
                Console.WriteLine($"AuctionId: {auction.AuctionId} Item Name: {auction.AuctionRequest.ItemName}, with a starting price of {auction.AuctionRequest.StartingPrice}");
            }

            return;
        }
    }
}