using OrleansMicroservices.IMessages;

namespace OrleansMicroservices.Customers.Grains
{
    public class CustomersGrain : Grain, ICustomersGrain
    {
        private readonly ILogger<CustomersGrain> _logger;
        private readonly IClusterClient _clusterClient;
        private readonly IGreetingGrain _greetingGrain;

        public CustomersGrain(ILogger<CustomersGrain> logger, IClusterClient clusterClient)
        {
            _logger = logger;
            _clusterClient = clusterClient;
            _greetingGrain = _clusterClient.GetGrain<IGreetingGrain>(Guid.NewGuid());
        }

        public Task<Customer> GetCustomerAsync(Guid customerId)
        {
            _logger.LogInformation("new request invoked {GrainId}", GrainReference.GrainId.Key);
            _greetingGrain.SayHelloAsync(customerId.ToString());

            return Task.FromResult(new Customer(customerId, "Islam Mostafa Tammam Abouzeid", "eslamtammam@hotmail.com"));
        }

        public Task ThrowException()
        {
            throw new Exception("a new exception has been invoked");
        }
    }
}
