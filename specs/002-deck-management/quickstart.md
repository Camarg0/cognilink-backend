# Quickstart: Gestão de Baralhos (Decks)

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Contrato**: [contracts/decks.openapi.yaml](./contracts/decks.openapi.yaml)

Guia de validação end-to-end para confirmar que a feature funciona conforme as user stories do spec. Não contém código de implementação.

## Pré-requisitos

- .NET 10 SDK instalado.
- Infraestrutura da feature 001 já em funcionamento (bootstrap do `Program.cs`, JWT, conexão Firestore) — esta feature não altera nada do bootstrap.
- Projeto Firestore configurado (ou emulador do Firestore) com credenciais disponíveis via `dotnet user-secrets` ou variáveis de ambiente.
- Solução `CogniLink.slnx` restaurada (`dotnet restore`).
- Um usuário autenticado (via `POST /api/auth/register` + `POST /api/auth/login` da feature 001) para obter `accessToken` — todos os endpoints de Decks exigem `Authorization: Bearer <accessToken>`.
- Um segundo usuário autenticado (usuário B), usado para validar o isolamento por proprietário (RF-06).

## Setup

```powershell
dotnet restore
dotnet build
dotnet run --project CogniLink.Api
```

A API deve subir em HTTPS com Swagger disponível (ex.: `https://localhost:<porta>/swagger`), agora exibindo também os endpoints `/api/decks`.

## Cenários de validação (mapeados às User Stories do spec)

### 1. Criar baralho (US1)

1. Autenticado como usuário A, `POST /api/decks` com `name` (3-100 chars), `difficulty` válida (`Facil`/`Medio`/`Dificil`) e até 5 `categories` (até 30 chars cada), sem `description`.
   - Esperado: `201` com o baralho criado, `cardCount: 0` e `description` nula/vazia.
2. `POST /api/decks` sem `name`.
   - Esperado: `400` com erro de validação.
3. `POST /api/decks` com 6 categorias ou uma categoria com 31+ caracteres.
   - Esperado: `400` com erro de validação.
4. `POST /api/decks` com `difficulty` fora do enum (ex.: `"Extreme"`).
   - Esperado: `400` com erro de validação.

### 2. Listar baralhos próprios com busca e paginação (US2)

1. Criar 3+ baralhos para o usuário A e 1+ baralho para o usuário B.
2. Autenticado como A, `GET /api/decks`.
   - Esperado: `200` retornando somente os baralhos de A (nenhum de B).
3. `GET /api/decks?search=<termo presente em parte do nome, em outra caixa>`.
   - Esperado: `200` retornando apenas os baralhos de A cujo nome contém o termo, ignorando maiúsculas/minúsculas.
4. `GET /api/decks?page=2&pageSize=1` com A tendo pelo menos 2 baralhos.
   - Esperado: `200` com 1 item (o segundo, conforme ordenação), `page: 2`, `pageSize: 1`, `totalCount` correto.
5. Autenticado como um usuário sem baralhos, `GET /api/decks`.
   - Esperado: `200` com `items: []` e `totalCount: 0`.

### 3. Visualizar detalhes de um baralho (US3)

1. Autenticado como A, `GET /api/decks/{id}` de um baralho próprio.
   - Esperado: `200` com nome, descrição, dificuldade, categorias e `cardCount: 0`.
2. Autenticado como A, `GET /api/decks/{id}` usando o `id` de um baralho de B.
   - Esperado: `404` (nunca `403`).
3. Autenticado como A, `GET /api/decks/{id}` com um `id` inexistente.
   - Esperado: `404`.

### 4. Editar baralho próprio (US4)

1. Autenticado como A, `PUT /api/decks/{id}` de um baralho próprio com novos `name`, `description`, `difficulty` e `categories` válidos.
   - Esperado: `200` com os dados atualizados; `GET /api/decks/{id}` subsequente reflete as mudanças.
2. `PUT /api/decks/{id}` com dados inválidos (ex.: `name` com 2 caracteres).
   - Esperado: `400`; `GET /api/decks/{id}` subsequente mostra que os dados não foram alterados.
3. Autenticado como A, `PUT /api/decks/{id}` usando o `id` de um baralho de B.
   - Esperado: `404`.

### 5. Excluir baralho próprio (US5)

1. Autenticado como A, `DELETE /api/decks/{id}` de um baralho próprio.
   - Esperado: `204`; `GET /api/decks/{id}` subsequente retorna `404`; o baralho não aparece mais em `GET /api/decks`.
2. Autenticado como A, `DELETE /api/decks/{id}` usando o `id` de um baralho de B.
   - Esperado: `404`; confirmar via login como B que o baralho de B ainda existe (`GET /api/decks/{id}` como B retorna `200`).

## Critérios de aceite gerais

- Nenhuma resposta de erro relacionada a recurso de outro usuário usa `403` — sempre `404` (FR-006, SC-002).
- `cardCount` é sempre `0` em todas as respostas desta feature (FR-011).
- Nenhum endpoint desta feature funciona sem `Authorization: Bearer <accessToken>` válido (herdado da feature 001).
