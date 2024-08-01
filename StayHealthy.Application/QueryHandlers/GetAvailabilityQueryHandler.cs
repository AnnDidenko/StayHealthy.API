using MediatR;
using Microsoft.Extensions.Options;
using StayHealthy.Application.Caching;
using StayHealthy.Application.Extensions;
using StayHealthy.Application.Models.Availability;
using StayHealthy.Application.Queries;
using StayHealthy.Application.Settings;
using StayHealthy.Client.ApiClients;
using StayHealthy.Client.Models.Availability;

namespace StayHealthy.Application.QueryHandlers;

public class GetAvailabilityQueryHandler
    : IRequestHandler<GetAvailabilityQuery, WeeklyAvailabilityResponseModel>
{
    private readonly ISlotsClient _slotsClient;
    private readonly ICacheProvider _cacheProvider;
    private readonly CacheSettings _cacheSettings;

    public GetAvailabilityQueryHandler(
        ISlotsClient slotsClient,
        ICacheProvider cacheProvider,
        IOptions<CacheSettings> cacheSettings)
    {
        _slotsClient = slotsClient;
        _cacheProvider = cacheProvider;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<WeeklyAvailabilityResponseModel> Handle(GetAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var mondayDate = DayOfWeekExtension.GetMondayOfWeek(request.Date);
        var formattedDate = mondayDate.ToString("yyyyMMdd");

        var weeklyAvailabilityResponse = await _cacheProvider.GetOrAdd(GetWeeklyAvailabilityCacheKey(formattedDate),
            () => GetAvailabilityAsync(formattedDate), TimeSpan.FromMinutes(_cacheSettings.ExpirationMinutes));

        return AvailabilityExtension.MapAvailabilityPeriods(mondayDate, weeklyAvailabilityResponse);
    }

    private async Task<WeeklyAvailabilityResponse> GetAvailabilityAsync(string date)
    {
        var weeklyAvailabilityResponse = await _slotsClient.GetWeeklyAvailabilityAsync(date);
        return weeklyAvailabilityResponse;
    }
    
    private string GetWeeklyAvailabilityCacheKey(string date)
    {
        return $"availability-{date}";
    }
}