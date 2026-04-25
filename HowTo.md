# Guia de Operações SharePoint CSOM (PnP Framework)

Este guia explica como realizar operações básicas (CRUD) no SharePoint de forma correta, utilizando o padrão recomendado com **PnP Framework**.

## Conceitos Fundamentais

*   **Contexto (`ClientContext`):** É o túnel de comunicação entre sua aplicação e o SharePoint. Todas as operações precisam dele.
*   **Load:** O CSOM trabalha com "carregamento diferido". Você avisa o que quer (`context.Load`), mas os dados só chegam após a execução.
*   **ExecuteQueryRetryAsync:** O motor que envia as instruções e gerencia falhas de rede ou limites de requisição (*throttling*).

---

## Exemplos de Operações (Pseudocódigo C#)

```csharp
// 1. OBTENDO O CONTEXTO (Setup Inicial)
// Sempre use 'using' para garantir que a conexão seja fechada corretamente
using (var context = await _contextFactory.CreateContextAsync())
{
    // --- OPERAÇÃO: GET (Leitura) ---
    // Passo A: Referenciar a lista
    var list = context.Web.Lists.GetByTitle("MinhaLista");
    
    // Passo B: Filtrar os dados (Opcional, mas recomendado usar CamlQuery para performance)
    var items = list.GetItems(CamlQuery.CreateAllItemsQuery());

    // Passo C: O "Load" é obrigatório. Sem ele, o objeto 'items' ficará vazio.
    // É aqui que você especifica quais campos quer trazer para economizar banda.
    context.Load(items, i => i.Include(item => item["Title"], item => item["ID"]));

    // Passo D: Enviar para o servidor com lógica de retentativa
    await context.ExecuteQueryRetryAsync();


    // --- OPERAÇÃO: POST (Criação) ---
    // Passo A: Preparar as informações do novo item
    ListItemCreationInformation itemInfo = new ListItemCreationInformation();
    ListItem newItem = list.AddItem(itemInfo);

    // Passo B: Atribuir valores aos campos
    newItem["Title"] = "Novo Registro";
    newItem["Descricao"] = "Criado via CSOM";

    // Passo C: Notificar o contexto que este item deve ser atualizado no servidor
    newItem.Update();

    // Passo D: Executar a chamada
    await context.ExecuteQueryRetryAsync();


    // --- OPERAÇÃO: UPDATE (Atualização) ---
    // Passo A: Obter o item existente pelo ID
    var itemToUpdate = list.GetItemById(1);

    // Passo B: Alterar os valores desejados
    itemToUpdate["Title"] = "Título Alterado";

    // Passo C: IMPORTANTE - Você deve chamar o .Update() no objeto para marcar a mudança
    itemToUpdate.Update();

    // Passo D: Executar
    await context.ExecuteQueryRetryAsync();


    // --- OPERAÇÃO: DELETE (Exclusão) ---
    // Passo A: Obter a referência do item
    var itemToDelete = list.GetItemById(2);

    // Passo B: Marcar para exclusão
    // DeleteObject() remove permanentemente ou Recycle() envia para a lixeira
    itemToDelete.Recycle(); 

    // Passo C: Executar para efetivar a remoção no SharePoint
    await context.ExecuteQueryRetryAsync();
}
```

## Checklist para o "Jeito Certo"

1.  **Sempre use `ExecuteQueryRetryAsync`**: Nunca use o `ExecuteQuery` simples em nuvem (SharePoint Online).
2.  **Seletividade no Load**: No `context.Load()`, peça apenas as colunas que você realmente vai usar.
3.  **Lógica de Batch**: O SharePoint acumula comandos no contexto. Você pode fazer vários `Update()` e um único `ExecuteQueryRetryAsync()` no final para melhorar a performance.
4.  **Tratamento de Erros**: Sempre envolva as operações em `try-catch` para capturar `ServerException` (erros de permissão, lista inexistente, etc).
