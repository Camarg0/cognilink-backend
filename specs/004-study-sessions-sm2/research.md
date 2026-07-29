# Research: Study Sessions SM-2

Nao ha marcadores `[NEEDS CLARIFICATION]` no spec desta feature. As decisoes abaixo consolidam os pontos de dominio e integracao para implementacao da feature 004.

## 1. Formula de agendamento SM-2 em dominio puro

- Decision: Implementar `UserFlashcardProgress.ApplyReview(isCorrect)` no Domain com SM-2 classico de Wozniak, mantendo `easeFactor` minimo de 1.3, `intervalDays` e `repetitions` recalculados no servidor.
- Rationale: O nucleo academico da plataforma e o agendamento; manter em dominio puro garante isolamento de regra de negocio, auditabilidade e testabilidade independente da persistencia.
- Alternatives considered: Calcular SM-2 no frontend ou na Infrastructure; rejeitado por violar regra central de dominio e aumentar risco de inconsistencias.

## 2. Ordenacao de proximo card na sessao

- Decision: `GetNextCard` prioriza cards nunca revisados (`nextReviewDate` nulo) antes dos vencidos (`nextReviewDate <= agora`), com ordenacao ascendente por `nextReviewDate` e desempate deterministico por `flashcardId`.
- Rationale: Atende RF-02 e evita omissao/repeticao de cards quando varios itens possuem a mesma data de revisao.
- Alternatives considered: Misturar nunca revisados com vencidos por data; rejeitado porque contraria a regra funcional prioritaria.

## 3. Idempotencia por attemptId

- Decision: `SubmitAnswer` valida `attemptId` como chave idempotente unica por tentativa; se ja existir resposta processada, retorna o resultado existente sem reexecutar `ApplyReview`.
- Rationale: Evita dupla aplicacao de SM-2 por retry de rede, garantindo consistencia do progresso por card.
- Alternatives considered: Idempotencia por hash de payload ou por janela temporal; rejeitado por menor previsibilidade e maior ambiguidade de colisao semantica.

## 4. Ownership antes de tocar sessao/progresso

- Decision: Toda operacao valida ownership com `IDeckRepository` e/ou `IFlashcardRepository` ja existentes antes de manipular `StudySession`, `StudyAnswer` ou `UserFlashcardProgress`.
- Rationale: Mantem o mesmo padrao de seguranca das features 001-003, com isolamento por proprietario como regra de dominio.
- Alternatives considered: Delegar ownership apenas a filtros de consulta nos novos repositorios; rejeitado por enfraquecer a regra de acesso consistente no use case.

## 5. Estrategia Firestore para pendencias e tentativas

- Decision: Criar repositorios Firestore dedicados: `FirestoreStudySessionRepository`, `FirestoreStudyAnswerRepository` (indice logico unico por `attemptId`) e `FirestoreUserFlashcardProgressRepository` (consulta por `ownerId + nextReviewDate`).
- Rationale: Segue o padrao do projeto e suporta os dois casos criticos: idempotencia por tentativa e listagem agregada de cards pendentes.
- Alternatives considered: Armazenar respostas embutidas no documento de sessao; rejeitado por dificultar consulta por `attemptId` e concorrencia de atualizacao.

## 6. Sem testes automatizados nesta etapa

- Decision: Nao incluir tarefas de testes automatizados no plano desta feature; incluir apenas roteiro de validacao manual e revisao manual focada em `ApplyReview`.
- Rationale: Restricao explicita de escopo do solicitante para esta etapa.
- Alternatives considered: Introduzir suite unit/integration completa agora; rejeitado por contrariar o pedido e expandir o escopo de entrega.
