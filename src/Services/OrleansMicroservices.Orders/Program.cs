using OrleansMicroservices.IMessages;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseOrleansClient(options =>
{
    options.UseLocalhostClustering(
        serviceId: "customers-services",
        clusterId: "customers-cluster"
    );
});
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/send-greet", async (IServiceProvider serviceProvider) =>
{
    var client = serviceProvider.GetRequiredService<IClusterClient>();
    var greetingGrain = client.GetGrain<IGreetingGrain>(Guid.NewGuid());
    await greetingGrain.SayHelloAsync();
    await greetingGrain.SayHelloAsync("Islam");
    return Results.Ok();
});

app.Run();
