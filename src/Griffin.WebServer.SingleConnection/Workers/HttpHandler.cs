using System.Net.Sockets;
using System.Text;

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

            builder.Append(Encoding.UTF8.GetString(buffer));
            // look for double CRLF
            if (builder.ToString().Contains(endOfMessageString))
            {
                endOfMessage = true;
            }

        } while (!endOfMessage);

        var request = new HttpRequest(builder.ToString());

        return request;
    }

    public async Task SendResponse(HttpResponse response)
    {
        var message = new StringBuilder();
        message.AppendLine($"HTTP/1.1 {response.StatusCode}");
        message.AppendLine($"Content-Type: {response.ContentType}");
        message.AppendLine($"Content-Length: {response.ContentLength}");
        message.AppendLine();
        message.AppendLine(response.Body);
        message.AppendLine();

        var buffer = Encoding.UTF8.GetBytes(message.ToString());
        await socket.SendAsync(buffer);
    }
}