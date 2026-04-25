# SharePoint CSOM Performance Lab - Design & Roadmap

## 🎯 Objective
Create a full-stack application (React + .NET Web API) designed specifically to practice and demonstrate advanced SharePoint CSOM techniques for handling large data volumes and ensuring resilience. This "Lab" will serve as a sandbox for an upcoming technical interview on Tuesday.

## 🏗️ Architecture: The "Comparison Engine"

### Frontend (React + Vite)
- **UI Framework:** Fluent UI (to match SharePoint's look and feel) or Shadcn/UI for a clean, modern aesthetic.
- **Structure:** A dashboard layout with navigation to different "Labs".
- **Visuals:** Side-by-side comparisons of different API calls, displaying execution time, data loaded, and retry metrics.

### Backend (.NET Web API)
- **CSOM Connection:** Utilizing `PnP.Framework` and `Microsoft.SharePoint.Client`.
- **Resilience Layer:** Implementing **Polly** for Exponential Backoff and retry policies.
- **Middleware/Interceptor:** A custom component to simulate throttling (returning HTTP 429) when requested by the frontend.
- **Metrics:** Endpoints will return both the payload and execution metrics (Time elapsed, Retries hit).

## 🧪 The Labs

### 1. The Reading Lab (Large Data Volumes)
Focuses on bypassing the 5,000 item list view threshold and efficient memory usage.
- **Scenario A:** `ListItemCollectionPosition` - Classic CSOM paging.
- **Scenario B:** `RenderListDataAsStream` - Modern, performant REST-like querying.
- **UI Demonstration:** Compare the time taken to load page 3 of a 10,000 item list using both methods.

### 2. The Writing Lab (Batching & Optimization)
Focuses on minimizing network round-trips when updating or creating data.
- **Scenario A:** Naive loop (`ExecuteQuery` per item).
- **Scenario B:** CSOM Batching (Grouping operations into a single `ExecuteQuery`).
- **UI Demonstration:** Create 100 dummy items. Show the massive performance difference between doing it sequentially vs. batched.

### 3. The Resilience Lab (Throttling & Backoff)
Focuses on making the application fault-tolerant against SharePoint Online throttling limits.
- **Feature:** A "Simulate Server Stress" toggle in the UI.
- **Implementation:** When enabled, the backend artificially simulates 429 Too Many Requests errors.
- **UI Demonstration:** Show the request failing in a naive implementation, and succeeding (with logged retries and delays) using the Polly Exponential Backoff implementation.

---

## 🗓️ Roadmap to Tuesday

### Phase 1: Foundation (Today)
- [ ] Initialize React frontend using Vite.
- [ ] Setup basic API communication between React and the existing .NET backend.
- [ ] Refactor `SharePointService.cs` to use interfaces for easier swapping of implementations.
- [ ] Create the "Seed Data" endpoint to generate 5,000+ items in a test SharePoint list.

### Phase 2: The Reading Lab (Tomorrow)
- [ ] Implement `ListItemCollectionPosition` logic.
- [ ] Implement `RenderListDataAsStream` logic.
- [ ] Build the UI comparison view for pagination.

### Phase 3: The Writing Lab (Sunday)
- [ ] Implement naive item creation.
- [ ] Implement CSOM batched item creation.
- [ ] Build the UI to trigger and time bulk inserts.

### Phase 4: Resilience & Polish (Monday)
- [ ] Integrate **Polly** into the SharePoint service.
- [ ] Build the Throttling Simulator in the backend.
- [ ] Add the "Simulate Stress" toggle to the React UI.
- [ ] Review the code, ensuring best practices (using `using` blocks, avoiding unnecessary `.Include()` calls, etc.).

### Phase 5: Interview Prep (Tuesday)
- [ ] Dry run demonstrations using the dashboard.
- [ ] Review trade-offs for each technique.
