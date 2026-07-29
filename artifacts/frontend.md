Alta prioridade
1. Autenticação e usuários
O frontend exibe um usuário fixo (“Usuário” e, no chat, “Lucas Martins”), sem login nem perfil real.

Backend necessário:

Cadastro de usuário.
Login e logout.
Autenticação JWT.
Refresh token ou renovação de sessão.
Recuperação/troca de senha.
Perfil do usuário: nome, e-mail, foto opcional, preferências.
Autorização para garantir que cada usuário acessa apenas seus dados.
Persistência da preferência de tema, que hoje fica em localStorage.
Entidades sugeridas: User, RefreshToken, UserPreference.

2. Gestão de baralhos
O usuário já cria e exclui baralhos no frontend, mas eles não sobrevivem fora do navegador e não pertencem a uma conta. O modelo está em types.ts:29-40, e as operações locais em App.tsx:192-208 e App.tsx:302-313.

Backend necessário:

Criar baralho.
Listar os baralhos do usuário autenticado.
Obter detalhes de um baralho, incluindo seus flashcards.
Editar nome, descrição, dificuldade e categorias.
Excluir baralho e seus flashcards associados.
Pesquisa de baralhos por nome.
Paginação, caso a quantidade de baralhos cresça.
Validações: nome obrigatório, tamanho de campos, limite de categorias, acesso do proprietário.
Entidades sugeridas: Deck, Category ou DeckCategory.

Endpoints iniciais:
GET    /api/decks
POST   /api/decks
GET    /api/decks/{deckId}
PUT    /api/decks/{deckId}
DELETE /api/decks/{deckId}
GET    /api/decks?search=kotlin&page=1&pageSize=20

3. Gestão completa de flashcards
O frontend já permite criar, editar e excluir cards, com tipos, alternativas, dicas e gabarito. Essas operações são locais em App.tsx:235-300, com o contrato em types.ts:10-27.

Backend necessário:

Criar flashcard em um baralho.
Listar flashcards de um baralho.
Buscar flashcard individual.
Editar todos os atributos.
Excluir flashcard.
Validar regras por tipo:
FrenteVerso: pergunta e resposta obrigatórias.
Cloze: sintaxe de lacunas válida, por exemplo {{c1::termo}}.
DigiteResposta: resposta esperada obrigatória.
MultiplaEscolha: alternativas obrigatórias, limite de cinco, uma resposta correta presente na lista.
Persistir dificuldade, subárea, dicas e alternativas.
Suportar futuramente versão/histórico de edição, caso seja útil academicamente.
Entidades sugeridas: Flashcard, FlashcardAlternative, FlashcardHint.

Endpoints iniciais:
GET    /api/decks/{deckId}/flashcards
POST   /api/decks/{deckId}/flashcards
GET    /api/flashcards/{flashcardId}
PUT    /api/flashcards/{flashcardId}
DELETE /api/flashcards/{flashcardId}

4. Sessões de estudo, respostas e repetição espaçada
Esta é a funcionalidade pedagógica central. O frontend já controla card atual, tempo, sequência de acertos, progresso e navegação, mas nada é salvo. O modelo já prevê nextReview, intervalDays e easeFactor para SM-2 em types.ts:21-25.

Backend necessário:

Iniciar uma sessão de estudo para um baralho.
Retornar cards pendentes de revisão, ordenados por nextReview.
Registrar a resposta do usuário a cada card.
Registrar score, acerto, tempo de resposta, dicas visualizadas e data.
Atualizar o algoritmo de repetição espaçada por resposta:
nextReview;
intervalDays;
easeFactor;
quantidade de revisões;
acertos e erros.
Concluir/cancelar sessão.
Calcular sequência de estudo (streak), tempo total e cards concluídos.
Garantir idempotência para não registrar duas respostas por clique/requisição repetida.
Retornar o próximo card de acordo com a agenda de revisão.
Entidades sugeridas: StudySession, StudyAnswer, UserFlashcardProgress, ReviewSchedule.

Endpoints iniciais:
POST /api/study-sessions
GET  /api/study-sessions/{sessionId}/next-card
POST /api/study-sessions/{sessionId}/answers
POST /api/study-sessions/{sessionId}/complete
GET  /api/reviews/due

Importante: o algoritmo SM-2 deve ficar no backend, não no React. Assim, o cronograma fica confiável e o usuário pode continuar o estudo em outro dispositivo.

5. Dashboard e métricas reais
Os indicadores atuais são valores estáticos: domínio 55%, tempo 20d, cards concluídos 180, retenção 85%, domínio do baralho 45% e revisões 12, em App.tsx:391-470.

Backend necessário:

Indicadores globais do usuário:
domínio geral;
tempo total de estudo;
número de cards concluídos;
taxa de retenção/acerto;
sequência atual e maior sequência;
cards pendentes para revisão.
Métricas por baralho:
total de cards;
cards pendentes;
domínio estimado;
desempenho por dificuldade, subárea e tipo de card.
Séries históricas para gráficos futuros: estudo por dia, acertos/erros e evolução de domínio.
Endpoints iniciais:
GET /api/dashboard
GET /api/decks/{deckId}/statistics
GET /api/analytics/study-history?from=2026-01-01&to=2026-01-31

Média prioridade
6. Validação semântica de respostas com IA
O tipo DigiteResposta foi modelado, e há um contrato pronto para retorno de IA em types.ts:58-65. Contudo, a tela atual usa setTimeout e score fixo em vez de chamar ApiService.validateResponseViaAI.

Backend necessário:

Endpoint que recebe card, gabarito e resposta do aluno.
Integração do ASP.NET Core com um provedor LLM, como Azure OpenAI, OpenAI ou outro.
Prompt estruturado para:
comparar semanticamente resposta e gabarito;
gerar score de 0 a 100;
classificar correto/incorreto;
gerar feedback pelo Método Feynman;
sugerir melhorias;
identificar tópicos que precisam de reforço.
Salvar o resultado da avaliação em StudyAnswer.
Aplicar limite de requisições por usuário e por período, devido a custo.
Não expor a chave da IA ao frontend.
Registrar auditoria técnica do provedor, modelo, latência e falhas, sem armazenar dados sensíveis além do necessário.
Endpoint:
POST /api/flashcards/{flashcardId}/evaluate-answer

O retorno pode seguir diretamente a interface ValidationResult existente.

7. Tutor IA conversacional
O chat está implementado visualmente, mas a resposta é fixa e gerada por setTimeout em App.tsx:315-340.

Backend necessário:

Criar conversas do usuário.
Salvar mensagens do usuário e do tutor.
Enviar pergunta para o LLM com orientação do Método Feynman.
Opcionalmente incluir contexto do usuário: baralho em estudo, cards errados, tópicos de reforço e dificuldade.
Recuperar conversas anteriores.
Renomear, arquivar ou excluir conversas.
Rate limiting, limite de tamanho de mensagem e moderação básica.
Preferencialmente usar streaming com Server-Sent Events ou SignalR para exibir a resposta gradualmente.
Entidades sugeridas: ChatConversation, ChatMessage.

Endpoints:
GET    /api/chat/conversations
POST   /api/chat/conversations
GET    /api/chat/conversations/{conversationId}/messages
POST   /api/chat/conversations/{conversationId}/messages
DELETE /api/chat/conversations/{conversationId}

8. Upload e processamento de PDFs/slides
A interface indica anexos “PDF” e “Slides”, mas hoje só altera uma string local e não seleciona nem envia arquivo em App.tsx:679-694.

Backend necessário:

Upload real de PDF, PPT/PPTX e, se desejado, DOCX.
Validação de extensão, MIME type, tamanho máximo e antivírus.
Armazenamento em disco, Azure Blob Storage, S3 ou equivalente.
Registro de metadados: proprietário, nome original, tipo, tamanho, data e status.
Extração de texto do arquivo.
Processamento assíncrono para arquivos maiores.
Vincular arquivo ao deck ou flashcard.
Disponibilizar o texto extraído como contexto para IA.
Excluir arquivo e seus dados derivados quando necessário.
Entidades sugeridas: StudyDocument, DocumentProcessingJob, DocumentTextChunk.

Endpoints:
POST   /api/documents
GET    /api/documents/{documentId}
DELETE /api/documents/{documentId}
GET    /api/documents/{documentId}/status

9. Geração de flashcards por IA
A função já existe em apiService.ts:33-50, embora ainda não esteja conectada a um botão da interface. Ela gera apenas cards mockados.

Backend necessário:

Receber tema, tipo de flashcard, dificuldade desejada, quantidade e documento de contexto opcional.
Gerar flashcards estruturados por IA.
Validar o JSON retornado pelo modelo.
Garantir conformidade com cada tipo de flashcard.
Retornar uma prévia para o usuário revisar antes de salvar.
Salvar apenas cards explicitamente aprovados pelo usuário.
Registrar origem do card: manual, IA por tema ou IA por documento.
Endpoints:
POST /api/ai/flashcards/generate
POST /api/ai/flashcards/save-approved

10. Locais favoritos e geofencing
Há tipo para localização, UI mostrando um local e mock de locais em apiService.ts:24-31. Porém, o frontend não usa a API de geolocalização do navegador: sempre usa o primeiro local retornado.

Backend necessário:

CRUD de locais favoritos.
Salvar nome, latitude, longitude e raio.
Associar métricas de estudo por local:
sessões;
taxa média de acerto;
tipos de cards mais usados.
Receber evento de estudo contendo localização, mediante consentimento explícito do usuário.
Calcular se o usuário estava dentro do raio configurado.
Gerar recomendação contextual, como sugerir cards adequados àquele local.
Entidades sugeridas: FavoriteLocation, StudyLocationEvent.

Endpoints:
GET    /api/locations
POST   /api/locations
PUT    /api/locations/{locationId}
DELETE /api/locations/{locationId}
POST   /api/study-sessions/{sessionId}/location-events
GET    /api/locations/{locationId}/statistics

A captura de GPS é responsabilidade do navegador (navigator.geolocation), mas persistência, análise e recomendações são responsabilidades do backend. Como são dados sensíveis, inclua consentimento, revogação e política de retenção.

Baixa prioridade
11. Sincronização, modo offline e resolução de conflitos
Hoje tudo é salvo localmente. Depois de existir API, o frontend ainda pode manter cache offline.

Backend necessário:

Versão/data de atualização por deck, card e progresso.
Endpoint de sincronização incremental.
Estratégia de conflito, por exemplo “última atualização vence” ou revisão manual.
Exclusões lógicas/tombstones para sincronizar remoções.
Backup/exportação dos dados do usuário.
Endpoint possível:
POST /api/sync

12. Notificações de revisão
Com nextReview salvo no servidor, é possível avisar o usuário sobre cards pendentes.

Backend necessário:

Preferências de notificação.
Job agendado para identificar revisões vencidas.
E-mail, push notification ou ambos.
Frequência configurável e opção de desativar.
Entidades sugeridas: NotificationPreference, NotificationLog.

13. Relatórios e exportação
Não há UI pronta, mas os dados de estudo podem justificar recursos acadêmicos e administrativos.

Backend necessário:

Exportar baralhos e cards em JSON/CSV.
Exportar histórico de estudo e desempenho em PDF/CSV.
Relatório por período, deck, subárea e dificuldade.
Importação de decks em formato compatível, se desejado.
14. Administração e auditoria
Não aparece no frontend, mas se houver apresentação institucional ou múltiplos usuários, é recomendável.

Backend necessário:

Perfis User e Admin.
Administração de usuários e conteúdos.
Logs de auditoria para exclusões, alterações críticas e uso de IA.
Métricas de saúde da aplicação e acompanhamento de falhas.
Configuração segura de provedores de IA e armazenamento.
Ordem prática de implementação em ASP.NET Core
Estruture a solução com Web API, Application, Domain e Infrastructure.
Configure EF Core, banco relacional e migrations. Para TCC/local, SQLite é suficiente; para produção, prefira PostgreSQL ou SQL Server.
Implemente Identity/JWT e o usuário autenticado.
Implemente CRUD de decks e flashcards.
Modele StudySession, StudyAnswer e UserFlashcardProgress, incluindo o SM-2 no servidor.
Crie endpoints de dashboard calculados a partir das respostas.
Integre IA para validação e tutor.
Adicione upload/processamento de documentos.
Implemente localização, notificações, sincronização e relatórios.
O núcleo mínimo viável que torna o projeto realmente multiusuário e funcional é: autenticação, decks, flashcards, sessões/respostas, repetição espaçada e dashboard. IA, documentos e localização enriquecem bastante a proposta, mas dependem desse núcleo persistido primeiro.