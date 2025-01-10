using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Consumer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Consumer.Services
{
    internal class TestConsumer:IDisposable
    {
        private IConfiguration _config;
        public TestConsumer(IConfiguration config)
        {
            _config = config;
        }
        public async void Run()
        {
            Console.WriteLine($"hash={Guid.NewGuid().ToString()}");
            var setting = _config.GetSection("RabbitMQ").Get<RabbitMQSetting>();
            if (
              setting != null
              && !string.IsNullOrEmpty(setting.UserName)
              && !string.IsNullOrEmpty(setting.Password)
              && !string.IsNullOrEmpty(setting.HostName)
              && !string.IsNullOrEmpty(setting.QueueName)
              && !string.IsNullOrEmpty(setting.ExchangeName)
              )
            {
                //取得目前執行緒ID
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
                {
                    //開啟 channel
                    using (var channel = await connection.CreateChannelAsync())
                    {
                        //宣告一個queue
                        await channel.QueueDeclareAsync(queueName, false, false, false, null);
                        //channel.QueueBind
                        await channel.QueueBindAsync(queueName, exchangeName, routeKey);
                        //設定consumer
                        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
                        //設定每次接收一筆資料
                        await channel.BasicQosAsync(0, 1, false);
                        //接收到消息事件 consumer.IsRunning
                        consumer.ReceivedAsync += async (ch, ea) =>
                        {
                            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                            Console.WriteLine($"Queue:{queueName}收到資料： {message}");
                            //自行回覆Ack，告知RabbitMQ已經處理完畢
                            await channel.BasicAckAsync(ea.DeliveryTag, false);
                        };
                        //開始接收(不自動回覆ACK)
                        await channel.BasicConsumeAsync(queueName, false, consumer);
                        Console.WriteLine($"Queue:{queueName}開始接收訊息");
                        while (Console.ReadKey().Key != ConsoleKey.Escape) ;
                    }
                }
            }
            else
            {
                Console.WriteLine("RabbitMQ連線資訊未設定");
            }
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing TestProducer");
        }
    }
}
