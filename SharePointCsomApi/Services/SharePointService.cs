using Microsoft.SharePoint.Client;
using PnP.Framework;
using System.Security.Cryptography.X509Certificates;

namespace SharePointCsomApi.Services;

public class SharePointService
{
    private readonly IConfiguration _config;

    public SharePointService(IConfiguration config)
    {
        _config = config;
    }

    private ClientContext GetContext()
    {
        var siteUrl = _config["SharePoint:SiteUrl"];
        var clientId = _config["SharePoint:ClientId"];
        var tenantId = _config["SharePoint:TenantId"];
        var certPath = _config["SharePoint:CertificatePath"];
        var certPassword = _config["SharePoint:CertificatePassword"];

        var certificate = new X509Certificate2(certPath, certPassword);

        var authManager = new AuthenticationManager(
            clientId,
            certificate,
            tenantId
        );

        return authManager.GetContext(siteUrl);
    }

    public List<object> GetTasks()
    {
        using var context = GetContext();

        var list = context.Web.Lists.GetByTitle("Tasks");

        var items = list.GetItems(CamlQuery.CreateAllItemsQuery());

        context.Load(items, collection => collection.Include(
            item => item.Id,
            item => item["Title"],
            item => item["Description"],
            item => item["Status"]
        ));

        context.ExecuteQuery();

        return items.ToList().Select(i => new
        {
            Id = i.Id,
            Title = i["Title"] != null ? i["Title"].ToString() : null,
            Description = i["Description"] != null ? i["Description"].ToString() : null,
            Status = i["Status"] != null ? i["Status"].ToString() : null
        }).Cast<object>().ToList();
    }
}