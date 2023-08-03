using OrleansMicroservices.IMessages;

namespace OrleansMicroservices.Customers.Grains
{
    public class CustomersGrain : Grain, ICustomersGrain
    {
        private readonly ILogger<CustomersGrain> _logger;

        public CustomersGrain(ILogger<CustomersGrain> logger)
        {
            _logger = logger;
        }

        public Task<Customer> GetCustomerAsync(Guid customerId)
        {
            _logger.LogInformation("new request invoked {GrainId}", GrainReference.GrainId.Key);
            return Task.FromResult(new Customer(customerId, "Islam Mostafa Tammam Abouzeid", "eslamtammam@hotmail.com"));
        }

        public Task ThrowException()
        {
            throw new Exception("a new exception has been invoked");
        }
    }
}
