using HotelAPI.Models;
using Microsoft.SharePoint.Client;
using PnP.Framework;
using HotelAPI.Infrastructure;

namespace HotelAPI.Services;

public class SharePointService : BaseSharePointService, ISharePointService
{
    public SharePointService(ISharePointContextFactory contextFactory, IConfiguration config) 
        : base(contextFactory, config)
    {
    }

    public async Task<List<HotelModel>> GetHotelsAsync()
    {
        using var context = await _contextFactory.CreateContextAsync();
        var list = context.Web.Lists.GetByTitle("Hotels");
        var items = list.GetItems(CamlQuery.CreateAllItemsQuery());
        
        context.Load(items, collection => collection.Include(
            item => item.Id,
            item => item["Title"],
            item => item["Location"],
            item => item["Stars"],
            item => item["Description"]
        ));
        
        await context.ExecuteQueryRetryAsync();
        
        return items.AsEnumerable().Select(i => new HotelModel
        {
            Id = i.Id,
            Name = i["Title"]?.ToString() ?? "",
            Location = i["Location"]?.ToString() ?? "",
            Stars = i["Stars"] != null ? Convert.ToInt32(i["Stars"]) : 0,
            Description = i["Description"]?.ToString() ?? ""
        }).ToList();
    }

    public async Task<List<RoomModel>> GetAllRoomsAsync()
    {
        using var context = await _contextFactory.CreateContextAsync();
        var list = context.Web.Lists.GetByTitle("Rooms");
        var items = list.GetItems(CamlQuery.CreateAllItemsQuery());
        
        context.Load(items, collection => collection.Include(
            item => item.Id,
            item => item["Title"],
            item => item["RoomType"],
            item => item["PricePerNight"],
            item => item["Status"],
            item => item["HotelLookup"]
        ));
        
        await context.ExecuteQueryRetryAsync();
        
        return items.AsEnumerable().Select(MapToRoomModel).ToList();
    }

    public async Task<List<RoomModel>> GetRoomsByHotelAsync(int hotelId)
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
        context.Load(items);
        await context.ExecuteQueryRetryAsync();
        
        return items.AsEnumerable().Select(MapToRoomModel).ToList();
    }

    public async Task<bool> UpdateRoomStatusAsync(int roomId, string newStatus)
    {
        using var context = await _contextFactory.CreateContextAsync();
        var list = context.Web.Lists.GetByTitle("Rooms");
        var item = list.GetItemById(roomId);
        
        item["Status"] = newStatus;
        item.Update();
        
        await context.ExecuteQueryRetryAsync();
        return true;
    }

    public async Task<BookingModel> CreateBookingAsync(BookingModel booking)
    {
        using var context = await _contextFactory.CreateContextAsync();
        var list = context.Web.Lists.GetByTitle("Bookings");
        
        // Validação de Overlap no Server-Side
        var isAvailable = await CheckRoomAvailabilityAsync(context, booking.RoomId, booking.CheckIn, booking.CheckOut);
        if (!isAvailable)
        {
            throw new Exception("Quarto não disponível para as datas selecionadas.");
        }

        var itemInfo = new ListItemCreationInformation();
        var newItem = list.AddItem(itemInfo);
        
        newItem["Title"] = $"RES-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        newItem["RoomLookup"] = new FieldLookupValue { LookupId = booking.RoomId };
        newItem["GuestName"] = booking.GuestName;
        newItem["CheckIn"] = booking.CheckIn;
        newItem["CheckOut"] = booking.CheckOut;
        newItem["TotalAmount"] = booking.TotalAmount;
        newItem["Status"] = "Confirmed";
        
        newItem.Update();
        context.Load(newItem);
        await context.ExecuteQueryRetryAsync();
        
        booking.Id = newItem.Id;
        booking.BookingCode = newItem["Title"].ToString();
        booking.Status = "Confirmed";
        
        return booking;
    }

    public async Task<List<BookingModel>> GetBookingsAsync()
    {
        using var context = await _contextFactory.CreateContextAsync();
        var list = context.Web.Lists.GetByTitle("Bookings");
        var items = list.GetItems(CamlQuery.CreateAllItemsQuery());
        
        context.Load(items);
        await context.ExecuteQueryRetryAsync();
        
        return items.AsEnumerable().Select(i => new BookingModel
        {
            Id = i.Id,
            BookingCode = i["Title"]?.ToString() ?? "",
            RoomId = i["RoomLookup"] != null ? ((FieldLookupValue)i["RoomLookup"]).LookupId : 0,
            GuestName = i["GuestName"]?.ToString() ?? "",
            CheckIn = Convert.ToDateTime(i["CheckIn"]),
            CheckOut = Convert.ToDateTime(i["CheckOut"]),
            TotalAmount = Convert.ToDecimal(i["TotalAmount"]),
            Status = i["Status"]?.ToString() ?? ""
        }).ToList();
    }

    public async Task<object> GetDashboardStatsAsync()
    {
        var rooms = await GetAllRoomsAsync();
        var bookings = await GetBookingsAsync();
        
        return new
        {
            TotalRooms = rooms.Count,
            OccupiedRooms = rooms.Count(r => r.Status == "Occupied"),
            AvailableRooms = rooms.Count(r => r.Status == "Available"),
            TotalRevenue = bookings.Sum(b => b.TotalAmount),
            RecentBookingsCount = bookings.Count(b => b.CheckIn >= DateTime.Now.AddDays(-7))
        };
    }

    private async Task<bool> CheckRoomAvailabilityAsync(ClientContext context, int roomId, DateTime start, DateTime end)
    {
        var list = context.Web.Lists.GetByTitle("Bookings");
        
        // Query para encontrar overlaps
        var camlQuery = new CamlQuery
        {
            ViewXml = $@"<View>
                <Query>
                    <Where>
                        <And>
                            <Eq>
                                <FieldRef Name='RoomLookup' LookupId='TRUE' />
                                <Value Type='Lookup'>{roomId}</Value>
                            </Eq>
                            <And>
                                <Lt>
                                    <FieldRef Name='CheckIn' />
                                    <Value Type='DateTime' IncludeTimeValue='TRUE'>{end:yyyy-MM-ddTHH:mm:ssZ}</Value>
                                </Lt>
                                <Gt>
                                    <FieldRef Name='CheckOut' />
                                    <Value Type='DateTime' IncludeTimeValue='TRUE'>{start:yyyy-MM-ddTHH:mm:ssZ}</Value>
                                </Gt>
                            </And>
                        </And>
                    </Where>
                </Query>
            </View>"
        };

        var items = list.GetItems(camlQuery);
        context.Load(items);
        await context.ExecuteQueryRetryAsync();
        
        return items.Count == 0;
    }

    private RoomModel MapToRoomModel(ListItem i)
    {
        return new RoomModel
        {
            Id = i.Id,
            Title = i["Title"]?.ToString() ?? "",
            RoomType = i["RoomType"]?.ToString() ?? "",
            PricePerNight = i["PricePerNight"] != null ? Convert.ToDecimal(i["PricePerNight"]) : 0,
            Status = i["Status"]?.ToString() ?? "",
            HotelId = i["HotelLookup"] != null ? ((FieldLookupValue)i["HotelLookup"]).LookupId : 0
        };
    }
}
