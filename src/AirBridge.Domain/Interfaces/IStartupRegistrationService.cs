namespace AirBridge.Domain.Interfaces;

public interface IStartupRegistrationService
{
    bool IsRegistered();
    void Register();
    void Unregister();
}