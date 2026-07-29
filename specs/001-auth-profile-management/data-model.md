# Data Model: Autenticacao e Gestao de Perfil

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Research**: [research.md](./research.md)

Entidades de Domain, puras e sem dependencias externas, persistidas via repositorios de Infrastructure sobre Firestore.

## User

Representa o titular de uma conta individual.

| Campo | Tipo | Regras |
|---|---|---|
| `Id` | string (identificador unico) | Gerado no cadastro; imutavel; usado como `userId` em claims e como chave de proprietario em todos os recursos de estudo. |
| `Name` | string | Obrigatorio, nao vazio. |
| `Email` | string | Obrigatorio; formato de e-mail valido; unico entre todos os usuarios (validado na Application antes de persistir). |
| `PasswordHash` | string | Nunca armazena senha em texto puro; somente o hash gerado por `IPasswordHasher`. Domain nao expoe metodo que aceite senha em claro alem do construtor/metodo de definicao que a converte imediatamente em hash via servico injetado na Application. |
| `CreatedAt` | datetime (UTC) | Definido na criacao; imutavel. |
| `Preference` | `UserPreference` | Associacao 1:1 (embutida ou referenciada); ver entidade abaixo. |
| `ProfilePhotoUrl` | string ou null | Opcional. |

**Regras de dominio**:
- E-mail unico e formato valido sao verificados antes da criacao/atualizacao (FR-001, FR-010).
- Alterar a senha exige sempre passar por hashing; nunca comparar/persistir texto puro (regra do Bloco 1, item 6).
- Atualizacao de perfil (nome, e-mail, foto, tema) so pode ser realizada pelo proprio `User` (FR-009), nunca por outro usuario.

## RefreshToken

Representa a credencial de continuidade de uma sessao.

| Campo | Tipo | Regras |
|---|---|---|
| `Id` / `Token` | string (valor opaco, unico) | Gerado no login/refresh; usado para localizar o registro no Firestore. |
| `UserId` | string | Referencia ao `User` proprietario da sessao. |
| `ExpiresAt` | datetime (UTC) | Define validade; apos esse instante o token e considerado expirado (FR-005). |
| `IsUsed` / `IsRevoked` | bool | Marcado ao ser trocado por um novo token (rotacao, FR-004) ou ao logout (FR-006). Uma vez marcado, nunca pode ser usado novamente (FR-005). |
| `CreatedAt` | datetime (UTC) | Auditoria. |

**Regras de dominio**:
- Uso unico: ao renovar a sessao, o token atual e marcado como usado/revogado e um novo e emitido (rotacao) (FR-004).
- Um token expirado, revogado ou ja usado nunca renova uma sessao; exige novo login (FR-005).
- Logout revoga explicitamente o refresh token ativo da sessao encerrada (FR-006).

## PasswordResetToken

Representa a autorizacao temporaria de redefinicao de senha (entidade de suporte a RF-05/FR-007).

| Campo | Tipo | Regras |
|---|---|---|
| `Id` / `Token` | string (valor opaco, unico) | Gerado ao solicitar recuperacao de senha. |
| `UserId` | string | Referencia ao `User` que solicitou a redefinicao. |
| `ExpiresAt` | datetime (UTC) | Prazo de validade limitado. |
| `IsUsed` | bool | Marcado como usado apos uma redefinicao bem-sucedida; impede reuso (FR-007). |
| `CreatedAt` | datetime (UTC) | Auditoria. |

**Regras de dominio**:
- Token de uso unico: apos uma redefinicao bem-sucedida, nao pode ser usado novamente.
- Token expirado ou ja utilizado nunca permite redefinir a senha (edge case do spec).

## UserPreference

Representa as preferencias pessoais de exibicao do usuario.

| Campo | Tipo | Regras |
|---|---|---|
| `Theme` | string (enum controlado pelo produto) | Valor dentro do conjunto de temas disponibilizados; obrigatorio ter um valor padrao na criacao da conta. |
| `UserId` | string | Referencia ao `User` proprietario; nunca alteravel por outro usuario. |

**Regras de dominio**:
- Somente o proprio usuario altera sua preferencia (FR-009).
- Tema deve pertencer ao conjunto de valores suportados pelo produto (Assumption do spec).

## Relacionamentos

```text
User (1) ──── (1) UserPreference
User (1) ──── (0..N) RefreshToken
User (1) ──── (0..N) PasswordResetToken
```

- Todos os recursos de estudo futuros (deck, flashcard, progresso, conversa) referenciam `User.Id` como proprietario; esta feature nao cria essas entidades, mas define o contrato de identidade (`userId`) que elas deverao usar (FR-002, FR-012, FR-013).

## Persistencia (Firestore, Infrastructure)

- Colecao `users`: documentos por `User.Id`, incluindo `preference` como campo embutido (mapa) e `profilePhotoUrl` opcional.
- Colecao `refreshTokens`: documentos por token, indexados por `userId`, com `expiresAt`, `isUsed`/`isRevoked`.
- Colecao `passwordResetTokens`: documentos por token, indexados por `userId`, com `expiresAt`, `isUsed`.
- Unicidade de e-mail verificada por consulta indexada no repositorio (`FirestoreUserRepository`) antes de criar/atualizar, dentro de Application (regra de negocio), nunca apenas por constraint de banco.
