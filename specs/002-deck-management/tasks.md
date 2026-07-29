---

description: "Task list template for feature implementation"
---

# Tasks: Gestão de Baralhos (Decks)

**Input**: Design documents from `/specs/002-deck-management/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/decks.openapi.yaml](./contracts/decks.openapi.yaml), [quickstart.md](./quickstart.md)

**Tests**: Removidos a pedido explícito do usuário — nenhuma tarefa de teste unitário ou de integração está incluída nesta lista. A validação funcional é feita manualmente via [quickstart.md](./quickstart.md) (ver Fase Polish).

**Organization**: Tarefas agrupadas por user story para permitir implementação independente de cada uma.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependências)
- **[Story]**: A qual user story esta tarefa pertence (US1..US5)
- Caminhos de arquivo exatos incluídos em cada descrição

## Path Conventions

Extensão do projeto único em Clean Architecture já existente (feature 001): `CogniLink.Api/`, `CogniLink.Application/`, `CogniLink.Domain/`, `CogniLink.Infrastructure/`. Nenhum novo projeto de produto é criado — apenas os artefatos de Decks dentro das camadas já estabelecidas.

---

## Phase 1: Foundational (Blocking Prerequisites)

**Purpose**: Entidade `Deck`, contrato de repositório, implementação Firestore, exceção de "não encontrado" e DTO de resposta — base compartilhada por todas as 5 user stories

**⚠️ CRITICAL**: Nenhuma user story pode ser implementada antes desta fase estar completa

- [X] T001 [P] Criar enum `DeckDifficulty` (Facil, Medio, Dificil) em `CogniLink.Domain/Enums/DeckDifficulty.cs`
- [X] T002 Criar entidade `Deck` (Id, OwnerId, Name, Description, Difficulty, Categories, CreatedAt, UpdatedAt) com `Create`, `Reconstitute` e `UpdateDetails` (invariantes: Name 3-100 chars, Description até 500 chars, até 5 Categories de até 30 chars cada) em `CogniLink.Domain/Entities/Deck.cs` (depende de T001)
- [X] T003 [P] Criar `NotFoundException` em `CogniLink.Application/Common/Exceptions/NotFoundException.cs`
- [X] T004 Mapear `NotFoundException` para HTTP 404 em `CogniLink.Api/Middleware/ExceptionHandlingMiddleware.cs` (depende de T003)
- [X] T005 [P] Criar `DeckResponse` (Id, Name, Description, Difficulty, Categories, CardCount fixo em 0, CreatedAt, UpdatedAt) em `CogniLink.Application/Common/Models/DeckResponse.cs`
- [X] T006 Criar interface `IDeckRepository` (GetByIdAsync, GetByOwnerAsync, AddAsync, UpdateAsync, DeleteAsync) em `CogniLink.Application/Common/Interfaces/IDeckRepository.cs` (depende de T002)
- [X] T007 Implementar `FirestoreDeckRepository` (coleção `"decks"`, mesmo padrão de `FirestoreUserRepository`) em `CogniLink.Infrastructure/Persistence/Firestore/FirestoreDeckRepository.cs` (depende de T006)
- [X] T008 Registrar `IDeckRepository -> FirestoreDeckRepository` em `CogniLink.Infrastructure/DependencyInjection.cs` (depende de T007)
- [X] T009 Criar `DecksController` vazio (rota `api/decks`, `[Authorize]`, injeção de `IMediator`) em `CogniLink.Api/Controllers/DecksController.cs` (depende de T008)

**Checkpoint**: Fundação pronta — implementação das user stories pode começar

---

## Phase 2: User Story 1 - Criar baralho (Priority: P1) 🎯 MVP

**Goal**: Usuário autenticado cria um baralho com nome, descrição opcional, dificuldade e categorias, vinculado ao seu `userId`.

**Independent Test**: Autenticar um usuário, enviar `POST /api/decks` com dados válidos e confirmar que o baralho é criado com `OwnerId` correto e `cardCount: 0`.

### Implementation for User Story 1

- [X] T010 [P] [US1] Criar `CreateDeckCommand` (Name, Description, Difficulty, Categories) em `CogniLink.Application/Decks/Commands/CreateDeck/CreateDeckCommand.cs`
- [X] T011 [US1] Criar `CreateDeckCommandValidator` em `CogniLink.Application/Decks/Commands/CreateDeck/CreateDeckCommandValidator.cs` (depende de T010)
- [X] T012 [US1] Criar `CreateDeckCommandHandler` (resolve `OwnerId` via `ICurrentUserService`, `Deck.Create(...)`, `IDeckRepository.AddAsync`, retorna `DeckResponse`) em `CogniLink.Application/Decks/Commands/CreateDeck/CreateDeckCommandHandler.cs` (depende de T010)
- [X] T013 [P] [US1] Criar `CreateDeckRequest` em `CogniLink.Api/Contracts/Requests.cs`
- [X] T014 [US1] Adicionar endpoint `POST /api/decks` (`Adapt<CreateDeckCommand>()`, retorna 201) em `CogniLink.Api/Controllers/DecksController.cs` (depende de T009, T012, T013)

**Checkpoint**: Criação de baralho funcional e testável de forma independente

---

## Phase 3: User Story 2 - Listar baralhos próprios (Priority: P1) 🎯 MVP

**Goal**: Usuário autenticado lista apenas seus próprios baralhos, com busca por nome e paginação.

**Independent Test**: Criar baralhos para dois usuários diferentes; confirmar que a listagem de um usuário retorna somente os seus, respeitando `search` (contains, case-insensitive) e `page`/`pageSize`.

### Implementation for User Story 2

- [X] T015 [P] [US2] Criar `PagedResult<T>` (Items, Page, PageSize, TotalCount) em `CogniLink.Application/Common/Models/PagedResult.cs`
- [X] T016 [P] [US2] Criar `ListDecksQuery` (Search, Page, PageSize) em `CogniLink.Application/Decks/Queries/ListDecks/ListDecksQuery.cs`
- [X] T017 [US2] Criar `ListDecksQueryValidator` em `CogniLink.Application/Decks/Queries/ListDecks/ListDecksQueryValidator.cs` (depende de T016)
- [X] T018 [US2] Criar `ListDecksQueryHandler` (resolve `OwnerId`, `IDeckRepository.GetByOwnerAsync`, filtra por nome em memória, pagina, mapeia para `DeckResponse`) em `CogniLink.Application/Decks/Queries/ListDecks/ListDecksQueryHandler.cs` (depende de T015, T016)
- [X] T019 [US2] Adicionar endpoint `GET /api/decks` (query params `search`, `page`, `pageSize`) em `CogniLink.Api/Controllers/DecksController.cs` (depende de T009, T018)

**Checkpoint**: Criação e listagem funcionam de forma independente (MVP completo)

---

## Phase 4: User Story 3 - Visualizar detalhes de um baralho (Priority: P2)

**Goal**: Usuário autenticado visualiza detalhes de um baralho próprio; acesso a baralho de outro usuário retorna 404.

**Independent Test**: Criar um baralho para o usuário A; confirmar `GET /api/decks/{id}` retorna os dados completos para A e 404 quando acessado pelo usuário B ou com id inexistente.

### Implementation for User Story 3

- [X] T020 [P] [US3] Criar `GetDeckByIdQuery` (Id) em `CogniLink.Application/Decks/Queries/GetDeckById/GetDeckByIdQuery.cs`
- [X] T021 [US3] Criar `GetDeckByIdQueryHandler` (resolve `OwnerId`, `IDeckRepository.GetByIdAsync`, lança `NotFoundException` se nulo ou `OwnerId` diferente, mapeia para `DeckResponse`) em `CogniLink.Application/Decks/Queries/GetDeckById/GetDeckByIdQueryHandler.cs` (depende de T020)
- [X] T022 [US3] Adicionar endpoint `GET /api/decks/{id}` em `CogniLink.Api/Controllers/DecksController.cs` (depende de T009, T021)

**Checkpoint**: Visualização de detalhes funcional, com isolamento por proprietário confirmado (404)

---

## Phase 5: User Story 4 - Editar baralho próprio (Priority: P2)

**Goal**: Usuário autenticado edita nome, descrição, dificuldade e categorias de um baralho próprio; edição de baralho de outro usuário retorna 404.

**Independent Test**: Criar um baralho para o usuário A; enviar `PUT /api/decks/{id}` com dados válidos e confirmar persistência; confirmar 404 ao tentar editar baralho do usuário B.

### Implementation for User Story 4

- [X] T023 [P] [US4] Criar `UpdateDeckCommand` (Id, Name, Description, Difficulty, Categories) em `CogniLink.Application/Decks/Commands/UpdateDeck/UpdateDeckCommand.cs`
- [X] T024 [US4] Criar `UpdateDeckCommandValidator` em `CogniLink.Application/Decks/Commands/UpdateDeck/UpdateDeckCommandValidator.cs` (depende de T023)
- [X] T025 [US4] Criar `UpdateDeckCommandHandler` (resolve `OwnerId`, `IDeckRepository.GetByIdAsync`, valida propriedade, `deck.UpdateDetails(...)`, `IDeckRepository.UpdateAsync`) em `CogniLink.Application/Decks/Commands/UpdateDeck/UpdateDeckCommandHandler.cs` (depende de T023)
- [X] T026 [P] [US4] Criar `UpdateDeckRequest` em `CogniLink.Api/Contracts/Requests.cs`
- [X] T027 [US4] Adicionar endpoint `PUT /api/decks/{id}` em `CogniLink.Api/Controllers/DecksController.cs` (depende de T009, T025, T026)

**Checkpoint**: Criação, listagem, visualização e edição funcionam de forma independente

---

## Phase 6: User Story 5 - Excluir baralho próprio (Priority: P3)

**Goal**: Usuário autenticado exclui um baralho próprio (hard delete, sem cascata); exclusão de baralho de outro usuário retorna 404.

**Independent Test**: Criar um baralho para o usuário A; `DELETE /api/decks/{id}` remove o baralho (confirmado via 404 subsequente); confirmar 404 e não remoção ao tentar excluir baralho do usuário B.

### Implementation for User Story 5

- [X] T028 [P] [US5] Criar `DeleteDeckCommand` (Id) em `CogniLink.Application/Decks/Commands/DeleteDeck/DeleteDeckCommand.cs`
- [X] T029 [US5] Criar `DeleteDeckCommandHandler` (resolve `OwnerId`, `IDeckRepository.GetByIdAsync`, valida propriedade, `IDeckRepository.DeleteAsync`) em `CogniLink.Application/Decks/Commands/DeleteDeck/DeleteDeckCommandHandler.cs` (depende de T028)
- [X] T030 [US5] Adicionar endpoint `DELETE /api/decks/{id}` (retorna 204) em `CogniLink.Api/Controllers/DecksController.cs` (depende de T009, T029)

**Checkpoint**: Todas as 5 user stories funcionam de forma independente

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Melhorias que afetam múltiplas user stories

- [X] T031 [P] Revisar anotações Swagger do `DecksController` (summary/produces/responses) seguindo o padrão de `AuthController`/`ProfileController` em `CogniLink.Api/Controllers/DecksController.cs`
- [ ] T032 Executar todos os cenários de [quickstart.md](./quickstart.md) manualmente (ou via Swagger) contra a API rodando localmente, confirmando os 5 endpoints e o isolamento por proprietário (404 uniforme)
- [X] T033 Rodar `dotnet build CogniLink.slnx`, confirmando que a solução compila sem erros junto com o restante da feature 001

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — pode começar imediatamente
- **Foundational (Phase 2)**: Depende da conclusão do Setup — BLOQUEIA todas as user stories
- **User Stories (Phase 3-7)**: Todas dependem da conclusão da Fase Foundational
  - US1 e US2 são P1 (MVP) e podem ser feitas em paralelo entre si (times diferentes) ou sequencialmente
  - US3 e US4 são P2, US5 é P3 — todas independentes entre si, seguem a mesma base Foundational
- **Polish (Phase 8)**: Depende da conclusão das user stories desejadas

### User Story Dependencies

- **US1 (P1)**: Depende apenas de Foundational — sem dependência de outras stories
- **US2 (P1)**: Depende apenas de Foundational — sem dependência de US1 (embora normalmente implementada em seguida, pois é a segunda forma de MVP)
- **US3 (P2)**: Depende apenas de Foundational
- **US4 (P2)**: Depende apenas de Foundational
- **US5 (P3)**: Depende apenas de Foundational

### Within Each User Story

- Command/Query record antes do Validator e do Handler
- Handler antes do endpoint no controller
- Todas as tarefas que editam `CogniLink.Api/Controllers/DecksController.cs` são sequenciais entre si (mesmo arquivo) — não marcadas `[P]`

### Parallel Opportunities

- Todas as tarefas de Setup (T001-T003) podem rodar em paralelo
- T004, T006, T008 (Foundational, arquivos distintos) podem rodar em paralelo
- Commands/Queries/Requests de uma mesma story marcados `[P]` podem rodar em paralelo até tocarem o mesmo arquivo (`DecksController.cs`, `Requests.cs`)
- Após Foundational, US1 e US2 (ambas P1) podem ser implementadas em paralelo por desenvolvedores diferentes; US3, US4 e US5 podem seguir o mesmo padrão

---

## Implementation Strategy

### MVP First (User Stories 1 e 2)

1. Completar Fase 1: Setup
2. Completar Fase 2: Foundational (CRÍTICO — bloqueia todas as stories)
3. Completar Fase 3: User Story 1 (criar baralho)
4. Completar Fase 4: User Story 2 (listar baralhos)
5. **PARAR e VALIDAR**: testar criação + listagem de forma independente (cobre RF-01 e RF-02)
6. Deploy/demo se pronto

### Incremental Delivery

1. Setup + Foundational → base pronta
2. US1 (criar) → testar independentemente → MVP inicial
3. US2 (listar) → testar independentemente → MVP completo (criar + listar)
4. US3 (detalhe) → testar independentemente → deploy/demo
5. US4 (editar) → testar independentemente → deploy/demo
6. US5 (excluir) → testar independentemente → deploy/demo
7. Cada story adiciona valor sem quebrar as anteriores

### Parallel Team Strategy

Com múltiplos desenvolvedores:

1. Time completa Setup + Foundational juntos
2. Após Foundational:
   - Dev A: US1 (criar)
   - Dev B: US2 (listar)
   - Dev C: US3 (detalhe), depois US4 (editar) e US5 (excluir)
3. Stories completam e integram de forma independente, coordenando apenas ao tocar `DecksController.cs`

---

## Notes

- `[P]` = arquivos diferentes, sem dependências
- Rótulo `[Story]` mapeia a tarefa à user story correspondente para rastreabilidade
- Cada user story deve ser completável e testável de forma independente
- Fazer commit após cada tarefa ou grupo lógico
- Parar em qualquer checkpoint para validar a story isoladamente
- Evitar: tarefas vagas, conflitos no mesmo arquivo, dependências entre stories que quebrem a independência
