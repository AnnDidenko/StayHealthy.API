namespace StayHealthy.Application.Models.Availability;

public class DayScheduleModel
{
    public DayScheduleModel(
        DateOnly date,
        List<TimeSlot> availableTimeSlots)
    {
        Date = date;
        AvailableTimeSlots = availableTimeSlots;
    }

    public DateOnly Date { get; set; }
    public List<TimeSlot> AvailableTimeSlots { get; set; }
}