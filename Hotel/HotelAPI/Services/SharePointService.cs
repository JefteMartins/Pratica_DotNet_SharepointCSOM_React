using Microsoft.SharePoint.Client;
using PnP.Framework;
using Polly;
using Polly.CircuitBreaker;
using HotelAPI.Models;

namespace HotelAPI.Services;

public class SharePointService : ISharePointService
{
    private readonly ISharePointContextFactory _contextFactory;
    private readonly IConfiguration _config;
    
    // Disjuntor estático para que o estado seja compartilhado entre todas as requisições da API
    private static readonly ResiliencePipeline _circuitBreakerPipeline = new ResiliencePipelineBuilder()
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            // Se 50% das chamadas falharem em um período de 30s, abre o disjuntor
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 3, // Precisa de pelo menos 3 chamadas para começar a avaliar
            BreakDuration = TimeSpan.FromSeconds(30), // Tempo que o disjuntor fica aberto
            ShouldHandle = new PredicateBuilder().Handle<Exception>()
        })
        .Build();

    public SharePointService(ISharePointContextFactory contextFactory, IConfiguration config)
    {
        _contextFactory = contextFactory;
        _config = config;
    }

    public async Task<List<HotelModel>> GetHotelsAsync()
    {
        return await _circuitBreakerPipeline.ExecuteAsync(async token =>
        {
            using var context = await _contextFactory.CreateContextAsync();
            var list = context.Web.Lists.GetByTitle("Hotels");
            var items = list.GetItems(CamlQuery.CreateAllItemsQuery());

            context.Load(items, collection => collection.Include(
                item => item.Id,
                item => item["Title"],
                item => item["Location"],
                item => item["Stars"],
                item => item["Description"],
                item => item["ImageUrl"]
            ));

            await context.ExecuteQueryRetryAsync();

            return items.AsEnumerable().Select(i => new HotelModel(
                i.Id,
                i["Title"]?.ToString() ?? string.Empty,
                i["Location"]?.ToString() ?? string.Empty,
                Convert.ToInt32(i["Stars"] ?? 0),
                i["Description"]?.ToString() ?? string.Empty,
                (i["ImageUrl"] as FieldUrlValue)?.Url ?? "https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&q=80&w=1000"
            )).ToList();
        });
    }

    public async Task<List<RoomModel>> GetRoomsByHotelAsync(int hotelId)
    {
        return await _circuitBreakerPipeline.ExecuteAsync(async token =>
        {
            using var context = await _contextFactory.CreateContextAsync();
            var list = context.Web.Lists.GetByTitle("Rooms");

            var query = new CamlQuery
            {
                ViewXml = $@"<View>
                    <Query>
                        <Where>
                            <Eq>
                                <FieldRef Name='HotelLookup' LookupId='TRUE' />
                                <Value Type='Lookup'>{hotelId}</Value>
                            </Eq>
                        </Where>
                    </Query>
                </View>"
            };

            var items = list.GetItems(query);
            context.Load(items, collection => collection.Include(
                item => item.Id,
                item => item["Title"],
                item => item["RoomType"],
                item => item["PricePerNight"],
                item => item["Status"],
                item => item["HotelLookup"]
            ));

            await context.ExecuteQueryRetryAsync();

            return items.AsEnumerable().Select(i => new RoomModel(
                i.Id,
                i["Title"]?.ToString() ?? string.Empty,
                i["RoomType"]?.ToString() ?? string.Empty,
                Convert.ToDecimal(i["PricePerNight"] ?? 0),
                (i["HotelLookup"] as FieldLookupValue)?.LookupId ?? 0,
                i["Status"]?.ToString() ?? string.Empty
            )).ToList();
        });
    }

    public async Task<bool> UpdateRoomStatusAsync(int roomId, string newStatus)
    {
        return await _circuitBreakerPipeline.ExecuteAsync(async token =>
        {
            using var context = await _contextFactory.CreateContextAsync();
            var list = context.Web.Lists.GetByTitle("Rooms");
            var item = list.GetItemById(roomId);
            
            item["Status"] = newStatus;
            item.Update();

            await context.ExecuteQueryRetryAsync();
            return true;
        });
    }

    public async Task<BookingModel> CreateBookingAsync(BookingModel booking)
    {
        return await _circuitBreakerPipeline.ExecuteAsync(async token =>
        {
            using var context = await _contextFactory.CreateContextAsync();
            var list = context.Web.Lists.GetByTitle("Bookings");

            var itemCreateInfo = new ListItemCreationInformation();
            var newItem = list.AddItem(itemCreateInfo);

            newItem["Title"] = $"BK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            newItem["RoomLookup"] = new FieldLookupValue { LookupId = booking.RoomId };
            newItem["GuestName"] = booking.GuestName;
            newItem["CheckIn"] = booking.CheckIn;
            newItem["CheckOut"] = booking.CheckOut;
            newItem["TotalAmount"] = booking.TotalAmount;
            newItem["Status"] = "Confirmed";

            newItem.Update();
            context.Load(newItem);
            await context.ExecuteQueryRetryAsync();

            return new BookingModel(
                newItem.Id,
                newItem["Title"].ToString(),
                booking.RoomId,
                newItem["GuestName"].ToString(),
                Convert.ToDateTime(newItem["CheckIn"]),
                Convert.ToDateTime(newItem["CheckOut"]),
                Convert.ToDecimal(newItem["TotalAmount"]),
                newItem["Status"].ToString()
            );
        });
    }

    public async Task<List<BookingModel>> GetBookingsAsync()
    {
        return await _circuitBreakerPipeline.ExecuteAsync(async token =>
        {
            using var context = await _contextFactory.CreateContextAsync();
            var list = context.Web.Lists.GetByTitle("Bookings");
            var items = list.GetItems(CamlQuery.CreateAllItemsQuery());

            context.Load(items);
            await context.ExecuteQueryRetryAsync();

            return items.AsEnumerable().Select(i => new BookingModel(
                i.Id,
                (string)(i["Title"] ?? "N/A"),
                (i["RoomLookup"] as FieldLookupValue)?.LookupId ?? 0,
                (string)(i["GuestName"] ?? "Guest"),
                Convert.ToDateTime(i["CheckIn"]),
                Convert.ToDateTime(i["CheckOut"]),
                Convert.ToDecimal(i["TotalAmount"] ?? 0),
                (string)(i["Status"] ?? "Unknown")
            )).ToList();
        });
    }

    public async Task<object> GetDashboardStatsAsync()
    {
        var hotels = await GetHotelsAsync();
        var bookings = await GetBookingsAsync();

        return new
        {
            TotalHotels = hotels.Count,
            TotalBookings = bookings.Count,
            TotalRevenue = bookings.Sum(b => b.TotalAmount),
            ActiveBookings = bookings.Count(b => b.Status == "Confirmed" || b.Status == "CheckedIn")
        };
    }
}
