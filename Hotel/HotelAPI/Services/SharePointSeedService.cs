using Microsoft.SharePoint.Client;
using PnP.Framework;

namespace HotelAPI.Services;

public class SharePointSeedService : ISharePointSeedService
{
    private readonly ISharePointContextFactory _contextFactory;

    public SharePointSeedService(ISharePointContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task SeedAsync()
    {
        using var context = await _contextFactory.CreateContextAsync();
        
        // 1. Criar Hotéis
        var hotel1Id = await CreateHotelAsync(context, 
            "Azure Resort & Spa", 
            "Maldives, Indian Ocean", 
            5, 
            "Um refúgio luxuoso sobre as águas cristalinas das Maldivas.",
            "https://images.unsplash.com/photo-1540541338287-41700207dee6?auto=format&fit=crop&q=80&w=1200");

        var hotel2Id = await CreateHotelAsync(context, 
            "The Urban Peak", 
            "Manhattan, New York", 
            4, 
            "Sofisticação e vista panorâmica no coração da cidade que nunca dorme.",
            "https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&q=80&w=1200");

        // 2. Criar Quartos para o Hotel 1 (Resort)
        await CreateRoomAsync(context, hotel1Id, "Overwater Villa 101", "Presidential", 2500, "Available");
        await CreateRoomAsync(context, hotel1Id, "Overwater Villa 102", "Presidential", 2500, "Occupied");
        await CreateRoomAsync(context, hotel1Id, "Beach Suite 201", "Suite", 1200, "Available");
        await CreateRoomAsync(context, hotel1Id, "Beach Suite 202", "Suite", 1200, "Cleaning");

        // 3. Criar Quartos para o Hotel 2 (Urban)
        await CreateRoomAsync(context, hotel2Id, "Skyline View 4001", "Deluxe", 550, "Available");
        await CreateRoomAsync(context, hotel2Id, "Skyline View 4002", "Deluxe", 550, "Available");
        await CreateRoomAsync(context, hotel2Id, "City Standard 1505", "Standard", 300, "Occupied");
        await CreateRoomAsync(context, hotel2Id, "City Standard 1506", "Standard", 300, "Maintenance");
    }

    private async Task<int> CreateHotelAsync(ClientContext context, string name, string location, int stars, string desc, string imageUrl)
    {
        var list = context.Web.Lists.GetByTitle("Hotels");
        
        var itemCreateInfo = new ListItemCreationInformation();
        var newItem = list.AddItem(itemCreateInfo);
        
        newItem["Title"] = name;
        newItem["Location"] = location;
        newItem["Stars"] = stars;
        newItem["Description"] = desc;
        newItem["ImageUrl"] = new FieldUrlValue { Url = imageUrl, Description = name };
        
        newItem.Update();
        context.Load(newItem, i => i.Id);
        await context.ExecuteQueryRetryAsync();
        
        return newItem.Id;
    }

    private async Task CreateRoomAsync(ClientContext context, int hotelId, string title, string type, decimal price, string status)
    {
        var list = context.Web.Lists.GetByTitle("Rooms");
        
        var itemCreateInfo = new ListItemCreationInformation();
        var newItem = list.AddItem(itemCreateInfo);
        
        newItem["Title"] = title;
        newItem["RoomType"] = type;
        newItem["PricePerNight"] = price;
        newItem["Status"] = status;
        newItem["HotelLookup"] = new FieldLookupValue { LookupId = hotelId };
        
        newItem.Update();
        await context.ExecuteQueryRetryAsync();
    }
}
