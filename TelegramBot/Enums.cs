using Telegram.Bot.Types;

namespace TelegramBot
{
    public class Enums
    {
        public enum UserState
        {
            None,
            AwaitingMessage,
            AwaitingForSecondMessage
        }

        public enum SelectedFilterType
        {
            None,
            FilterByStationStart,
            FilterByStationEnd,
            FilterByStationStartAndEnd
        }
    }
}
