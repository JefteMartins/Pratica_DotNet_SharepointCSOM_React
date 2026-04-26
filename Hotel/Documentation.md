# Documentação Técnica - Hotel Management System

## 🏨 Visão Geral
Este projeto é um sistema de gestão hoteleira que utiliza o SharePoint Online como banco de dados NoSQL/Relacional, aproveitando o CSOM para persistência e o Fluent UI v9 para uma interface administrativa moderna.

---

## 🏗️ Arquitetura do Sistema

### 1. Backend (HotelAPI)
- **Framework:** .NET 10 (ASP.NET Core Web API).
- **Persistência:** SharePoint Online via `PnP.Framework` (CSOM).
- **Resiliência:** `Polly` para tratamento de Throttling (HTTP 429) e falhas transientes.
- **Autenticação:** App-Only com Certificado Digital (X509Certificate2).

### 2. Frontend (HotelUI)
- **Framework:** React + TypeScript.
- **Design System:** Fluent UI v9 (Microsoft).
- **Comunicação:** Axios.
- **Navegação:** React Router v6.

---

## 📊 Estrutura de Dados (SharePoint)

As listas são criadas automaticamente via `SharePointProvisioningService`.

### Lista: `Hotels`
| Coluna | Tipo | Descrição |
| :--- | :--- | :--- |
| Title | Text | Nome do Hotel (Campo nativo) |
| Location | Multi-line Text | Endereço completo |
| Stars | Number | Classificação (1-5) |
| ImageUrl | Hyperlink | URL da imagem de capa (Placeholder) |
| Description | Multi-line Text | Detalhes do hotel |

### Lista: `Rooms`
| Coluna | Tipo | Descrição |
| :--- | :--- | :--- |
| Title | Text | Número/Identificador do Quarto |
| RoomType | Choice | Standard, Deluxe, Suite, Presidential |
| PricePerNight | Currency | Valor da diária |
| HotelLookup | Lookup | Vinculado à lista `Hotels` |
| Status | Choice | Available, Occupied, Maintenance, Cleaning |

### Lista: `Bookings`
| Coluna | Tipo | Descrição |
| :--- | :--- | :--- |
| Title | Text | Código da Reserva (ex: BK-A1B2C3D4) |
| RoomLookup | Lookup | Vinculado à lista `Rooms` |
| GuestName | Text | Nome do Hóspede |
| CheckIn | DateTime | Data de entrada |
| CheckOut | DateTime | Data de saída |
| TotalAmount | Currency | Valor total da reserva |
| Status | Choice | Confirmed, Cancelled, CheckedIn, CheckedOut |

---

## 🔌 API Endpoints

### Hotéis e Operações
- `GET /api/hotels`: Retorna todos os hotéis.
- `GET /api/hotels/{id}/rooms`: Busca quartos de um hotel específico via CAML Query.
- `POST /api/bookings`: Registra uma nova reserva.
- `PATCH /api/rooms/{id}/status`: Atualiza o estado de um quarto.
- `GET /api/dashboard/stats`: Retorna métricas agregadas.

### Administrativo
- `POST /api/admin/provision`: Verifica e cria a estrutura de listas no SharePoint.

---

## 🛡️ Estratégia de Resiliência

O sistema utiliza uma abordagem de **defesa em camadas**:

1. **Camada de Retentativa (PnP Framework):** O método `ExecuteQueryRetryAsync` gerencia retentativas automáticas para erros transientes de rede e Throttling (429/503) de forma granular por requisição.
2. **Camada de Interrupção (Polly - Circuit Breaker):** Implementado no `SharePointService`, o disjuntor monitora a saúde geral da conexão. Se a taxa de falhas atingir 50% em 30 segundos, o disjuntor "abre", impedindo novas chamadas por 30 segundos. Isso protege o sistema de loops de espera infinitos quando o serviço está indisponível.

---
