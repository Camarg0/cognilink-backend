Regras de negócio:

O CogniLink é uma plataforma de aprendizagem baseada em três técnicas 
cientificamente validadas: repetição espaçada (algoritmo SM-2), recordação 
ativa (testagem sem consulta) e método Feynman (explicação simplificada 
guiada por IA). O produto atende estudantes universitários que usam 
flashcards para estudo de longo prazo.

Regras de negócio centrais que todo spec deve respeitar:
- O agendamento de revisão (nextReview, intervalDays, easeFactor) é 
  sempre calculado no servidor; o cliente nunca decide quando um card 
  deve ser revisado.
- Um usuário só acessa e modifica seus próprios dados (decks, flashcards, 
  progresso, conversas com IA) — isolamento por proprietário é regra de 
  domínio, não apenas política de autorização.
- Existem 4 tipos de flashcard (FrenteVerso, Cloze, DigiteResposta, 
  MultiplaEscolha), cada um com regras de validação próprias definidas 
  no Domain.
- O MVP prioriza: autenticação → decks → flashcards → sessões de 
  estudo/SM-2 → dashboard. Recursos de IA, upload de documentos e 
  localização são extensões pós-MVP.

Regras técnicas:
A linguagem que será desenvolvido o projeto será .NET 10/ASP.NET Core.
Para a estruturação da arquitetura do projeto, utilizar Clean Architecture, com as camadas: 
- Presentation (Api) -> fazendo uso sempre de JWT + refresh token, protocolos HTTPS para desenvolvimento das APIs
- Application (Use cases) -> Fazer uso de CQRS (Commands/Queries) with MediatR, que não dependa da layer de infrastructure
- Domain -> Contendo entities, value objects, domain events, de forma que sejam classes puras, sem dependência de outras libs, e que contenha as regras de negócio.
- Infrastructure -> Repositórios customizados sobre o SDK oficial do Firebase Admin (Google.Cloud.Firestore) implementando as interfaces definidas em Application. Sem EF Core, sem migrations relacionais. Integrações com IA (Google AI Studio) também residem aqui. Não deve conter regras de negócio.
Para logs, utilizar a biblioteca NLog. 
Para mapeamento de entidades para DTOs, utilizar Mapster. 
Para validações, utilizar a FluentValidation.
Para a documentação da API é para utilizar sempre o  Swagger.
A IA utilizada para integração nos pontos que necessitam é o Google AI Studio.
O banco de dados será inteiramente não relacional, e irei utilizar o Firebase Firestore.
Não deve haver exposição de chaves de IA ao frontend, CORS explícito, sem features especulativas fora do que tenho definido no escopo de projeto.