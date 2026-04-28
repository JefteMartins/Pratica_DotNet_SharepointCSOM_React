# 🏨 Hospitality Excellence & Technical Lab

A high-performance Hotel Management System integrated with **SharePoint Online**, featuring a specialized **Technical Lab** for CSOM performance benchmarking.

---

## 🌟 Overview

This project is a full-stack solution designed to demonstrate professional-grade integration between a modern web frontend and SharePoint Online. It serves two purposes:
1.  **Business Logic:** Managing a luxury hotel group's portfolio, room availability, and guest bookings.
2.  **Performance Engineering:** A dedicated "Technical Lab" to experiment with and document high-volume data handling in SharePoint.

## 🏗️ Architecture

-   **Frontend:** React (TypeScript) + Vite + Fluent UI (Microsoft Design System).
-   **Backend:** ASP.NET 10 Web API + C# 13.
-   **Infrastructure:** SharePoint Online via CSOM (Client-Side Object Model) & PnP Framework.
-   **Security:** App-Only Authentication using X.509 Certificates.

---

## 🚀 Key Features

### 🏨 Hotel Management
-   **Live Dashboard:** Real-time metrics (Total Hotels, Bookings, Active Guests, Revenue) calculated server-side.
-   **Luxury Portfolio:** Visual cards for hotel browsing with data sourced from SharePoint URL fields.
-   **Smart Room Search:** Advanced filtering by date (availability check), hotel, rating, and price.
-   **Booking Ledger:** Detailed history showing hotel names and room types through cross-list lookups.

### 🧪 Technical Lab (Performance Benchmarking)
A "playground" for developers to test SharePoint limits:
-   **Reading Lab:** Comparison between `ListItemCollectionPosition` (Paging) and `RenderListDataAsStream` (Modern Rendering).
-   **Writing Lab:** Benchmarking Sequential vs. Batched item creation.
-   **Deletion Lab:** Mass cleanup using CAML queries and the `Recycle()` method in batches.
-   **Resilience Lab:** Integration with **Polly** to handle Throttling (HTTP 429) with Exponential Backoff and Jitter.
-   **Search Lab:** Complex dynamic CAML construction with Threshold Fallback strategies.

---

## 🛠️ Technical Deep Dive

### 🔧 Dynamic Context Factory
The system implements a `SharePointContextFactory` capable of connecting to multiple site collections dynamically.
-   **Hotel Site:** Stores business data.
-   **Lab Site:** Isolated environment for stress testing.

### 🛡️ Resilience with Polly
To ensure reliability against SharePoint Online's throttling, we implement:
```csharp
.AddRetry(new RetryStrategyOptions {
    ShouldHandle = new PredicateBuilder().Handle<Exception>(ex => ex.Message.Contains("429")),
    MaxRetryAttempts = 3,
    BackoffType = DelayBackoffType.Exponential,
    UseJitter = true
})
```

### ⚡ Batching Optimization
We utilize CSOM's ability to group operations, reducing network roundtrips from `O(N)` to `O(N/BatchSize)`, drastically improving performance for mass data operations.

---

## ⚙️ Setup & Configuration

### 1. SharePoint Requirements
-   An Azure AD App Registration with `Sites.FullControl.All` (App-Only).
-   An X.509 Certificate (`.pfx`) stored locally.

### 2. Backend (AppAPI)
Update `appsettings.json`:
```json
"SharePoint": {
  "SiteUrl": "https://tenant.sharepoint.com/sites/Hotel",
  "SiteLabUrl": "https://tenant.sharepoint.com/sites/Lab",
  "TenantId": "your-guid",
  "ClientId": "your-guid",
  "CertificatePath": "C:\\path\\to\\cert.pfx",
  "CertificatePassword": "password"
}
```

### 3. Frontend (HotelUI)
Update `.env`:
```env
VITE_API_URL=https://localhost:7233/api
```

---

## 📖 Documentation
-   Detailed Lab techniques: [LabDocumentation.md](./Hotel/LabDocumentation.md)
-   Project Logs: [log.md](.//Hotel/log.md)

---

Developed with ❤️ for SharePoint Performance Engineering.
