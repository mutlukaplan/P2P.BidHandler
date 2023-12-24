using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services
{
    public class ChatServiceImpl : ChatService.ChatServiceBase
    {
        public override Task<Message> SendMessage(Message request, ServerCallContext context)
        {
            Console.WriteLine($"Received message: {request.Text}");
            return Task.FromResult(new Message { Text = "Server received your message." });
        }
    }
}
