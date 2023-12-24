using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2P.SeedNode
{
    public class SeedNodeServiceImpl :SeedNodeService.SeedNodeServiceBase
    {
        public override Task<EmptyMsg> RegisterNode(RegisterRequest request, ServerCallContext context)
        {
            var nodeId = request.NodeId;

            var address = request.NodeAddress;

            var message= string.Empty;

            if (RegistDict.RegisteredNodes.TryAdd(nodeId, address))
            {
                message = $"Node '{nodeId}' registered with the seed node with the address of {address} .";
                Console.WriteLine(message);
            }
            else
            {
                message = $"Node '{nodeId}' is already registered.";
                Console.WriteLine(message);
            }

            return Task.FromResult(new EmptyMsg());
        }

        public override Task<RegisteredNodesResponse> GetRegisteredNodes(EmptyMsg request, ServerCallContext context)
        {
            var nodes = RegistDict.RegisteredNodes.Select(p => new Node { NodeId = p.Key, Address = p.Value }).ToList();
            RegisteredNodesResponse response= new();
            response.Nodes.AddRange(nodes);
            response.RegisteredNodes.AddRange(RegistDict.RegisteredNodes.Keys);
            return Task.FromResult(response);
        }
    }
}
