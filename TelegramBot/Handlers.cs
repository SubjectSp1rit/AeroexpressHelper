using Serilog;
using System;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static TelegramBot.Keyboards;
using static CSVnJSONAnalyzer.CSVProcessing;
using static CSVnJSONAnalyzer.JSONProcessing;
using static CSVnJSONAnalyzer.FormatChecking;
using CSVnJSONAnalyzer;
using System.Collections.Generic;
using System.Text;
using static TelegramBot.Enums;

namespace TelegramBot
{
    public class Handlers
    {

        static Dictionary<long, UserState> userStates = new Dictionary<long, UserState>();
        static Dictionary<long, SelectedFilterType> selectedFilterType = new Dictionary<long, SelectedFilterType>();

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
                                    "Первым делом отправь мне .csv/.json файл в корректном формате. " +
                                    "Для просмотра примера корректного формата нажми соответствующую кнопку ниже.";

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
                                        replyMarkup: inlineMenuKeyboardWithJSONandCSV);
                                }
                                else if (System.IO.File.Exists($"{user.Id}.csv"))
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
                                        replyMarkup: inlineMenuKeyboardWithCSV);
                                }
                                else if (System.IO.File.Exists($"{user.Id}.json"))
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
                                        replyMarkup: inlineMenuKeyboardWithJSON);
                                }
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
                                        replyMarkup: inlineMenuKeyboardSimplified);
                                }

                                return;
                            }
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
                                    replyMarkup: inlineBackToMenuKeyboard);

                                return;
                            }
                            else
                            {
                                if (userStates.ContainsKey(message.Chat.Id) &&
                                        userStates.ContainsValue(UserState.AwaitingMessage) &&
                                        message.Text != null &&
                                        message.Text != "")

                                {
                                    switch (selectedFilterType[message.Chat.Id])
                                    {
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
                                                    where aeroexpress.StationStart == message.Text
                                                    select aeroexpress).ToList();

                                            // СОХРАНЕНИЕ ФАЙЛА В .CSV

                                            // Получение потока с данными
                                            var dataStream = csvProcessing.Write(sortedAeroexpresses);

                                            using (var fileStream = new FileStream($"{user.Id}.csv", FileMode.Create, FileAccess.Write))
                                            {
                                                dataStream.CopyTo(fileStream);
                                            }
                                            dataStream.Close();

                                            // Возвращаем стейты в базовое значение
                                            userStates[chat.Id] = UserState.None;
                                            selectedFilterType[chat.Id] = SelectedFilterType.None;

                                            await botClient.SendTextMessageAsync(
                                                chatId: chat.Id,
                                                text: $"Файл успешно отфильтрован!",
                                                replyMarkup: inlineBackToMenuKeyboard,
                                                cancellationToken: cancellationToken);

                                            return;
                                        }

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
                                                    where aeroexpress.StationEnd == message.Text
                                                    select aeroexpress).ToList();

                                            // СОХРАНЕНИЕ ФАЙЛА В .CSV

                                            // Получение потока с данными
                                            var dataStream = csvProcessing.Write(sortedAeroexpresses);

                                            using (var fileStream = new FileStream($"{user.Id}.csv", FileMode.Create, FileAccess.Write))
                                            {
                                                dataStream.CopyTo(fileStream);
                                            }
                                            dataStream.Close();

                                            // Возвращаем стейты в базовое значение
                                            userStates[chat.Id] = UserState.None;
                                            selectedFilterType[chat.Id] = SelectedFilterType.None;

                                            await botClient.SendTextMessageAsync(
                                                chatId: chat.Id,
                                                text: $"Файл успешно отфильтрован!",
                                                replyMarkup: inlineBackToMenuKeyboard,
                                                cancellationToken: cancellationToken);

                                            return;
                                        }

                                        case (SelectedFilterType.FilterByStationStartAndEnd):
                                        {
                                            string[] parts = message.Text.Split(' ');
                                            if (parts.Length != 2)
                                            {
                                                await botClient.SendTextMessageAsync(
                                                chatId: chat.Id,
                                                text: $"Неправильный формат сообщения. Фильтрация не была произведена",
                                                replyMarkup: inlineBackToMenuKeyboard,
                                                cancellationToken: cancellationToken);

                                                return;
                                            }
                                            string stationStart = parts[0];
                                            string stationEnd = parts[1];

                                            // ЧТЕНИЕ ФАЙЛА 

                                            using FileStream fileStreamRead = new FileStream($"{user.Id}.csv", FileMode.Open, FileAccess.Read);
                                            fileStreamRead.Position = 0;

                                            List<Aeroexpress> aeroexpresses;
                                            bool successfullCreationArray = csvProcessing.Read(fileStreamRead, out aeroexpresses);
                                            fileStreamRead.Close();

                                            // ФИЛЬТРАЦИЯ ФАЙЛА

                                            var sortedAeroexpresses =
                                                (from aeroexpress in aeroexpresses
                                                    where aeroexpress.StationStart == stationStart &&
                                                    aeroexpress.StationEnd == stationEnd
                                                    select aeroexpress).ToList();

                                            // СОХРАНЕНИЕ ФАЙЛА В .CSV

                                            // Получение потока с данными
                                            var dataStream = csvProcessing.Write(sortedAeroexpresses);

                                            using (var fileStream = new FileStream($"{user.Id}.csv", FileMode.Create, FileAccess.Write))
                                            {
                                                dataStream.CopyTo(fileStream);
                                            }
                                            dataStream.Close();

                                            // Возвращаем стейты в базовое значение
                                            userStates[chat.Id] = UserState.None;
                                            selectedFilterType[chat.Id] = SelectedFilterType.None;

                                            await botClient.SendTextMessageAsync(
                                                chatId: chat.Id,
                                                text: $"Файл успешно отфильтрован!",
                                                replyMarkup: inlineBackToMenuKeyboard,
                                                cancellationToken: cancellationToken);

                                            return;
                                        }
                                    }
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
                                    replyMarkup: inlineMenuKeyboardWithJSONandCSV);
                                }
                                else if (System.IO.File.Exists($"{user.Id}.csv"))
                                {
                                    await botClient.SendTextMessageAsync(
                                    chatId: chat.Id,
                                    text: "Главное меню",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineMenuKeyboardWithCSV);
                                }
                                else if (System.IO.File.Exists($"{user.Id}.json"))
                                {
                                    await botClient.SendTextMessageAsync(
                                    chatId: chat.Id,
                                    text: "Главное меню",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineMenuKeyboardWithJSON);
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    text: "Главное меню",
                                    replyMarkup: inlineMenuKeyboardSimplified,
                                    cancellationToken: cancellationToken);
                                }
                                return;
                            }
                        }
                        if (message.Document != null)
                        {
                            var fileId = message.Document.FileId;
                            var fileInfo = await botClient.GetFileAsync(fileId, cancellationToken);
                            var filePath = fileInfo.FilePath;
                            var fileExtension = Path.GetExtension(filePath).ToLower();

                            Log.Information($"{message.Chat.Username ?? "Аноним"} (id64: {user.Id}) отправил файл типа {fileExtension}");

                            if (fileExtension == ".csv")
                            {
                                var savePath = $"{message.From.Id}{fileExtension}";

                                using (var saveFileStream = new FileStream(savePath, FileMode.Create))
                                {
                                    await botClient.DownloadFileAsync(filePath, saveFileStream);
                                }

                                // TODO:Добавить проверку на наличие второго заголовка
                                string checkMessage = "";
                                if (DoHaveSecondHeader(savePath))
                                {
                                    bool sucessfulRemoval = RemoveSecondHeader(savePath);
                                    checkMessage = CheckCSVFormat(savePath, true);
;                               }
                                else
                                {
                                    checkMessage = CheckCSVFormat(savePath);
                                }
                                // TODO:Добавить проверку на корректность данных
                                if (checkMessage != "" || checkMessage == "err")
                                {
                                    await botClient.SendTextMessageAsync(
                                        message.Chat.Id,
                                        text: $"Неверный формат файла! Ошибка: {checkMessage}" +
                                        ". Исправьте данные и отправьте файл снова.",
                                        replyMarkup: inlineBackToMenuKeyboard,
                                        cancellationToken: cancellationToken);

                                        // Удаление некорректного файла
                                        if (System.IO.File.Exists(savePath))
                                        {
                                            System.IO.File.Delete(savePath);
                                        }

                                        return;
                                }

                                await botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    text: $"Файл с расширением {fileExtension} успешно загружен!",
                                    replyMarkup: inlineBackToMenuKeyboard,
                                    cancellationToken: cancellationToken);
                            }
                            else if (fileExtension == ".json")
                            {
                                var savePath = $"{message.From.Id}{fileExtension}";

                                using (var saveFileStream = new FileStream(savePath, FileMode.Create))
                                {
                                    await botClient.DownloadFileAsync(filePath, saveFileStream);
                                }

                                // TODO: Добавить проверку на корректность данных

                                await botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    text: $"Файл с расширением {fileExtension} успешно загружен!",
                                    replyMarkup: inlineBackToMenuKeyboard,
                                    cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    text: $"Обработка файла такого типа не поддерживается",
                                    replyMarkup: inlineBackToMenuKeyboard,
                                    cancellationToken: cancellationToken);
                            }
                        }

                        return;
                    }

                    case UpdateType.CallbackQuery:
                    {
                        // Переменная, которая будет содержать в себе всю информацию о кнопке, которую нажали
                        var callbackQuery = update.CallbackQuery;
                        var user = callbackQuery.From;

                        var csvProcessing = new CSVProcessing();
                            var jsonProcessing = new JSONProcessing();

                        Log.Information($"{user.Username ?? "Аноним"} (id64: {user.Id}) нажал на inline-кнопку: {callbackQuery.Data}");
                  
                        // Вот тут нужно уже быть немножко внимательным и не путаться!
                        // Мы пишем не callbackQuery.Chat , а callbackQuery.Message.Chat , так как
                        // кнопка привязана к сообщению, то мы берем информацию от сообщения.
                        var chat = callbackQuery.Message.Chat; 
                    
                        switch (callbackQuery.Data)
                        {
                            // Data - указанный это нами id кнопки, указанный в параметре

                            case "fileFormat":
                            {
                                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                await botClient.EditMessageTextAsync(
                                    chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "<b>Пример правильного формата</b>:\n\n<i>CSV</i>:\nID;StationStart;Line;TimeStart;StationEnd;TimeEnd;global_id\n" +
                                    "1;Киевский вокзал;Во внуково;00:01;Аэропорт Внуково;00:34;2449491972\n\n<i>JSON</i>:\n" +
                                    "[\n  {\n    \"ID\": 1,\n    \"StationStart\": \"Киевский вокзал\",\n    \"Line\": \"Во внуково\",\n    " +
                                    "\"TimeStart\": \"00:01\",\n    \"StationEnd\": \"Аэропорт Внуково\",\n    \"TimeEnd\": \"00:34\",\n    " +
                                    "\"global_id\": 2449491972\n  }\n]",
                                    parseMode: ParseMode.Html,
                                    replyMarkup: inlineBackToMenuKeyboard,
                                    cancellationToken: cancellationToken);
                                return;
                            }

                            case "backToMenu":
                            {
                                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Вы вернулись в главное меню");

                                // Отмена стейта, так как пользователь вернулся в меню
                                userStates[chat.Id] = UserState.None;
                                selectedFilterType[chat.Id] = SelectedFilterType.None;

                                if (System.IO.File.Exists($"{user.Id}.csv") && System.IO.File.Exists($"{user.Id}.json"))
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Главное меню",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineMenuKeyboardWithJSONandCSV);
                                }
                                else if (System.IO.File.Exists($"{user.Id}.csv"))
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Главное меню",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineMenuKeyboardWithCSV);
                                }
                                else if (System.IO.File.Exists($"{user.Id}.json"))
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Главное меню",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineMenuKeyboardWithJSON);
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(
                                    chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Главное меню",
                                    replyMarkup: inlineMenuKeyboardSimplified,
                                    cancellationToken: cancellationToken);
                                }
                                return;
                            }

                            case "addFile":
                            {
                                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                await botClient.EditMessageTextAsync(
                                chat.Id,
                                messageId: callbackQuery.Message.MessageId,
                                text: "Отправьте мне .csv или .json файл",
                                replyMarkup: inlineBackToMenuKeyboard,
                                cancellationToken: cancellationToken);

                                return;
                            }

                            case "deleteCSV":
                            {
                                var path = $"{user.Id}.csv";

                                if (System.IO.File.Exists(path))
                                {
                                    System.IO.File.Delete(path);
                                }
                                
                                await botClient.EditMessageTextAsync(
                                    chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Файл .csv успешно удалён!",
                                    replyMarkup: inlineBackToMenuKeyboard,
                                    cancellationToken: cancellationToken);

                                Log.Information($"{user.Username ?? "Аноним"} (id64: {user.Id}) удалил файл типа .csv");

                                return;
                            }

                            case "deleteJSON":
                            {
                                var path = $"{user.Id}.json";

                                if (System.IO.File.Exists(path))
                                {
                                    System.IO.File.Delete(path);
                                }
                                
                                await botClient.EditMessageTextAsync(
                                    chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Файл .json успешно удалён!",
                                    replyMarkup: inlineBackToMenuKeyboard,
                                    cancellationToken: cancellationToken);

                                Log.Information($"{user.Username ?? "Аноним"} (id64: {user.Id}) удалил файл типа .json");

                                return;
                            }

                            case "filterFile":
                            {
                                if (System.IO.File.Exists($"{user.Id}.csv") && System.IO.File.Exists($"{user.Id}.json"))
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Выберите файл, который хотите отфильтровать",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineFilterJSONandCSVKeyboard);
                                }
                                else if (System.IO.File.Exists($"{user.Id}.csv"))
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Выберите файл, который хотите отфильтровать",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineFilterCSVKeyboard);
                                }
                                else if (System.IO.File.Exists($"{user.Id}.json"))
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Выберите файл, который хотите отфильтровать",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineFilterJSONKeyboard);
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Нет доступных файлов для фильтрации",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineBackToMenuKeyboard);
                                }
                                return;
                            }

                            case "sortFile":
                            {
                                if (System.IO.File.Exists($"{user.Id}.csv") && System.IO.File.Exists($"{user.Id}.json"))
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Выберите файл, который хотите отсортировать",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineSortJSONandCSVKeyboard);
                                }
                                else if (System.IO.File.Exists($"{user.Id}.csv"))
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Выберите файл, который хотите отсортировать",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineSortCSVKeyboard);
                                }
                                else if (System.IO.File.Exists($"{user.Id}.json"))
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Выберите файл, который хотите отсортировать",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineSortJSONKeyboard);
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Нет доступных файлов для сортировки",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineBackToMenuKeyboard);
                                }
                                return;
                            }

                            case "filterCSV":
                            {
                                await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Выберите поле для фильтрации",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineSelectFilterCSVTypeKeyboard);
                                return;
                            }

                            case "filterJSON":
                            {
                                return;
                            }
                            
                            case "sortCSV":
                            {
                                await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Выберите поле для сортировки",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineSelectSortCSVTypeKeyboard);
                                return;
                            }

                            case "sortJSON":
                            {
                                return;
                            }

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
                                    replyMarkup: inlineBackToMenuKeyboard);

                                return;
                            }

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
                                    replyMarkup: inlineBackToMenuKeyboard);

                                return;
                            }

                            case "filterCSVByStationStart":
                            {
                                await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Отправьте сообщение, содержащее корректную строку для фильтрации по полю StationStart:",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineBackToMenuKeyboard);

                                userStates[chat.Id] = UserState.AwaitingMessage;
                                selectedFilterType[chat.Id] = SelectedFilterType.FilterByStationStart;

                                return;
                            }

                            case "filterCSVByStationEnd":
                            {
                                await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Отправьте сообщение, содержащее корректную строку для фильтрации по полю StationEnd:",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineBackToMenuKeyboard);

                                userStates[chat.Id] = UserState.AwaitingMessage;
                                selectedFilterType[chat.Id] = SelectedFilterType.FilterByStationEnd;

                                return;
                            }
                            
                            case "filterCSVByStationStartAndEnd":
                            {
                                await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Отправьте сообщение, содержащее корректную строку для фильтрации по полю StationStart" +
                                    " и StationEnd. Корректный формат: два слова (Start и End соответственно) через пробел:",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineBackToMenuKeyboard);

                                userStates[chat.Id] = UserState.AwaitingMessage;
                                selectedFilterType[chat.Id] = SelectedFilterType.FilterByStationStartAndEnd;

                                return;
                            }
                        }
                    
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при обработке входящего сообщения: {ex.Message}");
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
