using Serilog;
using System.Text.Json;
namespace CSVnJSONAnalyzer
{
    /// <summary>
    /// Работает с файлами типа .json
    /// </summary>
    public class JSONProcessing
    {
        /// <summary>
        /// принимает на вход коллекцию объектов типа Aeroexpress и возвращает
        /// объект типа Stream, который будет использован для отправки json файла Telegram-
        /// ботом
        /// </summary>
        /// <param name="aeroexpresses"></param>
        /// <returns></returns>
        public static Stream Write(List<Aeroexpress> aeroexpresses)
        {
            try
            {
                var memoryStream = new MemoryStream();
                JsonSerializer.SerializeAsync(memoryStream, aeroexpresses);

                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка открытия потока для записи в JSON: {ex.Message}");
                return MemoryStream.Null;
            }
        }

        /// <summary>
        /// принимает на вход Stream с json файлом из Telegram-бота и
        // возвращает коллекцию объектов типа MyType
        /// </summary>
        /// <param name="jsonStream"></param>
        /// <returns></returns>
        public static List<Aeroexpress> Read(Stream jsonStream)
        {
            try
            {
                if (jsonStream.CanSeek)
                {
                    jsonStream.Seek(0, SeekOrigin.Begin);
                }

                return JsonSerializer.Deserialize<List<Aeroexpress>>(jsonStream) ?? new List<Aeroexpress>();
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка чтения JSON файла: {ex.Message}");
                return new List<Aeroexpress>();
            }
        }
    }
}
