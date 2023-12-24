using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services
{
    public interface IBroadCastService
    {
        Task<Empty> BroadcastMessage(Message request, ServerCallContext context);
    }
}
