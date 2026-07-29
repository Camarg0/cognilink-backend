# Data Model: Gestão de Baralhos (Decks)

## Deck

Representa uma coleção nomeada de estudo pertencente a um único usuário (owner). Entidade pura de `CogniLink.Domain`, sem dependências externas.

| Campo | Tipo | Obrigatório | Regras / Invariantes |
|---|---|---|---|
| `Id` | `string` | Sim | Identificador único (id do documento Firestore); gerado na criação. |
| `OwnerId` | `string` | Sim | `userId` do usuário autenticado dono do baralho (resolvido via `ICurrentUserService`); imutável após criação. |
| `Name` | `string` | Sim | 3 a 100 caracteres (FR-007). Trim aplicado antes de validar/persistir. |
| `Description` | `string?` | Não | Até 500 caracteres quando informada (FR-008); `null`/vazio permitido. |
| `Difficulty` | `DeckDifficulty` (enum) | Sim | Um de `Facil`, `Medio`, `Dificil` (FR-010). |
| `Categories` | `List<string>` | Sim (pode ser vazia) | No máximo 5 itens; cada item até 30 caracteres (FR-009). Duplicatas permitidas (ver Assumptions do spec). Lista embutida no documento — sem entidade/coleção própria. |
| `CreatedAt` | `DateTime` (UTC) | Sim | Definido na criação; imutável. |
| `UpdatedAt` | `DateTime` (UTC) | Sim | Atualizado em toda edição (`UpdateDeck`); igual a `CreatedAt` na criação. |

### Invariantes de Domain

- `Deck.Create(...)` valida: `OwnerId` e `Name` não vazios, `Name` entre 3-100 chars, `Description` (se houver) até 500 chars, `Categories` com no máximo 5 itens de até 30 chars cada. Lança `ArgumentException` em violação (mesmo padrão de `User.Create`).
- `Deck.UpdateDetails(name, description, difficulty, categories)` reaplica as mesmas invariantes e atualiza `UpdatedAt = DateTime.UtcNow`.
- `Deck.Reconstitute(...)` — factory usada pelo repositório para reidratar a entidade a partir do Firestore, sem revalidar (dados já confiáveis, persistidos por este mesmo sistema).

### Campo derivado — não persistido

- `CardCount` (`int`, sempre `0`): calculado apenas no DTO de resposta (Application/Presentation), não é um campo do documento Firestore nem do agregado `Deck` em Domain (FR-011). Fora de escopo até a feature de flashcards existir.

## DeckDifficulty (enum)

```csharp
public enum DeckDifficulty
{
    Facil,
    Medio,
    Dificil
}
```

Serializado como string no Firestore e nos DTOs (ex.: `"Facil"`, `"Medio"`, `"Dificil"`), para legibilidade e estabilidade caso a ordem dos valores mude no futuro.

## Representação no Firestore (coleção `decks`)

```json
{
  "ownerId": "string",
  "name": "string",
  "description": "string|null",
  "difficulty": "Facil|Medio|Dificil",
  "categories": ["string", "..."],
  "createdAt": "Timestamp",
  "updatedAt": "Timestamp"
}
```

O `Id` do documento corresponde ao `Deck.Id` (mesmo padrão de `FirestoreUserRepository`, que usa `Document(entity.Id)`).

## Relacionamentos

- **Deck → User (Owner)**: relação implícita via `OwnerId` (não é uma referência de objeto em Domain, apenas o identificador). Um usuário pode ter múltiplos baralhos; um baralho pertence a exatamente um usuário.
- **Deck → Flashcard**: fora de escopo desta feature. Nenhuma referência, chave estrangeira ou coleção relacionada é criada agora; `CardCount` é sempre 0 até a feature futura de flashcards.

## DTOs (Application/Presentation)

| DTO | Uso | Campos |
|---|---|---|
| `DeckDto` | Resposta de criação, detalhe, item de listagem, resposta de edição | `Id`, `Name`, `Description`, `Difficulty` (string), `Categories`, `CardCount` (fixo 0), `CreatedAt`, `UpdatedAt` |
| `PagedResult<DeckDto>` | Resposta de listagem | `Items` (`List<DeckDto>`), `Page`, `PageSize`, `TotalCount` |

`OwnerId` **não** é exposto nos DTOs de resposta — o dono já é implícito pelo usuário autenticado da requisição.

## Estados e transições

`Deck` não possui máquina de estados explícita nesta feature (sem soft delete, sem arquivamento). Ciclo de vida:

1. **Criado** (`CreateDeck`) → existe até ser excluído.
2. **Editado** (`UpdateDeck`) → atualiza `Name`/`Description`/`Difficulty`/`Categories` e `UpdatedAt`; não muda `OwnerId` nem `Id`.
3. **Excluído** (`DeleteDeck`) → remoção definitiva (hard delete) do documento Firestore; não há exclusão em cascata de outras entidades (nenhuma existe ainda).
