using System.Globalization;

namespace ASP_P22.Services.Time
{
    public class TimeService : ITimeService
    {
        public long GetCurrentDateTimestamp() => DateTimeOffset.Now.ToUnixTimeSeconds();

        public string GetCurrentDateSqlFormat() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public long ParseSqlFormatToTimestamp(string sqlFormat)
        {
            if (DateTime.TryParse(sqlFormat, out var date)) {
                return new DateTimeOffset(date).ToUnixTimeSeconds();
            }
            throw new Exception("Invalid date format!");
        }
    }
}
