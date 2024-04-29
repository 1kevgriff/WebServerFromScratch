using System.Net.Sockets;
using System.Text;
using System.IO.Compression;
using System.IO;

namespace Griffin.WebServer.Workers;

public class HttpHandler(Socket socket)
{
    public async Task<HttpRequest> GetMessageFromSocket()
    {
        byte[] buffer = new byte[32];
        StringBuilder builder = new StringBuilder(1024);

        var endOfMessageString = "\r\n\r\n";

        bool endOfMessage = false;
        do {
            var size = await socket.ReceiveAsync(buffer);
            if (size == 0) break;

            builder.Append(Encoding.UTF8.GetString(buffer, 0, size));
            // look for double CRLF
            if (builder.ToString().Contains(endOfMessageString))
            {
                endOfMessage = true;
            }

        } while (!endOfMessage);

        var request = new HttpRequest(builder.ToString());

        return request;
    }

    public async Task SendResponse(HttpResponse response, HttpRequest request)
    {
        var message = new StringBuilder();
        message.AppendLine($"HTTP/1.1 {response.StatusCode}");
        message.AppendLine($"Content-Type: {response.ContentType}");

        byte[] bodyBytes = Encoding.UTF8.GetBytes(response.Body);

        if (request.CompressionRequest)
        {
            using var memoryStream = new MemoryStream();

            Stream? compressionStream = request.CompressionType switch
            {
                "gzip" => new GZipStream(memoryStream, CompressionMode.Compress, true),
                "deflate" => new DeflateStream(memoryStream, CompressionMode.Compress, true),
                "br" => new BrotliStream(memoryStream, CompressionMode.Compress, true),
                _ => null
            };

            if (compressionStream != null)
            {
                await compressionStream.WriteAsync(bodyBytes, 0, bodyBytes.Length);
                compressionStream.Close();
                bodyBytes = memoryStream.ToArray();
                message.AppendLine($"Content-Encoding: {request.CompressionType}");
            }
        }

        message.AppendLine($"Content-Length: {bodyBytes.Length}");
        message.AppendLine();

        await socket.SendAsync(Encoding.UTF8.GetBytes(message.ToString()));
        await socket.SendAsync(bodyBytes);
    }
}