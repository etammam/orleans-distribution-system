namespace OrleansMicroservices.IMessages
{
    public interface ICustomersGrain : IGrainWithGuidKey
    {
        Task<Customer> GetCustomerAsync(Guid customerId);
    }

    [GenerateSerializer]
    public record Customer(Guid Id, string Name, string Email);
}
