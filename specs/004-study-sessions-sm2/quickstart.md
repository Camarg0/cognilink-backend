# Quickstart: Study Sessions SM-2

Feature: [spec.md](./spec.md) | Plan: [plan.md](./plan.md) | Contrato: [contracts/study-sessions.openapi.yaml](./contracts/study-sessions.openapi.yaml)

Guia de validacao manual end-to-end da feature 004 sem testes automatizados.

## Pre-requisitos

- .NET 10 SDK instalado.
- Features 001, 002 e 003 operacionais (auth, decks e flashcards).
- API em execucao com JWT, Firestore, Swagger, NLog, Mapster, FluentValidation e MediatR ja configurados.
- Usuario autenticado com token JWT valido.
- Deck proprio com conjunto de flashcards para estudo (incluindo ao menos 1 sem progresso e 1 vencido).

## Setup

```powershell
dotnet restore
dotnet build
dotnet run --project CogniLink.Api
```

Abrir Swagger em `https://localhost:<porta>/swagger`.

## Endpoints cobertos

- `POST /api/study-sessions`
- `GET /api/study-sessions/{sessionId}/next-card`
- `POST /api/study-sessions/{sessionId}/answers`
- `POST /api/study-sessions/{sessionId}/complete`
- `POST /api/study-sessions/{sessionId}/cancel`
- `GET /api/reviews/due`
- `GET /api/users/me/streak`

## Cenarios de validacao

### 1. Iniciar sessao (RF-01)

1. Iniciar sessao para deck proprio com cards.
- Esperado: `201` com `sessionId`, `status=InProgress` e `startedAt`.
2. Iniciar para deck inexistente.
- Esperado: `404`.
3. Iniciar para deck de outro usuario.
- Esperado: `404`.
4. Iniciar para deck proprio sem flashcards.
- Esperado: `422` (erro de negocio).

### 2. Obter proximo card (RF-02)

1. Chamar `GET /api/study-sessions/{sessionId}/next-card` repetidamente.
- Esperado: cards nunca revisados primeiro (`nextReviewDate = null`).
- Esperado: depois cards vencidos (`nextReviewDate <= agora`) em ordem ascendente.
- Esperado: cada retorno inclui `attemptId` unico da tentativa.

### 3. Registrar resposta e SM-2 (RF-03, RF-04, RF-05)

1. Enviar resposta correta para um `attemptId` valido.
- Esperado: `200` com campos de progresso recalculados (`easeFactor`, `intervalDays`, `nextReviewDate`, `repetitions`).
2. Enviar resposta incorreta para outro `attemptId` valido.
- Esperado: `200` com `repetitions` reiniciado, `intervalDays=1` e `easeFactor >= 1.3`.
3. Reenviar exatamente o mesmo payload do passo 1 com mesmo `attemptId`.
- Esperado: `200` idempotente, sem novo registro e sem novo recalculo.

### 4. Concluir sessao (RF-06)

1. Concluir sessao apos responder N cards.
- Esperado: `200` com `cardsStudied = N`, `accuracyRate` e `durationSeconds`.

### 5. Cancelar sessao (RF-06)

1. Iniciar nova sessao e responder apenas parte dos cards.
2. Cancelar sessao.
- Esperado: `200`/`204` conforme contrato; cards ainda nao respondidos permanecem sem novo recalculo.

### 6. Streak e pendencias agregadas (RF-07, RF-08)

1. Consultar `GET /api/users/me/streak` apos sessoes concluidas em dias distintos.
- Esperado: `currentStreak` e `longestStreak` coerentes com as datas.
2. Consultar `GET /api/reviews/due`.
- Esperado: lista agregada de cards pendentes do usuario em todos os decks.

## Revisao manual recomendada

- Revisar cuidadosamente a logica de `UserFlashcardProgress.ApplyReview(isCorrect)` por ser o nucleo academico SM-2 da plataforma.
- Validar em especial: piso de `easeFactor`, transicao de `intervalDays` e idempotencia por `attemptId`.
