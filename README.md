# CloudGames Notifications Service

O **CloudGames Notifications Service** é um microserviço responsável pelo processamento de notificações dentro da plataforma **CloudGames**.

Este serviço consome eventos publicados por outros microserviços e executa ações relacionadas a notificações, como envio de e-mails ou outras comunicações ao usuário.

A comunicação entre os serviços segue o padrão **Event Driven Architecture (EDA)** utilizando **RabbitMQ** como broker de mensagens e **MassTransit** como biblioteca de mensageria em .NET.

---

# Arquitetura

O projeto segue princípios de **Clean Architecture**, separando responsabilidades em diferentes camadas.


CloudGames.Notifications
│
├── CloudGames.Notifications.API
│ Camada responsável pela configuração da aplicação,
│ mensageria e consumers de eventos
│
├── CloudGames.Notifications.Application
│ Camada responsável pelos casos de uso da aplicação
│
└── CloudGames.Contracts
Contratos compartilhados contendo os eventos de integração


Essa separação permite:

- baixo acoplamento entre camadas
- maior facilidade de manutenção
- melhor testabilidade
- evolução independente dos serviços

---

# Arquitetura Orientada a Eventos (EDA)

A comunicação entre microserviços é feita de forma **assíncrona através de eventos**.

Em vez de um serviço chamar diretamente outro serviço via API, ele publica um evento informando que algo aconteceu.

Fluxo simplificado:


UsersAPI
│
│ Publica evento
▼
RabbitMQ
│
│ Mensagem enviada para fila
▼
NotificationsAPI
│
▼
Processamento da notificação


Exemplo de cenário:

1. O **UsersAPI** cria um novo usuário
2. Publica o evento `UserCreatedIntegrationEvent`
3. O **NotificationsAPI** consome o evento
4. O sistema envia um e-mail de boas-vindas

---

# Conceitos de Mensageria Utilizados

Este microserviço utiliza conceitos importantes para garantir **resiliência, escalabilidade e baixo acoplamento entre serviços**.

---

# Integration Events

## O que é

Um **Integration Event** é um evento publicado por um microserviço para informar outros microserviços que algo aconteceu no sistema.

Esses eventos permitem que os serviços se comuniquem **sem dependência direta entre APIs**.

## Exemplo

Quando um usuário é criado no **UsersAPI**, um evento é publicado:


UserCreatedIntegrationEvent


Fluxo:


UsersAPI
│
│ publica evento
▼
RabbitMQ
│
▼
NotificationsAPI


O **NotificationsAPI** consome o evento e executa ações como envio de notificações.

---

# Retry Policy

## O que é

**Retry Policy** significa tentar novamente quando ocorre uma falha no processamento de uma mensagem.

Falhas podem ocorrer por diversos motivos, como:

- falhas temporárias de rede
- serviço externo indisponível
- timeout de comunicação

Para evitar perda de mensagens, o sistema tenta novamente antes de considerar a falha definitiva.

## Exemplo de configuração

O projeto utiliza **MassTransit Retry Policy**.


RetryCount: 3
RetryIntervalSeconds: 5


Significado:

- se o processamento da mensagem falhar
- o sistema tentará novamente **3 vezes**
- aguardando **5 segundos entre cada tentativa**

---

# Dead Letter Queue (DLQ)

## O que é

Quando uma mensagem falha várias vezes e não pode ser processada com sucesso, ela é enviada para uma fila especial chamada:

**Dead Letter Queue (DLQ)**

Também conhecida como:


fila de mensagens mortas


## Para que serve

A DLQ permite:

- armazenar mensagens que falharam
- evitar perda de dados
- possibilitar análise posterior do erro

## Fluxo


Mensagem enviada
│
▼
Consumer tenta processar
│
▼
Falha
│
▼
Retry Policy executa tentativas
│
▼
Falha após várias tentativas
│
▼
Mensagem enviada para Dead Letter Queue


---

# Eventos de Integração

Os eventos são definidos no projeto **CloudGames.Contracts**.

Estrutura:


CloudGames.Contracts
│
└── IntegrationEvents
│
├── BaseEvent.cs
│
├── Users
│ └── UserCreatedIntegrationEvent.cs
│
└── Purchases
└── PurchaseCreatedIntegrationEvent.cs


### UserCreatedIntegrationEvent

Evento publicado quando um novo usuário é criado.

Exemplo de uso:

- envio de e-mail de boas-vindas
- início do fluxo de onboarding

---

### PurchaseCreatedIntegrationEvent

Evento publicado quando uma compra é realizada.

Exemplo de uso:

- envio de confirmação de compra
- notificação para sistemas de pagamento ou faturamento

---

# Logging

O projeto utiliza **NLog** para registro de logs da aplicação.

Os logs incluem:

- consumo de mensagens
- execução de casos de uso
- falhas no processamento
- envio de notificações

Exemplo de log:


Sending welcome email to user@example.com

Email successfully sent
Message processing failed


---

# Executando RabbitMQ Localmente

Para rodar RabbitMQ localmente utilizando Docker:


docker run -d
--hostname rabbit
--name rabbitmq
-p 5672:5672
-p 15672:15672
rabbitmq:3-management


Painel de administração:


http://localhost:15672


Credenciais padrão:


usuario: guest
senha: guest


---

# Tecnologias Utilizadas

Este projeto utiliza as seguintes tecnologias:

- .NET 8
- MassTransit
- RabbitMQ
- NLog
- Clean Architecture
- Event Driven Architecture

---

# Plataforma CloudGames

A plataforma CloudGames é composta por múltiplos microserviços:


UsersAPI
CatalogAPI
PaymentsAPI
NotificationsAPI


Cada serviço possui responsabilidades específicas e se comunica através de **eventos de integração**.

---

# Autor

Leandro Oliveira  

FIAP – Pós-Graduação em Arquitetura .NET