using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Consumer.Models
{
    public class RabbitMQSetting
    {
        /// <summary>
        /// 帳號
        /// </summary>
        public string? UserName { get; set; }
        /// <summary>
        /// 密碼
        /// </summary>
        public string? Password { get; set; }
        /// <summary>
        /// 主機名稱
        /// </summary>
        public string? HostName { get; set; }
        /// <summary>
        /// Queue名稱
        /// </summary>
        public string? QueueName { get; set; }
        /// <summary>
        /// Exchange名稱
        /// </summary>
        public string? ExchangeName { get; set; }
    }
}
