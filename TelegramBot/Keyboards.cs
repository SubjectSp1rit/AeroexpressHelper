using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public static class Keyboards
    {
        public static InlineKeyboardMarkup inlineMenuKeyboardWithJSONandCSV = new InlineKeyboardMarkup(
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

        public static InlineKeyboardMarkup inlineMenuKeyboardSimplified = new InlineKeyboardMarkup(
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

        public static InlineKeyboardMarkup inlineBackToMenuKeyboard = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Вернуться в меню", "backToMenu")
                                        }
                                    });

        public static InlineKeyboardMarkup inlineSelectSortCSVTypeKeyboard = new InlineKeyboardMarkup(
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

        public static InlineKeyboardMarkup inlineSelectFilterCSVTypeKeyboard = new InlineKeyboardMarkup(
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
