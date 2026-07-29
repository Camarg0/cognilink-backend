# Feature Specification: Autenticacao e Gestao de Perfil

**Feature Branch**: `001-auth-profile-management`

**Created**: 2026-07-28

**Status**: Draft

**Input**: User description: "Sistema de autenticacao e gestao de perfil do CogniLink."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Criar Conta e Acessar Dados Proprios (Priority: P1)

Um visitante cria uma conta com nome, e-mail e senha e passa a acessar apenas seus proprios decks, flashcards, progresso e demais dados de estudo.

**Why this priority**: Esta jornada viabiliza a conta individual e o isolamento de dados, que sao a base do MVP do CogniLink.

**Independent Test**: Um visitante se cadastra, entra na conta e confirma que consegue acessar seus dados, enquanto uma tentativa de acessar um recurso pertencente a outra conta e negada.

**Acceptance Scenarios**:

1. **Given** um visitante sem conta, **When** informa nome, e-mail valido e senha forte ainda nao cadastrada, **Then** uma conta individual e criada e ele passa a ter uma sessao autenticada.
2. **Given** duas contas distintas com dados de estudo, **When** uma conta tenta consultar, modificar ou excluir um recurso da outra, **Then** a operacao e negada e nenhum dado da outra conta e revelado.
3. **Given** um visitante tentando se cadastrar com e-mail ja usado, **When** envia o cadastro, **Then** recebe uma mensagem clara para usar outro e-mail ou recuperar o acesso, sem confirmar a existencia de uma conta associada ao endereco.
4. **Given** um visitante com e-mail inexistente ou senha incorreta, **When** tenta entrar, **Then** recebe a mesma mensagem generica de credenciais invalidas em ambos os casos.

---

### User Story 2 - Manter e Encerrar Sessao (Priority: P1)

Um usuario cadastrado entra com e-mail e senha, permanece autenticado por um periodo controlado e pode encerrar a sessao, tornando o acesso anterior inutilizavel.

**Why this priority**: A experiencia de acesso continuo precisa coexistir com controle de validade e revogacao de sessao para proteger contas de estudantes.

**Independent Test**: Um usuario entra, renova a sessao dentro do prazo, encerra a sessao e confirma que nao consegue mais usar as credenciais de acesso anteriormente emitidas.

**Acceptance Scenarios**:

1. **Given** um usuario com credenciais validas, **When** entra na conta, **Then** recebe credenciais de acesso de curta duracao e de continuidade de sessao de longa duracao.
2. **Given** uma credencial de continuidade valida, **When** o usuario renova a sessao, **Then** recebe novas credenciais e a credencial de continuidade usada deixa de ser valida.
3. **Given** uma credencial de continuidade expirada ou ja utilizada, **When** o usuario tenta renovar a sessao, **Then** a renovacao e recusada e um novo login e exigido.
4. **Given** um usuario autenticado, **When** encerra a sessao, **Then** a credencial de continuidade ativa e revogada e nao pode renovar a sessao.

---

### User Story 3 - Recuperar e Alterar Senha (Priority: P2)

Um usuario que esqueceu a senha solicita uma redefinicao por e-mail; um usuario autenticado tambem pode trocar a senha fornecendo a senha atual.

**Why this priority**: A recuperacao reduz perda de acesso, enquanto a troca autenticada permite que usuarios reajam a suspeitas de comprometimento.

**Independent Test**: Um usuario solicita a redefinicao, usa o link recebido dentro do prazo para cadastrar uma senha forte e entra com ela; tambem consegue trocar a senha quando informa corretamente a senha atual.

**Acceptance Scenarios**:

1. **Given** um usuario cadastrado, **When** solicita a redefinicao de senha, **Then** recebe por e-mail uma instrucao com token de uso unico e validade limitada.
2. **Given** um token valido de redefinicao, **When** o usuario define uma senha forte, **Then** a senha anterior deixa de permitir novos logins e o token nao pode ser reutilizado.
3. **Given** um token expirado, invalido ou ja utilizado, **When** o usuario tenta redefinir a senha, **Then** a redefinicao e negada com orientacao para iniciar uma nova solicitacao.
4. **Given** um usuario autenticado, **When** informa corretamente a senha atual e uma nova senha forte, **Then** a senha e alterada; se a senha atual estiver errada, a alteracao e negada.

---

### User Story 4 - Gerenciar Perfil e Preferencias (Priority: P2)

Um usuario autenticado consulta e atualiza seu nome, e-mail, foto opcional e tema de preferencia sem alterar o perfil de outra pessoa.

**Why this priority**: O perfil permite personalizacao basica e manutencao dos dados de contato da conta.

**Independent Test**: Um usuario consulta o perfil, altera cada campo permitido e confirma que as alteracoes persistem somente em sua propria conta.

**Acceptance Scenarios**:

1. **Given** um usuario autenticado, **When** consulta seu perfil, **Then** ve nome, e-mail, foto quando definida e tema de preferencia atuais.
2. **Given** um usuario autenticado, **When** atualiza nome, e-mail, foto opcional ou tema com valores validos, **Then** as alteracoes ficam disponiveis em consultas futuras de seu perfil.
3. **Given** um usuario autenticado, **When** tenta usar no perfil um e-mail associado a outra conta, **Then** a atualizacao e negada sem expor dados da outra conta.

### Edge Cases

- Tentativas de login com e-mail inexistente e com senha incorreta retornam a mesma resposta de credenciais invalidas.
- Uma credencial de continuidade expirada, revogada ou ja utilizada nao pode criar uma nova sessao e exige novo login.
- Um token de redefinicao de senha e aceito somente uma vez e somente dentro de seu prazo de validade.
- O cadastro e a atualizacao de e-mail protegem contra enumeracao de contas: nao revelam se um endereco ja esta vinculado a outra pessoa.
- Recursos sem proprietario correspondente ao usuario autenticado sao tratados como indisponiveis para esse usuario, sem revelar detalhes do recurso ou de seu dono.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE permitir que visitantes criem uma conta com nome, e-mail e senha, aceitando somente e-mails em formato valido, e-mails nao associados a outra conta e senhas que atendam a politica de senha forte definida pelo produto.
- **FR-002**: O sistema DEVE criar uma identidade de proprietario unica para cada conta e vincular todos os decks, flashcards, dados de progresso e conversas de estudo ao respectivo proprietario.
- **FR-003**: O sistema DEVE autenticar usuarios por e-mail e senha e retornar credenciais distintas para acesso de curta duracao e continuidade de sessao de longa duracao.
- **FR-004**: O sistema DEVE renovar uma sessao valida por meio da credencial de continuidade e invalidar a credencial de continuidade anterior ao emitir a nova.
- **FR-005**: O sistema DEVE recusar a renovacao de sessao por credencial expirada, revogada ou usada anteriormente, exigindo novo login.
- **FR-006**: O sistema DEVE permitir que o usuario encerre a sessao e revogar a credencial de continuidade associada, impedindo futuras renovacoes com ela.
- **FR-007**: O sistema DEVE fornecer recuperacao de senha por e-mail usando um token de uso unico com prazo de validade e invalidar esse token apos uma redefinicao bem-sucedida.
- **FR-008**: O sistema DEVE permitir a troca de senha de um usuario autenticado somente apos a confirmacao da senha atual e somente para uma nova senha que atenda a politica de senha forte.
- **FR-009**: O sistema DEVE permitir que um usuario autenticado consulte e atualize apenas seu proprio nome, e-mail, foto opcional e tema de preferencia.
- **FR-010**: O sistema DEVE validar a unicidade do e-mail no cadastro e na atualizacao de perfil e comunicar conflitos de forma que nao permita confirmar a existencia de uma conta vinculada a um endereco especifico.
- **FR-011**: O sistema DEVE retornar uma resposta indistinguivel para e-mail inexistente e senha incorreta durante o login.
- **FR-012**: Toda operacao autenticada DEVE resolver o proprietario a partir da identidade autenticada e negar leitura, criacao vinculada, atualizacao ou exclusao de recursos pertencentes a outros usuarios.
- **FR-013**: O sistema DEVE preservar o isolamento de proprietario mesmo quando o usuario fornece identificadores de recursos de outra conta diretamente em uma solicitacao.

### Key Entities

- **Usuario**: Titular da conta individual, identificado por nome e e-mail; possui credenciais de acesso e um perfil com foto opcional e tema de preferencia.
- **Sessao**: Contexto de autenticacao de um usuario que registra a credencial de continuidade, sua validade, estado de revogacao e uso para renovacao.
- **Token de Redefinicao de Senha**: Autorizacao temporaria, de uso unico, associada a um usuario para permitir a definicao de uma nova senha.
- **Preferencia de Usuario**: Configuracao pessoal do usuario, incluindo o tema selecionado e a foto opcional do perfil.
- **Recurso de Estudo**: Deck, flashcard, progresso ou conversa de estudo que possui um usuario como proprietario e so pode ser acessado por ele.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Pelo menos 95% dos participantes em teste conseguem criar uma conta valida e iniciar a primeira sessao em ate 2 minutos, sem assistencia.
- **SC-002**: Pelo menos 95% das tentativas validas de login, renovacao de sessao, consulta de perfil e atualizacao de perfil recebem resultado em ate 2 segundos sob a carga esperada do MVP.
- **SC-003**: Em uma bateria de 100 tentativas com identificadores de recursos de outras contas, 100% sao negadas sem expor conteudo, progresso ou dados do proprietario.
- **SC-004**: Em testes de seguranca funcional, 100% das credenciais de continuidade revogadas, expiradas ou ja usadas sao incapazes de renovar uma sessao.
- **SC-005**: Pelo menos 90% dos participantes em teste conseguem concluir a redefinicao de senha por e-mail em ate 5 minutos quando possuem acesso a caixa de entrada.
- **SC-006**: Pelo menos 90% dos participantes consideram claras as mensagens de erro para credenciais invalidas, sessao expirada e conflitos de e-mail, sem que as mensagens revelem a existencia de outra conta.

## Assumptions

- O produto define e mantem uma politica de senha forte antes da disponibilizacao desta funcionalidade; a politica se aplica igualmente a cadastro, redefinicao e troca de senha.
- O e-mail informado pelo usuario e um canal disponivel e acessivel para receber instrucoes de redefinicao de senha.
- A validade de credenciais de acesso, continuidade de sessao e redefinicao de senha sera configurada pelo produto de acordo com sua politica de seguranca.
- O tema de preferencia e uma escolha entre os temas disponibilizados pelo produto; a foto de perfil pode permanecer ausente.
- Login social, autenticacao em dois fatores e confirmacao de e-mail por link nao fazem parte deste escopo e permanecem como melhorias futuras.