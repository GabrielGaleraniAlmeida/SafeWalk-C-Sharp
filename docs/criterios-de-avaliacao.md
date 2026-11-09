# Relação com os critérios de avaliação

| Critério | Valor | Evidência na solução |
|---|---:|---|
| Programação Orientada a Objetos | 3,0 | Entidades de domínio encapsulam criação, confirmação, resolução, expiração e transições do SOS/jornada. Interfaces separam contrato e implementação. |
| Web API | 2,5 | Controllers REST para relatos, rotas, jornadas, SOS e painel municipal; validação, respostas HTTP e Swagger UI. |
| EF Core + LINQ | 1,5 | `SafeWalkDbContext`, provider SQLite, índices, repositórios assíncronos com consultas LINQ e agrupamento por categoria no painel. |
| Async/Await | 1,0 | Controllers, serviços e repositórios usam operações assíncronas e recebem `CancellationToken`. |
| Qualidade do código | 2,0 | Estrutura por responsabilidade, DI, DTOs validados, tratamento de erros, README, regras de privacidade e testes xUnit. |

## Como reproduzir

Consulte o [README](../README.md) para instalar o SDK, iniciar a aplicação e executar os testes. A pasta [evidencias](evidencias/README.md) contém respostas capturadas da API. Integrações com Smart Sampa, GCM e SP156 e dados de câmeras são simulados nesta versão.
