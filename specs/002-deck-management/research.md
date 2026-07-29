# Research: Gestão de Baralhos (Decks)

Nenhum `[NEEDS CLARIFICATION]` ficou pendente no spec — as decisões abaixo consolidam as escolhas técnicas para o planejamento, todas derivadas da infraestrutura já validada na feature 001 e das restrições explícitas do usuário.

## 1. Reaproveitamento da infraestrutura da feature 001

- **Decision**: Não recriar bootstrap, DI, JWT/`ICurrentUserService`, conexão Firestore, NLog, Swagger, Mapster, FluentValidation ou MediatR/`ValidationBehavior`. A feature 002 apenas registra `IDeckRepository -> FirestoreDeckRepository` em `Infrastructure/DependencyInjection.cs` e adiciona um novo `Controller`; nenhuma mudança em `Program.cs`.
- **Rationale**: `Program.cs`, `AddApplication()` e `AddInfrastructure()` já expõem MediatR com scan de assembly, `ValidationBehavior` pipeline, JWT bearer, CORS, Swagger e `FirestoreDb` singleton — tudo isso é agnóstico à entidade e funciona automaticamente para os novos commands/queries/validators de Decks assim que forem adicionados às assemblies existentes.
- **Alternatives considered**: Criar um pipeline de DI separado por feature — rejeitado por introduzir complexidade desnecessária e duplicar registro de serviços já compartilhados.

## 2. Isolamento por proprietário (owner) e erro 404 uniforme

- **Decision**: Toda query/command de Decks recebe/usa `ICurrentUserService.UserId` para resolver o dono autenticado e filtra por `OwnerId` **em Application**, antes de qualquer chamada a `IDeckRepository`. Quando o baralho não existe ou pertence a outro usuário, o handler lança a mesma exceção de "não encontrado" já usada na feature 001 (mapeada pelo `ExceptionHandlingMiddleware` existente para HTTP 404), nunca uma exceção de autorização/403.
- **Rationale**: Atende FR-006 e a regra de negócio da constituição de que isolamento por proprietário é regra de domínio/aplicação, não apenas política de autorização HTTP. Reaproveita o mesmo middleware de tratamento de exceções já testado na feature 001, sem necessidade de novo mapeamento de erros.
- **Alternatives considered**: Retornar 403 quando o recurso existir mas pertencer a outro usuário — rejeitado explicitamente pelo requisito RF-06 do spec (não revelar existência do recurso).

## 3. Busca por nome e paginação sobre Firestore

- **Decision**: `FirestoreDeckRepository` expõe um método que busca todos os documentos da coleção `decks` filtrados por `ownerId` (`WhereEqualTo("ownerId", userId)`), delegando a Application (`ListDecksQueryHandler`) a aplicação do filtro de busca por nome (`Contains`, case-insensitive via `ToLowerInvariant()`) e a paginação (`Skip`/`Take`) em memória sobre a lista já filtrada por dono.
- **Rationale**: O Firestore não oferece nativamente busca "contains" em string nem paginação por offset — apenas igualdade/prefixo e paginação por cursor (`StartAfter`). Como o volume esperado de baralhos por usuário no MVP é pequeno (dezenas a poucas centenas), buscar todos os documentos do dono e paginar/filtrar em memória é simples, correto e suficientemente performático (alinhado a SC-004), evitando introduzir Algolia/ElasticSearch ou índices compostos complexos fora do escopo do MVP.
- **Alternatives considered**:
  - Paginação por cursor nativa do Firestore (`StartAfter`) — rejeitada por não suportar diretamente números de página arbitrários (`page`/`pageSize`) exigidos pelo RF-02, e por adicionar complexidade de manter cursores entre requisições stateless.
  - Motor de busca externo (Algolia/Elasticsearch) — rejeitado por ser uma feature especulativa fora do escopo atual (constituição proíbe features especulativas fora do definido).

## 4. Modelagem da entidade `Deck` e categorias embutidas

- **Decision**: `Deck` é uma entidade pura de Domain com `Id`, `OwnerId`, `Name`, `Description` (nullable), `Difficulty` (enum `DeckDifficulty`: `Facil`, `Medio`, `Dificil`), `Categories` (`List<string>`, embutida, sem entidade própria), `CreatedAt`, `UpdatedAt`. Invariantes de tamanho (nome 3-100, descrição até 500, até 5 categorias de até 30 caracteres) são validadas em `Domain` (construção/edição da entidade) e reforçadas em `Application` via FluentValidation nos commands, seguindo o mesmo padrão duplo (Domain garante invariante; Application valida entrada antes de chegar ao Domain) já usado em `User`/`UserPreference` na feature 001.
- **Rationale**: Atende ao pedido explícito do usuário ("Categories: List<string>", "sem entidade própria de categoria") e às validações do spec (FR-007 a FR-010), mantendo Domain livre de dependências externas.
- **Alternatives considered**: Categoria como entidade própria com repositório — explicitamente rejeitada pelo usuário e pelo spec ("sem entidade própria de categoria").

## 5. Contagem de cards fixa em 0

- **Decision**: O DTO de detalhe/listagem de `Deck` inclui um campo `CardCount` calculado no momento da resposta (Application/Presentation), sempre retornando `0`, sem persistir esse valor no documento Firestore nem depender de uma coleção de flashcards inexistente.
- **Rationale**: Atende FR-011; evita acoplamento prematuro com uma feature futura de flashcards e evita persistir um dado que ficaria desatualizado assim que flashcards existirem.
- **Alternatives considered**: Persistir `cardCount` no documento do baralho desde já — rejeitado por ser especulativo e por exigir manutenção de consistência sem a feature de flashcards existir.
