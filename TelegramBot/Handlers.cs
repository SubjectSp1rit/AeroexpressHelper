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
using CSVnJSONAnalyzer;
using System.Collections.Generic;

namespace TelegramBot
{
    public class Handlers
    {
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

                        if (message.Text != null)
                        {
                            Log.Information($"{message.Chat.Username ?? "Аноним"} (id64: {user.Id}) написал сообщение: {message.Text}");
                            if (message.Text == "/start")
                            {
                                string startText = "<b>Привет!</b> Я - бот для работы с CSV/JSON файлами. Умею обрабатывать данные: фильтровать и сортировать их," +
                                    " а также сохранять обработанные данные в форматах CSV и JSON." +
                                    "Первым делом отправь мне .csv/.json файл в корректном формате. " +
                                    "Для просмотра примера корректного формата нажми соответствующую кнопку ниже.";
                                if (System.IO.File.Exists($"{user.Id}.csv") && System.IO.File.Exists($"{user.Id}.json"))
                                {
                                    await botClient.SendTextMessageAsync(
                                    chatId: chat.Id,
                                    text: startText,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineMenuKeyboardWithJSONandCSV);
                                }
                                else if (System.IO.File.Exists($"{user.Id}.csv"))
                                {
                                    await botClient.SendTextMessageAsync(
                                    chatId: chat.Id,
                                    text: startText,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineMenuKeyboardWithCSV);
                                }
                                else if (System.IO.File.Exists($"{user.Id}.json"))
                                {
                                    await botClient.SendTextMessageAsync(
                                    chatId: chat.Id,
                                    text: startText,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineMenuKeyboardWithJSON);
                                }
                                else
                                {
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
                                // TODO:Добавить проверку на корректность данных

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

                        Log.Information($"{user.Username ?? "Аноним"} (id64: {user.Id}) нажал на inline-кнопку: {callbackQuery.Data}");
                  
                        // Вот тут нужно уже быть немножко внимательным и не путаться!
                        // Мы пишем не callbackQuery.Chat , а callbackQuery.Message.Chat , так как
                        // кнопка привязана к сообщению, то мы берем информацию от сообщения.
                        var chat = callbackQuery.Message.Chat; 

                        var csvProcessing = new CSVProcessing();
                        var jsonProcessing = new JSONProcessing();
                    
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

                            case "FilterCSV":
                            {
                                return;
                            }

                            case "FilterJSON":
                            {
                                return;
                            }
                            
                            case "SortCSV":
                            {
                                await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Выберите поле для сортировки",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineSelectSortCSVTypeKeyboard);
                                return;
                            }

                            case "SortJSON":
                            {
                                return;
                            }

                            case "sortCSVByTimeStart":
                            {
                                using var fileStream = new MemoryStream();
                                fileStream.Position = 0;

                                List <Aeroexpress> lines;
                                bool successfullCreationArray = csvProcessing.Read(fileStream, out lines);
                                if (!successfullCreationArray)
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "К сожалению, возникла ошибка при обработке файла",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineBackToMenuKeyboard);
                                    Log.Error($"Неудачная сортировка файла {user.Id}.csv");
                                    return;
                                }

                                bool success = SortByTimeStart($"{user.Id}.csv");
                                if (success)
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Файл успешно отсортирован! Можете скачать его ниже.",
                                    cancellationToken: cancellationToken);

                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Нажмите кнопку ниже для возврата в меню",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineBackToMenuKeyboard);
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(
                                    chatId: chat.Id,
                                    messageId: callbackQuery.Message.MessageId,
                                    text: "Oops...Произошла ошибка во время сортировки. Попробуйте снова. Если ошибка \n" +
                                    "повторится, проверьте корректность данных в файле",
                                    cancellationToken: cancellationToken,
                                    replyMarkup: inlineBackToMenuKeyboard);
                                }
                                return;
                            }

                            case "sortCSVByTimeEnd":
                            {
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
