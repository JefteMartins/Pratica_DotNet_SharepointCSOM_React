namespace SharePointCsomApi.Services;

public interface ISharePointService
{
    /// <summary>
    /// Recupera tarefas da lista de forma básica (CSOM padrão).
    /// </summary>
    Task<List<object>> GetTasksAsync();

    /// <summary>
    /// Endpoint para popular a lista com dados de teste para o Lab.
    /// </summary>
    /// <param name="count">Quantidade de itens a gerar.</param>
    Task SeedDataAsync(int count);
}
