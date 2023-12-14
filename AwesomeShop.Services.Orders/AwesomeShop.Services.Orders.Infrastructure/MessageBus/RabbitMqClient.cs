using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using System.Text;

namespace AwesomeShop.Services.Orders.Infrastructure.MessageBus
{
    public class RabbitMqClient : IMessageBusClient
    {
        private readonly IConnection _connection;
        public RabbitMqClient(ProducerConnection producerConnection)
        {
            _connection = producerConnection.Connection;
        }

        public void Publish(object message, string routingKey, string exchange)
        {
            // Antes de tudo é preciso criar um canal
            var channel = _connection.CreateModel();

            // Nao acontecer problema com objetos nulos
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var payload = JsonConvert.SerializeObject(message, settings);
            var body = Encoding.UTF8.GetBytes(payload);

            // Se a exchange nao existir, ele vai corrigir
            channel.ExchangeDeclare(exchange, "topic", true);
            // Publicar
            channel.BasicPublish(exchange, routingKey, null, body);
        }
    }
}
