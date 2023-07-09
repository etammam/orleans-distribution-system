using Microsoft.Extensions.Logging;
using Orleans;
using OrleansMicroservices.IMessages;

namespace OrleansMicroservices.Messages.Grains
{
    public class GreetingGrain : Grain, IGreetingGrain
    {
        private readonly ILogger<GreetingGrain> _logger;

        public GreetingGrain(ILogger<GreetingGrain> logger)
        {
            _logger = logger;
        }

        public async Task SayHelloAsync(string name)
        {
            _logger.LogInformation("Hello {name}", name);
            await Task.CompletedTask;
        }
    }
}
