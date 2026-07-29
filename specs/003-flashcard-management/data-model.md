# Data Model: Gestao de Flashcards

## Flashcard

Entidade de estudo associada a um unico Deck e a um unico proprietario de forma indireta (via ownership do Deck). Persistida como um unico documento na colecao `flashcards`.

| Campo | Tipo | Obrigatorio | Regras |
|---|---|---|---|
| `Id` | `string` | Sim | Identificador unico do documento Firestore. |
| `DeckId` | `string` | Sim | Referencia ao deck pai; deve existir e pertencer ao usuario autenticado para qualquer operacao. |
| `Type` | `FlashcardType` | Sim | `FrenteVerso`, `Cloze`, `DigiteResposta`, `MultiplaEscolha`. |
| `Difficulty` | `FlashcardDifficulty` | Sim | `Facil`, `Medio`, `Dificil`. |
| `Subarea` | `string?` | Nao | Opcional; maximo 50 caracteres. |
| `Hints` | `List<string>?` | Nao | Opcional; maximo 3 itens. |
| `Question` | `string?` | Condicional | Obrigatorio e nao vazio quando `Type = FrenteVerso`. |
| `Answer` | `string?` | Condicional | Obrigatorio e nao vazio quando `Type = FrenteVerso`. |
| `ClozeText` | `string?` | Condicional | Obrigatorio quando `Type = Cloze`; deve conter lacunas `{{cN::termo}}` sequenciais de `c1` ate `cN`. |
| `ValidAnswers` | `List<string>?` | Condicional | Obrigatorio quando `Type = DigiteResposta`; minimo 1 item; nenhum vazio. |
| `Alternatives` | `List<Alternative>?` | Condicional | Obrigatorio quando `Type = MultiplaEscolha`; minimo 2 e maximo 5; exatamente 1 correta. |
| `CreatedAt` | `DateTime` UTC | Sim | Definido na criacao. |
| `UpdatedAt` | `DateTime` UTC | Sim | Atualizado em edicao. |

## Alternative (Value Object)

| Campo | Tipo | Obrigatorio | Regras |
|---|---|---|---|
| `Text` | `string` | Sim | Nao vazio. |
| `IsCorrect` | `bool` | Sim | Exatamente um item verdadeiro em `Alternatives`. |

## Enums

```csharp
public enum FlashcardType
{
    FrenteVerso,
    Cloze,
    DigiteResposta,
    MultiplaEscolha
}

public enum FlashcardDifficulty
{
    Facil,
    Medio,
    Dificil
}
```

## Invariantes de Dominio

- Campos comuns: `Difficulty` valido; `Subarea` <= 50; `Hints` <= 3.
- `FrenteVerso`: `Question` e `Answer` obrigatorios, nao vazios.
- `Cloze`: `ClozeText` obrigatorio, contem ao menos `{{c1::...}}`; indices sem saltos (c1, c2, c3...).
- `DigiteResposta`: `ValidAnswers` com pelo menos 1 item e sem string vazia.
- `MultiplaEscolha`: `Alternatives` entre 2 e 5, textos nao vazios, exatamente 1 com `IsCorrect = true`.

## Estados e Transicoes

- Criado: por `CreateFlashcard` com validacao de dominio aprovada.
- Atualizado: por `UpdateFlashcard` (todos os campos editaveis), reaplicando invariantes por tipo.
- Excluido: por `DeleteFlashcard` (hard delete no Firestore).

Nao ha versionamento, historico de tentativas ou campos SM-2 nesta feature.

## Representacao Firestore (colecao flashcards)

```json
{
  "deckId": "string",
  "type": "FrenteVerso|Cloze|DigiteResposta|MultiplaEscolha",
  "difficulty": "Facil|Medio|Dificil",
  "subarea": "string|null",
  "hints": ["string"],
  "question": "string|null",
  "answer": "string|null",
  "clozeText": "string|null",
  "validAnswers": ["string"],
  "alternatives": [
    { "text": "string", "isCorrect": true }
  ],
  "createdAt": "Timestamp",
  "updatedAt": "Timestamp"
}
```
