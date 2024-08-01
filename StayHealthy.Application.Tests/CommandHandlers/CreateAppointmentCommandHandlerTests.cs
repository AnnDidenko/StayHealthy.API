using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using StayHealthy.Application.Caching;
using StayHealthy.Application.CommandHandlers;
using StayHealthy.Application.Commands;
using StayHealthy.Application.Exceptions;
using StayHealthy.Application.Models.Appointment;
using StayHealthy.Application.Models.Availability;
using StayHealthy.Application.Queries;
using StayHealthy.Client.ApiClients;
using StayHealthy.Client.Models.Appointment;

namespace StayHealthy.Application.Tests.CommandHandlers;

public class CreateAppointmentCommandHandlerTests
{
    private readonly Mock<ISlotsClient> _slotsClient;
    private readonly Mock<IMediator> _mediator;
    private readonly Mock<ICacheProvider> _cacheProvider;
    private readonly Mock<IValidator<AppointmentRequestModel>> _validator;
    
    private readonly int _slotDurationMinutes = 60;
    private static readonly Guid FacilityId = Guid.NewGuid();
    private static readonly DateOnly MondayDate = new(2024, 07, 29);
    private readonly DateTime _appointmentStart = new (2024, 07, 29, 11, 0, 0);
    private readonly DateTime _appointmentEnd = new (2024, 07, 29, 12, 0, 0);
    
    public CreateAppointmentCommandHandlerTests()
    {
        _slotsClient = new Mock<ISlotsClient>();
        _mediator = new Mock<IMediator>();
        _cacheProvider = new Mock<ICacheProvider>();
        _validator = new Mock<IValidator<AppointmentRequestModel>>();
    }
    
    [Fact]
    public async Task Handle_WhenSlotIsAvailable_ShouldCreateASlot()
    {
        // Arrange
        var appointmentRequest = new AppointmentRequestModel
        {
            FacilityId = Guid.NewGuid(),
            Start = _appointmentStart,
            End = _appointmentEnd,
            Comments = "Comments",
            Patient = new PatientModel
            {
                Name = "Name",
                SecondName = "SecondName",
                Email = "Email",
                Phone = "Phone"
            }
        };
        
        var command = new CreateAppointmentCommand(appointmentRequest);

        _mediator.Setup(x => x.Send(It.IsAny<GetAvailabilityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeeklyAvailabilityResponseModel(
                FacilityId,
                new Dictionary<DayOfWeek, DayScheduleModel?>
                {
                    {
                        DayOfWeek.Monday, new DayScheduleModel(MondayDate, [
                            new(_appointmentStart, _appointmentEnd)
                        ])
                    },
                    { DayOfWeek.Tuesday, null },
                    { DayOfWeek.Wednesday, null },
                    { DayOfWeek.Thursday, null },
                    { DayOfWeek.Friday, null! }
                }
            ));
        
        _cacheProvider.Setup(x => x.Remove(It.IsAny<string>()));
        
        var handler = new CreateAppointmentCommandHandler(
            _slotsClient.Object,
            _mediator.Object,
            _cacheProvider.Object,
            _validator.Object);
        
        // Act
        await handler.Handle(command, CancellationToken.None);
        
        _slotsClient.Verify(x => x.MakeAppointmentAsync(It.IsAny<AppointmentRequest>()), Times.Once);
        _cacheProvider.Verify(x => x.Remove(It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenSlotIsNotAvailable_ShouldThrowTimeSlotConflictException()
    {
        // Arrange
        var appointmentRequest = new AppointmentRequestModel
        {
            FacilityId = Guid.NewGuid(),
            Start = _appointmentStart,
            End = _appointmentEnd,
            Comments = "Comments",
            Patient = new PatientModel
            {
                Name = "Name",
                SecondName = "SecondName",
                Email = "Email",
                Phone = "Phone"
            }
        };
        
        var command = new CreateAppointmentCommand(appointmentRequest);

        _mediator.Setup(x => x.Send(It.IsAny<GetAvailabilityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeeklyAvailabilityResponseModel(
                FacilityId,
                new Dictionary<DayOfWeek, DayScheduleModel?>
                {
                    {
                        DayOfWeek.Monday, new DayScheduleModel(MondayDate, [
                            new(_appointmentStart.AddHours(2), _appointmentEnd.AddHours(2))
                        ])
                    },
                    { DayOfWeek.Tuesday, null },
                    { DayOfWeek.Wednesday, null },
                    { DayOfWeek.Thursday, null },
                    { DayOfWeek.Friday, null! }
                }
            ));
        
        _cacheProvider.Setup(x => x.Remove(It.IsAny<string>()));
        
        var handler = new CreateAppointmentCommandHandler(
            _slotsClient.Object,
            _mediator.Object,
            _cacheProvider.Object,
            _validator.Object);
        
        // Act
        var exception = await Record.ExceptionAsync(() => handler.Handle(command, CancellationToken.None));
        
        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<TimeSlotConflictException>();
        exception.Message.Should().Be($"Time slot is already booked: {_appointmentStart} - {_appointmentEnd}.");
        
        _slotsClient.Verify(x => x.MakeAppointmentAsync(It.IsAny<AppointmentRequest>()), Times.Never);
        _cacheProvider.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
    }
}