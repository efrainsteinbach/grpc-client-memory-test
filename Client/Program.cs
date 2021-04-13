#region Copyright notice and license

// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Threading.Tasks;
using Greet;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace Client
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            await RunThingsInScope();

            GC.Collect(1000, GCCollectionMode.Forced, blocking:true);
            Console.WriteLine($"-------------------------------------------------------------------");
            WriteMemorySummary();
            
            Console.WriteLine("");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task RunThingsInScope()
        {
            for (int i = 0; i < 5; i++)
            {
                using var channel = GrpcChannel.ForAddress(
                    "https://localhost:5001",
                    new GrpcChannelOptions()
                    {
                        MaxReceiveMessageSize = null,
                        LoggerFactory = GetConsoleLoggerFactory,
                    });
                var client = new Greeter.GreeterClient(channel);
                Console.WriteLine($"Requesting some numbers (Request #{i})..");
                WriteMemorySummary();

                {
                    var reply = await client.GetNumbersAsync(new NumberRequest());
                    var size = reply.CalculateSize();
                    Console.WriteLine($"Got response with {reply.Points.Count} data points. Roughly {size / 1e6} MB");
                    WriteMemorySummary();
                }

                Console.WriteLine($"Calling GC.Collect()");
                GC.Collect();
                WriteMemorySummary();
                Console.WriteLine("");
            }
        }

        private static void WriteMemorySummary()
        {
            var memoryInfo = GC.GetGCMemoryInfo();
            Console.WriteLine($"Current memory footprint: {memoryInfo.TotalCommittedBytes/1e6:F2}MB");
        }

        private static ILoggerFactory GetConsoleLoggerFactory => LoggerFactory.Create(
            builder =>
                builder.AddSimpleConsole(
                    options =>
                    {
                        options.SingleLine = true;
                        options.TimestampFormat = "hh:mm:ss ";
                    }));
    }
}
