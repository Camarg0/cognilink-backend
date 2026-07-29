# Research: Dashboard e Métricas

Não há marcadores `[NEEDS CLARIFICATION]` no spec desta feature. As decisões abaixo consolidam os pontos de integração com o código já existente das features 001-004, já que esta feature é inteiramente somente-leitura sobre entidades já implementadas.

## 1. Centralizar a regra de mastery em Domain

- Decision: Adicionar `UserFlashcardProgress.IsMastered()` (`Repetitions >= 2 && IntervalDays >= 21`) como método de domínio na entidade já existente (feature 004), em vez de recalcular a mesma condição em cada query de Application.
- Rationale: Evita duplicar a regra de negócio de mastery entre `GetDashboard` e `GetDeckStatistics`; mantém a regra pura em Domain, testável isoladamente e alinhada ao princípio já adotado na feature 004 de manter cálculo central no servidor/domínio.
- Alternatives considered: Calcular a condição diretamente em cada query handler; rejeitado por duplicar a regra em dois lugares e criar risco de divergência futura caso o limiar de mastery mude.

## 2. Reaproveitar GetUserStreak e GetDueFlashcards via MediatR

- Decision: `GetDashboardQueryHandler` injeta `IMediator`/`ISender` e envia `GetUserStreakQuery` e `GetDueFlashcardsQuery` (ambas já existentes na feature 004) para obter `UserStreakDto` e a contagem de cards pendentes, em vez de reimplementar a lógica de streak/pendências.
- Rationale: Atende literalmente ao pedido do usuário ("reutilizar cálculo já existente da feature 004") e à FR-007/FR-008 do spec; garante que dashboard e sessões de estudo nunca divirjam no mesmo cálculo (edge case do spec).
- Alternatives considered: Duplicar a lógica de streak/pendências dentro do novo handler chamando os mesmos repositórios diretamente; rejeitado por violar DRY e criar risco real de divergência de resultado entre as duas features (peso do SC-006).

## 3. Agregação de `StudyAnswer` por usuário sem novo campo de ownership

- Decision: Como `StudyAnswer` não possui `OwnerId` (só `SessionId`/`FlashcardId`), a agregação de respostas do usuário é feita buscando as sessões do usuário (`IStudySessionRepository`) e, para cada sessão, as respostas via `IStudyAnswerRepository.ListBySessionIdAsync` (já existente), consolidando tudo em memória.
- Rationale: Preserva a restrição explícita de não introduzir novas entidades de escrita nem novo schema em `StudyAnswer`; reaproveita exatamente o método de repositório que já existe hoje.
- Alternatives considered: Denormalizar `OwnerId` no documento `studyAnswers` para permitir uma consulta direta por dono; rejeitado nesta etapa por exigir mudança de schema/dados já persistidos, fora do escopo "sem novas entidades de escrita" definido pelo usuário. Fica registrado como possível otimização futura caso o volume de sessões por usuário cresça o suficiente para tornar o fan-out (item 5) um problema de desempenho.

## 4. Novo método `ListByOwnerAsync` em `IStudySessionRepository`

- Decision: Adicionar `Task<IReadOnlyList<StudySession>> ListByOwnerAsync(string ownerId, CancellationToken cancellationToken)` a `IStudySessionRepository`/`FirestoreStudySessionRepository`, retornando sessões de **qualquer status** (mesma consulta Firestore de `ListCompletedByOwnerAsync`, apenas sem o filtro `status == Completed`).
- Rationale: RF-04/RF-05/RF-06/RF-13 pedem agregação sobre "todas as `StudyAnswer` do usuário" — como uma resposta é gravada no momento do `SubmitAnswer` (antes mesmo da sessão ser concluída), restringir a busca a `ListCompletedByOwnerAsync` deixaria de fora respostas de sessões ainda em andamento ou já canceladas, que já são fatos permanentes registrados. Streak (`GetUserStreak`) continua usando exclusivamente `ListCompletedByOwnerAsync`, sem alteração — o novo método é usado apenas pelas três novas queries desta feature.
- Alternatives considered: Reaproveitar apenas `ListCompletedByOwnerAsync`; rejeitado porque subestimaria tempo total de estudo, cards concluídos e taxa de retenção ao ignorar respostas de sessões não concluídas. Criar um repositório novo dedicado a analytics; rejeitado por contrariar a instrução explícita de não criar repositório separado — a extensão do método existente é suficiente e mínima.

## 5. Fan-out em memória em vez de nova consulta composta no Firestore

- Decision: Nenhuma consulta composta nova é criada no Firestore. `GetDashboardQuery`, `GetDeckStatisticsQuery` e `GetStudyHistoryQuery` buscam sessões por `ownerId` (consulta já existente/estendida) e, em memória, iteram cada sessão chamando `ListBySessionIdAsync` (consulta já existente por `sessionId`) para montar a lista completa de respostas antes de agregar (somas, contagens, agrupamentos por dificuldade/subárea/tipo, série diária).
- Rationale: Segue à risca a instrução do usuário de "não criar novas queries compostas no Firestore além das já existentes por OwnerId/deckId"; mantém toda a lógica de agregação em Application, sem lógica de negócio na Infrastructure.
- Alternatives considered: Nova consulta composta Firestore por intervalo de datas diretamente na coleção `studyAnswers`; rejeitada porque exigiria o campo `ownerId` no documento de resposta (ver item 3) e uma nova query além do padrão atual. Aceita-se o custo de N chamadas (uma por sessão do usuário) como tradeoff documentado; é compatível com o volume esperado de um MVP acadêmico e pode ser revisitado caso o SC-005 (dashboard em menos de 2s) não seja atingido em uso real.

## 6. Correção de registro de DI ausente para `IFlashcardRepository`

- Decision: Adicionar `services.AddScoped<IFlashcardRepository, FirestoreFlashcardRepository>();` em `CogniLink.Infrastructure/DependencyInjection.cs` como parte da implementação desta feature.
- Rationale: `IFlashcardRepository` já é implementado por `FirestoreFlashcardRepository` desde a feature 003, mas nunca foi registrado no container de DI (confirmado em `DependencyInjection.cs` e `Program.cs` — nenhum dos dois registra a interface). Isso já afeta a feature 004 em produção (`GetDueFlashcardsQueryHandler` depende de `IFlashcardRepository` e falharia na resolução de DI em runtime) e bloquearia diretamente `GetDeckStatisticsQuery` desta feature, que também depende dele para listar flashcards do baralho. Corrigir esse registro é pré-requisito funcional, não uma nova peça de infraestrutura.
- Alternatives considered: Reportar como bug separado sem corrigir agora; rejeitado porque a feature 005 não funciona sem essa correção, e a correção é de uma linha, sem introduzir repositório novo — compatível com a restrição de "nenhum repositório novo".

## 7. Validação de período do histórico via FluentValidation

- Decision: `GetStudyHistoryQuery(DateOnly? From, DateOnly? To)` tem um `AbstractValidator` que exige `From`/`To` não nulos, `To >= From` e `(To - From) <= 365 dias`, seguindo o mesmo `ValidationBehavior<TRequest,TResponse>` de pipeline MediatR já usado pelas features anteriores (erro vira 400 via `ExceptionHandlingMiddleware` capturando `FluentValidation.ValidationException`).
- Rationale: Reaproveita integralmente o pipeline de validação já configurado; não introduz mecanismo de validação novo.
- Alternatives considered: Validar manualmente dentro do handler com `if`/`throw`; rejeitado por fugir do padrão consistente de validação via FluentValidation já usado em todos os Commands/Queries com regras de entrada.

## 8. Preenchimento de dias sem atividade na série de histórico

- Decision: `GetStudyHistoryQueryHandler` gera a série diária completa do período solicitado (um item por dia entre `From` e `To`, inclusive), preenchendo com zero os dias sem nenhuma `StudyAnswer`.
- Rationale: Já registrado como assumption no spec; garante série contínua para consumo futuro por gráficos no frontend, sem exigir que o frontend trate "buracos" de dias ausentes.
- Alternatives considered: Retornar apenas os dias com atividade; rejeitado por spec explicitamente assumir preenchimento contínuo.

## 9. Sem testes automatizados nesta etapa

- Decision: Não incluir tarefas de teste automatizado no plano; apenas roteiro de validação manual via Swagger (`quickstart.md`).
- Rationale: Restrição explícita de escopo do solicitante para esta etapa, consistente com a feature 004.
- Alternatives considered: Introduzir suite de testes unitários/integração agora; rejeitado por contrariar o pedido explícito.
