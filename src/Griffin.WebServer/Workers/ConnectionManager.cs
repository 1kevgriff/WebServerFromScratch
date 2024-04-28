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
                logger.LogInformation("[CLIENT]:\n{message}", message);

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

public class HttpResponse
{
    public int StatusCode { get; set; }
    public string Body { get; set; }
    public string ContentType { get; set; }
    public int ContentLength =>  string.IsNullOrWhiteSpace(Body) ? 0 : Body.Length + 1; // +1 for newline

    // Utc Now in format: Mon, 27 Jul 2009 12:28:53 GMT
    public string ResponseDate => $"{DateTimeOffset.UtcNow:R}";

    public HttpRequest Request { get; set; }

    public static HttpResponse Generate(HttpRequest httpRequest, string clientPath)
    {
        // if GET, convert request path to clientPath + request path
       // if POST, ignore

       return httpRequest.Method switch
       {
           "GET" => Get(httpRequest, clientPath),
           "POST" => Post(httpRequest, clientPath),
           _ => MethodNotSupported(httpRequest)
       };
    }

    private static HttpResponse MethodNotSupported(HttpRequest httpRequest)
    {
        var response = new HttpResponse()
        {
            StatusCode = 405,
            Request = httpRequest
        };

        return response;
    }

    private static HttpResponse Get(HttpRequest httpRequest, string clientPath)
    {
        // if Path is /, we want to get index.html by default
        if (httpRequest.Path == "/")
        {
            httpRequest.Path = "/index.html";
        }

        if (httpRequest.Path == "/hello-world")
        {
            return new HttpResponse()
            {
                Request = httpRequest,
                Body = "<h1>Hello, World</h1>",
                ContentType = "text/html",
                StatusCode = 200
            };
        }

        // clientPath + httpRequest path
        var path = Path.Combine(clientPath, httpRequest.Path.TrimStart('/'));

        // does it exist?
        if (!File.Exists(path))
        {
            return NotFound(httpRequest);
        }

        // read the file
        var body = File.ReadAllText(path);
        var extension = Path.GetExtension(path);
        var contentType = extension switch
        {
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "text/javascript",
            _ => "text/plain"
        };

        var response = new HttpResponse()
        {
            StatusCode = 405,
            Request = httpRequest,
            Body = body,
            ContentType = contentType
        };

        return response;
    }

    private static HttpResponse NotFound(HttpRequest httpRequest)
    {
        var response = new HttpResponse()
        {
            StatusCode = 404,
            Request = httpRequest
        };
        return response;
    }

    private static HttpResponse Post(HttpRequest httpRequest, string clientPath)
    {
        throw new NotImplementedException();
    }
}