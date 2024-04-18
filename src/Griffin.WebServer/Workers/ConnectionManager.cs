using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ConnectionManagerWorker(ILogger<ConnectionManagerWorker> logger) : IHostedService
{

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Implement startup logic here
        // For example, initializing connection listeners
        Console.WriteLine("Connection Manager starting.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Implement shutdown logic here
        // For example, closing connections or releasing resources
        Console.WriteLine("Connection Manager stopping.");
    }

}