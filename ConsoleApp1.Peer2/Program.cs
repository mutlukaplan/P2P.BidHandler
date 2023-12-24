using Grpc.Core;
using Grpc.Net.Client;
using Shared.Services;
using System.Threading.Channels;

namespace ConsoleApp1.Peer2
{
    class Program
    {

        static async Task Main(string[] args)
        {
            const int Port = 50052;
            const int MainChannelPort = 50055;
            string nodeId = Guid.NewGuid().ToString();

            var server = new Server
            {
                Services = { ChatService.BindService(new ChatServiceImpl()), AuctionService.BindService(new AutionServiceImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };

            RegisterClientAndStart(Port, MainChannelPort, nodeId, server);

            var channel = GrpcChannel.ForAddress($"http://localhost:{50052}");//  new Channel("localhost", Port, ChannelCredentials.Insecure);
            var client = new ChatService.ChatServiceClient(channel);

            Console.WriteLine("Enter your name:");
            var name = Console.ReadLine();

            Console.WriteLine("Type 'exit' to quit the chat.");

            var chatTask = Task.Run(async () =>
            {
                while (true)
                {
                    var message = Console.ReadLine();

                    if (message.ToLower() == "exit")
                        break;

                    await client.SendMessageAsync(new Message { Text = $"{name}: {message}" });
                }

                channel.ShutdownAsync().Wait();
            });

            await chatTask;
            server.ShutdownAsync().Wait();
        }

        private static void RegisterClientAndStart(int Port, int MainChannelPort, string nodeId, Server server)
        {
            server.Start();

            Console.WriteLine($"Server listening on port {Port}");

            var mainChannel = GrpcChannel.ForAddress($"http://localhost:{MainChannelPort}");

            var seedClient = new SeedNodeService.SeedNodeServiceClient(mainChannel);

            var registerRequest = new RegisterRequest { NodeId = nodeId };

            var response = seedClient.RegisterNode(registerRequest);

            Console.WriteLine($"Registration response: {response}");

            var response2 = seedClient.RegisterNode(registerRequest);

            mainChannel.ShutdownAsync().Wait();
        }
    }
}