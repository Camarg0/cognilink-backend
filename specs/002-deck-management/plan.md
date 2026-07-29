# Implementation Plan: Gestão de Baralhos (Decks)

**Branch**: `002-deck-management` | **Date**: 2026-07-29 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-deck-management/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command; its definition describes the execution workflow.

## Summary

Implementar CRUD de baralhos (decks) pertencentes ao usuário autenticado: criação, listagem com busca por nome e paginação, obtenção de detalhes, edição e exclusão, com isolamento estrito por `OwnerId` e resposta 404 (nunca 403) para recursos de outros usuários. A feature reutiliza integralmente a infraestrutura compartilhada já criada na feature 001 (bootstrap do `Program.cs`, DI de `Application`/`Infrastructure`, autenticação JWT, `ICurrentUserService`, conexão `FirestoreDb`, NLog, Swagger, Mapster, FluentValidation, MediatR/`ValidationBehavior`, `ExceptionHandlingMiddleware`) — nenhum componente de bootstrap é recriado, apenas estendido com os artefatos próprios de Decks (entidade `Deck`, commands/queries, repositório Firestore, controller, DTOs).

## Technical Context

**Language/Version**: C# / .NET 10 (ASP.NET Core) — mesma solução `CogniLink.slnx` da feature 001

**Primary Dependencies**: MediatR (CQRS, reaproveitando `ValidationBehavior` já registrado), FluentValidation, Mapster, NLog, Google.Cloud.Firestore (mesma instância `FirestoreDb` injetada em `AddInfrastructure`), Swashbuckle/Swagger (mesmo pipeline do `Program.cs`), autenticação JWT já configurada (nenhuma dependência nova é introduzida)

**Storage**: Firebase Firestore — nova coleção `decks`, documentos com `ownerId` como campo indexado para filtro por proprietário; sem banco relacional, sem EF Core, sem migrations

**Testing**: xUnit — testes unitários de validators (FluentValidation) e handlers (Application, com `IDeckRepository` fake/mocked) seguindo o padrão de `CogniLink.Application.Tests`; testes de integração dos 5 endpoints seguindo o padrão de `CogniLink.Api.Tests` (mesma estrutura de testes da feature 001, ainda a ser criada caso não exista)

**Target Platform**: Serviço web Linux/containers, exposto via HTTPS (mesmo host da feature 001)

**Project Type**: Web service — extensão da mesma API em Clean Architecture (Api/Application/Domain/Infrastructure) da feature 001, sem novos projetos na solução

**Performance Goals**: Até 2s de resposta para criação, listagem paginada, detalhe, edição e exclusão sob carga esperada do MVP (alinhado ao SC-004 do spec)

**Constraints**: HTTPS obrigatório (herdado do bootstrap); toda rota autenticada via `[Authorize]` e `ICurrentUserService`; isolamento de proprietário reforçado em Application antes de tocar Infrastructure (FR-006); busca por nome (contains, case-insensitive) e paginação resolvidas em memória sobre o conjunto de baralhos do próprio dono, pois o Firestore não oferece nativamente `contains`/paginação por offset em texto — mantém-se aceitável dado o volume esperado por usuário (dezenas a poucas centenas de baralhos no MVP); nenhuma regra de negócio em Infrastructure (repositório apenas mapeia documentos)

**Scale/Scope**: MVP para estudantes universitários; escopo desta feature limitado a 5 endpoints de gestão de baralhos, sem flashcards associados (contagem de cards fixa em 0), sem exclusão em cascata, sem estatísticas de desempenho

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Camadas Clean Architecture**: `Deck` (Domain) sem dependências externas; `Application` define `IDeckRepository` e commands/queries que dependem apenas de Domain + Application.Common; `Infrastructure` implementa `FirestoreDeckRepository` dependendo de Application/Domain; `Api` depende de Application via MediatR. Nenhuma camada nova introduzida. PASS.
- **Isolamento por proprietário como regra de domínio**: todo command/query de Decks resolve `userId` via `ICurrentUserService` (já existente) e filtra por `OwnerId` em Application antes de chamar `IDeckRepository`; acesso a baralho de outro usuário retorna `NotFoundException` (mapeada para 404 pelo `ExceptionHandlingMiddleware` existente), nunca 403 (FR-006). PASS.
- **Sem regras de negócio em Infrastructure**: `FirestoreDeckRepository` apenas mapeia `Deck` ↔ documento Firestore, sem validação de propriedade ou de dados — essas regras residem em Domain (invariantes da entidade) e Application (validators/handlers). PASS.
- **Sem EF Core/migrations relacionais, Firestore como único banco**: nova coleção `decks` acessada via `Google.Cloud.Firestore`, reaproveitando a mesma `FirestoreDb` já registrada em `AddInfrastructure`. PASS.
- **Sem exposição de chaves de IA ao frontend, CORS explícito**: feature não introduz integração de IA nem configuração de CORS adicional; reaproveita a política `Default` já definida no `Program.cs`. PASS.
- **Sem features especulativas fora do escopo definido**: flashcards associados, exclusão em cascata e estatísticas de desempenho ficam explicitamente fora de escopo (spec), não implementados nesta feature. PASS.

Nenhuma violação identificada; Complexity Tracking permanece vazio.

**Re-avaliação pós-design (Fase 1)**: research.md, data-model.md, contracts/decks.openapi.yaml e quickstart.md foram revisados contra os mesmos gates acima. Nenhuma nova violação introduzida: `Deck` permanece uma entidade pura de Domain; o contrato de API não expõe dados de outros usuários nem chaves de IA; isolamento por proprietário reforçado em todos os 5 endpoints do contrato, com 404 uniforme para recursos de terceiros. PASS.

## Project Structure

### Documentation (this feature)

```text
specs/002-deck-management/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
CogniLink.Api/
├── Controllers/
│   └── DecksController.cs        # POST /decks, GET /decks, GET /decks/{id}, PUT /decks/{id}, DELETE /decks/{id}
├── Contracts/
│   └── Requests.cs               # + CreateDeckRequest, UpdateDeckRequest, ListDecksRequest (query params)
└── Program.cs                     # inalterado — bootstrap já cobre DI, JWT, CORS, Swagger, NLog, HTTPS

CogniLink.Application/
├── Decks/
│   ├── Commands/
│   │   ├── CreateDeck/ (CreateDeckCommand, CreateDeckCommandValidator, CreateDeckCommandHandler)
│   │   ├── UpdateDeck/ (UpdateDeckCommand, UpdateDeckCommandValidator, UpdateDeckCommandHandler)
│   │   └── DeleteDeck/ (DeleteDeckCommand, DeleteDeckCommandHandler)
│   └── Queries/
│       ├── ListDecks/ (ListDecksQuery, ListDecksQueryValidator, ListDecksQueryHandler, PagedResult<DeckDto>)
│       └── GetDeckById/ (GetDeckByIdQuery, GetDeckByIdQueryHandler)
├── Common/
│   ├── Interfaces/
│   │   └── IDeckRepository.cs    # novo — segue o padrão de IUserRepository
│   ├── Exceptions/                # reaproveita NotFoundException já existente (ou equivalente da feature 001)
│   └── Mappings/                  # + Mapster config/DTO de Deck, se necessário além do Adapt<> direto
└── DependencyInjection.cs         # inalterado — MediatR/FluentValidation já fazem scan da assembly

CogniLink.Domain/
└── Entities/
    └── Deck.cs                    # novo — Id, OwnerId, Name, Description, Difficulty (enum), Categories (List<string>), CreatedAt, UpdatedAt
    └── Enums/
        └── DeckDifficulty.cs      # novo — Facil, Medio, Dificil

CogniLink.Infrastructure/
├── Persistence/Firestore/
│   └── FirestoreDeckRepository.cs # novo — coleção "decks", mesmo padrão de FirestoreUserRepository/FirestoreRefreshTokenRepository
└── DependencyInjection.cs         # + registro de IDeckRepository -> FirestoreDeckRepository

tests/
├── CogniLink.Domain.Tests/        # + regras de Deck (invariantes de criação/edição)
├── CogniLink.Application.Tests/   # + validators e handlers de Decks (IDeckRepository fake/mocked)
└── CogniLink.Api.Tests/           # + testes de integração dos 5 endpoints de /api/decks
```

**Structure Decision**: Extensão do mesmo projeto único em Clean Architecture (.NET 10) já estabelecido na feature 001 — `CogniLink.Api`, `CogniLink.Application`, `CogniLink.Domain`, `CogniLink.Infrastructure`. Nenhum novo projeto, nenhuma nova configuração de bootstrap; apenas novas pastas/arquivos dentro das camadas existentes, seguindo exatamente a convenção de nomes e organização (Commands/Queries por caso de uso, Repositories por entidade) já usada em Auth/Profile.

## Complexity Tracking

> Nenhuma violação da Constitution Check identificada; seção mantida vazia intencionalmente.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |
