# Quickstart: Autenticacao e Gestao de Perfil

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Contrato**: [contracts/auth-profile.openapi.yaml](./contracts/auth-profile.openapi.yaml)

Guia de validacao end-to-end para confirmar que a feature funciona conforme as historias de usuario do spec. Nao contem codigo de implementacao.

## Pre-requisitos

- .NET 10 SDK instalado.
- Projeto Firestore configurado (ou emulador do Firestore) com credenciais disponiveis via `dotnet user-secrets` ou variaveis de ambiente (nunca hardcoded em `appsettings.json`).
- Solucao `CogniLink.sln` com os projetos `CogniLink.Api`, `CogniLink.Application`, `CogniLink.Domain`, `CogniLink.Infrastructure` restaurados (`dotnet restore`).
- Politica de senha forte e tempos de expiracao (access token, refresh token, token de reset) configurados em `appsettings`/secrets, conforme Assumptions do spec.

## Setup

```powershell
dotnet restore
dotnet build
dotnet run --project CogniLink.Api
```

A API deve subir em HTTPS com Swagger disponivel (ex.: `https://localhost:<porta>/swagger`).

## Cenarios de validacao (mapeados as User Stories do spec)

### 1. Cadastro e isolamento de dados (US1)

1. `POST /api/auth/register` com `name`, `email` novo e `password` forte.
   - Esperado: `201` com `accessToken` e `refreshToken` (AuthTokens).
2. Repetir o cadastro com o mesmo `email`.
   - Esperado: `409` com mensagem generica, sem confirmar que o e-mail ja existe como texto explicito de "conta encontrada".
3. `POST /api/auth/login` com `email` inexistente e depois com `email` existente + senha errada.
   - Esperado: ambas as respostas sao `401` com a mesma mensagem generica (FR-011).
4. Criar duas contas (A e B). Autenticado como A, tentar acessar/alterar um recurso cujo identificador pertence a B (quando os endpoints de recursos de estudo existirem) ou validar via teste de integracao que `ICurrentUserService` bloqueia qualquer chamada cujo `userId` resolvido nao seja o dono do recurso.
   - Esperado: acesso negado, sem vazar dados de B (FR-012, FR-013).

### 2. Sessao e logout (US2)

1. `POST /api/auth/login` com credenciais validas.
   - Esperado: `200` com `accessToken` (curta duracao) e `refreshToken` (longa duracao).
2. `POST /api/auth/refresh` com o `refreshToken` recebido.
   - Esperado: `200` com novo par de tokens; o `refreshToken` anterior deixa de funcionar (repetir o passo 2 com o token antigo deve retornar `401`).
3. `POST /api/auth/logout` com um `refreshToken` valido (header `Authorization: Bearer <accessToken>`).
   - Esperado: `204`; tentar `POST /api/auth/refresh` com esse `refreshToken` em seguida retorna `401`.

### 3. Recuperacao e troca de senha (US3)

1. `POST /api/auth/forgot-password` com um `email` cadastrado.
   - Esperado: `202`; token de reset gerado e "enviado" (log/stub do `EmailSender` no MVP).
2. `POST /api/auth/reset-password` com o token gerado e uma nova senha forte.
   - Esperado: `200`; login subsequente com a senha antiga falha (`401`) e com a nova senha funciona (`200`).
3. Repetir `POST /api/auth/reset-password` com o mesmo token ja usado.
   - Esperado: `401` (token ja utilizado).
4. `PUT /api/auth/change-password` autenticado, informando `currentPassword` correta e `newPassword` forte.
   - Esperado: `200`; repetir com `currentPassword` incorreta retorna `401`.

### 4. Perfil e preferencias (US4)

1. `GET /api/profile` autenticado.
   - Esperado: `200` com `name`, `email`, `profilePhotoUrl` (ou `null`) e `theme` atuais do usuario autenticado.
2. `PUT /api/profile` alterando `name`, `theme` e opcionalmente `profilePhotoUrl`.
   - Esperado: `200`; `GET /api/profile` subsequente reflete os novos valores.
3. `PUT /api/profile` tentando definir `email` ja usado por outra conta.
   - Esperado: `409` com mensagem generica (sem confirmar a existencia da outra conta).

## Criterios de sucesso a observar

- Tempos de resposta dentro de 2s para login, refresh, `GET`/`PUT /profile` sob carga esperada do MVP (SC-002).
- Nenhuma mensagem de erro do fluxo revela se um e-mail especifico esta cadastrado (SC-006, FR-010, FR-011).
- 100% das tentativas de reuso de refresh token/token de reset ja utilizados ou expirados sao negadas (SC-004).
- 100% das tentativas de acesso cruzado entre contas sao negadas em teste de isolamento (SC-003).

## Limpeza

- Remover usuarios, refresh tokens e tokens de reset de teste criados no Firestore (ou usar projeto/emulador dedicado a testes) para nao deixar dados residuais.
