namespace Monoboy.Dap;

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

public static class DapIO
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static bool TryRead(Stream stream, out JsonObject message)
    {
        message = null!;
        int? length = null;
        while (true)
        {
            string? line = ReadHeaderLine(stream);
            if (line == null)
            {
                return false;
            }

            if (line.Length == 0)
            {
                break;
            }

            const string prefix = "Content-Length:";
            if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && int.TryParse(line[prefix.Length..].Trim(), out int parsed)
                && parsed >= 0)
            {
                length = parsed;
            }
        }

        if (length == null)
        {
            return false;
        }

        byte[] body = new byte[length.Value];
        int read = 0;
        while (read < body.Length)
        {
            int n = stream.Read(body, read, body.Length - read);
            if (n == 0)
            {
                return false;
            }

            read += n;
        }

        if (JsonNode.Parse(body) is not JsonObject obj)
        {
            return false;
        }

        message = obj;
        return true;
    }

    public static void Write(Stream stream, JsonObject message)
    {
        byte[] body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
        byte[] header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
        stream.Write(header);
        stream.Write(body);
        stream.Flush();
    }

    static string? ReadHeaderLine(Stream stream)
    {
        var bytes = new List<byte>();
        int value;
        while ((value = stream.ReadByte()) >= 0)
        {
            if (value == '\n')
            {
                break;
            }

            if (value != '\r')
            {
                bytes.Add((byte)value);
            }
        }

        if (value < 0 && bytes.Count == 0)
        {
            return null;
        }

        return Encoding.ASCII.GetString(bytes.ToArray());
    }
}
