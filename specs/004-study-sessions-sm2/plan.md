# Implementation Plan: Study Sessions SM-2

**Branch**: `004-study-sessions-sm2` | **Date**: 2026-07-29 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/004-study-sessions-sm2/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command; its definition describes the execution workflow.

## Summary

Implementar sessoes de estudo com repeticao espacada SM-2 para decks proprios, cobrindo inicio de sessao, obtencao do proximo card pendente, submissao idempotente de resposta por `attemptId`, conclusao/cancelamento com resumo, streak de estudo e listagem agregada de cards pendentes. A implementacao reutiliza integralmente a infraestrutura e os padroes estabelecidos nas features 001-003 (bootstrap, DI, JWT/ICurrentUserService, Firestore, NLog, Swagger, Mapster, FluentValidation, MediatR, repositrios e controllers), introduzindo apenas novos artefatos de dominio/casos de uso/repositorios/controlador desta feature, sem redescrever os componentes existentes.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# / .NET 10 (ASP.NET Core)

**Primary Dependencies**: MediatR (CQRS), FluentValidation, Mapster, Google.Cloud.Firestore, NLog, Swashbuckle/Swagger, JWT bearer auth, ICurrentUserService

**Storage**: Firebase Firestore (colecoes para `studySessions`, `studyAnswers`, `userFlashcardProgress`)

**Testing**: Validacao manual via Swagger/HTTP nesta etapa; sem testes automatizados por restricao explicita de escopo

**Target Platform**: API HTTPS ASP.NET Core no mesmo host/solucao do backend CogniLink

**Project Type**: Web service em Clean Architecture (Api/Application/Domain/Infrastructure)

**Performance Goals**: Obter proximo card e registrar resposta com latencia compativel ao uso interativo de sessao; consultas de pendencias por usuario suportadas por indices de consulta

**Constraints**: Reutilizar integralmente padroes das features 001-003 sem redescrever bootstrap; validar ownership antes de tocar progresso/sessao; idempotencia obrigatoria por `attemptId`; calculo SM-2 exclusivamente no servidor; sem testes automatizados nesta etapa

**Scale/Scope**: 7 endpoints de sessao/revisao/streak, 3 novas entidades de dominio e 3 repositorios Firestore dedicados, mantendo escopo MVP da quarta feature

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Regra central de agendamento no servidor**: `UserFlashcardProgress.ApplyReview(isCorrect)` concentra o algoritmo SM-2 no Domain; cliente apenas informa resultado da tentativa (`isCorrect`, tempo, dicas) sem decidir `nextReviewDate`, `intervalDays` ou `easeFactor`. PASS.
- **Isolamento por proprietario**: todos os casos de uso validam ownership via `IDeckRepository`/`IFlashcardRepository` ja existentes antes de tocar `StudySession`/`StudyAnswer`/`UserFlashcardProgress`. PASS.
- **Clean Architecture e dependencias**: Domain permanece puro; Application coordena CQRS com MediatR e validacoes; Infrastructure implementa Firestore repositories; Api expoe endpoints autenticados seguindo padrao atual. PASS.
- **Stack obrigatoria mantida**: JWT, ICurrentUserService, Firestore, NLog, Swagger, Mapster, FluentValidation e MediatR reaproveitados integralmente, sem redescricao de bootstrap/DI. PASS.
- **Escopo MVP respeitado**: fora de escopo mantido (validacao semantica por IA, recomendacoes por localizacao, dashboard consolidado). PASS.

Nenhuma violacao identificada; Complexity Tracking permanece sem entradas.

**Re-avaliacao pos-design (Fase 1)**: os artefatos `research.md`, `data-model.md`, `contracts/study-sessions.openapi.yaml` e `quickstart.md` mantem os mesmos gates: SM-2 em dominio, ownership estrito, stack reaproveitada e sem escopo especulativo. PASS.

## Project Structure

### Documentation (this feature)

```text
specs/004-study-sessions-sm2/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
└── tasks.md
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
CogniLink.Domain/
├── Entities/
│   ├── StudySession.cs
│   ├── StudyAnswer.cs
│   └── UserFlashcardProgress.cs
└── Enums/
  └── StudySessionStatus.cs

CogniLink.Application/
├── Common/
│   ├── Interfaces/
│   │   ├── IStudySessionRepository.cs
│   │   ├── IStudyAnswerRepository.cs
│   │   └── IUserFlashcardProgressRepository.cs
│   └── Models/
│       ├── StudySessionDto.cs
│       ├── StudyAnswerResultDto.cs
│       ├── DueFlashcardDto.cs
│       └── UserStreakDto.cs
└── StudySessions/
  ├── Commands/
  │   ├── StartStudySession/
  │   ├── SubmitAnswer/
  │   ├── CompleteStudySession/
  │   └── CancelStudySession/
  └── Queries/
    ├── GetNextCard/
    ├── GetDueFlashcards/
    └── GetUserStreak/

CogniLink.Infrastructure/
├── Persistence/Firestore/
│   ├── FirestoreStudySessionRepository.cs
│   ├── FirestoreStudyAnswerRepository.cs
│   └── FirestoreUserFlashcardProgressRepository.cs
└── DependencyInjection.cs           # + registros das novas interfaces

CogniLink.Api/
├── Controllers/
│   └── StudySessionsController.cs
└── Contracts/
  └── Requests.cs                  # + DTOs de request/response da feature
```

**Structure Decision**: Estender as mesmas camadas e convencoes das features 001-003, sem criar novos projetos e sem redescrever bootstrap/DI/JWT/Firestore/NLog/Swagger/Mapster/FluentValidation/MediatR. A feature adiciona apenas artefatos de Study Sessions, mantendo validacao manual nesta etapa e sem estrutura de testes automatizados no plano.

## Complexity Tracking

> Nenhuma violacao da Constitution Check identificada; secao mantida vazia intencionalmente.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |
