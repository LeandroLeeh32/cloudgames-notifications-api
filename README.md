# CloudGames Notifications Serverless Functions

O **CloudGames Notifications Serverless Functions** é responsável pelo processamento de notificações da plataforma **CloudGames** utilizando arquitetura **Serverless** baseada em **Azure Functions**.
A solução foi refatorada a partir de uma arquitetura tradicional baseada em consumers contínuos para um modelo orientado a eventos utilizando **Functions acionadas sob demanda** através de mensagens recebidas no **RabbitMQ**.

# Infraestrutura como Código (IaC)

Toda a infraestrutura necessária para execução local e orquestrada da solução serverless encontra-se versionada neste repositório.

A infraestrutura é provisionada através de:

- docker-compose.yml
- definitions.json
- manifests Kubernetes (k8s)
- configurações das Azure Functions

Esses arquivos são responsáveis por provisionar:

- RabbitMQ
- exchanges
- queues
- bindings
- Azure Functions
- rede entre serviços
- containers necessários

A solução utiliza abordagem Infrastructure as Code (IaC) equivalente utilizando Docker Compose + Kubernetes manifests.
Todo o código das Azure Functions e toda a infraestrutura necessária para execução da solução encontram-se centralizados neste repositório, atendendo ao requisito de separação da arquitetura serverless.

# Arquitetura do Projeto

CloudGames.Notifications
├── CloudGames.Notifications.Functions
├── CloudGames.Notifications.Application
├── CloudGames.Notifications.Domain
├── CloudGames.Notifications.Infrastructure
├── docker-compose.yml
├── definitions.json
├── k8s
└── README.md

O projeto segue princípios de **Clean Architecture**, separando responsabilidades em diferentes camadas.

CloudGames.Notifications
│
├── CloudGames.Notifications.Functions
│ Camada responsável pelas Azure Functions e RabbitMQ Triggers
│
├── CloudGames.Notifications.Application
│ Camada responsável pelos casos de uso
│
├── CloudGames.Notifications.Infrastructure
│ Serviços externos e infraestrutura
│
├── CloudGames.Notifications.Domain
│ Regras de negócio


Essa separação permite:

- baixo acoplamento
- melhor manutenção
- maior testabilidade
- reutilização de regras de negócio
- evolução independente das camadas

---

# Arquitetura Orientada a Eventos (EDA)

A comunicação entre os microsserviços é feita através de eventos publicados no RabbitMQ.
Os serviços não se comunicam diretamente via HTTP para notificações.
Fluxo simplificado:

UsersAPI
│
│ publica evento
▼
RabbitMQ
│
▼
UserCreatedFunction
SendWelcomeEmailUseCase


---

# Azure Functions

A solução utiliza Azure Functions no modelo:

- .NET 8
- Isolated Worker
- RabbitMQ Trigger

Functions implementadas:

- UserCreatedFunction
- PaymentProcessedFunction

---

# RabbitMQ Trigger

As Functions são acionadas automaticamente quando novas mensagens chegam ao RabbitMQ.
Exemplo:

[Function("UserCreatedFunction")]
public async Task Run([RabbitMQTrigger("UserCreated",ConnectionStringSetting = "RabbitMQConnection")] string message)

---

# Eventos de Integração
UserCreatedIntegrationEvent

Evento publicado quando um usuário é criado.

Exemplo de uso:

- envio de e-mail de boas-vindas

---

PaymentProcessedEvent

Evento publicado quando uma compra é processada.

Exemplo de uso:

- envio de confirmação de compra

---

# Arquitetura de Mensageria

A arquitetura utiliza RabbitMQ como broker de mensageria para comunicação assíncrona entre os microsserviços.
O publisher utiliza **MassTransit** para publicação de eventos, enquanto a NotificationsAPI foi migrada para **Azure Functions utilizando RabbitMQ Trigger**.
o RabbitMQ Trigger não realiza criação automática de:

- exchanges
- queues
- bindings

Por esse motivo, o ambiente foi configurado para provisionar previamente os recursos necessários através de definitions.json

# Provisionamento das Filas e Exchanges

O provisionamento do RabbitMQ foi configurado através de:

definitions.json

Esse arquivo é carregado automaticamente durante o startup do container RabbitMQ via Docker Compose.

Com isso, o ambiente sobe automaticamente contendo:

- exchanges
- queues
- bindings
- permissões
- usuários

Sem necessidade de configuração manual.

---

## Ambiente Local 

Para simplificação de testes locais:

- RabbitMQ é executado via Docker Compose
- Azure Functions roda localmente
- Azurite é utilizado como emulador local do Azure Storage
- filas e bindings são provisionados automaticamente via definitions.json

---

# Retry e Resiliência

A solução possui tratamento de falhas utilizando:

- try/catch
- logging estruturado
- retry indireto através do RabbitMQ

Quando ocorre uma falha no processamento:

- a exceção é registrada
- a mensagem pode ser reenfileirada
- o processamento pode ser executado novamente

Exemplo:


try
{
    await _useCase.ExecuteAsync(...);
}
catch(Exception ex)
{
    _logger.LogError(ex, "Erro ao processar evento");

    throw;
}


---

# Execução Local da Arquitetura Serverless

A arquitetura serverless pode ser executada localmente utilizando RabbitMQ + Azure Functions.

O ambiente é iniciado em etapas.

---

## Ambiente necessário para rodar local a infra

1. Node.js / versão LTS
2. Instalar o Azurite
3. Azure Functions Core Tools
4. Docker Desktop (para RabbitMQ)
5. .NET 8 SDK

---

## 1. Subir RabbitMQ /  azurite / Azure Functions

Esse docker-compose.yml sobe toda a infraestrutura local da sua solução serverless baseada em Docker Compose, RabbitMQ, Azurite e Azure Functions.

SUBINDO O AMBIENTE RABBIT / azurite / Azure Functions

docker compose up -d

Em resumo, esse docker-compose sobe todo o ambiente da sua arquitetura localmente:

RabbitMQ → recebe e distribui eventos/mensagens.
Azurite → simula serviços do Azure Storage localmente.
Azure Functions → executa sua Function que consome os eventos.

Painel administrativo RabbitMQ: http://localhost:15672

Credenciais padrão: usuario: guest / senha: guest

---

## 2. Executar func

Abra um terminal dentro da pasta: CloudGames.Notifications.Functions 

Execute: func start

Esse comando irá:

- iniciar o runtime local das Azure Functions
- carregar os RabbitMQ Triggers
- ativar as Functions serverless
- começar a escutar mensagens recebidas no RabbitMQ

Functions carregadas:

PaymentProcessedFunction
UserCreatedFunction

---

## 3. Publicar Eventos de Teste

Os eventos podem ser publicados através dos executáveis disponibilizados na pasta:

teste-local

Exemplo de caminho utilizado no ambiente local:

\Notification\teste-local

Os executáveis presentes na pasta teste-local permitem publicar eventos de teste sem necessidade de Visual Studio ou SDK .NET instalado.
Os executáveis foram gerados como aplicações standalone utilizando .NET Publish Single File, permitindo execução sem necessidade de instalação do SDK .NET ou Visual Studio.

Exemplos:

- CloudGames.TestPublisher-payment.exe
- CloudGames.TestPublisher-users.exe

Basta executar o arquivo `.exe` correspondente ao evento desejado.

Ao executar:

- o evento será publicado no RabbitMQ
- a exchange será acionada
- o binding encaminhará a mensagem para a fila
- a Azure Function Trigger será executada automaticamente


---

## 6. Derrubar containers

docker compose down

Caso seja necessário limpar completamente o ambiente:

docker volume prune -f


# Tecnologias Utilizadas

- .NET 8
- Azure Functions
- RabbitMQ
- Docker
- Kubernetes
- MassTransit
- Azurite
- Clean Architecture
- Event Driven Architecture
- Azure Functions Isolated Worker

---

# Autor

Leandro Oliveira e Luciano Miranda

FIAP – Pós-Graduação em Arquitetura .NET