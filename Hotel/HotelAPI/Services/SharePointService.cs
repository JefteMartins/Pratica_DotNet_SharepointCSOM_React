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
    
    private static readonly ResiliencePipeline _circuitBreakerPipeline = new ResiliencePipelineBuilder()
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 3,
            BreakDuration = TimeSpan.FromSeconds(30),
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

            return items.AsEnumerable().Select(i => new HotelModel {
                Id = i.Id,
                Name = i["Title"]?.ToString() ?? string.Empty,
                Location = i["Location"]?.ToString() ?? string.Empty,
                Stars = Convert.ToInt32(i["Stars"] ?? 0),
                Description = i["Description"]?.ToString() ?? string.Empty,
                ImageUrl = (i["ImageUrl"] as FieldUrlValue)?.Url ?? "https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&q=80&w=1000"
            }).ToList();
        });
    }

    public async Task<List<RoomModel>> GetAllRoomsAsync()
    {
        return await _circuitBreakerPipeline.ExecuteAsync(async token =>
        {
            using var context = await _contextFactory.CreateContextAsync();
            var list = context.Web.Lists.GetByTitle("Rooms");
            var items = list.GetItems(CamlQuery.CreateAllItemsQuery());
            var hotels = await GetHotelsAsync();

            context.Load(items, collection => collection.Include(
                item => item.Id,
                item => item["Title"],
                item => item["RoomType"],
                item => item["PricePerNight"],
                item => item["Status"],
                item => item["HotelLookup"]
            ));

            await context.ExecuteQueryRetryAsync();

            return items.AsEnumerable().Select(i => {
                var hotelId = (i["HotelLookup"] as FieldLookupValue)?.LookupId ?? 0;
                var hotel = hotels.FirstOrDefault(h => h.Id == hotelId);
                return new RoomModel {
                    Id = i.Id,
                    Title = (string)(i["Title"] ?? "N/A"),
                    RoomType = (string)(i["RoomType"] ?? "Standard"),
                    PricePerNight = Convert.ToDecimal(i["PricePerNight"] ?? 0),
                    HotelId = hotelId,
                    Status = (string)(i["Status"] ?? "Available"),
                    HotelStars = hotel?.Stars ?? 0
                };
            }).ToList();
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
                ViewXml = $@"<View><Query><Where><Eq><FieldRef Name='HotelLookup' LookupId='TRUE' /><Value Type='Lookup'>{hotelId}</Value></Eq></Where></Query></View>"
            };

            var items = list.GetItems(query);
            context.Load(items);
            await context.ExecuteQueryRetryAsync();

            return items.AsEnumerable().Select(i => new RoomModel {
                Id = i.Id,
                Title = i["Title"]?.ToString() ?? string.Empty,
                RoomType = i["RoomType"]?.ToString() ?? string.Empty,
                PricePerNight = Convert.ToDecimal(i["PricePerNight"] ?? 0),
                HotelId = (i["HotelLookup"] as FieldLookupValue)?.LookupId ?? 0,
                Status = i["Status"]?.ToString() ?? string.Empty
            }).ToList();
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
        // 1. Validação básica de datas
        if (booking.CheckIn >= booking.CheckOut)
        {
            throw new ArgumentException("A data de check-out deve ser posterior à data de check-in.");
        }

        return await _circuitBreakerPipeline.ExecuteAsync(async token =>
        {
            using var context = await _contextFactory.CreateContextAsync();
            
            // 2. Verificação de Overlap (Race Condition Prevention)
            var bookingsList = context.Web.Lists.GetByTitle("Bookings");
            
            // Query para encontrar qualquer reserva existente para este quarto que intersete o período desejado
            // (Start1 < End2) AND (End1 > Start2)
            var overlapQuery = new CamlQuery
            {
                ViewXml = $@"<View>
                    <Query>
                        <Where>
                            <And>
                                <Eq>
                                    <FieldRef Name='RoomLookup' LookupId='TRUE' />
                                    <Value Type='Lookup'>{booking.RoomId}</Value>
                                </Eq>
                                <And>
                                    <Lt>
                                        <FieldRef Name='CheckIn' />
                                        <Value Type='DateTime' StorageTZ='TRUE'>{booking.CheckOut:yyyy-MM-ddTHH:mm:ssZ}</Value>
                                    </Lt>
                                    <Gt>
                                        <FieldRef Name='CheckOut' />
                                        <Value Type='DateTime' StorageTZ='TRUE'>{booking.CheckIn:yyyy-MM-ddTHH:mm:ssZ}</Value>
                                    </Gt>
                                </And>
                            </And>
                        </Where>
                    </Query>
                </View>"
            };

            var existingBookings = bookingsList.GetItems(overlapQuery);
            context.Load(existingBookings);
            await context.ExecuteQueryRetryAsync();

            if (existingBookings.Count > 0)
            {
                throw new InvalidOperationException("O quarto não está disponível para o período selecionado.");
            }

            // 3. Criação da reserva
            var itemCreateInfo = new ListItemCreationInformation();
            var newItem = bookingsList.AddItem(itemCreateInfo);

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

            return new BookingModel {
                Id = newItem.Id,
                BookingCode = newItem["Title"].ToString(),
                RoomId = booking.RoomId,
                GuestName = newItem["GuestName"].ToString(),
                CheckIn = Convert.ToDateTime(newItem["CheckIn"]),
                CheckOut = Convert.ToDateTime(newItem["CheckOut"]),
                TotalAmount = Convert.ToDecimal(newItem["TotalAmount"]),
                Status = newItem["Status"].ToString()
            };
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

            return items.AsEnumerable().Select(i => new BookingModel {
                Id = i.Id,
                BookingCode = (string)(i["Title"] ?? "N/A"),
                RoomId = (i["RoomLookup"] as FieldLookupValue)?.LookupId ?? 0,
                GuestName = (string)(i["GuestName"] ?? "Guest"),
                CheckIn = Convert.ToDateTime(i["CheckIn"]),
                CheckOut = Convert.ToDateTime(i["CheckOut"]),
                TotalAmount = Convert.ToDecimal(i["TotalAmount"] ?? 0),
                Status = (string)(i["Status"] ?? "Unknown")
            }).ToList();
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
