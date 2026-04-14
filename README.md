# Task Manager Web

Aplicacao full stack para gerenciamento de tarefas com Angular no frontend e ASP.NET Core Web API no backend. Esta versao evolui o desafio original com autenticacao JWT, persistencia em banco relacional, cache com Redis, mensageria com RabbitMQ, logs estruturados, validacao com FluentValidation, tratamento centralizado de erros, DTOs bem definidos, testes automatizados e ambiente completo com Docker.

## Stack

- Frontend: Angular 20
- Backend: ASP.NET Core 8 Web API
- Banco de dados: SQL Server
- Autenticacao: JWT Bearer
- Cache: Redis
- Fila: RabbitMQ
- Logs: Serilog com JSON estruturado no console
- Validacao: FluentValidation
- Testes: xUnit + FluentAssertions + EF Core InMemory
- Containers: Docker Compose
- Runtime do frontend em container: Node 20

## O que foi implementado

- Login e cadastro de usuarios com JWT
- CRUD de tarefas protegido por autenticacao
- Separacao das tarefas por usuario autenticado
- Tema claro/escuro com persistencia no navegador
- Cache de listas e detalhes de tarefas no Redis
- Publicacao e consumo de eventos de tarefas no RabbitMQ
- Tratamento global de erros com payload padronizado
- Validacao de requests com FluentValidation
- DTOs de entrada e saida mais consistentes
- Logs estruturados no backend
- Testes unitarios para servicos e validadores
- Orquestracao completa com Docker Compose

## Estrutura

- `frontend/`: aplicacao Angular
- `backend/TaskManager.Api/`: API ASP.NET Core
- `backend/TaskManager.Api.Tests/`: testes automatizados do backend
- `docker-compose.yml`: sobe frontend, backend, SQL Server, Redis e RabbitMQ
- `protagonize-tech.sln`: solucao .NET

## Como rodar com Docker

Pre-requisitos:

- Docker Desktop

Na raiz do projeto:

```bash
docker compose up --build
```

Servicos disponiveis:

- Frontend: `http://localhost:4200`
- Backend / Swagger: `http://localhost:5063/swagger`
- Health check da API: `http://localhost:5063/health`
- RabbitMQ Management: `http://localhost:15672`
- SQL Server: `localhost,1433`
- Redis: `localhost:6379`

Credenciais padrao do ambiente Docker:

- SQL Server
  - Usuario: `sa`
  - Senha: `StrongPassword!123`
- RabbitMQ
  - Usuario: `guest`
  - Senha: `guest`

Observacoes:

- O backend cria o banco automaticamente ao subir.
- O backend tambem executa um seed idempotente com usuarios e tarefas de demonstracao.
- O frontend em container usa Node 20.
- O frontend acessa a API via `http://localhost:5063`.

## Como rodar localmente sem Docker

Pre-requisitos:

- .NET SDK 8
- Node.js 20
- SQL Server
- Redis
- RabbitMQ

### 1. Backend

Edite `backend/TaskManager.Api/appsettings.json` se precisar ajustar a conexao:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=TaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Depois execute:

```bash
dotnet restore
dotnet run --project backend/TaskManager.Api/TaskManager.Api.csproj
```

Endpoints uteis:

- Swagger: `http://localhost:5063/swagger`
- Health: `http://localhost:5063/health`

### 2. Frontend

No diretorio `frontend/`:

```bash
npm install
npm start
```

Aplicacao:

- `http://localhost:4200`

## Fluxo de autenticacao

1. O usuario cria a conta em `POST /api/auth/register` ou faz login em `POST /api/auth/login`.
2. A API retorna um `accessToken` JWT e os dados do usuario autenticado.
3. O Angular salva a sessao no `localStorage`.
4. Um interceptor adiciona `Authorization: Bearer {token}` automaticamente nas chamadas protegidas.
5. Se a API responder `401`, a sessao local e encerrada.

## Dados de demonstracao

O seed e executado no startup de forma idempotente. Se os usuarios de demo ja existirem, nada e duplicado.

Usuarios disponiveis apos a primeira subida:

- `admin.demo` / `Admin@123`
- `analista.demo` / `Analista@123`

Cada usuario recebe um conjunto inicial de tarefas para facilitar a avaliacao do CRUD, cache e eventos.

## Endpoints principais

### Autenticacao

- `POST /api/auth/register`
- `POST /api/auth/login`

Exemplo:

```json
{
  "username": "andre",
  "password": "senha-segura-123"
}
```

### Tarefas

Todos exigem token JWT:

- `GET /api/tasks`
- `GET /api/tasks/{id}`
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `DELETE /api/tasks/{id}`

Exemplo de payload:

```json
{
  "titulo": "Implementar dashboard",
  "descricao": "Criar a tela principal com autenticacao e filtros",
  "status": "Pendente"
}
```

## Cache, fila e logs

- Redis guarda listas e itens de tarefas por usuario para reduzir leituras no banco.
- RabbitMQ recebe eventos `created`, `updated` e `deleted` das tarefas.
- O consumidor de fila registra o processamento no log.
- O Serilog escreve logs estruturados em JSON no console, facilitando observabilidade em ambiente local e conteinerizado.

## Validacao e tratamento de erros

- Requests de autenticacao e tarefas usam FluentValidation.
- Erros de validacao, conflito, nao encontrado e nao autorizado sao tratados por middleware.
- O retorno de erro segue um contrato padronizado com `title`, `status`, `detail`, `traceId` e `errors` quando aplicavel.

Exemplo de erro:

```json
{
  "title": "Erro de validacao",
  "status": 400,
  "detail": "Um ou mais campos estao invalidos.",
  "traceId": "0HMTL8...",
  "errors": {
    "Password": [
      "The length of 'Password' must be at least 6 characters. You entered 3 characters."
    ]
  }
}
```

## Testes

Testes implementados no backend:

- regras de autenticacao
- regras do servico de tarefas
- validadores com FluentValidation

Executar:

```bash
dotnet test
```

## Proximos passos sugeridos

- adicionar testes automatizados no frontend
- trocar `EnsureCreated()` por migrations do EF Core
- externalizar segredos sensiveis para variaveis de ambiente seguras
- adicionar rate limiting e refresh token se o projeto evoluir para producao
