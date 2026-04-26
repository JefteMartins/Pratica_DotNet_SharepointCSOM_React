namespace HotelAPI.Services;

public interface ISharePointProvisioningService
{
    Task ProvisionAsync();
    Task<string> TestConnectionAsync();
}
