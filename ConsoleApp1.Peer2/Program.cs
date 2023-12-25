using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Shared;
using Shared.Services;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ConsoleApp1.Peer2
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
            NodePort = 50052;
        }

        static async Task Main(string[] args)
        {
            RunPeer();
        }
    }

}