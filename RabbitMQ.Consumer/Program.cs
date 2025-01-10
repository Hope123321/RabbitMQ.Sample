using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Consumer.Services;

namespace RabbitMQ.Consumer
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
            serviceCollection.AddTransient<TestConsumer>();
            // 建立依賴服務提供者
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // 執行主服務
            TestConsumer services = serviceProvider.GetRequiredService<TestConsumer>();

            for (int i=1;i<=2;i++) {
                await Task.Run(() => services.Run());
            }
            Console.WriteLine("如需離開請按下Escape鍵");
            while (Console.ReadKey().Key != ConsoleKey.Escape) ;
        }
    }
}