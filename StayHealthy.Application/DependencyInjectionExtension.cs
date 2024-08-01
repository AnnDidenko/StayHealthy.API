using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StayHealthy.Application.Caching;
using StayHealthy.Application.CommandHandlers;
using StayHealthy.Application.Models.Appointment;
using StayHealthy.Application.Settings;
using StayHealthy.Application.Validators;

namespace StayHealthy.Application;

public static class DependencyInjectionExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        Client.DependencyInjectionExtension.ConfigureServices(services, configuration);
        
        services.Configure<CacheSettings>(options =>
            configuration.GetSection(nameof(CacheSettings)).Bind(options));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateAppointmentCommandHandler>());
        
        services.AddScoped<IValidator<PatientModel>, PatientValidator>()
            .AddScoped<IValidator<AppointmentRequestModel>, AppointmentValidator>();

        services.AddMemoryCache()
            .AddScoped<ICacheProvider, CacheProvider>();
        
        return services;
    }
}