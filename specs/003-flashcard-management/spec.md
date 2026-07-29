# Feature Specification: Flashcard Management

**Feature Branch**: `[003-flashcard-management]`

**Created**: 2026-07-29

**Status**: Draft

**Input**: User description: "Gestao de flashcards do CogniLink - terceira feature, apos autenticacao e decks."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Criar Flashcard Tipado em Deck Proprio (Priority: P1)

Como usuario autenticado, quero criar um flashcard dentro de um baralho meu escolhendo um tipo valido para registrar conteudo de estudo estruturado.

**Why this priority**: Sem criacao de flashcard, o usuario nao consegue iniciar o fluxo principal de aprendizagem da plataforma.

**Independent Test**: Pode ser testada de forma independente ao criar flashcards de cada tipo em um deck proprio e validar retorno de sucesso ou bloqueio por regra de negocio.

**Acceptance Scenarios**:

1. **Given** um usuario autenticado e um deck existente de sua propriedade, **When** ele cria um flashcard do tipo FrenteVerso com pergunta e resposta preenchidas, **Then** o flashcard e criado e associado ao deck informado.
2. **Given** um usuario autenticado e um deck proprio, **When** ele cria um flashcard do tipo Cloze com ao menos uma lacuna no formato `{{c1::termo}}` e numeracao sequencial, **Then** o flashcard e criado com sucesso.
3. **Given** um usuario autenticado e um deck proprio, **When** ele envia dados invalidos para o tipo selecionado, **Then** a criacao e bloqueada com erro de validacao.
4. **Given** um usuario autenticado, **When** ele tenta criar flashcard em deck inexistente ou de outro usuario, **Then** o sistema retorna 404.

---

### User Story 2 - Consultar Flashcards do Deck Proprio (Priority: P2)

Como usuario autenticado, quero listar e visualizar meus flashcards para revisar o conteudo cadastrado de um baralho especifico.

**Why this priority**: A consulta e essencial para validar o resultado do cadastro e dar visibilidade ao material de estudo antes de editar ou estudar.

**Independent Test**: Pode ser testada de forma independente listando flashcards de um deck proprio e obtendo um item especifico por id.

**Acceptance Scenarios**:

1. **Given** um usuario autenticado com flashcards em um deck proprio, **When** ele lista os flashcards desse deck, **Then** o sistema retorna somente os flashcards daquele deck.
2. **Given** um usuario autenticado, **When** ele solicita um flashcard por id cujo deck pai e de sua propriedade, **Then** o sistema retorna os dados completos do flashcard.
3. **Given** um usuario autenticado, **When** ele solicita flashcards de deck inexistente ou nao pertencente a ele, **Then** o sistema retorna 404.

---

### User Story 3 - Manter Flashcard Proprio (Priority: P3)

Como usuario autenticado, quero editar e excluir flashcards do meu deck para manter meu material atualizado e relevante.

**Why this priority**: Edicao e exclusao sustentam a qualidade do acervo, mas dependem da existencia previa de flashcards.

**Independent Test**: Pode ser testada de forma independente atualizando todos os campos de um flashcard proprio e depois removendo-o, incluindo tentativas nao autorizadas.

**Acceptance Scenarios**:

1. **Given** um usuario autenticado e um flashcard proprio existente, **When** ele edita todos os campos respeitando as regras do tipo, **Then** o flashcard e atualizado com sucesso.
2. **Given** um usuario autenticado, **When** ele tenta editar com dados invalidos para o tipo, **Then** a operacao e bloqueada com erro de validacao.
3. **Given** um usuario autenticado e um flashcard proprio existente, **When** ele solicita a exclusao, **Then** o flashcard e removido.
4. **Given** um usuario autenticado, **When** ele tenta editar ou excluir flashcard de deck alheio ou inexistente, **Then** o sistema retorna 404.

### Edge Cases

- Criacao ou edicao do tipo Cloze com lacunas sem sequencia (ex.: `{{c1::...}}` e `{{c3::...}}` sem `c2`) deve ser rejeitada.
- Criacao ou edicao do tipo MultiplaEscolha com menos de 2 alternativas, mais de 5 alternativas, nenhuma correta ou mais de uma correta deve ser rejeitada.
- Criacao ou edicao do tipo DigiteResposta com lista vazia ou itens vazios deve ser rejeitada.
- Criacao ou edicao do tipo FrenteVerso com pergunta ou resposta vazia deve ser rejeitada.
- Campo `subarea` com mais de 50 caracteres deve ser rejeitado.
- Campo `dicas` com mais de 3 itens deve ser rejeitado.
- Flashcard id inexistente deve retornar 404 nas operacoes de consulta individual, edicao e exclusao.
- Deck id inexistente ou de outro proprietario deve retornar 404 para criacao e listagem.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE permitir criar flashcard vinculado a um `deckId` existente e de propriedade do usuario autenticado.
- **FR-002**: O sistema DEVE retornar 404 quando houver tentativa de criar flashcard em deck inexistente ou nao pertencente ao usuario autenticado.
- **FR-003**: O sistema DEVE permitir listar flashcards apenas de um deck que pertenca ao usuario autenticado.
- **FR-004**: O sistema DEVE retornar 404 ao listar flashcards de deck inexistente ou sem propriedade do usuario autenticado.
- **FR-005**: O sistema DEVE permitir obter um flashcard por id apenas quando o deck pai pertencer ao usuario autenticado.
- **FR-006**: O sistema DEVE retornar 404 ao obter flashcard por id quando o flashcard nao existir, ou quando existir mas estiver associado a deck nao pertencente ao usuario autenticado.
- **FR-007**: O sistema DEVE permitir editar todos os campos de um flashcard proprio, incluindo mudanca de conteudo tipado e campos comuns.
- **FR-008**: O sistema DEVE aplicar validacoes bloqueantes de tipo na criacao e na edicao, conforme regras abaixo.
- **FR-009**: O sistema DEVE permitir excluir flashcard apenas quando o flashcard pertencer a deck do usuario autenticado.
- **FR-010**: O sistema DEVE retornar 404 ao excluir flashcard inexistente ou associado a deck que nao pertence ao usuario autenticado.
- **FR-011**: Para tipo FrenteVerso, o sistema DEVE exigir `pergunta` e `resposta` obrigatorias e nao vazias.
- **FR-012**: Para tipo Cloze, o sistema DEVE exigir `texto` obrigatorio contendo ao menos uma lacuna no formato `{{c1::termo}}` e numeracao sequencial iniciando em `c1`.
- **FR-013**: Para tipo DigiteResposta, o sistema DEVE exigir lista de respostas validas com pelo menos 1 item e sem itens vazios.
- **FR-014**: Para tipo MultiplaEscolha, o sistema DEVE exigir lista de alternativas com no minimo 2 e no maximo 5 itens, com exatamente uma alternativa marcada como correta.
- **FR-015**: O sistema DEVE exigir `dificuldade` em enum com valores permitidos `Facil`, `Medio` e `Dificil` para todos os tipos.
- **FR-016**: O sistema DEVE aceitar `subarea` opcional com tamanho maximo de 50 caracteres.
- **FR-017**: O sistema DEVE aceitar `dicas` opcional com no maximo 3 itens.
- **FR-018**: O sistema NAO DEVE incluir nesta feature os campos e comportamentos de agendamento de revisao (`nextReview`, `intervalDays`, `easeFactor`).
- **FR-019**: O sistema NAO DEVE incluir nesta feature historico de tentativas, validacao semantica via IA e versionamento/historico de edicao de flashcard.

### Key Entities *(include if feature involves data)*

- **Flashcard**: Unidade de estudo associada a um unico deck de um unico proprietario, contendo tipo (`FrenteVerso`, `Cloze`, `DigiteResposta`, `MultiplaEscolha`), dados especificos do tipo, `dificuldade`, `subarea` opcional e `dicas` opcionais.
- **Alternativa**: Item embutido no Flashcard para o tipo MultiplaEscolha, com texto e marcador de correta, obedecendo cardinalidade minima e maxima e unicidade de alternativa correta.
- **Deck (referencia de ownership)**: Agregado pai ja existente que define contexto e propriedade do Flashcard; todas as operacoes de Flashcard dependem da existencia e titularidade do Deck.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em testes de aceite, 100% das operacoes de criacao e edicao com dados validos para os 4 tipos de flashcard sao concluidas com sucesso.
- **SC-002**: Em testes de aceite, 100% das operacoes de criacao, listagem, consulta, edicao e exclusao em recursos inexistentes ou sem propriedade do usuario retornam 404.
- **SC-003**: Pelo menos 95% dos usuarios de homologacao conseguem cadastrar um novo flashcard valido em menos de 2 minutos na primeira tentativa.
- **SC-004**: Em homologacao, 100% dos cenarios de validacao bloqueante por tipo rejeitam payloads invalidos com mensagem de erro compreensivel ao usuario.

## Assumptions

- O fluxo de autenticacao e identificacao do usuario autenticado ja esta disponivel pela feature anterior.
- Decks ja existem e possuem regra de propriedade por usuario aplicada de forma consistente.
- Operacoes desta feature serao consumidas por cliente autenticado e nao incluem experiencias anonimas.
- A resposta de erro para validacoes bloqueantes segue padrao de erro funcional ja adotado no produto.
- Sessao de estudo, agendamento SM-2, historico de tentativas e IA semantica serao tratados em feature posterior especifica.
