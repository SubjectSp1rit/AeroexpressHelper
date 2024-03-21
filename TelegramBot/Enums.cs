using Telegram.Bot.Types;

namespace TelegramBot
{
    public class Enums
    {
        /// <summary>
        /// Состояние каждого пользователя - ничего, ожидание сообщения, ожидание второго сообщения
        /// </summary>
        public enum UserState
        {
            None,
            AwaitingMessage,
            AwaitingForSecondMessage
        }

        /// <summary>
        /// Выбранные пользователем фильтры
        /// </summary>
        public enum SelectedFilterType
        {
            None,
            FilterByStationStart,
            FilterByStationEnd,
            FilterByStationStartAndEnd
        }
    }
}
