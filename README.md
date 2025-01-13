# RabbitMQ �d��

## ����
- .Net Core 6.0
- RabbitMQ Client 7.0.0

## �w�� RabbitMQ
- [RabbitMQ �x��](https://www.rabbitmq.com/download.html)
- [RabbitMQ �w�˱о�](https://www.rabbitmq.com/docs/download)
- [RabbitMQ �޲z����](http://localhost:15672/)

## �w�� RabbitMQ Client
- [NuGet](https://www.nuget.org/packages/RabbitMQ.Client/)

 `Install-Package RabbitMQ.Client -Version 7.0.0`

## ����
![Rabbitmq Architecture](assets/images/rabbitmq_architecture.png)

- **Producer** �o�e�T���� **Exchange**
- **Exchange** �|�ھ� **Routing Key** �N�T����o�� **Queue**
- **Consumer** �q **Queue** ���o�T��

## �d��
- [Producer](RabbitMQ.Producer)
- [Consumer](RabbitMQ.Consumer)
