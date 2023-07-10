using System.Net;
using OrleansMicroservices.Common;
using OrleansMicroservices.IMessages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConsulClient(options =>
{
    options.Address = new Uri("http://localhost:8500");
});

builder.Services.AddConsulService(service =>
{
    service.ServiceName = "orders-service";
    service.ServiceNameId = $"orders-service-{Guid.NewGuid():N}";
    service.UrlSegment = "orders";
});

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.AddActivityPropagation();
    siloBuilder.AddMemoryGrainStorage("OrleansMemoryProvider-Orders");

    siloBuilder.UseConsulSiloClustering(consulOptions =>
    {
        consulOptions.ConfigureConsulClient(new Uri("http://localhost:8500"));
    });

    siloBuilder.ConfigureEndpoints(
        advertisedIP: IPAddress.Loopback,
        siloPort: NetworkScanner.GetPort(),
        listenOnAnyHostAddress: true,
        gatewayPort: 30000
    );
});


var app = builder.Build();


app.MapGet("/", () => "Orders Server Working...!");

app.MapGet("/get-customer", async (IServiceProvider serviceProvider) =>
{
    var client = serviceProvider.GetRequiredService<IClusterClient>();
    var customersGrain = client.GetGrain<ICustomersGrain>(Guid.NewGuid());
    var customer = await customersGrain.GetCustomerAsync(Guid.NewGuid());
    return Results.Ok(customer);
});

var lifetime = app.Services.GetService<IHostApplicationLifetime>();

lifetime?.ApplicationStopping.Register(() =>
{
    app.Services.LeaveConsulAsync().Wait();
}, false);

app.Run();
