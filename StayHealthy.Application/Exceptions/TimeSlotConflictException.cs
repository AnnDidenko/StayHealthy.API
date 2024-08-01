namespace StayHealthy.Application.Exceptions;

public class TimeSlotConflictException : ApplicationException
{
    public TimeSlotConflictException(DateTime appointmentStart,
        DateTime appointmentEnd) : base($"Time slot is already booked: {appointmentStart} - {appointmentEnd}.")
    {
    }
}