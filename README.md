# Desafio Engeneharia Juvo

Bem vindo(a) a etapa de entrevista técnica da Juvo. A seguir você terá uma descrição sobre esse projeto e uma breve explicação sobre o setup básico. Após o setup temos a seção com o desafio. 
Boa sorte e nos vemos na entrevista!

## O que é este projeto? 

Serviço de contas responsável por acompanhar o resultado de verificações de biometria facial feitas em um provedor externo.

## Como utilizar o projeto

### Pré-requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- `curl` (no Windows, use `curl.exe` no PowerShell para não cair no alias `Invoke-WebRequest`)

### Compilar e executar

Na raiz do projeto (pasta onde está o `Account.sln`):

```bash
dotnet build Account.sln
dotnet run --project source/Account.Api
```

A API sobe em `http://localhost:5080`. Todos os dados ficam em memória e são perdidos ao reiniciar a aplicação.

### Chamadas HTTP

**Processar uma verificação de biometria**

```bash
curl -X POST http://localhost:5080/api/accounts/biometrics/verifications \
  -H "Content-Type: application/json" \
  -d '{"reference":"verification-approved-1"}'
```

**Consultar o estado de uma verificação**

```bash
curl http://localhost:5080/api/accounts/biometrics/verifications/verification-approved-1
```

No PowerShell, o corpo JSON precisa de aspas escapadas:

```powershell
curl.exe -X POST http://localhost:5080/api/accounts/biometrics/verifications `
  -H "Content-Type: application/json" `
  -d '{\"reference\":\"verification-approved-1\"}'

curl.exe http://localhost:5080/api/accounts/biometrics/verifications/verification-approved-1
```

Exemplo de resposta:

```json
{
  "id": "645948a4-3076-4121-919e-98c60cd6756a",
  "reference": "verification-approved-1",
  "status": "Approved",
  "resultDescription": null,
  "createDate": "2026-10-20T14:13:39.5061104Z",
  "updatedAt": "2026-10-20T14:13:39.5124870Z"
}
```

### Provedor simulado

Não existe integração real. O provedor em memória decide a resposta pelo texto contido na referência:

| A referência contém | O provedor responde |
| --- | --- |
| `approved` | `APPROVED` |
| `rejected` | `REJECTED` |
| `pending` | `PENDING` |
| qualquer outro texto | `PENDING` |

Exemplos de referências: `verification-approved-1`, `verification-rejected-2`, `verification-pending-3`.

## Contexto

O Account Service precisa consultar o resultado de uma verificação de biometria facial em um provedor externo. A API do provedor retorna um dos seguintes estados:

- `PENDING`: verificação em andamento.
- `APPROVED`: verificação aprovada.
- `REJECTED`: verificação reprovada.

## O desafio

### Etapa 1 — Processar o resultado

Implemente o componente que interpreta a resposta do provedor, persiste o resultado e permite que o sistema consulte o estado da verificação.

Ao receber a referência de uma verificação, o serviço deve:

1. Consultar o provedor por meio da interface já disponibilizada.
2. Converter a resposta externa para o modelo interno do domínio.
3. Persistir o estado usando o repositório.
4. Retornar o estado atualizado.

Depois de processada, a mesma referência deve ser consultável pelo endpoint de consulta, com o estado que foi persistido.

### Etapa 2 — Verificações pendentes

Algumas consultas ao provedor retornam `PENDING`, pois a verificação ainda está em processamento. A conclusão acontece de forma assíncrona e pode ficar disponível somente em uma consulta posterior.

Evolua a solução para que uma verificação pendente possa ser consultada novamente até atingir um estado final, sem manter a requisição HTTP original aberta durante todo o processamento.

Explicar o raciocínio e os trade-offs conta tanto quanto o código.

## Estrutura do projeto

A solution segue uma organização em camadas:

```
source/
├── Account.Api/       # Startup, injeção de dependência e controllers HTTP
├── Account.Domain/    # Entidades, enums, commands, aggregates e contratos
│   └── Framework/     # Classes base: Entity, CommandHandlerBase, IRepository, IMediator
├── Account.Data/      # Implementações em memória: repositório, queries, provedor e serviços externos
└── Account.Tasks/     # Jobs agendados
```

Fluxo de uma requisição:

```
Controller -> IMediator -> CommandHandler -> (Provider, Aggregate, Repository)
```

- Os commands ficam em `Account.Domain/Commands` e estendem `CommandHandlerBase`, que abre e confirma a transação e mapeia a entidade para o response.
- Alterações de estado nas entidades passam pelos aggregates em `Account.Domain/Aggregates`.
- Leituras usam as interfaces de `Account.Domain/Queries`; escritas usam `IRepository`.
- Interfaces de serviços externos ficam em `Account.Domain/Providers` e `Account.Domain/Services`, com implementações em `Account.Data`.

Fique à vontade para criar, alterar ou remover arquivos, inclusive nas camadas de framework, se fizer sentido para a solução.
