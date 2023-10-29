using OrleansMicroservices.IMessages;

namespace OrleansMicroservices.Customers;

public class GreetingGrain : Grain, IGreetingGrain
{
    private readonly ILogger<GreetingGrain> _logger;

    public GreetingGrain(ILogger<GreetingGrain> logger)
    {
        _logger = logger;
    }

    public Task SayHelloAsync(string name)
    {
        _logger.LogInformation(name);
        return Task.CompletedTask;
    }

    public Task SayHelloAsync()
    {
        throw new NotImplementedException();
    }
}
