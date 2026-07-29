# Tasks: Autenticacao e Gestao de Perfil

**Input**: Design documents from `/specs/001-auth-profile-management/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/auth-profile.openapi.yaml](./contracts/auth-profile.openapi.yaml), [quickstart.md](./quickstart.md)

**Tests**: Não solicitados explicitamente na especificação; nenhuma tarefa de teste automatizado foi incluída. A validação funcional é feita via [quickstart.md](./quickstart.md).

**Organization**: Tarefas agrupadas por user story (US1–US4, conforme spec.md) para permitir implementação e teste independentes de cada uma.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependência de tarefas incompletas)
- **[Story]**: US1, US2, US3 ou US4
- Caminhos de arquivo exatos incluídos em cada descrição

## Path Conventions

Projeto único em Clean Architecture (.NET 10), conforme [plan.md](./plan.md#project-structure):
`CogniLink.Api/`, `CogniLink.Application/`, `CogniLink.Domain/`, `CogniLink.Infrastructure/`, `tests/`

---

## Phase 1: Setup (Bootstrap da solução — Bloco 0)

**Purpose**: Inicialização da solução .NET 10 e infraestrutura compartilhada (só ocorre nesta primeira feature)

- [X] T001 Criar solução `CogniLink.sln` e os projetos `CogniLink.Api`, `CogniLink.Application`, `CogniLink.Domain`, `CogniLink.Infrastructure` na raiz do repositório
- [X] T002 Configurar as referências entre projetos: `CogniLink.Api` → `CogniLink.Application` → `CogniLink.Domain`; `CogniLink.Infrastructure` → `CogniLink.Application` + `CogniLink.Domain` (depends on T001)
- [X] T003 [P] Adicionar pacotes NuGet aos projetos correspondentes: MediatR, FluentValidation, Mapster, NLog.Web.AspNetCore, Google.Cloud.Firestore, Swashbuckle.AspNetCore, Microsoft.AspNetCore.Authentication.JwtBearer (depends on T001)
- [X] T004 Configurar `CogniLink.Api/Program.cs` com DI, NLog, Swagger, autenticação JWT Bearer, política de CORS explícita e redirecionamento HTTPS (depends on T002, T003)
- [X] T005 [P] Configurar conexão inicial ao Firestore em `CogniLink.Api/appsettings.json` e `appsettings.Development.json`, com credenciais via user-secrets/variáveis de ambiente, nunca hardcoded (depends on T001)

**Checkpoint**: Solução compila, sobe com Swagger disponível e conexão Firestore configurada.

---

## Phase 2: Foundational (Domain, interfaces e serviços compartilhados — Bloco 1 + base do Bloco 3)

**Purpose**: Infraestrutura núcleo que TODAS as user stories exigem (entidades base, contratos de repositório/serviço, hashing, JWT, resolução do usuário autenticado)

**⚠️ CRITICAL**: Nenhuma user story pode iniciar antes desta fase estar completa

- [X] T006 [P] Criar entidade `User` em `CogniLink.Domain/Entities/User.cs` (e-mail único validado na Application, senha nunca em texto puro, apenas hash) (depends on T004)
- [X] T007 [P] Criar entidade `RefreshToken` em `CogniLink.Domain/Entities/RefreshToken.cs` (expiração, uso único, estado de rotação/revogação) (depends on T004)
- [X] T008 [P] Criar entidade `UserPreference` em `CogniLink.Domain/Entities/UserPreference.cs` (tema de preferência com valor padrão) (depends on T004)
- [X] T009 Definir interfaces em `CogniLink.Application/Common/Interfaces/` (`IUserRepository`, `IRefreshTokenRepository`, `IPasswordHasher`, `IJwtTokenGenerator`, `ICurrentUserService`, `IEmailSender`) (depends on T006, T007, T008)
- [X] T010 [P] Configurar pipeline do MediatR, comportamento de validação do FluentValidation e registro do Mapster na extensão de DI de `CogniLink.Application` (depends on T009)
- [X] T011 [P] Implementar `PasswordHasher` em `CogniLink.Infrastructure/Security/PasswordHasher.cs` usando algoritmo de hash adaptativo (depends on T009)
- [X] T012 [P] Implementar `JwtTokenGenerator` em `CogniLink.Infrastructure/Security/JwtTokenGenerator.cs` (emissão e validação de access token com claim `userId`) (depends on T009)
- [X] T013 [P] Implementar `CurrentUserService` em `CogniLink.Infrastructure/Security/CurrentUserService.cs`, extraindo `userId` do `ClaimsPrincipal` autenticado (depends on T009)
- [X] T014 [P] Implementar `FirestoreUserRepository` em `CogniLink.Infrastructure/Persistence/Firestore/FirestoreUserRepository.cs` (depends on T009)
- [X] T015 [P] Implementar `FirestoreRefreshTokenRepository` em `CogniLink.Infrastructure/Persistence/Firestore/FirestoreRefreshTokenRepository.cs` (depends on T009)
- [X] T016 Registrar os serviços de Application e Infrastructure no container de DI em `CogniLink.Api/Program.cs` (depends on T010, T011, T012, T013, T014, T015)
- [X] T017 Configurar middleware global de tratamento de erros e logging de eventos de segurança via NLog em `CogniLink.Api` (sem registrar senhas, tokens completos ou outros segredos) (depends on T016)

**Checkpoint**: Fundação pronta — implementação das user stories pode começar.

---

## Phase 3: User Story 1 - Criar Conta e Acessar Dados Proprios (Priority: P1) 🎯 MVP

**Goal**: Visitante se cadastra, entra na conta e passa a ter uma sessão autenticada vinculada exclusivamente aos seus próprios dados; login com e-mail inexistente/senha incorreta e cadastro com e-mail duplicado retornam respostas genéricas.

**Independent Test**: Cadastrar um visitante, autenticar com as credenciais criadas e confirmar que a sessão retornada pertence somente a essa conta; repetir cadastro com o mesmo e-mail e login com credenciais inválidas devem retornar erros genéricos sem revelar detalhes.

### Implementation for User Story 1

- [X] T018 [P] [US1] Criar `RegisterUserCommand` + `RegisterUserCommandValidator` (nome obrigatório, e-mail em formato válido, senha forte) em `CogniLink.Application/Auth/Commands/RegisterUser/`
- [X] T019 [US1] Implementar `RegisterUserCommandHandler`: validar unicidade de e-mail, gerar hash de senha, criar `User` + `UserPreference` padrão, emitir access token e refresh token, em `CogniLink.Application/Auth/Commands/RegisterUser/RegisterUserCommandHandler.cs` (depends on T018)
- [X] T020 [P] [US1] Criar `LoginCommand` + `LoginCommandValidator` em `CogniLink.Application/Auth/Commands/Login/`
- [X] T021 [US1] Implementar `LoginCommandHandler`: autenticar por e-mail/senha retornando erro genérico idêntico para e-mail inexistente ou senha incorreta, emitir novos tokens em caso de sucesso, em `CogniLink.Application/Auth/Commands/Login/LoginCommandHandler.cs` (depends on T020)
- [X] T022 [P] [US1] Criar DTOs `RegisterRequest`, `LoginRequest`, `AuthTokensResponse` e o profile de mapeamento Mapster correspondente em `CogniLink.Application/Common/Mappings/`
- [X] T023 [US1] Implementar `AuthController` com os endpoints `POST /api/auth/register` e `POST /api/auth/login` em `CogniLink.Api/Controllers/AuthController.cs` (depends on T019, T021, T022)
- [X] T024 [US1] Adicionar anotações Swagger para os endpoints de registro e login em `CogniLink.Api/Controllers/AuthController.cs` (depends on T023)

**Checkpoint**: Cadastro, login e isolamento básico de identidade funcionam de forma independente e testável.

---

## Phase 4: User Story 2 - Manter e Encerrar Sessao (Priority: P1)

**Goal**: Usuário autenticado renova a sessão via refresh token com rotação (uso único) e pode encerrar a sessão, revogando o refresh token ativo.

**Independent Test**: Após login (US1), renovar a sessão com o refresh token recebido e confirmar que o token anterior deixa de funcionar; encerrar a sessão e confirmar que o refresh token revogado não renova mais a sessão.

### Implementation for User Story 2

- [X] T025 [P] [US2] Criar `RefreshSessionCommand` em `CogniLink.Application/Auth/Commands/RefreshSession/`
- [X] T026 [US2] Implementar `RefreshSessionCommandHandler`: validar que o refresh token não está expirado, revogado ou já utilizado, aplicar rotação (invalidar o antigo, emitir novo par de tokens) em `CogniLink.Application/Auth/Commands/RefreshSession/RefreshSessionCommandHandler.cs` (depends on T025)
- [X] T027 [P] [US2] Criar `LogoutCommand` em `CogniLink.Application/Auth/Commands/Logout/`
- [X] T028 [US2] Implementar `LogoutCommandHandler`: revogar o refresh token ativo do usuário autenticado em `CogniLink.Application/Auth/Commands/Logout/LogoutCommandHandler.cs` (depends on T027)
- [X] T029 [US2] Implementar no `AuthController` os endpoints `POST /api/auth/refresh` e `POST /api/auth/logout` (logout exige Bearer token) em `CogniLink.Api/Controllers/AuthController.cs` (depends on T026, T028)
- [X] T030 [US2] Adicionar anotações Swagger para os endpoints de refresh e logout em `CogniLink.Api/Controllers/AuthController.cs` (depends on T029)

**Checkpoint**: Renovação de sessão com rotação e logout funcionam de forma independente e testável.

---

## Phase 5: User Story 3 - Recuperar e Alterar Senha (Priority: P2)

**Goal**: Usuário esquecido de senha solicita redefinição por e-mail com token de uso único e prazo limitado; usuário autenticado troca a senha informando a senha atual.

**Independent Test**: Solicitar redefinição de senha, usar o token único recebido para definir nova senha e confirmar que a senha antiga deixa de funcionar e o token não pode ser reutilizado; trocar a senha autenticado informando corretamente a senha atual.

### Implementation for User Story 3

- [X] T031 [P] [US3] Criar entidade `PasswordResetToken` em `CogniLink.Domain/Entities/PasswordResetToken.cs`
- [X] T032 [P] [US3] Definir interface `IPasswordResetTokenRepository` em `CogniLink.Application/Common/Interfaces/IPasswordResetTokenRepository.cs`
- [X] T033 [US3] Implementar `FirestorePasswordResetTokenRepository` em `CogniLink.Infrastructure/Persistence/Firestore/FirestorePasswordResetTokenRepository.cs` (depends on T031, T032)
- [X] T034 [P] [US3] Implementar `EmailSender` (stub/log no MVP, documentado como pendência) em `CogniLink.Infrastructure/Email/EmailSender.cs` (depends on T032)
- [X] T035 [P] [US3] Criar `RequestPasswordResetCommand` em `CogniLink.Application/Auth/Commands/RequestPasswordReset/`
- [X] T036 [US3] Implementar `RequestPasswordResetCommandHandler`: gerar token de uso único com expiração e enviar via `IEmailSender`, retornando resposta genérica independentemente de o e-mail existir, em `CogniLink.Application/Auth/Commands/RequestPasswordReset/RequestPasswordResetCommandHandler.cs` (depends on T033, T034, T035)
- [X] T037 [P] [US3] Criar `ResetPasswordCommand` + `ResetPasswordCommandValidator` (senha forte) em `CogniLink.Application/Auth/Commands/ResetPassword/`
- [X] T038 [US3] Implementar `ResetPasswordCommandHandler`: validar token não expirado/não usado, definir novo hash de senha, marcar token como usado, em `CogniLink.Application/Auth/Commands/ResetPassword/ResetPasswordCommandHandler.cs` (depends on T033, T037)
- [X] T039 [P] [US3] Criar `ChangePasswordCommand` + `ChangePasswordCommandValidator` (senha atual obrigatória, nova senha forte) em `CogniLink.Application/Auth/Commands/ChangePassword/`
- [X] T040 [US3] Implementar `ChangePasswordCommandHandler`: verificar senha atual do usuário autenticado antes de atualizar o hash, em `CogniLink.Application/Auth/Commands/ChangePassword/ChangePasswordCommandHandler.cs` (depends on T039)
- [X] T041 [US3] Implementar no `AuthController` os endpoints `POST /api/auth/forgot-password`, `POST /api/auth/reset-password` e `PUT /api/auth/change-password` em `CogniLink.Api/Controllers/AuthController.cs` (depends on T036, T038, T040)
- [X] T042 [US3] Adicionar anotações Swagger para os endpoints de forgot-password, reset-password e change-password em `CogniLink.Api/Controllers/AuthController.cs` (depends on T041)

**Checkpoint**: Recuperação e troca de senha funcionam de forma independente e testável.

---

## Phase 6: User Story 4 - Gerenciar Perfil e Preferencias (Priority: P2)

**Goal**: Usuário autenticado consulta e atualiza nome, e-mail, foto opcional e tema de preferência, sem afetar dados de outra conta.

**Independent Test**: Consultar o perfil próprio, atualizar cada campo permitido e confirmar que as mudanças persistem apenas na própria conta; tentar usar um e-mail de outra conta deve ser negado sem expor dados de terceiros.

### Implementation for User Story 4

- [X] T043 [P] [US4] Criar `UpdateProfileCommand` + `UpdateProfileCommandValidator` (e-mail em formato válido, checagem de conflito de e-mail único) em `CogniLink.Application/Profile/Commands/UpdateProfile/`
- [X] T044 [US4] Implementar `UpdateProfileCommandHandler`: resolver o proprietário via `ICurrentUserService` e atualizar apenas nome/e-mail/foto/tema da própria conta, em `CogniLink.Application/Profile/Commands/UpdateProfile/UpdateProfileCommandHandler.cs` (depends on T043)
- [X] T045 [P] [US4] Criar `GetProfileQuery` em `CogniLink.Application/Profile/Queries/GetProfile/`
- [X] T046 [US4] Implementar `GetProfileQueryHandler`: resolver o proprietário via `ICurrentUserService` e retornar somente o perfil da própria conta, em `CogniLink.Application/Profile/Queries/GetProfile/GetProfileQueryHandler.cs` (depends on T045)
- [X] T047 [P] [US4] Criar DTOs `ProfileResponse`, `UpdateProfileRequest` e o profile de mapeamento Mapster correspondente em `CogniLink.Application/Common/Mappings/`
- [X] T048 [US4] Implementar `ProfileController` com os endpoints `GET /api/profile` e `PUT /api/profile` em `CogniLink.Api/Controllers/ProfileController.cs` (depends on T044, T046, T047)
- [X] T049 [US4] Adicionar anotações Swagger para os endpoints de perfil em `CogniLink.Api/Controllers/ProfileController.cs` (depends on T048)

**Checkpoint**: Todas as user stories estão funcionais e testáveis independentemente.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Revisões que afetam múltiplas user stories

- [X] T050 [P] Revisar logs NLog em todos os fluxos de autenticação/perfil para confirmar que nenhuma senha, token completo ou segredo é registrado
- [X] T051 [P] Validar política de CORS explícita, redirecionamento HTTPS e configuração JWT contra as restrições de segurança da constituição
- [X] T052 Executar a validação de [quickstart.md](./quickstart.md) cobrindo as 4 user stories de ponta a ponta
- [X] T053 [P] Revisar a completude da documentação Swagger para os 8 endpoints do contrato

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — pode iniciar imediatamente
- **Foundational (Phase 2)**: Depende da conclusão do Setup — BLOQUEIA todas as user stories
- **User Stories (Phase 3-6)**: Todas dependem da conclusão da fase Foundational
  - US1 (P1) e US2 (P1) devem ser priorizadas primeiro (MVP)
  - US2 depende funcionalmente do login implementado em US1 (reutiliza o fluxo de autenticação)
  - US3 (P2) e US4 (P2) podem prosseguir em paralelo entre si após US1/US2, mas dependem apenas da fase Foundational
- **Polish (Phase 7)**: Depende de todas as user stories desejadas estarem completas

### User Story Dependencies

- **US1 (P1)**: Pode iniciar após Foundational (Phase 2) — sem dependência de outras stories
- **US2 (P1)**: Pode iniciar após Foundational, mas reutiliza o `LoginCommand`/tokens emitidos por US1 para ser exercitada de ponta a ponta
- **US3 (P2)**: Pode iniciar após Foundational — sem dependência direta de US1/US2 além da entidade `User` já existente
- **US4 (P2)**: Pode iniciar após Foundational — sem dependência direta de US1/US2/US3 além da entidade `User`/`UserPreference` já existentes

### Within Each User Story

- Commands/Queries e seus validators antes dos handlers
- Handlers antes dos endpoints do controller
- Endpoint implementado antes das anotações Swagger
- Story completa antes de avançar para a próxima prioridade

### Parallel Opportunities

- Todas as tarefas de Setup marcadas [P] podem rodar em paralelo
- Todas as tarefas Foundational marcadas [P] podem rodar em paralelo (entidades de Domain e implementações de Infrastructure independentes entre si)
- Após a fase Foundational, US3 e US4 podem ser desenvolvidas em paralelo por desenvolvedores diferentes
- Dentro de cada story, commands/queries marcados [P] (arquivos distintos) podem ser criados em paralelo

---

## Parallel Example: User Story 1

```bash
# Lançar os commands e DTOs de User Story 1 em paralelo:
Task: "Criar RegisterUserCommand + RegisterUserCommandValidator em CogniLink.Application/Auth/Commands/RegisterUser/"
Task: "Criar LoginCommand + LoginCommandValidator em CogniLink.Application/Auth/Commands/Login/"
Task: "Criar DTOs RegisterRequest, LoginRequest, AuthTokensResponse e profile Mapster em CogniLink.Application/Common/Mappings/"
```

## Parallel Example: Foundational

```bash
# Lançar as entidades de Domain em paralelo:
Task: "Criar entidade User em CogniLink.Domain/Entities/User.cs"
Task: "Criar entidade RefreshToken em CogniLink.Domain/Entities/RefreshToken.cs"
Task: "Criar entidade UserPreference em CogniLink.Domain/Entities/UserPreference.cs"

# Após T009, lançar as implementações de Infrastructure em paralelo:
Task: "Implementar PasswordHasher em CogniLink.Infrastructure/Security/PasswordHasher.cs"
Task: "Implementar JwtTokenGenerator em CogniLink.Infrastructure/Security/JwtTokenGenerator.cs"
Task: "Implementar CurrentUserService em CogniLink.Infrastructure/Security/CurrentUserService.cs"
Task: "Implementar FirestoreUserRepository em CogniLink.Infrastructure/Persistence/Firestore/FirestoreUserRepository.cs"
Task: "Implementar FirestoreRefreshTokenRepository em CogniLink.Infrastructure/Persistence/Firestore/FirestoreRefreshTokenRepository.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 + User Story 2)

1. Completar Phase 1: Setup
2. Completar Phase 2: Foundational (CRITICAL — bloqueia todas as stories)
3. Completar Phase 3: User Story 1 (cadastro, login, isolamento básico)
4. Completar Phase 4: User Story 2 (refresh com rotação, logout)
5. **STOP and VALIDATE**: testar US1 + US2 independentemente via [quickstart.md](./quickstart.md#1-cadastro-e-isolamento-de-dados-us1) e [quickstart.md](./quickstart.md#2-sessao-e-logout-us2)
6. Deploy/demo do MVP de autenticação e sessão

### Incremental Delivery

1. Setup + Foundational → fundação pronta
2. US1 (register + login) → testar independentemente → MVP inicial
3. US2 (refresh + logout) → testar independentemente → sessão completa
4. US3 (recuperação/troca de senha) → testar independentemente
5. US4 (perfil e preferências) → testar independentemente
6. Cada story adiciona valor sem quebrar as anteriores

### Parallel Team Strategy

Com múltiplos desenvolvedores:

1. Equipe completa Setup + Foundational em conjunto
2. Após Foundational:
   - Desenvolvedor A: User Story 1 → User Story 2 (sequenciais, pois US2 reutiliza o login de US1)
   - Desenvolvedor B: User Story 3
   - Desenvolvedor C: User Story 4
3. Stories completam e integram de forma independente

---

## Notes

- [P] tasks = arquivos diferentes, sem dependências entre si
- [Story] label mapeia a tarefa à user story correspondente para rastreabilidade
- Nenhuma tarefa de teste automatizado foi incluída, pois não foi solicitada na especificação; validação end-to-end é feita via [quickstart.md](./quickstart.md)
- Fazer commit após cada tarefa ou grupo lógico de tarefas
- Parar em qualquer checkpoint para validar a story isoladamente
- Evitar: tarefas vagas, conflitos no mesmo arquivo entre tarefas paralelas, dependências entre stories que quebrem a independência
