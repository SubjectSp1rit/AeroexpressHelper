using Serilog;
using static CSVnJSONAnalyzer.FormatChecking;
using static CSVnJSONAnalyzer.Aeroexpress;
using System.Data;
using System.Text;
using Telegram.Bot.Types;
using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace CSVnJSONAnalyzer
{
    /// <summary>
    /// Работает с файлами типа .csv
    /// </summary>
    public class CSVProcessing
    {
        public bool Read(Stream csvStream, out List<Aeroexpress> aeroexpressList)
        {
            aeroexpressList = new List<Aeroexpress>();
            try
            {
                var lines = new List<string>();
                using (var reader = new StreamReader(csvStream, Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        lines.Add(line);
                    }
                }

                foreach (var row in lines.Skip(1))
                {
                    var parts = row.Split(new char[] { ';', ',' }).Select(line => line.Trim('"')).ToArray();

                    int id = int.Parse(parts[0]);
                    string? stationStart = parts[1];
                    string? line = parts[2];
                    string? timeStart = parts[3];
                    string? stationEnd = parts[4];
                    string? timeEnd = parts[5];
                    long globalId = long.Parse(parts[6]);

                    aeroexpressList.Add(new Aeroexpress(id,
                                                        stationStart,
                                                        line,
                                                        timeStart,
                                                        stationEnd,
                                                        timeEnd,
                                                        globalId));
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при создании списка Aeroexpress: {ex.ToString()}");
                return false;
            }
        }

        public Stream Write(List<Aeroexpress> aeroexpresses)
        {
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            
            writer.WriteLine("\"ID\";\"StationStart\";\"Line\";\"TimeStart\";\"StationEnd\";\"TimeEnd\";\"global_id\"");
            foreach (var aeroexpress in aeroexpresses)
            {
                var line = $"\"{aeroexpress.Id}\";" +
                            $"\"{aeroexpress.StationStart}\";" +
                            $"\"{aeroexpress.Line}\";" +
                            $"\"{aeroexpress.TimeStart}\";" +
                            $"\"{aeroexpress.StationEnd}\";" +
                            $"\"{aeroexpress.TimeEnd}\";" +
                            $"\"{aeroexpress.GlobalId}\"";
                writer.WriteLine(line);
            }

            // Очистка всех буферов для writer и запись всех данных в базовый поток
            writer.Flush();
            
            memoryStream.Position = 0;
            return memoryStream;
        }

        public bool ConvertToJson(List<Aeroexpress> aeroexpresses, string savePath)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string jsonString = JsonSerializer.Serialize(aeroexpresses, options);

                System.IO.File.WriteAllText(savePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                // здесь надо лог добавить
                return false;
            }
        }
    }
}
