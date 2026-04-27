# Design Plan: Project Unification (Hotel Management & SharePoint Lab)

**Date:** 2026-04-27  
**Status:** Validated  
**Objective:** Consolidate the `Hotel` and `SharepointLab` projects into a single "Management & Training" platform to simplify development, maintenance, and presentation.

---

## 🏗️ 1. Architecture & Project Structure

The unified project will adopt a single-solution "Platform" model to maximize code reuse and simplify deployment.

### Backend (`HotelAPI`)
- **Consolidation:** Merge `SharePointCsomApi` controllers and services into the `HotelAPI` project.
- **Infrastructure Layer:** Move common logic (ClientContext creation, PnP Framework extensions, Polly policies) to a shared namespace (e.g., `Hotel.Core` or `Hotel.Infrastructure`).
- **Unified Routing:**
    - `/api/hotel/*`: Operational business logic (Hotels, Rooms, Bookings).
    - `/api/lab/*`: Technical experiments, seed tasks, and performance benchmarks.

### Frontend (`HotelUI`)
- **Monolithic React App:** Merge `sharepoint-lab-ui` into `HotelUI`.
- **Top-Level Routing (`react-router-dom`):**
    - `/hotel/*`: The Hotel Management interface.
    - `/lab/*`: The SharePoint Technical Lab.
- **Shared Assets:** Consolidated Fluent UI v9 theme, icons, and global state providers.

---

## 🧱 2. Components & Data Flow

### Shared Service Layer
- **Base Service:** Introduction of `BaseSharePointService` to handle "PnP Ceremonies" (Context, Retries, ExecuteQuery).
- **Inheritance:** `HotelService` and `LabService` will inherit from this base, ensuring both the real app and the lab use the same resilience engine.

### UI Shell & Provider
- **Unified Provider:** A single React Context will manage SharePoint Site URL and connection state for both views.
- **Component Library:** Move generic components (Modals, Status Badges, DataGrids) to a shared folder to ensure UI consistency.

---

## 📈 3. Telemetry & Error Handling

### Unified Resilience Monitor
- **Interception:** Implement a shared layer to track Polly retry events and Circuit Breaker triggers.
- **Live Feedback:** A `ResilienceMonitor` component will show real-time "under-the-hood" activity (e.g., "Throttling detected, retrying in 2s...") regardless of whether the user is in the Hotel or Lab view.

### Performance Tracking
- **Metadata:** Every API response will include `x-performance-info` (Execution time, SP Round-trips).
- **Presentation Value:** This allows direct comparison between the "Lab findings" and the "Production application" performance.

---

## 🛠️ 4. Transition Plan

- [x] **Phase 1:** Port the `SharepointLab` backend services and controllers to `HotelAPI`.
- [x] **Phase 2:** Merge the `sharepoint-lab-ui` components and routes into `HotelUI`.
- [x] **Phase 3:** Standardize `appsettings.json` and `.env` to a single configuration set.
- [x] **Phase 4:** Decommission the original `SharepointLab/` directory.
