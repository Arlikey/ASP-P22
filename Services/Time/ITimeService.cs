namespace ASP_P22.Services.Time
{
    public interface ITimeService
    {
        long GetCurrentDateTimestamp();
        string GetCurrentDateSqlFormat();
        long ParseSqlFormatToTimestamp(string sqlFormat);
    }
}
