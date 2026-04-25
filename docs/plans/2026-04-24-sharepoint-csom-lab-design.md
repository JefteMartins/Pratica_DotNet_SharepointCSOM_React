# SharePoint CSOM Performance Lab - Design & Roadmap

## 🎯 Objective
Create a full-stack application (React + .NET Web API) designed specifically to practice and demonstrate advanced SharePoint CSOM techniques for handling large data volumes and ensuring resilience. This "Lab" will serve as a sandbox for an upcoming technical interview on Tuesday.
DON'T IMPLEMENT ANYTHING WITHOUT CONSENT

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

### 4. The Resilience Lab (Throttling & Backoff)
Focuses on making the application fault-tolerant against SharePoint Online throttling limits.
- **Feature:** A "Simulate Server Stress" toggle in the UI.
- **Implementation:** When enabled, the backend artificially simulates 429 Too Many Requests errors.
- **UI Demonstration:** Show the request failing in a naive implementation, and succeeding (with logged retries and delays) using the Polly Exponential Backoff implementation.

### 5. The Custom Search Lab (CAML Query Mastery)
Focuses on advanced filtering and dynamic query building.
- **Feature:** Dynamic filter builder (Title, Status, DueDate range).
- **Implementation:** Backend logic to construct complex CAML Queries safely.
- **UI Demonstration:** Show how CAML filters data on the server-side, reducing the payload size.

### 6. The Deletion Lab (Cleanup & Bulk Operations)
Focuses on efficient ways to remove data from SharePoint.
- **Scenario A:** Individual Recycle - Sending items to the bin one by one.
- **Scenario B:** Batched Recycle - Grouping multiple items in a single round-trip.
- **UI Demonstration:** Compare the time to clear 50 items using both methods.

---

## 🗓️ Roadmap to Tuesday

### Phase 1: Foundation (Today)
- [x] Initialize React frontend using Vite.
- [x] Setup basic API communication between React and the existing .NET backend.
- [x] Refactor `SharePointService.cs` to use interfaces for easier swapping of implementations.
- [x] Create the "Seed Data" endpoint to generate 5,000+ items in a test SharePoint list.

### Phase 2: The Reading Lab (Tomorrow)
- [x] Implement `ListItemCollectionPosition` logic.
- [x] Implement `RenderListDataAsStream` logic.
- [x] Build the UI comparison view for pagination.

### Phase 3: The Writing Lab (Sunday)
- [x] Implement naive item creation.
- [x] Implement CSOM batched item creation.
- [x] Build the UI to trigger and time bulk inserts.

### Phase 4: Resilience & Polish (Monday)
- [x] Integrate **Polly** into the SharePoint service.
- [x] Build the Throttling Simulator in the backend.
- [x] Add the "Simulate Stress" toggle to the React UI.
- [x] Review the code, ensuring best practices (using `using` blocks, avoiding unnecessary `.Include()` calls, etc.).

### Phase 5: Custom Search (Monday/Tuesday)
- [x] Implement dynamic CAML Query builder in the backend.
- [x] Build the Search UI with multiple filters.

### Phase 6: Deletion & Final Prep (Monday/Tuesday)
- [x] Implement sequential and batched recycle logic.
- [x] Build the Deletion UI.
- [x] Build Task Edit Modal for inline updates.
- [x] Dry run demonstrations using the dashboard.
- [ ] Review trade-offs for each technique.

---

## 🧠 Technical Trade-offs Summary (Interview Cheat Sheet)

### 1. Reading: Classic Paging vs. Stream API
| Método | Prós | Contras | Quando usar? |
| :--- | :--- | :--- | :--- |
| **ListItemCollectionPosition** | Estável, funciona em todas as versões de CSOM, fácil de tipar. | Mais lento que o Stream, carrega objetos CSOM pesados no servidor. | Listas menores ou quando compatibilidade máxima é exigida. |
| **RenderListDataAsStream** | **Extremamente rápido**, retorna JSON puro, ignora limites de exibição de lista (View Threshold). | Complexo de fazer o Parse manual do JSON, menos "tipado" no .NET. | **Mandatório** para listas com > 5.000 itens e alta performance. |

### 2. Writing/Deletion: Sequential vs. Batching
| Técnica | Impacto | Performance | Risco de Throttling |
| :--- | :--- | :--- | :--- |
| **Sequential (One-by-one)** | 1 Round-trip por item. | Baixa (Latência de rede acumula). | **Alto**. Muitas requisições pequenas disparam o 429 rápido. |
| **Batching (Grouping)** | 1 Round-trip por lote (ex: 50 itens). | **Altíssima**. Reduz o custo da latência de rede. | **Baixo**. Mais eficiente para o servidor processar um pacote grande. |

### 3. Resiliência: Polly vs. PnP Framework Default
*   **PnP Default:** Faz retries básicos, mas é difícil de customizar o log ou a estratégia.
*   **Polly (Wait & Retry + Exponential Backoff):** Permite o "Jitter" (variação aleatória no tempo) para evitar que vários clientes tentem o retry ao mesmo tempo (*Thundering Herd Problem*). Demonstra domínio de arquitetura resiliente.

### 4. Search: Client-side Filter vs. CAML Query
*   **Client-side:** Carrega tudo na RAM do navegador. Péssimo para > 100 itens.
*   **CAML (Server-side):** O SharePoint filtra no banco de dados SQL. Essencial para escala. **Dica:** O campo deve estar indexado no SharePoint para evitar erros de Threshold.
