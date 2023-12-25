using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;
using System.Collections.Concurrent;

namespace ConsoleApp2
{
    class Program
    {
        public static GrpcChannel? mainChannel;
        public static SeedNodeService.SeedNodeServiceClient? seedClient;
        public static BroadcastService.BroadcastServiceClient? broadcastClient;
        public static AuctionService.AuctionServiceClient? auctionClient;
        //static readonly Dictionary<string, AuctionResponse> cacheDict;
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
            //cacheDict= new Dictionary<string, AuctionResponse>();
            //auctionCache = new AuctionCache(cacheDict);
            NodeHost = "localhost";
            NodePort = 50051;
        }

        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                    .AddSingleton<IAuctionCache, AuctionCache>()
                    .AddSingleton<Dictionary<string, AuctionResponse>>()// Register your local service implementation.
                    .BuildServiceProvider();


            var serviceCache = serviceProvider.GetRequiredService<IAuctionCache>();

            auctionCache = serviceCache;

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
            Console.WriteLine("                    4. My Auctions");
            Console.WriteLine("                    5. Accept the bid");
            Console.WriteLine("                    6. Exit");
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


        /// <summary>
        /// check if bid is your or not. Dont allow your node to accept the auction which it doesnt own.
        /// </summary>
        /// <param name="auctionId"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void AcceptTheBidbyId(string? auctionId)
        {

            var auction = auctionCache.GetAuctions().Where(a => a.AuctionId == auctionId).FirstOrDefault();


            if (auction.OwnerNodeId != NodeId)
            {
                Console.WriteLine("You can not accpet the bid for an auction you dont own!");
                return;
            }

            var auctionAddress = auction.Address;

            var channel = GrpcChannel.ForAddress(auctionAddress);
            auctionClient = new AuctionService.AuctionServiceClient(channel);
            auctionClient.FinalizeAuction(auction);

            Console.WriteLine("Bid is accepted!");
        }

        private static void MyAuctions()
        {
            var getMyAuctions = auctionCache.GetAuctions().Where(p => p.OwnerNodeId == NodeId).ToList();

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

            var auctionId = Guid.NewGuid().ToString();

            var request = new BroadcastMessage
            {
                Text = "add",
                AuctionId = auctionId,
                OwnerId = NodeId,
                Address = $"http://{NodeHost}:{NodePort}",
                ItemName = ItemName,
                StartingPrice = Convert.ToDouble(startingPrice),
                Bidder = string.Empty
            };
            _ = broadcastClient?.Broadcast(request);
            Console.WriteLine($"Auction is sent the the network! {auctionId} is created");
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

                            switch (message.Text)
                            {
                                case "add":
                                    Console.WriteLine($"New Auction added to the channel: Item Name:  {message.ItemName}, current price:{message.StartingPrice} ");
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
                                        Bidder = message.Bidder
                                    };
                                    auctionCache.AddAuction(auctionResponse);
                                    break;
                                case "update":

                                    Console.WriteLine($"Auction is  updated on the channel: Item Name:  {message.ItemName}, new bid:{message.StartingPrice} ");
                                    var auctionResponse2 = new AuctionResponse
                                    {
                                        AuctionId = message.AuctionId,
                                        OwnerNodeId = message.OwnerId,
                                        Address = message.Address,
                                        AuctionRequest = new InitiateAuctionRequest
                                        {
                                            StartingPrice = message.StartingPrice,
                                            ItemName = message.ItemName
                                        },
                                        Bidder = message.Bidder
                                    };
                                    auctionCache.UpdateAuction(auctionResponse2);

                                    break;

                                case "delete":
                                    // inform the channel about bid is ended and send messages about who won the auction

                                    Console.WriteLine($"Auction is  ended on the channel: Item Name:  {message.ItemName}, new bid:{message.StartingPrice} ");
                                    Console.WriteLine($"Winning bidder Id is {message.Bidder}, bidder address:  {message.Address}");
                                    var auctionResponse3 = new AuctionResponse
                                    {
                                        AuctionId = message.AuctionId,
                                        OwnerNodeId = message.OwnerId,
                                        Address = message.Address,
                                        AuctionRequest = new InitiateAuctionRequest
                                        {
                                            StartingPrice = message.StartingPrice,
                                            ItemName = message.ItemName
                                        },
                                        Bidder = message.Bidder
                                    };
                                    auctionCache.DeleteAuction(auctionResponse3);
                                    break;
                            }

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
            Environment.Exit(0);
        }

        /// <summary>
        /// check if its yours, dont allow to make a bid
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private static void MakeBid()
        {
            Console.WriteLine("Pls enter auctionId to be selected");

            var auctionSelection= Console.ReadLine();   

            Console.WriteLine("pls enter your bid amount");

            var bidAmount = Console.ReadLine();

            var auction = auctionCache.GetAuctions().Where(a => a.AuctionId == auctionSelection).FirstOrDefault();

            if (auction.OwnerNodeId == NodeId)
            {
                Console.WriteLine("you can not make a bid for your own auction!");
                return;
            }

            var auctionAddress = auction.Address;

            var channel = GrpcChannel.ForAddress(auctionAddress);
            auctionClient = new AuctionService.AuctionServiceClient(channel);

            var bidReq = new PlaceBidRequest { AuctionId = auction.AuctionId, BidAmount = Convert.ToDouble(bidAmount), Bidder = NodeId };

            var response = auctionClient.PlaceBid(bidReq);

            if (response.Accepted)
                Console.WriteLine("Congrats, your bid is currently the highest one!.Pls don't forget, owner of the auction still needs to end the auction");
            else
                Console.WriteLine("your bid is below the current bid amount");
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

    internal  class AuctionDict
    {
        public static readonly Dictionary<string, AuctionResponse> AuctionCache = new Dictionary<string, AuctionResponse>();
    }

}
