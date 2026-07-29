# Research: Gestao de Flashcards

Nao ha marcadores `[NEEDS CLARIFICATION]` no spec desta feature. As decisoes abaixo consolidam as escolhas de design para implementacao mantendo o mesmo padrao das features 001 e 002.

## 1. Reutilizacao integral da infraestrutura existente

- Decision: Reutilizar integralmente bootstrap, DI, JWT/ICurrentUserService, Firestore, NLog, Swagger, Mapster, FluentValidation e MediatR ja existentes, sem redescrever ou duplicar configuracoes.
- Rationale: O objetivo desta feature e adicionar comportamento de flashcards dentro da arquitetura existente, sem mudancas estruturais em `Program.cs` ou no pipeline global.
- Alternatives considered: Criar novo bootstrap por feature ou novas configuracoes paralelas de DI; rejeitado por redundancia e risco de divergencia.

## 2. Ownership via Deck antes de operar Flashcard

- Decision: Toda operacao de flashcard resolve primeiro o Deck via `IDeckRepository` com `deckId` e `userId` autenticado; deck inexistente ou de outro usuario retorna NotFound (404) antes de tocar `IFlashcardRepository`.
- Rationale: Garante isolamento por proprietario como regra de dominio/aplicacao e atende os RF-01 a RF-05 de forma uniforme.
- Alternatives considered: Validar ownership apenas no repositorio de flashcard; rejeitado porque enfraquece a regra de ownership pelo agregado pai (Deck).

## 3. Modelagem unificada de Flashcard no Firestore

- Decision: Usar uma unica entidade `Flashcard` e um unico documento Firestore por flashcard, com campos especificos por tipo opcionais/nulos conforme `Type`.
- Rationale: Alinhado ao pedido explicito de evitar heranca complexa e ao modelo NoSQL do Firestore, simplificando persistencia e evolucao.
- Alternatives considered: Entidades derivadas por tipo ou colecoes separadas por tipo; rejeitado por complexidade desnecessaria para o MVP.

## 4. Validacao por tipo em duas camadas

- Decision: Regras bloqueantes por tipo ficam no Domain (metodo de validacao do agregado) e sao reforcadas em FluentValidation no Application com um unico validator por comando, usando regras condicionais por `Type`.
- Rationale: Domain protege invariantes de negocio; Application oferece feedback consistente de entrada e integra com pipeline de validacao existente.
- Alternatives considered: Um validator por tipo; rejeitado por aumentar fragmentacao e contrariar requisito explicito de um validator unico com branches.

## 5. Estrategia de consulta no Firestore

- Decision: `FirestoreFlashcardRepository` usa colecao `flashcards` e consulta de listagem por `deckId` com filtro indexado (`WhereEqualTo("deckId", deckId)`).
- Rationale: A listagem funcional requerida e por deck, e `deckId` e a chave de consulta natural para escala e simplicidade.
- Alternatives considered: Indexar por ownerId e consultar sem deck; rejeitado porque o caso de uso principal e deck-centrico.

## 6. Escopo explicitamente excluido

- Decision: Nao incluir SM-2 (`nextReview`, `intervalDays`, `easeFactor`), historico de tentativas, validacao semantica via IA e versionamento de flashcard.
- Rationale: Fora de escopo formal desta feature e reservado para Sessoes de Estudo/SM-2.
- Alternatives considered: Antecipar campos de revisao no contrato; rejeitado por gerar feature especulativa.
