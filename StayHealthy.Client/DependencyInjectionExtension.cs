using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayHealthy.Client.ApiClients;
using StayHealthy.Client.Settings;

namespace StayHealthy.Client;

public static class DependencyInjectionExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SlotsClientSettings>(options =>
            configuration.GetSection(nameof(SlotsClientSettings)).Bind(options));
        
        services.Configure<UserSettings>(options =>
            configuration.GetSection(nameof(UserSettings)).Bind(options));
        
        services.AddHttpClient<ISlotsClient, SlotsClient>((serviceProvider, client) =>
        {
            var routing = serviceProvider.GetRequiredService<IOptions<SlotsClientSettings>>().Value;
            client.BaseAddress = new Uri(routing.BaseUrl);
        });
        
        services.AddLogging(c => c.AddConsole());
        
        return services;
    }
}