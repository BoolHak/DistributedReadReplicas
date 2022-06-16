using RabbitMQ.Client;

namespace Commun
{
    public class Constants
    {
        public const string RabbitMQConnection = "";
        public const string DbConnectionString = "";
        
        public const string RBS_TOKEN_NAME = "X-FY-Token";
        public const string RBS_TOKEN_VALUE = "clOHDmJ2D4MtJ2MhVQE0";
        public const string EventBrodcasterIp = "http://127.0.0.1:44859";

        

        public const string ExchangeName = "ModelsExchange";
        public const string QueueNameUser = "Models.User";
        public const string KeyUser = "Models.User.Changes";

        public static IConnection GetConnection()
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(RabbitMQConnection);
            factory.AutomaticRecoveryEnabled = true;
            factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
            factory.RequestedHeartbeat = TimeSpan.FromSeconds(30);
            return factory.CreateConnection();
        }
    }
}
