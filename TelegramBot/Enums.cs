using Telegram.Bot.Types;

namespace TelegramBot
{
    public class Enums
    {
        public enum UserState
        {
            None,
            AwaitingMessage
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
