using Microsoft.SharePoint.Client;
using PnP.Framework;

namespace SharePointCsomApi.Services;

public class SharePointService : ISharePointService
{
    private readonly ISharePointContextFactory _contextFactory;
    private readonly IConfiguration _config;

    public SharePointService(ISharePointContextFactory contextFactory, IConfiguration config)
    {
        _contextFactory = contextFactory;
        _config = config;
    }

    public async Task<List<object>> GetTasksAsync()
    {
        using var context = await _contextFactory.CreateContextAsync();

        var listName = _config["SharePoint:ListName"] ?? "Tasks";
        var list = context.Web.Lists.GetByTitle(listName);

        var items = list.GetItems(CamlQuery.CreateAllItemsQuery());

        context.Load(items, collection => collection.Include(
            item => item.Id,
            item => item["Title"],
            item => item["Description"],
            item => item["Status"],
            item => item["DueDate"]
        ));
        
        await context.ExecuteQueryRetryAsync();

        return items.AsEnumerable().Select(i => new
        {
            Id = i.Id,
            Title = i["Title"]?.ToString() ?? string.Empty,
            Description = i["Description"]?.ToString() ?? string.Empty,
            Status = i["Status"]?.ToString() ?? string.Empty,
            DueDate = i["DueDate"]?.ToString() ?? string.Empty
        }).Cast<object>().ToList();
    }

    public async Task SeedDataAsync(int count)
    {
        using var context = await _contextFactory.CreateContextAsync();
        var listName = _config["SharePoint:ListName"] ?? "Tasks";
        var list = context.Web.Lists.GetByTitle(listName);

        // Seeding in batches of 100 to avoid long-running single ExecuteQuery and handle timeouts
        int batchSize = 100;
        for (int i = 0; i < count; i++)
        {
            var itemCreateInfo = new ListItemCreationInformation();
            var newItem = list.AddItem(itemCreateInfo);
            newItem["Title"] = $"Task {i + 1} - Generated at {DateTime.Now}";
            newItem["Description"] = $"This is a test task for the performance lab. Item index: {i}";
            newItem["Status"] = i % 2 == 0 ? "Pending" : "Completed";
            newItem["DueDate"] = DateTime.Now.AddDays(i % 30);
            newItem.Update();

            if ((i + 1) % batchSize == 0 || (i + 1) == count)
            {
                await context.ExecuteQueryRetryAsync();
            }
        }
    }
}
