# Quickstart: Dashboard e Métricas

Feature: [spec.md](./spec.md) | Plan: [plan.md](./plan.md) | Contrato: [contracts/dashboard.openapi.yaml](./contracts/dashboard.openapi.yaml)

Guia de validação manual end-to-end da feature 005, sem testes automatizados.

## Pré-requisitos

- .NET 10 SDK instalado.
- Features 001-004 operacionais (autenticação, decks, flashcards, sessões de estudo SM-2).
- API em execução com JWT, Firestore, Swagger, NLog e MediatR já configurados.
- Usuário autenticado com token JWT válido.
- Ao menos um baralho próprio com flashcards de dificuldades/subáreas/tipos variados.
- Histórico de estudo com respostas registradas em dias distintos (via `POST /api/study-sessions/{sessionId}/answers` da feature 004), incluindo pelo menos:
  - Um flashcard já "dominado" (`Repetitions >= 2` e `IntervalDays >= 21`).
  - Um flashcard nunca respondido (para validar mastery/pendências parciais).

## Setup

```powershell
dotnet restore
dotnet build
dotnet run --project CogniLink.Api
```

Abrir Swagger em `https://localhost:<porta>/swagger`.

## Endpoints cobertos

- `GET /api/dashboard`
- `GET /api/decks/{deckId}/statistics`
- `GET /api/analytics/study-history?from=&to=`

## Cenários de validação

### 1. Resumo global (RF-01, US1)

1. Consultar `GET /api/dashboard` com usuário que possui histórico de estudo.
   - Esperado: `200` com `masteryPercentage`, `totalStudyTimeSeconds`, `completedCardsCount`, `retentionRate`, `currentStreak`, `longestStreak` e `dueFlashcardsCount` preenchidos.
   - Validar manualmente: `currentStreak`/`longestStreak` batem com `GET /api/users/me/streak`; `dueFlashcardsCount` bate com a contagem de `GET /api/reviews/due`.
2. Consultar `GET /api/dashboard` com um usuário novo, sem flashcards nem respostas.
   - Esperado: `200` com `masteryPercentage = 0` e `retentionRate = 0`, sem erro.
3. Repetir a consulta autenticado como outro usuário.
   - Esperado: valores refletem apenas os dados desse outro usuário, nunca do primeiro.

### 2. Estatísticas de baralho (RF-02, US2)

1. Consultar `GET /api/decks/{deckId}/statistics` para baralho próprio com flashcards de dificuldades/subáreas/tipos variados e histórico de respostas.
   - Esperado: `200` com `totalFlashcards`, `dueFlashcardsCount`, `deckMasteryPercentage` e os três agrupamentos de desempenho (`performanceByDifficulty`, `performanceBySubarea`, `performanceByType`) coerentes com os dados cadastrados.
2. Consultar com `deckId` inexistente.
   - Esperado: `404`.
3. Consultar `deckId` de um baralho de outro usuário.
   - Esperado: `404` (sem distinção de mensagem em relação ao caso anterior).
4. Consultar baralho próprio recém-criado, sem nenhum flashcard.
   - Esperado: `200` com `deckMasteryPercentage = 0` e listas de desempenho vazias, sem erro.

### 3. Histórico de estudo por período (RF-03, US3)

1. Consultar `GET /api/analytics/study-history?from=2026-07-01&to=2026-07-29` com respostas registradas em dias distintos desse intervalo.
   - Esperado: `200` com uma série diária ordenada cronologicamente; dias sem atividade aparecem com contadores zerados (continuidade da série).
2. Consultar sem informar `from` ou sem informar `to`.
   - Esperado: `400`.
3. Consultar com `from` posterior a `to`.
   - Esperado: `400`.
4. Consultar com intervalo maior que 1 ano entre `from` e `to`.
   - Esperado: `400`.
5. Consultar um período válido sem nenhuma resposta registrada.
   - Esperado: `200` com a série completa do período, todos os dias zerados, sem erro.

## Revisão manual recomendada

- Revisar `UserFlashcardProgress.IsMastered()` isoladamente, já que é a regra central de mastery reaproveitada por `GetDashboard` e `GetDeckStatistics`.
- Confirmar que `services.AddScoped<IFlashcardRepository, FirestoreFlashcardRepository>()` foi adicionado ao DI antes de testar `GetDeckStatistics` e `GET /api/reviews/due` (feature 004) — sem esse registro, ambos falham em runtime.
- Validar que `currentStreak`, `longestStreak` e `dueFlashcardsCount` do dashboard nunca divergem dos mesmos valores retornados pelos endpoints já existentes da feature 004.
