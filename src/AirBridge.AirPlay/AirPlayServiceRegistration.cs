using AirBridge.AirPlay.UxPlay;
using AirBridge.Domain.Interfaces;

namespace AirBridge.AirPlay;
using Microsoft.Extensions.DependencyInjection;

public static class AirPlayServiceRegistration
{
    public static IServiceCollection AddAirPlay(this IServiceCollection services)
    {
        services.AddSingleton<IAirPlayReceiver, UxPlayReceiver>();

        return services;
    }
}
