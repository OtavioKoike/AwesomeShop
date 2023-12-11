using AwesomeShop.Services.Orders.Core.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Application.Commands.Handlers
{
    public class AddOrderHandler : IRequestHandler<AddOrder, Guid>
    {
        private readonly IOrderRepository _orderRepository;

        public AddOrderHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Guid> Handle(AddOrder request, CancellationToken cancellationToken)
        {
            var order = request.ToEntity();
            await _orderRepository.AddAsync(order);

            return order.Id;
        }
    }
}
