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

        // WARNING: THIS IS NOT EFFICIENT AT ALL, DON'T @ ME

        bool endOfMessage = false;
        do {
            // clear buffer!
            Array.Clear(buffer, 0, buffer.Length);

            var size = await socket.ReceiveAsync(buffer);
            if (size == 0) break;

            builder.Append(Encoding.UTF8.GetString(buffer));

            // look for double CRLF
            var wip = builder.ToString();


            // do we have a Content-Length?
            if (wip.Contains("Content-Length:"))
            {
                var contentLength = int.Parse(wip.Split("Content-Length: ")[1].Split("\r\n")[0]);
                var body = wip.Split(endOfMessageString)[1];
                body = body.Trim('\0');
                if (body.Length == contentLength)
                {
                    endOfMessage = true;
                }
            } else if (wip.EndsWith(endOfMessageString))  // normal EOM check
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
        message.Append(response.Body);
        message.AppendLine();

        var buffer = Encoding.UTF8.GetBytes(message.ToString());
        int totalSent = 0;
        int length = buffer.Length;

        while (totalSent < length)
        {
            var sent = await socket.SendAsync(new ArraySegment<byte>(buffer, totalSent, length - totalSent), SocketFlags.None);
            if (sent == 0)
            {
                throw new SocketException();  // Consider a more specific exception or error handling strategy
            }
            totalSent += sent;
        }
    }
}