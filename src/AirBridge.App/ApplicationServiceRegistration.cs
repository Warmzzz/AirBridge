using AirBridge.Application.Services;
using AirBridge.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace AirBridge.App;

internal static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationUseCases(this IServiceCollection services)
    {
        services.AddSingleton<StartReceiverUseCase>();
        services.AddSingleton<StopReceiverUseCase>();
        services.AddSingleton<RestartReceiverUseCase>();
        services.AddSingleton<SelectAudioDeviceUseCase>();
        services.AddSingleton<GetReceiverStatusUseCase>();
        services.AddSingleton<GetAudioDeviceUseCase>();
        services.AddSingleton<UpdateReceiverConfigUseCase>();
        services.AddSingleton<ReceiverOrchestrationService>();
 
        return services;
    }
}