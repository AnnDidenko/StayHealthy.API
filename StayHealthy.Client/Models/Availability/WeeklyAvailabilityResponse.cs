namespace StayHealthy.Client.Models.Availability;

public class WeeklyAvailabilityResponse
{
    public WeeklyAvailabilityResponse(Facility facility, int slotDurationMinutes, DaySchedule? monday,
        DaySchedule? tuesday, DaySchedule? wednesday, DaySchedule? thursday, DaySchedule? friday, DaySchedule? saturday, DaySchedule? sunday)
    {
        Facility = facility;
        SlotDurationMinutes = slotDurationMinutes;
        Monday = monday;
        Tuesday = tuesday;
        Wednesday = wednesday;
        Thursday = thursday;
        Friday = friday;
        Saturday = saturday;
        Sunday = sunday;
    }

    public Facility Facility { get; set; }
    public int SlotDurationMinutes { get; set; }
    public DaySchedule? Monday { get; set; }
    public DaySchedule? Tuesday { get; set; }
    public DaySchedule? Wednesday { get; set; }
    public DaySchedule? Thursday { get; set; }
    public DaySchedule? Friday { get; set; }
    public DaySchedule? Saturday { get; set; }
    public DaySchedule? Sunday { get; set; }
}