using AwesomeShop.Services.Orders.Core.Repositories;
using AwesomeShop.Services.Orders.Core.Utils;
using AwesomeShop.Services.Orders.Infrastructure.MessageBus;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Application.Commands.Handlers
{
    public class AddOrderHandler : IRequestHandler<AddOrder, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMessageBusClient _messageBus;

        public AddOrderHandler(IOrderRepository orderRepository, IMessageBusClient messageBus)
        {
            _orderRepository = orderRepository;
            _messageBus = messageBus;
        }

        public async Task<Guid> Handle(AddOrder request, CancellationToken cancellationToken)
        {
            var order = request.ToEntity();
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
