---
name: pnp-framework
description: Use when building .NET applications that interact with SharePoint Online using the PnP Framework library, including authentication and ClientContext management
---

# PnP Framework

## Overview
PnP Framework is a .NET library (Standard 2.0, .NET 8/9) for Microsoft 365 that serves as the cross-platform successor to PnP-Sites-Core, specifically supporting SharePoint Online. It provides modern authentication mechanisms and extensions over the standard CSOM.

## When to Use
- Interacting with SharePoint Online APIs in modern .NET applications (.NET Core, .NET 6/8/9).
- Authenticating with SharePoint using App-Only (Certificate), Interactive Login, or Access Tokens via Azure AD.
- Performing CRUD operations on SharePoint Lists, Libraries, or Sites.
- When migrating legacy code using `OfficeDevPnP.Core` or `SharePointPnPCoreOnline` to modern .NET.

## Core Pattern

### Authentication and Context Generation
Modern .NET applications should use `AuthenticationManager` to get a `ClientContext`. Always prefer `Async` methods to avoid blocking threads.

**App-Only (Certificate) - Recommended for Background Services/APIs:**
```csharp
using Microsoft.SharePoint.Client;
using PnP.Framework;

// 1. Initialize the AuthenticationManager with certificate details
var authManager = AuthenticationManager.CreateWithCertificate(
    clientId, 
    certificatePath, 
    certificatePassword, 
    tenantId
);

// 2. Get the ClientContext (Async is recommended)
using (var context = await authManager.GetContextAsync(siteUrl))
{
    context.Load(context.Web, w => w.Title);
    
    // 3. Always use ExecuteQueryRetryAsync for automatic throttling handling
    await context.ExecuteQueryRetryAsync();
    
    Console.WriteLine(context.Web.Title);
}
```

## Quick Reference
| Operation | Legacy Pattern | Modern PnP Framework Pattern |
|-----------|----------------|------------------------------|
| Authentication | `new ClientContext(url)` + credentials | `AuthenticationManager.CreateWith...` -> `GetContextAsync()` |
| Query Execution | `context.ExecuteQuery()` | `await context.ExecuteQueryRetryAsync()` |
| Throttling | Custom retry logic | Built-in via `ExecuteQueryRetryAsync()` |

## Implementation
When writing code using PnP Framework:
1. **Package:** Ensure `PnP.Framework` NuGet package is installed.
2. **Context Lifecycle:** Always wrap `ClientContext` in a `using` block as it implements `IDisposable`.
3. **Resilience:** Always use `.ExecuteQueryRetryAsync()` (extension method from PnP) instead of the basic `ExecuteQueryAsync()`. This automatically handles SharePoint HTTP 429 (Too Many Requests) throttling responses.
4. **Dependency Injection:** Consider registering `AuthenticationManager` as a Singleton in your DI container if using certificate authentication, as it does not hold state per-request, and use it to spawn `ClientContext` per request.

## Common Mistakes
- **Using Legacy Packages:** Using `SharePointPnPCoreOnline` instead of `PnP.Framework`.
- **Synchronous Calls:** Using `ExecuteQuery()` or `ExecuteQueryAsync()` without retry logic in ASP.NET Core, which can cause thread starvation and throttling failures. Always use `ExecuteQueryRetryAsync()`.
- **Memory Leaks:** Forgetting to `using` or `Dispose()` the `ClientContext`.
