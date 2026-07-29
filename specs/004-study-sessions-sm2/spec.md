# Feature Specification: Study Sessions SM-2

**Feature Branch**: `[004-study-sessions-sm2]`

**Created**: 2026-07-29

**Status**: Draft

**Input**: User description: "Sessoes de estudo com repeticao espacada (SM-2) do CogniLink - quarta feature, apos autenticacao, decks e flashcards."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Iniciar e Executar Sessao de Estudo (Priority: P1)

Como usuario autenticado, quero iniciar uma sessao para um deck proprio e responder os cards pendentes para avancar no meu plano de revisao.

**Why this priority**: Este e o fluxo central de aprendizagem do produto e entrega o valor principal da repeticao espacada.

**Independent Test**: Pode ser testada de forma independente iniciando uma sessao valida, consumindo cards pendentes na ordem definida e registrando respostas ate nao haver mais cards.

**Acceptance Scenarios**:

1. **Given** um usuario autenticado com deck proprio contendo cards pendentes, **When** ele inicia uma sessao, **Then** a sessao e criada e fica pronta para fornecer o proximo card.
2. **Given** uma sessao ativa com cards nunca revisados e cards vencidos, **When** o usuario solicita o proximo card, **Then** o sistema retorna primeiro os nunca revisados e, em seguida, os vencidos em ordem crescente de data de revisao.
3. **Given** um card retornado para estudo, **When** o sistema entrega esse card na sessao, **Then** ele inclui um identificador unico de tentativa para aquele card naquela sessao.
4. **Given** um deck inexistente ou de outro usuario, **When** o usuario tenta iniciar sessao, **Then** o sistema retorna nao encontrado.
5. **Given** um deck proprio sem flashcards, **When** o usuario tenta iniciar sessao, **Then** o sistema bloqueia a abertura da sessao com erro de negocio.

---

### User Story 2 - Registrar Resposta e Recalcular Agendamento (Priority: P1)

Como usuario autenticado, quero registrar minhas respostas por tentativa para que o sistema recalcule automaticamente meu proximo agendamento de revisao pelo SM-2.

**Why this priority**: Sem registro de resposta e recalculo, nao existe aprendizagem adaptativa nem progresso real no metodo de repeticao espacada.

**Independent Test**: Pode ser testada de forma independente ao registrar respostas corretas e incorretas para um mesmo card em tentativas distintas e validar atualizacao unica por tentativa.

**Acceptance Scenarios**:

1. **Given** uma tentativa valida retornada pela sessao, **When** o usuario envia se acertou, tempo de resposta e quantidade de dicas visualizadas, **Then** o sistema registra a resposta e recalcula os campos de agendamento do card.
2. **Given** uma resposta incorreta, **When** o sistema recalcula o SM-2, **Then** ele reinicia repeticoes, define intervalo em 1 dia e respeita o limite minimo de facilidade.
3. **Given** uma resposta correta com poucas dicas, **When** o sistema recalcula o SM-2, **Then** ele aumenta o fator de facilidade e amplia o intervalo de revisao conforme regra de dominio.
4. **Given** um attemptId ja processado, **When** a mesma resposta e enviada novamente, **Then** o sistema nao duplica registro nem recalcula novamente, retornando o resultado ja processado.

---

### User Story 3 - Concluir, Cancelar e Acompanhar Streak (Priority: P2)

Como usuario autenticado, quero concluir ou cancelar minha sessao e visualizar o resumo e minha sequencia de estudo para acompanhar consistencia ao longo dos dias.

**Why this priority**: O encerramento com resumo e streak sustenta motivacao e controle de progresso, mas depende do fluxo principal de execucao da sessao.

**Independent Test**: Pode ser testada de forma independente concluindo uma sessao com respostas registradas, cancelando outra em andamento e verificando calculo de streak e maior streak por dias distintos com sessao concluida.

**Acceptance Scenarios**:

1. **Given** uma sessao com respostas registradas, **When** o usuario conclui a sessao, **Then** o sistema retorna total de cards respondidos, taxa de acerto e duracao da sessao.
2. **Given** uma sessao em andamento, **When** o usuario cancela a sessao, **Then** o sistema encerra a sessao sem recalcular cards que nao tiveram resposta registrada.
3. **Given** historico de sessoes concluidas em dias distintos, **When** o usuario consulta seu progresso, **Then** o sistema retorna streak atual e maior streak.
4. **Given** cards pendentes distribuidos em varios decks do mesmo usuario, **When** o usuario consulta pendencias globais, **Then** o sistema retorna a lista agregada de cards pendentes de revisao.

### Edge Cases

- Tentativa de obter proximo card para sessao inexistente, ja concluida ou cancelada deve retornar erro apropriado.
- Submissao de resposta com attemptId desconhecido, de outra sessao ou de outro usuario deve ser rejeitada.
- Submissao de resposta com valores invalidos (tempo negativo, dicas negativas) deve ser rejeitada.
- Sessao concluida sem nenhum card respondido deve retornar resumo com total zero, taxa de acerto zero e duracao valida.
- Cancelamento repetido da mesma sessao deve ser idempotente e nao alterar resultados ja processados.
- Em caso de multiplos cards com mesma data de revisao, a ordenacao deve permanecer deterministica para evitar repeticao/omissao na sessao.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE permitir iniciar sessao de estudo apenas para deck existente e de propriedade do usuario autenticado.
- **FR-002**: O sistema DEVE retornar nao encontrado ao iniciar sessao para deck inexistente ou de outro usuario.
- **FR-003**: O sistema DEVE bloquear inicio de sessao para deck proprio sem flashcards com erro de negocio.
- **FR-004**: O sistema DEVE retornar o proximo card pendente da sessao priorizando cards nunca revisados e, depois, cards vencidos em ordem crescente de data de revisao.
- **FR-005**: O sistema DEVE associar a cada card retornado um attemptId unico para aquela combinacao de sessao e card.
- **FR-006**: O sistema DEVE registrar resposta do usuario contendo attemptId, indicador de acerto, tempo de resposta em segundos e quantidade de dicas visualizadas.
- **FR-007**: O sistema DEVE recalcular o progresso do card no momento do registro da resposta, atualizando easeFactor, intervalDays, nextReviewDate e repetitions conforme regra SM-2.
- **FR-008**: O sistema DEVE tratar resposta duplicada com o mesmo attemptId de forma idempotente, sem duplicar registro e sem recalculo adicional.
- **FR-009**: O sistema DEVE concluir sessao calculando total de cards respondidos, taxa de acerto e duracao.
- **FR-010**: O sistema DEVE permitir cancelar sessao em andamento sem recalcular cards que ainda nao receberam resposta.
- **FR-011**: O sistema DEVE calcular e disponibilizar streak atual e maior streak do usuario com base em dias distintos com ao menos uma sessao concluida.
- **FR-012**: O sistema DEVE disponibilizar listagem agregada de cards pendentes de revisao do usuario em todos os decks proprios.
- **FR-013**: O sistema DEVE aplicar a regra de dominio SM-2: resposta incorreta reinicia repetitions, define intervalo em 1 dia e nunca reduz easeFactor abaixo de 1.3.
- **FR-014**: O sistema DEVE aplicar a regra de dominio SM-2: resposta correta com poucas dicas aumenta easeFactor e amplia intervalDays para proximas revisoes.
- **FR-015**: O sistema NAO DEVE permitir que o cliente defina diretamente agendamento de revisao; o agendamento deve sempre resultar do calculo de dominio no servidor ao registrar resposta.
- **FR-016**: O sistema NAO DEVE incluir nesta feature validacao semantica por IA da resposta, recomendacoes por localizacao ou dashboard consolidado.

### Key Entities *(include if feature involves data)*

- **StudySession**: Sessao de estudo vinculada a um deck do usuario, com status, data/hora de inicio e data/hora de encerramento (conclusao ou cancelamento).
- **StudyAnswer**: Registro de uma tentativa respondida em sessao, contendo sessionId, flashcardId, attemptId, indicador de acerto, tempo de resposta e quantidade de dicas visualizadas.
- **UserFlashcardProgress**: Estado de aprendizagem do usuario por flashcard, contendo easeFactor, intervalDays, nextReviewDate, repetitions, lapses e data da ultima revisao.
- **StudyStreak**: Visao derivada do historico de sessoes concluidas em dias distintos para determinar streak atual e maior streak.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em testes de aceite, 100% dos fluxos validos de iniciar sessao, obter proximo card, registrar resposta e concluir sessao sao finalizados sem erro funcional.
- **SC-002**: Em testes de aceite, 100% das tentativas de acesso a deck inexistente ou nao pertencente ao usuario retornam bloqueio correto de acesso ao recurso.
- **SC-003**: Em homologacao, pelo menos 90% dos usuarios de teste concluem uma sessao de estudo com no minimo 10 cards em menos de 8 minutos.
- **SC-004**: Em cenarios de reenvio do mesmo attemptId, 100% dos casos mantem resultado idempotente sem alterar novamente o agendamento do card.
- **SC-005**: Em dados de homologacao com historico controlado, 100% dos calculos de streak atual e maior streak refletem corretamente os dias distintos com sessao concluida.

## Assumptions

- O usuario ja esta autenticado e identificado pelo fluxo de autenticacao existente.
- Decks e flashcards das features anteriores ja seguem isolamento por proprietario.
- O cliente informa corretamente o campo de acerto como booleano para os tipos FrenteVerso, Cloze e MultiplaEscolha nesta fase.
- O tipo DigiteResposta permanece fora da validacao semantica por IA nesta feature e segue para evolucao futura.
- A sessao trabalha apenas com cards pendentes no momento em que sao solicitados pela sessao ativa.
- Erros de negocio e de recurso inexistente seguem o padrao de resposta de erro ja adotado pelo produto.
