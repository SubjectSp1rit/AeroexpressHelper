namespace CSVnJSONAnalyzer
{
    public class InputData
    {
        /// <summary>
        /// Ввод string поля для сортировки
        /// </summary>
        /// <returns></returns>
        public static string? GetStringField(string phrase)
        {
            Console.Write(phrase + ": ");
            string? field = Console.ReadLine();

            while (true)
            {
                if (field == null || field == "")
                {
                    Console.Write("Строка не может быть пустой. Введите еще раз: ");
                    field = Console.ReadLine();
                }
                else break;
            }

            return field;
        }
    }
}
