using AwesomeShop.Services.Orders.Core.Clients;
using AwesomeShop.Services.Orders.Core.Repositories;
using AwesomeShop.Services.Orders.Infrastructure.CacheStorage;
using AwesomeShop.Services.Orders.Infrastructure.Clients;
using AwesomeShop.Services.Orders.Infrastructure.MessageBus;
using AwesomeShop.Services.Orders.Infrastructure.Persistence;
using AwesomeShop.Services.Orders.Infrastructure.Persistence.Repositories;
using AwesomeShop.Services.Orders.Infrastructure.ServiceDiscovery;
using AwesomeShop.Services.Orders.Infrastructure.Subscribers;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using RabbitMQ.Client;
using System;

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

        public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = config.GetValue<string>("Consul:Host");
                consulConfig.Address = new Uri(address);
            }));

            services.AddTransient<IServiceDiscoveryService, ConsulService>();

            return services;
        }

        // Registar assim que a Aplicacao inicia
        public static IApplicationBuilder useConsul(this IApplicationBuilder app)
        {
            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var lifeTime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

            var registration = new AgentServiceRegistration
            {
                ID = "order-service",
                Name = "OrderServices",
                Address = "localhost",
                Port = 5003
            };

            consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true);

            Console.WriteLine("Service registed in Consul");

            lifeTime.ApplicationStopping.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
                Console.WriteLine("Service deregisted in Consul");
            });

            return app;
        }

        public static IServiceCollection AddClients(this IServiceCollection services)
        {
            services.AddScoped<IGenericHttpClient, GenericHttpClient>();
            return services;
        }

        public static IServiceCollection AddRedisCache(this IServiceCollection services)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = "OrdersCache";
                options.Configuration = "localhost:6379";
            });

            services.AddTransient<ICacheService, RedisService>();

            return services;
        }
    }
}
