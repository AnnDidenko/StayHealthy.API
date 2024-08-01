using System.Text;
using Microsoft.Extensions.Options;
using StayHealthy.Client.Models.Appointment;
using StayHealthy.Client.Models.Availability;
using StayHealthy.Client.Settings;

namespace StayHealthy.Client.ApiClients;

public class SlotsClient : ClientBase, ISlotsClient
{
    private readonly SlotsClientSettings _clientSettings;
    private readonly UserSettings _userSettings;

    public SlotsClient(HttpClient httpClient,
        IOptions<SlotsClientSettings> clientSettings,
        IOptions<UserSettings> userSettings) : base(httpClient)
    {
        _clientSettings = clientSettings.Value;
        _userSettings = userSettings.Value;
    }

    public async Task<WeeklyAvailabilityResponse> GetWeeklyAvailabilityAsync(string date)
    {
        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_userSettings.Login}:{_userSettings.Password}"));
        var response = await GetAsync<WeeklyAvailabilityResponse>($"{_clientSettings.GetAvailability}/{date}", credentials, 3);
        return response;
    }
    
    public async Task MakeAppointmentAsync(AppointmentRequest request)
    {
        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_userSettings.Login}:{_userSettings.Password}"));
        await PostAsync(_clientSettings.ReserveSlot, request, credentials, 3);
    }
}