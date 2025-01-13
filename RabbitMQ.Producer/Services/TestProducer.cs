using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Producer.Class;
using RabbitMQ.Producer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Producer.Services
{
    internal class TestProducer:IDisposable
    {
        private IConfiguration _config;
        public TestProducer(IConfiguration config)
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
                #region 連接RabbitMQ Server
                //建立連接工廠
                ConnectionFactory factory = new ConnectionFactory
                {
                    UserName = setting.UserName,
                    Password = setting.Password,
                    HostName = setting.HostName
                };

                using (RabbitMQPersistentConnection persistentConnection = new RabbitMQPersistentConnection(factory))
                {
                    TestExchange service = new TestExchange(persistentConnection, setting.ExchangeName, setting.QueueName);

                    Console.WriteLine("\nRabbitMQ連接成功,如需離開請按下Escape鍵");
                    string input = string.Empty;
                    do
                    {
                        Console.WriteLine("輸入訊息：");
                        input = Console.ReadLine();
                        var sendBytes = Encoding.UTF8.GetBytes(input);
                        //發布訊息到RabbitMQ Server
                        await service.PublishAsync($"{setting.QueueName}測試：{input}!");
                        //因使用ReadLine，所以要多一次Enter避免漏字
                        Console.WriteLine("點選Enter再繼續發出訊息");
                    } while (Console.ReadKey().Key != ConsoleKey.Escape);
                }

                #endregion
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
