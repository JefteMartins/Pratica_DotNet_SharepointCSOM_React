# Hotel Management System - SharePoint CSOM Backend

## 🏨 Visão Geral
Sistema de gerenciamento hoteleiro completo utilizando SharePoint Online como camada de persistência (DB), uma API .NET como camada de lógica e resiliência, e React como interface administrativa.

## 🏗️ Arquitetura
- **Frontend:** React (TypeScript) + Fluent UI v9.
- **Backend:** .NET 8/10 Web API + PnP.Framework.
- **Database:** SharePoint Online Lists (Relacionamentos via campos de Lookup).
- **Resiliência:** Polly (Exponential Backoff + Circuit Breaker).

## 📊 Estrutura de Listas (Esquema SharePoint)

### 1. Lista: `Hotels`
- **Title:** Nome do Hotel.
- **Location:** Texto (Endereço).
- **Stars:** Number (1-5).
- **Description:** Multi-line text.

### 2. Lista: `Rooms`
- **Title:** Número/Nome do Quarto.
- **RoomType:** Choice (Standard, Deluxe, Suite, Presidential).
- **PricePerNight:** Currency.
- **HotelLookup:** Lookup (vinculado a `Hotels`).
- **Status:** Choice (Available, Occupied, Maintenance, Cleaning).

### 3. Lista: `Bookings` (Reservas)
- **Title:** Código da Reserva (Auto-gerado).
- **RoomLookup:** Lookup (vinculado a `Rooms`).
- **GuestName:** Texto.
- **CheckIn:** DateTime.
- **CheckOut:** DateTime.
- **TotalAmount:** Currency.
- **Status:** Choice (Confirmed, Cancelled, CheckedIn, CheckedOut).

## 🔌 Endpoints (API .NET)
- `GET /api/hotels`: Lista todos os hotéis.
- `GET /api/hotels/{id}/rooms`: Lista quartos de um hotel específico (via CAML Query filtrando o Lookup).
- `POST /api/bookings`: Cria uma reserva (com lógica de validação de disponibilidade no server-side).
- `PATCH /api/rooms/{id}/status`: Atualiza status do quarto (Check-in/Out).
- `GET /api/dashboard/stats`: Retorna métricas (Ocupação total, Receita prevista).

## 🎨 Design System (Frontend)
- **Tokens de Cor:** `colorBrandBackground` (Principal), `colorNeutralBackground1` (Cards), `colorPaletteGreenBackground3` (Disponível).
- **Tipografia:** `Manrope` ou `Segoe UI` para um ar corporativo/Microsoft.
- **Roundness:** `tokens.borderRadiusLarge` para um visual moderno e amigável.
- **Unidade Visual:** Uso consistente de `Card`, `Badge` para status e `DataGrid` para listas massivas.

## 🧱 Componentes Necessários
- **SidebarNavigation:** Navegação entre Hotéis, Reservas e Configurações.
- **HotelCard:** Visualização resumida do hotel.
- **RoomGrid:** Grade de quartos com filtros por status e tipo.
- **BookingForm:** Modal/Formulário para nova reserva com DatePicker.
- **StatusPill:** Badge customizado para estados (Available, Occupied, etc).
- **AvailabilityCalendar:** Calendário visual de ocupação (Gantt style simplificado).
