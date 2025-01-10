using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Producer.Interface;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;

namespace RabbitMQ.Producer.Class
{
    internal class TestExchange 
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        string _queueName = string.Empty;
        string _exchangeName = string.Empty;
        string _routeKey = string.Empty;
        int _retryCount;

        public TestExchange(IRabbitMQPersistentConnection persistentConnection, string exchangeName, string queueName, int retryCount = 5)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _exchangeName = exchangeName;
            _queueName = queueName;
            _retryCount = retryCount;
        }

        public async Task PublishAsync(string message) {
            if (!_persistentConnection.IsConnected)
            {
                await _persistentConnection.TryConnectAsync();
            }

            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Could not publish  after {Timeout}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                //_logger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id, $"{time.TotalSeconds:n1}", ex.Message);
            });

            using (var channel = await _persistentConnection.CreateChannelAsync())
            {
                //建立Queue
                //await channel.QueueDeclareAsync(_queueName, false, false, false, null);
                //建立Exchange
                await channel.ExchangeDeclareAsync(exchange: _exchangeName,ExchangeType.Fanout);
                //把Queue跟Exchange Bind一起
                //await channel.QueueBindAsync(_queueName, _exchangeName, _routeKey);

                //var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                await policy.ExecuteAsync(async () =>
                {
                    //var properties = channel.CreateBasicProperties();
                    //properties.DeliveryMode = 2; // persistent

                    //_logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

                    await channel.BasicPublishAsync(
                        exchange: _exchangeName,
                        routingKey: _routeKey,
                        mandatory: true,
                        //basicProperties: properties,
                        body: body);
                });
            }
        }


        /// <summary>
        /// 舊Code備份
        /// </summary>
        private async void DoWork() {
            string queueName = "test";
            string exchangeName = "test";
            string routeKey = string.Empty;
            //建立連接工廠
            ConnectionFactory factory = new ConnectionFactory
            {
                UserName = "guest",
                Password = "guest",
                HostName = "rxnas1",
                Port = 5672
            };

            using (var connection = await factory.CreateConnectionAsync())
            using (var channel = await connection.CreateChannelAsync())
            {
                #region 如果在RabbitMq手動建立可以忽略這段程式
                //建立一個Queue
                await channel.QueueDeclareAsync(queueName, false, false, false, null);
                //建立一個Exchange
                await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, false, false, null);
                //把Queue跟Exchange
                await channel.QueueBindAsync(queueName, exchangeName, routeKey);
                #endregion

                IReadOnlyBasicProperties props = null;

                Console.WriteLine("\nRabbitMQ連接成功,如需離開請按下Escape鍵");

                string input = string.Empty;
                do
                {
                    input = Console.ReadLine();
                    var sendBytes = Encoding.UTF8.GetBytes(input);
                    //發布訊息到RabbitMQ Server
                    await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routeKey, body: sendBytes);

                } while (Console.ReadKey().Key != ConsoleKey.Escape);
            }
        }
    }
}
