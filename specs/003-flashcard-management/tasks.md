# Tasks: Gestao de Flashcards

**Input**: Documentos de design de `specs/003-flashcard-management/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/flashcards.openapi.yaml](./contracts/flashcards.openapi.yaml), [quickstart.md](./quickstart.md)

**Tests**: Nao incluir tarefas de teste automatizado nesta feature. Validacao funcional sera manual via [quickstart.md](./quickstart.md).

**Organization**: Tarefas organizadas por user story e agrupadas por camada (Domain, Application, Infrastructure, Presentation) para execucao em sessoes separadas.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar estrutura minima para a feature sem alterar bootstrap existente.

- [ ] T001 Criar estrutura de pastas da feature em `CogniLink.Application/Flashcards/`, `CogniLink.Infrastructure/Persistence/Firestore/` e `CogniLink.Api/Controllers/`
- [ ] T002 Criar enum `FlashcardType` em `CogniLink.Domain/Enums/FlashcardType.cs`
- [ ] T003 [P] Criar enum `FlashcardDifficulty` em `CogniLink.Domain/Enums/FlashcardDifficulty.cs`
- [ ] T004 [P] Criar value object `Alternative` em `CogniLink.Domain/ValueObjects/Alternative.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Base compartilhada obrigatoria antes de qualquer user story.

**CRITICAL**: Nenhuma implementacao de user story comeca antes desta fase.

### Domain

- [ ] T005 Criar entidade `Flashcard` com campos comuns e tipados opcionais em `CogniLink.Domain/Entities/Flashcard.cs`
- [ ] T006 Implementar validacao de invariantes por tipo no agregado `Flashcard` (metodo de validacao interno chamado por create/update) em `CogniLink.Domain/Entities/Flashcard.cs`

### Application

- [x] T007 Criar interface `IFlashcardRepository` (Add, GetById, ListByDeckId, Update, Delete) em `CogniLink.Application/Common/Interfaces/IFlashcardRepository.cs`
- [x] T008 [P] Criar DTO base de resposta `FlashcardDto` em `CogniLink.Application/Common/Models/FlashcardDto.cs`
- [x] T009 [P] Criar mapeamento de `Flashcard` para `FlashcardDto` via Mapster em `CogniLink.Application/Common/Mappings/FlashcardMappings.cs`

### Infrastructure

- [x] T010 Implementar `FirestoreFlashcardRepository` seguindo padrao de `FirestoreDeckRepository` em `CogniLink.Infrastructure/Persistence/Firestore/FirestoreFlashcardRepository.cs`
- [x] T011 Configurar consulta por `deckId` em `ListByDeckIdAsync` usando `WhereEqualTo("deckId", ...)` em `CogniLink.Infrastructure/Persistence/Firestore/FirestoreFlashcardRepository.cs`
- [ ] T012 Registrar `IFlashcardRepository` no DI em `CogniLink.Infrastructure/DependencyInjection.cs`

### Presentation

- [x] T013 Criar esqueleto de `FlashcardsController` com `[Authorize]` e injecao de `IMediator` em `CogniLink.Api/Controllers/FlashcardsController.cs`
- [x] T014 Adicionar DTOs HTTP de flashcard em `CogniLink.Api/Contracts/Requests.cs`

**Checkpoint**: Fundacao pronta para executar user stories em paralelo.

---

## Phase 3: User Story 1 - Criar Flashcard Tipado em Deck Proprio (Priority: P1) MVP

**Goal**: Permitir criacao de flashcard em deck proprio com validacoes bloqueantes por tipo.

**Independent Test**: Enviar `POST /api/decks/{deckId}/flashcards` com payload valido de cada tipo e confirmar `201`; repetir para deck inexistente/alheio e confirmar `404`.

### Domain

- [ ] T015 [US1] Implementar factories/metodos de criacao de `Flashcard` com normalizacao de campos opcionais por tipo em `CogniLink.Domain/Entities/Flashcard.cs`

### Application

- [x] T016 [P] [US1] Criar `CreateFlashcardCommand` em `CogniLink.Application/Flashcards/Commands/CreateFlashcard/CreateFlashcardCommand.cs`
- [x] T017 [US1] Criar validator unico condicional por `Type` para `CreateFlashcardCommand` em `CogniLink.Application/Flashcards/Commands/CreateFlashcard/CreateFlashcardCommandValidator.cs`
- [x] T018 [US1] Implementar `CreateFlashcardCommandHandler` validando deck via `IDeckRepository` antes de persistir em `IFlashcardRepository` em `CogniLink.Application/Flashcards/Commands/CreateFlashcard/CreateFlashcardCommandHandler.cs`

### Infrastructure

- [x] T019 [US1] Implementar persistencia de criacao de flashcard em `AddAsync` com um unico documento Firestore em `CogniLink.Infrastructure/Persistence/Firestore/FirestoreFlashcardRepository.cs`

### Presentation

- [x] T020 [P] [US1] Criar request DTO de criacao de flashcard em `CogniLink.Api/Contracts/Requests.cs`
- [x] T021 [US1] Implementar endpoint `POST /api/decks/{deckId}/flashcards` com mapeamento para command e retorno `201` em `CogniLink.Api/Controllers/FlashcardsController.cs`

**Checkpoint**: US1 funcional e validavel de forma independente.

---

## Phase 4: User Story 2 - Consultar Flashcards do Deck Proprio (Priority: P2)

**Goal**: Listar flashcards de deck proprio e obter flashcard por id com isolamento por proprietario.

**Independent Test**: Chamar `GET /api/decks/{deckId}/flashcards` e `GET /api/flashcards/{flashcardId}` para recursos proprios (200) e alheios/inexistentes (404).

### Domain

- [ ] T022 [US2] Ajustar reconstituicao de `Flashcard` para leitura completa de campos tipados opcionais em `CogniLink.Domain/Entities/Flashcard.cs`

### Application

- [x] T023 [P] [US2] Criar `ListFlashcardsByDeckQuery` em `CogniLink.Application/Flashcards/Queries/ListFlashcardsByDeck/ListFlashcardsByDeckQuery.cs`
- [x] T024 [US2] Implementar `ListFlashcardsByDeckQueryHandler` com validacao de deck ownership via `IDeckRepository` em `CogniLink.Application/Flashcards/Queries/ListFlashcardsByDeck/ListFlashcardsByDeckQueryHandler.cs`
- [x] T025 [P] [US2] Criar `GetFlashcardByIdQuery` em `CogniLink.Application/Flashcards/Queries/GetFlashcardById/GetFlashcardByIdQuery.cs`
- [x] T026 [US2] Implementar `GetFlashcardByIdQueryHandler` validando ownership pelo deck pai antes do retorno em `CogniLink.Application/Flashcards/Queries/GetFlashcardById/GetFlashcardByIdQueryHandler.cs`

### Infrastructure

- [x] T027 [P] [US2] Implementar listagem por `deckId` em `ListByDeckIdAsync` em `CogniLink.Infrastructure/Persistence/Firestore/FirestoreFlashcardRepository.cs`
- [x] T028 [P] [US2] Implementar leitura por id em `GetByIdAsync` em `CogniLink.Infrastructure/Persistence/Firestore/FirestoreFlashcardRepository.cs`

### Presentation

- [x] T029 [US2] Implementar endpoint `GET /api/decks/{deckId}/flashcards` em `CogniLink.Api/Controllers/FlashcardsController.cs`
- [x] T030 [US2] Implementar endpoint `GET /api/flashcards/{flashcardId}` em `CogniLink.Api/Controllers/FlashcardsController.cs`

**Checkpoint**: US2 funcional e independente com isolamento por ownership.

---

## Phase 5: User Story 3 - Manter Flashcard Proprio (Priority: P3)

**Goal**: Editar todos os campos e excluir flashcard proprio com regras por tipo e 404 para nao ownership.

**Independent Test**: Executar `PUT /api/flashcards/{flashcardId}` com payload valido/invalido e `DELETE /api/flashcards/{flashcardId}` para recurso proprio e alheio.

### Domain

- [ ] T031 [US3] Implementar metodo de atualizacao completa com reaplicacao de invariantes por tipo em `CogniLink.Domain/Entities/Flashcard.cs`

### Application

- [x] T032 [P] [US3] Criar `UpdateFlashcardCommand` em `CogniLink.Application/Flashcards/Commands/UpdateFlashcard/UpdateFlashcardCommand.cs`
- [x] T033 [US3] Criar validator unico condicional por `Type` para `UpdateFlashcardCommand` em `CogniLink.Application/Flashcards/Commands/UpdateFlashcard/UpdateFlashcardCommandValidator.cs`
- [x] T034 [US3] Implementar `UpdateFlashcardCommandHandler` validando ownership via deck antes de atualizar em `CogniLink.Application/Flashcards/Commands/UpdateFlashcard/UpdateFlashcardCommandHandler.cs`
- [x] T035 [P] [US3] Criar `DeleteFlashcardCommand` em `CogniLink.Application/Flashcards/Commands/DeleteFlashcard/DeleteFlashcardCommand.cs`
- [x] T036 [US3] Implementar `DeleteFlashcardCommandHandler` validando ownership via deck antes de excluir em `CogniLink.Application/Flashcards/Commands/DeleteFlashcard/DeleteFlashcardCommandHandler.cs`

### Infrastructure

- [x] T037 [P] [US3] Implementar persistencia de update em `UpdateAsync` em `CogniLink.Infrastructure/Persistence/Firestore/FirestoreFlashcardRepository.cs`
- [x] T038 [P] [US3] Implementar remocao definitiva em `DeleteAsync` em `CogniLink.Infrastructure/Persistence/Firestore/FirestoreFlashcardRepository.cs`

### Presentation

- [x] T039 [P] [US3] Criar request DTO de edicao de flashcard em `CogniLink.Api/Contracts/Requests.cs`
- [x] T040 [US3] Implementar endpoint `PUT /api/flashcards/{flashcardId}` em `CogniLink.Api/Controllers/FlashcardsController.cs`
- [x] T041 [US3] Implementar endpoint `DELETE /api/flashcards/{flashcardId}` (204) em `CogniLink.Api/Controllers/FlashcardsController.cs`

**Checkpoint**: US3 funcional e independente.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Ajustes finais transversais para consistencia da feature.

- [x] T042 [P] Atualizar anotacoes Swagger e codigos de resposta de `FlashcardsController` para refletir 200/201/204/400/401/404 em `CogniLink.Api/Controllers/FlashcardsController.cs`
- [ ] T043 [P] Revisar mapeamentos Mapster de requests/commands/DTOs de flashcard em `CogniLink.Api/Contracts/Requests.cs` e `CogniLink.Application/Common/Mappings/FlashcardMappings.cs`
- [ ] T044 Executar validacao manual completa dos cenarios de [quickstart.md](./quickstart.md) e registrar observacoes de aceite em `specs/003-flashcard-management/quickstart.md`
- [ ] T045 Rodar compilacao da solucao para verificar integracao da feature em `CogniLink.slnx`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: sem dependencias.
- **Phase 2 (Foundational)**: depende da conclusao da Phase 1 e bloqueia todas as user stories.
- **Phase 3 (US1)**: depende da Phase 2.
- **Phase 4 (US2)**: depende da Phase 2; pode iniciar em paralelo com US1 se houver equipe.
- **Phase 5 (US3)**: depende da Phase 2 e se beneficia de US1/US2 ja concluida para dados existentes.
- **Phase 6 (Polish)**: depende das stories concluidas.

### User Story Dependencies

- **US1 (P1)**: base para criar dados de flashcard; independe de outras stories.
- **US2 (P2)**: independe de US3; depende apenas da base foundational.
- **US3 (P3)**: independe de US2 no codigo, mas normalmente usa cards criados na US1 para validacao funcional.

### Within Each User Story

- Domain antes de handlers.
- Commands/Queries antes de Validators/Handlers.
- Infrastructure antes da exposicao final nos endpoints.
- Alteracoes no mesmo arquivo `CogniLink.Api/Controllers/FlashcardsController.cs` devem ser sequenciais.

### Parallel Opportunities

- Setup: T003 e T004 em paralelo com T002.
- Foundational: T008/T009 em paralelo; T010/T011 podem iniciar apos T007.
- US1: T016 e T020 em paralelo; T019 em paralelo com T018 apos contrato pronto.
- US2: T023 e T025 em paralelo; T027 e T028 em paralelo.
- US3: T032 e T035 em paralelo; T037 e T038 em paralelo; T039 em paralelo com handlers.

---

## Parallel Example: User Story 1

```bash
Task: "T016 [P] [US1] Criar CreateFlashcardCommand em CogniLink.Application/Flashcards/Commands/CreateFlashcard/CreateFlashcardCommand.cs"
Task: "T020 [P] [US1] Criar request DTO de criacao em CogniLink.Api/Contracts/Requests.cs"
Task: "T019 [US1] Implementar AddAsync em CogniLink.Infrastructure/Persistence/Firestore/FirestoreFlashcardRepository.cs"
```

## Parallel Example: User Story 2

```bash
Task: "T023 [P] [US2] Criar ListFlashcardsByDeckQuery em CogniLink.Application/Flashcards/Queries/ListFlashcardsByDeck/ListFlashcardsByDeckQuery.cs"
Task: "T025 [P] [US2] Criar GetFlashcardByIdQuery em CogniLink.Application/Flashcards/Queries/GetFlashcardById/GetFlashcardByIdQuery.cs"
Task: "T027 [P] [US2] Implementar ListByDeckIdAsync em CogniLink.Infrastructure/Persistence/Firestore/FirestoreFlashcardRepository.cs"
Task: "T028 [P] [US2] Implementar GetByIdAsync em CogniLink.Infrastructure/Persistence/Firestore/FirestoreFlashcardRepository.cs"
```

## Parallel Example: User Story 3

```bash
Task: "T032 [P] [US3] Criar UpdateFlashcardCommand em CogniLink.Application/Flashcards/Commands/UpdateFlashcard/UpdateFlashcardCommand.cs"
Task: "T035 [P] [US3] Criar DeleteFlashcardCommand em CogniLink.Application/Flashcards/Commands/DeleteFlashcard/DeleteFlashcardCommand.cs"
Task: "T037 [P] [US3] Implementar UpdateAsync em CogniLink.Infrastructure/Persistence/Firestore/FirestoreFlashcardRepository.cs"
Task: "T038 [P] [US3] Implementar DeleteAsync em CogniLink.Infrastructure/Persistence/Firestore/FirestoreFlashcardRepository.cs"
```

---

## Implementation Strategy

### MVP First (US1)

1. Concluir Phase 1 e Phase 2.
2. Implementar US1 completa (Phase 3).
3. Validar criacao dos 4 tipos e respostas 404/400 conforme regras.
4. Liberar MVP inicial para demonstracao.

### Incremental Delivery

1. Base pronta (Setup + Foundational).
2. Entregar US1 (criacao).
3. Entregar US2 (consulta).
4. Entregar US3 (edicao/exclusao).
5. Executar Polish e validacao manual final.

### Parallel Team Strategy

1. Time A: Domain + Application de cada story.
2. Time B: Infrastructure da mesma story em paralelo apos contratos.
3. Time C: Presentation endpoints e ajustes de Swagger ao final de cada story.

---

## Notes

- Todas as tarefas seguem formato checklist obrigatorio com ID sequencial e caminho de arquivo.
- Nenhuma tarefa de teste unitario/integracao foi incluida por requisito explicito.
- Agrupamento por camada foi mantido dentro de cada fase de user story para execucao em sessoes separadas.
