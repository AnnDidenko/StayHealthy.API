namespace StayHealthy.Client.Models.Availability;

public class DaySchedule
{
    public DaySchedule(WorkPeriod workPeriod, IReadOnlyCollection<BusySlot> busySlots)
    {
        WorkPeriod = workPeriod;
        BusySlots = busySlots;
    }
    
    public WorkPeriod WorkPeriod { get; set; }
    public IReadOnlyCollection<BusySlot>? BusySlots { get; set; }
}