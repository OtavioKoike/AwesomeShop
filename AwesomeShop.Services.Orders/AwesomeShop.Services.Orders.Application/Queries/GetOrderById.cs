using AwesomeShop.Services.Orders.Application.DTOs.ViewModels;
using MediatR;
using System;

namespace AwesomeShop.Services.Orders.Application.Queries
{
    public class GetOrderById : IRequest<OrderViewModel>
    {
        public Guid Id { get; private set; }

        public GetOrderById(Guid id)
        {
            Id = id;
        }
    }
}
