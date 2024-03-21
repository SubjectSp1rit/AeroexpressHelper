using Serilog;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text;

namespace CSVnJSONAnalyzer
{
    public class JSONProcessing
    {
        public static Stream WriteAsync(List<Aeroexpress> aeroexpresses)
        {
            var memoryStream = new MemoryStream();
            JsonSerializer.SerializeAsync(memoryStream, aeroexpresses);

            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        public static List<Aeroexpress> ReadAsync(Stream jsonStream)
        {
            if (jsonStream.CanSeek)
            {
                jsonStream.Seek(0, SeekOrigin.Begin);
            }

            return JsonSerializer.Deserialize<List<Aeroexpress>>(jsonStream) ?? new List<Aeroexpress>();
        }
    }
}
