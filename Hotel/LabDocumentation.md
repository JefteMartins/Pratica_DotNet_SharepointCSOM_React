# SharePoint Technical Lab - Documentação Técnica

Este laboratório foi projetado para demonstrar e testar técnicas avançadas de interação com o SharePoint via CSOM (Client Side Object Model), focando em performance, escalabilidade e resiliência.

---

## 1. The Reading Lab (Leitura de Grandes Volumes)
Foca em como extrair dados de listas que excedem o limite de 5.000 itens (Threshold).

### A. Paginação Clássica (ListItemCollectionPosition)
Utiliza o token de paginação nativo do SharePoint para navegar entre páginas.
- **Técnica:** O CSOM retorna um objeto `ListItemCollectionPosition` após cada consulta. Esse token é enviado de volta na próxima requisição para indicar de onde o SharePoint deve continuar a leitura.
- **Vantagem:** Baixo consumo de memória no servidor e compatibilidade total com o modelo de objetos clássico.
- **Limitação:** Navegação sequencial (não permite pular para uma página específica sem o token anterior).

### B. RenderListDataAsStream (Stream)
A técnica mais moderna e performática para leitura.
- **Técnica:** Utiliza o método `RenderListDataAsStream` que retorna os dados em formato JSON diretamente do engine de renderização do SharePoint.
- **Vantagem:** Muito mais rápido que o `GetItems` tradicional, permite carregar metadados complexos e é menos propenso a falhas de threshold.
- **Uso:** Ideal para tabelas de alta performance e dashboards que precisam carregar muitos dados rapidamente.

---

## 2. The Writing Lab (Escrita de Dados)
Demonstra a diferença de performance entre operações individuais e operações em lote.

### A. Sequential Write
- **Como funciona:** Para cada item, o sistema faz um "Roundtrip" completo (Cria o item -> Update -> ExecuteQuery).
- **Impacto:** Extremamente lento para grandes volumes devido à latência de rede em cada chamada individual.

### B. Batched Write
- **Como funciona:** O sistema adiciona vários itens ao contexto do SharePoint (`list.AddItem`) e só chama o `ExecuteQueryRetryAsync` uma vez a cada lote (ex: a cada 50 itens).
- **Impacto:** Drástica redução no tempo total de execução. O SharePoint processa o lote de comandos em uma única transação de rede.

---

## 3. The Deletion Lab (Exclusão em Massa)
Foca na limpeza eficiente de dados utilizando queries precisas.

- **Query CAML:** Utiliza `CamlQuery` para identificar os itens que devem ser removidos.
- **Técnica de Exclusão:** Em vez de `DeleteObject()` (que é imediato e custoso), utiliza-se o `Recycle()`, que envia o item para a lixeira, sendo uma operação mais leve para o banco de dados do SharePoint.
- **Batching:** Assim como na escrita, as exclusões são processadas em lotes para minimizar as chamadas ao servidor.

---

## 4. The Resilience Lab (Resiliência e Throttling)
Testa a capacidade do sistema de se recuperar de falhas temporárias e limites de API (HTTP 429).

### Polly Integration
- **Estratégia:** Utiliza a biblioteca **Polly** para implementar uma política de **Retry com Exponential Backoff** e **Jitter**.
- **Funcionamento:** Se o SharePoint retornar um erro de "Too Many Requests" (429), o sistema espera um tempo (que aumenta exponencialmente) e tenta novamente, em vez de falhar imediatamente.

### Stress Toggle
- **Função:** Ativa um modo de "Erro Artificial". Quando ligado, o sistema simula falhas de Throttling nas primeiras tentativas para que você possa observar o Polly entrando em ação e recuperando a operação com sucesso nas tentativas seguintes.

---

## 5. The Search Lab (Busca Customizada)
Demonstra como construir filtros dinâmicos que respeitam os limites do SharePoint.

- **Filtros:** Combina critérios de Título (Contains), Status (Eq) e Datas (Geq/Leq).
- **Construção Dinâmica:** O código monta a árvore XML do CAML dinamicamente, aninhando operadores `<And>` conforme necessário.
- **Fallback de Threshold:** Se a busca falhar por causa do limite de 5.000 itens (campos não indexados), o lab demonstra uma estratégia de "Fallback", tentando recuperar os itens mais recentes e filtrando-os em memória como último recurso.

---

## 6. Data Management
O utilitário de suporte para o laboratório.

- **Provisionamento:** Cria automaticamente a lista `Tasks` no site de destino (`SiteLabUrl`) com as colunas e tipos de dados corretos.
- **Seeding:** Permite injetar 100, 1.000 ou 5.000 itens. O objetivo de injetar 5.000 itens é "quebrar" propositalmente as consultas CAML simples para forçar o uso das técnicas aprendidas nos outros labs.
