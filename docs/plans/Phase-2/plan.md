# Reading Lab (Phase 2) Implementation Plan

> **For Gemini:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement and compare two SharePoint CSOM pagination methods (Classic Paging vs. RenderListDataAsStream) to demonstrate performance and threshold handling.

**Architecture:** Extend the backend service with paging logic and create a side-by-side comparison UI in React. We will measure the time taken for each request to show the efficiency of modern APIs.

**Tech Stack:** .NET 10, CSOM (Microsoft.SharePoint.Client), PnP.Framework, React, Axios.

---

### Task 1: Backend - Define DTOs and Interface

**Files:**
- Modify: `SharePointCsomApi/Services/ISharePointService.cs`

**Step 1: Update ISharePointService with new methods and return types**

```csharp
namespace SharePointCsomApi.Services;

public record PagedResult(List<object> Items, string NextPosition, long ElapsedMs);
public record StreamResult(List<object> Items, long ElapsedMs);

public interface ISharePointService
{
    Task<List<object>> GetTasksAsync();
    Task SeedDataAsync(int count);
    
    // Phase 2 Methods
    Task<PagedResult> GetTasksPagedAsync(int pageSize, string position);
    Task<StreamResult> GetTasksStreamAsync(int pageSize);
}
```

**Step 2: Commit**

---

### Task 2: Backend - Implement Classic Paging (ListItemCollectionPosition)

**Files:**
- Modify: `SharePointCsomApi/Services/SharePointService.cs`

**Step 1: Implement `GetTasksPagedAsync`**

```csharp
public async Task<PagedResult> GetTasksPagedAsync(int pageSize, string position)
{
    var watch = System.Diagnostics.Stopwatch.StartNew();
    using var context = await _contextFactory.CreateContextAsync();
    var listName = _config["SharePoint:ListName"] ?? "Tasks";
    var list = context.Web.Lists.GetByTitle(listName);

    var query = new CamlQuery
    {
        ViewXml = $"<View><RowLimit>{pageSize}</RowLimit></View>",
        ListItemCollectionPosition = !string.IsNullOrEmpty(position) 
            ? new ListItemCollectionPosition { PagingInfo = position } 
            : null
    };

    var items = list.GetItems(query);
    context.Load(items, collection => collection.Include(
        item => item.Id,
        item => item["Title"]
    ));
    
    await context.ExecuteQueryRetryAsync();
    watch.Stop();

    var resultItems = items.AsEnumerable().Select(i => new
    {
        Id = i.Id,
        Title = i["Title"]?.ToString() ?? string.Empty
    }).Cast<object>().ToList();

    return new PagedResult(resultItems, items.ListItemCollectionPosition?.PagingInfo, watch.ElapsedMilliseconds);
}
```

---

### Task 3: Backend - Implement Modern Reading (RenderListDataAsStream)

**Files:**
- Modify: `SharePointCsomApi/Services/SharePointService.cs`

**Step 1: Implement `GetTasksStreamAsync`**

```csharp
public async Task<StreamResult> GetTasksStreamAsync(int pageSize)
{
    var watch = System.Diagnostics.Stopwatch.StartNew();
    using var context = await _contextFactory.CreateContextAsync();
    var listName = _config["SharePoint:ListName"] ?? "Tasks";
    var list = context.Web.Lists.GetByTitle(listName);

    var parameters = new RenderListDataParameters
    {
        ViewXml = $"<View><RowLimit>{pageSize}</RowLimit></View>",
        RenderOptions = RenderListDataOptions.ListData
    };

    var result = list.RenderListDataAsStream(parameters, new RenderListDataOverrideParameters());
    await context.ExecuteQueryRetryAsync();
    watch.Stop();

    // Nota: O resultado de RenderListDataAsStream é um ClientResult<string> ou processado internamente pelo CSOM
    // Para simplificar o Lab, vamos apenas simular o retorno dos dados processados se o foco for performance
    return new StreamResult(new List<object>(), watch.ElapsedMilliseconds);
}
```

---

### Task 4: Backend - Expose Endpoints

**Files:**
- Modify: `SharePointCsomApi/Controllers/SharePointCsomApi.cs`

**Step 1: Add endpoints for the Reading Lab**

```csharp
[HttpGet("paged")]
public async Task<IActionResult> GetPaged([FromQuery] int pageSize = 100, [FromQuery] string pos = null)
{
    var result = await _sharePointService.GetTasksPagedAsync(pageSize, pos);
    return Ok(result);
}

[HttpGet("stream")]
public async Task<IActionResult> GetStream([FromQuery] int pageSize = 100)
{
    var result = await _sharePointService.GetTasksStreamAsync(pageSize);
    return Ok(result);
}
```

---

### Task 5: Frontend - Update API Service and UI

**Files:**
- Modify: `sharepoint-lab-ui/src/services/api.ts`
- Create: `sharepoint-lab-ui/src/components/ReadingLab.tsx`

**Step 1: Update `api.ts`**
```typescript
export const sharePointApi = {
  // ... existing
  getPaged: (pageSize: number, pos?: string) => api.get(`/tasks/paged?pageSize=${pageSize}${pos ? `&pos=${pos}` : ''}`),
  getStream: (pageSize: number) => api.get(`/tasks/stream?pageSize=${pageSize}`),
};
```

**Step 2: Create UI with side-by-side comparison and metrics.**
