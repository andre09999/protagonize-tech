# Task Manager Web

Aplicacao full stack para cadastro e gerenciamento de tarefas, desenvolvida com Angular no front-end e ASP.NET Core Web API no back-end.

## Tecnologias

- Angular 20
- ASP.NET Core 8 Web API
- Entity Framework Core
- SQL Server
- API REST com JSON

## Funcionalidades

- Listagem de tarefas
- Criacao de tarefa
- Edicao de tarefa
- Exclusao de tarefa
- Filtro por status
- Validacao basica de formulario
- Mensagens simples de sucesso e erro

## Estrutura do projeto

- `frontend/`: aplicacao Angular
- `backend/TaskManager.Api/`: API ASP.NET Core
- `protagonize-tech.sln`: solucao .NET

## Requisitos

- .NET SDK 8
- Node.js 22+ ou 20 LTS recomendado
- SQL Server em execucao

## Configuracao do banco

A API usa a connection string abaixo por padrao em `backend/TaskManager.Api/appsettings.json`:

```json
"DefaultConnection": "Server=localhost;Database=TaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Se o seu SQL Server usar outro host, instancia ou autenticacao, ajuste essa string antes de executar.

## Como rodar o back-end

No diretorio raiz:

```bash
dotnet build backend/TaskManager.Api/TaskManager.Api.csproj
dotnet run --project backend/TaskManager.Api/TaskManager.Api.csproj
```

API disponivel em:

- `http://localhost:5063`
- Swagger: `http://localhost:5063/swagger`

## Como rodar o front-end

No diretorio `frontend/`:

```bash
npm install
npm start
```

Aplicacao disponivel em:

- `http://localhost:4200`

Para gerar a build de producao:

```bash
npm run build
```

## Endpoints da API

- `GET /api/tasks`: lista todas as tarefas
- `GET /api/tasks/{id}`: busca tarefa por id
- `POST /api/tasks`: cria uma nova tarefa
- `PUT /api/tasks/{id}`: atualiza uma tarefa
- `DELETE /api/tasks/{id}`: remove uma tarefa

## Exemplo de payload

```json
{
  "titulo": "Estudar Angular",
  "descricao": "Criar a tela de cadastro de tarefas",
  "status": "Pendente"
}
```

## Observacoes

- A API tenta criar a base automaticamente com `EnsureCreated()` quando sobe.
- O front esta configurado para consumir a API em `http://localhost:5063/api/tasks`.
- Se houver erro de conexao, confira se o SQL Server e a API estao em execucao.

## Validacoes implementadas

- Titulo obrigatorio com maximo de 150 caracteres
- Descricao obrigatoria com maximo de 500 caracteres
- Status obrigatorio
- Aceita apenas `Pendente` ou `Concluida` na API
