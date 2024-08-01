namespace StayHealthy.Application.Models.Availability;

public class TimeSlot
{
    public TimeSlot(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}