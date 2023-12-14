using AwesomeShop.Services.Orders.Core.Repositories;
using AwesomeShop.Services.Orders.Infrastructure.MessageBus;
using AwesomeShop.Services.Orders.Infrastructure.Persistence;
using AwesomeShop.Services.Orders.Infrastructure.Persistence.Repositories;
using AwesomeShop.Services.Orders.Infrastructure.Subscribers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using RabbitMQ.Client;

namespace AwesomeShop.Services.Orders.Api.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services)
        {
            services.AddSingleton(s =>
            {
                var configuration = s.GetService<IConfiguration>(); // Adicionando os dados do AppSettings
                var options = new MongoDbOptions(); // Criando uma instancia do MongoDb

                // Pegar os dados do Mongo no AppSettings e aplicar no objeto vazio, substituindo diretamente
                configuration.GetSection("Mongo").Bind(options); 

                return options;
            });

            // Criar o MongoClient a partir da instancia de options ja criada
            services.AddSingleton<IMongoClient>(s =>
            {
                var options = s.GetService<MongoDbOptions>();
                return new MongoClient(options.ConnectionString);
            });

            // Obter o database e utilizar na aplicação
            services.AddTransient(s =>
            {
                // Salvar um Guid no Banco de Dados
                BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

                var options = s.GetService<MongoDbOptions>();
                var mongoClient = s.GetService<IMongoClient>();

                // Criar instancia do DataBase
                return mongoClient.GetDatabase(options.Database);
            });

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IOrderRepository, OrderRepository>();
            return services;
        }

        public static IServiceCollection AddMessageBus(this IServiceCollection services)
        {
            // Classe que é necessaria para criar conexoes com o RabbitMQ
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            // Criar conexao
            var connection = connectionFactory.CreateConnection("order-service-producer");

            services.AddSingleton(new ProducerConnection(connection));

            // A conexao vai ficar ativa (Singleton), mas quando alguma parte da aplicação
            // precisar usar o RabbitMQ, ele vai criar um canal dentro da conexão
            services.AddSingleton<IMessageBusClient, RabbitMqClient>();

            return services;
        }

        public static IServiceCollection AddSubscribers(this IServiceCollection services)
        {
            services.AddHostedService<PaymentAcceptedSubscriber>();
            return services;
        }
    }
}
