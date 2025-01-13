using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Producer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Polly.Retry;
using Polly;

namespace RabbitMQ.Producer.Class
{

    /// <summary>
    /// 持久性連接
    /// </summary>
    public class RabbitMQPersistentConnection
    : IRabbitMQPersistentConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly int _retryCount;
        IConnection _connection;
        bool _disposed;

        //object sync_root = new object();

        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            //_logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _retryCount = retryCount;
        }

        /// <summary>
        /// 是否連接
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _connection != null && _connection.IsOpen && !_disposed;
            }
        }

        /// <summary>
        /// 建立Model
        /// </summary>
        /// <returns></returns>
        public async Task<IChannel> CreateChannelAsync()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return await _connection.CreateChannelAsync();
        }

        /// <summary>
        /// 釋放
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.ToString());
                //_logger.LogCritical(ex.ToString());
            }
        }

        /// <summary>
        /// 連接
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TryConnectAsync()
        {
            Console.WriteLine("RabbitMQ Client is trying to connect");
            //_logger.LogInformation("RabbitMQ Client is trying to connect");

            //lock (sync_root)
            //{
                var policy = RetryPolicy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetryAsync(_retryCount,
                        retryAttempt =>
                            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                            {
                                Console.WriteLine(ex.ToString());
                                Console.WriteLine("RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                                //_logger.LogWarning(ex, "RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                            }
                );

               await policy.ExecuteAsync(async () =>
                {
                    _connection = await _connectionFactory
                          .CreateConnectionAsync();
                });
            //_connection = await _connectionFactory
            //            .CreateConnectionAsync();

            if (IsConnected)
                {
                    _connection.ConnectionShutdownAsync += OnConnectionShutdown;
                    _connection.CallbackExceptionAsync += OnCallbackException;
                    _connection.ConnectionBlockedAsync += OnConnectionBlocked;

                    Console.WriteLine($"RabbitMQ Client acquired a persistent connection to '{_connection.Endpoint.HostName}' and is subscribed to failure events");
                    //_logger.LogInformation("RabbitMQ Client acquired a persistent connection to '{HostName}' and is subscribed to failure events", _connection.Endpoint.HostName);

                    return true;
                }
                else
                {
                    Console.WriteLine("FATAL ERROR: RabbitMQ connections could not be created and opened");
                    //_logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");

                    return false;
                }
            }

        /// <summary>
        /// 連接被阻擋
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async Task OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;

            Console.WriteLine("A RabbitMQ connection is shutdown. Trying to re-connect...");
            //_logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

            await TryConnectAsync();
        }

        /// <summary>
        /// 連接出現例外
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async Task OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;

            Console.WriteLine("A RabbitMQ connection throw exception. Trying to re-connect...");
            //_logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

            await TryConnectAsync();
        }

        /// <summary>
        /// 連接被關閉
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reason"></param>
        async  Task OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;

            Console.WriteLine("A RabbitMQ connection is on shutdown. Trying to re-connect...");
            //_logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

            await TryConnectAsync();
        }

    }
}
