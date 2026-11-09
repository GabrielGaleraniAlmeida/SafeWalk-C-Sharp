# SafeWalk API

Backend em C#/.NET da proposta CitizenTech SafeWalk, desenvolvida para a Global Solution FIAP. A API apoia caminhadas mais seguras em São Paulo com comparação de rotas, relatos colaborativos, acompanhamento temporário de trajeto e alertas SOS.

## O que a aplicação faz

- Compara duas a quatro alternativas de rota. Cada opção recebe uma pontuação de risco por trecho e uma classificação de risco; a resposta identifica a alternativa mais segura e a mais rápida.
- Recebe relatos anônimos sobre segurança pública, trânsito, alagamento, calçada e iluminação. Relatos recentes de segurança pública e trânsito expiram após algumas horas; os demais ficam ativos até serem resolvidos.
- Aumenta o peso de relatos confirmados por outras pessoas.
- Cria uma jornada com link temporário para compartilhar a localização atual. O link expira em quatro horas e deixa de funcionar quando a jornada é encerrada.
- Cria alertas SOS com PIN, avanço em etapas e cancelamento protegido pelo PIN.
- Fornece uma visão agregada dos relatos para o painel municipal.

O cálculo de risco é uma regra demonstrativa, não um modelo de ML treinado, e usa os relatos cadastrados nesta aplicação. A versão atual não carrega bases SSP-SP, Infosiga, GeoSampa ou CGE. A cobertura de câmera é informada como dado simulado. Smart Sampa, GCM e SP156 não são chamados por esta API; os estados desses fluxos são demonstrativos, como previsto para a demo do SafeWalk.

## Tecnologias

- C# e ASP.NET Core Web API em .NET 10.
- Entity Framework Core com SQLite.
- Swagger UI para documentação interativa.
- xUnit para testes unitários.

## Executar localmente

Requisitos: .NET SDK 10 instalado.

Na raiz do repositório, execute:

```powershell
dotnet restore .\SafeWalk.sln
dotnet build .\SafeWalk.sln
dotnet test .\SafeWalk.sln
dotnet run --project .\src\SafeWalk.Api\SafeWalk.Api.csproj
```

Abra `http://localhost:5080/swagger` para explorar a API. O endpoint `http://localhost:5080/health` confirma que o serviço está ativo.

Na primeira inicialização, a aplicação cria o banco SQLite em `data/safewalk.db` e inclui três relatos demonstrativos perto do centro de São Paulo. O caminho pode ser alterado em `Storage:DatabasePath` no `appsettings.json` ou pela variável `Storage__DatabasePath`.

## Endpoints

| Método | Endpoint | Uso |
|---|---|---|
| `GET` | `/health` | Verifica se a API está ativa. |
| `GET` | `/api/reports?latitude=-23.55&longitude=-46.63&radiusKm=0.5` | Lista relatos ativos próximos. |
| `GET` | `/api/reports/{id}` | Consulta um relato. |
| `POST` | `/api/reports` | Cria um relato anônimo. |
| `POST` | `/api/reports/{id}/confirm` | Confirma um relato ativo. |
| `POST` | `/api/reports/{id}/resolve` | Marca um problema como resolvido. |
| `POST` | `/api/routes/compare` | Calcula risco para alternativas de rota enviadas pelo app. |
| `POST` | `/api/journeys` | Inicia acompanhamento e retorna tokens de controle e compartilhamento. |
| `PUT` | `/api/journeys/{id}/location` | Atualiza a localização com o token do viajante. |
| `GET` | `/api/journeys/shared/{shareToken}` | Consulta a localização pelo link compartilhado. |
| `POST` | `/api/journeys/{id}/complete` | Encerra a jornada e remove a localização armazenada. |
| `POST` | `/api/sos-alerts` | Abre um alerta SOS com PIN. |
| `GET` | `/api/sos-alerts/{id}` | Consulta o estado do alerta. |
| `POST` | `/api/sos-alerts/{id}/advance` | Avança uma etapa do fluxo demonstrativo. |
| `POST` | `/api/sos-alerts/{id}/cancel` | Cancela um alerta ativo usando o PIN. |
| `GET` | `/api/dashboard/reports` | Resume relatos por período e categoria. |

### Exemplo: criar um relato

```http
POST /api/reports HTTP/1.1
Host: localhost:5080
Content-Type: application/json

{
  "category": 1,
  "description": "Relato de roubo de celular perto do cruzamento.",
  "latitude": -23.5509,
  "longitude": -46.6342
}
```

Categorias: `1` segurança pública, `2` trânsito, `3` alagamento, `4` calçada e `5` iluminação. A API não coleta nome do autor e não o inclui na resposta pública.

### Exemplo: comparar rotas

```http
POST /api/routes/compare HTTP/1.1
Host: localhost:5080
Content-Type: application/json

{
  "routes": [
    {
      "name": "Mais rápida",
      "durationMinutes": 14,
      "segments": [
        { "latitude": -23.5509, "longitude": -46.6342, "simulatedCameraCoverage": false }
      ]
    },
    {
      "name": "Mais segura",
      "durationMinutes": 17,
      "segments": [
        { "latitude": -23.5488, "longitude": -46.6350, "simulatedCameraCoverage": true },
        { "latitude": -23.5480, "longitude": -46.6320, "simulatedCameraCoverage": false }
      ]
    }
  ]
}
```

O app fornece a geometria das alternativas; a API estima o risco com relatos ativos a até 500 metros de cada trecho. O cálculo inicial usa pesos por categoria, proximidade e confirmações. Quando informado, o indicador simulado de câmera reduz a pontuação em 12%. A pontuação vai de 0 a 100 e deve ser entendida como demonstração acadêmica, não como garantia de segurança.

### Jornada compartilhada

1. `POST /api/journeys` com latitude e longitude atuais.
2. Guarde o `trackerToken` no app e envie-o em `PUT /api/journeys/{id}/location` para atualizar a posição.
3. Compartilhe o `shareUrl` retornado com os contatos. O link público mostra somente a posição atual e os horários da sessão.
4. `POST /api/journeys/{id}/complete` com o token do viajante encerra a sessão e apaga posição e hashes dos tokens.

Os tokens são aleatórios e somente seus hashes ficam no banco. O `trackerToken` deve ser mostrado e guardado apenas pelo app; não o publique em evidências ou registros.

### Alerta SOS

`POST /api/sos-alerts` recebe localização e PIN numérico de 4 a 8 dígitos. `POST /{id}/advance` simula a etapa de notificação dos contatos e, na etapa seguinte, o acionamento da central. O endpoint de cancelamento exige o PIN. O fluxo não envia mensagens nem aciona a GCM de verdade. O PIN é armazenado com salt e PBKDF2-HMAC-SHA256.

## Estrutura e conceitos da disciplina

| Critério | Onde aparece |
|---|---|
| Programação Orientada a Objetos | Entidades `SafetyReport`, `Journey` e `SosAlert` encapsulam regras e transições de estado. |
| Interfaces | Serviços e repositórios têm contratos próprios em `Application/Abstractions`. |
| Injeção de dependência | `Program.cs` registra implementações com ciclo de vida explícito. O relógio é abstraído para facilitar testes. |
| Entity Framework Core | `SafeWalkDbContext` mapeia as entidades para SQLite e define índices. |
| LINQ | Repositórios filtram e ordenam registros no banco; os serviços agregam relatos por categoria. |
| APIs REST | Controllers organizam operações por recurso, com validação e códigos HTTP. |
| Async/await | Acesso ao banco e operações de serviço recebem `CancellationToken` e usam APIs assíncronas do EF Core. |
| Testes unitários | xUnit verifica expiração e confirmação de relatos, risco de rota, fluxo SOS e privacidade da jornada. |

O banco é criado com `EnsureCreated` para facilitar a demonstração local. Em um ambiente de produção, o schema deve ser gerenciado por migrations e o acesso à API precisa de autenticação e autorização de usuários.

## Testes

```powershell
dotnet test .\SafeWalk.sln
```

Os testes usam relógio e repositórios falsos; não dependem de rede ou do banco SQLite local.

## Publicar no GitHub

Na página do GitHub, crie um repositório vazio para o SafeWalk. Depois, na raiz desta pasta:

```powershell
git init
git add .
git commit -m "Entrega SafeWalk API"
git branch -M main
git remote add origin <URL_DO_REPOSITORIO>
git push -u origin main
```

Envie o link desse repositório na plataforma da disciplina. O workflow em `.github/workflows/dotnet.yml` executa restore, build e testes a cada push e pull request.

## Evidências

Os arquivos em [`docs/evidencias`](docs/evidencias/README.md) são respostas capturadas da API em execução. Para gerar novamente, inicie a API em um terminal e rode em outro:

```powershell
.\scripts\gerar-evidencias.ps1
```

O script salva respostas de saúde, relatos, comparação de rotas, localização compartilhada, SOS e painel. Tokens de controle e links ativos são removidos antes de salvar as evidências.
