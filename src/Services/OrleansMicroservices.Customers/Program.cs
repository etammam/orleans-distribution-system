using System.CommandLine;
using System.Net;
using OrleansMicroservices.Common;

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

if (useConsul)
{
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
}

const string connectionString = "Data Source=localhost\\sqlexpress;Initial Catalog=Trash-Orleans;Integrated Security=true;TrustServerCertificate=True";
const string invariant = "System.Data.SqlClient";



builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.AddActivityPropagation();

    siloBuilder.AddMemoryGrainStorage("OrleansMemoryProvider-Customers");
    //siloBuilder.AddAdoNetGrainStorage("Orleans-Customers", options =>
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

    siloBuilder.UseInMemoryReminderService();
});

var app = builder.Build();

app.MapGet("/", () => "Customers Server Working...!");

if (useConsul)
{
    var lifetime = app.Services.GetService<IHostApplicationLifetime>();

    lifetime?.ApplicationStopping.Register(() =>
    {
        app.Services.LeaveConsulAsync().Wait();
    }, false);
}

app.Run();