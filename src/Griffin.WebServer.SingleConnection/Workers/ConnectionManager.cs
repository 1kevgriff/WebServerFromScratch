using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Griffin.WebServer.Workers;

public class ConnectionManagerWorker(IConfiguration configuration, ILogger<ConnectionManagerWorker> logger) : BackgroundService
{
    private int _port = 4321;
    private string _clientPath = configuration["ClientPath"] ?? throw new ArgumentNullException($"ClientPath");
    private const int MaxConnections = 10;
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var localEndpoint = new IPEndPoint(IPAddress.Any, _port);
        using var listenSocket = new Socket(localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listenSocket.Bind(localEndpoint);
        listenSocket.Listen(MaxConnections);

        logger.LogInformation("[INFO] Listening at {addr}", listenSocket.LocalEndPoint);

        while (!cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("[WAITING]");
            var clientSocket = await listenSocket.AcceptAsync(cancellationToken);

            logger.LogInformation("[SOCKET] New connection from {ipAddress}", clientSocket.LocalEndPoint);

            var handler = new HttpHandler(clientSocket);

            try
            {
                var message = await handler.GetMessageFromSocket();
                logger.LogInformation("[CLIENT]:{message}", message);

                var response = HttpResponse.Generate(message, _clientPath);
                await handler.SendResponse(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[ERROR] Failed to read message from socket");
            }
            finally
            {
                clientSocket.Shutdown(SocketShutdown.Both);
            }
        }
    }
}