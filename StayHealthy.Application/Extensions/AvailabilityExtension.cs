using StayHealthy.Application.Models.Availability;
using StayHealthy.Client.Models.Availability;

namespace StayHealthy.Application.Extensions;

public static class AvailabilityExtension
{
    public static WeeklyAvailabilityResponseModel MapAvailabilityPeriods(
        DateOnly mondayDate,
        WeeklyAvailabilityResponse weeklyAvailability)
    {
        var facilityId = weeklyAvailability.Facility.FacilityId;
        var weekSchedule = GetWeekSchedule(mondayDate, weeklyAvailability);
        
        return new WeeklyAvailabilityResponseModel(facilityId, weekSchedule);
    }

    private static Dictionary<DayOfWeek, DayScheduleModel?> GetWeekSchedule(
        DateOnly mondayDate,
        WeeklyAvailabilityResponse weeklyAvailability)
    {
        var weekSchedule = new Dictionary<DayOfWeek, DayScheduleModel?>
        {
            {
                DayOfWeek.Monday,
                GetDaySchedule(weeklyAvailability.SlotDurationMinutes, mondayDate, weeklyAvailability.Monday)
            },
            {
                DayOfWeek.Tuesday,
                GetDaySchedule(weeklyAvailability.SlotDurationMinutes, mondayDate.AddDays(1),
                    weeklyAvailability.Tuesday)
            },
            {
                DayOfWeek.Wednesday,
                GetDaySchedule(weeklyAvailability.SlotDurationMinutes, mondayDate.AddDays(2),
                    weeklyAvailability.Wednesday)
            },
            {
                DayOfWeek.Thursday,
                GetDaySchedule(weeklyAvailability.SlotDurationMinutes, mondayDate.AddDays(3),
                    weeklyAvailability.Thursday)
            },
            {
                DayOfWeek.Friday,
                GetDaySchedule(weeklyAvailability.SlotDurationMinutes, mondayDate.AddDays(4), weeklyAvailability.Friday)
            },
            {
                DayOfWeek.Saturday,
                GetDaySchedule(weeklyAvailability.SlotDurationMinutes, mondayDate.AddDays(5),
                    weeklyAvailability.Saturday)
            },
            {
                DayOfWeek.Sunday,
                GetDaySchedule(weeklyAvailability.SlotDurationMinutes, mondayDate.AddDays(6), weeklyAvailability.Sunday)
            }
        };

        return weekSchedule;
    }

    private static DayScheduleModel? GetDaySchedule(
        int slotDurationMinutes,
        DateOnly date,
        DaySchedule? schedule)
    {
        if(schedule is null)
        {
            return null;
        }
        
        var workStartTime = date.ToDateTime(new TimeOnly(schedule.WorkPeriod.StartHour, 0));
        var workEndTime = date.ToDateTime(new TimeOnly(schedule.WorkPeriod.EndHour, 0));
        var availableTimeSlots = new List<TimeSlot>();

        var busySlots = GetBusySlots(date, schedule);

        var lastSlotEnd = workStartTime;

        foreach (var busySlot in busySlots)
        {
            var availableTimeBetweenSlots = (busySlot.Start - lastSlotEnd).TotalMinutes;
            if (availableTimeBetweenSlots >= slotDurationMinutes)
            {
                var separatedAvailableTimeSlot =
                    SeparateAvailableTimeSlotByDuration(availableTimeBetweenSlots, slotDurationMinutes, lastSlotEnd);

                availableTimeSlots.AddRange(separatedAvailableTimeSlot);
            }

            if (busySlot.End > lastSlotEnd)
            {
                lastSlotEnd = busySlot.End;
            }
        }

        if (lastSlotEnd < workEndTime)
        {
            var availableTimeBetweenSlots = (workEndTime - lastSlotEnd).TotalMinutes;

            if (availableTimeBetweenSlots >= slotDurationMinutes)
            {
                var separatedAvailableTimeSlot =
                    SeparateAvailableTimeSlotByDuration(availableTimeBetweenSlots, slotDurationMinutes, lastSlotEnd);

                availableTimeSlots.AddRange(separatedAvailableTimeSlot);
            }
        }

        return new DayScheduleModel(date, availableTimeSlots);
    }
    
    private static List<TimeSlot> SeparateAvailableTimeSlotByDuration(double availableTimeBetweenSlots,
        int slotDurationMinutes, DateTime lastSlotEnd)
    {
        var availableSlots = Enumerable.Range(0, (int)availableTimeBetweenSlots / slotDurationMinutes)
            .Select(i => new TimeSlot(lastSlotEnd.AddMinutes(i * slotDurationMinutes),
                lastSlotEnd.AddMinutes((i + 1) * slotDurationMinutes)));

        return availableSlots.ToList();
    }

    private static List<BusySlot> GetBusySlots(
        DateOnly date,
        DaySchedule schedule)
    {
        var busySlots = new List<BusySlot>
        {
            GetSlotForLunch(schedule.WorkPeriod.LunchStartHour, schedule.WorkPeriod.LunchEndHour, date)
        };
        
        if(schedule.BusySlots != null)
        {
            busySlots.AddRange(schedule.BusySlots);
        }
        
        return busySlots.OrderBy(s => s.Start).ToList();
    }

    private static BusySlot GetSlotForLunch(
        int lunchStartHour,
        int lunchEndHour,
        DateOnly date)
    {
        var lunchStart = date.ToDateTime(new TimeOnly(lunchStartHour, 0));
        var lunchEnd = date.ToDateTime(new TimeOnly(lunchEndHour, 0));
        return new BusySlot(lunchStart, lunchEnd);
    }
}