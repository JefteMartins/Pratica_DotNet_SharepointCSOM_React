using Microsoft.SharePoint.Client;
using PnP.Framework;

namespace HotelAPI.Services;

public class SharePointProvisioningService : ISharePointProvisioningService
{
    private readonly ISharePointContextFactory _contextFactory;

    public SharePointProvisioningService(ISharePointContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task ProvisionAsync()
    {
        using var context = await _contextFactory.CreateContextAsync();
        var web = context.Web;
        context.Load(web);
        await context.ExecuteQueryRetryAsync();

        // 1. Lista: Hotels
        var hotelsList = await EnsureListAsync(context, "Hotels", ListTemplateType.GenericList);
        await EnsureFieldAsync(hotelsList, "Location", "Note"); // Multi-line text
        await EnsureFieldAsync(hotelsList, "Stars", "Number");
        await EnsureFieldAsync(hotelsList, "Description", "Note");
        await EnsureFieldAsync(hotelsList, "ImageUrl", "URL"); // Campo para placeholder

        // 2. Lista: Rooms
        var roomsList = await EnsureListAsync(context, "Rooms", ListTemplateType.GenericList);
        await EnsureChoiceFieldAsync(roomsList, "RoomType", new[] { "Standard", "Deluxe", "Suite", "Presidential" });
        await EnsureFieldAsync(roomsList, "PricePerNight", "Currency");
        await EnsureLookupFieldAsync(roomsList, "HotelLookup", hotelsList, "Title");
        await EnsureChoiceFieldAsync(roomsList, "Status", new[] { "Available", "Occupied", "Maintenance", "Cleaning" });

        // 3. Lista: Bookings
        var bookingsList = await EnsureListAsync(context, "Bookings", ListTemplateType.GenericList);
        await EnsureLookupFieldAsync(bookingsList, "RoomLookup", roomsList, "Title");
        await EnsureFieldAsync(bookingsList, "GuestName", "Text");
        await EnsureFieldAsync(bookingsList, "CheckIn", "DateTime");
        await EnsureFieldAsync(bookingsList, "CheckOut", "DateTime");
        await EnsureFieldAsync(bookingsList, "TotalAmount", "Currency");
        await EnsureChoiceFieldAsync(bookingsList, "Status", new[] { "Confirmed", "Cancelled", "CheckedIn", "CheckedOut" });
    }

    public async Task<string> TestConnectionAsync()
    {
        using var context = await _contextFactory.CreateContextAsync();
        context.Load(context.Web, w => w.Title);
        await context.ExecuteQueryRetryAsync();
        return context.Web.Title;
    }

    private async Task<List> EnsureListAsync(ClientContext context, string title, ListTemplateType template)
    {
        var list = context.Web.Lists.GetByTitle(title);
        context.Load(list);
        try
        {
            await context.ExecuteQueryRetryAsync();
            return list;
        }
        catch (ServerException)
        {
            var listInfo = new ListCreationInformation
            {
                Title = title,
                TemplateType = (int)template
            };
            list = context.Web.Lists.Add(listInfo);
            list.Update();
            await context.ExecuteQueryRetryAsync();
            return list;
        }
    }

    private async Task EnsureFieldAsync(List list, string internalName, string type)
    {
        try
        {
            list.Fields.GetByInternalNameOrTitle(internalName);
            list.Context.Load(list.Fields);
            await list.Context.ExecuteQueryRetryAsync();
        }
        catch (ServerException)
        {
            string fieldXml = $"<Field Type='{type}' Name='{internalName}' DisplayName='{internalName}' />";
            list.Fields.AddFieldAsXml(fieldXml, true, AddFieldOptions.DefaultValue);
            list.Update();
            await list.Context.ExecuteQueryRetryAsync();
        }
    }

    private async Task EnsureChoiceFieldAsync(List list, string internalName, string[] choices)
    {
        try
        {
            list.Fields.GetByInternalNameOrTitle(internalName);
            await list.Context.ExecuteQueryRetryAsync();
        }
        catch (ServerException)
        {
            string choicesXml = string.Join("", choices.Select(c => $"<CHOICE>{c}</CHOICE>"));
            string fieldXml = $@"<Field Type='Choice' Name='{internalName}' DisplayName='{internalName}'>
                                    <Default>{choices[0]}</Default>
                                    <CHOICES>{choicesXml}</CHOICES>
                                 </Field>";
            list.Fields.AddFieldAsXml(fieldXml, true, AddFieldOptions.DefaultValue);
            list.Update();
            await list.Context.ExecuteQueryRetryAsync();
        }
    }

    private async Task EnsureLookupFieldAsync(List list, string internalName, List targetList, string targetFieldName)
    {
        try
        {
            list.Fields.GetByInternalNameOrTitle(internalName);
            await list.Context.ExecuteQueryRetryAsync();
        }
        catch (ServerException)
        {
            list.Context.Load(targetList, t => t.Id);
            await list.Context.ExecuteQueryRetryAsync();

            string fieldXml = $"<Field Type='Lookup' Name='{internalName}' DisplayName='{internalName}' List='{{{targetList.Id}}}' ShowField='{targetFieldName}' />";
            list.Fields.AddFieldAsXml(fieldXml, true, AddFieldOptions.DefaultValue);
            list.Update();
            await list.Context.ExecuteQueryRetryAsync();
        }
    }
}
