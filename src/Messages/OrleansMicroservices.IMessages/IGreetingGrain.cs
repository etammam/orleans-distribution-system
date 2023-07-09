using Orleans;

namespace OrleansMicroservices.IMessages
{
    public interface IGreetingGrain : IGrainWithGuidKey
    {
        Task SayHelloAsync(string name);
    }
}
