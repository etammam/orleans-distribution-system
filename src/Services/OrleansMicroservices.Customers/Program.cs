using System.Net;
using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(options =>
{
    options.UseLocalhostClustering(
        serviceId: "customers-services",
        clusterId: "customers-cluster"
    );
    options.AddMemoryGrainStorage("OrleansMemoryProvider");
    options.Configure<EndpointOptions>(endpointOptions => endpointOptions.AdvertisedIPAddress = IPAddress.Loopback);
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.Run();
