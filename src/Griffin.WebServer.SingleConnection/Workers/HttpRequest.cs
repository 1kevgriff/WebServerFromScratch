using System.Text;

namespace Griffin.WebServer.Workers;

public class HttpRequest
{
    public HttpRequest(string toString)
    {
        if (string.IsNullOrWhiteSpace(toString))
        {
            throw new Exception("Bad Request");
        }

        // parse the request
        var lines = toString.Split(Environment.NewLine);
        var requestLine = lines[0].Split(" ");

        // todo: handle error if request line doesn't have three octets

        Method = requestLine[0];
        Path = requestLine[1];
        Protocol = requestLine[2];

        // parse headers
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                Body = string.Join(Environment.NewLine, lines[(i + 1)..]);
                break;
            }

            var header = lines[i].Split(": ");
            Headers.Add(header[0], header[1]);
        }

        // parse body
        if (string.IsNullOrWhiteSpace(Body))
        {
            Body = string.Join(Environment.NewLine, lines[^1..]);
        }
    }

    public string Method { get; set; }
    public string Path { get; set; }
    public string Protocol { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Method} {Path} {Protocol}");
        foreach (var header in Headers)
        {
            sb.AppendLine($"{header.Key}: {header.Value}");
        }
        return sb.ToString();
    }
}