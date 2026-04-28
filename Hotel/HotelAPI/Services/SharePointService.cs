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
            item => item["Description"],
            item => item["ImageUrl"]
        ));
        
        await context.ExecuteQueryRetryAsync();
        
        return items.AsEnumerable().Select(i => {
            var imageUrl = "";
            if (i["ImageUrl"] is FieldUrlValue urlValue)
            {
                imageUrl = urlValue.Url;
            }
            else if (i["ImageUrl"] != null)
            {
                imageUrl = i["ImageUrl"].ToString();
            }

            return new HotelModel
            {
                Id = i.Id,
                Name = i["Title"]?.ToString() ?? "",
                Location = i["Location"]?.ToString() ?? "",
                Stars = i["Stars"] != null ? Convert.ToInt32(i["Stars"]) : 0,
                Description = i["Description"]?.ToString() ?? "",
                ImageUrl = imageUrl
            };
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
        var hotels = await GetHotelsAsync();
        var rooms = await GetAllRoomsAsync();

        using var context = await _contextFactory.CreateContextAsync();
        var list = context.Web.Lists.GetByTitle("Bookings");
        var items = list.GetItems(CamlQuery.CreateAllItemsQuery());
        
        context.Load(items, collection => collection.Include(
            item => item.Id,
            item => item["Title"],
            item => item["RoomLookup"],
            item => item["GuestName"],
            item => item["CheckIn"],
            item => item["CheckOut"],
            item => item["TotalAmount"],
            item => item["Status"]
        ));
        
        await context.ExecuteQueryRetryAsync();
        
        return items.AsEnumerable().Select(i => {
            var roomId = 0;
            if (i["RoomLookup"] is FieldLookupValue lv) roomId = lv.LookupId;
            else if (i["RoomLookup"] is FieldLookupValue[] lvs && lvs.Length > 0) roomId = lvs[0].LookupId;

            var room = rooms.FirstOrDefault(r => r.Id == roomId);
            var hotel = room != null ? hotels.FirstOrDefault(h => h.Id == room.HotelId) : null;

            return new BookingModel
            {
                Id = i.Id,
                BookingCode = i["Title"]?.ToString() ?? "",
                RoomId = roomId,
                RoomName = room?.Title ?? "Quarto não encontrado",
                HotelName = hotel?.Name ?? "Hotel não encontrado",
                GuestName = i["GuestName"]?.ToString() ?? "",
                CheckIn = Convert.ToDateTime(i["CheckIn"]),
                CheckOut = Convert.ToDateTime(i["CheckOut"]),
                TotalAmount = Convert.ToDecimal(i["TotalAmount"]),
                Status = i["Status"]?.ToString() ?? ""
            };
        }).ToList();
    }

    public async Task<object> GetDashboardStatsAsync()
    {
        using var context = await _contextFactory.CreateContextAsync();
        
        var hotelsList = context.Web.Lists.GetByTitle("Hotels");
        var bookingsList = context.Web.Lists.GetByTitle("Bookings");

        var hotelsItems = hotelsList.GetItems(CamlQuery.CreateAllItemsQuery());
        var bookingsItems = bookingsList.GetItems(CamlQuery.CreateAllItemsQuery());

        context.Load(hotelsItems);
        context.Load(bookingsItems, coll => coll.Include(
            item => item["TotalAmount"],
            item => item["Status"]
        ));

        await context.ExecuteQueryRetryAsync();

        var totalRevenue = bookingsItems.AsEnumerable()
            .Sum(i => i["TotalAmount"] != null ? Convert.ToDecimal(i["TotalAmount"]) : 0);
        
        var activeBookings = bookingsItems.AsEnumerable()
            .Count(i => i["Status"]?.ToString() == "CheckedIn");

        return new
        {
            TotalHotels = hotelsItems.Count,
            TotalBookings = bookingsItems.Count,
            TotalRevenue = totalRevenue,
            ActiveBookings = activeBookings
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
