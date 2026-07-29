# Feature Specification: Dashboard e Métricas

**Feature Branch**: `[005-dashboard-metrics]`

**Created**: 2026-07-29

**Status**: Draft

**Input**: User description: "Dashboard e métricas do CogniLink — quinta feature, consumindo dados já existentes de decks, flashcards e sessões de estudo (features 002-004)."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Visualizar Indicadores Globais de Progresso (Priority: P1)

Como usuário autenticado, quero ver um resumo consolidado do meu progresso geral ao abrir o dashboard, para entender rapidamente como estou evoluindo nos estudos sem precisar consultar cada deck individualmente.

**Why this priority**: É o valor central da feature — sem os indicadores globais não existe dashboard, e este resumo reaproveita cálculos já validados (streak e cards pendentes) das features anteriores.

**Independent Test**: Pode ser testada de forma independente consultando o resumo global de um usuário com histórico de estudo conhecido e validando cada indicador contra o histórico de `StudyAnswer` e `UserFlashcardProgress` desse usuário.

**Acceptance Scenarios**:

1. **Given** um usuário autenticado com histórico de respostas em múltiplos decks, **When** ele consulta seus indicadores globais, **Then** o sistema retorna domínio geral, tempo total de estudo, número de cards concluídos, taxa de retenção, streak atual, maior streak e quantidade de cards pendentes de revisão.
2. **Given** um usuário autenticado sem nenhum flashcard ou resposta registrada, **When** ele consulta seus indicadores globais, **Then** o sistema retorna domínio geral e taxa de retenção como zero, sem erro.
3. **Given** um usuário autenticado, **When** ele consulta seus indicadores globais, **Then** o sistema retorna apenas dados dos próprios decks, flashcards e sessões, nunca de outros usuários.

---

### User Story 2 - Visualizar Estatísticas de um Baralho Próprio (Priority: P2)

Como usuário autenticado, quero ver estatísticas detalhadas de um baralho específico que eu possuo, para identificar em quais subáreas, tipos de flashcard ou níveis de dificuldade preciso focar meus estudos.

**Why this priority**: Aprofunda o valor do dashboard ao nível de baralho, mas depende conceitualmente dos mesmos cálculos de domínio e pendências já usados no resumo global.

**Independent Test**: Pode ser testada de forma independente consultando as estatísticas de um baralho próprio com flashcards de diferentes dificuldades, subáreas e tipos, e validando os agrupamentos de desempenho retornados.

**Acceptance Scenarios**:

1. **Given** um baralho próprio com flashcards e histórico de respostas, **When** o usuário consulta as estatísticas desse baralho, **Then** o sistema retorna total de flashcards, cards pendentes de revisão, domínio estimado do baralho e desempenho agrupado por dificuldade, subárea e tipo de flashcard.
2. **Given** um baralho inexistente, **When** o usuário consulta suas estatísticas, **Then** o sistema retorna não encontrado.
3. **Given** um baralho pertencente a outro usuário, **When** o usuário autenticado consulta suas estatísticas, **Then** o sistema retorna não encontrado.
4. **Given** um baralho próprio sem nenhum flashcard, **When** o usuário consulta suas estatísticas, **Then** o sistema retorna domínio zero e listas de desempenho vazias, sem erro.

---

### User Story 3 - Consultar Histórico de Estudo por Período (Priority: P3)

Como usuário autenticado, quero consultar meu histórico de estudo dentro de um período informado, para alimentar gráficos de evolução no frontend futuramente.

**Why this priority**: Complementa o dashboard com uma visão temporal, mas é consumida principalmente por funcionalidades futuras de visualização, por isso tem prioridade menor que os resumos imediatos.

**Independent Test**: Pode ser testada de forma independente informando um período válido com atividade conhecida e validando que a série diária retornada bate com as respostas registradas dia a dia.

**Acceptance Scenarios**:

1. **Given** um usuário autenticado com respostas registradas em dias distintos dentro de um período informado, **When** ele consulta o histórico com datas de início e fim, **Then** o sistema retorna uma série diária ordenada cronologicamente com quantidade de respostas, acertos, erros e tempo total estudado em cada dia.
2. **Given** uma consulta de histórico sem data de início ou sem data de fim, **When** o usuário envia a requisição, **Then** o sistema rejeita a consulta com erro de validação.
3. **Given** uma consulta de histórico com período maior que 1 ano entre início e fim, **When** o usuário envia a requisição, **Then** o sistema rejeita a consulta com erro de validação.
4. **Given** um período válido sem nenhuma resposta registrada, **When** o usuário consulta o histórico, **Then** o sistema retorna a série do período sem erro, com os indicadores diários zerados.

### Edge Cases

- Usuário ou baralho sem nenhum flashcard elegível deve retornar domínio 0, não erro nem divisão por zero.
- Usuário sem nenhuma `StudyAnswer` deve retornar taxa de retenção 0 e tempo total de estudo 0, não erro.
- Data de início posterior à data de fim na consulta de histórico deve ser rejeitada com erro de validação.
- Baralho inexistente ou de outro usuário nas estatísticas por baralho deve retornar não encontrado (404), sem vazar informação sobre a existência do recurso de outro usuário.
- Flashcard sem subárea definida deve ser contabilizado no agrupamento de desempenho por dificuldade e tipo, e tratado como "sem subárea" no agrupamento por subárea.
- Cálculo de streak e de cards pendentes deve refletir exatamente a mesma lógica já usada na feature de sessões de estudo, sem divergência de resultado entre as duas features.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE disponibilizar, para o usuário autenticado, um resumo global de progresso contendo: domínio geral, tempo total de estudo, número de cards concluídos, taxa de retenção, streak atual, maior streak e quantidade de cards pendentes de revisão.
- **FR-002**: O sistema DEVE calcular o domínio (mastery) como a porcentagem de flashcards elegíveis do escopo (geral ou de um baralho) cujo `UserFlashcardProgress.Repetitions >= 2` E `IntervalDays >= 21`, arredondada a 1 casa decimal.
- **FR-003**: O sistema DEVE retornar domínio 0 quando o escopo (usuário ou baralho) não possuir nenhum flashcard elegível, sem gerar erro.
- **FR-004**: O sistema DEVE calcular o tempo total de estudo do usuário como a soma de `TimeToAnswerSeconds` de todas as `StudyAnswer` do usuário.
- **FR-005**: O sistema DEVE calcular o número de cards concluídos do usuário como a contagem de `flashcardId` distintos com ao menos uma `StudyAnswer` registrada.
- **FR-006**: O sistema DEVE calcular a taxa de retenção/acerto do usuário como a porcentagem de `StudyAnswer` com `IsCorrect = true` sobre o total de `StudyAnswer` do usuário, retornando 0 quando não houver nenhuma resposta registrada.
- **FR-007**: O sistema DEVE reutilizar o cálculo existente de streak atual e maior streak (mesma lógica de `GetUserStreak` da feature de sessões de estudo) para compor o resumo global.
- **FR-008**: O sistema DEVE reutilizar o cálculo existente de cards pendentes de revisão (mesma lógica de `GetDueFlashcards` da feature de sessões de estudo) para compor o resumo global.
- **FR-009**: O sistema DEVE disponibilizar, para um baralho de propriedade do usuário autenticado, estatísticas contendo: total de flashcards do baralho, cards pendentes de revisão daquele baralho, domínio estimado do baralho (mesma definição de domínio, restrita aos flashcards do baralho) e desempenho agrupado por dificuldade, subárea e tipo de flashcard, com taxa de acerto por grupo.
- **FR-010**: O sistema DEVE retornar não encontrado (404) ao consultar estatísticas de baralho inexistente ou de propriedade de outro usuário.
- **FR-011**: O sistema DEVE disponibilizar consulta de histórico de estudo por período, exigindo data de início e data de fim obrigatórias.
- **FR-012**: O sistema DEVE rejeitar consultas de histórico cujo intervalo entre data de início e data de fim seja maior que 1 ano, ou cuja data de início seja posterior à data de fim.
- **FR-013**: O sistema DEVE retornar, para o histórico de estudo, uma série diária ordenada cronologicamente contendo, para cada dia do período: quantidade de respostas, quantidade de acertos, quantidade de erros e tempo total estudado naquele dia.
- **FR-014**: O sistema DEVE restringir todos os indicadores, estatísticas e históricos desta feature aos dados de propriedade do próprio usuário autenticado.
- **FR-015**: O sistema NÃO DEVE introduzir nesta feature novas entidades de escrita; toda a funcionalidade consiste em consultas de agregação sobre entidades já existentes (Deck, Flashcard, StudySession, StudyAnswer, UserFlashcardProgress).
- **FR-016**: O sistema NÃO DEVE incluir nesta feature geração de gráficos/visualizações, exportação de relatórios ou recomendações personalizadas de estudo via IA.

### Key Entities *(include if feature involves data)*

- **DashboardSummary**: Visão agregada e somente leitura do progresso global do usuário, derivada de `StudyAnswer` e `UserFlashcardProgress` do próprio usuário; não é persistida como entidade de escrita.
- **DeckStatistics**: Visão agregada e somente leitura das estatísticas de um baralho específico do usuário, derivada de `Flashcard`, `StudyAnswer` e `UserFlashcardProgress` restritos àquele baralho.
- **StudyHistoryEntry**: Item de série diária derivado de `StudyAnswer` do usuário, agrupado por data, contendo quantidade de respostas, acertos, erros e tempo total estudado no dia.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em testes de aceite, 100% das consultas de resumo global, estatísticas de baralho próprio e histórico de estudo com dados válidos retornam os indicadores corretos sem erro funcional.
- **SC-002**: Em testes de aceite, 100% das tentativas de consultar estatísticas de baralho inexistente ou de outro usuário retornam bloqueio correto de acesso ao recurso (404).
- **SC-003**: Em testes de aceite, 100% dos cálculos de domínio e taxa de retenção para usuários ou baralhos sem flashcards ou respostas retornam 0, sem erro.
- **SC-004**: Em testes de aceite, 100% das consultas de histórico sem data obrigatória ou com período superior a 1 ano são corretamente rejeitadas com erro de validação.
- **SC-005**: Em homologação, usuários conseguem visualizar o resumo global do seu progresso ao abrir o dashboard em menos de 2 segundos.
- **SC-006**: Em dados de homologação com histórico controlado, 100% dos valores de streak e cards pendentes exibidos no dashboard coincidem com os valores já validados na feature de sessões de estudo.

## Assumptions

- O usuário já está autenticado e identificado pelo fluxo de autenticação existente.
- Os cálculos de streak atual, maior streak e cards pendentes de revisão reutilizam a lógica já implementada e validada na feature de sessões de estudo (SM-2), sem reimplementação divergente.
- Dias do período de histórico sem nenhuma resposta registrada são incluídos na série diária com todos os indicadores zerados, garantindo continuidade para consumo por gráficos futuros no frontend.
- Flashcards sem subárea definida são agrupados sob uma categoria "sem subárea" no desempenho por subárea, e normalmente incluídos nos agrupamentos por dificuldade e tipo.
- Geração de gráficos, exportação de relatórios e recomendações personalizadas de estudo via IA são funcionalidades futuras, fora do escopo desta feature.
- Não há novas entidades de escrita nesta feature; todas as funcionalidades são implementadas como consultas de agregação somente leitura sobre entidades já existentes.
