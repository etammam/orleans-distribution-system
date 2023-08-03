namespace OrleansMicroservices.IMessages
{
    public interface ICustomersGrain : IGrainWithGuidKey
    {
        Task<Customer> GetCustomerAsync(Guid customerId);
        Task ThrowException();
    }

    [GenerateSerializer]
    public record Customer(Guid Id, string Name, string Email);
}
