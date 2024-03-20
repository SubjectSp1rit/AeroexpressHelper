using Serilog;
using static CSVnJSONAnalyzer.FormatChecking;
using static CSVnJSONAnalyzer.Aeroexpress;
using System.Data;

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
                using (var reader = new StreamReader(csvStream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        lines.Add(line);
                    }
                }

                //bool doHaveSecondHeader = DoHaveSecondHeader(filePath);
                //if (doHaveSecondHeader) 
                //{
                //    bool successfulRemoval = RemoveSecondHeader(filePath);
                //    if (!successfulRemoval) return false;
                //}
                //bool isFormatCorrect = CheckCSVFormat(filePath);
                //if (!isFormatCorrect) return false;

                //var csvRows = File.ReadAllLines(filePath).Skip(1);
                //foreach (var row in csvRows)
                //{
                //    var parts = row.Split(';');

                //    int id = int.Parse(parts[0]);
                //    string? stationStart = parts[1];
                //    string? line = parts[2];
                //    string? timeStart = parts[3];
                //    string? stationEnd = parts[4];
                //    string? timeEnd = parts[5];
                //    int globalId = int.Parse(parts[6]);

                //    aeroexpressList.Add(new Aeroexpress(id,
                //                                        stationStart,
                //                                        line,
                //                                        timeStart,
                //                                        stationEnd,
                //                                        timeEnd,
                //                                        globalId));
                //}

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при создании списка Aeroexpress: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Сортирует .csv файлы по полю TimeStart в порядке возрастания значения
        /// </summary>
        /// <param name="filePath">путь до .csv файла</param>
        /// <returns>true - сортировка прошла успешно; false иначе</returns>
        public static bool SortByTimeStart(string filePath)
        {
            try
            {
                if (DoHaveSecondHeader(filePath)) RemoveSecondHeader(filePath);
                if (!CheckCSVFormat(filePath)) return false;

                var csvRows = File.ReadAllLines(filePath);
                var header = csvRows.First();
                var dataRows = csvRows.Skip(1)
                    .Select(row => new
                    {
                        OriginalRow = row,
                        TimeStart = row.Split(';')[3]
                    })
                    .OrderBy(x => x.TimeStart)
                    .Select(x => x.OriginalRow);

                var sortedRows = (new[] { header }).Concat(dataRows);
                File.WriteAllLines(filePath, sortedRows);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при сортировке {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Сортирует .csv файлы по полю TimeEnd в порядке возрастания значения
        /// </summary>
        /// <param name="filePath">путь до .csv файла</param>
        /// <returns>true - сортировка прошла успешно; false иначе</returns>
        public static bool SortByTimeEnd(string filePath)
        {
            try
            {
                if (DoHaveSecondHeader(filePath)) RemoveSecondHeader(filePath);
                if (!CheckCSVFormat(filePath)) return false;

                var csvRows = File.ReadAllLines(filePath);
                var header = csvRows.First();
                var dataRows = csvRows.Skip(1)
                    .Select(row => new
                    {
                        OriginalRow = row,
                        TimeEnd = row.Split(';')[5]
                    })
                    .OrderBy(x => x.TimeEnd)
                    .Select(x => x.OriginalRow);

                var sortedRows = (new[] { header }).Concat(dataRows);
                File.WriteAllLines(filePath, sortedRows);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при сортировке {filePath}: {ex.Message}");
                return false;
            }
        }

        public static bool FilterByStationStart(string filePath, string stationStart)
        {
            try
            {
                if (DoHaveSecondHeader(filePath)) RemoveSecondHeader(filePath);
                if (!CheckCSVFormat(filePath)) return false;

                var csvRows = File.ReadAllLines(filePath);
                var header = csvRows.First();
                var filteredRows = csvRows.Skip(1)
                    .Where(row => row.Split(';')[1].Equals(stationStart, StringComparison.OrdinalIgnoreCase))
                    .Prepend(header);

                File.WriteAllLines(filePath, filteredRows);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при фильтрации {filePath}: {ex.Message}");
                return false;
            }
        }

        public static bool FilterByStationEnd(string filePath, string stationEnd)
        {
            try
            {
                if (DoHaveSecondHeader(filePath)) RemoveSecondHeader(filePath);
                if (!CheckCSVFormat(filePath)) return false;

                var csvRows = File.ReadAllLines(filePath);
                var header = csvRows.First();
                var filteredRows = csvRows.Skip(1)
                    .Where(row => row.Split(';')[4].Equals(stationEnd, StringComparison.OrdinalIgnoreCase))
                    .Prepend(header);

                File.WriteAllLines(filePath, filteredRows);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при фильтрации {filePath}: {ex.Message}");
                return false;
            }
        }

        public static bool FilterByStationStartAndStationEnd(string filePath, string stationStart, string stationEnd)
        {
            try
            {
                if (DoHaveSecondHeader(filePath)) RemoveSecondHeader(filePath);
                if (!CheckCSVFormat(filePath)) return false;

                var csvRows = File.ReadAllLines(filePath);
                var header = csvRows.First();
                var filteredRows = csvRows.Skip(1)
                    .Where(row =>
                    {
                        var fields = row.Split(";");
                        return fields[1].Equals(stationStart, StringComparison.OrdinalIgnoreCase) &&
                               fields[4].Equals(stationEnd, StringComparison.OrdinalIgnoreCase);
                    })
                    .Prepend(header);

                File.WriteAllLines(filePath, filteredRows);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при фильтрации {filePath}: {ex.Message}");
                return false;
            }
        }
    }
}
