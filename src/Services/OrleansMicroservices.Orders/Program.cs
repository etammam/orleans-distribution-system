using System.CommandLine;
using System.Net;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrleansMicroservices.Common;
using OrleansMicroservices.IMessages;

var builder = WebApplication.CreateBuilder(args);

var useConsul = false;

var useConsulOption = new Option<bool>(
    name: "--use-consul",
    description: "allow application to setup cluster into consul");

var urlsOptions = new Option<string[]>(
    name: "--urls", description: "application host.");

var rootCommand = new RootCommand();
rootCommand.AddOption(useConsulOption);
rootCommand.AddOption(urlsOptions);

rootCommand.SetHandler(option => useConsul = option, useConsulOption);
await rootCommand.InvokeAsync(args);

Console.WriteLine($"consul hook {useConsul}");

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        // Set a service name
        tracing.SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: "Orders", serviceVersion: "1.0"));

        tracing.AddSource("Microsoft.Orleans.Runtime");
        tracing.AddSource("Microsoft.Orleans.Application");

        tracing.AddZipkinExporter(zipkin =>
        {
            zipkin.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
        });
    });

if (useConsul)
{

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
}

const string connectionString = "Data Source=localhost\\sqlexpress;Initial Catalog=Trash-Orleans;Integrated Security=true;TrustServerCertificate=True";
const string invariant = "System.Data.SqlClient";

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.AddActivityPropagation();

    siloBuilder.AddMemoryGrainStorage("OrleansMemoryProvider-Orders");
    //siloBuilder.AddAdoNetGrainStorage("Orleans-Orders", options =>
    //{
    //    options.ConnectionString = connectionString;
    //    options.Invariant = invariant;
    //});

    if (useConsul)
    {
        siloBuilder.UseConsulSiloClustering(consulOptions =>
        {
            consulOptions.ConfigureConsulClient(new Uri("http://localhost:8500"));
        });
    }
    else
    {
        siloBuilder.UseAdoNetClustering(cluster =>
        {
            cluster.ConnectionString = connectionString;
            cluster.Invariant = invariant;
        });
    }


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

app.MapGet("/throw-an-exception", async (IServiceProvider serviceProvider) =>
{
    var client = serviceProvider.GetRequiredService<IClusterClient>();
    var customersGrain = client.GetGrain<ICustomersGrain>(Guid.NewGuid());
    try
    {
        await customersGrain.ThrowException();
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
    return Results.Ok();
});

if (useConsul)
{
    var lifetime = app.Services.GetService<IHostApplicationLifetime>();

    lifetime?.ApplicationStopping.Register(() =>
    {
        app.Services.LeaveConsulAsync().Wait();
    }, false);
}

app.Run();
