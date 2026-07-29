# Implementation Plan: Gestao de Flashcards

**Branch**: `003-flashcard-management` | **Date**: 2026-07-29 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-flashcard-management/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command; its definition describes the execution workflow.

## Summary

Implementar gestao de flashcards proprios com 5 operacoes (criar, listar por deck, obter por id, editar e excluir), com isolamento estrito por proprietario e retorno 404 para deck/flashcard inexistente ou sem ownership do usuario autenticado. A feature segue integralmente o padrao ja estabelecido nas features 001 e 002 (bootstrap, DI, JWT/ICurrentUserService, Firestore, NLog, Swagger, Mapster, FluentValidation, MediatR, repositores e controllers), sem redesenhar infraestrutura; apenas acrescenta os artefatos de Flashcard em Domain/Application/Infrastructure/Api.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# / .NET 10 (ASP.NET Core)

**Primary Dependencies**: MediatR (CQRS), FluentValidation, Mapster, Google.Cloud.Firestore, NLog, Swashbuckle/Swagger, JWT bearer auth e ICurrentUserService ja existentes

**Storage**: Firebase Firestore com nova colecao `flashcards` (um documento por flashcard)

**Testing**: Validacao manual via Swagger/HTTP nesta etapa; sem testes automatizados por decisao explicita de escopo

**Target Platform**: API HTTPS (ASP.NET Core) no mesmo host da solucao CogniLink

**Project Type**: Web service em Clean Architecture (Api/Application/Domain/Infrastructure)

**Performance Goals**: Operacoes de CRUD e listagem com resposta dentro de SLA de API do MVP, mantendo listagem por deck eficiente com filtro indexado por `deckId`

**Constraints**: Reutilizar integralmente infraestrutura e padroes das features 001/002; validar ownership via deck antes de tocar repositrio de flashcard; retorno 404 em recurso inexistente/alheio; sem SM-2, historico de tentativas, IA semantica ou versionamento

**Scale/Scope**: Escopo limitado a 5 endpoints de flashcards e validacoes bloqueantes por tipo (FrenteVerso, Cloze, DigiteResposta, MultiplaEscolha)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Clean Architecture preservada**: `Flashcard` e `Alternative` no Domain (classes puras), CQRS e interfaces no Application, implementacao Firestore no Infrastructure e endpoints/DTOs no Api. PASS.
- **Isolamento por proprietario como regra de dominio/aplicacao**: toda operacao resolve o `Deck` por `IDeckRepository` com `ownerId` do usuario autenticado antes de qualquer leitura/escrita em `IFlashcardRepository`; deck inexistente/alheio retorna NotFound -> 404. PASS.
- **Regras de negocio fora da Infrastructure**: validacao por tipo reside no Domain (metodo de validacao do agregado) e e reforcada por FluentValidation no Application; repositrio Firestore apenas persiste/consulta documentos. PASS.
- **Stack e infraestrutura obrigatorias reaproveitadas**: JWT, ICurrentUserService, Firestore, NLog, Swagger, Mapster, MediatR e padrao de controller/repositrio seguem o ja existente sem redescricao de bootstrap. PASS.
- **Sem features especulativas fora de escopo**: sem agendamento SM-2, sem historico de tentativas, sem IA semantica, sem versionamento de flashcard nesta feature. PASS.

Nenhuma violacao identificada; Complexity Tracking permanece sem entradas.

**Re-avaliacao pos-design (Fase 1)**: os artefatos `research.md`, `data-model.md`, `contracts/flashcards.openapi.yaml` e `quickstart.md` mantem os mesmos gates: ownership garantido por deck, regras de tipo no Domain + Application, Firestore como armazenamento unico e ausencia de escopo especulativo. PASS.

## Project Structure

### Documentation (this feature)

```text
specs/003-flashcard-management/
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
│   └── Flashcard.cs                 # entidade unica com campos comuns e tipados opcionais
├── ValueObjects/
│   └── Alternative.cs               # { Text, IsCorrect }
└── Enums/
  ├── FlashcardType.cs             # FrenteVerso, Cloze, DigiteResposta, MultiplaEscolha
  └── FlashcardDifficulty.cs       # Facil, Medio, Dificil

CogniLink.Application/
├── Common/
│   ├── Interfaces/
│   │   └── IFlashcardRepository.cs
│   └── Models/
│       └── FlashcardDto.cs
└── Flashcards/
  ├── Commands/
  │   ├── CreateFlashcard/
  │   ├── UpdateFlashcard/
  │   └── DeleteFlashcard/
  └── Queries/
    ├── ListFlashcardsByDeck/
    └── GetFlashcardById/

CogniLink.Infrastructure/
├── Persistence/Firestore/
│   └── FirestoreFlashcardRepository.cs
└── DependencyInjection.cs           # + registro IFlashcardRepository

CogniLink.Api/
├── Controllers/
│   └── FlashcardsController.cs
└── Contracts/
  └── Requests.cs                  # + DTOs de request/response para flashcards
```

**Structure Decision**: Estender as mesmas camadas e convencoes das features 001/002 (mesma solucao e mesmo bootstrap), adicionando apenas artefatos de Flashcard dentro dos projetos existentes. Sem novos projetos, sem nova infraestrutura e sem estrutura de testes automatizados nesta etapa.

## Complexity Tracking

> Nenhuma violacao da Constitution Check identificada; secao mantida vazia intencionalmente.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |
