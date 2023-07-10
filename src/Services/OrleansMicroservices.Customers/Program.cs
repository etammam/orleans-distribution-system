using System.Net;
using OrleansMicroservices.Common;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddConsulClient(options =>
{
    options.Address = new Uri("http://localhost:8500");
});

builder.Services.AddConsulService(service =>
{
    service.ServiceName = "customers-service";
    service.ServiceNameId = $"customers-service-{Guid.NewGuid():N}";
    service.UrlSegment = "customers";
});
builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.AddActivityPropagation();

    siloBuilder.AddMemoryGrainStorage("OrleansMemoryProvider-Customers");

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

app.MapGet("/", () => "Customers Server Working...!");

var lifetime = app.Services.GetService<IHostApplicationLifetime>();

lifetime?.ApplicationStopping.Register(() =>
{
    app.Services.LeaveConsulAsync().Wait();
}, false);

app.Run();