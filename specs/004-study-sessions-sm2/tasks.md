# Tasks: Study Sessions SM-2

**Input**: Design documents from `/specs/004-study-sessions-sm2/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Nao incluir tarefas de teste automatizado nesta feature. Validacao sera manual via quickstart.

**Organization**: Tasks are grouped by user story and grouped by layer (Domain, Application, Infrastructure, Presentation) inside each phase for execution in separate work sessions.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar estrutura da feature reaproveitando integralmente o bootstrap e os padroes existentes.

- [ ] T001 Criar estrutura de pastas de Study Sessions em CogniLink.Application/StudySessions/Commands/, CogniLink.Application/StudySessions/Queries/, CogniLink.Domain/Entities/, CogniLink.Infrastructure/Persistence/Firestore/ e CogniLink.Api/Controllers/
- [ ] T002 Criar arquivo de contrato OpenAPI da feature em specs/004-study-sessions-sm2/contracts/study-sessions.openapi.yaml
- [ ] T003 [P] Revisar e alinhar roteiro de validacao manual da feature em specs/004-study-sessions-sm2/quickstart.md

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Base compartilhada obrigatoria antes de qualquer user story.

**CRITICAL**: Nenhuma implementacao de user story comeca antes desta fase.

### Domain

- [x] T004 Criar enum StudySessionStatus (InProgress, Completed, Cancelled) em CogniLink.Domain/Enums/StudySessionStatus.cs
- [x] T005 [P] Criar entidade StudySession com estado e transicoes basicas em CogniLink.Domain/Entities/StudySession.cs
- [x] T006 [P] Criar entidade StudyAnswer com attemptId e metadados de resposta em CogniLink.Domain/Entities/StudyAnswer.cs
- [x] T007 [P] Criar entidade UserFlashcardProgress com defaults de SM-2 em CogniLink.Domain/Entities/UserFlashcardProgress.cs

### Application

- [x] T008 Criar interface IStudySessionRepository em CogniLink.Application/Common/Interfaces/IStudySessionRepository.cs
- [x] T009 [P] Criar interface IStudyAnswerRepository com busca por attemptId em CogniLink.Application/Common/Interfaces/IStudyAnswerRepository.cs
- [x] T010 [P] Criar interface IUserFlashcardProgressRepository com consulta por owner e card em CogniLink.Application/Common/Interfaces/IUserFlashcardProgressRepository.cs
- [x] T011 [P] Criar DTO StudySessionDto em CogniLink.Application/Common/Models/StudySessionDto.cs
- [x] T012 [P] Criar DTO StudyAnswerResultDto em CogniLink.Application/Common/Models/StudyAnswerResultDto.cs
- [x] T013 [P] Criar DTO DueFlashcardDto em CogniLink.Application/Common/Models/DueFlashcardDto.cs
- [x] T014 [P] Criar DTO UserStreakDto em CogniLink.Application/Common/Models/UserStreakDto.cs

### Infrastructure

- [x] T015 Implementar FirestoreStudySessionRepository em CogniLink.Infrastructure/Persistence/Firestore/FirestoreStudySessionRepository.cs
- [x] T016 [P] Implementar FirestoreStudyAnswerRepository com consulta por attemptId em CogniLink.Infrastructure/Persistence/Firestore/FirestoreStudyAnswerRepository.cs
- [x] T017 [P] Implementar FirestoreUserFlashcardProgressRepository com consulta ownerId + nextReviewDate em CogniLink.Infrastructure/Persistence/Firestore/FirestoreUserFlashcardProgressRepository.cs
- [x] T018 Registrar IStudySessionRepository, IStudyAnswerRepository e IUserFlashcardProgressRepository em CogniLink.Infrastructure/DependencyInjection.cs

### Presentation

- [x] T019 Criar DTOs HTTP de estudo e revisao em CogniLink.Api/Contracts/Requests.cs
- [x] T020 Criar esqueleto de StudySessionsController com [Authorize] e IMediator em CogniLink.Api/Controllers/StudySessionsController.cs

**Checkpoint**: Fundacao pronta; user stories podem iniciar.

---

## Phase 3: User Story 1 - Iniciar e Executar Sessao de Estudo (Priority: P1) MVP

**Goal**: Permitir iniciar sessao para deck proprio e obter proximo card pendente com attemptId unico.

**Independent Test**: Iniciar sessao em deck proprio com cards e consumir next-card ate 204; validar 404 para deck inexistente/alheio e 422 para deck sem flashcards.

### Domain

- [x] T021 [US1] Implementar regra de criacao e encerramento basico de sessao em CogniLink.Domain/Entities/StudySession.cs

### Application

- [x] T022 [P] [US1] Criar StartStudySessionCommand em CogniLink.Application/StudySessions/Commands/StartStudySession/StartStudySessionCommand.cs
- [x] T023 [US1] Implementar StartStudySessionCommandValidator em CogniLink.Application/StudySessions/Commands/StartStudySession/StartStudySessionCommandValidator.cs
- [x] T024 [US1] Implementar StartStudySessionCommandHandler com validacao de ownership via IDeckRepository e bloqueio de deck vazio em CogniLink.Application/StudySessions/Commands/StartStudySession/StartStudySessionCommandHandler.cs
- [x] T025 [P] [US1] Criar GetNextCardQuery em CogniLink.Application/StudySessions/Queries/GetNextCard/GetNextCardQuery.cs
- [x] T026 [US1] Implementar GetNextCardQueryHandler com ordem never-reviewed primeiro e overdue por nextReviewDate ascendente em CogniLink.Application/StudySessions/Queries/GetNextCard/GetNextCardQueryHandler.cs
- [x] T027 [US1] Implementar geracao de attemptId unico por card/sessao no fluxo de next-card em CogniLink.Application/StudySessions/Queries/GetNextCard/GetNextCardQueryHandler.cs

### Infrastructure

- [ ] T028 [US1] Implementar persistencia de abertura de sessao (status InProgress) em CogniLink.Infrastructure/Persistence/Firestore/FirestoreStudySessionRepository.cs
- [ ] T029 [P] [US1] Implementar consulta de sessao por id e owner em CogniLink.Infrastructure/Persistence/Firestore/FirestoreStudySessionRepository.cs
- [ ] T030 [P] [US1] Implementar consulta de progresso por usuario e card para ordenacao de pendencias em CogniLink.Infrastructure/Persistence/Firestore/FirestoreUserFlashcardProgressRepository.cs

### Presentation

- [x] T031 [US1] Implementar endpoint POST /api/study-sessions em CogniLink.Api/Controllers/StudySessionsController.cs
- [x] T032 [US1] Implementar endpoint GET /api/study-sessions/{sessionId}/next-card em CogniLink.Api/Controllers/StudySessionsController.cs
- [x] T033 [US1] Atualizar schema de StartStudySession e NextCard no contrato em specs/004-study-sessions-sm2/contracts/study-sessions.openapi.yaml

**Checkpoint**: US1 funcional e validavel de forma independente.

---

## Phase 4: User Story 2 - Registrar Resposta e Recalcular Agendamento (Priority: P1)

**Goal**: Registrar resposta idempotente por attemptId e recalcular SM-2 exclusivamente no servidor.

**Independent Test**: Submeter resposta valida, verificar progresso recalculado; reenviar mesmo attemptId e validar retorno idempotente sem novo recalculo.

### Domain

- [x] T034 [US2] Implementar metodo ApplyReview(isCorrect) como tarefa isolada com algoritmo SM-2 completo (easeFactor, intervalDays, repetitions, lapses, nextReviewDate e piso 1.3) em CogniLink.Domain/Entities/UserFlashcardProgress.cs
- [x] T035 [US2] Implementar regras de consistencia de attempt e resposta na entidade StudyAnswer em CogniLink.Domain/Entities/StudyAnswer.cs

### Application

- [x] T036 [P] [US2] Criar SubmitAnswerCommand em CogniLink.Application/StudySessions/Commands/SubmitAnswer/SubmitAnswerCommand.cs
- [x] T037 [US2] Implementar SubmitAnswerCommandValidator em CogniLink.Application/StudySessions/Commands/SubmitAnswer/SubmitAnswerCommandValidator.cs
- [x] T038 [US2] Implementar SubmitAnswerCommandHandler com fluxo idempotente por attemptId antes de aplicar SM-2 em CogniLink.Application/StudySessions/Commands/SubmitAnswer/SubmitAnswerCommandHandler.cs
- [x] T039 [US2] Implementar atualizacao de UserFlashcardProgress via ApplyReview no handler de submit em CogniLink.Application/StudySessions/Commands/SubmitAnswer/SubmitAnswerCommandHandler.cs

### Infrastructure

- [ ] T040 [US2] Implementar busca por attemptId em FirestoreStudyAnswerRepository em CogniLink.Infrastructure/Persistence/Firestore/FirestoreStudyAnswerRepository.cs
- [ ] T041 [P] [US2] Implementar persistencia de StudyAnswer sem duplicacao de attemptId em CogniLink.Infrastructure/Persistence/Firestore/FirestoreStudyAnswerRepository.cs
- [ ] T042 [P] [US2] Implementar upsert de UserFlashcardProgress em CogniLink.Infrastructure/Persistence/Firestore/FirestoreUserFlashcardProgressRepository.cs

### Presentation

- [x] T043 [US2] Implementar endpoint POST /api/study-sessions/{sessionId}/answers em CogniLink.Api/Controllers/StudySessionsController.cs
- [x] T044 [US2] Atualizar schema de SubmitAnswer com resposta idempotente no contrato em specs/004-study-sessions-sm2/contracts/study-sessions.openapi.yaml

**Checkpoint**: US2 funcional e independente, com idempotencia garantida.

---

## Phase 5: User Story 3 - Concluir, Cancelar e Acompanhar Streak (Priority: P2)

**Goal**: Encerrar sessao com resumo, cancelar sessao sem recalculo indevido e expor streak e pendencias agregadas.

**Independent Test**: Concluir sessao e validar resumo; cancelar sessao parcial e validar que cards nao respondidos nao foram recalculados; consultar streak e due list.

### Domain

- [x] T045 [US3] Implementar regra de conclusao e cancelamento idempotente de sessao em CogniLink.Domain/Entities/StudySession.cs

### Application

- [x] T046 [P] [US3] Criar CompleteStudySessionCommand em CogniLink.Application/StudySessions/Commands/CompleteStudySession/CompleteStudySessionCommand.cs
- [x] T047 [US3] Implementar CompleteStudySessionCommandHandler com calculo de cardsStudied, accuracyRate e duration em CogniLink.Application/StudySessions/Commands/CompleteStudySession/CompleteStudySessionCommandHandler.cs
- [x] T048 [P] [US3] Criar CancelStudySessionCommand em CogniLink.Application/StudySessions/Commands/CancelStudySession/CancelStudySessionCommand.cs
- [x] T049 [US3] Implementar CancelStudySessionCommandHandler preservando cards nao respondidos em CogniLink.Application/StudySessions/Commands/CancelStudySession/CancelStudySessionCommandHandler.cs
- [x] T050 [P] [US3] Criar GetDueFlashcardsQuery em CogniLink.Application/StudySessions/Queries/GetDueFlashcards/GetDueFlashcardsQuery.cs
- [x] T051 [US3] Implementar GetDueFlashcardsQueryHandler agregando pendencias do usuario em CogniLink.Application/StudySessions/Queries/GetDueFlashcards/GetDueFlashcardsQueryHandler.cs
- [x] T052 [P] [US3] Criar GetUserStreakQuery em CogniLink.Application/StudySessions/Queries/GetUserStreak/GetUserStreakQuery.cs
- [x] T053 [US3] Implementar GetUserStreakQueryHandler por dias distintos com sessoes Completed em CogniLink.Application/StudySessions/Queries/GetUserStreak/GetUserStreakQueryHandler.cs

### Infrastructure

- [ ] T054 [US3] Implementar update de status e completedAt de sessao em CogniLink.Infrastructure/Persistence/Firestore/FirestoreStudySessionRepository.cs
- [ ] T055 [P] [US3] Implementar consulta de respostas por sessionId para resumo em CogniLink.Infrastructure/Persistence/Firestore/FirestoreStudyAnswerRepository.cs
- [ ] T056 [P] [US3] Implementar consulta agregada de due flashcards por ownerId em CogniLink.Infrastructure/Persistence/Firestore/FirestoreUserFlashcardProgressRepository.cs
- [ ] T057 [P] [US3] Implementar consulta de sessoes concluidas por ownerId para streak em CogniLink.Infrastructure/Persistence/Firestore/FirestoreStudySessionRepository.cs

### Presentation

- [x] T058 [US3] Implementar endpoint POST /api/study-sessions/{sessionId}/complete em CogniLink.Api/Controllers/StudySessionsController.cs
- [x] T059 [US3] Implementar endpoint POST /api/study-sessions/{sessionId}/cancel em CogniLink.Api/Controllers/StudySessionsController.cs
- [x] T060 [US3] Implementar endpoint GET /api/reviews/due em CogniLink.Api/Controllers/StudySessionsController.cs
- [x] T061 [US3] Implementar endpoint GET /api/users/me/streak em CogniLink.Api/Controllers/StudySessionsController.cs
- [x] T062 [US3] Atualizar schemas de complete/cancel/due/streak no contrato em specs/004-study-sessions-sm2/contracts/study-sessions.openapi.yaml

**Checkpoint**: US3 funcional e independente.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Ajustes finais transversais da feature.

- [x] T063 [P] Consolidar anotacoes de resposta Swagger no controller em CogniLink.Api/Controllers/StudySessionsController.cs
- [ ] T064 [P] Revisar mapeamentos Mapster de DTOs de sessao e revisao em CogniLink.Application/Common/Mappings/StudySessionMappings.cs
- [ ] T065 Realizar revisao manual focada em ApplyReview e registrar observacoes em specs/004-study-sessions-sm2/research.md
- [ ] T066 Executar validacao manual completa dos cenarios do quickstart em specs/004-study-sessions-sm2/quickstart.md
- [ ] T067 Rodar compilacao da solucao para verificar integracao da feature em CogniLink.slnx

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup and blocks all stories.
- **User Stories (Phase 3+)**: Depend on Foundational completion.
- **Polish (Phase 6)**: Depends on completion of desired stories.

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Phase 2; no dependency on other stories.
- **User Story 2 (P1)**: Starts after Phase 2; depends functionally on session and attempt flow from US1.
- **User Story 3 (P2)**: Starts after Phase 2; consumes outputs of US1 and US2 for summary, due list, and streak.

### Within Each User Story

- Domain tasks before handlers.
- Commands/queries before validators/handlers.
- Repository capabilities before endpoint final wiring.
- Tasks touching same file should run sequentially.

### Parallel Opportunities

- Foundational interfaces and DTO tasks marked [P] can run in parallel.
- Within US1, query/command definitions and repository read helpers marked [P] can run in parallel.
- Within US2, command definition and repository methods marked [P] can run in parallel after T034.
- Within US3, command/query definitions and repository read methods marked [P] can run in parallel.

---

## Parallel Example: User Story 1

```bash
Task: "T022 [P] [US1] Criar StartStudySessionCommand em CogniLink.Application/StudySessions/Commands/StartStudySession/StartStudySessionCommand.cs"
Task: "T025 [P] [US1] Criar GetNextCardQuery em CogniLink.Application/StudySessions/Queries/GetNextCard/GetNextCardQuery.cs"
Task: "T029 [P] [US1] Implementar consulta de sessao por id e owner em CogniLink.Infrastructure/Persistence/Firestore/FirestoreStudySessionRepository.cs"
Task: "T030 [P] [US1] Implementar consulta de progresso por usuario e card em CogniLink.Infrastructure/Persistence/Firestore/FirestoreUserFlashcardProgressRepository.cs"
```

## Parallel Example: User Story 2

```bash
Task: "T036 [P] [US2] Criar SubmitAnswerCommand em CogniLink.Application/StudySessions/Commands/SubmitAnswer/SubmitAnswerCommand.cs"
Task: "T041 [P] [US2] Implementar persistencia de StudyAnswer sem duplicacao de attemptId em CogniLink.Infrastructure/Persistence/Firestore/FirestoreStudyAnswerRepository.cs"
Task: "T042 [P] [US2] Implementar upsert de UserFlashcardProgress em CogniLink.Infrastructure/Persistence/Firestore/FirestoreUserFlashcardProgressRepository.cs"
```

## Parallel Example: User Story 3

```bash
Task: "T046 [P] [US3] Criar CompleteStudySessionCommand em CogniLink.Application/StudySessions/Commands/CompleteStudySession/CompleteStudySessionCommand.cs"
Task: "T048 [P] [US3] Criar CancelStudySessionCommand em CogniLink.Application/StudySessions/Commands/CancelStudySession/CancelStudySessionCommand.cs"
Task: "T050 [P] [US3] Criar GetDueFlashcardsQuery em CogniLink.Application/StudySessions/Queries/GetDueFlashcards/GetDueFlashcardsQuery.cs"
Task: "T052 [P] [US3] Criar GetUserStreakQuery em CogniLink.Application/StudySessions/Queries/GetUserStreak/GetUserStreakQuery.cs"
Task: "T055 [P] [US3] Implementar consulta de respostas por sessionId em CogniLink.Infrastructure/Persistence/Firestore/FirestoreStudyAnswerRepository.cs"
Task: "T056 [P] [US3] Implementar consulta agregada de due flashcards por ownerId em CogniLink.Infrastructure/Persistence/Firestore/FirestoreUserFlashcardProgressRepository.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational (critical).
3. Complete Phase 3: User Story 1.
4. Validate manually the US1 flow through quickstart scenarios.

### Incremental Delivery

1. Finish Setup + Foundational.
2. Deliver US1 for session start and next card.
3. Deliver US2 for idempotent answer submission and SM-2 recalculation.
4. Deliver US3 for completion/cancel plus due and streak endpoints.
5. Run Polish tasks and final manual validation.

### Parallel Team Strategy

1. Team completes Setup + Foundational together.
2. For each story, split work by layer sessions:
   - Domain session
   - Application session
   - Infrastructure session
   - Presentation session
3. Integrate at story checkpoint before moving to next story.

---

## Notes

- All tasks follow the required checklist format with sequential IDs and file paths.
- No unit, integration, or contract automated test tasks were included by explicit scope request.
- T034 is intentionally isolated as the single critical SM-2 implementation task for ApplyReview in Domain.
