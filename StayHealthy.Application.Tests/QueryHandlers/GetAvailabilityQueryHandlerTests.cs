using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using StayHealthy.Application.Caching;
using StayHealthy.Application.Models.Availability;
using StayHealthy.Application.Queries;
using StayHealthy.Application.QueryHandlers;
using StayHealthy.Application.Settings;
using StayHealthy.Client.ApiClients;
using StayHealthy.Client.Exceptions;
using StayHealthy.Client.Models.Availability;

namespace StayHealthy.Application.Tests.QueryHandlers;

public class GetAvailabilityQueryHandlerTests
{
    private readonly Mock<ISlotsClient> _slotsClient;
    private readonly Mock<ICacheProvider> _cacheProvider;
    private readonly IOptions<CacheSettings> _cacheSettings;
    private readonly GetAvailabilityQueryHandler _getAvailabilityQueryHandler;

    private static readonly Guid FacilityId = Guid.NewGuid();
    private static readonly DateOnly MondayDate = new(2024, 07, 29);
    private static readonly DateOnly TuesdayDate = new(2024, 07, 30);
    private static readonly DateOnly FridayDate = new(2024, 08, 2);

    public GetAvailabilityQueryHandlerTests()
    {
        _slotsClient = new Mock<ISlotsClient>();
        _cacheProvider = new Mock<ICacheProvider>();
        _cacheSettings = Options.Create(new CacheSettings { ExpirationMinutes = 5 });
        _getAvailabilityQueryHandler =
            new GetAvailabilityQueryHandler(_slotsClient.Object, _cacheProvider.Object, _cacheSettings);
    }

    [Theory]
    [MemberData(nameof(PrepareWeeklyWorkingHours))]
    public async Task Handle_WhenAvailabilityForTheWeekExists_ShouldReturnWeeklyAvailabilityResponseModel(
        DateOnly date,
        WeeklyAvailabilityResponse slotsClientAvailabilityResponse,
        WeeklyAvailabilityResponseModel expectedWeeklyAvailabilityResponseModel)
    {
        // Arrange
        var getAvailabilityQuery = new GetAvailabilityQuery { Date = date };

        _slotsClient.Setup(x => x.GetWeeklyAvailabilityAsync(It.IsAny<string>()))
            .ReturnsAsync(slotsClientAvailabilityResponse);
        _cacheProvider
            .Setup(x => x.GetOrAdd(It.IsAny<string>(), It.IsAny<Func<Task<WeeklyAvailabilityResponse>>>(),
                It.IsAny<TimeSpan>())).ReturnsAsync(slotsClientAvailabilityResponse);

        // Act
        var result = await _getAvailabilityQueryHandler.Handle(getAvailabilityQuery, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedWeeklyAvailabilityResponseModel);
    }

    [Fact]
    public async Task
        Handle_WhenSlotsApiClientThrowsBadRequestException_ShouldReceiveAnException()
    {
        // Arrange
        var getAvailabilityQuery = new GetAvailabilityQuery { Date = MondayDate };

        _slotsClient.Setup(x => x.GetWeeklyAvailabilityAsync(It.IsAny<string>()))
            .ThrowsAsync(
                new BadRequestException(new HttpClientException(HttpStatusCode.BadRequest, "Get",
                    new Uri("https://uri"),
                    "bad request")));

        _cacheProvider.Setup(x => x.GetOrAdd(It.IsAny<string>(), It.IsAny<Func<Task<WeeklyAvailabilityResponse>>>(),
                It.IsAny<TimeSpan>()))
            .ThrowsAsync(
                new BadRequestException(new HttpClientException(HttpStatusCode.BadRequest, "Get",
                    new Uri("https://uri"),
                    "bad request")));

        // Act
        var exception = await Record.ExceptionAsync(() =>
            _getAvailabilityQueryHandler.Handle(getAvailabilityQuery, CancellationToken.None));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<BadRequestException>();
    }

    public static IEnumerable<object?[]> PrepareWeeklyWorkingHours()
    {
        return new[]
        {
            new object?[]
            {
                MondayDate,
                new WeeklyAvailabilityResponse
                (new Facility
                    {
                        FacilityId = FacilityId,
                        Name = "FacilityName",
                        Address = "FacilityAddress"
                    },
                    60,
                    new DaySchedule(new WorkPeriod
                    {
                        StartHour = 11,
                        EndHour = 17,
                        LunchStartHour = 13,
                        LunchEndHour = 14
                    }, null!),
                    null!,
                    null!,
                    null!,
                    new DaySchedule(new WorkPeriod
                    {
                        StartHour = 8,
                        EndHour = 16,
                        LunchStartHour = 12,
                        LunchEndHour = 13
                    }, null!),
                    null!,
                    null!),
                new WeeklyAvailabilityResponseModel(
                    FacilityId,
                    new Dictionary<DayOfWeek, DayScheduleModel?>
                    {
                        {
                            DayOfWeek.Monday, new DayScheduleModel(MondayDate, [
                                new(new DateTime(MondayDate, new TimeOnly(11, 0)),
                                    new DateTime(MondayDate, new TimeOnly(12, 0))),

                                new(new DateTime(MondayDate, new TimeOnly(12, 0)),
                                    new DateTime(MondayDate, new TimeOnly(13, 0))),

                                new(new DateTime(MondayDate, new TimeOnly(14, 0)),
                                    new DateTime(MondayDate, new TimeOnly(15, 0))),

                                new(new DateTime(MondayDate, new TimeOnly(15, 0)),
                                    new DateTime(MondayDate, new TimeOnly(16, 0))),

                                new(new DateTime(MondayDate, new TimeOnly(16, 0)),
                                    new DateTime(MondayDate, new TimeOnly(17, 0)))
                            ])
                        },
                        { DayOfWeek.Tuesday, null },
                        { DayOfWeek.Wednesday, null },
                        { DayOfWeek.Thursday, null },
                        {
                            DayOfWeek.Friday, new DayScheduleModel(FridayDate, [
                                new(new DateTime(FridayDate, new TimeOnly(8, 0)),
                                    new DateTime(FridayDate, new TimeOnly(9, 0))),

                                new(new DateTime(FridayDate, new TimeOnly(9, 0)),
                                    new DateTime(FridayDate, new TimeOnly(10, 0))),

                                new(new DateTime(FridayDate, new TimeOnly(10, 0)),
                                    new DateTime(FridayDate, new TimeOnly(11, 0))),

                                new(new DateTime(FridayDate, new TimeOnly(11, 0)),
                                    new DateTime(FridayDate, new TimeOnly(12, 0))),

                                new(new DateTime(FridayDate, new TimeOnly(13, 0)),
                                    new DateTime(FridayDate, new TimeOnly(14, 0))),

                                new(new DateTime(FridayDate, new TimeOnly(14, 0)),
                                    new DateTime(FridayDate, new TimeOnly(15, 0))),

                                new(new DateTime(FridayDate, new TimeOnly(15, 0)),
                                    new DateTime(FridayDate, new TimeOnly(16, 0)))
                            ])
                        },
                        { DayOfWeek.Saturday, null },
                        { DayOfWeek.Sunday, null }
                    }
                )
            },
            new object?[]
            {
                TuesdayDate,
                new WeeklyAvailabilityResponse
                (new Facility
                    {
                        FacilityId = FacilityId,
                        Name = "FacilityName",
                        Address = "FacilityAddress"
                    },
                    30,
                    new DaySchedule(new WorkPeriod
                    {
                        StartHour = 11,
                        EndHour = 17,
                        LunchStartHour = 13,
                        LunchEndHour = 14
                    }, new[]
                    {
                        new BusySlot(new DateTime(MondayDate, new TimeOnly(11, 0)),
                            new DateTime(MondayDate, new TimeOnly(11, 30))),
                        new BusySlot(new DateTime(MondayDate, new TimeOnly(12, 30)),
                            new DateTime(MondayDate, new TimeOnly(13, 0))),
                        new BusySlot(new DateTime(MondayDate, new TimeOnly(16, 30)),
                            new DateTime(MondayDate, new TimeOnly(17, 0)))
                    }),
                    new DaySchedule(new WorkPeriod
                    {
                        StartHour = 8,
                        EndHour = 16,
                        LunchStartHour = 12,
                        LunchEndHour = 13
                    }, new[]
                    {
                        new BusySlot(new DateTime(TuesdayDate, new TimeOnly(13, 0)),
                            new DateTime(TuesdayDate, new TimeOnly(15, 0))),
                    }),
                    null!,
                    null!,
                    null!,
                    null!,
                    null!),
                new WeeklyAvailabilityResponseModel(
                    FacilityId,
                    new Dictionary<DayOfWeek, DayScheduleModel?>
                    {
                        {
                            DayOfWeek.Monday, new DayScheduleModel(MondayDate, [
                                new(new DateTime(MondayDate, new TimeOnly(11, 30)),
                                    new DateTime(MondayDate, new TimeOnly(12, 0))),

                                new(new DateTime(MondayDate, new TimeOnly(12, 0)),
                                    new DateTime(MondayDate, new TimeOnly(12, 30))),

                                new(new DateTime(MondayDate, new TimeOnly(14, 0)),
                                    new DateTime(MondayDate, new TimeOnly(14, 30))),

                                new(new DateTime(MondayDate, new TimeOnly(14, 30)),
                                    new DateTime(MondayDate, new TimeOnly(15, 0))),

                                new(new DateTime(MondayDate, new TimeOnly(15, 0)),
                                    new DateTime(MondayDate, new TimeOnly(15, 30))),

                                new(new DateTime(MondayDate, new TimeOnly(15, 30)),
                                    new DateTime(MondayDate, new TimeOnly(16, 0))),

                                new(new DateTime(MondayDate, new TimeOnly(16, 0)),
                                    new DateTime(MondayDate, new TimeOnly(16, 30)))
                            ])
                        },
                        {
                            DayOfWeek.Tuesday, new DayScheduleModel(TuesdayDate, [
                                new(new DateTime(TuesdayDate, new TimeOnly(8, 0)),
                                    new DateTime(TuesdayDate, new TimeOnly(8, 30))),

                                new(new DateTime(TuesdayDate, new TimeOnly(8, 30)),
                                    new DateTime(TuesdayDate, new TimeOnly(9, 0))),

                                new(new DateTime(TuesdayDate, new TimeOnly(9, 0)),
                                    new DateTime(TuesdayDate, new TimeOnly(9, 30))),

                                new(new DateTime(TuesdayDate, new TimeOnly(9, 30)),
                                    new DateTime(TuesdayDate, new TimeOnly(10, 0))),

                                new(new DateTime(TuesdayDate, new TimeOnly(10, 0)),
                                    new DateTime(TuesdayDate, new TimeOnly(10, 30))),

                                new(new DateTime(TuesdayDate, new TimeOnly(10, 30)),
                                    new DateTime(TuesdayDate, new TimeOnly(11, 0))),

                                new(new DateTime(TuesdayDate, new TimeOnly(11, 0)),
                                    new DateTime(TuesdayDate, new TimeOnly(11, 30))),

                                new(new DateTime(TuesdayDate, new TimeOnly(11, 30)),
                                    new DateTime(TuesdayDate, new TimeOnly(12, 0))),

                                new(new DateTime(TuesdayDate, new TimeOnly(15, 0)),
                                    new DateTime(TuesdayDate, new TimeOnly(15, 30))),

                                new(new DateTime(TuesdayDate, new TimeOnly(15, 30)),
                                    new DateTime(TuesdayDate, new TimeOnly(16, 0)))
                            ])
                        },
                        { DayOfWeek.Wednesday, null },
                        { DayOfWeek.Thursday, null },
                        { DayOfWeek.Friday, null },
                        { DayOfWeek.Saturday, null },
                        { DayOfWeek.Sunday, null }
                    }
                )
            },
            new object?[]
            {
                FridayDate,
                new WeeklyAvailabilityResponse
                (new Facility
                    {
                        FacilityId = FacilityId,
                        Name = "FacilityName",
                        Address = "FacilityAddress"
                    },
                    40,
                    new DaySchedule(new WorkPeriod
                    {
                        StartHour = 11,
                        EndHour = 17,
                        LunchStartHour = 13,
                        LunchEndHour = 14
                    }, new[]
                    {
                        new BusySlot(new DateTime(MondayDate, new TimeOnly(11, 0)),
                            new DateTime(MondayDate, new TimeOnly(11, 30))),
                        new BusySlot(new DateTime(MondayDate, new TimeOnly(12, 30)),
                            new DateTime(MondayDate, new TimeOnly(13, 0))),
                        new BusySlot(new DateTime(MondayDate, new TimeOnly(16, 30)),
                            new DateTime(MondayDate, new TimeOnly(17, 0)))
                    }),
                    new DaySchedule(new WorkPeriod
                    {
                        StartHour = 8,
                        EndHour = 16,
                        LunchStartHour = 12,
                        LunchEndHour = 13
                    }, new[]
                    {
                        new BusySlot(new DateTime(TuesdayDate, new TimeOnly(13, 0)),
                            new DateTime(TuesdayDate, new TimeOnly(15, 0))),
                    }),
                    null!,
                    null!,
                    null!,
                    null!,
                    null!),
                new WeeklyAvailabilityResponseModel(
                    FacilityId,
                    new Dictionary<DayOfWeek, DayScheduleModel?>
                    {
                        {
                            DayOfWeek.Monday, new DayScheduleModel(MondayDate, [
                                new(new DateTime(MondayDate, new TimeOnly(11, 30)),
                                    new DateTime(MondayDate, new TimeOnly(12, 10))),

                                new(new DateTime(MondayDate, new TimeOnly(14, 0)),
                                    new DateTime(MondayDate, new TimeOnly(14, 40))),

                                new(new DateTime(MondayDate, new TimeOnly(14, 40)),
                                    new DateTime(MondayDate, new TimeOnly(15, 20))),

                                new(new DateTime(MondayDate, new TimeOnly(15, 20)),
                                    new DateTime(MondayDate, new TimeOnly(16, 0)))
                            ])
                        },
                        {
                            DayOfWeek.Tuesday, new DayScheduleModel(TuesdayDate, [
                                new(new DateTime(TuesdayDate, new TimeOnly(8, 0)),
                                    new DateTime(TuesdayDate, new TimeOnly(8, 40))),

                                new(new DateTime(TuesdayDate, new TimeOnly(8, 40)),
                                    new DateTime(TuesdayDate, new TimeOnly(9, 20))),

                                new(new DateTime(TuesdayDate, new TimeOnly(9, 20)),
                                    new DateTime(TuesdayDate, new TimeOnly(10, 0))),

                                new(new DateTime(TuesdayDate, new TimeOnly(10, 0)),
                                    new DateTime(TuesdayDate, new TimeOnly(10, 40))),

                                new(new DateTime(TuesdayDate, new TimeOnly(10, 40)),
                                    new DateTime(TuesdayDate, new TimeOnly(11, 20))),

                                new(new DateTime(TuesdayDate, new TimeOnly(11, 20)),
                                    new DateTime(TuesdayDate, new TimeOnly(12, 0))),

                                new(new DateTime(TuesdayDate, new TimeOnly(15, 0)),
                                    new DateTime(TuesdayDate, new TimeOnly(15, 40)))
                            ])
                        },
                        { DayOfWeek.Wednesday, null },
                        { DayOfWeek.Thursday, null },
                        { DayOfWeek.Friday, null },
                        { DayOfWeek.Saturday, null },
                        { DayOfWeek.Sunday, null }
                    }
                )
            }
        };
    }
}