using OrleansMicroservices.IMessages;

namespace OrleansMicroservices.Customers.Grains
{
    public class CustomersGrain : Grain, ICustomersGrain
    {
        public Task<Customer> GetCustomerAsync(Guid customerId)
        {
            return Task.FromResult(new Customer(customerId, "Islam Mostafa Tammam", "eslamtammam@hotmail.com"));
        }
    }
}
