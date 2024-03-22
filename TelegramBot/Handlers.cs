using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static TelegramBot.Keyboards;
using static CSVnJSONAnalyzer.FormatChecking;
using CSVnJSONAnalyzer;
using static TelegramBot.Enums;
using System.Text.Json;

namespace TelegramBot
{
    public class Handlers
    {
        /// <summary>
        /// Словари-состояния каждого отдельного пользователя; long - tg id
        /// </summary>
        static Dictionary<long, UserState> userStates = new Dictionary<long, UserState>();
        static Dictionary<long, SelectedFilterType> selectedFilterType = new Dictionary<long, SelectedFilterType>();
        static Dictionary<long, string?> userFirstWord = new Dictionary<long, string?>();

        /// <summary>
        /// Обработчик приходящих Update`ов
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                    {
                        var message = update.Message;
                        var user = message.From;
                        var chat = message.Chat;

                        var csvProcessing = new CSVProcessing();
                        var jsonProcessing = new JSONProcessing();

                        if (message.Text != null)
                        {
                            // Если пользователь впервые пользуется ботом - добавляем ему стейт
                            if (!userStates.ContainsKey(message.Chat.Id))
                            {
                                userStates[message.Chat.Id] = UserState.None;
                                selectedFilterType[message.Chat.Id] = SelectedFilterType.None;
                            }

                            Log.Information($"{message.Chat.Username ?? "Аноним"} (id64: {user.Id}) написал сообщение: {message.Text}");

                            if (message.Text == "/start")
                            {
                                string stickerId = "CAACAgIAAxkBAAELwhJl_AJptAuvjuMV5FlPY1hH-9-tigACJlQAAp7OCwABoKNdD9edZb80BA";
                                string startText = "<b>Привет!</b> Я - бот для работы с CSV/JSON файлами. Умею обрабатывать данные: фильтровать и сортировать их," +
                                    " а также сохранять обработанные данные в форматах CSV и JSON." +
                                    " Первым делом отправь мне .csv/.json файл в корректном формате. " +
                                    "Для просмотра примера корректного формата нажми соответствующую кнопку ниже.";

                                // Отправка полного меню, если пользователь загрузил файл
                                if (System.IO.File.Exists($"{user.Id}.csv") && System.IO.File.Exists($"{user.Id}.json"))
                                {
                                    await botClient.SendStickerAsync(
                                        chatId: chat.Id,
                                        sticker: InputFile.FromFileId(stickerId),
                                        cancellationToken: cancellationToken);

                                    await botClient.SendTextMessageAsync(
                                        chatId: chat.Id,
                                        text: startText,
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cancellationToken,
                                        replyMarkup: InlineMenuKeyboardWithJSONandCSV);
                                }
                                // Отправка упрощенного меню, если пользователь не загруззил файл
                                else
                                {
                                    await botClient.SendStickerAsync(
                                        chatId: chat.Id,
                                        sticker: InputFile.FromFileId(stickerId),
                                        cancellationToken: cancellationToken);
                                    await botClient.SendTextMessageAsync(
                                        chatId: chat.Id,
                                        text: startText,
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cancellationToken,
                                        replyMarkup: InlineMenuKeyboardSimplified);
                                }

                                return;
                            }
                            // Секретная функция этого бота
                            else if (message.Text == "/secret")
                            {
                                string stickerId = "CAACAgIAAxkBAAELvTxl-Idf6c3jn4_kMOuKWbU8UwU_2wAC2kcAAufAwUuHoWpy_y09GzQE";

                                Log.Information($"{message.Chat.Username ?? "Аноним"} (id64: {user.Id}) увидел то, что не должен был");

                                await botClient.SendStickerAsync(
                                    chatId: chat.Id,
                                    sticker: InputFile.FromFileId(stickerId),
                                    cancellationToken: cancellationToken);

                                await botClient.SendTextMessageAsync(
                                    chatId: chat.Id,
                                    text: "???",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: InlineBackToMenuKeyboard);

                                return;
                            }
                            // Вход в первый стейт - ожидание первого сообщения-фильтра
                            else
                            {
                                if (userStates.ContainsKey(message.Chat.Id) &&
                                        userStates.ContainsValue(UserState.AwaitingMessage) &&
                                        message.Text != null &&
                                        message.Text != "")

                                {
                                    switch (selectedFilterType[message.Chat.Id])
                                    {
                                        // фильтр по StationStart
                                        case (SelectedFilterType.FilterByStationStart):
                                        {
                                            // ЧТЕНИЕ ФАЙЛА 

                                            using FileStream fileStreamRead = new FileStream($"{user.Id}.csv", FileMode.Open, FileAccess.Read);
                                            fileStreamRead.Position = 0;

                                            List <Aeroexpress> aeroexpresses;
                                            bool successfullCreationArray = csvProcessing.Read(fileStreamRead, out aeroexpresses);
                                            fileStreamRead.Close();

                                            // ФИЛЬТРАЦИЯ ФАЙЛА

                                            var sortedAeroexpresses =
                                                (from aeroexpress in aeroexpresses
                                                 where aeroexpress.StationStart.ToLower() == message.Text.ToLower()
                                                 select aeroexpress).ToList();

                                            // СОХРАНЕНИЕ ФАЙЛА В .CSV

                                            // Получение потока с данными
                                            var dataStream = csvProcessing.Write(sortedAeroexpresses);

                                            using (var fileStream = new FileStream($"{user.Id}.csv", FileMode.Create, FileAccess.Write))
                                            {
                                                dataStream.CopyTo(fileStream);
                                            }
                                            dataStream.Close();

                                            // КОНВЕРТАЦИЯ ДАННЫХ В JSON
                                            bool successfullConvertation = csvProcessing.ConvertToJson(sortedAeroexpresses, $"{user.Id}.json");

                                            // Возвращаем стейты в базовое значение
                                            userStates[chat.Id] = UserState.None;
                                            selectedFilterType[chat.Id] = SelectedFilterType.None;

                                            await botClient.SendTextMessageAsync(
                                                chatId: chat.Id,
                                                text: $"Файл успешно отфильтрован!",
                                                replyMarkup: InlineBackToMenuKeyboard,
                                                cancellationToken: cancellationToken);

                                            return;
                                        }
                                        // Фильтрация по StationEnd
                                        case (SelectedFilterType.FilterByStationEnd):
                                        {
                                            // ЧТЕНИЕ ФАЙЛА 

                                            using FileStream fileStreamRead = new FileStream($"{user.Id}.csv", FileMode.Open, FileAccess.Read);
                                            fileStreamRead.Position = 0;

                                            List<Aeroexpress> aeroexpresses;
                                            bool successfullCreationArray = csvProcessing.Read(fileStreamRead, out aeroexpresses);
                                            fileStreamRead.Close();

                                            // ФИЛЬТРАЦИЯ ФАЙЛА

                                            var sortedAeroexpresses =
                                                (from aeroexpress in aeroexpresses
                                                 where aeroexpress.StationEnd.ToLower() == message.Text.ToLower()
                                                 select aeroexpress).ToList();

                                            // СОХРАНЕНИЕ ФАЙЛА В .CSV

                                            // Получение потока с данными
                                            var dataStream = csvProcessing.Write(sortedAeroexpresses);

                                            using (var fileStream = new FileStream($"{user.Id}.csv", FileMode.Create, FileAccess.Write))
                                            {
                                                dataStream.CopyTo(fileStream);
                                            }
                                            dataStream.Close();

                                            // КОНВЕРТАЦИЯ ДАННЫХ В JSON
                                            bool successfullConvertation = csvProcessing.ConvertToJson(sortedAeroexpresses, $"{user.Id}.json");

                                            // Возвращаем стейты в базовое значение
                                            userStates[chat.Id] = UserState.None;
                                            selectedFilterType[chat.Id] = SelectedFilterType.None;

                                            await botClient.SendTextMessageAsync(
                                                chatId: chat.Id,
                                                text: $"Файл успешно отфильтрован!",
                                                replyMarkup: InlineBackToMenuKeyboard,
                                                cancellationToken: cancellationToken);

                                            return;
                                        }
                                        // Фильтрация по StationStart и StationEnd
                                        case (SelectedFilterType.FilterByStationStartAndEnd):
                                        {
                                            // Сохраняем первое слово-фильтр
                                            userFirstWord[chat.Id] = message.Text;
                                            // Меняем стейт на ожидание второго сообщения
                                            userStates[chat.Id] = UserState.AwaitingForSecondMessage;

                                            await botClient.SendTextMessageAsync(
                                                chatId: chat.Id,
                                                text: $"Отлично! Теперь отправьте второе сообщение-фильтр",
                                                replyMarkup: InlineBackToMenuKeyboard,
                                                cancellationToken: cancellationToken);

                                            return;
                                        }
                                    }
                                }
                                // Переход во второй стейт
                                else if (userStates.ContainsKey(message.Chat.Id) &&
                                        userStates.ContainsValue(UserState.AwaitingForSecondMessage) &&
                                        message.Text != null &&
                                        message.Text != "")
                                {
                                    string? stationStart = userFirstWord[chat.Id];
                                    string? stationEnd = message.Text;

                                    // ЧТЕНИЕ ФАЙЛА 

                                    using FileStream fileStreamRead = new FileStream($"{user.Id}.csv", FileMode.Open, FileAccess.Read);
                                    fileStreamRead.Position = 0;

                                    List<Aeroexpress> aeroexpresses;
                                    bool successfullCreationArray = csvProcessing.Read(fileStreamRead, out aeroexpresses);
                                    fileStreamRead.Close();

                                    // ФИЛЬТРАЦИЯ ФАЙЛА

                                    var sortedAeroexpresses =
                                        (from aeroexpress in aeroexpresses
                                            where aeroexpress.StationStart.ToLower() == stationStart.ToLower() &&
                                            aeroexpress.StationEnd.ToLower() == stationEnd.ToLower()
                                            select aeroexpress).ToList();

                                    // СОХРАНЕНИЕ ФАЙЛА В .CSV

                                    // Получение потока с данными
                                    var dataStream = csvProcessing.Write(sortedAeroexpresses);

                                    using (var fileStream = new FileStream($"{user.Id}.csv", FileMode.Create, FileAccess.Write))
                                    {
                                        dataStream.CopyTo(fileStream);
                                    }
                                    dataStream.Close();

                                    // КОНВЕРТАЦИЯ ДАННЫХ В JSON
                                    bool successfullConvertation = csvProcessing.ConvertToJson(sortedAeroexpresses, $"{user.Id}.json");

                                    // Возвращаем стейты в базовое значение
                                    userStates[chat.Id] = UserState.None;
                                    selectedFilterType[chat.Id] = SelectedFilterType.None;
                                    userFirstWord[chat.Id] = null;

                                    await botClient.SendTextMessageAsync(
                                        chatId: chat.Id,
                                        text: $"Файл успешно отфильтрован!",
                                        replyMarkup: InlineBackToMenuKeyboard,
                                        cancellationToken: cancellationToken);

                                    return;
                                }
                                        await botClient.SendTextMessageAsync(
                                    chatId: chat.Id,
                                    text: "Неизвестная команда. Возврат в меню.",
                                    replyToMessageId: message.MessageId,
                                    cancellationToken: cancellationToken);
                                if (System.IO.File.Exists($"{user.Id}.csv") && System.IO.File.Exists($"{user.Id}.json"))
                                {
                                    await botClient.SendTextMessageAsync(
                                    chatId: chat.Id,
                                    text: "Главное меню",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: InlineMenuKeyboardWithJSONandCSV);
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    text: "Главное меню",
                                    replyMarkup: InlineMenuKeyboardSimplified,
                                    cancellationToken: cancellationToken);
                                }
                                return;
                            }
                        }
                        // Обработчик входящих файлов
                        if (message.Document != null)
                        {
                            var fileId = message.Document.FileId;
                            var fileInfo = await botClient.GetFileAsync(fileId, cancellationToken);
                            var filePath = fileInfo.FilePath;
                            var fileExtension = Path.GetExtension(filePath).ToLower();

                            Log.Information($"{message.Chat.Username ?? "Аноним"} (id64: {user.Id}) отправил файл типа {fileExtension}");

                            // Обработка входящего CSV-файла
                            if (fileExtension == ".csv")
                            {
                                var savePath = $"{message.From.Id}{fileExtension}";
                                var jsonSavePath = $"{message.From.Id}.json";

                                using (var saveFileStream = new FileStream(savePath, FileMode.Create))
                                {
                                    await botClient.DownloadFileAsync(filePath, saveFileStream);
                                }

                                // Проверка наличия второго заголовка. При наличии - удаление
                                string checkMessage = "";
                                if (DoHaveSecondHeader(savePath))
                                {
                                    bool sucessfulRemoval = RemoveSecondHeader(savePath);

                                    // Проверка корректности формата данных
                                    checkMessage = CheckCSVFormat(savePath, true);
;                               }
                                else
                                {
                                    // Проверка корректности формата данных
                                    checkMessage = CheckCSVFormat(savePath);
                                }

                                // Если сообщение об ошибке не пусто - формат файла неверен
                                if (checkMessage != "" || checkMessage == "err")
                                {
                                    await botClient.SendTextMessageAsync(
                                        message.Chat.Id,
                                        text: $"Неверный формат файла! Ошибка: {checkMessage}" +
                                        ". Исправьте данные и отправьте файл снова.",
                                        replyMarkup: InlineBackToMenuKeyboard,
                                        cancellationToken: cancellationToken);

                                        // Удаление некорректного файла
                                        if (System.IO.File.Exists(savePath))
                                        {
                                            System.IO.File.Delete(savePath);
                                        }

                                        return;
                                }

                                // ЧТЕНИЕ ФАЙЛА И СОЗДАНИЕ КОЛЛЕКЦИИ ЭКЗЕМПЛЯРОВ
                                using FileStream fileStreamRead = new FileStream($"{user.Id}.csv", FileMode.Open, FileAccess.Read);
                                fileStreamRead.Position = 0;

                                List <Aeroexpress> aeroexpresses;
                                bool successfullCreationArray = csvProcessing.Read(fileStreamRead, out aeroexpresses);
                                fileStreamRead.Close();

                                // КОНВЕРТАЦИЯ CSV ФАЙЛА В JSON
                                bool successfullConvertation = csvProcessing.ConvertToJson(aeroexpresses, jsonSavePath);
                                if (!successfullConvertation)
                                {
                                    await botClient.SendTextMessageAsync(
                                        message.Chat.Id,
                                        text: $"Не удалось автоматически конвертировать файл в .json!" +
                                        $" Повторите попытку или проверьте корректность данных.",
                                        replyMarkup: InlineBackToMenuKeyboard,
                                        cancellationToken: cancellationToken);

                                        var csvPath = $"{user.Id}.csv";
                                        var jsonPath = $"{user.Id}.json";

                                        if (System.IO.File.Exists(csvPath))
                                        {
                                            System.IO.File.Delete(csvPath);
                                        }
                                        if (System.IO.File.Exists(jsonPath))
                                        {
                                            System.IO.File.Delete(jsonPath);
                                        }

                                        return;
                                }

                                await botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    text: $"Файл с расширением {fileExtension} успешно загружен!",
                                    replyMarkup: InlineBackToMenuKeyboard,
                                    cancellationToken: cancellationToken);
                            }
                            // Обработчик входящих JSON-файлов
                            else if (fileExtension == ".json")
                            {
                                var savePath = $"{message.From.Id}{fileExtension}";

                                using (var saveFileStream = new FileStream(savePath, FileMode.Create))
                                {
                                    await botClient.DownloadFileAsync(filePath, saveFileStream);
                                }

                                // Проверка корректности формата данных
                                if (!CheckJSONFormat(savePath))
                                {
                                    await botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    text: $"Неверный формат файла! Проверьте корректность данных",
                                    replyMarkup: InlineBackToMenuKeyboard,
                                    cancellationToken: cancellationToken);

                                    if (System.IO.File.Exists(savePath))
                                    {
                                        System.IO.File.Delete(savePath);
                                    }

                                    return;
                                }

                                // КОНВЕРТАЦИЯ ДАННЫХ В CSV

                                string jsonString = System.IO.File.ReadAllText(savePath);
                                List<Aeroexpress>? aeroexpresses = JsonSerializer.Deserialize<List<Aeroexpress>>(jsonString);
                                    
                                var dataStream = csvProcessing.Write(aeroexpresses);

                                using (var fileStream = new FileStream($"{user.Id}.csv", FileMode.Create, FileAccess.Write))
                                {
                                    dataStream.CopyTo(fileStream);
                                }
                                dataStream.Close(); 


                                await botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                text: $"Файл с расширением {fileExtension} успешно загружен!",
                                replyMarkup: InlineBackToMenuKeyboard,
                                cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    text: $"Обработка файла такого типа не поддерживается",
                                    replyMarkup: InlineBackToMenuKeyboard,
                                    cancellationToken: cancellationToken);
                            }
                        }

                        return;
                    }
                    // Обработчик нажатых клавиш
                    case UpdateType.CallbackQuery:
                    {
                        // Переменная, содержащая в себе всю информацию о кнопке, которую нажали
                        var callbackQuery = update.CallbackQuery;
                        var user = callbackQuery.From;

                        var csvProcessing = new CSVProcessing();
                        var jsonProcessing = new JSONProcessing();

                        Log.Information($"{user.Username ?? "Аноним"} (id64: {user.Id}) нажал на inline-кнопку: {callbackQuery.Data}");

                        var chat = callbackQuery.Message.Chat; 
                    
                        switch (callbackQuery.Data)
                        {
                            case "fileFormat":
                            {
                                await botClient.EditMessageTextAsync(
                                    chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "<b>Пример правильного формата</b>:\n\n<i>CSV</i>:\nID;StationStart;Line;TimeStart;StationEnd;TimeEnd;global_id\n" +
                                    "1;Киевский вокзал;Во внуково;00:01;Аэропорт Внуково;00:34;2449491972\n\n<i>JSON</i>:\n" +
                                    "[\n  {\n    \"ID\": 1,\n    \"StationStart\": \"Киевский вокзал\",\n    \"Line\": \"Во внуково\",\n    " +
                                    "\"TimeStart\": \"00:01\",\n    \"StationEnd\": \"Аэропорт Внуково\",\n    \"TimeEnd\": \"00:34\",\n    " +
                                    "\"global_id\": 2449491972\n  }\n]",
                                    parseMode: ParseMode.Html,
                                    replyMarkup: InlineBackToMenuKeyboard,
                                    cancellationToken: cancellationToken);
                                return;
                            }

                            case "backToMenu":
                            {
                                // Отмена стейта, так как пользователь вернулся в меню
                                userStates[chat.Id] = UserState.None;
                                selectedFilterType[chat.Id] = SelectedFilterType.None;
                                userFirstWord[chat.Id] = null;

                                if (System.IO.File.Exists($"{user.Id}.csv") && System.IO.File.Exists($"{user.Id}.json"))
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Главное меню",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: InlineMenuKeyboardWithJSONandCSV);
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(
                                    chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Главное меню",
                                    replyMarkup: InlineMenuKeyboardSimplified,
                                    cancellationToken: cancellationToken);
                                }
                                return;
                            }

                            // Добавление нового файла
                            case "addFile":
                            {
                                await botClient.EditMessageTextAsync(
                                chat.Id,
                                messageId: callbackQuery.Message.MessageId,
                                text: "Отправьте мне .csv или .json файл",
                                replyMarkup: InlineBackToMenuKeyboard,
                                cancellationToken: cancellationToken);

                                return;
                            }
                            
                            // Удаление файлов
                            case "deleteFile":
                            {
                                var csvPath = $"{user.Id}.csv";
                                var jsonPath = $"{user.Id}.json";

                                if (System.IO.File.Exists(csvPath))
                                {
                                    System.IO.File.Delete(csvPath);
                                }
                                if (System.IO.File.Exists(jsonPath))
                                {
                                    System.IO.File.Delete(jsonPath);
                                }
                                
                                await botClient.EditMessageTextAsync(
                                    chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Файл успешно удалён!",
                                    replyMarkup: InlineBackToMenuKeyboard,
                                    cancellationToken: cancellationToken);

                                Log.Information($"{user.Username ?? "Аноним"} (id64: {user.Id}) удалил файл");

                                return;
                            }
                            
                            // Фильтрация данных
                            case "filterCSV":
                            {
                                await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Выберите поле для фильтрации",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: InlineSelectFilterCSVTypeKeyboard);
                                return;
                            }
                            
                            // Сортировка данных
                            case "sortCSV":
                            {
                                await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Выберите поле для сортировки",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: InlineSelectSortCSVTypeKeyboard);
                                return;
                            }
                            
                            // Сортировка по TimeStart
                            case "sortCSVByTimeStart":
                            {
                                // ЧТЕНИЕ ФАЙЛА 

                                using FileStream fileStreamRead = new FileStream($"{user.Id}.csv", FileMode.Open, FileAccess.Read);
                                fileStreamRead.Position = 0;

                                List <Aeroexpress> aeroexpresses;
                                bool successfullCreationArray = csvProcessing.Read(fileStreamRead, out aeroexpresses);
                                fileStreamRead.Close();

                                // СОРТИРОВКА ФАЙЛА

                                var sortedAeroexpresses =
                                    (from aeroexpress in aeroexpresses
                                     orderby TimeSpan.Parse(aeroexpress.TimeStart)
                                     select aeroexpress).ToList();

                                // СОХРАНЕНИЕ ФАЙЛА В .CSV

                                // Получение потока с данными
                                var dataStream = csvProcessing.Write(sortedAeroexpresses);

                                using (var fileStream = new FileStream($"{user.Id}.csv", FileMode.Create, FileAccess.Write))
                                {
                                    dataStream.CopyTo(fileStream);
                                }
                                dataStream.Close();

                                await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Файл .csv успешно отсортирован по полю TimeStart!",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: InlineBackToMenuKeyboard);

                                // КОНВЕРТАЦИЯ ДАННЫХ В JSON
                                bool successfullConvertation = csvProcessing.ConvertToJson(sortedAeroexpresses, $"{user.Id}.json");

                                return;
                            }

                            // Сортировка по TimeEnd
                            case "sortCSVByTimeEnd":
                            {
                                // ЧТЕНИЕ ФАЙЛА 

                                using FileStream fileStreamRead = new FileStream($"{user.Id}.csv", FileMode.Open, FileAccess.Read);
                                fileStreamRead.Position = 0;

                                List<Aeroexpress> aeroexpresses;
                                bool successfullCreationArray = csvProcessing.Read(fileStreamRead, out aeroexpresses);
                                fileStreamRead.Close();

                                // СОРТИРОВКА ФАЙЛА

                                var sortedAeroexpresses =
                                    (from aeroexpress in aeroexpresses
                                        orderby TimeSpan.Parse(aeroexpress.TimeEnd)
                                        select aeroexpress).ToList();

                                // СОХРАНЕНИЕ ФАЙЛА В .CSV

                                // Получение потока с данными
                                var dataStream = csvProcessing.Write(sortedAeroexpresses);

                                using (var fileStream = new FileStream($"{user.Id}.csv", FileMode.Create, FileAccess.Write))
                                {
                                    dataStream.CopyTo(fileStream);
                                }
                                dataStream.Close();

                                await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Файл .csv успешно отсортирован по полю TimeEnd!",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: InlineBackToMenuKeyboard);

                                // КОНВЕРТАЦИЯ ДАННЫХ В JSON
                                bool successfullConvertation = csvProcessing.ConvertToJson(sortedAeroexpresses, $"{user.Id}.json");

                                return;
                            }

                            // Фильтрация по StationStart
                            case "filterCSVByStationStart":
                            {
                                await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Отправьте сообщение, содержащее корректную строку для фильтрации по полю StationStart:",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: InlineBackToMenuKeyboard);

                                userStates[chat.Id] = UserState.AwaitingMessage;
                                selectedFilterType[chat.Id] = SelectedFilterType.FilterByStationStart;

                                return;
                            }

                            // Фильтрация по StationEnd
                            case "filterCSVByStationEnd":
                            {
                                await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Отправьте сообщение, содержащее корректную строку для фильтрации по полю StationEnd:",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: InlineBackToMenuKeyboard);

                                // Вход в стейт
                                userStates[chat.Id] = UserState.AwaitingMessage;
                                selectedFilterType[chat.Id] = SelectedFilterType.FilterByStationEnd;

                                return;
                            }
                            
                            // Фильтрация по StationStart & StationEnd
                            case "filterCSVByStationStartAndEnd":
                            {
                                await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Последовательно отправьте два сообщения, содержащие слова для фильтрации" +
                                    " по полям StationStart и StationEnd соответственно:",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: InlineBackToMenuKeyboard);

                                // Вход в стейт
                                userStates[chat.Id] = UserState.AwaitingMessage;
                                selectedFilterType[chat.Id] = SelectedFilterType.FilterByStationStartAndEnd;

                                return;
                            }

                            // Скачивание CSV файла
                            case ("downloadCSVFile"):
                            {
                                string path = $"{user.Id}.csv";
                                if (!System.IO.File.Exists(path)) Log.Error("Неудачная попытка скачивания csv");

                                        await using Stream stream = System.IO.File.OpenRead($"{user.Id}.csv");

                                await botClient.SendDocumentAsync(
                                    chatId: chat.Id,
                                    document: InputFile.FromStream(
                                        stream: stream,
                                        fileName: "aeroexpress.csv"),
                                    caption: "Готово! Вот ваш файл формата .csv:");

                                stream.Close();

                                return;
                            }
                            
                            // Скачивание JSON файла
                            case ("downloadJSONFile"):
                            {
                                string path = $"{user.Id}.json";
                                if (!System.IO.File.Exists(path)) Log.Error("Неудачная попытка скачивания json");
                                
                                await using Stream stream = System.IO.File.OpenRead($"{user.Id}.json");

                                await botClient.SendDocumentAsync(
                                    chatId: chat.Id,
                                    document: InputFile.FromStream(
                                        stream: stream,
                                        fileName: "aeroexpress.json"),
                                    caption: "Готово! Вот ваш файл формата .json:");
                                    
                                stream.Close();

                                return;
                            }
                        }
                    
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при обработке входящего сообщения: {ex.ToString()}");
            }
        }

        public static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            // Переменная, содержащая код ошибки и её сообщение 
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Log.Error(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
