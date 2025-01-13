using RabbitMQ.Client;

namespace RabbitMQ.Producer.Interface
{
    public interface IRabbitMQPersistentConnection
    : IDisposable
    {
        bool IsConnected { get; }

        Task<bool> TryConnectAsync();

        Task<IChannel> CreateChannelAsync();
    }
}
