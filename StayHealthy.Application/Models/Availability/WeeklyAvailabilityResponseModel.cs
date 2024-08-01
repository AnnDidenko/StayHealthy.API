namespace StayHealthy.Application.Models.Availability;

public class WeeklyAvailabilityResponseModel
{
    public WeeklyAvailabilityResponseModel(
        Guid facilityId,
        Dictionary<DayOfWeek, DayScheduleModel?> weekSchedule)
    {
        FacilityId = facilityId;
        WeekSchedule = weekSchedule;
    }

    public Guid FacilityId { get; set; }
    public Dictionary<DayOfWeek, DayScheduleModel?> WeekSchedule { get; set; }
}