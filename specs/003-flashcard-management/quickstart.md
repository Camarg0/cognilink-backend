# Quickstart: Gestao de Flashcards

Feature: [spec.md](./spec.md) | Plan: [plan.md](./plan.md) | Contrato: [contracts/flashcards.openapi.yaml](./contracts/flashcards.openapi.yaml)

Guia de validacao end-to-end da feature 003 sem testes automatizados.

## Pre-requisitos

- .NET 10 SDK instalado.
- Features 001 (Auth/Profile) e 002 (Decks) ja operacionais no mesmo ambiente.
- API rodando com JWT, Firestore, Swagger e NLog conforme padrao existente.
- Usuario autenticado com access token JWT valido.
- Pelo menos 1 deck proprio existente.
- Pelo menos 1 deck de outro usuario para validar isolamento.

## Setup

```powershell
dotnet restore
dotnet build
dotnet run --project CogniLink.Api
```

Abrir Swagger em `https://localhost:<porta>/swagger`.

## Endpoints cobertos

- `POST /api/decks/{deckId}/flashcards`
- `GET /api/decks/{deckId}/flashcards`
- `GET /api/flashcards/{flashcardId}`
- `PUT /api/flashcards/{flashcardId}`
- `DELETE /api/flashcards/{flashcardId}`

## Cenarios de validacao

### 1. Criacao por tipo (RF-01)

1. Criar `FrenteVerso` com `question` e `answer` validos.
- Esperado: `201`.
2. Criar `Cloze` com `clozeText` contendo `{{c1::termo}}`.
- Esperado: `201`.
3. Criar `DigiteResposta` com `validAnswers` contendo 1+ itens nao vazios.
- Esperado: `201`.
4. Criar `MultiplaEscolha` com 2 a 5 alternativas e exatamente 1 correta.
- Esperado: `201`.

### 2. Listagem por deck proprio (RF-02)

1. Chamar `GET /api/decks/{deckId}/flashcards` para deck proprio com cards.
- Esperado: `200` com lista dos cards do deck.
2. Chamar listagem para deck proprio sem cards.
- Esperado: `200` com lista vazia.

### 3. Obter flashcard por id (RF-03)

1. Chamar `GET /api/flashcards/{flashcardId}` de card em deck proprio.
- Esperado: `200` com payload completo.
2. Chamar para `flashcardId` inexistente.
- Esperado: `404`.

### 4. Edicao completa (RF-04)

1. Chamar `PUT /api/flashcards/{flashcardId}` alterando todos os campos permitidos.
- Esperado: `200`.
2. Repetir com payload invalido para o tipo.
- Esperado: `400`.

### 5. Exclusao (RF-05)

1. Chamar `DELETE /api/flashcards/{flashcardId}` de card proprio.
- Esperado: `204`.
2. Confirmar `GET /api/flashcards/{flashcardId}` apos exclusao.
- Esperado: `404`.

### 6. Isolamento por proprietario (regras de seguranca)

1. Tentar criar/listar flashcards em `deckId` de outro usuario.
- Esperado: `404`.
2. Tentar obter/editar/excluir `flashcardId` pertencente a deck de outro usuario.
- Esperado: `404`.

## Criterios de aceite operacionais

- Validacoes bloqueantes por tipo sao aplicadas em criacao e edicao.
- Campos fora de escopo (SM-2, historico, IA semantica, versionamento) nao aparecem no contrato.
- Nao ha respostas 403 para recursos alheios; ownership quebrado resulta em 404.
