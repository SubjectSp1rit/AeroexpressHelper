using Serilog;

namespace CSVnJSONAnalyzer
{
    public class CSVProcessing
    {
        public static bool SortByTimeStart(string filePath)
        {
            try
            {
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

        public static bool SortByTimeEnd(string filePath)
        {
            try
            {
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
    }
}
