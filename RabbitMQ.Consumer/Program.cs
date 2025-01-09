using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Consumer.Models;
using System.Text;

namespace RabbitMQ.Consumer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //讀取appsettings.json
            var config = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile($"appsettings.json", true, true)
                  .Build();
            //讀取RabbitMQ連線資訊
            var setting = config.GetSection("RabbitMQ").Get<RabbitMQSetting>();


            if (
                setting != null
                && !string.IsNullOrEmpty(setting.UserName)
                && !string.IsNullOrEmpty(setting.Password)
                && !string.IsNullOrEmpty(setting.HostName)
                && !string.IsNullOrEmpty(setting.QueueName)
                && !string.IsNullOrEmpty(setting.ExchangeName)
                )
            {
                //開兩個執行緒處理接收
                for (int i = 1; i <= 2; i++)
                {
                    await Task.Run(() => Run(setting));
                }

                Console.WriteLine("如需離開請按下Escape鍵");
                do
                {
                    Console.ReadKey();
                } while (Console.ReadKey().Key != ConsoleKey.Escape);
            }
            else
            {
                Console.WriteLine("RabbitMQ連線資訊未設定");
            }
        }
        static async void Run(RabbitMQSetting setting)
        {
            int taskID = Task.CurrentId ?? 0;
            string queueName = setting.QueueName + taskID;
            string exchangeName = setting.ExchangeName;
            string routeKey = string.Empty;

            //初始化連線資訊
            ConnectionFactory factory = new ConnectionFactory();
            //設定 RabbitMQ 位置
            factory.HostName = setting.HostName;
            //設定連線 RabbitMQ username
            factory.UserName = setting.UserName;
            //設定 RabbitMQ password
            factory.Password = setting.Password;

            //開啟連線
            using (var connection = await factory.CreateConnectionAsync())
            //開啟 channel
            using (var channel = await connection.CreateChannelAsync())
            {
                //channel.QueueBind
                await channel.QueueDeclareAsync(queueName, false, false, false, null);

                await channel.QueueBindAsync(queueName, exchangeName, routeKey);

                AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
                await channel.BasicQosAsync(0, 1, false);
                //接收到消息事件 consumer.IsRunning
                consumer.ReceivedAsync += async (ch, ea) =>
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                    Console.WriteLine($"Queue:{queueName}收到資料： {message}");
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                };

                await channel.BasicConsumeAsync(queueName, false, consumer);
                Console.WriteLine($"Queue:{queueName}開始接收訊息");
                do
                {
                    Console.ReadKey();
                } while (Console.ReadKey().Key != ConsoleKey.Escape);
            }

        }
    }
}