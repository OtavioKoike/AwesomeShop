using AwesomeShop.Services.Orders.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Infrastructure.Subscribers
{
    // BackgroundService => Rodando esperando
    public class PaymentAcceptedSubscriber : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string Queue = "order-service/payment-accepted";
        private const string Exchange = "order-service";
        private const string RoutingKey = "payment-accepted";

        public PaymentAcceptedSubscriber(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            // Nome da Conexao (Diferenciar no RabbitMQ)
            _connection = connectionFactory.CreateConnection("order-service-payment-accepted-subscriber");
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(Exchange, "topic", true); // Se a exchange nao existir, ele vai corrigir
            _channel.QueueDeclare(Queue, true, false, false, null); // Se a queue nao existir, ele vai corrigir

            // Ligar a queue, conectando com o exchange, usando a chave
            // Toda vez que tiver uma mensagem publicada nesse exchange com a chave de roteamento "payment-accepted"
            // Vou rotear ela para a fila interna do order-service
            _channel.QueueBind(Queue, "payment-service", RoutingKey);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            // Criar evento => Quando receber a mensagem nesse canal, vai processar a mensagem
            consumer.Received += async (sender, EventArgs) =>
            {
                var byteArray = EventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(byteArray);
                var message = JsonConvert.DeserializeObject<PaymentAccepted>(contentString);

                Console.WriteLine($"Message Payment Accepted received with Id {message.Id}");

                // Atualizar o Status do pedido na base
                var result = await UpdateOrder(message);

                if(result)
                    // Dizer que processei a mensagem com sucesso
                    _channel.BasicAck(EventArgs.DeliveryTag, false);
            };

            // Realmente consumir
            _channel.BasicConsume(Queue, false, consumer);

            return Task.CompletedTask;
        }

        private async Task<bool> UpdateOrder(PaymentAccepted paymentAccepted)
        {
            // Criar Scope para acessar o Repositorio
            using (var scope = _serviceProvider.CreateScope())
            {
                // Pegando do Container de Injecao de Dependencia
                var orderRepository = scope.ServiceProvider.GetService<IOrderRepository>();

                var order = await orderRepository.GetByIdAsync(paymentAccepted.Id);
                order.SetAsCompleted();

                await orderRepository.UpdateAsync(order);

                return true;
            }
        }
    }

    public class PaymentAccepted
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
