using Griffin.WebServer.Workers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder();
builder.ConfigureAppConfiguration((hostingContext, config) =>
{
    var env = hostingContext.HostingEnvironment;
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
    config.AddEnvironmentVariables();
});

builder.ConfigureLogging(logging =>
{
    logging.AddSimpleConsole();
});

builder.ConfigureServices(services =>
{
    services.AddHostedService<ConnectionManagerWorker>();
});

var host = builder.Build();
await host.RunAsync();

