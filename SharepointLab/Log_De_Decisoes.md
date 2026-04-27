# [Data: 25/04/2026]

- [10:05] **Ação:** Implementação do método `SeedDataAsync` no `SharePointService.cs`.
  - **Por que:** Para o Reading Lab, precisamos de um volume grande de dados (5.000+ itens). Implementar o seed agora permite avançar para a Fase 2. **Boa prática:** O uso de batching (lotes de 100) evita timeouts e sobrecarga no `ExecuteQuery`, garantindo que o processo seja resiliente em ambientes com grandes volumes.
  
- [10:15] **Ação:** Criação da Skill `tutor-lab` e do documento `Log_De_Decisoes.md`.
  - **Por que:** Transformar o projeto em um laboratório de aprendizado ativo. O log serve como rastreabilidade técnica e pedagógica, essencial para revisão pré-entrevista. **Boa prática:** Documentar o "porquê" de decisões arquiteturais é fundamental em projetos de longo prazo para evitar "débito de conhecimento".

- [10:20] **Ação:** Configuração de política de CORS no `Program.cs`.
  - **Por que:** Permitir que o frontend React (localhost:5173) consuma a API .NET. **Boa prática:** Restringir as origens permitidas em vez de usar `AllowAnyOrigin()` para mitigar ataques de Cross-Site Request Forgery (CSRF).

- [10:50] **Ação:** Instalação do Axios e criação do serviço base de API no frontend.
  - **Por que:** Centralizar a comunicação com o backend. **Boa prática:** O uso de instâncias do Axios (`axios.create`) permite gerenciar headers, tokens e URLs base em um único lugar, facilitando a manutenção e testes.

- [11:30] **Ação:** Implementação de DTOs usando `record` e inclusão de métricas de performance no backend.
  - **Por que:** Para o Lab de Performance, o tempo de execução no servidor (`ElapsedMs`) é o KPI principal. **Boa prática:** Usar `record` em C# 10+ para DTOs garante imutabilidade e sintaxe concisa para transferência de dados.

- [11:45] **Ação:** Implementação de `RenderListDataAsStream` como alternativa ao `GetItems` (CSOM).
  - **Por que:** Demonstrar conhecimento de APIs modernas do SharePoint Online. **Boa prática:** `RenderListDataAsStream` é otimizado para listas grandes, retornando JSON bruto e ignorando limites de threshold de forma mais eficiente que queries CAML tradicionais.

- [12:15] **Ação:** Adoção do Fluent UI v9 no Frontend React.
  - **Por que:** Alinhamento visual com o ecossistema Microsoft 365. **Boa prática:** Usar bibliotecas de componentes oficiais (como Fluent UI) garante consistência de UX em aplicações corporativas e demonstra senioridade no stack da Microsoft.

- [12:30] **Ação:** Implementação de paginação baseada em tokens (`ListItemCollectionPosition`).
  - **Por que:** No SharePoint, paginação estilo SQL (`Skip/Take`) não é performática para grandes volumes. **Boa prática:** O uso de tokens de posição ("marcador de livro") permite que o servidor salte direto para o ponto de continuação sem reprocessar itens anteriores, essencial para listas com mais de 5.000 itens.

- [13:15] **Ação:** Implementação do Writing Lab com comparação entre Sequential e Batched.
  - **Por que:** Demonstrar como reduzir round-trips de rede. **Boa prática:** O batching no CSOM (vários `Update()` antes de um único `ExecuteQuery`) é a técnica mais eficaz para inserções massivas, reduzindo a latência acumulada e o risco de throttling.

- [13:30] **Ação:** Refinamento do esquema de escrita e tratamento de erro de esquema.
  - **Por que:** Identificado que campos inexistentes ou de tipos incompatíveis (ex: Status como Choice) causam falhas silenciosas no CSOM. **Boa prática:** Sempre validar o Internal Name dos campos e envolver operações de escrita em blocos try-catch específicos para `ServerException` para diagnósticos precisos.

- [14:00] **Ação:** Implementação de resiliência com Polly (v8) e simulador de Throttling.
  - **Por que:** SharePoint Online é agressivo com Throttling (429). **Boa prática:** O uso de Exponential Backoff com Jitter protege a aplicação e o servidor, permitindo que falhas transitórias sejam resolvidas sem intervenção do usuário.

- [14:30] **Ação:** Construção dinâmica de CAML Queries para busca customizada.
  - **Por que:** Filtragem no servidor é mandatória para performance em listas grandes. **Boa prática:** Ao construir CAML dinamicamente, é necessário respeitar a estrutura aninhada de `<And>` do SharePoint (que aceita apenas 2 argumentos por nó), garantindo que a query seja válida e performática.

