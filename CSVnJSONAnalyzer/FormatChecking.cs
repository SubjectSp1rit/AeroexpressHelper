using Serilog;
using System.Text.RegularExpressions;

namespace CSVnJSONAnalyzer
{
    public class FormatChecking
    {
        private static bool HeaderMatch(string actualHeader, string[] expectedHeader)
        {
            string[] actualHeaderParts = actualHeader.Split(';').Select(h => h.Trim().ToLower()).ToArray();
            expectedHeader = expectedHeader.Select(h => h.Trim().ToLower()).ToArray();
            return actualHeaderParts.SequenceEqual(expectedHeader);
        }

        public static bool DoHaveSecondHeader(string filePath)
        {
            string[] csvRows = File.ReadAllLines(filePath);
            string actualSecondHeaderParts = csvRows[1];
            string[] expectedSecondHeader = new string[] { "Локальный идентификатор",
                                                   "Станция отправления",
                                                   "Направление Аэроэкспресс",
                                                   "Время отправления со станции",
                                                   "Конечная станция направления Аэроэкспресс",
                                                   "Время прибытия на конечную станцию направления Аэроэкспресс",
                                                   "global_id" };

            return HeaderMatch(actualSecondHeaderParts, expectedSecondHeader);
        }

        public static bool RemoveSecondHeader(string filePath)
        {
            string[] csvRows = File.ReadAllLines(filePath);

            if (csvRows == null || csvRows.Length <= 1) return false;

            csvRows = csvRows.Where((element, index) => index != 1).ToArray();
            File.WriteAllLines(filePath, csvRows);
            return true;
        }

        public static bool CheckCSVFormat(string filePath)
        {
            try
            {
                string[] csvRows = File.ReadAllLines(filePath);

                if (csvRows.Length == 0) return false;

                string[] expectedHeader = new string[] { "ID", "StationStart", "Line", "TimeStart", "StationEnd", "TimeEnd", "global_id" };

                if (!HeaderMatch(csvRows[0], expectedHeader)) return false;

                for (int i = 1; i < csvRows.Length; i++)
                {
                    string[] fields = csvRows[i].Split(';');
                    // Проверка на количество столбцов
                    if (fields.Length != 7) return false;

                    // Проверка на цифры в ID и global_id
                    if (!fields[0].All(char.IsDigit) || !fields[6].All(char.IsDigit)) return false;

                    // Проверка, что есть значения
                    if (fields[1] == null || fields[1] == "" ||
                        fields[2] == null || fields[2] == "" ||
                        fields[3] == null || fields[3] == "" ||
                        fields[4] == null || fields[4] == "" ||
                        fields[5] == null || fields[5] == "") return false;

                    // Проверка формата времени (HH:mm)
                    string timePattern = @"^\d{2}:\d{2}$";
                    if (!Regex.IsMatch(fields[3], timePattern) || !Regex.IsMatch(fields[5], timePattern)) return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при проверке формата {filePath}: {ex.Message}");
                return false;
            }
        }

        //public static bool CheckJSONFormat(string filePath)
        //{
        //    ;
        //}
    }
}
