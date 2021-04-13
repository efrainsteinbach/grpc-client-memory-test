using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Greet;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Server.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger _logger;

        public GreeterService(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<GreeterService>();
        }

        public override Task<NumberReply> GetNumbers(NumberRequest request, ServerCallContext context)
        {
            this._logger.LogInformation($"Generating massive payload..");
            var now = DateTime.UtcNow;
            var random = new Random();
            var points = Enumerable.Range(0, 5_000_000).Select(i => new DoubleDataPoint()
            {
                Date = Timestamp.FromDateTime(now.AddHours(-i)),
                Value = random.NextDouble(),
            });
            var reply = new NumberReply();
            reply.Points.AddRange(points);

            this._logger.LogInformation($"Returning massive payload {request}..");
            return Task.FromResult(reply);
        }
    }
}
