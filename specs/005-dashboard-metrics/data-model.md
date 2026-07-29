# Data Model: Dashboard e Métricas

Esta feature não cria nenhuma entidade de escrita. Ela adiciona um método de domínio a uma entidade já existente e define visões (DTOs) somente-leitura derivadas de entidades das features 002-004.

## Alteração em entidade existente: `UserFlashcardProgress`

| Membro novo | Tipo | Regra |
|---|---|---|
| `IsMastered()` | método, retorna `bool` | `Repetitions >= 2 && IntervalDays >= 21`. Centraliza a definição de mastery usada por `GetDashboard` e `GetDeckStatistics`. |

Nenhum campo é adicionado à entidade; apenas um método derivado dos campos já existentes (`Repetitions`, `IntervalDays`).

## Visões somente-leitura (Application DTOs)

### DashboardSummaryDto

Resumo global de progresso do usuário autenticado (RF-01).

| Campo | Tipo | Origem |
|---|---|---|
| `MasteryPercentage` | `double` | `(flashcards com IsMastered() == true) / (total de flashcards elegíveis do usuário) × 100`, arredondado a 1 casa decimal; `0` se não houver flashcards. |
| `TotalStudyTimeSeconds` | `long` | Soma de `TimeToAnswerSeconds` de todas as `StudyAnswer` do usuário. |
| `CompletedCardsCount` | `int` | Contagem de `FlashcardId` distintos entre as `StudyAnswer` do usuário. |
| `RetentionRate` | `double` | `% de StudyAnswer com IsCorrect = true`, arredondado a 1 casa decimal; `0` se não houver respostas. |
| `CurrentStreak` | `int` | Reaproveitado de `GetUserStreakQuery` (feature 004). |
| `LongestStreak` | `int` | Reaproveitado de `GetUserStreakQuery` (feature 004). |
| `DueFlashcardsCount` | `int` | Contagem do resultado de `GetDueFlashcardsQuery` (feature 004). |

### DeckStatisticsDto

Estatísticas de um baralho de propriedade do usuário autenticado (RF-02).

| Campo | Tipo | Origem |
|---|---|---|
| `DeckId` | `string` | Baralho consultado. |
| `TotalFlashcards` | `int` | `IFlashcardRepository.ListByDeckIdAsync(deckId).Count`. |
| `DueFlashcardsCount` | `int` | Resultado de `GetDueFlashcardsQuery` filtrado por `DeckId == deckId`. |
| `DeckMasteryPercentage` | `double` | Mesma definição de mastery de `DashboardSummaryDto`, restrita aos flashcards do baralho; `0` se o baralho não tiver flashcards. |
| `PerformanceByDifficulty` | `IReadOnlyList<PerformanceGroupDto>` | Respostas do baralho agrupadas por `Flashcard.Difficulty`. |
| `PerformanceBySubarea` | `IReadOnlyList<PerformanceGroupDto>` | Respostas do baralho agrupadas por `Flashcard.Subarea` (`"Sem subárea"` quando nulo). |
| `PerformanceByType` | `IReadOnlyList<PerformanceGroupDto>` | Respostas do baralho agrupadas por `Flashcard.Type`. |

### PerformanceGroupDto

Item reutilizável de agrupamento de desempenho (usado nos três agrupamentos de `DeckStatisticsDto`).

| Campo | Tipo | Regra |
|---|---|---|
| `GroupKey` | `string` | Nome do grupo (ex.: `"Facil"`, `"Cloze"`, `"Sem subárea"`). |
| `TotalAnswers` | `int` | Total de `StudyAnswer` dos flashcards desse grupo, dentro do baralho. |
| `CorrectAnswers` | `int` | Total de `StudyAnswer` com `IsCorrect = true` dentro do grupo. |
| `AccuracyRate` | `double` | `CorrectAnswers / TotalAnswers × 100`, arredondado a 1 casa decimal; `0` se `TotalAnswers == 0`. |

### StudyHistoryEntryDto

Item de série diária do histórico de estudo por período (RF-03).

| Campo | Tipo | Regra |
|---|---|---|
| `Date` | `DateOnly` | Um item por dia do intervalo `[from, to]`, inclusive, mesmo sem atividade. |
| `AnswersCount` | `int` | Quantidade de `StudyAnswer` do usuário com `AnsweredAt` nesse dia. |
| `CorrectCount` | `int` | Quantidade com `IsCorrect = true` nesse dia. |
| `IncorrectCount` | `int` | `AnswersCount - CorrectCount`. |
| `TotalTimeSeconds` | `long` | Soma de `TimeToAnswerSeconds` das respostas do dia. |

## Relacionamentos reaproveitados (sem alteração)

- `Deck (1) -> (N) Flashcard`, ownership via `Deck.OwnerId`.
- `StudySession (1) -> (N) StudyAnswer`, `StudySession.OwnerId` é o único vínculo de propriedade de uma resposta (via sessão).
- `Flashcard (1) -> (N) StudyAnswer`, `Flashcard.Difficulty`/`Subarea`/`Type` usados para agrupamento de desempenho.
- `User + Flashcard (1) -> (1) UserFlashcardProgress`, base do cálculo de mastery e pendências.

## Consultas-chave (todas somente-leitura, sem nova query composta)

- Mastery geral: `IDeckRepository.GetByOwnerAsync(ownerId)` → `IFlashcardRepository.ListByDeckIdAsync(deckId)` por baralho → `IUserFlashcardProgressRepository.ListByOwnerAsync(ownerId)` indexado por `FlashcardId` → `IsMastered()` por flashcard elegível.
- Agregação de respostas do usuário: `IStudySessionRepository.ListByOwnerAsync(ownerId)` (novo método, todos os status) → `IStudyAnswerRepository.ListBySessionIdAsync(sessionId)` por sessão → flatten em memória.
- Estatísticas de baralho: mesma agregação de respostas, restrita às sessões cujo `DeckId == deckId`; flashcards do baralho via `ListByDeckIdAsync(deckId)`.
- Histórico por período: mesma agregação de respostas do usuário, filtrando `AnsweredAt` dentro de `[from, to]` e agrupando por `DateOnly.FromDateTime(AnsweredAt)`.
- Streak e pendências: reaproveitados via `IMediator.Send(GetUserStreakQuery)` / `IMediator.Send(GetDueFlashcardsQuery)`, sem reimplementação.
