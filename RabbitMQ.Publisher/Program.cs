using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Producer.Services;

namespace RabbitMQ.Producer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //讀取appsettings.json
            var configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile($"appsettings.json", true, true)
                  .Build();

            // 建立依賴注入的容器
            var serviceCollection = new ServiceCollection();
            // 註冊配置
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            // 註冊服務
            serviceCollection.AddTransient<TestProducer>();
            // 建立依賴服務提供者
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                // 執行主服務
                serviceProvider.GetRequiredService<TestProducer>().Run();
                //Console.ReadLine();
                while (Console.ReadKey().Key != ConsoleKey.Escape);
            }
        }
    }
}