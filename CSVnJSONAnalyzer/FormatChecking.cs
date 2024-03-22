using Serilog;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CSVnJSONAnalyzer
{
    /// <summary>
    /// Валидирует данные
    /// </summary>
    public class FormatChecking
    {
        /// <summary>
        /// Проверяет совпадают ли два заголовка из CSV файла
        /// </summary>
        /// <param name="actualHeader"></param>
        /// <param name="expectedHeader"></param>
        /// <returns></returns>
        private static bool HeaderMatch(string actualHeader, string[] expectedHeader)
        {
            try
            {
                string[] actualHeaderParts = actualHeader.Split(new char[] { ';', ',' }).Select(line => line.Trim('"'))
                .Select(h => h.Trim().ToLower()).Where(line => !string.IsNullOrEmpty(line)).ToArray();
                expectedHeader = expectedHeader.Select(h => h.Trim().ToLower()).ToArray();

                return actualHeaderParts.SequenceEqual(expectedHeader);
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при сравнении заголовков: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Проверка на наличие второго заголовка в csv файле
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>true - заголовок есть; false иначе</returns>
        public static bool DoHaveSecondHeader(string filePath)
        {
            try
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
            catch (Exception ex)
            {
                Log.Error($"Ошибка при поиске второго заголовка: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Убирает второй заголовок из файла
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool RemoveSecondHeader(string filePath)
        {
            try
            {
                string[] csvRows = File.ReadAllLines(filePath);

                if (csvRows == null || csvRows.Length <= 1) return false;

                csvRows = csvRows.Where((element, index) => index != 1).ToArray();
                File.WriteAllLines(filePath, csvRows);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при попытке убрать второй заголовок: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Проверка корректности формата csv-файла
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="hadSecondHeader"></param>
        /// <returns>строка - сообщение об ошибке. Если строка пустая - ошибок нет</returns>
        public static string CheckCSVFormat(string filePath, bool hadSecondHeader = false)
        {
            try
            {
                string[] csvRows = File.ReadAllLines(filePath);

                if (csvRows.Length == 0) return "файл пуст";

                string[] expectedHeader = new string[] { 
                    "ID",
                    "StationStart",
                    "Line",
                    "TimeStart",
                    "StationEnd",
                    "TimeEnd",
                    "global_id" };

                if (!HeaderMatch(csvRows[0], expectedHeader)) return "неправильный формат заголовка";

                int secondHeaderAddon = hadSecondHeader ? 1 : 0;

                for (int i = 1; i < csvRows.Length; i++)
                {

                    string[] fields = csvRows[i].Split(new char[] { ';', ',' }).Select(line => line.Trim('"')).Where(line => !string.IsNullOrEmpty(line)).ToArray();
                    
                    // Проверка на количество столбцов
                    if (fields.Length != 7) return $"неправильное количество столбцов в строчке {i+1+secondHeaderAddon}";

                    // Проверка на цифры в ID и global_id
                    if (!fields[0].All(char.IsDigit)) return $"ID должен полностью состоять из цифр. Ошибка в строчке {i+1+secondHeaderAddon}";
                    if (!fields[6].All(char.IsDigit)) return $"global_id должен полностью состоять из цифр. Ошибка в строчке {i+1+secondHeaderAddon}";

                    // Проверка, что есть значения
                    if (fields[1] == null || fields[1] == "" ||
                        fields[2] == null || fields[2] == "" ||
                        fields[3] == null || fields[3] == "" ||
                        fields[4] == null || fields[4] == "" ||
                        fields[5] == null || fields[5] == "") return $"пустая ячейка в строке {i+1+secondHeaderAddon}";

                    // Проверка формата времени (HH:mm)
                    string timePattern = @"^\d{2}:\d{2}$";
                    if (!Regex.IsMatch(fields[3], timePattern) || !Regex.IsMatch(fields[5], timePattern)) return $"неверный " +
                            $"формат времени в строчке {i+1+ secondHeaderAddon}";
                }

                return "";
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при проверке формата {filePath}: {ex.Message}");
                return "err";
            }
        }

        /// <summary>
        /// Проверка корректности JSON-файла
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>true - формат корректный; false иначе</returns>
        public static bool CheckJSONFormat(string filePath)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                List<Aeroexpress>? aeroexpresses = JsonSerializer.Deserialize<List<Aeroexpress>>(jsonString);
                if (aeroexpresses?.Count == 0)
                {
                    return false;
                }
                // Проверка корректности времени
                foreach (var item in aeroexpresses)
                {
                    string timePattern = @"^\d{2}:\d{2}$";
                    if (!Regex.IsMatch(item.TimeStart, timePattern) || !Regex.IsMatch(item.TimeEnd, timePattern))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при проверке формата {filePath}: {ex.Message}");
                return false;
            }
        }
    }
}
