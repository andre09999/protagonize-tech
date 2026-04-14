# Task Manager Web

Aplicacao full stack de gerenciamento de tarefas desenvolvida como evolucao de um desafio tecnico, com foco em boas praticas de engenharia, experiencia do usuario e preocupacoes comuns de um ambiente real de desenvolvimento.

O projeto combina um frontend Angular com uma API ASP.NET Core protegida por JWT, persistencia em SQL Server, cache com Redis, mensageria com RabbitMQ, logs estruturados, validacao com FluentValidation, tratamento centralizado de erros, seed de dados para demonstracao e ambiente conteinerizado com Docker Compose.

## Visao Geral

Esta entrega foi pensada para ir alem de um CRUD basico. O objetivo foi demonstrar:

- arquitetura full stack organizada
- autenticacao e autorizacao com separacao de dados por usuario
- experiencia de uso mais polida no frontend
- preocupacoes de backend como cache, fila, logs e tratamento de falhas
- automacao de ambiente com Docker
- testes automatizados para regras criticas

## Principais Funcionalidades

- cadastro e login de usuarios com JWT
- CRUD completo de tarefas protegido por autenticacao
- isolamento de tarefas por usuario autenticado
- filtro por status
- tema claro/escuro com persistencia no navegador
- seed idempotente com usuarios e tarefas de demonstracao
- cache de listas e detalhes de tarefas com Redis
- publicacao e consumo de eventos de tarefas com RabbitMQ
- middleware global de tratamento de erros
- validacao de entrada com FluentValidation
- logs estruturados em JSON com Serilog

## Arquitetura

```text
Angular 20
   |
   v
ASP.NET Core 8 Web API
   |-- SQL Server 2022  -> persistencia principal
   |-- Redis            -> cache distribuido
   |-- RabbitMQ         -> eventos de dominio
   |-- Serilog          -> logs estruturados
```

## Stack Tecnica

| Camada | Tecnologia | Responsabilidade |
| --- | --- | --- |
| Frontend | Angular 20 | Interface, autenticacao, consumo da API, tema claro/escuro |
| Backend | ASP.NET Core 8 | API REST, regras de negocio, autenticacao JWT |
| Persistencia | SQL Server | armazenamento relacional de usuarios e tarefas |
| Cache | Redis | cache de listas e detalhes de tarefas |
| Mensageria | RabbitMQ | publicacao e consumo de eventos |
| Validacao | FluentValidation | validacao declarativa de requests |
| Logs | Serilog | logs estruturados no console |
| Testes | xUnit, FluentAssertions, EF InMemory | validacao de servicos e regras de negocio |
| Containers | Docker Compose | orquestracao do ambiente completo |

## Diferenciais Tecnicos

- autenticacao JWT integrada ponta a ponta entre frontend e backend
- DTOs bem definidos para requests e responses
- middleware centralizado para respostas de erro padronizadas
- cache com invalidacao por usuario e por recurso
- fila de eventos para registrar alteracoes em tarefas
- health endpoint para verificacao rapida da API
- ambiente unico com frontend, backend, banco, Redis e RabbitMQ
- seed automatico para facilitar avaliacao tecnica e demonstracao

## Estrutura do Projeto

- `frontend/`: aplicacao Angular
- `backend/TaskManager.Api/`: API ASP.NET Core
- `backend/TaskManager.Api.Tests/`: testes automatizados do backend
- `docker-compose.yml`: orquestracao do ambiente completo
- `protagonize-tech.sln`: solucao .NET

## Como Executar com Docker

### Pre-requisitos

- Docker Desktop

### Subida completa do ambiente

Na raiz do projeto:

```bash
docker compose up --build
```

Para rodar em background:

```bash
docker compose up --build -d
```

### Servicos disponiveis

| Servico | URL / Porta |
| --- | --- |
| Frontend | `http://localhost:4200` |
| API / Swagger | `http://localhost:5063/swagger` |
| Health Check | `http://localhost:5063/health` |
| RabbitMQ Management | `http://localhost:15672` |
| SQL Server | `localhost,1433` |
| Redis | `localhost:6379` |

### Credenciais padrao

| Recurso | Usuario | Senha |
| --- | --- | --- |
| SQL Server | `sa` | `StrongPassword!123` |
| RabbitMQ | `guest` | `guest` |

### Observacoes

- o backend cria o banco automaticamente ao subir
- o seed de dados e executado no startup de forma idempotente
- o frontend em container utiliza Node 20
- o frontend consome a API em `http://localhost:5063`

## Como Executar Localmente

### Pre-requisitos

- .NET SDK 8
- Node.js 20
- SQL Server
- Redis
- RabbitMQ

### Backend

Se necessario, ajuste a connection string em `backend/TaskManager.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=TaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Executar:

```bash
dotnet restore
dotnet run --project backend/TaskManager.Api/TaskManager.Api.csproj
```

Endpoints uteis:

- Swagger: `http://localhost:5063/swagger`
- Health: `http://localhost:5063/health`

### Frontend

No diretorio `frontend/`:

```bash
npm install
npm start
```

Aplicacao:

- `http://localhost:4200`

## Fluxo de Autenticacao

1. O usuario cria uma conta em `POST /api/auth/register` ou faz login em `POST /api/auth/login`.
2. A API retorna um `accessToken` JWT com os dados do usuario autenticado.
3. O Angular persiste a sessao localmente.
4. Um interceptor injeta `Authorization: Bearer {token}` automaticamente nas chamadas protegidas.
5. Em caso de `401 Unauthorized`, a sessao local e encerrada.

## Dados de Demonstracao

Para facilitar a avaliacao tecnica, o projeto sobe com seed automatico e idempotente.

Usuarios disponiveis:

- `admin.demo` / `Admin@123`
- `analista.demo` / `Analista@123`

Cada usuario recebe tarefas iniciais para demonstrar:

- criacao, edicao e exclusao
- isolamento de dados por usuario
- invalidacao de cache
- emissao de eventos no RabbitMQ

## Resumo da API

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

Todos os endpoints abaixo exigem JWT:

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

## Qualidade, Observabilidade e Resiliencia

### Validacao e tratamento de erros

- validacao declarativa com FluentValidation
- responses de erro padronizadas com `title`, `status`, `detail`, `traceId` e `errors`
- tratamento centralizado via middleware

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

### Cache, fila e logs

- Redis para cache de listas e itens por usuario
- RabbitMQ para eventos `created`, `updated` e `deleted`
- Serilog para logs estruturados em JSON no console

## Testes

Os testes atuais cobrem:

- regras de autenticacao
- comportamento do servico de tarefas
- validadores de entrada

Executar:

```bash
dotnet test backend/TaskManager.Api.Tests/TaskManager.Api.Tests.csproj -c Release
```

## Decisoes de Implementacao

- `EnsureCreated()` foi utilizado para agilizar a demonstracao do ambiente completo
- o seed foi mantido idempotente para permitir subir e derrubar containers sem duplicar dados
- o frontend usa chamadas diretas para a API local para simplificar a execucao do projeto em avaliacao tecnica

## Melhorias Futuras

- adicionar testes automatizados no frontend
- substituir `EnsureCreated()` por migrations do EF Core
- externalizar segredos para variaveis de ambiente seguras
- adicionar refresh token e rate limiting
- servir o frontend por Nginx em build de producao no container

## Consideracoes Finais

Este repositorio foi estruturado para demonstrar capacidade de entrega full stack com preocupacoes reais de engenharia, indo alem da implementacao funcional basica. O foco foi combinar clareza de codigo, experiencia de uso e fundamentos de backend que normalmente aparecem em sistemas profissionais.
