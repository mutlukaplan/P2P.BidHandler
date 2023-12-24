using Grpc.Core;
using Grpc.Net.Client;
using Shared.Services;

namespace ConsoleApp1.Peer2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const int Port = 50052;

            var server = new Server
            {
                Services = { ChatService.BindService(new ChatServiceImpl()),AuctionService.BindService(new AutionServiceImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine($"Server listening on port {Port}");

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
    }
}