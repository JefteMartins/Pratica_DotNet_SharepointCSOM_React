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
- `GET /api/rooms`: Retorna todos os quartos com filtros de disponibilidade.
- `GET /api/hotels/{id}/rooms`: Busca quartos de um hotel específico via CAML Query.
- `GET /api/bookings`: Retorna todas as reservas.
- `POST /api/bookings`: Registra uma nova reserva (inclui validação de disponibilidade no servidor).
- `PATCH /api/rooms/{id}/status`: Atualiza o estado de um quarto.
- `GET /api/dashboard/stats`: Retorna métricas agregadas.

### Administrativo
- `POST /api/admin/provision`: Verifica e cria a estrutura de listas no SharePoint.
- `POST /api/admin/seed`: Popula o ambiente com dados iniciais e imagens reais.

---

## 🛡️ Estratégia de Resiliência e Integridade

### 1. Prevenção de Concorrência (Race Conditions)
O sistema não confia apenas na verificação de disponibilidade do Frontend. Ao tentar criar uma reserva via `POST /api/bookings`, o Backend executa uma **CAML Query de Intersecção**:
```xml
(RequestedStart < ExistingEnd) AND (RequestedEnd > ExistingStart)
```
Se qualquer conflito for encontrado no SharePoint antes da persistência, a API retorna um `400 Bad Request`, garantindo que um quarto nunca seja reservado duas vezes para o mesmo período.

### 2. Camada de Retentativa (PnP Framework - Otimista)
Utilizamos o método `ExecuteQueryRetryAsync` como nossa primeira linha de defesa contra falhas transientes de rede e HTTP 429/503.

### 3. Camada de Disjuntor (Polly - Circuit Breaker - Defensiva)
Monitora a saúde holística da conexão. Se 50% das requisições falharem em uma janela de 30 segundos, o disjuntor "abre", impedindo novas chamadas por 30 segundos para permitir o cooldown do tenant.

---

## 🏗️ Padrões de Implementação
- **Check-and-Create:** O provisionamento verifica a existência prévia de ativos via extensões PnP, garantindo idempotência.
- **Modelos de Dados:** Utilização de classes C# com propriedades automáticas para compatibilidade total com JSON Serialization.
- **Validação de Negócio:** Centralizada na camada de serviço, protegendo o SharePoint contra estados inconsistentes (ex: Check-Out < Check-In).
