using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBot;
using static TelegramBot.Handlers;
using Serilog;

public class Program
{ 
    public static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Async(x => x.File("log.txt"))
            .CreateLogger();

        ITelegramBotClient? _botClient = new TelegramBotClient("6881641707:AAFxZF5JuFb9imit9o8kwtJlzECsm04XnvA"); // Токен бота
        ReceiverOptions? _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] // Указываем типы получаемых Update`ов
            {
                UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
                UpdateType.CallbackQuery // Inline кнопки
            },
            // Параметр, отвечающий за обработку сообщений, пришедших за то время, когда бот был оффлайн
            // True - не обрабатывать
            ThrowPendingUpdates = true,
        };

        using var cts = new CancellationTokenSource();

        // UpdateHander - обработчик приходящих Update`ов
        // ErrorHandler - обработчик ошибок, связанных с Bot API
        try
        {
            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cancellationToken: cts.Token); // Запускаем бота

            var me = await _botClient.GetMeAsync(); // Создаем переменную, в которую помещаем информацию о нашем боте.
            Log.Information($"{me.FirstName} запущен!");

            Thread.Sleep(-1);
            cts.Cancel();
            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Log.Error($"Ошибка запуска бота");
        }
    }
}