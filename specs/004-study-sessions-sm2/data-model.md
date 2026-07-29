# Data Model: Study Sessions SM-2

## StudySession

Sessao de estudo vinculada a um deck proprio, controlando ciclo de vida da execucao (inicio, conclusao, cancelamento).

| Campo | Tipo | Obrigatorio | Regras |
|---|---|---|---|
| `Id` | `string` | Sim | Identificador unico da sessao. |
| `DeckId` | `string` | Sim | Deck alvo da sessao; deve existir e pertencer ao usuario autenticado. |
| `OwnerId` | `string` | Sim | Usuario dono da sessao. |
| `Status` | `StudySessionStatus` | Sim | `InProgress`, `Completed`, `Cancelled`. |
| `StartedAt` | `DateTime` UTC | Sim | Definido na abertura da sessao. |
| `CompletedAt` | `DateTime?` UTC | Nao | Preenchido ao concluir ou cancelar. |

### Regras e transicoes

- `Start`: cria sessao com `Status = InProgress`.
- `Complete`: muda para `Completed`, define `CompletedAt`, calcula resumo.
- `Cancel`: muda para `Cancelled`, define `CompletedAt`, sem recalculo para cards sem resposta.
- Sessao encerrada (`Completed`/`Cancelled`) nao aceita novas respostas.

## StudyAnswer

Registro de resposta individual por tentativa dentro de uma sessao.

| Campo | Tipo | Obrigatorio | Regras |
|---|---|---|---|
| `Id` | `string` | Sim | Identificador unico do registro. |
| `SessionId` | `string` | Sim | Referencia da sessao ativa no momento da resposta. |
| `FlashcardId` | `string` | Sim | Card respondido. |
| `AttemptId` | `Guid` | Sim | Chave de idempotencia da tentativa. |
| `IsCorrect` | `bool` | Sim | Indicador de acerto informado pelo cliente. |
| `TimeToAnswerSeconds` | `int` | Sim | Deve ser >= 0. |
| `HintsViewed` | `int` | Sim | Deve ser >= 0. |
| `AnsweredAt` | `DateTime` UTC | Sim | Instante do processamento da resposta. |

### Regras de idempotencia

- `AttemptId` deve ser unico no contexto funcional da tentativa.
- Reenvio com mesmo `AttemptId` retorna resultado ja processado.
- Reenvio nao cria novo `StudyAnswer` e nao reaplica `ApplyReview`.

## UserFlashcardProgress

Estado de aprendizado por usuario e por card, controlado por SM-2.

| Campo | Tipo | Obrigatorio | Regras |
|---|---|---|---|
| `Id` | `string` | Sim | Identificador unico do progresso. |
| `FlashcardId` | `string` | Sim | Card associado ao progresso. |
| `OwnerId` | `string` | Sim | Dono do progresso. |
| `EaseFactor` | `double` | Sim | Default 2.5; limite minimo 1.3. |
| `IntervalDays` | `int` | Sim | Default 0; em erro reinicia para 1. |
| `NextReviewDate` | `DateTime?` UTC | Nao | Nulo para nunca revisado; preenchido apos primeira resposta. |
| `Repetitions` | `int` | Sim | Sequencia de acertos consecutivos no SM-2. |
| `Lapses` | `int` | Sim | Contador de falhas (respostas incorretas). |
| `LastReviewedAt` | `DateTime?` UTC | Nao | Ultimo instante em que `ApplyReview` foi aplicado. |

### Metodo de dominio

- Metodo: `ApplyReview(isCorrect)`.
- Regras SM-2 desta feature:
  - Resposta incorreta: incrementa `Lapses`, zera `Repetitions`, define `IntervalDays = 1`, atualiza `NextReviewDate` para `hoje + 1 dia`, reduz `EaseFactor` respeitando piso 1.3.
  - Resposta correta: incrementa `Repetitions`, recalcula `EaseFactor` para cima conforme regra SM-2 classica, recalcula `IntervalDays` (1, 6, depois formula progressiva), atualiza `NextReviewDate`.
- O calculo e sempre servidor-side e nunca aceito do cliente.

## StudyStreak (visao derivada)

Visao calculada a partir de dias distintos com ao menos uma sessao `Completed`.

| Campo | Tipo | Origem |
|---|---|---|
| `CurrentStreak` | `int` | Dias consecutivos ate a data atual (ou ultimo dia concluido). |
| `LongestStreak` | `int` | Maior sequencia historica de dias consecutivos. |
| `LastStudyDate` | `DateOnly?` | Data da ultima sessao concluida. |

## Relacionamentos

- `StudySession (1) -> (N) StudyAnswer`
- `Flashcard (1) -> (N) StudyAnswer`
- `User + Flashcard (1) -> (1) UserFlashcardProgress`
- `StudySession` referencia `Deck`; ownership deriva de `Deck.OwnerId`.

## Consultas-chave

- Proximo card da sessao: pendentes da sessao com prioridade `NextReviewDate = null` e depois `NextReviewDate <= now`, ordenando por `nextReviewDate` asc e desempate deterministico.
- Pendencias agregadas: por `OwnerId`, filtrando progresso vencido e nunca revisado.
- Streak: por `OwnerId`, considerando somente sessoes `Completed` e dias distintos.
