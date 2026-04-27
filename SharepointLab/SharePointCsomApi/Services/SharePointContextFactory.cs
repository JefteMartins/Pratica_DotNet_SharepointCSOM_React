using System.Security.Cryptography.X509Certificates;
using Microsoft.SharePoint.Client;
using PnP.Framework;

namespace SharePointCsomApi.Services;

public interface ISharePointContextFactory
{
    Task<ClientContext> CreateContextAsync();
}

public class SharePointContextFactory : ISharePointContextFactory
{
    private readonly IConfiguration _config;

    public SharePointContextFactory(IConfiguration config)
    {
        _config = config;
    }

    public async Task<ClientContext> CreateContextAsync()
    {
        var siteUrl = _config["SharePoint:SiteUrl"]
            ?? throw new Exception("SiteUrl não configurado");

        var clientId = _config["SharePoint:ClientId"]
            ?? throw new Exception("ClientId não configurado");

        var tenantId = _config["SharePoint:TenantId"]
            ?? throw new Exception("TenantId não configurado");

        var certificatePath = _config["SharePoint:CertificatePath"]
            ?? throw new Exception("CertificatePath não configurado");

        var certificatePassword = _config["SharePoint:CertificatePassword"]
            ?? throw new Exception("CertificatePassword não configurado");

        var certificate = new X509Certificate2(
            certificatePath,
            certificatePassword
        );

        var authManager = new AuthenticationManager(
            clientId,
            certificate,
            tenantId
        );

        return authManager.GetContext(siteUrl);
    }
}
