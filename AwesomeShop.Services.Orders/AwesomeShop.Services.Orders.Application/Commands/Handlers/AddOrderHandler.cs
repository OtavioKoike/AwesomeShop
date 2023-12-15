using AwesomeShop.Services.Orders.Core.Clients;
using AwesomeShop.Services.Orders.Core.DTOs;
using AwesomeShop.Services.Orders.Core.Repositories;
using AwesomeShop.Services.Orders.Core.Utils;
using AwesomeShop.Services.Orders.Infrastructure.MessageBus;
using AwesomeShop.Services.Orders.Infrastructure.ServiceDiscovery;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Application.Commands.Handlers
{
    public class AddOrderHandler : IRequestHandler<AddOrder, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMessageBusClient _messageBus;
        private readonly IServiceDiscoveryService _serviceDiscovery;
        private readonly IGenericHttpClient _genericHttpClient;

        public AddOrderHandler(IOrderRepository orderRepository, IMessageBusClient messageBus, IServiceDiscoveryService serviceDiscovery, IGenericHttpClient genericHttpClient)
        {
            _orderRepository = orderRepository;
            _messageBus = messageBus;
            _serviceDiscovery = serviceDiscovery;
            _genericHttpClient = genericHttpClient;
        }

        public async Task<Guid> Handle(AddOrder request, CancellationToken cancellationToken)
        {
            var order = request.ToEntity();

            var costumerUrl = await _serviceDiscovery
                .GetServiceUri("CostumerService", $"/api/costumers/{order.Customer.Id}");

            var stringResult = await _genericHttpClient.GetAsync(costumerUrl);
            var custumerDto = JsonConvert.DeserializeObject<GetCustomerByIdDto>(stringResult);
            Console.WriteLine(custumerDto.FullName);

            await _orderRepository.AddAsync(order);

            foreach (var @event in order.Events)
            {
                // OrderCreated => order-created
                var routingKey = ToDashCaseUtils.ToDashCase(@event.GetType().Name);
                _messageBus.Publish(@event, routingKey, "order-service");
            }
            return order.Id;
        }
    }
}
