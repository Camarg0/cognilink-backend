# Implementation Plan: Dashboard e Métricas

**Branch**: `005-dashboard-metrics` | **Date**: 2026-07-29 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/005-dashboard-metrics/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command; its definition describes the execution workflow.

## Summary

Implementar dashboard e métricas de progresso do CogniLink como funcionalidade inteiramente somente-leitura: resumo global do usuário (mastery, tempo de estudo, cards concluídos, retenção, streak, pendências), estatísticas de um baralho próprio (total de flashcards, pendências, mastery do baralho, desempenho por dificuldade/subárea/tipo) e histórico diário de estudo por período. A implementação reutiliza integralmente a infraestrutura e os padrões das features 001-004 (bootstrap, DI, JWT/`ICurrentUserService`, Firestore, NLog, Swagger, Mapster, MediatR) e os repositórios já existentes, sem criar entidades de escrita nem repositórios novos — apenas três novas Queries CQRS, um método de domínio (`UserFlashcardProgress.IsMastered()`), uma extensão mínima de `IStudySessionRepository` (`ListByOwnerAsync`) e a correção de um registro de DI ausente (`IFlashcardRepository`) sem o qual a feature não pode funcionar.

## Technical Context

**Language/Version**: C# / .NET 10 (ASP.NET Core)

**Primary Dependencies**: MediatR (CQRS), FluentValidation, Google.Cloud.Firestore, NLog, Swashbuckle/Swagger, JWT bearer auth, ICurrentUserService

**Storage**: Firebase Firestore (leitura das coleções já existentes `decks`, `flashcards`, `studySessions`, `studyAnswers`, `userFlashcardProgress`; nenhuma coleção nova)

**Testing**: Validação manual via Swagger/HTTP nesta etapa; sem testes automatizados por restrição explícita de escopo

**Target Platform**: API HTTPS ASP.NET Core no mesmo host/solução do backend CogniLink

**Project Type**: Web service em Clean Architecture (Api/Application/Domain/Infrastructure)

**Performance Goals**: Resumo global do dashboard exibido em menos de 2 segundos (SC-005); agregações feitas em memória sobre respostas obtidas por fan-out de sessões do usuário (ver research.md item 5), aceitável para o volume esperado de um MVP acadêmico

**Constraints**: Reutilizar integralmente padrões das features 001-004 sem redescrever bootstrap; nenhuma entidade de escrita nova; nenhum repositório novo (apenas um método adicional em `IStudySessionRepository` e a correção do registro de DI de `IFlashcardRepository`); todas as consultas restritas por `OwnerId`/ownership; sem gráficos, exportação de relatórios ou recomendações por IA; sem testes automatizados nesta etapa

**Scale/Scope**: 3 endpoints de leitura (dashboard, estatísticas de baralho, histórico), 3 novas Queries MediatR, 1 método de domínio novo, 1 método de repositório novo, mantendo escopo MVP da quinta e última feature planejada do MVP

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Regra central de agendamento no servidor**: feature não escreve nem recalcula agendamento SM-2; apenas lê `Repetitions`/`IntervalDays` já calculados por `UserFlashcardProgress.ApplyReview` (feature 004) através do novo método de leitura `IsMastered()`. PASS.
- **Isolamento por proprietário**: `GetDashboard` resolve `ownerId` via `ICurrentUserService` e filtra todas as consultas por esse `ownerId`; `GetDeckStatistics` valida ownership do baralho via `IDeckRepository.GetByIdAsync` + comparação de `OwnerId` antes de agregar, retornando `NotFoundException` (404) em caso de baralho inexistente ou de outro usuário, sem vazar informação de existência. PASS.
- **Clean Architecture e dependências**: Domain ganha apenas um método derivado puro (`IsMastered()`), sem dependência externa; Application concentra toda a agregação (soma, contagem, agrupamento, série diária) em três Queries CQRS via MediatR; Infrastructure não recebe nenhuma regra de negócio, apenas um método de leitura adicional idêntico ao padrão já existente; Api expõe os três endpoints seguindo a mesma convenção de `StudySessionsController`. PASS.
- **Stack obrigatória mantida**: JWT, `ICurrentUserService`, Firestore, NLog, Swagger, MediatR e FluentValidation (validação do período do histórico) reaproveitados integralmente, sem redescrição de bootstrap/DI; Mapster não é necessário para os novos DTOs (mesmo padrão já observado em `GetDueFlashcardsQueryHandler`/`GetUserStreakQueryHandler`, que constroem DTOs diretamente). PASS.
- **Escopo MVP respeitado**: dashboard é a quinta e última etapa do MVP definida na constitution ("autenticação → decks → flashcards → sessões de estudo/SM-2 → dashboard"); gráficos, exportação de relatórios e recomendações por IA permanecem fora de escopo, conforme FR-016. PASS.
- **Correção de gap pré-existente**: `IFlashcardRepository` nunca foi registrado no container de DI (`DependencyInjection.cs`/`Program.cs`), apesar de `FirestoreFlashcardRepository` existir desde a feature 003 e já ser dependência de `GetDueFlashcardsQueryHandler` (feature 004). Esta feature adiciona esse registro por ser pré-requisito funcional direto de `GetDeckStatistics`; é uma correção de uma linha, não introduz repositório novo nem viola a restrição de escopo. PASS, com nota registrada (não é uma violação de constitution, mas um requisito de infraestrutura necessário para a entrega funcionar).

Nenhuma violação da Constitution Check identificada; Complexity Tracking permanece sem entradas.

**Re-avaliação pós-design (Fase 1)**: os artefatos `research.md`, `data-model.md`, `contracts/dashboard.openapi.yaml` e `quickstart.md` mantêm os mesmos gates: nenhuma entidade de escrita nova, ownership estrito por `OwnerId`/`Deck.OwnerId`, reaproveitamento de `GetUserStreak`/`GetDueFlashcards` via MediatR (sem duplicação de lógica), stack reaproveitada e escopo sem funcionalidades especulativas. PASS.

## Project Structure

### Documentation (this feature)

```text
specs/005-dashboard-metrics/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── dashboard.openapi.yaml
└── tasks.md
```

### Source Code (repository root)

```text
CogniLink.Domain/
└── Entities/
    └── UserFlashcardProgress.cs        # + método IsMastered() (feature 004, editado nesta feature)

CogniLink.Application/
├── Common/
│   ├── Interfaces/
│   │   └── IStudySessionRepository.cs  # + ListByOwnerAsync(ownerId, ct) (todos os status)
│   └── Models/
│       ├── DashboardSummaryDto.cs
│       ├── DeckStatisticsDto.cs
│       ├── PerformanceGroupDto.cs
│       └── StudyHistoryEntryDto.cs
└── Dashboard/
    └── Queries/
        ├── GetDashboard/
        │   ├── GetDashboardQuery.cs
        │   └── GetDashboardQueryHandler.cs
        ├── GetDeckStatistics/
        │   ├── GetDeckStatisticsQuery.cs
        │   └── GetDeckStatisticsQueryHandler.cs
        └── GetStudyHistory/
            ├── GetStudyHistoryQuery.cs
            ├── GetStudyHistoryQueryHandler.cs
            └── GetStudyHistoryQueryValidator.cs

CogniLink.Infrastructure/
├── Persistence/Firestore/
│   └── FirestoreStudySessionRepository.cs  # + implementação de ListByOwnerAsync
└── DependencyInjection.cs                  # + registro de IFlashcardRepository (gap pré-existente)

CogniLink.Api/
└── Controllers/
    └── DashboardController.cs              # GET /api/dashboard, GET /api/decks/{deckId}/statistics,
                                             # GET /api/analytics/study-history
```

**Structure Decision**: Estender as mesmas camadas e convenções das features 001-004, sem criar novos projetos e sem redescrever bootstrap/DI/JWT/Firestore/NLog/Swagger/MediatR/FluentValidation. A feature adiciona apenas artefatos de Dashboard (Queries, DTOs, controller), um método de domínio e uma extensão mínima de repositório existente, mantendo validação manual nesta etapa e sem estrutura de testes automatizados no plano.

## Complexity Tracking

> Nenhuma violação da Constitution Check identificada; seção mantida vazia intencionalmente.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |
