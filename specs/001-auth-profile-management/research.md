# Phase 0 Research: Autenticacao e Gestao de Perfil

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

Todos os itens de "NEEDS CLARIFICATION" do Technical Context foram resolvidos a partir da constituicao do projeto e do detalhamento tecnico fornecido pelo usuario. Nenhum item permanece em aberto.

## 1. Framework e arquitetura

- **Decision**: .NET 10 / ASP.NET Core com Clean Architecture (Api, Application, Domain, Infrastructure) e CQRS via MediatR.
- **Rationale**: Definido na constituicao do projeto como padrao obrigatorio para todo o backend; garante Domain puro sem dependencias externas e separa regras de negocio de detalhes de infraestrutura (Firestore, JWT).
- **Alternatives considered**: Arquitetura em camadas tradicional (N-Tier) sem CQRS — rejeitada por nao estar alinhada a constituicao e por acoplar leitura/escrita nos mesmos servicos, dificultando testes independentes por historia de usuario.

## 2. Persistencia

- **Decision**: Firebase Firestore via `Google.Cloud.Firestore` (Firebase Admin SDK), sem EF Core e sem migrations relacionais. Repositorios customizados em Infrastructure implementam interfaces definidas em Application.
- **Rationale**: Constituicao exige banco inteiramente nao relacional (Firestore) e proibe EF Core/migrations. Documentos de usuario, refresh token e preferencias mapeiam bem para colecoes Firestore (`users`, `refreshTokens`).
- **Alternatives considered**: PostgreSQL/EF Core — rejeitado por violar a constituicao. Realtime Database — rejeitado por Firestore ja ser o padrao definido para o projeto.

## 3. Autenticacao e sessao

- **Decision**: JWT de curta duracao (access token) assinado com chave simetrica/assimetrica configurada via secrets, mais refresh token de longa duracao armazenado no Firestore com rotacao (uso unico) a cada renovacao. Logout revoga o refresh token ativo (flag `revoked`/exclusao logica).
- **Rationale**: Atende RF-02/RF-03/RF-04 do spec original e FR-003 a FR-006 da especificacao; rotacao de refresh token e uso unico mitigam reuso de token roubado (edge case do spec).
- **Alternatives considered**: Sessao baseada em cookie/servidor com estado — rejeitada por nao se alinhar ao padrao stateless de API JWT definido na constituicao (Presentation "sempre com JWT + refresh token").

## 4. Hashing de senha

- **Decision**: Hash de senha via algoritmo adaptativo (ex.: BCrypt/Argon2 atraves de biblioteca .NET consolidada) encapsulado em `IPasswordHasher`/`PasswordHasher` em Infrastructure; Domain nunca manipula senha em texto puro, apenas o hash.
- **Rationale**: Constituicao e o detalhamento tecnico exigem que a senha nunca fique em texto puro no Domain (regra da entidade `User`). Um adaptive hash resiste a ataques de forca bruta melhor que hash simples (SHA-256).
- **Alternatives considered**: Hash simples (SHA-256) — rejeitado por vulnerabilidade a ataques de forca bruta/rainbow table; nao atende a politica de senha forte implicita nos requisitos de seguranca.

## 5. Recuperacao de senha

- **Decision**: Token de reset de uso unico com expiracao, gerado e persistido (ex.: colecao `passwordResetTokens` ou subcampo do usuario) e invalidado apos uso. Envio por `IEmailSender`, com implementacao stub/log no MVP (pendencia documentada), conforme Bloco 3, item 22.
- **Rationale**: Atende RF-05/FR-007 e edge cases do spec (token expirado/ja usado deve falhar). Stub de e-mail permite entregar o fluxo completo sem bloquear no provedor de e-mail transacional, que fica como trabalho futuro.
- **Alternatives considered**: Envio de e-mail real via provedor externo (SendGrid, etc.) nesta feature — rejeitado por estar fora do escopo tecnico fornecido pelo usuario para este slice (Bloco 3 item 22 explicitamente marca `EmailSender` como stub/log).

## 6. Isolamento por proprietario

- **Decision**: `ICurrentUserService` extrai `userId` das claims do `ClaimsPrincipal` autenticado; todo handler de Application que acessa recurso pertencente a um usuario recebe/valida esse `userId` antes de delegar a Infrastructure, negando acesso quando o recurso pertence a outro usuario.
- **Rationale**: Constituicao trata isolamento por proprietario como regra de dominio, nao apenas politica de autorizacao; FR-012/FR-013 do spec exigem isso explicitamente inclusive quando IDs de terceiros sao fornecidos diretamente.
- **Alternatives considered**: Filtragem apenas no nivel de Infrastructure/repositorio — rejeitada porque nao garante que Application rejeite a operacao de forma explicita e testavel, dificultando cobertura dos casos de borda do spec.

## 7. Validacao e mapeamento

- **Decision**: FluentValidation para validators de cada Command (RegisterUser, Login implicito via validacao de request, ResetPassword, ChangePassword, UpdateProfile); Mapster para mapear entidades de Domain para DTOs de resposta e vice-versa.
- **Rationale**: Padrao obrigatorio definido na constituicao para validacoes e mapeamento; evita codigo de mapeamento manual repetitivo e centraliza regras de formato (e-mail valido, senha forte) fora do Domain.
- **Alternatives considered**: DataAnnotations — rejeitado por ser menos expressivo para regras compostas (ex.: politica de senha forte) e por a constituicao exigir FluentValidation.

## 8. Logging e observabilidade

- **Decision**: NLog configurado no bootstrap (`Program.cs`) para registrar eventos de autenticacao relevantes (login, falha de login, logout, reset de senha) sem registrar segredos (senha, tokens completos).
- **Rationale**: Constituicao exige NLog; auditabilidade de eventos de seguranca e uma boa pratica para deteccao de abuso, sem violar OWASP (nao logar credenciais).
- **Alternatives considered**: `ILogger` builtin sem NLog — rejeitado por a constituicao exigir NLog explicitamente.

## 9. Documentacao de API

- **Decision**: Swashbuckle/Swagger configurado no bootstrap, com anotacoes nos controllers de Auth e Profile.
- **Rationale**: Constituicao exige Swagger para toda documentacao de API.
- **Alternatives considered**: Nenhuma — requisito direto da constituicao.
