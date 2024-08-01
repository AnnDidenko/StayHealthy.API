namespace StayHealthy.Application.Extensions;

public static class DayOfWeekExtension
{
    public static DateOnly GetMondayOfWeek(DateOnly date)
    {
        var daysToSubtract = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

        return date.AddDays(-daysToSubtract);
    }
}