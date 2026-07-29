# Implementation Plan: Autenticacao e Gestao de Perfil

**Branch**: `001-auth-profile-management` | **Date**: 2026-07-28 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-auth-profile-management/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command; its definition describes the execution workflow.

## Summary

Implementar cadastro, login com JWT + refresh token rotativo, logout, recuperacao/troca de senha e gestao de perfil (nome, e-mail, foto opcional, tema), com isolamento estrito de dados por proprietario. A solucao segue Clean Architecture em .NET 10/ASP.NET Core (Api, Application, Domain, Infrastructure), CQRS via MediatR, validacao com FluentValidation, mapeamento com Mapster, persistencia em Firebase Firestore (sem EF Core/migrations relacionais), logging com NLog e documentacao via Swagger. Esta e a primeira feature do projeto, portanto inclui o bootstrap da solucao e da infraestrutura compartilhada (DI, autenticacao JWT, CORS, HTTPS, conexao Firestore) que as proximas features reutilizarao.

## Technical Context

**Language/Version**: C# / .NET 10 (ASP.NET Core)

**Primary Dependencies**: MediatR (CQRS), FluentValidation, Mapster, NLog, Google.Cloud.Firestore (Firebase Admin SDK), Swashbuckle/Swagger, Microsoft.AspNetCore.Authentication.JwtBearer

**Storage**: Firebase Firestore (NoSQL/documentos); sem banco relacional, sem EF Core, sem migrations

**Testing**: xUnit para testes unitarios (Domain/Application) e de integracao (Api); FluentValidation testado via testes unitarios dos validators

**Target Platform**: Servico web Linux/containers, exposto via HTTPS

**Project Type**: Web service (API unica em Clean Architecture: Api/Application/Domain/Infrastructure)

**Performance Goals**: Ate 2s de resposta para login, refresh, consulta e atualizacao de perfil sob carga esperada do MVP (alinhado ao SC-002 do spec)

**Constraints**: HTTPS obrigatorio; CORS explicito (sem wildcard); nenhuma chave de IA ou segredo exposto ao frontend; senha nunca armazenada ou trafegada em texto puro no Domain; refresh token de uso unico com rotacao; isolamento de proprietario reforcado em toda rota autenticada (FR-012, FR-013)

**Scale/Scope**: MVP para estudantes universitarios; escopo desta feature limitado a autenticacao e perfil (8 endpoints), servindo de base de infraestrutura para as demais features do produto (decks, flashcards, sessoes de estudo)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Camadas Clean Architecture**: Api depende de Application depende de Domain; Infrastructure depende de Application e Domain; Domain permanece livre de dependencias externas. PASS (estrutura definida na secao Project Structure).
- **Isolamento por proprietario como regra de dominio**: `ICurrentUserService` resolve o `userId` autenticado e toda query/command de Application filtra e valida propriedade antes de tocar Infrastructure; FR-012/FR-013 cobrem isso explicitamente. PASS.
- **Sem regras de negocio em Infrastructure**: hashing, geracao/validacao de JWT e acesso a Firestore em Infrastructure sao mecanismos, nao regras (ex.: politica de senha forte fica em Domain/Application via FluentValidation). PASS.
- **Sem EF Core/migrations relacionais, Firestore como unico banco**: Repositorios de Infrastructure usam Google.Cloud.Firestore diretamente. PASS.
- **Sem exposicao de chaves de IA ao frontend, CORS explicito**: Nao ha integracao de IA nesta feature; CORS e configurado explicitamente no bootstrap do Bloco 0. PASS.
- **Sem features especulativas fora do escopo definido**: Login social, 2FA e verificacao de e-mail por link ficam fora de escopo (conforme spec), documentados como melhorias futuras, nao implementados. PASS.

Nenhuma violacao identificada; Complexity Tracking permanece vazio.

**Re-avaliacao pos-design (Fase 1)**: research.md, data-model.md, contracts/auth-profile.openapi.yaml e quickstart.md foram revisados contra os mesmos gates acima. Nenhuma nova violacao introduzida: entidades de Domain (`User`, `RefreshToken`, `PasswordResetToken`, `UserPreference`) permanecem puras; contrato de API nao expoe segredos nem chaves de IA; isolamento por proprietario reforcado em `/profile` e em todo endpoint autenticado do contrato. PASS.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
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
│   ├── AuthController.cs        # register, login, refresh, logout, forgot-password, reset-password, change-password
│   └── ProfileController.cs     # GET/PUT profile
├── Program.cs                    # DI, NLog, Swagger, JWT auth, CORS, HTTPS redirection
└── appsettings.json / appsettings.*.json  # Firestore + JWT config (secrets via user-secrets/env, nunca hardcoded)

CogniLink.Application/
├── Auth/
│   ├── Commands/
│   │   ├── RegisterUser/ (Command, Validator, Handler)
│   │   ├── Login/ (Command, Validator, Handler)
│   │   ├── RefreshSession/ (Command, Handler)
│   │   ├── Logout/ (Command, Handler)
│   │   ├── RequestPasswordReset/ (Command, Handler)
│   │   ├── ResetPassword/ (Command, Validator, Handler)
│   │   └── ChangePassword/ (Command, Validator, Handler)
├── Profile/
│   ├── Commands/UpdateProfile/ (Command, Validator, Handler)
│   └── Queries/GetProfile/ (Query, Handler)
├── Common/
│   ├── Interfaces/ (IUserRepository, IRefreshTokenRepository, IPasswordHasher, IJwtTokenGenerator, ICurrentUserService, IEmailSender)
│   └── Mappings/ (Mapster profiles/DTOs)

CogniLink.Domain/
├── Entities/ (User, RefreshToken, PasswordResetToken, UserPreference)
└── ValueObjects/ (ex.: Email, PasswordHash, se aplicavel)

CogniLink.Infrastructure/
├── Persistence/Firestore/ (FirestoreUserRepository, FirestoreRefreshTokenRepository)
├── Security/ (JwtTokenGenerator, PasswordHasher, CurrentUserService)
└── Email/ (EmailSender - stub/log no MVP, documentado como pendencia)

tests/
├── CogniLink.Domain.Tests/       # regras de User, RefreshToken, UserPreference
├── CogniLink.Application.Tests/  # handlers + validators (contract-level, com repositorios fake/mocked)
└── CogniLink.Api.Tests/          # testes de integracao dos endpoints de auth/profile
```

**Structure Decision**: Projeto unico em Clean Architecture (.NET 10) com quatro camadas — `CogniLink.Api`, `CogniLink.Application`, `CogniLink.Domain`, `CogniLink.Infrastructure` — seguindo a dependencia Api→Application→Domain e Infrastructure→Application+Domain definida na constituicao. Nao ha frontend nesta feature (consumido futuramente por um cliente separado), portanto a Option 2 (web app com frontend) nao se aplica; a estrutura acima e uma especializacao da Option 1 (single project) adaptada a Clean Architecture em .NET.

## Complexity Tracking

> Nenhuma violacao da Constitution Check identificada; secao mantida vazia intencionalmente.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |
