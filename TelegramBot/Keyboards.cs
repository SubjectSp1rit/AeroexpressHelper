using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    /// <summary>
    /// Хранит все клавиатуры, которые выдаются пользователю
    /// </summary>
    public static class Keyboards
    {
        /// <summary>
        /// Полная клавиатура главного меню
        /// </summary>
        public static InlineKeyboardMarkup InlineMenuKeyboardWithJSONandCSV { get; } = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Загрузить новый файл", "addFile")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Произвести выборку", "filterCSV"),
                                            InlineKeyboardButton.WithCallbackData("Отсортировать", "sortCSV")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Посмотреть формат файла", "fileFormat")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скачать CSV файл", "downloadCSVFile"),
                                            InlineKeyboardButton.WithCallbackData("Скачать JSON файл", "downloadJSONFile")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Удалить файл", "deleteFile")
                                        }
                                    });

        /// <summary>
        /// Упрощенная клавиатура главного меню
        /// </summary>
        public static InlineKeyboardMarkup InlineMenuKeyboardSimplified { get; } = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Загрузить файл", "addFile")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Посмотреть формат файла", "fileFormat")
                                        }
                                    });

        /// <summary>
        /// Клавиатура возврата в меню
        /// </summary>
        public static InlineKeyboardMarkup InlineBackToMenuKeyboard { get; } = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Вернуться в меню", "backToMenu")
                                        }
                                    });

        /// <summary>
        /// Клавиатура выбора сортировки
        /// </summary>
        public static InlineKeyboardMarkup InlineSelectSortCSVTypeKeyboard { get; } = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("TimeStart в порядке увеличения времени", "sortCSVByTimeStart"),
                                            InlineKeyboardButton.WithCallbackData("TimeEnd в порядке увеличения времени", "sortCSVByTimeEnd")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Вернуться в меню", "backToMenu")
                                        }
                                    });

        /// <summary>
        /// Клавиатура выбора фильтра
        /// </summary>
        public static InlineKeyboardMarkup InlineSelectFilterCSVTypeKeyboard { get; } = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("StationStart", "filterCSVByStationStart"),
                                            InlineKeyboardButton.WithCallbackData("StationEnd", "filterCSVByStationEnd"),
                                            InlineKeyboardButton.WithCallbackData("StationStart & StationEnd", "filterCSVByStationStartAndEnd")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Вернуться в меню", "backToMenu")
                                        }
                                    });
    }
}
