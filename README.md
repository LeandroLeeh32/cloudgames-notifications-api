# CloudGames Notifications Serverless Functions

O **CloudGames Notifications Serverless Functions** é responsável pelo processamento de notificações da plataforma **CloudGames** utilizando arquitetura **Serverless** baseada em **Azure Functions**.
A solução foi refatorada a partir de uma arquitetura tradicional baseada em consumers contínuos para um modelo orientado a eventos utilizando **Functions acionadas sob demanda** através de mensagens recebidas no **RabbitMQ**.

# Arquitetura Serverless

A arquitetura utiliza:

- Azure Functions
- RabbitMQ Trigger
- processamento sob demanda
- execução orientada a eventos

As Functions são executadas apenas quando uma nova mensagem é recebida no broker.

Fluxo atual:

UsersAPI / PaymentAPI
│
│ publica evento
▼
RabbitMQ
│
│ mensagem recebida
▼
Azure Functions
│
▼
UseCases
│
▼
EmailService


Essa abordagem reduz consumo de recursos e melhora a escalabilidade da solução.

---

# Arquitetura do Projeto

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

Os eventos de integração estão definidos dentro da camada:

CloudGames.Notifications.Application

Estrutura:

CloudGames.Notifications.Application
│
└── IntegrationEvents
│
├── Users
│ └── UserCreatedIntegrationEvent.cs
│
└── Purchases
└── PaymentProcessedEvent.cs

---

# UserCreatedIntegrationEvent

Evento publicado quando um usuário é criado.

Exemplo de uso:

- envio de e-mail de boas-vindas

---

# PaymentProcessedEvent

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

---

# Fluxo Esperado da Mensageria

Publisher
↓
Exchange
↓
Binding
↓
Queue
↓
Azure Function Trigger

---

# Convenções de Exchanges do MassTransit

O MassTransit publica eventos utilizando convenções internas baseadas nos tipos dos contratos de integração.

Exemplo de exchange criada automaticamente:

FIAP.Messages:PaymentProcessedEvent

Durante a integração foi necessário alinhar os bindings do RabbitMQ para consumir corretamente os eventos publicados pelo MassTransit.

---

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

# Executando o Ambiente Local (Simulador do ambiente de infra - serveless)

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

## 1. Subir RabbitMQ

Execute o comando abaixo na raiz do projeto Notification:

docker compose down

Caso seja necessário limpar completamente o ambiente:

docker volume prune -f

SUBINDO O AMBIENTE RABBIT

docker compose up -d

Esse comando irá:

- iniciar o container RabbitMQ
- disponibilizar o broker localmente
- abrir as portas necessárias para comunicação
- disponibilizar o painel administrativo do RabbitMQ
- provisionar exchanges, queues e bindings automaticamente

Portas utilizadas:

- 5672 → comunicação AMQP
- 15672 → painel administrativo

Painel administrativo:

http://localhost:15672

Credenciais padrão:

usuario: guest
senha: guest

---

## 2. Executar Azurite

libere a porta 10000

Executar em outro terminal:

Get-NetTCPConnection -LocalPort 10000 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }

Executar em outro terminal: 

azurite

Azurite é utilizado como emulador local do Azure Storage necessário para execução do runtime das Azure Functions.

---

## 3. Executar Azure Functions

Abra um terminal dentro da pasta:

CloudGames.Notifications.Functions 

Execute:

func start

Esse comando irá:

- iniciar o runtime local das Azure Functions
- carregar os RabbitMQ Triggers
- ativar as Functions serverless
- começar a escutar mensagens recebidas no RabbitMQ

Functions carregadas:

PaymentProcessedFunction
UserCreatedFunction

---

## 4. Publicar Eventos de Teste

Os eventos podem ser publicados através dos executáveis disponibilizados na pasta:

teste-local

Exemplo de caminho utilizado no ambiente local:

\Notification\teste-local

Os executáveis presentes na pasta teste-local permitem publicar eventos de teste sem necessidade de Visual Studio ou SDK .NET instalado.
Os executáveis foram gerados como aplicações standalone utilizando .NET Publish Single File, permitindo execução sem necessidade de instalação do SDK .NET ou Visual Studio.

Exemplos:

- CloudGames.TestPublisher-payment.exe
- CloudGames.TestPublisher-user.exe

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

---

# Execução em Ambiente Orquestrado

A solução também pode ser executada em ambiente orquestrado utilizando:

- Kubernetes
- Docker Compose
- Docker Desktop Kubernetes

Nesse cenário:

- RabbitMQ sobe como serviço/container da infraestrutura
- Azure Functions executa containerizada
- os serviços se comunicam através da rede interna do orquestrador

Exemplo de hostname utilizado no ambiente orquestrado:

rabbitmq

---

# Fluxo Completo da Arquitetura


UsersAPI / CatalogAPI
        ↓
RabbitMQ
        ↓
Azure Functions Runtime
        ↓
RabbitMQ Trigger
        ↓
UseCases
        ↓
EmailService


As Functions são executadas somente quando novas mensagens chegam ao broker, caracterizando uma arquitetura orientada a eventos utilizando modelo serverless.

---

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