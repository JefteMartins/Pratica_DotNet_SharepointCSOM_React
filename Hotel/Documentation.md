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

## 🛡️ Estratégia de Resiliência (Deep Dive)

O sistema implementa o padrão **Resilience in Depth**, combinando duas camadas complementares para garantir estabilidade mesmo sob alta carga ou instabilidade do SharePoint Online.

### 1. Camada de Retentativa (PnP Framework - Otimista)
Utilizamos o método `ExecuteQueryRetryAsync` como nossa primeira linha de defesa.
- **Funcionamento:** Age de forma granular em cada requisição individual.
- **Estratégia:** Se o SharePoint retornar um erro transiente (como interrupção de rede ou HTTP 429/503), o PnP realiza retentativas rápidas com backoff incremental (1s, 2s, 5s...).
- **Objetivo:** Resolver falhas momentâneas sem que o usuário perceba.

### 2. Camada de Disjuntor (Polly - Circuit Breaker - Defensiva)
Implementado como um `static readonly` no `SharePointService`, o disjuntor monitora a saúde holística da API.
- **Configuração de Falha (`FailureRatio`):** Se 50% das requisições falharem em uma janela de 30 segundos, o disjuntor "abre".
- **Estado Aberto (`BreakDuration`):** Durante 30 segundos, todas as chamadas ao SharePoint são bloqueadas **imediatamente** no nível da API.
- **Motivação dos 30 Segundos:** Este intervalo é crítico para:
    - **Cooldown do SharePoint:** Evita que a aplicação continue "bombardeando" o tenant durante um Throttling agressivo, o que poderia estender a punição.
    - **UX Responsiva:** Em vez de deixar o usuário esperando um timeout de rede de 60s, a API retorna um erro imediato, permitindo que o Frontend informe que o sistema está em "modo de recuperação".
    - **Recuperação de Infra:** Tempo suficiente para que falhas de roteamento ou failovers de serviço da Microsoft se estabilizem.

### Sinergia Técnica
Enquanto o **PnP** tenta consertar pequenas rachaduras, o **Polly** garante que, se a barragem romper, a aplicação não desperdice recursos tentando o impossível, preservando a integridade do servidor e a clareza para o usuário final.

---
