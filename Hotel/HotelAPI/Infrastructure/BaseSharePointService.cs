using HotelAPI.Infrastructure;

namespace HotelAPI.Infrastructure;

public abstract class BaseSharePointService
{
    protected readonly ISharePointContextFactory _contextFactory;
    protected readonly IConfiguration _config;

    protected BaseSharePointService(ISharePointContextFactory contextFactory, IConfiguration config)
    {
        _contextFactory = contextFactory;
        _config = config;
    }
}
