# [Data: 25/04/2026]

- [10:05] **Ação:** Implementação do método `SeedDataAsync` no `SharePointService.cs`.
  - **Por que:** Para o Reading Lab, precisamos de um volume grande de dados (5.000+ itens). Implementar o seed agora permite avançar para a Fase 2. **Boa prática:** O uso de batching (lotes de 100) evita timeouts e sobrecarga no `ExecuteQuery`, garantindo que o processo seja resiliente em ambientes com grandes volumes.
  
- [10:15] **Ação:** Criação da Skill `tutor-lab` e do documento `Log_De_Decisoes.md`.
  - **Por que:** Transformar o projeto em um laboratório de aprendizado ativo. O log serve como rastreabilidade técnica e pedagógica, essencial para revisão pré-entrevista. **Boa prática:** Documentar o "porquê" de decisões arquiteturais é fundamental em projetos de longo prazo para evitar "débito de conhecimento".

- [10:20] **Ação:** Configuração de política de CORS no `Program.cs`.
  - **Por que:** Permitir que o frontend React (localhost:5173) consuma a API .NET. **Boa prática:** Restringir as origens permitidas em vez de usar `AllowAnyOrigin()` para mitigar ataques de Cross-Site Request Forgery (CSRF).

- [10:50] **Ação:** Instalação do Axios e criação do serviço base de API no frontend.
  - **Por que:** Centralizar a comunicação com o backend. **Boa prática:** O uso de instâncias do Axios (`axios.create`) permite gerenciar headers, tokens e URLs base em um único lugar, facilitando a manutenção e testes.

- [11:00] **Ação:** Refatoração do `SharePointContextFactory.cs` para usar autenticação baseada em certificado (X.509).
  - **Por que:** Alinhamento com as práticas recomendadas de segurança do Azure/SharePoint Online, substituindo o Client Secret por certificados `.pfx`. **Boa prática:** O uso de certificados é mais seguro contra vazamentos, pois exige a posse do arquivo físico e, opcionalmente, uma senha para descriptografar a chave privada.

