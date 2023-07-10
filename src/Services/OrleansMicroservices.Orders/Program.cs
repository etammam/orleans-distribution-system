using System.Net;
using OrleansMicroservices.Common;
using OrleansMicroservices.IMessages;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.AddActivityPropagation();
    siloBuilder.AddMemoryGrainStorage("OrleansMemoryProvider-Orders");


    siloBuilder.UseConsulSiloClustering(consulOptions =>
    {
        consulOptions.ConfigureConsulClient(new Uri("http://localhost:8500"));
    });
    siloBuilder.ConfigureEndpoints(IPAddress.Loopback, siloPort: NetworkScanner.GetPort(), listenOnAnyHostAddress: true, gatewayPort: 30000);
});


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/get-customer", async (IServiceProvider serviceProvider) =>
{
    var client = serviceProvider.GetRequiredService<IClusterClient>();
    var customersGrain = client.GetGrain<ICustomersGrain>(Guid.NewGuid());
    var customer = await customersGrain.GetCustomerAsync(Guid.NewGuid());
    return Results.Ok(customer);
});

app.Run();
