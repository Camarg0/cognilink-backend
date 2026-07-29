# Tasks: Dashboard e Métricas

**Input**: Design documents from `/specs/005-dashboard-metrics/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/dashboard.openapi.yaml, quickstart.md

**Tests**: Não incluídos nesta etapa por restrição explícita de escopo (sem testes automatizados).

**Organization**: Por pedido explícito, esta feature é organizada por camada de arquitetura (Domain → Application → Presentation) em vez de por user story, já que é majoritariamente leitura/agregação sobre entidades já existentes. Uma fase de Infrastructure foi incluída porque research.md identificou que ela é estritamente necessária (novo método em `IStudySessionRepository` e correção de um registro de DI ausente) — sem ela, nenhuma das três queries funciona. Cada tarefa mantém o rótulo `[USx]` apenas para rastreabilidade com o user story do spec.md que ela serve.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependência de tarefa incompleta)
- **[Story]**: User story do spec.md que a tarefa serve (US1, US2, US3) — apenas nas tarefas de Application/Presentation ligadas a uma query/endpoint específico
- Caminhos de arquivo exatos em cada descrição

---

## Fase 1: Infrastructure (pré-requisito bloqueante)

**Motivo de existir**: research.md (itens 4 e 6) identificou que as três queries desta feature não funcionam sem estas duas correções mínimas nos repositórios já existentes — não é a criação de nenhum repositório novo.

- [ ] T001 [P] Adicionar `Task<IReadOnlyList<StudySession>> ListByOwnerAsync(string ownerId, CancellationToken cancellationToken)` à interface `IStudySessionRepository` em `CogniLink.Application/Common/Interfaces/IStudySessionRepository.cs`, retornando sessões de **qualquer status** do usuário (sem filtrar por `Completed`, ao contrário de `ListCompletedByOwnerAsync`, que permanece inalterado).
- [ ] T002 Implementar `ListByOwnerAsync` em `CogniLink.Infrastructure/Persistence/Firestore/FirestoreStudySessionRepository.cs`, reaproveitando a mesma consulta Firestore de `ListCompletedByOwnerAsync` (`WhereEqualTo("ownerId", ownerId)`) porém sem o `WhereEqualTo("status", ...)`. Depende de T001.
- [ ] T003 [P] Registrar `services.AddScoped<IFlashcardRepository, FirestoreFlashcardRepository>();` em `CogniLink.Infrastructure/DependencyInjection.cs` — corrige gap pré-existente (a interface nunca foi registrada desde a feature 003), pré-requisito direto de `GetDeckStatistics` e também de `GetDueFlashcardsQueryHandler` (feature 004), que hoje falharia em runtime sem este registro.

**Checkpoint**: `IStudySessionRepository`/`FirestoreStudySessionRepository` expõem `ListByOwnerAsync` e `IFlashcardRepository` resolve via DI. Nenhuma query de Application pode ser implementada antes disso.

---

## Fase 2: Domain

- [x] T004 [P] Adicionar o método `IsMastered()` a `UserFlashcardProgress` em `CogniLink.Domain/Entities/UserFlashcardProgress.cs`, retornando `Repetitions >= 2 && IntervalDays >= 21`. Método público, sem parâmetros, sem efeito colateral — centraliza a regra de mastery usada por `GetDashboard` e `GetDeckStatistics`.

**Checkpoint**: Regra de mastery disponível e reutilizável por qualquer query de Application.

---

## Fase 3: Application

### DTOs (Common/Models)

- [x] T005 [P] Criar o record `DashboardSummaryDto` em `CogniLink.Application/Common/Models/DashboardSummaryDto.cs` com os campos `MasteryPercentage` (`double`), `TotalStudyTimeSeconds` (`long`), `CompletedCardsCount` (`int`), `RetentionRate` (`double`), `CurrentStreak` (`int`), `LongestStreak` (`int`), `DueFlashcardsCount` (`int`), conforme data-model.md.
- [x] T006 [P] Criar o record `PerformanceGroupDto` em `CogniLink.Application/Common/Models/PerformanceGroupDto.cs` com os campos `GroupKey` (`string`), `TotalAnswers` (`int`), `CorrectAnswers` (`int`), `AccuracyRate` (`double`), conforme data-model.md.
- [x] T007 Criar o record `DeckStatisticsDto` em `CogniLink.Application/Common/Models/DeckStatisticsDto.cs` com os campos `DeckId` (`string`), `TotalFlashcards` (`int`), `DueFlashcardsCount` (`int`), `DeckMasteryPercentage` (`double`), `PerformanceByDifficulty`/`PerformanceBySubarea`/`PerformanceByType` (`IReadOnlyList<PerformanceGroupDto>`). Depende de T006 (referencia `PerformanceGroupDto`).
- [x] T008 [P] Criar o record `StudyHistoryEntryDto` em `CogniLink.Application/Common/Models/StudyHistoryEntryDto.cs` com os campos `Date` (`DateOnly`), `AnswersCount` (`int`), `CorrectCount` (`int`), `IncorrectCount` (`int`), `TotalTimeSeconds` (`long`), conforme data-model.md.

### Query: GetDashboard [US1]

- [x] T009 [US1] Criar o record `GetDashboardQuery` (`IRequest<DashboardSummaryDto>`, sem parâmetros) em `CogniLink.Application/Dashboard/Queries/GetDashboard/GetDashboardQuery.cs`.
- [x] T010 [US1] Implementar `GetDashboardQueryHandler` em `CogniLink.Application/Dashboard/Queries/GetDashboard/GetDashboardQueryHandler.cs`: resolver `ownerId` via `ICurrentUserService` (lançar `AuthenticationFailedException` se nulo); calcular `MasteryPercentage` combinando `IDeckRepository.GetByOwnerAsync` + `IFlashcardRepository.ListByDeckIdAsync` (total de flashcards elegíveis) com `IUserFlashcardProgressRepository.ListByOwnerAsync` indexado por `FlashcardId` e `IsMastered()` (retornar 0 se não houver flashcards); agregar `TotalStudyTimeSeconds`, `CompletedCardsCount` e `RetentionRate` buscando `IStudySessionRepository.ListByOwnerAsync` (T001/T002) e, para cada sessão, `IStudyAnswerRepository.ListBySessionIdAsync`, consolidando em memória (retornar 0 em `RetentionRate` se não houver respostas); obter `CurrentStreak`/`LongestStreak` enviando `GetUserStreakQuery` via `IMediator`; obter `DueFlashcardsCount` como a contagem do resultado de `GetDueFlashcardsQuery` via `IMediator`; retornar `DashboardSummaryDto`. Depende de T001, T002, T003, T004, T005.

### Query: GetDeckStatistics [US2]

- [x] T011 [US2] Criar o record `GetDeckStatisticsQuery(string DeckId)` (`IRequest<DeckStatisticsDto>`) em `CogniLink.Application/Dashboard/Queries/GetDeckStatistics/GetDeckStatisticsQuery.cs`.
- [x] T012 [US2] Implementar `GetDeckStatisticsQueryHandler` em `CogniLink.Application/Dashboard/Queries/GetDeckStatistics/GetDeckStatisticsQueryHandler.cs`: resolver `ownerId`; buscar o deck via `IDeckRepository.GetByIdAsync(request.DeckId)` e lançar `NotFoundException("Baralho não encontrado.")` se nulo ou `deck.OwnerId != ownerId`; listar flashcards do deck via `IFlashcardRepository.ListByDeckIdAsync` para `TotalFlashcards`; calcular `DeckMasteryPercentage` restringindo `IsMastered()` aos flashcards do deck (0 se `TotalFlashcards == 0`); calcular `DueFlashcardsCount` filtrando o resultado de `GetDueFlashcardsQuery` (via `IMediator`) por `DeckId == request.DeckId`; agregar respostas do deck filtrando `IStudySessionRepository.ListByOwnerAsync` por `session.DeckId == request.DeckId` e expandindo com `IStudyAnswerRepository.ListBySessionIdAsync`; agrupar essas respostas por `Flashcard.Difficulty`, `Flashcard.Subarea` (`"Sem subárea"` quando nulo) e `Flashcard.Type`, produzindo `PerformanceGroupDto` por grupo (0 em `AccuracyRate` se o grupo não tiver respostas); retornar `DeckStatisticsDto`. Depende de T001, T002, T003, T004, T006, T007.

### Query: GetStudyHistory [US3]

- [x] T013 [US3] Criar o record `GetStudyHistoryQuery(DateOnly? From, DateOnly? To)` (`IRequest<IReadOnlyList<StudyHistoryEntryDto>>`) em `CogniLink.Application/Dashboard/Queries/GetStudyHistory/GetStudyHistoryQuery.cs`.
- [x] T014 [P] [US3] Criar `GetStudyHistoryQueryValidator : AbstractValidator<GetStudyHistoryQuery>` em `CogniLink.Application/Dashboard/Queries/GetStudyHistory/GetStudyHistoryQueryValidator.cs`, exigindo `From`/`To` não nulos (`NotNull()`), `To >= From`, e `(To - From) <= 365 dias`. Depende de T013 (referencia a query).
- [x] T015 [US3] Implementar `GetStudyHistoryQueryHandler` em `CogniLink.Application/Dashboard/Queries/GetStudyHistory/GetStudyHistoryQueryHandler.cs`: resolver `ownerId`; buscar `IStudySessionRepository.ListByOwnerAsync` e expandir com `IStudyAnswerRepository.ListBySessionIdAsync` por sessão; filtrar respostas com `AnsweredAt` dentro de `[From, To]`; agrupar por `DateOnly.FromDateTime(AnsweredAt)` somando `AnswersCount`, `CorrectCount`, `IncorrectCount` (`AnswersCount - CorrectCount`) e `TotalTimeSeconds`; gerar a série completa do período (um `StudyHistoryEntryDto` por dia entre `From` e `To` inclusive, zerando os dias sem resposta) ordenada cronologicamente. Depende de T001, T002, T008, T013, T014.

**Checkpoint**: As três queries funcionam de forma independente via `IMediator.Send(...)`, cada uma retornando seu DTO correspondente.

---

## Fase 4: Presentation

- [x] T016 [US1] Criar `CogniLink.Api/Controllers/DashboardController.cs` com `[ApiController] [Route("api")] [Authorize]` (mesmo padrão de `StudySessionsController`, injetando `IMediator` no construtor) e adicionar a ação `[HttpGet("dashboard")]` que envia `GetDashboardQuery` e retorna `Ok(result)`, com `[ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]` e `[ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]`.
- [x] T017 [US2] Adicionar a ação `[HttpGet("decks/{deckId}/statistics")]` em `CogniLink.Api/Controllers/DashboardController.cs` que envia `GetDeckStatisticsQuery(deckId)` e retorna `Ok(result)` (o 404 de `NotFoundException` é tratado pelo `ExceptionHandlingMiddleware` já existente, sem try/catch no controller), com `[ProducesResponseType(typeof(DeckStatisticsDto), StatusCodes.Status200OK)]`, `401` e `404`. Depende de T016 (mesmo arquivo).
- [x] T018 [US3] Adicionar a ação `[HttpGet("analytics/study-history")]` em `CogniLink.Api/Controllers/DashboardController.cs`, recebendo `[FromQuery] DateOnly? from` e `[FromQuery] DateOnly? to`, enviando `GetStudyHistoryQuery(from, to)` e retornando `Ok(result)` (o 400 de `ValidationException` é tratado pelo `ExceptionHandlingMiddleware` já existente), com `[ProducesResponseType(typeof(IReadOnlyList<StudyHistoryEntryDto>), StatusCodes.Status200OK)]`, `400` e `401`. Depende de T017 (mesmo arquivo).

**Checkpoint**: Os 3 endpoints de `contracts/dashboard.openapi.yaml` estão expostos e navegáveis via Swagger.

---

## Fase 5: Validação Manual

- [ ] T019 Executar todos os cenários de `specs/005-dashboard-metrics/quickstart.md` via Swagger contra os 3 endpoints implementados, confirmando em especial que `currentStreak`/`longestStreak`/`dueFlashcardsCount` do dashboard coincidem com `GET /api/users/me/streak` e `GET /api/reviews/due` (feature 004).

---

## Dependencies & Execution Order

### Ordem entre fases

- **Fase 1 (Infrastructure)**: sem dependências — pode começar imediatamente. Bloqueia todas as queries da Fase 3.
- **Fase 2 (Domain)**: sem dependências — pode rodar em paralelo à Fase 1. Bloqueia o cálculo de mastery na Fase 3.
- **Fase 3 (Application)**: depende da Fase 1 e da Fase 2 completas.
- **Fase 4 (Presentation)**: depende da Fase 3 completa (precisa dos DTOs e das queries já implementadas).
- **Fase 5 (Validação Manual)**: depende da Fase 4 completa.

### Dependências dentro da Fase 3

- T005-T008 (DTOs) podem ser feitos em paralelo entre si, exceto T007 que depende de T006.
- GetDashboard (T009-T010) depende apenas de T005 + Fases 1 e 2.
- GetDeckStatistics (T011-T012) depende de T006, T007 + Fases 1 e 2.
- GetStudyHistory (T013-T015) depende de T008 + Fase 1 (não depende da Fase 2/mastery).
- As três queries (US1, US2, US3) são independentes entre si e podem ser implementadas em paralelo por pessoas diferentes uma vez concluídas as Fases 1 e 2.

### Dentro da Fase 4

- T016, T017 e T018 editam o mesmo arquivo (`DashboardController.cs`) e por isso são sequenciais, não paralelos, apesar de corresponderem a user stories diferentes.

---

## Parallel Example: Fases 1 e 2 (podem rodar juntas)

```bash
Task: "T001 Adicionar ListByOwnerAsync à interface IStudySessionRepository"
Task: "T003 Registrar IFlashcardRepository no DI"
Task: "T004 Adicionar IsMastered() a UserFlashcardProgress"
```

## Parallel Example: DTOs da Fase 3

```bash
Task: "T005 Criar DashboardSummaryDto"
Task: "T006 Criar PerformanceGroupDto"
Task: "T008 Criar StudyHistoryEntryDto"
```

---

## Implementation Strategy

### MVP First (User Story 1 — resumo global)

1. Completar Fase 1 (Infrastructure) e Fase 2 (Domain) — podem rodar em paralelo.
2. Completar T005 (DTO) + T009-T010 (GetDashboard) na Fase 3.
3. Completar T016 na Fase 4 (endpoint `GET /api/dashboard`).
4. **PARAR e VALIDAR**: testar `GET /api/dashboard` isoladamente via Swagger antes de prosseguir.

### Entrega Incremental

1. Fases 1+2 → fundação pronta.
2. GetDashboard (US1) → validar isoladamente → MVP do dashboard.
3. GetDeckStatistics (US2) → validar isoladamente.
4. GetStudyHistory (US3) → validar isoladamente.
5. Fase 5 → roteiro completo de `quickstart.md`.

---

## Notes

- [P] = arquivos diferentes, sem dependência de tarefa incompleta.
- [Story] rastreia a tarefa até o user story do spec.md, mesmo com a organização por camada.
- Sem tarefas de teste automatizado nesta etapa, por restrição explícita de escopo.
- Fazer commit após cada tarefa ou grupo lógico concluído.
- T002, T003 e T012 tocam repositórios/DI já existentes — revisar com atenção para não quebrar comportamento das features 001-004 (em especial `GetDueFlashcardsQueryHandler` e `GetUserStreakQueryHandler`, que continuam usando `ListCompletedByOwnerAsync` sem alteração).
