using RabbitMQ.Client;
using RabbitMQ.Producer.Class;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Producer.Models;

namespace RabbitMQ.Producer
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
                ) {
                #region 連接RabbitMQ Server
                //建立連接工廠
                ConnectionFactory factory = new ConnectionFactory
                {
                    UserName = setting.UserName,
                    Password = setting.Password,
                    HostName = setting.HostName
                };
                
                using (RabbitMQPersistentConnection persistentConnection = new RabbitMQPersistentConnection(factory)) {
                    TestExchangeByDirect service = new TestExchangeByDirect(persistentConnection, setting.ExchangeName,setting.QueueName);

                    Console.WriteLine("\nRabbitMQ連接成功,如需離開請按下Escape鍵");
                    string input = string.Empty;
                    do
                    {
                        input = Console.ReadLine();
                        var sendBytes = Encoding.UTF8.GetBytes(input);
                        //發布訊息到RabbitMQ Server
                        await service.PublishAsync($"{setting.QueueName}測試：{input}!");

                    } while (Console.ReadKey().Key != ConsoleKey.Escape);
                }

                #endregion
            }
            else
            {
                Console.WriteLine("RabbitMQ連線資訊未設定");
            }
        }
    }
}