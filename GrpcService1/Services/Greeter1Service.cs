using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcService1.Services
{
    public class Greeter1Service : Greeter1.Greeter1Base
    {
        private readonly ILogger<GreeterService> _logger;
        public Greeter1Service(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Fuck you, " + request.Name
            });
        }
    }
}
