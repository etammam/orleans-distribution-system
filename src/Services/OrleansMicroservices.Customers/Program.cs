using System.Net;
using OrleansMicroservices.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans((context, options) =>
{
    options.AddActivityPropagation();

    options.AddMemoryGrainStorage("OrleansMemoryProvider-Customers");

    options.UseConsulSiloClustering(consulOptions =>
    {
        consulOptions.ConfigureConsulClient(new Uri("http://localhost:8500"));
    });
    options.ConfigureEndpoints(IPAddress.Loopback, siloPort: NetworkScanner.GetPort(), listenOnAnyHostAddress: true, gatewayPort: 30000);
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.Run();