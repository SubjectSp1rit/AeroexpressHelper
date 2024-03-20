using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSVnJSONAnalyzer
{
    public class Aeroexpress
    {
        int _id;
        string? _stationStart;
        string? _line;
        string? _timeStart;
        string? _stationEnd;
        string? _timeEnd;
        int _globalId;

        [JsonPropertyName("ID")]
        public int Id
        {
            get => _id;
            set
            {
                if (!int.TryParse(value.ToString(), out _id)) throw new ArgumentException(
                    nameof(value), "Значение поля id должно быть числом.");
            }
        }

        [JsonPropertyName("StationStart")]
        public string? StationStart
        {
            get => _stationStart;
            set
            {
                if (!(value == null) && !(value == "")) _stationStart = value;
                else throw new ArgumentNullException(nameof(value),
                    "Значение поля stationStart не может быть null");
            }
        }

        [JsonPropertyName("Line")]
        public string? Line
        {
            get => _line;
            set
            {
                if (!(value == null) && !(value == "")) _line = value;
                else throw new ArgumentNullException(nameof(value),
                    "Значение поля line не может быть null");
            }
        }

        [JsonPropertyName("TimeStart")]
        public string? TimeStart
        {
            get => _timeStart;
            set
            {
                if (!(value == null) && !(value == "")) _timeStart = value;
                else throw new ArgumentNullException(nameof(value),
                    "Значение поля timeStart не может быть null");
            }
        }

        [JsonPropertyName("StationEnd")]
        public string? StationEnd
        {
            get => _stationEnd;
            set
            {
                if (!(value == null) && !(value == "")) _stationEnd = value;
                else throw new ArgumentNullException(nameof(value),
                    "Значение поля stationEnd не может быть null");
            }
        }

        [JsonPropertyName("TimeEnd")]
        public string? TimeEnd
        {
            get => _timeEnd;
            set
            {
                if (!(value == null) && !(value == "")) _timeEnd = value;
                else throw new ArgumentNullException(nameof(value),
                    "Значение поля timeEnd не может быть null");
            }
        }

        [JsonPropertyName("global_id")]
        public int GlobalId
        {
            get => _globalId;
            set
            {
                if (!int.TryParse(value.ToString(), out _globalId)) throw new ArgumentException(
                    nameof(value), "Значение поля globalId должно быть числом.");
            }
        }

        [JsonConstructor]
        public Aeroexpress(int id,
                           string? stationStart,
                           string? line,
                           string? timeStart,
                           string? stationEnd,
                           string? timeEnd,
                           int globalId)
        {
            (Id, StationStart, Line, TimeStart, StationEnd, TimeEnd, GlobalId) =
                (id, stationStart, line, timeStart, stationEnd, timeEnd, globalId);
        }

        public Aeroexpress()
        {
            throw new NotImplementedException("Класс Aeroexpress не предусматривает вызов пустого конструктора.");
        }
    }
}
