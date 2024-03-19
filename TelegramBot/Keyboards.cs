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
                                            InlineKeyboardButton.WithCallbackData("Произвести выборку", "filterFile"),
                                            InlineKeyboardButton.WithCallbackData("Отсортировать", "sortFile")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Посмотреть формат файла", "fileFormat")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Удалить CSV файл", "deleteCSV"),
                                            InlineKeyboardButton.WithCallbackData("Удалить JSON файл", "deleteJSON")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скачать CSV файл", "downloadCSVFile"),
                                            InlineKeyboardButton.WithCallbackData("Скачать JSON файл", "downloadJSONFile")
                                        },

                                    });

        public static InlineKeyboardMarkup inlineMenuKeyboardWithJSON = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Загрузить новый файл", "addFile")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Произвести выборку", "filterFile"),
                                            InlineKeyboardButton.WithCallbackData("Отсортировать", "sortFile")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Посмотреть формат файла", "fileFormat")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Удалить JSON файл", "deleteJSON")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скачать JSON файл", "downloadJSONFile")
                                        },

                                    });

        public static InlineKeyboardMarkup inlineMenuKeyboardWithCSV = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Загрузить новый файл", "addFile")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Произвести выборку", "filterFile"),
                                            InlineKeyboardButton.WithCallbackData("Отсортировать", "sortFile")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Посмотреть формат файла", "fileFormat")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Удалить CSV файл", "deleteCSV")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скачать CSV файл", "downloadCSVFile")
                                        },

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

        public static InlineKeyboardMarkup inlineFilterJSONandCSVKeyboard = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData(".csv", "filterCSV"),
                                            InlineKeyboardButton.WithCallbackData(".json", "filterJSON")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Вернуться в меню", "backToMenu")
                                        }
                                    });

        public static InlineKeyboardMarkup inlineFilterJSONKeyboard = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData(".json", "filterJSON")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Вернуться в меню", "backToMenu")
                                        }
                                    });

        public static InlineKeyboardMarkup inlineFilterCSVKeyboard = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData(".csv", "filterCSV")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Вернуться в меню", "backToMenu")
                                        }
                                    });

        public static InlineKeyboardMarkup inlineSortJSONandCSVKeyboard = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData(".csv", "sortCSV"),
                                            InlineKeyboardButton.WithCallbackData(".json", "sortJSON")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Вернуться в меню", "backToMenu")
                                        }
                                    });

        public static InlineKeyboardMarkup inlineSortJSONKeyboard = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData(".json", "sortJSON")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Вернуться в меню", "backToMenu")
                                        }
                                    });

        public static InlineKeyboardMarkup inlineSortCSVKeyboard = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData(".csv", "sortCSV")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Вернуться в меню", "backToMenu")
                                        }
                                    });
    }
}
