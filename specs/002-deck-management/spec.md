# Feature Specification: Gestão de Baralhos (Decks)

**Feature Branch**: `002-deck-management`

**Created**: 2026-07-29

**Status**: Draft

**Input**: User description: "Gestão de baralhos (decks) do CogniLink — segunda feature, após autenticação. Usuário autenticado cria, lista, visualiza, edita e exclui baralhos próprios (nome, descrição, dificuldade, categorias). Acesso restrito ao dono do baralho, com retorno 404 para baralhos de outros usuários."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Criar baralho (Priority: P1)

Como usuário autenticado, quero criar um novo baralho informando nome, descrição opcional, dificuldade e categorias, para começar a organizar meu conteúdo de estudo.

**Why this priority**: Sem a criação de baralhos nenhuma outra funcionalidade da feature tem sentido — é a base de todo o fluxo de gestão de baralhos.

**Independent Test**: Pode ser testado autenticando um usuário e enviando uma requisição de criação de baralho com dados válidos; o sistema deve persistir o baralho vinculado ao usuário e retornar seus dados, incluindo contagem de cards igual a zero.

**Acceptance Scenarios**:

1. **Given** um usuário autenticado, **When** ele cria um baralho com nome válido (3-100 caracteres), dificuldade e até 5 categorias válidas, **Then** o baralho é criado e vinculado ao seu `userId`, com contagem de cards igual a 0.
2. **Given** um usuário autenticado, **When** ele tenta criar um baralho sem informar o nome, **Then** o sistema rejeita a requisição com erro de validação.
3. **Given** um usuário autenticado, **When** ele cria um baralho sem informar descrição, **Then** o baralho é criado normalmente com descrição vazia/nula.
4. **Given** um usuário autenticado, **When** ele tenta criar um baralho com mais de 5 categorias ou categoria com mais de 30 caracteres, **Then** o sistema rejeita a requisição com erro de validação.
5. **Given** um usuário autenticado, **When** ele tenta criar um baralho com dificuldade fora dos valores permitidos (Fácil, Médio, Difícil), **Then** o sistema rejeita a requisição com erro de validação.

---

### User Story 2 - Listar baralhos próprios (Priority: P1)

Como usuário autenticado, quero listar apenas os meus próprios baralhos, podendo buscar por nome e navegar por páginas, para encontrar rapidamente o baralho que preciso.

**Why this priority**: É a forma primária de navegação e descoberta dos baralhos já criados; tão essencial quanto a criação para tornar a feature utilizável.

**Independent Test**: Pode ser testado criando múltiplos baralhos para dois usuários diferentes e verificando que a listagem de um usuário retorna apenas os seus próprios baralhos, respeitando busca por nome e paginação.

**Acceptance Scenarios**:

1. **Given** um usuário autenticado com baralhos próprios e outros usuários com seus próprios baralhos, **When** ele lista seus baralhos, **Then** somente os baralhos de sua propriedade são retornados.
2. **Given** um usuário com vários baralhos, **When** ele busca por um termo presente no nome de alguns baralhos, **Then** somente os baralhos cujo nome contém o termo (sem diferenciar maiúsculas/minúsculas) são retornados.
3. **Given** um usuário com mais baralhos do que o tamanho de uma página, **When** ele solicita uma página específica com um tamanho de página, **Then** o sistema retorna somente os itens daquela página, junto com metadados de paginação (total de itens, página atual, tamanho de página).
4. **Given** um usuário sem nenhum baralho, **When** ele lista seus baralhos, **Then** o sistema retorna uma lista vazia sem erro.

---

### User Story 3 - Visualizar detalhes de um baralho (Priority: P2)

Como usuário autenticado, quero visualizar os detalhes de um baralho que me pertence, para conferir suas informações completas.

**Why this priority**: Complementa a listagem, permitindo ver informações completas antes de editar; não é bloqueante para o MVP de criação/listagem, mas necessário para o fluxo de edição.

**Independent Test**: Pode ser testado criando um baralho para um usuário e requisitando seus detalhes por id, validando que os dados retornados correspondem ao que foi criado.

**Acceptance Scenarios**:

1. **Given** um baralho pertencente ao usuário autenticado, **When** ele solicita os detalhes por id, **Then** o sistema retorna nome, descrição, dificuldade, categorias e contagem de cards (0, por ora).
2. **Given** um baralho pertencente a outro usuário, **When** o usuário autenticado solicita os detalhes por id, **Then** o sistema retorna um erro de "não encontrado" (404), sem indicar que o baralho existe.
3. **Given** um id de baralho inexistente, **When** o usuário autenticado solicita os detalhes, **Then** o sistema retorna um erro de "não encontrado" (404).

---

### User Story 4 - Editar baralho próprio (Priority: P2)

Como usuário autenticado, quero editar o nome, a descrição, a dificuldade e as categorias de um baralho que me pertence, para manter suas informações atualizadas.

**Why this priority**: Importante para manutenção contínua dos baralhos, mas depende da criação e visualização já existirem.

**Independent Test**: Pode ser testado criando um baralho, enviando uma edição válida e confirmando que os novos valores são persistidos e retornados na consulta subsequente.

**Acceptance Scenarios**:

1. **Given** um baralho pertencente ao usuário autenticado, **When** ele edita nome, descrição, dificuldade e/ou categorias com valores válidos, **Then** o sistema persiste as alterações e retorna os dados atualizados.
2. **Given** um baralho pertencente ao usuário autenticado, **When** ele tenta editar com dados inválidos (ex.: nome muito curto, mais de 5 categorias), **Then** o sistema rejeita a requisição com erro de validação e não altera os dados.
3. **Given** um baralho pertencente a outro usuário, **When** o usuário autenticado tenta editá-lo, **Then** o sistema retorna um erro de "não encontrado" (404).

---

### User Story 5 - Excluir baralho próprio (Priority: P3)

Como usuário autenticado, quero excluir um baralho que me pertence, para remover conteúdo que não uso mais.

**Why this priority**: Ação destrutiva e menos frequente que as demais; complementa o ciclo de vida do baralho, mas não é essencial ao MVP inicial de criação/consulta.

**Independent Test**: Pode ser testado criando um baralho, excluindo-o e confirmando que ele deixa de aparecer na listagem e retorna 404 em consultas subsequentes.

**Acceptance Scenarios**:

1. **Given** um baralho pertencente ao usuário autenticado, **When** ele solicita a exclusão, **Then** o baralho é removido e deixa de aparecer na listagem e em consultas de detalhe.
2. **Given** um baralho pertencente a outro usuário, **When** o usuário autenticado tenta excluí-lo, **Then** o sistema retorna um erro de "não encontrado" (404) e o baralho do outro usuário permanece intacto.

---

### Edge Cases

- O que acontece se o usuário enviar categorias duplicadas na mesma lista? (Assumido: duplicatas são aceitas sem deduplicação automática, pois não há requisito de unicidade especificado.)
- Como o sistema trata paginação com `page` ou `pageSize` inválidos (zero, negativo, ou excessivamente grande)? (Assumido: valores inválidos são normalizados para os padrões mínimos/máximos definidos.)
- O que acontece se o usuário buscar por um termo vazio? (Assumido: retorna todos os baralhos do usuário, como se nenhuma busca tivesse sido aplicada.)
- Como o sistema trata a tentativa de acessar um baralho com um id em formato inválido? (Assumido: tratado como "não encontrado" (404), mantendo a consistência do RF-06.)
- O que acontece se todos os campos editáveis forem omitidos em uma edição? (Assumido: campos omitidos mantêm o valor atual; ao menos um campo válido deve ser fornecido.)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE permitir que um usuário autenticado crie um baralho vinculado ao seu `userId` (owner), com nome, descrição opcional, dificuldade e categorias.
- **FR-002**: O sistema DEVE listar somente os baralhos pertencentes ao usuário autenticado, com suporte a busca por nome (contém, sem diferenciar maiúsculas/minúsculas) e paginação por `page` e `pageSize`.
- **FR-003**: O sistema DEVE permitir obter os detalhes de um baralho por id, somente se ele pertencer ao usuário autenticado.
- **FR-004**: O sistema DEVE permitir editar nome, descrição, dificuldade e categorias de um baralho pertencente ao usuário autenticado.
- **FR-005**: O sistema DEVE permitir excluir um baralho pertencente ao usuário autenticado.
- **FR-006**: O sistema DEVE retornar erro "não encontrado" (404) para qualquer operação (visualizar, editar, excluir) sobre um baralho que não pertença ao usuário autenticado, nunca revelando sua existência através de um erro de permissão (403).
- **FR-007**: O sistema DEVE validar que o nome do baralho é obrigatório e possui entre 3 e 100 caracteres.
- **FR-008**: O sistema DEVE validar que a descrição, quando informada, possui no máximo 500 caracteres.
- **FR-009**: O sistema DEVE validar que a lista de categorias possui no máximo 5 itens, cada um com no máximo 30 caracteres.
- **FR-010**: O sistema DEVE validar que a dificuldade pertence a um conjunto fixo de valores: Fácil, Médio ou Difícil.
- **FR-011**: O sistema DEVE retornar a contagem de cards de um baralho como 0 em todas as consultas de detalhe, visto que a associação de flashcards está fora do escopo desta feature.
- **FR-012**: O sistema NÃO DEVE excluir em cascata flashcards ou quaisquer outras entidades relacionadas ao excluir um baralho, pois essa associação ainda não existe nesta feature.

### Key Entities *(include if feature involves data)*

- **Deck (Baralho)**: Representa uma coleção nomeada de estudo pertencente a um usuário. Atributos: identificador único, nome (3-100 caracteres), descrição opcional (até 500 caracteres), dificuldade (Fácil, Médio ou Difícil), lista de categorias (até 5 strings de até 30 caracteres cada, embutida no próprio baralho, sem entidade própria), `userId` do proprietário (owner), contagem de cards (fixa em 0 nesta feature), datas de criação e última atualização.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Usuários conseguem criar um novo baralho em menos de 30 segundos, do preenchimento dos dados até a confirmação de criação.
- **SC-002**: 100% das tentativas de acesso, edição ou exclusão de baralhos de outros usuários resultam em resposta "não encontrado", sem exposição de dados ou existência do recurso.
- **SC-003**: Usuários com centenas de baralhos conseguem localizar um baralho específico por busca de nome em até 3 interações (busca + escolha na lista).
- **SC-004**: A listagem de baralhos retorna resultados paginados corretamente para qualquer volume de baralhos por usuário, sem degradação perceptível para o usuário.

## Assumptions

- A autenticação de usuários (feature anterior) já está implementada e disponível para identificar o usuário autenticado (`userId`) em todas as operações desta feature.
- Categorias são strings livres definidas pelo usuário no momento da criação/edição do baralho, sem cadastro prévio, moderação ou entidade própria.
- Duplicatas na lista de categorias são permitidas, pois não há requisito de unicidade especificado.
- Valores padrão de paginação (ex.: tamanho de página padrão e máximo) seguem práticas comuns de APIs REST e serão definidos durante o planejamento técnico.
- A contagem de cards por baralho será sempre 0 até que a feature de flashcards seja implementada.
- Não há requisito de auditoria ou histórico de alterações de baralhos nesta feature.
- A exclusão de baralho é definitiva (hard delete), não havendo requisito de exclusão lógica (soft delete) especificado.
