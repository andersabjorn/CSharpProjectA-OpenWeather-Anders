namespace Assignment_A1_01.Utils;

public static class UnixTimeStampHelper
{
    public static DateTime UnixTimeStampToDateTime(long unixTimeSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).LocalDateTime;
    }
}
