namespace StayHealthy.Client.Models.Availability;

public class BusySlot
{
    public BusySlot(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}