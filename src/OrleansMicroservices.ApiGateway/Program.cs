using OrleansMicroservices.ApiGateway;
using OrleansMicroservices.Common;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ConsulMonitor>();
builder.Services.AddSingleton<IProxyConfigProvider>(p => p.GetService<ConsulMonitor>()!);
builder.Services.AddReverseProxy();
builder.Services.AddConsulClient(options =>
{
    options.Address = new Uri("http://localhost:8500");
});

builder.Services.AddHostedService(p => p.GetService<ConsulMonitor>());

var app = builder.Build();
app.MapReverseProxy();
app.UseRouting();

app.MapGet("/", () => "Api Gateway");
app.MapGet("/_configurations", (IProxyConfigProvider proxyConfiguration) =>
{
    var configuration = proxyConfiguration.GetConfig();
    return Results.Ok(new
    {
        Routes = configuration.Routes,
        Clusters = configuration.Clusters,
    });
});
app.Run();
